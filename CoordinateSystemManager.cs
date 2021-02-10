using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CEllipseData
{
    public float a;    // Current mayor axis of the ellipse
    public float b;    // Current minor axis of the ellipse
    public float c;    // Current minor axis of the ellipse
    public float Ec;    // Asymptote of the perpendiculars to the ellipse        
}

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
    bool Init(Vector3 extents, Vector3 cameraInitialPosition, /*Vector2 fieldOfView,*/ out float minimumCameraDistance, out Vector3 pointToLook);

    /// <summary>Calculate the new camera position in terms of pseudoLatitude, pseudoLongitude and pseusoRadio</summary>
    /// <param name="pseudoLatitude">Incremental translation movements inside of the camera plane. A value of 2·PI corresponds to a complete revolution.
    /// Notice that for spherical coordinates, this corresponds to actual latitude (elevation).</param>
    /// <param name="pseudoLongitude">Incremental rotations movements of the camera plane. A value of 2·PI corresponds to a complete revolution.
    /// Notice that this corresponds to actual longitude "azimuth" for navigation systems with latitude trajectories independent from longitude.</param>
    /// <param name="pseudoRadio">Incremental movements of the camera approaching to the 3D object (zoom). It is expressed in virtual units in the Z axis 
    /// between previous and next orbits. </param>
    /// <param name="cameraPlanePosition">Out parameter that contains the new camera position within the plane (coordinates X and Y, where X corresponds
    /// to the object's intrinsic rotation axis). </param>
    /// <param name="planeRotation">Out parameter that contains the rotation that has to be applied to the plane. Expressed in radians.</param>
    /// <param name="pointToLook">Out parameter that indicated the direction in which the camera has to look</param>
    void CalculateCameraPosition(float pseudoLatitude, float pseudoLongitude, float pseudoRadio, Vector2 fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook);
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
    //Control variable
    bool navigationInitialized = false;
    
    Vector3 extents;            // Store the product bounding box extents
    Vector2 fieldOfView;        // Store the camera field of View

    public bool Init(Vector3 _extents, Vector3 cameraInitialPosition, /*Vector2 _fieldOfView,*/ out float minimumCameraDistance, out Vector3 pointToLook)
    {
        /////////////////////
        // Save parameters 
        /////////////////////
        extents = _extents;
        //fieldOfView = _fieldOfView;
        

        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        /////////////////////////////////////////////////////////////// 
        r = cameraInitialPosition.magnitude;                // radius of the circumference passing through that point 
        t = Mathf.Asin(cameraInitialPosition.z / r);        // t = ASin(z/r)
        t = MathHom3r.NormalizeAngleInRad(t);               // Normalize t angle between 0-2PI
        this.DrawTranslationTrajectory(r, r);               // Draw the trajectory of the translation, for debug reasons
        
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

    public void CalculateCameraPosition(float pseudoLatitudeVariation, float pseudoLongitudeVariation, float pseudoRadio, Vector2 fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;
        if (navigationInitialized)
        {
            /////////////////////////////////////////////////////////
            // Apply pseudoLatitude and pseudoLongitude correction
            /////////////////////////////////////////////////////////
            pseudoLatitudeVariation = pseudoLatitudeVariation * CalculatePseudoLatitudeLongitudeCorrectionParameter(fieldOfView.x);             
            pseudoLongitudeVariation = pseudoLongitudeVariation * CalculatePseudoLatitudeLongitudeCorrectionParameter(fieldOfView.y);

            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////        
            r += pseudoRadio;                                           // Circumference Radius
            if (Mathf.Abs(r) < minimunRadious) { r = minimunRadious; }  // We can not closer that minimum
            this.DrawTranslationTrajectory(r, r);                       // Draw the trajectory of the translation, for debug reasons
            
            // Latitude - which is a translation movement on the camera plane
            t += pseudoLatitudeVariation;                    // Add to the current translation angle
            t = MathHom3r.NormalizeAngleInRad(t);   // Normalize new t angle 

            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            if ((0 < t) && (t < Mathf.PI))
            {
                pseudoLongitudeVariation = -pseudoLongitudeVariation;   // Depends of the camera position, to avoid that the mouse behaviour change between front and back
            }
            planeAngle = pseudoLongitudeVariation;

            
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
        }        
    }

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>point which camera has to look</returns>
    private Vector3 CalculatePointToLook()
    {
        return Vector3.zero;
    }

    /// <summary>Draw the translation trajectory</summary>
    /// <param name="a">Axis mayor ellipse parameter</param>
    /// <param name="b">Axis minor ellipse parameter</param>
    private void DrawTranslationTrajectory(float a, float b)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(a, b);
        }        
    }

    /// <summary>
    /// Calculate pseudo latitude and pseudo longitude correction.
    /// Based on the projection from the camera circumference to the circumference inscribed in the object.
    /// </summary>
    /// <param name="fieldOfView_rad">field of view of the camera in radians</param>
    /// <returns></returns>
    private float CalculatePseudoLatitudeLongitudeCorrectionParameter(float fieldOfView_rad)
    {        
        float rMin = MathHom3r.Max(extents);     
        float k = 2* (r - rMin) * Mathf.Tan(fieldOfView_rad);        
        return k * (1/rMin) ;        
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

    public bool Init(Vector3 extents, Vector3 cameraInitialPosition, /*Vector2 _fieldOfView,*/ out float minimumCameraDistance, out Vector3 pointToLook)    
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
    CEllipseData translationEllipse = new CEllipseData();

    float t_translationEllipse;                    // Current t parameter that define camera position on the ellipse
    //Plane rotation angle
    float planeAngle;           // Current rotation angle    
    //Ellipse limits
    float minimunAllowedAxis;       // Minimum possible minor axis

    //Object Geometry classification
    enum TGeometryType { Prolate, Oblate };
    TGeometryType geometryType;

    //Control variables
    bool navigationInitialized = false;


    Vector3 extents;            // Store the product bounding box extents
    Vector2 fieldOfView;        // Store the camera field of View

    public bool Init(Vector3 _extents, Vector3 cameraInitialPosition,/* Vector2 _fieldOfView,*/ out float minimumCameraDistance, out Vector3 pointToLook)
    {
        /////////////////////
        // Save parameters 
        /////////////////////
        extents = _extents;
        //fieldOfView = _fieldOfView;

        //////////////////////////////////////////////////
        // Identify object geometry type. Long or flat
        //////////////////////////////////////////////////   
        geometryType = ClassifyObjectGeometry(_extents);         // Get if the object is flat or long

        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        ///////////////////////////////////////////////////////////////      
        translationEllipse.Ec = CalculateCParameter(_extents);                                           // Calculate the parameter c according to the geometry of the object and its dimensions.
        CalculateEllipseSemiAxesParameters(cameraInitialPosition, ref translationEllipse);              // Calculate semi-axis of the ellipse a,b
        t_translationEllipse = CalculateInitialTParameter(cameraInitialPosition, translationEllipse);   // Calculate the initial value of t parameter       

        ///////////////////////////////////
        // Initialize plane angle to 0
        ///////////////////////////////////
        planeAngle = 0.0f;

        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////        
        minimunAllowedAxis = CalculateMinimunEllipse(_extents, translationEllipse);      // Calculate the minimum ellipse semi-axes
        minimumCameraDistance = minimunAllowedAxis;                 // update out parameter

        //////////////////////////////////////////////////
        // Calculate the point which camera has to look
        //////////////////////////////////////////////////        
        pointToLook = CalculatePointToLook(t_translationEllipse, translationEllipse);


        //////////////////////////////////////
        // HELPER
        //////////////////////////////////////
        DrawTranslationTrajectory(translationEllipse);                            // Draw the trajectory of the translation, for debug reasons   
        DrawRotationTrajectory(cameraInitialPosition);
        //DrawCameraPosition(cameraInitialPosition, pointToLook);

        /////////////////////////////////////////////////////////////////
        //Check if the proposed initial position for the camera is OK
        /////////////////////////////////////////////////////////////////        
        if (CheckInitialMinimunDistance(cameraInitialPosition))
        {            
            navigationInitialized = true;
            return true;
        }
        else
        {
            Debug.Log("Error");
            return false;
        }


    }

    public void CalculateCameraPosition(float pseudoLatitudeVariation, float pseudoLongitudeVariation, float pseudoRadio, Vector2 _fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;

        if (navigationInitialized)
        {
            // Update field of view
            fieldOfView = _fieldOfView;

            /////////////////////////////////////////////////////////
            // Apply pseudoLatitude and pseudoLongitude correction
            /////////////////////////////////////////////////////////
            pseudoLatitudeVariation = pseudoLatitudeVariation * CalculatePseudoLatitudeMappingFactor(fieldOfView.x) * CalculatePseudoLatitudeCorrectionParameter(fieldOfView.x, translationEllipse);
            pseudoLongitudeVariation = pseudoLongitudeVariation * CalculatePseudoLongitudeCorrectionParameter(fieldOfView.y, translationEllipse);

            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////            
            translationEllipse = CalculateNewEllipseSemiAxesParametersAfterRadialMovement(pseudoRadio, translationEllipse);
                    

            // Latitude - which is a translation movement on the camera plane
            t_translationEllipse += pseudoLatitudeVariation;                    // Add to the current translation angle
            t_translationEllipse = MathHom3r.NormalizeAngleInRad(t_translationEllipse);   // Normalize new t angle 

            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            if ((0 < t_translationEllipse) && (t_translationEllipse < Mathf.PI))
            {
                pseudoLongitudeVariation = -pseudoLongitudeVariation;   // Depends of the camera position, to avoid that the mouse behaviour change between front and back
            }
            planeAngle = pseudoLongitudeVariation;


            //////////////////////////////////////
            // Calculate new camera position
            //////////////////////////////////////            
            cameraPlanePosition = CalculateNewCameraPosition(t_translationEllipse, translationEllipse);
           
            //////////////////////////////////////
            // Calculate new plane rotation
            //////////////////////////////////////
            planeRotation = planeAngle;

            //////////////////////////////////////////////////
            // Calculate the point which camera hast to look
            //////////////////////////////////////////////////        
            pointToLook = CalculatePointToLook(t_translationEllipse, translationEllipse);

            //////////////////////////////////////
            // HELPER
            //////////////////////////////////////
            DrawTranslationTrajectory(translationEllipse);                            // Draw the trajectory of the translation, for debug reasons   
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
        if ((extents.x >= extents.y) && (extents.x >= extents.z)) { return TGeometryType.Prolate; }
        else {return TGeometryType.Oblate; }
    }

    /// <summary>
    /// Calculate the parameter c according to the geometry of the object and its dimensions.
    /// If the object is long, c is the extension of the object on the x-axis. 
    /// If the object is flat, c is the extension of the object on the z-axis.
    /// </summary>
    /// <param name="extents">Object Bounding Box extents</param>
    /// <returns></returns>
    private float CalculateCParameter(Vector3 extents)
    {
        if (geometryType == TGeometryType.Prolate) { return extents.x; }
        else { return extents.z; }
    }

    /// <summary>
    /// Calculate the initial b and a parameter of the ellipse based on the object geometry and initial camera position.
    /// Store results into b and a class parameters.
    /// </summary>
    /// <param name="cameraPosition">Current camera position.</param>
    private void CalculateEllipseSemiAxesParameters(Vector3 cameraPosition, ref CEllipseData ellipse)
    {
        if (geometryType == TGeometryType.Prolate)
        {
            ellipse.b = Mathf.Abs(cameraPosition.z);
            ellipse.a = 0.5f * (ellipse.Ec + Mathf.Sqrt(MathHom3r.Pow2(ellipse.Ec) + 4 * MathHom3r.Pow2(ellipse.b)));
        }
        else
        {
            ellipse.a = Mathf.Abs(cameraPosition.z);
            ellipse.b = Mathf.Sqrt(MathHom3r.Pow2(ellipse.a) - ellipse.a * ellipse.Ec);
        }
    }

    /// <summary>
    /// Calculate the initial T parameter of the ellipse based on b/a and current camera position
    /// </summary>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>return the t parameter float value</returns>
    private float CalculateInitialTParameter(Vector3 cameraPosition, CEllipseData ellipse)
    {
        if (geometryType == TGeometryType.Prolate)
        {           
            return Mathf.Asin((cameraPosition.z) / ellipse.b);
        }
        else
        {           
            return Mathf.Asin((cameraPosition.z) / ellipse.a);
        }
    }
        
    /// <summary>
    /// Calculate the minimum ellipse that which surrounds the 3d object without cross it. 
    /// In the case of long object this value is the b minimum, 
    /// while in the case of flat objects this vale is the a minimum.
    /// </summary>
    /// <param name="extents">Bounding box extends of the 3D object</param>
    /// <returns>Return the minimum a of b of the allowed ellipse between the camera and the object</returns>
    private float CalculateMinimunEllipse(Vector3 extents, CEllipseData ellipse)
    {
        float minimunAxis = 0.0f;

        // Polynomial Coefficients: x^3 + a1 * x^2 + a2 * x + a3 = 0
        float a1 = -ellipse.Ec;
        float a2;
        if (geometryType == TGeometryType.Prolate) {
            a2 = -(MathHom3r.Pow2(ellipse.Ec) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z));
        }
        else
        {
            a2 = -(MathHom3r.Pow2(ellipse.Ec) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.x));
        }
        float a3 = Mathf.Pow(ellipse.Ec, 3);        
        
        //Calculate Q, R
        float Q = (1 / 9.0f) * (3.0f * a2 - MathHom3r.Pow2(a1));
        float R = (1 / 54.0f) * (9.0f * a1 * a2 - 27 * a3 - 2 * Mathf.Pow(a1, 3));
        //Calculate Q^3 and R^2
        float Q3 = Mathf.Pow(Q, 3);
        //float R2 = Mathf.Pow(R, 2);
        //Calculate D
        //float D = Q3 + R2;
                       
        // D is always < 0 in our case
        //if (D < 0)
        //{            
            float teta = Mathf.Acos(-R / Mathf.Sqrt(-Q3));
            float x2 = -2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((teta + (2.0f * Constants.Pi)) / 3.0f) - (a1 / 3.0f);        
            
        //}

        if (geometryType == TGeometryType.Prolate)
        {
            float b = Mathf.Sqrt(MathHom3r.Pow2(x2) - x2 * ellipse.Ec);
            minimunAxis = b;        // Long object return the b minimum of the ellipse
        }
        else
        {
            minimunAxis = x2;       //Flat object return a minimum of the ellipse
        }
        //Debug.Log(minimunAxis);
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
    /// <param name="pseudoRadio"></param>
    private CEllipseData CalculateNewEllipseSemiAxesParametersAfterRadialMovement(float pseudoRadio, CEllipseData currentEllipse)
    {
        CEllipseData newEllipse = new CEllipseData();
        newEllipse.Ec = currentEllipse.Ec;

        if (geometryType == TGeometryType.Prolate)
        {
            newEllipse.b = currentEllipse.b + pseudoRadio;
            if (Mathf.Abs(newEllipse.b) < minimunAllowedAxis) { newEllipse.b = minimunAllowedAxis; }  // We can not closer that minimum
            newEllipse.a = 0.5f * (newEllipse.Ec + Mathf.Sqrt(MathHom3r.Pow2(newEllipse.Ec) + 4 * MathHom3r.Pow2(newEllipse.b)));
        }
        else
        {
            newEllipse.a = currentEllipse.a + pseudoRadio;
            if (Mathf.Abs(newEllipse.a) < minimunAllowedAxis) { newEllipse.a = minimunAllowedAxis; }  // We can not closer that minimum
            newEllipse.b = Mathf.Sqrt(MathHom3r.Pow2(newEllipse.a) - newEllipse.a * newEllipse.Ec);
        }

        return newEllipse;
    }

    /// <summary>
    /// Calculate the new camera position in function of a,b and t of the ellipse
    /// </summary>
    /// <returns>Returns the new camera position</returns>
    private Vector3 CalculateNewCameraPosition(float t, CEllipseData currentEllipse)
    {
        Vector3 cameraPositionOnPlane;
        if (geometryType == TGeometryType.Prolate)
        {
            cameraPositionOnPlane.x = currentEllipse.a * Mathf.Cos(t);
            cameraPositionOnPlane.y = 0;
            cameraPositionOnPlane.z = currentEllipse.b * Mathf.Sin(t);
        }
        else
        {
            cameraPositionOnPlane.x = currentEllipse.b * Mathf.Cos(t);
            cameraPositionOnPlane.y = 0;
            cameraPositionOnPlane.z = currentEllipse.a * Mathf.Sin(t);
        }       
        return cameraPositionOnPlane;
    }

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>Return the point which camera has to look</returns>
    private Vector3 CalculatePointToLook_old(float t, CEllipseData currentEllipse)
    {
        float intersectionXaxis;
        if (geometryType == TGeometryType.Prolate)
        {
            intersectionXaxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a)) * Mathf.Cos(t);
        }
        else
        {
            intersectionXaxis = (currentEllipse.b - (MathHom3r.Pow2(currentEllipse.a) / currentEllipse.b)) * Mathf.Cos(t);
        }

        return new Vector3(intersectionXaxis, 0, 0);
    }

    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>Return the point which camera has to look</returns>
    private Vector3 CalculatePointToLook(float t, CEllipseData currentEllipse)
    {
        Vector3 intersectionPoint = new Vector3();    
        if (geometryType == TGeometryType.Prolate)
        {
            float intersectionXaxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a)) * Mathf.Cos(t);
            intersectionPoint = new Vector3(intersectionXaxis, 0, 0);
        }
        else
        {
            float intersectionZaxis = (currentEllipse.a - (MathHom3r.Pow2(currentEllipse.b) / currentEllipse.a)) * Mathf.Sin(t);
            intersectionPoint = new Vector3(0, 0, intersectionZaxis);
        }

        return intersectionPoint;
    }


    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawTranslationTrajectory(CEllipseData currentEllipse)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            if (geometryType == TGeometryType.Prolate)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(currentEllipse.a, currentEllipse.b);                
            }
            else
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(currentEllipse.b, currentEllipse.a);
            }
        }
    }

    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawRotationTrajectory(Vector3 _cameraPlanePosition)
    {        
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            float radio = _cameraPlanePosition.magnitude;
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(_cameraPlanePosition.z, _cameraPlanePosition.z, _cameraPlanePosition.x);
            /*if (geometryType == TGeometryType.Long)
            {                
                hom3r.quickLinks.orbitPlane.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(radio, radio);
            }
            else
            {
                hom3r.quickLinks.orbitPlane.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(b, a);
            }*/
            
        }
    }

    //private void DrawCameraPosition(Vector3 _cameraPlanePosition, Vector3 _pointToLook)
    //{
    //    //hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().MoveCameraHelper(_cameraPlanePosition, _pointToLook);
    //}


    private float CalculatePseudoLatitudeMappingFactor(float fieldOfView_rad)
    {      
        float aMin;
        float bMin;
        // Calculate minimum ellipse
        if (geometryType == TGeometryType.Prolate)
        {
            aMin = extents.x;
            bMin = extents.z;
        }
        else
        {
            aMin = extents.z;
            bMin = extents.x;
        }
        float aMin2 = MathHom3r.Pow2(aMin);
        float bMin2 = MathHom3r.Pow2(bMin);

        float arco = Mathf.Sqrt(MathHom3r.Pow2(aMin * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bMin * Mathf.Cos(t_translationEllipse)));

        float factor = 1 / arco;
        Debug.Log("PseudoLatitudeMappingFactor " + factor);
        return factor;
    }

    /// <summary>
    /// Calculate pseudo latitude correction.
    /// Based on the projection from the camera ellipse to the ellipse inscribed in the object.
    /// </summary>
    /// <param name="fieldOfView_rad">field of view of the camera in rads</param>
    /// <returns></returns>
    private float CalculatePseudoLatitudeCorrectionParameter(float fieldOfView_rad, CEllipseData currentEllipse)
    {
        float aMin;
        float bMin;
        // Calculate minimum ellipse
        if (geometryType == TGeometryType.Prolate)
        {
            aMin = extents.x;
            bMin = extents.z;
        }
        else
        {
            aMin = extents.z;
            bMin = extents.x;
        }
        float aMin2 = MathHom3r.Pow2(aMin);
        float bMin2 = MathHom3r.Pow2(bMin);

        //Calculate normal to camera ellipse
        float a2 = MathHom3r.Pow2(currentEllipse.a);
        float b2 = MathHom3r.Pow2(currentEllipse.b);

        
        float M = (currentEllipse.a / currentEllipse.b) * Mathf.Tan(t_translationEllipse);
        float D = ((a2 / currentEllipse.b) - currentEllipse.b) * Mathf.Sin(t_translationEllipse);
        float M2 = MathHom3r.Pow2(M);
        float D2 = MathHom3r.Pow2(D);

        //Calculate P        
        Vector3 P = CalculateNewCameraPosition(t_translationEllipse, translationEllipse);
        //Debug.Log("P: " + P);

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
        float k = 2 * dPQ * Mathf.Tan(fieldOfView_rad);
        //Debug.Log(k);
        //float arco = Mathf.Sqrt(MathHom3r.Pow2(aMin * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bMin * Mathf.Cos(t_translationEllipse)));

        Debug.Log("CalculatePseudoLatitudeCorrectionParameter " + k);
        return k;///arco;
    }

    /// <summary>
    /// Calculate pseudo longitude correction.
    /// Based on the projection from the camera circumference to the circumference inscribed in the object.
    /// </summary>
    /// <param name="fieldOfView_rad">field of view of the camera in radians</param>
    /// <returns></returns>
    private float CalculatePseudoLongitudeCorrectionParameter(float fieldOfView_rad, CEllipseData currentEllipse)
    {
        // Calculate minimum circumference
        float rMin = MathHom3r.Max(extents.y, extents.z);        
        
        // Calculate current circumference
        float r;        // Long 
        if (geometryType == TGeometryType.Prolate) { r = currentEllipse.b; }
        else { r = currentEllipse.a; }

        float dPQ = r - rMin;
        if (dPQ < 1) { dPQ = 1; }       //TODO Delete this Chapuza -> Maybe the problem is that the minimum ellipse is to big
        //Debug.Log("dPQ: " + dPQ);
        float k = 2 * (dPQ) * Mathf.Tan(fieldOfView_rad);

        //Debug.Log("k: " + k * (1 / rMin));
        //return 1f;
        return k * (1 / rMin);
    }
}

