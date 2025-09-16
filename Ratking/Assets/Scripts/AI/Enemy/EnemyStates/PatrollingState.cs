using UnityEngine;

public class PatrollingState : AgentStateBase<EnemyAgentAI>
{
    public override void OnEnter()
    {
        //this.Agent.SpriteRenderer.color = Color.white;
        if (this.Agent.PatrolPath != null)
        {
            this.Agent.PatrolPath.StartPatrolPath(this.Agent.Pathfinding);
        }
        this.Agent.AgentAnimator.SetInteger("MovementType", 1);

    }

    public override void OnExit()
    {
        if (this.Agent.PatrolPath != null)
        {
            this.Agent.PatrolPath.RemoveFromPath(this.Agent.Pathfinding);
        }
    }

    public PatrollingState(AgentStateType type, EnemyAgentAI agent) : base(type, agent)
    {
    }
    public override AgentStateType Update()
    {
        //if (this.Agent.Pathfinding.PathTrack.Path.Count<=0)
        //{
        //    OnEnter();
        //}


        if (Agent.Sensor.CanSeePlayer)
        {
            return AgentStateType.Alert;
        }
        if (this.Agent.Sensor.SuspicionPointsList.Count != 0)
        {
            return AgentStateType.Suspicious;
        }
        return AgentStateType.None;
    }

}
