using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum UpgradePathType
{
    Health, Inventory, Smell
}

[System.Serializable]
public class PlayerUpgrade
{
    [SerializeField] public float Cost;
    [SerializeField] public string Description;
    [SerializeField] public float UpgradedValue;

    public PlayerUpgrade(float value=0,float cost=0, string desc="Base")
    {
        this.Cost = cost;
        this.Description = desc;
        this.UpgradedValue=value;
    }
}
[System.Serializable]
public class PlayerUpgradePath
{
    [SerializeField] public UpgradePathType UpgradePathType;
    [SerializeField] public List<PlayerUpgrade> Upgrades=new List<PlayerUpgrade>();

    public PlayerUpgradePath(UpgradePathType t)
    {
        this.UpgradePathType = t;
        this.Upgrades=new List<PlayerUpgrade>();
        this.Upgrades.Add(new PlayerUpgrade());
    }
}

[CreateAssetMenu(fileName = "New Player Ugrades", menuName = "Player Stat Progression")]
public class PlayerUpgradesStats : ScriptableObject
{
    public List<PlayerUpgradePath> PlayerUpgradePaths = new List<PlayerUpgradePath>()
    {
        new PlayerUpgradePath(UpgradePathType.Health), 
        new PlayerUpgradePath(UpgradePathType.Inventory),
        new PlayerUpgradePath(UpgradePathType.Smell)
    };

    public PlayerUpgradePath Get(UpgradePathType type)
    {
        return this.PlayerUpgradePaths.Find(x => x.UpgradePathType == type);
    }


}
