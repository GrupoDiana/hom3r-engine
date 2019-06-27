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

    void Awake()
    {
        labelList = new List<GameObject>();
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

    private void AddLabel(string _labelId, string _areaId, TLabelType _labelType, string _text, CLabelPosition _labelPosition)
    {
        GameObject newLabel = null;
        if (_labelType == TLabelType.board)
        {
            newLabel = (GameObject)Resources.Load("prefabs/Label/BoardPrefab", typeof(GameObject));
        } else if (_labelType == TLabelType.anchoredLabel) {
            newLabel = (GameObject)Resources.Load("prefabs/Label/AnchoredLabelPrefab", typeof(GameObject));
        }        
        GameObject newLabelGO = Instantiate(newLabel, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));

        newLabelGO.transform.parent = hom3r.quickLinks.labelsObject.transform;                  // Change parent
        newLabelGO.GetComponent<Label2>().Create(_labelId, _areaId, _labelType, _text, _labelPosition);   // Create the label
        labelList.Add(newLabelGO);
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
        local_position.z = (-1.0f) * pos;// (plugyObjectBounds.extents.z);// + Mathf.Abs(plugyObjectBounds.center.z));// * pluggy3DModelScale;
        local_position.x = (+1.0f) * pos;// (plugyObjectBounds.extents.x);// + Mathf.Abs(plugyObjectBounds.center.x));// * pluggy3DModelScale;
        Quaternion local_rotation = Camera.main.transform.localRotation;

        labelPos.panelPosition = local_position;
        labelPos.panelRotation = local_rotation;
               
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
    


    public void RemoveLabel(string _labelId)
    {
        GameObject labelToRemove = this.labelList.Find(r => r.GetComponent<Label2>().GetLabelId() == _labelId);
        if (labelToRemove != null)
        {
            Destroy(labelToRemove);
            this.labelList.Remove(labelToRemove);
        }                
    }

    public void RemoveAllLabel()
    {
        foreach (GameObject label in this.labelList)
        {
            Destroy(label);
        }
        this.labelList.Clear();
    }
}
