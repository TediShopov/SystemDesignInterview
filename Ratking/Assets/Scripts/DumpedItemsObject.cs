using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class DumpedItemsObject : MonoBehaviour
{
    public InventoryItemData ItemData;
    public int Count = 0;
    public Image ItemImage;
    public TMP_Text ItemCountText;

    void Awake()
    {
        ItemImage = this.GetComponent<Image>();
        ItemCountText = this.GetComponentInChildren<TMP_Text>();
    }

}
