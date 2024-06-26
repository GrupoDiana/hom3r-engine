﻿using System.Collections;
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
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY, _event.data.mouseDragMovementXPercentage, _event.data.mouseDragMovementYPercentage, 0.0f);
                    break;
                case TCoreEvent.MouseManager_CentralButtonDown:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(true);
                    break;
                case TCoreEvent.TouchManager_TwoFingersDragMovement_Begin:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(true);
                    break;
                case TCoreEvent.TouchManager_ThreeFingersDragMovement_Begin:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(true);
                    break;
                case TCoreEvent.MouseManager_CentralButtonUp:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(false);
                    break;
                case TCoreEvent.TouchManager_TwoFingerDragMovement_End:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(false);
                    break;
                case TCoreEvent.TouchManager_ThreeFingerDragMovement_End:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetActivePanNavitagion(false);
                    break;
                case TCoreEvent.MouseManager_WheelMovement:
                    if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveMouseWheelInteration())
                    {
                        hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(0.0f, 0.0f, 0.0f, 0.0f, _event.data.mouseWhellMovement);
                    }                    
                    break;

                case TCoreEvent.MouseManager_WheelMovementSecundaryCamera:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovementSecundaryCamera(0.0f, 0.0f, 0.0f, 0.0f, _event.data.mouseWhellMovement);
                    break;
                case TCoreEvent.MouseManager_CentralButtonDragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY, _event.data.mouseDragMovementXPercentage, _event.data.mouseDragMovementYPercentage, 0.0f);
                    break;
                case TCoreEvent.TouchManager_TwoFingerDragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY, _event.data.mouseDragMovementXPercentage, _event.data.mouseDragMovementYPercentage, 0.0f);
                    break;
                case TCoreEvent.TouchManager_ThreeFingerDragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY, _event.data.mouseDragMovementXPercentage, _event.data.mouseDragMovementYPercentage, 0.0f);
                    break;
                case TCoreEvent.ModelManagement_3DLoadSuccess:                    
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation(_event.data.text);
                    break;                
                case TCoreEvent.ModelManagement_ModelReset_Success:                    
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().StopNavigation();
                    break;
                case TCoreEvent.ModelManagement_NavigationAxisChange_Success:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation();
                    break;
                case TCoreEvent.TouchManager_OneFingerDragMovement:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetTouchMovement(_event.data.value1, _event.data.value2);
                    break;
                case TCoreEvent.TouchManager_TwoFingersPinch:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetTouchPithZoom(_event.data.value1);
                    break;
                case TCoreEvent.Occlusion_Isolate_Enabled:                    
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation();
                    break;
                case TCoreEvent.Occlusion_Isolate_Disabled:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation();
                    break;
                case TCoreEvent.ModelManagement_FileDownloadBegin:
                    hom3r.state.navigationBlocked = true;
                    break;
                case TCoreEvent.ModelManagement_FileDownloadEnd:
                    hom3r.state.navigationBlocked = false;
                    break;
                case TCoreEvent.ConfigurationManager_ConfigurationUpdated:
                    hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().CheckConfigurationUpdated();
                    break;
                default:
                    break;
            }
        }
    }
}
