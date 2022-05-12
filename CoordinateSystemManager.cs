using UnityEngine;

public class CEllipseData
{
    public float a;    // Current mayor axis of the ellipse
    public float b;    // Current minor axis of the ellipse
    public float c;    // Current minor axis of the ellipse
    public float Ec;    // Asymptote of the perpendiculars to the ellipse        
}

/// <summary>Semi-axes of an ellipse. A is the semi-major axis B the semi-minor axis    
class CSemiAxes
{
    public float a;
    public float b;

    public CSemiAxes() { }
    public CSemiAxes(float _a, float _b) { a = _a; b = _b; }
}

/// <summary>
/// To store axis options
/// </summary>
enum TAxis { x, y, z };

/// <summary>
/// Class that stores all the information of the virtual ellipsoid over which the camera moves. 
/// The desired evolutes cusps values are also stored for the x- and Z-axes.
/// </summary>
class CEllipsoidData
{
    public float radiousXAxis;
    public float radiousZAxis;
    public float radiousYAxis;

    float evoluteCusp_XAxis;            //Desired value of the evolute cusp for the X-axis
    float evoluteCusp_ZAxis;            //Desired value of the evolute cusp for the Z-axis

    /// <summary>Stores the values of the evolute cusps</summary>
    /// <param name="evoluteCusp_XAxis"></param>
    /// <param name="evoluteCusp_ZAxis"></param>
    public void SetEvoluteCups(float _evoluteCusp_XAxis, float _evoluteCusp_ZAxis)
    {
        evoluteCusp_XAxis = _evoluteCusp_XAxis;
        evoluteCusp_ZAxis = _evoluteCusp_ZAxis;
    }

    /// <summary>Returns the value of the cuspid of the evolution along the requested axis. Only works for the x-axis and z-axis.</summary>
    /// <param name="_axis">X or Z axis for which the evolute cusps is to be known.</param>
    /// <returns></returns>
    public float GetEvoluteCusp(TAxis _axis)
    {
        if (_axis == TAxis.x) { return evoluteCusp_XAxis; }
        else if (_axis == TAxis.z) { return evoluteCusp_ZAxis; }
        else return 0;
    }
}

/// <summary>
/// Store camera elliptical trajectory data
/// </summary>
class CTranslationEllipseData
{
    public float radiousZAxis;
    public float radiousXAxis;
    public float evoluteCusp;

    /// <summary>
    /// Returns both semi-axes of the ellipse
    /// </summary>
    /// <returns></returns>
    public CSemiAxes GetSemiAxes()
    {
        if (radiousXAxis > radiousZAxis) { return new CSemiAxes(radiousXAxis, radiousZAxis); }
        else { return new CSemiAxes(radiousZAxis, radiousXAxis); }
    }
    /// <summary>
    /// Returns in which axis is the semi-major axis of the ellipse
    /// </summary>
    /// <returns></returns>
    public TAxis GetSemiMajorAxis()
    {
        if (radiousXAxis > radiousZAxis) { return TAxis.x; }
        else { return TAxis.z; }
    }

    public TAxis GetSemiMinorAxis()
    {
        if (radiousXAxis < radiousZAxis) { return TAxis.x; }
        else { return TAxis.z; }
    }
}

/// <summary>
/// translationLimited : limited only in t € [0, PI]
/// </summary>
//public enum TCoordinateSystemConstraints { none, translationLimited }


public interface CCoordinateSystemManager
{
    /// <summary>Initialize the navigation system</summary>
    /// <param name="extents"> Vector3 with the extents of the bounding box to orbit around. X coordinate is assumed to be the object's intrinsic rotation axis. </param>
    /// <param name="cameraInitialPosition">Initial position proposed for the camera. There is no restrictions by this interface, but implementations of
    /// the Coordinate System Manager can expect some specific positions (e. g. along -Z axis)</param>
    /// <param name="fieldOfView">
    /// <param name="minimumCameraDistance">Out parameter that indicates the minimum distance allowed between the camera and the 3D object. It is expressed as 
    /// the distance between the object's intrinsic rotation axis and the point Zo, which is the closest allowed position in the Z axis </param>
    /// <param name="pointToLook">Out parameter that indicated the direction in which the camera has to look</param>
    /// <returns>Returns true if the initial position proposed for the camera is possible</returns>
    bool Init(Vector3 extents, Vector3 cameraInitialPosition, TNavigationSystemConstraints navigationConstraints, /*Vector2 fieldOfView,*/ out float minimumCameraDistance, out Vector3 pointToLook);

    /// <summary>Calculate the new camera position in terms of pseudoLatitude, pseudoLongitude and pseusoRadio</summary>
    /// <param name="latitudeVariation">Incremental translation movements inside of the camera plane. A value of 2·PI corresponds to a complete revolution.
    /// Notice that for spherical coordinates, this corresponds to actual latitude (elevation).</param>
    /// <param name="longitudeVariation">Incremental rotations movements of the camera plane. A value of 2·PI corresponds to a complete revolution.
    /// Notice that this corresponds to actual longitude "azimuth" for navigation systems with latitude trajectories independent from longitude.</param>
    /// <param name="radialVariation">Incremental movements of the camera approaching to the 3D object (zoom). It is expressed in virtual units in the Z axis 
    /// between previous and next orbits. </param>
    /// <param name="cameraPlanePosition">Out parameter that contains the new camera position within the plane (coordinates X and Y, where X corresponds
    /// to the object's intrinsic rotation axis). </param>
    /// <param name="planeRotation">Out parameter that contains the rotation that has to be applied to the plane. Expressed in radians.</param>
    /// <param name="pointToLook">Out parameter that indicated the direction in which the camera has to look</param>
    void CalculateCameraPosition(float latitudeVariation, float longitudeVariation, float radialVariation, Vector2 fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook);    
}


public class CSphericalCoordinatesManager : CCoordinateSystemManager
{
    // Parametric equations to implemented
    // x = r cos (t)         t€[0, 2PI]
    // z = r sin (t)         t€[0, 2PI]

    //Parameters that define the camera translation circumference
    float r;                    // Current circumference radius
    float t;                    // Current t parameter that defines camera position on the circumference
    //Plane rotation angle
    float planeAngle;           // Current rotation angle
    //Circumference limits
    float minimunRadious;       // Minimum possible radius    
    float initialRadious;
    //Control variable
    bool navigationInitialized = false;
    
    Vector3 extents;                                    // Store the product bounding box extents    
    TNavigationSystemConstraints navigationConstraints; 

    public bool Init(Vector3 _extents, Vector3 cameraInitialPosition, TNavigationSystemConstraints _navigationConstraints, out float minimumCameraDistance, out Vector3 pointToLook)
    {
        /////////////////////
        // Save parameters 
        /////////////////////
        extents = _extents;
        navigationConstraints = _navigationConstraints;
        

        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        /////////////////////////////////////////////////////////////// 
        r = cameraInitialPosition.magnitude;                // radius of the circumference passing through that point 
        t = Mathf.Asin(cameraInitialPosition.z / r);        // t = ASin(z/r)
        t = MathHom3r.NormalizeAngleInRad(t);               // Normalize t angle between 0-2PI

        initialRadious = r;
        ////////////////////////////////
        // Initialize plane angle to 0
        ////////////////////////////////
        planeAngle = 0.0f;

        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////
        // Calculating the radius of a circumference that encompasses the object. 
        
        //float r01 = MathHom3r.Pow2(_extents.x) + MathHom3r.Pow2(_extents.y) + MathHom3r.Pow2(_extents.z);
        //minimunRadious = Mathf.Sqrt(r01);
        Debug.Log(extents);
        float maxExtent = MathHom3r.Max(extents);
        float r01 = 3 * MathHom3r.Pow2(maxExtent);        
        minimunRadious = Mathf.Sqrt(r01);
        Debug.Log(minimunRadious);

        minimumCameraDistance = minimunRadious;         // update out parameter


        //////////////////////////////////////////////////
        // Calculate the point which camera has to look
        //////////////////////////////////////////////////        
        pointToLook = CalculatePointToLook();

        //////////////////////////////////////
        // HELPER
        //////////////////////////////////////
        this.DrawTranslationTrajectory(r);               // Draw the trajectory of the translation, for debug reasons
        DrawRotationTrajectory(cameraInitialPosition);
        DrawReferenceEllipses(r);


        /////////////////////////////////////////////////////////////////
        //Check if the proposed initial position for the camera is OK
        /////////////////////////////////////////////////////////////////
        if (r < minimunRadious)
        {
            Debug.Log("Error");
            return false;
        }
        else
        {
            navigationInitialized = true;            
            return true;
        }
        
    }

    public void CalculateCameraPosition(float latitudeVariation, float longitudeVariation, float radialVariation, Vector2 fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;
        if (navigationInitialized)
        {
            /////////////////////////////////////////////////////////
            // Apply pseudoLatitude and pseudoLongitude correction
            /////////////////////////////////////////////////////////
            //float tVariation = latitudeVariation * CalculateCorrectionParameter(fieldOfView.x);
            //float angleVariation = longitudeVariation * CalculateCorrectionParameter(fieldOfView.y);
            float tVariation = Calculate_t_Variation(latitudeVariation, fieldOfView.x);
            float angleVariation = CalculatePlaneAngleVariation(longitudeVariation, fieldOfView.y);

            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////                  
            r = CalculateNewR(radialVariation);

            // Latitude - which is a translation movement on the camera plane
            //t += tVariation;                    // Add to the current translation angle
            //t = MathHom3r.NormalizeAngleInRad(t);   // Normalize new t angle 
            float newt = t + tVariation;                    // Add to the current translation angle
            newt = MathHom3r.NormalizeAngleInRad(newt);    // Normalize new t angle 
            t = ApplyTranslationConstraints(newt);            

            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            if ((0 < t) && (t < Mathf.PI))
            {
                angleVariation = -angleVariation;   // Depends of the camera position, to avoid that the mouse behaviour change between front and back
            }
            planeAngle = angleVariation;

            
            //////////////////////////////////////
            // Calculate new camera position
            //////////////////////////////////////
            cameraPlanePosition.x = r * Mathf.Cos(t);
            cameraPlanePosition.y = 0;
            cameraPlanePosition.z = r * Mathf.Sin(t);

            //////////////////////////////////////
            // Calculate new plane rotation
            //////////////////////////////////////
            planeRotation = planeAngle;

            //////////////////////////////////////////////////
            // Calculate the point which camera hast to look
            //////////////////////////////////////////////////        
            pointToLook = CalculatePointToLook();

            //////////////////////////////////////
            // HELPER
            //////////////////////////////////////            
            this.DrawTranslationTrajectory(r);                       // Draw the trajectory of the translation, for debug reasons
            DrawRotationTrajectory(cameraPlanePosition);
            DrawReferenceEllipses(r);
        }
    }

    /// <summary>
    /// Calculate the new value of the circunference radious
    /// </summary>
    /// <param name="radialVariation"></param>
    /// <returns></returns>
    private float CalculateNewR(float radialVariation)
    {
        float new_r;
        if (radialVariation <= -10000)
        {
            //new_r = minimunRadious;
            float zoomPercentage = -0.01f * (radialVariation + 10000);
            float offset = (initialRadious - minimunRadious) * (1 - zoomPercentage);
            new_r = minimunRadious + offset;

        }
        else if (radialVariation >= 10000)
        {
            //new_r = initialRadious;
            float zoomPercentage = 0.01f * (radialVariation - 10000);
            float offset = (initialRadious - minimunRadious) * (1 - zoomPercentage);
            new_r = minimunRadious + offset;

        }
        else
        {
            new_r = r + radialVariation;                                           // Circumference Radius
            if (Mathf.Abs(new_r) < minimunRadious) { new_r = minimunRadious; }  // We can not closer that minimum
        }
        return new_r;
    }

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>point which camera has to look</returns>
    private Vector3 CalculatePointToLook()
    {
        return Vector3.zero;
    }


    private float Calculate_t_Variation(float latitudeVariation, float fieldOfView_rad)
    {
        TInteractionMappingCorrectionMode latitudeCorrection = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLatitudeInteractionCorrectionMode();

        float final_t_variation = latitudeVariation;

        if (latitudeCorrection == TInteractionMappingCorrectionMode.distance)
        {
            // Get Semiaxes of the inscribed ellipse inside the object            
            float inscribedCircunfereRadius = GetInscribedTranslationCircunferenceRadious();
            //Calculate angle variation in function of the ARC variation
            final_t_variation *= CalculateCircunferenceArcMappingFactor(inscribedCircunfereRadius);
            //Calculate angle variation applying the distance between camera and object correction  
            final_t_variation *= CalculateDistanceCorrectionFactor(inscribedCircunfereRadius, fieldOfView_rad);
        }
        else if (latitudeCorrection == TInteractionMappingCorrectionMode.ellipsePerimeter)
        {
            // Get Semiaxes of the camera rotation ellipse           
            float radius = r;
            //Calculate angle variation in function of the ARC variation
            final_t_variation *= CalculateCircunferenceArcMappingFactor(radius);
            // Calculate t-variation applying the ellipse perimeter correction
            final_t_variation *= (0.5f * CalculateCircunferencePerimeter(radius));
        }
        else if (latitudeCorrection == TInteractionMappingCorrectionMode.none)
        {
            // Get Semiaxes of the camera rotation ellipse           
            float radius = r;
            //Calculate angle variation in function of the ARC variation
            final_t_variation *= CalculateCircunferenceArcMappingFactor(radius);
        }

        return final_t_variation;
    }

