using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PolygonCollider2D))]
public class Room : MonoBehaviour
{
    private PolygonCollider2D polygonCollider;
    public List<GameObject> OverlappingObjects;
    public List<GameObject> Enemies;
    //Slime droplets that are still processed
    public List<int> SpawnerInPlay;
    public Door DoorToDisable;
    public Door DoorToEnable;

    // All enemy animations use sprite masking to function correctly.
    // Each enemy attack must be affected only by its mask that
    // is why the mask must be registered to a specific sorting range.
    public int NextEnemyAttackMaskOrder= 200;
    public int EnemyAttackMaskOrderRange= 2;

    //In the future the system could be expanded to reuse certain ranges after enemy death
    public bool SetEnemyUniqueMaskingRange(Enemy e)
    {
        var attData = e.gameObject.GetComponentInChildren<AttackData>(true);
        var sprite = attData.gameObject.GetComponent<SpriteRenderer>();
        var spriteMask = attData.gameObject.GetComponentInChildren<SpriteMask>();
        if(attData == null || sprite == null || spriteMask == null)
        {
         return false; 
        }


        int rangeStart = NextEnemyAttackMaskOrder + 1;
        int rangeEnd = NextEnemyAttackMaskOrder + EnemyAttackMaskOrderRange;
        sprite.sortingOrder = rangeStart;

        spriteMask.backSortingOrder = rangeStart;
        spriteMask.frontSortingOrder = rangeEnd;
        NextEnemyAttackMaskOrder += EnemyAttackMaskOrderRange + 2;
        Debug.LogWarning($"{e.name} range is {rangeStart} to {rangeEnd}");
        return true;
    }







    public BoxCollider2D AreaOfTheTilemapToDisable; 


    //List of tiles to disable when the enemies in the room have been defeated
    public List<Vector3Int> GridCellsToToggle;
    public Tilemap TilemapVissualOver;
    public Tilemap TilemapVissualUnder;
    public Tilemap TilemapCollider;

    //Room active properties;
    public bool IsActive = false;
    

    //Event to fire when room has been cleared
    public delegate void RoomDelegate(Room room);
    public event RoomDelegate OnRoomClear ; // Event to notify health changes
    
    // Start is called before the first frame update
    void Start()
    {
        this.polygonCollider = GetComponent<PolygonCollider2D>();
        ExtractAllObjectOverlappingTheCollider();
        Enemies = OverlappingObjects.Where(o => o != null && o.gameObject.GetComponent<Enemy>() != null ).ToList();
        bool hasPlayer = OverlappingObjects.Any(o => o != null && o.gameObject.GetComponent<PlayerController2D>() != null );
        SpawnerInPlay = new List<int>();

        IsActive = hasPlayer;


        //Subscribe to enemy healt
        foreach (GameObject e in Enemies) 
        {
            AddEnemyToRoom(e);

        }
        SetEnemiesActiveState(IsActive);
        if(DoorToEnable != null)
        {
            DoorToEnable.SetState(!IsActive);
        }


        //;TestRemoveTilemapProgrammatically();

    }
    public void AddEnemyToRoom(GameObject e)
    {

        var healthComponent = e.GetComponent<Health>();
        var enemy = e.GetComponent<Enemy>();
        enemy.Room = this;
        if (SetEnemyUniqueMaskingRange(enemy) == false)
        {
            Debug.LogWarning("Could not set enemy unique masking range");
        }
        if (healthComponent == null)
            return;
        healthComponent.OnDeath += RemoveEnemyOnDeath;

    }

    void DisableRoomExits(bool b)
    {
//        if(AreaOfTheTilemapToDisable != null)
//            TestProjectPolygonColliderPointsInWorldCoordinates();
        if(DoorToDisable != null)
        {
           DoorToDisable.SetState(false);
        }

//        foreach (Vector3Int e in GridCellsToToggle)
//        {
//            TilemapVissualOver.SetTile(e, null);
//            TilemapVissualUnder.SetTile(e, null);
//            this.TilemapCollider.SetTile(e, null);
//        }
    }
    void SetEnemiesActiveState(bool b)
    {
        foreach (var e in this.Enemies)
        {
            e.gameObject.SetActive(b);
        }
    }


    void TestRemoveTilemapProgrammatically()
    {
        int fromX = -4;
        int toX = 9;
        int y = 8;
        for (int i = fromX; i <= toX; i++)
        {

            this.TilemapVissualOver.SetTile(new Vector3Int(i, 9, 0), null);
            this.TilemapVissualUnder.SetTile(new Vector3Int(i, 9, 0), null);

        }


    }
    
    bool DisableRoomCondition()
    {
        return SpawnerInPlay.Count ==0 && Enemies.Count==0;
    }
    void RemoveEnemyOnDeath(GameObject gameObject)
    {
        Enemies.Remove(gameObject);
        if (DisableRoomCondition())
        {
            IsActive = false;
            DisableRoomExits(false);
            OnRoomClear?.Invoke(this);

        }
    }

