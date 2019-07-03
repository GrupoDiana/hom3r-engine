using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabelEditorManager : MonoBehaviour
{
    GameObject labelGO;
    bool saved;
    bool editing;
    CLabelTransform initialPosition;
    Slider labelRotationSlider;  
        
    void Awake()
    {
        labelGO = null;
        editing = false;
        saved = false;
        labelRotationSlider = this.gameObject.transform.Find("Panel_LabelEditor").gameObject.transform.Find("Slider").GetComponent<Slider>();
    }

    
    void Update()
    {
        
    }


    public void Init(GameObject _label)
    {
        labelGO = _label;
        Debug.Log(labelGO.transform.rotation.eulerAngles.y);
        labelRotationSlider.value = labelGO.transform.rotation.eulerAngles.y;
    }



    public void OnClickButtonSave()
    {
        Debug.Log("Save button pressed");
    }

    public void OnClickButtonClose()
    {
        Debug.Log("Close button pressed");
    }

    public void OnSliderChange()
    {
        Debug.Log(labelRotationSlider.value);
        Vector3 labelRotation = labelGO.transform.rotation.eulerAngles;
        labelRotation.y = labelRotationSlider.value;


        labelGO.transform.localEulerAngles = labelRotation;
    }


    /////////////////
    // EVENTS
    /////////////////
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
                case TCoreEvent.MouseManager_MouseDragGestureBegin:

                    if(_event.data.obj.transform.parent.gameObject == labelGO) {
                        editing = true;
                        Debug.Log("editing - true");
                        if (saved)
                        {
                            initialPosition = labelGO.GetComponent<Label2>().GetLabelPosition();
                            saved = false;
                        }
                        
                    }
                    break;
                case TCoreEvent.MouseManager_MouseDragGesture:
                    if (!editing) { return; }
                    //if (_event.data.obj.transform.parent.gameObject == labelGO) {                       
                        labelGO.GetComponent<Label2>().SetLabelPosition(_event.data.mouseDragMovementX, _event.data.mouseDragMovementY);
                    //}
                    break;
                case TCoreEvent.MouseManager_MouseDragGestureEnd:
                    if (!editing) { return; }
                    if (_event.data.obj.transform.parent.gameObject == labelGO)
                    {
                        editing = false;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
