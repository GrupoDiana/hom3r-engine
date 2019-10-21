using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelManager2EventReceiver : MonoBehaviour
{
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
                case TCoreEvent.PointOnSurface_PointCaptureSuccess:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().AfterAnchorPointCapture(_event.data.anchorPosition, _event.data.areaId);
                    break;
                case TCoreEvent.PointOnSurface_PointCaptureError:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().AfterAnchorPointCaptureError();
                    break;
                case TCoreEvent.Navigation_CameraMoved:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().UpdateAnchoredLabelsOrientation();
                    break;
                case TCoreEvent.MouseManager_LabelDragGestureBegin:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().StartEditLabel(_event.data.obj);
                    break;
                case TCoreEvent.MouseManager_LabelDragGestureEnd:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().StopDragLabelLabel();
                    break;
                case TCoreEvent.MouseManager_LabelDragGesture:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().DragLabel(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY);                   
                    break;
                case TCoreEvent.Navigation_NavigationInitiaded:                    
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().UpdateAnchoredLabelsOrientationAndPole();
                    break;
                case TCoreEvent.Occlusion_Removed_Area:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().CheckIfAreThereAnyLabelToHide(_event.data.text);
                    break;
                case TCoreEvent.Occlusion_Shown_Area:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().CheckIfAreThereAnyHiddenLabelToShow(_event.data.text);                    
                    break;
                case TCoreEvent.ModelManagement_ModelReset_Success:
                    hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().RemoveAllLabel();
                    break;
                default:
                    break;
            }
        }
    }
}
