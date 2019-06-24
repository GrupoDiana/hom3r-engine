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
        if (command.GetType() == typeof(CNavigationCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoLabelManager2Command(CCoreCommand command)
    {
        if (command.GetType() == typeof(CNavigationCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }
}


/// <summary>Navigation Commands</summary>
public enum TLabelManager2Commands
{
    AddLabel, RemoveLabel
}


/// <summary>Navigation data</summary>
public class CLabelManager2CommandData
{
    public TLabelManager2Commands commandEvent;

    public string areaId { get; set; }
    public Vector3 labelPosition { get; set; }

    public CLabelManager2CommandData(TLabelManager2Commands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CLabelMananager2Command : CCoreCommand
{
    public CLabelManager2CommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CLabelMananager2Command(TLabelManager2Commands _command)
    {
        data = new CLabelManager2CommandData(_command);
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
                    case TLabelManager2Commands.AddLabel:

                        break;

                    case TLabelManager2Commands.RemoveLabel:

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