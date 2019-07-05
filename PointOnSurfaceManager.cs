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
    public void CapturePointOnSurface(Vector3 mousePosition, GameObject rayCastedArea)
    {
        if ((pointCaptureManagerState == TPointOnSurfaceManagerState.capturing) && (rayCastedArea != null))
        {
            int productRootLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
            Vector3 clickPosition = Raycast(mousePosition, Camera.main, productRootLayer);          
            Vector3 pointLocal = rayCastedArea.transform.InverseTransformPoint(clickPosition);
                                               
            string areaId = rayCastedArea.GetComponent<ObjectStateManager>().areaID;
            //Emit event            
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.PointOnSurface_PointCaptureSuccess, pointLocal, areaId));
            hom3r.state.selectionBlocked = false;
            pointCaptureManagerState = TPointOnSurfaceManagerState.iddle;
        }        
    }

    public void DrawPointOnSurface(Vector3 pointLocalPosition, string areaID) {

        GameObject areaObj = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);

        //Draw point on surface
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.parent = areaObj.transform;
        sphere.transform.position = areaObj.transform.TransformPoint(pointLocalPosition);

    }

    
    /// <summary>Method that use Ray Casting technique</summary>
    private Vector3 Raycast(Vector3 mouseCurrentPosition, Camera _camera, int _layer)
    {
        
        // Convert mouse position from screen space to three-dimensional space
        // Build ray, directed from mouse position to “camera forward” way
        Ray ray = _camera.ScreenPointToRay(mouseCurrentPosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layer))
        {            
            return hit.point;   // Now, let’s determine intersected GameObject
        }
        else
        {
            return Vector3.zero;
        }
    }

}
