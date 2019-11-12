using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TNavigationSystem { Spherical, LimitedSpherical, Elliptical }
public enum TMouseMapping { standard, inverse }
public class ConfigurationManager : MonoBehaviour
{    
    private bool UIEnabled;                     // UI enable or not    
    private bool UIHierarchyPanelEnabled;       // UI Hierarchy panel enable or not
    private bool UIOcclusionPanelEnabled;       // UI Occlusion panel enable or not

    private bool uiSelectionEnabled;            // Selection by the UI enabled or not
    private bool uiAutomaticSelectionEnabled;   // This controlled what happens when the user click over a object

    private bool touchInteractionEnabled;       // This control if the touch interaction is On or not
    private bool mouseInteractionEnabled;       // This control if the mouse interaction is On or not
    private TMouseMapping mouseMapping;         // This control which action is done by left mouse button and which by the right

    private bool navigationEnabled;             // This control if the navigation is On or not
    private bool navigationZoomEnabled;         // This control if the navigation Zoom is On or not
    private TNavigationSystem navigationSystem; // This control the navigation system that is going to be used

    private bool labelEditionEnabled;           // This control if the labels can be edit or not

    private bool explosionEnabled;              // This control if the explosion is enabled or not

    private float durationTransparencyAnimation;    // This control the duration of the fade-in, fade-out animation effect of the transparency 
    private float durationRemoveAnimation;          // This control the duration of the fade-in, fade-out animation effect of the remove

    private void Awake()
    {
        UIEnabled                   = true;     // Initially the UI is activated
        UIHierarchyPanelEnabled     = true;     // Initially the UI Hierarchy panel is activated
        UIOcclusionPanelEnabled     = true;     // Initially the UI Occlusion panel is activated
        explosionEnabled            = true;        

        uiSelectionEnabled          = true;     // Initially the selection from UI is activated
        uiAutomaticSelectionEnabled = true;     // Initially direct selection is activated  
        

        touchInteractionEnabled     = false;
        mouseInteractionEnabled     = true;
        mouseMapping                = TMouseMapping.standard;

        navigationEnabled           = false;
        navigationZoomEnabled       = true;
        navigationSystem            = TNavigationSystem.Spherical;

        labelEditionEnabled         = true;

        durationTransparencyAnimation   = 0.75f;
        durationRemoveAnimation         = 0.75f;

        Debug.Log("Configuration Manager Awake");     
    } 

    /////////////////////
    // UI 
    /////////////////////   
    /// <summary>
    /// Set if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <param name="_enabled">true UI is visible</param>
    public void SetActiveUI(bool _enabled)
    {
        UIEnabled = _enabled;
        this.SendUpdateEvent();
    }

    /// <summary>
    /// Get if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <returns>True if the UI is activated</returns>
    public bool GetActiveUI()
    {
        return UIEnabled;
    }

    /// <summary>
    /// Get if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <returns>True if the UI is activated</returns>
    public bool GetActiveUIHierarchyPanel()
    {
        return UIHierarchyPanelEnabled;
    }

    /// <summary>
    /// Set if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <param name="_enabled">true UI is visible</param>
    public void SetActiveUIHierarchyPanel(bool _enabled)
    {
        UIHierarchyPanelEnabled = _enabled;
        this.SendUpdateEvent();
    }


    /// <summary>
    /// Get if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <returns>True if the UI is activated</returns>
    public bool GetActiveUIOcclusionPanel()
    {
        return UIOcclusionPanelEnabled;
    }

    /// <summary>
    /// Set if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <param name="_enabled">true UI is visible</param>
    public void SetActiveUIOcclusionPanel(bool _enabled)
    {
        UIOcclusionPanelEnabled = _enabled;
        this.SendUpdateEvent();
    }



    /////////////////////
    // Selection
    /////////////////////
    /// <summary>
    /// Set if the selection through the UI activated or not. 
    /// </summary>
    /// <param name="_enabled">true UI visible</param>
    public void SetActiveUISelection(bool _enabled)
    {
        uiSelectionEnabled = _enabled;
        this.SendUpdateEvent();
    }

    /// <summary>
    /// Get the selection through the UI activated or not. 
    /// </summary>
    /// <returns>True if selection through the UI is activated</returns>
    public bool GetActiveUISelection()
    {
        return uiSelectionEnabled;
    }

