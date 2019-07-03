using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TLabelType { board, anchoredLabel}

public class CLabelTransform
{
    public Vector3 panelPosition;
    public Quaternion panelRotation;
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
        labelPosition.panelPosition = _boardPosition;
        labelPosition.panelRotation = _boardRotation;
        
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
        //CLabelTransform labelPosition = this.GetDefaultPositionAnchoredLabel(_areaId);
        //this.AddLabel(_labelId, _areaId, TLabelType.anchoredLabel, _text, labelPosition);
        currentLabel.id = _labelId;
        currentLabel.text = _text;
        hom3r.coreLink.Do(new CPointOnSurfaceCommand(TPointOnSurfaceCommands.StartPointCapture));
    }


    public void AddAnchoredLabel(string _labelId, string _areaId, string _text, Vector3 _labelPosition, Vector3 _anchorPosition)
    {
        
        CLabelTransform labelPosition = new CLabelTransform();
        labelPosition.panelPosition = _labelPosition;
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

        // If not exit we create a new one
        GameObject newLabel = null;
        if (_labelType == TLabelType.board)
        {
            newLabel = (GameObject)Resources.Load("prefabs/Label/BoardPrefab", typeof(GameObject));
        } else if (_labelType == TLabelType.anchoredLabel) {
            newLabel = (GameObject)Resources.Load("prefabs/Label/AnchoredLabelPrefab", typeof(GameObject));
        }        
        GameObject newLabelGO = Instantiate(newLabel, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));       
        //newLabelGO.SetActive(false);                                                                         //Hide label while is being configures        

        newLabelGO.transform.parent = hom3r.quickLinks.labelsObject.transform;                              // Change parent
        newLabelGO.transform.name = "BoardPrefab_" + _labelId;
        newLabelGO.GetComponent<Label2>().Create(_labelId, _areaId, _labelType, _text, _labelPosition);     // Create the label

        labelList.Add(newLabelGO);
        hom3r.state.currentLabelMode = THom3rLabelMode.show;
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
        float pos = Mathf.Sqrt(plugyObjectBounds.extents.z * plugyObjectBounds.extents.z + plugyObjectBounds.extents.x * plugyObjectBounds.extents.x);
        local_position.z = (-1.0f) * pos;
        local_position.x = (+1.0f) * pos;

        labelPos.panelPosition = local_position;
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

        labelTransform.panelPosition = poleEnd;

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

    /// <summary>
    /// Calculate the pole end position, based on anchor, pole origin and object geometry
    /// </summary>
    /// <param name="_anchorPosition"></param>
    /// <param name="_poleOrigin"></param>
    /// <returns></returns>
    private Vector3 CalculatePoleEndPosition(Vector3 _anchorPosition, Vector3 _poleOrigin)
    {
        Bounds modelBoundingBox = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox();


        //Calculate real bounding box extents, because the object mass center could be translate (not in 0,0,0)
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
    //  SET/ GET //
    /////////////////
    public void SetColor(Color32 _labelColor)
    {

    }



    /////////////////
    //  EDIT LABEL //
    /////////////////

    public void StartEditLabel(string _labelId)
    {
        Debug.Log("StartEditLabel: " + _labelId);
        if (_labelId == null) { return; }

        GameObject labelToEdit = this.labelList.Find(r => r.GetComponent<Label2>().GetLabelId() == _labelId);
        if (labelToEdit != null)
        {
            /*GameObject labelCanvas = (GameObject)Resources.Load("prefabs/Label/LabelEditorCanvasPrefab", typeof(GameObject));
            GameObject labelCanvasGO = Instantiate(labelCanvas, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));        
            labelCanvasGO.transform.parent = hom3r.quickLinks.uiObject.transform;*/
            
            GameObject labelCanvasGO = hom3r.coreLink.InstantiatePrefab("prefabs/Label/LabelEditorCanvasPrefab", hom3r.quickLinks.uiObject);
            labelCanvasGO.GetComponent<LabelEditorManager>().Init(labelToEdit);

        }
        hom3r.state.currentLabelMode = THom3rLabelMode.edit; 
        
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
}
