using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelManager2CommandReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoLabelManager2Command, UndoLabelManager2Command);    //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoLabelManager2Command, UndoLabelManager2Command);  //Unsubscribe a method to the event delegate        
    }

    private void DoLabelManager2Command(CCoreCommand command)
    {
        if (command.GetType() == typeof(CLabelManager2Command)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoLabelManager2Command(CCoreCommand command)
    {
        if (command.GetType() == typeof(CLabelManager2Command)) { command.Undo(this); }
        else { /* Do nothing */ }
    }
}


/// <summary>Navigation Commands</summary>
public enum TLabelManager2Commands
{
    AddBoardLabel,  AddAnchoredLabel,
    ShowBoardLabel, ShowAnchoredLabel,
    EditLabel, UpdateLabelText,
    RemoveLabel, RemoveAllLabel,
    UpdateAnchoredLabelView
}


/// <summary>Navigation data</summary>
public class CLabelManager2CommandData
{
    public TLabelManager2Commands commandEvent;

    public string labelId { get; set; }
    public string text { get; set; }
    public string areaId { get; set; }
    public string colour { get; set; }
    public Vector3 labelPosition { get; set; }
    public Vector3 anchorPosition { get; set; }
    public Quaternion boardRotation { get; set; }
    public float scaleFactor { get; set; }


    public CLabelManager2CommandData(TLabelManager2Commands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CLabelManager2Command : CCoreCommand
{
    public CLabelManager2CommandData data;    

    //////////////////
    // Constructors //
    //////////////////
    public CLabelManager2Command(TLabelManager2Commands _command)
    {
        data = new CLabelManager2CommandData(_command);
    }

    public CLabelManager2Command(TLabelManager2Commands _command, string _labelId)
    {
        data = new CLabelManager2CommandData(_command);
        this.data.labelId = _labelId;        
    }

    public CLabelManager2Command(TLabelManager2Commands _command, string _labelId, string _text)
    {
        data = new CLabelManager2CommandData(_command);
        this.data.labelId = _labelId;
        this.data.text = _text;    
    }

    public CLabelManager2Command(TLabelManager2Commands _command, string _labelId, string _areaId, string _text, string _colour)
    {
        data = new CLabelManager2CommandData(_command);
        this.data.labelId = _labelId;
        this.data.text = _text;
        this.data.areaId = _areaId;
        this.data.colour = _colour;
    }

    public CLabelManager2Command(TLabelManager2Commands _command, string _labelId, string _text, Vector3 _labelPosition, Quaternion _boardRotation, float _scaleFactor)
    {
        data = new CLabelManager2CommandData(_command);
        this.data.labelId = _labelId;
        this.data.text = _text;
        this.data.labelPosition = _labelPosition;
        this.data.boardRotation = _boardRotation;
        this.data.scaleFactor = _scaleFactor;
    }
    public CLabelManager2Command(TLabelManager2Commands _command, string _labelId, string _areaId, string _text, Vector3 _labelPosition, Vector3 _anchorPosition, float _scaleFactor)
    {
        data = new CLabelManager2CommandData(_command);
        this.data.labelId = _labelId;
        this.data.areaId = _areaId;
        this.data.text = _text;
        this.data.labelPosition = _labelPosition;
        this.data.anchorPosition = _anchorPosition;
        this.data.scaleFactor = _scaleFactor;
    }

    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (!hom3r.state.navigationBlocked)
        {
            if (data != null)
            {
                switch (data.commandEvent)
                {
                    case TLabelManager2Commands.AddBoardLabel:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().AddBoard(data.labelId, data.text);
                        break;
                    case TLabelManager2Commands.AddAnchoredLabel:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().AddAnchoredLabel(data.labelId, data.areaId, data.text, data.colour);
                        break;

                    case TLabelManager2Commands.ShowBoardLabel:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().AddBoard(data.labelId, data.text, data.labelPosition, data.boardRotation, data.scaleFactor);
                        break;
                    case TLabelManager2Commands.ShowAnchoredLabel:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().AddAnchoredLabel(data.labelId, data.areaId, data.text, data.labelPosition, data.anchorPosition, data.scaleFactor);
                        break;
                    case TLabelManager2Commands.RemoveLabel:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().RemoveLabel(data.labelId);
                        break;
                    case TLabelManager2Commands.RemoveAllLabel:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().RemoveAllLabel();
                        break;
                    case TLabelManager2Commands.UpdateAnchoredLabelView:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().UpdateAnchoredLabelsOrientationAndPole();
                        break;
                    case TLabelManager2Commands.UpdateLabelText:
                        hom3r.quickLinks.scriptsObject.GetComponent<LabelManager2>().UpdateLabelText(data.labelId, data.text);
                        break;
                    default:
                        Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                        break;
                }
            }
            else
            {
                Debug.LogError("Error: Has been called a Point Capture command without command.");
            }
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }
}