using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {        
        hom3r.coreLink.SubscribeCommandObserver(DoLabelCommand, UndoLabelCommand);  //Subscribe a method to the event delegate        
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoLabelCommand, UndoLabelCommand);    //Unsubscribe a method to the event delegate
    }

    private void DoLabelCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CLabelCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoLabelCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CLabelCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }

        
    //////////////////////////////////////// Auxiliary Methods ///////////////////////////////////


    /// <summary>Add label to every confirmed area/component in the confirmed list</summary>
    public void ExecuteAddLabel_ToAllConfirmedGameObject()
    {
        //1. Tag Components first
        //Get the list of the special nodes
        IEnumerable<string> _specialNodeList = this.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList();
        foreach (string nodeID in _specialNodeList)
        {
            //Get the list of children areas of each special node
            List<GameObject> everyAreaOfaSpecialNode = new List<GameObject>(this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(nodeID));
            if (everyAreaOfaSpecialNode.Count != 0)
            {
                //Get the name of the special node in the product model
                string nodeDescription = this.GetComponent<ModelManager>().GetProductNodeFullDescription_ByNodeID(nodeID);
                nodeDescription = "Component:\n" + nodeDescription;
                //Add label to the component
                this.GetComponent<LabelManager>().AddLabelToGameObjectGroup(nodeID, everyAreaOfaSpecialNode, nodeDescription);
            }
        }

        //2. Tag areas if its special ancestor is not tagged yet
        IEnumerable<GameObject> _objectAreaList = this.GetComponent<SelectionManager>().GetListOfConfirmedObjects();
        foreach (GameObject areaToLabel in _objectAreaList)
        {
            //Check if the area ancestor has label
            string areaAncestor = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaToLabel.GetComponent<ObjectStateManager>().areaID);
            List<string> temp_specialNodeList = new List<string>(this.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList());
            //If the ancestor is confirmed -> add label to the ancestor
            if (!temp_specialNodeList.Contains(areaAncestor))
            {
                //Get area IDs
                //List<string> areaIDs = new List<string>(areaToLabel.GetComponent<ObjectStateManager>().areaID);
                string areaID = areaToLabel.GetComponent<ObjectStateManager>().areaID;
                //Get the name of the areas with its parent
                string areaDescription = "";
                //foreach (var itemID in areaIDs)
                //{
                    if (areaDescription != "") { areaDescription += ", "; }
                    areaDescription += this.GetComponent<ModelManager>().GetAreaFullDescription_ByAreaID(areaID);
                //}
                //string areaDescription = this.GetComponent<ModelManagement_Script>().GetAreaFullDescription_ByAreaID(areaToLabel.GetComponent<ObjectState_Script>().areaID);
                areaDescription = "Area:\n" + areaDescription;
                //Add label to the area
                bool largeText = areaDescription.Length > Constants.largeText;
                this.GetComponent<LabelManager>().AddLabelToSurface(areaToLabel.GetComponent<ObjectStateManager>().areaID, areaToLabel, areaDescription, largeText);
            }
            else
            {
                if (!this.GetComponent<LabelManager>().LabelContains(areaAncestor))
                {
                    //Get the list of children areas of each special node
                    List<GameObject> everyAreaOfaSpecialNode = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(areaAncestor);
                    string nodeDescription = this.GetComponent<ModelManager>().GetProductNodeFullDescription_ByNodeID(areaAncestor);
                    nodeDescription = "Component:\n" + nodeDescription;
                    //Add label to the component
                    this.GetComponent<LabelManager>().AddLabelToGameObjectGroup(areaAncestor, everyAreaOfaSpecialNode, nodeDescription);
                }
            }
        }
    }

    /// <summary>Remove every label that show the name of the component/area</summary>
    public void ExecuteRemoveAllLabelOfConfirmedGOList()
    {
        //Remove area labels
        IEnumerable<GameObject> objectArea_List = this.GetComponent<SelectionManager>().GetListOfConfirmedObjects();
        foreach (GameObject areaToRemoveLabel in objectArea_List)
        {
            //Get area ID
            string areaToRemoveLabel_ID = areaToRemoveLabel.GetComponent<ObjectStateManager>().areaID;
            this.GetComponent<LabelManager>().RemoveLabel(areaToRemoveLabel_ID);
        }

        //Remove component labels
        IEnumerable<string> specialNodeID_List = this.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList();
        foreach (string specialNodeToRemoveLabel in specialNodeID_List)
        {
            this.GetComponent<LabelManager>().RemoveLabel(specialNodeToRemoveLabel);
        }
    }

    /// <summary>
    /// Add label to a specific confirmed Area, given by the area gameObect. When the selection come from ASP HOM3R has to work in the Hom3rSelectionMode_Type.AREA mode always
    /// </summary>
    /// <param name="areaToLabel"></param>
    public void ExecuteAddLabel_ToAGivenArea(GameObject areaToLabel)
    {
        //Check if the area ancestor has label
        string areaAncestor = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaToLabel.GetComponent<ObjectStateManager>().areaID);
        List<string> temp_specialNodeList = new List<string>(this.GetComponent<SelectionManager>().GetConfirmedSpecialNodeList());
        //If the ancestor is confirmed -> add label to the ancestor
        if (!temp_specialNodeList.Contains(areaAncestor)) // !this.GetComponent<Labels_Script>().LabelContains(this.GetComponent<ModelManagement_Script>().GetSpecialAncestorID_ByAreaID(labelID))
        {
            //string areaDescription = this.GetComponent<ModelManagement_Script>().GetAreaFullDescription_ByAreaID(areaToLabel.GetComponent<ObjectState_Script>().areaID);
            //Get area IDs
            //List<string> areaIDs = new List<string>(areaToLabel.GetComponent<ObjectStateManager>().areaID);
            string areaID = areaToLabel.GetComponent<ObjectStateManager>().areaID;
            //Get the name of the areas with its parent
            string areaDescription = "";
            //foreach (var itemID in areaIDs)
            //{
                if (areaDescription != "") { areaDescription += ", "; }
                areaDescription += this.GetComponent<ModelManager>().GetAreaFullDescription_ByAreaID(areaID);
            //}
            areaDescription = "Area:\n" + areaDescription;
            //Add label to the area
            bool largeText = areaDescription.Length > Constants.largeText;
            this.GetComponent<LabelManager>().AddLabelToSurface(areaToLabel.GetComponent<ObjectStateManager>().areaID, areaToLabel, areaDescription, largeText);
        }
        else
        {
            if (!this.GetComponent<LabelManager>().LabelContains(areaAncestor))
            {
                //Get the list of children areas of each special node
                List<GameObject> everyAreaOfaSpecialNode = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(areaAncestor);
                string nodeDescription = this.GetComponent<ModelManager>().GetProductNodeFullDescription_ByNodeID(areaAncestor);
                nodeDescription = "Component:\n" + nodeDescription;
                //Add label to the component
                this.GetComponent<LabelManager>().AddLabelToGameObjectGroup(areaAncestor, everyAreaOfaSpecialNode, nodeDescription);
            }
        }
    }

    /// <summary>
    /// Add label to a specific node, given by its ID
    /// </summary>
    /// <param name="specialNodeID"></param>
    public void ExecuteAddLabel_ToAGivenSpecialNode(string specialNodeID)
    {
        //Add the label if it doesn't exist yet
        if (!this.GetComponent<LabelManager>().LabelContains(specialNodeID))
        {
            //Get the list of children areas of each special node
            List<GameObject> everyAreaOfaSpecialNode = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(specialNodeID);
            //Get the name of the special node in the product model
            string nodeDescription = this.GetComponent<ModelManager>().GetProductNodeFullDescription_ByNodeID(specialNodeID);
            nodeDescription = "Component:\n" + nodeDescription;
            //Add label to the component
            this.GetComponent<LabelManager>().AddLabelToGameObjectGroup(specialNodeID, everyAreaOfaSpecialNode, nodeDescription);
        }
    }
}



