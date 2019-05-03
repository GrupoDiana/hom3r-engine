using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
//using UnityEditor;

public class LabelManager : MonoBehaviour
{
    List<CLabel> labels;
   
    // Attraction force from the target to the annotations:
    // Keep annotations close to the target.
    float targetAttractionK = 1400.0f;  // Stiffness
    float targetAttractionD = 0.0f;     // Damping (not used in impulse-based implementation)              

    // Repulsion force from the target to the annotations:
    // Avoids occlusion of target by annotations
    float targetRepulsionK = 1400.0f;   // Stiffness
    float targetRepulsionD = 0.0f;      // Damping (not used in impulse-based implementation)

    // Repulsion force from viewport limits to the annotations:
    // Keep annotations inside the viewport, even if they occlude the target.
    float viewportRepulsionK = 1400.0f; // Stiffness
    float viewportRepulsionD = 0.0f;    // Damping (not used in impulse-based implementation)

    // Repulsion force between annotations:
    // Avoid occlusion between annotations.
    float annotationsRepulsionK = 2400.0f;  // Stiffness
    float annotationsRepulsionD = 0.0f;     // Damping (not used in impulse-based implementation)

    // Force to avoid too short poles after zooming out    
    float shortPoleRepulsionK = 1400.0f;  // Stiffness
    float shortPoleRepulsionD = 0.0f;     // Damping (not used in impulse-based implementation)

    // Delayed physics update after camera approximation (zoom in/out)
    float approximationDelay = 0.1f;    // Number of seconds
    bool approximationTimerActive;
    float approximationTimer;

    // Delayed label redraw
    bool redrawTimerActive;
    float redrawTimer;

    // Camera distance used as reference for labels size
    float referenceCameraDistance;

    //Maximum number of labels
    int maximumLabels = 20;
    int movingLabelIndex;   // Manual movement of labels

    // M&D
    int autoLabelID;        // Automatic generated label ID


    private void Awake()
    {
        hom3r.quickLinks.labelsObject = GameObject.FindGameObjectWithTag("labels");
        labels = new List<CLabel>();
        approximationTimer = 0.0f;
        approximationTimerActive = false;
        redrawTimer = 0.0f;
        redrawTimerActive = false;
        referenceCameraDistance = Mathf.Sqrt(Camera.main.transform.position.y * Camera.main.transform.position.y + Camera.main.transform.position.z * Camera.main.transform.position.z);
        movingLabelIndex = -1;
        autoLabelID = -1;
    }

    
    void Update()
    {
        //// Check if we want to recompute layout after approximation
        //if (approximationTimerActive)
        //{
        //    approximationTimer += Time.deltaTime;
        //    if (approximationTimer > approximationDelay)
        //    {
        //        approximationTimerActive = false;
        //        StopApproximation();
        //    }
        //}

        //// Redraw labels after delay
        //if (redrawTimerActive)
        //{
        //    redrawTimer -= Time.deltaTime;
        //    if (redrawTimer < 0.0f)
        //    {
        //        redrawTimerActive = false;
        //        RedrawAllLabels();
        //    }
        //}
    }

    
    void FixedUpdate()
    {
        foreach (CLabel label in labels)
        {
            if ((movingLabelIndex == -1) || (label != labels[movingLabelIndex]))  // Do not compute forces for currently moving label
            {
        //        // Clear forces:
        //        label.ClearForces();

        //        // Apply forces:

        //        // Viewport repulsion force: check if the annotation is outside of the screen
        //        // This force has priority over other forces            
        //        if (!ApplyViewportRepulsion(label))
        //        {
        //            // Occlusion of annotation over other annotations
        //            ApplyAnnotationRepulsion(label);

        //            // Occlusion of annotation over target
        //            ApplyTargetRepulsion(label);

        //            // Annotation is inside target concavities
        //            ApplyConcavityEscape(label);

        //            // Keep annotations close to target, if possible
        //            ApplyTargetAttraction(label);

        //            // Ensure that poles are not too short
        //            ApplyShortPoleForce(label);
        //        }

        //        // Compute forces:
        //        label.ComputeForces();

        //        // Update pole render (governed by annotation physics):
        //        label.UpdatePoleRender();

        //        // Face annotation to camera
                label.FaceAnnotationToCamera();
            }
        }  
    }
    
    //////////////////////////////////////////////////////////////

