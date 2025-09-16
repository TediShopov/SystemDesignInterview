using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AIConductor : MonoBehaviour
{
    public BeatmapTrack Track;
    public BeatObject GetUpcomingBeat()
    {
        if (Track == null)
            Debug.LogError("No BeatMap Track Assigned To AI Conductor");
        if (this.Track.BeatObjects == null || this.Track.BeatObjects.Count == 0)
            return null;

        return this.Track.BeatObjects[this.Track.ApproachingBeatIndex];

    }
//    public void ScheduleForNextBeat()
//    {
//
//    }





}
