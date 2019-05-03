using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionManager : MonoBehaviour
{
    // Awake is called at start
    void Awake()
    {
        
    }

    //////////////////////////////
    // REMOVE                   //
    //////////////////////////////
    
    /// <summary>Remove a game object from the scene</summary>
    /// <param name="obj">Pointer to the object to be hide</param>
    public void RemoveGameObject(GameObject obj)
    {
        //1. De confirm the area/special node if it's confirmed
        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.ConfirmationOff, obj), Constants.undoNotAllowed);
        //2. Perform the remove algorithm
        ExecuteRemove(obj);
        //3. Update mode
        hom3r.state.currentVisualizationMode = THom3rIsolationMode.WITH_REMOVEDNODES;
        //4. Re-focus
        //this.GetComponent<Isolate_Script>().ReFocusIsolatedGO();
    }
    
    /// <summary>Change an object to remove state</summary>
    /// <param name="goToRemove"></param>
    private void ExecuteRemove(GameObject goToRemove)
    {
        //Get the areaID of the area selected        
        string areaID = goToRemove.GetComponent<ObjectStateManager>().areaID;
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
                        //Desconfirm the area special node if it's confirmed
                        hom3r.coreLink.Do(new CSelectionCommand(TSelectionCommands.ConfirmationOff, go), Constants.undoNotAllowed);
                        //Change the state of the object to hide (this method will be in charge of changing the object material)
                        go.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_On, 0.8f);
                    }
                    // Send list of areas to WebApp
                    List<string> areaIDList = this.GetComponent<ModelManager>().GetAreaList_ByLeafID(leafID);
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Activate, areaIDList));
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


    public void ShowRemovedGameObject(GameObject obj, float duration = 0.0f)
    {
        List<string> areaIdList = new List<string>();

        if (hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().IsRemovedGameObject(obj)) {

            if (obj.GetComponent<ObjectStateManager>() != null)
            {
                obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_Off, duration);
                areaIdList.Add(obj.GetComponent<ObjectStateManager>().areaID);
            }

            if (areaIdList.Count != 0)
            {
                hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Deactivated, areaIdList));
            }
        }        
    }


    //////////////////////////////
    // TRANSPARENCY             //
    //////////////////////////////
    
    public void StartSmartTransparency()
    {        
        hom3r.state.smartTransparencyModeActive = true;     // Start transparency
        ExecuteSmartTransparency(true);                     //Execute algorithms                
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_SmartTransparencyON));      // Indicate to others
    }


    public void StopSmartTransparency()
    {
        //Reset objects materials
        hom3r.quickLinks.scriptsObject.GetComponent<TransparencyManager>().AllGameObjectTransparencyOff();
        hom3r.quickLinks.scriptsObject.GetComponent<HiddenManager>().RevealAllHiddenGameObjects();
                
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_SmartTransparencyOFF));      // Indicate to others

        //Execute algorithms
        hom3r.state.smartTransparencyModeActive = false;
        ExecuteSmartTransparency(false);
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
            hom3r.coreLink.SetCurrentMode(THom3rMode.LOCALEXPLOSION);            
        }
    }

    public void StopLocalExplosionMode()
    {
        hom3r.coreLink.SetCurrentMode(THom3rMode.IDLE);
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

}