/// <summary>Label commands</summary>
public enum TLabelCommands
{
    AddLabel,
    AddAutomaticLabelToArea, AddAutomaticLabelToSpecialNode,
    RemoveLabel, RemoveAllLabel, RemoveAllLabelOfConfirmedObjects,
    LabelModeManager, ShowPartNameLabel,
    LabelUISelection
}

/// <summary>Label command data</summary>
public class CLabelCommandData
{
    public TLabelCommands command;

    public string labelID { get; set; }
    public string partID { get; set; }
    public string text { get; set; }
    public Vector3 position { get; set; }
    public bool status { get; set; }
    public GameObject obj { get; set; }
    public string specialNode { get; set; }

    public CLabelCommandData(TLabelCommands _command)
    {
        this.command = _command;
    }
}

/// <summary>A 'Concrete Command' class</summary>
public class CLabelCommand : CCoreCommand
{
    public CLabelCommandData data;

    //////////////////
    // Constructors //
    //////////////////
    public CLabelCommand(TLabelCommands _command)
    {
        data = new CLabelCommandData(_command);
    }
    public CLabelCommand(TLabelCommands _command, bool _status)
    {
        data = new CLabelCommandData(_command);
        data.status = _status;
    }
    public CLabelCommand(TLabelCommands _command, string _specialNode)
    {
        data = new CLabelCommandData(_command);
        data.specialNode = _specialNode;
    }    
    public CLabelCommand(TLabelCommands _command, GameObject gameObject)
    {
        data = new CLabelCommandData(_command);

        data.obj = gameObject;
    }
    public CLabelCommand(TLabelCommands _command, string _partID, string _text, Vector3 _position)
    {
        data = new CLabelCommandData(_command);

        data.labelID = null;
        data.partID = _partID;
        data.text = _text;
        data.position = _position;
    }
    public CLabelCommand(TLabelCommands _command, string _labelID, string _partID, string _text, Vector3 _position)
    {
        data = new CLabelCommandData(_command);

        data.labelID = _labelID;
        data.partID = _partID;
        data.text = _text;
        data.position = _position;
    }

    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour _m)
    {
        if (data != null)
        {
            switch (data.command)
            {
                //case TLabelCommands.ShowLabel:                    
                //    //Not allowed two labels with same ID
                //    if (_m.GetComponent<LabelManager>().LabelContains(data.labelID))
                //    {                        
                //        _m.GetComponent<LabelManager>().RemoveLabel(data.labelID);    //Remove the label to show a new one
                //    }

                //    //Check visual state of the area
                //    List<GameObject> objectList = new List<GameObject>();
                //    if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(data.partID))
                //    {
                //        if (hom3r.quickLinks.scriptsObject.GetComponent<ObjectStateCommonManager>().IsAreaVisible(data.partID))
                //        {
                //            objectList.Add(hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(data.partID));
                //        }                        
                //    }
                //    else if(hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(data.partID))
                //    {
                //        if (hom3r.quickLinks.scriptsObject.GetComponent<ObjectStateCommonManager>().IsLeafVisible(data.partID))
                //        {
                //            objectList.AddRange(hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByLeafID(data.partID));
                //        }
                //    }
                //    else if(hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsNode(data.partID))
                //    {
                //        if (hom3r.quickLinks.scriptsObject.GetComponent<ObjectStateCommonManager>().IsNodeVisible(data.partID))
                //        {
                //            objectList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(data.partID);
                //        }                        
                //    }    
                                    
                //    if (objectList.Count != 0)
                //    {
                //        bool largeText = data.text.Length > Constants.largeText;
                //        if (data.position == Vector3.zero)
                //        {
                //            hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().AddLabelToGameObjectGroup(data.labelID, objectList, data.text, largeText);
                //        }
                //        else
                //        {
                //            hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().AddLabelToSpecificPoint(data.labelID, objectList[0], data.position, data.text, largeText);
                //        }
                //    }
                //    else
                //    {
                //        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.LabelManager_ShowMessage, "Labels can only be attached to visible parts. The current selection is not visible"));                        
                //    }                    
                //    break;
                case TLabelCommands.AddLabel:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().AddLabel(data.partID, data.labelID, data.position, data.text);
                    break;

                case TLabelCommands.AddAutomaticLabelToArea:
                    _m.GetComponent<LabelCommandReceiver>().ExecuteAddLabel_ToAGivenArea(data.obj);
                    break;
                case TLabelCommands.AddAutomaticLabelToSpecialNode:
                    _m.GetComponent<LabelCommandReceiver>().ExecuteAddLabel_ToAGivenSpecialNode(data.specialNode);
                    break;
                case TLabelCommands.RemoveLabel:                    
                    //Hide a Label by ID
                    _m.GetComponent<LabelManager>().RemoveLabel(data.labelID);                    
                    break;

                case TLabelCommands.RemoveAllLabel:                    
                    //Hide all labels
                    _m.GetComponent<LabelManager>().RemoveAllLabels();                    
                    break;

                case TLabelCommands.LabelModeManager:                    
                    if (data.status)
                    {
                        if (hom3r.state.currentLabelMode == THom3rLabelMode.IDLE)
                        {
                            hom3r.state.currentLabelMode = THom3rLabelMode.SHOWLABEL;
                            //Execute show area name labels
                            _m.GetComponent<LabelCommandReceiver>().ExecuteAddLabel_ToAllConfirmedGameObject();
                        }
                    }
                    else
                    {
                        if (hom3r.state.currentLabelMode == THom3rLabelMode.SHOWLABEL)
                        {
                            hom3r.state.currentLabelMode = THom3rLabelMode.IDLE;
                            //this.GetComponent<Labels_Script>().RemoveAllLabels();
                            _m.GetComponent<LabelCommandReceiver>().ExecuteRemoveAllLabelOfConfirmedGOList();
                        }
                    }                    
                    break;

                //case to manage the mode SHOWLABEL (when this mode is activated, every selected part or component will show a label with the part name)
                case TLabelCommands.ShowPartNameLabel:                    
                    if (hom3r.state.currentLabelMode == THom3rLabelMode.IDLE)
                    {
                        hom3r.state.currentLabelMode = THom3rLabelMode.SHOWLABEL;
                        //Execute show area name labels
                        _m.GetComponent<LabelCommandReceiver>().ExecuteAddLabel_ToAllConfirmedGameObject();
                    }
                    else
                    {
                        hom3r.state.currentLabelMode = THom3rLabelMode.IDLE;                        
                        _m.GetComponent<LabelCommandReceiver>().ExecuteRemoveAllLabelOfConfirmedGOList();
                    }                    
                    break;

                case TLabelCommands.RemoveAllLabelOfConfirmedObjects:                    
                    _m.GetComponent<LabelCommandReceiver>().ExecuteRemoveAllLabelOfConfirmedGOList();                    
                    break;

                case TLabelCommands.LabelUISelection:                    
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().HandleUISelection(data.obj);                    
                    break;

                default:
                    Debug.LogError("Error: This command " + data.command + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Label command without a valid command");
        }
    }

    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }
}
