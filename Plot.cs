using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CPlotLabelPosition
{
    public float x;
    public float y;
    public float z;

    public CPlotLabelPosition(float _x, float _y, float _z)
    {
        this.x = _x;
        this.y = _y;
        this.z = _z;
    }
}

[System.Serializable]
public class CPlotSelectedAreaData
{
    public string areaID;
    public string colour;
    public CPlotSelectedAreaData() { }
    public CPlotSelectedAreaData(string _areaID, string _colour)
    {
        this.areaID = _areaID;
        this.colour = _colour;
    }
}

[System.Serializable]
public class CPlotHiddenAreaData
{
    public string id;
    public CPlotHiddenAreaData() { }
    public CPlotHiddenAreaData(string _areaID)
    {
        this.id = _areaID;
    }
}

[System.Serializable]
public class CPlotLabelData
{
    public string areaID;
    public string text;
    public string language;
    public CPlotLabelPosition position;

    public CPlotLabelData() { }

    public CPlotLabelData(string _areaID, string _text, string _language, CPlotLabelPosition _position)
    {
        this.areaID = _areaID;
        this.text = _text;
        this.language = _language;
        this.position = _position;
    }
}

[System.Serializable]
public class CPlotViewfinderData
{
    public int id;
    public CPlotLabelPosition position;
    public float radious;

    public CPlotViewfinderData() { }

    public CPlotViewfinderData(int _id, CPlotLabelPosition _position, float _radious)
    {
        this.id = _id;
        this.position = _position;
        this.radious = _radious;
    }
    public CPlotViewfinderData(int _id, float _positionX, float _positionY, float _positionZ, float _radious)
    {
        position = new CPlotLabelPosition(_positionX, _positionY, _positionZ);        
        this.radious = _radious;
    }
}

[System.Serializable]
public class CPlotTextData
{
    public string title;
    public string text;
    public string language;

    public CPlotTextData() { }
}

[System.Serializable]
public class CPlotPoint
{    
    public string productModelUrl;
    public string plotUrl;
    public List<CPlotTextData> textList;
    public List<CPlotSelectedAreaData> selectedAreaList;
    public List<CPlotHiddenAreaData> hiddenAreaList;
    public List<CPlotLabelData> labelList;
    public List<CPlotViewfinderData> viewfinderList;

    /*public CPlotPoint()
    {
        selectedAreaList = new List<CPlotSelectedAreaData>();
        labelList = new List<CPlotLabelData>();
        textList = new List<CPlotTextData>();

    }*/
    /*public CPlotPoint(string _modelURL, List<CPlotTextData> _textList , List<CPlotSelectedAreaData> _selectedAreaList, List<CPlotLabelData> _label)
    {
        this.productModelUrl = _modelURL;
        this.selectedAreaList = _selectedAreaList;
        this.labelList = _label;
        this.textList = _textList;
    }*/
}

[System.Serializable]
public class CPlot
{
    public string title;
    public string description;
    public string type;
    public List<CPlotPoint> plotPointList;

    public CPlot() { plotPointList = new List<CPlotPoint>(); }
    public CPlot(string _title, string _descrition, string _type, List<CPlotPoint> _plotPointList)
    {
        this.title = _title;
        this.description = _descrition;
        this.type = _type;
        this.plotPointList = _plotPointList;
    }
    public void Clear()
    {
        plotPointList.Clear();
        title = null;
        description = null;
        type = null;
    }
}
