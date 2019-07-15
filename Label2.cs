using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum TLabelState { idle, creating, selected }
public enum TLabelSelectedState { idle, moving }        // Once the label is selected the label can be doing nothing or changing its position/rotation

public class Label2 : MonoBehaviour
{
    string id;
    TLabelType type;
    string areaId;
    string text;
    CLabelTransform labelTransform;    
    TLabelState state;
    TLabelSelectedState selectedState;

    GameObject boardGO;     // All kind of labels have a board object and this have two children Panel3D and Text
    GameObject textGO;      // Children of board object
    GameObject panelGO;     // Children of board object
    GameObject poleGO;      // Only in Anchored label
    GameObject anchorGO;    // Only in Anchored label

    float verticalFieldOfView_rad;      // Camera parameters. We use it to calculate correction parameters/factors
    float horizontalFieldOfView_rad;    // Camera parameters. We use it to calculate correction parameters/factors


    float defaultScaleFactor;               // To save the board default scale
    Vector3 defaultBoardGOScale;            // To save the original board GameObject scale
    float scaleFactor;                      // Factor to modify the board default scale

    float panelGOPadding = 0.1f;    // Top/Bottom margin between panel and text
    Color32 idleColor;
    Color32 selectedColor;

    private void Awake()
    {
        boardGO = this.transform.Find("Board").gameObject;
        panelGO = boardGO.transform.Find("Panel3D").gameObject;
        textGO = boardGO.transform.Find("TextMeshPro").gameObject;
        
        if (this.transform.Find("Pole")) {
            poleGO = this.transform.Find("Pole").gameObject;
        }
        if (this.transform.Find("Anchor")) {
            anchorGO = this.transform.Find("Anchor").gameObject;
        }

        state = TLabelState.creating;
        selectedState = TLabelSelectedState.idle;

        // Scale Variables
        defaultScaleFactor = 0.1f;                              // By default we want 10% of the main axis of the bounding box as Scale Factor
        defaultBoardGOScale = boardGO.transform.localScale;     // Get the initial GO scale
        scaleFactor = 1.0f;                                     // Initially the slider will start with 1

        //Color variable
        idleColor = Color.white;
        selectedColor = new Color32(31,90,228,0);

    }

    // Start is called before the first frame update
    void Start()
    {
        CalculateFielOfView();
    }
        
    /// <summary>
    /// Create/Initialize a new label
    /// </summary>
    /// <param name="_labelId"></param>
    /// <param name="_areaID"></param>
    /// <param name="_labelType"></param>
    /// <param name="_text"></param>
    /// <param name="_labelTransform"></param>
    public void Create(string _labelId, string _areaID, TLabelType _labelType, string _text, CLabelTransform _labelTransform, float _scaleFactor)
    {
        this.id = _labelId;
        this.areaId = _areaID;
        this.type = _labelType;
        this.text = _text;
        this.labelTransform = _labelTransform;
        this.scaleFactor = _scaleFactor;

        state = TLabelState.creating;

        if (type == TLabelType.boardLabel) {
            this.CreateBoard();
        } else if (type == TLabelType.anchoredLabel) {
            this.CreateAnchoredLabel(); }
        else { //Error 
        }
        state = TLabelState.idle;      // Change class state
    }

    /// <summary>
    /// Create a Board Label
    /// </summary>
    private void CreateBoard()
    {
        // Des-normalized board position        
        this.labelTransform.boardPosition = DesnormalizeBoardPosition(this.labelTransform.boardPosition);

        // Create Board
        boardGO.transform.position = this.labelTransform.boardPosition;     // Emplace position
        boardGO.transform.rotation = this.labelTransform.boardRotation;     // Apply orientation        
        boardGO.transform.Rotate(-1.0f * boardGO.transform.rotation.eulerAngles.x, 0f, 0f); // Force just vertical board

        boardGO.transform.localScale = this.scaleFactor * this.GetDefaultScaleLabelFactor() * defaultBoardGOScale;    // Size

        //textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;   // Update Text

        // resize according to the text                
        //panelGO.transform.localScale = new Vector3(panelGO.transform.localScale.x, textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + panelGOPadding, panelGO.transform.localScale.z);
        this.UpdateText(this.text);
        //Invoke("Invoke_RelocateBoard", 0.05f);
    }