    private float CalculatePlaneAngleVariation(float _longitudeVariation, float fieldOfView_rad)
    {
        TInteractionMappingCorrectionMode longitudeCorrection = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLongitudeInteractionCorrectionMode();

        float finalAngleVariation = _longitudeVariation;

        if (longitudeCorrection == TInteractionMappingCorrectionMode.distance)
        {
            // Get Semiaxes of the inscribed ellipse inside the object            
            float inscribedCircunfereRadius = GetInscribedLongitudeCircunferenceRadious();
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateCircunferenceArcMappingFactor(inscribedCircunfereRadius);
            //Calculate angle variation applying the distance between camera and object correction  
            finalAngleVariation *= CalculateDistanceCorrectionFactor(inscribedCircunfereRadius, fieldOfView_rad);
        }
        else if (longitudeCorrection == TInteractionMappingCorrectionMode.ellipsePerimeter)
        {
            // Get Semiaxes of the camera rotation ellipse           
            float radius = r;
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateCircunferenceArcMappingFactor(radius);
            // Calculate t-variation applying the ellipse perimeter correction
            finalAngleVariation *= (0.5f * CalculateCircunferencePerimeter(radius));
        }
        else if (longitudeCorrection == TInteractionMappingCorrectionMode.none)
        {
            // Get Semiaxes of the camera rotation ellipse           
            float radius = r;
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateCircunferenceArcMappingFactor(radius);
        }

        return finalAngleVariation;
    }


    private float CalculateCircunferencePerimeter(float r)
    {
        return 2 * Mathf.PI * r;
    }

    private float CalculateCircunferenceArcMappingFactor(float radious)
    {
        return (1 / radious);
    }

    private float GetInscribedTranslationCircunferenceRadious()
    {
        float rMin = MathHom3r.Max(extents.x, extents.z);
        return rMin;
    }
    private float GetInscribedLongitudeCircunferenceRadious()
    {
        float rMin = MathHom3r.Max(extents.y, extents.z);
        return rMin;
    }

    /// <summary>
    /// Calculate pseudo latitude and pseudo longitude correction.
    /// Based on the projection from the camera circumference to the circumference inscribed in the object.
    /// </summary>
    /// <param name="inscribedCircunferenRadious">radious of the inscribed circunference</param>
    /// <param name="fieldOfView_rad">field of view of the camera in radians</param>
    /// <returns></returns>
    private float CalculateDistanceCorrectionFactor(float inscribedCircunferenRadious, float fieldOfView_rad)
    {
        // Calculate minimum circumference
        float rMin = inscribedCircunferenRadious; //MathHom3r.Max(extents.y, extents.z);        
            
        float dPQ = r - rMin;
        if (dPQ < 1) { dPQ = 1; }       //TODO Delete this Chapuza -> Maybe the problem is that the minimum ellipse is to big
        float k = 2 * (dPQ) * Mathf.Tan(fieldOfView_rad);

        return k;
    }
    /// <summary>
    /// Calculate pseudo latitude and pseudo longitude correction.
    /// Based on the projection from the camera circumference to the circumference inscribed in the object.
    /// </summary>
    /// <param name="fieldOfView_rad">field of view of the camera in radians</param>
    /// <returns></returns>
    //private float CalculateCorrectionParameter(float fieldOfView_rad)
    //{        
    //    float rMin = MathHom3r.Max(extents);     
    //    float k = 2* (r - rMin) * Mathf.Tan(fieldOfView_rad);        
    //    return k * (1/rMin) ;        
    //}

    /// <summary>
    /// Check navigation constraints and applied to the translation t parameter
    /// </summary>
    /// <param name="desire_tValue"></param>
    /// <returns></returns>
    private float ApplyTranslationConstraints(float desire_tValue)
    {
        float new_tValue = t;

        if (navigationConstraints == TNavigationSystemConstraints.translationLimited)
        {
            if (!((0 < desire_tValue) && (desire_tValue < Mathf.PI))) { new_tValue = desire_tValue; }   //Translation are limited only in t € [0, PI]
        }
        else
        {
            new_tValue = desire_tValue;
        }
        return new_tValue;
    }

    //////////////////////////////////////
    // HELPER
    //////////////////////////////////////

    /// <summary>Draw the translation trajectory</summary>
    /// <param name="a">Axis mayor ellipse parameter</param>
    /// <param name="b">Axis minor ellipse parameter</param>
    private void DrawTranslationTrajectory(float radious)
    {
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(radious, radious);
    }

    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawRotationTrajectory(Vector3 _cameraPlanePosition)
    {
        float radio = _cameraPlanePosition.magnitude;
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(_cameraPlanePosition.z, _cameraPlanePosition.z, _cameraPlanePosition.x);
    }

    /// <summary>
    /// Draw a reference ellipses to help understand camera movements.
    /// </summary>
    private void DrawReferenceEllipses(float radious)
    {
        float xRadious = radious;
        float zRadious = radious;
        float yRadious = radious;
       
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(xRadious, zRadious);
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(xRadious, yRadious);
    }

}

public class CLimitedSphericalCoordinatesManager : CCoordinateSystemManager
{
    // Parametric equations to be implemented
    // x = r cos (t)         t€[0, 2PI]
    // z = r sin (t)         t€[0, 2PI]

    //Parameters that define the camera translation circumference
    float r;                    // Current circumference radius
    float t;                    // Current t parameter that define camera position on the circumference
    //Plane rotation angle
    float planeAngle;           // Current rotation angle    
    //Circumference limits
    float minimunRadious;       // Minimum possible radius
    //Control variables
    bool navigationInitialized = false;

    public bool Init(Vector3 extents, Vector3 cameraInitialPosition, TNavigationSystemConstraints _navigationConstraints, out float minimumCameraDistance, out Vector3 pointToLook)    
    {
        
        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        /////////////////////////////////////////////////////////////// 
        r = cameraInitialPosition.magnitude;                //radius of the circumference passing through that point 
        t = Mathf.Asin(cameraInitialPosition.z / r);        // t= ASin(z/r)
        t = MathHom3r.NormalizeAngleInRad(t);               // Normalize t angle between 0-2PI

        ////////////////////////////////
        // Initialize plane angle to 0
        ////////////////////////////////
        planeAngle = 0.0f;

        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////
        float r01 = MathHom3r.Pow2(extents.x) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z);
        minimunRadious = Mathf.Sqrt(r01);
        
        minimumCameraDistance = minimunRadious;         // update out parameter


        //////////////////////////////////////////////////
        // Calculate the point which camera has to look
        //////////////////////////////////////////////////        
        pointToLook = CalculatePointToLook();


        /////////////////////////////////////////////////////////////////
        //Check if the proposed initial position for the camera is OK
        /////////////////////////////////////////////////////////////////
        if (r < minimunRadious)
        {
            Debug.Log("Error");
            return false;
        }else
        {
            navigationInitialized = true;
            return true;
        }
    }
        
    public void CalculateCameraPosition(float pseudoLatitude, float pseudoLongitude, float pseudoRadio, Vector2 fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;

        if (navigationInitialized)
        {
            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////        
            r += pseudoRadio;                                           // Circumference Radius
            if (Mathf.Abs(r) < minimunRadious) { r = minimunRadious; }  // We can not go closer than that minimum

            // Latitude - which is a translation movement on the camera plane
            float newt = t + pseudoLatitude;                        // Add to the current translation angle
            newt = MathHom3r.NormalizeAngleInRad(newt);             // Normalize new t angle 
            if (!((0 < newt) && (newt < Mathf.PI))) { t = newt; }   //Translation are limited only in t € [PI, 2PI]

            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            planeAngle = pseudoLongitude;

            //////////////////////////////////////
            // Calculate new camera position
            //////////////////////////////////////
            cameraPlanePosition.x = r * Mathf.Cos(t);
            cameraPlanePosition.y = 0;
            cameraPlanePosition.z = r * Mathf.Sin(t);

            //////////////////////////////////////
            // Calculate new plane rotation
            //////////////////////////////////////
            planeRotation = planeAngle;

            //////////////////////////////////////////////////////
            // Calculate the point which camera is going to look
            //////////////////////////////////////////////////////        
            pointToLook = CalculatePointToLook();
        }        
    }

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>point which camera has to look</returns>
    private Vector3 CalculatePointToLook()
    {
        return Vector3.zero;
    }


}

public class CSpheroidCoordinatesManager : CCoordinateSystemManager
{

    // Parametric equations to implemented
    // x = a cos (t)         t€[0, 2PI]
    // z = b sin (t)         t€[0, 2PI]

    //Parameters that define the camera translation ellipse    
    CEllipsoidData ellipsoidData = new CEllipsoidData();                            // Current ellipsoid data
    CTranslationEllipseData translationEllipseNew = new CTranslationEllipseData();  // Current tranlation ellipse data

    //CEllipseData translationEllipse = new CEllipseData();

    
    float t_translationEllipse;                    // Current t parameter that define camera position on the ellipse
    //Plane rotation angle
    float planeAngle;                               // Current rotation angle    

    //Ellipse limits
    CEllipsoidData minimunEllipsoidData;
    float minimunAllowedAxis;                       // Minimum possible minor axis
    float initialAxis;

    //Object Geometry classification
    enum TGeometryType { Prolate, Oblate };
    TGeometryType geometryType;

    //Control variables
    bool navigationInitialized = false;


    Vector3 extents;            // Store the product bounding box extents
    //Vector2 fieldOfView;        // Store the camera field of View
    TNavigationSystemConstraints navigationConstraints;
    float k_lastvalidvalue;     // TODO Delete me


    public bool Init(Vector3 _extents, Vector3 cameraInitialPosition, TNavigationSystemConstraints _navigationConstraints, out float minimumCameraDistance, out Vector3 pointToLook)
    {
        /////////////////////
        // Save parameters 
        /////////////////////
        extents = _extents;
        navigationConstraints = _navigationConstraints;

        //////////////////////////////////////////////////
        // Identify object geometry type. Long or flat
        //////////////////////////////////////////////////   
        geometryType = ClassifyObjectGeometry(_extents);         // Get if the object is flat or long


        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        ///////////////////////////////////////////////////////////////      

        ellipsoidData = CalculateEllipsoid(_extents, cameraInitialPosition);    // Init ellipsoid
        translationEllipseNew = CalculateInitialTranslationEllipse_new();       // Calculate semi-axis of the ellipse a,b    
        t_translationEllipse = CalculateTranslationEllipseInitialT_new();       // Calculate the initial value of t parameter 
        ///////////////////////////////////
        // Initialize plane angle to 0
        ///////////////////////////////////
        planeAngle = 0.0f;

        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////        
        //minimunAllowedAxis = CalculateMinimunEllipse(_extents, translationEllipse);      // Calculate the minimum ellipse semi-axes
        minimunAllowedAxis = CalculateMinimunEllipse_new(_extents);
        minimumCameraDistance = minimunAllowedAxis;                 // update out parameter

        //////////////////////////////////////////////////
        // Calculate the point which camera has to look
        //////////////////////////////////////////////////        
        //pointToLook = CalculatePointToLook(t_translationEllipse, translationEllipse);
        pointToLook = CalculatePointToLook_new(t_translationEllipse, translationEllipseNew);

        //////////////////////////////////////
        // HELPER
        //////////////////////////////////////
        DrawTranslationTrajectory();                            // Draw the trajectory of the translation, for debug reasons   
        DrawRotationTrajectory(cameraInitialPosition);
        DrawReferenceEllipses(); 

        /////////////////////////////////////////////////////////////////
        //Check if the proposed initial position for the camera is OK
        /////////////////////////////////////////////////////////////////        
        if (CheckInitialMinimunDistance(cameraInitialPosition))
        {            
            navigationInitialized = true;
            initialAxis = Mathf.Abs(cameraInitialPosition.z);
            return true;
        }
        else
        {
            Debug.Log("Error");
            return false;
        }


    }

    public void CalculateCameraPosition(float latitudeVariation, float longitudeVariation, float radialVariation, Vector2 _fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;

        if (navigationInitialized)
        {
            // Update field of view
            //fieldOfView = _fieldOfView;

            /////////////////////////////////////////////////////////
            // Apply pseudoLatitude and pseudoLongitude correction
            /////////////////////////////////////////////////////////            
            //latitudeVariation = latitudeVariation * CalculatePseudoLatitudeMappingFactor(fieldOfView.x) * CalculatePseudoLatitudeCorrectionParameter(fieldOfView.x, translationEllipse);
            float tVariation = Calculate_t_Variation(latitudeVariation, _fieldOfView.x);
            float angleVariation = CalculatePlaneAngleVariation(longitudeVariation, _fieldOfView.y);
            //longitudeVariation = longitudeVariation * CalculatePseudoLongitudeCorrectionParameter(fieldOfView.y, translationEllipse);


            
            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////            
            //translationEllipse = CalculateNewEllipseSemiAxesParametersAfterRadialMovement(radialVariation, translationEllipse);
            CalculateNewEllipseSemiAxesParametersAfterRadialMovement_New(radialVariation);
            translationEllipseNew = CalculateInitialTranslationEllipse_new();
            // Latitude - which is a translation movement on the camera plane
            //t_translationEllipse += tVariation;                                             // Add to the current translation angle
            //t_translationEllipse = MathHom3r.NormalizeAngleInRad(t_translationEllipse);     // Normalize new t angle 

            float new_t_translationEllipse = t_translationEllipse + tVariation;                     // Calculate new translation angle
            new_t_translationEllipse = MathHom3r.NormalizeAngleInRad(new_t_translationEllipse);     // Normalize new t angle 
            t_translationEllipse = ApplyTranslationConstraints(new_t_translationEllipse);



            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            if ((0 < t_translationEllipse) && (t_translationEllipse < Mathf.PI))
            {
                angleVariation = -angleVariation;   // Depends of the camera position, to avoid that the mouse behaviour change between front and back
            }
            planeAngle = angleVariation;


            //////////////////////////////////////
            // Calculate new camera position
            //////////////////////////////////////            
            //cameraPlanePosition = CalculateNewCameraPosition(t_translationEllipse, translationEllipse);
            cameraPlanePosition = CalculateNewCameraPosition_New(t_translationEllipse);
             //////////////////////////////////////
             // Calculate new plane rotation
             //////////////////////////////////////
            planeRotation = planeAngle;

            //////////////////////////////////////////////////
            // Calculate the point which camera hast to look
            //////////////////////////////////////////////////        
            //pointToLook = CalculatePointToLook(t_translationEllipse, translationEllipse);
            pointToLook = CalculatePointToLook_new(t_translationEllipse, translationEllipseNew);
            //////////////////////////////////////
            // HELPER
            //////////////////////////////////////
            DrawTranslationTrajectory();                            // Draw the trajectory of the translation, for debug reasons   
            DrawRotationTrajectory(cameraPlanePosition);
            //DrawCameraPosition(cameraPlanePosition, pointToLook);

        }
    }



