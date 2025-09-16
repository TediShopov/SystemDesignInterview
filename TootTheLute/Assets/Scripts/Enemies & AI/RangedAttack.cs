using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;


//Ranged attack scheduled on the beat track
[RequireComponent(typeof(Enemy))]
public class RangedAttack : MonoBehaviour
{
    public EnemyProjectile projectilePrefab;
    public Enemy Enemy;
    public float OffsetFromEnemy; 
    public float fireRateInSeconds = 1f;
    public float immobileOnShotFor = 1.0f;

    public event Action OnShoot; // Event for shooting

    [Header("Sound")]
    public AK.Wwise.Event RangedAttackSound;


    void Start()
    {
        Enemy = GetComponent<Enemy>();
        InvokeRepeating(nameof(ScheduleShotOnNextBeat), fireRateInSeconds, fireRateInSeconds);

    }
    void ScheduleShotOnNextBeat()
    {
        var beat = this.Enemy.AIConductor.GetUpcomingBeat();
        beat.OnBeatPassed += Shoot;
    }

    private void Shoot(BeatObject beat)
    {
        if (Enemy is null) return;
        var health = Enemy.GetComponent<Health>();
        if (health is null) return;

        if (health.GetCurrentHealth() <= 0)
        {
            return ;
        }


        //Only if not already immobilized by the shot
        if (this.Enemy.IsMovementAllowed == true)
        {
            if (this.gameObject.activeSelf == false)
                return;
            OnShoot?.Invoke(); // Trigger event

            if (RangedAttackSound != null)
            {
                RangedAttackSound.Post(this.gameObject);
            }
            this.Enemy.Animator.SetTrigger("Attack");

            Vector2 spawnPoint = (Vector2)this.transform.position + Enemy.GetDirectionToTarget() * OffsetFromEnemy;

            if (projectilePrefab)
            {
                var spawnedProjectile = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity);
                spawnedProjectile.Direction = Enemy.GetDirectionToTarget();
            }
            this.Enemy.StartCoroutine(Enemy.ImmobilizeFor(immobileOnShotFor));

        }
    }
}
