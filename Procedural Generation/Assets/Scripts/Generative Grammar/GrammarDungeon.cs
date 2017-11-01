using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarDungeon : MonoBehaviour {

    [System.Serializable]
    public struct NodeConnection
    {
        public GrammarNode Node1;
        public GrammarNode Node2;
        public int p1;
        public int p2;
    }

    public List<NodeConnection> NodeConnections;
    public List<GrammarNode> Nodes;

    private int NodeID = 0;

    private void Start()
    {
        NodeConnections = new List<NodeConnection>();
        Nodes = new List<GrammarNode>();
    }

    public void AddNode(GrammarNode _NewNode)
    {
        Nodes.Add(_NewNode);
        NodeID++;
    }

    public void RemoveNode(GrammarNode _NodeToRemove)
    {
        Nodes.Remove(_NodeToRemove);
    }

    public void AddConnection(NodeConnection _NewConnection)
    {
        NodeConnections.Add(_NewConnection);
    }

    public void RemoveConnection(NodeConnection _ConnectionToRemove)
    {
        NodeConnections.Remove(_ConnectionToRemove);
    }

    public void RemoveConnection(GrammarNode _NodeInConnection)
    {
        List<NodeConnection> MarkedConnections = new List<NodeConnection>();
        //Find Connections with the matching node
        foreach (NodeConnection Connect in NodeConnections)
        {
            if(Connect.Node1 == _NodeInConnection || Connect.Node2 == _NodeInConnection)
            {
                MarkedConnections.Add(Connect);
            }
        }
        //Remove the matching connections
        foreach(NodeConnection Marked in MarkedConnections)
        {
            NodeConnections.Remove(Marked);
        }
        MarkedConnections.Clear();
    }

    public int GetNodeID()
    {
        return NodeID;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && this.enabled)
        {
            foreach(NodeConnection i in NodeConnections)
            {

            }
        }
    }
}
