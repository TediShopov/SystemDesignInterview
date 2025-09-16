using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

//using System.Diagnostics;

public class BeatmapTrack : MonoBehaviour
{
    public bool IsInputCallibration = false;
    public int VisibleBeatsOnScreen = 8;
    public int ApproachingBeatIndex = 0;
    public int SuccessfulInputCount = 0;
    public int UnsuccessfulInputCount = 0;
    public int BeatIndicesToDestroy = 0;
    public float DestroyAfter = 1;
    public float BeatInputErrorSeconds = 0.15f;
    public float NextBeatOffset = -1;
    public float RemainingTimeUntilVisibleBeatExhaustion = 0;

    public Beatmap Beatmap;
    public RectTransform ReferenceTrack;
    //IMPORTANT the origin of the prefab is expected to be on the Target Reference Line
    public GameObject BeatVisualCuePrefab;
    public Transform ReferenceTrackTransform;
    //This is the target the player is expected to hit the beats on
    public Transform Target;

    public List<BeatObject> BeatObjects = new List<BeatObject>();
    public List<float> PlayerClickedTrackTimes;
    public List<float> PlayerDeviations;
    public List<GameObject> RecordedOnTrack;
    public GameObject InputTrackMarker;
    public bool IsApproachingBeatIndexInRange => ApproachingBeatIndex >= 0 && ApproachingBeatIndex < BeatObjects.Count;

    public int PlayerInputsToCollect = 5; public float AvgDeviationFromNearestBeat = 0;
    public float HalfInputError => BeatInputErrorSeconds / 2.0f;
    public float VisibleSoundtrackInSeconds => Beatmap.Conductor.Crochet * VisibleBeatsOnScreen;

    public bool IsInputOnTime(float error) => error > -HalfInputError && error < HalfInputError;
    public float GetBeatLength(Beat beat) => beat.Length * Beatmap.Conductor.Crochet;

    private void Start()
    {
        this.Beatmap.Setup();
        this.Beatmap.Conductor.Play();
        InitializeBeatObjects();
    }

    private void Update()
    {
        if (IsInputCallibration)
        {
            RedrawPlayTrackedInputsOnBeatTrack();
        }

        for (int i = 0; i < BeatObjects.Count; i++)
        {
            var beatObject = BeatObjects[i];
            float timeToBeatArrival = GetTimeToBeatArrival(i, true);
            if (timeToBeatArrival < -DestroyAfter)
            {
                BeatIndicesToDestroy++;
            }

            //Only check the beats that are not marked for destruction
            if (timeToBeatArrival < -HalfInputError && i >= ApproachingBeatIndex)
            {
                if (BeatObjects[ApproachingBeatIndex].State != BeatObjectState.Successful)
                {
                    BeatObjects[ApproachingBeatIndex].State = BeatObjectState.Unsuccessful;
                }

                //Beat passed udapte arriving time
                ApproachingBeatIndex++;
            }

            float offsetFromTargetX = CalculateBeatOffsetFromTargetPosition(i);

            //Move the beat object along the track
            var rectTransform = beatObject.gameObject.GetComponent<RectTransform>();

            if (beatObject.State != BeatObjectState.Successful)
            {
                rectTransform.localPosition = new Vector3(
                    Target.localPosition.x + offsetFromTargetX,
                    rectTransform.localPosition.y,
                    rectTransform.localPosition.z);
            }
        }

        if (ShouldLoadLoopingBeats())
            InitializeBeatObjects();

        if (BeatIndicesToDestroy > 0)
        {
            //Destroy the beat gameobjects
            for (int i = 0; i < BeatIndicesToDestroy; i++)
                Destroy(BeatObjects[i].gameObject);

            //Remove the from the list of active objects
            BeatObjects.RemoveRange(0, BeatIndicesToDestroy);

            //Adjust the index of the next beat
            ApproachingBeatIndex -= BeatIndicesToDestroy;

            //Reset counter
            BeatIndicesToDestroy = 0;
        }
    }
    public void RedrawPlayTrackedInputsOnBeatTrack()
    {
        if (PlayerDeviations.Count == 0 || RecordedOnTrack.Count == 0) return;
        for (int i = 0; i < BeatObjects.Count; i++)
        {
            if (i < RecordedOnTrack.Count)
            {
                float offsetFromTargetX = CalculateBeatOffsetFromTargetPosition(i, true, true);

                float rectSizeX = ReferenceTrack.rect.width;
                float deviationInPixels = Remap(AvgDeviationFromNearestBeat, 0, VisibleSoundtrackInSeconds, 0, rectSizeX);

                offsetFromTargetX -= deviationInPixels;
                //Debug.Log($"Offset {i}From Target: {offsetFromTargetX}");
                var markerRectT = RecordedOnTrack[i].gameObject.GetComponent<RectTransform>();
                markerRectT.localPosition = new Vector3(
                    Target.localPosition.x + offsetFromTargetX,
                    markerRectT.localPosition.y,
                    markerRectT.localPosition.z);
            }
        }
    }
    public void SwitchToBeatmap(Beatmap newBeatmap)
    {
        if (this.Beatmap != null)
        {
            this.Beatmap.Conductor.Stop();
        }

        this.ApproachingBeatIndex = 0;
        this.AvgDeviationFromNearestBeat = 0;
        this.SuccessfulInputCount = 0;
        this.UnsuccessfulInputCount = 0;
        NextBeatOffset = -1;

        if (BeatObjects != null)
        {
            foreach (var o in BeatObjects)
            {
                Destroy(o.gameObject);
            };
            this.BeatObjects.Clear();
        }

        this.Beatmap = newBeatmap;
        this.Beatmap.Setup();
        this.Beatmap.Conductor.Play();
        InitializeBeatObjects();
    }

