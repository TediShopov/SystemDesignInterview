using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public InventorySaveData InventorySaveData;
    public PlayerProgressionTracker PlayerProgressionTracker;
    public CraftingResourceData CraftingResourceData;
    public UmlockedRecipesData UmlockedRecipesData;
    public ToolboxData ToolboxData;
    public int PlayerMaxHealth = 100;
    public int SmellRangeTiles = 5;
    public GameData(PlayerUpgradesStats playerUpgrades)
    {
        if (playerUpgrades == null || playerUpgrades.PlayerUpgradePaths==null || playerUpgrades.PlayerUpgradePaths.Count==0)
        {
            Debug.LogError("Player Upgrades Stats is not initialized correctly.");
        }

        this.PlayerProgressionTracker = new PlayerProgressionTracker(playerUpgrades);
        this.InventorySaveData = new InventorySaveData(playerUpgrades);
        this.UmlockedRecipesData = new UmlockedRecipesData();
        this.CraftingResourceData = new CraftingResourceData();
        this.ToolboxData = new ToolboxData();
        this.SmellRangeTiles = (int)playerUpgrades.Get(UpgradePathType.Smell).Upgrades[0].UpgradedValue;
        this.PlayerMaxHealth = (int)playerUpgrades.Get(UpgradePathType.Health).Upgrades[0].UpgradedValue;

    }

    public override string ToString()
    {
        string logValues = string.Empty;
        logValues += "Game Persistent Data \n";
        if (PlayerProgressionTracker != null)
        {
            logValues += $"Smell Upgrade Index = {PlayerProgressionTracker.EarnedUpgradeIndexes[UpgradePathType.Smell]} \n";
            logValues += $"Inventory Upgrade Index = {PlayerProgressionTracker.EarnedUpgradeIndexes[UpgradePathType.Inventory]} \n";
            logValues += $"Health Upgrade Index = {PlayerProgressionTracker.EarnedUpgradeIndexes[UpgradePathType.Health]} \n";
        }
        else
        {
            logValues += $"Error when trying to read Player Upgrades";
        }

        if (InventorySaveData != null)
        {
            logValues += $"Base Gold = {InventorySaveData.BaseGold} \n";
            logValues += $"Inventory X Dimension = {InventorySaveData.GridDimensionX} \n";
            logValues += $"Inventory Y Dimension = {InventorySaveData.GridDimensionY} \n";
        }
        else
        {
            logValues += $"Error when trying to read Inventory Save Data";
        }

        //if (CraftingResourceData != null)
        //{
        //    logValues += $"Crafting Resource: Iron = {CraftingResourceData.Resources[CraftingReosurceType.Iron]} \n";
        //    logValues += $"Crafting Resource: Food = {CraftingResourceData.Resources[CraftingReosurceType.Food]} \n";
        //    logValues += $"Crafting Resource: String = {CraftingResourceData.Resources[CraftingReosurceType.String]} \n";
        //}
        //else
        //{
        //    logValues += $"Error when trying to read Crafting Resource Data";
        //}

        logValues += $"Smell Range Tiles = {SmellRangeTiles}";
        logValues += $"Player Max Health = {PlayerMaxHealth}";

        return logValues;
    }


}
