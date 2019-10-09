using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelEventReceiver : MonoBehaviour {

    private void OnEnable()
    {
        hom3r.coreLink.SubscribeEventObserver(DoInternalEventCommand);      //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeEventObserver(DoInternalEventCommand);    //Unsubscribe a method to the event delegate
    }

    private void DoInternalEventCommand(CCoreEvent _event)
    {
        if (_event.GetType() == typeof(CCoreEvent)) { ExecuteCoreEvents(_event); }
        else { /* Do nothing */ }
    }

    /// <summary>Execute internal events commands</summary>
    /// <param name="_event">command to be executed</param>
    private void ExecuteCoreEvents(CCoreEvent _event)
    {
        if (_event.data != null)
        {
            switch (_event.data.commandEvent)
            {
                case TCoreEvent.Occlusion_ExplosionBegin:
                    //hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().HideAllLabels();
                    break;
                case TCoreEvent.Occlusion_ExplosionEnd:
                    //hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().RedrawAllLabels();
                    break;
                case TCoreEvent.Occlusion_Isolate_Enabled:
                    //hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().HideAllLabels();
                    break;
                case TCoreEvent.Navigation_NavigationToFocusEnd:
                    //Labels scale update
                    // Draw only labels of isolated objects, using local center                     
                    /*IEnumerable<GameObject> isolatedGOs = hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().GetListOfConfirmedObjects();
                    List<GameObject> isolatedGOList = new List<GameObject>(isolatedGOs);
                    foreach (GameObject go in isolatedGOList)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().RedrawLabelsLocallyForGO(go);
                    }*/
                    break;
                case TCoreEvent.Navigation_ApproximationEnd:
                    //hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().RedrawAllLabels();      //Labels scale update
                    break;
                case TCoreEvent.Occlusion_Isolate_Disabled:
                    //hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().RedrawAllLabels();
                    break;
                case TCoreEvent.ObjectState_AreaRemoved:
                    //Remove label
                    /*if (this.GetComponent<LabelManager>().LabelContainsTarget(_event.data.obj))
                    {
                        this.GetComponent<LabelManager>().DestroyLabelFromTargetGO(_event.data.obj);
                    }*/
                    break;
                
                case TCoreEvent.Selection_AreaConfirmationOn:
                    // this.AddLabelToConfirmedArea(_event.data.obj);
                    break;
                case TCoreEvent.Selection_AllPartsDeselected:
                    // hom3r.coreLink.Do(new CLabelCommand(TLabelCommands.RemoveAllLabelOfConfirmedObjects));
                    break;
                case TCoreEvent.ObjectState_AreaConfirmationOn:
                    // this.AddLabelToConfirmedArea(_event.data.obj);
                    break;
                case TCoreEvent.ObjectState_AreaConfirmationOff:
                    /*if (hom3r.state.currentLabelMode == THom3rLabelMode.show)
                    {
                        //Every time an area is "deselected" we have to check if it has a label and remove it in case.                        
                        //foreach (string area in _event.data.obj.GetComponent<ObjectStateManager>().areaID)
                        //{
                        string area = _event.data.obj.GetComponent<ObjectStateManager>().areaID;
                        if (this.GetComponent<LabelManager>().LabelContains(area))
                        {
                            this.GetComponent<LabelManager>().RemoveLabel(area);
                        }
                        //}
                        string specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(_event.data.obj.GetComponent<ObjectStateManager>().areaID);
                        if (this.GetComponent<LabelManager>().LabelContains(specialNodeID))
                        {
                            this.GetComponent<LabelManager>().RemoveLabel(specialNodeID);
                        }
                    }*/
                    break;
                case TCoreEvent.ModelManagement_ResetModel:
                    //hom3r.coreLink.Do(new CLabelCommand(TLabelCommands.RemoveAllLabelOfConfirmedObjects), Constants.undoNotAllowed);
                    //hom3r.quickLinks.scriptsObject.GetComponent<LabelCommandReceiver>().ExecuteRemoveAllLabelOfConfirmedGOList();
                    /*hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().RemoveAllLabels();     */               
                    break;


                default:
                    break;
            }
        }
    }


    private void AddLabelToConfirmedArea(GameObject obj)
    {
        if (hom3r.state.currentLabelMode == THom3rLabelMode.show)
        {
            if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
            {                
                hom3r.coreLink.Do(new CLabelCommand(TLabelCommands.AddAutomaticLabelToArea, obj));
            }
            else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
            {
                //Select all the areas with the same special ancestor. Suppose all the IDs of the area have the same ancestor.
                string specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(obj.GetComponent<ObjectStateManager>().areaID);
                if (!this.GetComponent<LabelManager>().LabelContains(specialNodeID))
                {
                    //ExecuteAddLabel_ToAGivenSpecialNode(specialNodeID);
                    hom3r.coreLink.Do(new CLabelCommand(TLabelCommands.AddAutomaticLabelToSpecialNode, specialNodeID));
                }
            }
        }
    }    
}
