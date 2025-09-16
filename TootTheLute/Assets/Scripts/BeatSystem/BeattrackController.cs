using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BeatmapTrackCommand
{
    Play, Stop, Pause, Resume,Switch
}

[RequireComponent(typeof(Collider2D))]
public class BeattrackController : MonoBehaviour
{
    public Conductor Conductor;
    public BeatmapTrackCommand Command;
    public Beatmap SwitchSource;
    private Collider2D _collider;
    //The number of command this object could invoke
    public int MaxCommandInvokes = 1;
    public int CurrentCommandInvokes = 0;
    public void Awake()
    {
        _collider = GetComponent<Collider2D>();
        
    }
    public void InvokeCommand(BeatmapTrackCommand Command)
    {
        switch (Command)
        {
            case BeatmapTrackCommand.Play:

                Conductor.Play();
                break;
            case BeatmapTrackCommand.Stop:

                Conductor.Stop();
                break;
            case BeatmapTrackCommand.Pause:

                Conductor.Pause();
                break;
            case BeatmapTrackCommand.Resume:
                Conductor.Resume();
                break;
            case BeatmapTrackCommand.Switch:
                SwitchAllBeatmapTracks();
                break;
            default:
                break;
        }

    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(CurrentCommandInvokes < MaxCommandInvokes)
        {
            InvokeCommand(this.Command);
            CurrentCommandInvokes++;

        }
    }
    public void SwitchAllBeatmapTracks()
    {
        var _tracks = FindObjectsOfType<BeatmapTrack>();
        foreach (var track in _tracks) {
            track.SwitchToBeatmap(SwitchSource);
        }

        
    }


}
