using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TTouchManagerMode       { iddle, one_finger, two_fingers, three_fingers };
public enum TOneTouchIteractionMode { iddle, selection, drag_movement };
public enum TTwoTouchIteractionMode { began, moved };
public enum TThreeTouchIteractionMode { began, moved };

public class TouchManager : MonoBehaviour
{
    // General
    private TTouchManagerMode touchManagerMode;
    private TTouchManagerMode lastTouchManagerMode;
    private List<int> initialTouchsId;


    // private int     selectionTouchId;
    // private bool    isDragMovement;    
    private float   currentCountdownValue;
    private int     movementCounter;
    private int     movementDetector;    
    private float   touchMovementDetectorMargin;

    //One touch gesture
    private TOneTouchIteractionMode oneTouchIteractionMode;

    // Two Touches gestures vars
    private TTwoTouchIteractionMode twoTouchInteractionMode;
    private float twoTouchInitialDistance;
    
    List<Vector2> previousTouchPositions;

    private TThreeTouchIteractionMode threeTouchInteractionMode;

    
    private float pinchMovementDetectorMargin;
    private float panMovementDetectorMargin;


    //private LowpassFilterButterworthImplementation lowPassFilter;

    // Ray-casting variables
    private int     productRootLayer;
    private int     gizmoLayer;
    private Camera  gizmoCamera;

    // Others    
    void Awake()
    {
        touchManagerMode = TTouchManagerMode.iddle;
        lastTouchManagerMode = TTouchManagerMode.iddle;
        this.InitGestureModes();        
                
        this.movementCounter = 0;
        this.movementDetector = 5;              // TODO do this in a different way ?
        this.pinchMovementDetectorMargin = 0.25f;
        this.panMovementDetectorMargin = 0.35f;
        this.touchMovementDetectorMargin = 7f;

        
       
        //this.lowPassFilter = new LowpassFilterButterworthImplementation(40f, 4, 120f);
        ////////////////////////////////////////
        // Initialize Ray-casting variables
        ////////////////////////////////////////
        GameObject gizmoGO = GameObject.FindWithTag("GizmoCamera");
        if (gizmoGO !=null) { gizmoCamera = gizmoGO.GetComponent<Camera>(); }

        //Initialize the layer masks
        // NameToLayer() returns the layer index 
        //'1 << ...' converts that to a bit mask, turning on the bit associated with that layer
        productRootLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        gizmoLayer = 1 << LayerMask.NameToLayer("gizmo_layer");

    }