    /////////////////////////////////
    // INIT Auxiliary Methods
    /////////////////////////////////

    /// <summary>
    /// Classify the object geometry. It decides if the object is long or flat. 
    /// An object is long when its larger dimension coincides with the rotation axis.
    /// An object if flat when its larger dimension doesn't coincide with the rotation axis.
    /// </summary>
    /// <param name="boundingBox"></param>
    /// <returns></returns>
    private TGeometryType ClassifyObjectGeometry(Vector3 extents)
    {
        if ((extents.x >= extents.y) && (extents.x >= extents.z)) {
            Debug.Log("Prolate Object");
            return TGeometryType.Prolate;
        }
        else {
            Debug.Log("Oblate Object");
            return TGeometryType.Oblate;
        }
    }

    /// <summary>
    /// Calculate the reference ellipses, these are the ellipses on the xz and xy plane that frame the projections of the object on those planes
    /// </summary>
    /// <param name="extents"></param>
    /// <param name="cameraPosition"></param>   
    private CEllipsoidData CalculateEllipsoid(Vector3 extents, Vector3 cameraPosition)
    {
        CEllipsoidData _ellipsoidData = new CEllipsoidData();
        
        if (geometryType == TGeometryType.Prolate)
        {            
            _ellipsoidData.SetEvoluteCups(extents.x, extents.z);        // Evolute cusp will be in the limit of the object in X-axis
            _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            _ellipsoidData.radiousXAxis = CalculateMayorAxis(_ellipsoidData.radiousZAxis, extents.x);
            _ellipsoidData.radiousYAxis = Mathf.Abs(cameraPosition.z);
        }
        else if (geometryType == TGeometryType.Oblate)
        {
            // The semimayor axis is on the z axis, but its value depends on whether it is mayor ez or ey
            if (extents.z >= extents.y)
            {
                _ellipsoidData.SetEvoluteCups(extents.x, extents.z);
                _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);                
                _ellipsoidData.radiousXAxis = CalculateMinorAxis(_ellipsoidData.radiousZAxis, extents.z);
            }
            else {
                _ellipsoidData.SetEvoluteCups(extents.x, extents.y);
                _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);
                _ellipsoidData.radiousXAxis = CalculateMinorAxis(_ellipsoidData.radiousZAxis, extents.y);
            }
            _ellipsoidData.radiousYAxis = Mathf.Abs(cameraPosition.z);      // Because it is a rotation around X (Spheroid)
        } 

        return _ellipsoidData;
    }

    private float CalculateMayorAxis(float minorAxis, float Ec)
    {
        return 0.5f * (Ec + Mathf.Sqrt(MathHom3r.Pow2(Ec) + 4 * MathHom3r.Pow2(minorAxis)));
    }
    private float CalculateMinorAxis(float mayorAxis, float Ec)
    {
        return Mathf.Sqrt(MathHom3r.Pow2(mayorAxis) - (Ec * mayorAxis));

    }
    //private float CalculateEvoluteCusps(float a, float b)
    //{
    //    return a - (MathHom3r.Pow2(b) / a);
    //}

    /// <summary>
    /// Calculate the parameter c according to the geometry of the object and its dimensions.
    /// If the object is long, c is the extension of the object on the x-axis. 
    /// If the object is flat, c is the extension of the object on the z-axis.
    /// </summary>
    /// <param name="extents">Object Bounding Box extents</param>
    /// <returns></returns>
    private float CalculateEvolute(Vector3 extents)
    {
        if (geometryType == TGeometryType.Prolate) { return extents.x; }
        else {
            return extents.z;
            //return Mathf.Max(extents.z, extents.y) ;
        }
    }

    private CTranslationEllipseData CalculateInitialTranslationEllipse_new()
    {        
        CTranslationEllipseData _ellipse = new CTranslationEllipseData();
        _ellipse.radiousXAxis = ellipsoidData.radiousXAxis;
        _ellipse.radiousZAxis = ellipsoidData.radiousZAxis;
        _ellipse.evoluteCusp = ellipsoidData.GetEvoluteCusp(_ellipse.GetSemiMajorAxis());

        return _ellipse;        
    }

    /// <summary>
    /// Calculate the initial T parameter of the ellipse based on b/a and current camera position
    /// </summary>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>return the t parameter float value</returns>
    private float CalculateTranslationEllipseInitialT_new()
    {
        return (Mathf.PI * -0.5f);
    }


    /// <summary>
    /// Calculate the minimum ellipse that which surrounds the 3d object without cross it. 
    /// In the case of long object this value is the b minimum, 
    /// while in the case of flat objects this vale is the a minimum.
    /// </summary>
    /// <param name="extents">Bounding box extends of the 3D object</param>
    /// <returns>Return the minimum a of b of the allowed ellipse between the camera and the object</returns>
    //private float CalculateMinimunEllipse(Vector3 extents, CEllipseData ellipse)
    //{
    //    float minimunAxis = 0.0f;
        
    //    // Polynomial Coefficients: x^3 + a1 * x^2 + a2 * x + a3 = 0
    //    float a1 = -ellipse.Ec;
    //    float a2;
    //    if (geometryType == TGeometryType.Prolate) {
    //        a2 = -(MathHom3r.Pow2(ellipse.Ec) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z));
    //    }
    //    else
    //    {
    //        a2 = -(MathHom3r.Pow2(ellipse.Ec) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z));
    //    }
    //    float a3 = Mathf.Pow(ellipse.Ec, 3);        
        
    //    //Calculate Q, R
    //    float Q = (1 / 9.0f) * (3.0f * a2 - MathHom3r.Pow2(a1));
    //    float R = (1 / 54.0f) * (9.0f * a1 * a2 - 27 * a3 - 2 * Mathf.Pow(a1, 3));
    //    //Calculate Q^3 and R^2
    //    float Q3 = Mathf.Pow(Q, 3);
    //    //float R2 = Mathf.Pow(R, 2);
    //    //Calculate D
    //    //float D = Q3 + R2;
                       
    //    // D is always < 0 in our case
    //    //if (D < 0)
    //    //{            
    //        float teta = Mathf.Acos(-R / Mathf.Sqrt(-Q3));
    //        float x2 = -2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((teta + (2.0f * Constants.Pi)) / 3.0f) - (a1 / 3.0f);        
            
    //    //}

    //    if (geometryType == TGeometryType.Prolate)
    //    {
    //        float b = Mathf.Sqrt(MathHom3r.Pow2(x2) - x2 * ellipse.Ec);
    //        minimunAxis = b;        // Long object return the b minimum of the ellipse
    //    }
    //    else
    //    {
    //        minimunAxis = x2;       //Flat object return a minimum of the ellipse
    //    }
    //    //Debug.Log(minimunAxis);
    //    return minimunAxis;
    //}

    private float CalculateMinimunEllipse_new(Vector3 extents)
    {
        float minimunAxis = 0.0f;

        // Initial data
        float Ec = ellipsoidData.GetEvoluteCusp(translationEllipseNew.GetSemiMajorAxis());
        float r2 = MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z);

        // Polynomial Coefficients: x^3 + a1 * x^2 + a2 * x + a3 = 0
        float A = -Ec;
        float B = -(MathHom3r.Pow2(Ec) + r2);
        float C = Mathf.Pow(Ec, 3);

        ////Calculate P, Q
        //float P = (1 / 3.0f) * (3.0f * B - MathHom3r.Pow2(A));
        //float Q = (1 / 27.0f) * (9.0f * A * B + 27 * C + 2 * Mathf.Pow(A, 3));

        //// Calculate 
        //float P3 = Mathf.Pow((P/3), 3);

        ////Calculate roots
        //float teta = Mathf.Acos( (-0.5f * Q) / Mathf.Sqrt(-P3));
        //float temp1 = 4f * MathHom3r.Pow2(Ec) + 3f * r2;
        //float x2 = ((2f/3f) * Mathf.Sqrt(temp1) * Mathf.Cos((teta/ 3.0f))) + (Ec / 3.0f);

        //Calculate Q, R
        float Q = (1 / 9.0f) * (3.0f * B - MathHom3r.Pow2(A));
        float R = (1 / 54.0f) * (9.0f * A * B - 27 * C - 2 * Mathf.Pow(A, 3));
        //Calculate Q^3 and R^2
        float Q3 = Mathf.Pow(Q, 3);
        // Calculate roots
        float teta = Mathf.Acos(-R / Mathf.Sqrt(-Q3));
        float x2 = -2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((teta + (2.0f * Constants.Pi)) / 3.0f) - (A / 3.0f);


        //Calculate minimum ellipsoid
        minimunEllipsoidData = new CEllipsoidData();
        if (geometryType == TGeometryType.Prolate)
        {
            // We have calculated the semi-mayor axis
            minimunEllipsoidData.radiousXAxis = x2;
            minimunEllipsoidData.radiousZAxis = CalculateMinorAxis(x2, Ec);
            minimunEllipsoidData.radiousYAxis = minimunEllipsoidData.radiousZAxis;            
        }
        else
        {
            // We have calculated the semi-minor axis
            minimunEllipsoidData.radiousZAxis = x2;
            minimunEllipsoidData.radiousXAxis = CalculateMinorAxis(x2, Ec);
            minimunEllipsoidData.radiousYAxis = minimunEllipsoidData.radiousZAxis;
        }
        minimunAxis = minimunEllipsoidData.radiousZAxis;

        return minimunAxis;
    }

    /// <summary>
    /// Check if the initial camera position proposed is correct or not
    /// </summary>
    /// <param name="cameraPosition">Camera position</param>
    /// <returns>true is the proposed camera position is OK</returns>
    private bool CheckInitialMinimunDistance(Vector3 cameraPosition)
    {        
        //TODO What Do happen if the proposed camera position is not in Z axis?
        if (Mathf.Abs(cameraPosition.z) < minimunAllowedAxis) return false;    
        return true;
    }


    //////////////////////////////////////////////////
    //////////////////////////////////////////////////
    //                                              //
    // CALCULATECameraPosition Auxiliary Methods    //
    //                                              //
    //////////////////////////////////////////////////    
    //////////////////////////////////////////////////

    /// <summary>
    /// Calculate the b and a parameters of the ellipse based of a pseudoRadio movement.    
    /// </summary>
    /// <param name="radialVariation"></param>
    //private CEllipseData CalculateNewEllipseSemiAxesParametersAfterRadialMovement(float radialVariation, CEllipseData currentEllipse)
    //{
    //    CEllipseData newEllipse = new CEllipseData();
    //    newEllipse.Ec = currentEllipse.Ec;

    //    if (geometryType == TGeometryType.Prolate)
    //    {
    //        //newEllipse.b = currentEllipse.b + radialVariation;
    //        //if (Mathf.Abs(newEllipse.b) < minimunAllowedAxis) { newEllipse.b = minimunAllowedAxis; }  // We can not closer that minimum
    //        newEllipse.b = CalculateNewEllipseRadious(currentEllipse.b, radialVariation);
    //        newEllipse.a = 0.5f * (newEllipse.Ec + Mathf.Sqrt(MathHom3r.Pow2(newEllipse.Ec) + 4 * MathHom3r.Pow2(newEllipse.b)));
    //    }
    //    else
    //    {
    //        //newEllipse.a = currentEllipse.a + radialVariation;
    //        //if (Mathf.Abs(newEllipse.a) < minimunAllowedAxis) { newEllipse.a = minimunAllowedAxis; }  // We can not closer that minimum
    //        newEllipse.a = CalculateNewEllipseRadious(currentEllipse.a, radialVariation);
    //        newEllipse.b = Mathf.Sqrt(MathHom3r.Pow2(newEllipse.a) - newEllipse.a * newEllipse.Ec);
    //    }

    //    DrawReferenceEllipses(newEllipse);

    //    return newEllipse;
    //}

    private void CalculateNewEllipseSemiAxesParametersAfterRadialMovement_New(float radialVariation)
    {
        if (radialVariation != 0f)
        {
            float new_z = CalculateNewEllipseRadious(ellipsoidData.radiousZAxis, radialVariation);
            CEllipsoidData ellipsoidCandidate = CalculateEllipsoid(extents, new Vector3(0, 0, new_z));

            // TO Think Do I need to check if the full ellipsoid is valid instead only the radious in Z?
            ellipsoidData = ellipsoidCandidate;

            DrawReferenceEllipses();
        }        
    }


    private float CalculateNewEllipseRadious(float currentEllipseRadious, float radialVariation)
    {
        float new_r;
        if (radialVariation <= -10000)
        { 
            float zoomPercentage = -0.01f * (radialVariation + 10000);
            float offset = (initialAxis - minimunAllowedAxis) * (1-zoomPercentage);
            new_r = minimunAllowedAxis + offset;
            //new_r = minimunAllowedAxis * 1.75f;
        }
        else if (radialVariation >= 10000)
        {
            float zoomPercentage = 0.01f * (radialVariation - 10000);
            float offset = (initialAxis - minimunAllowedAxis) * (1 - zoomPercentage);
            new_r = minimunAllowedAxis + offset;
            //new_r = initialAxis;
        }
        else
        {
            new_r = currentEllipseRadious + radialVariation;                                           // Circumference Radius
            if (Mathf.Abs(new_r) < minimunAllowedAxis) { new_r = minimunAllowedAxis; }  // We can not closer that minimum
        }
        return new_r;
    }

    /// <summary>
    /// Calculate the new camera position in function of a,b and t of the ellipse
    /// </summary>
    /// <returns>Returns the new camera position</returns>
    //private Vector3 CalculateNewCameraPosition(float t, CEllipseData currentEllipse)
    //{
    //    Vector3 cameraPositionOnPlane;
    //    if (geometryType == TGeometryType.Prolate)
    //    {
    //        cameraPositionOnPlane.x = currentEllipse.a * Mathf.Cos(t);
    //        cameraPositionOnPlane.y = 0;
    //        cameraPositionOnPlane.z = currentEllipse.b * Mathf.Sin(t);
    //    }
    //    else
    //    {
    //        cameraPositionOnPlane.x = currentEllipse.b * Mathf.Cos(t);
    //        cameraPositionOnPlane.y = 0;
    //        cameraPositionOnPlane.z = currentEllipse.a * Mathf.Sin(t);
    //    }       
    //    return cameraPositionOnPlane;
    //}

    private Vector3 CalculateNewCameraPosition_New(float t)
    {
        Vector3 cameraPositionOnPlane = Vector3.zero;
        cameraPositionOnPlane.x =  translationEllipseNew.radiousXAxis * Mathf.Cos(t);
        cameraPositionOnPlane.y = 0;
        cameraPositionOnPlane.z = translationEllipseNew.radiousZAxis * Mathf.Sin(t);
        
        return cameraPositionOnPlane;
    }

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>Return the point which camera has to look</returns>
    //private Vector3 CalculatePointToLook_old(float t, CEllipseData currentEllipse)
    //{
    //    float intersectionXaxis;
    //    if (geometryType == TGeometryType.Prolate)
    //    {
    //        intersectionXaxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a)) * Mathf.Cos(t);
    //    }
    //    else
    //    {
    //        intersectionXaxis = (currentEllipse.b - (MathHom3r.Pow2(currentEllipse.a) / currentEllipse.b)) * Mathf.Cos(t);
    //    }

    //    return new Vector3(intersectionXaxis, 0, 0);
    //}

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>Return the point which camera has to look</returns>
    //private Vector3 CalculatePointToLook(float t, CEllipseData currentEllipse)
    //{
    //    Vector3 intersectionPoint = new Vector3();    
    //    if (geometryType == TGeometryType.Prolate)
    //    {
            
    //        float intersectionXaxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a)) * Mathf.Cos(t);
    //        intersectionPoint = new Vector3(intersectionXaxis, 0, 0);
    //    }
    //    else
    //    {            
    //        if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection())
    //        {
    //            float evoluteZAxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a));
    //            float intersectionZaxis = evoluteZAxis * Mathf.Sin(t);
    //            float evoluteDesired = Mathf.Sqrt(MathHom3r.Pow2(extents.z * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(extents.x * Mathf.Cos(planeAngle)));
    //            intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, evoluteZAxis, evoluteDesired);
    //            intersectionPoint = new Vector3(0, 0, intersectionZaxis);
    //        }
    //        else
    //        {
    //            float intersectionZaxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a)) * Mathf.Sin(t);
    //            intersectionPoint = new Vector3(0, 0, intersectionZaxis);
    //        }
    //    }

    //    //Debug.Log("Plane point to look : " + intersectionPoint);
    //    return intersectionPoint;
    //}

    private Vector3 CalculatePointToLook_new(float t, CTranslationEllipseData currentEllipse)
    {
        Vector3 intersectionPoint = new Vector3();       

        // Get semiaxes
        CSemiAxes _translationEllipseSemiAxes = currentEllipse.GetSemiAxes();
        float a = _translationEllipseSemiAxes.a;
        float b = _translationEllipseSemiAxes.b;
        
        //Calculate point to look
        if (geometryType == TGeometryType.Prolate)
        {

            //float evoluteCusp = (a - (MathHom3r.Pow2(b) / a));            
            float intersectionXaxis = currentEllipse.evoluteCusp * Mathf.Cos(t);
            intersectionPoint = new Vector3(intersectionXaxis, 0, 0);
        }
        else
        {
            float intersectionZaxis = 0.0f; ;
            if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.none)
            {
                //float evoluteCusp = (a - (MathHom3r.Pow2(b) / a));
                intersectionZaxis = currentEllipse.evoluteCusp * Mathf.Sin(t);
                //intersectionPoint = new Vector3(0, 0, intersectionZaxis);
            }
            else if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.minimun)
            {
                float evoluteCusp = ellipsoidData.GetEvoluteCusp(currentEllipse.GetSemiMinorAxis());
                float temp = currentEllipse.evoluteCusp;
                intersectionZaxis = evoluteCusp * Mathf.Sin(t);
            }
            else if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.interpolation)
            {
                float evoluteZAxis = (a - (MathHom3r.Pow2(b) / a));
                intersectionZaxis = evoluteZAxis * Mathf.Sin(t);
                float evoluteDesired = Mathf.Sqrt(MathHom3r.Pow2(extents.z * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(extents.x * Mathf.Cos(planeAngle)));
                intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, evoluteZAxis, evoluteDesired);
                intersectionPoint = new Vector3(0, 0, intersectionZaxis);
            }
            intersectionPoint = new Vector3(0, 0, intersectionZaxis);           
        }

        //Debug.Log("Plane point to look : " + intersectionPoint);
        return intersectionPoint;
    }
    /// <summary>
    /// Calculates linear interpolation to keep the direction the camera is facing always inside the object.
    /// </summary>
    /// <param name="intersectionXaxis"></param>
    /// <param name="_cameraEllipseEc"></param>
    /// <param name="_desiredEvoluteCusps"></param>
    /// <returns></returns>
    private float CalculateLinearInterpolation(float intersectionXaxis, float _cameraEllipseEc, float _desiredEvoluteCusps)
    {

        return ((intersectionXaxis / _cameraEllipseEc) * _desiredEvoluteCusps);
    }

    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawTranslationTrajectory(/*CEllipseData currentEllipse*/)
    {
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(translationEllipseNew.radiousXAxis, translationEllipseNew.radiousZAxis);

        //if (geometryType == TGeometryType.Prolate)
        //{
        //    hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(currentEllipse.a, currentEllipse.b);                
        //}
        //else
        //{
        //    hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(currentEllipse.b, currentEllipse.a);
        //}
    }

    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawRotationTrajectory(Vector3 _cameraPlanePosition)
    {               
        float radio = _cameraPlanePosition.magnitude;
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(_cameraPlanePosition.z, _cameraPlanePosition.z, _cameraPlanePosition.x);                    
    }

    /// <summary>
    /// Draw a reference ellipses to help understand camera movements.
    /// </summary>
    private void DrawReferenceEllipses(/*CEllipseData currentEllipse*/)
    {
        //float xRadious;
        //float zRadious;
        //float yRadious;

        //if (geometryType == TGeometryType.Prolate)
        //{
        //    zRadious = currentEllipse.b;
        //    xRadious = currentEllipse.a;

        //}
        //else
        //{
        //    xRadious = currentEllipse.b;
        //    zRadious = currentEllipse.a;
        //}

        //yRadious = zRadious;

        //hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(xRadious, zRadious);
        //hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(xRadious, yRadious);        
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(ellipsoidData.radiousXAxis, ellipsoidData.radiousZAxis);
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(ellipsoidData.radiousXAxis, ellipsoidData.radiousYAxis);
    }

    private float Calculate_t_Variation(float _latitudeVariation, float fieldOfViewX)
    {

        TInteractionMappingCorrectionMode latitudeCorrection = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLatitudeInteractionCorrectionMode();

        float final_t_Variation = _latitudeVariation;

        if (latitudeCorrection == TInteractionMappingCorrectionMode.distance)
        {
            // Get Semiaxes of the inscribed ellipse inside the object
            CSemiAxes _incribedEllipseSemiAxes = GetInscribedTranslationEllipseSemiAxes();
            //Calculate t-variation in function of the ARC variation
            final_t_Variation *= CalculateLatitudeArcMappingFactor(_incribedEllipseSemiAxes.a, _incribedEllipseSemiAxes.b);


            //Calculate t-variation applying the distance between camera and object correction  
            final_t_Variation *= CalculateLatitudeDistanceCorrectionFactor(t_translationEllipse, fieldOfViewX);
        }
        else if (latitudeCorrection == TInteractionMappingCorrectionMode.ellipsePerimeter)
        {
            // Get Semiaxes of the camera translation ellipse
            //CSemiAxes _cameraEllipseSemiAxes = GetTranslationEllipseSemiAxes();
            CSemiAxes _cameraEllipseSemiAxes = translationEllipseNew.GetSemiAxes();
            //Calculate t-variation in function of the ARC variation
            final_t_Variation *= CalculateLatitudeArcMappingFactor(_cameraEllipseSemiAxes.a, _cameraEllipseSemiAxes.b);
            // Calculate t-variation applying the ellipse perimeter correction
            final_t_Variation *= (0.5f * CalculateEllipsePerimeter(_cameraEllipseSemiAxes.a, _cameraEllipseSemiAxes.b));
        } else if (latitudeCorrection == TInteractionMappingCorrectionMode.none)
        {
            //Do nothing
        }
        return final_t_Variation;
    }
       

    /// <summary>
    /// Calculate the ellipse perimeter using the Ranaujan II-Cantrell.
    /// ref: https://www.universoformulas.com/matematicas/geometria/perimetro-elipse/
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float CalculateEllipsePerimeter(float a, float b)
    {
        float ellipsePerimeter;

        float H = MathHom3r.Pow2((a - b) / (a + b));
        float _3H = 3 * H;
        float HTwelve = Mathf.Pow(H, 12);

        float secondSummand = (3 * H) / (10 + Mathf.Sqrt(4 - _3H));
        float thirdSummand = ((4 / Mathf.PI) - (14 / 11)) * HTwelve;

        ellipsePerimeter = Mathf.PI * (a + b) * (1 + secondSummand + thirdSummand);

        return ellipsePerimeter;
    }

    private CSemiAxes GetInscribedTranslationEllipseSemiAxes()
    {
        CSemiAxes _inscribedEllipseSemiAxes = new CSemiAxes();
        // Calculate minimum ellipse
        if (geometryType == TGeometryType.Prolate)
        {
            _inscribedEllipseSemiAxes.a = extents.x;
            _inscribedEllipseSemiAxes.b = extents.z;
        }
        else
        {
            _inscribedEllipseSemiAxes.a = extents.z;
            _inscribedEllipseSemiAxes.b = extents.x;
        }

        return _inscribedEllipseSemiAxes;
    }

    //private CSemiAxes GetTranslationEllipseSemiAxes()
    //{
    //    CSemiAxes _translationEllipseSemiAxes = new CSemiAxes();

    //    _translationEllipseSemiAxes.a = translationEllipse.a;
    //    _translationEllipseSemiAxes.b = translationEllipse.b;

    //    return _translationEllipseSemiAxes;
    //}

    private float CalculateLatitudeArcMappingFactor(float a, float b)
    {      
        //float aMin;
        //float bMin;
        //// Calculate minimum ellipse
        //if (geometryType == TGeometryType.Prolate)
        //{
        //    aMin = extents.x;
        //    bMin = extents.z;
        //}
        //else
        //{
        //    aMin = extents.z;
        //    bMin = extents.x;
        //}
      

        float arco = Mathf.Sqrt(MathHom3r.Pow2(a * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(b * Mathf.Cos(t_translationEllipse)));

        float factor = 1 / arco;
        
        return factor;
    }

 
    /// <summary>
    /// Calculate pseudo latitude correction.
    /// Based on the projection from the camera ellipse to the ellipse inscribed in the object.
    /// </summary>
    /// <param name="fieldOfView_rad">field of view of the camera in rads</param>
    /// <returns></returns>    
    private float CalculateLatitudeDistanceCorrectionFactor(float t, float horizontalFieldOfViewAngle_rad)
    {
        // Get Semiaxes of the inscribed ellipse inside the object
        CSemiAxes _incribedEllipseSemiAxes = GetInscribedTranslationEllipseSemiAxes();

        float aMin = _incribedEllipseSemiAxes.a;
        float bMin = _incribedEllipseSemiAxes.b;


        // CalculateMappingCorrectionParameterK
        float k = 1f;

        float aMin2 = MathHom3r.Pow2(aMin);
        float bMin2 = MathHom3r.Pow2(bMin);

        //Calculate normal to camera ellipse
        CSemiAxes _translationEllipseSemiAxes = translationEllipseNew.GetSemiAxes();
        float a2 = MathHom3r.Pow2(_translationEllipseSemiAxes.a);
        float b2 = MathHom3r.Pow2(_translationEllipseSemiAxes.b);

        
        float M = (_translationEllipseSemiAxes.a / _translationEllipseSemiAxes.b) * Mathf.Tan(t);
        float D = ((a2 / _translationEllipseSemiAxes.b) - _translationEllipseSemiAxes.b) * Mathf.Sin(t);
        float M2 = MathHom3r.Pow2(M);
        float D2 = MathHom3r.Pow2(D);

        //Calculate P        
        //Vector3 P = CalculateNewCameraPosition(t_translationEllipse, _translationEllipseSemiAxes);
        Vector3 P = CalculateNewCameraPosition_New(t);
        
        //Calculate Q
        float raiz = Mathf.Sqrt(bMin2 - D2 + aMin2 * M2);
        float secondTerm = bMin * aMin * raiz;
        float firstTerm = aMin2 * M * D;
        float denom = bMin2 + aMin2 * M2;       
        float xQ = (firstTerm + secondTerm) / denom;

        float zQ = 0f;
        float radicand = 1 - MathHom3r.Pow2(xQ / aMin);
        //Debug.Log(radicand);
        if (radicand > 0f) {  zQ = bMin * Mathf.Sqrt(radicand); }     // Sometimes it is <0 --> I don't know why
        //float zQ = bMin * Mathf.Sqrt(1 - MathHom3r.Pow2(xQ / aMin));

        if (P.z < 0) { zQ *= -1; }  //TODO Resolve this "chapuza"
        Vector3 Q = new Vector3(xQ, 0, zQ);                
        //Debug.Log("Q: " + Q);

        float dPQ = Mathf.Sqrt((P - Q).sqrMagnitude);
        //Debug.Log(dPQ);
               
        //float rMin = MathHom3r.Max(extents);
        k = 2 * dPQ * Mathf.Tan(horizontalFieldOfViewAngle_rad);

        if (float.IsNaN(k))
        {        
            k = k_lastvalidvalue;
        }
        else
        {
            k_lastvalidvalue = k;
        }

        //Debug.Log(k);
        //float arco = Mathf.Sqrt(MathHom3r.Pow2(aMin * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bMin * Mathf.Cos(t_translationEllipse)));

        //Debug.Log("CalculatePseudoLatitudeCorrectionParameter " + k);
        return k;///arco;
    }



    private float CalculatePlaneAngleVariation(float _longitudeVariation, float fieldOfViewY)
    {
        float finalAngleVariation = _longitudeVariation;
        TInteractionMappingCorrectionMode longitudeCorrection = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLongitudeInteractionCorrectionMode();

        if (longitudeCorrection == TInteractionMappingCorrectionMode.distance)
        {

            // Get Semiaxes of the inscribed ellipse inside the object            
            float radius = GetInscribedCircunferenceRadius();
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateLongitudeArcMappingFactor(radius);

            //Calculate angle variation applying the distance between camera and object correction  
            finalAngleVariation *= CalculateLongitudeDistanceCorrectionFactor(fieldOfViewY);
        }
        else if (longitudeCorrection == TInteractionMappingCorrectionMode.ellipsePerimeter)
        {
            // Get Semiaxes of the camera rotation ellipse           
            float radius = GetRotationCameraCircunferenceRadius(); ;
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateLongitudeArcMappingFactor(radius);
            // Calculate t-variation applying the ellipse perimeter correction
            finalAngleVariation *= (0.5f * CalculateCircunferencePerimeter(radius));

            //finalAngleVariation *= Mathf.PI;
        } else if (longitudeCorrection == TInteractionMappingCorrectionMode.none)
        {
            // Get Semiaxes of the camera rotation ellipse           
            float radius = GetRotationCameraCircunferenceRadius(); ;
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateLongitudeArcMappingFactor(radius);
        }

        return finalAngleVariation;
    }

    private float GetInscribedCircunferenceRadius()
    {
        float rMin = MathHom3r.Max(extents.y, extents.z);
        return rMin;
    }

    private float GetRotationCameraCircunferenceRadius()
    {
        //float r;        // Long 
        //if (geometryType == TGeometryType.Prolate) { r = translationEllipse.b; }
        //else { r = translationEllipse.a; }

        //return r;

        return ellipsoidData.radiousYAxis;
    }

    private float CalculateCircunferencePerimeter(float r)
    {
        return 2 * Mathf.PI * r;
    }

    private float CalculateLongitudeArcMappingFactor(float radious)
    {        
        return (1 / radious);
    }

    /// <summary>
    /// Calculate pseudo longitude correction.
    /// Based on the projection from the camera circumference to the circumference inscribed in the object.
    /// </summary>
    /// <param name="fieldOfView_rad">field of view of the camera in radians</param>
    /// <returns></returns>
    private float CalculateLongitudeDistanceCorrectionFactor(float fieldOfView_rad)
    {
        // Calculate minimum circumference
        float rMin = GetInscribedCircunferenceRadius(); //MathHom3r.Max(extents.y, extents.z);        
        
        // Calculate current circumference
        float r = GetRotationCameraCircunferenceRadius();        // Long 
        //if (geometryType == TGeometryType.Prolate) { r = currentEllipse.b; }
        //else { r = currentEllipse.a; }

        float dPQ = r - rMin;
        if (dPQ < 1) { dPQ = 1; }       //TODO Delete this Chapuza -> Maybe the problem is that the minimum ellipse is to big
        //Debug.Log("dPQ: " + dPQ);
        float k = 2 * (dPQ) * Mathf.Tan(fieldOfView_rad);

        //Debug.Log("k: " + k * (1 / rMin));
        //return 1f;
        return k;
    }

    /// <summary>
    /// Check navigation constraints and applied to the translation t parameter
    /// </summary>
    /// <param name="desired_tValue"></param>
    /// <returns></returns>
    private float ApplyTranslationConstraints(float desired_tValue)
    {
        float new_tValue = t_translationEllipse;

        if (navigationConstraints == TNavigationSystemConstraints.translationLimited)
        {
            if (!((0 < desired_tValue) && (desired_tValue < Mathf.PI))) { new_tValue = desired_tValue; }   //Translation are limited only in t € [0, PI]
        }
        else
        {
            new_tValue = desired_tValue;
        }
        return new_tValue;
    }
}

