﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionCommandReceiver : MonoBehaviour {
    	
    private void OnEnable()
    {                                
        hom3r.coreLink.SubscribeCommandObserver(DoOcclusionCommand, UndoOcclusionCommand);  //Subscribe a method to the event delegate       
    }

    private void OnDisable()
    {        
        hom3r.coreLink.UnsubscribeCommandObserver(DoOcclusionCommand, UndoOcclusionCommand);    //Unsubscribe a method to the event delegate        
    }

    private void DoOcclusionCommand(CCoreCommand command)
    {        
        if (command.GetType() == typeof(COcclusionCommand))    { command.Do(this); }
        else { /* Do nothing */ }
    }

    private void UndoOcclusionCommand(CCoreCommand command)
    {
        if (command.GetType() == typeof(COcclusionCommand)) { command.Undo(this); }
        else { /* Do nothing */ }
    }

        
    //////////////////////////////////////// Auxiliary Methods ///////////////////////////////////

    

    

    /// <summary>Change the selected objects state to Visible</summary>
    public void ExecuteRemoveUndo(GameObject goToRemove)
    {
        //Get the areaID of the area selected                
        string areaID = goToRemove.GetComponent<ObjectStateManager>().areaID;
        if (areaID != null)
        {
            List<GameObject> _areaList = new List<GameObject>();
            //Filled _areaList to be selected in a different way, depending of the selection mode
            if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
            {
                // _areaList.Add(goToRemove);
                string leafID = this.GetComponent<ModelManager>().GetNodeLeafID_ByAreaID(areaID);
                if (leafID != null)
                {
                    List<GameObject> areasOfaLeaf = this.GetComponent<ModelManager>().GetAreaGameObjectList_ByLeafID(leafID);
                    foreach (GameObject go in areasOfaLeaf)
                    {
                        //Change the state of the object to hide (this method will be in charge of changing the object material)
                        go.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_Off);
                    }

                    // Send list of areas to WebApp
                    List<string> areaIDList = this.GetComponent<ModelManager>().GetAreaList_ByLeafID(leafID);
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Deactivated, areaIDList));
                }
                else { Debug.LogError("areaID not found"); }
            }
            else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
            {
                //Select all the areas with the same special ancestor.
                string _specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                _areaList = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(_specialNodeID);
                foreach (var item in _areaList)
                {
                    item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_Off);
                }
                // Send list of areas to WebApp
                List<string> areaIDList = this.GetComponent<ModelManager>().GetAreaIDList_BySpecialAncestorID(_specialNodeID);
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Deactivated, areaIDList));
            }
        }
    }
}



/// <summary>Occlusions commands</summary>
public enum TOcclusionCommands
{
    EnableSmartTransparency, DisableSmartTransparency, ResetSmartTransparency, SetTransparencyAlphaLevel,
    Isolate,
    RemoveGameObject, ShowRemovedGameObject,
    ShowAll,
    StartStopRemoveMode,
    StartStopGlobalExplosion, StartStopLayoutExplosionMode, OneObjectLayoutExplosion
}

/// <summary>Occlusions data</summary>
public class COcclusionCommandData
{
    public TOcclusionCommands command;

    public List<GameObject> listRemovedObjets { get; set; }
    public THom3rIsolationMode visualizationMode { get; set; }
    public GameObject obj { get; set; }
    public bool confirmedObject { get; set; }
    public float alphaLevel { get; set; }

    public COcclusionCommandData(TOcclusionCommands _command)
    {
        this.command = _command;
    }
}

/// <summary>A 'ConcreteCommand' class</summary>
public class COcclusionCommand : CCoreCommand
{
    public COcclusionCommandData data;
    //////////////////
    // Constructors //
    //////////////////
    public COcclusionCommand(TOcclusionCommands _command)
    {
        data = new COcclusionCommandData(_command);
    }
    public COcclusionCommand(TOcclusionCommands _command, List<GameObject> _listRemovedObjets)
    {
        data = new COcclusionCommandData(_command);

        data.listRemovedObjets = _listRemovedObjets;
    }
    public COcclusionCommand(TOcclusionCommands _command, THom3rIsolationMode _currentMode, List<GameObject> _listRemovedObjets)
    {
        data = new COcclusionCommandData(_command);

        data.visualizationMode = _currentMode;
        data.listRemovedObjets = _listRemovedObjets;
    }
    public COcclusionCommand(TOcclusionCommands _command, GameObject _obj)
    {
        data = new COcclusionCommandData(_command);

        data.obj = _obj;
    }
    
