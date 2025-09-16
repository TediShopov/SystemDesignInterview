
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeVisualizer : MonoBehaviour
{
    [SerializeField] private PlayerUpgradesStats PlayerProgressionStats;
    [SerializeField] private UpgradePathType UpgradePathType;
    [SerializeField] private TMP_Text CostLabel;
    [SerializeField] private TMP_Text EffectLabel;
    [SerializeField] private TMP_Text NumberOfUpgradeLabel;
    [SerializeField] private Button Button;
   
    void Start()
    {
        var playerProgressionTracker = DataPersistenceManager._gameData.PlayerProgressionTracker;
        VisualizeData(DataPersistenceManager._gameData);
        if (Button)
        {
            Button.onClick.AddListener(() =>
            {
                Debug.Log("Got Upgrade");
                playerProgressionTracker.BuyUpgrade(UpgradePathType);
                //playerProgressionTracker.ReloadData();
                VisualizeData(DataPersistenceManager._gameData);
            });
        }
    }

    public void VisualizeData(GameData data)
    {
        var playerProgressionTracker = data.PlayerProgressionTracker;
        if (playerProgressionTracker != null)
        {

            var currUpgrade = playerProgressionTracker.GetNextUpgrade(UpgradePathType);
            int currentIndexOfUpgrade = playerProgressionTracker.EarnedUpgradeIndexes[UpgradePathType]+1;
            int numOfUpgrades = PlayerProgressionStats.PlayerUpgradePaths.Find(x => x.UpgradePathType == UpgradePathType).Upgrades.Count;
            if (CostLabel)
            {
                if (currUpgrade==null)
                {
                    CostLabel.text = "Max";
                    EffectLabel.text = "";
                    NumberOfUpgradeLabel.text = $"{numOfUpgrades}/{numOfUpgrades-1}";
                }
                else
                {
                    CostLabel.text = currUpgrade.Cost.ToString();
                    EffectLabel.text = currUpgrade.Description;
                    NumberOfUpgradeLabel.text = $"{currentIndexOfUpgrade}/{numOfUpgrades-1}";
                }
             
            }
        }
    }

 
    public IDataPersistence.Loaded OnLoaded { get; set; }
}
