using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TMainAxis { Vertical, Horizontal};
//public enum THom3rNavigationMode { regular, pan };

public class NavigationManager : MonoBehaviour {
        
    // Scene parameter      
    Vector3 cameraInitialPosition;      // Store the initial position of the camera within the plane. Always (0, 0, Zo)
    float cameraMinimumDistance;        // Store the minimum distance allowed between camera and 3D model      

    Bounds modelBoundingBox;            // Store the 3D model bounding box
    float verticalFieldOfView_rad;      // 
    float horizontalFieldOfView_rad;    // 


    // Navigation mode parameters
    TMainAxis mainAxis;                             // Store the 3d model main axis for navigation
    TNavigationSystemMode navigationSystemMode;         // Store the navigation mode in use
    CCoordinateSystemManager navigationSystem;      // Define the navigation mode
    PanNavigatioManager panNavigationSystem;

    // Control parameters
    bool navigationInitialized;          // Store if the navigation has been initialized or not 

    // Correction Parameters
    float pseudoRadioCorrection;        // Store the parameter to make the pseudo-radio correction
    float pseudoRadioScale;
    
    /// <summary>Initialize variables and structures</summary>
    private void Awake()
    {
        hom3r.quickLinks.navigationSystemObject = GameObject.FindGameObjectWithTag("NavigationSystem_tag");       //Initialize the quick link to the orbit plane object

        this.InitializeVariables();       
    }

    private void InitializeVariables()
    {
        navigationInitialized = false;       // This parameter controls when the navigation has been initialized

        cameraInitialPosition = Vector3.zero;       // Initialize Scene parameters 
        cameraMinimumDistance = 0.0f;               // Initialize Scene parameters
        CalculateFielOfView();                      // Initialize Scene parameters               

        mainAxis = TMainAxis.Vertical;              // Initialize Navigation mode parameters

        pseudoRadioCorrection = 100f;               // Initialize Correction Parameters
        pseudoRadioScale = 5f;                      //

        //Navigation System        
        InitNavigationSystem();
        //navigationSystem = new CSphericalCoordinatesManager();
        //navigationSystem = new CLimitedSphericalCoordinatesManager();     
        //navigationSystem = new CSpheroidCoordinatesManager();
        //navigationSystem = new CEllipsoidCoordinatesManager();
        panNavigationSystem = new PanNavigatioManager();
        hom3r.state.navigationMode = THom3rNavigationMode.regular;        
    }

    private void InitNavigationSystem()
    {
        //Get the navigation system to use from configuration module
        navigationSystemMode = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigationSystemMode();
        //Set that navigation system
        if (navigationSystemMode == TNavigationSystemMode.Spheroid)
        {
            navigationSystem = new CSpheroidCoordinatesManager();
        }
        else if (navigationSystemMode == TNavigationSystemMode.Ellipsoid)
        {
            navigationSystem = new CEllipsoidCoordinatesManager();
        }
        else if (navigationSystemMode == TNavigationSystemMode.Spherical)
        {
            navigationSystem = new CSphericalCoordinatesManager();
        }
    }


