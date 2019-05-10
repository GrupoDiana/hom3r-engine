using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


/// <summary>
/// CLabel CLASS
/// </summary>
public class CLabel
{
    // Basic components
    GameObject fatherGO;        // Main father of all labels
    GameObject targetGO;        // Object to which label is locally attached    
    GameObject anchorGO;        // Point in the object surface to which label is attached
    GameObject annotationGO;    // Quad surface where the text of the label is written
    GameObject poleGO;          // Line connecting the anchor with the annotation
    GameObject textGO;          // Text of the label

    // User interface for labels
    GameObject uiCloseButtonGO; // Surface of close button
    GameObject uiCloseTextGO;   // Text of close button
    Vector3 localAnnotationPosition;    // Position of annotation with respect to pole, in annotation-space coordinates (X=1 means right border...)
    Vector3 poleEnd;                    // End position of pole. It is the reference for local annotation position
    float mouseDragScale = 10.0f;       // Scale for manual movement when dragging mouse
    float initialMouseDragScale = 10.0f;// Initial mouse drag scale
    string labelID;               // ID assigned from external application
                                        //string areaID;                      // ID of the area or component where the label has been placed

    // Precomputed data for optimization
    Vector3 poleDirection;          // Precomputed pole direction       
    Vector3 poleReference;          // Precomputed pole reference point
    Bounds referenceBounds;         // Precomputed bounds of target object
    float anchorLength;             // Precomputed length of pole from reference to anchor
    //GameObject parentGO;            // For organizing the hierarchy, put all gameobjects of this label as children of this GO
    float annotationWidth;          // Width of annotation depending on text size
    float annotationHeight;         // Height of annotation depending on text size
    Vector3 localAnchorPosition;    // Anchor position, local to targetGO

    // Variables for resizing
    Vector3 initialAnnotationSize;  // Initial size of annotation, at initialCameraDistance
    Vector3 initialAnchorSize;      // Initial size of anchor, at initialCameraDistance
    float initialPoleWidth;         // Initial width of pole, at initialCameraDistance
    float initialCameraDistance;    // Camera distance when annotation was created
    float initialMinimalSeparation = 80.0f; // Minimal separation from annotation to anchor allowed (with initial size)        
    float annotationScale = 0.5f;       // Size of annotation with respect to default size obtained through initial separation
    float poleScale = 1.0f;             // Minimal size of pole with respect to default size
    float anchorScale = 0.8f;           // Size of anchor with respect to default size obtained through initial separation
    float poleWidthScale = 0.3f;        // Width of pole with respect to default width obtained through initial separation

    // Variables and constraints to control the single DOF of the annotation: the pole length
    float poleLength;                   // Current pole length, from reference to annotation    
    bool constraintTargetAttraction;    // Label is constrained and cannot be attracted by target
    bool constraintStretch;             // Label is constrained and cannot stretch
    bool constraintElongate;            // Label is constrained and cannot elongate
    bool isElongating;                  // Remember the last movement direction, to minimize vibration
    bool isStretching;                  // Remember the last movement direction, to minimize vibration
    float minimalSeparation;            // Minimal separation between anchor and annotation, depending on size
    float repulsionAngleThreshold = 170.0f;         // Do not apply repulsion if the angle between the camera and the pole exceeds this threshold (NEW IN OUR IMPLEMENTATION):
                                                    // When an annotation is almost in front of us, we dont try to avoid target occlusion by applying an excesive pole elongation.

    // Default geometric characteristics
    float poleWidth = 1.0f;
    float anchorSize = 2.0f;
    int textSize = 8;
    int uiTextSize = 6;
    float maximalSeparation = 1400.0f;  // Maximal separation from annotation to anchor allowed
    float textMarginLeft = 10.0f;       // Horizontal text margin inside annotation
    float textMarginTopBottom = 5.0f;   // Vertical text margin inside annotation
    float uiLeftMargin = 10.0f;         // Separation between annotation text and UI buttons
    float uiRightMargin = 7.0f;         // Separation between UI buttons and annotation right
    float uiTopMargin = 7.0f;           // Separation between annotation top and UI buttons
    //float uiCloseWidth = 100.0f;        // Width of close button
    //float uiCloseHeight = 100.0f;       // Height of close button
    bool largeText = false;

    // Default colors
    Color poleColorStart = Color.grey;
    Color poleColorEnd = Color.grey;
    Color anchorColor = Color.white;
    Color textColor = Color.white;
    Color annotationColor = Color.grey;
    float annotationBrightness = 0.5f;
    float annotationOpacity = 0.8f;
    Color uiColor = Color.white;
    float uiBrightness = 0.7f;
    float uiOpacity = 0.8f;
    public Color uiTextColor = Color.white;

    // PHYSICS
    List<CForce1D> forces;                          // List of 1D forces attached to the label
    float annotationMass = 10.0f;                   // Mass     

    // DEBUG:
    //GameObject debugTL, debugTR, debugBL, debugBR;

    // Large text labels:
    TextSize ts;
    float largeTextWidth = 325.0f;
    float largeTextHeight = 250.0f;

    //GameObject panel3D_canvas;
    //TMP_Text panel3D_text;

    //////////////////////////////////////////
    //////////////////////////////////////////

    // Empty label
    public static CLabel EmptyLabel = new CLabel(); // Handle with care...
    public CLabel() { }

