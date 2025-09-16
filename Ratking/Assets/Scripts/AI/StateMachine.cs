using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

public class StateMachine : MonoBehaviour
{

    public EnemyAgentAI AgentAi;
    //TODO maybe change to hash structure --> Dictionary or set
    [SerializeField]
    public List<AgentStateBase<EnemyAgentAI>> AgentStates;
    public AgentStateBase<EnemyAgentAI> CurentState;

    [RequiredMember]
    public PatrollerData PatrollerData;

    public Transform RenderPoputAt;

   
    // Start is called before the first frame update
    void Start()
    {
        InitalizeStates();
    }
    void OnEnable()
    {
        if (CurentState==null && AgentStates!=null)
        {
            CurentState = AgentStates.First(x => x.Type == AgentStateType.Patrol);
        }
    }

    public void InitalizeStates()
    {
        AgentStates = new List<AgentStateBase<EnemyAgentAI>>();
        //Enlist all states available
        AgentStates.Add(new PatrollingState(AgentStateType.Patrol, AgentAi));
        AgentStates.Add(new SuspiciousState(AgentStateType.Suspicious, AgentAi));
        AgentStates.Add(new AlertState(AgentStateType.Alert, AgentAi));

        ShownNotification = null;

        //Setup the default state
        CurentState = AgentStates.First(x => x.Type == AgentStateType.Patrol);
        CurentState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurentState is not null)
        {
            TransitionState(CurentState.Update());
        }
    }

    void FixedUpdate()
    {
        if (CurentState is not null)
        {
            TransitionState(CurentState.FixedUpdate());
        }
    }

    public void TransitionState(AgentStateType transitionToStateType)
    {
        AgentStateBase<EnemyAgentAI> TransitionTo = null;
        if (transitionToStateType != AgentStateType.None)
        {
            TransitionTo = AgentStates.First(x => x.Type == transitionToStateType);
        }

        if (TransitionTo is not null )
        {
            CurentState.OnExit();
            CurentState = TransitionTo;
            CurentState.OnEnter();
            RenderEnterStatePopup(transitionToStateType);
        }
    }


    public GameObject ShownNotification; 
    public void RenderEnterStatePopup(AgentStateType transitionToStateType)
    {
        if (transitionToStateType == AgentStateType.Alert)
        {
            if (ShownNotification != null && ShownNotification.activeSelf == true)
            {
                DestroyImmediate(ShownNotification);
            }

            ShownNotification = Instantiate(PatrollerData.AlertStateNotificationPrefab,  this.RenderPoputAt);
            ShownNotification.transform.localPosition=new Vector3(0,0,0);
        }
        if (transitionToStateType == AgentStateType.Suspicious)
        {
            if (ShownNotification != null && ShownNotification.activeSelf == true)
            {
                DestroyImmediate(ShownNotification);
            }
            ShownNotification = Instantiate(PatrollerData.SuspiciousStateNotificationPrefab, this.RenderPoputAt);
            ShownNotification.transform.localPosition = new Vector3(0, 0, 0);

        }
    }



    public void ChangePawnMovementAndConeRotationSpeed()
    {

    }
}
