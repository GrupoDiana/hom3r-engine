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
                default:
                    break;
            }
        }
    }
}
