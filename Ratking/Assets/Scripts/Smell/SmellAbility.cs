using System;
using System.Collections.Generic;
using System.Linq;
using SuperTiled2Unity;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static IDataPersistence;

[RequireComponent(typeof(BoxCollider2D))]
//[ExecuteAlways]
public class SmellAbility : MonoBehaviour, IDataPersistence
{
    private Grid _grid;
    [Header("Grid Parameters")]
    [SerializeField]
    public SuperMap PFGrid;

    [SerializeField]
    private SmellCooldownBar _smellCooldownBar;
    public const float SMELL_COOLDOWN = 1.5f;
    private const float SMELL_ACTIVE_TIME = 2.0f;

    public float SmellCooldown;
    private float _smellActiveTime;


    public List<GameObject> PotentiallySmellableItems;
    private BoxCollider2D _smellArea;
    private Vector3 _tileSize;
   
    [Range(0, 500)] private int SmellRangeInTiles;
    public GameObject SmellTrailHolder;
    public GameObject SmellTrailPrefab;
    [HideInInspector] public Graph<GridNode> SmellTraversabilityGraph;
    private CoreControl PlayerController;
    public List<GridNode> UnvisitedNodesFromDjikstra = new();

    [Range(0, 500)] public int VisibleSmellTrailTiles;

    public Animator ratAnimator;

    // Start is called before the first frame update
    private void Awake()
    {
        Init();
    }

     Vector2 GetSmellAreaWorld()
     {
         return new Vector2(this.SmellRangeInTiles * _tileSize.x, this.SmellRangeInTiles * _tileSize.y);
    }


    // Update is called once per frame
    private void Update()
    {
        if (_smellArea != null) _smellArea.size = GetSmellAreaWorld();

        if (_smellActiveTime > 0)
        {
            _smellActiveTime -= Time.deltaTime;
            return;
        }
        if (_smellActiveTime < 0)
        {
            _smellActiveTime = 0;
            Helpers.DeleteAllChildren(SmellTrailHolder.transform);
            SmellCooldown = SMELL_COOLDOWN;
          
                _smellCooldownBar.Activate();
        }


        SmellCooldown -= Time.deltaTime;

        //if (Input.GetKeyDown(KeyCode.Tab) && SmellCooldown <= 0)
        //    ResetSmellTrails();
        //else
        //    SmellCooldown -= Time.deltaTime;
        //if(Input.GetKeyDown(KeyCode.M))
        //    Helpers.DeleteAllChildren(SmellTrailHolder.transform);

    }


    public void sniffInput(InputAction.CallbackContext context)
    {
        if (context.performed && context.action.WasPressedThisFrame())
        {
            if (SmellCooldown < 0 && PlayerController.canSmell())
            {
                ResetSmellTrails();
            }
        }
    }


    public void Init()
    {

            SmellCooldown = 0f;

        PotentiallySmellableItems = new List<GameObject>();
        _smellArea = gameObject.GetComponent<BoxCollider2D>();
        _grid = PFGrid.GetComponentInChildren<Grid>();
        _tileSize = new Vector3();
        _tileSize.x = _grid.cellSize.x * PFGrid.transform.localScale.x;
        _tileSize.y = _grid.cellSize.x * PFGrid.transform.localScale.x;
        PlayerController = LevelData.PlayerObject.GetComponent<CoreControl>();
    }

    
    private void InitGraphAdjacencies()
    {
        foreach (GridNode tile in SmellTraversabilityGraph.Nodes)
        {
            Vector3Int gridPos = tile.PositionInGrid;

            //Horizontal Neighbour
            TryAddNeighbour(tile, Vector3Int.right);
            TryAddNeighbour(tile, Vector3Int.left);

            //Vertical Neighbours
            TryAddNeighbour(tile, Vector3Int.up);
            TryAddNeighbour(tile, Vector3Int.down);

            //Diagonal
            TryAddNeighbour(tile, Vector3Int.left + Vector3Int.up);
            TryAddNeighbour(tile, Vector3Int.right + Vector3Int.up);
            TryAddNeighbour(tile, Vector3Int.left + Vector3Int.down);
            TryAddNeighbour(tile, Vector3Int.right + Vector3Int.down);
        }
    }

    public bool CellOverlapsLayer(Vector3Int nodeGridPosition, LayerMask mask)
    {
        Collider2D freeSpaceCollider = Physics2D.OverlapBox(
            _grid.GetCellCenterWorld(nodeGridPosition), _tileSize * 0.1f, 0, mask.value);
        if (freeSpaceCollider) return true;

        return false;
    }

    public bool IsGround(Vector3Int gridPosition)
    {
        return CellOverlapsLayer(gridPosition, LayerMask.GetMask("Ground"));
    }


    private void TryAddNeighbour(GridNode startTile, Vector3Int relativePos)
    {
        Vector3Int targetPos = startTile.PositionInGrid + relativePos;
        if (!IsGround(targetPos))
        {
            GridNode neighbourTile =
                SmellTraversabilityGraph.Nodes.FirstOrDefault(x => x.PositionInGrid == targetPos);
            if (neighbourTile != null)
                SmellTraversabilityGraph.AddNeighbor(startTile, neighbourTile, relativePos.magnitude);
        }
    }

    public GridNode GetClosestTile(Vector3 worldPosition)
    {

        Vector3Int toGridCell = _grid.WorldToCell(worldPosition);
        toGridCell.z = 0;
        GridNode gridNodeToReturn=null;
        this.SmellTraversabilityGraph.Nodes.TryGetValue(new GridNode(toGridCell), out gridNodeToReturn);
        if (gridNodeToReturn == null)
        {
        }
        return gridNodeToReturn;

    }