    /////////////////////
    // Automatic Selection
    /////////////////////

    public void SetActiveUIAutomaticSelection(bool _enabled)
    {
        uiAutomaticSelectionEnabled = _enabled;
        this.SendUpdateEvent();
    }

    public bool GetActiveUIAutomaticSelection()
    {
        return uiAutomaticSelectionEnabled;
    }

    /////////////////////
    // Touch Interaction
    /////////////////////
    /// <summary>
    /// Set if the touch interaction is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate the touch interaction</param>
    public void SetActiveTouchInteration(bool _enabled)
    {
        touchInteractionEnabled = _enabled;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if touch interaction is activated or not. 
    /// </summary>
    /// <returns>True if touch interaction is activated</returns>
    public bool GetActiveTouchInteration()
    {
        return touchInteractionEnabled;
    }

    /////////////////////
    // Mouse Interaction
    /////////////////////
    /// <summary>
    /// Set if the mouse interaction is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate the mouse interaction</param>
    public void SetActiveMouseInteration(bool _enabled)
    {
        mouseInteractionEnabled = _enabled;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if mouse interaction is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public bool GetActiveMouseInteration()
    {
        return mouseInteractionEnabled;
    }

    /// <summary>
    /// Set the mouse that is going to be used
    /// </summary>
    /// <param name="_mouseMapping"></param>
    public void SetMouseMapping(TMouseMapping _mouseMapping)
    {
        mouseMapping = _mouseMapping;
        this.SendUpdateEvent();
    }

    /// <summary>
    /// Get which is the current mouse mapping 
    /// </summary>
    /// <returns></returns>
    public TMouseMapping GetMouseMapping()
    {
        return mouseMapping;
    }

      
    /////////////////////
    // Navigation
    /////////////////////
    /// <summary>
    /// Set if the navigation is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate the mouse interaction</param>
    public void SetActiveNavigation(bool _enabled)
    {
        navigationEnabled = _enabled;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if the navigation is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public bool GetActiveNavigation()
    {
        return navigationEnabled;
    }

    /// <summary>
    /// Set if the Zoom is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate Zoom interaction</param>
    public void SetActiveNavigationZoom(bool _enabled)
    {
        navigationZoomEnabled = _enabled;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if the zoom is activated or not. 
    /// </summary>
    /// <returns>True if Zoom interaction is activated</returns>
    public bool GetActiveNavigationZoom()
    {
        return navigationZoomEnabled;
    }


    /////////////////////
    // Label Edition
    /////////////////////
    /// <summary>
    /// Set if the label edition is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate the mouse interaction</param>
    public void SetActiveLabelEdition(bool _enabled)
    {
        labelEditionEnabled = _enabled;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if the label edition is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public bool GetActiveLabelEdition()
    {
        return labelEditionEnabled;
    }

    /////////////////////
    // Explosion
    /////////////////////
    /// <summary>
    /// Set if the explosion is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate the mouse interaction</param>
    public void SetActiveExplosion(bool _enabled)
    {
        explosionEnabled = _enabled;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if the explosion is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public bool GetActiveExplosion()
    {
        return explosionEnabled;
    }


    /////////////////////
    // Transparency Effect
    /////////////////////
    /// <summary>
    /// Set if the explosion is activated or not. 
    /// </summary>
    /// <param name="_duration">true activate the mouse interaction</param>
    public void SetDurationTransparencyAnimation(float _duration)
    {
        durationTransparencyAnimation = _duration;
        this.SendUpdateEvent();
    }
    /// <summary>
    /// Get if the explosion is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public float GetDurationTransparencyAnimation()
    {
        return durationTransparencyAnimation;
    }

    /////////////////////
    // Remove Effect
    /////////////////////
    /// <summary>
    /// Set if the explosion is activated or not. 
    /// </summary>
    /// <param name="_duration">true activate the mouse interaction</param>
    public void SetDurationRemoveAnimation(float _duration)
    {
        durationRemoveAnimation = _duration;
        this.SendUpdateEvent();
    }
    
    /// <summary>
    /// Get if the explosion is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public float GetDurationRemoveAnimation()
    {
        return durationRemoveAnimation;
    }


    // Other
    private void SendUpdateEvent()
    {
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ConfigurationManager_ConfigurationUpdated));
    }
}
