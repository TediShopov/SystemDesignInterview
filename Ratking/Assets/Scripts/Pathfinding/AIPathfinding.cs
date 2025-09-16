using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public struct PathTrack<Node>
{
    public Vector3 ExactStart;
    public Vector3 ExactDestination;
    public List<Node> Path;
    public int NodeReachedIndex;
    public Node StartNode => Path.ElementAtOrDefault(0);
    public Node DestinationNode => Path.ElementAtOrDefault(Path.Count - 1);
    public Node NodeReached => Path.ElementAtOrDefault(NodeReachedIndex);
    public Node NextNodeToFollow => Path.ElementAtOrDefault(NodeReachedIndex + 1);

    public bool ReachedDestinationNode() => (DestinationNode != null && DestinationNode.Equals(NodeReached));

}

public class AIPathfinding : MonoBehaviour
{
    [SerializeField] public PathfindingGrid TraversableGrid;
    public PathTrack<GridNode> PathTrack;

    // Start is called before the first frame update
    private void Awake()
    {
        ResetPathTrack();
    }

    private void ResetPathTrack()
    {
        PathTrack = new PathTrack<GridNode>();
        PathTrack.ExactStart = new Vector3();
        PathTrack.ExactDestination = new Vector3();
        PathTrack.Path = new List<GridNode>();
        PathTrack.NodeReachedIndex = -1;
    }

    public delegate void DestinationReachedAction();

    public event DestinationReachedAction OnDestinationReached;

    public bool ReachedDestinationPlatform => PathTrack.Path.ElementAtOrDefault(PathTrack.Path.Count - 2) != null &&
                                              PathTrack.Path[PathTrack.Path.Count - 2].Equals(PathTrack.NodeReached);



    public bool ReachedDestination =>
        Vector2.Distance(PathTrack.ExactDestination, transform.position) <= DistanceToReach;

    public float DistanceToReach = 0.1f;

    public bool ReachedNextNode()
    {
        if (PathTrack.NextNodeToFollow != null)
            return Vector2.Distance(TraversableGrid.GetPosition(PathTrack.NextNodeToFollow.PositionInGrid),
                transform.position) <= DistanceToReach;
        return false;
    }

    private void FixedUpdate()
    {
        if (PathTrack.Path != null && PathTrack.Path.Count > 0)
        {
            if (ReachedDestination)
                if (OnDestinationReached != null)
                    OnDestinationReached();

            if (ReachedNextNode()) PathTrack.NodeReachedIndex++;
        }
    }

    private void RemoveUnnecessaryNodeOnPlatform(int index, int indexSecond, Vector3Int pos)
    {
        GridNode first = PathTrack.Path[index];
        GridNode second = PathTrack.Path[indexSecond];

        //Get platofmr
        var platformFound = TraversableGrid.GetPlatform(first, second);
        if (platformFound != null && platformFound.IsValid)
        {
            if (platformFound.TraversableCells.Contains(pos))
            {
                Debug.LogWarning($"Removed Unnecesary node connection {index} - {indexSecond}");
                PathTrack.Path.RemoveAt(indexSecond);
            }
        }

        //if (TraversableGrid.IsGridTileContainedInPlatform(first, second, pos))
        //{
        //    Debug.LogWarning($"Removed Unnecesary node connection {index} - {indexSecond}");
        //    PathTrack.Path.RemoveAt(indexSecond);
        //}
    }

    /// <summary>
    /// Gets all platoforms connected to the node, and return a platofmr that contain both of the grid position
    /// </summary>
    /// <param name="node"></param>
    public bool AreTilesOnSamePlatform(GridNode node, Vector3Int startGridPos, Vector3Int destinationGridPos)
    {
        foreach (var connection in node.Connections)
        {
            var platform = this.TraversableGrid.GetPlatform(node, connection.Neighbor);
            if (platform.TraversableCells.Contains(startGridPos) && (platform.TraversableCells.Contains(destinationGridPos)))
                return true;
        }
        return false;
    }


