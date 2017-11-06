using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarDungeon : MonoBehaviour {

    [System.Serializable]
    public struct NodeConnection
    {
        public GrammarNode Node1;
        public GrammarNode Node2;
        //Used for local grammar node indexs when replacing nodes with a rule
        public int p1;
        public int p2;
    }

    public GameObject FloorTile;
    public List<NodeConnection> NodeConnections;
    public List<GrammarNode> Nodes;

    private int NodeID = 0;

    private void Start()
    {
        //NodeConnections = new List<NodeConnection>();
        //Nodes = new List<GrammarNode>();

        RenameNodes();
        RepositionNodes();
        RegenerateRooms();
    }

    private void Update()
    {
    }

    public void ProcessNodes()
    {
        Debug.Log("Processing Nodes...");

        Debug.Log("...Checking Rules...");
        foreach (GrammarNode _node in Nodes)
        {
            _node.CheckRules();           
        }
        Debug.Log("......Done");
        Debug.Log("...Processing Rules...");
        List<GrammarNode> ProcessNodes = new List<GrammarNode>();
        ProcessNodes.AddRange(Nodes);
        foreach (GrammarNode _node in ProcessNodes) // _node.ProcessRule modifies the Nodes list so must be seperated
        {
            _node.ProcessRule();
        }
        Debug.Log("......Done");
        Debug.Log("...Done");

        RepositionNodes(); // Reposition nodes after everythings been messed with
    }

    public void RepositionNodes()
    {
        Nodes[0].Reposition();
    }

    public void RegenerateRooms()
    {
        foreach(GrammarNode Node in Nodes)
        {
            Node.gameObject.GetComponent<CellularAutomata>().GenerateRoom();
        }
    }

    public void RenameNodes()
    {
        foreach(GrammarNode Node in Nodes)
        {
            Node.gameObject.name = Node.Symbol.ToString();
        }
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

    public void AddConnection(GrammarNode _Node1, GrammarNode _Node2)
    {
        NodeConnection _NewConnection;
        _NewConnection.Node1 = _Node1;
        _NewConnection.Node2 = _Node2;
        _NewConnection.p1 = -1;
        _NewConnection.p2 = -1;

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
        if (Application.isEditor && this.enabled)
        {
            foreach(NodeConnection i in NodeConnections)
            {
                Gizmos.DrawLine(i.Node1.transform.position, i.Node2.transform.position);
            }
            foreach (GrammarNode node in Nodes)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node.transform.position, 0.5f);
                UnityEditor.Handles.Label(node.transform.position + transform.up, node.name);
            }
        }
    }
}
