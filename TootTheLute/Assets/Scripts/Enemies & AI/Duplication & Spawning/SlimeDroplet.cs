using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeDroplet : MonoBehaviour
{
    private Rigidbody2D Rigidbody;

    public float TimeUntilSpawn;
    public float TimeToSpawnAfter;

    public Vector2 InitialImpulse;
    public Vector2 ScaleRange;
    public Vector2 ShadowScaleRange;
    public Vector2 YRange;

    public Enemy Emitter;
    public GameObject EnemyPrefab;
    public SpriteRenderer ProjectileSprite;
    public SpriteRenderer ShadowSprite;


    public void Start()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        this.TimeUntilSpawn = this.TimeToSpawnAfter;
    }
    public void OnDestroy()
    {
        this.Emitter.Room.SpawnerInPlay.RemoveAt(this.Emitter.Room.SpawnerInPlay.Count - 1);
    }
    public void Update()
    {
        this.TimeUntilSpawn -= Time.deltaTime;
        if (this.TimeUntilSpawn <= 0)
        {
            //Do something
            var spawnedEnemy = Instantiate(EnemyPrefab, this.gameObject.transform.position, Quaternion.identity, null);
            if (spawnedEnemy != null)
            {
                this.Emitter.Room.Enemies.Add(spawnedEnemy);
                this.Emitter.Room.AddEnemyToRoom(spawnedEnemy);
            }
            Destroy(this.gameObject);
        }

        //Convert time to scale
        float relTime = 1 - ((TimeUntilSpawn) / TimeToSpawnAfter);
        if (relTime < 0.5f)
        {
            float scaleDim = Remap(relTime, 0, 0.5f, ScaleRange.x, ScaleRange.y);
            float shadowScaleDim = Remap(relTime, 0, 0.5f, ShadowScaleRange.x, ShadowScaleRange.y);
            float y = Remap(relTime, 0, 0.5f, YRange.x, YRange.y);
            this.ProjectileSprite.transform.localScale = new Vector3(scaleDim, scaleDim, scaleDim);
            this.ProjectileSprite.transform.localPosition = new Vector3(0, y, 0);
            this.ShadowSprite.transform.localScale = new Vector3(shadowScaleDim, shadowScaleDim, shadowScaleDim);
        }
        else
        {
            float scaleDim = Remap(relTime, 0.5f, 1, ScaleRange.y, ScaleRange.x);
            float shadowScaleDim = Remap(relTime, 0.5f, 1, ShadowScaleRange.y, ShadowScaleRange.x);
            float y = Remap(relTime, 0.5f, 1, YRange.y, YRange.x);
            this.ProjectileSprite.transform.localScale = new Vector3(scaleDim, scaleDim, scaleDim);
            this.ProjectileSprite.transform.localPosition = new Vector3(0, y, 0);
            this.ShadowSprite.transform.localScale = new Vector3(shadowScaleDim, shadowScaleDim, shadowScaleDim);
        }
    }
    public static float Remap(float value, float sourceMin, float sourceMax, float targetMin, float targetMax)
    {
        return targetMin + ((value - sourceMin) / (sourceMax - sourceMin)) * (targetMax - targetMin);
    }
    public void FixedUpdate()
    {
        Rigidbody.AddForce(InitialImpulse, ForceMode2D.Force);
    }
}