public class CEllipsoidCoordinatesManager : CCoordinateSystemManager
{
    ///// <summary>
    ///// To store axis options
    ///// </summary>
    //enum TAxis { x, y, z };

    ///// <summary>
    ///// Class that stores all the information of the virtual ellipsoid over which the camera moves. 
    ///// The desired evolutes cusps values are also stored for the x- and Z-axes.
    ///// </summary>
    //class CEllipsoidData
    //{
    //    public float radiousXAxis;
    //    public float radiousZAxis;
    //    public float radiousYAxis;
        
    //    float evoluteCusp_XAxis;            //Desired value of the evolute cusp for the X-axis
    //    float evoluteCusp_ZAxis;            //Desired value of the evolute cusp for the Z-axis

    //    /// <summary>Stores the values of the evolute cusps</summary>
    //    /// <param name="evoluteCusp_XAxis"></param>
    //    /// <param name="evoluteCusp_ZAxis"></param>
    //    public void SetEvoluteCups(float _evoluteCusp_XAxis, float _evoluteCusp_ZAxis)
    //    {
    //        evoluteCusp_XAxis = _evoluteCusp_XAxis;
    //        evoluteCusp_ZAxis = _evoluteCusp_ZAxis;
    //    }

    //    /// <summary>Returns the value of the cuspid of the evolution along the requested axis. Only works for the x-axis and z-axis.</summary>
    //    /// <param name="_axis">X or Z axis for which the evolute cusps is to be known.</param>
    //    /// <returns></returns>
    //    public float GetEvoluteCusp(TAxis _axis)
    //    {
    //        if (_axis == TAxis.x) { return evoluteCusp_XAxis; }
    //        else if (_axis == TAxis.z) { return evoluteCusp_ZAxis; }
    //        else return 0;
    //    }       
    //}



