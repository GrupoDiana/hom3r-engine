using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Constants
{
    public const float Pi = 3.14159265359f;
    public const bool undoAllowed = true;             //Const to store the valueo to allow undo 
    public const bool undoNotAllowed = false;         //Const to store the valueo to not allow undo 
    public const int largeText = 25;                  //Const to indicate when a text is considered long    
}

static class MathHom3r
{
    /// <summary>Calculate Power of two of a float input</summary>
    /// <param name="value">float value to power two</param>
    /// <returns>value * value </returns>
    static public float Pow2(float value)
    {
        return value * value;
    }

    /// <summary>Normalize a rad angle to [0, 2*PI]</summary>
    /// <param name="inputRad">Angle in radians</param>
    /// <returns>Equivalent angle in radians in the interval [0, 2*PI]</returns>
    static public float NormalizeAngleInRad(float inputRad)
    {
        float value = inputRad % (2 * Mathf.PI);
        if (value < 0) { value += 2 * Mathf.PI; }
        return value;
    }


    static public float Maximun(float num1, float num2)
    {
        if (num1 > num2)    { return num1; }
        else                { return num2; }
    }

    /// <summary>
    /// Returns true if the value to check has a finite value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    static public bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);        
    }

    static public bool IsFinite(Vector3 value)
    {
        return  (!float.IsNaN(value.x) && !float.IsInfinity(value.x)) && 
                (!float.IsNaN(value.y) && !float.IsInfinity(value.y)) &&
                (!float.IsNaN(value.z) && !float.IsInfinity(value.z));
    }

    public static float Max(float x, float y)
    {
        // Or in-line it as x < y ? (y < z ? z : y) : (x < z ? z : x);
        // Time it before micro-optimizing though!
        return Mathf.Max(x, y);
    }

    public static float Max(float x, float y, float z)
    {
        // Or in-line it as x < y ? (y < z ? z : y) : (x < z ? z : x);
        // Time it before micro-optimizing though!
        return Mathf.Max(x, Mathf.Max(y, z));
    }

    public static float Max(Vector3 value)
    {
        // Or in-line it as x < y ? (y < z ? z : y) : (x < z ? z : x);
        // Time it before micro-optimizing though!
        return Mathf.Max(value.x, Mathf.Max(value.y, value.z));
    }

    /// <summary>
    /// Returns true if x is in range [low..high], else false. Working for negative numbers also.
    /// </summary>
    /// <param name="low"></param>
    /// <param name="high"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    public static bool InRange(int low, int high, int x)
    {
        return ((x - high) * (x - low) <= 0);
    }

}

static class StringHom3r
{
    /// <summary>Return string with the first char in uppercase and the rest in lowercase</summary>
    /// <param name="str">string to ve converter</param>
    /// <returns>string converter</returns>
    static public string FirstLetterToUpper(string str)
    {
        str = str.ToLowerInvariant();
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }
}

/// <summary>Hom3rModes_Type will try to show the mode in which HOM3R is working every moment</summary>
public enum THom3rPlatform { Editor, WebGL, Android, IOS, Windows, Other };
public enum THom3rMode { idle, capturing_surface_point, remove, local_explosion };
public enum THom3rExplosionMode { EXPLODE, IMPLODE };
public enum THom3rIsolationMode { idle, ISOLATE, WITH_REMOVEDNODES };
public enum THom3rSelectionMode { AREA, SPECIAL_NODE };
public enum THom3rLabelMode { idle, show, edit, add };
public enum THom3rPointCaptureMode { iddle, capturing, editing };
public enum THom3rCommandOrigin { ui, io};
public enum THom3rNavigationMode { regular, pan };

/// <summary> Class to store the state of the app in every moment.</summary>
public class CHom3rState
{
    // Configuration from the interface
    //public bool UIEnable { get; set; }                  // UI enable or not
    //public bool automaticSelection { get; set; }        // This controlled what happens when the user click over a object
    //public bool touchInteractionEnable { get; set; }    // This control if the touch interaction is On or not
    //public bool mouseInteractionEnable { get; set; }    // This control if the touch interaction is On or not

    //Internal use
    public bool generalState { get; set; }                      //Is false when any error happens
    public bool productModelLoaded { get; set; }               //Store if the model has been loaded or not    
    public bool _3DModelLoaded { get; set; }               //Store if the model has been loaded or not    
    public THom3rPlatform platform { get; set; }                //Store the environment if which Hom3r is working    
    public float smartTransparencyAlphaLevel { get; set; }

    //Modes Management
    public bool selectionBlocked { get; set; }                          //External blocking/unblocking of selection (indicate and confirm)   
    public bool captureSinglePointBlocked { get; set; }                 //External blocking/unblocking of SinglePoints capture      
    public bool navigationBlocked { get; set; }                         //External blocking/unblocking of Navigation (camera movements using the mouse)
    public bool isolateModeActive { get; set; }                         //Store if the isolate mode is activate or not
    public bool smartTransparencyModeActive { get; set; }               //Store if the isolate mode is activate or not
    public bool singlePointLocationModeActive { get; set; }             //Store if the node 

    // Store the current mode where HOM3R is working
    private THom3rMode current_mode;
    public THom3rMode currentMode {
        get { return current_mode; }

        set {
            current_mode = value;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Core_ModeChanged));
        }
    }                         

    public THom3rExplosionMode      currentExplosionMode { get; set; }       //Store the current explosion mode where HOM3R is working
    public THom3rSelectionMode      currentSelectionMode { get; set; }       //Store the current mode of selection 
    public THom3rIsolationMode      currentIsolateMode { get; set; }         //Store the current visualization mode (if there are removed nodes ot not)
    public THom3rLabelMode          currentLabelMode { get; set; }               //Store the current labeling mode                     
    public THom3rPointCaptureMode   currentPointCaptureMode { get; set; }
    public THom3rNavigationMode     navigationMode;             // Which navigation system are we using

    //Layers
    public string labelsUILayer;                // Store camera layer for Labels
    public string productRootLayer;             // Store camera layer for 3D model
}

public class CQuickLinks
{
    public GameObject navigationSystemObject;   // Pointer to the main camera object        
    public GameObject uiObject;                 // Pointer to the UI object
    public GameObject scriptsObject;            // Pointer to the scripts container object
    //public GameObject configurationObject;      // Pointer to the configuration script container object
    public GameObject _3DModelRoot;             // Pointer to the main product object
    public GameObject labelsObject;             // Pointer to the label container object
    public GameObject manipulatorObject;        //Pointer to the ARCore manipulator Object     
} 

public static class hom3r {

    static public Core coreLink;     
    static public CQuickLinks quickLinks = new CQuickLinks();
    static public CHom3rState state;                                // Current state of the app
}
