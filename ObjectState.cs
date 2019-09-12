using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>The 'State' abstract class </summary>
abstract class State
{
    public abstract void StateIn(VisualStateContext context);
    public abstract void StateIn(VisualStateContext context, float duration);
    public abstract void StateOut(VisualStateContext context);
    public abstract void StateOut(VisualStateContext context, float duration);
}


////////////////////////////////////////////////////////////////////////
/////////////      AREA VISUAL STATE MACHINE   /////////////////////////
////////////////////////////////////////////////////////////////////////
/// <summary> Idle State class</summary>
class Idle_State : State
{
    public override void StateIn(VisualStateContext context)
    {
        StateIn(context, 0.0f);
    }
    public override void StateIn(VisualStateContext context, float duration)
    {
        context.parent.SetMaterialInitialConditions(duration);      //Init material                
    }

    public override void StateOut(VisualStateContext context) { }
    public override void StateOut(VisualStateContext context, float duration) { }
}
/// <summary> Indicated State class </summary>
class Indicated_State : State
{
    public override void StateIn(VisualStateContext context)
    {                                
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaIndicateOn, context.parent.gameObject));             //Emit and event: I'm indicated
        ObjectStateMaterialUtils.SetColourToMaterial(context.parent.GetComponent<Renderer>().material, context.parent.indicatedColour);    //Change color of the material    
    }
    public override void StateIn(VisualStateContext context, float duration) { }
    public override void StateOut(VisualStateContext context)
    {        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaIndicateOff, context.parent.gameObject));    //Emit and event: I'm not indicated anymore
    }
    public override void StateOut(VisualStateContext context, float duration) { }
}
/// <summary> Confirmed State class</summary>
class Confirmed_State : State
{
    public override void StateIn(VisualStateContext context)
    {
        
        //Add to the list of confirmed objects
        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmGameObjectON(context.parent.gameObject);
        //Change color of the material             
        //context.parent.gameObject.GetComponent<Renderer>().material.color = context.parent.confirmedColour;
        ObjectStateMaterialUtils.SetColourToMaterial(context.parent.GetComponent<Renderer>().material, context.parent.confirmedColour);
        //Add label to the area if proceed
        hom3r.quickLinks.scriptsObject.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaConfirmationOn, context.parent.gameObject));
    }
    public override void StateIn(VisualStateContext context, float duration) { }
    public override void StateOut(VisualStateContext context)
    {
        //Delete from the list of confirmed GameObjects
        hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmSingleGameObjectOFF(context.parent.gameObject);
        hom3r.quickLinks.scriptsObject.GetComponent<Core>().EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaConfirmationOff, context.parent.gameObject));
    }
    public override void StateOut(VisualStateContext context, float duration) { }
}
/// <summary> TransparentIdle State class</summary>
class TransparentIdle_State : State
{
    public override void StateIn(VisualStateContext context)
    {        
        StateIn(context, 0.0f);
    }
    public override void StateIn(VisualStateContext context, float duration)
    {
        context.parent.SetMaterialInitialColor();      // Set Initial color. testing 
        context.parent.ProcessFadeOutEffect(0.0f, duration, ObjectStateMaterialUtils.TMaterialState.Transparent);                      //Make the material transparent                 
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaTransparencyOn, context.parent.gameObject));     //Emit and event telling we are going to be transparent
    }
    public override void StateOut(VisualStateContext context)
    {        
        StateOut(context, 0.0f);
    }
    public override void StateOut(VisualStateContext context, float duration)
    {        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaTransparencyOff, context.parent.gameObject));     //Emit and event telling we are not going to be transparent anymore
        context.parent.ProcessFadeInEffect(0, duration, ObjectStateMaterialUtils.TMaterialState.Visible);                      //Make the material visible
    }
}
/// <summary> TransparentIndicated State class</summary>
class TransparentIndicated_State : State
{
    public override void StateIn(VisualStateContext context)
    {
        context.parent.ProcessFadeOutEffect(0.0f, 0.0f, ObjectStateMaterialUtils.TMaterialState.Transparent);                          //Make the material transparent                 
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaTransparencyOn, context.parent.gameObject));     //Emit and event telling we are going to be transparent                
        
        //Get new the color of the material   
        Color temp = context.parent.indicatedColour;                 
        //temp.a = UIMaterialTransparency.GetMaterialAlphaLevel();
        temp.a = hom3r.state.smartTransparencyAlphaLevel;
        ObjectStateMaterialUtils.SetColourToMaterial(context.parent.GetComponent<Renderer>().material, temp);  //Change color of the material                

        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaIndicateOn, context.parent.gameObject));             //Emit and event: I'm indicated        
    }
    public override void StateIn(VisualStateContext context, float duration) { }
    public override void StateOut(VisualStateContext context)
    {        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaTransparencyOff, context.parent.gameObject));        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaIndicateOff, context.parent.gameObject));    //Emit and event: I'm not indicated anymore
    }
    public override void StateOut(VisualStateContext context, float duration) { }
}