    public GridNode BuildGraphFromNearbyTiles(Vector3Int cellStart)
    {
        var listOfNodes = new List<GridNode>();
        GridNode toReturn=null;
        var halfRange = GetSmellableGraphDimension();
        for (int i = -halfRange; i <= halfRange; i++)
        {
            for (int j = -halfRange; j <= halfRange; j++)
            {
                Vector3Int cellToAdd = cellStart + new Vector3Int(i,j,0);
                var currentNode = new GridNode(cellToAdd);
                listOfNodes.Add(currentNode);

                if (i==0 && j==0)
                {
                    toReturn = currentNode;
                }
            }
        }

        SmellTraversabilityGraph = new Graph<GridNode>(listOfNodes);
        InitGraphAdjacencies();
        return toReturn;
    }

   

    int GetSmallestGridArea(Vector3Int center, Vector3Int includedCell)
    {
        Vector2Int difference = new Vector2Int(Math.Abs(center.x - includedCell.x), Math.Abs(center.y - includedCell.y));
        return Math.Max(difference.x, difference.y);
    }

    int GetSmellableGraphDimension()
    {
        int maxGridAreaNeeded = 0;
        foreach (GameObject smellableObejct in PotentiallySmellableItems)
        {
            int gridAreaNeeded= GetSmallestGridArea(_grid.WorldToCell(this.transform.position),
                _grid.WorldToCell(smellableObejct.transform.position));
            if (gridAreaNeeded > maxGridAreaNeeded)
            {
                maxGridAreaNeeded = gridAreaNeeded;
            }

            //var smellableScript = smellableObejct.GetComponent<Smellable>();

            //if (smellableScript!= null)
            //{
               


            //    float dist = Vector3Int.Distance(_grid.WorldToCell(smellableObejct.transform.position),
            //        _grid.WorldToCell(this.transform.position));
            //    int smellDistnace = (int)Math.Round(dist / _tileSize.magnitude) ;

            //    if (smellDistnace > maxGridAreaNeeded)
            //    {
            //        maxGridAreaNeeded = smellDistnace;
            //    }

            //}
            
        }

        return maxGridAreaNeeded;
    }

    public void ResetSmellTrails()
    {
        if(PlayerController.grounded)
            ratAnimator.SetTrigger("Smell");

        _smellActiveTime = SMELL_ACTIVE_TIME;
        //Helpers.DeleteAllChildren(SmellTrailHolder.transform);
        Vector3Int cellStart = _grid.WorldToCell(this.gameObject.transform.position);
        if (!this.IsGround(cellStart))
        {
           // int dimension = GetSmellableGraphDimesnion();
            GridNode startNode = BuildGraphFromNearbyTiles(cellStart);
            if (PotentiallySmellableItems.Count > 0)
            { var shortestPathsDijkstra =
                    SmellTraversabilityGraph.GetShortestPathDjikstra(startNode);



                foreach (GameObject smellabelObejcts in PotentiallySmellableItems)
                {
                    var smellableScript = smellabelObejcts.GetComponent<Smellable>();

                    GridNode destinationNode = GetClosestTile(smellabelObejcts.transform.position);

                    if (destinationNode != null)
                    {

                        List<GridNode> path = SmellTraversabilityGraph.BacktrackPath(shortestPathsDijkstra, startNode, destinationNode);

                        if (path == null || path.Count == 0 || smellableScript == null)
                        {

                            continue;
                        }

                        if (path.Count < (smellableScript.MinVisibleSmellTrailTiles + this.SmellRangeInTiles))
                        {


                            //Reachable by players smell
                            GameObject trailObject = Instantiate(SmellTrailPrefab, SmellTrailHolder.transform);
                            LineRenderer trail = trailObject.GetComponent<LineRenderer>();
                            trail.material.SetFloat("_speed",smellableScript.SpeedOfTrail);
                            trail.startColor = smellableScript.SmellPathColor;
                            trail.endColor = smellableScript.SmellPathColor;


                            //visible trail --> max of path and max possible visible
                            int minVisible = Math.Min(path.Count,
                                this.VisibleSmellTrailTiles + smellableScript.MinVisibleSmellTrailTiles);

                            trail.positionCount = minVisible;
                            for (var i = 0; i < minVisible; i++)
                                trail.SetPosition(i, _grid.GetCellCenterWorld(path[i].PositionInGrid));
                        }
                        else
                        {

                        }


                    }
                    else
                    {


                    }
                }
            }
        }
       
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(this.transform.position, GetSmellAreaWorld());
       
    }

    public void DebugSmellTraversableNodes()
    {
        if (SmellTraversabilityGraph != null)
            foreach (GridNode tile in SmellTraversabilityGraph.Nodes)
            {
                Gizmos.color = new Color(252, 172, 0);
                Gizmos.DrawWireSphere(_grid.GetCellCenterWorld(tile.PositionInGrid), 0.15f);

                foreach (var connection in tile.Connections)
                    Gizmos.DrawLine(
                        _grid.GetCellCenterWorld(tile.PositionInGrid),
                        _grid.GetCellCenterWorld(connection.Neighbor.PositionInGrid));
            }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PotentiallySmellableItems.Add(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PotentiallySmellableItems.Remove(other.gameObject);
    }

    public void LoadData(GameData data)
    {
        this.SmellRangeInTiles = data.SmellRangeTiles;
        Init();
    }

    public void SaveData(ref GameData data)
    {
        data.SmellRangeTiles = (int)data.PlayerProgressionTracker.GetCurrentUpgrade(UpgradePathType.Smell).UpgradedValue;
        // throw new NotImplementedException();
    }

    public IDataPersistence.Loaded OnLoaded { get; set; }
}