    public bool InputBeat()
    {
        float error = GetInputErrorFromApproachingBeat();

        //Only if error if valid
        if (float.IsNaN(error)) return false;

        //This is the same as the arrival time
        //Negative error means the input came in late, positive means early
        if (IsInputOnTime(error))
        {
            BeatObjects[ApproachingBeatIndex].State = BeatObjectState.Successful;
            SuccessfulInputCount++;
            return true;
        }
        else
        {
            UnsuccessfulInputCount++;
            return false;
        }
    }

    public void RegisterInput(InputAction.CallbackContext context)
    {
        if (context.performed) // Fires event when left mouse button is pressed
        {
            if (this.PlayerClickedTrackTimes.Count < this.PlayerInputsToCollect)
            {
                this.PlayerClickedTrackTimes.Add(this.Beatmap.Conductor.SongPosition);
                //float error = GetInputErrorFromClosestBeat();
                float error = GetSmallestInputError();
                this.PlayerDeviations.Add(error);
                this.AvgDeviationFromNearestBeat = PlayerDeviations.Sum() / PlayerDeviations.Count;
                var m = Instantiate(InputTrackMarker, ReferenceTrackTransform);
                this.RecordedOnTrack.Add(m);
            }
        }
    }
    public void RegisterInputRaw()
    {
        if (this.PlayerClickedTrackTimes.Count < this.PlayerInputsToCollect)
        {
            this.PlayerClickedTrackTimes.Add(this.Beatmap.Conductor.SongPosition);
            //float error = GetInputErrorFromClosestBeat();
            float error = GetSmallestInputError();
            this.PlayerDeviations.Add(error);
            this.AvgDeviationFromNearestBeat = PlayerDeviations.Sum() / PlayerDeviations.Count;
            var m = Instantiate(InputTrackMarker, ReferenceTrackTransform);
            this.RecordedOnTrack.Add(m);
        }
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public void InitializeBeatObjects()
    {
        var newBeats = Beatmap.GetFlattenedBeats();
        if (NextBeatOffset < 0)
            NextBeatOffset = Beatmap.Conductor.Offset;

        for (int i = 0; i < newBeats.Count; i++)
        {
            var beatObject =
                Instantiate(BeatVisualCuePrefab, ReferenceTrackTransform).GetComponent<BeatObject>();
            beatObject.Beat = newBeats[i];
            beatObject.TimingOnTrack = NextBeatOffset;
            beatObject.BeatTrackOn = this;
            NextBeatOffset += GetBeatLength(newBeats[i]);

            BeatObjects.Add(beatObject);
        }
    }

    //The difference between the song start and the time the beat would be played
    public float GetTimeToBeatArrival(int index, bool withOffset = true)
    {
        return GetApproachingBeatTime(index, withOffset) - Beatmap.Conductor.SongPosition;
    }

    private float GetApproachingBeatTime(int index, bool withAudioOffset = true)
    {
        if (index >= 0 && index < this.BeatObjects.Count)
        {
            if (withAudioOffset)
                return this.BeatObjects[index].TimingOnTrack + InputCalibration.InputOffset;
            else
                return this.BeatObjects[index].TimingOnTrack;
        }
        return -1;
    }

    public float CalculateBeatOffsetFromTargetPosition(int index, bool audioOffset = true, bool visualOffset = true)
    {
        float timeToBeatArrival = GetTimeToBeatArrival(index, audioOffset);
        if (visualOffset)
            timeToBeatArrival += InputCalibration.VAOffset;
        float rectSizeX = ReferenceTrack.rect.width;
        return Remap(timeToBeatArrival, 0, VisibleSoundtrackInSeconds, 0, rectSizeX);
    }

    public bool ShouldLoadLoopingBeats()
    {
        //If empty relaod
        if (BeatObjects.Count == 0) return true;
        //If beats not empty
        //float remainingTimeFromBeats = Mathf.Abs(Beatmap.Conductor.SongPosition - BeatTimings[BeatTimings.Count - 1]);
        float remainingTimeFromBeats = Mathf.Abs(Beatmap.Conductor.SongPosition - BeatObjects[BeatObjects.Count - 1].TimingOnTrack);
        float visibleSoundInSeconds = (VisibleSoundtrackInSeconds + DestroyAfter);
        Debug.Log($"{remainingTimeFromBeats} < {visibleSoundInSeconds}");
        return remainingTimeFromBeats < visibleSoundInSeconds;
    }

    public float GetInputErrorFromApproachingBeat()
    {
        if (BeatObjects.Count <= 0 && IsApproachingBeatIndexInRange) return float.NaN;
        return GetTimeToBeatArrival(ApproachingBeatIndex);
    }

    private float GetSmallestInputError()
    {
        float absoluteDistanceToBeat = float.MaxValue;
        int lastSign = +1;
        for (int i = ApproachingBeatIndex - 1; i < BeatObjects.Count; i++)
        {
            var beatObject = BeatObjects[i];
            float absTime = Mathf.Abs(GetTimeToBeatArrival(i, true));
            if (absTime < absoluteDistanceToBeat)
            {
                absoluteDistanceToBeat = absTime;
                lastSign = Math.Sign(GetTimeToBeatArrival(i, true));
            }
        }
        return absoluteDistanceToBeat * (float)lastSign;
    }
}