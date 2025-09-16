using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EntityHealth : MonoBehaviour, IDataPersistence
{

    [SerializeField] public  float DamageTimer = 1f;
    [Range(0,1)]
    [SerializeField]public  float SlideSpeed = 0.5f;
    public int MaxHealth;// = 250;
    [HideInInspector] public int StartingHealth;
    [HideInInspector] public int CurrentHealth;
    public HealthBar HealthBar;
    private float _damageHealthSlideTimer;
    public Slider DamageSlider;

    void Awake()
    {
        //if (DataPersistenceManager._gameData.PlayerProgressionTracker != null)
        //{
        //    this.MaxHealth = (int)DataPersistenceManager._gameData.PlayerProgressionTracker.GetCurrentUpgrade(UpgradePathType.Health).UpgradedValue;
        //}
        //this.MaxHealth = 100;
       // this.StartingHealth = MaxHealth;
        Init();
    }

    void Init()
    {
        this.StartingHealth = MaxHealth;
        this.CurrentHealth = Helpers.ClampToRangeInt(StartingHealth, 0, MaxHealth);
        HealthBar.SetMaxHealth(MaxHealth);
        //HealthBar.SetHealth(CurrentHealth);
        _damageHealthSlideTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_damageHealthSlideTimer <= 0 && DamageSlider.value > (float)CurrentHealth / (float)MaxHealth)
        {
           // Debug.LogError("I AM SHRINKING");
            DamageSlider.value -= SlideSpeed * Time.deltaTime;
        }
        else if (_damageHealthSlideTimer > 0)
        {
            _damageHealthSlideTimer -= Time.deltaTime;
        }
        
            
    }

    public void Reset()
    {
        HealthBar.SetHealth(MaxHealth);
        DamageSlider.value = 1;
    }

    public void TakeDamage(int dmg)
    {

        this.CurrentHealth -= dmg;
        HealthBar.SetHealth(CurrentHealth);
        
        
        if (this.CurrentHealth<=0)
        {
            Die();
        }
        
        _damageHealthSlideTimer = DamageTimer;

    }

    void Die()
    {
        Destroy(this.gameObject);
        SceneManager.LoadScene("GameOverScene");
    }

    public void LoadData(GameData data)
    {
        this.MaxHealth = data.PlayerMaxHealth;
        this.CurrentHealth=this.MaxHealth;
        //HealthBar.SetHealth(this.MaxHealth);
        Init();
    }

    public void SaveData(ref GameData data)
    {
        var health = (int)data.PlayerProgressionTracker.GetCurrentUpgrade(UpgradePathType.Health).UpgradedValue;
        data.PlayerMaxHealth = health;
    }

    public IDataPersistence.Loaded OnLoaded { get; set; }
}
