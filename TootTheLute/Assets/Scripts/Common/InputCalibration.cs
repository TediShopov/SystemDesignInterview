using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class InputCalibration : MonoBehaviour
{
    private static readonly string VAOffsetKey = "VAOffset";
    private static readonly string InputOffsetKey = "InputOffset";


    public static float VAOffset => PlayerPrefs.GetFloat(VAOffsetKey);
    public static float InputOffset => PlayerPrefs.GetFloat(InputOffsetKey);

    //public BeattrackInputCalibrationSettings Data;

    public TMPro.TMP_Text ActualOffsetLabel;
    public TMPro.TMP_Text PredictedInputOffsetLabel;
    public TMPro.TMP_Text ActualInputOffsetLabel;
    public BeatmapTrack BeatmapTrack;
    // Start is called before the first frame update
    void Start()
    {
        
        //TODO remove in actual buidl
        //PlayerPrefs.SetFloat(VAOffsetKey, 0);
        //PlayerPrefs.SetFloat(InputOffsetKey, 0);
        //If no record of previous calibration exists create the key/data structure
        if(PlayerPrefs.HasKey(VAOffsetKey) == false)
        {
            PlayerPrefs.SetFloat(VAOffsetKey, 0);
        }

        if(PlayerPrefs.HasKey(InputOffsetKey) == false)
        {
            PlayerPrefs.SetFloat(InputOffsetKey, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ActualOffsetLabel.text = PlayerPrefs.GetFloat(VAOffsetKey).ToString("F3");
        ActualInputOffsetLabel.text = PlayerPrefs.GetFloat(InputOffsetKey).ToString("F3");
        PredictedInputOffsetLabel.text = BeatmapTrack.AvgDeviationFromNearestBeat.ToString("F3");
        
//        if(BeatmapTrack.PlayerInputsToCollect >= BeatmapTrack.PlayerDeviations)
//        {
//            PlayerPrefs.SetFloat()
//
//        }
        
    }

    public void SetInputOffset()
    {
        PlayerPrefs.SetFloat(InputOffsetKey,InputOffset + BeatmapTrack.AvgDeviationFromNearestBeat);
        ResetDeviations();
        
    }
    public void FullReset()
    {
        PlayerPrefs.SetFloat(InputOffsetKey,0);
    }
    public void ResetDeviations()
    {
        BeatmapTrack.AvgDeviationFromNearestBeat = 0;
        BeatmapTrack.PlayerDeviations.Clear();
        BeatmapTrack.PlayerClickedTrackTimes.Clear();
        foreach (var item in BeatmapTrack.RecordedOnTrack)
        {
            Destroy(item);
        }
        BeatmapTrack.RecordedOnTrack.Clear();

    }



    #region Offset Calibration Events
    public void VAOffsetPositiveBy10ms()
    {
        if(PlayerPrefs.HasKey(VAOffsetKey))
        {
            PlayerPrefs.SetFloat(VAOffsetKey,PlayerPrefs.GetFloat(VAOffsetKey) + 0.1f);
        }
    }
    public void VAOffsetPositiveBy5ms()
    {
        if(PlayerPrefs.HasKey(VAOffsetKey))
        {
            PlayerPrefs.SetFloat(VAOffsetKey,PlayerPrefs.GetFloat(VAOffsetKey) + 0.05f);
        }
    }
    public void VAOffsetPositiveBy1ms()
    {
        if(PlayerPrefs.HasKey(VAOffsetKey))
        {
            PlayerPrefs.SetFloat(VAOffsetKey,PlayerPrefs.GetFloat(VAOffsetKey) + 0.01f);
        }
    }
    public void VAOffsetNegativeBy10ms()
    {
        if(PlayerPrefs.HasKey(VAOffsetKey))
        {
            PlayerPrefs.SetFloat(VAOffsetKey,PlayerPrefs.GetFloat(VAOffsetKey) - 0.1f);
        }

    }
    public void VAOffsetNegativeBy5ms()
    {
        if(PlayerPrefs.HasKey(VAOffsetKey))
        {
            PlayerPrefs.SetFloat(VAOffsetKey,PlayerPrefs.GetFloat(VAOffsetKey) - 0.05f);
        }

    }
    public void VAOffsetNegativeBy1ms()
    {
        if(PlayerPrefs.HasKey(VAOffsetKey))
        {
            PlayerPrefs.SetFloat(VAOffsetKey,PlayerPrefs.GetFloat(VAOffsetKey) - 0.01f);
        }

    }
//    public void InputffsetPositiveBy10ms()
//    {
//        if(PlayerPrefs.HasKey(InputOffsetKey))
//        {
//            PlayerPrefs.SetFloat(InputOffsetKey,PlayerPrefs.GetFloat(InputOffsetKey) + 10);
//        }
//
//    }
//    public void InputOffsetNegativeBy10ms()
//    {
//
//        if(PlayerPrefs.HasKey(InputOffsetKey))
//        {
//            PlayerPrefs.SetFloat(InputOffsetKey,PlayerPrefs.GetFloat(InputOffsetKey) - 10);
//        }
//    }
    #endregion


}
