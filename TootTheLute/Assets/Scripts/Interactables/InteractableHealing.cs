using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableHealing : BeatObjectBase
{
    [Header("Interaction Properties")]
    public bool IsActive = true;
    private int ActiveBeatsSinceLastUse = 0;
    public int CooldownTimeInBeats;
    public Color ActiveColor;
    public Color InactiveColor;

    [Header("Pulsing Properties")]
    public float MinScale = 1f;
    public float MaxScale = 3f;
    public float TimeShrink = 0.5f;

    [Header("Ranged Projectile Properties")]
    public int maxNumberOfHealingObjects = 10;          // Number of objects to spawn
    public float spawnRadius = 5f;                      // Radius of the circle
    public Transform spawnAroundPoint;              
    public GameObject objectToSpawn;              // The prefab to spawn
    public List<GameObject> ReadyToUseHealingCharges;   // This would be the healing projectile BEFORE they are launched
    public float HealingStrength = 10;

    private SpriteRenderer SpriteRenderer;

    private void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        ReadyToUseHealingCharges = new List<GameObject>();
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
    }
    //The plyaer would interact with this object by attacking it as of now
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Tigger Entry Detected");
        if(collision.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            Debug.Log("Attack Recognized");
            InteractOnPlayerAttack();
        }
    }

    public  void InteractOnPlayerAttack()
    {
        if (this.IsActive == false) return;
        this.IsActive = false;
        this.ActiveBeatsSinceLastUse = 0;
        Debug.Log("Started Interaction On Player Attack");
        var foundPlayerObject = FindObjectOfType<PlayerController2D>().gameObject;

        foundPlayerObject.GetComponent<Health>().DirectlyIncrement(-this.HealingStrength * ReadyToUseHealingCharges.Count);
        foreach (var item in ReadyToUseHealingCharges)
        {
            Destroy(item.gameObject);
        }
        ReadyToUseHealingCharges.Clear();
        return;


    }



    void SpawnHealingChargeInNextPosition()
    {
        int spawnIndex = ReadyToUseHealingCharges.Count;
        float angle = spawnIndex * Mathf.PI * 2 / maxNumberOfHealingObjects; // Distribute objects evenly
        float x = Mathf.Cos(angle) * spawnRadius;
        float y = Mathf.Sin(angle) * spawnRadius;
        Transform center = this.transform;
        if(spawnAroundPoint != null)
            center = this.spawnAroundPoint;
        Vector3 spawnPosition = new Vector3(x, y, 0) + center.position;
        GameObject healingChargeObject =Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        ReadyToUseHealingCharges.Add(healingChargeObject);
    }

    //These are the actions that occur on each beat
    // Not to be confused with actual interactions
    public override void InteractOnBeat()
    {
        //Check if it is active 
        if(!IsActive) 
        {
            ActiveBeatsSinceLastUse++;
            if(ActiveBeatsSinceLastUse >= CooldownTimeInBeats)
            {
                this.IsActive = true;

            }

        }
        else
        {
            //Cooldown
            if (ReadyToUseHealingCharges.Count < maxNumberOfHealingObjects)
            {
                SpawnHealingChargeInNextPosition();
            }

            this.transform.localScale = new Vector3(MaxScale, MaxScale, MaxScale);

        }


    }



}