    // Constructor
    public CLabel(int labelNumber, string _labelID, GameObject _fatherGO, GameObject _targetGO, Vector3 anchorPosition, string _text, bool _largeText)
    {
        // SET MATERIALS:       

        // Set shaders
        // IMP: These shaders must be added to the Editor in order to work in webGL (manually include shaders by going to Edit > Project Settings > Graphics and adding it into Always Include Shaders list)        
        //Material poleMaterial = new Material(Shader.Find("Particles/Additive"));
        Material poleMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));    
        Material anchorMaterial = new Material(Shader.Find("Diffuse"));
        //Material annotationMaterial = new Material(Shader.Find("Transparent/Diffuse"));
        //Material textMaterial = Resources.Load("3DTextMaterial", typeof(Material)) as Material;
        Material uiMaterial = new Material(Shader.Find("Transparent/Diffuse"));

        // Set colors
        anchorMaterial.color = anchorColor;
        //annotationMaterial.color = new Color(annotationBrightness * annotationColor.r, annotationBrightness * annotationColor.g, annotationBrightness * annotationColor.b, annotationOpacity);
        uiMaterial.color = uiColor;
        uiMaterial.color = new Color(uiBrightness * uiColor.r, uiBrightness * uiColor.g, uiBrightness * uiColor.b, uiOpacity);
        //textMaterial.color = textColor;
        largeText = _largeText;

        // INIT GAME OBJECTS:

        // Set target and parent gameobjects
        fatherGO = _fatherGO;
        targetGO = _targetGO;        

        // Init pole
        poleGO = new GameObject("Pole_" + labelNumber);
        poleGO.transform.parent = hom3r.quickLinks.labelsObject.transform;
        poleGO.AddComponent<LineRenderer>();
        LineRenderer poleSettings = poleGO.GetComponent<LineRenderer>();
        poleSettings.material = poleMaterial;

        Color temp = Color.white;
        temp.a = 0.8f;
        poleSettings.startColor = temp;
        //poleSettings.startColor = Color.white;
        temp = Color.grey;        
        temp.a = 0.2f;
        poleSettings.endColor = temp;


        poleGO.layer = LayerMask.NameToLayer(hom3r.state.labelsUILayer);    // Change label layer to layer define

        // Init anchor
        anchorGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        anchorGO.name = "Anchor_" + labelNumber;
        anchorGO.transform.parent = hom3r.quickLinks.labelsObject.transform;
        anchorGO.GetComponent<Renderer>().material = anchorMaterial;
        anchorGO.transform.localScale = new Vector3(anchorSize, anchorSize, anchorSize);        
        localAnchorPosition = anchorPosition;
        anchorGO.transform.position = targetGO.transform.TransformPoint(localAnchorPosition);

        anchorGO.layer = LayerMask.NameToLayer(hom3r.state.labelsUILayer);    // Change label layer to layer define

        // Init annotation

        annotationGO = hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().InstantiateCurrentLabel(_text);

    
        


        //panel3D_canvas = annotationGO.transform.Find("Panel3D").gameObject;
        //panel3D_text = annotationGO.transform.Find("TextMeshPro").gameObject.GetComponent<TMP_Text>();


        //panel3D_text.text = "label";
        //panel3D_canvas.transform.localScale = new Vector3(panel3D_canvas.transform.localScale.x / 0.5f, panel3D_text.GetPreferredValues().y, panel3D_canvas.transform.localScale.z);

        //annotationGO = GameObject.CreatePrimitive(PrimitiveType.Quad);        
        // annotationGO.layer = LayerMask.NameToLayer("labels_ui_layer");
        annotationGO.layer = LayerMask.NameToLayer(hom3r.state.labelsUILayer);
        annotationGO.name = "Annotation_" + labelNumber;
        annotationGO.transform.parent = hom3r.quickLinks.labelsObject.transform;
        //annotationGO.GetComponent<Renderer>().material = annotationMaterial;

        //// Init annotations physics 
        //forces = new List<CForce1D>();

        //// Init text
        //textGO = new GameObject("Text_" + labelNumber);
        //textGO.AddComponent<TextMesh>();
        //TextMesh textSettings = textGO.GetComponent<TextMesh>();
        //if (largeText) { ts = new TextSize(textSettings); }    // Large text labels

        //textSettings.text = _text;
        //textSettings.color = Color.black;
        //textSettings.characterSize = textSize;
        //textSettings.font = Resources.Load("arialbd") as Font;
        ////textSettings.font = Resources.Load("ufonts.com_american-typewriter") as Font;        
        //textGO.GetComponent<Renderer>().material = textMaterial;
        //textGO.layer = LayerMask.NameToLayer(hom3r.state.labelsUILayer);    // Change label layer to layer define
        //if (largeText)                          // Large text labels
        //{
        //    ts.FitToWidth(largeTextWidth);
        //    ts.FitToHeight(largeTextHeight);
        //}

        //// Init Layer UI objects
        localAnnotationPosition = Vector3.zero;
        poleEnd = annotationGO.transform.position;
        //uiCloseButtonGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        //uiCloseButtonGO.layer = LayerMask.NameToLayer(hom3r.state.labelsUILayer);
        //uiCloseButtonGO.name = "UIClose_" + labelNumber;
        //uiCloseButtonGO.GetComponent<Renderer>().material = uiMaterial;
        ////uiCloseButtonGO.SetActive(false);       //TODO Delete me after IOANNINA
        //uiCloseTextGO = new GameObject("UICloseText_" + labelNumber);
        //uiCloseTextGO.AddComponent<TextMesh>();
        //TextMesh uiCloseText = uiCloseTextGO.GetComponent<TextMesh>();
        //uiCloseText.text = "X";
        //uiCloseText.color = Color.black;
        //uiCloseText.characterSize = uiTextSize;
        //uiCloseText.font = Resources.Load("arialbd") as Font;
        //uiCloseText.GetComponent<Renderer>().material = textMaterial;
        //uiCloseText.GetComponent<Renderer>().material.color = uiTextColor;
        ////uiCloseTextGO.SetActive(false);         //TODO Delete me after IOANNINA
        //Bounds closeTextBounds = uiCloseTextGO.GetComponent<TextMesh>().GetComponent<Renderer>().bounds;
        //float uiCloseWidth = (closeTextBounds.extents.x) * 2.0f;
        //float uiCloseHeight = (closeTextBounds.extents.y) * 2.0f;
        //float realUICloseWidth = uiCloseWidth;
        //// Square button:
        //if (uiCloseWidth > uiCloseHeight)
        //    uiCloseHeight = uiCloseWidth;
        //else
        //    uiCloseWidth = uiCloseHeight;

        //// Scale annotation with text width and height
        //Bounds textBounds = textGO.GetComponent<TextMesh>().GetComponent<Renderer>().bounds;
        //annotationWidth = textMarginLeft + textBounds.extents.x * 2.0f + uiLeftMargin + uiCloseWidth + uiRightMargin;
        //annotationHeight = (textBounds.extents.y + textMarginTopBottom) * 2.0f;
        //annotationGO.transform.localScale = new Vector3(annotationWidth, annotationHeight, 1.0f); // (Button inside label)        
        //textGO.transform.parent = annotationGO.transform;        
        //textSettings.transform.position = new Vector3(textMarginLeft * 0.5f - textBounds.extents.x - (uiCloseWidth + uiLeftMargin + uiRightMargin) * 0.5f, textBounds.extents.y, -10.0f); // Center text and avoid z-fight with annotation surface (button outside label)

        //// Set UI close button scale 
        //uiCloseButtonGO.transform.localScale = new Vector3(uiCloseWidth, uiCloseHeight, 1.0f);
        //uiCloseButtonGO.transform.parent = annotationGO.transform;
        //uiCloseTextGO.transform.parent = uiCloseButtonGO.transform;
        //uiCloseButtonGO.transform.position = new Vector3(textMarginLeft * 0.5f + textBounds.extents.x + uiLeftMargin * 0.5f - uiRightMargin * 0.5f, textBounds.extents.y + textMarginTopBottom - uiTopMargin - uiCloseHeight * 0.5f, -20.0f); // Put close button in top-right corner of annotation (button inside label)                        
        //uiCloseTextGO.transform.Translate(-realUICloseWidth * 0.5f, uiCloseHeight / 2.0f, -5.0f);   // Center text and avoid z-fight with annotation surface                

        //uiCloseTextGO.layer = LayerMask.NameToLayer(hom3r.state.labelsUILayer);    //TESTING DANI

        // Set application ID
        labelID = _labelID;

        // By default, compute label attached to global father
        FirstComputeLabel(anchorPosition);
    }

    


    //////////////////////////////////////////

    public void FirstComputeLabel(Vector3 anchorPosition)
    {
        // Default: attach to father        
        //RecomputeLabel(true);
        RecomputeLabel_Ellipse(fatherGO);

        // Init sizes
        initialAnnotationSize = annotationGO.transform.localScale;
        initialAnchorSize = anchorGO.transform.localScale;
        initialPoleWidth = poleWidth;
        minimalSeparation = initialMinimalSeparation;
        mouseDragScale = initialMouseDragScale;

        // Start cconstraint-free
        UnconstraintTargetAttraction();
        UnconstraintElongate();
        UnconstraintStretch();
        isElongating = false;
        isStretching = false;
    }

    //////////////////////////////////////////

    public void RecomputeLabel(bool attachedToFather)
    {
        GameObject attachedGO;
        if (attachedToFather) { attachedGO = fatherGO; }
        else { attachedGO = targetGO; }            

        // Compute target center        
        List<GameObject> referenceObjects = new List<GameObject>();
        referenceObjects.Add(attachedGO);        
        referenceBounds = ComputeBoundingBox(referenceObjects);

        poleReference = referenceBounds.center;

        // Set direction of pole        
        Vector3 anchorPosition = targetGO.transform.TransformPoint(localAnchorPosition);
        anchorGO.transform.position = anchorPosition;
        LineRenderer pole = poleGO.GetComponent<LineRenderer>();
        poleDirection = anchorPosition - poleReference;
        poleDirection.Normalize();

        // Render initial pole 
        poleLength = Vector3.Distance(anchorPosition, poleReference) + initialMinimalSeparation;
        //Vector3 poleEnd = poleReference + poleDirection * poleLength;
        poleEnd = poleReference + poleDirection * poleLength;
        pole.SetPosition(0, anchorPosition);
        pole.SetPosition(1, poleEnd);

        // Set annotation position and anchor length
        annotationGO.transform.position = poleEnd;
        anchorLength = ProjectVectorOnPole(anchorPosition);
    }

    private Vector3 CalculatePoleReferencePosition(Vector3 anchorPosition)
    {
        Bounds modelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        Vector3 cuttingPointWithAxis;

        float a;

        if (modelBoundingBox.size.x > modelBoundingBox.size.y)
        {
            ///////////////////////
            // horizontal object //
            ///////////////////////
            //Move to center in order to do the calculations, because the bounding box could not be centered in origin (0,0)             
            anchorPosition = anchorPosition - modelBoundingBox.center;


            //Projection to the XZ plane
            float x0 = anchorPosition.x;
            float z0 = Mathf.Sqrt(MathHom3r.Pow2(anchorPosition.y) + MathHom3r.Pow2(anchorPosition.z));
            
            //Calculate the ellipse on the plane XZ
            a = modelBoundingBox.size.x * 0.5f;                  // We fix a of the ellipse to object size in x axis
            float aPow2 = MathHom3r.Pow2(a);
            float bPow2 = (aPow2 * MathHom3r.Pow2(z0)) / (aPow2 - MathHom3r.Pow2(x0));

            //To avoid very flat ellipse, this happens when x0 ~= a --> b ~= 0
            if (bPow2 < 10.0f)
            {
                //Calculate the ellipse fixing b to object size in z axis
                float b = modelBoundingBox.size.z * 0.5f;
                bPow2 = MathHom3r.Pow2(b);
                aPow2 = (bPow2 * MathHom3r.Pow2(x0)) / (bPow2 - MathHom3r.Pow2(z0));
            }
            

            //Cutting Point with the X axis            
            cuttingPointWithAxis.x = (x0 * (1 - (bPow2 / aPow2)));
            cuttingPointWithAxis.y = 0.0f;
            cuttingPointWithAxis.z = 0.0f;
            
            //Move back to its original position, because the bounding box could not be centered in origin (0,0)             
            cuttingPointWithAxis = cuttingPointWithAxis + modelBoundingBox.center;
        }
        else
        {
            ///////////////////////
            // vertical object //
            ///////////////////////

            //Move to center in order to do the calculations, because the bounding box could not be centered in origin (0,0)             
            anchorPosition = anchorPosition - modelBoundingBox.center;            
            
            //Projection to the YZ plane
            float y0 = anchorPosition.y;
            float z0 = Mathf.Sqrt(MathHom3r.Pow2(anchorPosition.x) + MathHom3r.Pow2(anchorPosition.z));
            
            //Calculate the ellipse on the plane YZ
            a = modelBoundingBox.size.y * 0.5f;        // We fix a of the ellipse to object size in y axis
            float aPow2 = MathHom3r.Pow2(a);
            float bPow2 = (aPow2 * MathHom3r.Pow2(z0)) / (aPow2 - MathHom3r.Pow2(y0));

            //To avoid very flat ellipse, this happens when y0 ~= a --> b ~= 0
            if (bPow2 < 10.0f)
            {
                //Calculate the ellipse fixing b to object size in z axis
                float b = modelBoundingBox.size.z * 0.5f;
                bPow2 = MathHom3r.Pow2(b);
                aPow2 = (bPow2 * MathHom3r.Pow2(y0)) / (bPow2 - MathHom3r.Pow2(z0));
            }

            //Cutting Point with the Y axis
            cuttingPointWithAxis.x = 0.0f;
            cuttingPointWithAxis.y = (y0 * (1 - (bPow2 / aPow2)));
            cuttingPointWithAxis.z = 0.0f;

            //Move back to its original position, because the bounding box could not be centered in origin (0,0)             
            cuttingPointWithAxis = cuttingPointWithAxis + modelBoundingBox.center;
        }
        return cuttingPointWithAxis;
    }

    private Vector3 CalculatePoleEndPosition(Vector3 _anchorPosition, Vector3 _poleReference)
    {
        Bounds modelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        

        //Calculate real bounding box extents, because the object mass center could be translate (not in 0,0,0)
        Vector3 modelBoundingBox_halfSize = modelBoundingBox.size * 0.5f;

        // Set direction of pole                       
        poleDirection = _anchorPosition - _poleReference;
        poleDirection.Normalize();

        if (modelBoundingBox.size.x > modelBoundingBox.size.y)
        {
            //Horizontal
            poleLength = Mathf.Sqrt(MathHom3r.Pow2(modelBoundingBox.size.z) + MathHom3r.Pow2(modelBoundingBox.size.y));
            
        }
        else
        {
            //Vertical
            poleLength =    Mathf.Sqrt(MathHom3r.Pow2(modelBoundingBox.size.z) + MathHom3r.Pow2(modelBoundingBox.size.x));
        }

        //poleLength *= 0.5f;
        poleLength -= Vector3.Distance(modelBoundingBox.center, _anchorPosition);

        //Minimun size of the pole in order to avoid collisions between the label and the object
        //float poleLenghtMin = 0.5f * MathHom3r.Maximun(annotationGO.transform.localScale.x, annotationGO.transform.localScale.y);
        ///////poleLength += 0.5f * annotationGO.transform.localScale.y;
        //float poleLenghtMin = 0.5f * annotationGO.transform.localScale.y;
        //if (poleLength < poleLenghtMin)
        //{
        //    poleLength = poleLenghtMin;
        //}
        //Pole end position
        poleEnd = _anchorPosition + poleDirection * poleLength;

        return poleEnd;
    }

    public void RecomputeLabel_Ellipse(GameObject _attachedGO)
    {
        
        Vector3 anchorPosition = targetGO.transform.TransformPoint(localAnchorPosition);
        anchorGO.transform.position = anchorPosition;
       
        poleReference = CalculatePoleReferencePosition(anchorPosition);

        GameObject attachedGO = _attachedGO;        
       
        poleEnd = CalculatePoleEndPosition(anchorPosition, poleReference);

        LineRenderer pole = poleGO.GetComponent<LineRenderer>();
        pole.SetPosition(0, anchorPosition);
        pole.SetPosition(1, poleEnd);

        // Set annotation position and anchor length
        annotationGO.transform.position = poleEnd;
        anchorLength = ProjectVectorOnPole(anchorPosition);
    }

    /// <summary>
    /// Calculate the BoundingBox of a list of objects. 
    /// </summary>
    /// <param name="objects"> The list of Gameobjects </param> 
    /// <returns> The axis aligned bounding box. </returns> 
    private Bounds ComputeBoundingBox(List<GameObject> objects)
    {
        Bounds totalBB = new Bounds();
        bool firstObject = true;		// Needed to avoid that Unity includes (0,0,0) center inside the BB

        // Get info from all objects 
        foreach (GameObject go in objects)
        {
            // Expand bounding box 
            if (go != null)
            {
                if (go.GetComponent<Collider>() != null)
                {
                    Bounds goBB = go.GetComponent<Collider>().bounds;

                    if (firstObject)
                    {
                        totalBB = goBB;
                        firstObject = false;
                    }
                    else
                        totalBB.Encapsulate(goBB);
                }

                // Do the same for all children objects
                List<GameObject> children = new List<GameObject>();
                for (int i = go.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject newChild = go.transform.GetChild(i).gameObject;
                    if (newChild != null)
                        children.Add(newChild);
                }
                if (children.Count > 0)
                {
                    Bounds childrenBB = ComputeBoundingBox(children);
                    totalBB.Encapsulate(childrenBB);
                }
            }
        }
        return totalBB;
    }
    //////////////////////////////////////////

    public void SetInitialCameraDistance(float _distance)
    {
        initialCameraDistance = _distance;
    }

    //////////////////////////////////////////

    public void DestroyLabel()
    {
        Object.Destroy(anchorGO);
        Object.Destroy(annotationGO);
        Object.Destroy(poleGO);
        Object.Destroy(textGO);
    }

    //////////////////////////////////////////
    // RENDER AND SCREEN-SPACE AND POLE-SPACE METHODS
    //////////////////////////////////////////

    public float GetDistanceToTarget()
    {
        //GameObject orbitPlaneGO = GameObject.FindGameObjectWithTag("OrbitPlane_Tag");
        //return orbitPlaneGO.GetComponent<NavigationManager>().GetDistanceToAxis();

        

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        Vector3 intersectionVector = new Vector3(mainCamera.transform.localPosition.x, 0, 0);
        float distance = Vector3.Distance(intersectionVector, mainCamera.transform.localPosition);
        return distance;

    }

    public void Resize()
    {
        // Set annotation scale        
        float distance = GetDistanceToTarget();         //TODO make a new resize method
        float newSize;                
        newSize = distance / initialCameraDistance; 
        if (newSize < 0.08f) { newSize = 0.08f; }       //FIXME please removed me        
        
        if (hom3r.state.platform == THom3rPlatform.Android || hom3r.state.platform == THom3rPlatform.IOS)
        {
            newSize = 0.005f;       //AR_APP Hard code
        }else if (hom3r.state.platform == THom3rPlatform.Editor)
        {
            //newSize = 0.01f;       //AR_APP Hard code
            newSize = 0.2f;       //hom3r web Hard code
        }

        Debug.Log("distance : " + distance + " - initialCameraDistance: " + initialCameraDistance + " - newSize: " + newSize );
        //annotationGO.transform.localScale = initialAnnotationSize * newSize * annotationScale;
        //annotationWidth = annotationGO.transform.localScale.x;
        //annotationHeight = annotationGO.transform.localScale.y;

        //annotationGO.transform.localScale = annotationGO.transform.localScale * 50;
        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager>().DoAfterScaling(annotationGO);

        // Set anchor scale
        anchorGO.transform.localScale = initialAnchorSize * newSize * anchorScale;

        // Set pole minimal separation and width
        minimalSeparation = initialMinimalSeparation * newSize * poleScale;
        poleWidth = initialPoleWidth * newSize * poleWidthScale;
        poleGO.GetComponent<LineRenderer>().SetWidth(poleWidth, poleWidth);

        // Set mouse scale for manual movement
        mouseDragScale = initialMouseDragScale * newSize;
    }


    //////////////////////////////////////////

    public void Hide()
    {
        anchorGO.SetActive(false);
        annotationGO.SetActive(false);
        textGO.SetActive(false);
        poleGO.SetActive(false);
    }

    //////////////////////////////////////////

    public void Draw()
    {
        //anchorGO.SetActive(true);
        annotationGO.SetActive(true);
        textGO.SetActive(true);
        poleGO.SetActive(true);
    }

    //////////////////////////////////////////

    public void FaceAnnotationToCamera()
    {
        annotationGO.transform.position = poleEnd;

        annotationGO.transform.LookAt(Camera.main.transform, Camera.main.transform.up);
        annotationGO.transform.Rotate(0.0f, 180.0f, 0.0f);

        Rect annotationRect = GetAnnotationScreenSpace();
        float annotationScreenWidth = annotationRect.width;
        float annotationScreenHeight = annotationRect.height;
        float annotationPositionResizeX = annotationScreenWidth * 0.5f;
        float annotationPositionResizeY = annotationScreenHeight * 0.5f;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(poleEnd);
        Vector3 screenIncrement = new Vector3(localAnnotationPosition.x * annotationPositionResizeX, localAnnotationPosition.y * annotationPositionResizeY, 0.0f);
        screenPosition = screenPosition + screenIncrement;
        annotationGO.transform.position = Camera.main.ScreenToWorldPoint(screenPosition);
    }

    //////////////////////////////////////////

    public void UpdatePoleRender()
    {
        // Get reference to pole renderer
        LineRenderer pole = poleGO.GetComponent<LineRenderer>();

        // Render pole depending on annotation position                
        pole.SetPosition(0, anchorGO.transform.position);
        pole.SetPosition(1, poleEnd);
    }

    //////////////////////////////////////////

    public int GetLayer()
    {
        return annotationGO.layer;
    }

    //////////////////////////////////////////

    public Vector3 FaceAnnotationPointToCamera(Vector3 point)
    {
        Vector3 center = annotationGO.transform.position;
        Vector3 localPoint = point - center;
        localPoint = annotationGO.transform.rotation * localPoint;
        return center + localPoint;
    }

    //////////////////////////////////////////

    public bool IsInsideAnnotation(Vector3 point)
    {
        Rect annotationRect = GetAnnotationScreenSpace();

        if (point.x < annotationRect.xMin)
            return false;
        if (point.x > annotationRect.xMax)
            return false;
        if (point.y < annotationRect.yMin)
            return false;
        if (point.y > annotationRect.yMax)
            return false;

        return true;
    }

    //////////////////////////////////////////

    public Rect GetAnnotationScreenSpace()
    {
        // Based on: http://answers.unity3d.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html

        //Vector3 center = annotationGO.GetComponent<Renderer>().bounds.center;
        Vector3 center = annotationGO.transform.position;

        // Compute bounds dimensions
        float width = annotationWidth / 2.0f;
        float height = annotationHeight / 2.0f;

        // Compute bounding points        
        Vector3 bottomLeft = new Vector3(center.x - width, center.y - height, center.z);
        Vector3 bottomRight = new Vector3(center.x + width, center.y - height, center.z);
        Vector3 topLeft = new Vector3(center.x - width, center.y + height, center.z);
        Vector3 topRight = new Vector3(center.x + width, center.y + height, center.z);

        // Rotate points to face camera
        topLeft = FaceAnnotationPointToCamera(topLeft);
        topRight = FaceAnnotationPointToCamera(topRight);
        bottomLeft = FaceAnnotationPointToCamera(bottomLeft);
        bottomRight = FaceAnnotationPointToCamera(bottomRight);

        // DEBUG
        //debugTL.transform.position = topLeft;
        //debugTR.transform.position = topRight;
        //debugBL.transform.position = bottomLeft;
        //debugBR.transform.position = bottomRight;

        Vector2[] extentPoints = new Vector2[4]
        {
            WorldToGUIPoint(topLeft),
            WorldToGUIPoint(topRight),
            WorldToGUIPoint(bottomLeft),
            WorldToGUIPoint(bottomRight)
        };

        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    //////////////////////////////////////////

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        //screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }

    //////////////////////////////////////////

    public bool IsPointOutsideViewport(Vector3 point)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(point);

        if (viewportPos.x < 0)
            return true;
        if (viewportPos.y < 0)
            return true;
        if (viewportPos.x > 1.0f)
            return true;
        if (viewportPos.y > 1.0f)
            return true;

        return false;
    }

    //////////////////////////////////////////

    public bool IsAnnotationOutsideViewport()
    {
        Vector3 center = annotationGO.transform.position;
        float width = annotationWidth;
        float height = annotationHeight;

        Vector3 topLeft = new Vector3(center.x - width, center.y + height, center.z);
        Vector3 topRight = new Vector3(center.x + width, center.y + height, center.z);
        Vector3 bottomLeft = new Vector3(center.x - width, center.y - height, center.z);
        Vector3 bottomRight = new Vector3(center.x + width, center.y - height, center.z);

        return (IsPointOutsideViewport(topLeft) || IsPointOutsideViewport(topRight) || IsPointOutsideViewport(bottomLeft) || IsPointOutsideViewport(bottomRight));
    }

    //////////////////////////////////////////

    public bool IsOccludingTarget(out Vector3 hitPoint)
    {
        RaycastHit hitInfo;
        int targetLayerMask = 1 << targetGO.layer;

        if (Physics.Raycast(annotationGO.transform.position, Camera.main.transform.forward, out hitInfo, maximalSeparation, targetLayerMask))
        {
            //textGO.GetComponent<TextMesh>().color = Color.red;  // DEBUG
            hitPoint = hitInfo.point;
            return true;
        }
        else
        {
            //textGO.GetComponent<TextMesh>().color = Color.black;    // DEBUG
            hitPoint = Vector3.zero;
            return false;
        }
    }

    //////////////////////////////////////////

    public bool IsPoleAlignedWithCamera()
    {
        return (Vector3.Angle(poleDirection, Camera.main.transform.forward) > repulsionAngleThreshold);
    }

    //////////////////////////////////////////

    public float ProjectVectorOnPole(Vector3 myVector)
    {
        Vector3 myVectorProjection = Vector3.Project(myVector, poleDirection);
        return myVectorProjection.magnitude;
    }

    //////////////////////////////////////////

    public Vector3 ProjectPoleOnVector(float poleLength)
    {
        return poleReference + poleDirection * poleLength;
    }

    ////////////////////////////////////////// 

    public void SetDebugTextColor()
    {
        //Color newColor;
        //if (IsStretchConstrained())
        //{
        //    if (IsElongateConstrained())
        //        newColor = Color.red;       // Both constrained
        //    else
        //        newColor = Color.magenta;   // Only stretch constrained
        //}
        //else
        //{
        //    if (IsElongateConstrained())
        //        newColor = Color.yellow;    // Only elongate constrained
        //    else
        //        newColor = Color.black;     // Nothing constrained
        //}

        ////textGO.GetComponent<TextMesh>().color = newColor;
        //textGO.GetComponent<Renderer>().material.color = newColor;
    }

    //////////////////////////////////////////
    // PHYSICS AND CONSTRAINTS METHODS
    //////////////////////////////////////////    

    public bool IsPoleTooShort()
    {
        return (GetAnnotationAnchorSeparation() < minimalSeparation);
    }

    //////////////////////////////////////////

    public bool IsPoleTooLong()
    {
        return (GetAnnotationAnchorSeparation() > maximalSeparation);
    }

    //////////////////////////////////////////


    public bool AttractionWouldElongate(float attractionPoint)
    {
        return (attractionPoint > poleLength);
    }

    //////////////////////////////////////////

    public bool AttractionWouldStretch(float attractionPoint)
    {
        return (attractionPoint <= poleLength);
    }

    //////////////////////////////////////////

    public void Apply1DAttraction(float K, float D, float attractionPoint)
    {
        // Check cases in which we dont want to apply attraction

        // Elongation   (should never happen in current implementation)
        if (AttractionWouldElongate(attractionPoint))
        {
            if (IsElongateConstrained())    // Cannot elongate
                return;

            if (IsPoleTooLong())    // Avoid elongation of too long poles
            {
                ConstraintElongate();
                return;
            }
            if (IsPoleAlignedWithCamera())  // Avoid elongation of poles (almost) aligned with camera forward vector
            {
                ConstraintElongate();
                return;
            }
        }
        else    // Stretching
        {
            if (IsStretchConstrained()) // Cannot stretch
                return;

            if (IsPoleTooShort()) // Avoid stretching of too short poles
            {
                ConstraintStretch();
                return;
            }
        }

        // Give priority to elongation. We always try to elongate if we need it
        if (AttractionWouldStretch(attractionPoint) && IsElongateConstrained())
        {
            UnconstraintElongate();
        }

        // Apply force if all checks are passed  
        CForce1D newForce = new CForce1D(K, D);
        newForce.SetMass(annotationMass);
        newForce.SetTargetPosition(attractionPoint);
        newForce.SetAttractive(true);
        forces.Add(newForce);
    }

    //////////////////////////////////////////

    public bool RepulsionWouldElongate(float repulsionPoint)
    {
        return (repulsionPoint < poleLength);
    }

    //////////////////////////////////////////

    public bool RepulsionWouldStretch(float repulsionPoint)
    {
        return (repulsionPoint >= poleLength);
    }

    //////////////////////////////////////////

    public void Apply1DRepulsion(float K, float D, float repulsionPoint)
    {
        // Check cases in which we dont want to apply repulsion

        // Elongation
        if (RepulsionWouldElongate(repulsionPoint))
        {
            if (IsElongateConstrained())    // Cannot elongate
                return;

            if (IsPoleTooLong())    // Avoid elongation of too long poles
            {
                ConstraintElongate();
                return;
            }
            if (IsPoleAlignedWithCamera())  // Avoid elongation of poles (almost) aligned with camera forward vector
            {
                ConstraintElongate();
                return;
            }
        }
        else    // Stretching
        {
            if (IsStretchConstrained()) // Cannot stretch
                return;

            if (IsPoleTooShort()) // Avoid stretching of too short poles
            {
                ConstraintStretch();
                return;
            }
        }

        // Give priority to elongation. We always try to elongate if we need it
        if (RepulsionWouldStretch(repulsionPoint) && IsElongateConstrained())
        {
            UnconstraintElongate();
        }

        // Apply force if all checks are passed        
        CForce1D newForce = new CForce1D(K, D);
        newForce.SetMass(annotationMass);
        newForce.SetTargetPosition(repulsionPoint);
        newForce.SetAttractive(false);
        forces.Add(newForce);
    }

    //////////////////////////////////////////

    public void ClearForces()
    {
        forces.Clear();
    }

    //////////////////////////////////////////

    public float ComputeVelocity()
    {
        // Current implementation is kinematic, without taking into account velocity/acceleration        
        return 0.0f;
    }

    //////////////////////////////////////////

    public void ComputeForces()
    {
        float deltaX = 0.0f;
        float velocity = ComputeVelocity();

        foreach (CForce1D force in forces)
        {
            force.SetPosition(poleLength);
            force.SetVelocity(velocity);
            deltaX += force.Compute();
        }

        poleLength += deltaX;
        poleEnd = ProjectPoleOnVector(poleLength);

        if (Mathf.Approximately(deltaX, 0.0f))
        {
            isElongating = false;
            isStretching = false;
        }
        else if (deltaX > 0.0f)
        {
            isElongating = true;
            isStretching = false;
        }
        else
        {
            isElongating = false;
            isStretching = true;
        }
    }

    //////////////////////////////////////////

    public void ClearConstraints()
    {
        UnconstraintElongate();
        UnconstraintStretch();
        UnconstraintTargetAttraction();
    }

    //////////////////////////////////////////

    public bool IsTargetAttractionConstrained()
    {
        return constraintTargetAttraction;
    }

    //////////////////////////////////////////

    public void ConstraintTargetAttraction()
    {
        constraintTargetAttraction = true;
    }

    //////////////////////////////////////////

    public void UnconstraintTargetAttraction()
    {
        constraintTargetAttraction = false;
    }

    //////////////////////////////////////////

    public bool IsElongateConstrained()
    {
        return constraintElongate;
    }

    //////////////////////////////////////////

    public bool IsStretchConstrained()
    {
        return constraintStretch;
    }

    //////////////////////////////////////////

    public void ConstraintStretch()
    {
        constraintStretch = true;
        SetDebugTextColor();
    }

    //////////////////////////////////////////

    public void UnconstraintStretch()
    {
        constraintStretch = false;
        SetDebugTextColor();
    }

    //////////////////////////////////////////

    public void ConstraintElongate()
    {
        constraintElongate = true;
        SetDebugTextColor();
    }

    //////////////////////////////////////////

    public void UnconstraintElongate()
    {
        constraintElongate = false;
        SetDebugTextColor();
    }


    //////////////////////////////////////////
    // GET METHODS
    //////////////////////////////////////////     

    public string GetLabelID()
    {
        return labelID;
    }

    //////////////////////////////////////////

    public GameObject GetCloseButtonGO()
    {
        return uiCloseButtonGO;
    }

    //////////////////////////////////////////

    public GameObject GetTargetGO()
    {
        return targetGO;
    }

    //////////////////////////////////////////

    public GameObject GetAnnotationGO()
    {
        return annotationGO;
    }

    //////////////////////////////////////////

    public float GetPoleLength()
    {
        return poleLength;
    }

    //////////////////////////////////////////

    public Vector3 GetAnnotationCenter()
    {
        return annotationGO.transform.position;
    }

    //////////////////////////////////////////

    public float GetAnnotationLength()
    {
        return ProjectVectorOnPole(GetAnnotationCenter());
    }

    //////////////////////////////////////////

    public Vector3 GetAnchorPoint()
    {
        return anchorGO.transform.position;
    }

    //////////////////////////////////////////

    public GameObject GetFatherGO()
    {
        return fatherGO;
    }

    //////////////////////////////////////////

    public Vector3 GetPoleDirection()
    {
        return poleDirection;
    }

    //////////////////////////////////////////

    public float GetAnnotationAnchorSeparation()
    {
        return (poleLength - anchorLength);
    }

    //////////////////////////////////////////

    public float GetAnchorLength()
    {
        return anchorLength;
    }

    //////////////////////////////////////////

    public bool IsElongating()
    {
        return isElongating;
    }

    //////////////////////////////////////////

    public bool IsStretching()
    {
        return isStretching;
    }

    //////////////////////////////////////////

    public void SetElongating(bool _elongating)
    {
        isElongating = _elongating;
    }

    //////////////////////////////////////////

    public void SetStretching(bool _stretching)
    {
        isStretching = _stretching;
    }

    //////////////////////////////////////////

    public Bounds GetReferenceBounds()
    {
        return referenceBounds;
    }

    //////////////////////////////////////////
    // UI METHODS
    //////////////////////////////////////////

    public void UpdateLocalAnnotationPosition()
    {
        Rect annotationRect = GetAnnotationScreenSpace();
        float annotationScreenWidth = annotationRect.width;
        float annotationScreenHeight = annotationRect.height;
        Vector3 annotationScreenPosition = Camera.main.WorldToScreenPoint(annotationGO.transform.position);
        Vector3 poleScreenPosition = Camera.main.WorldToScreenPoint(poleEnd);
        localAnnotationPosition = annotationScreenPosition - poleScreenPosition;
        localAnnotationPosition.x = localAnnotationPosition.x / (annotationScreenWidth * 0.5f);   // X=1 means right boundary, X=-1 means left boundary
        localAnnotationPosition.y = localAnnotationPosition.y / (annotationScreenHeight * 0.5f);
    }

    public void ManualMove(float x, float y)
    {
        // Compute goal translation
        float newX = x * mouseDragScale;
        float newY = y * mouseDragScale;
        Vector3 translation = new Vector3(newX, newY, 0.0f);

        // Check if new translation keeps pole end inside annotation boundaries
        annotationGO.transform.Translate(translation);                              // Translate
        Vector3 poleScreenPosition = Camera.main.WorldToScreenPoint(poleEnd);
        if (IsInsideAnnotation(poleScreenPosition))
            UpdateLocalAnnotationPosition();    // Store new position of annotation wrt pole end            
        else
            annotationGO.transform.Translate(-translation); // Undo translate
    }
}