/// <summary> HiddenCollider State class</summary>
class HiddenColliderOn_State : State
{
    public override void StateIn(VisualStateContext context)
    {
        //Add to the list of hidden objects        
        //hom3r.quickLinks.scriptsObject.GetComponent<Hidden_Script>().GameObjectHiddenOn(context.parent.gameObject);
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaHiddenOn, context.parent.gameObject));
        //We Activate the mesh collider, we can select
        context.parent.GetComponent<MeshCollider>().enabled = true;
    }
    public override void StateIn(VisualStateContext context, float duration) { }
    public override void StateOut(VisualStateContext context)
    {
        //Delete from the list of hidden GameObjects
        //hom3r.quickLinks.scriptsObject.GetComponent<Hidden_Script>().GameObjectHiddenOff(context.parent.gameObject);
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaHiddenOff, context.parent.gameObject));
        //We deactivate the mesh collider, we cannot select it any more
        context.parent.GetComponent<MeshCollider>().enabled = false;
    }
    public override void StateOut(VisualStateContext context, float duration) { }
}
/// <summary> HiddenCollider State class</summary>
class RemoveIdle_State : State
{
    public override void StateIn(VisualStateContext context)
    {
        StateIn(context, 0.0f);
    }
    public override void StateIn(VisualStateContext context, float duration)
    {
        context.parent.ProcessFadeOutEffect(0.0f, duration, ObjectStateMaterialUtils.TMaterialState.Hide);                     //Make the material Hide                    
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaRemoveOn, context.parent.gameObject));   //Add to the list of hidden objects             
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaRemoved, context.parent.gameObject));              //Remove label and single point group
    }
    public override void StateOut(VisualStateContext context)
    {
        StateOut(context, 0.0f);
    }
    public override void StateOut(VisualStateContext context, float duration)
    {
        context.parent.ProcessFadeInEffect(0, duration, ObjectStateMaterialUtils.TMaterialState.Visible);        
        hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.ObjectState_AreaRemoveOff, context.parent.gameObject));  //Delete from the list of confirmed GameObjects        
    }
}

/// <summary> The 'Context' class </summary>
class VisualStateContext
{
    public State state { get; set; }
    public ObjectStateManager parent { get; set; }

    
    public VisualStateContext(ObjectStateManager _parent, State _state)
    {
        this.state = _state;
        this.parent = _parent;
        this.state.StateIn(this);
    }
    
    public void ChangeState_toIdle()
    {
        if (this.state.GetType().Name == "Idle_State" || this.state.GetType().Name == "Indicated_State" || this.state.GetType().Name == "Confirmed_State" || this.state.GetType().Name == "TransparentIdle_State" || this.state.GetType().Name == "RemoveIdle_State")
        {
            //Finish current state
            state.StateOut(this);
            //Start new state
            state = new Idle_State();
            state.StateIn(this);
        }
    }

