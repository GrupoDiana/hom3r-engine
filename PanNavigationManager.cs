using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanNavigatioManager
{

    Vector3 extentsVector;
    Vector2 fielOfViewVector;
    float cameraMinimumDistance;

    public void Init(Vector3 _extentsVector, /*Vector2 _fielOfViewVector,*/ float _cameraMinimumDistance)
    {
        this.extentsVector = _extentsVector;
        //this.fielOfViewVector = _fielOfViewVector;
        this.cameraMinimumDistance = _cameraMinimumDistance;
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
        //Vector3 directionXVector = new Vector3(-1.0f, 0.0f, 0.0f);
        //Camera movement must be in the opposite direction that the mouse with an offset
        Vector3 incrementXVector = directionXVector * pseudoMouseMovementX * CalculateCorrectionParameterX();

        ///////
        // Y
        ///////
        Vector3 directionYVector = -1.0f * Camera.main.transform.up;// - new Vector3(0, Camera.main.transform.localRotation.y, 0);
        //Debug.Log(directionYVector);
        //Vector3 directionYVector = new Vector3(0.0f, -1.0f, 0.0f);
        //Get camera X axis without rotation        
        Vector3 incrementYVector = directionYVector * pseudoMouseMovementY * CalculateCorrectionParameterY();


        //Move plane        
        newPlanePosition = currentPlanePosition + incrementXVector + incrementYVector;
        



        return newPlanePosition;

    }

    private float CalculateCorrectionParameterX()
    {
        float k;        
        //Vector3 cameraPositionInPlane = new Vector3(Camera.main.transform.position.x, 0.0f, Camera.main.transform.position.z);
        Vector3 cameraPositionInPlane = Camera.main.transform.position;
        float distanceObject = cameraPositionInPlane.magnitude - cameraMinimumDistance;   // One of this should work
        //float distanceObject = cameraPositionInPlane.magnitude - extentsVector.z;         // One of this should work        
        k = 2 * distanceObject * Mathf.Tan(fielOfViewVector.x);

        //Debug.Log(k);

        return k;

    }

    private float CalculateCorrectionParameterY()
    {
        float k;
        //Vector3 cameraPositionInPlane = new Vector3(Camera.main.transform.position.x, 0.0f, Camera.main.transform.position.z);
        Vector3 cameraPositionInPlane = Camera.main.transform.position;
        float distanceObject = cameraPositionInPlane.magnitude - cameraMinimumDistance;   // One of this should work
        //float distanceObject = cameraPositionInPlane.magnitude - extentsVector.z;         // One of this should work        
        k = 2 * distanceObject * Mathf.Tan(fielOfViewVector.y);

        //Debug.Log(k);

        return k;

    }
}