    /// <summary>
    /// Store data of rotation ellipse
    /// </summary>
    class CRotationEllipseData
    {
        public float radiousZAxis;
        public float radiousYAxis;

        /// <summary>Return ellipse semiaxes values</summary>
        /// <returns>Ellipse semiaxes values</returns>
        public CSemiAxes GetSemiAxes()
        {            
            if (radiousZAxis > radiousYAxis)    { return new CSemiAxes(radiousZAxis, radiousYAxis); }
            else                                { return new CSemiAxes(radiousYAxis, radiousZAxis); }            
        }
    }

    /// <summary>
    /// Store camera elliptical trajectory data
    /// </summary>
    //class CTranslationEllipseData
    //{
    //    public float radiousZAxis;
    //    public float radiousXAxis;
    //    public float evoluteCusp;        

    //    /// <summary>
    //    /// Returns both semi-axes of the ellipse
    //    /// </summary>
    //    /// <returns></returns>
    //    public CSemiAxes GetSemiAxes()
    //    {            
    //        if (radiousXAxis > radiousZAxis)    { return new CSemiAxes(radiousXAxis, radiousZAxis); }
    //        else                                { return  new CSemiAxes(radiousZAxis, radiousXAxis);}            
    //    }
    //    /// <summary>
    //    /// Returns in which axis is the semi-major axis of the ellipse
    //    /// </summary>
    //    /// <returns></returns>
    //    public TAxis GetSemiMajorAxis()
    //    {
    //        if (radiousXAxis > radiousZAxis) { return TAxis.x; }
    //        else {                             return TAxis.z; }
    //    }
    //}
    
    class CMovementEllipses {        
        public CTranslationEllipseData translation;
        public CRotationEllipseData rotation;

        public CMovementEllipses()
        {
            translation = new CTranslationEllipseData();
            rotation = new CRotationEllipseData();
        }
    }

    //Ellipsoid data
    CEllipsoidData ellipsoidData = new CEllipsoidData();             // Current ellipsoid data
    CMovementEllipses movementEllipses = new CMovementEllipses();       // Parameters that define the camera translation and rotation ellipses            
    float t_translationEllipse;                                         // Current t parameter that define camera position on the ellipse    
    float planeAngle;                                                   // Current plane rotation angle    

    //Ellipse limits
    CEllipsoidData minimunEllipsoidData;
    float minimunAllowedAxisSize;       // Minimum possible minor axis
    float initialAxisSize;

    enum TGeometryType3
    {
        Type_I,
        Type_II,
        Type_III,
        Type_IV,
        Type_V,
        Type_VI,
    }
    TGeometryType3 geometryType;
    
    //Control variables
    bool navigationInitialized = false;


    Vector3 extents;                                        // Store the product bounding box extents
    TNavigationSystemConstraints navigationConstraints;     // Store configuration

    float k_lastvalidvalue;     // TODO Delete me

    public bool Init(Vector3 _extents, Vector3 cameraInitialPosition, TNavigationSystemConstraints _navigationConstraints, out float minimumCameraDistance, out Vector3 pointToLook)
    {
        ///////////////////////////////////
        // Initialize plane angle to 0
        ///////////////////////////////////
        planeAngle = 0.0f;

        /////////////////////
        // Save parameters 
        /////////////////////
        extents = _extents;
        navigationConstraints = _navigationConstraints;

        //////////////////////////////////////////////////
        // Identify object geometry type. Long or flat
        //////////////////////////////////////////////////           
        geometryType = ClassifyObjectGeometry3(_extents);

        //Init ellipsoid
        ellipsoidData = CalculateEllipsoid(_extents, cameraInitialPosition);

        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        ///////////////////////////////////////////////////////////////                      
        movementEllipses.translation = CalculateInitialTranslationEllipseParameters_new(cameraInitialPosition);
        t_translationEllipse = CalculateTranslationEllipseInitialTParameter();    // Calculate the initial value of t parameter                   
        movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);
        
        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////        
        float aproximationToMinimunAllowedZAxisSize = CalculateMinimunEllipsoid(_extents);       // Calculate the minimum ellipse semi-axes
        minimunAllowedAxisSize = CalculateRealMinimumZRadious(aproximationToMinimunAllowedZAxisSize);
        minimunAllowedAxisSize = aproximationToMinimunAllowedZAxisSize;     //TODO remove
        minimumCameraDistance = minimunAllowedAxisSize;                                                 // update out parameter

        //////////////////////////////////////////////////
        // Calculate the point which camera has to look
        //////////////////////////////////////////////////                
        pointToLook = CalculatePointToLook(t_translationEllipse, movementEllipses.translation);

        //////////////////////////////////////
        // HELPER
        //////////////////////////////////////        
        DrawTranslationTrajectory();        
        DrawRotationTrajectory(cameraInitialPosition);        
        DrawReferenceEllipses();

