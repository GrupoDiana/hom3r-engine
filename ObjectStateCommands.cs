using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Object Visual State Command</summary>
public enum TObjectVisualStateCommands
{
    Indication_On, Indication_Off, Indication_Multiple_On,
    Confirmation_On, Confirmation_Off, Confirmation_Multiple_On,
    Transparency_On, Transparency_Off,
    Hidden_On, Hidden_Off,    
    Remove_On, Remove_Off,
    Explode, Implode
};

/// <summary>ObjectVisualState command data</summary>
public class CObjectVisualStateCommandData
{
    public TObjectVisualStateCommands commandEvent;

    public float duration { get; set; }
    public string colour { get; set; }
    // public bool multiple { get; set; }

    public CObjectVisualStateCommandData(TObjectVisualStateCommands _commandEvent) { this.commandEvent = _commandEvent; }
}

public class CObjectVisualStateCommand
{
    public CObjectVisualStateCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CObjectVisualStateCommand(TObjectVisualStateCommands _command, string _colour = "")
    {
        data = new CObjectVisualStateCommandData(_command);
        data.colour = _colour;
        data.duration = 0.0f;
        //data.multiple = _multiple;
    }
    
    public CObjectVisualStateCommand(TObjectVisualStateCommands _command, float _duration, string _colour = "")
    {
        data = new CObjectVisualStateCommandData(_command);
        data.duration = _duration;
        data.colour = _colour;
    }


    //////////////////
    //   Execute    //
    //////////////////
    public void Do(ObjectStateManager objManager)
    {
        if (data != null)
        {
            switch (data.commandEvent)
            {
                case TObjectVisualStateCommands.Indication_On:
                    objManager.Goto_Indication_State(false);
                    break;
               
                case TObjectVisualStateCommands.Indication_Multiple_On:
                    objManager.Goto_Indication_State(true);
                    break;

                case TObjectVisualStateCommands.Indication_Off:
                    objManager.Quit_Indication_State();
                    break;

                case TObjectVisualStateCommands.Confirmation_On:
                    objManager.Goto_Confirmation_State(false, data.duration, data.colour);
                    break;
                case TObjectVisualStateCommands.Confirmation_Multiple_On:
                    objManager.Goto_Confirmation_State(true, data.duration, data.colour);
                    break;
                case TObjectVisualStateCommands.Confirmation_Off:
                    objManager.Quit_Confirmation_State();
                    break;
                case TObjectVisualStateCommands.Remove_On:
                    objManager.Goto_Remove_State(data.duration);
                    break;
                case TObjectVisualStateCommands.Remove_Off:
                    objManager.Quit_Remove_State(data.duration);
                    break;
                case TObjectVisualStateCommands.Transparency_On:
                    objManager.Goto_Transparency_State(data.duration);
                    break;
                case TObjectVisualStateCommands.Transparency_Off:
                    objManager.Quit_Transparency_State(data.duration);
                    break;
                case TObjectVisualStateCommands.Explode:
                    objManager.Goto_Explosion_State();
                    break;
                case TObjectVisualStateCommands.Implode:
                    objManager.Quit_Explosion_State();
                    break;
                default:
                    Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Object State command without a command.");
        }

    }
    public void AbortCommand(ObjectStateManager objManager) { objManager.AbortCommandExecution(); }
}

