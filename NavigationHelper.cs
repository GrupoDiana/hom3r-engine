﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationHelper : MonoBehaviour {

    enum TPlanes { XZ, XY, YZ };

    int segments;
    LineRenderer lineTranslationEllipse;
    LineRenderer lineRotationEllipse;
    LineRenderer lineHorizontalFrameworkEllipse;
    LineRenderer lineVerticalFrameworkEllipse;
    GameObject helperCamera;
    LineRenderer helperCameraViewLine;
    GameObject navigationAssistants;
    GameObject plane;

    // Use this for initialization
    void Awake () {

        if (hom3r.state.platform == THom3rPlatform.Editor)
        {                       
            foreach (Transform child in transform)
            {
                //if (child.name == "HelperRotationTrajectory")
                //{
                //    lineRotationEllipse = child.GetComponentInChildren<LineRenderer>();
                //}
                if (child.name == "HelperTranslationTrajectory")
                {
                    lineTranslationEllipse = child.GetComponentInChildren<LineRenderer>();
                }
                if (child.name == "HelperCamera")
                {
                    helperCamera = child.gameObject;
                    helperCameraViewLine = child.GetComponentInChildren<LineRenderer>();
                }
                if (child.name == "Plane")
                {
                    plane = child.gameObject;
                }
            }

            navigationAssistants = GameObject.Find("NavigationAssistants");
            foreach (Transform child in navigationAssistants.transform)
            {
                if (child.name == "HelperRotationTrajectory")
                {
                    lineRotationEllipse = child.GetComponentInChildren<LineRenderer>();
                }
                if (child.name == "HelperFrameworkEllipseH")
                {
                    lineHorizontalFrameworkEllipse = child.GetComponentInChildren<LineRenderer>();
                }
                if (child.name == "HelperFrameworkEllipseV")
                {
                    lineVerticalFrameworkEllipse = child.GetComponentInChildren<LineRenderer>();
                }
            }


            segments = 3600;
            InitLineRenderer(lineTranslationEllipse, 1.2f, segments);
            InitLineRenderer(lineRotationEllipse, 1.2f, segments);
            InitLineRenderer(helperCameraViewLine, 1f, 1);
            InitLineRenderer(lineHorizontalFrameworkEllipse, 0.9f, segments);
            InitLineRenderer(lineVerticalFrameworkEllipse, 0.9f, segments);
        
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
                if (child.name == "HelperFrameworkEllipseH") { child.gameObject.SetActive(false); }
                if (child.name == "HelperFrameworkEllipseV") { child.gameObject.SetActive(false); }
            }
        } 
    }

    private void InitLineRenderer(LineRenderer lineRenderer, float widthMultiplier, int positions)
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = widthMultiplier;
        lineRenderer.positionCount = (positions + 1);
        lineRenderer.useWorldSpace = false;
    }


    public void InitNavigationAssistantsPosition(Vector3 position)
    {
        navigationAssistants.transform.position = position;
    }

    public void SetBiggerLines()
    {
        lineTranslationEllipse.widthMultiplier += 0.1f;
        lineRotationEllipse.widthMultiplier += 0.1f;
        lineHorizontalFrameworkEllipse.widthMultiplier +=0.1f;
        lineVerticalFrameworkEllipse.widthMultiplier += 0.1f;
        helperCameraViewLine.widthMultiplier += 0.1f;
    }

    public void SetSmallerLines()
    {
        lineTranslationEllipse.widthMultiplier -=  0.1f;
        lineRotationEllipse.widthMultiplier -= 0.1f;
        lineHorizontalFrameworkEllipse.widthMultiplier -= 0.1f;
        lineVerticalFrameworkEllipse.widthMultiplier -=  0.1f;
        helperCameraViewLine.widthMultiplier -= 0.1f;
    }

    public void SetBiggerCamera()
    {
        helperCamera.transform.localScale = new Vector3 (helperCamera.transform.localScale.x + 0.2f, helperCamera.transform.localScale.y + 0.2f, helperCamera.transform.localScale.z + 0.2f);
    }

    public void SetSmallerCamera()
    {
        helperCamera.transform.localScale = new Vector3(helperCamera.transform.localScale.x - 0.2f, helperCamera.transform.localScale.y - 0.2f, helperCamera.transform.localScale.z - 0.2f);
    }

    ////////////////////////////////
    // Draw Ellipses Methods
    ////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xRadius"></param>
    /// <param name="zRadius"></param>
    public void DrawTranslationEllipse(float xRadius, float zRadius)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            DrawEllipse(xRadius, zRadius, 0f, TPlanes.XZ, lineTranslationEllipse);
        }        
    }
  
    public void DrawRotationEllipse(float zRadius, float yRadius, float offset)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)

        {
            DrawEllipse(zRadius, yRadius, -offset, TPlanes.YZ, lineRotationEllipse);            
        }
    }

    public void DrawHorizontalFrameworkEllipse(float xRadius, float zRadius)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)

        {
            DrawEllipse(xRadius, zRadius, 0f, TPlanes.XZ, lineHorizontalFrameworkEllipse);
        }
    }

    public void DrawVerticalFrameworkEllipse(float xRadius, float yRadius)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            DrawEllipse(xRadius, yRadius, 0f, TPlanes.XY, lineVerticalFrameworkEllipse);
        }
    }

    public void MoveCameraHelper(Vector3 cameraPosition)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            // Move Camera
            helperCamera.transform.localPosition = cameraPosition;            
        }        
    }

    public void SetCamaraOrientationHelper(Vector3 pointToLookWorld)
    {
        if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            // Rotate Fake Camera
            helperCamera.transform.localEulerAngles = Camera.main.transform.localEulerAngles;


            // Rotate Fake Camera
            //helperCamera.transform.LookAt(pointToLook);
            
            // Draw look direction line            
            helperCameraViewLine.SetPosition(0, Vector3.zero);
            Vector3 poinToLook_local = helperCamera.transform.InverseTransformPoint(pointToLookWorld);
            helperCameraViewLine.SetPosition(1, poinToLook_local);
        }
    }


    /// <summary>
    /// Draw a ellipse, in one plane, using a lineRenderer of the scene
    /// </summary>
    /// <param name="a">Semi-major axis of the ellipse</param>
    /// <param name="b">Semi-minos axis of the ellipse</param>
    /// <param name="offset">Offset of the ellipse from the origin</param>
    /// <param name="plane">Plane where the plane is going to be drawn</param>
    /// <param name="lineRenderer">Line rendeder that is going to used to draw the ellipse
    /// </param>
    private void DrawEllipse(float a, float b, float offset, TPlanes plane, LineRenderer lineRenderer)
    {
        List<Vector3> points = new List<Vector3>();
        float x = 0f;
        float y = 0f;
        float z = 0f;
        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            if (plane == TPlanes.XZ)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * a;
                z = Mathf.Sin(Mathf.Deg2Rad * angle) * b;
                y = offset;
            }
            else if (plane == TPlanes.YZ)
            {
                z = Mathf.Cos(Mathf.Deg2Rad * angle) * a;
                y = Mathf.Sin(Mathf.Deg2Rad * angle) * b;
                x = offset;
            } else if (plane == TPlanes.XY)
            {
                x = Mathf.Cos(Mathf.Deg2Rad * angle) * a;
                y = Mathf.Sin(Mathf.Deg2Rad * angle) * b;
                z = offset;
            }
            points.Add(new Vector3(x, y, z));
            angle += (360f / segments);
        }

        lineRenderer.SetPositions(points.ToArray());
    }


    public void InitHelpPlaneSize(Bounds boundingBox)
    {
        if (plane != null)
        {
            plane.transform.localScale = new Vector3(boundingBox.extents.x * .5f, 1, boundingBox.extents.z * .5f);
        }      
    }
}