    public void SetNewPath(Vector3 destination)
    {
        ResetPathTrack();
        Vector3Int startTile = TraversableGrid.GetClosestTraversableTile(transform.position);
        Vector3Int destinationTile = TraversableGrid.GetClosestTraversableTile(destination);

        PathTrack.ExactStart = TraversableGrid.GetPosition(startTile);
        PathTrack.ExactDestination = TraversableGrid.GetPosition(destinationTile);


        GridNode startNode = TraversableGrid.GetClosestPlatformNode(startTile);
        GridNode destinationNode = TraversableGrid.GetClosestPlatformNode(destinationTile);

        if (startNode != null && destinationNode != null)
        {
            Debug.LogWarning("Path Nodes are not null");
            if (startNode.Equals(destinationNode))
            {
                PathTrack.Path.Add(startNode);
                PathTrack.Path.Add(destinationNode);
                if (this.AreTilesOnSamePlatform(startNode, startTile, destinationTile))
                {
                    Debug.Log("On the same side of platform as destination");
                    this.PathTrack.NodeReachedIndex = 0;
                }
                else
                {
                    Debug.Log("On different platforms");

                }



            }
            else
            {
                var path = this.TraversableGrid.TraversableGraph.GetShortestPath(startNode, destinationNode);
                if (path!=null)
                {
                    PathTrack.Path = path;
                    if (PathTrack.Path.Count >= 2)
                        RemoveUnnecessaryNodeOnPlatform(1, 0,
                            TraversableGrid.GetClosestTraversableTile(PathTrack.ExactStart));

                    if (PathTrack.Path.Count >= 2)
                        RemoveUnnecessaryNodeOnPlatform(PathTrack.Path.Count - 2, PathTrack.Path.Count - 1,
                            TraversableGrid.GetClosestTraversableTile(PathTrack.ExactDestination));
                }
              
            }

            if (PathTrack.Path == null || PathTrack.Path.Count == 0)
                // SetNewPath(destination);
                ResetPathTrack();
            else
                DebugPath();
        }
        else
        {
            if (startNode != null)
            {
                Debug.LogWarning($"Start node is:  {startNode.PositionInGrid}");

            }
            else
            {
                Debug.LogWarning($"Destination node is:  {destinationNode.PositionInGrid}");

            }
        }
    }

    private void DebugPath()
    {
        Debug.Log($"Exact Start {PathTrack.ExactStart}");
        Debug.Log($"Exact Destination {PathTrack.ExactDestination}");
        foreach (GridNode node in PathTrack.Path) Debug.Log($"    Path point: {node.PositionInGrid}");
    }

    private void OnDrawGizmos()
    {
        if (PathTrack.Path != null && PathTrack.Path.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(PathTrack.ExactDestination, new Vector3(0.2f, 0.2f, 0.2f));
            Gizmos.DrawCube(this.TraversableGrid.GetPosition(PathTrack.DestinationNode.PositionInGrid), new Vector3(0.3f, 0.3f, 0.3f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(PathTrack.ExactStart, new Vector3(0.2f, 0.2f, 0.2f));
            Gizmos.DrawCube(this.TraversableGrid.GetPosition(PathTrack.StartNode.PositionInGrid), new Vector3(0.3f, 0.3f, 0.3f));

            Gizmos.color = Color.green;
            if (PathTrack.NodeReached != null)
            {
                Gizmos.DrawCube(this.TraversableGrid.GetPosition(PathTrack.NodeReached.PositionInGrid), new Vector3(0.3f, 0.3f, 0.3f));
            }

            Gizmos.color = new Color(252, 179, 10, 1);
            if (PathTrack.NextNodeToFollow != null)
            {
                Gizmos.DrawCube(this.TraversableGrid.GetPosition(PathTrack.NextNodeToFollow.PositionInGrid), new Vector3(0.3f, 0.3f, 0.3f));
            }

        }

    }
}