/// <summary>
/// CForce1D CLASS
/// </summary>
public class CForce1D
{
    float K;    // Stiffness
    float D;    // Damping
    float X;    // Position
    float V;    // Velocity
    float T;    // Target position
    float M;    // Mass
    float invM; // Inverse of Mass    
    bool isAttractive;  // Force type (sign)

    //////////////////////////////////////////

    public CForce1D(float _stiffness, float _damping)
    {
        K = _stiffness;
        D = _damping;
        X = 0.0f;
        V = 0.0f;
        T = 0.0f;
        M = 0.0f;
        invM = 0.0f;
        isAttractive = true;
    }

    //////////////////////////////////////////

    public void SetPosition(float _position)
    {
        X = _position;
    }

    public void SetVelocity(float _velocity)
    {
        V = _velocity;
    }

    public void SetMass(float _mass)
    {
        M = _mass;
        if (_mass != 0)
            invM = 1.0f / _mass;
        else
            invM = 0.0f;
    }

    public void SetTargetPosition(float _target)
    {
        T = _target;
    }

    public void SetAttractive(bool _attractive)
    {
        isAttractive = _attractive;
    }

    //////////////////////////////////////////

    public float ExplicitIntegration()
    {
        float sign;
        if (isAttractive)
            sign = -1.0f;
        else
            sign = 1.0f;

        float F = sign * K * (X - T) - sign * D * V;
        V = V + invM * F * Time.deltaTime;
        float deltaX = V * Time.deltaTime;
        return deltaX;
    }

