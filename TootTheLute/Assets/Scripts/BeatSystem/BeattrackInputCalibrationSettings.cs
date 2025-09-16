using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BeattrackInputCalibration", order = 1)]

public class BeattrackInputCalibrationSettings : ScriptableObject
{
    public float VAOffset = 0f;
    public float InputOffset = 0f;
}
