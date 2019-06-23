using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionEventReceiver : MonoBehaviour {

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
                case TCoreEvent.MouseManager_MousePosition:
                    if (hom3r.state.currentMode == THom3rMode.IDLE || hom3r.state.currentMode == THom3rMode.SMARTTRANSPARENCY)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().IndicationByMousePosition(_event.data.mousePosition);
                    }                    
                    break;
                case TCoreEvent.MouseManager_LeftButtonUp:
                    if (hom3r.state.currentMode == THom3rMode.IDLE || hom3r.state.currentMode == THom3rMode.SMARTTRANSPARENCY)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmByMouseLeftClickAndMousePosition(_event.data.mousePosition, _event.data.obj, _event.data.control);
                    }                        
                    break;
                case TCoreEvent.TouchManager_OneSelectionTouch:
                    if (hom3r.state.currentMode == THom3rMode.IDLE || hom3r.state.currentMode == THom3rMode.SMARTTRANSPARENCY)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmByMouseLeftClickAndMousePosition(_event.data.mousePosition, _event.data.obj, _event.data.control);
                    }
                    break;                                     
                case TCoreEvent.Occlusion_ExplodingAreas:
                    foreach (GameObject go in _event.data.gameObjectList)
                    {
                        go.GetComponent<ObjectStateManager>().SendEvent(ObjectExplosionStateEvents_Type.Explode);
                    }
                    break;
                case TCoreEvent.Occlusion_ImplodingAreas:
                    foreach (GameObject go in _event.data.gameObjectList)
                    {
                        go.GetComponent<ObjectStateManager>().SendEvent(ObjectExplosionStateEvents_Type.Implode);
                    }
                    break;
                case TCoreEvent.ModelManagement_FileDownloadBegin:
                    hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.BlockSelection, true), Constants.undoNotAllowed); //Block Selection
                    break;
                case TCoreEvent.ModelManagement_FileDownloadEnd:
                    hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.BlockSelection, false), Constants.undoNotAllowed); //UnBlock selection
                    break;
                case TCoreEvent.ObjectState_AreaIndicateOn:
                    hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().AddToIndicatedList(_event.data.obj);    //Introduce into the indicated object list
                    break;
                case TCoreEvent.ObjectState_AreaIndicateOff:
                    hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().RemoveFromIndicatedList(_event.data.obj);
                    break;
                default:
                    break;
            }
        }
    }
}
