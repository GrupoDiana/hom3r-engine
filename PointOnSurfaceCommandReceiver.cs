using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnSurfaceCommandReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoPointOnSurfaceCommand, UndoPointOnSurfaceCommand);    //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoPointOnSurfaceCommand, UndoPointOnSurfaceCommand);  //Unsubscribe a method to the event delegate        
    }

    private void DoPointOnSurfaceCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CPointOnSurfaceCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoPointOnSurfaceCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CPointOnSurfaceCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }
}

/// <summary>Navigation Commands</summary>
public enum TPointOnSurfaceCommands
{
    StartPointCapture, DrawAnchorPoint
}

/// <summary>Navigation data</summary>
public class CPointOnSurfaceCommandData
{
    public TPointOnSurfaceCommands commandEvent;

    public string areaId { get; set; }
    public Vector3 pointPosition { get; set; }

    public CPointOnSurfaceCommandData(TPointOnSurfaceCommands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CPointOnSurfaceCommand : CCoreCommand
{
    public CPointOnSurfaceCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CPointOnSurfaceCommand(TPointOnSurfaceCommands _command)
    {
        data = new CPointOnSurfaceCommandData(_command);
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
                    case TPointOnSurfaceCommands.StartPointCapture:
                        hom3r.quickLinks.scriptsObject.GetComponent<PointOnSurfaceManager>().StartPointCapture();
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