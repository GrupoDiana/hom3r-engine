/*******************TRANSPARENCY_SCRIPT****************************************************
 * 
 * Script implmented to make, unmake and more actions with transparent objects
 * 
 * 
 * Creation Date:  26/03/2015
 * Last Update: 08/04/2015
 *
 * UiW European Project
 * Grupo DIANA - University of Malaga
 ***************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    List<GameObject> gameObjectIndicatedList;             //In this list will be stored the gameobjects that have been made indicated
    List<GameObject> gameObjectConfirmedList;             //In this list will be stored the gameobjects that have been made confirmed
    
    List<string> nodeLeafID_ConfirmedList;      //List of components confirmed. Element to be Isolated.This list can have repeat elements.
    List<string> specialNodeID_ConfirmedList;      //List of components confirmed. Element to be Isolated.This list can have repeat elements.
            

    /////////////////////////////////////////////////////////////////////////// 
    /// <summary> Start this instance. Use this for initialization.</summary> //
    ///////////////////////////////////////////////////////////////////////////
    void Start () 
	{
        //We create a new blank list
        gameObjectIndicatedList = new List<GameObject>();       //Initially the list is empty
        gameObjectConfirmedList = new List<GameObject>();       //Initially the list is empty
        nodeLeafID_ConfirmedList = new List<string>();          //Initially the list is empty
        specialNodeID_ConfirmedList = new List<string>();       //Initially the list is empty       
    }

    /////////////////////////////////////////////////////////////////////////
    /////////////////////////// INDICATION Methods //////////////////////////
    /////////////////////////////////////////////////////////////////////////

    /// <summary> Check if an object is Indicated.</summary> //
    public bool IsIndicatedGameObject(GameObject obj)
    {
        return gameObjectIndicatedList.Contains(obj);
    }

    /// <summary>Method to know the number of indicated game objects.</summary>
    /// <returns>The number of confirmed game objects.</returns>
    public int NumberOfIndicatedGameObjects()
    {
        return gameObjectIndicatedList.Count;
    }

    /// <summary>Add object to indication list</summary>
    /// <param name="obj">Object to be indicated</param>
    public void AddToIndicatedList(GameObject obj)
    {
        //Check if the object is already indicated              
        if (!IsIndicatedGameObject(obj))
        {
            //Check if the indicated object it's a confirmated object
            if (!IsConfirmedGameObject(obj))
            {
                //AddToIndicatedList(obj);
                gameObjectIndicatedList.Add(obj);
            }
            else
            {
                // If the raycasted obj is a confirmed object, we indicated off the previous one (if exist)             
                IndicationGameObjectAllOFF();
            }
        }
    }

    /// <summary>Indicated off one objet </summary>
    /// <param name="obj">Object to be indicated off</param>    
    public void RemoveFromIndicatedList(GameObject obj)
    {
        if (IsIndicatedGameObject(obj))
        {                       
            gameObjectIndicatedList.Remove(obj);
        }
    }
    
    /// <summary> Remove the indication state of all the objects.</summary>	    
    public void IndicationGameObjectAllOFF()
    {
        if (NumberOfIndicatedGameObjects() > 0)
        {
            List<GameObject> temp = new List<GameObject>(gameObjectIndicatedList);            
            foreach (GameObject obj in temp)
            {            
                obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Indication_Off);
            }
            
        }
    }
    
   

    
    /////////////////////////////////////////////////////////////////////////
    /////////////////////////// CONFIRMATION Methods ////////////////////////
    /////////////////////////////////////////////////////////////////////////

    /////////////////////////////////////////////////////////////
    /// <summary> Check if an object is Confirmed.</summary> //
    /////////////////////////////////////////////////////////////
    public bool IsConfirmedGameObject(GameObject obj)
	{
		return gameObjectConfirmedList.Contains (obj);
    }//END IsConfirmedGameObject

    /////////////////////////////////////////////////////////////////
    /// <summary>Return the number of confirmed game objects.</summary>
    /// <returns>The number of confirmed game objects.</returns>
    //////////////////////////////////////////////////////////////// 
    public int GetNumberOfConfirmedGameObjects()
	{
		return gameObjectConfirmedList.Count;
	}//NumberOfConfirmedGameObjects


    /// <summary>Return the list of confirmed game objects.</summary>
    /// <returns>List of GameObjets</returns>    
    public IEnumerable<GameObject> GetListOfConfirmedObjects()
    {
        return gameObjectConfirmedList.AsReadOnly();
    }

    /// <summary>Return the list IDs of confirmed game objects.</summary>
    /// <returns>List of GameObjets</returns>
    public List<string> GetListOfConfirmedObjectsIDs()
    {
        List<string> gameObjectIDConfirmedList;
        gameObjectIDConfirmedList = new List<string>();

        foreach (GameObject _area in gameObjectConfirmedList)
        {
            gameObjectIDConfirmedList.Add(_area.GetComponent<ObjectStateManager>().areaID);
        }
        return gameObjectIDConfirmedList;
    }//GetListOfConfirmedObjectsIDs

    /////////////////////////////////////////////////////
    /// <summary>Confirm ON an object.</summary> //
    /////////////////////////////////////////////////////
    public void ConfirmGameObjectON(GameObject obj)
    {
        if (obj != null) { 
            if (!IsConfirmedGameObject(obj))
            {                
                //We store the object that we are confirming into the list.
                gameObjectConfirmedList.Add(obj);
                //Confirm the Component             
                string objID=null;
                if (obj.GetComponent<ObjectStateManager>().areaID != null) { objID = obj.GetComponent<ObjectStateManager>().areaID; }               
                string objparent_ID = this.GetComponent<ModelManager>().GetNodeLeafID_ByAreaID(objID);                
                if ((objparent_ID != null)) { nodeLeafID_ConfirmedList.Add(objparent_ID); }
            }
        }
    }//confirmOneObject

    /////////////////////////////////////////////////////
    /// <summary>Confirm OFF an object.</summary> //
    /////////////////////////////////////////////////////
    private void ConfirmGameObjectOFF(GameObject obj)
    {
        if (obj != null) { 
            if (IsConfirmedGameObject(obj))
            {
                //We remove the object from the list
                gameObjectConfirmedList.Remove(obj);
                //Confirm off the component                    
                string objID = obj.GetComponent<ObjectStateManager>().areaID;
                string objparent_ID = this.GetComponent<ModelManager>().GetNodeLeafID_ByAreaID(objID);
                //FIXME Check that no one of is childres is selected.
                if ((objparent_ID != null) && (nodeLeafID_ConfirmedList.Contains(objparent_ID)))
                    nodeLeafID_ConfirmedList.Remove(objparent_ID);
            }
        }
    }//ConfirmGameObjectOFF

    ///////////////////////////////////////////////////////////////
    ///<summary>Confirm OFF all the objects confirmed </summary>
    ///////////////////////////////////////////////////////////////
    public void ConfirmALLGameObjectOFF(bool sentToWebApp=true)
    {
        if (gameObjectConfirmedList.Count != 0)
        {
            if (sentToWebApp)
            {
                //Send the areaIDs to be deselected to the WebAPP
                List<string> _deselectAreaIDList = this.GetListOfConfirmedObjectsIDs();
                //this.GetComponent<IO_Script>().ToWebApp_DeselectPart(_deselectAreaIDList);
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, _deselectAreaIDList));

            }            
            //Desconfirm all objects
            List<GameObject> temp = new List<GameObject>(gameObjectConfirmedList);
            foreach (GameObject go in temp)
            {
                if (go.GetComponent<ObjectStateManager>() != null)
                {
                    go.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
                }
            }
            //Clean the List
            gameObjectConfirmedList.Clear();
            nodeLeafID_ConfirmedList.Clear();
            specialNodeID_ConfirmedList.Clear();
        }
    }//ConfirmALLGameObjectOFF

    
    //////////////////////////////////////////////////
    /// <summary>Confirm OFF and objected</summary>
    /// <param name="obj">object that is going to be confirm off</param>
    //////////////////////////////////////////////////
    public void ConfirmSingleGameObjectOFF(GameObject obj)
    {
        ConfirmGameObjectOFF(obj);        
    }//ConfirmSingleGameObjectOFF

    ///////////////////////////////////////////////////////////
    /// <summary>Confirmation of an object.</summary> //
    ///////////////////////////////////////////////////////////
    public void ConfirmGameObject(GameObject obj, string type)
    {        
        //1. Delete the indicated GameObject because now it is the confirmed GO
        IndicationGameObjectAllOFF();
        if (type == "single")
        {
            //2. Des-confirm all previous objects
            ConfirmALLGameObjectOFF();
        }
        //3. Add to the list
        ConfirmGameObjectON(obj);        
    }//END ConfirmSingleGameObjectON

    /////////////////////////////////////////////////////////////////////////
    //////////////////// LEAF       CONFIRMATION Methods /////////////////////
    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Returns if an object is part of a selected component
    /// </summary>
    /// <param name="obj">object</param>
    /// <returns>True if is part of a selected component</returns>
    public bool IsConfirmedNodeLeaf(GameObject obj)
    {
        //We seek the parentID of the object
        if (obj.GetComponent<ObjectStateManager>() != null)
        {
            string objID = obj.GetComponent<ObjectStateManager>().areaID;
            if (objID != null)
            {
                string nodeLeafID = this.GetComponent<ModelManager>().GetNodeLeafID_ByAreaID(objID);
                //We seek the father, NodeLeaf, in the model            
                if (nodeLeafID != null) { return nodeLeafID_ConfirmedList.Contains(nodeLeafID); }
            }
        }
        return false;
    }//END IsConfirmedNodeLeaf

    /// <summary>
    /// Return the list of Game Objects that belong to the components confirmed 
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetListOfComponentConfirmedObjects()
    {
        List<GameObject> componentConfirmedObjectsList;
        componentConfirmedObjectsList = new List<GameObject>();
               
        componentConfirmedObjectsList = this.GetComponent<ModelManager>().GetAreaGameObjectList_ByNodeLeafIDList(removeDuplicates(nodeLeafID_ConfirmedList));     
        return componentConfirmedObjectsList;
    }

    /////////////////////////////////////////////////////////////////////////
    //////////////////// COMPONENT CONFIRMATION Methods /////////////////////
    /////////////////////////////////////////////////////////////////////////    

    public bool IsConfirmedSpecialNode(string _specialNodeID)
    {
        return specialNodeID_ConfirmedList.Contains(_specialNodeID);
    }

    public int GetNumberOfConfirmedSpecialNode()
    {
        return specialNodeID_ConfirmedList.Count;
    }//GetNumberOfConfirmedSpecialNode

    public void AddConfirmedSpecialNode(string _specialNodeID)
    {
        if (_specialNodeID != null && !specialNodeID_ConfirmedList.Contains(_specialNodeID))
        {
            specialNodeID_ConfirmedList.Add(_specialNodeID);
        }       
    }

    public void RemoveConfirmedSpecialNode(string _specialNodeID)
    {
        if (_specialNodeID!=null && specialNodeID_ConfirmedList.Contains(_specialNodeID))
        {
            specialNodeID_ConfirmedList.Remove(_specialNodeID);
        }
    }

    public void RemoveAllConfirmedSpecialNode()
    {
       specialNodeID_ConfirmedList.Clear();
    }

    public IEnumerable<string> GetConfirmedSpecialNodeList()
    {
        return specialNodeID_ConfirmedList;
    }


    /// <summary>
    /// Return if a special node is complete selected
    /// </summary>
    /// <param name="specialNodeID"></param>
    /// <returns></returns>
    public bool AreAllAreaOfSpecialNodeSeleted(string specialNodeID)
    {
        //Get list of confirmed areas
        List<GameObject> confirmedAreas = new List<GameObject>(this.GetListOfConfirmedObjects());
        //Get the list of children areas of each special node
        List<GameObject> AllAreasOfaSpecialNode = new List<GameObject>(this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(specialNodeID));

        if (confirmedAreas.Count < AllAreasOfaSpecialNode.Count)
        {
            return false;
        }
        else
        {
            bool control = false;
            foreach (var item in AllAreasOfaSpecialNode)
            {
                if (confirmedAreas.Contains(item)) {
                    control = true;
                } else {
                    return false;
                }
            }
            return control;
        }
    }



    //////////////////////
    // Auxiliar Methods //
    //////////////////////

    /// <summary>
    /// Remove the string duplicate in a list of strings
    /// </summary>
    /// <param name="inputList">List of strings</param>
    /// <returns>List of string without repeated elements</returns>
    private List<string> removeDuplicates(List<string> inputList)
    {
        Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
        List<string> finalList = new List<string>();
        foreach (string currValue in inputList)
        {
            if (!uniqueStore.ContainsKey(currValue))
            {
                uniqueStore.Add(currValue, 0);
                finalList.Add(currValue);
            }
        }
        return finalList;
    }//removeDuplicates




    /// <summary>
    /// Execute the indication of and Area or Component
    /// </summary>
    /// <param name="obj"></param>
    public void ExecuteIndication(GameObject obj)
    {
        //Get the areaIDs of the area selected        
        string areaID = obj.GetComponent<ObjectStateManager>().areaID;
        if (areaID != null)
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsAreaSelectable(areaID))
            {
                List<GameObject> _areaList = new List<GameObject>();                

                if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
                {
                    obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Indication_On);                    
                }
                else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
                {
                    //Select all the areas with the same special ancestor.
                    string _specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                    _areaList = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(_specialNodeID);
                    IndicationGameObjectAllOFF();               //Remove indication of the previous objects
                    //Multiple indication
                    foreach (var item in _areaList)
                    {
                        item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Indication_Multiple_On);
                    }
                }
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_IndicationOnFinished, obj));
            }
            else
            {
                this.GetComponent<SelectionManager>().IndicationGameObjectAllOFF();     //Remove indication of the previous objects
            }
        }
        else
        {
            //Debug.Log("This object cannot be select because is not on the product model. The object name is " + obj.name);
            //this.GetComponent<Core_Script>().Do(new UICoreCommand("UI/SetActiveAlertText", "This product part cannot be selected because it is not in the product model. PartName = " + obj.name), Constants.undoNotAllowed);
        }
    }
    private void IndicateList()
    {

    }



    ///////////////////////
    /// RAYCAST METHODS ///
    ///////////////////////
    
    public void IndicationByMousePosition(Vector3 mousePosition)
    {
        GameObject obj = Raycast(mousePosition);
        
        if (obj != null) {
            //INDICATE the ray-cast object
            ExecuteIndication(obj);
        }
        else {
            // "DE-INDICATE" the indicated object   
            //this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.IndicateOffByMouse)), Constants.undoNotAllowed);
            IndicationGameObjectAllOFF();
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_IndicationOffFinished));
        }

    }

    public void ConfirmByMouseLeftClickAndMousePosition(Vector3 mousePosition, GameObject obj, bool keyControlPressed)
    {
        // GameObject obj = Raycast(mousePosition);
        if (obj != null)
        {
            
            if (IsConfirmedGameObject(obj))
            {
                //"DESCONFIRM" the ray-cast object if the object is already confirmed                                
                
                //Check if we have only one component or area selected
                bool onlyOneComponentOrArea = false;
                if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
                {
                    onlyOneComponentOrArea = (GetNumberOfConfirmedGameObjects() == 1);
                }
                else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
                {
                    onlyOneComponentOrArea = (GetNumberOfConfirmedSpecialNode() == 1);
                }

                //If is only one we deselect this one                    )
                if (onlyOneComponentOrArea)
                {
                    this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.ConfirmationOff, obj)), Constants.undoNotAllowed);
                }
                else 
                {
                    if (keyControlPressed)
                    {
                        //Multiple Confirmation, des-confirm one by one
                        this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.ConfirmationOff, obj)), Constants.undoNotAllowed);
                    }
                    else
                    {
                        //Multiple Des-confirmation, des-confirm all except the selected one
                        this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.Multiple_Confirmation_Desconfirmation, obj)), Constants.undoNotAllowed);
                    }
                }
            }            
            else
            {
                //CONFIRM the ray-cast object
                //Multiple CONFIRMATION
                if (keyControlPressed)
                {
                    //Debug.Log("MULTIPLE Selection Active");                                            
                    this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.MultipleConfirmation, obj)), Constants.undoNotAllowed);
                }
                //Single CONFIRMATION
                else
                {
                    //Debug.Log("SINGLE Selection Active");                                            
                    this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.SingleConfirmationByMouse, obj)), Constants.undoNotAllowed);
                }
            }

        }
        else
        {
            // User has clicked in a empty space
            // Un-confirm all the objects
            if (!hom3r.state.selectionBlocked)
            {
                ConfirmALLGameObjectOFF();
            }            
        }
    }

    private GameObject Raycast(Vector3 mouseCurrentPosition)
    {       
        // Convert mouse position from screen space to three-dimensional space
        // Build ray, directed from mouse position to “camera forward” way
        Ray ray = Camera.main.ScreenPointToRay(mouseCurrentPosition);
        int productRoot = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, productRoot))
        {            
            GameObject rayCastedGO_current = hit.collider.gameObject;       // Now, let’s determine intersected GameObject
            return rayCastedGO_current;
        }
        else
        {
            return null;
        }
    }

    /// <summary>Method that use Ray Casting technique to make Indications and Confirmations.</summary>
    private void SelectObjectByMousePosition()
    {
        //METHOD OPERATION
        // When we pass the mouse over an object we indicate it
        // When we press the left button on an indicated object we confirm it
        // When we press the left button on a confirmed object we unselect it
        // When we press the left button and the key "Control" we make multiple confirmation

        GameObject rayCastedGO_current;         //Object ray-cast

        //Convert mouse position from screen space to three-dimensional space
        //Build ray, directed from mouse position to “camera forward” way
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //find ray intersection with any object in scene
        //if intersection exists, function «Physics.Raycast» returns «true» and sets «hit» variable        

        int productRoot = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, productRoot))
        {
            // Now, let’s determine intersected GameObject
            rayCastedGO_current = hit.collider.gameObject;
            
            //ExecuteIndication(rayCastedGO_current);     //INDICATE the ray-cast object

            //Select the ray-casted object when mouse-click
            if (Input.GetMouseButtonDown(0))
            {
                
            }
        }
        //NO RAYCASTED object
        else
        {
            // "DE-INDICATE" the indicated object            
           // this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.IndicateOffByMouse)), Constants.undoNotAllowed);
        }
    }//END SelectObjectByMousePosMainCamera


}

