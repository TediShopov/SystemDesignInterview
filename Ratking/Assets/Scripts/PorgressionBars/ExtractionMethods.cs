using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ExtractionMethods : MonoBehaviour
{
    public static event EventHandler onUpdateZone;

    private EntityHealth HealthOfPlayer;
   

    public GameObject ExtractionPoint;

    [Header("Buttons")] 
    [SerializeField] public Button HealButton;
    [SerializeField] public Button DumpButton;
    [SerializeField] public Button ExtractButton;


    // Start is called before the first frame update
    void Start()
    {
        
        this.HealButton.onClick.AddListener(HealToMax);
        this.DumpButton.onClick.AddListener(DumpInventory);
        this.ExtractButton.onClick.AddListener(Extract);
    }

    // Update is called once per frame
    void Update()
    {

    }

   

    public bool IsHealAvailable => this.HealthOfPlayer.CurrentHealth != HealthOfPlayer.MaxHealth;
    public bool IsDumpAvailable => LevelData.Inventory.Items.Count != 0;
    public bool IsExtractAvailable => !IsHealAvailable || !IsDumpAvailable || LevelData.Inventory.TotalGoldValue != 0;

    void OnEnable()
    {
        if (LevelData.PlayerObject != null)
        {
            var entityHealth = LevelData.PlayerObject.GetComponent<EntityHealth>();
            if (entityHealth != null)
            {
                HealthOfPlayer = entityHealth;
            }
        }
        this.HealButton.interactable = IsHealAvailable;
        this.DumpButton.interactable = IsDumpAvailable;
        this.ExtractButton.interactable = IsExtractAvailable;

    }

    void updateZone()
    {
        if (onUpdateZone != null)
            onUpdateZone(this, EventArgs.Empty);
    }

    public void HealToMax()
    {
        if (this.isActiveAndEnabled)
        {
            HealthOfPlayer.Reset();
            Destroy(ExtractionPoint);
            updateZone();
            this.gameObject.SetActive(false);

        }
    }

    public void DumpInventory()
    {
        if (this.isActiveAndEnabled)
        {
            int totalValueOfInventory = LevelData.Inventory.TotalGoldValue;
            LevelData.Inventory.DumpItems();
            //this.Inventory.Clear();
            //this.Inventory.TotalGoldValue = totalValueOfInventory;
            updateZone();
            Destroy(ExtractionPoint);
            this.gameObject.SetActive(false);

        }
    }

    public void Extract()
    {
       if (this.isActiveAndEnabled)
       {
           PlayerPrefs.SetString("TotalGoldValue", LevelData.Inventory.TotalGoldValue.ToString());
           LevelData.CraftingResourceHolder.AddCraftableItemsFromInventory();
           DataPersistenceManager.Instance.SaveGame();
           SceneManager.LoadScene("VictoryScene");
            Destroy(ExtractionPoint);
            this.gameObject.SetActive(false);

       }
    }
}