    void TestProjectPolygonColliderPointsInWorldCoordinates()
    {

        if (AreaOfTheTilemapToDisable == null) return;

        BoundsInt isobounds =
            ExtractIsometricBoundingBox(AreaOfTheTilemapToDisable.bounds, this.TilemapVissualOver);
        Vector3Int minInt = isobounds.min;  
        Vector3Int maxInt = isobounds.max;  


        Debug.Log($"minInt {minInt}");
        Debug.Log($"maxInt {maxInt}");

        for (int j = minInt.y; j <= maxInt.y; j++)
        {
            for (int i = minInt.x; i <= maxInt.x; i++)
            {
                var worldCellPossition = this.TilemapVissualOver.CellToWorld(new Vector3Int(i, j, 0));
                //Gizmos.color = Color.red;
                //Gizmos.DrawCube(worldCellPossition, this.TilemapVissualOver.cellSize);

                this.TilemapVissualOver.SetTile(new Vector3Int(i, j, 0), null);
                this.TilemapCollider.SetTile(new Vector3Int(i, j, 0), null);
            }
        }

        Destroy(AreaOfTheTilemapToDisable.gameObject);



        //Vector3 min = AreaOfTheTilemapToDisable.bounds.min;
        //Vector3 max = AreaOfTheTilemapToDisable.bounds.max;

        //Vector3Int minInt = TilemapCollider.layoutGrid.WorldToCell(min);
        //Vector3Int maxInt = TilemapCollider.layoutGrid.WorldToCell(max);

        //Debug.Log($"minInt {minInt}");
        //Debug.Log($"maxInt {maxInt}");

        //for (int i = minInt.x; i <= maxInt.x; i++)
        //{
        //    for (int j = minInt.y; j <= maxInt.y; j++)
        //    {
        //        this.TilemapVissualOver.SetTile(new Vector3Int(i, j, 0), null);
        //        this.TilemapCollider.SetTile(new Vector3Int(i, j, 0), null);

        //    }
        //}
        //Destroy(AreaOfTheTilemapToDisable.gameObject);
   
    }

    public List<T> GetOverlappingObjects<T>(bool useTriggers = true)
    {
         // Create a ContactFilter2D to detect everything except triggers
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = useTriggers; // Ignore other triggers

        Collider2D[] results = new Collider2D[50]; // Buffer size (adjust as needed)
        int count = polygonCollider.OverlapCollider(filter, results);

        List<T> list = new List<T>();
        for (int i = 0; i < count; i++)
        {
            var comp = results[i].GetComponent<T>(); 
            if(comp != null )
                list.Add(comp);
        }
        return list;

    }
     void ExtractAllObjectOverlappingTheCollider()
    {
         // Create a ContactFilter2D to detect everything except triggers
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false; // Ignore other triggers

        Collider2D[] results = new Collider2D[50]; // Buffer size (adjust as needed)
        int count = polygonCollider.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            OverlappingObjects.Add(results[i].gameObject);
        }

        Debug.Log($"Found {OverlappingObjects.Count} overlapping objects.");

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController2D>() != null)
        {
            Debug.Log($"{this.gameObject.name}: Player Has Enetered The Room");
            SetEnemiesActiveState(true);
            if (DoorToEnable != null)
            {
                DoorToEnable.SetState(true);
            }
            this.IsActive = true;
        }

    }

    public static BoundsInt ExtractIsometricBoundingBox(Bounds bounds, Tilemap tilemap)
    {
        float wminX = bounds.min.x;
        float wmaxX = bounds.max.x;
        float wminY = bounds.min.y;
        float wmaxY = bounds.max.y;

        Vector3 a = new Vector3(wminX,wminY, 0);
        Vector3 b = new Vector3(wmaxX,wminY, 0);
        Vector3 c = new Vector3(wminX,wmaxY, 0);
        Vector3 d = new Vector3(wmaxX,wmaxY, 0);

        Vector3Int aInt = tilemap.WorldToCell(a);
        Vector3Int bInt= tilemap.WorldToCell(b);
        Vector3Int cInt = tilemap.WorldToCell(c);
        Vector3Int dInt = tilemap.WorldToCell(d);

        int minXInt = Mathf.Min(aInt.x, bInt.x, cInt.x, dInt.x);
        int maxXInt = Mathf.Max(aInt.x, bInt.x, cInt.x, dInt.x);
        int minYInt = Mathf.Min(aInt.y, bInt.y, cInt.y, dInt.y);
        int maxYInt = Mathf.Max(aInt.y, bInt.y, cInt.y, dInt.y);
        Vector3Int minInt = new Vector3Int(minXInt,minYInt,0);
        Vector3Int maxInt = new Vector3Int(maxXInt,maxYInt,0);
        BoundsInt boundsInt = new BoundsInt();
        boundsInt.xMin = minXInt;
        boundsInt.xMax = maxXInt;
        boundsInt.yMin = minYInt;   
        boundsInt.yMax = maxYInt;   
        boundsInt.min = minInt;
        boundsInt.max = maxInt;
        return boundsInt;

    }
    private void OnDrawGizmos()
    {
        if (AreaOfTheTilemapToDisable == null) return;

        BoundsInt isobounds =
            ExtractIsometricBoundingBox(AreaOfTheTilemapToDisable.bounds, this.TilemapVissualOver);
        Vector3Int minInt = isobounds.min;  
        Vector3Int maxInt = isobounds.max;  


        Debug.Log($"minInt {minInt}");
        Debug.Log($"maxInt {maxInt}");

            for (int j = minInt.y; j <= maxInt.y; j++)
            {
        for (int i = minInt.x; i <= maxInt.x; i++)
        {
                var worldCellPossition = this.TilemapVissualOver.CellToWorld(new Vector3Int(i, j, 0));
                Gizmos.color = Color.red;
                Gizmos.DrawCube(worldCellPossition, this.TilemapVissualOver.cellSize);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
