using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Core events</summary>
public enum TCoreEvent
{
    Core_Hom3rReadyToStart, Core_ModeChanged,
    
    ConfigurationManager_ConfigurationUpdated,

    ModelManagement_ProductModelLoadSuccess, ModelManagement_ModelLoadError,
    ModelManagement_3DLoadSuccess, ModelManagement_3DLoadError, ModelManagement_3DListLoadSuccess,
    ModelManagement_ShowMessage,
    ModelManagement_ReadyToLoadExplosionModel, 
    ModelManagement_FileDownloadBegin, ModelManagement_FileDownloadEnd,
    ModelManagement_ModelReset_Success,
    ModelManagement_ProductModelEditOk, ModelManagement_ProductModelEditError,
    ModelManagement_NavigationAxisChanged,

    _3DFileManager_ShowMessage, _3DFileManager_ShowMessageConsole,

    MouseManager_MousePosition,
    MouseManager_LeftButtonUp, MouseManager_LeftButtonDown,
    MouseManager_RightButtonUp, MouseManager_RightButtonDown,
    MouseManager_CentralButtonUp, MouseManager_CentralButtonDown,
    MouseManager_LeftButtonDragMovement,
    MouseManager_WheelMovement,
    MouseManager_CentralButtonDragMovement,

    MouseManager_LabelDragGestureEnd, MouseManager_LabelDragGestureBegin,
    MouseManager_LabelDragGesture,

    TouchManager_OneSelectionTouch,
    TouchManager_DragMovementBegin,TouchManager_DragMovement, TouchManager_DragMovementEnd,
    TouchManager_PinchZoomBegin, TouchManager_PinchZoom, TouchManager_PinchZoomEnd,
    TouchManager_TwoFingerMovementBegin, TouchManager_TwoFingerMovementEnd, TouchManager_TwoFingerDragMovement,

    Selection_ShowMessage,
    Selection_IndicationOnFinished, Selection_IndicationOffFinished,
    Selection_ConfirmationOnFinished, Selection_ConfirmationOffFinished,
    Selection_AreaConfirmationOn,
    Selection_AllPartsDeselected,

    RemovedPart_Activate, RemovedPart_Deactivated,

    ObjectState_AreaIndicateOn, ObjectState_AreaIndicateOff,
    ObjectState_AreaConfirmationOn, ObjectState_AreaConfirmationOff,
    ObjectState_AreaTransparencyOn, ObjectState_AreaTransparencyOff,
    ObjectState_AreaHiddenOn, ObjectState_AreaHiddenOff,
    ObjectState_AreaRemoveOn, ObjectState_AreaRemoveOff,

    UI_SmartTransparency_AlphaLevelUpdated, UI_SmartTransparency_Enabled, UI_SmartTransparency_Disabled,

    Navigation_NavigationInitiaded,
    Navigation_PanNavigation_Enabled, Navigation_PanNavigation_Disabled,
    Navigation_NavigationToFocusEnd, Navigation_ApproximationEnd,
    Navigation_PseudoLatitudeMovement, Navigation_PseudoLongitudeMovement, Navigation_PseudoRadioMovement,
    Navigation_CameraMoved,

    //Occlusion_ExplosionModel_Empty,
    Occlusion_ExplosionBegin, Occlusion_ExplosionEnd, Occlusion_ExplodingAreas, Occlusion_ImplodingAreas,
    Occlusion_ExplosionGlobalON, Occlusion_ExplosionGlobalOFF,
    Occlusion_ExplosionChangedMode,
    Occlusion_Removed_Area, Occlusion_Shown_Area,
    Occlusion_Isolate_Enabled, Occlusion_Isolate_Disabled,
    Occlusion_SmartTransparency_Enabled, Occlusion_SmartTransparency_Disabled, Occlusion_SmartTransparency_AlphaLevelUpdated,

    ObjectState_AreaRemoved,

    LabelManager_ShowMessage, LabelManager_LabelRemoved,
    LabelManager_LabelDataUpdated,

    PointOnSurface_PointCaptureBegin, PointOnSurface_PointCaptureEnd,
    PointOnSurface_PointCaptureSuccess, PointOnSurface_PointCaptureError,

    UIAR_StartAuthoring, UIAR_StopAuthoring,
    ExhibitionManager_ExhibitionPlotLoadError, ExhibitionManager_ExhibitionPlotLoadSuccess,
    ExhibitionManager_ExhibitionPointLoaded, ExhibitionManager_ExhibitionLanguageChanged,

    AR_TapToPlaceScene_Success
};

/// <summary>Core events data</summary>
public class CCoreEventData
{
    public TCoreEvent commandEvent;

