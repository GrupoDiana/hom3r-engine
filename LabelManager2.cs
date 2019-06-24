using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TLabelType { billboard, anchoredLabel}

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

    public void AddBillboard(string _labelId, string _text)
    {
        CLabelPosition labelPosition = this.GetDefaultPosition(TLabelType.billboard);
        this.AddLabel(_labelId, null, TLabelType.billboard, _text, labelPosition);
    }
    
    public void AddAnchoredLabel(string _labelId, string _areaId, string _text)
    {
        CLabelPosition labelPosition = this.GetDefaultPosition(TLabelType.anchoredLabel, _areaId);
        this.AddLabel(_labelId, _areaId, TLabelType.anchoredLabel, _text, labelPosition);
    }

    private void AddLabel(string _labelId, string _areaId, TLabelType _labelType, string _text, CLabelPosition _labelPosition)
    {
        GameObject newLabel = null;
        if (_labelType == TLabelType.billboard)
        {
            newLabel = (GameObject)Resources.Load("prefabs/Label/BillboardPrefab", typeof(GameObject));
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
        if (_labelType == TLabelType.billboard)
        {
            return GetDefaultPositionBillboard();
        }
        else if (_labelType == TLabelType.anchoredLabel)
        {
            return GetDefaultPositionAnchoredLabel(_areadId);
        }
        return null;
    }

    private CLabelPosition GetDefaultPositionBillboard()
    {
        return null;
    }

    private CLabelPosition GetDefaultPositionAnchoredLabel(string _areadId)
    {
        return null;
    }


    /////////////////
    //  SET/GETL //
    /////////////////
    public void SetColor(Color32 _labelColor)
    {

    }
    
}
