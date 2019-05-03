using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectStateCommonManager : MonoBehaviour {

    private void Awake()
    {
        
    }

    /// <summary>Return if a area is in visible condition</summary>
    /// <param name="areaID">Area ID of the area to checked</param>
    /// <returns>true if one area is visible</returns>
    public bool IsAreaVisible(string areaID)
    {
        GameObject obj = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(areaID);
        if (obj != null)
        {
            return (IsAreaVisible(obj.GetComponent<ObjectStateManager>().GetVisualState()));            
        }
        return false;
    }      
  
    public bool IsLeafVisible(string leafID)
    {
        bool result = false;
        List<GameObject> objList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByLeafID(leafID);
        if (objList != null)
        {
            foreach (GameObject obj in objList)
            {
                if (IsAreaVisible(obj.GetComponent<ObjectStateManager>().GetVisualState()))
                {
                    result = true;
                }
                else
                {
                    return false;
                }
            }            
        }
        return result;
    }

    /// <summary>Return if a node is in visible condition</summary>
    /// <param name="nodeID">Node ID of the node to checked</param>
    /// <returns>true if the node is visible</returns>
    public bool IsNodeVisible(string nodeID)
    {
        List<GameObject> nodeAreaList = new List<GameObject>();
        nodeAreaList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(nodeID);    //Find the list of areas of this node       

        if (nodeAreaList!=null &&  nodeAreaList.Count != 0)
        {
            foreach (GameObject area in nodeAreaList)
            {
                if (!IsAreaVisible(area.GetComponent<ObjectStateManager>().GetVisualState()))
                {
                    return false;
                }
            }
            return true;
        }
        return false;        
    }

    //public List<GameObject> GetListAreaNotRemoved(string productNodeID)
    //{

    //    List<GameObject> notRemovedGameObjectList = new List<GameObject>();
    //    List<GameObject> productNodeArea_List = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(productNodeID);    //Find the list of areas of this node       

    //    foreach (GameObject obj in productNodeArea_List)
    //    {
    //        if ((obj.GetComponent<ObjectStateManager>().GetVisualState() != TObjectVisualStates.Remove_Idle))
    //        {
    //            notRemovedGameObjectList.Add(obj);
    //        }
    //    }
    //    return notRemovedGameObjectList;
    //}

    /// <summary>Return if a area is in visible condition</summary>
    /// <param name="areaState">area state</param>
    /// <returns>true if it visible</returns>
    private bool IsAreaVisible(TObjectVisualStates areaState)
    {
        if ((areaState != TObjectVisualStates.Hidden_Idle) && (areaState != TObjectVisualStates.Hidden_Collider_On) && (areaState != TObjectVisualStates.Remove_Idle))
        {
            return true;
        }
        return false;
    }

}
