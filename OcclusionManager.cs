using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionManager : MonoBehaviour
{
    // Awake is called at start
    void Awake()
    {
        // hom3r.quickLinks.scriptsObject.GetComponent<OcclusionCommandReceiver>().occlusionManager = this.GetComponent<OcclusionManager>();
    }

    //////////////////////////////
    // REMOVE                   //
    //////////////////////////////
    
    public void RemoveArea(string areaId, THom3rCommandOrigin _origin)
    {
        GameObject objToRemove = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaId);
        this.RemoveGameObject(objToRemove, areaId, _origin);
    }

    /// <summary>Remove a game object from the scene</summary>
    /// <param name="objToRemove">Pointer to the object to be hide</param>
    public void RemoveGameObject(GameObject objToRemove, string areaID = null, THom3rCommandOrigin _origin = THom3rCommandOrigin.ui)
    {
        //1. De confirm the area/special node if it's confirmed
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.ConfirmationOff, objToRemove), Constants.undoNotAllowed);
        //2. Perform the remove algorithm        
        if (areaID == null) { areaID = objToRemove.GetComponent<ObjectStateManager>().areaID; }
        ExecuteRemove(areaID, _origin);
        //3. Update mode
        hom3r.state.currentIsolateMode = THom3rIsolationMode.WITH_REMOVEDNODES;
        //4. Re-focus
        //this.GetComponent<Isolate_Script>().ReFocusIsolatedGO();
    }
    
    /// <summary>Change an object to remove state</summary>
    /// <param name="goToRemove"></param>
    private void ExecuteRemove(string areaID, THom3rCommandOrigin _origin)
    {
        //Get the areaID of the area selected        
        //string areaID = goToRemove.GetComponent<ObjectStateManager>().areaID;
        if (areaID != null)
        {
            //Filled _areaList to be selected in a different way, depending of the selection mode
            if (hom3r.state.currentSelectionMode == THom3rSelectionMode.AREA)
            {
                string leafID = this.GetComponent<ModelManager>().GetNodeLeafID_ByAreaID(areaID);
                if (leafID != null)
                {
                    // Desconfirm and remove
                    List<GameObject> areasOfaLeaf = this.GetComponent<ModelManager>().GetAreaGameObjectList_ByLeafID(leafID);
                    foreach (GameObject go in areasOfaLeaf)
                    {
                        //Desconfirm the area if it's confirmed
                        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.ConfirmationOff, go), Constants.undoNotAllowed);
                        //Change the state of the object to hide (this method will be in charge of changing the object material)
                        go.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_On, 0.8f);
                    }
                    // Send list of areas to WebApp only if the action starts in the UserInterface
                    if (_origin == THom3rCommandOrigin.ui)
                    {                    
                        List<string> areaIDList = this.GetComponent<ModelManager>().GetAreaList_ByLeafID(leafID);
                        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Activate, areaIDList));
                    }                    
                }
                else { Debug.LogError("areaID not found"); }
            }
            else if (hom3r.state.currentSelectionMode == THom3rSelectionMode.SPECIAL_NODE)
            {
                //Select all the areas with the same special ancestor.
                string _specialNodeID = this.GetComponent<ModelManager>().GetSpecialAncestorID_ByAreaID(areaID);
                List<GameObject> _areaList = this.GetComponent<ModelManager>().GetAreaGameObjectList_BySpecialAncestorID(_specialNodeID);
                foreach (var item in _areaList)
                {
                    item.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_On, 0.8f);
                }

                // Send list of areas to WebApp
                List<string> areaIDList = this.GetComponent<ModelManager>().GetAreaIDList_BySpecialAncestorID(_specialNodeID);
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Activate, areaIDList));
            }
        }
    }

    public void ShowRemovedArea(string areaID, THom3rCommandOrigin _origin, float duration = 0.0f)
    {
        GameObject objToShow = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);
        this.ShowRemovedGameObject(objToShow, duration, _origin);

    }

    public void ShowRemovedGameObject(GameObject obj, float duration = 0.0f, THom3rCommandOrigin _origin = THom3rCommandOrigin.ui)
    {
        List<string> areaIdList = new List<string>();

        if (hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().IsRemovedGameObject(obj)) {

            if (obj.GetComponent<ObjectStateManager>() != null)
            {
                obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_Off, duration);
                areaIdList.Add(obj.GetComponent<ObjectStateManager>().areaID);
            }

            if ((areaIdList.Count != 0) && (_origin == THom3rCommandOrigin.ui))
            {
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Deactivated, areaIdList));
            }
        }        
    }

    public void StartStopRemoveMode()
    {
        if (hom3r.state.currentMode == THom3rMode.remove) { this.StopRemoveMode(); }
        else { this.StartRemoveMode(); }
    }
  
    private void StartRemoveMode()
    {
        hom3r.state.currentMode = THom3rMode.remove;
        if (hom3r.state.currentIsolateMode == THom3rIsolationMode.idle)
        {
            hom3r.state.currentIsolateMode = THom3rIsolationMode.WITH_REMOVEDNODES;
        }
    }

    private void StopRemoveMode()
    {
        if (hom3r.state.currentMode == THom3rMode.remove)
        {
            hom3r.state.currentMode = THom3rMode.idle;
        }            
    }
    //////////////////////////////
    // TRANSPARENCY             //
    //////////////////////////////
    
    public void StartSmartTransparency(THom3rCommandOrigin _origin)
    {        
        hom3r.state.smartTransparencyModeActive = true;     // Start transparency
        ExecuteSmartTransparency(true);                     //Execute algorithms                
        // Indicate to others
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_SmartTransparency_Enabled, _origin));      
    }


    public void StopSmartTransparency(THom3rCommandOrigin _origin)
    {
        //Reset objects materials
        hom3r.quickLinks.scriptsObject.GetComponent<TransparencyManager>().AllGameObjectTransparencyOff();
        hom3r.quickLinks.scriptsObject.GetComponent<HiddenManager>().RevealAllHiddenGameObjects();
        //Execute stop tranparency algorithms
        hom3r.state.smartTransparencyModeActive = false;
        ExecuteSmartTransparency(false);
        // Indicate to others
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_SmartTransparency_Disabled, _origin));
    }

    /// <summary>Execute SmartTransaprency</summary>
    /// <param name="status">true=start, false = stop</param>    
    private void ExecuteSmartTransparency(bool status)
    {
        if (status)
        {
            //Start Smart Transparency
            this.GetComponent<TransparencyManager>().SmartTransparencyStart();
        }
        else
        {
            //Stop Smart Transparency
            this.GetComponent<TransparencyManager>().SmartTransparencyStop();
        }
    }


    //////////////////////////////
    // EXPLOSION                //
    //////////////////////////////
    
    public void StartGlobalExplosion()
    {
        if (!hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().IsEmpty())
        {
            ExecuteExplosion(true, "global");
            //hom3r.coreLink.Do(new UICoreCommand(TUICommands.DisableExplodelayoutButton), Constants.undoNotAllowed);  //Disable LayoutExplosion
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplosionGlobalON));
            hom3r.state.currentExplosionMode = THom3rExplosionMode.EXPLODE;
        }
    }

    public void RevertGlobalExplosion()
    {        
        ExecuteExplosion(false, "global");
        //hom3r.coreLink.Do(new UICoreCommand(TUICommands.EnableExplodelayoutButton), Constants.undoNotAllowed);  //Enable LayoutExplosion                          
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplosionGlobalOFF));
        hom3r.state.currentExplosionMode = THom3rExplosionMode.IMPLODE;
    }
    
    public void StartLocalExplosionMode()
    {
        if (!hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().IsEmpty())
        {
            hom3r.state.currentMode = THom3rMode.local_explosion;            
        }
    }

    public void StopLocalExplosionMode()
    {        
        hom3r.state.currentMode = THom3rMode.idle;
        if (hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().IsAnyObjectExploded())
        {            
            hom3r.state.currentExplosionMode = THom3rExplosionMode.EXPLODE;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplosionChangedMode));
        }
        else
        {         
            hom3r.state.currentExplosionMode = THom3rExplosionMode.IMPLODE;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_ExplosionChangedMode));
        }
    }
        
    /// <summary>Execute Explosion</summary>
    /// <param name="status">true=start, false=stop</param>
    /// <param name="type">Type of explosion, allow "global" or "focussed".</param>
    private void ExecuteExplosion(bool status, string type)
    {
        if (status)
        {
            if (type == "global")
            {
                //Start Explosion
                hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().Implode();
                hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().Explode(1.0f);
            }
            else if (type == "focussed")
            {
                //Start Explosion
                hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().ExplodeConfirmedAll();
            }
        }
        else
        {
            //Stop Explosion
            hom3r.quickLinks._3DModelRoot.GetComponent<ExplosionManager>().Implode();
        }
    }
    
    //////////////////////////////
    // ISOLATE                //
    //////////////////////////////
    public void StartIsolate()
    {
        if (hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().GetNumberOfConfirmedGameObjects() > 0)
        {
            //Stop transparency mode if is activated
            if (hom3r.state.smartTransparencyModeActive)
            {
                hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency), Constants.undoNotAllowed);
            }
            //1. Update Core mode
            hom3r.state.currentIsolateMode = THom3rIsolationMode.ISOLATE;
            //2. Execute algorithms                        
            hom3r.quickLinks.scriptsObject.GetComponent<IsolateManager>().StartIsolateMode();
        }
        else
        {
            //TODO: Show messege in UI    
            //m.GetComponent<Core>().Do(new UICoreCommand(TUICommands.ShowAlertText, "No product was selected: there is nothing to focus"), Constants.undoNotAllowed);
        }
    }

    public void ShowAll()
    {
        //Exit transparency mode if is activate
        if (hom3r.state.smartTransparencyModeActive)
        {
            hom3r.coreLink.Do(new COcclusionCommand(TOcclusionCommands.DisableSmartTransparency), Constants.undoNotAllowed);
        }
        if (hom3r.state.currentIsolateMode == THom3rIsolationMode.ISOLATE)
        {
            //1. Update Core mode
            hom3r.state.currentIsolateMode = THom3rIsolationMode.idle;
            //2. Execute algorithms
            hom3r.quickLinks.scriptsObject.GetComponent<IsolateManager>().StopIsolateMode();
            //3. Update buttons
            if (hom3r.state.currentMode == THom3rMode.capturing_surface_point)
            {
                // hom3r.quickLinks.uiObject.GetComponent<UIManager>().UpdateDisableButtons_SinglePointMode(true);
            }
        }
        else if (hom3r.state.currentIsolateMode == THom3rIsolationMode.WITH_REMOVEDNODES)
        {
            //1. Update Core mode
            hom3r.state.currentIsolateMode = THom3rIsolationMode.idle;
            //2. Execute algorithms
            hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().RevealAllRemovedGameObjects(1.0f);
        }
    }


    public void UpdateOcclusionMode()
    {
        // Do nothing for now
    }
}
