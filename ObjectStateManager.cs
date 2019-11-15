using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ObjectStateManager : MonoBehaviour {

    VisualStateContext objectVisualState;               //To store the object visual state    
    ObjectExplosionState_Type objectExplosionState;     //To store the object explosion state 

    Queue<CObjectVisualStateCommand> commandsQueue;
    CObjectVisualStateCommand commandRunning;
    bool doCommandCoroutineStarted;
    bool commandExecuting;
    bool abortCommandExecuting;
    //public List<string> areaID { get; set; }      //To store model information, area_id    
    public string areaID;// { get; set; }              //To store model information, area_id        

    Color32 confirmedDefaultColor;                  //To store the default colour for confirmation;
    Color32 indicatedDefaultColor;                  //To store the default colour for indication;

    Color32 initialColor;                            //To store the object default colour
    public Color32 confirmedColour;                 //To store the current colour for confirmation
    public Color32 indicatedColour;                 //To store the current colour for indication

    private void Awake()
    {
        confirmedDefaultColor = ObjectStateMaterialUtils.HexToColor("#1F5AE4FF");          //Default colour to confirmation
        indicatedDefaultColor = ObjectStateMaterialUtils.HexToColor("#9DB1E1FF");          //Default colour to indication
        ResetColours();                                                         //Reset colours
        initialColor = ObjectStateMaterialUtils.GetColourOfMaterial(this.GetComponent<Renderer>().material);
        objectVisualState = new VisualStateContext(this, new Idle_State()); //Initialize the object visual state
        objectExplosionState = ObjectExplosionState_Type.No_Explode;        //Initialize the object explosion state
        
        areaID = null;

        commandsQueue = new Queue<CObjectVisualStateCommand>();
        commandRunning = null;
        doCommandCoroutineStarted = false;
        commandExecuting = false;
        abortCommandExecuting = false;
    }

    
    /// <summary>Get the state of the area</summary>
    /// <returns>Area state</returns>
    public TObjectVisualStates GetVisualState()
    {
        TObjectVisualStates currentState = TObjectVisualStates.Idle;
        string currentState_str = objectVisualState.state.GetType().Name;
        
        switch (currentState_str)
        {
            case "Idle_State":
                currentState= TObjectVisualStates.Idle;                
                break;
            case "Indicated_State":
                currentState= TObjectVisualStates.Indicated;                
                break;
            case "Confirmed_State":
                currentState= TObjectVisualStates.Confirmed;
                break;
            case "TransparentIdle_State":
                currentState = TObjectVisualStates.Transparent_Idle;
                break;
            case "TransparentIndicated_State":
                currentState = TObjectVisualStates.Transparent_Indicated;
                break;
            case "HiddenIdle_State":
                currentState = TObjectVisualStates.Hidden_Idle;
                break;
            case "HiddenColliderOn_State":
                currentState = TObjectVisualStates.Hidden_Collider_On;
                break;
            case "RemoveIdle_State":
                currentState = TObjectVisualStates.Remove_Idle;
                break;
        }
        return currentState;
    }//End GetState

    public ObjectExplosionState_Type GetExplosionState()
    {
        return objectExplosionState;
    }


    /// <summary>
    /// Excute Object actions
    /// </summary>
    /// <param name="_command"></param>
    public void Do(CObjectVisualStateCommand _command)
    {
        //Enqueue 3D file and start download
        commandsQueue.Enqueue(_command);        
        this.Do_Next();

        //if (!doCommandCoroutineStarted)
        //{
        //    doCommandCoroutineStarted = true;
        //    StartCoroutine(Coroutine_ExecuteCommandQueue());
        //}
        //else
        //{
        //    commandRunning.AbortCommand(this);
        //}
    }

    /// <summary>Load 3D file one by one</summary>
    //IEnumerator Coroutine_ExecuteCommandQueue()
    //{
    //    //CObjectVisualStateCommand _command;
    //    while (commandsQueue.Count > 0)
    //    {            
    //        commandRunning = commandsQueue.Dequeue();
    //        //Debug.Log("AreaID: " + this.areaID + " command: " + commandRunning.data.commandEvent);
    //        commandRunning.Do(this);  // Execute the command
    //        this.commandExecuting = true;
    //        //We exceute the commands one by one, so we wait until the current one have been finished
    //        while (commandExecuting) { yield return new WaitForSeconds(0.2f); }
    //    }
    //    doCommandCoroutineStarted = false;
    //}

    private void Do_Next()
    {
        if (commandExecuting) {
            commandRunning.AbortCommand(this);
            //Debug.Log("AreaID: " + this.areaID + " command aborting: " + commandRunning.data.commandEvent);
            return;
        }        
        if (commandsQueue.Count > 0)
        {
            commandRunning = commandsQueue.Dequeue();
            commandRunning.Do(this);  // Execute the command    
            //Debug.Log("AreaID: " + this.areaID + " command: " + commandRunning.data.commandEvent);
        }
    }


    //////////////
    // INDICATE
    //////////////
    public void Goto_Indication_State(bool _multiple)
    {       
        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Idle)
        {
            objectVisualState.ChangeState_toIndicate(_multiple);
        }
        else if (currentState == TObjectVisualStates.Transparent_Idle)
        {
            objectVisualState.ChangeState_toTransparentIndicated();
        }
    }    
    public void Quit_Indication_State()
    {
        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Indicated)
        {
            objectVisualState.ChangeState_toIdle();
        }
        else if (currentState == TObjectVisualStates.Transparent_Indicated)
        {
            objectVisualState.ChangeState_toTransparentIdle();
        }
    }

    //////////////////
    // CONFIRMATION
    ///////////////////
    public void Goto_Confirmation_State(bool _multiple, float _duration, string _colour = "")
    {
        this.SetColour(_colour);                                    //Colour management

        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Idle || currentState == TObjectVisualStates.Indicated || currentState == TObjectVisualStates.Transparent_Idle || currentState == TObjectVisualStates.Transparent_Indicated || currentState == TObjectVisualStates.Hidden_Idle || currentState == TObjectVisualStates.Remove_Idle)
        {
            objectVisualState.ChangeState_toConfirmed(_multiple, _duration);
        }
    }
    public void Quit_Confirmation_State()
    {
        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Confirmed) { objectVisualState.ChangeState_toIdle(); }
    }

    //////////////////
    // REMOVE
    ///////////////////
    public void Goto_Remove_State(float _duration)
    {     
        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Idle || currentState == TObjectVisualStates.Indicated || currentState == TObjectVisualStates.Confirmed || currentState == TObjectVisualStates.Transparent_Idle)
        {     
            objectVisualState.ChangeState_toRemove(_duration);
        }
    }
    public void Quit_Remove_State(float _duration)
    {
        TObjectVisualStates currentState = this.GetVisualState();        
        if (currentState == TObjectVisualStates.Remove_Idle)
        {
            objectVisualState.ChangeState_toIdle(_duration);
        }
    }

    //////////////////
    // TRANSPARENCY
    ///////////////////
    public void Goto_Transparency_State(float _duration)
    {
        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Idle || currentState == TObjectVisualStates.Hidden_Idle)
        {
            objectVisualState.ChangeState_toTransparent(_duration);
        }
    }
    public void Quit_Transparency_State(float _duration)
    {
        TObjectVisualStates currentState = this.GetVisualState();
        if (currentState == TObjectVisualStates.Transparent_Idle)
        {            
            objectVisualState.ChangeState_toIdle(_duration);
        }
    }

    //////////////////
    // EXPLODE
    ///////////////////
    public void Goto_Explosion_State()
    {
        if (objectExplosionState == ObjectExplosionState_Type.No_Explode)
        {
            //Update explosion state
            objectExplosionState = ObjectExplosionState_Type.Explode;
        }
    }
    public void Quit_Explosion_State()
    {
        if (objectExplosionState == ObjectExplosionState_Type.Explode)
        {
            //Update explosion state
            objectExplosionState = ObjectExplosionState_Type.No_Explode;
        }
    }

    public void AbortCommandExecution()
    {
        this.abortCommandExecuting = true;
    }


    /// <summary>    
    /// 
    /// </summary>
    /// <param name="colour"></param>
    /////////////////////
    // Colour Methods
    /////////////////////    
    private void SetColour(string colour = "")
    {
        //Colour management
        if (colour != "")
        {
            if (confirmedColour != ObjectStateMaterialUtils.HexToColor(colour))
            {
                confirmedColour = ObjectStateMaterialUtils.HexToColor(colour);
                indicatedColour = ObjectStateMaterialUtils.IndicatedColourCalculate(confirmedColour);
            }
        }
    }

    /// <summary>Reset colous to default ones</summary>
    public void ResetColours()
    {
        confirmedColour = confirmedDefaultColor;
        indicatedColour = indicatedDefaultColor;
    }

    /////////////////////////////////////
    // Reset to Initial Material State //
    /////////////////////////////////////

    public void SetMaterialInitialColor() {
        ObjectStateMaterialUtils.SetColourToMaterial(this.GetComponent<Renderer>().material, initialColor);
    }

    /// <summary>Set the original colour and reder mode to the object material</summary>
    public void SetMaterialInitialConditions(float delayTime)
    {
        StartCoroutine(CoroutineSetMaterialInitialConditions(delayTime));       
    }

    IEnumerator CoroutineSetMaterialInitialConditions(float delayTime)
    {
        //Start delay
        if (delayTime != 0) { yield return new WaitForSeconds(delayTime); }

        //Change Material
        ObjectStateMaterialUtils.SetMaterialRenderingMode(this.GetComponent<Renderer>().material, ObjectStateMaterialUtils.TBlendMode.Opaque);
        ObjectStateMaterialUtils.SetColourToMaterial(this.GetComponent<Renderer>().material, initialColor);
    }

    /////////////////////////////////////
    // Change to confirmed State //
    /////////////////////////////////////

    public void SetConfirmedState(float delayTime)
    {
        StartCoroutine(CoroutineSetConfirmedState(delayTime));
    }
    IEnumerator CoroutineSetConfirmedState(float delayTime)
    {
        //Start delay
        if (delayTime != 0) { yield return new WaitForSeconds(delayTime); }

        //Add to the list of confirmed objects
        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmGameObjectON(this.gameObject);
        //Change color of the material             
        //context.parent.gameObject.GetComponent<Renderer>().material.color = context.parent.confirmedColour;
        ObjectStateMaterialUtils.SetColourToMaterial(this.GetComponent<Renderer>().material, this.confirmedColour);
        //Add label to the area if proceed
        hom3r.quickLinks.scriptsObject.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaConfirmationOn, this.gameObject));
    }


    /////////////////////////
    // Fade effect Methods //
    /////////////////////////
    public void ProcessFadeInEffect(float delayTime, float durationTime, ObjectStateMaterialUtils.TMaterialState newState)
    {
        float targetAlpha = 1.0f;
        if (newState == ObjectStateMaterialUtils.TMaterialState.Visible) {            
            this.GetComponent<MeshCollider>().enabled = true;         //We Activate the mesh collider, we want to select it
            targetAlpha = 1.0f;
        }
        StartCoroutine(CoroutineFadeInEffect(0, durationTime, targetAlpha));
    }

    IEnumerator CoroutineFadeInEffect(float delayTime, float durationTime, float targetAlpha)
    {
        this.commandExecuting = true;
        //Start the FadeIn
        if (durationTime != 0) { yield return StartCoroutine(FadeAlpha(delayTime, durationTime, targetAlpha)); }            //Make the fade In effect       
        else { ObjectStateMaterialUtils.SetAlphaColorToMaterial(this.GetComponent<Renderer>().material, 1.0f); }
        ObjectStateMaterialUtils.SetMaterialRenderingMode(this.GetComponent<Renderer>().material, ObjectStateMaterialUtils.TBlendMode.Opaque);    //Change Rendering mode to opaque                       
        this.commandExecuting = false;
        this.Do_Next();
    }

    public void ProcessFadeOutEffect(float delayTime, float durationTime, ObjectStateMaterialUtils.TMaterialState newState)
    {
        float targetAlpha = 0.0f;

        if (newState != ObjectStateMaterialUtils.TMaterialState.Visible) {

            ObjectStateMaterialUtils.SetMaterialRenderingMode(this.GetComponent<Renderer>().material, ObjectStateMaterialUtils.TBlendMode.Fade);    //Change Rendering mode to fade            

            if      (newState == ObjectStateMaterialUtils.TMaterialState.Hide)         { targetAlpha = 0.0f; }
            else if (newState == ObjectStateMaterialUtils.TMaterialState.Transparent)  { targetAlpha = hom3r.state.smartTransparencyAlphaLevel; }

            StartCoroutine(CoroutineFadeOutEffect(0.0f, durationTime, targetAlpha));        //Start Fade-Out effect
        }           
    }    

    IEnumerator CoroutineFadeOutEffect(float delayTime, float durationTime, float targetAlpha)
    {
        this.commandExecuting = true;
        if (durationTime != 0) { yield return StartCoroutine(FadeAlpha(delayTime, durationTime, targetAlpha)); }
        else { ObjectStateMaterialUtils.SetAlphaColorToMaterial(this.GetComponent<Renderer>().material, targetAlpha); }


        //We change the material to the invisible one	
        if (targetAlpha == 0.0f)
        {
            this.GetComponent<MeshCollider>().enabled = false;  //We deactivate the mesh collider, we cant select any more
        }
        this.commandExecuting = false;
        this.Do_Next();
    }
    
    /// <summary>Function that fades alpha for a game object up or down with a start delay and duration of fade</summary>
    /// <param name="delayTime">Waiting time before start the effect</param>
    /// <param name="durationTime">Duration time of the fade effect</param>
    /// <param name="targetAlpha">The effect start in the current alpha value and will finish in the target value</param>
    /// <returns></returns>
    IEnumerator FadeAlpha(float delayTime, float durationTime, float targetAlpha)
    {

        //Variable to store alpha and colour
        float alpha;
        Color currentColor;
        float currentAlpha;

        //Variable to store the interpolation parameter
        float t;
        t = 0.0f;

        //Initilize varibles with the actual colour and alpha of the materia.
        currentColor = this.GetComponent<Renderer>().material.GetColor("_Color");
        currentAlpha = currentColor.a;

        //Start delay
        yield return new WaitForSeconds(delayTime);

        //Make de Fade.
        //Lerp interpolate the value of alpha between current and target en funciton of t.
        //t=0 --> curret, t=1 --> target
        while (t <= 1)
        {            
            if (abortCommandExecuting) {
                abortCommandExecuting = false;
                t = 1;
                // Debug.Log("Aborting: " + this.areaID);             
            }
            alpha = Mathf.Lerp(currentAlpha, targetAlpha, t);   // Calculate the new alpha value in function of deltaTime            
            t += Time.deltaTime / durationTime;                 // Calculare new t
            
            
            //Assing the alpha value to the object material 
            currentColor.a = alpha;
            //if (this.gameObject.activeSelf)
            {
                this.GetComponent<Renderer>().material.SetColor("_Color", currentColor);
            }
            //Wait until the next frame to continue
            yield return true;
        }
        //Change the standard material. If you don't do that the slider doesn't run properly.
        //ChangeMaterial();        
    }    
}



