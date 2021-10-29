using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////
/// Product Model Classes
////////////////////////////////

class CAreaDataDictionary : CAreaData
{
    public bool selectable { get; set; }
    public string specialAncestorID { get; set; }    
    public GameObject gameobject_pointer { get; set; }

    public CAreaDataDictionary() { }
    public CAreaDataDictionary(CAreaData _data)
    {
        this.id             = _data.id;
        this.parentId       = _data.parentId;
        this.description    = _data.description;
        this.meshname       = _data.meshname;
    }

}

[System.Serializable]
public class CNodeData
{
    public string id;
    public string parentId;
    public string description;    
    public bool special_node;

    public CNodeData() { }
    public CNodeData(string _id, string _parentId, string _description, bool _special_node)
    {
        this.id = _id;
        this.parentId = _parentId;
        this.description = _description;        
        this.special_node = _special_node;
    }
        
}

[System.Serializable]
public class CLeafData
{
    public string id;
    public string parentId;
    public string description;
    public bool selectable;

    public CLeafData() { }
    public CLeafData(string _id, string _parentId, string _description, bool _selectable)
    {
        this.id = _id;
        this.parentId = _parentId;
        this.description = _description;
        this.selectable = _selectable;
    }

}

[System.Serializable]
public class CAreaData
{
    public string id;
    public string parentId;
    public string description;
    public string meshname;

    public CAreaData() { }
    public CAreaData(string _id, string _parent, string _description, string _meshname)
    {
        this.id = _id;
        this.parentId = _parent;
        this.description = _description;
        this.meshname = _meshname;
    }
}

[System.Serializable]
public class CGeometryFileData
{    
    public string   fileName;
    public string   fileUrl;
    public string   fileType;
    public int      version;
    public string   crc;
    public string   assetName;
    public bool     active;
    public bool     invertZAxis;
    public double   position_x;
    public double   position_y;
    public double   position_z;
    public double   rotation_w;
    public double   rotation_x;
    public double   rotation_y;
    public double   rotation_z;


    public CGeometryFileData() { }
    public CGeometryFileData(string _fileName, string _fileUrl, string _fileType, int _version, string _assetName, bool _active, bool _invertZAxis, double _position_x, double _position_y, double _position_z, double _rotation_w, double _rotation_x, double _rotation_y, double _rotation_z)
    {        
        this.fileName = _fileName;
        this.fileUrl = _fileUrl;
        this.fileType = _fileType;
        this.version = _version;
        this.assetName = _assetName;
        this.active = _active;
        this.invertZAxis = _invertZAxis;
        this.position_x = _position_x;
        this.position_y = _position_y;
        this.position_z = _position_z;
        this.rotation_w = _rotation_w;
        this.rotation_x = _rotation_x;
        this.rotation_y = _rotation_y;
        this.rotation_z = _rotation_z;
    }

}

[System.Serializable]
public class CResourceFileData
{
    public string fileName;
    public string fileUrl;
    public string fileType;

    public CResourceFileData() { }
    public CResourceFileData(string _fileName, string _fileUrl, string _fileType) {
        this.fileName = _fileName;
        this.fileUrl = _fileUrl;
        this.fileType = _fileType;
    }
}

[System.Serializable]
public class CProductModel
{
    public string                   title;
    public string                   navigation_axis;
    public List<CNodeData>          nodes;              //List to store info coming from the json
    public List<CLeafData>          leaves;             //List to store info coming from the json
    public List<CAreaData>          areas;              //List to store info coming from the json
    public List<CGeometryFileData>  geometry_files;     //List to store info coming from the json
    public List<CResourceFileData>  resource_files;     //List to store info coming from the json

    List<GameObject>                                areaGameObjectList;                    //List to store a link to each of the areas
    Dictionary<string, CAreaDataDictionary>         area_DictionaryByID;                   //Store the areas Node by its ID <area_id, areaNodeData>
    Dictionary<string, List<string>>                area_DictionaryByLeafID;               //Store the areas Node by its Parent ID <parentarea_id, areas_id>        
    Dictionary<string, List<string>>                area_DictionaryBySpecialAncestorID;    //Store the areas Node by its SpecialAncestor ID <specialAncestor_id, areas_id>
    
    bool dictionariesInitialized;
    uint autoGeneratedID;

    public CProductModel()
    {
        Init();
    }

    public CProductModel(List<CNodeData> _nodes, List<CLeafData> _leaves, List<CAreaData> _areas)
    {
        Init();
        this.nodes = _nodes;
        this.leaves = _leaves;
        this.areas = _areas;    
    }

    public CProductModel(List<CNodeData> _nodes, List<CLeafData> _leaves, List<CAreaData> _areas, List<CGeometryFileData> _geometry_files)
    {
        Init();
        this.nodes = _nodes;
        this.leaves = _leaves;
        this.areas = _areas;
        this.geometry_files = _geometry_files;        
    }

    ///////////////////////////
    ///  INIT MODEL METHODS
    ///////////////////////////

