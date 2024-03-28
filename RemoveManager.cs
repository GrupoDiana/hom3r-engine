using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RemoveManager : MonoBehaviour {

    List<GameObject> gameObjectRemoveList;          //In this list will be stored the game-objects that have been made hidden

    private void Awake()
    {
        // hom3r.quickLinks.scriptsObject.GetComponent<OcclusionCommandReceiver>().removeManager = this.GetComponent<RemoveManager>();
        gameObjectRemoveList = new List<GameObject>();      //We create a list in which will be stored the game-objects that are hidden        
    }
    	
    /////////////////////////////
    /////  Public Methods
    ////////////////////////////
    
    /// <summary>Check if an object has been removed</summary>     
    public bool IsRemovedGameObject(GameObject obj)
    {        
        return gameObjectRemoveList.Contains(obj);
    }
    
    /// <summary>Check if an area has been removed</summary>     
    public bool IsRemovedArea(string _areaId)
    {
        if (!hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(_areaId)) { return false; }

        GameObject obj = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(_areaId);
        if (obj == null ) { return false; }

        return this.IsRemovedGameObject(obj);
    }

    /// <summary>Add an object to Remove list.</summary>    
    public void AddToRemovedList(GameObject obj )
    {
        if (!IsRemovedGameObject(obj))  {
            gameObjectRemoveList.Add(obj);
            string _areaID = obj.GetComponent<ObjectStateManager>().areaID;  
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Removed_Area, _areaID));
        }
    }
    
    /// <summary> Remove an object from removed list.</summary>    
    public void RemoveFromRemovedList(GameObject obj)
    {
        if (IsRemovedGameObject(obj)) {
            gameObjectRemoveList.Remove(obj);
            string _areaID = obj.GetComponent<ObjectStateManager>().areaID;
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_Shown_Area, _areaID));
        }
    }

    /// <summary>Reveals all hidden game objects.</summary>
	public void RevealAllRemovedGameObjects()
    {
        //Reveal all hidden objects
        List<GameObject> temp = new List<GameObject>(gameObjectRemoveList);
        List<string> areaIdList = new List<string>();

        float duration = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetDurationRemoveAnimation();

        foreach (GameObject obj in temp) {
            if (obj.GetComponent<ObjectStateManager>() != null) {
                //obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateCommands.Remove_Off, duration);
                obj.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Remove_Off, duration));
                areaIdList.Add(obj.GetComponent<ObjectStateManager>().areaID);
            }                
        }
        if (areaIdList.Count != 0)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Deactivated, areaIdList));
        }
    }

    /// <summary>Make hidden all the objects not confirmed.</summary>    
    public void RemoveNotCorfirmedNodes()
    {
        List<GameObject> removedAreas = new List<GameObject>();

        //Call the recursive algorithm with the Turbine Father		
        removedAreas = RemoveNotCorfirmedNodesRecursive(hom3r.quickLinks._3DModelRoot);

        //Emit event with removes areas
        this.EmitRemovedAreaEvent(removedAreas);        
    }

    /// <summary>Recursively runs through the turbine and becomes transparent objects that are unconfirmed     
    private List<GameObject> RemoveNotCorfirmedNodesRecursive(GameObject obj)
    {
        List<GameObject> _removedAreas = new List<GameObject>();
        float duration = hom3r.quickLinks.scriptsObject.GetComponent<ConfigurationManager>().GetDurationRemoveAnimation();

        //If the object is not in the list of confirmed we make it transparent
        if (obj.GetComponent<Renderer>())
        {
            if (!this.GetComponent<SelectionManager>().IsConfirmedNodeLeaf(obj))
            {                
                obj.GetComponent<ObjectStateManager>().Do(new CObjectVisualStateCommand(TObjectVisualStateCommands.Remove_On, duration));
                _removedAreas.Add(obj);
                return _removedAreas;
                //}
            }
        }
        //Call again to itself with one child
        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject newChild = obj.transform.GetChild(i).gameObject;
            _removedAreas.AddRange(RemoveNotCorfirmedNodesRecursive(newChild));
        }
        return _removedAreas;
    }


    private void EmitRemovedAreaEvent(List<GameObject> objList)
    {
        List<string> areaIDList = new List<string>();

        foreach (GameObject obj in objList)
        {
            if (obj != null) {
                if (obj.GetComponent<ObjectStateManager>().areaID != null || obj.GetComponent<ObjectStateManager>().areaID != "")
                {
                    areaIDList.Add(obj.GetComponent<ObjectStateManager>().areaID);
                }

            }                                              
        }
        if (areaIDList.Count != 0)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Activate, areaIDList));
        }        
    }

    public List<GameObject> GetRemovedList() {
        return gameObjectRemoveList;
    }
}
