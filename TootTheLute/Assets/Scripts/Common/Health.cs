
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Health : MonoBehaviour
{
    public AttackData LastAttack  { get; set; }
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Death Settings")]
    public float DeathTimeBeforeTermination = 1;

    public delegate void HealthChanged(float current, float max);
    public delegate void Death(GameObject gameObject);
    public event HealthChanged OnHealthChanged; // Event to notify health changes
    public event Death OnDeath; // Event to notify when health is reduced below zero


    public bool IsInvulnerable = false;
    public bool CanBeDamaged => IsInvulnerable == false;


    private Coroutine InvulnarabilityCoroutine;



    public void SetInvulnarability(float seconds )
    {
        if(InvulnarabilityCoroutine == null)
        {
            InvulnarabilityCoroutine = StartCoroutine(SetInvulnarabilityFor(seconds));
        }
    }
    IEnumerator SetInvulnarabilityFor(float seconds)
    {
        IsInvulnerable = true;
        yield return new WaitForSeconds(seconds);
        IsInvulnerable = false;
        InvulnarabilityCoroutine = null;

    }



    private void Awake()
    {
        currentHealth = maxHealth;
    }
    private void Update()
    {
        //Debug Take Damage when J is pressed
        if(Input.GetKeyDown(KeyCode.J)) 
        {
            Debug.Log("J debug is pressed");
            //TakeDamage(25);
        }
    }

    //Getter for the health values
    public float GetMaxHealth() => maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public float GetHealthPercentage()
    {
        return currentHealth/maxHealth;
    }

    public bool TakeDamage(AttackData data)
    {
        if (CanBeDamaged == false) return false;
        LastAttack = data;
        // Reduce health
        currentHealth -= data.Damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify listeners about the health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);


        // Check if enemy is dead
        if (currentHealth <= 0)
        {
            Die(DeathTimeBeforeTermination);
        }
        return true;
    }

    public void DirectlyIncrement(float damage)
    {
        // Reduce health
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify listeners about the health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);


        // Check if enemy is dead
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private bool HasDiedOnce=false; 

    private void Die(float waitDestroy=0.0f)
    {
        if (!HasDiedOnce) 
        {
            OnDeath?.Invoke(this.gameObject);
            Debug.Log("Enemy died!");
            Destroy(gameObject, waitDestroy);
            HasDiedOnce = true;

        }
        // Add death logic here (e.g., destroy game object, play animation)
    }
}
