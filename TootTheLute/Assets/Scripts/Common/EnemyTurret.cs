using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : MonoBehaviour
{

    [Header("Ranged Enemy Projectile Properties")]
     public EnemyProjectile Projectile;  // The prefab to spawn
    public int numberOfObjects = 10;  // Number of objects to spawn
    public float radius = 5f;         // Radius of the circle

    private SpriteRenderer SpriteRenderer;
    private Collider2D HomeTo;

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }



    public void SpawnObjectInCircularPattern()
    {
        HomeTo = FindObjectOfType<PlayerController2D>().gameObject.GetComponent<Collider2D>();
        //Collider2D nearestEnemyToHomeTo = GetNearestEnemyCollider();

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects; // Distribute objects evenly
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector3 spawnPosition = new Vector3(x, y, 0) + transform.position;
            EnemyProjectile currProj =Instantiate(Projectile, spawnPosition, Quaternion.identity);
            Vector2 dirToPlayer = (HomeTo.ClosestPoint(this.transform.position) - (Vector2)currProj.transform.position).normalized;
            currProj.Direction = dirToPlayer;
        }
    }




}
