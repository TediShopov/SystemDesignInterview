using System.Collections.Generic;
using UnityEngine;

public class InteractableReplenishingHealing : BeatObjectBase
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
    public GameObject objectToSpawn;              // The prefab to spawn
    public List<GameObject> ReadyToUseHealingCharges;   // This would be the healing projectile BEFORE they are launched

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
                SpriteRenderer.color = ActiveColor;
            else 
                SpriteRenderer.color = InactiveColor;

        }

    }
    public void FixedUpdate()
    {
        ApplyShrinking();
        //GetConductorFromScene();
    }


    //Shrinks from MaxScale (On Beat) to the initial scale of the object
    private void ApplyShrinking()
    {
        float ratePerSecond = (MaxScale - MinScale) * (1 / TimeShrink);
        float currentScale = this.transform.localScale.x;
        float scale = currentScale - ratePerSecond * Time.fixedDeltaTime;
        scale = Mathf.Clamp(scale, MinScale, MaxScale);
        this.transform.localScale = new Vector3(scale, scale, scale);
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
        Debug.Log("Started Interaction On Player Attack");
        var foundPlayerObject = FindObjectOfType<PlayerController2D>().gameObject;

        foundPlayerObject.GetComponent<Health>().DirectlyIncrement(-10 * ReadyToUseHealingCharges.Count);
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
        Vector3 spawnPosition = new Vector3(x, y, 0) + transform.position;
        GameObject healingChargeObject =Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
        ReadyToUseHealingCharges.Add(healingChargeObject);
    }

    //These are the actions that occur on each beat
    // Not to be confused with actual interactions
    public override void InteractOnBeat()
    {
        //Cooldown
        if (ReadyToUseHealingCharges.Count < maxNumberOfHealingObjects)
        {
            SpawnHealingChargeInNextPosition();
        }
        
        this.transform.localScale = new Vector3(MaxScale, MaxScale, MaxScale);
    }



}
