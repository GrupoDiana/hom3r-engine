using UnityEngine;
using System.Collections;

public class GizmoManager : MonoBehaviour
{
    public Camera GizmoCamera;
    private float initialRectWidth;
    private float initialRectHeight;
    private int initialScreenWidth;
    private int initialScreenHeight;

    // Label hiding
    private float HIDE_OFFSET = 20.0f;
    private float gizmoLabelHideDistance;

    // Text positioning
    private float DISTANCE_TO_CENTER;
    private Vector3 leftExtents;
    private Vector3 rightExtents;
    private Vector3 topExtents;
    private Vector3 bottomExtents;
    private Vector3 frontExtents;
    private Vector3 backExtents;
    private float EPSILON_PROJECTION = 0.6f;

    // set the initial aspect ratio, and setup reference camera
    void Start()
    {
        // Get gizmo camera and initial viewport
        GizmoCamera = GameObject.FindGameObjectWithTag("GizmoCamera").GetComponent<Camera>();
        initialRectWidth = GizmoCamera.rect.width;
        initialRectHeight = GizmoCamera.rect.height;
        initialScreenWidth = Screen.width;
        initialScreenHeight = Screen.height;

        // Compute total distance from axis edge to centre of gizmo 3D model
        float centerHalfX = GameObject.Find("Center_Cube").GetComponent<MeshRenderer>().bounds.extents.x;
        float axisX = GameObject.Find("X_ToRight").GetComponent<MeshRenderer>().bounds.extents.x * 2.0f;
        DISTANCE_TO_CENTER = centerHalfX + axisX;

        // We need to pre-compute text bounds, because Unity computes axis-aligned bounds and we are going to rotate text 
        leftExtents = GameObject.Find("Text_Left").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.extents;
        rightExtents = GameObject.Find("Text_Right").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.extents;
        topExtents = GameObject.Find("Text_Top").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.extents;
        bottomExtents = GameObject.Find("Text_Bottom").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.extents;
        frontExtents = GameObject.Find("Text_Front").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.extents;
        backExtents = GameObject.Find("Text_Back").GetComponent<TextMesh>().GetComponent<Renderer>().bounds.extents;
    }

// scale object relative to distance from camera plane
void Update()
    {
        UpdateGizmo();
    }

    private float AngleBetween(Vector3 vec1, Vector3 vec2)
    {
        float angle = Vector3.Angle(vec1, vec2);
        angle = 180.0f - angle;                                       
        //angle *= Mathf.Sign(Vector3.Cross(vec1, vec2).y);
        return Mathf.Deg2Rad * angle;
    }

    private Vector3 ProjectVectorOnPlane(Vector3 v, Vector3 planeNormal)
    {
        planeNormal.Normalize();
        float distance = -Vector3.Dot(planeNormal.normalized, v);
        return v + planeNormal* distance;
    }

