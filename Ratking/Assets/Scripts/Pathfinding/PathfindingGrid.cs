using SuperTiled2Unity;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GridNode : INode<GridNode>
{
    public GridNode(Vector3Int position)
    {
        Connections = new List<NodeConnection<GridNode>>();
        PositionInGrid = position;
    }

    public Vector3Int PositionInGrid { get; }

    public override bool Equals(object obj)
    {
        return PositionInGrid == obj?.ConvertTo<GridNode>().PositionInGrid;
    }

    public override int GetHashCode()
    {
        return PositionInGrid.GetHashCode();
    }

}

public class Platform
{
    public GridNode EdgeNodeOne;
    public GridNode EdgeNodeTwo;
    public List<Vector3Int> TraversableCells = new List<Vector3Int>();

    public Platform(List<Vector3Int> cells, Graph<GridNode> graph)
    {
        if (cells.Count < 2 || graph == null)
        {
            return;
        }

        TraversableCells = new List<Vector3Int>(cells);
        graph.Nodes.TryGetValue(new GridNode(TraversableCells[0]), out EdgeNodeOne);
        graph.Nodes.TryGetValue(new GridNode(TraversableCells[TraversableCells.Count - 1]), out EdgeNodeTwo);
    }

    public bool IsValid => TraversableCells.Count >= 2 && EdgeNodeOne != null && EdgeNodeTwo != null;

}

[ExecuteAlways]
public class PathfindingGrid : MonoBehaviour
{
    [Header("Grid Parameters")]
    [SerializeField]
    public SuperMap SuperTileMap;

    [SerializeField] public Tilemap LadderTilemap;
    [SerializeField] public BoxCollider2D BoxCollider;
    [SerializeField] public bool Reset;
    [SerializeField] public bool SaveToPrecomputed;
    [SerializeField] public bool SaveToPaths;



    public Graph<GridNode> TraversableGraph;


    private Grid _tilemapGrid;
    private Vector3 _tilemapSize;
    private Vector2Int _cellDimensions;

    private Vector3 _tileSize;
    private readonly float _tileBoundsOffset = 0.1f;
    public List<Platform> Platforms = new List<Platform>();
    private List<Vector3Int> _traversableTiles;
    private List<Vector3Int> _30degSlopeTiles;
    //  private List<Vector3Int> _traverseNodes;
    public PrecomputedGraph PrecomputedGraph;
    public PrecomputedPaths PrecomputedPaths;
    private bool calculatedOnce;

    private void Awake()
    {
        if (TraversableGraph == null)
        {
            Init();
        }

    }

    //void ReadPrecomputed()
    //{
    //    if (PrecomputedGraph!=null)
    //    {
    //        Debug.Log("Read Precomputed Graph");
    //        foreach (var nodePos in this.PrecomputedGraph.Nodes)
    //        {
    //            this.TraversableGraph.AddNode(new GridNode(nodePos));
    //            //this.TraversableGraph.Nodes.Add(new GridNode(nodePos));
    //        }

    //        if (PrecomputedGraph.NodeConnections!=null)
    //        {
    //            Debug.Log($"Precomputed graph connenctions {PrecomputedGraph.NodeConnections.Count}");

    //            for (int i = 0; i < PrecomputedGraph.NodeConnections.Count; i++)
    //            {
    //                for (int j = 0; j < PrecomputedGraph.NodeConnections[i].ConnectionIndices.Count; j++)
    //                {
    //                    var neighbor =
    //                        this.TraversableGraph.Nodes[PrecomputedGraph.NodeConnections[i].ConnectionIndices[j]];
    //                    this.TraversableGraph.AddNeighbor(
    //                        this.TraversableGraph.Nodes[i], neighbor, PrecomputedGraph.NodeConnections[i].ConnecionWeight[j]);
    //                }

    //            }
    //        }
    //        else
    //        {
    //            Debug.Log($"Precomputed has null connections");
    //        }
    //    }

    //    if (PrecomputedPaths != null) 
    //    {
    //        this.TraversableGraph.PrecomputedShortestPaths.Clear();
    //        for (int i = 0; i < this.PrecomputedPaths.ShortestPathsToNode.Count; i++)
    //        {
    //            ShortestPathsSerializable shortestPathsSerializable = this.PrecomputedPaths.ShortestPathsToNode[i];
    //            GridNode node = this.TraversableGraph.Nodes[i];

