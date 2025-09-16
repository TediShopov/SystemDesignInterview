using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterableMeleeObject : BeatObjectBase
{
    [Header("Interaction Properties")]
    private int BeatsInCoooldown = 0;
    public bool IsActive = true;
    public int CooldownTimeInBeats;
    public float AttackDurationInSeconds = 0.15f;
    public float AttackActiveFor = 0;
    public Color ActiveColor;
    public Color InactiveColor;

    [Header("Pulsing Properties")]
    public float MinScale = 1f;
    public float MaxScale = 3f;
    public float TimeShrink = 0.5f;

    public GameObject AttackObjectToSpawn;
    public GameObject SpawnedAttack = null;

    private SpriteRenderer SpriteRenderer;
    private Collider2D collider;

    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
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
                SpriteRenderer.color = new Color(ActiveColor.r, ActiveColor.g,ActiveColor.b,SpriteRenderer.color.a);
            else 
                SpriteRenderer.color = new Color(InactiveColor.r, InactiveColor.g,InactiveColor.b,SpriteRenderer.color.a);

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
        if(SpawnedAttack != null)
        {
            AttackActiveFor += Time.deltaTime;
            if(AttackActiveFor > AttackDurationInSeconds)
            {
                Destroy(SpawnedAttack);
                AttackActiveFor = 0;
            }

        }

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
        SpawnObjectInDirection();
        
        return;


    }




    public float AttackOffset = 2.0f;
    void SpawnObjectInDirection()
    {
        //Find the player
        var player = FindObjectOfType<PlayerController2D>();

        Vector2 rbPosition = collider.ClosestPoint(player.gameObject.transform.position);
        Debug.Log($"RB Position {rbPosition}");

        //Find the opposite direction
        Vector2 directon = (rbPosition - (Vector2)player.gameObject.transform.position).normalized;
        //Apply offset
        Vector2 spawnPosition =rbPosition + (directon * AttackOffset);

        //Place position of the spawned attack

         SpawnedAttack =Instantiate(AttackObjectToSpawn, spawnPosition, Quaternion.identity);
        AttackActiveFor = 0;

        //Figure out the appropriate rotation in the XZ plane
        float angle = Mathf.Atan2(directon.y, directon.x) * Mathf.Rad2Deg;
        //TODO figure out a vector that we can rotate by to achieve the correct transformation to draw
        // rectangle on the isometric projectoin
		//SpawnedAttack.transform.rotation = Quaternion.AngleAxis(angle, (Vector3.forward + new Vector3(0,61,0)).normalized);
		SpawnedAttack.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		SpawnedAttack.transform.rotation = Quaternion.Euler(new Vector3 (
            -61,
            SpawnedAttack.transform.rotation.eulerAngles.y,
            SpawnedAttack.transform.rotation.eulerAngles.z
            ))
            ;



    }
    private void OnDrawGizmosSelected()
    {

        //Find the player
        var player = FindObjectOfType<PlayerController2D>();
        Vector2 rbPosition = collider.ClosestPoint(player.gameObject.transform.position);

        //Find the opposite direction
        Vector2 directon = (rbPosition - (Vector2)player.gameObject.transform.position).normalized;
        //Apply offset
        Vector2 spawnPosition =rbPosition + (directon * AttackOffset);
        Gizmos.DrawRay(spawnPosition, directon*5);
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


}
