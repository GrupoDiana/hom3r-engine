using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoSelectionCommand, UndoSelectionCommand);  //Subscribe a method to the event delegate        
    }

    private void OnDisable()
    {        
        hom3r.coreLink.UnsubscribeCommandObserver(DoSelectionCommand, UndoSelectionCommand);        
    }

    private void DoSelectionCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CSelectionCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoSelectionCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CSelectionCommand)) { command.Undo(this); }
        else { /*  Do nothing */ }
    }

       


    //////////////////////////////////////// Auxiliary Methods ///////////////////////////////////

   

    public void ExecuteConfirmation(GameObject obj, bool multiple)
    {
        List<GameObject> _areaList = new List<GameObject>();
        List<string> _areaIDList = new List<string>();
        //Get the areaIDs of the area selected
        //List<string> areaIDs = new List<string>(obj.GetComponent<ObjectStateManager>().areaID);
        string areaID = obj.GetComponent<ObjectStateManager>().areaID;
        if (areaID != null)
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsAreaSelectable(areaID)) {
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

                if (hom3r.state.automaticSelection)
                {
                    //Deselect new area in 3D Scene     
                    if (!multiple) { ExecuteDesconfirmationAll(_specialNodeID, _areaList); }
                    //Confirm all the areas 
                    foreach (var item in _areaList)
                    {
                        item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Multiple_On);
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
            if (hom3r.state.automaticSelection)
            {
                //Desconfirm in the 3D Scenario  
                foreach (var item in _areaList)
                {
                    item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
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
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);        //Desconfirm every object

            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, _areaIDList));    //I emit an event warning that the desconfirmation has ended
        }

        //Update Smart Transparency
        //if (hom3r.hom3rStatus.smartTransparencyModeActive) { this.GetComponent<Core>().Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed); }        
    }


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
                    item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
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
                item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
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
}


/// <summary>Selection Command</summary>
public enum TSelectionCommands
{
    ChangeHierarchyOfSelectionMode,
    IndicateByMouse, IndicateOffByMouse,
    SingleConfirmationByMouse, ConfirmationOff,
    MultipleConfirmation, Multiple_Confirmation_Desconfirmation,
    SelectPart, DeselectPart, DeselectAllParts, ClearSelectionColour,
    BlockSelection

};

/// <summary>Selection data</summary>
public class CSelectionCommandData
{
    public TSelectionCommands commandEvent;

    public GameObject obj { get; set; }             //Use in Selection and Confirmation           
    public string partID { get; set; }
    public string colour { get; set; }
    public bool selectionBlocked { get; set; }

    public CSelectionCommandData(TSelectionCommands _commandEvent) { this.commandEvent = _commandEvent; }

}

/// <summary>A 'ConcreteCommand' class</summary>
public class CSelectionCommand : CCoreCommand
{
    public CSelectionCommandData data;

