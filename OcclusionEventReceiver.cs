using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionEventReceiver : MonoBehaviour {

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
                case TCoreEvent.Occlusion_ExplosionEnd:
                    if (hom3r.state.smartTransparencyModeActive)
                    {
                        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed);  //Re-launch smart transparency
                    }
                    break;
                case TCoreEvent.Selection_ConfirmationOnFinished:
                    if (hom3r.state.smartTransparencyModeActive) {
                        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed);   //Re-launch smart transparency
                    }
                    break;
                case TCoreEvent.Selection_ConfirmationOffFinished:
                    if (hom3r.state.smartTransparencyModeActive) {
                        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed);  //Re-launch smart transparency
                    }
                    break;
                case TCoreEvent.ObjectState_AreaConfirmationOn:

                    if (hom3r.state.smartTransparencyModeActive)
                    {
                        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed);  //Re-launch smart transparency
                    }
                    //Visualize the area if we are in ISOLATE mode
                    if (hom3r.state.currentIsolateMode == THom3rIsolationMode.ISOLATE || hom3r.state.currentIsolateMode == THom3rIsolationMode.WITH_REMOVEDNODES)
                    {
                        //string areaID = _event.data.obj.GetComponent<ObjectState_Script>().areaID[0];
                        //string parentID = hom3r.quickLinks.scriptsObject.GetComponent<ModelManagement_Script>().GetNodeLeafID_ByAreaID(areaID);
                        //List<GameObject> objectAreaList = hom3r.coreLink.GetComponent<ModelManagement_Script>().GetAreaGameObjectList_ByNodeLeafID(parentID);
                        List<GameObject> objectAreaList = hom3r.coreLink.GetComponent<ModelManager>().GetAreaGameObjectList_ByArea(_event.data.obj);

                        foreach (GameObject go in objectAreaList)
                        {
                            if (hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().IsRemovedGameObject(go))
                            {
                                // go.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Remove_Off);
                                go.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Remove_Off));
                            }
                        }
                    }
                    break;
                case TCoreEvent.ObjectState_AreaConfirmationOff:                    
                    if (hom3r.state.smartTransparencyModeActive)
                    {
                        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.ResetSmartTransparency), Constants.undoNotAllowed);  //Re-launch smart transparency
                    }
                    break;

                case TCoreEvent.ObjectState_AreaTransparencyOn:
                    hom3r.quickLinks.scriptsObject.GetComponent<TransparencyManager>().TransparencyOn(_event.data.obj);     //Add to the list of transparent objects
                    break;

                case TCoreEvent.ObjectState_AreaTransparencyOff:
                    hom3r.quickLinks.scriptsObject.GetComponent<TransparencyManager>().TransparencyOff(_event.data.obj);    //Delete from the list of transparent GameObjects        
                    break;
                case TCoreEvent.ObjectState_AreaHiddenOn:
                    hom3r.quickLinks.scriptsObject.GetComponent<HiddenManager>().GameObjectHiddenOn(_event.data.obj);
                    break;
                case TCoreEvent.ObjectState_AreaHiddenOff:
                    hom3r.quickLinks.scriptsObject.GetComponent<HiddenManager>().GameObjectHiddenOff(_event.data.obj);
                    break;
                case TCoreEvent.ObjectState_AreaRemoveOn:
                    hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().AddToRemovedList(_event.data.obj);      //Add to the list of hidden objects             
                    break;


                case TCoreEvent.MouseManager_LeftButtonUp:
                    if (hom3r.state.currentMode == THom3rMode.remove)
                    {
                        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.RemoveGameObject, _event.data.obj);
                        this.GetComponent<Core>().Do(command, Constants.undoAllowed);
                    } else if (hom3r.state.currentMode == THom3rMode.local_explosion) {
                        CCoreCommand command = new COcclusionCommand(TOcclusionCommands.OneObjectLayoutExplosion, _event.data.obj);
                        this.GetComponent<Core>().Do(command, Constants.undoAllowed);
                    }
                    break;
                case TCoreEvent.ObjectState_AreaRemoveOff:
                    hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().RemoveFromRemovedList(_event.data.obj);
                    break;
                case TCoreEvent.ModelManagement_ReadyToLoadExplosionModel:                    
                    hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().LoadProducExplosionModel(_event.data.text);
                    break;
                case TCoreEvent.ModelManagement_ModelReset_Success:                    
                    hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().ResetOcclusionProcess();                    
                    break;

                case TCoreEvent.UI_SmartTransparency_AlphaLevelUpdated:                    
                    hom3r.quickLinks.scriptsObject.GetComponent<TransparencyManager>().SetSmartTransparencyAlphaLevel(_event.data.value1, THom3rCommandOrigin.ui);                    
                    break;
                case TCoreEvent.Core_ModeChanged:
                    hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().UpdateOcclusionMode();
                    break;
                default:
                    break;
            }
        }
    }
}
