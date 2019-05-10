using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationHelper : MonoBehaviour {
    int segments;
    LineRenderer lineTranslationEllipse;
    LineRenderer lineRotationEllipse;
    GameObject helperCamera;
    LineRenderer helperCameraViewLine;

    // Use this for initialization
    void Awake () {

        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            segments = 160;
            foreach (Transform child in transform)
            {
                if (child.name == "HelperRotationTrajectory")
                {
                    lineRotationEllipse = child.GetComponentInChildren<LineRenderer>();
                }
                if (child.name == "HelperTranslationTrajectory")
                {
                    lineTranslationEllipse = child.GetComponentInChildren<LineRenderer>();
                }
                if (child.name == "HelperCamera")
                {
                    helperCamera = child.gameObject;
                    helperCameraViewLine = child.GetComponentInChildren<LineRenderer>();
                }
            }

            lineTranslationEllipse.material = new Material(Shader.Find("Sprites/Default"));
            lineTranslationEllipse.widthMultiplier = 0.2f;
            lineTranslationEllipse.positionCount = (segments + 1);
            lineTranslationEllipse.useWorldSpace = false;

            lineRotationEllipse.material = new Material(Shader.Find("Sprites/Default"));
            lineRotationEllipse.widthMultiplier = 0.2f;
            lineRotationEllipse.positionCount = (segments + 1);
            lineRotationEllipse.useWorldSpace = false;

            helperCameraViewLine.material = new Material(Shader.Find("Sprites/Default"));
            helperCameraViewLine.widthMultiplier = 0.2f;
            helperCameraViewLine.positionCount = 2;
            helperCameraViewLine.useWorldSpace = false;
        }
        else
        {
            foreach (Transform child in transform)
            {
                if (child.name == "HelperRotationTrajectory")
                {
                    child.gameObject.SetActive(false);
                }
                if (child.name == "HelperTranslationTrajectory")
                {
                    child.gameObject.SetActive(false);
                }
                if (child.name == "HelperCamera")
                {
                    child.gameObject.SetActive(false);
                }
            }
        } 
    }

    public void DrawTranslationEllipse(float xRadius, float zRadius)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            List<Vector3> points = new List<Vector3>();
            float x;
            float y = 0f;
            float z;

            float angle = 20f;

            for (int i = 0; i < (segments + 1); i++)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * xRadius;
                z = Mathf.Sin(Mathf.Deg2Rad * angle) * zRadius;

                //line.SetPosition(i, new Vector3(x, y, z));  
                points.Add(new Vector3(x, y, z));
                angle += (360f / segments);
            }
            lineTranslationEllipse.SetPositions(points.ToArray());
        }        
    }

    public void DrawRotationEllipse(float xRadius, float zRadius, float offset)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            //var points = new Vector3[segments + 1];
            List<Vector3> points = new List<Vector3>();
            float z;
            float x = 0f;
            float y;

            float angle = 20f;

            for (int i = 0; i < (segments + 1); i++)
            {
                z = Mathf.Cos(Mathf.Deg2Rad * angle) * xRadius;
                y = Mathf.Sin(Mathf.Deg2Rad * angle) * zRadius;
                x = offset;

                //line2.SetPosition(i, new Vector3(x, y, z));
                points.Add(new Vector3(x, y, z));
                angle += (360f / segments);
            }

            lineRotationEllipse.SetPositions(points.ToArray());
        }        
    }

    public void MoveCameraHelper(Vector3 cameraPosition, Vector3 pointToLook)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            // Move Camera
            helperCamera.transform.localPosition = cameraPosition;
            Vector3 pointToLook_world = hom3r.quickLinks.navigationSystemObject.transform.TransformPoint(pointToLook);
            helperCamera.transform.LookAt(pointToLook_world);

            // Draw look direction line        
            helperCameraViewLine.SetPosition(0, Vector3.zero);
            Vector3 poinToLook_local = helperCamera.transform.InverseTransformPoint(pointToLook_world);
            helperCameraViewLine.SetPosition(1, poinToLook_local);
        }        
    }
}
