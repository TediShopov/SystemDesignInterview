using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class DumpedItemsVisuals : MonoBehaviour
{
    public DumpedItemsObject DumpedItemPrefab;

    public Dictionary<InventoryItemData,int> ItemsByType=new Dictionary<InventoryItemData, int>();
    public List<DumpedItemsObject> VisualDumpedItemsObjects = new   List<DumpedItemsObject>();
    public Vector2 ItemDimensions;

    void Start()
    {
        InitializeItemByType();
        VisualizeDumpedItemSets();
    }

    void InitializeItemByType()
    {
        var inventory = LevelData.Inventory;
        ItemsByType.Clear();
        if (inventory!=null)
        {
            foreach (InventoryObject dumpedItem in inventory.DumpedItems)
            {
                var dumpedItemData=dumpedItem.ItemData;

                if (ItemsByType.ContainsKey(dumpedItemData))
                {
                    ItemsByType[dumpedItemData]++;
                }
                else
                {
                    ItemsByType.Add(dumpedItemData, 1);
                }
               
            }
        }
    }

    void VisualizeDumpedItemSets()
    {
        var itemPanelRect= this.gameObject.GetComponent<RectTransform>();
        if (ItemsByType.Count==0)
        {
            return;
        }
        float xIncrement= itemPanelRect.rect.size.x/ ItemsByType.Count;




        VisualDumpedItemsObjects.Clear();

        int iter = 0;

        foreach (var dumpedItemData in ItemsByType)
        {
            var dumpItemSet = Instantiate(DumpedItemPrefab,this.transform);
            var dumpItemRect= dumpItemSet.GetComponent<RectTransform>();
           //Place anchors to the left middle of parrent
            dumpItemRect.anchorMin=new Vector2(0,0.5f);
            dumpItemRect.anchorMax = new Vector2(0, 0.5f);
            //Place pivot of the object to the left middle
            dumpItemRect.pivot = new Vector2(0, 0.5f);

            //Set items sprite and count
            dumpItemRect.sizeDelta=ItemDimensions;
            dumpItemRect.anchoredPosition = new Vector2(iter * ItemDimensions.x,0);
            dumpItemSet.ItemImage.sprite= dumpedItemData.Key.Sprite;
            dumpItemSet.ItemCountText.text = "x" + dumpedItemData.Value.ToString();
            VisualDumpedItemsObjects.Add(dumpItemSet);
            iter++;
        }
    }



}
