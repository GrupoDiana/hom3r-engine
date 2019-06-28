using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TPointOnSurfaceManagerState { iddle, capturing, editing };

public class PointOnSurfaceManager : MonoBehaviour
{
    TPointOnSurfaceManagerState pointCaptureManagerState;

    void Awake()
    {
        pointCaptureManagerState = TPointOnSurfaceManagerState.iddle;
    }



    /// <summary>
    /// Start point capture
    /// </summary>
    public void StartPointCapture()
    {
        hom3r.state.selectionBlocked = true;
        pointCaptureManagerState = TPointOnSurfaceManagerState.capturing;
    }

    
    public bool GetPointCapturingActivated() { return this.pointCaptureManagerState == TPointOnSurfaceManagerState.capturing; }

    /// <summary>
    /// Capture a point into the surface of an area
    /// </summary>
    /// <param name="clickPosition"></param>
    /// <param name="rayCastedArea"></param>
    public void CapturePointOnSurface(Vector3 clickPosition, GameObject rayCastedArea)
    {
        if ((pointCaptureManagerState == TPointOnSurfaceManagerState.capturing) && (rayCastedArea != null))
        {
            Debug.Log(clickPosition);

            Vector3 pointlocal = rayCastedArea.transform.InverseTransformPoint(clickPosition);
            Debug.Log(clickPosition);

            string areaId = rayCastedArea.GetComponent<ObjectStateManager>().areaID;
            //Emit event

        }        
    }

    public void DrawPointOnSurface(Vector3 clickPosition, string areaID) {

        GameObject areaObj = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);

        //Draw point on surface
    }

    
}
