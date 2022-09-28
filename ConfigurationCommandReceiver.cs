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
    ActiveUIGizmo,
    ActivateUIHierarchyPanel, ActivateUIOcclusionPanel, ActivateUISettingsPanel,
    ActiveUISelection, ActiveUIAutomaticSelection,    
    ActiveTouchInteration, ActiveMouseInteration, ActiveMouseWheelInteraction,    
    ActiveNavigation, ActiveNavigationZoom, ActiveNavigationZoomMinimumLimit, ActiveNavigationZoomMaximumLimit, SetNavigationZoomMinimumLimit, SetNavigationZoomMaximumLimit,
    SetNavigationLatitudeCorrectioFactorMode, SetNavigationLongitudeCorrectioFactorMode, ActiveNavigationOblateOrientationCorrection,
    SetNavigationSystemMode,
    ActivePanNavigation,
    ActiveLabelEdition,
    ActiveExplosion,
    ActiveKeyboard,
    SetDurationTransparencyAnimation,
    SetDurationRemoveAnimation,
    SetMouseMapping
}

/// <summary>Model data</summary>
public class CConfigurationCommandData
{
    public TConfigurationCommands command;
    public bool activate;
    public TMouseMapping mouseMapping;
    public TNavigationSystemMode navigationSystem;
    public TInteractionMappingCorrectionMode interactionMappingCorrectionMode;
    public TOblateSpheroidCorrectionMode oblateSpheroidCorrectionMode;
    public float value;

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

    public CConfigurationCommand(TConfigurationCommands _command, TNavigationSystemMode _navigationSystem)
    {
        data = new CConfigurationCommandData(_command);
        data.navigationSystem = _navigationSystem;
    }
    public CConfigurationCommand(TConfigurationCommands _command, TInteractionMappingCorrectionMode _interactionMappingCorrectionMode)
    {
        data = new CConfigurationCommandData(_command);
        data.interactionMappingCorrectionMode = _interactionMappingCorrectionMode;
    }

    public CConfigurationCommand(TConfigurationCommands _command, TOblateSpheroidCorrectionMode _oblateSpheroidCorrectionMode)
    {
        data = new CConfigurationCommandData(_command);
        data.oblateSpheroidCorrectionMode = _oblateSpheroidCorrectionMode;
    }

    public CConfigurationCommand(TConfigurationCommands _command, float _value)
    {
        data = new CConfigurationCommandData(_command);
        data.value = _value;
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
                case TConfigurationCommands.ActiveUIGizmo:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUIGizmo(data.activate);
                    break;
                case TConfigurationCommands.ActivateUIHierarchyPanel:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUIHierarchyPanel(data.activate);
                    break;
                case TConfigurationCommands.ActivateUIOcclusionPanel:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUIOcclusionPanel(data.activate);
                    break;
                case TConfigurationCommands.ActivateUISettingsPanel:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveUISettingsPanel(data.activate);
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
                case TConfigurationCommands.ActiveMouseWheelInteraction:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveMouseWheelInteration(data.activate);
                    break;
                case TConfigurationCommands.ActiveNavigation:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigation(data.activate);
                    break;
                case TConfigurationCommands.ActivePanNavigation:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActivePanNavigation(data.activate);
                    break;
                case TConfigurationCommands.ActiveNavigationZoom:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigationZoom(data.activate);
                    break;
                case TConfigurationCommands.ActiveNavigationZoomMinimumLimit:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigationZoomMinimumLimit(data.activate);
                    break;
                case TConfigurationCommands.ActiveNavigationZoomMaximumLimit:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigationZoomMaximumLimit(data.activate);
                    break;
                case TConfigurationCommands.SetNavigationZoomMinimumLimit:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetNavigationZoomMinimumLimit(data.value);
                    break;
                case TConfigurationCommands.SetNavigationZoomMaximumLimit:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetNavigationZoomMaximumLimit(data.value);
                    break;
                case TConfigurationCommands.SetMouseMapping:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetMouseMapping(data.mouseMapping);
                    break;
                case TConfigurationCommands.ActiveLabelEdition:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveLabelEdition(data.activate);
                    break;
                case TConfigurationCommands.ActiveExplosion:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveExplosion(data.activate);
                    break;
                case TConfigurationCommands.SetNavigationLatitudeCorrectioFactorMode:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetModeLatitudeInteractionCorrectionFactor(data.interactionMappingCorrectionMode);
                    break;
                case TConfigurationCommands.SetNavigationLongitudeCorrectioFactorMode:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetModeLongitudeInteractionCorrectionFactor(data.interactionMappingCorrectionMode);
                    break;
                case TConfigurationCommands.ActiveNavigationOblateOrientationCorrection:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveNavigationOblateOrientationCorrection(data.oblateSpheroidCorrectionMode);
                    break;
                case TConfigurationCommands.SetNavigationSystemMode:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetNavigationSystemMode(data.navigationSystem);
                    break;
                case TConfigurationCommands.SetDurationTransparencyAnimation:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetDurationTransparencyAnimation(data.value);
                    break;
                case TConfigurationCommands.SetDurationRemoveAnimation:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetDurationRemoveAnimation(data.value);
                    break;
                case TConfigurationCommands.ActiveKeyboard:
                    hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().SetActiveKeyboard(data.activate);
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

