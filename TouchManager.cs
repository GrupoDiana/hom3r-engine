using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TOneTouchIteractionMode { iddle, selection, navigation, pan_navigation };
public enum TTwoTouchIteractionMode { iddle, pinch_zoom, pan_navigation };

public class TouchManager : MonoBehaviour
{
    private int     selectionTouchId;
    private bool    isDragMovement;
    private bool    isPitchMovement;
    private float   currentCountdownValue;
    private int     movementCounter;
    private int     movementDetector;
    private float   initTwoTouchesDistance;

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
        this.isPitchMovement = false;
        this.movementCounter = 0;
        this.movementDetector = 5;         // TODO do this in a different way ?
        this.initTwoTouchesDistance = 0f;
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
                this.selectionTouchId = Input.GetTouch(0).fingerId;
                if (this.isDragMovement)
                {
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_DragMovementEnd));
                    this.isDragMovement = false;
                }
                if (this.isPitchMovement)
                {
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoomEnd));
                    this.isPitchMovement = false;
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
            if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.iddle) {
                twoTouchInitialDistance = this.GetTwoTouchCurrentDistance();
                this.twoTouchInteractionMode = TTwoTouchIteractionMode.pan_navigation;
            }
            
            if (this.PinchMovementDetector(twoTouchInitialDistance, this.GetTwoTouchCurrentDistance()))
            {
                this.twoTouchInteractionMode = TTwoTouchIteractionMode.pinch_zoom;
                isPitchMovement = true;
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoomBegin));
            } 

            if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pan_navigation)
            {
                Debug.Log("PAN NAVIGATION");
            } else if (this.twoTouchInteractionMode == TTwoTouchIteractionMode.pinch_zoom)
            {
                this.PinchMovement();
            }

            //if (!isPitchMovement)
            //{
            //    isPitchMovement = true;
            //    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoomBegin));
            //}
            
            // Store both touches.
            //Touch touchZero = Input.GetTouch(0);
            //Touch touchOne = Input.GetTouch(1);
            
            //// Get the distance in the previous frame of each touch.
            //Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
            //Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
            //float touchesPreviousDistance = (touchOnePreviousPosition - touchZeroPreviousPosition).magnitude;
            //// Get the distance in the current frame of each touch.
            //float touchesCurrentDistance = (touchZero.position - touchOne.position).magnitude;
            //// Distance change in %
            //float distancePercentageChange = touchesCurrentDistance / touchesPreviousDistance;

            //float inverseDistancePercentageChange = 1 - distancePercentageChange;
            //Debug.Log("hom3r: " + inverseDistancePercentageChange);
            //hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoom, inverseDistancePercentageChange));            
        }
    }
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
        Debug.Log("hom3r: " + inverseDistancePercentageChange);
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_PinchZoom, inverseDistancePercentageChange));
    }
    private bool PinchMovementDetector(float initialDistance, float currentDistance)
    {
        float movement = initialDistance - currentDistance;
        if (movement > pinchMovementDetectorMargin) { return true; }
        return false;
    }

    private float GetTwoTouchCurrentDistance()
    {
        // Store both touches.
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);
        // Get the distance in the previous frame of each touch.
        Vector2 touchZeroPreviousPosition = touchZero.position - touchZero.deltaPosition;
        Vector2 touchOnePreviousPosition = touchOne.position - touchOne.deltaPosition;
        float touchesPreviousDistance = (touchOnePreviousPosition - touchZeroPreviousPosition).magnitude;
        // Get the distance in the current frame of each touch.
        float touchesCurrentDistance = (touchZero.position - touchOne.position).magnitude;
        return touchesCurrentDistance;
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
