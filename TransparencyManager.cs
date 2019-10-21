/*******************TRANSPARENCY_SCRIPT****************************************************
 * 
 * Script implmented to make, unmake and more actions with transparent objects
 * 
 * 
 * Creation Date:  26/03/2015
 *
 * UiW European Project
 * Grupo DIANA - University of Malaga
 ***************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransparencyManager : MonoBehaviour {

	//Variables
	List<GameObject> gameObjectTransparentList;                 //List of transparent gameobjects
    public bool smartTransparencyState;                         //State of the smart transparency
    //private GameObject goTurbine;                               //Link to the object that contain the scripts    
    //private GameObject mainCamera;                              //Link to the object that contains the main camera
    WaitForSeconds capsuleCastWait = new WaitForSeconds(1.0f);  //Smart transparency corutine wait time

    //AdaptativeTransparency- RayCast
    Vector3 mainCameraPos;                      //Main Camera Global position 
    List<Vector3> RayDestinationPoints_List;    //List to store every destinition points (Vector3) to draw the ray. Ray comes from the camera to the destity points
    //Constants 
    public const int RAYS_PER_AXIS = 5;        //Num of rays to be casted to the selected Area per semi-axes
    public const int MAX_RAYS_TOTAL = 100;      //Maximum number of rays to be casted in total



    /////////////////////////////////////////////////////////////////////////// 
    /// <summary> Start this instance. Use this for initialization.</summary> //
    ///////////////////////////////////////////////////////////////////////////
    private void Awake()
    {                
        gameObjectTransparentList = new List<GameObject>();     //We create a new blank list        
        RayDestinationPoints_List = new List<Vector3>();        //Destination points for rays        
        smartTransparencyState = false;                         //Initialize smart transperency state
    }

	///////////////////////////////////////////////////////////////
	////////////////////// Others  Methods ////////////////////////
	///////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////////
	/// <summary> Method to return the number of transparent objects.</summary>
	/// <returns> The of tranparent game objects.</returns>
	//////////////////////////////////////////////////////////////////////////////
	public int NumberOfTranparentGameObjects()
	{
		return gameObjectTransparentList.Count;
	}

	/////////////////////////////////////////////////////////////
	/// <summary> Check if an object is transparent.</summary> //
	/////////////////////////////////////////////////////////////
	public bool IsTransparentGameObject(GameObject obj)
	{
		return gameObjectTransparentList.Contains (obj);
	}//END IsTransparentGameObject

	/// <summary> Numbers the of transparent game objects. </summary>
	/// <returns>The of transparent game objects.</returns>
	public int NumberOfTransparentGameObjects()
	{
		return gameObjectTransparentList.Count;
	}

	/////////////////////////////////////////////////////
	/// <summary> Make an object transparent.</summary> //
	/////////////////////////////////////////////////////
	public void TransparencyOn(GameObject obj) 
	{
		if (!IsTransparentGameObject (obj)) 
		{
			//We store the object that we are removing into a list.
			gameObjectTransparentList.Add (obj);

		}
	}//END GameObjectTransparencyON

	////////////////////////////////////////////////////////////////
	/// <summary> Remove the transparency of an object.</summary> //
	////////////////////////////////////////////////////////////////
	public void TransparencyOff(GameObject obj)
	{
		if (IsTransparentGameObject (obj))
		{
			//We store the object that we are removing into a list.
			gameObjectTransparentList.Remove (obj);
			//We change the material of the object to the transparent one
			//obj.GetComponent<ObjectState_Script> ().GameObjectTransparencyOff ();
		}
	}//END GameObjectTransparencyOff

	public bool IsTransparentGameObjectList(List<GameObject> listObj)
	{
		bool temp;
		temp = true;
		//Move in the list of objects and check if is transparent
		foreach (GameObject obj in listObj) { 
			temp = temp & gameObjectTransparentList.Contains (obj);		
		}//End foreach
		//temp is true only if all the objects are transparent (are in the list)
		return temp;
	}

	///////////////////////////////////////////////////////////////////////
	/// <summary> Remove the transparency of all the objects.</summary>	 //
	///////////////////////////////////////////////////////////////////////
	public void AllGameObjectTransparencyOff()
	{
        //Make not transparent one by one
        List<GameObject> temp = new List<GameObject>(gameObjectTransparentList);
		foreach (GameObject obj in temp)
        { 
			//obj.GetComponent<ObjectState_Script> ().GameObjectTransparencyOff ();
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Transparency_Off , 0.5f);
        }
		//Empty the list of hidden objects
		gameObjectTransparentList.Clear (); 
	}


	////////////////////////////////////////////////////////////////////////////
	/// <summary> Make transparent a list of objets. </summary>               //
	////////////////////////////////////////////////////////////////////////////
	public void GameObjectListTransparencyOn(List<GameObject> listObj, float duration) 
	{
		//Move in the list of objects and make transparent
		foreach (GameObject obj in listObj)
        {
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Transparency_On, duration);
		}
	}
	
	////////////////////////////////////////////////////////////////////////////
	/// <summary> Make not transparent a list of objets. </summary>           //
	////////////////////////////////////////////////////////////////////////////
	public void GameObjectListTransparencyOff(List<GameObject> listObj, float duration)
	{
		//Move in the list of objects and make transparent
		foreach (GameObject obj in listObj) {
            obj.GetComponent<ObjectStateManager>().SendEvent(TObjectVisualStateEvents.Transparency_Off, duration);
        }
	}

    /////////////////////////////
    /// ALPHA Level
    ///////////////////////////// 
    
    /// <summary>
    /// 
    /// </summary>
    private void SetSmartTransparencyLevelToAllTransparentObjects()
    {
        foreach (GameObject obj in gameObjectTransparentList)
        {
            ObjectStateMaterialUtils.SetAlphaColorToMaterial(obj.GetComponent<Renderer>().material, hom3r.state.smartTransparencyAlphaLevel);
        }
    }


    public void SetSmartTransparencyAlphaLevel(float value, THom3rCommandOrigin _origin)
    {
        hom3r.state.smartTransparencyAlphaLevel = value;
        if (_origin == THom3rCommandOrigin.io)
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.Occlusion_SmartTransparency_AlphaLevelUpdated));
        }        
        this.SetSmartTransparencyLevelToAllTransparentObjects();
    }








	/////////////////////////////////////////////////////////
	/// <summary> Start the smart transparency.</summary>
	/////////////////////////////////////////////////////////
	public void SmartTransparencyStart()
	{
		if (!smartTransparencyState) {
			//Change the boolean value that control when is on this mode
			smartTransparencyState = true;
			//Excute the smartTrsnaparency
			StartCoroutine (SmartTransparency ());	
		}
	}//END smartTransparencyStart

	/////////////////////////////////////////////////////
	/// <summary> Stop the Smart transparency.</summary>
	///////////////////////////////////////////////////// 
	public void SmartTransparencyStop()
	{
		smartTransparencyState = false;
	}//END smartTransparencyStop


    /// <summary> Corutine to perform the smart transperency doing a multiple ray cast each 0.3 ms </summary>
    /// <returns></returns>
    IEnumerator SmartTransparency()
    {
        //Temp Var to store the list of objects to make transparent (distractors).
        List<GameObject> distractorList;
        distractorList = new List<GameObject>();

        //Temporaly List of obj to make visible
        List<GameObject> toMakeList;
        toMakeList = new List<GameObject>();

        //Calculate rays destination points and save them in a class parameter: RayDestinationPoints_List. This method will be also called  from the Core when the area selectiopn change.
        SetRayDestinationPoints_AllSelectedAreas();

        //Check if the smartTransparency is activate
        while (smartTransparencyState)
        {
            //Execute the "MultipleRaysCast" algorithm to determine the list of objects to make transparent (distractors)
            distractorList = OcclusionDetection_RayCast();
          
            if (distractorList != null)
            {
                //Move in the list of transparent objects and check with the list of distractors
                //If a object is not a distractor we make it visible. 
                toMakeList = gameObjectTransparentList.FindAll(x => !distractorList.Contains(x));
                //Make visible objects that are not a distractor now					
                GameObjectListTransparencyOff(toMakeList, 0.5f);
                //Move in the list of distractor and check if it is already transparent
                //If not we make it transparent
                toMakeList = distractorList.FindAll(x => !gameObjectTransparentList.Contains(x));
                GameObjectListTransparencyOn(toMakeList, 0.5f);
            }//END if
            distractorList.Clear();
            toMakeList.Clear();
            //Execute just 3 time per second
            yield return new WaitForSeconds(0.3f);
        }//END while
        distractorList.Clear();
        toMakeList.Clear();

    }


    /// <summary> Algorithm to do the smart tranparency using raycast with multiple rays </summary>
    /// <returns>gameObjectDistractorList list of distractors gameobject list</returns>
    public List<GameObject> OcclusionDetection_RayCast()
    {
        List<GameObject> gameObjectDistractorList = new List<GameObject>(); //List to store every distractor game object
        List<RaycastHit[]> rayCastHitsList = new List<RaycastHit[]>();      //List to store the RaycastHit[] arrays returned by the (Physics.RaycastAll method
        GameObject temp_gameObjectHit;                                      //GameObject to move through the Raycast list
        Vector3 ray_direction;                                              //variable to store each ray direction

        // Get list of confirmed components, that include every area 
        List<GameObject> listConfirmedObjets = this.GetComponent<SelectionManager>().GetListOfComponentConfirmedObjects();        
        //Get the ID of the turbine layer
        //int turbineLayer = 1 << LayerMask.NameToLayer( "go_father_layer");
        int turbineLayer = 1 << LayerMask.NameToLayer(hom3r.state.productRootLayer);

        //*** Ray cast Process
        if (listConfirmedObjets != null)
        {
            //1. Update Camera position
            mainCameraPos = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
            
            //2. Do the RayCast
            for (int i = 0; i < RayDestinationPoints_List.Count/*numOfRays*/; i++) {
                ray_direction = RayDestinationPoints_List[i] - mainCameraPos;                                                   //Calculate the direction of the ray
                rayCastHitsList.Add (Physics.RaycastAll(mainCameraPos, ray_direction, ray_direction.magnitude, turbineLayer));  //RayCast only the turbine father layer
            }

            //3. Create the distractor gameobjects list with the rayCastHitsList         
            foreach (RaycastHit[] oneRay_GOList_Hit in rayCastHitsList)
            {
                    foreach (RaycastHit oneRay_GO_Hit in oneRay_GOList_Hit)
                    {
                    temp_gameObjectHit = oneRay_GO_Hit.transform.gameObject;
                        //Check if the obj is ones of the selected object and if it's not in the distractor list (for not being included again)
                        if (temp_gameObjectHit != null && !listConfirmedObjets.Contains(temp_gameObjectHit) && !gameObjectDistractorList.Contains(temp_gameObjectHit))
                        {
                            //Store the object into a list.
                            gameObjectDistractorList.Add(temp_gameObjectHit);
                    }
                }      
            }
            return gameObjectDistractorList;
        }

        else {
            Debug.LogError("ERROR << No GameObject in the listConfirmedObjets");
            return null;
        }
    }

   
    /// <summary>
    /// Calculate ray destination points list for every selected area. Save the list in a class parameter.
    /// </summary>
    public void SetRayDestinationPoints_AllSelectedAreas()
    {
        Bounds selectedGO_bounds;
        Vector3 portion;

        //Get list of confirmed objets (areas). Copy the list to allow the list access
        List<GameObject> listConfirmedAreas = new List<GameObject>(this.GetComponent<SelectionManager>().GetListOfConfirmedObjects());        

        //Clear the list to construct new rays
        RayDestinationPoints_List.Clear();

        if (listConfirmedAreas.Count != 0)
        {
            //Limit the number of Rays (depending of the MAX_RAYS_TOTAL value and the number of selected areas. 6 is the number of semiaxes)
            int raysPerSemiAxis;
            if (RAYS_PER_AXIS <= ((MAX_RAYS_TOTAL / listConfirmedAreas.Count)-1)/6)
            {
                raysPerSemiAxis = RAYS_PER_AXIS;
            }
            else
            {
                raysPerSemiAxis = ((MAX_RAYS_TOTAL / listConfirmedAreas.Count) - 1) / 6;
            }
            
            //Update destiny points values and do the raycast for each selected area
            foreach (GameObject selectedGO in listConfirmedAreas)
            {
                selectedGO_bounds = selectedGO.GetComponent<MeshRenderer>().bounds;              //TODO: Any difference with GetComponent<Renderer>?
                portion = new Vector3(selectedGO_bounds.extents.x / raysPerSemiAxis, selectedGO_bounds.extents.y / raysPerSemiAxis, selectedGO_bounds.extents.z / raysPerSemiAxis);

                //Center ray
                RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x, selectedGO_bounds.center.y, selectedGO_bounds.center.z));
                //Semiaxis rays
                for (int i = 1; i <= raysPerSemiAxis; i++)
                {
                    RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x + portion.x * i, selectedGO_bounds.center.y, selectedGO_bounds.center.z));
                    RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x - portion.x * i, selectedGO_bounds.center.y, selectedGO_bounds.center.z));
                    RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x, selectedGO_bounds.center.y + portion.y * i, selectedGO_bounds.center.z));
                    RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x, selectedGO_bounds.center.y - portion.y * i, selectedGO_bounds.center.z));
                    RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x, selectedGO_bounds.center.y, selectedGO_bounds.center.z + portion.z * i));
                    RayDestinationPoints_List.Add(new Vector3(selectedGO_bounds.center.x, selectedGO_bounds.center.y, selectedGO_bounds.center.z - portion.z * i));
                }
            }// end foreach 
        }//end if (listConfirmedAreas != null)
    }// end SetRayDestinationPoints_AllSelectedAreas

}//END Class

