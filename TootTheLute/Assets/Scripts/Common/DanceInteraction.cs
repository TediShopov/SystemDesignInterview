using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceInteraction: BeatObjectBase
{
    public float MinScale = 1f;
    public float MaxScale = 3f;
    public float TimeShrink = 0.5f;

    public void Update()
    {
        BeatTrackerUpdate();
        
    }
    public void FixedUpdate()
    {
        //Timeshrink is in seconds
        float ratePerSecond = (MaxScale - MinScale) * (1/TimeShrink);
        float currentScale = this.transform.localScale.x;
        float scale = currentScale - ratePerSecond*Time.deltaTime;
        scale = Mathf.Clamp(scale,MinScale,MaxScale);   
        this.transform.localScale = new Vector3(scale, scale, scale);


        GetConductorFromScene();
    }
    public override void InteractOnBeat()
    {
        this.transform.localScale = new Vector3(MaxScale, MaxScale, MaxScale);
    }

}
