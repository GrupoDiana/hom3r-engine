using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanNavigatioManager
{
    Vector3 initialPlanePosition;
    Vector3 extentsVector;
    Vector2 fielOfViewVector;
    float cameraMinimumDistance;

    public void Init(Vector3 _extentsVector, /*Vector2 _fielOfViewVector,*/ float _cameraMinimumDistance, Vector3 _currentPlanePosition)
    {
        this.extentsVector = _extentsVector;
        //this.fielOfViewVector = _fielOfViewVector;
        this.cameraMinimumDistance = _cameraMinimumDistance;
        this.initialPlanePosition = _currentPlanePosition;
    }


    public Vector3 CalculatePlanePosition(float pseudoMouseMovementX, float pseudoMouseMovementY, Vector3 currentPlanePosition, Vector2 _fielOfViewVector)
    {
        Vector3 newPlanePosition;

        // Update field of view
        this.fielOfViewVector = _fielOfViewVector;     

        ///////
        // X
        ///////
        //Get camera X axis without rotation
        Vector3 directionXVector = -1.0f * Camera.main.transform.right;// - new Vector3(Camera.main.transform.localRotation.x, 0, 0);        
        //Camera movement must be in the opposite direction that the mouse with an offset
        Vector3 incrementXVector = directionXVector * pseudoMouseMovementX * CalculateCorrectionParameterX();

        ///////
        // Y
        ///////
        Vector3 directionYVector = -1.0f * Camera.main.transform.up;// - new Vector3(0, Camera.main.transform.localRotation.y, 0);        
        //Get camera X axis without rotation        
        Vector3 incrementYVector = directionYVector * pseudoMouseMovementY * CalculateCorrectionParameterY();
        //Move plane        
        newPlanePosition = currentPlanePosition + incrementXVector + incrementYVector;

        newPlanePosition = CalculateLimitPosition(currentPlanePosition, newPlanePosition);        

        return newPlanePosition;

    }

    public Vector3 ResetPlanePosition()
    {
        return initialPlanePosition;
    }
        
    private Vector3 CalculateLimitPosition(Vector3 _currentPlanePosition, Vector3 _newPlanePositionCandidate)
    {
        Vector3 newPlanePosition = _newPlanePositionCandidate;

        Vector3 limit1 = initialPlanePosition - extentsVector;
        Vector3 limit2 = initialPlanePosition + extentsVector;


        if ((newPlanePosition.x < limit1.x) || (newPlanePosition.x > limit2.x)) { newPlanePosition.x = _currentPlanePosition.x; }
        if ((newPlanePosition.y < limit1.y) || (newPlanePosition.y > limit2.y)) { newPlanePosition.y = _currentPlanePosition.y; }
        if ((newPlanePosition.z < limit1.z) || (newPlanePosition.z > limit2.z)) { newPlanePosition.z = _currentPlanePosition.z; }

        return newPlanePosition;
    }


    private float CalculateCorrectionParameterX()
    {
        float k;                
        Vector3 cameraPositionInPlane = Camera.main.transform.position;
        float inscribedCircunfereRadius = GetInscribedCircunferenceRadiousX();

        //
        float distanceObject = cameraPositionInPlane.magnitude - inscribedCircunfereRadius;   // One of this should work                
        k = 2 * distanceObject * Mathf.Tan(fielOfViewVector.x);

        // When there are no limit in the maximun zoom I need to avoid a too low interaction speed.
        // when the camera is to close to the object k is to low
        if (k < 10) { k = 10f; }        // TODO this is a magic number
        //Debug.Log(k);            
        return k;
    }

    private float CalculateCorrectionParameterY()
    {
        float k;        
        Vector3 cameraPositionInPlane = Camera.main.transform.position;
        float inscribedCircunfereRadius = GetInscribedCircunferenceRadiousY();

        float distanceObject = cameraPositionInPlane.magnitude - inscribedCircunfereRadius;   // One of this should work                               
        k = 2 * distanceObject * Mathf.Tan(fielOfViewVector.y);
        
        if (k < 10) { k = 10f; }
        
        return k;
    }

    private float GetInscribedCircunferenceRadiousX()
    {
        float rMin = MathHom3r.Max(extentsVector.x, extentsVector.z);
        return rMin;
    }
    private float GetInscribedCircunferenceRadiousY()
    {
        float rMin = MathHom3r.Max(extentsVector.y, extentsVector.z);
        return rMin;
    }
}
