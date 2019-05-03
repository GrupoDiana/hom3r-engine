using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RemoveManager : MonoBehaviour {

    List<GameObject> gameObjectRemoveList;          //In this list will be stored the game-objects that have been made hidden

    private void Awake()
    {
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
    
    /// <summary>Add an object to Remove list.</summary>    
    public void AddToRemovedList(GameObject obj )
    {
        if (!IsRemovedGameObject(obj))  {
            gameObjectRemoveList.Add(obj);
            string _areaID = obj.GetComponent<ObjectStateManager>().areaID;            
        }
    }
    
    /// <summary> Remove an object from removed list.</summary>    
    public void RemoveFromRemovedList(GameObject obj)
    {
        if (IsRemovedGameObject(obj)) {
            gameObjectRemoveList.Remove(obj);
            //string _areaID = obj.GetComponent<ObjectStateManager>().areaID;            
        }
    }

    /// <summary>Reveals all hidden game objects.</summary>
	public void RevealAllRemovedGameObjects(float duration = 0.0f)
    {
        //Reveal all hidden objects
        List<GameObject> temp = new List<GameObject>(gameObjectRemoveList);
        List<string> areaIdList = new List<string>();
        foreach (GameObject obj in temp) {
            if (obj.GetComponent<ObjectStateManager>() != null) {
                obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_Off, duration);
                areaIdList.Add(obj.GetComponent<ObjectStateManager>().areaID);
            }                
        }
        if (areaIdList.Count != 0)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.RemovedPart_Deactivated, areaIdList));
        }
    }

    /// <summary>Make hidden all the objects not confirmed.</summary>    
    public void RemoveNodes(float duration = 0.0f)
    {
        //Call the recursive algorithm with the Turbine Father		
        RemoveNodesRecursive(hom3r.quickLinks._3DModelRoot, duration);
    } //END EasyHide

    /// <summary>Recursively runs through the turbine and becomes transparent objects that are unconfirmed     
    void RemoveNodesRecursive(GameObject obj, float duration = 0.0f)
    {
        //If the object is not in the list of confirmed we make it transparent
        if (obj.GetComponent<Renderer>())
        {
            if (!this.GetComponent<SelectionManager>().IsConfirmedNodeLeaf(obj))
            {
                //if (!this.GetComponent<SinglePointManager>().IsASinglePoint(obj))
                //{
                //    obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Remove_On, duration);
                //}
            }
        }
        //Call again to itself with one child
        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject newChild = obj.transform.GetChild(i).gameObject;
            RemoveNodesRecursive(newChild, duration);
        }
    }

    public List<GameObject> GetRemovedList() {
        return gameObjectRemoveList;
    }
}