    public void HandleUISelection(GameObject clickedGO)
    {        
        // Close button
        // NOTE: C# Lists do not allow removing inside a foreach. For this reason, I use a standard for loop
        for (int i=0; i < labels.Count; i++)
        {
            if (labels[i].GetCloseButtonGO() == clickedGO)
            {
                if (labels[i].GetLabelID()!=null)
                {                                                            
                    hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.LabelManager_LabelRemoved, labels[i].GetLabelID()));
                }
                DestroyLabelFromList(i);                
            }                
            else if (labels[i].GetAnnotationGO() == clickedGO)
            {
                ManualMoveLabel(i);
            }                
        }
    }

    //////////////////////////////////////////////////////////////

    public void HandleUIRelease()
    {
        if (movingLabelIndex != -1)        
            movingLabelIndex = -1;        
    }

    public void NewManualLabelPosition (float x, float y)
    {        
        if (movingLabelIndex != -1)
            labels[movingLabelIndex].ManualMove(x, y);
    }

    //////////////////////////////////////////////////////////////

    // Updates for each camera translation/rotation
    public void UpdateRotationTranslation()
    {        
        foreach (CLabel label in labels)
        {
            label.FaceAnnotationToCamera();         // Orientate annotations towards camera
        }
    }

    //////////////////////////////////////////////////////////////

    // Updates for each camera approximation (zoom in/out)
    public void UpdateApproximation()
    {
        // Orientate annotations towards camera
        foreach (CLabel label in labels)
        {            
            label.Resize();
            label.FaceAnnotationToCamera();
        }

        // Activate timer for delayed physics update
        approximationTimerActive = true;
        approximationTimer = 0.0f;
    }

    //////////////////////////////////////////////////////////////
    
    // Updates when no more camera rotation/translations are expected for a while
    public void StopRotationTranslation()
    {
        // Recompute forces
        foreach (CLabel label in labels)
        {
            label.ClearConstraints();
        }
    }

    //////////////////////////////////////////////////////////////

    // Updates when no more camera approximations (zoom in/out) are expected for a while
    public void StopApproximation()
    {
        // Recompute forces
        foreach (CLabel label in labels)
        {
            label.ClearConstraints();
        }
    }

        

    //////////////////////////////////////////////////////////////

    public bool LabelContains(string _labelID)
    {
        foreach (CLabel item in labels)
        {
            if (item.GetLabelID() == _labelID)
            {
                return true;
            }
        }
        return false;
    }

    public bool LabelContainsTarget(GameObject targetGO)
    {
        foreach (CLabel item in labels)
        {
            if (item.GetTargetGO() == targetGO)
            {
                return true;
            }
        }
        return false;
    }
    
    ///////////////////////////////////////
    ///////////  ADD LABEL  ///////////////
    ///////////////////////////////////////

    public void AddLabel(string _partID, string _labelID, Vector3 _position, string _text)
    {

        if ((_labelID == "") || (_labelID == null)) { _labelID = GetAutomaticLabelID(); }       // Get label ID if we dont have one        
        if (LabelContains(_labelID)) { RemoveLabel(_labelID); }                                 // Not allowed two labels with same ID

        //Check visual state of the area
        List<GameObject> objectList = new List<GameObject>();
        if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsArea(_partID))
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ObjectStateCommonManager>().IsAreaVisible(_partID))
            {
                objectList.Add(hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObject_ByAreaID(_partID));
            }
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsLeaf(_partID))
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ObjectStateCommonManager>().IsLeafVisible(_partID))
            {
                objectList.AddRange(hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByLeafID(_partID));
            }
        }
        else if (hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().IsNode(_partID))
        {
            if (hom3r.quickLinks.scriptsObject.GetComponent<ObjectStateCommonManager>().IsNodeVisible(_partID))
            {
                objectList = hom3r.quickLinks.scriptsObject.GetComponent<ModelManager>().GetAreaGameObjectList_ByProductNodeID(_partID);
            }
        }

        if (objectList.Count != 0)
        {
            bool largeText = _text.Length > Constants.largeText;
            if (_position == Vector3.zero)
            {
                AddLabelToGameObjectGroup(_labelID, objectList, _text, largeText);
            }
            else
            {
                AddLabelToSpecificPoint(_labelID, objectList[0], _position, _text, largeText);
            }
        }
        else
        {
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.LabelManager_ShowMessage, "Labels can only be attached to visible parts. The current selection is not visible"));
        }

    }


    /// <summary>Method to add a point to any point in the surface of an object (in world space)</summary>
    public void AddLabelToSurface(string labelID, GameObject _obj, string text, bool largeText = false)
    {
        //** Compute position
        // Find point and direction for casting ray
        Vector3 bbCenter = _obj.GetComponent<Collider>().bounds.center;         //BoundingBox center in world space
        Vector3 bbCenterProjection = new Vector3(bbCenter.x, 0.0f, 0.0f);
        Vector3 castDirection = bbCenter - bbCenterProjection;
        castDirection.Normalize();

        float castDistance = _obj.GetComponent<Collider>().bounds.size.y + _obj.GetComponent<Collider>().bounds.size.z;
        Vector3 castPoint = bbCenterProjection + castDirection * castDistance;
        bool objRaycasted = false; //To control if the _obj has been raycasted

        // Ray cast
        RaycastHit[] hits;
        hits = Physics.RaycastAll(castPoint, -castDirection, Mathf.Infinity);

        // Select first raycasted point in our object
        Vector3 position = _obj.transform.InverseTransformPoint(bbCenter); //default value (Transform from world to local position)
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == _obj)
            {
                //Get the obg hit point in local position
                position = hit.point;
                //Transform from world to local position, for ShowLabel
                position = _obj.transform.InverseTransformPoint(position);
                //text = "Number of flaws: 3";//"Hit Point";
                objRaycasted = true;
                break;
            }
        }

        //If _obj has not been raycasted, then use a mesh vertex position to place the label
        if (!objRaycasted)
        {
            //Get the furthest vertex
            Mesh objMesh = _obj.GetComponent<MeshFilter>().mesh;
            //Vertices positions in local space
            Vector3[] vertices = objMesh.vertices;
            // Transform from world to local position, for get the distance
            bbCenter = _obj.transform.InverseTransformPoint(bbCenter);
            //Get the Furthest position
            position = SortDistances(vertices, bbCenter);
        }
        // CREATE LABEL
        AddLabelToSpecificPoint(labelID, _obj, position, text, largeText);
    }


    /// <summary>Method to add a point to any point in the surface of an object (in world space)</summary>
    public void AddLabelToGameObjectGroup(string labelID, List<GameObject> _objects, string text = "", bool largeText = false)
    {

        //******** Compute position
        // Bounding box of every child
        Bounds goListBounds = ComputeBoundingBox(_objects);
        Vector3 bbCenter = goListBounds.center;                                 // BoundingBox center in world space
        Vector3 bbCenterProjection = new Vector3(bbCenter.x, 0.0f, 0.0f);       // BB center X axis projection
        Vector3 castDirection = bbCenter - bbCenterProjection;                  // Ray direction
        castDirection.Normalize();
        float castDistance = goListBounds.size.y + goListBounds.size.z + 500.0f;
        Vector3 castPoint = bbCenterProjection + castDirection * castDistance;  //Ray origin point

        bool objRaycasted = false; //To control if the _obj has been raycasted
        //Defuals value: first component of the list
        GameObject surfaceGO = _objects[0];
        //Vector3 closestVertex = surfaceGO.transform.InverseTransformPoint(bbCenter); 
        Vector3 closestVertex = bbCenterProjection; // Fix bug with some components

        // Ray cast
        RaycastHit[] hits;
        hits = Physics.RaycastAll(castPoint, -castDirection, Mathf.Infinity);

        // Select first raycasted point in our list of objects
        foreach (RaycastHit hit in hits)
        {
            if (_objects.Contains(hit.collider.gameObject))
            {
                surfaceGO = hit.collider.gameObject;
                //Get the obj hit and Transform from world to local position
                if (Vector3.Distance(hit.point, bbCenterProjection) > Vector3.Distance(closestVertex, bbCenterProjection))  // Fix bug with some components               
                    closestVertex = surfaceGO.transform.InverseTransformPoint(hit.point);
                objRaycasted = true;
            }
        }

        //If _obj has not been raycasted, then use a mesh vertex position to place the label
        if (!objRaycasted)
        {
            float minDistance = 0.0f; ;
            bool fistTime = true;
            float dist;
            // Go throught every GameObject of the list to: (1)Get the closest vertex to the Bounding Box center for each GameObject. (2) Get the closest vertex from the closest vertex for each GO list.   
            foreach (GameObject go in _objects)
            {
                //Get the furthest vertex
                Mesh objMesh = go.GetComponent<MeshFilter>().mesh;
                //Vertices positions in local space
                Vector3[] vertices = objMesh.vertices;
                // Transform from world to local position, for get the distance
                Vector3 bbCenter_localGOPos = go.transform.InverseTransformPoint(bbCenter);
                //Get the Furthest position
                Vector3 currentGO_LocalPosition = SortDistances(vertices, bbCenter_localGOPos); // Sort vertices by distance to  Bounding Box center X axis projection

                //Get the closest one
                dist = (currentGO_LocalPosition - bbCenter_localGOPos).sqrMagnitude;
                if (fistTime)
                {
                    minDistance = dist;
                    surfaceGO = go;
                    closestVertex = currentGO_LocalPosition;
                    fistTime = false;
                }
                else if (dist < minDistance)
                {
                    minDistance = dist;
                    surfaceGO = go;
                    closestVertex = currentGO_LocalPosition;
                }
            }
        }
        // Create label. Important to send the label position in the surface object local coordinates
        AddLabelToSpecificPoint(labelID, surfaceGO, closestVertex, text, largeText);
    }


    /// <summary> Add a Label in a specific Point </summary>
    /// <param name="obj"> gameObject where place the SinglePoint anchor</param>
    /// <param name="position"> anchor position</param>
    /// <param name="labelText"> Text to show in the label</param>
    public void AddLabelToSpecificPoint(string labelID, GameObject obj, Vector3 position, string labelText, bool largeText = false)

    {                
        this.AddLabel(labelID, hom3r.quickLinks._3DModelRoot, obj, position, labelText, largeText);       
    }

    
    /// <summary>Create a new label</summary>
    /// <param name="labelID"></param>
    /// <param name="fatherGO"></param>
    /// <param name="targetGO"></param>
    /// <param name="anchorPosition"></param>
    /// <param name="labelText"></param>
    /// <param name="largeText"></param>
    private void AddLabel(string labelID, GameObject fatherGO, GameObject targetGO, Vector3 anchorPosition, string labelText, bool largeText=false)
    {
        // Limit the number of labels
        if (labels.Count < maximumLabels)
        {            
            CLabel newLabel = new CLabel(labels.Count, labelID, fatherGO, targetGO, anchorPosition, labelText, largeText);
            newLabel.FaceAnnotationToCamera();
            newLabel.SetInitialCameraDistance(referenceCameraDistance);
            newLabel.Resize();
            labels.Add(newLabel);
            newLabel.RecomputeLabel_Ellipse(fatherGO);
        }
        else {            
            hom3r.coreLink.EmitEvent(new CCoreEvent(TCoreEvent.LabelManager_ShowMessage, "Too many labels to show"));
        }
    }           
    


    private string GetAutomaticLabelID()
    {
        autoLabelID++;
        return autoLabelID.ToString();
    }




    Vector3 SortDistances(Vector3[] positions, Vector3 origin)
    {
        float[] distances = new float[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            distances[i] = (positions[i] - origin).sqrMagnitude;
        }
        System.Array.Sort(distances, positions);
        //return positions[positions.Length-1];
        return positions[0];
    }


    

    /// <summary>Remove all labels</summary>
    public void RemoveAllLabels()
    {
        foreach (CLabel label in labels)
        {
            label.DestroyLabel();
        }
        labels.Clear();
    }
    
    /// <summary>Hide all labels</summary>
    public void HideAllLabels()
    {
        foreach (CLabel label in labels)
        {
            label.Hide();
        }
    }


    public void ChangeLabelsLayer(string newLabelLayer)
    {
        hom3r.state.labelsUILayer = newLabelLayer;
        
        //TODO Update the layer of the already created labels
    }

    //////////////////////////////////////////////////////////////

    // Redraw all labels
    public void RedrawAllLabels()
    {
        foreach (CLabel label in labels)
        {

            label.RecomputeLabel(true);    // Attached to global father
            label.Resize();
            label.Draw();

        }
    }

    //////////////////////////////////////////////////////////////

    // Timer for redrawing labels after a number of seconds
    public void RedrawLabelsAfterSeconds(float nseconds)
    {
        redrawTimerActive = true;
        redrawTimer = nseconds;
    }

    //////////////////////////////////////////////////////////////

    // Redraw all labels
    public void RedrawLabelsLocallyForGO(GameObject go)
    {
        foreach (CLabel label in labels)
        {
            if (label.GetTargetGO() == go)
            {
                label.RecomputeLabel(false);   // Attached to local target
                label.Resize();
                label.Draw();
            }
        }
    }

    //////////////////////////////////////////////////////////////

    // WARNING: Due to C# Lists limitations, this method cannot be called from a foreach loop iterating through labels List
    public void DestroyLabelFromList (int arrayIndex)
    {        
        labels[arrayIndex].DestroyLabel();
        labels.RemoveAt(arrayIndex);        
    }

    //////////////////////////////////////////////////////////////

    // WARNING: Due to C# Lists limitations, this method cannot be called from a foreach loop iterating through labels List
    public void ManualMoveLabel(int arrayIndex)
    {     
        movingLabelIndex = arrayIndex;
        labels[arrayIndex].ConstraintElongate();
        labels[arrayIndex].ConstraintStretch();
        labels[arrayIndex].ConstraintTargetAttraction();
    }

    //////////////////////////////////////////////////////////////

    public void RemoveLabel(string _labelID)
    {
        // NOTE: C# Lists do not allow removing inside a foreach. For this reason, I use a standard for loop
        for (int i = 0; i < labels.Count; i++)
        {
            if (labels[i].GetLabelID() == _labelID)
                DestroyLabelFromList(i);
        }
    }

    //////////////////////////////////////////////////////////////

    public void DestroyLabelFromTargetGO(GameObject target)
    {
        // NOTE: C# Lists do not allow removing inside a foreach. For this reason, I use a standard for loop
        for (int i = 0; i < labels.Count; i++)
        {
            if (labels[i].GetTargetGO() == target)
                DestroyLabelFromList(i);
        }
    }

    //////////////////////////////////////////////////////////////
    // PHYSICS IMPLEMENTATION
    //////////////////////////////////////////////////////////////

    // Force to avoid annotations go outside of the screen
    public bool ApplyViewportRepulsion(CLabel label)
    {
        if (label.IsAnnotationOutsideViewport())
        {            
            label.ConstraintElongate();                     // Do not allow further elongation, until camera update
            if (!label.IsTargetAttractionConstrained())     // Try to go towards target, if not constrained
            {
                label.Apply1DAttraction(viewportRepulsionK, viewportRepulsionD, 0.0f);                
            }            
            return true;
        }
        return false;
    }

    //////////////////////////////////////////

    // Force to avoid occlusion of annotations over target
    public bool ApplyTargetRepulsion(CLabel label)
    {
        Vector3 targetPoint;
        if (label.IsOccludingTarget(out targetPoint))
        {            
            float repulsionPoint = label.ProjectVectorOnPole(targetPoint);
            label.Apply1DRepulsion(targetRepulsionK, targetRepulsionD, repulsionPoint);            
            label.ConstraintTargetAttraction(); // Avoid attraction from target until camera is updated
            label.ConstraintStretch();          // Avoid stretch of pole, which will always lead to approximation to target, until camera update
            return true;
        }
        return false;
    }

    //////////////////////////////////////////////////////////////

    // Force to avoid that annotations remain inside concavities of father GO
    public bool ApplyConcavityEscape(CLabel label)
    {
        // Find far point along the pole
        GameObject targetGO = label.GetTargetGO();
        Bounds targetBounds = label.GetReferenceBounds();
        float castDistance = targetBounds.size.y + targetBounds.size.z;
        Vector3 castPoint = label.ProjectPoleOnVector(castDistance);

        // Ray cast
        RaycastHit[] hits;
        hits = Physics.RaycastAll(castPoint, -label.GetPoleDirection(), Mathf.Infinity);

        // Select first raycasted point in father
        Vector3 raycastPosition = label.GetAnchorPoint();
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == targetGO)
            {
                raycastPosition = hit.point;
                break;
            }
        }

        // Check if annotation is inside father
        if (label.ProjectVectorOnPole(raycastPosition) > label.GetAnnotationLength())
        {
            label.Apply1DRepulsion(targetRepulsionK, targetRepulsionD, 0.0f);
            label.ConstraintTargetAttraction(); // Avoid attraction from target until camera is updated
            label.ConstraintStretch();          // Avoid stretch of pole, which will always lead to approximation to target, until camera update
            return true;
        }

        return false;
    }

    //////////////////////////////////////////////////////////////

    // Force to keep poles long enough depending on zoom size
    public bool ApplyShortPoleForce(CLabel label)
    {
        if (label.IsPoleTooShort())
        {
            label.Apply1DRepulsion(shortPoleRepulsionK, shortPoleRepulsionD, label.GetAnchorLength());
            //label.ConstraintTargetAttraction(); // Avoid attraction from target until camera is updated
            //label.ConstraintStretch();          // Avoid stretch of pole, which will always lead to approximation to target, until camera update
            return true;
        }

        return false;
    }

    //////////////////////////////////////////////////////////////

    // Force to keep annotations close to target, if possible
    public bool ApplyTargetAttraction(CLabel label)
    {        
        if (!label.IsTargetAttractionConstrained())     // Check constraint
        {
            float attractionPoint = label.GetAnchorLength();
            label.Apply1DAttraction(targetAttractionK, targetAttractionD, attractionPoint); 
            return true;
        }
        return false;
    }

    //////////////////////////////////////////////////////////////
    
    // Force to avoid occlusion between annotations
    public bool ApplyAnnotationRepulsion(CLabel label)
    {        
        CLabel occludedLabel; 

        if (IsLabelOccluding(label, out occludedLabel))
        {
            // Apply repulsion on label //
            float occludedCenter = label.ProjectVectorOnPole(occludedLabel.GetAnnotationCenter());
            if (label.RepulsionWouldStretch(occludedCenter) && occludedLabel.IsElongateConstrained())
            {
                // Propagate elongate constraint and do not apply force
                //label.ConstraintElongate();   // REMOVED TO INCREASE CONVERGENCY
                return true;
            }
            if (label.RepulsionWouldElongate(occludedCenter) && (occludedLabel.IsTargetAttractionConstrained() || occludedLabel.IsStretchConstrained()))
            {
                // Propagate target attraction constraint, but still apply force
                //label.ConstraintTargetAttraction();   
                label.ConstraintStretch();                            
            }
            if (label.RepulsionWouldElongate(occludedCenter) && label.IsStretching())
            {
                // Avoid quick change from stretch to elongate, to minimize vibration
                label.SetStretching(false);
                return true;
            }
            if (label.RepulsionWouldStretch(occludedCenter) && label.IsElongating())
            {
                // Avoid quick change from elongate to stretch, to minimize vibration
                label.SetElongating(false);
                return true;
            }

            // Repulsion from the occluded label to this label
            label.Apply1DRepulsion(annotationsRepulsionK, annotationsRepulsionD, occludedCenter);


            // Apply repulsion on occluded label //
            float thisCenter = occludedLabel.ProjectVectorOnPole(label.GetAnnotationCenter());
            if (occludedLabel.RepulsionWouldStretch(thisCenter) && label.IsElongateConstrained())
            {
                // Propagate elongate constraint and do not apply force
                //occludedLabel.ConstraintElongate();   // REMOVED TO INCREASE CONVERGENCY
                return true;
            }
            if (occludedLabel.RepulsionWouldElongate(thisCenter) && (label.IsTargetAttractionConstrained() || label.IsStretchConstrained()))
            {
                // Propagate target attraction constraint, but still apply force
                //occludedLabel.ConstraintTargetAttraction();   
                occludedLabel.ConstraintStretch();                      
            }
            if (occludedLabel.RepulsionWouldElongate(thisCenter) && occludedLabel.IsStretching())
            {
                // Avoid quick change from stretch to elongate, to minimize vibration
                occludedLabel.SetStretching(false);
                return true;
            }
            if (occludedLabel.RepulsionWouldStretch(thisCenter) && occludedLabel.IsElongating())
            {
                // Avoid quick change from elongate to stretch, to minimize vibration
                occludedLabel.SetElongating(false);
                return true;
            }

            // Repulsion from this label to the occluded label
            occludedLabel.Apply1DRepulsion(annotationsRepulsionK, annotationsRepulsionD, thisCenter);
            
            return true;
        }
        
        return false;
    }

    //////////////////////////////////////////////////////////////
    
    // Tells the first label found which is being occluded by myLabel
    public bool IsLabelOccluding(CLabel myLabel, out CLabel otherLabel)
    {
        otherLabel = myLabel;        

        foreach (CLabel occludedCandidate in labels)
        {
            // Avoid self-check
            if (occludedCandidate == myLabel)
                continue;

            // Project both labels in screen space
            Rect myRect = myLabel.GetAnnotationScreenSpace();
            Rect candidateRect = occludedCandidate.GetAnnotationScreenSpace();

            // Find intersection between quad projections
            if (QuadIntersection(myRect, candidateRect))
            {
                otherLabel = occludedCandidate;
                break;
            }
        }



        return (otherLabel != myLabel);
    }
        
    //////////////////////////////////////////////////////////////
    // AUXILIARY METHODS
    //////////////////////////////////////////////////////////////

    // Returns true if two quads intersect in 2D
    public bool QuadIntersection(Rect rect1, Rect rect2)
    {
        return ((rect1.x < rect2.x + rect2.width) &&
            (rect1.x + rect1.width > rect2.x) &&
            (rect1.y < rect2.y + rect2.height) &&
            (rect1.y + rect1.height > rect2.y));
    }

    //////////////////////////////////////////////////////////////

    // Get one label object from the vector, from one of its game objects
    public CLabel GetLabelFromGO(GameObject go)
    {
        return labels[GetArrayIndex(go)];
    }

    //////////////////////////////////////////////////////////////

    // Get the ID or array index of one label in the vector, from one of its game objects
    public int GetArrayIndex(GameObject go)
    {
        string[] nameSplit = go.name.Split('_');    // "Whatever_X"
        string indexString = nameSplit[1];          // "X"
        return int.Parse(indexString);              // X
    }

    /// <summary>
    /// Calculate the BoundingBox of a list of objects. 
    /// </summary>
    /// <param name="objects"> The list of Gameobjects </param> 
    /// <returns> The axis aligned bounding box. </returns> 
    private Bounds ComputeBoundingBox(List<GameObject> objects)
    {
        Bounds totalBB = new Bounds();
        bool firstObject = true;		// Needed to avoid that Unity includes (0,0,0) center inside the BB

        // Get info from all objects 
        foreach (GameObject go in objects)
        {
            // Expand bounding box 
            if (go != null)
            {
                if (go.GetComponent<Collider>() != null)
                {
                    Bounds goBB = go.GetComponent<Collider>().bounds;

                    if (firstObject)
                    {
                        totalBB = goBB;
                        firstObject = false;
                    }
                    else
                        totalBB.Encapsulate(goBB);
                }

                // Do the same for all children objects
                List<GameObject> children = new List<GameObject>();
                for (int i = go.transform.childCount - 1; i >= 0; i--)
                {
                    GameObject newChild = go.transform.GetChild(i).gameObject;
                    if (newChild != null)
                        children.Add(newChild);
                }
                if (children.Count > 0)
                {
                    Bounds childrenBB = ComputeBoundingBox(children);
                    totalBB.Encapsulate(childrenBB);
                }
            }
        }
        return totalBB;
    }

    public void RecomputeAfterManipulation()
    {
        Debug.Log("Redrawing labels");
        for (int i = 0; i < labels.Count; i++)
        {

            labels[i].RecomputeLabel_Ellipse(hom3r.quickLinks._3DModelRoot);

        }
    }



    public GameObject InstantiateCurrentLabel(string _text)
    {
        //GameObject descriptionPanel_PrefabGO = Resources.Load("prefabs/label", GameObject) as GameObject;
        GameObject descriptionPanel_PrefabGO = (GameObject)Resources.Load("prefabs/Label3DObject", typeof(GameObject));
        GameObject label = Instantiate(descriptionPanel_PrefabGO, new Vector3(0f, 0f, 0f), new Quaternion(0f, 0f, 0f, 0f));

        TMP_Text panel3D_text = label.transform.Find("TextMeshPro").gameObject.GetComponent<TMP_Text>();
        panel3D_text.text = _text;
        label.transform.Find("Panel3D").gameObject.GetComponent<MeshRenderer>().material.color = new Color(150, 65, 123);

        return label;
    }

    public void DoAfterScaling(GameObject _label)
    {        
        StartCoroutine(DoAfterScalingCouroutine(_label, 2.0f));
    }
   

    IEnumerator DoAfterScalingCouroutine(GameObject _label, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        GameObject panel3D_canvas = _label.transform.Find("Panel3D").gameObject;
        TMP_Text panel3D_text = _label.transform.Find("TextMeshPro").gameObject.GetComponent<TMP_Text>();

        panel3D_canvas.transform.localPosition = new Vector3(panel3D_text.transform.localPosition.x, panel3D_canvas.transform.localPosition.y, panel3D_canvas.transform.localPosition.z);
    }
}