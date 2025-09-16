using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevel : MonoBehaviour
{
    

    public void EndLevelOnClick()
    {
        LevelData.Inventory.ItemsToBaseGold();
        LevelData.Inventory.ClearAll();
        LevelData.Toolbox.Clear();
    }
}