    public void MoveAndHideOneGizmoLabel(string labelName, Vector3 direction, Vector3 extents)
    {
        GameObject textObject = GameObject.Find(labelName);
        GameObject gizmoCenter = GameObject.Find("Center_Cube");        

    // ROTATE:

        // Look towards camera             
        textObject.transform.LookAt(GizmoCamera.transform, GizmoCamera.transform.up);
        textObject.transform.Rotate(0.0f, 180.0f, 0.0f);

    // TRANSLATE:

        // Compute where text should be in world space
        Vector3 goalPosition = gizmoCenter.transform.position + direction * DISTANCE_TO_CENTER;

        // Centre text in screen space, wrt goal position
        Vector3 textScreen = GizmoCamera.WorldToScreenPoint(goalPosition);        
        textScreen.x -= extents.x;
        textScreen.y += extents.y;

        // Project direction onto camera forward-right plane
        Vector3 projectedDirection = ProjectVectorOnPlane(direction, GizmoCamera.transform.up);

        // Compute angle between projected direction and forward vector
        float angle;
        if (projectedDirection.magnitude > EPSILON_PROJECTION)
            angle = AngleBetween(GizmoCamera.transform.forward, projectedDirection);
        else
            angle = 0.0f;

        // Compute X and Y increments depending on angle with camera
        float incX = extents.x * Mathf.Abs(Mathf.Sin(angle));
        float incY = extents.y * Mathf.Abs(Mathf.Cos(angle));

        // Apply X and Y increments, depending on relative position wrt gizmo centre
        Vector3 gizmoCenterScreen = GizmoCamera.WorldToScreenPoint(gizmoCenter.transform.position);
        if (textScreen.x < gizmoCenterScreen.x)
            textScreen.x -= incX;
        else
            textScreen.x += incX;
        if (textScreen.y < gizmoCenterScreen.y)
            textScreen.y -= incY;
        else
            textScreen.y += incY;

        // Go back to world space
        textObject.transform.position = GizmoCamera.ScreenToWorldPoint(textScreen);                

        //// Compute increment along axis depending on text size, using triangle similarity
        //Vector3 axisScreen = GizmoCamera.WorldToScreenPoint(textObject.transform.position);
        //Vector3 centerScreen = GizmoCamera.WorldToScreenPoint(gizmoCenter.transform.position);
        //float y = Mathf.Abs(axisScreen.y - centerScreen.y);
        //float x = Mathf.Abs(axisScreen.x - centerScreen.x);
        //float increment;
        //if (y > x)
        //    increment = (DISTANCE_TO_CENTER * textBounds.extents.y) / y;                    
        //else
        //    increment = (DISTANCE_TO_CENTER * textBounds.extents.x) / x;
        //// Limit increment
        //if (increment > textBounds.extents.x * 2.0f)
        //    increment = textBounds.extents.x * 2.0f;

    // HIDE:

        // Hide and show depending on distance to camera
        if (Vector3.Distance(textObject.transform.position, GizmoCamera.transform.position) > gizmoLabelHideDistance)
        {
            Renderer textRender = textObject.GetComponent<Renderer>();
            textRender.enabled = false;
        }
        else
        {
            Renderer textRender = textObject.GetComponent<Renderer>();
            textRender.enabled = true;
        }

    }

    public void UpdateGizmo()
    {
        // Change viewport depending on screen size, to keep the gizmo always visible in all proportions without scaling
        Rect newRect = GizmoCamera.rect;
        newRect.width = (float)initialScreenWidth / (float)Screen.width * initialRectWidth;
        newRect.height = (float)initialScreenHeight / (float)Screen.height * initialRectHeight;
        GizmoCamera.rect = newRect;
        transform.position = GizmoCamera.ScreenToWorldPoint(new Vector3(GizmoCamera.pixelWidth / 2, GizmoCamera.pixelHeight / 2, GizmoCamera.nearClipPlane + 1100.0f));

        // Compute hide distance for the current viewport
        GameObject gizmoCenter = GameObject.Find("Center_Cube");
        gizmoLabelHideDistance = Vector3.Distance(gizmoCenter.transform.position, GizmoCamera.transform.position);
        gizmoLabelHideDistance += HIDE_OFFSET;

        // Move and hide gizmo labels
        MoveAndHideOneGizmoLabel("Text_Top", new Vector3 (0.0f, 1.0f, 0.0f), topExtents);
        MoveAndHideOneGizmoLabel("Text_Bottom", new Vector3(0.0f, -1.0f, 0.0f), bottomExtents);
        MoveAndHideOneGizmoLabel("Text_Left", new Vector3(-1.0f, 0.0f, 0.0f), leftExtents);
        MoveAndHideOneGizmoLabel("Text_Right", new Vector3(1.0f, 0.0f, 0.0f), rightExtents);
        MoveAndHideOneGizmoLabel("Text_Back", new Vector3(0.0f, 0.0f, 1.0f), backExtents);
        MoveAndHideOneGizmoLabel("Text_Front", new Vector3(0.0f, 0.0f, -1.0f), frontExtents);        
    }    
}