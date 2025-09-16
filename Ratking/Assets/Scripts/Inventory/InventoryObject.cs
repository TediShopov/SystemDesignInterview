using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(UnityEngine.UI.Image))]
public class InventoryObject : MonoBehaviour
{

    [SerializeField] [HideInInspector] public Image Image;
    private InventoryItemData _itemData;
    [SerializeField]
    [HideInInspector]
    public InventoryItemData ItemData
    {
        get { return _itemData; }
        set
        {
            _itemData = value;
        }
    }
    [SerializeField] [HideInInspector] public List<Vector3Int> OccupiedCells;

    private RectTransform _rectTransform;

   

    // Start is called before the first frame update
    private void Awake()
    {
        if (OccupiedCells == null) OccupiedCells = new List<Vector3Int>();
        this._rectTransform = this.GetComponent<RectTransform>();
        Image = GetComponent<Image>();
    }

    //Used for remembering which item was taken exactly. And when transforming from this to physical item this hash will be  reasiggned.
    [HideInInspector] public int UniqueCollectibleHash;


    public GameObject GetCollectableCopy()
    {
        var collectible = this.ItemData.PrefabReference.GetComponent<Collectible>();
        if (collectible != null)
        {
            var objHandle = Instantiate(this.ItemData.PrefabReference);
            //Assign the collectable component script to the new item
            collectible = objHandle.GetComponent<Collectible>();
            collectible.UniquenessHash = this.UniqueCollectibleHash;

            return objHandle;
        }

        return null;

    }

    public void SetToCollectable(Collectible collectible)
    {
        if (collectible==null)
        {
            return;
        }
        this.ItemData = collectible.InventoryItemData;
        this.ItemData.PrefabReference = collectible.InventoryItemData.PrefabReference;
        this.UniqueCollectibleHash = collectible.UniquenessHash;
    }

    public void UpdatePosition(Vector3Int cooridinatePointedByMouse, Grid grid)
    {
        OccupiedCells.Clear();
        Vector3Int startingPos = cooridinatePointedByMouse;
        startingPos -= new Vector3Int(ItemData.Size.x / 2, ItemData.Size.y / 2, 0);
        for (var y = 0; y < ItemData.Size.y; y++)
            for (var x = 0; x < ItemData.Size.x; x++)
            {
                Vector3Int currentCoordinateFromItem = new(startingPos.x + x, startingPos.y + y, 0);
                OccupiedCells.Add(currentCoordinateFromItem);
            }

        UpdateItemSprite(cooridinatePointedByMouse, grid);
    }

    //public void UpdateItemSprite( Grid inventoryGrid)
    //{
    //    Image.transform.position = GetCenterOfItemArea(inventoryGrid);
    //    this._rectTransform.sizeDelta = new Vector2(inventoryGrid.cellSize.x, inventoryGrid.cellSize.y);
    //    Image.transform.localScale = new Vector3(ItemData.Size.x, ItemData.Size.y, 1);
    //    Image.sprite = ItemData.Sprite;
    //}

    private void UpdateItemSprite(Vector3Int cooridinatePointedByMouse, Grid inventoryGrid)
    {
        Image.transform.position = GetCenterOfItemArea(cooridinatePointedByMouse, inventoryGrid);
        this._rectTransform.sizeDelta = new Vector2(inventoryGrid.cellSize.x, inventoryGrid.cellSize.y);
        Image.transform.localScale = new Vector3(ItemData.Size.x, ItemData.Size.y, 1);
        Image.sprite = ItemData.Sprite;
    }

    public Vector3 GetCenterOfItemArea(Vector3Int cooridinatePointedByMouse, Grid inventoryGrid)
    {
        Vector3Int gridStartingPos =
            cooridinatePointedByMouse - new Vector3Int(ItemData.Size.x / 2, ItemData.Size.y / 2, 0);
        Vector3Int gridEndPos = gridStartingPos + new Vector3Int(ItemData.Size.x - 1, ItemData.Size.y - 1);
        Vector3 toReturn =
            (inventoryGrid.GetCellCenterWorld(gridStartingPos) + inventoryGrid.GetCellCenterWorld(gridEndPos)) / 2.0f;
        toReturn.z = 1.0f;
        return toReturn;
    }

    public Vector3 GetCenterOfItemArea(Grid inventoryGrid)
    {
        Vector3Int gridStartingPos = this.OccupiedCells[0];
        Vector3Int gridEndPos = this.OccupiedCells[this.OccupiedCells.Count-1];
        Vector3 toReturn =
            (inventoryGrid.GetCellCenterWorld(gridStartingPos) + inventoryGrid.GetCellCenterWorld(gridEndPos)) / 2.0f;
        toReturn.z = 1.0f;
        return toReturn;
    }
}