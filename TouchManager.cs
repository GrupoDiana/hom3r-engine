using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TOneTouchIteractionMode { iddle, selection, navigation, pan_navigation };
public enum TTwoTouchIteractionMode { iddle, began, pinch_zoom, pan_navigation };

public class TouchManager : MonoBehaviour
{
    private int     selectionTouchId;
    private bool    isDragMovement;
    private bool    twoTouchGesture;
    private float   currentCountdownValue;
    private int     movementCounter;
    private int     movementDetector;
    // private float   initTwoTouchesDistance;
    private float   panMovementDetectorMargin;
    private float   touchMovementDetectorMargin;


    private TTwoTouchIteractionMode twoTouchInteractionMode;
    private float twoTouchInitialDistance;
    private float pinchMovementDetectorMargin;


    // Ray-casting variables
    private int     productRootLayer;
    private int     gizmoLayer;
    private Camera  gizmoCamera;

    // Others    
    void Awake()
    {        
        this.selectionTouchId = -1;
        this.isDragMovement = false;
        this.twoTouchGesture = false;
        this.movementCounter = 0;
        this.movementDetector = 5;              // TODO do this in a different way ?
        this.pinchMovementDetectorMargin = 0.35f;
        // this.initTwoTouchesDistance = 0f;
        this.panMovementDetectorMargin = 10f;
        this.touchMovementDetectorMargin = 10f;

        this.twoTouchInteractionMode = TTwoTouchIteractionMode.iddle;

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
            OneTouchManager(); 
            TwoTouchManager(); 
        }
    }

    ////////////////////////
    //  ONE TOUCH MANAGER
    ////////////////////////

    /// <summary>One Touch Manager, this method manages the one touch interaction.</summary>
    private void OneTouchManager()
    {
        if (Input.touchCount == 1)
        {
            //Debug.Log("hom3r - one touch");
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Debug.Log(" TouchPhase.Began ");
                this.selectionTouchId = Input.GetTouch(0).fingerId;
                if (this.isDragMovement)
                {
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_DragMovementEnd));
                    this.isDragMovement = false;
                }
                if (this.twoTouchGesture)
                {
                    this.twoTouchGesture = false;
                    this.twoTouchInteractionMode = TTwoTouchIteractionMode.iddle;
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoomEnd));                                        
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonUp));
                }

            }  
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (!isDragMovement  && (Input.GetTouch(0).fingerId == this.selectionTouchId))
                {
                    //SelectionManager(Input.GetTouch(0).position);
                    Debug.Log("hom3r - selection");
                    SelectionManager(Input.GetTouch(0).position);
                }
                this.selectionTouchId = -1;
                this.movementCounter = 0;
                if (this.isDragMovement) {
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_DragMovementEnd));
                    this.isDragMovement = false;
                }
                if (this.twoTouchInteractionMode != TTwoTouchIteractionMode.iddle) { this.twoTouchInteractionMode = TTwoTouchIteractionMode.iddle; }
                
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
                this.selectionTouchId = -1;
                this.movementCounter = 0;
                this.isDragMovement = false;
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
            isDragMovement = true;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_DragMovementBegin));
        }
        if (isDragMovement)
        {
            movementCounter = 0;
            //Debug.Log("hom3r - Movement");
            //TODO DO SOMETHING
            Touch touch = Input.GetTouch(0);

            float xMovement = touch.deltaPosition.x;
            float yMovement = touch.deltaPosition.y;

            float touchMovementX = xMovement / Screen.width;     // Get touch x movement in screen %
            float touchMovementY = yMovement / Screen.height;    // Get mouse y movement in screen %   
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_DragMovement, touchMovementX, touchMovementY));
        }
    }

    private void TwoTouchManager()
    {
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            //if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.iddle)
            //{
            //    twoTouchInitialDistance = this.GetTwoTouchCurrentDistance();
            //    this.twoTouchInteractionMode = TTwoTouchIteractionMode.pan_navigation;
            //    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonDown));
            //}


            //if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pan_navigation && this.PinchMovementDetector(twoTouchInitialDistance, this.GetTwoTouchCurrentDistance()))
            //{
            //    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonUp));

            //    this.twoTouchInteractionMode = TTwoTouchIteractionMode.pinch_zoom;
            //    isPitchMovement = true;
            //    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoomBegin));

            //}

            /////// new
            
            // Detect which gesture is being made
            if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.iddle)
            {
                twoTouchGesture = true;
                twoTouchInitialDistance = this.GetTwoTouchCurrentDistance();
                this.twoTouchInteractionMode = TTwoTouchIteractionMode.began;
            }
            else if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.began)
            {
                if (this.PanMovementDetector())
                {
                    this.twoTouchInteractionMode = TTwoTouchIteractionMode.pan_navigation;
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonDown));
                } else if (this.PinchMovementDetector_new())
                {
                    twoTouchInitialDistance = this.GetTwoTouchCurrentDistance(); /// ?
                    this.twoTouchInteractionMode = TTwoTouchIteractionMode.pinch_zoom;                    
                }                               
            }
            else if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pan_navigation)
            {
                if (this.PinchMovementDetector_new())
                {
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonUp));
                    twoTouchInitialDistance = this.GetTwoTouchCurrentDistance(); /// ?
                    this.twoTouchInteractionMode = TTwoTouchIteractionMode.pinch_zoom;
                }
            }
            else if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pinch_zoom)
            {
                if (this.PanMovementDetector())
                {
                    this.twoTouchInteractionMode = TTwoTouchIteractionMode.pan_navigation;
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonDown));
                }
            }


            // Make the touch gesture proccess
            if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pan_navigation)
            {
                this.PanMovement();
            }
            else if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pinch_zoom)
            {
                this.PinchMovement();
            }
          
        }
    }

    // Pan
    private void PanMovement() {

        Touch touchZero = Input.GetTouch(0);
        Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        Vector2 touchZeroPosition = touchZero.position;

        float touchMovementX;
        float touchMovementY;
        
        touchMovementX = (touchZeroPosition.x - touchZeroPreviousPosition.x) / Screen.width;     // Get mouse x movement in screen %
        touchMovementY = (touchZeroPosition.y - touchZeroPreviousPosition.y) / Screen.height;    // Get mouse y movement in screen %
        // previousMousePosition = currentMousePosition;                                           // Save current mouse position in pixels
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.MouseManager_CentralButtonDragMovement, touchZeroPosition, touchMovementX, touchMovementY));

    }

    /// <summary>
    /// Checks whether a touch pan movement has occurred or not
    /// </summary>
    /// <param name="originPosition"></param>
    /// <param name="currentPosition"></param>
    /// <returns></returns>
    private bool PanMovementDetector()
    {
        // Check both fingers have moved        
        bool bothTouchMoves = this.HasTheTouchMoved(Input.GetTouch(0)) && HasTheTouchMoved(Input.GetTouch(1));
        if (!bothTouchMoves) { return false; }
        // Check distance between fingers have 
        bool distanceChange = this.HasChangedTouchsDistance(twoTouchInitialDistance, this.GetTwoTouchCurrentDistance());

        return !distanceChange;
    }

    private bool HasChangedTouchsDistance(float initialDistance, float currentDistance)
    {
        if (initialDistance < 0.05f) return false;
        float pinchMovement = Mathf.Abs((currentDistance - initialDistance) / initialDistance);
        if (pinchMovement > pinchMovementDetectorMargin) { return true; }
        return false;
    }

    private bool HasTheTouchMoved(Touch touch) {

        Vector2 touchPreviousPosition = touch.position - touch.deltaPosition;
        Vector2 touchCurrentPosition = touch.position;

        Vector3 touchMovement = touchCurrentPosition - touchPreviousPosition;
        if (touchMovement.magnitude > touchMovementDetectorMargin) { return true; }
        return false;
    }
    
    private Vector2 GetPositionBetweenTwoTouches()
    {        
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchZeroPosition = touchZero.position;
        Vector2 touchOnePosition = touchOne.position;

        Vector2 twoTouchposition = 0.5f *(touchZeroPosition + touchOnePosition);
        return twoTouchposition;
    }


    // Pinch

    private void PinchMovement()
    {
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        // Get the distance in the previous frame of each touch.
        Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        float touchesPreviousDistance = (touchOnePreviousPosition - touchZeroPreviousPosition).magnitude;
        // Get the distance in the current frame of each touch.
        float touchesCurrentDistance = (touchZero.position - touchOne.position).magnitude;
        // Distance change in %
        float distancePercentageChange = touchesCurrentDistance / touchesPreviousDistance;

        float inverseDistancePercentageChange = 1 - distancePercentageChange;
        Debug.Log("hom3r - inverseDistancePercentageChange: " + inverseDistancePercentageChange);
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoom, inverseDistancePercentageChange));
    }

    private bool PinchMovementDetector(float initialDistance, float currentDistance)
    {
        if (initialDistance < 0.05f) return false;
        float pinchMovement = Mathf.Abs((currentDistance - initialDistance)/ initialDistance);        
        if (pinchMovement > pinchMovementDetectorMargin) { return true; }
        return false;
    }


    private bool PinchMovementDetector_new()
    {
        // Check both fingers have moved        
        bool anyTouchMoves = this.HasTheTouchMoved(Input.GetTouch(0)) || HasTheTouchMoved(Input.GetTouch(1));
        if (!anyTouchMoves) { return false; }
        // Check distance between fingers have 
        bool distanceChange = this.HasChangedTouchsDistance(twoTouchInitialDistance, this.GetTwoTouchCurrentDistance());

        return distanceChange;      
    }

    private float GetTwoTouchCurrentDistance()
    {
        // Store both touches.
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);
        // Get the distance in the previous frame of each touch.
        //Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        //Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        //float touchesPreviousDistance = (touchOnePreviousPosition - touchZeroPreviousPosition).magnitude;
        // Get the distance in the current frame of each touch.
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



    public IEnumerator StartCountdown(float countdownValue = 10)
    {
        currentCountdownValue = countdownValue;
        while (currentCountdownValue > 0)
        {
            Debug.Log("Countdown: " + currentCountdownValue);
            yield return new WaitForSeconds(1.0f);

            currentCountdownValue--;
        }
    }
}