    private void Init()
    {
        nodes = new List<CNodeData>();
        leaves = new List<CLeafData>();
        areas = new List<CAreaData>();
        geometry_files = new List<CGeometryFileData>();
        resource_files = new List<CResourceFileData>();
        areaGameObjectList = new List<GameObject>();
        //area_DictionaryByID = new Dictionary<string, CAreaDataDictionary>();
        //area_DictionaryByLeafID = new Dictionary<string, List<string>>();
        //area_DictionaryBySpecialAncestorID = new Dictionary<string, List<string>>();
        //dictionariesInitialized = false;
        InitDictionaries();
        autoGeneratedID = 0;
    }

    private void InitDictionaries()
    {
        area_DictionaryByID = new Dictionary<string, CAreaDataDictionary>();
        area_DictionaryByLeafID = new Dictionary<string, List<string>>();
        area_DictionaryBySpecialAncestorID = new Dictionary<string, List<string>>();
        dictionariesInitialized = false;
    }

    /// <summary>Initialization the product model manager to start to work</summary>
    /// <returns>true if everything went OK</returns>
    public bool SetupDictionaries()
    {
        bool re_setup = false;
        if (isInit())
        {
            // Check if is not the first time that we call setup (we are modifying the model)
            if (dictionariesInitialized) {
                re_setup = true;
                InitDictionaries();
            }    
            // Setup dictionaries                        
            if (nodes != null & leaves!=null & areas != null & geometry_files != null)
            {
                if (CreateAreasDictionaries())      //UPDATE Areas Dictionaries
                {
                    CreateSpecialNodeDictionary();    //UPDATE Special Nodes Dictionary   
                    dictionariesInitialized = true;
                    if (re_setup)
                    {
                        // Update dictionary with areas gameobject 
                        if (UpdateAreaDictionaryWithAreaGameObject()) {
                            return true;
                        }
                    }else { return true; }                
                }
            }
        }        
        return false;
    }


    /// <summary>Add a list of nodes, leaves, areas and geometry to a product model and rebuild the dictionaries.</summary>
    /// <param name="_nodes"></param>
    /// <param name="_leaves"></param>
    /// <param name="_areas"></param>
    /// <param name="listOfGeometryData"></param>
    /// <returns></returns>
    public bool Add(List<CNodeData> _nodes, List<CLeafData> _leaves, List<CAreaData> _areas, List<CGeometryFileData> listOfGeometryData)
    {

        if ((_nodes.Count == 0) && (_areas.Count == 0)) { return false; }      //Return false is inputs parameters are empty

        if (isInit() && dictionariesInitialized)
        {
            int lastValueNodes = nodes.Count;
            int lastValueLeaves = leaves.Count;
            int lastValueAreas = areas.Count;
            nodes.AddRange(_nodes);
            leaves.AddRange(leaves);
            areas.AddRange(_areas);
            geometry_files.AddRange(listOfGeometryData);

            //TODO Do we have to update more dictionaries?
            if (UpdateAreasDictionaries(lastValueAreas))       //UPDATE Areas Dictionaries
            {
                CreateSpecialNodeDictionary(lastValueNodes);    //Update
                return true;
            }
        }       
        return false;                  
    }

    /// <summary>Add one node the model</summary>
    /// <param name="_node">new node to be added</param>
    /// <returns>true if the node could be added</returns>
    public bool AddNode(CNodeData _node)
    {
        if (_node == null) { return false; }
        if (isInit() && dictionariesInitialized)
        {
            if (ItExist(_node.id)) { return false; }        // Check if the ID already exist
            if (!IsNode(_node.parentId)) { return false; }  // Check if the parent if exist            
            nodes.Add(_node);
            return true;
        }
        return false;
    }

    public bool isInit()
    {
        if ((this.nodes.Count > 0) && (this.leaves.Count > 0) && (this.areas.Count > 0))
        {
            return true;
        }
        return false;
    }

    public void Clear()
    {
        nodes.Clear();
        leaves.Clear();
        areas.Clear();
        geometry_files.Clear();
        areaGameObjectList.Clear();
        area_DictionaryByID.Clear();
        area_DictionaryByLeafID.Clear();
        area_DictionaryBySpecialAncestorID.Clear();
        dictionariesInitialized = false;
        autoGeneratedID = 0;
}
            
