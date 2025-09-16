using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceHolderViusals : MonoBehaviour
{
    public TMP_Text IronAmountLabel;
    public TMP_Text StringAmountLabel;
    public TMP_Text FoodAmountLabel;


    void Awake()
    {
        IronAmountLabel.text = "0";
        StringAmountLabel.text = "0";
        FoodAmountLabel.text = "0";
        LevelData.CraftingResourceHolder.OnLoaded += UpdateResourceAmountLevels;
        LevelData.CraftingResourceHolder.OnUpdate += UpdateResourceAmountLevels;
    }

    void UpdateResourceAmountLevels()
    {
        int iron, str, food;
        LevelData.CraftingResourceHolder.Resources.TryGetValue(CraftingReosurceType.Iron, out iron);
        LevelData.CraftingResourceHolder.Resources.TryGetValue(CraftingReosurceType.String, out str);
        LevelData.CraftingResourceHolder.Resources.TryGetValue(CraftingReosurceType.Food, out food);
        IronAmountLabel.text = iron.ToString();
        StringAmountLabel.text = str.ToString();
        FoodAmountLabel.text = food.ToString();
    }

    void UpdateResourceAmountLevels(GameData data)
    {
        int iron, str,food; 
        data.CraftingResourceData.Resources.TryGetValue(CraftingReosurceType.Iron,out iron);
        data.CraftingResourceData.Resources.TryGetValue(CraftingReosurceType.String, out str);
        data.CraftingResourceData.Resources.TryGetValue(CraftingReosurceType.Food, out food);

        IronAmountLabel.text = iron.ToString();
        StringAmountLabel.text = str.ToString();
        FoodAmountLabel.text = food.ToString();
    }


}