    /*private void Invoke_RelocateBoard() { RelocateBoard(); }

    private void RelocateBoard()
    {
        //Reallocate board over the AR plane
        Bounds boardBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().CalculateExtern3DModelBoundingBox(this.gameObject);

        float yPos = panelGO.transform.localPosition.y + boardBounds.extents.y;
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, yPos, this.transform.localPosition.z);

        //show boards once is ready
        //this.gameObject.SetActive(true);
    }*/

    private void CreateAnchoredLabel()
    {
        // Des-normalized board position        
        this.labelTransform.boardPosition = DesnormalizeBoardPosition(this.labelTransform.boardPosition);

        // PANEL                
        boardGO.transform.position = this.labelTransform.boardPosition;             // Emplace
        this.UpdateOrientation();                                                   // Orientation        
        boardGO.transform.localScale = this.scaleFactor * this.GetDefaultScaleLabelFactor() * defaultBoardGOScale;    // Size

        // ANCHOR
        // Calculate ANCHOR position world coordinates based on data received
        GameObject areaGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(this.areaId);
        Vector3 globalAnchorPosition = areaGO.transform.TransformPoint(this.labelTransform.anchorPosition);
        labelTransform.anchorPosition = globalAnchorPosition;

        anchorGO.transform.position = this.labelTransform.anchorPosition;           // Emplace
        anchorGO.transform.localScale = this.GetDefaultScaleLabelFactor() * anchorGO.transform.localScale;
        // POLE
        LineRenderer pole = poleGO.GetComponent<LineRenderer>();        
        pole.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        pole.SetPosition(0, this.labelTransform.anchorPosition);
        pole.SetPosition(1, this.labelTransform.boardPosition);
        this.UpdatePoleVisualAspect(pole);

        // Add Text to PANEL
        this.UpdateText(this.text);
        /*textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;
        textGO.GetComponent<TextMeshPro>().ForceMeshUpdate();
        Bounds textBound = textGO.GetComponent<TextMeshPro>().bounds;                
        float xCorrection = textBound.size.x;        
        float yCorrection = textBound.size.y;       
        panelGO.transform.localScale = new Vector3(xCorrection + panelGOPadding, yCorrection + panelGOPadding, panelGO.transform.localScale.z);
        */
    }

    private Vector3 DesnormalizeBoardPosition(Vector3 position)
    {
        Bounds _3DObjectBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        float nomalizeFactor = Mathf.Sqrt(MathHom3r.Pow2(_3DObjectBounds.size.x) + MathHom3r.Pow2(_3DObjectBounds.size.y) + MathHom3r.Pow2(_3DObjectBounds.size.z));
        Vector3 boardGlobalPosition = (position * nomalizeFactor) + _3DObjectBounds.center;        

        return boardGlobalPosition;
    }

    private void UpdatePoleVisualAspect(LineRenderer pole)
    {
        // Change Pole Colour
        Color tempColor = Color.white;
        tempColor.a = 0.8f;
        pole.startColor = tempColor;        
        tempColor = Color.grey;
        tempColor.a = 0.2f;
        pole.endColor = tempColor;

        // Change Pole Width 
        float poleDefaultWidth = 0.02f;
        pole.startWidth = poleDefaultWidth * this.GetDefaultScaleLabelFactor();
        pole.endWidth   = poleDefaultWidth * this.GetDefaultScaleLabelFactor();
    }

