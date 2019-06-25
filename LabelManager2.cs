using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TLabelType { board, anchoredLabel}

public class CLabelPosition
{
    public Vector3 panelPosition;
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
        //Recolocate panel over the AR plane
        //Bounds panelBounds = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().CalculateExtern3DModelBoundingBox(current_descriptionPanelGO);
        //float yPos = panel3D_canvas.transform.localPosition.y + panelBounds.extents.y;
        //current_descriptionPanelGO.transform.localPosition = new Vector3(current_descriptionPanelGO.transform.localPosition.x, yPos, current_descriptionPanelGO.transform.localPosition.z);
        return null;
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
        Destroy(labelToRemove);
    }
}
