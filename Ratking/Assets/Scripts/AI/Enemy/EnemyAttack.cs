using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    // Start is called before the first frame update
    [Range(1, 5)]
    [SerializeField] private float _cooldown;
    [SerializeField] private float _lifetime;

    private Coroutine _shootCoroutine;
    private Coroutine _lineLifeTime;

    public EnemySensor Sensor;
    private EntityHealth PlayerHealth;
    public Animator AgentAnimator;

    private LineRenderer lineRenderer;
    public bool Running { get; private set; }

    private EnemyAudioPlayer _enemyAudioPlayer;
    public void Start()
    {
        PlayerHealth = LevelData.PlayerObject.GetComponent<EntityHealth>();
        Sensor = this.GetComponentInChildren<EnemySensor>();
        _enemyAudioPlayer = this.GetComponent<EnemyAudioPlayer>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        Running = false;
    }
    void OnEnable()
    {
        Running = false;
    }

    public void StartAttack()
    {
        Debug.LogError("Attack start");
        Running = true;
        _shootCoroutine = StartCoroutine(ShootWithCooldown(_cooldown));

    }

    IEnumerator ShootWithCooldown(float cooldown)
    {
        while (true)
        {
            // Wait for the cooldown time
            yield return new WaitForSeconds(cooldown);

            //If the enemy lost sight of the player than it stops
            if (!Sensor.CanSeePlayer)
            {
                 Running = false;
                StopCoroutine(_shootCoroutine);
            }
            else
            {
                //if(!Sensor.CanSeePlayer)
                //    Debug.LogError("I AM JOHN WICK");
                GameObject target = LevelData.PlayerObject;
                AttackPlayer(target);
            }
        }
    }

    void Update()
    {
        if (Sensor.CanSeePlayer && Running == false)
        {
            StartAttack();
        }
        //if (Sensor.CanSeePlayer)
        //{
        //    Debug.LogError("cannot see player");
        //}
    }

    float GetShootingAngle(Vector3 targetPos)
    {
         Vector3 relDirOfShot= (targetPos - transform.position).normalized;

        float closestAngle= Vector2.Angle(relDirOfShot, Vector3.right);

        //shot is to the left
        if (closestAngle>90)
        {
            closestAngle = 180 - closestAngle;
        }
        


        //if shot is down make it negative
        if (Vector2.Dot(relDirOfShot,Vector2.down)>=0)
        {
            closestAngle = -closestAngle;
        }
         Debug.Log($"Shooting Angle {closestAngle}");
        return closestAngle;
    }

    private void AttackPlayer(GameObject target)
    {
        //this.
        _enemyAudioPlayer.PlayAudio(_enemyAudioPlayer.ShootClip,false);
        PlayerHealth.TakeDamage(15);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, target.transform.position);
        lineRenderer.enabled = true;
        //_lineLifeTime = DisableLineAfterLifeTime(_lifetime);
        this.AgentAnimator.SetTrigger("Shoot");
        this.AgentAnimator.SetFloat("ShotAngle",GetShootingAngle(target.transform.position));
        _lineLifeTime = StartCoroutine(DisableLineAfterLifeTime(_lifetime));

        //StartCoroutine(_lineLifeTime);
   }

    IEnumerator DisableLineAfterLifeTime(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        lineRenderer.enabled = false;
        StopCoroutine(_lineLifeTime);
    }

}