    ////////////////////////////////////
    // Initialize Navigation methods
    ////////////////////////////////////
    public void CheckConfigurationUpdated()
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigationSystemMode() != this.navigationSystemMode)
        {
            // Init with the new navigation system
            this.StopNavigation();
            this.InitNavigation();
        }
    }

    public void StopNavigation()
    {
        Debug.Log("Stop Navigation");
        this.InitializeVariables();
    }
        
    /// <summary>Initialize Navigation</summary>
    public void InitNavigation()
    {
        string navigation_axis = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetModelNavigationAxis();
        this.InitNavigation(navigation_axis);
    }

    /// <summary>Initialize Navigation</summary>
    /// <param name="newMainAxis">3D model Main axis of navigation</param>
    public void InitNavigation(string newMainAxis_text)
    {
        Debug.Log("Init Navigation");

        TMainAxis newMainAxis = ParseMainAxis(newMainAxis_text);    // Parse input main Axis
        SetNavigationAxis(newMainAxis);                             // Change main axis                    
        this.modelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox(true); //Get model bounding box        
        if (this.modelBoundingBox.size == Vector3.zero) { return; }
        InitOrbitPlanePosition();                                   // Initialize Orbit Plane position
        InitCameraRotation();                                       // Initialize Camera rotation        
        InitNavigationHelper();
        cameraInitialPosition = CalculateInitialCameraPosition();           // Calculate the initial Camera position
        InitSecundaryCameraPosition(cameraInitialPosition);
        Vector3 extentsVector = Get3DModelExtentsVector(modelBoundingBox);  // Get extents vector from bounding box in terms of main axis
        //Vector2 fielOfViewVector = GetFieldOfView();
        Vector3 pointToLook;    //Vector to store direction in which the camera has to look
        TNavigationSystemConstraints navigationConstraints = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetNavigationConstraints();
        if (navigationSystem.Init(extentsVector, cameraInitialPosition, navigationConstraints, out cameraMinimumDistance, out pointToLook))
        {
            panNavigationSystem.Init(extentsVector, /*fielOfViewVector,*/ cameraMinimumDistance);
            navigationInitialized = true;            
            MoveCameraWithinThePlane(cameraInitialPosition);        // Move camera to the initial position
            InitPseudoRadioCorrection();
            OrientateCamera(pointToLook);
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_NavigationInitiaded));
        }
        else
        {
            Debug.Log("Error camera initial position nor allowed");
        }                   
    }

    /// <summary>Parse input main axis in text format to TMainAxis</summary>
    /// <param name="mainAxis_text">string that contains the MainAxis</param>
    /// <returns>TMainAxis data</returns>
    private TMainAxis ParseMainAxis(string mainAxis_text)
    {
        if (mainAxis_text == null) { return TMainAxis.Vertical; }
        TMainAxis mainAxis = TMainAxis.Vertical;
        switch (mainAxis_text.ToLower())
        {
            case "vertical":
                mainAxis = TMainAxis.Vertical;
                break;
            case "horizontal":
                mainAxis = TMainAxis.Horizontal;
                break;           
        }
        return mainAxis;
    }

    /// <summary>Calculate the Initial camera position</summary>
    private Vector3 CalculateInitialCameraPosition()
    {
        CalculateFielOfView();      //Update the field of view
        float z0 = modelBoundingBox.extents.z + Math.Max((modelBoundingBox.extents.y / Mathf.Tan(verticalFieldOfView_rad)), modelBoundingBox.extents.x / Mathf.Tan(horizontalFieldOfView_rad));        
        return (new Vector3(0, 0, -z0));
    }

    /// <summary>Calculate the Field Of View of the camera based on the camera aspect ratio</summary>
    private void CalculateFielOfView()
    {
        verticalFieldOfView_rad = Camera.main.fieldOfView * Mathf.Deg2Rad * .5f;    //We use the half of this angle
        float cameraHeightAt1 = Mathf.Tan(verticalFieldOfView_rad);
        horizontalFieldOfView_rad = Mathf.Atan(cameraHeightAt1 * Camera.main.aspect);        
    }
    
    /// <summary>Move the orbit plane to the centre of the bounding box, and rotate according to the main Axis </summary>
    private void InitOrbitPlanePosition()
    {
        this.transform.position = modelBoundingBox.center;  // Move the Orbit plane to the centre of the target BB          

        if ( mainAxis == TMainAxis.Horizontal)
        {
            this.transform.eulerAngles = new Vector3(0, 0, 0);  //Initialize orbit plane orientation for horizontal orientation
            hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().InitNavigationHelperRotation(new Vector3(0, 0, 180));
        }
        else if(mainAxis == TMainAxis.Vertical)
        {            
            this.transform.eulerAngles = new Vector3(0, 0, 90); //Initialize orbit plane orientation for vertical orientation
            hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().InitNavigationHelperRotation(new Vector3(0, 0, -90));
        }
        else
        {
            //Error
        }
    }

    /// <summary>Initialize the camera rotation according to the main Axis.
    /// Y axis of the camera always to up at the beginning</summary>
    private void InitCameraRotation()
    {
        if (mainAxis == TMainAxis.Horizontal)
        {
            Camera.main.transform.eulerAngles = new Vector3(0, 0, 0);  
        }
        else if (mainAxis == TMainAxis.Vertical)
        {
            Camera.main.transform.Rotate(new Vector3(0, 0, -90));       //Rotate to have camera y axis to up
        }
    }
    
    public void SetNavigationAxis(TMainAxis newMainAxis)
    {
        mainAxis = newMainAxis;
    }

    public string GetNavigationAxis()
    {                       
        string _mainAxis;
        // Parse output data
        switch (this.mainAxis)
        {
            case TMainAxis.Vertical:
                _mainAxis= "vertical";
                break;
            case TMainAxis.Horizontal:
                _mainAxis = "horizontal";
                break;
            default:
                _mainAxis = "";
                break;
        }
        return _mainAxis;
    }

    public void ChangeCoordinateSystem()
    {

    }

    /// <summary>
    /// Return the 3d model extents vector taking into account the navigation main axis. 
    /// If the main axis is horizontal the extents vector correspond with the boundingBox.extents
    /// If the main axis is vertical the extents vector correspond with the boundinBox.extents but changing exchanging x and y
    /// </summary>
    /// <param name="boundingBox">Bounding box of the 3D object</param>
    /// <returns>The 3D model extent vector taking into account the navigation main axis</returns>
    private Vector3 Get3DModelExtentsVector(Bounds boundingBox)
    {        
        if (mainAxis == TMainAxis.Horizontal)
        {
            return new Vector3(boundingBox.extents.x, boundingBox.extents.y, boundingBox.extents.z);
        }
        else
        {
            return new Vector3(boundingBox.extents.y, boundingBox.extents.x, boundingBox.extents.z);            
        }
        
    }
    /// <summary>
    /// Return the camera field of view taking into account the navigation main axis. 
    /// If the main axis is horizontal the x coordinates correspond with the camera horizontal field of view angle
    /// If the main axis is vertical the x coordinates correspond with the camera vertical field of view angle
    /// </summary>
    /// <returns>The field of view taking into account the navigation axis</returns>
    private Vector2 GetFieldOfView(bool force = false)
    {
        if (force) { this.CalculateFielOfView();}
        if (mainAxis == TMainAxis.Horizontal)
        {
            return new Vector2(horizontalFieldOfView_rad, verticalFieldOfView_rad);
        }
        else
        {
            return new Vector2(verticalFieldOfView_rad, horizontalFieldOfView_rad);
        }
    }

    //private void InitHelpPlaneSize(Bounds boundingBox)
    //{
    //    Transform[] ts = hom3r.quickLinks.navigationSystemObject.GetComponentsInChildren<Transform>();
    //    foreach (Transform child in ts)
    //    {
    //        if (child.name == "Plane")
    //        {
    //            child.transform.localScale = new Vector3(boundingBox.extents.x /5.0f, 1, boundingBox.extents.z /5.0f ) ;                
    //        }
    //        if (child.name == "Cube")
    //        {
    //            child.transform.localScale = new Vector3(boundingBox.extents.x * 2, boundingBox.extents.y * 2, boundingBox.extents.z * 2);                
    //        }
    //        //child is your child transform
    //    }
    //}

    private void InitNavigationHelper()
    {
        hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().InitNavigationAssistantsPosition(modelBoundingBox.center);
        hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationHelper>().InitHelpPlaneSize(modelBoundingBox);
    }


    ////////////////////////////
    // MOUSE RECEPTION METHODS
    ////////////////////////////

    /// <summary>
    /// Receive the movement of the mouse and transfers it to the appropriate navigation system
    /// </summary>
    /// <param name="mouseMovementX">Movement of the mouse in X axis, % of the screen size</param>
    /// <param name="mouseMovementY">Movement of the mouse in Y axis, % of the screen size</param>
    /// <param name="mouseWhellMovement">Movement of the whell mouse, % of the screen size</param>
    public void SetMouseMovement(float mouseMovementX, float mouseMovementY, float mouseMovementPercentageX, float mouseMovementPercentageY, float mouseWhellMovement)
    {
        if (hom3r.state.navigationMode == THom3rNavigationMode.regular)
        {            
            this.SetMouseMovementRegularNavigation(mouseMovementX, mouseMovementY, mouseMovementPercentageX, mouseMovementPercentageY, mouseWhellMovement);
        } else
        {
            this.SetMouseMovementPanNavigation(mouseMovementX, mouseMovementY, mouseWhellMovement);
        }
    }

    /// <summary>Receive the movement of the mouse and apply them</summary>
    /// <param name="mouseMovementX">Movement of the mouse in X axis, % of the screen size</param>
    /// <param name="mouseMovementY">Movement of the mouse in Y axis, % of the screen size</param>
    /// <param name="mouseWhellMovement">Movement of the whell mouse, % of the screen size</param>
    private void SetMouseMovementRegularNavigation(float mouseMovementX, float mouseMovementY, float mouseMovementXPercentage, float mouseMovementYPercentage, float mouseWhellMovement)
    {
        float latitudeVariation;
        float longitudeVariation;
        float radialVariation;
                
        // Check if the navigation is activated or not        
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigation()) { return;  }
        if (hom3r.state.navigationBlocked) { return; }
        
        if ((navigationInitialized) & ((mouseMovementX != 0) || (mouseMovementY != 0) || (mouseWhellMovement != 0.0f)))
        {
            ///////////////////////////////////////////////////
            //  Calculate pseudoLatitude and pseudoLongitude
            ///////////////////////////////////////////////////
            // Vertical axis ----> Latitude = Mouse Y     
            //   Horizontal axis --> Latitude = Mouse X
            if (mainAxis == TMainAxis.Vertical)
            {
                //latitudeVariation = -mouseMovementY;
                //longitudeVariation = mouseMovementX;
                latitudeVariation = GetLatitudeVariationWithMappingCorrection(-mouseMovementY, -mouseMovementYPercentage);
                longitudeVariation = GetLongitudeVariationWithMappingCorrection(mouseMovementX, mouseMovementXPercentage);                                
            }
            else
            {
                //latitudeVariation = -mouseMovementX;
                //longitudeVariation = -mouseMovementY;
                latitudeVariation = GetLatitudeVariationWithMappingCorrection(-mouseMovementX, -mouseMovementXPercentage);
                longitudeVariation = GetLongitudeVariationWithMappingCorrection(-mouseMovementY, -mouseMovementYPercentage);
            }

            //////////////////////////
            //  Calculate pseudoRadio
            //////////////////////////
            radialVariation = 0.0f;
            if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigationZoom())
            {
                radialVariation = -1.0f * CalculatePseudoRadioVariation(mouseWhellMovement);    //Calculate pseudo Radio
            }
            

            /////////////////////////////////////////////////
            // Emit pseudoData just in case someone need it
            /////////////////////////////////////////////////
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PseudoLatitudeMovement, latitudeVariation));     // Emit event
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PseudoLongitudeMovement, longitudeVariation));   // Emit event
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PseudoRadioMovement, radialVariation));   // Emit event


            ////////////////
            // Move Camera
            ////////////////
            Vector3 newCameraPosition = new Vector3();
            float planeRotation;
            Vector3 pointToLook;
            Vector2 fielOfViewVector = GetFieldOfView(true);    // Get current field of view

            //Calculate movement           
            navigationSystem.CalculateCameraPosition(latitudeVariation, longitudeVariation, radialVariation, fielOfViewVector, out newCameraPosition, out planeRotation, out pointToLook);            
            //Apply movement
            RotateCameraPlane(planeRotation);
            MoveCameraWithinThePlane(newCameraPosition);
            OrientateCamera(pointToLook);

            //Emit event
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_CameraMoved));                 
        }
    }


    private float GetLatitudeVariationWithMappingCorrection(float mouseMovement, float mouseMovementPercentage)
    {        
        if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLatitudeInteractionCorrectionMode() == TInteractionMappingCorrectionMode.none)
        {
            return mouseMovement;
        }
        else
        {
            return mouseMovementPercentage;
        }                
    }
    private float GetLongitudeVariationWithMappingCorrection(float mouseMovement, float mouseMovementPercentage)
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLongitudeInteractionCorrectionMode() == TInteractionMappingCorrectionMode.none)
        {
            return mouseMovement;
        }
        else
        {
            return mouseMovementPercentage;
        }
    }



    /// <summary>Receive the movement of the mouse and apply them</summary>
    /// <param name="mouseMovementX">Movement of the mouse in X axis, % of the screen size</param>
    /// <param name="mouseMovementY">Movement of the mouse in Y axis, % of the screen size</param>
    /// <param name="mouseWhellMovement">Movement of the whell mouse, % of the screen size</param>
    private void SetMouseMovementPanNavigation(float mouseMovementX, float mouseMovementY, float mouseWhellMovement)
    {
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActivePanNavigation()) { return; }

        Vector2 fielOfViewVector = GetFieldOfView(true);    // Get current field of view
        Vector3 newPlanePosition = panNavigationSystem.CalculatePlanePosition(mouseMovementX, mouseMovementY, this.transform.position, fielOfViewVector);        
        this.MovePlane(newPlanePosition);
    }

    private float CalculatePseudoRadioVariation(float mouseWheelMovement)
    {       
        return mouseWheelMovement * pseudoRadioCorrection;
    }

    /// <summary>Initialize pseudo radio correction parameter.
    /// The idea is that the movements of the radius are independent of the geometry
    /// of the object and the distance to the camera.</summary>
    private void InitPseudoRadioCorrection()
    {
        // TODO What if the minimum distance was not calculated in the same plane as the initial position? 
        pseudoRadioCorrection = pseudoRadioScale * (cameraInitialPosition.magnitude - cameraMinimumDistance) / 100;
    }

    ////////////////////////////
    // TOUCH RECEPTION METHODS
    ////////////////////////////
    public void SetTouchMovement(float mouseMovementX, float mouseMovementY)
    {
        float pseudoLatitude;
        float pseudoLongitude;
        
        // Check if the navigation is activated or not
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigation()) { return; }
        if (hom3r.state.navigationBlocked) { return; }
        if ((navigationInitialized) & ((mouseMovementX != 0) || (mouseMovementY != 0)))
        {
            ///////////////////////////////////////////////////
            //  Calculate pseudoLatitude and pseudoLongitude
            ///////////////////////////////////////////////////
            // Vertical axis ----> Latitude = Mouse Y     
            //   Horizontal axis --> Latitude = Mouse X
            if (mainAxis == TMainAxis.Vertical)
            {
                pseudoLatitude = -mouseMovementY; 
                pseudoLongitude = mouseMovementX;  
            }
            else
            {
                pseudoLatitude = -mouseMovementX;
                pseudoLongitude = -mouseMovementY;
            }
            
            /////////////////////////////////////////////////
            // Emit pseudoData just in case someone need it
            /////////////////////////////////////////////////
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PseudoLatitudeMovement, pseudoLatitude));     // Emit event
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PseudoLongitudeMovement, pseudoLongitude));   // Emit event
            

            ////////////////
            // Move Camera
            ////////////////
            Vector3 newCameraPosition = new Vector3();
            float planeRotation;
            Vector3 pointToLook;
            Vector2 fielOfViewVector = GetFieldOfView(true);    // Get current field of view

            //Calculate movement            
            navigationSystem.CalculateCameraPosition(pseudoLatitude, pseudoLongitude, 0.0f, fielOfViewVector, out newCameraPosition, out planeRotation, out pointToLook);
            //Apply movement
            RotateCameraPlane(planeRotation);
            MoveCameraWithinThePlane(newCameraPosition);
            OrientateCamera(pointToLook);
            //Emit event
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_CameraMoved));
        }
    }

    public void SetTouchPithZoom(float percentageOfSize)
    {
        // Check if the zoom is activated
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveNavigationZoom()) { return; }
        
        //float pseudoRadio = cameraInitialPosition.magnitude * percentageOfSize;
        float pseudoRadio = GetCurrentCameraPositionWithinPlane().magnitude * percentageOfSize;

        /////////////////////////////////////////////////
        // Emit pseudoData just in case someone need it
        /////////////////////////////////////////////////        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PseudoRadioMovement, pseudoRadio));   // Emit event
        
        ////////////////
        // Move Camera
        ////////////////
        Vector3 newCameraPosition = new Vector3();
        float planeRotation;
        Vector3 pointToLook;
        Vector2 fielOfViewVector = GetFieldOfView(true);    // Get current field of view

        //Calculate movement        
        navigationSystem.CalculateCameraPosition(0.0f, 0.0f, pseudoRadio, fielOfViewVector, out newCameraPosition, out planeRotation, out pointToLook);
        //Apply movement
        RotateCameraPlane(planeRotation);
        MoveCameraWithinThePlane(newCameraPosition);
        OrientateCamera(pointToLook);
        //Emit event
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_CameraMoved));
    }

    private Vector2 GetCurrentCameraPositionWithinPlane()
    {
        Vector3 currentCameraPosition = Camera.main.transform.localPosition;

        if (mainAxis == TMainAxis.Vertical)
        {
            return new Vector2(currentCameraPosition.x, currentCameraPosition.z);
        }
        else
        {
            return new Vector2(currentCameraPosition.x, currentCameraPosition.z);
        }

        //= new Vector3(newCameraPosition.x, 0.0f, newCameraPosition.z);
    }



    /////////////////////////////
    // CAMERA Private Methods    
    /////////////////////////////

    private void InitSecundaryCameraPosition(Vector3 newCameraPosition)
    {
        GameObject secundaryCamera = GameObject.FindGameObjectWithTag("secundarycamera");

        Vector3 current = new Vector3(newCameraPosition.z * -5f, newCameraPosition.z * -5f, newCameraPosition.z * 5f);
        //float distance = Vector3.Distance(newCameraPosition, modelBoundingBox.center);        
        float distance = Mathf.Abs(newCameraPosition.z - modelBoundingBox.extents.z);
        Vector3 direction = newCameraPosition.normalized;
        direction.x = 1f;
        direction.y = 1f;
        direction.z = -1f;
        Vector3 temp = newCameraPosition + 3f * distance * direction;


        //secundaryCamera.transform.localPosition = new Vector3(newCameraPosition.z * -5f, newCameraPosition.z * -5f, newCameraPosition.z * 5f);
        secundaryCamera.transform.localPosition = temp;
        InitSecundaryCameraOrientation();
    }
    private void InitSecundaryCameraOrientation()
    {
        GameObject secundaryCamera = GameObject.FindGameObjectWithTag("secundarycamera");
        secundaryCamera.transform.LookAt(modelBoundingBox.center);

        //Reorient the camera: fix the orientation of the camera to orientate it parallel to the orbit plane
        //Vector3 rotation = new Vector3();
        //if (mainAxis == TMainAxis.Vertical)
        //{
        //    rotation = new Vector3(secundaryCamera.transform.localEulerAngles.x, secundaryCamera.transform.localEulerAngles.y, -90);
        //}
        //else
        //{
        //    rotation = new Vector3(secundaryCamera.transform.localEulerAngles.x, secundaryCamera.transform.localEulerAngles.y, 0);
        //}
        //secundaryCamera.transform.localEulerAngles = rotation;
    }

    /// <summary>Moves the navigation plane</summary>
    /// <param name="newPlanePosition">New plane position</param>
    private void MovePlane(Vector3 newPlanePosition)
    {
        if (navigationInitialized)
        {
            this.transform.position = newPlanePosition;
        }
    }

    /// <summary>Moves the camera within the plane to a new position.</summary>
    /// <param name="newCameraPosition">New position of the camera. 
    /// The y-coordinate of the position will be ignored.</param>
    private void MoveCameraWithinThePlane(Vector3 newCameraPosition) {
        if (navigationInitialized)
        {
            Camera.main.transform.localPosition = new Vector3 (newCameraPosition.x, 0.0f, newCameraPosition.z);
            //Move Helper camera
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().MoveCameraHelper(newCameraPosition);
        }        
    }

    private void RotateCameraPlane(float planeRotation)
    {
        if (navigationInitialized)
        {
            // Rotate around the centre of the plane = boundingbox.center (because we moved the plane to put there)
            Vector3 targetCenter = this.transform.position;     
            Vector3 orbitVector;
            if (mainAxis == TMainAxis.Vertical) { orbitVector = new Vector3(0.0f, 1.0f, 0.0f); }
            else { orbitVector = new Vector3(1.0f, 0.0f, 0.0f); }
            
            //Rotates the transform about axis passing through point in world coordinates by angle degrees.
            hom3r.quickLinks.navigationSystemObject.transform.RotateAround(targetCenter, orbitVector, planeRotation * Mathf.Rad2Deg );
        }        
    }

    /// <summary>Orients the camera to a specific point</summary>
    /// <param name="pointToLook">Coordinates of the point to look, local to the plane.</param>
    private void OrientateCamera(Vector3 pointToLook)
    {
        if (navigationInitialized)
        {            
            Vector3 pointToLookWorld = hom3r.quickLinks.navigationSystemObject.transform.TransformPoint(pointToLook);      // Transform the coordinates of the point to global            
            Camera.main.transform.LookAt(pointToLookWorld);            // Orientate the camera
            //pointToLook = Vector3.zero;
            //Camera.main.transform.LookAt(pointToLook);            // Orientate the camera            
            //Debug.Log(pointToLook);
            //Debug.Log(pointToLook_world);
            
            //Reorient the camera: fix the orientation of the camera to orientate it parallel to the orbit plane
            Vector3 rotation = new Vector3();
            if (mainAxis == TMainAxis.Vertical)
            {
                rotation = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, -90);
            }
            else
            {
                rotation = new Vector3(Camera.main.transform.localEulerAngles.x, Camera.main.transform.localEulerAngles.y, 0);
            }
            Camera.main.transform.localEulerAngles = rotation;

            // Reorient HELPER camera
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().SetCameraOrientationHelper(pointToLookWorld);

            //this.InitSecundaryCameraOrientation();
        }
    }

    
    ////////////////////////////
    // MODE Methods
    ////////////////////////////    
    /// <summary>
    /// Set if the Pan navigation is activated or not
    /// </summary>
    /// <param name="_enabled">True if pan navigation has to be enabled</param>
    public void SetActivePanNavitagion(bool _enabled)
    {
        if (_enabled) {
            hom3r.state.navigationMode = THom3rNavigationMode.pan;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PanNavigation_Enabled));
        } else {
            hom3r.state.navigationMode = THom3rNavigationMode.regular;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Navigation_PanNavigation_Disabled));
        }
    }

}

