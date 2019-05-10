using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    private int     selectionTouchId;
    private bool    isMovement;
    private float   currentCountdownValue;
    private int     movementCounter;
    private int     movementDetector;
    private float   initTwoTouchesDistance;

    // Ray-casting variables
    private int     productRootLayer;
    private int     gizmoLayer;
    private Camera  gizmoCamera;

    // Others    
    void Awake()
    {        
        this.selectionTouchId = -1;
        this.isMovement = false;
        this.movementCounter = 0;
        this.movementDetector = 5;         // TODO do this in a different way ?
        this.initTwoTouchesDistance = 0f;
              
        ////////////////////////////////////////
        // Initialize Ray-casting variables
        ////////////////////////////////////////
        gizmoCamera = GameObject.FindWithTag("GizmoCamera").GetComponent<Camera>();
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
            Debug.Log("hom3r - one touch");
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                this.selectionTouchId = Input.GetTouch(0).fingerId;
                this.isMovement = false;
            }  
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (!isMovement  && (Input.GetTouch(0).fingerId == this.selectionTouchId))
                {
                    //SelectionManager(Input.GetTouch(0).position);
                    Debug.Log("hom3r - selection");
                    SelectionManager2(Input.GetTouch(0).position);
                }
                this.selectionTouchId = -1;
                this.movementCounter = 0;
                this.isMovement = false;
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
                this.isMovement = false;
            }
        }
    }



    
    /// <summary>
    ///When we touch on an object we confirm it
    ///When we touch on a confirmed object we unselect it
    ///When we press the **** and the key "Control" we make multiple confirmation
    /// </summary>
    /// <param name="position"></param>
    //private void SelectionManager(Vector2 position)
    //{        
    //    GameObject rayCastedGameObject;         //Object ray-casted


    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    int productLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, Mathf.Infinity, productLayer))
    //    {
    //        rayCastedGameObject = hit.collider.gameObject;

    //        //"DESCONFIRM" the ray-casted object if the object is already confirmed                                
    //        if (this.GetComponent<SelectionManager>().IsConfirmedGameObject(rayCastedGameObject))
    //        {
    //            //Check if we have only one component or area selected
    //            bool onlyOneComponentOrArea = false;
    //            if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
    //            {
    //                onlyOneComponentOrArea = (this.GetComponent<SelectionManager>().GetNumberOfConfirmedGameObjects() == 1);
    //            }
    //            else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
    //            {
    //                onlyOneComponentOrArea = (this.GetComponent<SelectionManager>().GetNumberOfConfirmedSpecialNode() == 1);
    //            }

    //            //If is only one we deselect this one                    )
    //            if (onlyOneComponentOrArea)
    //            {
    //                this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.ConfirmationOff, rayCastedGameObject)), Constants.undoNotAllowed);
    //            }
    //            else 
    //            {
    //                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) /*|| keyControlPressed*/)
    //                {
    //                    //Multiple Confirmation, descorfirn one by one
    //                    this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.ConfirmationOff, rayCastedGameObject)), Constants.undoNotAllowed);
    //                }
    //                else
    //                {
    //                    //Multiple Desconfirmation, descorfirm all except the selected one
    //                    this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.Multiple_Confirmation_Desconfirmation, rayCastedGameObject)), Constants.undoNotAllowed);
    //                }
    //            }
    //        }
    //        //CONFIRM the ray-casted object
    //        else
    //        {
    //            //Multiple CONFIRMATION
    //            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) /*|| keyControlPressed*/)
    //            {
    //                //Debug.Log("MULTIPLE Selection Active");                        
    //                this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.MultipleConfirmation, rayCastedGameObject)), Constants.undoNotAllowed);
    //            }
    //            //Single CONFIRMATION
    //            else
    //            {
    //                //Debug.Log("SINGLE Selection Active");                 
    //                this.GetComponent<Core>().Do((new CSelectionCommand(TSelectionCommands.SingleConfirmationByMouse, rayCastedGameObject)), Constants.undoNotAllowed);
    //            }
    //        }
    //    }
    //}

    
    private void SelectionManager2(Vector3 currentTouchPosition)
    {        
        // 1. Check if user has clicked in gizmo object
        GameObject gizmoRayCastedGO = Raycast(currentTouchPosition, gizmoCamera, gizmoLayer);
        if (gizmoRayCastedGO != null)
        {
            Debug.Log("Click in gizmo");            
            return;
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
        Debug.Log("hom3r: " + currentTouchPosition.ToString());
        Debug.Log("hom3r: " + rayCastedGO.name);       
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_OneTouch, currentTouchPosition, rayCastedGO, false));
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
            isMovement = true;
        }
        if (isMovement)
        {
            movementCounter = 0;
            Debug.Log("hom3r - Movement");
            //TODO DO SOMETHING
            Touch touch = Input.GetTouch(0);

            float xMovement = touch.deltaPosition.x;
            float yMovement = touch.deltaPosition.y;

            float touchMovementX = xMovement / Screen.width;     // Get touch x movement in screen %
            float touchMovementY = yMovement / Screen.height;    // Get mouse y movement in screen %   
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.TouchManager_DragMovemment, touchMovementX, touchMovementY));
        }
    }

    private void TwoTouchManager()
    {
        // If there are two touches on the device...
        if (Input.touchCount == 2)
        {
            isMovement = true;
            // Store both touches.
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
