/*******************Hidden_SCRIPT****************************************************
 * 
 * Script implmented to hidde and reveal objects
 * 
 * 
 * Creation Date:  08/04/2015
 * Last Update: 08/04/2015
 *
 * UiW European Project
 * Grupo DIANA - University of Malaga
 ***************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HiddenManager : MonoBehaviour {

	//In this list will be stored the gameobjects that have been made hidden
	List<GameObject> gameObjectHiddenList;

	//To store a link to the object that contain the scripts
	//GameObject goTurbine;	

	//To store the state of the intelli
	public bool smartHideState;

	//To know if the mouse is on motion
	// bool mouseOnMotion;

    private void Awake()
    {        
        gameObjectHiddenList = new List<GameObject>();  //We create a list in which will be stored the gameobjects that are hidden
        smartHideState = false;
       // mouseOnMotion = false;
    }
    
	// Update is called once per frame
	void Update () {
		// mouseOnMotion = Input.GetMouseButton(1);
	}

	//////////////////////////////////////////////////////////////////////////////
	/// <summary> Method to return the number of hidden objects.</summary>
	/// <returns> The of tranparent game objects.</returns>
	//////////////////////////////////////////////////////////////////////////////
	public int NumberOfHiddenGameObjects()
	{
		return gameObjectHiddenList.Count;
	}


	/////////////////////////////////////////////////////////////
	/// <summary> Check if an object is hidden</summary> //
	/////////////////////////////////////////////////////////////
	public bool IsHiddenGameObject(GameObject obj)
	{
		//Debug.Log (obj.name);
		return gameObjectHiddenList.Contains (obj);
	}//END IsHiddenGameObject
	
	/////////////////////////////////////////////////////
	/// <summary> Make an object transparent.</summary> //
	/////////////////////////////////////////////////////
	public void GameObjectHiddenOn(GameObject obj, float duration=0.0f) 
	{
		if (!IsHiddenGameObject (obj)) 
		{
			//if (obj.GetComponent<ObjectState_Script> ()!=null){
				//We store the object that we are removing into a list.
				gameObjectHiddenList.Add (obj);
				//We change the material of the object to the transparent one	
				//obj.GetComponent<ObjectState_Script> ().GameObjectHiddenOn (duration);
			//}
		}
	}//END GameObjectHiddenOn

	////////////////////////////////////////////////////////////////
	/// <summary> Reveal an object.</summary> //
	////////////////////////////////////////////////////////////////
	public void GameObjectHiddenOff(GameObject obj, float duration = 0.0f)
	{
		if (IsHiddenGameObject (obj))
		{
			//if (obj.GetComponent<ObjectState_Script> ()!=null)
			//{
				//We store the object that we are removing into a list.
				gameObjectHiddenList.Remove (obj);
				//We change the material of the object to the transparent one
				//obj.GetComponent<ObjectState_Script> ().GameObjectHiddenOff (duration);
			//}
		}
	}//END GameObjectHiddenOff

	
	/// <summary> Make hidden a list of objets. </summary>	
	public void GameObjectListHiddenOn(List<GameObject> listObj, float duration) 
	{
	    //Move in the list of objects and make Hidden
	    foreach (GameObject obj in listObj) {            
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Hidden_On, duration);
	    }
	}//END GameObjectListHideOn
	
	
	/// <summary> Make not Hidde a list of objets. </summary>	
	public void GameObjectListHiddenOff(List<GameObject> listObj, float duration)
	{
		//Move in the list of objects and make transparent
		foreach (GameObject obj in listObj)
        {		
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Hidden_Off, duration);
        }
	}//END GameObjectListHideOff


	/// <summary>Reveals all hidden game objects.</summary>
	public void RevealAllHiddenGameObjects(float duration = 0.0f){

        //Reveal all hidden objects
        List<GameObject> temp = new List<GameObject>(gameObjectHiddenList);
		foreach (GameObject obj in temp) { 
			if (obj.GetComponent<ObjectStateManager> ()!=null)
				obj.GetComponent<ObjectStateManager> ().SendEvent(TObjectVisualStateEvents.Hidden_Off, duration);
		}
		//Empty the list of hidden objects
		gameObjectHiddenList.Clear (); 
	}//END RevealAllHiddenGameObjects

	
	/// <summary>Reveals all hidden game objects.</summary>	
	//public void FromHidetoTransparentAllGameObjects()
 //   {
 //       //Reveal all hidden objects
 //       List<GameObject> temp = new List<GameObject>(gameObjectHiddenList);
	//	foreach (GameObject obj in temp)
 //       { 			
 //           obj.GetComponent<ObjectState_Script>().SendEvent(ObjectVisualStateEvents_Type.Transparency_On, 0.0f);
 //       }	
	//}

	/////////////////////////////////////////////////////////
	/// <summary> Start the smart transparency.</summary>
	/////////////////////////////////////////////////////////
	//public void smartHideStart()
	//{
	//	if (!smartHideState) {
	//		//Change the boolean value that control when is on this mode
	//		smartHideState = true;
	//		//Start Smart Hide
	//		StartCoroutine (smartHide ());	
	//		//Debug.Log ("Smart Hide Start");
	//	}
	//}//END smartTransparencyStart
	
	/////////////////////////////////////////////////////
	/// <summary> Stop the Smart transparency.</summary>
	///////////////////////////////////////////////////// 
	//public void smartHideStop()
	//{
	//	//Debug.Log ("Smart Hide Stop");
	//	smartHideState = false;
	//	//FromHidetoTransparentAllGameObjects ();

	//}//END smartTransparencyStop
	
	/////////////////////////////////////////////////////////
	/// <summary> Smart transparency.</summary>
	///////////////////////////////////////////////////////////
	//IEnumerator smartHide()
	//{
	//	//Temp Var to store the list of objects to make transparent (distractors).
	//	List<GameObject> distractorList;
	//	distractorList = new List<GameObject>();
		
	//	//Temporaly List of obj to make visible
	//	List<GameObject> toMakeList;
	//	toMakeList = new List<GameObject>();
				
	//	//Check if the smartTransparency is activate
	//	while (smartHideState) 
	//	{
	//		//Debug.Log("mouseOnMotion: "+mouseOnMotion);
	//		//Activate mesh colliders if we are navigating
	//		if (mouseOnMotion)
 //           {
 //               List<GameObject> temp = new List<GameObject>(gameObjectHiddenList);
	//			foreach (GameObject obj in temp) { 
	//				if (obj.GetComponent<ObjectState_Script> ()!=null){						
 //                       obj.GetComponent<ObjectState_Script>().SendEvent(ObjectVisualStateEvents_Type.Collider_On);
 //                   }
	//			}
	//		}
 //           //yield return new WaitForSeconds (0.25f);
 //           //Execute the "Capsulazo" algorithm to determine the list of objects to make transparent (distractors)
 //           //distractorList = this.GetComponent<Transparency_Script> ().OcclusionDetection_CapsuleCast ();
 //           //Execute the "MultipleRaysCast" algorithm to determine the list of objects to make transparent (distractors)
 //           distractorList = this.GetComponent<Transparency_Script>().OcclusionDetection_RayCast();
 //           //Debug.Log("distractorList.Count: "+distractorList.Count);
 //           if (distractorList != null) {
	//			if (mouseOnMotion) {
	//				//Move in the list of transparent objects and check with the list of distractors
	//				//If a object is not a distractor we make it visible. 
	//				toMakeList = gameObjectHiddenList.FindAll(x=> !distractorList.Contains(x));
	//				//Make visible objects that are not a distractor now					
	//				GameObjectListHiddenOff(toMakeList, 0.0f);
 //                   //Deactive the collider of the hidden objects
 //                   List<GameObject> temp = new List<GameObject>(gameObjectHiddenList);
 //                   foreach (GameObject obj in temp)
 //                   { 
	//					if (obj.GetComponent<ObjectState_Script> ()!=null){
 //                           obj.GetComponent<ObjectState_Script>().SendEvent(ObjectVisualStateEvents_Type.Collider_Off);
	//					}
	//				}
	//			}
				
	//			//Move in the list of distractor and check if it is already transparent
	//			//If not we make it transparent
	//			toMakeList = distractorList.FindAll(x=> !gameObjectHiddenList.Contains(x));
	//			GameObjectListHiddenOn(toMakeList, 0.0f);
	//			//Debug.Log("Hacer transparentes: " + toMakeList.Count);
	//		}//END if
	//		distractorList.Clear();
	//		toMakeList.Clear();				
	//		//Execute just 1 time per second
	//		yield return new WaitForSeconds (1.0f);
	//	}//END if
	//	distractorList.Clear();
	//	toMakeList.Clear();					
	//}//END smartHide

        
    /// <summary> Make hidde all the objets not confirmed.</summary>    
    public void EasyHide(float duration = 0.0f)
    {        
        EasyHideRecursive(hom3r.quickLinks._3DModelRoot, duration);     //Call the recursive algorithm with the Turbine Father		
    } 

    /// <summary> Recursively runs through the turbine and becomes transparent objects that are unconfirmed     
    void EasyHideRecursive (GameObject obj, float duration=0.0f)
	{
		//If the obj is not in the list of confimed we make it transparente.
		if (obj.GetComponent<Renderer>())
		{            
            if (!this.GetComponent<SelectionManager> ().IsConfirmedNodeLeaf(obj))
            {
                //if (!this.GetComponent<SinglePointManager>().IsASinglePoint(obj))
                //{
                //    obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Hidden_On, duration);
                //}
			}            
		}
		//Call again to itself with one child
		for (int i = obj.transform.childCount - 1; i >= 0; i--)
		{
			GameObject newChild = obj.transform.GetChild(i).gameObject;
                EasyHideRecursive(newChild, duration);
		}
    }//END EasyHideRecursive
                
    /// <summary>
    /// Recalculate hidden objects when objects are add o remove from the confirmed list
    /// </summary>    
    /// <param name="duration">Time spend into animations</param>
    public bool RecalculateEasyHide(List<GameObject> objectAreaList, string type, float duration = 0.0f)
    {
        bool needFocus = false;
                
        foreach (GameObject area in objectAreaList)
        {
            if (type == "show")
            {
                if (IsHiddenGameObject(area))
                {
                    //GameObjectHiddenOff(area, duration);
                    area.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Hidden_Off);
                    needFocus = true;
                }
            }
            else if (type == "hidden")
            {
                if (!this.GetComponent<SelectionManager>().IsConfirmedNodeLeaf(area))
                {                    
                    area.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Hidden_On, duration);
                    needFocus = true;
                }               
            }
        }

        return needFocus;
        //TODO Hacer visibles los hijos del padre seleccionado        
    }//END EasyHideRecursive

   public List<GameObject> GetHiddenList() {
        return gameObjectHiddenList;
    }


    //void EasyHideRecursive_v2(GameObject obj, float duration = 0.0f)
    //{
    //    //If the obj is not in the list of confimed we make it transparente.
    //    if (obj.GetComponent<Renderer>())
    //    {
    //        if (this.GetComponent<Selection2_Script>().IsConfirmedNodeLeaf(obj))
    //        //if (!this.GetComponent<Selection2_Script>().IsConfirmedGameObject(obj))
    //        {
    //            GameObjectHiddenOff(obj, duration);
    //        }
    //        else
    //        {               
    //            GameObjectHiddenOn(obj, duration);
    //        }

    //    }
    //    //Call again to itself with one child
    //    for (int i = obj.transform.childCount - 1; i >= 0; i--)
    //    {
    //        GameObject newChild = obj.transform.GetChild(i).gameObject;
    //        EasyHideRecursive(newChild, duration);
    //    }
    //}//END EasyHideRecursive
}//END class



