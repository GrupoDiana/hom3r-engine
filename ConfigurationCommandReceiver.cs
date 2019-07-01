using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationCommandReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        hom3r.coreLink.SubscribeCommandObserver(DoModelCommand, UndoModelCommand);  //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoModelCommand, UndoModelCommand);
    }

    private void DoModelCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CConfigurationCommand)) { command.Do(this); }
        else { /* Error - Do nothing */ }
    }

    private void UndoModelCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CConfigurationCommand)) { command.Undo(this); }
        else { /* Error - Do nothing */ }
    }
}


/// <summary>Model commands</summary>
public enum TConfigurationCommands
{
    ActiveUI,
    ActiveUISelection, ActiveUIAutomaticSelection,
    ActiveTouchInteration, ActiveMouseInteration,
    ActiveNavigation, ActiveNavigationZoom,
    SetMouseMapping
}

/// <summary>Model data</summary>
public class CConfigurationCommandData
{
    public TConfigurationCommands command;
    public bool activate;
    public TMouseMapping mouseMapping;


    public CConfigurationCommandData(TConfigurationCommands _command)
    {
        this.command = _command;
    }   
}



/// <summary>A 'ConcreteCommand' class</summary>
public class CConfigurationCommand : CCoreCommand
{
    public CConfigurationCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CConfigurationCommand(TConfigurationCommands _command)
    {
        data = new CConfigurationCommandData(_command);
    }

    public CConfigurationCommand(TConfigurationCommands _command, bool _activate)
    {
        data = new CConfigurationCommandData(_command);
        data.activate = _activate;
    }

    public CConfigurationCommand(TConfigurationCommands _command, TMouseMapping _mouseMapping)
    {
        data = new CConfigurationCommandData(_command);
        data.mouseMapping = _mouseMapping;
    }
    //////////////////
    //   Execute    //
    //////////////////
    public void Do(MonoBehaviour m)
    {
        if (data != null)
        {
            switch (data.command)
            {
                case TConfigurationCommands.ActiveUI:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUI(data.activate);
                    break;
                case TConfigurationCommands.ActiveUISelection:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUISelection(data.activate);
                    break;
                case TConfigurationCommands.ActiveUIAutomaticSelection:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUIAutomaticSelection(data.activate);
                    break;
                case TConfigurationCommands.ActiveTouchInteration:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveTouchInteration(data.activate);
                    break;
                case TConfigurationCommands.ActiveMouseInteration:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveMouseInteration(data.activate);
                    break;
                case TConfigurationCommands.ActiveNavigation:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigation(data.activate);
                    break;
                case TConfigurationCommands.ActiveNavigationZoom:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigationZoom(data.activate);
                    break;
                case TConfigurationCommands.SetMouseMapping:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetMouseMapping(data.mouseMapping);
                    break;
                default:
                    Debug.LogError("Error: This command " + data.command + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Configuration command without a valid command");
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }
}

