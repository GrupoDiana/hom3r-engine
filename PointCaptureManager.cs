using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TPointCaptureManagerState { iddle, capturing, editing };

public class PointCaptureManager : MonoBehaviour
{
    TPointCaptureManagerState pointCaptureManagerState;

    void Awake()
    {
        pointCaptureManagerState = TPointCaptureManagerState.iddle;
    }



    /// <summary>
    /// Start point capture
    /// </summary>
    public void StartPointCapture()
    {
        pointCaptureManagerState = TPointCaptureManagerState.capturing;
    }

    

}
