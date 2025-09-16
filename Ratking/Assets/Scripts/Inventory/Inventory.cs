using System;
using System.Collections.Generic;
using System.Linq;
using FunkyCode.EventHandling;
using Mono.Cecil.Cil;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public interface IPlayerInventory: IDataPersistence
{
    int BaseGoldValue { get; }
    int TotalGoldValue {get; }
    Vector2Int GridDimensions { get; set; }

    public List<InventoryObject> Items { get; set; }
    public List<InventoryObject> DumpedItems { get;   }


    public bool IsInGrid(Vector3Int cell);
    bool CanPlaceItem(InventoryObject item);
    void PlaceItem(InventoryObject item);
    //void PickItemInHand(InventoryObject item);
    InventoryObject GetItem(Vector3Int cell);
    InventoryObject PickItem(Vector3Int cell);
    void DumpItems();
    void ClearAll();

    public delegate void InventoryUpdatedAction();
    public delegate void UniqueItemAdded(InventoryObject item);

    public event UniqueItemAdded OnUniqueItemAdded;
    public event InventoryUpdatedAction OnInventoryUpdated;




}
[System.Serializable]
public class InventorySaveData
{
    public int BaseGold;
    public int GridDimensionX;
    public int GridDimensionY;

    public InventorySaveData(PlayerUpgradesStats stats)
    {
        BaseGold = 0;
        if (stats!=null)
        {
            GridDimensionX = (int)stats.Get(UpgradePathType.Inventory).Upgrades[0].UpgradedValue;
            GridDimensionY = (int)stats.Get(UpgradePathType.Inventory).Upgrades[0].UpgradedValue;
        }
      
    }
    //public List<InventoryObject> Items=new List<InventoryObject>();
    //public List<InventoryObject> DumpedItems=new List<InventoryObject>();
}

public class Inventory : MonoBehaviour,IPlayerInventory
{
    [SerializeField] public int BaseGoldValue { get;  set; }
    public int TotalGoldValue => BaseGoldValue + InventoryItemValue;

    [HideInInspector] public List<InventoryObject> Items { get; set; } = new List<InventoryObject>();
    [HideInInspector] public List<InventoryObject> DumpedItems { get; private set; } = new List<InventoryObject>();

    [HideInInspector] private Vector2Int _gridDimensions;

    public int InventoryItemValue => this.Items.Sum(x=>x.ItemData.GoldValue);
    public Vector2Int GridDimensions
    {
        get { return _gridDimensions; }
        set { _gridDimensions = value; }

        
    }

    //public Inventory(int dimX, int dimY)
    //{
    //    //dim = (int)LevelData.PlayerProgressionTracker.GetCurrentUpgrade(UpgradePathType.Inventory)
    //    //    .UpgradedValue;
    //    this.GridDimensions=new Vector2Int(dimX, dimY);
    //}



    void Awake()
    {
        int dim = 0;
        this.GridDimensions = new Vector2Int(dim, dim);
    }

    public void ItemsToBaseGold()
    {
        this.BaseGoldValue += this.InventoryItemValue;
    }



    //[HideInInspector] public List<InventoryObject> Items;
    private List<int> UniqueCollectablesHashes = new List<int>();

    public InventoryObject IsOccuiedByotherItem(Vector3Int cell)
    {
        foreach (InventoryObject item in Items)
        {
            if (item.OccupiedCells.Contains(cell)) return item;
        }

        return null;
    }


    public bool IsInGrid(Vector3Int cell)
    {
        return (-GridDimensions.x/2.0f <= cell.x && cell.x < GridDimensions.x/2.0f &&
                -GridDimensions.y / 2.0f <= cell.y && cell.y < GridDimensions.y / 2.0f);
    }
    public bool CanPlaceItem(InventoryObject item)
    {
        var isValidPosition = true;
        
        //ItemToPlace.UpdatePosition(cooridinatePointedByMouse, InventoryGrid);
        var cellsInItem = new List<Vector3Int>(item.OccupiedCells);
        isValidPosition = cellsInItem.All(c => IsInGrid(c) && IsOccuiedByotherItem(c) == null);
        cellsInItem = cellsInItem.Where(c => IsInGrid(c) && IsOccuiedByotherItem(c) == null).ToList();
        return isValidPosition;
    }

    public void PlaceItem(InventoryObject item)
    {
        if (CanPlaceItem(item))
        {
            this.Items.Add(item);
            //this.TotalGoldValue += item.ItemData.GoldValue;

            if (OnInventoryUpdated != null)
                OnInventoryUpdated.Invoke();

            if (!UniqueCollectablesHashes.Contains(item.UniqueCollectibleHash))
            {
                UniqueCollectablesHashes.Add(item.UniqueCollectibleHash);
                if (OnUniqueItemAdded != null)
                    OnUniqueItemAdded(item);

            }
        }
       
    }

    

    public InventoryObject GetItem(Vector3Int cell)
    {
        return this.Items.FirstOrDefault(x => x.OccupiedCells.Contains(cell));
    }

    public InventoryObject PickItem(Vector3Int cell)
    {
        var item = GetItem(cell);
        if (item != null)
        {
            this.Items.Remove(item);
           // this.TotalGoldValue -= item.ItemData.GoldValue;

            if (OnInventoryUpdated != null)
                OnInventoryUpdated.Invoke();
        }
        return item;
    }


    public void DumpItems()
    {
        foreach (var inventoryObject in this.Items)
        {
           // this.TotalGoldValue -= inventoryObject.ItemData.GoldValue;
            this.BaseGoldValue += inventoryObject.ItemData.GoldValue;
            this.DumpedItems.Add(inventoryObject);

            //TODO destroy visual part of object in GirdVisualizer
            GameObject.Destroy(inventoryObject.gameObject);

        }
        this.Items.Clear();

        if (OnInventoryUpdated != null) 
                OnInventoryUpdated.Invoke();
    }

    public void ClearAll()
    {
        this.Items.Clear();

        if (OnInventoryUpdated != null)
            OnInventoryUpdated.Invoke();
    }

    public void SubtractFromBaseGold(int amount)
    {
        this.BaseGoldValue -= amount;
        if (OnInventoryUpdated != null)
            OnInventoryUpdated.Invoke();
    }

    public event IPlayerInventory.UniqueItemAdded OnUniqueItemAdded;
    public event IPlayerInventory.InventoryUpdatedAction OnInventoryUpdated;
    public void LoadData(GameData data)
    {
        //this.Items = data.InventorySaveData.Items;
        //this.DumpedItems = data.InventorySaveData.DumpedItems;
        this.BaseGoldValue = data.InventorySaveData.BaseGold;
        this.GridDimensions =
            new Vector2Int(data.InventorySaveData.GridDimensionX, data.InventorySaveData.GridDimensionY);

        if (OnInventoryUpdated != null)
            OnInventoryUpdated.Invoke();
    }

    public void SaveData(ref GameData data)
    {
        //data.InventorySaveData.Items = this.Items;
        //data.InventorySaveData.DumpedItems = this.DumpedItems;

        var newDimension= (int) data.PlayerProgressionTracker.GetCurrentUpgrade(UpgradePathType.Inventory).UpgradedValue;


        data.InventorySaveData.BaseGold = this.BaseGoldValue;
        data.InventorySaveData.GridDimensionX = newDimension;
        data.InventorySaveData.GridDimensionY = newDimension;

    }

    public IDataPersistence.Loaded OnLoaded { get; set; }
}
