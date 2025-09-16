using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{

    private const int _HIGH_DMG_THRESHOLD = 15;

    public int MaxHealth = 100;
    public int MinHealth = 0;
    public int CurrentHealth;


    public bool IsEnemy;
    //public int CurrentHealth { get;  private set; }

    private void Awake()
    {
        CurrentHealth = MaxHealth;
    }

    public void SetValues(HealthScript healthScript) 
    {
        this.MaxHealth = healthScript.MaxHealth;
        this.MinHealth = healthScript.MinHealth;
        this.CurrentHealth = healthScript.CurrentHealth;
        this.IsEnemy = healthScript.IsEnemy;

    }

    public void TakeDamage(int dmgAmount) 
    {

        FighterController Fighter = this.gameObject.GetComponent<FighterController>();
        if (Fighter.State.isBlocking)
        {
            dmgAmount = 0;
            Debug.LogError("Blocked");
            return;
        }
        this.CurrentHealth -= dmgAmount;
        if (this.CurrentHealth<0)
        {
            this.CurrentHealth = 0;
            Fighter.SetDying(true);
        }

        if (dmgAmount > _HIGH_DMG_THRESHOLD)
        {
            Fighter.SetDamaged(false);
        }
        else 
        {
            Fighter.SetDamaged(true);
        }
    }

    
  
    // Update is called once per frame
    void Update()
    {
      

        float fillValue = (float)CurrentHealth / (float)MaxHealth;
    }
}
