using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


//********************************************
// CLASS ProductNode: 
// Contains the info for each node of the product
//********************************************
public class ProductNode
{
	// General information about the node
	public string name;

	// Associated game objects
	public List<GameObject> gameObjects;

	// Explosion information
	public Vector3 explodeDirection;	// Direction of explosion
	public float explodeMin;			// Minimum explosion offset
	public float explodeMax; 			// Maximum explosion offset

	// Constraint-based implementation
	public List<ProductNode> blockedBy;
	public List<ProductNode> blocks;
	public List<ProductNode> followers;
	public List<ProductNode> attracts;

	// Exploding status 
	public int explodedChild;		// Array index of the last exploded child
	public float explodingOffset;	// How much this node is currently exploded
	public float explodingWeight;	// How much this node should be exploded
	public bool exploding; 			// Is this node currently exploding? (for parallel explosion)
    public bool exploded;           // This node has ended explosion

	// Exploding flags
	public bool explodable;			// Should I explode this node given the current type of partial explosion?
	public bool follower;			// Should I explode in my own direction or just follow the nodes I am blocking along their own direction?

	// Constructor from a string, with default no-explosion settings
	public ProductNode(string newname)
	{
		name = newname;
		gameObjects = new List<GameObject>(); // Must be explicitly filled after
		blockedBy = new List<ProductNode> (); // Must be explicitly filled after	
		blocks = new List<ProductNode> (); // Must be explicitly filled after	
		followers = new List<ProductNode> (); // Must be explicitly filled after
		attracts = new List<ProductNode> (); // Must be explicitly filled after
		explodeDirection = Vector3.zero;
		explodeMin = 0.0f;
		explodeMax = 0.0f; 
		explodedChild = -1;
		explodingOffset = 0.0f; 
		explodingWeight = 0.0f;
		exploding = false;
		follower = false;
		explodable = false;
        exploded = false;
	}

	// Constructor from node info found in the XML file
	public ProductNode(XMLNode xml)
	{
		name = xml.Name;
		gameObjects = new List<GameObject>(); // Must be explicitly filled after
		blockedBy = new List<ProductNode> (); // Must be explicitly filled after
		blocks = new List<ProductNode> (); // Must be explicitly filled after	
		followers = new List<ProductNode> (); // Must be explicitly filled after
		attracts = new List<ProductNode> (); // Must be explicitly filled after
		explodeDirection = new Vector3 (xml.ExplosionInfo.ExplodeX, xml.ExplosionInfo.ExplodeY, xml.ExplosionInfo.ExplodeZ);
		explodeMin = xml.ExplosionInfo.ExplodeMin;
		explodeMax = xml.ExplosionInfo.ExplodeMax;
		explodedChild = -1;
		explodingOffset = 0.0f; 
		explodingWeight = explodeMax;
		exploding = false;
		explodable = false;
		follower = false;
        exploded = false;
	}    
}


//********************************************
// CLASS XMLNode:
// Contains the attributes present in an XML file specifying the info for a product node
//********************************************
public class XMLExplosionInfo
{
	// Explosion direction and minimum offset (Vector3 is not serializable)
	public float ExplodeX;
	public float ExplodeY;
	public float ExplodeZ;
	public float ExplodeMin;
	public float ExplodeMax;

	public XMLExplosionInfo(){}
}


//********************************************
// CLASS XMLNode:
// Contains the attributes present in an XML file specifying the info for a product node
//********************************************
public class XMLNode
{
	// Node info (other than explosion) not included yet

	// Name of node
	[XmlAttribute("name")]
	public string Name;

	// Explosion info
	public XMLExplosionInfo ExplosionInfo;

	// Children nodes
	[XmlArray("Children")]
	[XmlArrayItem("Node")]
	public List<XMLNode> nodes = new List<XMLNode>();

	// Associated game objects
	[XmlArray("GameObjects")]
	[XmlArrayItem("GO")]
	public List<string> gameObjects = new List<string>();

	// List of blocking constraints
	[XmlArray("BlockedBy")]
	[XmlArrayItem("Node")]
	public List<string> blockingNodes = new List<string> ();

	// List of attraction constraints
	[XmlArray("Attracts")]
	[XmlArrayItem("Node")]
	public List<string> attractedNodes = new List<string> ();

	public XMLNode()
	{
		Name = "noname";
	}
}


//********************************************
// CLASS ProductContainer:
// Container of the serialized info for a product. The info is read from a XML file, using the XML serializer.
// Currently contains only explosion info
//********************************************
[XmlRoot("Product")]
public class ProductContainer
{
	// Global is not included yet

	[XmlArray("ProductTree")]
	[XmlArrayItem("Node")]
	public List<XMLNode> nodes = new List<XMLNode>();

	public ProductContainer(){}

	public static ProductContainer Load(string fileName)
	{
		var serializer = new XmlSerializer (typeof(ProductContainer));
		
		//string fullPath = Path.Combine (Application.dataPath, fileName);
		string fullPath = Application.dataPath + "/Xml/" + fileName;		
		using (var stream = new FileStream(fullPath, FileMode.Open)) 
		{
			return serializer.Deserialize (stream) as ProductContainer;
			//stream.Close();
		}
	}
	
	//public static ProductContainer Load(WWW wwwLevel)
	public static ProductContainer Load(WWW wwwLevel)
	{
		var serializer = new XmlSerializer (typeof(ProductContainer));
		return serializer.Deserialize (new StringReader (wwwLevel.text)) as ProductContainer;      
	}
}


//********************************************
// CLASS Product:
// The product attached to the GameObject containing all product parts
// On start, it automatically creates a product tree based on a XML file 
//********************************************
public class ExplosionManager : MonoBehaviour
{

    // Type definitions
    public enum TexplosionSign { FORWARD, BACKWARD };   // FORWARD is normal explosion, while BACKWARD is implosion/collapse
    public struct Texplosion                            // Type defining different explosion types and parameters
    {
        public float weight;        // Global scaling factor for the explosion of the whole product
        public List<string> selectedNodes;  // Selected nodes (containing selected game objects), for focused layout
        public TexplosionSign sign; // Explosion sign is forward or backward
        public bool parallel;       // Is explosion animation parallel? (multiple nodes translating at the same time)
        public float speed;         // Global explosion speed
    }

    // Constant values
    public const float MIN_EXPLOSION_WEIGHT = 0.0f;     // Explode up to minimum offset
    public const float MAX_EXPLOSION_WEIGHT = 1.0f;     // Explode up to maximum weighted offset


    //********************************************
    // ATTRIBUTES:

