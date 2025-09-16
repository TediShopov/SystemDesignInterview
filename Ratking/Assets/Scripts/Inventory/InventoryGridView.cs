using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryGridView : MonoBehaviour
{
    [HideInInspector] public IPlayerInventory Inventory;
    public TMP_Text BaseGoldLabel;
    public TMP_Text TotalGoldLabel;
    public Vector2 GridDimensionCanvasSpace;
    public Grid Grid;

    public bool IsInteractalbe = true;
    public bool SetInactiveOnStart = true;

    public RectTransform GridFillRect;



    public InventoryObject ItemToPlacePrefab;
    public InventoryObject ItemToPlace=null;
    public InventoryCell PlaceHolderTile;
    public Dictionary<Vector3Int, InventoryCell> _indexedCells=new Dictionary<Vector3Int, InventoryCell>();
    [SerializeField] public GridInteractionColors ColorPallete;

    void Awake()
    {
        this.Inventory = LevelData.Inventory;
        Inventory.OnUniqueItemAdded += PushProgressBar;
        Inventory.OnInventoryUpdated += UpdateLabels;
        this.Inventory.OnLoaded += Init;
        
    }

    void Init(GameData data)
    {
        ConstructGrid();
        ReinitializeInventoryObjects();
        UpdateLabels();
        if (SetInactiveOnStart)
        {
            this.gameObject.SetActive(false);
        }
    }

    void PushProgressBar(InventoryObject item)
    {
        LevelData.RewardBar.CurrentScore += (int)(item.ItemData.GoldValue);
    }

    public void ReinitializeInventoryObjects()
    {
        foreach (var item in this.Inventory.Items)
        {
            ReconstructInventoryObject(item);
        }
    }
    public void ConstructGrid()
    {
        this._indexedCells = new Dictionary<Vector3Int, InventoryCell>();
        Vector3Int startingPos = new(-Inventory.GridDimensions.x / 2, -Inventory.GridDimensions.y / 2, 0);

        float cellWidth= this.GridFillRect.rect.width / Inventory.GridDimensions.x;
        float cellHeight = this.GridFillRect.rect.height / Inventory.GridDimensions.y;

        Grid.cellSize=new Vector2(cellWidth, cellHeight);
        for (var y = 0; y < Inventory.GridDimensions.y; y++)
            for (var x = 0; x < Inventory.GridDimensions.x; x++)
            {
                Vector3Int gridPosition = new(startingPos.x + x, startingPos.y + y, 0);
                InventoryCell cellGameObject = Instantiate(PlaceHolderTile,
                    Grid.GetCellCenterWorld(gridPosition), Quaternion.identity, Grid.transform);
                var cellRect= cellGameObject.GetComponent<RectTransform>();
                cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);
                _indexedCells.Add(gridPosition, cellGameObject);
            }
       
    }

    public bool CanEnable()
    {
        var thrower = LevelData.PlayerObject.GetComponent<Thrower>();
        bool throwItemCanFitInInvetory = thrower.ThrowItemPrefab == null || (thrower.ThrowItemPrefab != null  && thrower.ThrowItemPrefab.GetComponent<Collectible>() != null);
       return  thrower!=null && throwItemCanFitInInvetory;
    }

    public void inventoryInput(InputAction.CallbackContext context)
    {
        if (context.action.WasPressedThisFrame() && context.performed)
        {
            if (this.gameObject.activeSelf==false)
            {
                if (this.CanEnable())
                {
                    gameObject.SetActive(!gameObject.activeSelf);

                }
            }
            else
            {
                gameObject.SetActive(!gameObject.activeSelf);

            }
        }
    }

    public void tryPlaceOrPickItemAction(InputAction.CallbackContext context)
    {
        
        if (context.action.WasPressedThisFrame() && context.performed && this.gameObject.activeSelf &&  IsInteractalbe)
        {
            Vector3Int cooridinatePointedByMouse = Grid.WorldToCell(Mouse.current.position.ReadValue());

            if (ItemToPlace != null)
            {
                bool canPlace = Inventory.CanPlaceItem(ItemToPlace);

                if (canPlace)
                {
                    Inventory.PlaceItem(ItemToPlace);
                    var thrower = LevelData.PlayerObject.GetComponent<Thrower>();
                    if (thrower != null)
                    {
                        thrower.SetThrowItem(null);
                    }
                    ItemToPlace = null;
                }
                else if (!Inventory.IsInGrid(cooridinatePointedByMouse))
                {
                    //Item is drawn out of grid, and placed in throwing mode

                   var thrower= LevelData.PlayerObject.GetComponent<Thrower>();
                   if (thrower != null)
                   {
                       //Get the physical object linked to the inventory obejct
                       var physicalThrowableCopy = this.ItemToPlace.GetCollectableCopy();
                       thrower.OnItemThow += DestroyItemToPlace;
                            thrower.SetThrowItem(physicalThrowableCopy);
                   }

                   gameObject.SetActive(false);
                }

            }
            else
            {
                InventoryObject selectedItem = this.Inventory.PickItem(cooridinatePointedByMouse);
                ItemToPlace = selectedItem;

            }
        }
       
    }


    public void OnDestroy()
    {
        Inventory.OnUniqueItemAdded -= PushProgressBar;
        Inventory.OnInventoryUpdated -= UpdateLabels;
        this.Inventory.OnLoaded -= Init;
    }


    private void Update()
    {
        ResetHighlights();
        Vector3Int cooridinatePointedByMouse = Grid.WorldToCell(Mouse.current.position.ReadValue());


        if (ItemToPlace != null && ItemToPlace.ItemData!=null)
        {
            ItemToPlace.UpdatePosition(cooridinatePointedByMouse, Grid);
            bool canPlace = Inventory.CanPlaceItem(ItemToPlace);
            UpdateCellVisual(ItemToPlace.OccupiedCells, canPlace);
        }
        
    }



    void UpdateLabels()
    {
        if (TotalGoldLabel != null)
        {
            TotalGoldLabel.text = $"{Inventory.TotalGoldValue}";
        }

        if (BaseGoldLabel != null)
        {
            BaseGoldLabel.text = $"{Inventory.BaseGoldValue}";
        } 
    }

    void UpdateCellVisual(List<Vector3Int> cellsInItem, bool canPlace)
    {
        if (canPlace)
        {
            foreach (Vector3Int cell in cellsInItem)
                if (_indexedCells.ContainsKey(cell))
                {
                    _indexedCells[cell].ChangeColors(ColorPallete.ValidPlacementBackgroundColor,
                        ColorPallete.ValidPlacementBorderColor);
                }
        }

        else
        {
            foreach (Vector3Int cell in cellsInItem)
                if (_indexedCells.ContainsKey(cell))
                {
                    _indexedCells[cell].ChangeColors(ColorPallete.InValidPlacementBackgroundColor,
                        ColorPallete.InValidPlacementBorderColor);
                }
        }
    }

    public void DestroyItemToPlace(GameObject obj=null, Vector2 vec=new Vector2())
    {
        if (ItemToPlace != null)
        {
            DestroyImmediate(ItemToPlace.gameObject);
        }
    }

    public void SetItem(Collectible collectible)
    {
        //Make copy of the prefab and assign new internal values
        ItemToPlace= Instantiate(ItemToPlacePrefab, this.transform);
        this.ItemToPlace.SetToCollectable(collectible);
    }


    public void ReconstructInventoryObject(InventoryObject item)
    {
        Collectible collectible = item.ItemData.PrefabReference.GetComponent<Collectible>();
        if (collectible)
        {
            Vector3Int centerOfItemCell = this.Grid.WorldToCell(item.GetCenterOfItemArea(this.Grid));
            //DefaultItemPrefab.ItemData = collectible.InventoryItemData;
            //DefaultItemPrefab.ItemData.PrefabReference = collectible.InventoryItemData.PrefabReference;
            //DefaultItemPrefab.UniqueCollectibleHash = collectible.UniquenessHash;

            //i/*tem = Instantiate(DefaultItemPrefab, this.gameObject.transform);*/
            item = Instantiate(ItemToPlacePrefab, this.gameObject.transform);
            item.SetToCollectable(collectible);
            item.UpdatePosition(centerOfItemCell, this.Grid);
            item.UniqueCollectibleHash = collectible.UniquenessHash;
        }

    }

    //public void SetCollectibleToPlaceInInventory(Collectible collectible)
    //{
    //    DestroyItemToPlace();
    //    DefaultItemPrefab.ItemData = collectible.InventoryItemData;
    //    DefaultItemPrefab.ItemData.PrefabReference = collectible.InventoryItemData.PrefabReference;
    //    DefaultItemPrefab.UniqueCollectibleHash = collectible.UniquenessHash;
    //    ItemToPlace = Instantiate(DefaultItemPrefab,this.gameObject.transform);
    //    ItemToPlace.UniqueCollectibleHash=collectible.UniquenessHash;

    //}

    void HighlightCells()
    {

    }

    void ResetHighlights()
    {
        foreach (var cell in _indexedCells) cell.Value.ResetToDefaultColors();
    }


}