    //            ShortestPaths<GridNode> allShortestPaths = new ShortestPaths<GridNode>();
    //            for (int j = 0; j < shortestPathsSerializable.ShortestPathList.Count; j++)
    //            {
    //                ShortestPathSerializable shortestPathSerializable = shortestPathsSerializable.ShortestPathList[j];

    //                var shortestPathForCurrentNode = new ShortestPath<GridNode>();
    //                var currentNode = this.TraversableGraph.Nodes[j];

    //                if (shortestPathSerializable.ReachedByNodeIndex == -1)
    //                {
    //                    shortestPathForCurrentNode.ReachedByNode = null;

    //                }
    //                else
    //                {
    //                    shortestPathForCurrentNode.ReachedByNode = this.TraversableGraph.Nodes[shortestPathSerializable.ReachedByNodeIndex];
    //                }

    //                shortestPathForCurrentNode.MinDistance =shortestPathSerializable.MinimumDistanceToNode;
    //                allShortestPaths.Paths.Add(currentNode, shortestPathForCurrentNode);
    //            }

    //            this.TraversableGraph.PrecomputedShortestPaths.Add(node, allShortestPaths);
    //        }
    //        Debug.Log($"Added Precomputed Path: {this.TraversableGraph.PrecomputedShortestPaths.Count}");

    //    }
    //}
    public int RemoveSinglePlatformBelowTiles;
    public void EliminateIsolatedPlatformNodes()
    {
        List<GridNode> NodesToRemove = new List<GridNode>();
        foreach (GridNode node in this.TraversableGraph.Nodes)
        {
            if (node.Connections.Count == 0)
            {
                NodesToRemove.Add(node);
            }
            if (node.Connections.Count == 1 && node.Connections[0].Neighbor.Connections.Count == 1)
            {
                if (node.Connections[0].Weight>RemoveSinglePlatformBelowTiles)
                {
                    continue;
                }
                NodesToRemove.Add(node);
                NodesToRemove.Add(node.Connections[0].Neighbor);

                //Remove from traversable nodes aswell 
                Vector3Int neighborPositionInGrid = node.Connections[0].Neighbor.PositionInGrid;
                Vector3 dir = (_tilemapGrid.GetCellCenterWorld(neighborPositionInGrid) -
                              _tilemapGrid.GetCellCenterWorld(node.PositionInGrid)).normalized;


                Vector3Int gridDir = Vector3Int.RoundToInt(dir);
                Debug.Log($"Grid Direction : {gridDir}");
                //Direction in grid
                //Vector3Int neighborPositionInGrid = node.Connections[0].Neighbor.PositionInGrid;
                //var gridDir= GridDirection(node.PositionInGrid, neighborPositionInGrid);


                Vector3Int currentRemovedTile = node.PositionInGrid;
                while (currentRemovedTile != neighborPositionInGrid)
                {
                    if (!_traversableTiles.Remove(currentRemovedTile))
                    {
                        break;
                    }
                    currentRemovedTile = currentRemovedTile + gridDir;

                }


            }
        }

        foreach (GridNode node in NodesToRemove)
        {
            this.TraversableGraph.RemoveNode(node);
        }

    }

    protected virtual void Init()
    {

        Debug.LogWarning("Called Init Pathfinding Grid");
        _30degSlopeTiles = new List<Vector3Int>();
        _traversableTiles = new List<Vector3Int>();
        UpdateTileMapRelatedVariable();
        SetRowsAndCols(BoxCollider);
        _tileSize = new Vector3();
        _tileSize.x = _tilemapGrid.cellSize.x * SuperTileMap.transform.localScale.x;
        _tileSize.y = _tilemapGrid.cellSize.x * SuperTileMap.transform.localScale.x;

        //this.TraversableGraph = new Graph<GridNode>();
        SetTraversablePoints();


        TraversableGraph = new Graph<GridNode>(ToNodeList(GetTraversableWalkingPoints()));
        //TraversableGraph.AddNodes(ToNodeList(GetTraversableWalkingPoints()));
        //TraversableGraph.AddNodes(ToNodeList(GetTraversableLadderPoints()));
        ConnectTraversePoints();
        EliminateIsolatedPlatformNodes();

        //TraversableGraph = new Graph<GridNode>();
        //    TraversableGraph.AddNodes(ToNodeList(GetTraversableWalkingPoints()));
        //    TraversableGraph.AddNodes(ToNodeList(GetTraversableLadderPoints()));
        //    SetTraversablePoints();
        //    ConnectTraversePoints();

        //if (PrecomputedGraph != null )
        //{
        //    ReadPrecomputed();
        //}

    }