    public List<GameObject> gameObjectList { get; set; }
    public GameObject obj { get; set; }    
    public string text { get; set; }
    public string text2 { get; set; }
    public string areaId { get; set; }
    public float value1 { get; set; }
    public float value2 { get; set; }
    public bool control { get; set; }
    public string labelId { get; set; }
    public TLabelType labelType { get; set; }
    public Vector3 mousePosition { get; set; }
    public Vector3 anchorPosition { get; set; }
    public Vector3 boardPosition { get; set; }
    public Quaternion boardRotation { get; set; }
    public float scaleFactor { get; set; }
    public float mouseDragMovementX { get; set; }
    public float mouseDragMovementY { get; set; }
    public float mouseWhellMovement { get; set; }
    public List<string> textList { get; set; }
    public THom3rCommandOrigin origin { get; set; }

    public CCoreEventData(TCoreEvent _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>The Core Event Class</summary>
public class CCoreEvent
{
    public CCoreEventData data;
    //////////////////
    // Constructors //
    //////////////////
    public CCoreEvent(TCoreEvent _command)
    {
        data = new CCoreEventData(_command);
    }
    public CCoreEvent(TCoreEvent _command, THom3rCommandOrigin _origin)
    {
        data = new CCoreEventData(_command);
        data.origin = _origin;
    }
    public CCoreEvent(TCoreEvent _command, List<GameObject> _goList)
    {
        data = new CCoreEventData(_command);
        data.gameObjectList = _goList;
    }
    public CCoreEvent(TCoreEvent _command, GameObject _gameObject)
    {
        data = new CCoreEventData(_command);
        data.obj = _gameObject;
    }

    public CCoreEvent(TCoreEvent _command, string _text)
    {
        data = new CCoreEventData(_command);
        data.text = _text;
    }
      
    public CCoreEvent(TCoreEvent _command, string _text, float _value1)
    {
        data = new CCoreEventData(_command);
        data.text = _text;
        data.value1 = _value1;
    }
    public CCoreEvent(TCoreEvent _command, string _text, string _text2, float _value1)
    {
        data = new CCoreEventData(_command);
        data.text = _text;
        data.text2 = _text2;
        data.value1 = _value1;
    }
    public CCoreEvent(TCoreEvent _command, float _value1)
    {
        data = new CCoreEventData(_command);
        data.value1 = _value1;
        data.mouseWhellMovement = _value1;
    }
    public CCoreEvent(TCoreEvent _command, float _value1, float _value2)
    {
        data = new CCoreEventData(_command);
        data.value1 = _value1;
        data.value2 = _value2;
    }

    public CCoreEvent(TCoreEvent _command, string _text, List<string> _texlist)
    {
        data = new CCoreEventData(_command);
        data.text = _text;        
        data.textList = _texlist;
    }

    public CCoreEvent(TCoreEvent _command, Vector3 _mousePosition)
    {
        data = new CCoreEventData(_command);
        data.mousePosition = _mousePosition;
    }

    public CCoreEvent(TCoreEvent _command, Vector3 _anchorPosition, string _areaID)
    {
        data = new CCoreEventData(_command);
        data.anchorPosition = _anchorPosition;
        data.areaId = _areaID;
    }

    public CCoreEvent(TCoreEvent _command, Vector3 _mousePosition, GameObject _gameObject)
    {
        data = new CCoreEventData(_command);
        data.mousePosition  = _mousePosition;
        data.obj            = _gameObject;        
    }

    public CCoreEvent(TCoreEvent _command, Vector3 _mousePosition, GameObject _gameObject, bool _control)
    {
        data = new CCoreEventData(_command);
        data.mousePosition = _mousePosition;
        data.obj = _gameObject;
        data.control = _control;        
    }

    public CCoreEvent(TCoreEvent _command, Vector3 _mousePosition, float _mouseMovementX, float _mouseMovementY)
    {
        data = new CCoreEventData(_command);
        data.mousePosition = _mousePosition;        
        data.mouseDragMovementX = _mouseMovementX;
        data.mouseDragMovementY = _mouseMovementY;    
    }
    public CCoreEvent(TCoreEvent _command, List<string> _texlist)
    {
        data = new CCoreEventData(_command);
        data.textList = _texlist;
    }

    public CCoreEvent(TCoreEvent _command, string _labelId, TLabelType _labelType, Vector3 _boardPosition, Quaternion _boardRotation, float _scaleFactor )
    {
        data = new CCoreEventData(_command);
        data.labelId = _labelId;
        data.labelType = _labelType;
        data.boardPosition = _boardPosition;       
        data.boardRotation = _boardRotation;
        data.scaleFactor = _scaleFactor;
    }
    public CCoreEvent(TCoreEvent _command, string _labelId, string _areaId, TLabelType _labelType, Vector3 _boardPosition, Vector3 _anchorPosition, float _scaleFactor)
    {
        data = new CCoreEventData(_command);
        data.labelId = _labelId;
        data.areaId = _areaId;
        data.labelType = _labelType;
        data.boardPosition = _boardPosition;
        data.anchorPosition = _anchorPosition;
        data.scaleFactor = _scaleFactor;
    }
}


