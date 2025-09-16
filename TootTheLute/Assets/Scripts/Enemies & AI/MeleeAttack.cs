using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{

    public float OffsetFromEnemy; 
    public float FireRateSeconds = 1f;
    public float AttackRange = 1f;
    //Attack action when this reaches 0
    public float AttackMaxCooldown = 5;
    public float CurrentAttackTimer = 1f;

    public Enemy Enemy;
    public Animator AttackAnimator;
    public GameObject RotationPivot;

    [Header("Sound")]
    public AK.Wwise.Event MeleeAttackSound;

    void Start()
    {
        Enemy = GetComponent<Enemy>();
        CurrentAttackTimer = AttackMaxCooldown;

    }

    public void Update()
    {

        if (Enemy == null) return;
        if (Enemy.GetComponent<Health>().GetCurrentHealth() <= 0)
        {
            this.enabled = false;
            return ;
        }

        float zAngle = OrbitalApproachingCircle.GetRotationAngle(RotationPivot.transform.position, Enemy.AttackTarget.transform.position);
        var prevEulerAngles = RotationPivot.transform.eulerAngles;
        RotationPivot.transform.eulerAngles = new Vector3(prevEulerAngles.x, prevEulerAngles.y, zAngle);

        float d = Vector2.Distance(Enemy.AttackTarget.transform.position, this.transform.position);

        // In Range
        if (d < AttackRange) 
        {
            CurrentAttackTimer -= Time.deltaTime;
        }
        else
        {
            CurrentAttackTimer += Time.deltaTime;
            CurrentAttackTimer = Mathf.Clamp(CurrentAttackTimer, 0,AttackMaxCooldown);
        }

        if(CurrentAttackTimer < 0.0f) 
        {
            //Schedule attack at the time of next beat
            BeatObject nextBeat = Enemy.AIConductor.GetUpcomingBeat();
            nextBeat.OnBeatPassed += Attack;
            CurrentAttackTimer = AttackMaxCooldown;
        }
    }

    void Attack(BeatObject beat)
    {
        if(MeleeAttackSound != null)
        {
            MeleeAttackSound.Post(this.gameObject);
        }
        //Update mouse pointer
        if (this.gameObject.activeSelf == false)
            return;
        this.Enemy.Animator.SetTrigger("Attack");
        AttackAnimator.SetTrigger("AttackAnimation");

        //Attack should execute only once 
        //Remove from next beat
        beat.OnBeatPassed -= Attack;
    }
}