    // Update is called once per frame
    private void Update()
    {
        if (Reset)
        {
            Reset = !Reset;
            Init();
            //calculatedOnce = !calculatedOnce;
        }

        //if (SaveToPrecomputed)
        //{
        //    SaveToPrecomputed = !SaveToPrecomputed;
        //    this.TraversableGraph = new Graph<GridNode>();
        //    TraversableGraph.AddNodes(ToNodeList(GetTraversableWalkingPoints()));
        //    //TraversableGraph.AddNodes(ToNodeList(GetTraversableLadderPoints()));
        //    ConnectTraversePoints();
        //    this.PrecomputedGraph.SaveGraph(this.TraversableGraph);
        //    EditorUtility.SetDirty(PrecomputedGraph);
        //    //calculatedOnce = !calculatedOnce;
        //}
        //if (SaveToPaths)
        //{
        //    SaveToPaths = !SaveToPaths;
        //    this.TraversableGraph.PrecomputeAllPaths();
        //    this.PrecomputedPaths.SavePrecomputedPaths(this.TraversableGraph);
        //}

        //if (!calculatedOnce)
        //{
        //    Init();
        //    calculatedOnce = true;
        //}

    }

    private void UpdateTileMapRelatedVariable()
    {
        _tilemapGrid = SuperTileMap.GetComponentInChildren<Grid>();
        _tilemapSize.x = SuperTileMap.m_Width * _tilemapGrid.cellSize.x * SuperTileMap.transform.localScale.x;
        _tilemapSize.y = SuperTileMap.m_Height * _tilemapGrid.cellSize.y * SuperTileMap.transform.localScale.y;
    }

    private void SetRowsAndCols(BoxCollider2D aabb)
    {
        _cellDimensions = new Vector2Int();
        _cellDimensions.x = Mathf.CeilToInt(_tilemapSize.x / aabb.bounds.size.x);
        _cellDimensions.y = Mathf.CeilToInt(_tilemapSize.y / aabb.bounds.size.y);
    }

    private Vector3 GetTopLeftCornerOfTileMap()
    {
        Vector3 toReturn = new();
        if (_tilemapGrid)
        {
            //Get the center of the first cell
            toReturn = _tilemapGrid.GetCellCenterWorld(new Vector3Int(0, -1, 0));
            //Offset to top left of the cell
            toReturn.x -= _tilemapGrid.cellSize.x * 0.5f * SuperTileMap.transform.localScale.x;
            toReturn.y += _tilemapGrid.cellSize.y * 0.5f * SuperTileMap.transform.localScale.y;
        }

        return toReturn;
    }



    private Vector3 GetStartingPointOfNewGrid(Vector2 playerColliderSize)
    {
        Vector3 startingPoint = GetTopLeftCornerOfTileMap();
        startingPoint.x += playerColliderSize.x / 2.0f;
        startingPoint.y -= playerColliderSize.y / 2.0f;
        return startingPoint;
    }

    private List<GridNode> ToNodeList(List<Vector3Int> pointList)
    {
        return pointList.Select(x => new GridNode(x)).ToList();
    }

    /// <summary>
    /// Given a node and a direction checks if there are opposing relative nodes that are traversable.
    /// E.g 0,0 and Dir - 1,1, will check them top right node and its opposing node bottom left --> if both are traversable result is true
    /// </summary>

    private bool HasOpposingRelativeTraversableTiles(Vector3Int gridPos, Vector3Int direction)
    {
        Vector3Int node = gridPos + direction;
        Vector3Int nodeOpposite = gridPos - direction;
        return _traversableTiles.Contains(node) && _traversableTiles.Contains(nodeOpposite);
    }

    private bool IsConnectingNTraversbleNodesInDirection(Vector3Int gridPos, Vector3Int direction, int count = 2)
    {
        int connectedNodesInDirection = 0;
        for (int i = 1; i <= count; i++)
        {
            if (_traversableTiles.Contains(gridPos + direction * i))
            {
                connectedNodesInDirection++;
            }
        }

        return connectedNodesInDirection == count;
    }