public class CEllipsoidCoordinatesManager : CCoordinateSystemManager
{
    /// <summary>
    /// To store axis options
    /// </summary>
    enum TAxis { x, y, z /*, xz*/ };

    /// <summary>
    /// Class that stores all the information of the virtual ellipsoid over which the camera moves. 
    /// The desired evolutes cusps values are also stored for the x- and Z-axes.
    /// </summary>
    class CEllipsoideData
    {
        public float radiousXAxis;
        public float radiousZAxis;
        public float radiousYAxis;
        //public TAxis semimajor_axis;        
        float evoluteCusp_XAxis;            //Desired value of the evolute cusp for the X-axis
        float evoluteCusp_ZAxis;            //Desired value of the evolute cusp for the X-axis

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

    /// <summary>Semi-axes of an ellipse. A is the semi-major axis B the semi-minor axis    
    class CSemiAxes
    {
        public float a;
        public float b;

        public CSemiAxes() { }
        public CSemiAxes (float _a, float _b) { a = _a; b = _b;}
    }

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
            if (radiousXAxis > radiousZAxis)    { return new CSemiAxes(radiousXAxis, radiousZAxis); }
            else                                { return  new CSemiAxes(radiousZAxis, radiousXAxis);}            
        }
        /// <summary>
        /// Returns in which axis is the semi-major axis of the ellipse
        /// </summary>
        /// <returns></returns>
        public TAxis GetSemiMajorAxis()
        {
            if (radiousXAxis > radiousZAxis) { return TAxis.x; }
            else {                             return TAxis.z; }
        }
    }
    
    class CMovementEllipses {        
        public CTranslationEllipseData translation;
        public CRotationEllipseData rotation;

        public CMovementEllipses()
        {
            translation = new CTranslationEllipseData();
            rotation = new CRotationEllipseData();
        }
    }
      
    CMovementEllipses movementEllipses = new CMovementEllipses();       //Parameters that define the camera translation and rotation ellipses        
    CEllipsoideData ellipsoideData = new CEllipsoideData();

    float t_translationEllipse;                    // Current t parameter that define camera position on the ellipse
    //Plane rotation angle
    float planeAngle;           // Current rotation angle    
    //Ellipse limits
    float minimunAllowedAxis;       // Minimum possible minor axis
    

    enum TGeometryType3
    {
        Type_I,
        Type_II,
        Type_III,
        Type_IV,
        Type_V,
        Type_VI,
    }
    TGeometryType3 geometryType3;
    
    //Control variables
    bool navigationInitialized = false;


    Vector3 extents;            // Store the product bounding box extents
    Vector2 fieldOfView;        // Store the camera field of View

    float k_lastvalidvalue;     // TODO Delete me

    public bool Init(Vector3 _extents, Vector3 cameraInitialPosition,/* Vector2 _fieldOfView,*/ out float minimumCameraDistance, out Vector3 pointToLook)
    {
        /////////////////////
        // Save parameters 
        /////////////////////
        extents = _extents;        

        //////////////////////////////////////////////////
        // Identify object geometry type. Long or flat
        //////////////////////////////////////////////////           
        geometryType3 = ClassifyObjectGeometry3(_extents);

        //Init ellipsoid
        CalculateEllipdoid(_extents, cameraInitialPosition);

        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        ///////////////////////////////////////////////////////////////                      
        movementEllipses.translation = CalculateInitialTranslationEllipseParameters_new(cameraInitialPosition);
        t_translationEllipse = CalculateTranslationEllipseInitialTParameter();    // Calculate the initial value of t parameter                   
        movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);

        ///////////////////////////////////
        // Initialize plane angle to 0
        ///////////////////////////////////
        planeAngle = 0.0f;
        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////        
        //minimunAllowedAxis = CalculateMinimunEllipse(_extents, movementEllipses.translation);       // Calculate the minimum ellipse semi-axes
        minimumCameraDistance = minimunAllowedAxis;                                                 // update out parameter

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
            return true;
        }
        else
        {
            Debug.Log("Error");
            return false;
        }
    }


    public void CalculateCameraPosition(float pseudoLatitudeVariation, float pseudoLongitudeVariation, float pseudoRadio, Vector2 _fieldOfView, out Vector3 cameraPlanePosition, out float planeRotation, out Vector3 pointToLook)
    {
        cameraPlanePosition = Vector3.zero;
        planeRotation = 0.0f;
        pointToLook = Vector3.zero;

        if (navigationInitialized)
        {
            // Update field of view
            fieldOfView = _fieldOfView;

            /////////////////////////////////////////////////////////
            // Apply pseudoLatitude and pseudoLongitude correction
            /////////////////////////////////////////////////////////
            pseudoLatitudeVariation = pseudoLatitudeVariation * CalculatePseudoLatitudeMappingFactor(fieldOfView.x) * CalculatePseudoLatitudeCorrectionFactor(fieldOfView.x);
            pseudoLongitudeVariation = pseudoLongitudeVariation * CalculatePseudoLongitudeMappingFactor(fieldOfView.y) * CalculatePseudoLongitudeCorrectionFactor(fieldOfView.y);


            Debug.Log(CalculatePseudoLongitudeMappingFactor(fieldOfView.x));
            Debug.Log(CalculatePseudoLongitudeCorrectionFactor(fieldOfView.x));


            /////////////////////////////////////////////////////
            // Mapping of parameters - Plane rotation parameter
            /////////////////////////////////////////////////////                
            if ((0 < t_translationEllipse) && (t_translationEllipse < Mathf.PI))
            {
                pseudoLongitudeVariation = -pseudoLongitudeVariation;   // Depends of the camera position, to avoid that the mouse behaviour change between front and back
            }
            planeAngle += pseudoLongitudeVariation;
            planeAngle = MathHom3r.NormalizeAngleInRad(planeAngle);
            movementEllipses.translation = CalculateTranslationEllipseParameters(planeAngle);

            /////////////////////////////////////////////////////
            // Mapping of parameters - Translation parameters
            /////////////////////////////////////////////////////                        
            CalculateNewEllipsoidAndRestOfEllipsesAfterRadialMovement(pseudoRadio);

            // Latitude - which is a translation movement on the camera plane1
            t_translationEllipse += pseudoLatitudeVariation;                    // Add to the current translation angle
            t_translationEllipse = MathHom3r.NormalizeAngleInRad(t_translationEllipse);   // Normalize new t angle 
            
            movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);

            //////////////////////////////////////
            // Calculate new camera position
            //////////////////////////////////////                        
            cameraPlanePosition = CalculateNewCameraPosition(t_translationEllipse);

            //////////////////////////////////////
            // Calculate new plane rotation
            //////////////////////////////////////
            planeRotation = pseudoLongitudeVariation;

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
        }
        else if ((extents.x >= extents.y) && (extents.y >= extents.z))
        {
            geometryType3 = TGeometryType3.Type_II;
        }
        else if ((extents.z >= extents.x) && (extents.x >= extents.y))
        {
            geometryType3 = TGeometryType3.Type_III;
        }
        else if ((extents.z >= extents.y) && (extents.y >= extents.x))
        {
            geometryType3 = TGeometryType3.Type_V;
        }
        else if ((extents.y >= extents.x) && (extents.x >= extents.z))
        {
            geometryType3 = TGeometryType3.Type_IV;
        }
        else if ((extents.y >= extents.z) && (extents.z >= extents.x))
        {
            geometryType3 = TGeometryType3.Type_VI;
        }
        return geometryType3;
    }



    private float CalculateA(float b, float c)
    {
        return 0.5f * (c + Mathf.Sqrt(MathHom3r.Pow2(c) + 4 * MathHom3r.Pow2(b)));
    }
    private float CalculateB(float a, float c)
    {
        return Mathf.Sqrt(MathHom3r.Pow2(a) - c * a);

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
    private void CalculateEllipdoid(Vector3 extents, Vector3 cameraPosition)
    {
        float cameraObjectDistance = Mathf.Abs(cameraPosition.z) - extents.z;

        if (geometryType3 == TGeometryType3.Type_I || geometryType3 == TGeometryType3.Type_II)
        {
            //ellipsoideData.semimajor_axis = TAxis.x;            // The semi-major axis of the ellipse on the XZ-plane is in the X-Axis
            //ellipsoideData.Ec = extents.x;                      // Evolute cusp will be in the limit of the object in X-axis
            ellipsoideData.SetEvoluteCups(extents.x, 0);        // Evolute cusp will be in the limit of the object in X-axis
            ellipsoideData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            ellipsoideData.radiousXAxis = CalculateA(ellipsoideData.radiousZAxis, extents.x);
            ellipsoideData.radiousYAxis = cameraObjectDistance + extents.y;
        }
        else if (geometryType3 == TGeometryType3.Type_V || geometryType3 == TGeometryType3.Type_VI)
        {
            //ellipsoideData.semimajor_axis = TAxis.z;    // The semi-major axis of the ellipse on the XZ-plane is in the Z-Axis
            //ellipsoideData.Ec = extents.z;              // Evolute cusp will be in the limit of the object in Z-axis
            ellipsoideData.SetEvoluteCups(0, extents.z);
            ellipsoideData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            ellipsoideData.radiousXAxis = CalculateB(ellipsoideData.radiousZAxis, extents.z);
            ellipsoideData.radiousYAxis = cameraObjectDistance + extents.y;
        }
        else if (geometryType3 == TGeometryType3.Type_III)
        {
            //ellipsoideData.semimajor_axis = TAxis.xz;    // The semi-major axis of the ellipse on the XZ-plane is in the Z-Axis or X-Axis
            //ellipsoideData.Ec = extents.z;              // Evolute cusp will be in the limit of the object in Z-axis
            ellipsoideData.SetEvoluteCups(extents.x, extents.z);
            ellipsoideData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            ellipsoideData.radiousXAxis = CalculateB(ellipsoideData.radiousZAxis, extents.z);
            ellipsoideData.radiousYAxis = cameraObjectDistance + extents.y;
        }
        else if (geometryType3 == TGeometryType3.Type_IV)
        {
            //ellipsoideData.semimajor_axis = TAxis.xz;    // The semi-major axis of the ellipse on the XZ-plane is in the Z-Axis or X-Axis
            //ellipsoideData.Ec = extents.x;                  // Evolute cusp will be in the limit of the object in Z-axis
            ellipsoideData.SetEvoluteCups(extents.x, extents.z);
            ellipsoideData.radiousZAxis = Mathf.Abs(cameraPosition.z);
            ellipsoideData.radiousXAxis = CalculateA(ellipsoideData.radiousZAxis, extents.x);
            ellipsoideData.radiousYAxis = cameraObjectDistance + extents.y;
        }    
    }

    /// <summary>    
    /// Calculate the initial parameters of the ellipse that the camera follows as a trajectory in its translational movements in the plane
    /// </summary>
    /// <param name="cameraPosition">Current camera position.</param>       
    private CTranslationEllipseData CalculateInitialTranslationEllipseParameters_new(Vector3 cameraPosition)
    {
        CTranslationEllipseData _ellipse = new CTranslationEllipseData(); ;
        _ellipse.radiousXAxis = ellipsoideData.radiousXAxis;
        _ellipse.radiousZAxis = ellipsoideData.radiousZAxis;
        _ellipse.evoluteCusp = ellipsoideData.GetEvoluteCusp(_ellipse.GetSemiMajorAxis());       

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
    private float CalculateMinimunEllipse(Vector3 extents, CEllipseData ellipse)
    {
        float minimunAxis = 0.0f;

        //// Polynomial Coefficients: x^3 + a1 * x^2 + a2 * x + a3 = 0
        //float a1 = -ellipse.Ec;
        //float a2;
        //if (geometryType == TGeometryType.Prolate)
        //{
        //    a2 = -(MathHom3r.Pow2(ellipse.Ec) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z));
        //}
        //else
        //{
        //    a2 = -(MathHom3r.Pow2(ellipse.Ec) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.x));
        //}
        //float a3 = Mathf.Pow(ellipse.Ec, 3);

        ////Calculate Q, R
        //float Q = (1 / 9.0f) * (3.0f * a2 - MathHom3r.Pow2(a1));
        //float R = (1 / 54.0f) * (9.0f * a1 * a2 - 27 * a3 - 2 * Mathf.Pow(a1, 3));
        ////Calculate Q^3 and R^2
        //float Q3 = Mathf.Pow(Q, 3);
        ////float R2 = Mathf.Pow(R, 2);
        ////Calculate D
        ////float D = Q3 + R2;

        //// D is always < 0 in our case
        ////if (D < 0)
        ////{            
        //float teta = Mathf.Acos(-R / Mathf.Sqrt(-Q3));
        //float x2 = -2.0f * Mathf.Sqrt(-Q) * Mathf.Cos((teta + (2.0f * Constants.Pi)) / 3.0f) - (a1 / 3.0f);

        ////}

        //if (geometryType == TGeometryType.Prolate)
        //{
        //    float b = Mathf.Sqrt(MathHom3r.Pow2(x2) - x2 * ellipse.Ec);
        //    minimunAxis = b;        // Long object return the b minimum of the ellipse
        //}
        //else
        //{
        //    minimunAxis = x2;       //Flat object return a minimum of the ellipse
        //}
        ////Debug.Log(minimunAxis);
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
    /// Calculate the parameters of the ellipse that the camera follows as a trajectory in its translational movements in the plane
    /// </summary>
    /// <param name="planeAngle"></param>
    /// <returns></returns>
    private CTranslationEllipseData CalculateTranslationEllipseParameters(float planeAngle)
    {
        CTranslationEllipseData newTranslationEllipse = new CTranslationEllipseData();

        float nume = ellipsoideData.radiousZAxis * ellipsoideData.radiousYAxis;
        float deno = MathHom3r.Pow2(ellipsoideData.radiousZAxis * Mathf.Sin(planeAngle)) + MathHom3r.Pow2(ellipsoideData.radiousYAxis * Mathf.Cos(planeAngle));
        float newSemiAxis = nume / Mathf.Sqrt(deno);

        newTranslationEllipse.radiousXAxis = ellipsoideData.radiousXAxis;
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

        _rotationEllipseData.radiousZAxis = Mathf.Abs(ellipsoideData.radiousZAxis * Mathf.Sin(t));
        _rotationEllipseData.radiousYAxis = Mathf.Abs(ellipsoideData.radiousYAxis * Mathf.Sin(t));
        
        return _rotationEllipseData;
    }

    /// <summary>
    /// Calculate the new ellipsoid and the new rotation and translation ellipses after a radial movement of the camera
    /// </summary>
    /// <param name="pseudoRadio"></param>
    private void CalculateNewEllipsoidAndRestOfEllipsesAfterRadialMovement(float pseudoRadio)
    {
        float new_z;
        
        new_z = ellipsoideData.radiousZAxis + pseudoRadio;

        CalculateEllipdoid(extents, new Vector3(0, 0, new_z));
        movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);

        //Update translation ellipse completly
        movementEllipses.translation = CalculateTranslationEllipseParameters(planeAngle);
        DrawReferenceEllipses();
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
            float evoluteCusps = ellipsoideData.GetEvoluteCusp(TAxis.x);

            intersectionXaxis = CalculateLinearInterpolation(intersectionXaxis, currentEllipse.evoluteCusp, evoluteCusps);          
            intersectionPoint = new Vector3(intersectionXaxis, 0, 0);
        }
        else
        {
            float intersectionZaxis = (a - (MathHom3r.Pow2(b) / a)) * Mathf.Sin(t);
            float evoluteCusps = ellipsoideData.GetEvoluteCusp(TAxis.z);

            intersectionZaxis = CalculateLinearInterpolation(intersectionZaxis, currentEllipse.evoluteCusp, evoluteCusps);            
            intersectionPoint = new Vector3(0, 0, intersectionZaxis);
        }

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

    

    /// <summary>
    /// Returns the parameters of the ellipse enclosed in the object, taking into account rotation of the plane
    /// </summary>
    /// <returns></returns>
    private CEllipseData GetEnclosedEllipseParameters()
    {
        CEllipseData rotatedEnclosedEllipseParemeter = new CEllipseData();
        
               
        if (extents.x > extents.z)
        {
            rotatedEnclosedEllipseParemeter.a = extents.x;
            rotatedEnclosedEllipseParemeter.b = extents.z;
        }
        else
        {
            rotatedEnclosedEllipseParemeter.a = extents.z;
            rotatedEnclosedEllipseParemeter.b = extents.x;
        }


        Debug.Log("extents.x: " + extents.x + " - extents.y: " + extents.y + " - extents.z: " + extents.z);
        Debug.Log("a: " + rotatedEnclosedEllipseParemeter.a + " b: " + rotatedEnclosedEllipseParemeter.b);

        return rotatedEnclosedEllipseParemeter;
    }

    /// <summary>
    /// Returns the pseudo latitude mapping factor, t in function of the ellipse arc.
    /// </summary>
    /// <param name="fieldOfView_rad"></param>
    /// <returns></returns>
    private float CalculatePseudoLatitudeMappingFactor(float fieldOfView_rad, bool enclosedEllipse = true)
    {
        float factor = 0f;

        float ai;
        float bi;
        
        CEllipseData rotatedEnclosedEllipseParameter = GetEnclosedEllipseParameters();
        ai = rotatedEnclosedEllipseParameter.a;

        bi = rotatedEnclosedEllipseParameter.b;
               
        float arco = Mathf.Sqrt(MathHom3r.Pow2(ai * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bi * Mathf.Cos(t_translationEllipse)));       
        factor = 1 / arco;

        return factor;
    }

    /// <summary>
    /// Returns pseudolatitude correction factor, make Δt independent of the distance between the camera and the object. 
    /// </summary>
    /// <param name="horizontalFieldOfViewAngle_rad">camera horizontal field of view angle</param>
    /// <returns></returns>
    private float CalculatePseudoLatitudeCorrectionFactor(float horizontalFieldOfViewAngle_rad)
    {
        float ai;
        float bi;

        CEllipseData rotatedEnclosedEllipseParemeter = GetEnclosedEllipseParameters();
        ai = rotatedEnclosedEllipseParemeter.a;
        bi = rotatedEnclosedEllipseParemeter.b;
              
        
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


        CSemiAxes _translationEllipseSemiAxes = movementEllipses.translation.GetSemiAxes();
        float aTranslationEllipse = _translationEllipseSemiAxes.a;
        float bTranslationEllipse = _translationEllipseSemiAxes.b;        

        float Mt0 = (aTranslationEllipse / bTranslationEllipse) * Mathf.Tan(t_translationEllipse);
        float D = ((MathHom3r.Pow2(aTranslationEllipse) / bTranslationEllipse) - bTranslationEllipse) * Mathf.Sin(t_translationEllipse);
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


    private float CalculatePseudoLongitudeMappingFactor(float fieldOfView_rad)
    {
        float factor = 0f;

        float ai;
        float bi;

        if (extents.z > extents.y)
        {
            ai = extents.z;
            bi = extents.y;
        } else
        {
            ai = extents.y;
            bi = extents.z;
        }
                

        float arco = Mathf.Sqrt(MathHom3r.Pow2(bi * Mathf.Cos(planeAngle)) + MathHom3r.Pow2(ai * Mathf.Sin(planeAngle)));
        factor = 1 / arco;

        return factor;
    }
    
    private float CalculatePseudoLongitudeCorrectionFactor(float verticalFieldOfViewAngle_rad)
    {
        // Calculate minimum circumference
        float ai;
        float bi;

        if (extents.z> extents.y)
        {
            ai = extents.z;
            bi = extents.y;
        }
        else
        {
            ai = extents.y;
            bi = extents.z;
        }
        
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


    /// <summary>Draw the ellipse that the camera follows as a trajectory in its translational movements in the plane.</summary>
    private void DrawTranslationTrajectory()
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(movementEllipses.translation.radiousXAxis, movementEllipses.translation.radiousZAxis);
        }
    }

    /// <summary>
    /// Draw the ellipse that the camera follows as a trajectory in its rotational movements.
    /// </summary>
    /// <param name="_cameraPlanePosition"></param>
    private void DrawRotationTrajectory(Vector3 _cameraPlanePosition)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            float radio = _cameraPlanePosition.magnitude;

            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(movementEllipses.rotation.radiousZAxis, movementEllipses.rotation.radiousYAxis, _cameraPlanePosition.x);            
        }
    }

    /// <summary>
    /// Draw a reference ellipses to help understand camera movements.
    /// </summary>
    private void DrawReferenceEllipses()
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(ellipsoideData.radiousXAxis, ellipsoideData.radiousZAxis);
            hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(ellipsoideData.radiousXAxis, ellipsoideData.radiousYAxis);
        }
    }
}
