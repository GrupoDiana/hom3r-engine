using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationCommandReceiver : MonoBehaviour {

    private void OnEnable()
    {        
        hom3r.coreLink.SubscribeCommandObserver(DoNavigationCommand, UndoNavigationCommand);    //Subscribe a method to the event delegate
    }

    private void OnDisable()
    {
        hom3r.coreLink.UnsubscribeCommandObserver(DoNavigationCommand, UndoNavigationCommand);  //Unsubscribe a method to the event delegate        
    }

    private void DoNavigationCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CNavigationCommand)) { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoNavigationCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(CNavigationCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }    
}



/// <summary>Navigation Commands</summary>
public enum TNavigationCommands
{
    InitializeNavigation,
    MouseMovement
}

/// <summary>Navigation data</summary>
public class CNavigationCommandData
{
    public TNavigationCommands commandEvent;

    public float mouseX { get; set; }
    public float mouseY { get; set; }
    public float mouseWheel { get; set; }
    public TMainAxis main3DModelAxis { get; set; }

    public CNavigationCommandData(TNavigationCommands _commandEvent) { this.commandEvent = _commandEvent; }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class CNavigationCommand : CCoreCommand
{
    public CNavigationCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public CNavigationCommand(TNavigationCommands _command)
    {
        data = new CNavigationCommandData(_command);
    }
    public CNavigationCommand(TNavigationCommands _command, TMainAxis _mainAxis = TMainAxis.Vertical)
    {
        data = new CNavigationCommandData(_command);
        data.main3DModelAxis = _mainAxis;
    }
    public CNavigationCommand(TNavigationCommands _command, float _mouseX, float _mouseY, float _mouseWheel)
    {
        data = new CNavigationCommandData(_command);
        data.mouseX = _mouseX;
        data.mouseY = _mouseY;
        data.mouseWheel = _mouseWheel;
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
                    case TNavigationCommands.InitializeNavigation:
                        string navigation_axis = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetModelNavigationAxis();
                        hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().InitNavigation(navigation_axis);
                        hom3r.coreLink.Do(new CLabelManager2Command(TLabelManager2Commands.UpdateAnchoredLabelView)); //Needed for label face to camera before starting the navigation
                        break;
                    case TNavigationCommands.MouseMovement:                        
                        hom3r.quickLinks.navigationSystemObject.GetComponent<NavigationManager>().SetMouseMovement(data.mouseX, data.mouseY, data.mouseWheel);
                        break;
                    default:
                        Debug.LogError("Error: This command " + data.commandEvent + " is not valid.");
                        break;
                }
            }
            else
            {
                Debug.LogError("Error: Has been called a Navigation command without command.");
            }
        }
    }
    public void Undo(MonoBehaviour m)
    {
        throw new System.NotImplementedException();

    }
}
