using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/// <summary>The 'Command' abstract class</summary>
public interface CCoreCommand
{
    void Do(MonoBehaviour m);
    void Undo(MonoBehaviour m);
}


/// <summary>
/// Core_Script Class of the 
/// </summary>
public class Core : MonoBehaviour {
      
    public event Action<CCoreCommand> OnCommandDo;       // Event
    public event Action<CCoreCommand> OnCommandUndo;
    public event Action<CCoreEvent> OnCoreEvent;

    private Stack<CCoreCommand> undoStack;               // Stack to store commands to be undo 
    private Stack<CCoreCommand> redoStack;               // Stack to store commands to be redo 

    private void Awake()
    {
        //Initialize stack of commands
        undoStack = new Stack<CCoreCommand>();       //Initialize stack of commands to be undo
        redoStack = new Stack<CCoreCommand>();       //Initialize stack of commands to be redo   

        //Initialize others
        InitializeHom3rLinks();                
        InitializeHom3rState();                     //Define the Initial State             

    }

    private void InitializeHom3rLinks()
    {
        hom3r.coreLink = this;
        hom3r.quickLinks.scriptsObject = GameObject.FindGameObjectWithTag("go_script");        //Point to Script object        
        hom3r.quickLinks._3DModelRoot = GameObject.FindGameObjectWithTag("go_father");        // Point to Main product        
        //hom3r.quickLinks.mainCamera = GameObject.FindGameObjectWithTag("MainCamera");        
        hom3r.quickLinks.labelsObject = GameObject.FindGameObjectWithTag("labels");

    }

    private void InitializeHom3rState()
    {
        hom3r.state = new CHom3rState();                       //Initialize status object   
        hom3r.state.selectionBlocked = false;                  //Initially selection is activated
        hom3r.state.captureSinglePointBlocked = false;         //Initially single point capture is deactivated
        hom3r.state.navigationBlocked = false;                 //Initially navigation using mouse is activated          
        hom3r.state.generalState = false;                      //Initially we are not ready  
        hom3r.state.productModelLoaded = false;               //Initially there are not any model loaded
        hom3r.state._3DModelLoaded = false;
        hom3r.state.isolateModeActive = false;                 //Initially the isolate mode is not activate
        hom3r.state.smartTransparencyModeActive = false;       //Initially the smartTransparency mode is not activate
        hom3r.state.singlePointLocationModeActive = false;     //Initially the point location mode is not activate        
        hom3r.state.currentMode = THom3rMode.idle;                                 // Initially HOM3R is not in a specific mode
        hom3r.state.currentExplosionMode = THom3rExplosionMode.IMPLODE;            // Initially HOM3R objects are imploded
        //hom3r.state.currentSelectionMode = THom3rSelectionMode.SPECIAL_NODE;     // Initially HOM3R is selecting by component
        hom3r.state.currentSelectionMode = THom3rSelectionMode.AREA;               // Initially HOM3R is selecting by area
        hom3r.state.currentIsolateMode = THom3rIsolationMode.idle;           // Initially HOM3R show every node of the product
        hom3r.state.currentLabelMode = THom3rLabelMode.idle;                       // Initially HOM3R doesn't show any label                 
        hom3r.state.currentPointCaptureMode = THom3rPointCaptureMode.iddle;
        hom3r.state.labelsUILayer = "Default";    
        hom3r.state.productRootLayer = "go_father_layer";
        hom3r.state.platform = GetPlatform();
    }

    private THom3rPlatform GetPlatform()
    {
        //Set the Hom3r environment
        if ((Application.platform == RuntimePlatform.WindowsEditor) || (Application.platform == RuntimePlatform.OSXEditor) || (Application.platform == RuntimePlatform.LinuxEditor))
        {
            return THom3rPlatform.Editor;
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return THom3rPlatform.WebGL;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return THom3rPlatform.Android;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return THom3rPlatform.IOS;
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            return THom3rPlatform.Windows;
        }        
        else
        {
            return THom3rPlatform.Other;
        }
    }

    // Use this for initialization
    void Start()
    {
        this.UpdateHom3rGeneralState();
    }

    private void UpdateHom3rGeneralState()
    {
        //TODO I would like to do this after receive confirmation that all the modules are well initiated.
        hom3r.state.generalState = true;
        this.EmitEvent(new CCoreEvent(TCoreEvent.Core_Hom3rReadyToStart));        
    }
   
