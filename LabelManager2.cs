using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TLabelType { board, anchoredLabel}

public class CLabelTransform
{
    public Vector3 boardPosition;
    public Quaternion boardRotation;
    public Vector3 anchorPosition;
}

public class CLabelData
{
    public string id;
    public string text;
}

public class LabelManager2 : MonoBehaviour
{
    List<GameObject> labelList;    
    CLabelData currentLabel;        // Store the label ID during the anchor point position capture
    GameObject selectedLabel;
    GameObject labelCanvasGO;       // Label Editor Canvas

    void Awake()
    {
        labelList = new List<GameObject>();
        currentLabel = new CLabelData();        
    }


    /////////////////
    //  ADD LABEL //
    /////////////////

    /// <summary>
    /// Add a new label of type board into the scene. The board will be draw in a default position/orientation.
    /// </summary>
    /// <param name="_labelId">Id assign to the board</param>
    /// <param name="_text">Text to show into the board</param>
    public void AddBoard(string _labelId, string _text)
    {
        hom3r.state.currentLabelMode = THom3rLabelMode.add;
        CLabelTransform labelPosition = this.GetDefaultPositionBoard();
        this.AddLabel(_labelId, null, TLabelType.board, _text, labelPosition);
    }

    /// <summary>
    /// Add a new label of type board into the scene. The board will be draw in the position/orientation indicated.
    /// </summary>
    /// <param name="_labelId">Id assign to the board</param>
    /// <param name="_text">Text to show into the board</param>
    /// <param name="_boardPosition">Position in which the label will be draw</param>
    /// <param name="_boardRotation">Orientation in which the label will be draw</param>
    public void AddBoard(string _labelId, string _text, Vector3 _boardPosition, Quaternion _boardRotation)
    {
        CLabelTransform labelPosition = new CLabelTransform();
        labelPosition.boardPosition = _boardPosition;
        labelPosition.boardRotation = _boardRotation;
        
        this.AddLabel(_labelId, null, TLabelType.board, _text, labelPosition);
    }


    /// <summary>
    /// Add a new label into the scene. The label will be draw in a default position/orientation based...
    /// </summary>
    /// <param name="_labelId"></param>
    /// <param name="_areaId"></param>
    /// <param name="_text"></param>
    public void AddAnchoredLabel(string _labelId, string _areaId, string _text)
    {
        if (hom3r.state.currentLabelMode == THom3rLabelMode.add) { return; }

        if (hom3r.state.currentLabelMode == THom3rLabelMode.edit)
        {
            this.OnClickLabelCloseButton();
        }

        hom3r.state.currentLabelMode = THom3rLabelMode.add;
        currentLabel.id = _labelId;     // Save to use later
        currentLabel.text = _text;      // Save to use later
        hom3r.coreLink.Do(new CPointOnSurfaceCommand(TPointOnSurfaceCommands.StartPointCapture));
    }


    public void AddAnchoredLabel(string _labelId, string _areaId, string _text, Vector3 _labelPosition, Vector3 _anchorPosition)
    {        
        CLabelTransform labelPosition = new CLabelTransform();
        labelPosition.boardPosition = _labelPosition;
        labelPosition.anchorPosition = _anchorPosition;

        this.AddLabel(_labelId, _areaId, TLabelType.anchoredLabel, _text, labelPosition);
    }


    public void AfterAnchorPointCapture(Vector3 _anchorPosition, string _areaId)
    {

        Debug.Log("AfterAnchorPointCapture: " + _anchorPosition + " - " + _areaId);
        CLabelTransform labelPosition = this.GetDefaultPositionAnchoredLabel(_anchorPosition, _areaId);
        this.AddLabel(currentLabel.id, _areaId, TLabelType.anchoredLabel, currentLabel.text, labelPosition);
    }

