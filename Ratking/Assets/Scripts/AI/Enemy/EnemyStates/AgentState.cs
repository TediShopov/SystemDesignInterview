public enum AgentStateType
{
    None, Patrol, Suspicious, Alert
}



[System.Serializable]
public abstract class AgentStateBase<AgentType>
{

    public AgentStateBase(AgentStateType type, AgentType agent)
    {
        this.Agent = agent;
        this.Type = type;
    }
    public AgentType Agent { get; }
    public AgentStateType Type { get; }

    public abstract void OnEnter();
    public abstract void OnExit();

    public bool CanTransitionStates(AgentStateType from, AgentStateType to)
    {
        return true;
    }

    public virtual AgentStateType Update()
    {
        return AgentStateType.None;
    }

    public virtual AgentStateType FixedUpdate()
    {
        return AgentStateType.None;
    }
}
