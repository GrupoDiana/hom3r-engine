using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationHelper : MonoBehaviour {

    enum TPlanes { XZ, XY, YZ };

    int segments;

    GameObject helperTranslationTrajectoryGO;    
    GameObject helperCameraGO;
    GameObject navigationAssistantsGO;
    GameObject planeGO;
    GameObject helperPointToLookCameraGO;
    GameObject helperEvoluteCusp1GO;
    GameObject helperEvoluteCusp2GO;

    LineRenderer lineTranslationEllipse;
    LineRenderer lineRotationEllipse;
    LineRenderer lineHorizontalFrameworkEllipse;
    LineRenderer lineVerticalFrameworkEllipse;    
    LineRenderer helperCameraViewLine;

    bool activated;
    // Use this for initialization
    void Awake ()
    {
        activated = false;
        foreach (Transform child in transform)
        {            
            if (child.name == "HelperTranslationTrajectory")
            {
                helperTranslationTrajectoryGO = child.gameObject;
                lineTranslationEllipse = child.GetComponentInChildren<LineRenderer>();
            }
            if (child.name == "HelperCamera")
            {
                helperCameraGO = child.gameObject;
                helperCameraViewLine = child.GetComponentInChildren<LineRenderer>();
            }
            if (child.name == "Plane")
            {
                planeGO = child.gameObject;
            }
        }

        navigationAssistantsGO = GameObject.Find("NavigationAssistants");
        foreach (Transform child in navigationAssistantsGO.transform)
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
            if (child.name == "HelperPointToLookCamera")
            {
                helperPointToLookCameraGO = child.gameObject;
            }
            if (child.name == "HelperPointEvoluteCusp1")
            {
                helperEvoluteCusp1GO = child.gameObject;
            }
            if (child.name == "HelperPointEvoluteCusp2")
            {
                helperEvoluteCusp2GO = child.gameObject;
            }
        }

        segments = 3600;
        //if (hom3r.state.platform == THom3rPlatform.Editor || hom3r.state.platform == THom3rPlatform.Windows) { SetActivatedNavigationHelper(true); }
        //else { SetActivatedNavigationHelper(false); }
        SetActivatedNavigationHelper(true);
    }

    public void SetActivatedNavigationHelper(bool _activated)
    {
        if (_activated && activated) { return; }
        activated = _activated;
        if (_activated)
        {
            helperTranslationTrajectoryGO.SetActive(true);
            helperCameraGO.SetActive(true);
            helperPointToLookCameraGO.SetActive(true);
            helperEvoluteCusp1GO.SetActive(true);
            helperEvoluteCusp2GO.SetActive(true);
            planeGO.SetActive(true);
            navigationAssistantsGO.SetActive(true);

            InitLineRenderer(lineTranslationEllipse, 1.2f, segments);
            InitLineRenderer(lineRotationEllipse, 1.2f, segments);
            InitLineRenderer(helperCameraViewLine, 1f, 1);
            InitLineRenderer(lineHorizontalFrameworkEllipse, 0.9f, segments);
            InitLineRenderer(lineVerticalFrameworkEllipse, 0.9f, segments);
        }
        else
        {
            helperTranslationTrajectoryGO.SetActive(false);
            helperCameraGO.SetActive(false);
            planeGO.SetActive(false);
            navigationAssistantsGO.SetActive(false);
            helperPointToLookCameraGO.SetActive(false);
            helperEvoluteCusp1GO.SetActive(false);
            helperEvoluteCusp2GO.SetActive(false);
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
        navigationAssistantsGO.transform.position = position;
    }

    public void SetBiggerLines()
    {
        if (!activated) { return; }
        lineTranslationEllipse.widthMultiplier += 0.1f;
        lineRotationEllipse.widthMultiplier += 0.1f;
        lineHorizontalFrameworkEllipse.widthMultiplier +=0.1f;
        lineVerticalFrameworkEllipse.widthMultiplier += 0.1f;
        helperCameraViewLine.widthMultiplier += 0.1f;
    }

    public void SetSmallerLines()
    {
        if (!activated) { return; }
        lineTranslationEllipse.widthMultiplier -=  0.1f;
        lineRotationEllipse.widthMultiplier -= 0.1f;
        lineHorizontalFrameworkEllipse.widthMultiplier -= 0.1f;
        lineVerticalFrameworkEllipse.widthMultiplier -=  0.1f;
        helperCameraViewLine.widthMultiplier -= 0.1f;
    }

    public void SetBiggerCamera()
    {
        if (!activated) { return; }
        Vector3 newScale = helperCameraGO.transform.localScale * 1.1f;
        helperCameraGO.transform.localScale = newScale;
    }

    public void SetSmallerCamera()
    {
        if (!activated) { return; }
        Vector3 newScale = helperCameraGO.transform.localScale * 0.9f;
        helperCameraGO.transform.localScale = newScale;
    }

    public void SetBiggerPointToLook()
    {
        if (!activated) { return; }
        Vector3 newScale = helperPointToLookCameraGO.transform.localScale * 1.1f;
        helperPointToLookCameraGO.transform.localScale = newScale;
    }
    public void SetSmallerPointToLook()
    {
        if (!activated) { return; }
        Vector3 newScale = helperPointToLookCameraGO.transform.localScale * 0.9f;
        helperPointToLookCameraGO.transform.localScale = newScale;
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
        if (activated) 
        //if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            DrawEllipse(xRadius, zRadius, 0f, TPlanes.XZ, lineTranslationEllipse);
        }        
    }
  
    public void DrawRotationEllipse(float zRadius, float yRadius, float offset)
    {
        if (activated)
        //    if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            DrawEllipse(zRadius, yRadius, -offset, TPlanes.YZ, lineRotationEllipse);            
        }
    }

    public void DrawHorizontalFrameworkEllipse(float xRadius, float zRadius)
    {
        if (activated)
        //if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            DrawEllipse(xRadius, zRadius, 0f, TPlanes.XZ, lineHorizontalFrameworkEllipse);
        }
    }

    public void DrawVerticalFrameworkEllipse(float xRadius, float yRadius)
    {
        if (activated)
        //   if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            DrawEllipse(xRadius, yRadius, 0f, TPlanes.XY, lineVerticalFrameworkEllipse);
        }
    }

    public void MoveCameraHelper(Vector3 cameraPosition)
    {
        if (activated)
            //if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            // Move Camera
            helperCameraGO.transform.localPosition = cameraPosition;            
        }        
    }

    public void SetCameraOrientationHelper(Vector3 pointToLookWorld)
    {
        if (activated)
            //if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            // Rotate Fake Camera
            helperCameraGO.transform.localEulerAngles = Camera.main.transform.localEulerAngles;


            // Rotate Fake Camera
            //helperCamera.transform.LookAt(pointToLook);
            
            // Draw look direction line            
            helperCameraViewLine.SetPosition(0, Vector3.zero);
            Vector3 poinToLook_local = helperCameraGO.transform.InverseTransformPoint(pointToLookWorld);
            helperCameraViewLine.SetPosition(1, poinToLook_local);

            helperPointToLookCameraGO.GetComponent<Transform>().SetPositionAndRotation(pointToLookWorld, Quaternion.identity);
        }
    }

    public void SetEvoluteCuspHelper(Vector3 pointEvoluteCusp1, Vector3 pointEvoluteCusp2)
    {
        Vector3 pointEvoluteCusp1World = hom3r.quickLinks.navigationSystemObject.transform.TransformPoint(pointEvoluteCusp1);
        helperEvoluteCusp1GO.GetComponent<Transform>().SetPositionAndRotation(pointEvoluteCusp1World, Quaternion.identity);

        Vector3 pointEvoluteCusp2World = hom3r.quickLinks.navigationSystemObject.transform.TransformPoint(pointEvoluteCusp2);
        helperEvoluteCusp2GO.GetComponent<Transform>().SetPositionAndRotation(pointEvoluteCusp2World, Quaternion.identity);        
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
        if (!activated) { return; }
        if (planeGO != null)
        {
            planeGO.transform.localScale = new Vector3(boundingBox.extents.x * .5f, 1, boundingBox.extents.z * .5f);
        }      
    }

    public void InitNavigationHelperRotation(Vector3 rotationEulerAngles)
    {
        if (!activated) { return; }
        navigationAssistantsGO.transform.eulerAngles = rotationEulerAngles;
    }
}
