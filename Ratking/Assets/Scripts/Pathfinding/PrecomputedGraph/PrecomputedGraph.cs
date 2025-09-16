using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeConnectionSerializable
{
    public List<int> ConnectionIndices = new List<int>();
    public List<float> ConnecionWeight = new List<float>();
}


[CreateAssetMenu(fileName = "New Precomputed Graph", menuName = "PrecomputedGraph")]
[Serializable]
public class PrecomputedGraph : ScriptableObject
{
    public List<Vector3Int> Nodes;

    [SerializeField]
    public List<NodeConnectionSerializable> NodeConnections;

    public void SaveGraph(Graph<GridNode> graphToSave)
    {
        //Nodes = new List<Vector3Int>();
        //NodeConnections = new List<NodeConnectionSerializable>(graphToSave.Nodes.Count);

        //for (int i = 0; i < graphToSave.Nodes.Count; i++)
        //{
        //    var nodeToAdd = graphToSave.Nodes[i].PositionInGrid;

        //    Nodes.Add(nodeToAdd);
        //    //Connections[i] = new List<Vector3Int>();

        //    var nodeConneciton = new NodeConnectionSerializable();

        //    //Serach all neighbours
        //    foreach (var connection in graphToSave.Nodes[i].Connections)
        //    {
        //        var nodeIndex = graphToSave.Nodes.FindIndex(x=>x.PositionInGrid== connection.Neighbor.PositionInGrid);
        //        nodeConneciton.ConnectionIndices.Add(nodeIndex);
        //        nodeConneciton.ConnecionWeight.Add(connection.Weight);

        //    }
        //    NodeConnections.Add(nodeConneciton);

        //}
    }
}
