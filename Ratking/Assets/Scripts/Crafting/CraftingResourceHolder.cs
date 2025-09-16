using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[System.Serializable]
public class CraftingResourceData
{
    public SerializedDictionary<CraftingReosurceType, int> Resources=new SerializedDictionary<CraftingReosurceType, int>();

    public CraftingResourceData()
    {
        //this.Resources = new SerializedDictionary<CraftingReosurceType, int>();
        //Default initiatilzation of resources
        //if (!this.Resources.ContainsKey(CraftingReosurceType.Iron))
        //{
        //    this.Resources.Add(CraftingReosurceType.Iron, 0);
        //}

        //if (!this.Resources.ContainsKey(CraftingReosurceType.String))
        //{
        //    this.Resources.Add(CraftingReosurceType.String, 0);
        //}
        //if (!this.Resources.ContainsKey(CraftingReosurceType.Food))
        //{
        //    this.Resources.Add(CraftingReosurceType.Food, 0);
        //}



        //this.Resources.Add(CraftingReosurceType.Iron, 0);
        //this.Resources.Add(CraftingReosurceType.Food, 0);
        //this.Resources.Add(CraftingReosurceType.String, 0);
    }
}

public class CraftingResourceHolder : MonoBehaviour, IDataPersistence
{
    public SerializedDictionary<CraftingReosurceType,int> Resources;

    public delegate void Updated();
    public Updated OnUpdate;
    public void AddCraftableItemsFromInventory()
    {
        foreach (var item in LevelData.Inventory.Items)
        {
            var craftingResource = item.ItemData.PrefabReference.GetComponent<CraftingResource>();
            if (craftingResource != null)
            {
                this.Resources[craftingResource.Type] += craftingResource.Amount;
            }
        }

        foreach (var item in LevelData.Inventory.DumpedItems)
        {
            var craftingResource = item.ItemData.PrefabReference.GetComponent<CraftingResource>();
            if (craftingResource != null)
            {
                this.Resources[craftingResource.Type] += craftingResource.Amount;
            }
        }
    }

    public void LoadData(GameData data)
    {
       
        this.Resources=data.CraftingResourceData.Resources;
        if (!this.Resources.ContainsKey(CraftingReosurceType.Iron))
        {
            this.Resources.Add(CraftingReosurceType.Iron, 0);
        }

        if (!this.Resources.ContainsKey(CraftingReosurceType.String))
        {
            this.Resources.Add(CraftingReosurceType.String, 0);
        }
        if (!this.Resources.ContainsKey(CraftingReosurceType.Food))
        {
            this.Resources.Add(CraftingReosurceType.Food, 0);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (!this.Resources.ContainsKey(CraftingReosurceType.Iron))
        {
            this.Resources.Add(CraftingReosurceType.Iron, 0);
        }

        if (!this.Resources.ContainsKey(CraftingReosurceType.String))
        {
            this.Resources.Add(CraftingReosurceType.String, 0);
        }
        if (!this.Resources.ContainsKey(CraftingReosurceType.Food))
        {
            this.Resources.Add(CraftingReosurceType.Food, 0);
        }
        data.CraftingResourceData.Resources= this.Resources;
    }

    public IDataPersistence.Loaded OnLoaded { get; set; }
}