    public void ChangeState_toIdle(float duration)
    {
        if (this.state.GetType().Name == "TransparentIdle_State" || this.state.GetType().Name == "HiddenIdle_State" || this.state.GetType().Name == "RemoveIdle_State")
        {
            //Finish current state
            state.StateOut(this, duration);
            //Start new state
            state = new Idle_State();
            state.StateIn(this, duration);
        }
    }

    public void ChangeState_toIndicate(bool multiple)
    {
        if (this.state.GetType().Name == "Idle_State")
        {
            //Finish current state
            state.StateOut(this);
            if (!multiple)
            {
                hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().IndicationGameObjectAllOFF();
            }
            //Start new state       
            state = new Indicated_State();
            state.StateIn(this);
        }
    }

    public void ChangeState_toTransparentIdle()
    {
        if (this.state.GetType().Name == "TransparentIndicated_State")
        {
            //Finish current state
            state.StateOut(this);
            //Start new state
            state = new TransparentIdle_State();
            state.StateIn(this);
        }
    }

    public void ChangeState_toTransparentIndicated()
    {
        if (this.state.GetType().Name == "TransparentIdle_State")
        {
            //Finish current state
            state.StateOut(this);
            //Start new state       
            state = new TransparentIndicated_State();
            state.StateIn(this);
        }
    }

    public void ChangeState_toConfirmed(bool multiple)
    {
        //Finish current state
        state.StateOut(this);
        if (!multiple)
        {
            //2. Des-confirm all previous objects
            hom3r.quickLinks.scriptsObject.GetComponent<SelectionManager>().ConfirmALLGameObjectOFF();
        }
        //Change to the new state
        state = new Confirmed_State();
        state.StateIn(this);
    }

    public void ChangeState_toTransparent(float duration)
    {
        if (this.state.GetType().Name == "Idle_State")
        {
            //Finish current state
            state.StateOut(this);
            //Change to the new state
            state = new TransparentIdle_State();
            state.StateIn(this, duration);
        }
        else if (this.state.GetType().Name == "HiddenIdle_State")
        {
            //Finish current state
            state.StateOut(this, duration);
            //Change to the new state
            state = new TransparentIdle_State();
            state.StateIn(this, duration);
        }
    }

    public void ChangeState_toColliderOn()
    {
        // Finish current state
        state.StateOut(this);
        //Start new state       
        state = new HiddenColliderOn_State();
        state.StateIn(this);
    }

    public void ChangeState_toRemove(float duration)
    {
        if (this.state.GetType().Name == "Idle_State" || this.state.GetType().Name == "Confirmed_State" || this.state.GetType().Name == "TransparentIdle_State")
        {
            //Finish current state
            state.StateOut(this);
            //Change to the new state 
            state = new RemoveIdle_State();
            state.StateIn(this, duration);
        }
    }
}

/// <summary> Enum to return the current visual state of the area </summary>
public enum TObjectVisualStates { Idle, Indicated, Confirmed, Transparent_Idle, Transparent_Indicated, Hidden_Idle, Hidden_Collider_On, Remove_Idle };
/// <summary> Enum to get events and change the state. </summary>
public enum TObjectVisualStateEvents { Indication_On, Indication_Off, Indication_Multiple_On, Confirmation_On, Confirmation_Off, Confirmation_Multiple_On, Transparency_On, Transparency_Off, Hidden_On, Hidden_Off, Collider_On, Collider_Off, Remove_On, Remove_Off };




//////////////////////////////////////////////////////////////////////////
////////////      AREA EXPLOSION STATE MACHINE   /////////////////////////
//////////////////////////////////////////////////////////////////////////

public enum ObjectExplosionState_Type { No_Explode, Explode };
public enum ObjectExplosionStateEvents_Type { Explode, Implode };


