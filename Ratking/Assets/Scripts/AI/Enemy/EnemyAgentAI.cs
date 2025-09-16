using System.Collections;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class EnemyAgentAI : MonoBehaviour
{
    public GameObject VisualSuspicionPoint;
    public GameObject VisualPlayerLastSeenPoint;
    private Vector3 _previousPosition;
    private Quaternion _previousRotation;

    public GameObject EnableOnSuspicionAndAlertEnter;
    public float DurationOfEnbabled;



    public SpriteRenderer SpriteRenderer;


    [Header("Sensory Related")]
    public EnemySensor Sensor;

    [Header("Long-Range Attack")]
    public EnemyAttack Attack;

    [Header("Path")]
    public AIPathfinding Pathfinding;


    [Header("Patrol State Values")]
    [SerializeField] public EnemyPatrolPath PatrolPath;

    [SerializeField] public Animator AgentAnimator;



    private Quaternion? _targetRotation;
    //private Vector3 _seekLocation;

    private SuspicionPoint? _prioritizedSuspicionPoint;
    private Vector3 _previousSeekLocation;
    public StateMachine EnemyStateMachine;

    public Coroutine EnableObjectForCoroutine;

    public IEnumerator EnableObjectFor(float x)
    {
        this.EnableOnSuspicionAndAlertEnter.SetActive(true);
        yield return new WaitForSeconds(x);
        this.EnableOnSuspicionAndAlertEnter.SetActive(false);
        EnableObjectForCoroutine = null;
    }


    // Start is called before the first frame update
    void Start()
    {
        Sensor = this.GetComponentInChildren<EnemySensor>();
        Attack = this.GetComponent<EnemyAttack>();
        Pathfinding = this.GetComponent<AIPathfinding>();
        //SpriteRenderer = this.GetComponent<SpriteRenderer>();
        EnemyStateMachine = this.GetComponent<StateMachine>();
        _prioritizedSuspicionPoint = null;
        this._previousPosition = new Vector3(0, 0, 0);
    }






    public GameObject CreateObjectAt(Vector3 pos, GameObject visualGameObject)
    {
        return Instantiate(visualGameObject, pos, Quaternion.identity);
    }
    public void DestroyObject(GameObject obj)
    {
        if (obj != null)
        {
            DestroyImmediate(obj);
        }
    }

    

    private Vector3 GetNewSeekLocation()
    {
        var pathTrack = this.Pathfinding.PathTrack;
        if (pathTrack.NodeReached != null && this.Pathfinding.PathTrack.ReachedDestinationNode())
        {
            return pathTrack.ExactDestination;

        }
        else if (pathTrack.NextNodeToFollow != null)
        {
            return this.Pathfinding.TraversableGrid.GetPosition(pathTrack.NextNodeToFollow.PositionInGrid);
        }


        return this.gameObject.transform.position;
    }

    private void MovePawn(Vector3 seekLocation)
    {
        this.gameObject.transform.position = Vector2.MoveTowards(this.gameObject.transform.position, seekLocation, EnemyStateMachine.PatrollerData.GetSpeed(EnemyStateMachine.CurentState.Type));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion currentRotation;
        Vector3 currentPosition = this.gameObject.transform.position;

        var seekLocation = GetNewSeekLocation();
        MovePawn(seekLocation);
        _prioritizedSuspicionPoint = GetPrioritizedSuspicionPoint();
        Debug.Log("");
        if (Sensor.CanSeePlayer)
        {
            if (!Attack.Running)
            {
                Debug.LogError("Can Attack player");
                Attack.Start();
            }
            LookAtInstant2D(LevelData.PlayerObject.transform.position);
            this._previousPosition = currentPosition;
            //MovePawn(Sensor.PlayerReference.transform.position);
            return;
        }

        if (_prioritizedSuspicionPoint.HasValue)
        {
            Vector3 dir = _prioritizedSuspicionPoint.Value.Position - currentPosition;
            RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, dir, this.Sensor.ViewDiameter / 2.0f, LayerMask.GetMask("Ground"));
            if (hit.collider == null)
            {
                LookAtInstant2D(_prioritizedSuspicionPoint.Value.Position);
                this._previousPosition = currentPosition;
                return;
            }

        }


        if (this.Pathfinding.PathTrack.Path.Count > 1)
        {
            Vector3 dirToPreviousSeek = (_previousSeekLocation - this.gameObject.transform.position).normalized;
            Vector3 dirToSeek = (seekLocation - this.gameObject.transform.position).normalized;

            //Will return only closest - cannot exceed 180 adn cannot be 0

            if (this._previousSeekLocation != seekLocation && Vector3.Angle(dirToSeek, dirToPreviousSeek) <= 160.0f)
            {
                StartLookAtInterpolated2D(seekLocation, EnemyStateMachine.PatrollerData.GetRotationSpeed(EnemyStateMachine.CurentState.Type));
            }
            else
            {
                LookAtInterpolated2D();
            }


        }
        else
        {
            LookAtInstant2D(seekLocation);
        }

        this._previousSeekLocation = seekLocation;


    }

    private SuspicionPoint? GetPrioritizedSuspicionPoint()
    {
        if (Sensor.SuspicionPointsList.Count > 0)
        {
            //if (Sensor.SuspicionPointsList[0].Priority == Single.MaxValue)
            //{
            //    Sensor.SuspicionPointsList.RemoveRange(1, Sensor.SuspicionPointsList.Count-1);
            //}
            return Sensor.SuspicionPointsList[0];
        }
        else
        {
            return null;
        }
    }





    public void LookAtInstant2D(Vector3 pos)
    {
        Sensor.gameObject.transform.rotation = GetRotationLookAtPosition2D(pos);
        Vector3 relPos = pos - this.gameObject.transform.position;

        if (Vector3.Angle(Vector3.right, relPos) <= 90)
        {
            this.SpriteRenderer.flipX = true;
            //this.gameObject.transform.localScale =  new Vector3(-1,1,1);
        }
        else
        {
            this.SpriteRenderer.flipX = false;

            //this.gameObject.transform.localScale = new Vector3(1, 1, 1);

        }
    }



    public void StartLookAtInterpolated2D(Vector3 pos, float interpolattionRate)
    {
        //turnnginEyeForDuration = 0.0f;
        //RateOfInterpolation = interpolattionRate;
        this._previousRotation = this.gameObject.transform.rotation;
        this._targetRotation = GetRotationLookAtPosition2D(pos);
    }


    private void LookAtInterpolated2D()
    {
        if (_targetRotation.HasValue)
        {

            

            Sensor.gameObject.transform.rotation = Quaternion.RotateTowards(Sensor.gameObject.transform.rotation,
                _targetRotation.Value, EnemyStateMachine.PatrollerData.GetRotationSpeed(EnemyStateMachine.CurentState.Type) * Time.deltaTime);
        }

    }

    public Quaternion GetRotationLookAtPosition2D(Vector3 targetLocation)
    {
        Vector3 myLocation = Sensor.gameObject.transform.position;
        targetLocation.z = myLocation.z; // ensure there is no 3D rotation by aligning Z position

        // vector from this object towards the target location
        Vector3 vectorToTarget = targetLocation - myLocation;
        // rotate that vector by 90 degrees around the Z axis
        Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * vectorToTarget;

        // get the rotation that points the Z axis forward, and the Y axis 90 degrees away from the target
        // (resulting in the X axis facing the target)
        Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);
        return targetRotation;

    }

}
