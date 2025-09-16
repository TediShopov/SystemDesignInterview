using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertState : AgentStateBase<EnemyAgentAI>
{

    private Vector3 _targetPosition;
    private Quaternion _currentRotation;

    //private IEnumerator ChasingCoroutine;

    private Coroutine ChasingPlayerInSight;
    private Coroutine ChaseOutOfSightPlayerCountDownCoroutine;
    private Coroutine FillDangerBar;

    private bool _chasePlayerOutOfSight;
    private bool _couldSeePlayerLastFrame;

    private Vector3 _lastSpottedPosition;


    public float UpdateDangerBarEvery = 0.5f;
    public float ResetPathInSeconds = 0.2f;
    public float SecondToStayInSearch = 7.0f;
    private Vector3 _exactDestinationOnPreviousPath = new Vector3();


    public IEnumerator FillDangerBarEvery()
    {
        while (true)
        {
            yield return new WaitForSeconds(UpdateDangerBarEvery);
            GlobalProgress.Instance.DangerBar.CurrentScore +=
                GlobalProgress.Instance.BarProgressData.InEnemyVisionPerTick;
        }
    }
    public IEnumerator ChasePlayerTargetAfter()
    {
        while (true)
        {
            var targetPosition = LevelData.PlayerObject.transform.position;
            if (this.Agent.Sensor.CanSeePlayer && !Helpers.CompareVectors(this.Agent.transform.position, _exactDestinationOnPreviousPath, 0.5f))
            {
                this.Agent.Pathfinding.SetNewPath(targetPosition);
                _exactDestinationOnPreviousPath = this.Agent.Pathfinding.PathTrack.ExactDestination;
            }

            Debug.LogWarning("Reset Path from alert mode");
            yield return new WaitForSeconds(ResetPathInSeconds);
        }
    }

    public bool IsInGeneralDirection(Vector3 dir, Vector3 dirTwo)
    {
        return Vector3.Dot(dir, dirTwo) > 0.0f;
    }

    [Range(1, 5)]
    public int PlatformConnectionsToSearch = 3;


    List<GridNode> _nodesToCheckWhenSearching = new List<GridNode>();
    private List<GameObject> _visualSearchPoints = new List<GameObject>();

    public void GetLastNodeToCheckInConnections(GridNode startNode, Vector3 generalDirection, int iterCount = 0)
    {
        if (iterCount >= 3)
        {
            Debug.Log(" Search Algorithm : Added");
            _nodesToCheckWhenSearching.Add(startNode);
            return;
        }
        foreach (var startNodeConnection in startNode.Connections)
        {
            Vector3 dirOfNode = (this.Agent.Pathfinding.TraversableGrid.GetPosition(startNodeConnection.Neighbor.PositionInGrid) - this.Agent.transform.position).normalized;



            if (IsInGeneralDirection(generalDirection, dirOfNode))
            {
                GetLastNodeToCheckInConnections(startNodeConnection.Neighbor, generalDirection, iterCount + 1);
            }
        }
    }

    public void GetNodesToReach()
    {
        _nodesToCheckWhenSearching.Clear();
        Vector3Int startTile = this.Agent.Pathfinding.TraversableGrid.GetClosestTraversableTile(_lastSpottedPosition);
        GridNode startNode = this.Agent.Pathfinding.TraversableGrid.GetClosestPlatformNode(startTile);
        Vector3 dir = (_lastSpottedPosition - this.Agent.transform.position).normalized;
        GetLastNodeToCheckInConnections(startNode, dir, 0);
        foreach (var n in this._nodesToCheckWhenSearching)
        {
            this._visualSearchPoints.Add(this.Agent.CreateObjectAt(this.Agent.Pathfinding.TraversableGrid.GetPosition(n.PositionInGrid),
                this.Agent.VisualSuspicionPoint));

        }
        ReachNextSearchTarget();
        this.Agent.Pathfinding.OnDestinationReached += OnReachedTarget;
    }

    private IEnumerator ChaseOutOfSightPlayerCountDown(float seconds)
    {
        _chasePlayerOutOfSight = true;
        yield return new WaitForSecondsRealtime(seconds);
        _chasePlayerOutOfSight = false;
    }

    public AlertState(AgentStateType type, EnemyAgentAI agent) : base(type, agent)
    {
        //ChasingPlayerInSight = ChasePlayerTargetAfter();
        //ChaseOutOfSightPlayerCountDownCoroutine = StartChasingPlayerOutOfSight();
    }

    public override void OnEnter()
    {
        this.Agent.EnableObjectForCoroutine = this.Agent.StartCoroutine(this.Agent.EnableObjectFor(this.Agent.DurationOfEnbabled));
        TutorialBox.Instance.SetActionTutorialText(TutorialBox.TutorialAction.Detected);
        this.VisualLastSeenGameObject = null;
        this._chasePlayerOutOfSight = true;
        //this.Agent.SpriteRenderer.color = Color.red;
        this.Agent.AgentAnimator.SetInteger("MovementType",2);
        GlobalProgress.Instance.DangerBar.CurrentScore += GlobalProgress.Instance.BarProgressData.OnDetected;
        Debug.LogError("Enter alert mode");

        _targetPosition = LevelData.PlayerObject.transform.position;
        ChasingPlayerInSight = this.Agent.StartCoroutine(ChasePlayerTargetAfter());
    }

    public override void OnExit()
    {
        Agent.Sensor.SuspicionPointsList.Clear();
        //Agent.Sensor.AddSuspicionPoint(new SuspicionPoint() { Position = _targetPosition, Priority = Single.MaxValue });
        this.Agent.StopCoroutine(ChasingPlayerInSight);
        this.Agent.DestroyObject(VisualLastSeenGameObject);
        ResetSearchPoints();
    }

    private void ResetSearchPoints()
    {
        foreach (var searchPoint in this._visualSearchPoints)
        {
            this.Agent.DestroyObject(searchPoint);
        }

        _visualSearchPoints.Clear();
        this._nodesToCheckWhenSearching.Clear();
    }

    void ReachNextSearchTarget()
    {
        if (_nodesToCheckWhenSearching.Count > 0)
        {
            this.Agent.Pathfinding.SetNewPath(
                this.Agent.Pathfinding.TraversableGrid.GetPosition(_nodesToCheckWhenSearching[0].PositionInGrid));
        }
        else
        {
            this.Agent.Pathfinding.OnDestinationReached -= OnReachedTarget;
            OnExit();
        }
    }

    public void OnReachedTarget()
    {
        if (_nodesToCheckWhenSearching.Count > 0)
        {
            _nodesToCheckWhenSearching.RemoveAt(0);
            this.Agent.DestroyObject(_visualSearchPoints[0]);
            _visualSearchPoints.RemoveAt(0);
            ReachNextSearchTarget();
        }


    }


    public GameObject VisualLastSeenGameObject;



    public override AgentStateType Update()
    {
        _targetPosition = LevelData.PlayerObject.transform.position;

        if (!Agent.Sensor.CanSeePlayer && _couldSeePlayerLastFrame)
        {
            if (ChasingPlayerInSight != null)
            {
                this.Agent.StopCoroutine(ChasingPlayerInSight);

            }
            this.Agent.StopCoroutine(FillDangerBar);
            if (this.VisualLastSeenGameObject == null)
            {
                this.VisualLastSeenGameObject = this.Agent.CreateObjectAt(_lastSpottedPosition, this.Agent.VisualPlayerLastSeenPoint);
            }


            //Refresh 

            Debug.LogWarning("Chasing Player last position");
            //this.Agent.Pathfinding.SetNewPath(_lastSpottedPosition);
            Debug.Log("Get Search Algorithm");
            this.GetNodesToReach();
            //return AgentStateType.Suspicious;
            if (ChaseOutOfSightPlayerCountDownCoroutine != null)
            {
                this.Agent.StopCoroutine(ChaseOutOfSightPlayerCountDownCoroutine);
            }
            Debug.LogWarning("Started Looking for player out of sight");

            ChaseOutOfSightPlayerCountDownCoroutine = this.Agent.StartCoroutine(this.ChaseOutOfSightPlayerCountDown(SecondToStayInSearch));
        }


        if (Agent.Sensor.CanSeePlayer)
        {
            this.Agent.DestroyObject(VisualLastSeenGameObject);


            //GlobalProgress.Instance.DangerBar.CurrentScore += (int)
            //    GlobalProgress.Instance.BarProgressData.InEnemyVisionPerSecond * Time.fixedDeltaTime;

            _lastSpottedPosition = _targetPosition;
            if (!_couldSeePlayerLastFrame)
            {


                ResetSearchPoints();
                this.FillDangerBar = this.Agent.StartCoroutine(this.FillDangerBarEvery());

                ChasingPlayerInSight = this.Agent.StartCoroutine(ChasePlayerTargetAfter());
                if (ChaseOutOfSightPlayerCountDownCoroutine != null)
                {
                    this.Agent.StopCoroutine(ChaseOutOfSightPlayerCountDownCoroutine);
                }
            }

        }

        if (_chasePlayerOutOfSight == false)
        {
            //Chasing Player failed return to suspicious state
            return AgentStateType.Suspicious;
        }



        //else
        //{
        //    ChaseTarget();
        //}
        _couldSeePlayerLastFrame = this.Agent.Sensor.CanSeePlayer;
        return AgentStateType.None;
    }

}