    // PUBLIC ATTRIBUTES FOR INSPECTOR:
    //string XMLFileName;                      // Name of XML file
    float explosionSpeed = 200;                    // Explosion speed, set in the Inspector

    // Global attributes
    private Tree<ProductNode> productTree;

    // Explosion attributes
    private Texplosion currentExplosion;        // Current explosion type and parameters
    private bool exploding;                     // The product is currently exploding/imploding
    private Tree<ProductNode> explodingParent;  // Currently exploding subtree

    // Explosion queue
    private Queue<Texplosion> explosionQueue;       // Simple queue to avoid overlapping two explosions/implosions. 

    // Focused explosion based on selection
    private GameObject goScript;        // Script to access selected game object


    //********************************************
    // METHODS:

    //**********************************************************************************************************************
    // UNITY METHODS
    //**********************************************************************************************************************
    private void Awake()
    {
        // Starts without exploding
        exploding = false;
        explosionQueue = new Queue<Texplosion>();

        // Get access to the selection script, to know which part is selected at any time
        goScript = GameObject.FindGameObjectWithTag("go_script");
        currentExplosion.selectedNodes = new List<string>();        
    }

    void Start()
    {
        
    }

    void Update()
    {
        // Integrate (serial) explosion
        if ((exploding) && (!currentExplosion.parallel))
            IntegrateExplosion(currentExplosion.speed * Time.deltaTime);    // Advance one step further in the serial explosion 
    }

    public void LoadProducExplosionModel(string url)
    {
        // Create product from XML file        
        if (Application.isEditor)
        {            
            StartCoroutine(CreateProductFromWWW(url));
            Debug.Log("Loading Explosion : " + url);         
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {                                       
            StartCoroutine(CreateProductFromWWW(url));
            goScript = GameObject.FindGameObjectWithTag("go_script");
            Debug.Log("Loading Explosion : " + url);
            //goScript.GetComponent<Core>().DebugConsoleWeb("Loading Explosion XML: " + url);
            string message = "Loading Explosion XML: " + url;
            hom3r.coreLink.Do(new CIOCommand(TIOCommands.DebugConsoleWeb, message), Constants.undoNotAllowed);
        }
    }//END LoadProducExplosionModel

    //**********************************************************************************************************************
    // GUI Methods
    //**********************************************************************************************************************


    //********************************************
    // METHOD ExplodeConfirmedSingle:
    // For remote access from GUI, launch explosion of first cofirmed object (taken from selection script)
    public void ExplodeConfirmedSingle()
    {
        // Check if we have selected a new part to explode 
        string newSelectedNode = FindConfirmedNode();
        if ((newSelectedNode != null))
        {
            this.Implode();
            this.Explode(MAX_EXPLOSION_WEIGHT, newSelectedNode);
        }
    }

    //********************************************
    // METHOD ExplodeConfirmed:
    // For remote access from GUI, launch explosion of all cofirmed objects (taken from selection script)
    public void ExplodeConfirmedAll()
    {
        if (goScript.GetComponent<SelectionManager>().GetNumberOfConfirmedGameObjects() > 0)
        {
            // List of selected nodes
            List<string> selectedNodes = new List<string>();

            // Reset explosion
            this.Implode();


            //Get list of confirmed objets
            List<GameObject> listConfirmedObjets = new List<GameObject>(goScript.GetComponent<SelectionManager>().GetListOfConfirmedObjects());
            // Select explosion targets
            foreach (GameObject confirmedGO in listConfirmedObjets)
            {
                selectedNodes.Add(FindNodeByGameObject(confirmedGO.name).name);
            }

            // Launch explosion
            this.Explode(MAX_EXPLOSION_WEIGHT, selectedNodes, TexplosionSign.FORWARD);
        }
    }

    /// <summary>
    /// Explode a specific GameObject
    /// </summary>
    /// <param name="go"> gameObject to explode</param>
    public void ExplodeGameObject(GameObject go)
    {
        // List of selected nodes
        List<string> selectedNodes = new List<string>();

        if (FindNodeByGameObject(go.name) != null)
        {
            selectedNodes.Add(FindNodeByGameObject(go.name).name);

            // Launch explosion
            this.Explode(MAX_EXPLOSION_WEIGHT, selectedNodes, TexplosionSign.FORWARD);
        }

        else {
            Debug.Log("Product Node " + go.name + " not found");
        }
    }


    /// <summary>
    /// Explode a specific GameObject
    /// </summary>
    /// <param name="go"> gameObject to explode</param>
    public void ImplodeGameObject(GameObject go)
    {        
        // List of selected nodes
        List<string> selectedNodes = new List<string>();
        if (FindNodeByGameObject(go.name) != null)
        {
            selectedNodes.Add(FindNodeByGameObject(go.name).name);
            // Launch explosion
            this.Explode(MAX_EXPLOSION_WEIGHT, selectedNodes, TexplosionSign.BACKWARD);
        }
        else
        {
            Debug.Log("Product Node not found");
        }
    }

    bool IsNotRestPosition(ProductNode node)
    {
        return !(Mathf.Approximately(node.explodingOffset, 0.0f));
    }

    /// <summary>
    /// Inform if there is any object exploded
    /// </summary>
    /// <returns>true when there is an exploded object</returns>
    public bool IsAnyObjectExploded()
    {
        return productTree.MeetsCondition(productTree, new TreeNodeCondition<ProductNode>(IsNotRestPosition));
    }
    
    //********************************************
    // METHOD FindConfirmedNode:
    // Returns the name of the first confirmed node in selection script. If there are not selected nodes, it returns null
    public string FindConfirmedNode()
    {
        GameObject selectedObject = new List<GameObject>(goScript.GetComponent<SelectionManager>().GetListOfConfirmedObjects())[0];
        if (selectedObject != null)
            return FindNodeByGameObject(selectedObject.name).name;
        else
            return null;
    }

    //********************************************
    // METHOD ReportExplodingAreas:
    // Send commands to HOM3R reporting which areas of the product are currently exploding
    public void ReportExplodingAreas(ProductNode node)//, TexplosionSign sign)
    {
        TexplosionSign sign = currentExplosion.sign;
        List<GameObject> explodedAreas= new List<GameObject>();

        for (int i = 0; i < node.gameObjects.Count; i++)
        {
            explodedAreas.Add(node.gameObjects[i]);
        }

       //Notify the Core the exploded or imploded
       if (sign == TexplosionSign.FORWARD)
            goScript.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplodingAreas, explodedAreas));
       else
            goScript.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ImplodingAreas, explodedAreas));
    }

