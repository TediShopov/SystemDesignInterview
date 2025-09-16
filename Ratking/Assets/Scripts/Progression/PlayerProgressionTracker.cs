using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class PlayerProgressionTracker
{
  

    private PlayerUpgradesStats _playerPrgoressionStats;

    public void SetPlayerProgressionStats(PlayerUpgradesStats stats)
    {
        this._playerPrgoressionStats = stats;
    }

    public PlayerProgressionTracker(PlayerUpgradesStats playerPrgoression)
    {
        this._playerPrgoressionStats = playerPrgoression;
        this.EarnedUpgradeIndexes.Add(UpgradePathType.Health, 0);
        this.EarnedUpgradeIndexes.Add(UpgradePathType.Inventory, 0);
        this.EarnedUpgradeIndexes.Add(UpgradePathType.Smell, 0);
    }


    public SerializedDictionary<UpgradePathType, int> EarnedUpgradeIndexes=new SerializedDictionary<UpgradePathType, int>();
    public PlayerUpgrade GetCurrentUpgrade(UpgradePathType type)
    {
        return _playerPrgoressionStats.PlayerUpgradePaths.Find(x => x.UpgradePathType == type).Upgrades[EarnedUpgradeIndexes[type]];
    }

    public PlayerUpgrade GetNextUpgrade(UpgradePathType type)
    {
        if (EarnedUpgradeIndexes[type] + 1 <_playerPrgoressionStats.PlayerUpgradePaths.Find(x => x.UpgradePathType == type).Upgrades.Count)
        {
            return _playerPrgoressionStats.PlayerUpgradePaths.Find(x => x.UpgradePathType == type).Upgrades[EarnedUpgradeIndexes[type] + 1];
        }

        return null;

    }

    public bool CanBuyNextUpgrade(UpgradePathType type)
    {
        var nextUpgrade = this.GetNextUpgrade(type);
        if (nextUpgrade != null)
        {
            return nextUpgrade.Cost <= LevelData.Inventory.BaseGoldValue;
        }
        return false;
    }

    public void BuyUpgrade(UpgradePathType type)
    {
        var nextUpgrade = this.GetNextUpgrade(type);
        if (CanBuyNextUpgrade(type))
        {
            //LevelData.Inventory.BaseGoldValue-=(int)nextUpgrade.Cost;
            LevelData.Inventory.SubtractFromBaseGold((int)nextUpgrade.Cost);
            EarnedUpgradeIndexes[type]++;
        }
    }

    public void ReloadData()
    {
        DataPersistenceManager.Instance.SaveGame();
        DataPersistenceManager.Instance.LoadGame();
    }


}