    public bool IsConnectingNode(Vector3Int gridPos)
    {
        List<Vector3Int> directionToTry = new List<Vector3Int>();
        directionToTry.Add(Vector3Int.right);
        directionToTry.Add(Vector3Int.down);
        directionToTry.Add(new Vector3Int(1, 1, 0));
        directionToTry.Add(new Vector3Int(1, -1, 0));
        directionToTry.Add(new Vector3Int(2, 1, 0));
        directionToTry.Add(new Vector3Int(2, -1, 0));



        for (int i = 0; i < directionToTry.Count; i++)
        {
            if (HasOpposingRelativeTraversableTiles(gridPos, directionToTry[i]))
            {
                //Test is a crossroad
                for (int j = 0; j < directionToTry.Count - 2; j++)
                {

                    if (j == i)
                    {
                        continue;
                    }

                    if (IsConnectingNTraversbleNodesInDirection(gridPos, directionToTry[j], 2)
                        || IsConnectingNTraversbleNodesInDirection(gridPos, -directionToTry[j], 2))
                    {
                        return false;
                    }
                }

                if (_30degSlopeTiles.Contains(gridPos))
                {
                    if (_traversableTiles.Contains(gridPos + new Vector3Int(2, 1, 0)) || _traversableTiles.Contains(gridPos + new Vector3Int(2, -1, 0))
                        || _traversableTiles.Contains(gridPos + new Vector3Int(-2, 1, 0)) || _traversableTiles.Contains(gridPos + new Vector3Int(-2, -1, 0)))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        //Zero connection in any single direction --> unique
        return false;
    }

    private List<Vector3Int> GetTraversableWalkingPoints()
    {
        var walkableNodesSimplified = new List<Vector3Int>(_traversableTiles);
        walkableNodesSimplified = walkableNodesSimplified
            .Where(cell =>
            {
                return !IsConnectingNode(cell);
            }
            ).ToList();
        return walkableNodesSimplified;
    }

    private void DrawConnectionGizmos()
    {
        foreach (GridNode node in TraversableGraph.Nodes)
            foreach (var connection in node.Connections)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_tilemapGrid.GetCellCenterWorld(node.PositionInGrid),
                    _tilemapGrid.GetCellCenterWorld(connection.Neighbor.PositionInGrid));
            }
    }

    private void DrawTraversableGridCells()
    {
        foreach (Vector3Int tile in _traversableTiles)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GetPosition(tile), 0.1f);
        }