    /// <summary>
    /// Update pole position, in the ARApp sometimes the pole lost its position. 
    /// </summary>
    public void UpdatePolePosition()
    {
        LineRenderer pole = poleGO.GetComponent<LineRenderer>();
        pole.SetPosition(0, anchorGO.transform.position);
        pole.SetPosition(1, boardGO.transform.position);
    }

    /// <summary>
    /// Calculate a scale factor in terms of 3D bounding box. 
    /// It is calculated as the x% of the size of the main axis of the bounding box
    /// </summary>
    /// <returns>Scale factor to be applied</returns>
    private float GetDefaultScaleLabelFactor()
    {
        Bounds _3DModelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        
        if (_3DModelBoundingBox.size.y > _3DModelBoundingBox.size.x)
        {            
            return (defaultScaleFactor  * _3DModelBoundingBox.size.y);
        } else {
            return (defaultScaleFactor * _3DModelBoundingBox.size.x);
        }
    }


    ////////////
    //  EDIT  //
    ////////////

    /// <summary>
    /// Set if the edit mode is activated or not. 
    /// </summary>
    /// <param name="_enabled">true means activated</param>
    public void SelectLabel(bool _enabled) {          
        if (_enabled) {
            state = TLabelState.selected;
            this.SetColor(selectedColor);
        } else {
            state = TLabelState.idle;
            selectedState = TLabelSelectedState.idle;
            this.SetColor(idleColor);
        }        
    }

    /// <summary>
    /// Update the label orientation look to the camera. Currently only used in Anchored Label
    /// </summary>
    public void UpdateOrientation()
    {
        boardGO.transform.LookAt(Camera.main.transform, Camera.main.transform.up);  // Orientation
        boardGO.transform.Rotate(0.0f, 180.0f, 0.0f);                               // Orientation
    }

    public void UpdateBoardPosition(float dragMovementX, float dragMovementY)
    {
        if (this.selectedState != TLabelSelectedState.moving) { return; }        
        // Debug.Log("UpdateBoardPosition");

        // Calculate correction parameter
        Vector3 distanceVector = boardGO.transform.position - Camera.main.transform.position;
        Vector3 normalizedZVectorCamera = Camera.main.transform.forward.normalized;
        float distanceBetweenCameraAndLabel = Vector3.Dot(distanceVector, normalizedZVectorCamera);

        float verticalAxisCorrection = Mathf.Tan(verticalFieldOfView_rad) * 2.0f * distanceBetweenCameraAndLabel;
        float horizontalAxisCorrection = Mathf.Tan(horizontalFieldOfView_rad) * 2.0f * distanceBetweenCameraAndLabel;


        // Calculate mapping of mouse x/y 
        Vector3 normalizedXVectorCamera = Camera.main.transform.right.normalized;
        Vector3 xz = normalizedXVectorCamera * horizontalAxisCorrection * dragMovementX;

        // Save new panel position
        CLabelTransform desiredPosition = new CLabelTransform();

        desiredPosition.boardPosition = boardGO.transform.localPosition + xz;
        desiredPosition.boardPosition.y = boardGO.transform.localPosition.y + verticalAxisCorrection * dragMovementY / Camera.main.transform.up.normalized.y;

        boardGO.transform.localPosition = desiredPosition.boardPosition;

        // Redraw Pole
        if (type == TLabelType.anchoredLabel)
        {
            LineRenderer pole = poleGO.GetComponent<LineRenderer>();            
            pole.SetPosition(1, boardGO.transform.position);
        }

    }

    public void UpdateBoardOrientation(float yAxisRotationAngle)
    {
        if (this.type != TLabelType.boardLabel) { return; }
        if (this.state == TLabelState.selected) 
        {
            Vector3 boardRotation = boardGO.transform.rotation.eulerAngles;
            boardRotation.y = yAxisRotationAngle;
            boardGO.transform.localEulerAngles = boardRotation;
        }        
    }