    //////////////////////////////////////////

    //public float ImplicitIntegration()
    //{        
    //    float sign;
    //    if (isAttractive)
    //        sign = -1.0f;
    //    else
    //        sign = 1.0f;

    //    float X0 = X;
    //    float V0 = V;
    //    float invDeltaT = 1.0f / Time.deltaTime;

    //    // Compute V
    //    V = (sign * K * (T - X0) - invDeltaT * M * V0) / (sign * K * Time.deltaTime - sign * D - invDeltaT * M);

    //    // Compute X
    //    X = X0 + V * Time.deltaTime;

    //    // Compute deltaX
    //    float deltaX = X - X0;
    //    return deltaX;
    //}

    public float ImplicitIntegration()
    {
        float sign;
        if (isAttractive)
            sign = -1.0f;
        else
            sign = 1.0f;

        float X0 = X;
        float sqDeltaTinvM = (Time.deltaTime * Time.deltaTime) * invM;

        // Compute X
        X = (X0 - sqDeltaTinvM * sign * K * T) / (1 - sqDeltaTinvM * sign * K);

        // Compute deltaX
        float deltaX = X - X0;
        return deltaX;
    }

    //////////////////////////////////////////

    public float Compute()
    {
        //return ExplicitIntegration();
        return ImplicitIntegration();
    }
}


