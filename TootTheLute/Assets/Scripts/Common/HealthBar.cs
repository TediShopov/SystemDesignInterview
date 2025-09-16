using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider; // Reference to the slider UI component
    [SerializeField] private Health enemyHealth; // Reference to the Health script

    private void Start()
    {
        // Ensure the slider matches the initial health
        healthSlider.maxValue = enemyHealth.GetMaxHealth();
        healthSlider.value = enemyHealth.GetCurrentHealth();

        // Subscribe to the health change event
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged += UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(float currentHealth, float MaxHealth)
    {
        healthSlider.value = currentHealth;
        if(healthSlider.value <= 0) 
        {
            this.gameObject.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        //Unsubsrive from event
        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }





}