    /// <summary>
    /// Update the board scale as the default x a factor that modified it
    /// </summary>
    /// <param name="_scaleFactor"></param>
    public void UpdateBoardScale(float _scaleFactor)
    {        
        if (this.state == TLabelState.selected)
        {
            this.scaleFactor = _scaleFactor;            
            boardGO.transform.localScale = this.scaleFactor * this.GetDefaultScaleLabelFactor() * defaultBoardGOScale;            
        }
    }
    
    public void UpdateText(string _newText)
    {
        this.text = _newText;
        textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;   // Update Text
        textGO.GetComponent<TextMeshPro>().ForceMeshUpdate();
        // resize according to the text                    
        if (this.type == TLabelType.boardLabel) {         
            panelGO.transform.localScale = new Vector3(panelGO.transform.localScale.x, textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + panelGOPadding, panelGO.transform.localScale.z);

        }
        else if (this.type == TLabelType.anchoredLabel)
        {            
            Bounds textBound = textGO.GetComponent<TextMeshPro>().bounds;
            float xCorrection = textBound.size.x;
            float yCorrection = textBound.size.y;
            panelGO.transform.localScale = new Vector3(xCorrection + panelGOPadding, yCorrection + panelGOPadding, panelGO.transform.localScale.z);
        }
        
    }

    /////////////////
    //  SET/ GET   //
    /////////////////

    public string GetLabelId() { return this.id; }

    public string GetAreaId() { return areaId; }

    public TLabelType GetLabelType() { return this.type; }

    public float GetScaleFactor() { return this.scaleFactor; }

    public CLabelTransform GetLabelTransform()
    {
        CLabelTransform labelTransform = new CLabelTransform();
        labelTransform.boardPosition = boardGO.transform.position;
        labelTransform.boardRotation = boardGO.transform.rotation;
        if (this.type == TLabelType.anchoredLabel) {
            labelTransform.anchorPosition = anchorGO.transform.localPosition;
        }
        
        return labelTransform;
    }

    public CLabelTransform GetLabelLocalTransform()
    {
        CLabelTransform labelLocalTransform = new CLabelTransform();

        //Position Board
        Bounds _3DObjectBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        Vector3 boardPositionRelativeTo3DCentre = boardGO.transform.position - _3DObjectBounds.center;
        float nomalizeFactor = Mathf.Sqrt(MathHom3r.Pow2(_3DObjectBounds.size.x) + MathHom3r.Pow2(_3DObjectBounds.size.y) + MathHom3r.Pow2(_3DObjectBounds.size.z));
        Vector3 boardPositionRelativeTo3DCentreNormalized = boardPositionRelativeTo3DCentre / nomalizeFactor;

        labelLocalTransform.boardPosition = boardPositionRelativeTo3DCentreNormalized;

        // Rotation
        labelLocalTransform.boardRotation = boardGO.transform.rotation;
        // Anchor
        if (this.type == TLabelType.anchoredLabel)
        {
            GameObject areaGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(this.areaId);
            labelLocalTransform.anchorPosition = areaGO.transform.InverseTransformPoint(anchorGO.transform.position);
        }
        
        return labelLocalTransform;
    }



    public void StartMovingLabel()
    {
        this.selectedState = TLabelSelectedState.moving;
    }

    public void StopMovingLabel()
    {
        this.selectedState = TLabelSelectedState.idle;
    }

    /// <summary>Calculate the Field Of View of the camera based on the camera aspect ratio</summary>
    private void CalculateFielOfView()
    {
        verticalFieldOfView_rad = Camera.main.fieldOfView * Mathf.Deg2Rad * .5f;    //We use the half of this angle
        float cameraHeightAt1 = Mathf.Tan(verticalFieldOfView_rad);
        horizontalFieldOfView_rad = Mathf.Atan(cameraHeightAt1 * Camera.main.aspect);
    }

    private void SetColor(Color32 newColor)
    {
        this.panelGO.GetComponent<Renderer>().material.color = newColor;
    }
}