    private bool CreateAreasDictionaries()
    {
        foreach (CLeafData leaf in leaves)
        {
            List<CAreaData> areasByIsLeaf;
            if (!area_DictionaryByLeafID.ContainsKey(leaf.id))
            {

                areasByIsLeaf = areas.FindAll(r => r.parentId == leaf.id);                // Get areas kid of this leaf
                if (areasByIsLeaf.Count != 0)
                {
                    List<string> areasIdList = AddToAreaDictionary(areasByIsLeaf, leaf.selectable);   // Add areas to dictionary and get string of areas ids 
                    if (areasIdList.Count != 0)
                    {
                        area_DictionaryByLeafID.Add(leaf.id, areasIdList);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private bool UpdateAreasDictionaries(int initial)
    {
        for (int i = initial; i < leaves.Count; i++)
        {
            List<CAreaData> areasByIsLeaf;
            if (!area_DictionaryByLeafID.ContainsKey(leaves[i].id))
            {

                areasByIsLeaf = areas.FindAll(r => r.parentId == leaves[i].id);                // Get areas kid of this leaf
                if (areasByIsLeaf.Count != 0)
                {
                    List<string> areasIdList = AddToAreaDictionary(areasByIsLeaf, leaves[i].selectable);   // Add areas to dictionary and get string of areas ids 
                    if (areasIdList.Count != 0)
                    {
                        area_DictionaryByLeafID.Add(leaves[i].id, areasIdList);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }


    /// <summary>Add one area to the dictionary of areas order by areaId</summary>
    /// <param name="_area">area to be added</param>
    /// <param name="_selectable">if the area is selectable</param>
    /// <returns>True if the area has been added, false otherwise</returns>
    private bool AddToAreaDictionary(CAreaData _area, bool _selectable)
    {
        if (!area_DictionaryByID.ContainsKey(_area.id))
        {            
            CAreaDataDictionary temp = new CAreaDataDictionary(_area);
            temp.selectable = _selectable;        
            temp.gameobject_pointer = null;
            area_DictionaryByID.Add(temp.id, temp);   //Introduce the area data into the dictionary, by ID
            return true;
        }
        return false;
    }

    /// <summary>Add a list of areas to the dictionary of areas order by areaId</summary>
    /// <param name="_area">list of areas to be added</param>
    /// <param name="_selectable">if the areas are selectable</param>
    /// <returns>true if has been added</returns>
    private List<string> AddToAreaDictionary(List<CAreaData> areaList, bool selectable)
    {
        List<string> areaIdList = new List<string>();
        foreach (CAreaData area in areaList)
        {            
            if (AddToAreaDictionary(area, selectable)) { areaIdList.Add(area.id); }

        }
        return areaIdList;
    }


    /// <summary>Introduce into the area dictionary the Special Ancestor of a list of areas</summary>
    /// <param name="specialAncestorID">Special Ancestor ID</param>
    /// <param name="areaIDList">A list of areas that have this special ancestor</param>
    /// <returns>true if everything go well</returns>
    private bool UpdateAreaDictionary_WithSpecialAncestor(string specialAncestorID, List<string> areaIDList)
    {
        foreach (string area in areaIDList)
        {
            CAreaDataDictionary areaData = new CAreaDataDictionary();
            if (area_DictionaryByID.TryGetValue(area, out areaData))
            {
                areaData.specialAncestorID = specialAncestorID;         //Check if this area node is already in the dictionary.
            }
        }
        return true;
    }
        

    private void CreateSpecialNodeDictionary(int initial = 0)
    {
        List<CNodeData> specialNodesList = nodes.FindAll(r => r.special_node);      //Get all the special nodes
        foreach (CNodeData node in specialNodesList) {
            string specialAncestorID = node.id;
            List<string> leafList = this.GetAllLeafDescendantsOfNode(specialAncestorID);  // Find all the node leaf which hang from this node                
            List<string> areaIDList = this.GetAreaIDListByLeafIDList(leafList);           // Find all the areas behind that leafs                
            area_DictionaryBySpecialAncestorID.Add(specialAncestorID, areaIDList);        // Introduce into the dictionary by Ancestor, by AncestorID           
            UpdateAreaDictionary_WithSpecialAncestor(specialAncestorID, areaIDList);       // Introduce into the area dictionary
        }       
    }
    

    
    ///////////////////////////
    ///  GET & SET METHODS
    ///////////////////////////

    /// <summary>Return if an node ID is in the model</summary>
    /// <param name="nodeID">Node ID to check</param>
    /// <returns>boolean value</returns>
    public bool IsNode(string nodeID)
    {
        if (isInit()) { return (nodes.FindIndex(r => r.id == nodeID) != -1); }
        else { return false; }
    }

    /// <summary>Return true if a node is a Special node</summary>
    /// <param name="nodeID">string that contains the nodeID</param>
    /// <returns>true if is special node, false in all other cases</returns>
    public bool IsSpecialNode(string nodeID)
    {
        if (dictionariesInitialized) { return area_DictionaryBySpecialAncestorID.ContainsKey(nodeID); }
        else { return false; }
    }

    /// <summary>Return true if a node is a Leaf</summary>
    /// <param name="leafID">string that contains the leafID</param>
    /// <returns>true if is leaf, false in all other cases</returns>
    public bool IsLeaf(string leafID)
    {
        if (dictionariesInitialized) { return area_DictionaryByLeafID.ContainsKey(leafID); }
        else { return (leaves.FindIndex(r => r.id == leafID) != -1); }
    }

    /// <summary>Return true if all the list of nodes is a Leaf</summary>
    /// <param name="leavesIdList"></param>
    /// <returns></returns>
    public bool IsLeafList(List<string> leavesIdList)
    {        
        foreach (string leafId in leavesIdList)
        {
            if (!IsLeaf(leafId)) { return false; }
        }
        return true;
    }

    /// <summary>Return if an area ID is in the model</summary>
    /// <param name="areaID">Area ID to check</param>
    /// <returns>boolean value</returns>
    public bool IsArea(string areaID) {
        if (dictionariesInitialized)
        {
            return area_DictionaryByID.ContainsKey(areaID);
        }
        else
        {            
            return (areas.FindIndex(r => r.id == areaID) != -1);
        }
    }

    /// <summary>Check one id already exist in the model</summary>
    /// <param name="partID">id to check</param>
    /// <returns>true if the id a node, leaf or area.</returns>
    public bool ItExist(string partID)
    {
        return (IsNode(partID) || IsLeaf(partID) || IsArea(partID));
    }

    /// <summary>Return if an area is selectable</summary>
    /// <param name="areaID">Area ID to check</param>
    /// <returns>boolean value</returns>
    public bool IsAreaSelectable(string areaID)
    {
        if (dictionariesInitialized)
        {
            CAreaDataDictionary temp = new CAreaDataDictionary();
            if( area_DictionaryByID.TryGetValue(areaID, out temp))
            {
                return temp.selectable;
            }
        }
        return false;
    }

    /// <summary>Return if the one id is a node, leaf or area</summary>
    /// <param name="id">id to discover the type</param>
    /// <returns></returns>
    public string GetNodeType(string id)
    {

        if (IsNode(id)) return "node";
        else if (IsLeaf(id)) return "leaf";
        else if (IsArea(id)) return "area";
        else return null;
    }

    /// <summary>Change a node description</summary>
    /// <param name="_id">Node ID</param>
    /// <param name="_description">New description</param>
    /// <returns>Return true if the change has been made</returns>
    public bool SetNodeDescription(string _id, string _description) {
        if (isInit())
        {
            int index = nodes.FindIndex(r => r.id == _id);
            if (index != -1) {
                nodes[index].description = _description;
                return true;
            }
        }        
        return false;
    }

    /// <summary>Change a leaf description</summary>
    /// <param name="_id">Leaf ID</param>
    /// <param name="_description">New description</param>
    /// <returns>Return true if the change has been made</returns>
    public bool SetLeafDescription(string _id, string _description)
    {
        if (isInit())
        {
            int index = leaves.FindIndex(r => r.id == _id);
            if (index != -1)
            {
                leaves[index].description = _description;
                return true;
            }
        }
        return false;
    }

    /// <summary>Change an area description</summary>
    /// <param name="_id">Area ID</param>
    /// <param name="_description">New description</param>
    /// <returns>Return true if the change has been made</returns>
    public bool SetAreaDescription(string _id, string _description)
    {
        if (isInit())
        {
            int index = areas.FindIndex(r => r.id == _id);
            if (index != -1)
            {
                areas[index].description = _description;
                return true;
            }
        }
        return false;
    }

    /// <summary>Return the product model navigation axis</summary>
    /// <returns>string that contains the navigation axis</returns>
    public string GetNavigationAxis() { return navigation_axis; }

    /// <summary>Set a new navigation Axis</summary>
    /// <param name="newNavigationAxis">New navigation axis.</param>
    /// <returns>Return true if the changes has been done</returns>
    public bool SetNavigationAxis(string newNavigationAxis) {
        if (newNavigationAxis != null && newNavigationAxis != "")
        {
            navigation_axis = newNavigationAxis;
            return true;
        }
        return false;
    }

    /// <summary>Changes leaf parent</summary>
    /// <param name="_id">id of the leaf</param>
    /// <param name="newParentID">Id of the parent of this leaf(has to be a node)</param>
    /// <returns>Return true if the change was done</returns>
    public bool MoveLeaf(string _id, string newParentID)
    {
        if (isInit())
        {
            // Check if the new parent is a leaf
            if (!IsNode(newParentID)) { return false; }
            // Check if the area exist and move
            int index = leaves.FindIndex(r => r.id == _id);
            if (index != -1)
            {
                leaves[index].parentId = newParentID;   // Change area parent to the new one
                // Check if there are special node above
                if (GetSpecialNodeAbove(newParentID) == null) {
                    MakeNodeSpecial(newParentID);
                    ResolveSpecialNodeDuplicities(newParentID);
                }
                SetupDictionaries();        // Redo model setup                
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_listString"></param>
    /// <param name="newParentID"></param>
    /// <returns></returns>
    public bool MoveLeaves(List<string> _LeavesIdList, string newParentID)
    {        
        if (isInit())
        {
            // Check if the new parent is a node
            if (!IsNode(newParentID)) { return false; }
            // Check if all are leaf
            if (!IsLeafList(_LeavesIdList)) { return false; }
            // Move the leafs
            foreach (string leaf in _LeavesIdList)
            {
                int index = leaves.FindIndex(r => r.id == leaf);
                if (index != -1)
                {
                    leaves[index].parentId = newParentID;   // Change area parent to the new one
                }
            }

            //Check if there are any special-node above of the parent
            string specialNodeAbove = GetSpecialNodeAbove(newParentID);

            if (specialNodeAbove != null)
            {
                if (PushDownSpecialNode(specialNodeAbove)) { MakeNodeNoSpecial(specialNodeAbove); }
            }
            else
            {
                MakeNodeSpecial(newParentID);                   // If not we make it special
                ResolveSpecialNodeDuplicities(newParentID);     // Remove all the special node below it
            }
            SetupDictionaries();        // Redo model setup                
            return true;

        }
        return false;
    }

    /// <summary>Changes area parent</summary>
    /// <param name="_id">Id of the area</param>
    /// <param name="newParentID">Id of the parent of this area(has to be a leaf)</param>
    /// <returns>Return true if the change was done</returns>
    public bool MoveArea(string _id, string newParentID)
    {
        if (isInit())
        {
            // Check if the new parent is a leaf
            if (!IsLeaf(newParentID)) { return false; }            
            // Check if the area exist and move
            int index = areas.FindIndex(r => r.id == _id);
            if (index != -1)
            {                                    
                areas[index].parentId = newParentID;    // Change area parent to the new one               
                SetupDictionaries();                                // Redo model setup                
                return true;
            }
        }        
        return false;
    }

    

    /// <summary>Make a node special node</summary>
    /// <param name="nodeID">node to be converted in special node</param>
    /// <returns>true if everything go well</returns>
    private bool MakeNodeSpecial(string nodeID)
    {        
        CNodeData node = GetNodeByID(nodeID);
        if (node != null)
        {
            node.special_node = true;
            return true;
        }
        return false;
    }

    /// <summary>Make a node NO special node</summary>
    /// <param name="nodeID">node to be converted in No special node</param>
    /// <returns>true if everything go well</returns>
    private bool MakeNodeNoSpecial(string nodeID)
    {
        CNodeData node = GetNodeByID(nodeID);
        if (node != null)
        {
            node.special_node = false;
            return true;
        }
        return false;
    }

    /// <summary>Given a special node, look for any other node below it that is spatial and makes it non-special</summary>
    /// <param name="nodeID">special node id</param>
    /// <returns>true if everything go well</returns>
    private bool ResolveSpecialNodeDuplicities(string nodeID)
    {        
        if (!IsSpecialNode(nodeID)) { return false; }           //Check if it is special node
        List<string> listSpecialNodesID = new List<string>();
        // Get all the special nodes below this one
        List<CNodeData> childrenNode = nodes.FindAll(r => r.parentId == nodeID);
        foreach (CNodeData node in childrenNode)
        {         
            listSpecialNodesID = IsThereSpecialNodeBelow(node.id);         
        }
        // Make it no SpecialNode
        foreach (string id in listSpecialNodesID) {            
            MakeNodeNoSpecial(id);
        }        
        return true;
    }


    private bool PushDownSpecialNode(string nodeID)
    {
        if (!IsSpecialNode(nodeID)) { return false; }           //Check if it is special node

        List<CLeafData> childrenLeaves = leaves.FindAll(r => r.parentId == nodeID);
        if (childrenLeaves.Count == 0) {
            List<CNodeData> childrenNode = nodes.FindAll(r => r.parentId == nodeID);
            foreach (CNodeData node in childrenNode) {
                node.special_node = true;                
            }            
        }else
        {
            return false;
        }
        return true;
    }

    /// <summary>Check if there are any special node above  certain node recursively</summary>
    /// <param name="_id">Node Id</param>
    /// <returns>return the node id if exist, null otherwise </returns>
    private string GetSpecialNodeAbove(string _id)
    {
        int index = nodes.FindIndex(r => r.id == _id);

        if (nodes[index].special_node) { return nodes[index].id; }
        else if (nodes[index].parentId == "#") { return null; }
        else return (GetSpecialNodeAbove(nodes[index].parentId));
    }
    
    /// <summary>Check if there are any special node below a certain node recursively</summary>
    /// <param name="nodeID">nodeId</param>
    /// <returns>The list of special nodes, null if there isn't any</returns>
    private List<string> IsThereSpecialNodeBelow(string nodeID)
    {
        List<string> listSpecialNodes = new List<string>();
       
        int index = nodes.FindIndex(r => r.id == nodeID);       // Find node
        if (index == -1) { return null; }                       // If is not a node we return null
        if (nodes[index].special_node) {
            listSpecialNodes.Add(nodes[index].id);
            return listSpecialNodes;                            // If it is a special node we return it
        }
        else
        {
            //Is not a special Node - We find recursively all the nodes below it
            List<CNodeData> childrenNodes = nodes.FindAll(r => r.parentId == nodeID);
            foreach (CNodeData node in childrenNodes)
            {                
                listSpecialNodes.AddRange(IsThereSpecialNodeBelow(node.id));                
            }
            return listSpecialNodes;                
        }        
    }


    public bool RemoveNode(string _id)
    {
        if (isInit() && _id != "#")
        {
            CNodeData nodeToRemove = nodes.Find(r => r.id == _id);
            if (nodeToRemove != null)
            {
                // Find all the nodes children of this node and change its parent
                List<CNodeData> childrenNodes = nodes.FindAll(r => r.parentId == _id);
                foreach (CNodeData node in childrenNodes) { node.parentId = nodeToRemove.parentId; }
                // Find all the leaves children of this node and change its parent
                List<CLeafData> childrenLeaves = leaves.FindAll(r => r.parentId == _id);
                foreach (CLeafData leaf in childrenLeaves) { leaf.parentId = nodeToRemove.parentId; }
                //Check is the node is special
                if (nodeToRemove.special_node)
                {
                    MakeNodeSpecial(nodeToRemove.parentId);                   // If not we make it special
                    ResolveSpecialNodeDuplicities(nodeToRemove.parentId);     // Remove all the special node below it
                }
                // Remove the node
                if (nodes.Remove(nodeToRemove)) { return true; }
            }            
        }
        return false;
    }

    /// <summary>Remove a leaf from the tree if it has no area underneath it.</summary>
    /// <param name="_id">leaf id</param>
    /// <returns>true if the leaf has been removed</returns>
    public bool RemoveLeaf(string _id)
    {
        if (isInit())
        {
            int index = leaves.FindIndex(r => r.id == _id);
            if (index != -1)
            {
                List<string> areasList = new List<string>();
                if (area_DictionaryByLeafID.TryGetValue(_id, out areasList)) {
                    if (areasList.Count == 0)
                    {
                        leaves.RemoveAt(index);                 // Remove from leaves structure
                        area_DictionaryByLeafID.Remove(_id);    // Remove from leaves dictionary
                        return true;
                    }
                }                                
            }
        }
        return false;
    }

    /// <summary>Return the root Node ID, assuming that the root node parent is "#" or "".</summary>
    /// <returns>string that contains the root node ID</returns>
    public string GetRootNodeId()
    {
        int index = nodes.FindIndex(r => ((r.parentId == "#") || (r.parentId == "")));
        return nodes[index].id;
    }

    /// <summary>Return a new ID auto-generated</summary>
    /// <returns>string contains a new ID</returns>
    public string GetAutogeneratedID()
    {
        uint newID = 0;

        while (newID == 0)
        {
            if (!IsNode(autoGeneratedID.ToString()))
            {
                if (!IsLeaf(autoGeneratedID.ToString()))
                {
                    if (!IsArea(autoGeneratedID.ToString()))
                    {
                        newID = autoGeneratedID;    //This number does not exist as ID
                    }
                }                
            }
            autoGeneratedID++;
            if (autoGeneratedID == 4294967295) { break; }       //Avoid infinite loop
        }
        return newID.ToString();
    }

    /// <summary>Return a list of Special Ancestor ID behind a node ID</summary>
    /// <param name="nodeID">String that contains a node ID</param>
    /// <returns>special ancestor ID list</returns>
    public List<string> GetSpecialAncestorIDListByNodeID(string nodeID)
    {
        List<string> specialAncestorIDList = new List<string>();

        if (!IsNode(nodeID)) { return null; }
        else { return GetSpecialAncestorIDListByNodeID_Recursively(nodeID); }        
    }

    private List<string> GetSpecialAncestorIDListByNodeID_Recursively(string nodeID)
    {
        List<string> specialAncestorIDList = new List<string>();
        
        if (area_DictionaryBySpecialAncestorID.ContainsKey(nodeID))
        {
            specialAncestorIDList.Add(nodeID);
            return specialAncestorIDList;
        }
        else
        {
            //Is a Node - We have to find recursively                            
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].parentId == nodeID)
                {
                    List<string> temp = new List<string>();
                    temp = GetSpecialAncestorIDListByNodeID_Recursively(nodes[i].id);
                    if (temp != null) { specialAncestorIDList.AddRange(temp); }
                }
            }
            return specialAncestorIDList;
        }        
    }

    public string GetSpecialAncestorIDByAreaID(string areaID) {
        if (dictionariesInitialized){
            CAreaDataDictionary temp = new CAreaDataDictionary();
            if (area_DictionaryByID.TryGetValue(areaID, out temp))
            {
                return temp.specialAncestorID;
            }
        }
        return null;
    }

    /// <summary>Get area description</summary>
    /// <param name="_areaID">ID of the area </param>
    /// <returns>description</returns>
    public string GetAreaDescription(string _areaID)
    {
        CAreaData area = areas.Find(r => r.id == _areaID);
        if (area!=null) { return area.description; }        
        return null;
    }

    public string GetAreaParentIDByAreaID(string areaID)
    {
        if (dictionariesInitialized)
        {
            CAreaDataDictionary temp = new CAreaDataDictionary();
            if (area_DictionaryByID.TryGetValue(areaID, out temp))
            {
                return temp.parentId;
            }
        }
        return null;
    }

    /// <summary>Search all the areas hanging from a leaf</summary>
    /// <param name="leafList">Leaf ID</param>
    /// <returns>List of area IDs</returns>
    public List<string> GetAreaIDListByLeafID(string leafID)
    {
        List<string> areaIdList = new List<string>();
        if (isInit())
        {
            List<string> temp = new List<string>();    //Temporary list of string to store the areas ID                                
            if (area_DictionaryByLeafID.TryGetValue(leafID, out temp)) { areaIdList.AddRange(temp); }
            return areaIdList;
        }
        return null;
    }

    public GameObject GetAreaGameObjectByAreaID(string areaID)
    {
        if (dictionariesInitialized && areaID != null)
        {
            CAreaDataDictionary temp = new CAreaDataDictionary();
            if (area_DictionaryByID.TryGetValue(areaID, out temp))
            {
                return temp.gameobject_pointer;
            }
        }
        return null;
    }

    /// <summary>
    /// Return a list of al the areas gameobjects
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetAllAreaGameObjectList() {
        if (dictionariesInitialized) {
            List<GameObject> tempGameObjectList = new List<GameObject>();   //List of game objects to be return    
            foreach (var pair in area_DictionaryByID)
            {
                if (pair.Value.gameobject_pointer != null) { tempGameObjectList.Add(pair.Value.gameobject_pointer);}
            }
            return tempGameObjectList;
        }
        return null;
    }

    public List<GameObject> GetAreaGameObjectListByLeafID(string leafID)
    {
        if (dictionariesInitialized)
        {
            List<GameObject> gameObjectList = new List<GameObject>();   //List of game objects to be return            
            List<string> temp = new List<string>();   //Temporary list of string to store the areas ID

            if (area_DictionaryByLeafID.TryGetValue(leafID, out temp))
            {
                //AreaNodeData areaNode = new AreaNodeData();      //Here we have a list of areas IDs, we need a list of gameobjecs
                foreach (string areaID in temp)
                {
                    GameObject tempObj = this.GetAreaGameObjectByAreaID(areaID);
                    if (tempObj != null) { gameObjectList.Add(tempObj); }
                }
                return gameObjectList;             
            }
        }
        return null;
    }

    public List<GameObject> GetAreaGameObjectListByNodeID(string nodeID)
    {
        if (dictionariesInitialized)
        {            
            List<string> leafList = this.GetAllLeafDescendantsOfNode(nodeID);     //Find all the node leaf behind this node

            List<GameObject> gameObjectList = new List<GameObject>();                
            foreach (string cell in leafList)
            {
                List<GameObject> temp = new List<GameObject>();
                temp = GetAreaGameObjectListByLeafID(cell);
                if (temp != null) { gameObjectList.AddRange(temp); }
            }
            return gameObjectList;
        }
        return null;
    }

    public List<GameObject> GetAreaGameObjectListBySpecialAncestorID(string specialAncestorID)
    {
        if (dictionariesInitialized)
        {
            List<string> tempListAreas = new List<string>();
            tempListAreas = GetAreaIDListBySpecialAncestorID(specialAncestorID);      //Find the areaID list in the Ancestor dictionary           
            if (tempListAreas != null)
            {
                List<GameObject> tempGameObjectList = new List<GameObject>();
                foreach (string areaID in tempListAreas)
                {
                    GameObject temp = GetAreaGameObjectByAreaID(areaID);
                    if (temp != null) { tempGameObjectList.Add(temp); }
                }
                return tempGameObjectList;
            }
        }
        return null;
    }

    public List<string> GetAreaIDListBySpecialAncestorID(string specialAncestorID)
    {
        if (dictionariesInitialized)
        {
            List<string> tempListAreas = new List<string>();
            if (area_DictionaryBySpecialAncestorID.TryGetValue(specialAncestorID, out tempListAreas))
            {
                return tempListAreas;
            }
        }
        return null;
    }

    public string GetLeafDescriptionByLeafID(string leafID)
    {
        if (dictionariesInitialized)
        {
            CLeafData temp = new CLeafData();
            temp = leaves.Find(r => r.id == leafID);
            if (temp != null)
            {
                return temp.description;
            }
        }
        return null;
    }

    /*public List<string> GetNodeChildsIdByNodeID(string nodeID)
    {
        List<string> childrenIdList = null;
        if (isInit())
        {
            List<CNodeData> children = nodes.FindAll(r => r.parentId == nodeID);
            if (children.Count > 0)
            {
                childrenIdList = new List<string>();
                for-each (CNodeData child in children) { childrenIdList.Add(child.id); }
            }            
        }
        return childrenIdList;
    }*/

    public CNodeData GetNodeByID(string nodeID)
    {
        int index = nodes.FindIndex(r => r.id == nodeID);
        if (index != -1) { return nodes[index]; }
        return null;
    }

    public string GetNodeParentIDByNodeID(string nodeID)
    {
        if (dictionariesInitialized)
        {
            CNodeData temp = new CNodeData();
            temp = nodes.Find(r => r.id == nodeID);
            if (temp != null)
            {
                return temp.parentId;
            }
        }
        return null;
    }

    public string GetNodeDescriptionByNodeID(string nodeID)
    {
        if (dictionariesInitialized)
        {
            CNodeData temp = new CNodeData();
            temp = nodes.Find(r => r.id == nodeID);                      
            if (temp!=null)
            {
                return temp.description;
            }
        }
        return null;
    }

    /// <summary>Update the area gameobject pointer into the Area dictionary. And the area object with its id</summary>
    /// <param name="meshname">area mesh-name</param>
    /// <param name="obj">pointer to the gameobject</param>
    /// <returns>True if everything went well</returns>
    public bool UpdateAreaDictionaryAndGameObject(string meshname, GameObject obj)
    {
        if (dictionariesInitialized)
        {            
            foreach (var pair in area_DictionaryByID)
            {
                if (pair.Value.meshname.ToUpper() == meshname.ToUpper())
                {
                    pair.Value.gameobject_pointer = obj;                            //Store the gameobject of this area
                    areaGameObjectList.Add(obj);                                    //Store the gameobject of this area
                    obj.GetComponent<ObjectStateManager>().areaID = pair.Key;       //Store the ID in this area                    
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>Update the area gameobject pointer into the Area dictionary.</summary>
    /// <returns>True if everything went well</returns>
    public bool UpdateAreaDictionaryWithAreaGameObject()
    {
        if (dictionariesInitialized)
        {
            foreach (GameObject areaObj in areaGameObjectList)
            {
                CAreaDataDictionary areaData = new CAreaDataDictionary();
                if (area_DictionaryByID.TryGetValue(areaObj.GetComponent<ObjectStateManager>().areaID, out areaData))
                {
                    areaData.gameobject_pointer = areaObj;
                }
            }
            return true;
        }
        return false;
    }

    

    public List<GameObject> GetAllAreaGameobjects()
    {
        return areaGameObjectList;
    }

    /// <summary>Search recursively all leaves hanging from a node</summary>
    /// <param name="nodeID">A node of the product model tree</param>
    /// <returns>A list of IDs of leaves</returns>
    private List<string> GetAllLeafDescendantsOfNode(string nodeID)
    {
        List<string> leafIDList = new List<string>();

        // Add all the leaf that are direct children
        List<CLeafData> leavesChildren = leaves.FindAll(r => r.parentId == nodeID);
        foreach (CLeafData leaf in leavesChildren) { leafIDList.Add(leaf.id); }
        
        //Find all the nodes children of this one and its leaf children and so on
        List<CNodeData> nodesChildren  = nodes.FindAll(r => r.parentId == nodeID);
        if (nodesChildren.Count != 0) {            
            foreach (CNodeData node in nodesChildren) {
                leafIDList.AddRange(GetAllLeafDescendantsOfNode_Recursively(node.id));
            }
        }        
        return leafIDList;
    }


    /// <summary>Search recursively all leaves hanging from a node.</summary>
    /// <param name="nodeID">A node of the product model tree</param>
    /// <returns>A list of IDs of leaves</returns>
    private List<string> GetAllLeafDescendantsOfNode_Recursively(string nodeID)
    {
        List<string> leafNodeIDList;
        leafNodeIDList = new List<string>();

        //if is a leaf we have finished
        List<CLeafData> leafList = leaves.FindAll(r=> r.parentId == nodeID);
        if (leafList != null)
        {
            foreach (CLeafData leaf in leafList) { leafNodeIDList.Add(leaf.id); }            
            return leafNodeIDList;
        }
        else
        {
            //Is a Node - We have to find recursively all the leafs behind this node                                  
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].parentId == nodeID)
                {
                    leafNodeIDList.AddRange(GetAllLeafDescendantsOfNode(nodes[i].id));
                }
            }
            return leafNodeIDList;
        }        
    }
   
    //private List<string> GetLeafIDListByNodeID(string nodeID)
    //{
    //    List<string> leafNodeIDList = new List<string>();

    //    leafNodeIDList = leaves.FindAll(r => r.parentId == nodeID).ConvertAll<string>(r => r.id);
        
    //    for-each (CLeafData leaf in leaves)
    //    {
    //        if (leaf.parentId == nodeID) { leafNodeIDList.Add(leaf.id);}
    //    }
    //    return leafNodeIDList;
    //}

    /// <summary>Search all the areas hanging from a list of leaf node IDs.</summary>
    /// <param name="leafList">List of leaf nodes</param>
    /// <returns>List of area IDs</returns>
    private List<string> GetAreaIDListByLeafIDList(List<string> leafList)
    {
        List<string> areaIdList = new List<string>();
        if (isInit())
        {
            foreach (string cell in leafList)
            {
                List<string> temp = new List<string>();    //Temporary list of string to store the areas ID                                
                if (area_DictionaryByLeafID.TryGetValue(cell, out temp)) { areaIdList.AddRange(temp); }
            }
            return areaIdList;
        }
        return null;
    }
        
    public string GetGeometryFileRotation(int fileId) {
        string fileRotation = null;

        if (fileId < geometry_files.Count)
        {
            fileRotation = "(";
            fileRotation += geometry_files[fileId].rotation_w.ToString() + ",";
            fileRotation += geometry_files[fileId].rotation_x.ToString() + ",";
            fileRotation += geometry_files[fileId].rotation_y.ToString() + ",";
            fileRotation += geometry_files[fileId].rotation_z.ToString() + ")";
        }
        
        return fileRotation;
    }
}

[System.Serializable]
public class CFileProductModel
{
    public string model_name;
    public string navigation_axis;
    public List<CNodeData> nodes;                   //List to store info coming from the json
    public List<CLeafData> leaves;                  //List to store info coming from the json
    public List<CAreaData> areas;                   //List to store info coming from the json
    public List<CGeometryFileData> geometry_files;  //List to store info coming from the json
    public List<CResourceFileData> resource_files;  //List to store info coming from the json

    public CFileProductModel() { }

    public CFileProductModel(CProductModel _model)
    {
        this.model_name = _model.title;
        this.navigation_axis = _model.navigation_axis;  
        this.nodes = _model.nodes;
        this.leaves = _model.leaves;
        this.areas = _model.areas;
        this.geometry_files = _model.geometry_files;
        this.resource_files = _model.resource_files;
    }
}