/////////////////////////////////////////////////////////////////////////////////////////////
// TextSize CLASS
/////////////////////////////////////////////////////////////////////////////////////////////
// Based on: TextSize for Unity3D by thienhaflash: https://github.com/Mystfit/TeslaRift/blob/master/TeslaRift-Unity/Assets/Scripts/Visuals/TextSize.cs
public class TextSize
{
    private Hashtable dict; //map character -> width

    private TextMesh textMesh;
    private Renderer renderer;

    public TextSize(TextMesh tm)
    {
        textMesh = tm;
        renderer = tm.GetComponent<Renderer>();
        dict = new Hashtable();
        getSpace();
    }

    private void getSpace()
    {
        //the space can not be got alone
        string oldText = textMesh.text;

        textMesh.text = "a";
        float aw = renderer.bounds.size.x;
        textMesh.text = "a a";
        float cw = renderer.bounds.size.x - 2 * aw;

        MonoBehaviour.print("char< > " + cw);
        dict.Add(' ', cw);
        dict.Add('a', aw);

        textMesh.text = oldText;
    }

    public float GetTextWidth(string s)
    {
        char[] charList = s.ToCharArray();
        float w = 0;
        char c;
        string oldText = textMesh.text;

        for (int i = 0; i < charList.Length; i++)
        {
            c = charList[i];

            if (dict.ContainsKey(c))
            {
                w += (float)dict[c];
            }
            else
            {
                textMesh.text = "" + c;
                float cw = renderer.bounds.size.x;
                dict.Add(c, cw);
                w += cw;
                //MonoBehaviour.print("char<" + c +"> " + cw);
            }
        }

        textMesh.text = oldText;
        return w;
    }