        foreach (Vector3Int tile in _30degSlopeTiles)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(GetPosition(tile), 0.15f);
        }
    }

    private void OnDrawGizmosSelected()
    {

        DrawDebugNodes();
        DrawConnectionGizmos();
        DrawTraversableGridCells();
    }


    public Vector3 GetPosition(Vector3Int tile)
    {
        return _tilemapGrid.GetCellCenterWorld(tile);
    }

    public GridNode GetClosestVisibleNode(Vector3 worldPosition, Vector3 agentPosition)
    {
        //TODO filter out any other thing than ground
        var minDistance = float.MaxValue;
        GridNode nodeToReturn = null;


        foreach (GridNode node in TraversableGraph.Nodes)
        {
            Vector3 nodeWolrdPos = _tilemapGrid.GetCellCenterWorld(node.PositionInGrid);
            Vector3 rayDir = (worldPosition - nodeWolrdPos).normalized;

            RaycastHit2D hit2d = Physics2D.Raycast(worldPosition, rayDir, 100, LayerMask.GetMask("Ground"));
            var distanceToNode = Vector3.Distance(nodeWolrdPos, worldPosition);
            var distanceToRayHit = Vector3.Distance(worldPosition, hit2d.point);

            if (distanceToNode <= distanceToRayHit)
                if (distanceToNode < minDistance)
                {
                    minDistance = distanceToNode;
                    nodeToReturn = node;
                }
        }


        Debug.DrawLine(worldPosition, _tilemapGrid.GetCellCenterWorld(nodeToReturn.PositionInGrid), Color.green, 1.0f);
        return nodeToReturn;
    }

    public Vector3Int GetClosestTraversableTile(Vector3 worldPosition)
    {
        Vector3Int toGridCell = _tilemapGrid.WorldToCell(worldPosition);
        return _traversableTiles.OrderBy(x => Vector3Int.Distance(x, toGridCell)).FirstOrDefault();
    }

    public Platform GetPlatform(Vector3Int walkableTile = new Vector3Int())
    {
        return this.Platforms.FirstOrDefault(x => x.IsValid && (x.TraversableCells.Contains(walkableTile)));
    }

    public Platform GetPlatform(GridNode one, GridNode two)
    {
        if (one.Equals(two))
            return null;

        return Platforms.FirstOrDefault(x =>
            (x.EdgeNodeOne.Equals(one) || x.EdgeNodeOne.Equals(two)) && (x.EdgeNodeTwo.Equals(one) || x.EdgeNodeTwo.Equals(two)));
    }

    public GridNode GetClosestPlatformNode(Vector3Int walkableTile)
    {
        var platform = GetPlatform(walkableTile);
        if (platform != null)
        {
            if (Vector3Int.Distance(walkableTile, platform.EdgeNodeOne.PositionInGrid) <
                Vector3Int.Distance(walkableTile, platform.EdgeNodeTwo.PositionInGrid))
            {
                return platform.EdgeNodeOne;
            }

            return platform.EdgeNodeTwo;
        }
        return null;
    }

    private void DrawDebugNodes()
    {
        foreach (GridNode node in TraversableGraph.Nodes)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_tilemapGrid.GetCellCenterWorld(node.PositionInGrid), 0.2f);
        }

        //foreach (var tile in _traversableTiles)
        //{
        //    Gizmos.color = Color.green;

        //    Gizmos.DrawWireSphere(_tilemapGrid.GetCellCenterWorld(tile), 0.2f);
        //}


    }

    public LayerMask LadderMask;
    public LayerMask NotTraversable;
    public LayerMask GroundMask;
    public LayerMask PlatformMask;
    public bool IsLadder(Vector3Int nodeGridPosition)
    {
        //return LadderTilemap.HasTile(nodeGridPosition + new Vector3Int(0, 1, 0));
        //return CellOverlapsLayer(nodeGridPosition, LayerMask.GetMask("Ladder"));
        return CellOverlapsLayer(nodeGridPosition, LadderMask);

    }

    public bool CellOverlapsLayer(Vector3Int nodeGridPosition, LayerMask mask, float percentageOfTileFromCenter = 0.9f)
    {
        Collider2D freeSpaceCollider = Physics2D.OverlapBox(
            _tilemapGrid.GetCellCenterWorld(nodeGridPosition), _tileSize * percentageOfTileFromCenter, 0, mask.value);
        if (freeSpaceCollider) return true;

        return false;
    }

    public bool RayCollidesWith(Vector3 posStart, Vector3 posEnd, LayerMask mask, float bias = 0.0f)
    {
        var dir = posEnd - posStart;
        var worldEndPos = posEnd + dir.normalized * bias;
        RaycastHit2D hit = Physics2D.Linecast(posStart,
            worldEndPos, mask);
        if (hit) return true;
        return false;
    }

    /// <summary>
    /// Check is a current tile is considered ground or not. 
    /// Second argument is used to check if a platform should be treated as ground or not.
    /// If check from grid pos is higher that current one, one-way platform is active
    /// </summary>
    public bool IsGround(Vector3Int gridPosition,Vector3Int checkFromGridPos)
    {

        if (gridPosition.y < checkFromGridPos.y)
            return CellOverlapsLayer(gridPosition, GroundMask) || CellOverlapsLayer(gridPosition,PlatformMask);
        else 
            return CellOverlapsLayer(gridPosition, GroundMask);


    }

    public bool OnGroundOrPlatform(Vector3Int gridPosition)
    {
        var posStart = _tilemapGrid.GetCellCenterWorld(gridPosition) + new Vector3(0, -_tileSize.y / 2.0f, 0);
        return 
            RayCollidesWith(posStart, _tilemapGrid.GetCellCenterWorld(gridPosition + Vector3Int.down), GroundMask, 0.1f)
            ||
            RayCollidesWith(posStart, _tilemapGrid.GetCellCenterWorld(gridPosition + Vector3Int.down), PlatformMask, 0.1f)
            ;
    }

    public bool IsGroundOrSlope(Vector3Int gridPosition)
    {
        return CellOverlapsLayer(gridPosition, LayerMask.GetMask("Ground", "Platform"), 0.1f);
    }

    private GridNode NodeOnPosition(Vector3Int p)
    {
        return TraversableGraph.Nodes.FirstOrDefault(x => x.PositionInGrid == p);
    }


    private Platform GetConnectingNodeInDirection(Vector3Int startingPosition, Vector3Int gridDirection, int allowedLength = 100)
    {
        var platformCells = new List<Vector3Int>();
        for (var j = 1; j < allowedLength; j++)
        {
            Vector3Int nodeReached = startingPosition + gridDirection * j;

            if (_traversableTiles.Contains(nodeReached))
            {
                if (TraversableGraph.Nodes.Any(n => n.PositionInGrid == nodeReached))
                {
                    //Added platform contained tiles
                    //Start Position included
                    for (int i = 0; i <= j; i++)
                    {
                        Vector3Int gridCellInPlatform = startingPosition + gridDirection * i;
                        platformCells.Add(gridCellInPlatform);
                    }

                    return new Platform(platformCells, this.TraversableGraph);
                }
            }
            else
            {
                break;
            }
        }

        return null;
    }

    private void AddPlatform(GridNode node, Vector3Int dir, int allowedLength = 100)
    {
        var platform = GetConnectingNodeInDirection(node.PositionInGrid, dir, allowedLength);
        if (platform != null)
        {
            if (platform.IsValid)
            {
                Vector2 start = GetPosition(platform.EdgeNodeOne.PositionInGrid);
                Vector2 end = GetPosition(platform.EdgeNodeTwo.PositionInGrid);
                
                if (!Physics2D.Linecast(start, end, NotTraversable))
                {
                    TraversableGraph.AddTwoWayConnection(platform.EdgeNodeOne, platform.EdgeNodeTwo,
                        Vector3Int.Distance(platform.EdgeNodeOne.PositionInGrid, platform.EdgeNodeTwo.PositionInGrid));
                    Platforms.Add(platform);
                }
               
            }
            else
            {
                Debug.Log($"Platform {platform.TraversableCells[0]} - {platform.TraversableCells[platform.TraversableCells.Count - 1]} is invalid");
            }
        }
    }

    private void ConnectTraversePoints()
    {
        foreach (var node in TraversableGraph.Nodes)
        {

            //If is in a platform -- Connect to the Right
            if (_traversableTiles.Contains(node.PositionInGrid))
            {
                //Connect to right
                AddPlatform(node, Vector3Int.right);
                AddPlatform(node, new Vector3Int(2, 1, 0));
                AddPlatform(node, new Vector3Int(2, -1, 0));
                AddPlatform(node, new Vector3Int(1, 1, 0));
                AddPlatform(node, new Vector3Int(1, -1, 0));
            }

            //Is a ladder-- Check up direction aswell
            if (IsLadder(node.PositionInGrid))
            {
                AddPlatform(node, Vector3Int.right, 2);
                AddPlatform(node, Vector3Int.up);
            }
        }
    }

    public bool IsTraversable(Vector3Int positionInGrid, Vector3Int checkedFrom)
    {
       return !IsGround(positionInGrid, checkedFrom) && !CellOverlapsLayer(positionInGrid, NotTraversable);
    }
    public bool OnGroundOrPlatformGrid(Vector3Int positionInGrid)
    {
        var gridPositionDown = positionInGrid + Vector3Int.down;
        return !IsGround(gridPositionDown,positionInGrid) && !CellOverlapsLayer(gridPositionDown, NotTraversable);
    }

    private void SetTraversablePoints()
    {
        for (var y = 0; y < SuperTileMap.m_Height; y++)
            for (var x = 0; x < SuperTileMap.m_Width; x++)
            {
                Vector3Int positionInGrid = new(x, -y);
                //Ladder tile is a tile that is found on ladder layer
                if (IsLadder(positionInGrid))
                {
                    _traversableTiles.Add(positionInGrid);
                    continue;
                }


                Vector3Int bottomTile = positionInGrid + Vector3Int.down;
                if (
                    IsGround(positionInGrid, positionInGrid) == false 
                    && OnGroundOrPlatform(positionInGrid)
                    && IsTraversable(positionInGrid+Vector3Int.up,positionInGrid)) 
                    _traversableTiles.Add(new Vector3Int(x, -y));


                Vector3 startingPositionOnbottomOfTile = _tilemapGrid.GetCellCenterWorld(positionInGrid);
                Vector3 positionOfBottomTIle = _tilemapGrid.GetCellCenterWorld(positionInGrid + Vector3Int.down);
                ContactFilter2D conmFilter2D = new ContactFilter2D();
                conmFilter2D.layerMask = LayerMask.GetMask("Ground", "Platform");
                conmFilter2D.useLayerMask = true;
                List<RaycastHit2D> hits = new List<RaycastHit2D>();
                var hitCount = Physics2D.Linecast(startingPositionOnbottomOfTile, positionOfBottomTIle,
                    conmFilter2D, hits);
                Vector2 dirOfRay = (positionOfBottomTIle - startingPositionOnbottomOfTile).normalized;
                if (hitCount > 1)
                {
                    _30degSlopeTiles.Add(positionInGrid);
                }

            }
    }
}