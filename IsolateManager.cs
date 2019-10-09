using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IsolateManager : MonoBehaviour
{
    private void Awake()
    {
        //hom3r.quickLinks.scriptsObject.GetComponent<OcclusionCommandReceiver>().isolateManager = this.GetComponent<IsolateManager>();
    }

    /// <summary> Isolate the selected objects. Make transparent all the objets not confirmed. </summary>	 //
    public void StartIsolateMode()
    {
        //1. Update Core mode
        hom3r.state.currentVisualizationMode = THom3rIsolationMode.ISOLATE;

        hom3r.quickLinks.scriptsObject.GetComponent<RemoveManager>().RemoveNotCorfirmedNodes(1.5f);       //Remove all not confirmed objects
        //this.FocusIsolatedGameObjects();               //Focus the list of confirmed objects objects
        this.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Isolate_Enabled), 1.6f);

    }

    /// <summary> Isolate the selected objects. Make transparent all the objets not confirmed. </summary>	 //
    public void IsolateAGivenGOList_ModeON(List<GameObject> goToRemoveList)
    {
        //Remove the object of the given list
        foreach (var obj in goToRemoveList)
        {
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_On);
        }
        //Focus the visible objects
        ReFocusIsolatedGO();
    }

    /// <summary> Navigate to focus the isolated gameobject or group of game objects </summary>
    public void FocusIsolatedGameObjects()
    {
        //Check that we have at least one confirmed object
        int nConfirmed = this.GetComponent<SelectionManager>().GetNumberOfConfirmedGameObjects();
        // Do when we have something selected
        if (nConfirmed > 0)
        {
            // Bounding box containing all confirmed objects, for approximation
            //Bounds totalBB = new Bounds();
            // Get info from all confirmed objects to compute targets
            //List<GameObject> listConfirmedObjets = this.GetComponent<SelectionManager>().GetListOfComponentConfirmedObjects();
            //totalBB = hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().ComputeBoundingBox(listConfirmedObjets);//FIXME after Navigation Refactoring
            //Move the camera to focus the isolated object
            //StartCoroutine(hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().NavigateToFocusObject_BothNavSystems(totalBB));//-->ORIGINAL //FIXME after Navigation Refactoring
            //StartCoroutine(camera_OrbitPlane.GetComponent<Navigation_Script>().NavigateToFocusObject_andResetPos_withoutRotation(totalBB));
            //StartCoroutine(camera_OrbitPlane.GetComponent<Navigation_Script>().NavigateToFocusObject_andResetPos_BothNavSystems(totalBB));

            // Launch command to core
            //this.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Isolate_Enabled), 1.6f);
        }
        else
        {
            //Debug.LogError("ANY GameObject selected");
            //TODO: show messege in UI
            //this.GetComponent<Core>().Do(new UICoreCommand(TUICommands.ShowAlertText, "No product was selected: there is nothing to focus"), Constants.undoNotAllowed);
        }
    }

    /// <summary> Navigate to focus the visible gameobjects </summary>
    public void ReFocusIsolatedGO()
    {
        //Get list of visible objects
        List<GameObject> objVisibleList = this.GetComponent<ModelManager>().GetAreaNodeList().FindAll(x => !this.GetComponent<RemoveManager>().GetRemovedList().Contains(x));

        // Do when we have something selected
        if (objVisibleList.Count > 0)
        {
            // Bounding box containing all confirmed objects, for approximation
            //Bounds totalBB = hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().ComputeBoundingBox(objVisibleList);  //FIXME after Navigation Refactoring
            //Move the camera to focus the isolated object
            //StartCoroutine(hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().NavigateToFocusObject_BothNavSystems(totalBB));//-->ORIGINAL  //FIXME after Navigation Refactoring
            //StartCoroutine(camera_OrbitPlane.GetComponent<Navigation_Script>().NavigateToFocusObject_andResetPos_withoutRotation(totalBB));
            // Launch command to core
            this.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Isolate_Enabled));
        }
        else
        {
            //Debug.LogError("ANY GameObject selected");
            //TODO: Show messege in UI    
            //this.GetComponent<Core>().Do(new UICoreCommand(TUICommands.ShowAlertText, "No product was selected: there is nothing to focus"), Constants.undoNotAllowed);
        }
    }

    public void ReFocusIsolatedGO_withRotation()
    {
        //Get list of visible objects
        List<GameObject> objVisibleList = this.GetComponent<ModelManager>().GetAreaNodeList().FindAll(x => !this.GetComponent<RemoveManager>().GetRemovedList().Contains(x));

        // Do when we have something selected
        if (objVisibleList.Count > 0)
        {
            // Bounding box containing all confirmed objects, for approximation
            //Bounds totalBB = hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().ComputeBoundingBox(objVisibleList);  //FIXME after Navigation Refactoring
            // Move the camera to focus the isolated object
            //StartCoroutine(hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().NavigateToFocusObject_andResetPos_BothNavSystems(totalBB));      //FIXME after Navigation Refactoring
            // Launch command to core
            this.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Isolate_Enabled));
        }
        else
        {
            //Debug.LogError("ANY GameObject selected");
            //TODO: Show messege in UI    
        //this.GetComponent<Core>().Do(new UICoreCommand(TUICommands.ShowAlertText, "No product was selected: there is nothing to focus"), Constants.undoNotAllowed);
        }
    }

    /// <summary> Deactivate the Isolate Mode and reset the View </summary>
    public void StopIsolateMode()
    {
        //1. Reset View
        // hom3r.quickLinks.orbitPlane.GetComponent<NavigationManager>().ResetView_BothNavSystems();  //FIXME after Navigation Refactoring
        //2. Reset object materials (show every components)
        this.GetComponent<TransparencyManager>().AllGameObjectTransparencyOff();
        this.GetComponent<RemoveManager>().RevealAllRemovedGameObjects(1.5f);
        // Launch command to core
        this.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Isolate_Disabled), 0.1f);
    }
   
}
