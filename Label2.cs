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


    float scaleFactor = 0.1f;       // TODO: Think better idea. Constant to calculate the scaleFactor of the label/panel
    float panelGOPadding = 0.1f;    // Top/Bottom margin between panel and text
    Color32 idleColor = Color.white;
    Color32 selectedColor = Color.red;

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
    public void Create(string _labelId, string _areaID, TLabelType _labelType, string _text, CLabelTransform _labelTransform)
    {
        this.id = _labelId;
        this.areaId = _areaID;
        this.type = _labelType;
        this.text = _text;
        this.labelTransform = _labelTransform;

        state = TLabelState.creating;

        if (type == TLabelType.board) { this.CreateBoard();
        } else if (type == TLabelType.anchoredLabel) { this.CreateAnchoredLabel(); }
        else { //Error 
        }

        state = TLabelState.idle;      // Change class state
    }

    /// <summary>
    /// Create a Board Label
    /// </summary>
    private void CreateBoard()
    {
        boardGO.transform.position = this.labelTransform.boardPosition;     // Emplace position
        this.UpdateOrientation();                                           // Change orientation        
        boardGO.transform.Rotate(-1.0f * boardGO.transform.rotation.eulerAngles.x, 0f, 0f); // Force just vertical board

        boardGO.transform.localScale = this.GetLabeScaleFactor() * boardGO.transform.localScale;    // Size

        textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;   // Update Text

        // resize according to the text        
        Debug.Log(this.transform.name + ": " + textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + " - " + panelGOPadding);
        panelGO.transform.localScale = new Vector3(panelGO.transform.localScale.x, textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + panelGOPadding, panelGO.transform.localScale.z);

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
        // PANEL                
        boardGO.transform.position = this.labelTransform.boardPosition;             // Emplace
        this.UpdateOrientation();                                                   // Orientation        
        boardGO.transform.localScale = this.GetLabeScaleFactor() * boardGO.transform.localScale;    // Size
        
        // ANCHOR
        anchorGO.transform.position = this.labelTransform.anchorPosition;           // Emplace
        
        // POLE
        LineRenderer pole = poleGO.GetComponent<LineRenderer>();        
        pole.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        pole.SetPosition(0, this.labelTransform.anchorPosition);
        pole.SetPosition(1, this.labelTransform.boardPosition);
        this.UpdatePoleVisualAspect(pole);

        // Add Text to PANEL
        textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;
        textGO.GetComponent<TextMeshPro>().ForceMeshUpdate();
        Bounds textBound = textGO.GetComponent<TextMeshPro>().bounds;
        Debug.Log(textBound.size.x);
        Debug.Log(textBound.size.y);
        //float yCorrection = textGO.GetComponent<TextMeshPro>().GetPreferredValues().y; // DOESN'T WORK
        //float xCorrection = textBound.size.x;
        //float xCorrection = textGO.GetComponent<TextMeshPro>().GetPreferredValues().x;
        float yCorrection = textBound.size.y;
        //if (xCorrection > 0.8f ) { xCorrection = 1.0f; }

        // Resize PANEL according text          
        //Debug.Log(this.transform.name + ": " + textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + " - " + panelGOPadding);
        panelGO.transform.localScale = new Vector3(panelGO.transform.localScale.x, yCorrection + panelGOPadding, panelGO.transform.localScale.z);
        //panelGO.transform.localScale = new Vector3(xCorrection, yCorrection + panelGOPadding, panelGO.transform.localScale.z);

        //textGO.transform.localScale = new Vector3(xCorrection - 0.1f, textGO.transform.localScale.y, textGO.transform.localScale.z);
        //textGO.GetComponent<TextMeshPro>().margin = new Vector4((1 - xCorrection) *0.5f, textGO.GetComponent<TextMeshPro>().margin.y, textGO.GetComponent<TextMeshPro>().margin.z, textGO.GetComponent<TextMeshPro>().margin.w);


    }


    private void UpdatePoleVisualAspect(LineRenderer pole)
    {
        // Change Pole Colour
        Color temp = Color.white;
        temp.a = 0.8f;
        pole.startColor = temp;        
        temp = Color.grey;
        temp.a = 0.2f;
        pole.endColor = temp;

        // Change Pole Width 
        Debug.Log(pole.startWidth);
        pole.startWidth = 0.2f;
        pole.endWidth = 0.2f;
    }

    private float GetLabeScaleFactor()
    {
        Bounds _3DModelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        
        if (_3DModelBoundingBox.size.y > _3DModelBoundingBox.size.x)
        {
            
            return (scaleFactor * _3DModelBoundingBox.size.y);
        } else {

            return (scaleFactor * _3DModelBoundingBox.size.x);
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
        // Check if we are editing
        //if (this.state != TLabelState.editing) { return; }
        Debug.Log("UpdateBoardPosition");

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
        if (this.type != TLabelType.board) { return; }
        if (this.state == TLabelState.selected) 
        {
            Vector3 boardRotation = boardGO.transform.rotation.eulerAngles;
            boardRotation.y = yAxisRotationAngle;
            boardGO.transform.localEulerAngles = boardRotation;
        }        
    }
    /////////////////
    //  SET/ GET   //
    /////////////////

    public string GetLabelId() { return this.id; }

    public TLabelType GetLabelType() { return this.type; }


    public CLabelTransform GetLabelTransform()
    {
        CLabelTransform labelTransform = new CLabelTransform();
        labelTransform.boardPosition = boardGO.transform.position;
        labelTransform.boardRotation = boardGO.transform.rotation;
        //labelTransform.anchorPosition = asdas ;

        return labelTransform;
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
