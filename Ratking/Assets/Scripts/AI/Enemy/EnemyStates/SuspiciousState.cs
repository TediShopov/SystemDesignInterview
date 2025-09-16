using UnityEngine;

public class SuspiciousState : AgentStateBase<EnemyAgentAI>
{

    private GameObject SuspicionPointObject;
    private SuspicionPoint _currentSuspicionPoint;
    private bool _attachedToOnReachEvent = false;
    public SuspiciousState(AgentStateType type, EnemyAgentAI agent) : base(type, agent)
    {
    }

    public override void OnEnter()
    {
        TutorialBox.Instance.SetActionTutorialText(TutorialBox.TutorialAction.Suspicion);

        //this.Agent.SpriteRenderer.color = Color.yellow;
        this.Agent.AgentAnimator.SetInteger("MovementType", 1);
        //this.Agent.Sensor = this.Agent.GetComponentInChildren<EnemySensor>();nh
        this.Agent.Pathfinding.OnDestinationReached += OnReachedTarget;
        ChaseSuspicionPoint();
        this.Agent.EnableObjectForCoroutine = this.Agent.StartCoroutine(this.Agent.EnableObjectFor(this.Agent.DurationOfEnbabled));


        //if (this.Agent.Sensor)
        //{
        //    this.Agent.Pathfinding = this.Agent.gameObject.GetComponent<AIPathfinding>();
        //    if (this.Agent.Pathfinding is not null)
        //    {
        //        ChaseSuspicionPoint();
        //    }
        //}
    }

    public override void OnExit()
    {
        this.Agent.DestroyObject(SuspicionPointObject);
        this.Agent.Pathfinding.OnDestinationReached -= OnReachedTarget;
        this._attachedToOnReachEvent = false;


    }
    private void ChaseSuspicionPoint()
    {

        if (this.Agent.Sensor.SuspicionPointsList.Count == 0)
        {
            return;
        }

        _currentSuspicionPoint = this.Agent.Sensor.SuspicionPointsList[0];
        if (this.SuspicionPointObject == null)
        {
            this.SuspicionPointObject = this.Agent.CreateObjectAt(_currentSuspicionPoint.Position, this.Agent.VisualSuspicionPoint);
        }
        else if (_currentSuspicionPoint.Position != SuspicionPointObject.gameObject.transform.position)
        {
            this.Agent.DestroyObject(SuspicionPointObject);
            this.SuspicionPointObject = this.Agent.CreateObjectAt(_currentSuspicionPoint.Position, this.Agent.VisualSuspicionPoint);

        }


        this.Agent.Pathfinding.SetNewPath(_currentSuspicionPoint.Position);
        if (this.Agent.Pathfinding.PathTrack.Path.Count==0)
        {

            //Unreachable
            this.Agent.Sensor.SuspicionPointsList.RemoveAt(0);
            this.Agent.DestroyObject(SuspicionPointObject);
            SuspicionPointObject = null;
        }

    }

    public override AgentStateType Update()
    {

        if (Agent.Sensor.CanSeePlayer)
        {
            return AgentStateType.Alert;
        }


        if (this.Agent.Sensor.SuspicionPointsList.Count == 0)
        {
            return AgentStateType.Patrol;
        }

        if (!_currentSuspicionPoint.Equals(this.Agent.Sensor.SuspicionPointsList[0]))
        {
            ChaseSuspicionPoint();
        }

        return AgentStateType.None;
    }

    public void OnReachedTarget()
    {
        if (_currentSuspicionPoint.SourceObject ?? false)
        {
            Vector3 agentPosition = this.Agent.transform.position;
            Vector3 targetPosition = _currentSuspicionPoint.SourceObject.transform.position;
            Vector2 dirVector2 = (targetPosition - agentPosition).normalized;
            float distanceToTarget = Vector2.Distance(agentPosition, targetPosition);

            var hit2D = Physics2D.Raycast(this.Agent.transform.position, dirVector2, distanceToTarget, LayerMask.GetMask("Ground"));
            if (hit2D.collider == null && distanceToTarget < this.Agent.Sensor.ViewDiameter / 2.0f)
            {
               //I can see item
            }
        }
        if (this.Agent.Sensor.SuspicionPointsList.Count != 0)
        {

            this.Agent.Sensor.SuspicionPointsList.RemoveAt(0);
            this.Agent.DestroyObject(SuspicionPointObject);
            SuspicionPointObject = null;


            ChaseSuspicionPoint();
        }

    }
}
