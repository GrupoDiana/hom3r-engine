using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//public enum THom3rPointCaptureMode { iddle, capturing, editing };

public class PointOnSurfaceManager : MonoBehaviour
{
    //TPointOnSurfaceManagerState currentPointCaptureMode;
    string specificAreaId;
    GameObject specificAreaGO;

    void Awake()
    {
        //currentPointCaptureMode = TPointOnSurfaceManagerState.iddle;
        specificAreaGO = null;
    }



    /// <summary>
    /// Start point capture
    /// </summary>
    public void StartPointCapture()
    {
        hom3r.state.selectionBlocked = true;
        hom3r.state.currentMode = THom3rMode.capturing_surface_point;
        hom3r.state.currentPointCaptureMode = THom3rPointCaptureMode.capturing;        
        //hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.PointOnSurface_PointCaptureBegin));
        this.SendMessageToUI("Please, select a point into the object surface..");
    }

    public void StartPointCapture(string _areaId)
    {        
        specificAreaId = _areaId;
        specificAreaGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(specificAreaId);
        if (specificAreaGO == null)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.PointOnSurface_PointCaptureError));
            Debug.Log("AREA NOT VALID");
        } else
        {
            this.StartPointCapture();
        }        
    }

    public bool GetPointCapturingActivated() { return hom3r.state.currentPointCaptureMode == THom3rPointCaptureMode.capturing; }

    /// <summary>
    /// Capture a point into the surface of an area
    /// </summary>
    /// <param name="clickPosition"></param>
    /// <param name="rayCastedArea"></param>
    public void CapturePointOnSurface(Vector3 mousePosition, GameObject rayCastedArea)
    {
        if (rayCastedArea == null) { return; }
        if (hom3r.state.currentPointCaptureMode != THom3rPointCaptureMode.capturing) { return; }
        if ((specificAreaGO == null) || (specificAreaGO != rayCastedArea)) { return; }
                        
        int productRootLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);
        Vector3 clickPosition = Raycast(mousePosition, Camera.main, productRootLayer);          
        Vector3 pointLocal = rayCastedArea.transform.InverseTransformPoint(clickPosition);
                                               
        string areaId = rayCastedArea.GetComponent<ObjectStateManager>().areaID;
        //Emit event            
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.PointOnSurface_PointCaptureSuccess, pointLocal, areaId));
        hom3r.state.selectionBlocked = false;
        hom3r.state.currentMode = THom3rMode.idle;
        hom3r.state.currentPointCaptureMode = THom3rPointCaptureMode.iddle;
        //hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.PointOnSurface_PointCaptureEnd));
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


    void SendMessageToUI(string _message)
    {
        SendMessageToUI(_message, 0.0f);
    }
    void SendMessageToUI(string _message, float _time)
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ModelManagement_ShowMessage, _message, _time));
        Debug.Log(_message);
    }
}
