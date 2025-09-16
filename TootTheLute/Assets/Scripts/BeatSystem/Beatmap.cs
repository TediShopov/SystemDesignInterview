using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Beat
{
    public float Length;
    public bool isPause;
}

[Serializable]
public struct Measure
{
    public List<Beat> Beats;
    public int RepeatCount;

    //TODO add a button so fill the measure with beats of the same length
    //TODO add a button for the most common beat lengths - 4,2,1,0.5,0.25,0.12

}

//The object holding the beats that player should click.
//The WWise events to control the playing music source associated with the beatmap
[System.Serializable]
public class BeatmapSource
{
    public AK.Wwise.Event PlaySoundtrack;
    public AK.Wwise.Event StopSoundtrack;
    public AK.Wwise.Event PauseSoundtrack;
    public AK.Wwise.Event ResumeSoundtrack;

}

public class Beatmap : MonoBehaviour
{
    public bool Loop = true;
    public float BPM;

    public Conductor Conductor;
    public BeatmapSource BeatmapSource;

    public List<Measure> Measures;

    //Populates the measures into a singular beat container
    public List<Beat> GetFlattenedBeats()
    {
        List<Beat> list = new List<Beat>();
        foreach (var measure in Measures) 
        {
            for (int i = 0; i < measure.RepeatCount; i++) 
            {
                list.AddRange(measure.Beats);
            }

        }
        return list;
    }

    public void Setup()
    {
        this.Conductor.BPM = BPM;
        this.Conductor.BeatmapSource = this.BeatmapSource;
    }



    // Start is called before the first frame update
    void Start()
    {

    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
