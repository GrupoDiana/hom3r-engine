using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CEllipseData
{
    public float a;    // Current mayor axis of the ellipse
    public float b;    // Current minor axis of the ellipse
    public float c;    // Asymptote of the perpendiculars to the ellipse        
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
        translationEllipse.c = CalculateCParameter(_extents);                                           // Calculate the parameter c according to the geometry of the object and its dimensions.
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
        DrawCameraPosition(cameraInitialPosition, pointToLook);

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
            DrawCameraPosition(cameraPlanePosition, pointToLook);
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
            ellipse.a = 0.5f * (ellipse.c + Mathf.Sqrt(MathHom3r.Pow2(ellipse.c) + 4 * MathHom3r.Pow2(ellipse.b)));
        }
        else
        {
            ellipse.a = Mathf.Abs(cameraPosition.z);
            ellipse.b = Mathf.Sqrt(MathHom3r.Pow2(ellipse.a) - ellipse.a * ellipse.c);
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
        float a1 = -ellipse.c;
        float a2;
        if (geometryType == TGeometryType.Prolate) {
            a2 = -(MathHom3r.Pow2(ellipse.c) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z));
        }
        else
        {
            a2 = -(MathHom3r.Pow2(ellipse.c) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.x));
        }
        float a3 = Mathf.Pow(ellipse.c, 3);        
        
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
            float b = Mathf.Sqrt(MathHom3r.Pow2(x2) - x2 * ellipse.c);
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
        newEllipse.c = currentEllipse.c;

        if (geometryType == TGeometryType.Prolate)
        {
            newEllipse.b = currentEllipse.b + pseudoRadio;
            if (Mathf.Abs(newEllipse.b) < minimunAllowedAxis) { newEllipse.b = minimunAllowedAxis; }  // We can not closer that minimum
            newEllipse.a = 0.5f * (newEllipse.c + Mathf.Sqrt(MathHom3r.Pow2(newEllipse.c) + 4 * MathHom3r.Pow2(newEllipse.b)));
        }
        else
        {
            newEllipse.a = currentEllipse.a + pseudoRadio;
            if (Mathf.Abs(newEllipse.a) < minimunAllowedAxis) { newEllipse.a = minimunAllowedAxis; }  // We can not closer that minimum
            newEllipse.b = Mathf.Sqrt(MathHom3r.Pow2(newEllipse.a) - newEllipse.a * newEllipse.c);
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
    private Vector3 CalculatePointToLook(float t, CEllipseData currentEllipse)
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

    private void DrawCameraPosition(Vector3 _cameraPlanePosition, Vector3 _pointToLook)
    {
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().MoveCameraHelper(_cameraPlanePosition, _pointToLook);
    }


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
    enum TAxis { x, y, z};
    class CEllipseDataExtended: CEllipseData
    {
        public TAxis a_Axis;
        public TAxis b_Axis;
    }

    class CMovementEllipses {
        public CEllipseData translation;
        public CEllipseData rotation;

        public CMovementEllipses()
        {
            translation = new CEllipseData();
            rotation = new CEllipseData();
        }
    }


    class CFrameworkEllipses
    {
        public CEllipseDataExtended xz;
        public CEllipseDataExtended xy;

        public CFrameworkEllipses()
        {
            xz = new CEllipseDataExtended();
            xy = new CEllipseDataExtended();
        }
    }

    CMovementEllipses movementEllipses = new CMovementEllipses();       //Parameters that define the camera translation and rotation ellipses
    CFrameworkEllipses frameworkEllipses = new CFrameworkEllipses();      //Parameters that define the horizontal and vertical ellipses

    float t_translationEllipse;                    // Current t parameter that define camera position on the ellipse
    //Plane rotation angle
    float planeAngle;           // Current rotation angle    
    //Ellipse limits
    float minimunAllowedAxis;       // Minimum possible minor axis

    //Object Geometry classification
    enum TGeometryType2 {
        Type_I,  //X>Z>Y
        Type_II, //Z>Y>X
        Type_III, Type_IV};
    TGeometryType2 geometryType2;


    enum TGeometryType { Prolate, Oblate };    
    TGeometryType geometryType;
    

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
        //fieldOfView = _fieldOfView;

        //////////////////////////////////////////////////
        // Identify object geometry type. Long or flat
        //////////////////////////////////////////////////   
        //geometryComplexType = ClassifyObjectGeometry(_extents);         // Get if the object is flat or long
        geometryType2=ClassifyObjectGeometry2(_extents);

        //Init Framework ellipses 
        CalculateFrameworkEllipses(_extents, cameraInitialPosition);
        
        /////////////////////////////////////////////////////////////////
        // Get translation parameters from the initial camera position
        ///////////////////////////////////////////////////////////////              
        movementEllipses.translation    = CalculateInitialTranslationEllipseParameters(cameraInitialPosition);                                         // Calculate semi-axis of the ellipse a,b and c
        t_translationEllipse            = CalculateTranslationEllipseInitialTParameter(cameraInitialPosition, movementEllipses.translation);    // Calculate the initial value of t parameter           
        movementEllipses.rotation       = CalculateRotationEllipseParameters(t_translationEllipse);
        
        ///////////////////////////////////
        // Initialize plane angle to 0
        ///////////////////////////////////
        planeAngle = 0.0f;
        ////////////////////////////////////////////
        // Calculate the minimum radius possible
        ////////////////////////////////////////////        
        minimunAllowedAxis = CalculateMinimunEllipse(_extents, movementEllipses.translation);       // Calculate the minimum ellipse semi-axes
        minimumCameraDistance = minimunAllowedAxis;                                                 // update out parameter

        //////////////////////////////////////////////////
        // Calculate the point which camera has to look
        //////////////////////////////////////////////////        
        pointToLook = CalculatePointToLook(t_translationEllipse, movementEllipses.translation);


        //////////////////////////////////////
        // HELPER
        //////////////////////////////////////
        DrawTranslationTrajectory();                            // Draw the trajectory of the translation, for debug reasons   
        DrawRotationTrajectory(cameraInitialPosition);
        DrawCameraPosition(cameraInitialPosition, pointToLook);
        DrawFrameworkEllipses();

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
            //pseudoLatitudeVariation = pseudoLatitudeVariation * CalculatePseudoLatitudeCorrectionParameter(fieldOfView.x, movementEllipses.translation);
            pseudoLatitudeVariation = pseudoLatitudeVariation * CalculatePseudoLatitudeMappingFactor(fieldOfView.x) * CalculatePseudoLatitudeCorrectionFactor(fieldOfView.x);
            //pseudoLongitudeVariation = pseudoLongitudeVariation * CalculatePseudoLongitudeCorrectionParameter(fieldOfView.y, translationEllipse);
            Debug.Log(CalculatePseudoLatitudeMappingFactor(fieldOfView.x));
            Debug.Log(CalculatePseudoLatitudeCorrectionFactor(fieldOfView.x));

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
            //translationEllipse = CalculateNewEllipseSemiAxesParametersAfterRadialMovement(pseudoRadio, translationEllipse);
            CalculateEllipsesAfterRadialMovement(pseudoRadio);

            //pseudoLatitudeVariation = 0.0f; //TODO Delete me                                    
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
            DrawCameraPosition(cameraPlanePosition, pointToLook);
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
   

    private TGeometryType2 ClassifyObjectGeometry2(Vector3 extents)
    {
        geometryType = TGeometryType.Prolate;   //TO DO delete me


        TGeometryType2 temp = TGeometryType2.Type_I;
        if ((extents.x >= extents.z) && (extents.x >= extents.y))
        {
            temp = TGeometryType2.Type_I;
        }
        else if ((extents.z >= extents.x) && (extents.y >= extents.x))
        {
            temp = TGeometryType2.Type_II;
        }
        else if ((extents.z >= extents.x) && (extents.x >= extents.y))
        {
            temp = TGeometryType2.Type_III;
        }
        else if ((extents.x >= extents.z) && (extents.y >= extents.x))
        {
            temp = TGeometryType2.Type_IV;
        }
        return temp;
    }

    private float CalculateA(float b, float c)
    {
        return 0.5f * (c + Mathf.Sqrt(MathHom3r.Pow2(c) + 4 * MathHom3r.Pow2(b)));
    }
    private float CalculateB(float a, float c)
    {
        return Mathf.Sqrt(MathHom3r.Pow2(a) - c*a);

    }
    private float CalculateC(float a, float b)
    {
        return a - (MathHom3r.Pow2(b) / a);
    }

    /// <summary>
    /// Calculate the frame ellipses, these are the ellipses on the xz and xy plane that frame the projections of the object on those planes
    /// </summary>
    /// <param name="extents"></param>
    /// <param name="cameraPosition"></param>
    private void CalculateFrameworkEllipses(Vector3 extents, Vector3 cameraPosition)
    {
        //float cameraObjectDistance = (Mathf.Abs(cameraPosition.z) - extents.z) / extents.z;        
        //float cOD_1 = (Mathf.Abs(cameraPosition.z) - extents.z) / Mathf.Abs(cameraPosition.z);
        //float cOD_2 = Mathf.Abs(cameraPosition.z) - extents.z;
        float cameraObjectDistance2 = Mathf.Abs(cameraPosition.z) - extents.z;

        if (geometryType2 == TGeometryType2.Type_I) {
            // XZ Plane            
            frameworkEllipses.xz.c = extents.x;
            frameworkEllipses.xz.b_Axis = TAxis.z;
            frameworkEllipses.xz.a_Axis = TAxis.x;
            frameworkEllipses.xz.b = Mathf.Abs(cameraPosition.z);            
            frameworkEllipses.xz.a = CalculateA(frameworkEllipses.xz.b, frameworkEllipses.xz.c);
            // XY Plane                        
            frameworkEllipses.xy.b_Axis = TAxis.y;
            frameworkEllipses.xy.a_Axis = TAxis.x;
            frameworkEllipses.xy.b = cameraObjectDistance2 + extents.y;            
            frameworkEllipses.xy.a = frameworkEllipses.xz.a;            
            frameworkEllipses.xy.c = CalculateC(frameworkEllipses.xy.a, frameworkEllipses.xy.b);

            if (frameworkEllipses.xy.a < frameworkEllipses.xy.b)
            {             
                Debug.Log("frameworkEllipses.xy.a < frameworkEllipses.xy.b");
            }

        }
        else if (geometryType2 == TGeometryType2.Type_IV)
        {
            // XZ Plane            
            frameworkEllipses.xz.c = extents.x;
            frameworkEllipses.xz.b_Axis = TAxis.z;
            frameworkEllipses.xz.a_Axis = TAxis.x;
            frameworkEllipses.xz.b = Mathf.Abs(cameraPosition.z);
            frameworkEllipses.xz.a = CalculateA(frameworkEllipses.xz.b, frameworkEllipses.xz.c);
            // XY Plane            
            frameworkEllipses.xy.b_Axis = TAxis.x;
            frameworkEllipses.xy.a_Axis = TAxis.y;
            frameworkEllipses.xy.b = frameworkEllipses.xz.a;
            //frameworkEllipses.xy.a = (extents.y * cameraObjectDistance) + extents.y;            
            //float actual = (cameraObjectDistance + 1) * extents.y;
            //float option1 = extents.y / (1 - cOD_1);
            //float option2 = cOD_2 + extents.y;
            frameworkEllipses.xy.a = cameraObjectDistance2 + extents.y;

            frameworkEllipses.xy.c = CalculateC(frameworkEllipses.xy.a, frameworkEllipses.xy.b);

            if (frameworkEllipses.xy.a < frameworkEllipses.xy.b)
            {
                Debug.Log("frameworkEllipses.xy.a < frameworkEllipses.xy.b");
            }
        }

        else if (geometryType2 == TGeometryType2.Type_II)
        {
            // XZ Plane  
            frameworkEllipses.xz.a_Axis = TAxis.z;
            frameworkEllipses.xz.b_Axis = TAxis.x;
            frameworkEllipses.xz.c = extents.z;
            frameworkEllipses.xz.a = Mathf.Abs(cameraPosition.z);            
            frameworkEllipses.xz.b = CalculateB(frameworkEllipses.xz.a, frameworkEllipses.xz.c);
            // XY Plane
            frameworkEllipses.xy.a_Axis = TAxis.y;
            frameworkEllipses.xy.b_Axis = TAxis.x;            
            frameworkEllipses.xy.b = frameworkEllipses.xz.b;                        
            frameworkEllipses.xy.a = cameraObjectDistance2 + extents.y;

            if (frameworkEllipses.xy.a < frameworkEllipses.xy.b) {
                frameworkEllipses.xy.a = frameworkEllipses.xy.b;
                Debug.Log("frameworkEllipses.xy.a < frameworkEllipses.xy.b");
                //geometryType2 = TGeometryType2.Type_III;               
                //frameworkEllipses.xy.a_Axis = TAxis.x;
                //frameworkEllipses.xy.b_Axis = TAxis.y;
                //frameworkEllipses.xy.a = frameworkEllipses.xz.b;
                //frameworkEllipses.xy.b = extents.y + cameraObjectDistance2;

            }      // TODO Why?
            frameworkEllipses.xy.c = CalculateC(frameworkEllipses.xy.a, frameworkEllipses.xy.b);
        }
        else if (geometryType2 == TGeometryType2.Type_III)
        {
            // XZ Plane
            frameworkEllipses.xz.a_Axis = TAxis.z;
            frameworkEllipses.xz.b_Axis = TAxis.x;
            frameworkEllipses.xz.c = extents.z;
            frameworkEllipses.xz.a = Mathf.Abs(cameraPosition.z);
            frameworkEllipses.xz.b = CalculateB(frameworkEllipses.xz.a, frameworkEllipses.xz.c);            
            // XY Plane
            frameworkEllipses.xy.a_Axis = TAxis.x;
            frameworkEllipses.xy.b_Axis = TAxis.y;
            frameworkEllipses.xy.a = frameworkEllipses.xz.b;            
            frameworkEllipses.xy.b = extents.y + cameraObjectDistance2;
            frameworkEllipses.xy.c = CalculateC(frameworkEllipses.xy.a, frameworkEllipses.xy.b);
            if (frameworkEllipses.xy.a < frameworkEllipses.xy.b)
            {
                Debug.Log("frameworkEllipses.xy.a < frameworkEllipses.xy.b");
            }
        }
        
    }
    

    /// <summary>
    /// Calculate the initial b and a parameter of the ellipse based on the object geometry and initial camera position.
    /// Store results into b and a class parameters.
    /// </summary>
    /// <param name="cameraPosition">Current camera position.</param>
    private CEllipseData CalculateInitialTranslationEllipseParameters(Vector3 cameraPosition)
    {
        CEllipseData ellipse = new CEllipseData(); ;
        ellipse.b = frameworkEllipses.xz.b;
        ellipse.a = frameworkEllipses.xz.a;
        ellipse.c = frameworkEllipses.xz.c;
        return ellipse;       
    }

    /// <summary>
    /// Calculate the initial T parameter of the ellipse based on b/a and current camera position
    /// </summary>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>return the t parameter float value</returns>
    private float CalculateTranslationEllipseInitialTParameter(Vector3 cameraPosition, CEllipseData ellipse)
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

        // Polynomial Coefficients: x^3 + a1 * x^2 + a2 * x + a3 = 0
        float a1 = -ellipse.c;
        float a2;
        if (geometryType == TGeometryType.Prolate)
        {
            a2 = -(MathHom3r.Pow2(ellipse.c) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.z));
        }
        else
        {
            a2 = -(MathHom3r.Pow2(ellipse.c) + MathHom3r.Pow2(extents.y) + MathHom3r.Pow2(extents.x));
        }
        float a3 = Mathf.Pow(ellipse.c, 3);

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
            float b = Mathf.Sqrt(MathHom3r.Pow2(x2) - x2 * ellipse.c);
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

    private CEllipseDataExtended CalculateTranslationEllipseParameters(float pseudoLongitude)
    {
        CEllipseDataExtended newTranlationEllipse = new CEllipseDataExtended();
      
        if (geometryType2 == TGeometryType2.Type_I)
        {            
            newTranlationEllipse.a = frameworkEllipses.xz.a;
            float nume = frameworkEllipses.xz.b * frameworkEllipses.xy.b;
            float deno = MathHom3r.Pow2(frameworkEllipses.xz.b * Mathf.Sin(pseudoLongitude)) + MathHom3r.Pow2(frameworkEllipses.xy.b * Mathf.Cos(pseudoLongitude));
            newTranlationEllipse.b =  nume / Mathf.Sqrt(deno);            
        } else if (geometryType2 == TGeometryType2.Type_II) {            
            newTranlationEllipse.b = frameworkEllipses.xz.b;
            float nume = frameworkEllipses.xz.a * frameworkEllipses.xy.a;
            float deno = MathHom3r.Pow2(frameworkEllipses.xz.a * Mathf.Sin(pseudoLongitude)) + MathHom3r.Pow2(frameworkEllipses.xy.a * Mathf.Cos(pseudoLongitude));
            newTranlationEllipse.a =  nume / Mathf.Sqrt(deno);
        }
        else if (geometryType2 == TGeometryType2.Type_III)
        {            
            newTranlationEllipse.b = frameworkEllipses.xz.b;
            float nume = frameworkEllipses.xz.a * frameworkEllipses.xy.b;
            float deno = MathHom3r.Pow2(frameworkEllipses.xz.a * Mathf.Sin(pseudoLongitude)) + MathHom3r.Pow2(frameworkEllipses.xy.b * Mathf.Cos(pseudoLongitude));
            newTranlationEllipse.a = nume / Mathf.Sqrt(deno);

        }
        else if (geometryType2 == TGeometryType2.Type_IV)
        {         
            newTranlationEllipse.a = frameworkEllipses.xz.a;
            float nume = frameworkEllipses.xz.b * frameworkEllipses.xy.a;
            float deno = MathHom3r.Pow2(frameworkEllipses.xz.b * Mathf.Sin(pseudoLongitude)) + MathHom3r.Pow2(frameworkEllipses.xy.a * Mathf.Cos(pseudoLongitude));
            newTranlationEllipse.b = nume / Mathf.Sqrt(deno);
        }
        newTranlationEllipse.c = CalculateC(newTranlationEllipse.a, newTranlationEllipse.b);

        return newTranlationEllipse;
    }


    private CEllipseDataExtended CalculateRotationEllipseParameters(float t)
    {
        CEllipseDataExtended newRotationEllipse = new CEllipseDataExtended();

        if (geometryType2 == TGeometryType2.Type_I)
        {
            newRotationEllipse.a_Axis = TAxis.z;
            newRotationEllipse.b_Axis = TAxis.y;
            newRotationEllipse.a = Mathf.Abs(frameworkEllipses.xz.b * Mathf.Sin(t));
            newRotationEllipse.b = Mathf.Abs(frameworkEllipses.xy.b * Mathf.Sin(t));
        }
        else if (geometryType2 == TGeometryType2.Type_II)
        {
            newRotationEllipse.a_Axis = TAxis.z;
            newRotationEllipse.b_Axis = TAxis.y;
            newRotationEllipse.a = Mathf.Abs(frameworkEllipses.xz.a * Mathf.Sin(t));
            newRotationEllipse.b = Mathf.Abs(frameworkEllipses.xy.a * Mathf.Sin(t));
        }
        else if (geometryType2 == TGeometryType2.Type_III)
        {
            newRotationEllipse.a_Axis = TAxis.z;
            newRotationEllipse.b_Axis = TAxis.y;
            newRotationEllipse.a = Mathf.Abs(frameworkEllipses.xz.a * Mathf.Sin(t));
            newRotationEllipse.b = Mathf.Abs(frameworkEllipses.xy.b * Mathf.Sin(t));
        }
        else if (geometryType2 == TGeometryType2.Type_IV)
        {
            newRotationEllipse.a_Axis = TAxis.y;
            newRotationEllipse.b_Axis = TAxis.z;
            newRotationEllipse.a = Mathf.Abs(frameworkEllipses.xy.a * Mathf.Sin(t));
            newRotationEllipse.b = Mathf.Abs(frameworkEllipses.xz.b * Mathf.Sin(t));
        }
        return newRotationEllipse;
    }

    /// <summary>
    /// Calculate the b and a parameters of the ellipse based of a pseudoRadio movement.    
    /// </summary>
    /// <param name="pseudoRadio"></param>
    private void CalculateEllipsesAfterRadialMovement(float pseudoRadio)
    {
        float new_z;
        if (frameworkEllipses.xz.b_Axis == TAxis.z) { new_z = frameworkEllipses.xz.b;
        } else { new_z = frameworkEllipses.xz.a; }

        new_z = new_z + pseudoRadio;

        CalculateFrameworkEllipses(extents, new Vector3(0, 0, new_z));
        movementEllipses.rotation = CalculateRotationEllipseParameters(t_translationEllipse);
      
        //Update translation ellipse completly
        movementEllipses.translation = CalculateTranslationEllipseParameters(planeAngle);
        DrawFrameworkEllipses();
    }

    /// <summary>
    /// Calculate the new camera position in function of a,b and t of the ellipse
    /// </summary>
    /// <returns>Returns the new camera position</returns>
    private Vector3 CalculateNewCameraPosition(float t)
    {
        Vector3 cameraPositionOnPlane = Vector3.zero;     
        if (geometryType2 == TGeometryType2.Type_I)
        {
            cameraPositionOnPlane.x = movementEllipses.translation.a * Mathf.Cos(t);
            cameraPositionOnPlane.y = 0;
            cameraPositionOnPlane.z = movementEllipses.translation.b * Mathf.Sin(t);
        }
        else if (geometryType2 == TGeometryType2.Type_II)
        {
            cameraPositionOnPlane.x = movementEllipses.translation.b * Mathf.Cos(t);
            cameraPositionOnPlane.y = 0;
            cameraPositionOnPlane.z = movementEllipses.translation.a * Mathf.Sin(t);
        }
        else if (geometryType2 == TGeometryType2.Type_III)
        {
            cameraPositionOnPlane.x = movementEllipses.translation.b * Mathf.Cos(t);
            cameraPositionOnPlane.y = 0;
            cameraPositionOnPlane.z = movementEllipses.translation.a * Mathf.Sin(t);
        }
        else if (geometryType2 == TGeometryType2.Type_IV)
        {
            cameraPositionOnPlane.x = movementEllipses.translation.a * Mathf.Cos(t);
            cameraPositionOnPlane.y = 0;
            cameraPositionOnPlane.z = movementEllipses.translation.b * Mathf.Sin(t);
        }

        return cameraPositionOnPlane;
    }
    /// <summary>Calculate the point which camera has to look</summary>
    /// <returns>Return the point which camera has to look</returns>
    private Vector3 CalculatePointToLook(float t, CEllipseData currentEllipse)
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
        Vector3 P = CalculateNewCameraPosition(t_translationEllipse);
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
        if (radicand > 0f) { zQ = bMin * Mathf.Sqrt(radicand); }     // Sometimes it is <0 --> I don't know why
        //float zQ = bMin * Mathf.Sqrt(1 - MathHom3r.Pow2(xQ / aMin));

        if (P.z < 0) { zQ *= -1; }  //TODO Resolve this "chapuza"
        Vector3 Q = new Vector3(xQ, 0, zQ);
        //Debug.Log("Q: " + Q);

        float dPQ = Mathf.Sqrt((P - Q).sqrMagnitude);
        //Debug.Log(dPQ);

        //float rMin = MathHom3r.Max(extents);
        float k = 2 * dPQ * Mathf.Tan(fieldOfView_rad);
        //Debug.Log(k);
        float arco = Mathf.Sqrt(MathHom3r.Pow2(aMin * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bMin * Mathf.Cos(t_translationEllipse)));

        return k / arco;
    }

    private float CalculatePseudoLatitudeMappingFactor(float fieldOfView_rad)
    {
        float factor = 0f;

        float ai;
        float bi;

        if ((geometryType2 == TGeometryType2.Type_I) || (geometryType2 == TGeometryType2.Type_IV))
        {
            ai = extents.x;
            bi = extents.z;
        }
        else
        {
            ai = extents.z;
            bi = extents.x;
        }
        
        float arco = Mathf.Sqrt(MathHom3r.Pow2(ai * Mathf.Sin(t_translationEllipse)) + MathHom3r.Pow2(bi * Mathf.Cos(t_translationEllipse)));       
        factor = 1 / arco;

        return factor;
    }
    private float CalculatePseudoLatitudeCorrectionFactor(float fieldOfView_rad)
    {
        float ai;
        float bi;

        if ((geometryType2 == TGeometryType2.Type_I) || (geometryType2 == TGeometryType2.Type_IV))
        {
            ai = extents.x;
            bi = extents.z;
        }
        else
        {
            ai = extents.z;
            bi = extents.x;
        }

        
        float k = CalculateMappingCorrectionParameterK(fieldOfView_rad, ai, bi);
        return k;
    }

    private float CalculateMappingCorrectionParameterK(float fieldOfView_rad, float ai, float bi)
    {
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
        
        
        float Mt0 = (movementEllipses.translation.a / movementEllipses.translation.b) * Mathf.Tan(t_translationEllipse);
        float D = ((MathHom3r.Pow2(movementEllipses.translation.a)/ movementEllipses.translation.b) - movementEllipses.translation.b) * Mathf.Sin(t_translationEllipse);        
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
        k = 2 * dPQ * Mathf.Tan(fieldOfView_rad);
        
        if (float.IsNaN(k))
        {
            //Debug.Log("nan");
            k = k_lastvalidvalue;
        } else
        {
            k_lastvalidvalue = k;
        }
        return k;
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

    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawTranslationTrajectory()
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            //if (geometryType == TGeometryType.Prolate)
            //{
            //    hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(currentEllipse.a, currentEllipse.b);
            //}
            //else
            //{
            //    hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(currentEllipse.b, currentEllipse.a);
            //}

            if (geometryType2 == TGeometryType2.Type_I)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(movementEllipses.translation.a, movementEllipses.translation.b);
            }
            else if (geometryType2 == TGeometryType2.Type_II)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(movementEllipses.translation.b, movementEllipses.translation.a);
            }
            else if (geometryType2 == TGeometryType2.Type_III)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(movementEllipses.translation.b, movementEllipses.translation.a);
            }
            else if (geometryType2 == TGeometryType2.Type_IV)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawTranslationEllipse(movementEllipses.translation.a, movementEllipses.translation.b);
            }
        }
    }

    /// <summary>Draw the translation trajectory, just for support in the editor</summary>
    private void DrawRotationTrajectory(Vector3 _cameraPlanePosition)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            float radio = _cameraPlanePosition.magnitude;
            
            if (geometryType2 == TGeometryType2.Type_I) {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(movementEllipses.rotation.a, movementEllipses.rotation.b, _cameraPlanePosition.x);
            }
            else if (geometryType2 == TGeometryType2.Type_II)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(movementEllipses.rotation.a, movementEllipses.rotation.b, _cameraPlanePosition.x);
            }
            else if (geometryType2 == TGeometryType2.Type_III)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(movementEllipses.rotation.a, movementEllipses.rotation.b, _cameraPlanePosition.x);
            }
            else if (geometryType2 == TGeometryType2.Type_IV)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawRotationEllipse(movementEllipses.rotation.b, movementEllipses.rotation.a, _cameraPlanePosition.x);
            }            
        }
    }

    private void DrawFrameworkEllipses()
    {
        if (hom3r.state.platform == THom3rPlatform.Editor) {

            if (geometryType2 == TGeometryType2.Type_I)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(frameworkEllipses.xz.a, frameworkEllipses.xz.b);
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(frameworkEllipses.xy.a, frameworkEllipses.xy.b);
            }
            else if (geometryType2 == TGeometryType2.Type_II)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(frameworkEllipses.xz.b, frameworkEllipses.xz.a);
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(frameworkEllipses.xy.b, frameworkEllipses.xy.a);
            }
            else if (geometryType2 == TGeometryType2.Type_III)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(frameworkEllipses.xz.b, frameworkEllipses.xz.a);
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(frameworkEllipses.xy.a, frameworkEllipses.xy.b);
            }
            else if (geometryType2 == TGeometryType2.Type_IV)
            {
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawHorizontalFrameworkEllipse(frameworkEllipses.xz.a, frameworkEllipses.xz.b);
                hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().DrawVerticalFrameworkEllipse(frameworkEllipses.xy.b, frameworkEllipses.xy.a);
            }
        }
    }

    private void DrawCameraPosition(Vector3 _cameraPlanePosition, Vector3 _pointToLook)
    {
        hom3r.quickLinks.navigationSystemObject.GetComponentInChildren<NavigationHelper>().MoveCameraHelper(_cameraPlanePosition, _pointToLook);
    }
}
