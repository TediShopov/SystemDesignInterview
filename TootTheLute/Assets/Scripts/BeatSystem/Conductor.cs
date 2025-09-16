using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Code adapted from https://fizzd.me/posts/how-to-make-a-rhythm-game-a-quick-and-dirty-guide-to-setting-up-your-project
public class Conductor : MonoBehaviour
{

    public float CrochetsPerBar;
    public float BPM; //Beats Per Minute
    public float Crochet => 60/BPM; //60/bpm.  Also known as a quarter note in a 4/4 measure
    public float SongPosition;
    public float Offset;
    public BeatmapSource BeatmapSource;
    public uint soundID;

    private float dspTimeSongStart = 0;
    private float oldSongPos = 0;
    public float TimeSongStart => dspTimeSongStart;

    //Epsilon for comparing song timestamps
    private float _EPS = 0.05f; 

    [SerializeField, Tooltip("Unused with WWise; Kept for legacy")]
    public AudioSource Soundtrack;
    public void Play()
    {
        // Cast the flag to uint and pass it as part of the PostEvent call.
        uint flags = (uint)AkCallbackType.AK_EnableGetSourcePlayPosition;
        AK.Wwise.CallbackFlags callbackFlags = new AK.Wwise.CallbackFlags();
        callbackFlags.value = flags;
        //soundID = AkSoundEngine.PostEvent(PlaySoundtrack.Id, this.gameObject, (uint)AkCallbackType.AK_EnableGetSourcePlayPosition,null,null);
        soundID = AkSoundEngine.PostEvent(this.BeatmapSource.PlaySoundtrack.Id, this.gameObject, flags, EventCallback, null);


        dspTimeSongStart = (float)AudioSettings.dspTime;
    }
    public void Resume()
    {
        uint flags = (uint)AkCallbackType.AK_EnableGetSourcePlayPosition;
        AK.Wwise.CallbackFlags callbackFlags = new AK.Wwise.CallbackFlags();
        callbackFlags.value = flags;
        
        //soundID = AkSoundEngine.PostEvent(ResumeSoundtrack.Id, this.gameObject, flags, EventCallback, null);
         AkSoundEngine.PostEvent(this.BeatmapSource.ResumeSoundtrack.Id, this.gameObject, flags, EventCallback, null);
    }
    public void Pause()
    {
        this.BeatmapSource.PauseSoundtrack.Post(this.gameObject);
    }

    private void EventCallback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {
        Debug.Log("Callback received: " + in_type);
    }

    
    // Update is called once per frame
    void Update()
    {

        //SongPosition = (float)(AudioSettings.dspTime - dspTimeSongStart) * Soundtrack.pitch - offset;
        int elapseMS;
        AKRESULT res = AkSoundEngine.GetSourcePlayPosition(soundID, out elapseMS);

        if(res == AKRESULT.AK_Success)
        {
            //Debug.Log($"Elapsed MS: {elapseMS}");
            float songPositionToAssign = oldSongPos + ((float)elapseMS / 1000.0f);

            //Compare with the song position from the previous frame
            if(songPositionToAssign - SongPosition < -  _EPS)
            {
                //Song has most likely looped
                oldSongPos = SongPosition;

            }
            songPositionToAssign = oldSongPos + ((float)elapseMS / 1000.0f);
            SongPosition = songPositionToAssign;

        }

    }
    public void Stop()
    {
        if(this.BeatmapSource.StopSoundtrack != null) 
        {
            this.BeatmapSource.StopSoundtrack.Post(this.gameObject );
        }
    }
    private void OnDestroy()
    {
        Stop();
    }



}