    /// <summary>
    /// Add label to scene
    /// </summary>
    /// <param name="_labelId"></param>
    /// <param name="_areaId"></param>
    /// <param name="_labelType"></param>
    /// <param name="_text"></param>
    /// <param name="_labelPosition"></param>
    private void AddLabel(string _labelId, string _areaId, TLabelType _labelType, string _text, CLabelTransform _labelPosition)
    {
        // Check if this labelID already exist
        if (this.labelList.Find(r => r.GetComponent<Label2>().GetLabelId() == _labelId) != null) { return; }        

        // If not this label ID doesn't exit we'll create a new one        
        GameObject newLabelGO = null;
        if (_labelType == TLabelType.board) {        
            newLabelGO = hom3r.coreLink.InstantiatePrefab("prefabs/Label/BoardPrefab", hom3r.quickLinks.labelsObject);
            newLabelGO.transform.name = "Board_" + _labelId;
        } else if (_labelType == TLabelType.anchoredLabel) {            
            newLabelGO = hom3r.coreLink.InstantiatePrefab("prefabs/Label/AnchoredLabelPrefab", hom3r.quickLinks.labelsObject);
            newLabelGO.transform.name = "AnchoredLabel_" + _labelId;
        } else
        {
            return;
        }        
        // Initialize the new label
        newLabelGO.GetComponent<Label2>().Create(_labelId, _areaId, _labelType, _text, _labelPosition);
        
        labelList.Add(newLabelGO);        //Add to label list
        
        //Update core and emit events
        hom3r.state.currentLabelMode = THom3rLabelMode.idle;
        this.EmitLabelTransform(newLabelGO);
    }

    