    //////////////////
    // Constructors //
    //////////////////
    public CSelectionCommand(TSelectionCommands _command)
    {
        data = new CSelectionCommandData(_command);
    }
    public CSelectionCommand(TSelectionCommands _command, GameObject _obj)
    {
        data = new CSelectionCommandData(_command);
        data.obj = _obj;
    }
    public CSelectionCommand(TSelectionCommands _command, bool _selectionBlocked)
    {
        data = new CSelectionCommandData(_command);

        data.selectionBlocked = _selectionBlocked;
    }
    public CSelectionCommand(TSelectionCommands _command, string _partID, string _colour = "")
    {
        data = new CSelectionCommandData(_command);
        data.partID = _partID;
        data.colour = _colour;
    }

    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (data != null)
        {
            switch (data.commandEvent)
            {
                case TSelectionCommands.ChangeHierarchyOfSelectionMode:
                    if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
                    {
                        hom3r.state.currentSelectionMode = THom3rSelectionMode.SPECIAL_NODE;
                    }
                    else
                    {
                        hom3r.state.currentSelectionMode = THom3rSelectionMode.AREA;
                    }
                    break;
                case TSelectionCommands.IndicateByMouse:
                    if (!hom3r.state.selectionBlocked)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ExecuteIndication(data.obj);
                        //hom3r.quickLinks.scriptsObject.GetComponent<SelectionCommandReceiver>().ExecuteIndication(data.obj);                        
                        //hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_IndicationOnFinished, data.obj));
                    }

                    break;
                case TSelectionCommands.IndicateOffByMouse:
                    if (!hom3r.state.selectionBlocked)
                    {
                        m.GetComponent<SelectionManager>().IndicationGameObjectAllOFF();                        
                        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_IndicationOffFinished));
                    }
                    break;

                case TSelectionCommands.SingleConfirmationByMouse:
                    if (!hom3r.state.selectionBlocked)
                    {
                        if (data.obj.GetComponent<ObjectStateManager>() != null)
                        {
                            m.GetComponent<SelectionCommandReceiver>().ExecuteConfirmation(data.obj, false);
                        }
                    }
                    break;

                case TSelectionCommands.ConfirmationOff:
                    if (!hom3r.state.selectionBlocked) { m.GetComponent<SelectionCommandReceiver>().ExecuteDesconfirmation(data.obj); }
                    break;

                case TSelectionCommands.MultipleConfirmation:
                    if (!hom3r.state.selectionBlocked)
                    {
                        if (data.obj.GetComponent<ObjectStateManager>() != null)
                        {
                            m.GetComponent<SelectionCommandReceiver>().ExecuteConfirmation(data.obj, true);
                        }
                    }
                    break;

                case TSelectionCommands.Multiple_Confirmation_Desconfirmation:
                    //Arrive here when click on an area that is already selected
                    if (!hom3r.state.selectionBlocked)
                    {                        
                        List<GameObject> _areaList = new List<GameObject>();
                        List<string> _areaIDList = new List<string>();
                        
                        //Get the areaID an special node ID of the area of interest
                        //List<string> areaIDs = new List<string>(data.obj.GetComponent<ObjectStateManager>().areaID);
                        string areaID = data.obj.GetComponent<ObjectStateManager>().areaID;
                        string _specialNodeID = m.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                        
                        //Get the list of confirmed objects
                        List<GameObject> listOfConfirmedObjects = new List<GameObject>(m.GetComponent<SelectionManager>().GetListOfConfirmedObjects());
                        List<string> listOfConfirmedSpecialNodeList = new List<string>(m.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList());

                        if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
                        {
                            //Check if is it a correct area
                            if (listOfConfirmedObjects.Count != 0 & areaID != null)
                            {
                                //We have to delete all the areas except this one
                                listOfConfirmedObjects.Remove(data.obj);
                                _areaList = listOfConfirmedObjects;                                
                                //Deselect into the 3D                                                                 
                                foreach (var item in _areaList)
                                {
                                    item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
                                }
                                // Update the list of component selected
                                m.GetComponent<SelectionManager>().RemoveAllConfirmedSpecialNode();
                                
                                // Emmit events
                                foreach (var itemArea in _areaList) { _areaIDList.Add(itemArea.GetComponent<ObjectStateManager>().areaID); }
                                //Send the areas ID to be deselected to WebApp                                      
                                //m.GetComponent<IO_Script>().ToWebApp_DeselectPart(_areaIDList);

                                m.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, _areaIDList));
                                m.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Selection_AreaConfirmationOn, data.obj));

                                
                                //Update more things
                                //updateOthers = true;
                            }
                        }
                        else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
                        {
                            if (listOfConfirmedObjects.Count != 0)
                            {
                                //Check if is it a correct area
                                if (areaID != null && _specialNodeID != null)
                                {
                                    //Check if the component is complete confirm, if not confirm it                                                                        
                                    m.GetComponent<SelectionCommandReceiver>().ExecuteConfirmation(data.obj, true);

                                    //Delete all the special_nodes except this one
                                    listOfConfirmedSpecialNodeList.Remove(_specialNodeID);
                                    //Get the list of areas to deselect                                    
                                    foreach (var item in listOfConfirmedSpecialNodeList)
                                    {
                                        _areaList.AddRange(m.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(item));                                        
                                        _areaIDList.Add(item);
                                    }
                                    
                                    //Deselect into the 3D                                                                 
                                    foreach (var item in _areaList)
                                    {
                                        item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
                                    }
                                    //Update the list of component selected 
                                    m.GetComponent<SelectionManager>().RemoveAllConfirmedSpecialNode();
                                    m.GetComponent<SelectionManager>().AddConfirmedSpecialNode(_specialNodeID);
                                    //Update more things

                                    //Send the areas ID to be deselected to WebApp                                      
                                    //m.GetComponent<IO_Script>().ToWebApp_DeselectPart(_areaIDList); //FIXME should use a command
                                    m.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Selection_ConfirmationOffFinished, _areaIDList));
                                    //updateOthers = true;
                                }
                            }
                        }                       
                    }

                    break;

                case TSelectionCommands.SelectPart:
                    //Action coming from the Web APP (Select in a different way if is Area or Node)                    
                    if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(data.partID))
                    {
                        this.SelectArea(data.partID, data.colour);
                    }
                    else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(data.partID))
                    {
                        this.SelectLeaf(data.partID, data.colour);
                    }
                    else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsNode(data.partID))
                    {
                        this.SelectNode(data.partID, data.colour);
                    }
                    else
                    {
                        //It is not an valid ID
                        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_ShowMessage, "This product part, id = " + data.partID + ", does not exist."));                        
                    }
                    break;

                case TSelectionCommands.DeselectPart:
                    //Action coming from the Web APP. We have to deselect in a different way if is and Area or and Node                                                            
                    if (m.GetComponent<ModelManager>().IsArea(data.partID))
                    {
                        this.DeselectAreaNode(data.partID);
                    }
                    else if (m.GetComponent<ModelManager>().IsNode(data.partID))
                    {
                        //Is a Node?
                        this.DeselectProductNode(data.partID);                        
                    }
                    break;

                case TSelectionCommands.DeselectAllParts:                    
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Selection_AllPartsDeselected));
                    m.GetComponent<SelectionManager>().ConfirmALLGameObjectOFF(false);
                    break;

                case TSelectionCommands.ClearSelectionColour:
                    List<GameObject> temp = m.GetComponent<ModelManager>().ClearAreaSelectionColour();
                    foreach (var obj in temp)
                    {
                        obj.GetComponent<ObjectStateManager>().ResetColours();
                    }
                    break;

                case TSelectionCommands.BlockSelection:
                    hom3r.state.selectionBlocked = data.selectionBlocked;
                    if (data.selectionBlocked)
                    {                        
                        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().IndicationGameObjectAllOFF();
                    }
                    break;

                default:
                    Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Selecction command without command.");
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }

    private void SelectArea(string areaID, string colour)
    {
        //Is an AREA                   
        GameObject objectArea = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);
        if (objectArea != null)
        {
            if (!hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().IsConfirmedGameObject(objectArea))
            {
                //Select that area.                            
                objectArea.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Multiple_On, data.colour);
                                
                //Add component to the list if is apropiate                            
                string _specialNodeID = hom3r.coreLink.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                if (hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().AreAllAreaOfSpecialNodeSeleted(_specialNodeID)) {
                    hom3r.coreLink.GetComponent<SelectionManager>().AddConfirmedSpecialNode(_specialNodeID);
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
            hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().AddConfirmedSpecialNode(nodeID);    //Is SpecialNode
        }
        else
        {
            //Is not SpecialNode but is it a specialnode parent?
            List<string> specialNodeKidsList = new List<string>();
            specialNodeKidsList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetSpecialAncestorIDList_ByProductID(nodeID);
            if (specialNodeKidsList != null)
            {
                foreach (var item in specialNodeKidsList) { hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().AddConfirmedSpecialNode(item); }
            }
        }        
        List<GameObject> objectAreaList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(nodeID);     //Find the list of areas to select

        //Select the list of objects
        foreach (var obj in objectAreaList) {
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Multiple_On, colour);
        }                
    }

    private void DeselectAreaNode(string areaID)
    {
        GameObject objectArea1 = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);
        if (objectArea1 != null)
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().IsConfirmedGameObject(objectArea1))
            {
                //Is an AREA - select only that area                                                                               
                objectArea1.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);    //Deselect the part
                                                                                                                        //Deselect Special Node
                string _specialNodeID = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().RemoveConfirmedSpecialNode(_specialNodeID);
            }
        }
    }

    private void DeselectProductNode(string nodeID)
    {
        //Update special node selected list
        if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsSpecialNode(nodeID))
        {
            hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().RemoveConfirmedSpecialNode(nodeID);     //It is SpecialNode
        }
        else
        {
            //It's not SpecialNode but is it a specialnode parent?
            List<string> specialNodeKidsList = new List<string>();
            specialNodeKidsList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetSpecialAncestorIDList_ByProductID(nodeID);
            if (specialNodeKidsList != null)
            {
                foreach (var item in specialNodeKidsList) { hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().RemoveConfirmedSpecialNode(item); }
            }
        }
        
        List<GameObject> objectAreaList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(nodeID);  //Find a list of areas
        
        //Deselect the list of objects
        foreach (var obj in objectAreaList)
        {
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Confirmation_Off);
        }        
    }
}