    public float width {
        get { return GetTextWidth(textMesh.text); }
    }

    public float height {
        get { return renderer.bounds.size.y; }
    }

    public void FitToWidth(float wantedWidth)
    {
        if (width <= wantedWidth) return;

        string oldText = textMesh.text;
        textMesh.text = "";

        string[] lines = oldText.Split('\n');

        foreach (string line in lines)
        {
            textMesh.text += wrapLine(line, wantedWidth);
            textMesh.text += "\n";
        }
    }

    public void FitToHeight(float wantedHeight)
    {
        if (height <= wantedHeight) return;

        string oldText = textMesh.text;
        textMesh.text = "";

        string[] lines = oldText.Split('\n');

        foreach (string line in lines)
        {
            textMesh.text += line;
            textMesh.text += "\n";
            if (height > wantedHeight)
            {
                textMesh.text += "...";
                return;
            }
        }
    }

    private string wrapLine(string s, float w)
    {
        // need to check if smaller than maximum character length, really...
        if (w == 0 || s.Length <= 0) return s;

        char c;
        char[] charList = s.ToCharArray();

        float charWidth = 0;
        float wordWidth = 0;
        float currentWidth = 0;

        string word = "";
        string newText = "";
        string oldText = textMesh.text;

        for (int i = 0; i < charList.Length; i++)
        {
            c = charList[i];

            if (dict.ContainsKey(c))
            {
                charWidth = (float)dict[c];
            }
            else
            {
                textMesh.text = "" + c;
                charWidth = renderer.bounds.size.x;
                dict.Add(c, charWidth);
                //here check if max char length
            }

            if (c == ' ' || i == charList.Length - 1)
            {
                if (c != ' ')
                {
                    word += c.ToString();
                    wordWidth += charWidth;
                }

                if (currentWidth + wordWidth < w)
                {
                    currentWidth += wordWidth;
                    newText += word;
                }
                else
                {
                    currentWidth = wordWidth;
                    newText += word.Replace(" ", "\n");
                }

                word = "";
                wordWidth = 0;
            }

            word += c.ToString();
            wordWidth += charWidth;
        }

        textMesh.text = oldText;
        return newText;
    }
}
