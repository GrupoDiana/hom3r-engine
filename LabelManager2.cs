using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TLabelType { board, anchoredLabel}

public class CLabelPosition
{
    public Vector3 panelPosition;
    public Quaternion panelRotation;
    public Vector3 anchorPosition;
}

public class LabelManager2 : MonoBehaviour
{
    List<GameObject> labelList;
    GameObject labelEditing;

    void Awake()
    {
        labelList = new List<GameObject>();
        labelEditing = null;
    }


    /////////////////
    //  ADD LABEL //
    /////////////////

    
    public void AddBoard(string _labelId, string _text)
    {
        CLabelPosition labelPosition = this.GetDefaultPosition(TLabelType.board);
        this.AddLabel(_labelId, null, TLabelType.board, _text, labelPosition);
    }
    
    public void AddAnchoredLabel(string _labelId, string _areaId, string _text)
    {
        CLabelPosition labelPosition = this.GetDefaultPosition(TLabelType.anchoredLabel, _areaId);
        this.AddLabel(_labelId, _areaId, TLabelType.anchoredLabel, _text, labelPosition);
    }

    
    /// <summary>
    /// Add label to scene
    /// </summary>
    /// <param name="_labelId"></param>
    /// <param name="_areaId"></param>
    /// <param name="_labelType"></param>
    /// <param name="_text"></param>
    /// <param name="_labelPosition"></param>
    private void AddLabel(string _labelId, string _areaId, TLabelType _labelType, string _text, CLabelPosition _labelPosition)
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
        hom3r.state.currentLabel2Mode = THom3rLabel2Mode.showinglabel;
    }


    private CLabelPosition GetDefaultPosition(TLabelType _labelType, string _areadId = null)
    {
        if (_labelType == TLabelType.board)
        {
            return GetDefaultPositionBoard();
        }
        else if (_labelType == TLabelType.anchoredLabel)
        {
            return GetDefaultPositionAnchoredLabel(_areadId);
        }
        return null;
    }

    private CLabelPosition GetDefaultPositionBoard()
    {
        CLabelPosition labelPos = new CLabelPosition();
        Bounds plugyObjectBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().Get3DModelBoundingBox(true);

        Vector3 local_position = Vector3.zero;
        float pos = Mathf.Sqrt(plugyObjectBounds.extents.z * plugyObjectBounds.extents.z + plugyObjectBounds.extents.x * plugyObjectBounds.extents.x);
        local_position.z = (-1.0f) * pos;
        local_position.x = (+1.0f) * pos;

        labelPos.panelPosition = local_position;
        return labelPos;
    }

    private CLabelPosition GetDefaultPositionAnchoredLabel(string _areadId)
    {
        return null;
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

    public void StartEditLabel(GameObject _labelObj)
    {
        if (_labelObj == null) { return; }

        _labelObj.transform.parent.GetComponent<Label2>().SetActivateEditMode(true);

        // Instantiate general edit label canvas

        labelEditing = _labelObj;

        //Emit event
        hom3r.state.currentLabel2Mode = THom3rLabel2Mode.editinglabel;
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
        
        if (this.labelList.Count == 0) { hom3r.state.currentLabel2Mode = THom3rLabel2Mode.idle; }
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
        hom3r.state.currentLabel2Mode = THom3rLabel2Mode.idle;
    }
}