        /////////////////////////////////////////////////////////////////
        //Check if the proposed initial position for the camera is OK
        /////////////////////////////////////////////////////////////////        
        if (CheckInitialMinimunDistance(cameraInitialPosition))
        {
            navigationInitialized = true;
            initialAxisSize = Mathf.Abs(cameraInitialPosition.z);
            return true;
        }
        else
        {
            Debug.Log("Error");
            return false;
        }
    }


    public void CalculateCameraPosition(float latitudeVariation, float longitudeVariation, float radialVariation, Vector2 _fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;

        if (navigationInitialized)
        {
            // Update field of view
            //fieldOfView = _fieldOfView;

            /////////////////////////////////////////////////////////
            // Apply pseudoLatitude and pseudoLongitude correction
            /////////////////////////////////////////////////////////
            //latitudeVariation = latitudeVariation * CalculatePseudoLatitudeMappingFactor(_fieldOfView.x) * CalculatePseudoLatitudeCorrectionFactor(_fieldOfView.x);
            //longitudeVariation = longitudeVariation * CalculatePseudoLongitudeMappingFactor(_fieldOfView.y) * CalculatePseudoLongitudeCorrectionFactor(_fieldOfView.y);
            float tVariation        = Calculate_t_Variation(latitudeVariation, _fieldOfView.x);
            float angleVariation    = CalculatePlaneAngleVariation(longitudeVariation, _fieldOfView.y);

            //Debug.Log(CalculatePseudoLongitudeMappingFactor(fieldOfView.x));
            //Debug.Log(CalculatePseudoLongitudeCorrectionFactor(fieldOfView.x));


            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            if ((0 < t_translationEllipse) && (t_translationEllipse < Mathf.PI))
            {
                angleVariation = -angleVariation;   // Depends of the camera position, to avoid that the mouse behaviour change between front and back
            }
            planeAngle += angleVariation;
            planeAngle = MathHom3r.NormalizeAngleInRad(planeAngle);
            movementEllipses.translation = CalculateTranslationEllipseParameters(planeAngle);
            
            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////                        
            CalculateNewEllipsoidAndRestOfEllipsesAfterRadialMovement(radialVariation);

            // Latitude - which is a translation movement on the camera plane1
            //t_translationEllipse += tVariation;                    // Add to the current translation angle
            //t_translationEllipse = MathHom3r.NormalizeAngleInRad(t_translationEllipse);   // Normalize new t angle 

            float new_t_translationEllipse = t_translationEllipse + tVariation;                     // Calculate new translation angle
            new_t_translationEllipse = MathHom3r.NormalizeAngleInRad(new_t_translationEllipse);     // Normalize new t angle 
            t_translationEllipse = ApplyTranslationConstraints(new_t_translationEllipse);


            movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);

            //////////////////////////////////////
            // Calculate new camera position
            //////////////////////////////////////                        
            cameraPlanePosition = CalculateNewCameraPosition(t_translationEllipse);

            //////////////////////////////////////
            // Calculate new plane rotation
            //////////////////////////////////////
            planeRotation = angleVariation;

            //////////////////////////////////////////////////
            // Calculate the point which camera hast to look
            //////////////////////////////////////////////////                
            pointToLook = CalculatePointToLook(t_translationEllipse, movementEllipses.translation);
            
            
            //////////////////////////////////////
            // HELPER
            //////////////////////////////////////            
            DrawTranslationTrajectory();                            // Draw the trajectory of the translation, for debug reasons   
            DrawRotationTrajectory(cameraPlanePosition);            
        }
    }

    /////////////////////////////////
    // INIT Auxiliary Methods
    /////////////////////////////////

    /// <summary>
    /// Classify the object geometry based on its bouding box
    /// </summary>
    /// <param name="boundingBox"></param>
    /// <returns></returns>    
    private TGeometryType3 ClassifyObjectGeometry3(Vector3 extents)
    {        
        TGeometryType3 geometryType3 = TGeometryType3.Type_I;
        if ((extents.x >= extents.z) && (extents.z >= extents.y))
        {
            geometryType3 = TGeometryType3.Type_I;
            Debug.Log("Type_I");
        }
        else if ((extents.x >= extents.y) && (extents.y >= extents.z))
        {
            geometryType3 = TGeometryType3.Type_II;
            Debug.Log("Type_II");
        }
        else if ((extents.z >= extents.x) && (extents.x >= extents.y))
        {
            geometryType3 = TGeometryType3.Type_III;
            Debug.Log("Type_III");
        }
        else if ((extents.z >= extents.y) && (extents.y >= extents.x))
        {
            geometryType3 = TGeometryType3.Type_V;
            Debug.Log("Type_V");
        }
        else if ((extents.y >= extents.x) && (extents.x >= extents.z))
        {
            geometryType3 = TGeometryType3.Type_IV;
            Debug.Log("Type_IV");
        }
        else if ((extents.y >= extents.z) && (extents.z >= extents.x))
        {
            geometryType3 = TGeometryType3.Type_VI;
            Debug.Log("Type_VI");
        }
        return geometryType3;
    }



    private float CalculateMayorAxis(float minorAxis, float Ec)
    {
        return 0.5f * (Ec + Mathf.Sqrt(MathHom3r.Pow2(Ec) + 4 * MathHom3r.Pow2(minorAxis)));
    }
    private float CalculateMinorAxis(float mayorAxis, float Ec)
    {
        return Mathf.Sqrt(MathHom3r.Pow2(mayorAxis) - (Ec * mayorAxis));

    }
    private float CalculateEvoluteCusps(float a, float b)
    {
        return a - (MathHom3r.Pow2(b) / a);
    }

    /// <summary>
    /// Calculate the reference ellipses, these are the ellipses on the xz and xy plane that frame the projections of the object on those planes
    /// </summary>
    /// <param name="extents"></param>
    /// <param name="cameraPosition"></param>   
    private CEllipsoidData CalculateEllipsoid(Vector3 extents, Vector3 cameraPosition)
    {
        CEllipsoidData _ellipsoidData = new CEllipsoidData();
        float cameraObjectDistance = Mathf.Abs(cameraPosition.z) - extents.z;

        if (geometryType == TGeometryType3.Type_I || geometryType == TGeometryType3.Type_II)
        {
            //ellipsoideData.semimajor_axis = TAxis.x;            // The semi-major axis of the ellipse on the XZ-plane is in the X-Axis
            //ellipsoideData.Ec = extents.x;                      // Evolute cusp will be in the limit of the object in X-axis
            _ellipsoidData.SetEvoluteCups(extents.x, extents.z);        // Evolute cusp will be in the limit of the object in X-axis
            _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            _ellipsoidData.radiousXAxis = CalculateMayorAxis(_ellipsoidData.radiousZAxis, extents.x);
            _ellipsoidData.radiousYAxis = cameraObjectDistance + extents.y;
        }
        else if (geometryType == TGeometryType3.Type_V || geometryType == TGeometryType3.Type_VI)
        {
            //ellipsoideData.semimajor_axis = TAxis.z;    // The semi-major axis of the ellipse on the XZ-plane is in the Z-Axis
            //ellipsoideData.Ec = extents.z;              // Evolute cusp will be in the limit of the object in Z-axis
            _ellipsoidData.SetEvoluteCups(extents.x, extents.z);
            _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            _ellipsoidData.radiousXAxis = CalculateMinorAxis(_ellipsoidData.radiousZAxis, extents.z);
            _ellipsoidData.radiousYAxis = cameraObjectDistance + extents.y;
        }
        else if (geometryType == TGeometryType3.Type_III)
        {
            //ellipsoideData.semimajor_axis = TAxis.xz;    // The semi-major axis of the ellipse on the XZ-plane is in the Z-Axis or X-Axis
            //ellipsoideData.Ec = extents.z;              // Evolute cusp will be in the limit of the object in Z-axis
            _ellipsoidData.SetEvoluteCups(extents.x, extents.z);
            _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            _ellipsoidData.radiousXAxis = CalculateMinorAxis(_ellipsoidData.radiousZAxis, extents.z);
            _ellipsoidData.radiousYAxis = cameraObjectDistance + extents.y;
        }
        else if (geometryType == TGeometryType3.Type_IV)
        {
            //ellipsoideData.semimajor_axis = TAxis.xz;    // The semi-major axis of the ellipse on the XZ-plane is in the Z-Axis or X-Axis
            //ellipsoideData.Ec = extents.x;                  // Evolute cusp will be in the limit of the object in Z-axis
            _ellipsoidData.SetEvoluteCups(extents.x, extents.z);
            _ellipsoidData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            _ellipsoidData.radiousXAxis = CalculateMayorAxis(_ellipsoidData.radiousZAxis, extents.x);
            _ellipsoidData.radiousYAxis = cameraObjectDistance + extents.y;
        }

        return _ellipsoidData;
    }

    /// <summary>    
    /// Calculate the initial parameters of the ellipse that the camera follows as a trajectory in its translational movements in the plane
    /// </summary>
    /// <param name="cameraPosition">Current camera position.</param>       
    private CTranslationEllipseData CalculateInitialTranslationEllipseParameters_new(Vector3 cameraPosition)
    {
        CTranslationEllipseData _ellipse = new CTranslationEllipseData(); ;
        _ellipse.radiousXAxis = ellipsoidData.radiousXAxis;
        _ellipse.radiousZAxis = ellipsoidData.radiousZAxis;
        _ellipse.evoluteCusp = ellipsoidData.GetEvoluteCusp(_ellipse.GetSemiMajorAxis());       

        return _ellipse;
    }

    /// <summary>
    /// Calculate the initial T parameter of the ellipse based on b/a and current camera position
    /// </summary>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>return the t parameter float value</returns>
    private float CalculateTranslationEllipseInitialTParameter()
    {      
        return (Mathf.PI * -0.5f);
    }

    /// <summary>
    /// Calculate the minimum ellipse that which surrounds the 3d object without cross it. 
    /// In the case of long object this value is the b minimum, 
    /// while in the case of flat objects this vale is the a minimum.
    /// </summary>
    /// <param name="extents">Bounding box extends of the 3D object</param>
    /// <returns>Return the minimum a of b of the allowed ellipse between the camera and the object</returns>
    private float CalculateMinimunEllipsoid(Vector3 extents)
    {
        float minimunAxis = 0.0f;

        float Ec = ellipsoidData.GetEvoluteCusp(movementEllipses.translation.GetSemiMajorAxis());
        float r2 = MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z);
        
        // Polynomial Coefficients: x^3 + a1 * x^2 + a2 * x + a3 = 0
        float A = -Ec;                      
        float B = -(MathHom3r.Pow2(Ec) + r2);                
        float C = Mathf.Pow(Ec, 3);

        //Calculate Q, R
        float Q = (1 / 9.0f) * (3.0f * B - MathHom3r.Pow2(A));
        float R = (1 / 54.0f) * (9.0f * A * B - 27 * C - 2 * Mathf.Pow(A, 3));
        //Calculate Q^3 and R^2
        float Q3 = Mathf.Pow(Q, 3);
        // Calculate roots
        float teta = Mathf.Acos(-R / Mathf.Sqrt(-Q3));
        float x2 = -2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((teta + (2.0f * Constants.Pi)) / 3.0f) - (A / 3.0f);


        ///
        //float P = ((3f * B) - MathHom3r.Pow2(A)) / 3f;
        //float q = ((2f * Mathf.Pow(A,3f)) + (9.0f * A * B) + 27f *C)/ 27f;
        //float teta2 = Mathf.Acos((- 0.5f * q) / Mathf.Sqrt(-1f * Mathf.Pow((P/3f), 3)));
        //float a0 = 2f * Mathf.Sqrt(-P / 3) * Mathf.Cos(teta2 / 3f) - (A / 3);


        //float Ec2 = MathHom3r.Pow2(Ec);
        //float teta_num = 16 * Mathf.Pow(Ec, 3) - 9 * Ec * r2;
        //float teta_deno = Mathf.Sqrt(64 * Mathf.Pow(Ec, 6) + 144 * Mathf.Pow(Ec, 4) * r2 + 108 * Ec2 * MathHom3r.Pow2(r2) + 27 * Mathf.Pow(r2, 3));
        //float teta2 = Mathf.Acos(-0.5f * (teta_num / teta_deno));
        //float amin = (2 / 3) * Mathf.Sqrt(4 * Ec2 + 3 * r2) * Mathf.Cos(teta2 / 3) + (Ec / 3);
        ///
        //float a_min = amin;
        
        //Calculate ellipsoide minimal
        float a_min = x2;            
        float b_min2 = MathHom3r.Pow2(a_min) - a_min * Ec;
        float b_min = Mathf.Sqrt(b_min2);

        // Calculate c minimal
        float b_p2 = b_min2 * (1 - MathHom3r.Pow2(Ec/a_min));
        float c_p2 = (MathHom3r.Pow2(extents.y) * b_p2) / (b_p2- MathHom3r.Pow2(extents.z));
        float temp = MathHom3r.Pow2(a_min) / (MathHom3r.Pow2(a_min) - (MathHom3r.Pow2(Ec)));
        float c_min = Mathf.Sqrt(temp * c_p2);
            

        minimunEllipsoidData = new CEllipsoidData();
        minimunEllipsoidData.radiousXAxis = a_min;
        minimunEllipsoidData.radiousZAxis = b_min;
        minimunEllipsoidData.radiousYAxis = c_min;
        if (geometryType == TGeometryType3.Type_I || geometryType == TGeometryType3.Type_II || geometryType == TGeometryType3.Type_IV)
        {
            minimunAxis = b_min;
        }
        else if (geometryType == TGeometryType3.Type_V || geometryType == TGeometryType3.Type_VI || geometryType == TGeometryType3.Type_III)
        {
            minimunAxis = a_min;
        }

        ///
        if (geometryType == TGeometryType3.Type_I || geometryType == TGeometryType3.Type_II)
        {
            minimunEllipsoidData.radiousXAxis = x2;
            minimunEllipsoidData.radiousZAxis = CalculateMinorAxis(x2, Ec);
            minimunEllipsoidData.radiousYAxis = c_min;
        }
        else 
        {
            // Falla para D?
            minimunEllipsoidData.radiousZAxis = x2;
            minimunEllipsoidData.radiousXAxis = CalculateMinorAxis(x2, Ec);
            minimunEllipsoidData.radiousYAxis = c_min;
        }
        minimunAxis = minimunEllipsoidData.radiousZAxis;
        ///

        


        return minimunAxis;
    }

    private float CalculateRealMinimumZRadious(float zRadious)
    {
        return 1.05f * CalculateRealMinimumZRadiousRecursive(zRadious);
    }
    /// <summary>
    /// Recursive method to calculate the real minimum radius.
    /// </summary>
    /// <param name="zRadious"></param>
    /// <returns></returns>
    private float CalculateRealMinimumZRadiousRecursive(float zRadious)
    {

        CEllipsoidData ellipsoidCandidate = CalculateEllipsoid(extents, new Vector3(0, 0, zRadious));

        if (ellipsoidCandidate.radiousXAxis < minimunEllipsoidData.radiousXAxis
            || ellipsoidCandidate.radiousYAxis < minimunEllipsoidData.radiousYAxis
            || ellipsoidCandidate.radiousZAxis < minimunEllipsoidData.radiousZAxis)
        {
            return zRadious = CalculateRealMinimumZRadiousRecursive(zRadious * 1.02f);
        }
        return zRadious;
    }

    /// <summary>
    /// Check if the initial camera position proposed is correct or not
    /// </summary>
    /// <param name="cameraPosition">Camera position</param>
    /// <returns>true is the proposed camera position is OK</returns>
    private bool CheckInitialMinimunDistance(Vector3 cameraPosition)
    {
        //TODO What Do happen if the proposed camera position is not in Z axis?
        if (Mathf.Abs(cameraPosition.z) < minimunAllowedAxisSize) return false;
        return true;
    }

    //////////////////////////////////////////////////
    //////////////////////////////////////////////////
    //                                              //
    // CALCULATECameraPosition Auxiliary Methods    //
    //                                              //
    //////////////////////////////////////////////////    
    //////////////////////////////////////////////////
    
   


    /// <summary>
    /// Calculate the parameters of the ellipse that the camera follows as a trajectory in its translational movements in the plane
    /// </summary>
    /// <param name="planeAngle"></param>
    /// <returns></returns>
    private CTranslationEllipseData CalculateTranslationEllipseParameters(float planeAngle)
    {
        CTranslationEllipseData newTranslationEllipse = new CTranslationEllipseData();

        float nume = ellipsoidData.radiousZAxis * ellipsoidData.radiousYAxis;
        float deno = MathHom3r.Pow2(ellipsoidData.radiousZAxis * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(ellipsoidData.radiousYAxis * Mathf.Cos(planeAngle));
        float newSemiAxis = nume / Mathf.Sqrt(deno);

        newTranslationEllipse.radiousXAxis = ellipsoidData.radiousXAxis;
        newTranslationEllipse.radiousZAxis = newSemiAxis;
        
        //The evolute cusps has to be calculated because this ellipse is changing after any rotation or radial movement
        CSemiAxes _semiAxes = newTranslationEllipse.GetSemiAxes();
        newTranslationEllipse.evoluteCusp = CalculateEvoluteCusps(_semiAxes.a, _semiAxes.b);
       
        return newTranslationEllipse;
    }
        
    /// <summary>
    /// Calculate the parameters of the ellipse that the camera follows as a trajectory in its rotational movements.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private CRotationEllipseData CalculateRotationEllipseParameters(float t)
    {
        CRotationEllipseData _rotationEllipseData = new CRotationEllipseData();

        _rotationEllipseData.radiousZAxis = Mathf.Abs(ellipsoidData.radiousZAxis * Mathf.Sin(t));
        _rotationEllipseData.radiousYAxis = Mathf.Abs(ellipsoidData.radiousYAxis * Mathf.Sin(t));
        
        return _rotationEllipseData;
    }

    /// <summary>
    /// Calculate the new ellipsoid and the new rotation and translation ellipses after a radial movement of the camera
    /// </summary>
    /// <param name="radialVariation"></param>
    private void CalculateNewEllipsoidAndRestOfEllipsesAfterRadialMovement(float radialVariation)
    {
        float new_z;
        
        //new_z = ellipsoidData.radiousZAxis + radialVariation;
        new_z = CalculateNewZRadious(ellipsoidData.radiousZAxis, radialVariation);
        CEllipsoidData ellipsoidCandidate = CalculateEllipsoid(extents, new Vector3(0, 0, new_z));      // Calculate new ellipsoid after camera distance changed
        // Check if the new ellipsoid is valid
        if (IsEllipsoidAllowed(ellipsoidCandidate))
        {
            ellipsoidData = ellipsoidCandidate;     // Update ellipsoid data
            movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);       // Calculate new rotation trajectory            
            movementEllipses.translation = CalculateTranslationEllipseParameters(planeAngle);           // Calculate new translation ellipse
            DrawReferenceEllipses();
        }
    }

    private float CalculateNewZRadious(float currentZRadious, float radialVariation)
    {
        float new_z;
        if (radialVariation <= -10000)
        {
            float zoomPercentage = -0.01f * (radialVariation + 10000);
            float offset = (initialAxisSize - minimunAllowedAxisSize) * (1 - zoomPercentage);
            new_z = minimunAllowedAxisSize + offset;
            //new_z = minimunAllowedAxisSize * 1.75f;            
        }
        else if (radialVariation >= 10000)
        {
            float zoomPercentage = 0.01f * (radialVariation - 10000);
            float offset = (initialAxisSize - minimunAllowedAxisSize) * (1 - zoomPercentage);
            new_z = minimunAllowedAxisSize + offset;
            //new_z = initialAxisSize;
        }
        else
        {
            new_z = currentZRadious + radialVariation;              
        }
        return new_z;
    }

    private bool IsEllipsoidAllowed(CEllipsoidData ellipsoidCandidate)
    {
        bool allowedEllipsoid = true;

        if (    ellipsoidCandidate.radiousXAxis < minimunEllipsoidData.radiousXAxis 
            ||  ellipsoidCandidate.radiousYAxis < minimunEllipsoidData.radiousYAxis 
            ||  ellipsoidCandidate.radiousZAxis < minimunEllipsoidData.radiousZAxis)
        {
            return false;
        }

        return allowedEllipsoid;
    }

    /// <summary>
    /// Calculate the new camera position in function of a,b and t of the ellipse
    /// </summary>
    /// <returns>Returns the new camera position</returns>
    private Vector3 CalculateNewCameraPosition(float t)
    {
        Vector3 cameraPositionOnPlane = Vector3.zero;
        cameraPositionOnPlane.x = movementEllipses.translation.radiousXAxis * Mathf.Cos(t);
        cameraPositionOnPlane.y = 0;
        cameraPositionOnPlane.z = movementEllipses.translation.radiousZAxis * Mathf.Sin(t);
        return cameraPositionOnPlane;
    }
    
    /// <summary>
    /// Calculate the direction in which the camera should face
    /// </summary>
    /// <param name="t"></param>
    /// <param name="currentEllipse"></param>
    /// <returns></returns>
    private Vector3 CalculatePointToLook(float t, CTranslationEllipseData currentEllipse)
    {
        Vector3 intersectionPoint = new Vector3();
       
        CSemiAxes _translationEllipseSemiAxes = currentEllipse.GetSemiAxes();
        float a = _translationEllipseSemiAxes.a;
        float b = _translationEllipseSemiAxes.b;

        if (currentEllipse.GetSemiMajorAxis() == TAxis.x)
        {
            float intersectionXaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Cos(t);
            float evoluteCusps = ellipsoidData.GetEvoluteCusp(TAxis.x);

            intersectionXaxis = CalculateLinearInterpolation(intersectionXaxis, currentEllipse.evoluteCusp, evoluteCusps);
            intersectionPoint = new Vector3(intersectionXaxis, 0, 0);
        }
        else
        {
            float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);

            if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.none)
            {
                //float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
                float evoluteCusps = ellipsoidData.GetEvoluteCusp(TAxis.z);
                intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);
            } else if ((hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.interpolation))
            {
                float evoluteCuspsZ = ellipsoidData.GetEvoluteCusp(TAxis.z);
                float evoluteCuspsX = ellipsoidData.GetEvoluteCusp(TAxis.x);
                float evoluteCusps = Mathf.Sqrt(MathHom3r.Pow2(evoluteCuspsX * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(evoluteCuspsZ * Mathf.Cos(planeAngle)));
                intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);
            }
            //if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection())
            //{
                
            //    float evoluteCuspsZ = ellipsoidData.GetEvoluteCusp(TAxis.z);
            //    float evoluteCuspsX = ellipsoidData.GetEvoluteCusp(TAxis.x);
            //    float evoluteCusps = Mathf.Sqrt(MathHom3r.Pow2(evoluteCuspsX * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(evoluteCuspsZ * Mathf.Cos(planeAngle)));
            //    intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);
            //} else
            //{
            //    //float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
            //    float evoluteCusps = ellipsoidData.GetEvoluteCusp(TAxis.z);
            //    intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);
            //}
            intersectionPoint = new Vector3(0, 0, intersectionZaxis);
        }
        if (geometryType == TGeometryType3.Type_III || geometryType == TGeometryType3.Type_IV /*|| geometryType == TGeometryType3.Type_V*/)
        {

            if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.none)
            {
                float evoluteCuspsX = ellipsoidData.GetEvoluteCusp(TAxis.x);
                float intersectionXaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Cos(t);
                intersectionXaxis = CalculateLinearInterpolation(intersectionXaxis, currentEllipse.evoluteCusp, evoluteCuspsX);

                float evoluteCuspsZ = ellipsoidData.GetEvoluteCusp(TAxis.z);
                float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
                intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCuspsZ);

                intersectionPoint = new Vector3(intersectionXaxis, 0, intersectionZaxis);
            } else if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection() == TOblateSpheroidCorrectionMode.interpolation) {
                float evoluteCuspsX = ellipsoidData.GetEvoluteCusp(TAxis.x);
                float evoluteCuspsZ = ellipsoidData.GetEvoluteCusp(TAxis.z);
                float evoluteCusps = Mathf.Sqrt(MathHom3r.Pow2(evoluteCuspsX * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(evoluteCuspsZ * Mathf.Cos(planeAngle)));

                float intersectionXaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Cos(t);
                intersectionXaxis = CalculateLinearInterpolation(intersectionXaxis, currentEllipse.evoluteCusp, evoluteCusps);
                float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
                intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);

                intersectionPoint = new Vector3(intersectionXaxis, 0, intersectionZaxis);
            }



            //    if (hom3r.coreLink.GetComponent<ConfigurationManager>().GetActiveNavigationOblateOrientationCorrection())
            //{
            //    float evoluteCuspsX = ellipsoidData.GetEvoluteCusp(TAxis.x);
            //    float evoluteCuspsZ = ellipsoidData.GetEvoluteCusp(TAxis.z);
            //    float evoluteCusps = Mathf.Sqrt(MathHom3r.Pow2(evoluteCuspsX * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(evoluteCuspsZ * Mathf.Cos(planeAngle)));

            //    float intersectionXaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Cos(t);
            //    intersectionXaxis = CalculateLinearInterpolation(intersectionXaxis, currentEllipse.evoluteCusp, evoluteCusps);
            //    float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
            //    intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);

            //    intersectionPoint = new Vector3(intersectionXaxis, 0, intersectionZaxis);
            //}
            //else
            //{
            //    float evoluteCuspsX = ellipsoidData.GetEvoluteCusp(TAxis.x);
            //    float intersectionXaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Cos(t);                
            //    intersectionXaxis = CalculateLinearInterpolation(intersectionXaxis, currentEllipse.evoluteCusp, evoluteCuspsX);

            //    float evoluteCuspsZ = ellipsoidData.GetEvoluteCusp(TAxis.z);
            //    float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
            //    intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCuspsZ);

            //    intersectionPoint = new Vector3(intersectionXaxis, 0, intersectionZaxis);
            //}
        }


        //Debug.Log(intersectionPoint);
        return intersectionPoint;
    }


    /// <summary>
    /// Calculates linear interpolation to keep the direction the camera is facing always inside the object.
    /// </summary>
    /// <param name="intersectionXaxis"></param>
    /// <param name="_cameraEllipseEc"></param>
    /// <param name="_desiredEvoluteCusps"></param>
    /// <returns></returns>
    private float CalculateLinearInterpolation(float intersectionXaxis, float _cameraEllipseEc, float _desiredEvoluteCusps)
    {

        return ((intersectionXaxis / _cameraEllipseEc) * _desiredEvoluteCusps);
    }

    private float Calculate_t_Variation(float _latitudeVariation, float fieldOfViewX)
    {
        float final_t_Variation = _latitudeVariation;
        TInteractionMappingCorrectionMode latitudeCorrection = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLatitudeInteractionCorrectionMode();

        if (latitudeCorrection == TInteractionMappingCorrectionMode.distance)
        {
            // Get Semiaxes of the inscribed ellipse inside the object
            CSemiAxes _incribedEllipseSemiAxes = GetInscribedTranslationEllipseSemiAxes();
            //Calculate t-variation in function of the ARC variation
            final_t_Variation *= CalculateLatitudeArcMappingFactor(fieldOfViewX, _incribedEllipseSemiAxes.a, _incribedEllipseSemiAxes.b);


            //Calculate t-variation applying the distance between camera and object correction  
            final_t_Variation *= CalculateLatitudeDistanceCorrectionFactor(t_translationEllipse, fieldOfViewX);
        }
        else if (latitudeCorrection == TInteractionMappingCorrectionMode.ellipsePerimeter)
        {
            // Get Semiaxes of the camera translation ellipse
            CSemiAxes _cameraEllipseSemiAxes = movementEllipses.translation.GetSemiAxes();
            //Calculate t-variation in function of the ARC variation
            final_t_Variation *= CalculateLatitudeArcMappingFactor(fieldOfViewX, _cameraEllipseSemiAxes.a, _cameraEllipseSemiAxes.b);
            // Calculate t-variation applying the ellipse perimeter correction
            final_t_Variation *= (0.5f * CalculateEllipsePerimeter(_cameraEllipseSemiAxes.a, _cameraEllipseSemiAxes.b));
        }
        else if (latitudeCorrection == TInteractionMappingCorrectionMode.none)
        {
            //Do nothing
            // Get Semiaxes of the camera translation ellipse
            CSemiAxes _cameraEllipseSemiAxes = movementEllipses.translation.GetSemiAxes();
            //Calculate t-variation in function of the ARC variation
            final_t_Variation *= CalculateLatitudeArcMappingFactor(fieldOfViewX, _cameraEllipseSemiAxes.a, _cameraEllipseSemiAxes.b);
        }
        return final_t_Variation;
    }




    private float CalculatePlaneAngleVariation(float _longitudeVariation, float fieldOfViewY)
    {
        float finalAngleVariation = _longitudeVariation;
        TInteractionMappingCorrectionMode longitudeCorrection = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetLongitudeInteractionCorrectionMode();

        if (longitudeCorrection == TInteractionMappingCorrectionMode.distance)
        {

            // Get Semiaxes of the inscribed ellipse inside the object            
            CSemiAxes _incribedEllipseSemiAxes = GetInscribedRotationEllipseSemiAxes();
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateLongitudeArcMappingFactor(fieldOfViewY, _incribedEllipseSemiAxes.a, _incribedEllipseSemiAxes.b);

            //Calculate angle variation applying the distance between camera and object correction  
            finalAngleVariation *= CalculateLongitudeDistanceCorrectionFactor(fieldOfViewY);
        }
        else if (longitudeCorrection == TInteractionMappingCorrectionMode.ellipsePerimeter)
        {
            // Get Semiaxes of the camera rotation ellipse           
            CSemiAxes _rotationEllipseSemiAxes = movementEllipses.rotation.GetSemiAxes();
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateLongitudeArcMappingFactor(fieldOfViewY, _rotationEllipseSemiAxes.a, _rotationEllipseSemiAxes.b);
            // Calculate t-variation applying the ellipse perimeter correction
            finalAngleVariation *= (0.5f * CalculateEllipsePerimeter(_rotationEllipseSemiAxes.a, _rotationEllipseSemiAxes.b));

            //finalAngleVariation *= Mathf.PI;
        }
        else if (longitudeCorrection == TInteractionMappingCorrectionMode.none)
        {            
            // Get Semiaxes of the camera rotation ellipse           
            CSemiAxes _rotationEllipseSemiAxes = movementEllipses.rotation.GetSemiAxes();
            //Calculate angle variation in function of the ARC variation
            finalAngleVariation *= CalculateLongitudeArcMappingFactor(fieldOfViewY, _rotationEllipseSemiAxes.a, _rotationEllipseSemiAxes.b);
        }

        return finalAngleVariation;
    }

    /// <summary>
    /// Calculate the ellipse perimeter using the Ranaujan II-Cantrell.
    /// ref: https://www.universoformulas.com/matematicas/geometria/perimetro-elipse/
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float CalculateEllipsePerimeter(float a, float b)
    {
        float ellipsePerimeter;

        float H = MathHom3r.Pow2((a - b) / (a + b));
        float _3H = 3 * H;
        float HTwelve = Mathf.Pow(H, 12);

        float secondSummand = (3 * H) / (10 + Mathf.Sqrt(4 - _3H));
        float thirdSummand = ((4 / Mathf.PI) - (14 / 11)) * HTwelve;

        ellipsePerimeter = Mathf.PI * (a + b) * (1 + secondSummand + thirdSummand);

        return ellipsePerimeter;
    }

    /// <summary>
    /// Returns the parameters of the ellipse enclosed in the object, taking into account rotation of the plane
    /// </summary>
    /// <returns></returns>
    private CSemiAxes GetInscribedTranslationEllipseSemiAxes()
    {
        CSemiAxes inscribedEllipseSemiAxes = new CSemiAxes();
                              
        float axisOne = extents.x;
        float nume = extents.z * extents.y;
        float deno = MathHom3r.Pow2(extents.z * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(extents.y * Mathf.Cos(planeAngle));
        float axisTwo = nume / Mathf.Sqrt(deno);

        inscribedEllipseSemiAxes.a = axisOne > axisTwo ? axisOne : axisTwo;
        inscribedEllipseSemiAxes.b = axisOne > axisTwo ? axisTwo : axisOne;


        //Debug.Log("Inscribed Ellipse SemiAxes");
        //Debug.Log("extents.x: " + extents.x + " - extents.y: " + extents.y + " - extents.z: " + extents.z);
        //Debug.Log("a: " + inscribedEllipseSemiAxes.a + " b: " + inscribedEllipseSemiAxes.b);

        return inscribedEllipseSemiAxes;
    }

    private CSemiAxes GetInscribedRotationEllipseSemiAxes()
    {
        CSemiAxes inscribedEllipseSemiAxes = new CSemiAxes();

        float axisOne = extents.y;
        float axisTwo = extents.z;        

        inscribedEllipseSemiAxes.a = axisOne > axisTwo ? axisOne : axisTwo;
        inscribedEllipseSemiAxes.b = axisOne > axisTwo ? axisTwo : axisOne;


        //Debug.Log("Inscribed Rotation Ellipse SemiAxes");
        //Debug.Log("extents.x: " + extents.x + " - extents.y: " + extents.y + " - extents.z: " + extents.z);
        //Debug.Log("a: " + inscribedEllipseSemiAxes.a + " b: " + inscribedEllipseSemiAxes.b);

        return inscribedEllipseSemiAxes;
    }

    /// <summary>
    /// Returns the pseudo latitude mapping factor, t in function of the ellipse arc.
    /// </summary>
    /// <param name="fieldOfView_rad"></param>
    /// <returns></returns>
    //private float CalculatePseudoLatitudeMappingFactor(float fieldOfView_rad, bool useEnclosedEllipse = true)
    //{
    //    float factor = 0f;

    //    float ai;
    //    float bi;

    //    if (useEnclosedEllipse)
    //    {
    //        CEllipseData rotatedEnclosedEllipseParameter = GetEnclosedEllipseParameters();
    //        ai = rotatedEnclosedEllipseParameter.a;
    //        bi = rotatedEnclosedEllipseParameter.b;
    //    }
    //    else
    //    {
    //        CSemiAxes _semiAxes = movementEllipses.translation.GetSemiAxes();
    //        ai = _semiAxes.a;
    //        bi = _semiAxes.b;
    //    }

    //    float arco = Mathf.Sqrt(MathHom3r.Pow2(ai * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bi * Mathf.Cos(t_translationEllipse)));       
    //    factor = 1 / arco;

    //    return factor;
    //}

    private float CalculateLatitudeArcMappingFactor(float fieldOfView_rad, float ai, float bi)
    {
        float factor = 0f;
        
        float arco = Mathf.Sqrt(MathHom3r.Pow2(ai * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bi * Mathf.Cos(t_translationEllipse)));
        factor = 1 / arco;

        return factor;
    }

    /// <summary>
    /// Returns pseudolatitude correction factor, make Δt independent of the distance between the camera and the object. 
    /// </summary>
    /// <param name="horizontalFieldOfViewAngle_rad">camera horizontal field of view angle</param>
    /// <returns></returns>
    private float CalculateLatitudeDistanceCorrectionFactor(float t, float horizontalFieldOfViewAngle_rad)
    {
        float ai;
        float bi;

        CSemiAxes rotatedEnclosedEllipseParemeter = GetInscribedTranslationEllipseSemiAxes();
        ai = rotatedEnclosedEllipseParemeter.a;
        bi = rotatedEnclosedEllipseParemeter.b;
              
        
        // CalculateMappingCorrectionParameterK
        float k = 1f;

        /////////////////////////
        // Calculate P        
        /////////////////////////
        Vector3 P = CalculateNewCameraPosition(t);


        /////////////////////////
        // Calculate point Q     
        /////////////////////////

        float ai2 = MathHom3r.Pow2(ai);
        float bi2 = MathHom3r.Pow2(bi);


        CSemiAxes _translationEllipseSemiAxes = movementEllipses.translation.GetSemiAxes();
        float aTranslationEllipse = _translationEllipseSemiAxes.a;
        float bTranslationEllipse = _translationEllipseSemiAxes.b;        

        float Mt0 = (aTranslationEllipse / bTranslationEllipse) * Mathf.Tan(t);
        float D = ((MathHom3r.Pow2(aTranslationEllipse) / bTranslationEllipse) - bTranslationEllipse) * Mathf.Sin(t);
        float Mt02 = MathHom3r.Pow2(Mt0);
        float D2 = MathHom3r.Pow2(D);


        // Calculate xQ
        float xq_nume_firstTerm = ai2 * Mt0 * D;
        float xq_nume_radicand = Mathf.Sqrt(bi2 - D2 + ai2 * Mt02);
        float xq_nume_secondTerm = bi * ai * xq_nume_radicand;
        float denom = bi2 + ai2 * Mt02;
        float xQ = (xq_nume_firstTerm + xq_nume_secondTerm) / denom;

        // Calculate zQ
        float zQ = 0f;
        float radicand = 1 - MathHom3r.Pow2(xQ / ai);
        //Debug.Log(radicand);
        if (radicand > 0f) { zQ = bi * Mathf.Sqrt(radicand); }     // Sometimes it is <0 --> I don't know why
        //float zQ = bMin * Mathf.Sqrt(1 - MathHom3r.Pow2(xQ / aMin));

        if (P.z < 0) { zQ *= -1; }  //TODO Resolve this "chapuza"
        Vector3 Q = new Vector3(xQ, 0, zQ);


        // Calculate distance P-Q
        float dPQ = Mathf.Sqrt((P - Q).sqrMagnitude);
        //Vector3 PWorld = hom3r.quickLinks.navigationSystemObject.transform.TransformPoint(P);
        //Vector3 QWorld = hom3r.quickLinks.navigationSystemObject.transform.TransformPoint(Q);        
        //float dPQW = Mathf.Sqrt((PWorld - QWorld).sqrMagnitude);
        //Debug.Log(dPQ + " - " + dPQW);


        //Calculate K    
        k = 2 * dPQ * Mathf.Tan(horizontalFieldOfViewAngle_rad);

        if (float.IsNaN(k))
        {
            //Debug.Log("nan");
            k = k_lastvalidvalue;
        }
        else
        {
            k_lastvalidvalue = k;
        }
        return k;        
    }


    private float CalculateLongitudeArcMappingFactor(float fieldOfView_rad, float ai, float bi)
    {
        float factor = 0f;

        //float ai;
        //float bi;

        //if (extents.z > extents.y)
        //{
        //    ai = extents.z;
        //    bi = extents.y;
        //} else
        //{
        //    ai = extents.y;
        //    bi = extents.z;
        //}
                

        float arco = Mathf.Sqrt(MathHom3r.Pow2(bi * Mathf.Cos(planeAngle)) + MathHom3r.Pow2(ai * Mathf.Sin(planeAngle)));
        factor = 1 / arco;

        return factor;
    }
    
    private float CalculateLongitudeDistanceCorrectionFactor(float verticalFieldOfViewAngle_rad)
    {
        // Calculate minimum circumference
        CSemiAxes _incribedEllipseSemiAxes = GetInscribedRotationEllipseSemiAxes();
        float ai = _incribedEllipseSemiAxes.a;
        float bi = _incribedEllipseSemiAxes.b;

        //if (extents.z> extents.y)
        //{
        //    ai = extents.z;
        //    bi = extents.y;
        //}
        //else
        //{
        //    ai = extents.y;
        //    bi = extents.z;
        //}
        


        // CalculateMappingCorrectionParameterK
        float k = 1f;

        /////////////////////////
        // Calculate P        
        /////////////////////////
        Vector3 P = CalculateNewCameraPosition(t_translationEllipse);
        
        /////////////////////////
        // Calculate point Q     
        /////////////////////////

        float ai2 = MathHom3r.Pow2(ai);
        float bi2 = MathHom3r.Pow2(bi);

        
        //movementEllipses.rotation2.GetSemiAxesParametres(geometryType3, out aRotationEllipse, out bRotationEllipse);
        CSemiAxes _rotationEllipseSemiAxes = movementEllipses.rotation.GetSemiAxes();
        float aRotationEllipse = _rotationEllipseSemiAxes.a;
        float bRotationEllipse = _rotationEllipseSemiAxes.b;


        float Mt0 = (aRotationEllipse / bRotationEllipse) * Mathf.Tan(planeAngle);
        float D = ((MathHom3r.Pow2(aRotationEllipse) / bRotationEllipse) - bRotationEllipse) * Mathf.Sin(planeAngle);
        float Mt02 = MathHom3r.Pow2(Mt0);
        float D2 = MathHom3r.Pow2(D);


        // Calculate xQ
        float xq_nume_firstTerm = ai2 * Mt0 * D;
        float xq_nume_radicand = Mathf.Sqrt(bi2 - D2 + ai2 * Mt02);
        float xq_nume_secondTerm = bi * ai * xq_nume_radicand;
        float denom = bi2 + ai2 * Mt02;
        float xQ = (xq_nume_firstTerm + xq_nume_secondTerm) / denom;

        // Calculate zQ
        float yQ = 0f;
        float radicand = 1 - MathHom3r.Pow2(xQ / ai);
        //Debug.Log(radicand);
        if (radicand > 0f) { yQ = bi * Mathf.Sqrt(radicand); }     // Sometimes it is <0 --> I don't know why
        //float zQ = bMin * Mathf.Sqrt(1 - MathHom3r.Pow2(xQ / aMin));

        if (P.y < 0) { yQ *= -1; }  //TODO Resolve this "chapuza"
        Vector3 Q = new Vector3(xQ, yQ, 0);


        // Calculate distance P-Q
        float dPQ = Mathf.Sqrt((P - Q).sqrMagnitude);

        //Calculate K    
        k = 2 * dPQ * Mathf.Tan(verticalFieldOfViewAngle_rad);

        if (float.IsNaN(k))
        {
            //Debug.Log("nan");
            k = k_lastvalidvalue;
        }
        else
        {
            k_lastvalidvalue = k;
        }
        return k;
    }

    /// <summary>
    /// Check navigation constraints and applied to the translation t parameter
    /// </summary>
    /// <param name="desired_tValue"></param>
    /// <returns></returns>
    private float ApplyTranslationConstraints(float desired_tValue)
    {
        float new_tValue = t_translationEllipse;

        if (navigationConstraints == TNavigationSystemConstraints.translationLimited)
        {
            if (!((0 < desired_tValue) && (desired_tValue < Mathf.PI))) { new_tValue = desired_tValue; }   //Translation are limited only in t € [0, PI]
        }
        else
        {
            new_tValue = desired_tValue;
        }
        return new_tValue;
    }

    /// <summary>Draw the ellipse that the camera follows as a trajectory in its translational movements in the plane.</summary>
    private void DrawTranslationTrajectory()
    {        
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(movementEllipses.translation.radiousXAxis, movementEllipses.translation.radiousZAxis);        
    }

    /// <summary>
    /// Draw the ellipse that the camera follows as a trajectory in its rotational movements.
    /// </summary>
    /// <param name="_cameraPlanePosition"></param>
    private void DrawRotationTrajectory(Vector3 _cameraPlanePosition)
    {
        float radio = _cameraPlanePosition.magnitude;
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(movementEllipses.rotation.radiousZAxis, movementEllipses.rotation.radiousYAxis, _cameraPlanePosition.x);            
    }

    /// <summary>
    /// Draw a reference ellipses to help understand camera movements.
    /// </summary>
    private void DrawReferenceEllipses()
    {
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(ellipsoidData.radiousXAxis, ellipsoidData.radiousZAxis);
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(ellipsoidData.radiousXAxis, ellipsoidData.radiousYAxis);
    }
}