    /// <summary>
    /// Calculate a default position/rotation to emplace a board label on the scene
    /// </summary>
    /// <returns></returns>
    private CLabelTransform GetDefaultPositionBoard()
    {
        CLabelTransform labelPos = new CLabelTransform();
        Bounds plugyObjectBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox(true);

        Vector3 local_position = Vector3.zero;        
        float pos = Mathf.Sqrt(MathHom3r.Pow2(plugyObjectBounds.extents.z) + MathHom3r.Pow2(plugyObjectBounds.extents.x));

        local_position.z = (-1.0f) * pos;
        local_position.x = (+1.0f) * pos;

        labelPos.boardPosition = local_position;
        return labelPos;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_areadId"></param>
    /// <returns></returns>
    private CLabelTransform GetDefaultPositionAnchoredLabel(Vector3 _localAnchorPosition, string _areadId)
    {
        CLabelTransform labelTransform = new CLabelTransform();

        // Calculate ANCHOR position world coordinates based on data received
        GameObject areaGO = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(_areadId);
        labelTransform.anchorPosition = areaGO.transform.TransformPoint(_localAnchorPosition);

        Debug.Log(labelTransform.anchorPosition);
        // Calculate POLE origin
        Vector3 poleOrigin = CalculatePoleOriginPosition(labelTransform.anchorPosition);

        // Calculate POLE end        
        Vector3 poleEnd = CalculatePoleEndPosition(labelTransform.anchorPosition, poleOrigin);

        labelTransform.boardPosition = poleEnd;

        return labelTransform;
    }

    /// <summary>
    /// Calculate the pole origin, based on the anchor position and object geometry
    /// </summary>
    /// <param name="anchorPosition"></param>
    /// <returns></returns>
    private Vector3 CalculatePoleOriginPosition(Vector3 anchorPosition)
    {
        Bounds modelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();
        Vector3 cuttingPointWithAxis;

        float a;

        if (modelBoundingBox.size.x > modelBoundingBox.size.y)
        {
            ///////////////////////
            // horizontal object //
            ///////////////////////
            //Move to centre in order to do the calculations, because the bounding box could not be centred in origin (0,0)             
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

            //Move back to its original position, because the bounding box could not be centred in origin (0,0)             
            cuttingPointWithAxis = cuttingPointWithAxis + modelBoundingBox.center;
        }
        else
        {
            ///////////////////////
            // vertical object //
            ///////////////////////

            //Move to centre in order to do the calculations, because the bounding box could not be centred in origin (0,0)             
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

            //Move back to its original position, because the bounding box could not be centred in origin (0,0)             
            cuttingPointWithAxis = cuttingPointWithAxis + modelBoundingBox.center;
        }
        return cuttingPointWithAxis;
    }

    /// <summary>
    /// Calculate the pole end position, based on anchor, pole origin and object geometry
    /// </summary>
    /// <param name="_anchorPosition"></param>
    /// <param name="_poleOrigin"></param>
    /// <returns></returns>
    private Vector3 CalculatePoleEndPosition(Vector3 _anchorPosition, Vector3 _poleOrigin)
    {
        Bounds modelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();


        //Calculate real bounding box extents, because the object mass centre could be translate (not in 0,0,0)
        Vector3 modelBoundingBox_halfSize = modelBoundingBox.size * 0.5f;

        // Set direction of pole                       
        Vector3 poleDirection = _anchorPosition - _poleOrigin;
        poleDirection.Normalize();

        float poleLength;
        if (modelBoundingBox.size.x > modelBoundingBox.size.y)
        {
            //Horizontal
            poleLength = Mathf.Sqrt(MathHom3r.Pow2(modelBoundingBox.size.z) + MathHom3r.Pow2(modelBoundingBox.size.y));

        }
        else
        {
            //Vertical
            poleLength = Mathf.Sqrt(MathHom3r.Pow2(modelBoundingBox.size.z) + MathHom3r.Pow2(modelBoundingBox.size.x));
        }

        //poleLength *= 0.5f;
        poleLength -= Vector3.Distance(modelBoundingBox.center, _anchorPosition);
       
        Vector3 poleEnd = _anchorPosition + poleDirection * poleLength;

        return poleEnd;
    }

    /////////////////
    //  EDIT LABEL //
    /////////////////

    /// <summary>
    /// Start to edit a new label
    /// </summary>
    /// <param name="_newPanelToEdit">Pointer to the label object to edit</param>
    public void StartEditLabel(GameObject _newPanelToEdit)
    {
        // Get pointer to selected label
        GameObject _newSelectedLabel = _newPanelToEdit.transform.parent.parent.gameObject;
        // Check that everything is OK
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetActiveLabelEdition()) { return; }
        if (_newSelectedLabel == null) { return; }
        if (_newPanelToEdit.transform.parent.gameObject.name != "Board") { return; }
        
        //Start selection process
        if (hom3r.state.currentLabelMode == THom3rLabelMode.idle)
        {
            // Currently we are no editing any label            
            hom3r.state.currentLabelMode = THom3rLabelMode.edit;
            labelCanvasGO = InstantiateLabelEditorCanvas();         // Show canvas
            this.selectedLabel = _newSelectedLabel;                 // Save selected label           
        }
        else if ((hom3r.state.currentLabelMode == THom3rLabelMode.edit) && this.selectedLabel != _newSelectedLabel)
        {
            // We have change the selected label
            this.selectedLabel.GetComponent<Label2>().SelectLabel(false);   // Change label state to NO-selected state
            this.selectedLabel = _newSelectedLabel;                         // Save NEW selected label            
        }
        this.updateCanvasRotationSlider(this.selectedLabel.GetComponent<Label2>().GetLabelType());
        this.selectedLabel.GetComponent<Label2>().SelectLabel(true);    // Change label state to selected state
        this.selectedLabel.GetComponent<Label2>().StartMovingLabel();   // Change label state to moving
    }

    /// <summary>
    /// Finish label drag gesture
    /// </summary>
    public void StopDragLabelLabel()
    {        
        if (this.selectedLabel == null) { return; }
        this.selectedLabel.GetComponent<Label2>().StopMovingLabel();   // Change label state to idle
        this.EmitLabelTransform();
    }

    /// <summary>
    /// Execute label drag movements based. The real movement of the label will depend of 
    /// camera position and object size on the screen
    /// </summary>
    /// <param name="dragMovementX">Label movement in X axis </param>
    /// <param name="dragMovementY">Label movement in Y axis</param>
    public void DragLabel(float dragMovementX, float dragMovementY)
    {
        if (hom3r.state.currentLabelMode != THom3rLabelMode.edit) { return; }        
        this.selectedLabel.GetComponent<Label2>().UpdateBoardPosition(dragMovementX, dragMovementY);
    }