    // Update is called once per frame
    void Update()
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveTouchInteration())
        {
            this.lastTouchManagerMode = touchManagerMode;
            this.touchManagerMode = GetTouchManagerMode(Input.touchCount);

            if (this.touchManagerMode != this.lastTouchManagerMode) { this.InitLastTouchMode(); }

            if (this.touchManagerMode == TTouchManagerMode.one_finger) { OneTouchManager(); }
            else if (this.touchManagerMode == TTouchManagerMode.two_fingers) { TwoTouchManager(); }
            else if (this.touchManagerMode == TTouchManagerMode.three_fingers) { ThreeTouchManager(); }
            else { this.InitLastTouchMode(); }
        }
    }

    ////////////////////////
    //  MODE MANAGER
    ////////////////////////
    private TTouchManagerMode GetTouchManagerMode(int touchCount)
    {
        TTouchManagerMode currentTouchManagerMode = TTouchManagerMode.iddle;
        switch (touchCount)
        {
            case 1:
                currentTouchManagerMode = TTouchManagerMode.one_finger;
                break;
            case 2:
                currentTouchManagerMode = TTouchManagerMode.two_fingers;
                break;
            case 3:
                currentTouchManagerMode = TTouchManagerMode.three_fingers;
                break;
            default:
                currentTouchManagerMode = TTouchManagerMode.iddle;
                break;
        }
        return currentTouchManagerMode;
    }

    private void InitLastTouchMode()
    {
        if (this.lastTouchManagerMode == TTouchManagerMode.iddle) { return; }
        switch (this.lastTouchManagerMode)
        {
            case TTouchManagerMode.one_finger:
                this.InitOneTouchMode();
                break;
            case TTouchManagerMode.two_fingers:
                this.InitTwoTouchsMode();
                break;
            case TTouchManagerMode.three_fingers:
                this.InitThreeTouchsMode();
                break;
        }
        this.lastTouchManagerMode = TTouchManagerMode.iddle;
    }

    private void InitGestureModes()
    {
        this.oneTouchIteractionMode = TOneTouchIteractionMode.iddle;
        this.twoTouchInteractionMode = TTwoTouchIteractionMode.began;
        this.threeTouchInteractionMode = TThreeTouchIteractionMode.began;
        this.initialTouchsId = new List<int>();        
    }

    private void InitOneTouchMode()
    {
        if (this.oneTouchIteractionMode != TOneTouchIteractionMode.iddle)
        {
            this.initialTouchsId = new List<int>();
            this.oneTouchIteractionMode = TOneTouchIteractionMode.iddle;
            this.movementCounter = 0;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneFingerDragMovement_End));
        }        
    }

    private void InitTwoTouchsMode()
    {
        if (this.twoTouchInteractionMode != TTwoTouchIteractionMode.began)
        {
            this.initialTouchsId = new List<int>();
            this.twoTouchInteractionMode = TTwoTouchIteractionMode.began;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_TwoFingersPinch_End));
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_TwoFingerDragMovement_End));
        }        
    }


    private void InitThreeTouchsMode()
    {
        if (this.threeTouchInteractionMode != TThreeTouchIteractionMode.began)
        {            
            this.initialTouchsId = new List<int>();
            this.threeTouchInteractionMode = TThreeTouchIteractionMode.began;            
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_ThreeFingerDragMovement_End));
        }
    }
    ////////////////////////
    //  ONE TOUCH MANAGER
    ////////////////////////

    /// <summary>One Touch Manager, this method manages the one touch interaction/gestures.</summary>
    private void OneTouchManager()
    {
        // Just in case
        if (this.touchManagerMode != TTouchManagerMode.one_finger) { return; }

        // If there are one touch on the device                    
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {            
            initialTouchsId = new List<int>() { Input.GetTouch(0).fingerId };            
            this.oneTouchIteractionMode = TOneTouchIteractionMode.selection;           
        }  
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {            
            if ((this.oneTouchIteractionMode == TOneTouchIteractionMode.selection)  && (Input.GetTouch(0).fingerId == this.initialTouchsId[0]))
            {                         
                SelectionManager(Input.GetTouch(0).position);
            }
            else if (this.oneTouchIteractionMode == TOneTouchIteractionMode.drag_movement) {
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneFingerDragMovement_End));                
            }

            this.InitOneTouchMode();
            if (this.twoTouchInteractionMode != TTwoTouchIteractionMode.began) {
                this.InitTwoTouchsMode();
            }
            if (this.threeTouchInteractionMode != TThreeTouchIteractionMode.began)
            {
                this.InitThreeTouchsMode();
            }
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {            
            movementCounter++;
            this.Movement_Manager();
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Stationary)
        {
            // DO NOTHING
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Canceled)
        {            
            this.InitOneTouchMode();
            if (this.oneTouchIteractionMode == TOneTouchIteractionMode.drag_movement)
            {
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneFingerDragMovement_End));
            }
        }
        
    }

    private void SelectionManager(Vector3 currentTouchPosition)
    {        
        // 1. Check if user has clicked in gizmo object
        if (gizmoCamera != null) {
            GameObject gizmoRayCastedGO = Raycast(currentTouchPosition, gizmoCamera, gizmoLayer);
            if (gizmoRayCastedGO != null)
            {
                Debug.Log("Click in gizmo");
                return;
            }
        }        
        // 2. Check if user has clicked in a label object
        //TODO This should be in GetMouseButton
        /*GameObject labelRayCastedGO = Raycast(currentMousePosition, Camera.main, labelsUILayer);
        if (labelRayCastedGO != null)
        {
            isDraggingMouse = true;
            //this.GetComponent<Core>().Do((new CLabelCommand(TLabelCommands.LabelUISelection, rayCastedGO_current)), Constants.undoNotAllowed);
            return;
        }*/

        // 3. Check if user has clicked in a product area
        GameObject rayCastedGO = Raycast(currentTouchPosition, Camera.main, productRootLayer);
        // Debug.Log("hom3r: " + currentTouchPosition.ToString());
        // Debug.Log("hom3r: " + rayCastedGO.name);       
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneSelectionTouch, currentTouchPosition, rayCastedGO, false));
    }

    /// <summary>Method that use Ray Casting technique</summary>
    private GameObject Raycast(Vector3 mouseCurrentPosition, Camera _camera, int _layer)
    {
        // Convert mouse position from screen space to three-dimensional space
        // Build ray, directed from mouse position to “camera forward” way
        Ray ray = _camera.ScreenPointToRay(mouseCurrentPosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layer))
        {
            GameObject rayCastedGO_current = hit.collider.gameObject;       // Now, let’s determine intersected GameObject
            return rayCastedGO_current;
        }
        else
        {
            return null;
        }
    }

    /// <summary>Touch Manager, this method manages the multi touch interaction.</summary>
    private void Movement_Manager()
    {
        if (movementCounter > movementDetector)
        {
            this.oneTouchIteractionMode = TOneTouchIteractionMode.drag_movement;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneFingerDragMovement_Begin));
        }
        if (this.oneTouchIteractionMode == TOneTouchIteractionMode.drag_movement)
        {
            movementCounter = 0;
            Touch touch = Input.GetTouch(0);

            float xMovement = touch.deltaPosition.x;
            float yMovement = touch.deltaPosition.y;

            float touchMovementX = xMovement / Screen.width;     // Get touch x movement in screen %
            float touchMovementY = yMovement / Screen.height;    // Get mouse y movement in screen %   
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneFingerDragMovement, touchMovementX, touchMovementY));
        }
    }

    /// <summary>
    /// Manage gestures that use two fingers
    /// </summary>
    private void TwoTouchManager()
    {
        // Just in case
        if (this.touchManagerMode != TTouchManagerMode.two_fingers) { return; }
        
        // If there are two touches on the device        
        if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.began)
        {            
            twoTouchInitialDistance = this.GetTwoTouchCurrentDistance();                                    // get initial fingers distance
            this.twoTouchInteractionMode = TTwoTouchIteractionMode.moved;                                   // start move mode
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_TwoFingersDragMovement_Begin)); // emit initial

            initialTouchsId = new List<int> { Input.GetTouch(0).fingerId, Input.GetTouch(1).fingerId };
            previousTouchPositions = new List<Vector2>() { Input.GetTouch(0).position, Input.GetTouch(1).position };
        }
        else if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.moved)
        {
            if (!IsSameTwoTouchesId()) { return; }
            List<Vector2> newPosition = new List<Vector2>() { Input.GetTouch(0).position, Input.GetTouch(1).position };
            
            // If both fingers have moved, it is a PAN movement
            bool bothTouchMoves = this.HasTheTouchMoved(Input.GetTouch(0)) && HasTheTouchMoved(Input.GetTouch(1));
            if (bothTouchMoves) { this.TwoFingersDragMovementGesture(); }

            // If any of the finge fingers have moved, it is a PINCH gesture
            bool anyTouchMoves = this.HasTheTouchMoved(Input.GetTouch(0)) || HasTheTouchMoved(Input.GetTouch(1));
            if (anyTouchMoves) { this.TwoFingersPinchGesture(); }                
        }            
        
    }

    /// <summary>
    /// Check touches id, to avoid mistake fingers
    /// </summary>
    /// <returns></returns>
    private bool IsSameTwoTouchesId()
    {
        List<int> currentTouchesId = new List<int> { Input.GetTouch(0).fingerId, Input.GetTouch(1).fingerId };

        if ((initialTouchsId[0] == currentTouchesId[0]) && (initialTouchsId[1] == currentTouchesId[1])) { return true; }

        return false;
    }


    /// <summary>
    /// Calculate the Drag-Movement Gesture and send the data - PAN 
    /// </summary>
    private void TwoFingersDragMovementGesture() {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        Vector2 previousIntermediateTwoTouchposition = 0.5f * (touchZeroPreviousPosition + touchOnePreviousPosition);

        Vector2 touchZeroPosition = touchZero.position;
        Vector2 touchOnePosition = touchOne.position;
        Vector2 currentIntermediateTwoTouchposition = 0.5f * (touchZeroPosition + touchOnePosition);
                                              
        float touchMovementX;
        float touchMovementY;
        
        touchMovementX = (currentIntermediateTwoTouchposition.x - previousIntermediateTwoTouchposition.x) / Screen.width;     // Get mouse x movement in screen %
        touchMovementY = (currentIntermediateTwoTouchposition.y - previousIntermediateTwoTouchposition.y) / Screen.height;    // Get mouse y movement in screen %
        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_TwoFingerDragMovement, touchZeroPosition, touchMovementX, touchMovementY));
    }

    /// <summary>
    /// Check if one touch has moved between one frame and the previous one
    /// </summary>
    /// <param name="touch">True if yes</param>
    /// <returns></returns>
    private bool HasTheTouchMoved(Touch touch) {

        Vector2 touchPreviousPosition = touch.position - touch.deltaPosition;
        Vector2 touchCurrentPosition = touch.position;

        Vector3 touchMovement = touchCurrentPosition - touchPreviousPosition;
        if (touchMovement.magnitude > touchMovementDetectorMargin) { return true; }
        return false;
    }
    
    /// <summary>
    ///  Calculate the Pinch Gesture and send the data - ZOOM
    /// </summary>
    private void TwoFingersPinchGesture()
    {
        //Touch touchZero = Input.GetTouch(0);
        //Touch touchOne = Input.GetTouch(1);        
        //// Get the distance in the previous frame of each touch.
        ////Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        ////Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        ////float touchesPreviousDistance = (touchOnePreviousPosition - touchZeroPreviousPosition).magnitude;
        //// Get the distance in the current frame of each touch.
        //float touchesCurrentDistance = (touchZero.position - touchOne.position).magnitude;

        float touchesCurrentDistance = this.GetTwoTouchCurrentDistance();

        // If the distance between fingers is to small with quick. To avoid strange data comming from the android screen
        if (touchesCurrentDistance < 300f) { return; } 
        
        
        // Distance change in %
        //float distancePercentageChange = touchesCurrentDistance / touchesPreviousDistance;
        float distancePercentageChange = touchesCurrentDistance / twoTouchInitialDistance;
        float inverseDistancePercentageChange = 1 - distancePercentageChange;

        //Testing
        this.twoTouchInitialDistance = touchesCurrentDistance;

        //if (Mathf.Abs(inverseDistancePercentageChange) > 0.05f)
        //{
        //    Debug.Log("touchesCurrentDistance: " + touchesCurrentDistance);
        //    Debug.Log("touchesPreviousDistance: " + touchesPreviousDistance);
        //    Debug.Log("hom3r - inverseDistancePercentageChange: " + inverseDistancePercentageChange);
        //}

        //Clamp
        float temp = Mathf.Clamp(inverseDistancePercentageChange, -0.08f, 0.08f);                

        //Debug.Log("Input: " + inverseDistancePercentageChange + "  output: " + temp);
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_TwoFingersPinch, temp));
    }

    /// <summary>
    /// Calculate two touches current distance
    /// </summary>
    /// <returns></returns>
    private float GetTwoTouchCurrentDistance()
    {
        // Store both touches.
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);        
        float touchesCurrentDistance = (touchZero.position - touchOne.position).magnitude;
        return touchesCurrentDistance;
    }

    private float GetTwoTouchPreviousDistance()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        // Get the distance in the previous frame of each touch.
        Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        float touchesPreviousDistance = (touchOnePreviousPosition - touchZeroPreviousPosition).magnitude;

        return touchesPreviousDistance;
    }


    /// <summary>
    /// Manage gestures that use three fingers
    /// </summary>
    private void ThreeTouchManager()
    {
        // Just in case
        if (this.touchManagerMode != TTouchManagerMode.three_fingers) { return; }

        // If there are three touches on the device  
      
        if (threeTouchInteractionMode == TThreeTouchIteractionMode.began)
        {                
            threeTouchInteractionMode = TThreeTouchIteractionMode.moved;
            initialTouchsId = new List<int> { Input.GetTouch(0).fingerId, Input.GetTouch(1).fingerId, Input.GetTouch(2).fingerId };
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_ThreeFingersDragMovement_Begin)); // emit initial
        }
        else if (this.threeTouchInteractionMode == TThreeTouchIteractionMode.moved)
        {
            if (!IsSameThreeTouchesId()) { return; }

            // If both fingers have moved, it is a PAN movement
            bool threeTouchMoves = this.HasTheTouchMoved(Input.GetTouch(0)) && HasTheTouchMoved(Input.GetTouch(1)) && HasTheTouchMoved(Input.GetTouch(2));
            if (threeTouchMoves) { this.ThreeFingersDragMovementGesture(); }                         
        }                
    }

    /// <summary>
    /// Check touches id, to avoid mistake fingers
    /// </summary>
    /// <returns></returns>
    private bool IsSameThreeTouchesId()
    {
        List<int> currentTouchesId = new List<int> { Input.GetTouch(0).fingerId, Input.GetTouch(1).fingerId, Input.GetTouch(2).fingerId };

        if ((initialTouchsId[0] == currentTouchesId[0]) && (initialTouchsId[1] == currentTouchesId[1]) && (initialTouchsId[2] == currentTouchesId[2])) { return true; }

        return false;
    }

    /// <summary>
    /// Calculate the Drag-Movement Gesture and send the data - PAN 
    /// </summary>
    private void ThreeFingersDragMovementGesture()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        Vector2 previousIntermediateTwoTouchposition = 0.5f * (touchZeroPreviousPosition + touchOnePreviousPosition);

        Vector2 touchZeroPosition = touchZero.position;
        Vector2 touchOnePosition = touchOne.position;
        Vector2 currentIntermediateTwoTouchposition = 0.5f * (touchZeroPosition + touchOnePosition);

        float touchMovementX;
        float touchMovementY;

        touchMovementX = (currentIntermediateTwoTouchposition.x - previousIntermediateTwoTouchposition.x) / Screen.width;     // Get mouse x movement in screen %
        touchMovementY = (currentIntermediateTwoTouchposition.y - previousIntermediateTwoTouchposition.y) / Screen.height;    // Get mouse y movement in screen %

        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_ThreeFingerDragMovement, touchZeroPosition, touchMovementX, touchMovementY));
    }

}
