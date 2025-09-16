using System;
using System.Collections;
using System.Collections.Generic;
using FunkyCode;
using Unity.VisualScripting;
using UnityEngine;



public struct SuspicionPoint
{
    public SuspicionPoint(float prio, Vector3 pos, GameObject sourceObject = null)
    {
        this.Priority = prio;
        this.Position = pos;
        this.SourceObject = sourceObject;

    }
    //TODO check if float comparison is alright
    public float Priority;
    public Vector3 Position;
    public GameObject SourceObject;
    public override string ToString()
    {
        return $"Priority: {Priority}, Position: {Position}";
    }
    public override bool Equals(object obj)
    {
        if (!(obj is SuspicionPoint))
            return false;

        SuspicionPoint otherSusPoint = (SuspicionPoint)obj;

        return Helpers.CompareFloats(this.Priority, otherSusPoint.Priority, 0.05f) &&
               Helpers.CompareVectors(this.Position, this.Position, 0.05f);
        // compare elements here

    }

}

[RequireComponent(typeof(CircleCollider2D))]
public class EnemySensor : MonoBehaviour
{

    public AIPathfinding Pawn;
    public float ViewDiameter = 5f;
    [Range(0, 360)]
    public float ViewAngle = 30f;

    //public float HearingDistance = 10f;

    public LayerMask TargetLayer;
    public LayerMask ObstacleLayer;
    public LayerMask SoundLayer;

    public Light2D EnemyLightCone;


    //public GameObject PlayerReference;

    public bool CanSeePlayer { get; private set; }
    public bool CanHearObject { get; private set; }

    [SerializeField] public List<SuspicionPoint> SuspicionPointsList;


    private CircleCollider2D _hearingRange;

    public float EnemySize = 0.5f;
    // Start is called before the first frame update

    void Awake()
    {
        StartCoroutine(FOVCheck());

        //PlayerReference = GameObject.FindGameObjectWithTag("Player");
        SuspicionPointsList = new List<SuspicionPoint>();
        _hearingRange = this.gameObject.GetComponent<CircleCollider2D>();
    }

    void OnEnable()
    {
        StartCoroutine(FOVCheck());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }


    // Handling sight detection
    private IEnumerator FOVCheck()
    {
        if (EnemyLightCone!=null)
        {
            EnemyLightCone.spotAngleInner = this.ViewAngle;
            EnemyLightCone.spotAngleOuter = this.ViewAngle;
            EnemyLightCone.size = this.ViewDiameter / 2.0f;

        }
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            FOV();
        }
    }
    private void FOV()
    {
        Vector3 sigthDirection = transform.right;
        if (transform.localScale.x < 0)
            sigthDirection *= -1;

        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, ViewDiameter / 2, TargetLayer);

        // If target is within range
        if (rangeCheck.Length > 0)
        {
            Transform target = rangeCheck[0].transform;
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            if (Vector2.Angle(sigthDirection, directionToTarget) < ViewAngle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);

                //Check for obstacles between enemy and player
                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, ObstacleLayer))
                    CanSeePlayer = true;
                else
                    CanSeePlayer = false;
            }
            else if (Vector2.Distance(transform.position, target.position) < EnemySize)
            {
                CanSeePlayer = true;
            }
            else
                CanSeePlayer = false;

        }

        else if (CanSeePlayer)
            CanSeePlayer = false;

    }

    // Handling sound detection
    void OnTriggerEnter2D(Collider2D collider2D)
    {
        //Debug.Log("I KNOW");
        GameObject target = collider2D.gameObject;

        if (target.layer == LayerMask.NameToLayer("Sound"))
        {
            CanHearObject = true;

            //Calculate priority

            if (collider2D.GetType() == typeof(CircleCollider2D))
            {
                CircleCollider2D overlappingCircle = (CircleCollider2D)collider2D;
                float dist = Vector3.Distance(_hearingRange.transform.position, target.transform.position);
                float prio = _hearingRange.radius + overlappingCircle.radius - dist;

                //var soundGenerator = overlappingCircle.gameObject.GetComponent<SoundGenerator>();
                //if (soundGenerator!=null)
                //{

                //}
                var soundGenerator = overlappingCircle.gameObject.GetComponent<SoundExpansionAnimation>();
                if (soundGenerator != null)
                {
                    GameObject caster = overlappingCircle.gameObject.GetComponent<SoundExpansionAnimation>().Caster;
                    //if (caster == null)
                    //{
                    //    Debug.LogError("There is no Caster");
                    //}
                    var sp = new SuspicionPoint(prio, target.transform.position, overlappingCircle.gameObject.GetComponent<SoundExpansionAnimation>().Caster);
                    AddSuspicionPoint(sp);
                }

                //Debug.LogWarning($"Max prio Suspicion point is : {SuspicionPointsList[0]}");
            }
            else
            {
                throw new ArgumentException("Sound collision must be a circle");
            }
        }


    }

    public void AddSuspicionPoint(SuspicionPoint sp)
    {
        SuspicionPointsList.Insert(0, sp);
        ReorderSuspicionPointList();
    }

    void ReorderSuspicionPointList()
    {
        //By priority
        // SuspicionPointsList = SuspicionPointsList.OrderByDescending(x => x.Priority).ToList();

        //By most recent
        //var sp=this.SuspicionPointsList[this.SuspicionPointsList.Count - 1];

    }

    void OnTriggerExit2D(Collider2D collider2D)
    {
        Debug.LogWarning("Exited sound trigger");
        CanHearObject = false;
    }


    private void OnDrawGizmos()
    {
        //if (CanSeePlayer)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(transform.position, PlayerReference.transform.position);
        //}

        //Gizmos.color = Color.magenta;
        //UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, EnemySize);

        //Gizmos.color = Color.white;
        //UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, ViewDistance / 2);

        //Vector3 viewAngle1 = DirectionFromAngle(-ViewAngle / 2);
        //Vector3 viewAngle2 = DirectionFromAngle(ViewAngle / 2);

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + viewAngle1 * (ViewDistance / 2));
        //Gizmos.DrawLine(transform.position, transform.position + viewAngle2 * (ViewDistance / 2));

    }

    private Vector2 DirectionFromAngle(float angleInDegrees)
    {
        angleInDegrees -= transform.eulerAngles.z - 90;

        if (transform.localScale.x < 0)
            angleInDegrees += 180;
        return new Vector2(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
