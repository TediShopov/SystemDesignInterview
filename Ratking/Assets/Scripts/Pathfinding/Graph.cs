using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class NodeConnection<T>
{
    [SerializeReference]
    public T Neighbor;
    public float Weight;
}

[System.Serializable]
public class INode<T>
{

    public List<NodeConnection<T>> Connections { get; set; }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }
}

public class Graph<T> where T : INode<T>
{
    public HashSet<T> Nodes;
    //public Dictionary<T, ShortestPaths<T>> PrecomputedShortestPaths;
    public void AddNode(T node)
    {
        if (!Nodes.Contains(node))
        {
            Nodes.Add(node);
            //PrecomputedShortestPaths.Add(node, null);
        }
    }

    public void AddNodes(List<T> nodesToAdd)
    {
        foreach (T node in nodesToAdd) AddNode(node);
    }

    public void RemoveNode(T nodeToRemove)
    {
        foreach (T node in Nodes)
            node.Connections = node.Connections.Where(x => !x.Neighbor.Equals(nodeToRemove)).ToList();

        Nodes.Remove(nodeToRemove);
    }

    public Graph(List<T> nodes)
    {
        Nodes = new HashSet<T>(nodes);
        //PrecomputedShortestPaths = new Dictionary<T, ShortestPaths<T>>();
    }

    public void Clear()
    {
        Nodes.Clear();
        //PrecomputedShortestPaths.Clear();
    }

    public void AddNeighbor(T node, T neighborNode, float weight = 1)
    {
        if (Nodes.Contains(node) && Nodes.Contains(neighborNode))
            node.Connections.Add(new NodeConnection<T> { Neighbor = neighborNode, Weight = weight });
    }


    public void AddTwoWayConnection(T nodeOne, T nodeTwo, float weight = 1)
    {
        AddNeighbor(nodeOne, nodeTwo, weight);
        AddNeighbor(nodeTwo, nodeOne, weight);
    }

    public void ClearAllNeighbors(T nodeIndex)
    {
        nodeIndex.Connections.Clear();
    }



    //public ShortestPaths<T> GetAllShortestPathsDjikstraLimitedConnection(T startNode, float maxConnectionDistance)
    //{
    //    Debug.LogWarning("Djikstra initaited");
    //    if (!Nodes.Contains(startNode)) return null;

    //    var shortestPathsToNodes = new ShortestPaths<T>();
    //    shortestPathsToNodes.AddShortestPath(startNode, startNode, 0);


    //    var unvisitedNodes = new List<T>(Nodes);

    //    T currNode = startNode;


    //    //Stop when path is found or no path is found
    //    while (unvisitedNodes.Count > 1)
    //    {



    //        //Node with lowest distance
    //        //Remove from unvisited, visiting now
    //        unvisitedNodes.Remove(currNode);
    //        var shortestDistanceToCurrent = shortestPathsToNodes.GetShortestPathDistance(currNode);

    //        if (shortestDistanceToCurrent >= maxConnectionDistance)
    //        {
    //            if (shortestDistanceToCurrent == float.MaxValue)
    //            {
    //                Debug.Log("Djikstra max connection limit reached");
    //                break;
    //            }
    //        }
    //        else
    //        {
    //            //List<int> currentNeighbourNodes = GetAllNeighbors(currentNode);
    //            foreach (var connection in currNode.Connections)
    //            {
    //                shortestPathsToNodes.AddShortestPath(connection.Neighbor, currNode,
    //                    shortestDistanceToCurrent + connection.Weight);
    //            }
    //        }



    //        currNode = unvisitedNodes
    //            .OrderBy(x => shortestPathsToNodes.GetShortestPathDistance(x))
    //            .First();


    //        //No more connected nodes
    //        //if (shortestPathsToNodes.GetShortestPathDistance(currNode) == float.MaxValue)
    //        //{
    //        //   con
    //        //}
    //    }

    //    return shortestPathsToNodes;

    //}
    //Return unvisited nodes

    public List<T> BacktrackPath(ShortestPaths<T> paths, T startNode, T endNode)
    {
        //Backtrack
        //If path found

        if (startNode == null || endNode == null || paths == null)
        {
            return null;
        }

        if (!paths.Paths.ContainsKey(endNode) || !paths.Paths.ContainsKey(startNode))
        {
            return null;
        }

        List<T> pathToEndNode = new List<T>();

        pathToEndNode.Add(endNode);

        T currentNode = endNode;
        while (!pathToEndNode.Last().Equals(startNode))
        {
            var reachedBy = paths.GetReachedByNode(currentNode);

            if (reachedBy.Node == null || reachedBy.MinDistance == float.MaxValue)
            {
                pathToEndNode.Clear();
                return pathToEndNode;
            }
            pathToEndNode.Add(reachedBy.Node);
            currentNode = reachedBy.Node;
        }
        pathToEndNode.Reverse();
        return pathToEndNode;
    }

    public List<T> GetShortestPath(T startNode, T endNode)
    {
        ShortestPaths<T> shortestPaths = this.GetShortestPathDjikstra(startNode, endNode);
        return this.BacktrackPath(shortestPaths, startNode, endNode);
    }

    public ShortestPaths<T> GetShortestPathDjikstra(T startNode, T endNode = null)
    {
        if (!Nodes.Contains(startNode)) return null;

        var shortestPathsToNodes = new ShortestPaths<T>();
        var unvisitedNodes = new HashSet<T>(Nodes);
        shortestPathsToNodes.AddShortestPath(startNode, startNode, 0);
        T currNode = startNode;

        while (unvisitedNodes.Count != 0 && currNode != null)
        {
            if (currNode.Equals(endNode))
                break;

            unvisitedNodes.Remove(currNode);
            foreach (var connection in currNode.Connections)
            {
                var shortestDistanceToCurrent = shortestPathsToNodes.GetReachedByNode(currNode).MinDistance;
                shortestPathsToNodes.AddShortestPath(connection.Neighbor, currNode,
                    shortestDistanceToCurrent + connection.Weight);
            }
            //Next node to check -- shortest reach by weight
            currNode = shortestPathsToNodes.GetClosestNode(unvisitedNodes);
        }

        return shortestPathsToNodes;
    }


}

public class ShortestPaths<T> where T : class
{
    public Dictionary<T, ReachedByNode<T>> Paths;


    public ShortestPaths() { Paths = new Dictionary<T, ReachedByNode<T>>(); }

    public ReachedByNode<T> GetReachedByNode(T node) => (Paths.ContainsKey(node)) ? Paths[node] : new ReachedByNode<T>();

    public T GetClosestNode(HashSet<T> nodes) => nodes.OrderBy(x => GetReachedByNode(x).MinDistance).FirstOrDefault();

    public void AddShortestPath(T nodeTo, T reachedBym, float weight)
    {
        ReachedByNode<T> toAdd = new(reachedBym, weight);
        if (Paths.ContainsKey(nodeTo))
            Paths[nodeTo].TryUpdate(toAdd);
        else
            Paths.Add(nodeTo, toAdd);
    }
}

public class ReachedByNode<T> where T : class
{
    public float MinDistance;
    public T Node;

    public ReachedByNode(T reached = null, float weight = float.MaxValue)
    {
        Node = reached;
        MinDistance = weight;
    }

    public void TryUpdate(ReachedByNode<T> otherPath)
    {
        if (otherPath.MinDistance < MinDistance)
        {
            MinDistance = otherPath.MinDistance;
            Node = otherPath.Node;
        }
    }
}