//**********************************************************************************************************************
// START-related methods (Setting up product, explosion, etc)
//**********************************************************************************************************************


//********************************************
// METHOD CreateProductFromXML:
// Creates product tree and sets explosion info from a XML file
// PRECONDITION: In XML file, ProductTree contains only one ("root") node.
// PRECONDITION: explosion sequence is implicit in the order in which the children are described in the XML file
    public void CreateProductFromXML(string filename)
    {
        // Deserialize XML into product container
        var container = ProductContainer.Load(filename);

        // Create product tree
        productTree = new Tree<ProductNode>(new ProductNode("root"));
        CreateChildrenFromXML(productTree, container.nodes[0]);

        // Provisional solution: set now the blocking constraints in the specific case of nodes with explode direction equal to (0,0,0)
        // In this case, we assume that we must explode its children to avoid blocks
        // This solution requires going through the whole tree again, and is not efficient
        productTree.DoIf(productTree, new TreeNodeCondition<ProductNode>(IsContainer), new TreeAction<ProductNode>(PropagateBlocksToChildren));
    }
    //********************************************
    // METHOD CreateProductFromWWW:
    // TO DO: put order here...
    public IEnumerator CreateProductFromWWW(string url)
    {
        WWW xmlFile = new WWW(url);
        yield return xmlFile;
        if (xmlFile.error != null) { Debug.Log("Error loading explosion file from" + url); }
        else
        {
            // Deserialize XML into product container
            ProductContainer container;
            container = ProductContainer.Load(xmlFile);
            
            // Create product tree
            productTree = new Tree<ProductNode>(new ProductNode("root"));
            CreateChildrenFromXML(productTree, container.nodes[0]);

            // Provisional solution: set now the blocking constraints in the specific case of nodes with explode direction equal to (0,0,0)
            // In this case, we assume that we must explode its children to avoid blocks
            // This solution requires going through the whole tree again, and is not efficient
            productTree.DoIf(productTree, new TreeNodeCondition<ProductNode>(IsContainer), new TreeAction<ProductNode>(PropagateBlocksToChildren));
        }       
    }

    public void Clear()
    {
        if (productTree != null) { productTree.Clear(); }        
    }

    //********************************************
    // METHOD CreateChildrenFromXML:
    // Recursive method, for creating a subtree with all children of the parent node, based on info from a container deserialized from XML
    // PRECONDITION: explosion sequence is implicit in the order in which the children are described in the XML file
    public void CreateChildrenFromXML(Tree<ProductNode> parent, XMLNode container)
    {
        // Create children 
        foreach (XMLNode Nxml in container.nodes)
        {
            // Add new child
            ProductNode newNode = new ProductNode(Nxml);
            parent.AddChild(newNode);

            // Set game objects for the new child
            SetNodeGameObjects(newNode, Nxml);

            // Set blocking constraints
            // PRECONDITION: the blocking nodes must be created first (precondition for the creation of the XML file)
            SetNodeBlockingConstraints(newNode, Nxml);

            // Set attraction constraints
            // PRECONDITION: the attracted nodes must be created first (precondition for the creation of the XML file)
            SetNodeAttractionConstraints(newNode, Nxml);

            // Recursively create the branch for the new child			
            CreateChildrenFromXML(parent.GetChild(parent.GetChildCount() - 1), Nxml);
        }
    }


    //********************************************
    // METHOD SetNodeGameObjects:
    // For each go name in the XML container, search for that go in the Unity scene and associate it to the node
    public void SetNodeGameObjects(ProductNode node, XMLNode container)
    {
        int stringIndex;

        for (stringIndex = 0; stringIndex < container.gameObjects.Count; stringIndex++)
        {
            GameObject newGO = GameObject.Find(container.gameObjects[stringIndex]);

            if (newGO == null)
            {
                //Debug.Log ("WARNING!! Game Object not found: " + container.gameObjects[stringIndex]);
            }
            else
            {
                node.gameObjects.Add(newGO);
            }
        }
    }


    //********************************************
    // METHOD SetNodeBlockingConstraints:
    // For each blocking node name in the XML container, search for that node in the (currently built) tree and set it as a blocking node for this node
    // PRECONDITION: the XML built must be properly built, so that all blocking nodes are defined before the nodes blocked by them
    public void SetNodeBlockingConstraints(ProductNode node, XMLNode container)
    {
        int stringIndex;

        for (stringIndex = 0; stringIndex < container.blockingNodes.Count; stringIndex++)
        {
            // NON-PROPAGATED BLOCKS
            ProductNode blockingNode = this.FindNodeByName(container.blockingNodes[stringIndex]);
            if (blockingNode == null)
                Debug.Log("WARNING! Blocking node not found: " + container.blockingNodes[stringIndex]);
            else
                SetBlock(blockingNode, node);

            // PROPAGATED BLOCKS
            //Tree<ProductNode> blockingSubtree = this.FindSubtreeByName (container.blockingNodes[stringIndex]);			
            //if (blockingSubtree == null)	
            //	Debug.Log ("WARNING!! Blocking node not found: " + container.blockingNodes[stringIndex]);
            //else
            //{
            //PropagateOneBlock(blockingSubtree, node);				
            //}
        }
    }


    //********************************************
    // METHOD SetNodeAttractionConstraints:
    // For each attracted node name in the XML container, search for that node in the (currently built) tree and set it as an attracted node for this node
    // PRECONDITION: the XML built must be properly built, so that all attracted nodes are defined before the nodes attracting them
    public void SetNodeAttractionConstraints(ProductNode node, XMLNode container)
    {
        int stringIndex;

        for (stringIndex = 0; stringIndex < container.attractedNodes.Count; stringIndex++)
        {
            ProductNode attractedNode = this.FindNodeByName(container.attractedNodes[stringIndex]);
            if (attractedNode == null)
                Debug.Log("WARNING! Attracted node not found: " + container.attractedNodes[stringIndex]);
            else
                node.attracts.Add(attractedNode);
        }
    }


    //********************************************
    // METHOD PropagateOneBlock:
    // Sets a blocking relation, propagating the "blocks" from the blocking node to all its children
    public void PropagateOneBlock(Tree<ProductNode> blocker, ProductNode blocked)
    {
        //blocked.blockedBy.Add (blocker.GetData ());
        //blocker.GetData ().blocks.Add (blocked);
        SetBlock(blocker.GetData(), blocked);

        // boB = blocker of blocker
        for (int boB = 0; boB < blocker.GetData().blockedBy.Count; boB++)
        {
            Tree<ProductNode> boBSubtree = FindSubtreeByName(blocker.GetData().blockedBy[boB].name); // Not very efficient...
            PropagateOneBlock(boBSubtree, blocked);
        }
    }


    //**********************************************************************************************************************
    // AUXILIARY methods
    //**********************************************************************************************************************


    //********************************************
    // METHOD FindSubtreeByName:
    // Returns the subtree whose name matches the provided string
    public Tree<ProductNode> FindSubtreeByName(string name)
    {
        ProductNode nodeName = new ProductNode(name);
        return productTree.GetIf(productTree, new TreeNodeCompare<ProductNode>(NamesAreEqual), nodeName);
    }


    //********************************************
    // METHOD NamesAreEqual:
    // Auxiliary method. Returns true if the name of two product nodes is the same
    public bool NamesAreEqual(ProductNode n1, ProductNode n2)
    {
        if (n1.name.Equals(n2.name))
            return true;
        else
            return false;
    }


    //********************************************
    // METHOD ContainsGameObject:
    // Auxiliary method. Returns true if the node contains a gameObject with the name in node "goNode"
    public bool ContainsGameObject(ProductNode node, ProductNode goNode)
    {
        string goName = goNode.name;
        GameObject go = GameObject.Find(goName);
        if (go == null)
            return false;

        if (node.gameObjects.Contains(go))
            return true;
        else
            return false;
    }

    //********************************************
    // METHOD FindNodeByGameObject:
    // Returns the node which contains a given gameobject (identified by name)
    public ProductNode FindNodeByGameObject(string name)
    {
        ProductNode goName = new ProductNode(name);

        if (productTree.GetIf(productTree, new TreeNodeCompare<ProductNode>(ContainsGameObject), goName) != null)
        {
            return productTree.GetIf(productTree, new TreeNodeCompare<ProductNode>(ContainsGameObject), goName).GetData();
        }
        else {
            return null;
        }
    }


    //********************************************
    // METHOD FindNodeByName:
    // Returns the node whose name matches the provided string
    public ProductNode FindNodeByName(string name)
    {
        if (name == "LES") {
            Debug.Log(name);
        }
        ProductNode nodeName = new ProductNode(name);
        return productTree.GetIf(productTree, new TreeNodeCompare<ProductNode>(NamesAreEqual), nodeName).GetData();
    }


    //********************************************
    // METHOD IsContainer:
    // Auxiliary method, returning true if the explosion direction of a given node is (0,0,0)
    public bool IsContainer(ProductNode node)
    {
        return (node.explodeDirection.Equals(Vector3.zero));
    }
    

    //********************************************
    // METHOD SetBlock:
    // Auxiliary method, setting one blocking relationship between two nodes
    public void SetBlock(ProductNode blocker, ProductNode blocked)
    {
        blocker.blocks.Add(blocked);
        blocked.blockedBy.Add(blocker);
    }

    //********************************************
    // METHOD PropagateBlocksToChildren:
    // Auxiliary method, propagating blocks to the direct children of a subtree. Used only for the specific case of container with (0,0,0) explodeDir
    public void PropagateBlocksToChildren(Tree<ProductNode> subtree)
    {
        int child;
        for (child = 0; child < subtree.GetChildCount(); child++)
        {
            foreach (ProductNode block in subtree.GetData().blocks)
            {
                //subtree.GetChild(child).GetData().blocks.Add(block);
                SetBlock(subtree.GetChild(child).GetData(), block);
            }
        }
    }


    //********************************************
    // METHOD IsBlocking: 
    // Auxiliary method, returns true if blocked is in the blocks list of blocker
    public bool IsBlocking(ProductNode blocker, ProductNode blocked)
    {
        return blocker.blocks.Contains(blocked);
    }

    //********************************************
    // METHOD IsBlockkedBy: 
    // Auxiliary method, ...
    public bool IsBlockedBy(ProductNode blocked, ProductNode blocker)
    {
        return blocked.blockedBy.Contains(blocker);
    }

    //********************************************
    // METHOD HasBlockingRelation: 
    // Auxiliary method, returns true if other is in the blocks or blockedBy list of selected
    public bool HasBlockingRelation(ProductNode selected, ProductNode other)
    {
        return ((selected.blocks.Contains(other)) || (selected.blockedBy.Contains(other)));
    }


    //********************************************
    // METHOD HasIndirectBlockingRelation: 
    // Auxiliary method, returns true if other is in the blocks or blockedBy list of the blocker or blocked nodes of selected
    public bool HasIndirectBlockingRelation(ProductNode selected, ProductNode other)
    {
        // Blockers of selected
        bool isIndirectBlocker = false;
        foreach (ProductNode blocker in selected.blockedBy)
        {
            isIndirectBlocker = HasBlockingRelation(selected, blocker);
        }

        // Blocked by selected
        bool isIndirectBlocked = false;
        foreach (ProductNode blocked in selected.blocks)
        {
            isIndirectBlocked = HasBlockingRelation(selected, blocked);
        }

        return (isIndirectBlocker && isIndirectBlocked);
    }


    //********************************************
    // METHOD IsIndirectBlocker: 
    // Auxiliary method, returns true if indirectBlocker is in the blocks list of the blockers of blocked
    public bool IsIndirectBlocker(ProductNode indirectBlocker, ProductNode blocked)
    {
        foreach (ProductNode blocker in blocked.blockedBy)
        {
            if (IsBlocking(indirectBlocker, blocker))
                return true;
            else
                if (IsIndirectBlocker(indirectBlocker, blocker))
                return true;
        }

        return false;
    }

    //********************************************
    // METHOD IsIndirectBlockedBy: 
    // Auxiliary method, ...
    public bool IsIndirectBlockedBy(ProductNode blocked, ProductNode indirectBlocker)
    {
        foreach (ProductNode newBlocked in indirectBlocker.blocks)
        {            
            if (IsBlockedBy(newBlocked, indirectBlocker))
                return true;
            else                
                if (IsIndirectBlockedBy(newBlocked, indirectBlocker))
                return true;
        }

        return false;
    }

    //********************************************
    // METHOD IsRecursiveBlocking: 
    // Auxiliary method, returns true if blocker is in the blockedBy list of blocked or of its blockers, recursively
    public bool IsRecursiveBlocking(ProductNode blocker, ProductNode blocked)
    {
        if (IsBlocking(blocker, blocked))
            return true;
        else
        {
            foreach (ProductNode blockerOfBlocker in blocked.blockedBy)
            {
                if (IsRecursiveBlocking(blocker, blockerOfBlocker))
                    return true;
            }
        }

        return false;
    }

    //********************************************
    // METHOD IsRecursiveBlockedBy: 
    // Auxiliary method, returns true if blocker is in the blockedBy list of blocked or of its blockers, recursively
    public bool IsRecursiveBlockedBy(ProductNode blocked, ProductNode blocker)
    {
        if (IsBlockedBy(blocked, blocker))
            return true;
        else
        {
            foreach (ProductNode blocksBlocked in blocked.blockedBy)
            {
                if (IsRecursiveBlockedBy(blocksBlocked, blocker))
                    return true;
            }
        }

        return false;
    }

    //********************************************
    // METHOD IsRecursiveBlocking: 
    // Auxiliary method, returns true if blocker is in the blockedBy list of blocked or of its blockers, recursively
    public bool IsRecursiveBlockingNotAttracted(ProductNode blocker, ProductNode blocked)
    {
        if (IsBlocking(blocker, blocked))
        {
            if (blocked.attracts.Contains(blocker))
                return false;
            else
                return true;
        }
        else
        {
            foreach (ProductNode blockerOfBlocker in blocked.blockedBy)
            {
                if (IsRecursiveBlocking(blocker, blockerOfBlocker))
                    return true;
            }
        }

        return false;
    }


    //********************************************
    // METHOD SetFollower: 
    // Auxiliary method, sets the follower flag of a node
    public void SetFollower(ProductNode node)
    {
        node.follower = true;
    }

    //********************************************
    // METHOD AddFollower: 
    // Auxiliary method, adds follower to the followers list of master
    public void AddFollower(ProductNode follower, ProductNode master)
    {
        master.followers.Add(follower);
        follower.follower = true;
    }

    //********************************************
    // METHOD AddIndirectFollower: 
    // Auxiliary method, adds follower to the followers list of the blockers of master
    public void AddIndirectFollower(ProductNode follower, ProductNode master)
    {
        follower.follower = true;
        foreach (ProductNode blocker in master.blockedBy)
        {
            if (blocker.blockedBy.Contains(follower))
            {
                // Avoid adding twice the same follower
                if (!blocker.followers.Contains(follower))
                    blocker.followers.Add(follower);
            }
            else
                AddIndirectFollower(follower, blocker);
        }
    }

    public void PropagateWeightFromBlocker(ProductNode follower, ProductNode master)
    {
        foreach (ProductNode blocker in master.blockedBy)
        {
            if (blocker.blockedBy.Contains(follower))
                follower.explodingWeight = blocker.explodingWeight;
            else
                PropagateWeightFromBlocker(follower, blocker);
        }
    }

    //********************************************
    // METHOD SetExplodable: 
    // Auxiliary method, sets the explodable flag of a node
    public void SetExplodable(ProductNode node)
    {
        node.explodable = true;
    }


    //********************************************
    // METHOD ResetExplodingFlags: 
    // Auxiliary method, unsets the explodable and follower flags of a node, and clears the list of followers
    public void ResetExplodingFlags(ProductNode node)
    {
        node.explodable = false;
        node.follower = false;
        node.followers.Clear();
    }


    //********************************************
    // METHOD SetNotExploding:  
    // Auxiliary method, sets the exploding flag of one node to false
    public void SetNotExploding(ProductNode node)
    {
        node.exploding = false;
    }


    //********************************************
    // METHOD SetNotExplodingAndNotExploded:  
    // Auxiliary method, sets the exploding and exploded flags of one node to false
    public void SetNotExplodingAndNotExploded(ProductNode node)
    {
        node.exploding = false;
        node.exploded = false;
    }


    //********************************************
    // METHOD IsExplodable:
    // Checks if a given node is explodable
    public bool IsExplodable(ProductNode node)
    {
        return node.explodable;
    }


    //********************************************
    // METHOD IsExploding:
    // Checks if a given node is currently exploding
    public bool IsExploding(ProductNode node)
    {
        return node.exploding;
    }

    //********************************************
    // METHOD IsExplodableExploding:
    // Checks if a given node is explodable but has not ended explosion
    public bool IsExplodableExploding(ProductNode node)
    {
        return (node.explodable && !node.exploded);
    }

    //********************************************
    // METHOD IsExploded:
    // Checks if a given node has ended explosion
    public bool IsExploded(ProductNode node)
    {
        return node.exploded;
    }

    //********************************************
    // METHOD ContainsExplodable:
    // Checks if a given subtree contains any explodable nodes
    public bool ContainsExplodable(Tree<ProductNode> subtree)
    {
        return subtree.MeetsCondition(productTree, new TreeNodeCondition<ProductNode>(IsExplodable));
    }

    //********************************************
    // METHOD SetMaximumWeight:
    // Sets the exploding weight of a node to its explodeMax
    public void SetMaximumWeight(ProductNode node)
    {
        node.explodingWeight = node.explodeMax;
    }


    //**********************************************************************************************************************
    // EXPLOSION methods
    //**********************************************************************************************************************

    // LAUNCH EXPLOSION

    //********************************************
    // METHOD Explode: OVERLOADED 
    // Launch the explosion sequence, depending on explosionType
    public void Explode(Texplosion newExplosion)
    {
        // Launch explosion
        if (!exploding)
        {
            // Things to do for all explosion types
            exploding = true;
            explodingParent = productTree;
            currentExplosion = newExplosion;
            currentExplosion.parallel = true;   // NOW forced to parallel
            explodingParent.Do(explodingParent, new TreeNodeAction<ProductNode>(SetNotExplodingAndNotExploded));    // we start without exploding nor exploded nodes
               
            // Different things to do for each explosion type
            if (newExplosion.sign == TexplosionSign.BACKWARD)
            {
                // Global implosion
                if (newExplosion.selectedNodes.Count == 0)
                {
                    explodingParent.Do(explodingParent, new TreeNodeAction<ProductNode>(SetExplodable));                    
                }

                // Focused explosion
                else
                {
                    foreach (string nodeName in newExplosion.selectedNodes)
                    {
                        ProductNode nodeToExplode = FindNodeByName(nodeName);
                        nodeToExplode.explodable = true;

                        //explodingParent.DoIf(explodingParent, new TreeNodeCompare<ProductNode>(HasBlockingRelation), nodeToExplode, new TreeNodeAction<ProductNode>(SetExplodable)); 
                        explodingParent.DoIf(explodingParent, new TreeNodeCompare<ProductNode>(IsRecursiveBlockedBy), nodeToExplode, new TreeNodeAction<ProductNode>(SetExplodable));                        
                        //explodingParent.DoIf(explodingParent, new TreeNodeCompare<ProductNode>(IsIndirectBlockedBy), nodeToExplode, new TreeNodeAction<ProductNode>(SetExplodable));
                    }                    
                }
            }
            else
            {
                // Global explosion
                if (newExplosion.selectedNodes.Count == 0)
                {
                    explodingParent.Do(explodingParent, new TreeNodeAction<ProductNode>(SetExplodable));
                    explodingParent.Do(explodingParent, new TreeNodeAction<ProductNode>(SetMaximumWeight));
                }

                // Focused explosion
                else
                {
                    explodingParent.Do(explodingParent, new TreeNodeAction<ProductNode>(SetMaximumWeight));

                    foreach (string nodeName in newExplosion.selectedNodes)
                    {
                        ProductNode nodeToExplode = FindNodeByName(nodeName);
                        nodeToExplode.explodable = true;

                        //explodingParent.DoIf(explodingParent, new TreeNodeCompare<ProductNode>(HasBlockingRelation), nodeToExplode, new TreeNodeAction<ProductNode>(SetExplodable)); 
                        explodingParent.DoIf(explodingParent, new TreeNodeCompare<ProductNode>(IsRecursiveBlocking), nodeToExplode, new TreeNodeAction<ProductNode>(SetExplodable));
                        //explodingParent.DoIf(explodingParent, new TreeNodeCompare<ProductNode>(IsIndirectBlocker), nodeToExplode, new TreeNodeDo<ProductNode>(AddIndirectFollower));
                    }
                }
            }

            // Start parallel explosion
            if (newExplosion.parallel)
                StartParallelExplode(explodingParent);
        }

        // Queue explosion
        else
            explosionQueue.Enqueue(newExplosion);
    }
    

    //********************************************
    // METHOD Explode: OVERLOADED
    // Launch the global explosion sequence, up to a given maximum (weight)
    public void Explode(float weight)
    {
        // Setup explosion type
        Texplosion newExplosion;
        newExplosion.sign = TexplosionSign.FORWARD;
        newExplosion.weight = weight;
        newExplosion.selectedNodes = new List<string>();
        newExplosion.parallel = true;
        newExplosion.speed = explosionSpeed;

        // Launch explosion
        Explode(newExplosion);
    }


    //********************************************
    // METHOD Explode: OVERLOADED 
    // Launch the partial explosion sequence, up to explosion of a node with a given name. up to a given maximum (weight)
    public void Explode(float weight, string nodeName)
    {
        // Setup explosion type
        Texplosion newExplosion;
        newExplosion.sign = TexplosionSign.FORWARD;
        newExplosion.weight = weight;
        newExplosion.selectedNodes = new List<string>();
        newExplosion.selectedNodes.Add(nodeName);
        newExplosion.parallel = true;
        newExplosion.speed = explosionSpeed;

        // Launch explosion
        Explode(newExplosion);
    }


    //********************************************
    // METHOD Explode: OVERLOADED 
    // Launch the partial explosion sequence, up to explosion of a set of nodes with given names. up to a given maximum (weight)
    public void Explode(float weight, List<string> nodeNames, TexplosionSign sign)
    {
        // Setup explosion type
        Texplosion newExplosion;
        //newExplosion.sign = TexplosionSign.FORWARD;
        newExplosion.sign = sign;
        newExplosion.weight = weight;
        newExplosion.parallel = true;
        newExplosion.speed = explosionSpeed;

        newExplosion.selectedNodes = new List<string>();
        foreach (string name in nodeNames)
            newExplosion.selectedNodes.Add(name);

        // Launch explosion
        Explode(newExplosion);


    }

    //********************************************
    // METHOD Implode:
    // Launch the inverse explosion sequence to go back (collapse) to rest state
    public void Implode() // 
    {
        // Setup explosion type
        Texplosion newExplosion;
        newExplosion.sign = TexplosionSign.BACKWARD;
        newExplosion.weight = MAX_EXPLOSION_WEIGHT;
        newExplosion.selectedNodes = new List<string>();
        newExplosion.parallel = true;
        newExplosion.speed = explosionSpeed * 2.0f; // Double speed for implosions

        // Launch explosion
        Explode(newExplosion);
    }


    // INTEGRATE EXPLOSION

    //********************************************
    // METHOD IntegrateNodeExplosion: OVERLOADED
    // Move one node along a given explosion direction.
    public void IntegrateNodeExplosion(ProductNode node, float increment, Vector3 explodeDirection)
    {
        node.explodingOffset += increment;

        for (int i = 0; i < node.gameObjects.Count; i++)
        {
            node.gameObjects[i].transform.Translate(explodeDirection * increment, Space.World);
        }

        // Integrate followers
        foreach (ProductNode follower in node.followers)
        {
            if (!node.attracts.Contains(follower))
                IntegrateNodeExplosion(follower, increment, explodeDirection);
        }

        // Move attracted nodes
        foreach (ProductNode attracted in node.attracts)
        {
            for (int i = 0; i < attracted.gameObjects.Count; i++)
                attracted.gameObjects[i].transform.Translate(explodeDirection * increment, Space.World); // WARNING! 
        }
    }


    //********************************************
    // METHOD IntegrateNodeExplosion: OVERLOADED
    // Move one node along its explosion direction.
    public void IntegrateNodeExplosion(ProductNode node, float increment)
    {
        node.explodingOffset += increment;

        for (int i = 0; i < node.gameObjects.Count; i++)
        {
            node.gameObjects[i].transform.Translate(node.explodeDirection * increment, Space.World);
        }

        // Integrate followers
        foreach (ProductNode follower in node.followers)
        {
            if (!node.attracts.Contains(follower))
                IntegrateNodeExplosion(follower, increment, node.explodeDirection);
        }

        // Move attracted nodes
        foreach (ProductNode attracted in node.attracts)
        {
            for (int i = 0; i < attracted.gameObjects.Count; i++)
                attracted.gameObjects[i].transform.Translate(node.explodeDirection * increment, Space.World); // WARNING!
        }
    }


    //********************************************
    // METHOD IntegrateSubtreeExplosion: OVERLOADED
    // Move one subtree along a given explosion direction.
    // This will be called from the other version of IntegrateSubtreeExplosion, where the parent explode direction is passed.
    // We assume that nodes without associated gameobjects have children with associated gameobjects.
    public void IntegrateSubtreeExplosion(Tree<ProductNode> subtree, float increment, Vector3 explodeDirection)
    {
        ProductNode node = subtree.GetData();

        // If the node has associated gameobjects, we just move them
        if (node.gameObjects.Count > 0)
        {
            IntegrateNodeExplosion(node, increment, explodeDirection);
        }

        // If the node is parent of other nodes with gameobjects, we move all children gameobjects
        else
        {
            // Move all children along given direction
            for (int i = 0; i < subtree.GetChildCount(); i++)
            {
                IntegrateSubtreeExplosion(subtree.GetChild(i), increment, explodeDirection);
            }
        }
    }


    //********************************************
    // METHOD IntegrateSubtreeExplosion: OVERLOADED
    // Move one subtree along its explosion direction.
    // This is called from IntegrateExplosion.
    // We assume that nodes without associated gameobjects have children with associated gameobjects.
    public void IntegrateSubtreeExplosion(Tree<ProductNode> subtree, float increment)
    {
        ProductNode node = subtree.GetData();

        // If the node has an associated gameobject, we just move it
        if (node.gameObjects.Count > 0)
        {
            IntegrateNodeExplosion(node, increment);
        }

        // If the node is parent of other nodes with gameobjects, we move all children gameobjects
        else
        {
            // Get the parent explosion direction
            Vector3 explodeDirection = node.explodeDirection;

            // And move all children along parent direction
            for (int i = 0; i < subtree.GetChildCount(); i++)
            {
                IntegrateSubtreeExplosion(subtree.GetChild(i), increment, explodeDirection);
            }
        }
    }


    //********************************************
    // METHOD CheckSubtreeExplosion:
    // Checks if the current subtree is already exploded.
    // In that case, it goes up in the tree hierarchy.
    // Continues checking subtrees until it finds a non-exploded subtree or until it pass through the whole tree
    public void CheckSubtreeExplosion()
    {
        // Check condition for changing subtree, depending on explosion sign
        bool changeSubtree;
        if (currentExplosion.sign == TexplosionSign.FORWARD)
            changeSubtree = explodingParent.GetChildCount() - 1 <= explodingParent.GetData().explodedChild;
        else
            changeSubtree = explodingParent.GetData().explodedChild < 0;

        // Check condition for changing subtree, depending on explodable condition
        bool explodable = ContainsExplodable(explodingParent);
        changeSubtree = changeSubtree | !explodable;

        // Change subtree
        if (changeSubtree)
        {
            // Select grandpa as the new parent
            Tree<ProductNode> newParent = explodingParent.GetParent();

            // Check if we are done
            if (newParent == null)
                exploding = false;
            else
            {
                explodingParent = newParent;
                CheckSubtreeExplosion();    // Check another subtree
            }
        }

        // Ok. Let's explode this subtree...
    }


    //********************************************
    // METHOD GetExplosionWeight:
    // Returns the maximum explosion offset for a given node, depending on the heterogeneous explosion settings
    // Currently tests explosion of selected object
    public float GetExplosionWeight(ProductNode node)
    {
        // Check if it is already computed
        if (node.explodingOffset == 0.0f)   // Caution! It wont work for weighted implosions
        {
            //			// First test: hard-coded selected node
            //			if (node.name.Equals ("Parte superior")) 
            //			{
            //				node.explodingWeight = node.explodeMax;
            //			}
            //			else
            //				node.explodingWeight = 0.0f;

            // SEcond test: selected object PART SELECTION
            //			if (node.gameObjects.Contains(selectedObject))
            //				node.explodingWeight = node.explodeMax;
            //			else
            //				node.explodingWeight = 0.0f;

            // Third test: no weights
            //node.explodingWeight = node.explodeMax;

            // Focused layout
            //			if (currentExplosion.selectedNode != null)
            //			{
            //				if (!currentExplosion.selectedNode.Equals(node.name))
            //				{
            //					ProductNode selected = FindNodeByName (currentExplosion.selectedNode);
            //					node.explodingWeight += selected.explodingWeight;
            //				}
            //			}
        }

        return node.explodingWeight * currentExplosion.weight;
    }


    // SERIAL EXPLOSION


    //********************************************
    // METHOD IntegrateExplosion:
    // Advance one step further in the explosion
    // Explodes each node up to its weighted max (depending on heterogeneous explosion settings)
    // Currently explodes one node at a time.
    public void IntegrateExplosion(float increment)
    {
        // First of all, check if the current subtree is already exploded. In that case, search for the next subtree to explode.
        CheckSubtreeExplosion();

        // Check if explosion is done, after checking subtrees.
        // If it is done, launch the following queued explosion
        if (!exploding)
        {
            // Set everything to not explodable
            productTree.Do(productTree, new TreeNodeAction<ProductNode>(ResetExplodingFlags));

            // Dequeue
            if (explosionQueue.Count > 0)
            {
                Texplosion newExplosion = explosionQueue.Dequeue();
                Explode(newExplosion);
            }

            return;
        }

        // Get the node to explode
        int explodingChild;
        if (currentExplosion.sign == TexplosionSign.FORWARD)
            explodingChild = explodingParent.GetData().explodedChild + 1;
        else
            explodingChild = explodingParent.GetData().explodedChild;

        // Easy access to the current subtree and node
        Tree<ProductNode> subtree = explodingParent.GetChild(explodingChild);
        ProductNode node = subtree.GetData();

        // Find weighted maximum offset for this node	
        float weightedmaxoffset = 0.0f;
        if (currentExplosion.sign == TexplosionSign.FORWARD)
            weightedmaxoffset = GetExplosionWeight(node);

        // Change sign of increment, depending on explosion sign
        if (currentExplosion.sign == TexplosionSign.BACKWARD)
            increment = -1.0f * increment;

        // Check if we should explode the current node
        bool nodeDone;
        nodeDone = !IsExplodable(node);

        // If the node is explodable, we increment the offset
        if (!nodeDone)
        {
            // Increment offset
            float totalOffset = node.explodingOffset + increment;

            // Do something when reaching minoffset??? 
            //if (explosionOffset >= minoffset) ...		

            // Check if we have passed the maximum offset
            if (currentExplosion.sign == TexplosionSign.FORWARD)
                nodeDone = totalOffset >= node.explodeMin + weightedmaxoffset;
            else
                nodeDone = totalOffset <= 0.0f;

            // If we have passed the limit, solve it
            if (nodeDone)
            {
                // Check which was the limit that ve passed
                float passedLimit;
                if (currentExplosion.sign == TexplosionSign.FORWARD)
                    passedLimit = node.explodeMin + weightedmaxoffset;
                else
                    passedLimit = 0.0f;

                // Compute remaining offset to reach the limit
                float remainingOffset = increment - totalOffset + passedLimit;

                // Move the remaining offset
                //node.explodingOffset = passedLimit;
                IntegrateSubtreeExplosion(subtree, remainingOffset);
            }
        }

        // SELECT NEW NODE:
        if (nodeDone)
        {
            // Mark the current node as exploded
            ProductNode parentNode = explodingParent.GetData();
            if (currentExplosion.sign == TexplosionSign.FORWARD)
                parentNode.explodedChild++;
            else
                parentNode.explodedChild--;
            explodingParent.SetData(parentNode);

            // We try first going deeper into the current node sub-tree
            Tree<ProductNode> newParent = explodingParent.GetChild(explodingChild);

            // If the current node has children, its first child will be the new parent
            if (newParent.GetChildCount() > 0)
                explodingParent = newParent;

            // If the current node has no children, we will continue with the next child of the current parent.
            // Since we start checking if the subtree is already exploded, this may lead to going up in the tree (CheckSubtreeExplosion).
        }

        // We keep moving the same node
        else
        {
            IntegrateSubtreeExplosion(subtree, increment);
        }
    }

    // PARALLEL EXPLOSION

    //********************************************
    // METHOD LaunchParallelExplode:
    // Launch coroutines for all explodable and non-blocked nodes
    // Each coroutine will be responsible of checking if its blocked nodes should start exploding
    public void StartParallelExplode(Tree<ProductNode> subtree)
    {
        subtree.DoIf(subtree, new TreeNodeCondition<ProductNode>(CanStartExplosion), new TreeNodeAction<ProductNode>(LaunchNodeExplosion));

        // Launch command to notify Core
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplosionBegin));        
    }


    //********************************************
    // METHOD CanStartExplosion:
    // Tells if a node is free of blocks to start its explosion, if it is not already exploding and if it is explodable.
    public bool CanStartExplosion(ProductNode node)
    {
        // If its explosion was already start, it can NOT start again
        if (node.exploding)
            return false;

        // If the node is not explodable or follower, it can NOT start its explosion
        if (!node.explodable) //&& (!node.follower))
            return false;

        // First approach: always implode everything (without checking blocks)
        if (currentExplosion.sign == TexplosionSign.BACKWARD)
            return true;

        // Check if there are unsolved blocks (in forward explosions)
        foreach (ProductNode blocker in node.blockedBy)
        {
            // We don't need to actually explode follower blockers
            if (!blocker.follower)
            {
                // Explodable blockers must reach explodemin
                if (blocker.explodingOffset < blocker.explodeMin)
                    return false;
            }
        }

        // This node can start forward explosion!
        return true;
    }


    //********************************************
    // METHOD LaunchNodeExplosion:
    // Auxiliary method, to launch the coroutine that explodes one node
    public void LaunchNodeExplosion(ProductNode node)
    {
        StartCoroutine(ParallelNodeExplosion(node));
    }


    //********************************************
    // METHOD ParallelNodeExplosion:
    // Coroutine for parallel explosion of one node
    public IEnumerator ParallelNodeExplosion(ProductNode node)
    {
        // Sets this node as exploding
        //node.exploding = exploding;
        node.exploding = true;

        // Report exploding areas to HOM3R
        ReportExplodingAreas(node);

        // Do until the node is no longer exploding
        while (node.exploding)
        {
            // Find weighted maximum offset for this node	
            float weightedmaxoffset = 0.0f;
            if (currentExplosion.sign == TexplosionSign.FORWARD)
                weightedmaxoffset = GetExplosionWeight(node);

            // Compute increment for offset
            float increment = currentExplosion.speed * Time.deltaTime;

            // Change sign of increment, depending on explosion sign
            if (currentExplosion.sign == TexplosionSign.BACKWARD)
                increment = -1.0f * increment;

            // Increment offset
            float totalOffset = node.explodingOffset + increment;

            // Check if we have passed the minimum offset. In this case, try to launch blocked nodes explosion
            //if (currentExplosion.sign == TexplosionSign.FORWARD)
            //{
            if (totalOffset >= node.explodeMin)
            {
                foreach (ProductNode blocked in node.blocks)
                {
                    if (CanStartExplosion(blocked))
                        LaunchNodeExplosion(blocked);
                }
            }
            //}

            // Check if we have passed the maximum offset			
            if (currentExplosion.sign == TexplosionSign.FORWARD)
                node.exploding = !(totalOffset >= node.explodeMin + weightedmaxoffset);
            else
                node.exploding = !(totalOffset <= 0.0f);


            // If we have passed the limit, solve it
            if (!node.exploding)
            {
                // Check which was the limit that ve passed
                float passedLimit;
                if (currentExplosion.sign == TexplosionSign.FORWARD)
                    passedLimit = node.explodeMin + weightedmaxoffset;
                else
                    passedLimit = 0.0f;

                // Compute remaining offset to reach the limit
                increment = increment - totalOffset + passedLimit;

                // Move the remaining offset
                //totalOffset = passedLimit;
            }

            // Move node (integrate node position)
            IntegrateNodeExplosion(node, increment);

            // Wait for new frame
            yield return null;
        }

        // Node has ended explosion
        node.exploded = true;

        // Check if explosion is done, and then launch new explosion in the queue
        // WARNING: are we sure that this will happen only once? think on it
        CheckExplosionEnd();
    }

    //********************************************
    // METHOD CheckExplosionEnd:
    // Check if a parallel explosion has ended (all threads are done)
    public void CheckExplosionEnd()
    {
        // First condition: all explodable nodes have ended explosion
        //if (!explodingParent.MeetsCondition(explodingParent, new TreeNodeCondition<ProductNode>(IsExploding)))
        if (!explodingParent.MeetsCondition(explodingParent, new TreeNodeCondition<ProductNode>(IsExplodableExploding)))
        {
            // Second condition: no nodes are exploding (followers, blockers...)
            if (!explodingParent.MeetsCondition(explodingParent, new TreeNodeCondition<ProductNode>(IsExploding)))
            {
                // Explosion has ended
                exploding = false;

                // Launch command to notify Core
                GameObject.FindGameObjectWithTag("go_script").GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplosionEnd));

                // Set everything to not explodable
                productTree.Do(productTree, new TreeNodeAction<ProductNode>(ResetExplodingFlags));

                // Dequeue
                if (explosionQueue.Count > 0)
                {
                    Texplosion newExplosion = explosionQueue.Dequeue();
                    Explode(newExplosion);
                }
            }
        }
    }

    public bool IsEmpty()
    {
        if (productTree != null)
        {
            return productTree.isEmpty();
        }
        return true;
    }
}