using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableMenuSounds : MonoBehaviour
{
    public MainMenuSounds Sounds;
    public bool OnStart = true;
    public void Start()
    {
        DisableMainMenuTrack();
    }
    public void DisableMainMenuTrack()
    {
        try
        {
            Sounds.SoundtrackStop.Post(MainMenuAudioPlayer.Instance.gameObject);
            MainMenuAudioPlayer.Instance.IsMainMenuPlaying = false;
        }
        catch (System.Exception)
        {

        }
    }
}
