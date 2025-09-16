using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableObject : BeatObjectBase
{
    [Header("Interaction Properties")]
    private int BeatsInCoooldown = 0;
    public bool IsActive = true;
    public int CooldownTimeInBeats;
    public Color ActiveColor;
    public Color InactiveColor;

    [Header("Pulsing Properties")]
    public float MinScale = 1f;
    public float MaxScale = 3f;
    public float TimeShrink = 0.5f;

    [Header("Ranged Projectile Properties")]
    public List<Collider2D> neighbors;
    public float detectionRadius = 5f;
     public HomingProjectile objectToSpawn;  // The prefab to spawn
    public int numberOfObjects = 10;  // Number of objects to spawn
    public float radius = 5f;         // Radius of the circle
    public LayerMask EnemyLayer;


    private SpriteRenderer SpriteRenderer;

    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        BeatTrackerUpdate();
        UpdateColorBasedOnState();
    }
    public void UpdateColorBasedOnState()
    {
        if (SpriteRenderer == null)
            return;
        else
        {
            if (IsActive)
                SpriteRenderer.color = ActiveColor;
            else 
                SpriteRenderer.color = InactiveColor;

        }

    }
    public void FixedUpdate()
    {
        //Timeshrink is in seconds
        float ratePerSecond = (MaxScale - MinScale) * (1/TimeShrink);
        float currentScale = this.transform.localScale.x;
        float scale = currentScale - ratePerSecond*Time.deltaTime;
        scale = Mathf.Clamp(scale,MinScale,MaxScale);   
        this.transform.localScale = new Vector3(scale, scale, scale);


        GetConductorFromScene();
    }
    //The plyaer would interact with this object by attacking it as of now
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            if(this.IsActive)
            {
                //Change the state
                IsActive = false;

                // Perform the action
                InteractOnPlayerAttack();

            }
        }
    }

    public  void InteractOnPlayerAttack()
    {
        //Spawn projectile in a circular patterns
        SpawnObjectInCircularPattern();
        
        return;


    }



    void SpawnObjectInCircularPattern()
    {
        Collider2D nearestEnemyToHomeTo = GetNearestEnemyCollider();

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects; // Distribute objects evenly
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector3 spawnPosition = new Vector3(x, y, 0) + transform.position;
            HomingProjectile homingProjectObject =Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

            homingProjectObject.Target = nearestEnemyToHomeTo;


            homingProjectObject.FallbackDirection = new Vector2(x,y);
        }
    }

    //These are the actions that occur on each beat
    // Not to be confused with actual interactions
    public override void InteractOnBeat()
    {
        //Cooldown

        if (this.IsActive == false)
        {
            this.BeatsInCoooldown++;
            if(BeatsInCoooldown >= CooldownTimeInBeats)
            {
                BeatsInCoooldown = 0; 
                //Activate 
                this.IsActive = true;
            }
        }
        
        this.transform.localScale = new Vector3(MaxScale, MaxScale, MaxScale);
    }


    public List<Collider2D> GetEnemiesInRadius()
    {
        Collider2D[] neighborColliders = Physics2D.OverlapCircleAll(
            this.gameObject.transform.position,
            this.detectionRadius,
            EnemyLayer);

        var colliderList = neighborColliders.Where(
            x =>
            x.isTrigger == false // Not a trigger
            && x.gameObject != this.gameObject // Not self
            ).ToList();

        return colliderList;

    }
    public Collider2D GetNearestEnemyCollider()
    {
        var listOfEnemiesInRadius = GetEnemiesInRadius();
        if(listOfEnemiesInRadius!= null && listOfEnemiesInRadius.Count > 0)
            return GetEnemiesInRadius().OrderBy(x => Vector2.Distance(
                this.gameObject.transform.position, x.transform.position)).First();
        else return null;
    }

}
