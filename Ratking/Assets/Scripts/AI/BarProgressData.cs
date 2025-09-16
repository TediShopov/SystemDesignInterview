using UnityEngine;

[CreateAssetMenu(fileName = "Bar Progress Data", menuName = "Bar Progress Data")]
public class BarProgressData : ScriptableObject
{
    [Header("Enemy Detection Dangers")]
    [SerializeField] public int InEnemyVisionPerTick;
    [SerializeField] public int OnDetected;

    [Header("Sneak Rewards")]
    [SerializeField] public int UndetectedInEnemyHearingRangePerSecond;
    [SerializeField] public int MaxUndetectedProgressFromSingleEnemy;

    [Header("Collection Rewards")]
    [SerializeField] public float GoldToProgressModifier;

}