    public COcclusionCommand(TOcclusionCommands _command, GameObject _obj, bool _confirmedObject)
    {
        data = new COcclusionCommandData(_command);
        data.obj = _obj;
        data.confirmedObject = _confirmedObject;
    }
    public COcclusionCommand(TOcclusionCommands _command, float _alphaLevel)
    {
        data = new COcclusionCommandData(_command);
        data.alphaLevel = _alphaLevel;
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

                ////////  SMART TRANSPARENCY  //////////////
                case TOcclusionCommands.SetTransparencyAlphaLevel:
                    hom3r.quickLinks.scriptsObject.GetComponent<TransparencyManager>().SetTransparencyLevelToAllTransparentObjects(data.alphaLevel);
                    break;

                case TOcclusionCommands.EnableSmartTransparency:
                    if (!hom3r.state.smartTransparencyModeActive)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().StartSmartTransparency();
                    }
                    break;

                case TOcclusionCommands.DisableSmartTransparency:
                    if (hom3r.state.smartTransparencyModeActive)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().StopSmartTransparency();                        
                    }
                    break;

                case TOcclusionCommands.ResetSmartTransparency:
                    m.GetComponent<TransparencyManager>().SetRayDestinationPoints_AllSelectedAreas();
                    break;

                ////////  ISOLATE  //////////////
                case TOcclusionCommands.Isolate:
                    if (m.GetComponent<SelectionManager>().GetNumberOfConfirmedGameObjects() > 0)
                    {
                        //Stop transparency mode if is activated
                        if (hom3r.state.smartTransparencyModeActive)
                        {
                            //this.GetComponent<Core_Script>().Do(new UICoreCommand(TUIEvent.SmartTransparency, false), Constants.undoNotAllowed);
                            m.GetComponent<Core>().Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency), Constants.undoNotAllowed);
                        }
                        //1. Update Core mode
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.ISOLATE;
                        //2. Execute algorithms
                        m.GetComponent<IsolateManager>().IsolateMode_ON();
                    }
                    else
                    {
                        //TODO: Show messege in UI    
                        //m.GetComponent<Core>().Do(new UICoreCommand(TUICommands.ShowAlertText, "No product was selected: there is nothing to focus"), Constants.undoNotAllowed);
                    }
                    break;

                case TOcclusionCommands.ShowAll:
                    //Exit transparency mode if is activate
                    if (hom3r.state.smartTransparencyModeActive)
                    {                        
                        hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency), Constants.undoNotAllowed);
                    }
                    if (hom3r.state.currentVisualizationMode == THom3rIsolationMode.ISOLATE)
                    {
                        //1. Update Core mode
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.IDLE;
                        //2. Execute algorithms
                        m.GetComponent<IsolateManager>().IsolateMode_OFF();
                        //3. Update buttons
                        if (hom3r.state.currentMode == THom3rMode.SINGLEPOINTLOCATION)
                        {
                           // hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateDisableButtons_SinglePointMode(true);
                        }
                    }
                    else if (hom3r.state.currentVisualizationMode == THom3rIsolationMode.WITH_REMOVEDNODES)
                    {
                        //1. Update Core mode
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.IDLE;
                        //2. Execute algorithms
                        hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().RevealAllRemovedGameObjects(1.0f);
                    }
                    break;

                ////////  REMOVE  //////////////
                case TOcclusionCommands.StartStopRemoveMode:
                    if (hom3r.state.currentMode == THom3rMode.REMOVE)
                    {
                        hom3r.coreLink.SetCurrentMode(THom3rMode.IDLE);                        
                    }
                    else
                    {
                        hom3r.coreLink.SetCurrentMode(THom3rMode.REMOVE);                        
                        if (hom3r.state.currentVisualizationMode == THom3rIsolationMode.IDLE)
                        {
                            hom3r.state.currentVisualizationMode = THom3rIsolationMode.WITH_REMOVEDNODES;
                        }
                    }
                    break;

                case TOcclusionCommands.RemoveGameObject:
                    if (!hom3r.state.selectionBlocked)
                    {                       
                        hom3r.coreLink.GetComponent<OcclusionManager>().RemoveGameObject(data.obj);
                    }
                    break;

                case TOcclusionCommands.ShowRemovedGameObject:
                    hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().ShowRemovedGameObject(data.obj, 1f);
                    break;

                ////////  EXPLOSION  //////////////
                case TOcclusionCommands.StartStopGlobalExplosion:
                    if (hom3r.state.currentExplosionMode == THom3rExplosionMode.EXPLODE)
                    {
                        //hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().Implode();
                        //hom3r.coreLink.Do(new UICoreCommand(TUICommands.EnableExplodelayoutButton), Constants.undoNotAllowed);  //Enable LayoutExplosion                          
                        //hom3r.state.currentExplosionMode = THom3rExplosionMode.IMPLODE;

                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().RevertGlobalExplosion();
                    }
                    else
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().StartGlobalExplosion();
                        /*if (!hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().IsEmpty())
                        {
                            m.GetComponent<OcclusionCommandReceiver>().ExecuteExplosion(true, "global");
                            hom3r.coreLink.Do(new UICoreCommand(TUICommands.DisableExplodelayoutButton), Constants.undoNotAllowed);  //Disable LayoutExplosion
                            hom3r.state.currentExplosionMode = THom3rExplosionMode.EXPLODE;
                        }*/
                    }
                    break;

                case TOcclusionCommands.StartStopLayoutExplosionMode:
                    if (hom3r.state.currentMode == THom3rMode.LOCALEXPLOSION)
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().StopLocalExplosionMode();                        
                    }
                    else
                    {
                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().StartLocalExplosionMode();                        
                    }
                    break;

                case TOcclusionCommands.OneObjectLayoutExplosion:
                    if (!hom3r.state.selectionBlocked)
                    {
                        if (data.obj.GetComponent<ObjectStateManager>().GetExplosionState() == ObjectExplosionState_Type.Explode)
                        {
                            //Execute algorithm
                            hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().ImplodeGameObject(data.obj);
                        }
                        else if (data.obj.GetComponent<ObjectStateManager>().GetExplosionState() == ObjectExplosionState_Type.No_Explode)
                        {
                            hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().ExplodeGameObject(data.obj);
                        }
                    }
                    break;

                default:
                    Debug.LogError("Error: This command " + data.command + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Occlusion command without a valid command");
        }
    }

    public void Undo(MonoBehaviour m)
    {
        if (data != null)
        {
            switch (data.command)
            {
                ////////  ISOLATE  //////////////
                case TOcclusionCommands.Isolate:
                    //1. Get List of removed objects                
                    List<GameObject> listRemovedObjets = new List<GameObject>(hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().GetRemovedList());
                    if (listRemovedObjets.Count != 0)
                    {
                        //2. Make visible objects that have been removed
                        //Temporally List of objects to make visible
                        List<GameObject> toMakeVisibleList = new List<GameObject>();
                        //If a object is removed now but wasn't before, we make it visible
                        toMakeVisibleList = listRemovedObjets.FindAll(x => !data.listRemovedObjets.Contains(x));
                        foreach (var obj in toMakeVisibleList)
                        {
                            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_Off);
                        }
                        //3. Update Modes
                        if (hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().GetRemovedList().Count > 0)
                        {
                            hom3r.state.currentVisualizationMode = THom3rIsolationMode.WITH_REMOVEDNODES;
                        }
                        else
                        {
                            hom3r.state.currentVisualizationMode = THom3rIsolationMode.IDLE;
                        }
                        //4. Move to reset Position
                        //TODO: Do reset with a navigation command
                        //hom3r.coreLink.Do(new UICoreCommand(TUICommands.HomeButtonPressed), Constants.undoNotAllowed);
                    }
                    break;

                case TOcclusionCommands.ShowAll:
                    //undo
                    if (data.visualizationMode == THom3rIsolationMode.ISOLATE)
                    {
                        //1. Update the mode
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.ISOLATE;
                        //2. Execute Isolate
                        m.GetComponent<IsolateManager>().IsolateAGivenGOList_ModeON(data.listRemovedObjets);
                    }
                    else if (data.visualizationMode == THom3rIsolationMode.WITH_REMOVEDNODES)
                    {
                        //1. Update the mode
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.WITH_REMOVEDNODES;
                        //2. Execute Remove of previously removed nodes
                        foreach (GameObject goToRemove in data.listRemovedObjets)
                        {
                            goToRemove.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_On);
                        }
                    }
                    break;


                ////////  REMOVE  //////////////
                case TOcclusionCommands.RemoveGameObject:
                    //1. Perform the opposite remove algorithm
                    m.GetComponent<OcclusionCommandReceiver>().ExecuteRemoveUndo(data.obj);
                    //2. Confirm the area/special node if were confirmed
                    if (data.confirmedObject)
                    {
                        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.MultipleConfirmation, data.obj), Constants.undoNotAllowed);
                    }
                    //3. Update Visualization Modes
                    if (hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().GetRemovedList().Count > 0)
                    {
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.WITH_REMOVEDNODES;
                    }
                    else
                    {
                        hom3r.state.currentVisualizationMode = THom3rIsolationMode.IDLE;
                    }
                    break;

                ////////  EXPLOSION  //////////////
                case TOcclusionCommands.StartStopGlobalExplosion:
                    //UNDO
                    if (!hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().IsEmpty())
                    {
                        //m.GetComponent<OcclusionCommandReceiver>().ExecuteExplosion(false, "global");
                        hom3r.quickLinks.scriptsObject.GetComponent<OcclusionManager>().RevertGlobalExplosion();
                    }
                    break;

                default:
                    Debug.LogError("Error: This command " + data.command + " is not valid.");
                    break;
            }
        }
        else
        {
            Debug.LogError("Error: Has been called a Occlusion command without a valid command");
        }

    }
}