using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CPlotLabelPosition
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class CPlotAreaData
{
    public string areaID;
    public string colour;
    public CPlotAreaData() { }
    public CPlotAreaData(string _areaID, string _colour)
    {
        this.areaID = _areaID;
        this.colour = _colour;
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
    public List<CPlotTextData> textList;
    public List<CPlotAreaData> areaList;
    public List<CPlotLabelData> labelList;

    public CPlotPoint()
    {
        areaList = new List<CPlotAreaData>();
        labelList = new List<CPlotLabelData>();
        textList = new List<CPlotTextData>();

    }
    public CPlotPoint(string _modelURL, List<CPlotTextData> _textList , List<CPlotAreaData> _selectedAreas, List<CPlotLabelData> _label)
    {
        this.productModelUrl = _modelURL;
        this.areaList = _selectedAreas;
        this.labelList = _label;
        this.textList = _textList;
    }
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
