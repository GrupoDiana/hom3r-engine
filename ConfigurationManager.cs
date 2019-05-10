using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationManager : MonoBehaviour
{    
    private bool UIEnabled;                     // UI enable or not    

    private bool uiSelectionEnabled;            // Selection by the UI enabled or not
    private bool uiAutomaticSelectionEnabled;   // This controlled what happens when the user click over a object

    private bool touchInteractionEnabled;       // This control if the touch interaction is On or not
    private bool mouseInteractionEnabled;       // This control if the mouse interaction is On or not

    private bool navigationEnabled;              // This control if the navigation is On or not

    private void Awake()
    {        
        UIEnabled = true;                        //Initially the UI is activated
        uiSelectionEnabled = true;               //Initially the selection from UI is activated
        uiAutomaticSelectionEnabled = true;      //Initially direct selection is activated  
        navigationEnabled = true;
        touchInteractionEnabled = false;
        mouseInteractionEnabled = true;
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
        // TODO send command message to UIManager
    }

    /// <summary>
    /// Get if the UI is visible or not. NOT WORKING YET
    /// </summary>
    /// <returns>True if the UI is activated</returns>
    public bool GetActiveUI()
    {
        return UIEnabled;
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
    }
    /// <summary>
    /// Get if mouse interaction is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public bool GetActiveMouseInteration()
    {
        return mouseInteractionEnabled;
    }

    /////////////////////
    // Navigation
    /////////////////////
    /// <summary>
    /// Set if the mouse interaction is activated or not. 
    /// </summary>
    /// <param name="_enabled">true activate the mouse interaction</param>
    public void SetActiveNavigation(bool _enabled)
    {
        navigationEnabled = _enabled;
    }
    /// <summary>
    /// Get if mouse interaction is activated or not. 
    /// </summary>
    /// <returns>True if mouse interaction is activated</returns>
    public bool GetActiveNavigation()
    {
        return navigationEnabled;
    }
}
