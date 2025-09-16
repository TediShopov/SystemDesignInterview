using UnityEngine;

[CreateAssetMenu(fileName = "New Patroller Data", menuName = "Patroller Data")]
public class PatrollerData : ScriptableObject
{
    [Header("Patrol State Values")]
    [Range(0.02f, 0.09f)]
    [SerializeField] public float PawnMovementSpeed;
    [SerializeField] public float SightConeTurnSpeed;

    [Header("Suspicion State Values")]
    [Range(0.02f, 0.09f)]
    [SerializeField][InspectorName("Pawn Movement Speed")] public float SuspicionPawnMovementSpeed;
    [SerializeField][InspectorName("Pawn Movement Speed")] public float SuspicionSightConeTurnSpeed;

    [Header("Alert State Values")]
    [Range(0.02f, 0.09f)]
    [SerializeField][InspectorName("Pawn Movement Speed")] public float AlertPawnMovementSpeed;
    [SerializeField][InspectorName("Pawn Movement Speed")] public float AlertSightConeTurnSpeed;

    [SerializeField] public GameObject AlertStateNotificationPrefab;
    [SerializeField] public GameObject SuspiciousStateNotificationPrefab;



    public float GetSpeed(AgentStateType type)
    {
        if (type == AgentStateType.Patrol)
        {
            return PawnMovementSpeed;
        }
        else if (type == AgentStateType.Suspicious)
        {
            return SuspicionPawnMovementSpeed;
        }
        else if (type == AgentStateType.Alert)
        {
            return AlertPawnMovementSpeed;
        }

        return 0;
    }

    public float GetRotationSpeed(AgentStateType type)
    {
        if (type == AgentStateType.Patrol)
        {
            return SightConeTurnSpeed;
        }
        else if (type == AgentStateType.Suspicious)
        {
            return SuspicionSightConeTurnSpeed;
        }
        else if (type == AgentStateType.Alert)
        {
            return AlertSightConeTurnSpeed;
        }

        return 0;
    }
}
