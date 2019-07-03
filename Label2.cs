using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum TLabelState { iddle, creating, editing }

public class Label2 : MonoBehaviour
{
    string id;
    TLabelType type;
    string areaId;
    string text;
    CLabelTransform labelTransform;    
    TLabelState state;

    GameObject textGO;
    GameObject panelGO;
    GameObject poleGO;
    GameObject anchorGO;

    float verticalFieldOfView_rad;      // 
    float horizontalFieldOfView_rad;    // 

    private void Awake()
    {
        textGO = this.transform.Find("TextMeshPro").gameObject;
        panelGO = this.transform.Find("Panel3D").gameObject;

        if (this.transform.Find("Pole")) {
            poleGO = this.transform.Find("Pole").gameObject;
        }
        if (this.transform.Find("Anchor")) {
            anchorGO = this.transform.Find("Anchor").gameObject;
        }        
        state = TLabelState.creating;       
    }

    // Start is called before the first frame update
    void Start()
    {
        CalculateFielOfView();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void Create(string _labelId, string _areaID, TLabelType _labelType, string _text, CLabelTransform _labelTransform)
    {
        this.id = _labelId;
        this.areaId = _areaID;
        this.type = _labelType;
        this.text = _text;
        this.labelTransform = _labelTransform;


        if (type == TLabelType.board)
        {
            this.CreateBoard();
        } else if (type == TLabelType.anchoredLabel)
        {
            // Whatever
            this.CreateAnchoredLabel();
        } else
        {
            //Error
        }

        state = TLabelState.iddle;
    }

    private void CreateBoard()
    {

        this.transform.position = this.labelTransform.panelPosition;
        this.transform.localRotation = labelTransform.panelRotation;

        textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;
        
        // resize according to the text
        float upDownPadding = 0.1f;
        panelGO.transform.localScale = new Vector3(panelGO.transform.localScale.x, textGO.GetComponent<TextMeshPro>().GetPreferredValues().y + upDownPadding, panelGO.transform.localScale.z);

        Invoke("Invoke_RelocateBoard", 0.05f);
    }

    private void Invoke_RelocateBoard() { RelocateBoard(); }

    private void RelocateBoard()
    {
        //Reallocate board over the AR plane
        Bounds boardBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().CalculateExtern3DModelBoundingBox(this.gameObject);

        float yPos = panelGO.transform.localPosition.y + boardBounds.extents.y;
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, yPos, this.transform.localPosition.z);

        //show boards once is ready
        //this.gameObject.SetActive(true);
    }

    private void CreateAnchoredLabel()
    {
        // this.transform.position = this.labelTransform.panelPosition;
        panelGO.transform.position = this.labelTransform.panelPosition;
        anchorGO.transform.position = this.labelTransform.anchorPosition;
        
        // this.transform.localRotation = this.labelTransform.panelRotation;
        this.transform.LookAt(Camera.main.transform, Camera.main.transform.up);


        LineRenderer pole = poleGO.GetComponent<LineRenderer>();
        pole.SetPosition(0, this.labelTransform.anchorPosition);
        pole.SetPosition(1, this.labelTransform.panelPosition);

        textGO.GetComponent<TextMeshPro>().GetComponent<TMP_Text>().text = this.text;
    }



    ////////////
    //  EDIT  //
    ////////////

    /// <summary>
    /// Set if the edit mode is activated or not. 
    /// </summary>
    /// <param name="_enabled">true means activated</param>
    public void SetActivateEditMode(bool _enabled) {   
        if (state != TLabelState.iddle) { return; }
        if (_enabled)
        {
            state = TLabelState.editing;
        } else
        {
            state = TLabelState.iddle;
        }
        
    }

    
    /////////////////
    //  SET/ GET   //
    /////////////////

    public string GetLabelId() { return this.id; }
    

    public CLabelTransform GetLabelPosition() { return this.labelTransform; }

    public void SetLabelPosition(float dragMovementX, float dragMovementY)
    {
        // Calculate correction parameter
        Vector3 distanceVector = this.transform.position - Camera.main.transform.position;        
        Vector3 normalizedZVectorCamera = Camera.main.transform.forward.normalized;        
        float distanceBetweenCameraAndLabel = Vector3.Dot(distanceVector, normalizedZVectorCamera);

        float a = Mathf.Tan(verticalFieldOfView_rad) * 2.0f * distanceBetweenCameraAndLabel;
        float b = Mathf.Tan(horizontalFieldOfView_rad) * 2.0f * distanceBetweenCameraAndLabel;


        // Calculate mapping of mouse x/y 
        Vector3 normalizedXVectorCamera = Camera.main.transform.right.normalized;
        Vector3 xz = normalizedXVectorCamera * b *dragMovementX;
        
        // Save new panel position
        CLabelTransform desiredPosition = new CLabelTransform();

        desiredPosition.panelPosition = this.transform.localPosition + xz;
        desiredPosition.panelPosition.y = this.transform.localPosition.y + a * dragMovementY / Camera.main.transform.up.normalized.y;

        this.transform.localPosition = desiredPosition.panelPosition;
    }


    /// <summary>Calculate the Field Of View of the camera based on the camera aspect ratio</summary>
    private void CalculateFielOfView()
    {
        verticalFieldOfView_rad = Camera.main.fieldOfView * Mathf.Deg2Rad * .5f;    //We use the half of this angle
        float cameraHeightAt1 = Mathf.Tan(verticalFieldOfView_rad);
        horizontalFieldOfView_rad = Mathf.Atan(cameraHeightAt1 * Camera.main.aspect);
    }
}