    public void UpdateAnchoredLabelsOrientation()
    {
        foreach (GameObject label in labelList)
        {
            if (label.GetComponent<Label2>().GetLabelType() == TLabelType.anchoredLabel)
            {
                label.GetComponent<Label2>().UpdateOrientation();
            }
        }
    }

    ///////////////////
    // CANVAS       ///
    //////////////////
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private GameObject InstantiateLabelEditorCanvas()
    {
        GameObject tempGO = hom3r.coreLink.InstantiatePrefab("prefabs/Label/LabelEditorCanvasPrefab", hom3r.quickLinks.uiObject);        
        tempGO.transform.Find("Panel_LabelEditor").gameObject.transform.Find("Slider").gameObject.GetComponent<Slider>().onValueChanged.AddListener(OnLabelEditorSliderChange);
        tempGO.transform.Find("Panel_LabelEditor").gameObject.transform.Find("ImageClose").gameObject.GetComponent<Button>().onClick.AddListener(OnClickLabelCloseButton);
        return tempGO;
    }

    private void OnLabelEditorSliderChange(float value)
    {       
        selectedLabel.GetComponent<Label2>().UpdateBoardOrientation(value);        
    } 

    private void OnClickLabelCloseButton()
    {
        selectedLabel.GetComponent<Label2>().SelectLabel(false);       
        Destroy(labelCanvasGO);
        selectedLabel = null;
        hom3r.state.currentLabelMode = THom3rLabelMode.idle;
    }

    private void updateCanvasRotationSlider(TLabelType _labelType)
    {
        if (_labelType == TLabelType.board)
        {
            labelCanvasGO.transform.Find("Panel_LabelEditor").gameObject.transform.Find("Slider").gameObject.SetActive(true);
            labelCanvasGO.transform.Find("Panel_LabelEditor").gameObject.transform.Find("Slider").gameObject.GetComponent<Slider>().value = this.selectedLabel.GetComponent<Label2>().GetLabelTransform().boardRotation.eulerAngles.y;
        } else
        {
            labelCanvasGO.transform.Find("Panel_LabelEditor").gameObject.transform.Find("Slider").gameObject.SetActive(false);
        }
        
    }   

    ///////////////////
    //  REMOVE LABEL //
    ///////////////////


    /// <summary>
    /// Remove one label from the scene
    /// </summary>
    /// <param name="_labelId"></param>
    public void RemoveLabel(string _labelId)
    {
        GameObject labelToRemove = this.labelList.Find(r => r.GetComponent<Label2>().GetLabelId() == _labelId);
        if (labelToRemove != null)
        {
            Destroy(labelToRemove);
            this.labelList.Remove(labelToRemove);
        }    
        
        if (this.labelList.Count == 0) { hom3r.state.currentLabelMode = THom3rLabelMode.idle; }
    }

    /// <summary>
    /// Remove all the labels from the scene
    /// </summary>
    public void RemoveAllLabel()
    {
        foreach (GameObject label in this.labelList)
        {
            Destroy(label);
        }
        this.labelList.Clear();
        hom3r.state.currentLabelMode = THom3rLabelMode.idle;
    }


    /////////////////
    //  OTHERS //
    /////////////////

    /// <summary>
    /// Emit label transform after to rest of the hom3r
    /// </summary>
    private void EmitLabelTransform()
    {
        if (this.selectedLabel == null) { return; }
        this.EmitLabelTransform(this.selectedLabel);
    }

    private void EmitLabelTransform(GameObject labelGO)
    {
        CLabelTransform labelTransform = labelGO.GetComponent<Label2>().GetLabelTransform();
        string labelId = labelGO.GetComponent<Label2>().GetLabelId();
        TLabelType labelType = labelGO.GetComponent<Label2>().GetLabelType();

        if (labelType == TLabelType.board)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.LabelManager_LabelTransform, labelId, labelTransform.boardPosition, labelTransform.boardRotation));
        } else if (labelType == TLabelType.anchoredLabel)
        {

        }
        
    }
}