    //////////////////////////////////////////
    // Methods to manage the Command Pattern //
    //////////////////////////////////////////      
    ///<summary>Execute a Command </summary>
    public void Do(CCoreCommand command, bool undo = Constants.undoNotAllowed)
    {                
        if (undo) undoStack.Push(command);  // Add command to undo stack                  
        redoStack.Clear();                  // Once we issue a new command, the redo stack clears                   
        ExecuteDo(command);                 //Execute the command  
    }

    ///<summary>Undo a the last command </summary>    
    public void Undo()
    {
        // Perform undo operations
        if (undoStack.Count > 0)
       {
            CCoreCommand command = undoStack.Pop();      //Create a new command with the current command                                
            ExecuteUndo(command);                       //Execute the command 
            redoStack.Push(command);                    //Add to redo stack
        }
    }

    ///<summary>Redo a the last command undo. </summary>    
    public void Redo()
    {
        // Perform redo operations
        if (redoStack.Count > 0)
        {            
            CCoreCommand command = redoStack.Pop();      //Create a new command with the current command                          
            ExecuteDo(command);                         //Execute the command 
            undoStack.Push(command);                    //Add to undo stack
        }
    }


    /// <summary>Execute the events, commands, send by the other classes of the HOM3R.</summary>
    /// <param name="command">Event to be executed</param>
    private void ExecuteDo(CCoreCommand command)
    {
       if (OnCommandDo != null)   { OnCommandDo.Invoke(command); }       
    }    
    /// <summary>Execute the events, commands, store on the undo stack</summary>    
    /// <param name="command">Event to be executed</param>
    private void ExecuteUndo(CCoreCommand command)
    {
        if (OnCommandUndo != null) { OnCommandUndo.Invoke(command); }        
    }

    //////////////////////////////////////////
    // Methods to manage the Events
    ////////////////////////////////////////// 

    ///<summary>Execute a Command </summary>
    public void EmitEvent(CCoreEvent _event, float _delay = 0f)
    {
        if (OnCoreEvent == null) { return; }
        if (_delay == 0f) {     OnCoreEvent.Invoke(_event);
        } else {                StartCoroutine(EmitEventWithDelay(_event, _delay));
        }
    }

    private IEnumerator EmitEventWithDelay(CCoreEvent _event, float _delay)
    {
        yield return new WaitForSeconds(_delay);
        OnCoreEvent.Invoke(_event);
    }

    ////////////////////////////////////////////////////////
    // Subscribe  Unsubscribe methods to manage the Events
    //////////////////////////////////////////////////////// 

    /// <summary>Subscribe methods to the events delegate</summary>
    /// <param name="DoCommand"></param>
    /// <param name="UndoCommand"></param>
    public void SubscribeCommandObserver(Action<CCoreCommand> DoCommand, Action<CCoreCommand> UndoCommand)
    {
        OnCommandDo += DoCommand;               //Subscribe a method to the event delegate
        OnCommandUndo += UndoCommand;           //Subscribe a method to the event delegate              
    }

    public void UnsubscribeCommandObserver(Action<CCoreCommand> DoCommand, Action<CCoreCommand> UndoCommand)
    {
        OnCommandDo -= DoCommand;               //Unsubscribe a method to the event delegate
        OnCommandUndo -= UndoCommand;           //Unsubscribe a method to the event delegate        
    }

    public void SubscribeEventObserver(Action<CCoreEvent> _InternalEvent)
    {
       OnCoreEvent += _InternalEvent;   //Subscribe a method to the event delegate
    }   

    public void UnsubscribeEventObserver(Action<CCoreEvent> _InternalEvent)
    {
        OnCoreEvent -= _InternalEvent;   //Unsubscribe a method to the event delegate
    }

    //////////////////////////////////////////
    // Instantiate Prefab
    ////////////////////////////////////////// 

    public GameObject InstantiatePrefab(string _prefabPath, GameObject _parent)
    {
        GameObject newPrefab = (GameObject)Resources.Load(_prefabPath, typeof(GameObject));
        GameObject newPrefabGO = Instantiate(newPrefab, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));
        if (newPrefabGO != null)
        {
            newPrefabGO.transform.parent = _parent.transform;
            Debug.Log("InstantiatePrefab: " + newPrefabGO.name);
        } else
        {
            Debug.Log("InstantiatePrefab ERROR - GO null: " + _prefabPath);
        }        
        return newPrefabGO;
    }
}
