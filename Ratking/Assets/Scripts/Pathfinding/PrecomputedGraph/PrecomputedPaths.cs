using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShortestPathSerializable
{
    public int ReachedByNodeIndex = 0;
    public float MinimumDistanceToNode = float.MaxValue;
}

[System.Serializable]
public class ShortestPathsSerializable
{
    public List<ShortestPathSerializable> ShortestPathList = new List<ShortestPathSerializable>();

    public ShortestPathsSerializable(ShortestPaths<GridNode> shortestPathFromNode, Graph<GridNode> graph)
    {
        if (shortestPathFromNode != null)
        {
            //foreach (var node in graph.Nodes)
            //{
            //    var shortestPathSeraizPathSerializable = new ShortestPathSerializable();
            //    var shortestPath = shortestPathFromNode.GetShortestPath(node);
            //    if (shortestPath == null)
            //    {
            //        shortestPath = new ShortestPath<GridNode>();
            //        shortestPath.MinDistance = float.MaxValue;
            //        shortestPath.ReachedByNode = null;
            //    }



            //    if (shortestPath.ReachedByNode == null)
            //    {
            //        shortestPathSeraizPathSerializable.ReachedByNodeIndex = -1;
            //    }
            //    else
            //    {
            //        shortestPathSeraizPathSerializable.ReachedByNodeIndex = graph.Nodes.FindIndex(x =>
            //            x.PositionInGrid == shortestPath.ReachedByNode.PositionInGrid);
            //    }

            //    shortestPathSeraizPathSerializable.MinimumDistanceToNode = shortestPath.MinDistance;
            //    ShortestPathList.Add(shortestPathSeraizPathSerializable);
            //}






            //foreach (var shortestPath in shortestPathFromNode.Paths)
            //{
            //    var sp = new ShortestPathSerializable();

            //    if (shortestPath.Value.ReachedByNode == null)
            //    {
            //        sp.ReachedByNodeIndex = -1;
            //    }
            //    else
            //    {
            //        sp.ReachedByNodeIndex = graph.Nodes.FindIndex(x =>
            //            x.PositionInGrid == shortestPath.Value.ReachedByNode.PositionInGrid);
            //    }

            //    sp.MinimumDistanceToNode = shortestPath.Value.MinDistance;
            //    ShortestPathList.Add(sp);

            //}
        }
    }
}

[CreateAssetMenu(fileName = "New Graph Paths", menuName = "Precomp Graph Paths")]

[System.Serializable]
public class PrecomputedPaths : ScriptableObject
{
    public List<ShortestPathsSerializable> ShortestPathsToNode;

    public void SavePrecomputedPaths(Graph<GridNode> graph)
    {
        //ShortestPathsToNode = new List<ShortestPathsSerializable>();
        //int index = 0;




        //foreach (KeyValuePair<GridNode, ShortestPaths<GridNode>> pair in graph.PrecomputedShortestPaths)
        //{
        //    var shortestPaths = new ShortestPathsSerializable(pair.Value, graph);
        //    ShortestPathsToNode.Add(shortestPaths);
        //    index++;
        //}

    }


}
