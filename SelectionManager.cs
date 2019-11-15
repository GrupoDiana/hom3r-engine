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
    void Awake () 
	{
        //We create a new blank list
        gameObjectIndicatedList = new List<GameObject>();       //Initially the list is empty
        gameObjectConfirmedList = new List<GameObject>();       //Initially the list is empty
        nodeLeafID_ConfirmedList = new List<string>();          //Initially the list is empty
        specialNodeID_ConfirmedList = new List<string>();       //Initially the list is empty       
    }


    public void ResetSelectionLists()
    {        
        gameObjectIndicatedList.Clear();
        gameObjectConfirmedList.Clear();
        nodeLeafID_ConfirmedList.Clear();
        specialNodeID_ConfirmedList.Clear();
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
    public int GetNumberOfIndicatedGameObjects()
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
        bool controlNull = false;
        if (GetNumberOfIndicatedGameObjects() > 0)
        {
            List<GameObject> temp = new List<GameObject>(gameObjectIndicatedList);
            foreach (GameObject obj in temp)
            {
                if (obj != null) {                 
                    obj.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Indication_Off));
                } else
                {
                    controlNull = true;
                }
            }            
        }
        if (controlNull) { RemoveNullFromIndicationList(); }
    }
    
    private void RemoveNullFromIndicationList()
    {
        List<GameObject> temp = gameObjectIndicatedList.FindAll(item => item == null);        
        foreach (GameObject item in temp)
        {
            gameObjectIndicatedList.Remove(item);
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
                if (go!=null && go.GetComponent<ObjectStateManager>() != null)
                {
                    //go.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Off);
                    go.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Off));
                }
            }
            //Clean the List
            gameObjectConfirmedList.Clear();
            nodeLeafID_ConfirmedList.Clear();
            specialNodeID_ConfirmedList.Clear();
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_AllPartsDeselected));
        }
    }

    
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
    // Colour Methods //
    //////////////////////

    public void ClearSelectionColour()
    {
        List<GameObject> allAreaGameObjects = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().ClearAreaSelectionColour();
        if (allAreaGameObjects == null) { return; }
        foreach (var obj in allAreaGameObjects)
        {            
            obj.GetComponent<ObjectStateManager>().ResetColours();
        }
    }



    //////////////////////
    // MANAGER Methods //
    //////////////////////
    /// <summary>
    /// Action coming from the Web APP (Select in a different way if is Area or Node) 
    /// </summary>
    /// <param name="partId"></param>
    public void SelectPartFromInterface(string partId, string colour)
    {                      
        if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(partId))
        {
            this.SelectArea(partId, colour);
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(partId))
        {
            this.SelectLeaf(partId, colour);
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsNode(partId))
        {
            this.SelectNode(partId, colour);
        }
        else
        {
            //It is not an valid ID
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ShowMessage, "This product part, id = " + partId + ", does not exist."));
        }
    }


    private void SelectArea(string areaID, string colour)
    {
        //Is an AREA                   
        GameObject objectArea = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);
        if (objectArea != null)
        {
            if (!this.IsConfirmedGameObject(objectArea))
            {
                float duration = 0.0f;
                // If it is now hidden we have to do it with a delay
                if (hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().IsRemovedGameObject(objectArea)) { duration = 1.5f; }
                //Select that area.                            
                // objectArea.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Multiple_On, duration, colour);
                objectArea.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Multiple_On, duration, colour));

                //Add component to the list if is apropiate                            
                string _specialNodeID = hom3r.coreLink.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                if (this.AreAllAreaOfSpecialNodeSeleted(_specialNodeID))
                {
                    this.AddConfirmedSpecialNode(_specialNodeID);
                }

            }
        }
    }

    private void SelectLeaf(string leafID, string colour)
    {
        //Update Component selected list
        if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(leafID))
        {
            List<string> areaList = new List<string>();
            areaList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaList_ByLeafID(leafID);
            if (areaList != null)
            {
                foreach (string area in areaList)
                {
                    SelectArea(area, colour);
                }
            }
        }
        else
        {
            // ERROR  
        }
    }

    private void SelectNode(string nodeID, string colour)
    {
        //Update Component selected list
        if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsSpecialNode(nodeID))
        {
            this.AddConfirmedSpecialNode(nodeID);    //Is SpecialNode
        }
        else
        {
            //Is not SpecialNode but is it a specialnode parent?
            List<string> specialNodeKidsList = new List<string>();
            specialNodeKidsList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetSpecialAncestorIDList_ByProductID(nodeID);
            if (specialNodeKidsList != null)
            {
                foreach (var item in specialNodeKidsList) { this.AddConfirmedSpecialNode(item); }
            }
        }
        List<GameObject> objectAreaList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(nodeID);     //Find the list of areas to select

        //Select the list of objects
        foreach (var obj in objectAreaList)
        {
            // obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Multiple_On, colour);
            obj.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Multiple_On, colour));
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
    /// Execute the indication of an Area or Component
    /// </summary>
    /// <param name="obj">Area GameObject to indicate</param>
    public void ExecuteIndication(GameObject obj)
    {
        if (IsConfirmedGameObject(obj)) {
            this.GetComponent<SelectionManager>().IndicationGameObjectAllOFF();
            return;
        }
        if (IsIndicatedGameObject(obj)) { return; }
        //Get the areaIDs of the area selected        
        string areaID = obj.GetComponent<ObjectStateManager>().areaID;
        if (areaID != null)
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsAreaSelectable(areaID))
            {
               
                List<GameObject> _areaList = new List<GameObject>();                

                if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
                {
                    // obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Indication_On);
                    obj.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Indication_On));
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
                        // item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Indication_Multiple_On);
                        item.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Indication_Multiple_On));
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


    /// <summary>
    /// Execute the confirmation of an Area or Component
    /// </summary>
    /// <param name="obj">Area GameObject to indicate</param>
    /// <param name="multiple">True if we are executing multiple confirmation</param>
    public void ExecuteConfirmation(GameObject obj, bool multiple)
    {
        List<GameObject> _areaList = new List<GameObject>();
        List<string> _areaIDList = new List<string>();
        //Get the areaIDs of the area selected
        //List<string> areaIDs = new List<string>(obj.GetComponent<ObjectStateManager>().areaID);
        string areaID = obj.GetComponent<ObjectStateManager>().areaID;
        if (areaID != null)
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsAreaSelectable(areaID))
            {
                //Filled _areaList to be selected in a different way, depending of the selection mode
                string _specialNodeID = null;

                if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
                {//Select only the area selected by the mouse
                    _areaList.Add(obj);
                    //_areaIDList = areaIDs;
                    _areaIDList.Add(areaID);
                }
                else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
                {
                    //Select all the areas with the same special ancestor. Suppose all the IDs of the area have the same ancestor.
                    _specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                    _areaList = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(_specialNodeID);                  //Get all the area
                    _areaIDList.Add(_specialNodeID);                    //Get all the areaIds
                }

                //Select an AREA in 3D scenario if it is allowed by the mode.

                if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUIAutomaticSelection())
                //if (hom3r.state.automaticSelection)
                {
                    //Deselect new area in 3D Scene     
                    if (!multiple) { ExecuteDesconfirmationAll(_specialNodeID, _areaList); }
                    //Confirm all the areas 
                    foreach (var item in _areaList)
                    {
                        // item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Multiple_On);
                        item.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Multiple_On));
                    }

                    //Update the list of component selected 
                    if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
                    {
                        GetComponent<SelectionManager>().AddConfirmedSpecialNode(_specialNodeID);
                    }
                    else
                    {
                        _specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                        if (hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().AreAllAreaOfSpecialNodeSeleted(_specialNodeID))
                        {
                            this.GetComponent<SelectionManager>().AddConfirmedSpecialNode(_specialNodeID);
                        }
                    }
                    //I emit an event warning that the confirmation has ended
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOnFinished, _areaIDList));
                }
                else
                {
                    //I emit an event warning that the confirmation has ended            
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOnFinished, _areaIDList));
                }
            }
        }
        else
        {
            string _message = "Cannot find " + obj.name + " area in product model ";
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ShowMessage, _message));
        }
    }


    /// <summary>
    /// Execute the des-confirmation of one object
    /// </summary>
    /// <param name="obj"></param>
    public void ExecuteDesconfirmation(GameObject obj)
    {
        List<string> _areaIDList = new List<string>();

        // If the object is not confirmed we finish 
        if (!hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().IsConfirmedGameObject(obj)) { return; }

        string areaID = obj.GetComponent<ObjectStateManager>().areaID;     //Get the areaIDs of the area selected
        if (areaID != null)
        {
            string _specialNodeID = null;
            List<GameObject> _areaList = new List<GameObject>();
            _specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
            //Filled _areaList to be selected in a different way, depending of the selection mode
            if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
            {
                //Select only the area selected by the mouse
                _areaList.Add(obj);
                _areaIDList.Add(areaID);
            }
            else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
            {
                //Select all the areas with the same special ancestor.                                    
                _areaList = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(_specialNodeID);

                if (hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().AreAllAreaOfSpecialNodeSeleted(_specialNodeID))
                {
                    //Deselect the full specialNode
                    _areaIDList.Add(_specialNodeID);
                }
                else
                {
                    //Deselect only the selected areas
                    //Get all the areaIds
                    _areaIDList = this.GetComponent<ModelManager>().GetAreaIDList_BySpecialAncestorID(_specialNodeID);
                }
            }
            //Deselect an AREA in 3D scenario if it is allowed by the mode.
            if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUIAutomaticSelection())
            //if (hom3r.state.automaticSelection)
            {
                //Desconfirm in the 3D Scenario  
                foreach (var item in _areaList)
                {
                    // item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Off);
                    item.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Off));
                }
                this.GetComponent<SelectionManager>().RemoveConfirmedSpecialNode(_specialNodeID);
            }
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, _areaIDList));    //Emit an event warning that the desconfirmation has ended
        }
        else
        {
            ////Remove labels with the name of the GO
            //if (hom3r.hom3rStatus.currentLabelMode == Hom3rLabelModes_Type.SHOWLABEL)
            //{
            //    //Remove the label of the desconfirmed object
            //    hom3r.coreLink.Do(new CLabelCommand(TLabelCommands.RemoveAllLabelOfConfirmedObjects), Constants.undoNotAllowed);                
            //}            
            // obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Off);        //Desconfirm every object
            obj.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Off));
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, _areaIDList));    //I emit an event warning that the desconfirmation has ended
        }

        //Update Smart Transparency
        //if (hom3r.hom3rStatus.smartTransparencyModeActive) { this.GetComponent<Core>().Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed); }        
    }

    /// <summary>
    /// Des-confirm all the confirmed objects except the indicated in the list.
    /// </summary>
    /// <param name="exceptionSpecialNode">Special that is not going to be des-confirmed</param>
    /// <param name="exceptionList">List of areas that are not going to be des-confirmed</param>
    private void ExecuteDesconfirmationAll(string exceptionSpecialNode, List<GameObject> exceptionList)
    {
        List<GameObject> _areaList = new List<GameObject>();
        List<string> listOfConfirmedSpecialNodeList = new List<string>();

        listOfConfirmedSpecialNodeList = new List<string>(this.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList());
        if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
        {
            listOfConfirmedSpecialNodeList = new List<string>(this.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList());
            listOfConfirmedSpecialNodeList.Remove(exceptionSpecialNode);
            if (listOfConfirmedSpecialNodeList.Count != 0)
            {
                //Get the list of areas to deselect                                    
                foreach (var item in listOfConfirmedSpecialNodeList)
                {
                    _areaList.AddRange(this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(item));
                }
                //Deselect into the 3D                                                                 
                foreach (var item in _areaList)
                {
                    //item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Off);
                    item.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Off));
                }
                this.GetComponent<SelectionManager>().RemoveAllConfirmedSpecialNode();      //Update the list of component selected 

                //Send the areas ID to be deselected to WebApp                                      
                //this.GetComponent<IO_Script>().ToWebApp_DeselectPart(listOfConfirmedSpecialNodeList);
                //updateOthers = true;

                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, listOfConfirmedSpecialNodeList));    //Emit an event warning that the desconfirmation has ended
            }
        }

        this.GetComponent<SelectionManager>().RemoveAllConfirmedSpecialNode();  //Clean confirmed componet list        
        List<GameObject> listOfConfirmedObjects = new List<GameObject>(this.GetComponent<SelectionManager>().GetListOfConfirmedObjects());  // Check if exist more areas and unconfirmed all        
        listOfConfirmedObjects = listOfConfirmedObjects.FindAll(x => !exceptionList.Contains(x));                                           // If a object is not a exceptionList. 

        if (listOfConfirmedObjects.Count != 0)
        {
            List<string> listOfConfirmedAreasID = new List<string>();
            //Deselect into the 3D                                                                 
            foreach (var item in listOfConfirmedObjects)
            {
                //item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Confirmation_Off);
                item.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Confirmation_Off));
                listOfConfirmedAreasID.Add(item.GetComponent<ObjectStateManager>().areaID);
            }

            ////this.GetComponent<IO_Script>().ToWebApp_DeselectAllParts(); //Send the areas ID to be deselected to WebApp
            //this.GetComponent<IO_Script>().ToWebApp_DeselectPart(listOfConfirmedAreasID);
            //updateOthers = true;

            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, listOfConfirmedAreasID));    //Emit an event warning that the desconfirmation has ended                  
        }
        //if (updateOthers)
        //{
        //    //Update the rays for the smartTransparency multiple rayscast
        //    if (hom3r.hom3rStatus.smartTransparencyModeActive) { this.GetComponent<Core>().Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed); }
        //}
    }

    ///////////////////////
    /// RAYCAST METHODS ///
    ///////////////////////

    public void IndicationByMousePosition(Vector3 mousePosition)
    {
        // If is not activated we do nothing
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUISelection()) { return; }
        if (hom3r.state.selectionBlocked) { return; }
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
        // If is not activated we do nothing
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveUISelection()) { return; }
        if (hom3r.state.selectionBlocked) { return; }
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
                    // this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.ConfirmationOff, obj)), Constants.undoNotAllowed);
                    ExecuteDesconfirmation(obj);
                }
                else 
                {
                    if (keyControlPressed)
                    {
                        //Multiple Confirmation, des-confirm one by one
                        // this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.ConfirmationOff, obj)), Constants.undoNotAllowed);
                        ExecuteDesconfirmation(obj);
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
                // Debug.Log("hom3r: CONFIRM the ray-cast object: " + obj.name);
                this.RemoveFromIndicatedList(obj);
                //CONFIRM the ray-cast object
                //Multiple CONFIRMATION                
                if (keyControlPressed)
                {
                    // Debug.Log("MULTIPLE Selection Active");                                            
                    // this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.MultipleConfirmation, obj)), Constants.undoNotAllowed);
                    this.ExecuteConfirmation(obj, true);
                }
                //Single CONFIRMATION
                else
                {
                    // Debug.Log("SINGLE Selection Active");                                            
                    // this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.SingleConfirmationByMouse, obj)), Constants.undoNotAllowed);
                    this.ExecuteConfirmation(obj, false);
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

