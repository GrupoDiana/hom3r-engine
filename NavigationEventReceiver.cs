using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationEventReceiver : MonoBehaviour {

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
                case TCoreEvent.MouseManager_LeftButtonDragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY, 0.0f);
                    break;
                case TCoreEvent.MouseManager_CentralButtonDown:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(true);
                    break;
                case TCoreEvent.MouseManager_CentralButtonUp:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(false);
                    break;
                case TCoreEvent.MouseManager_WheelMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(0.0f, 0.0f, _event.data.mouseWhellMovement);
                    break;
                case TCoreEvent.MouseManager_CentralButtonDragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY, 0.0f);
                    break;
                case TCoreEvent.ModelManagement_3DLoadSuccess:                    
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation(_event.data.text);
                    break;                
                case TCoreEvent.ModelManagement_ResetModel:                                        
                    break;
                case TCoreEvent.TouchManager_DragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetTouchMovement(_event.data.value1, _event.data.value2);
                    break;
                case TCoreEvent.TouchManager_PinchZoom:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetTouchPithZoom(_event.data.value1);
                    break;
                case TCoreEvent.Occlusion_Isolate_Enabled:                    
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation();
                    break;
                case TCoreEvent.Occlusion_Isolate_Disabled:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation();
                    break;
                default:
                    break;
            }
        }
    }
}
