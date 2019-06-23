using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCaptureCommandReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoPointCaptureCommand, UndoPointCaptureCommand);    //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoPointCaptureCommand, UndoPointCaptureCommand);  //Unsubscribe a method to the event delegate        
    }

    private void DoPointCaptureCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CNavigationCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoPointCaptureCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CNavigationCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }
}

/// <summary>Navigation Commands</summary>
public enum TPointCaptureCommands
{
    StartPointCapture, DrawAnchorPoint
}

/// <summary>Navigation data</summary>
public class CPointCaptureCommandData
{
    public TPointCaptureCommands commandEvent;

    public string areaId { get; set; }
    public Vector3 pointPosition { get; set; }

    public CPointCaptureCommandData(TPointCaptureCommands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CTPointCaptureCommand : CCoreCommand
{
    public CPointCaptureCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CTPointCaptureCommand(TPointCaptureCommands _command)
    {
        data = new CPointCaptureCommandData(_command);
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
                    case TPointCaptureCommands.StartPointCapture:
                        
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