using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrammarDungeon : MonoBehaviour
{

    [System.Serializable]
    public struct NodeConnection
    {
        public GrammarNode Node1;
        public GrammarNode Node2;
        //Used for local grammar node indexs when replacing nodes with a rule
        public int p1;
        public int p2;
    }

    public bool DebugMode = false;
    public bool DebugTiles = false;

    public int TotalTiles = 0;
    public GameObject FloorTile;
    public List<NodeConnection> NodeConnections;
    public List<GrammarNode> Nodes;
    public CellularAutomata.Tile[,] Tiles = null;
    public GrammarNode EntranceNode;
    public GrammarNode ExitNode;

    public int DungeonWidth = 0;
    public int DungeonLength = 0;
    Vector3 DungeonOffset;

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

        foreach (GrammarNode _node in ProcessNodes) // _node.ProcessRule modifies the Nodes list so must be seperated from the main List<GrammarNode>
        {
            _node.ProcessRule();
        }


        foreach (GrammarNode _node in Nodes)
        {
            if (_node.Symbol == GrammarRuleSet.Symbol.entrance)
            {
                EntranceNode = _node;
            }
            if (_node.Symbol == GrammarRuleSet.Symbol.exit)
            {
                ExitNode = _node;
            }
        }

        Debug.Log("......Done");
        Debug.Log("...Done");

        RepositionNodes(); // Reposition nodes after everythings been messed with
        RegenerateRooms();
        CollateTiles();
    }

    //Stitching together all the cellular automata rooms 
    void CollateTiles()
    {
        //Figure out the dimensions of the dungeon
        int MostLeft = 0;
        int MostRight = 0;
        int MostUp = 0;
        int MostDown = 0;
        foreach (GrammarNode node in Nodes)
        {
            Vector3 pos = node.transform.position;
            if (pos.x < MostLeft) MostLeft = (int)pos.x;
            else if (pos.x > MostRight) MostRight = (int)pos.x;
            if (pos.z < MostDown) MostDown = (int)pos.z;
            else if (pos.z > MostUp) MostUp = (int)pos.z;
        }
        //Each node position is centered, so offset by half width(17) to get true dungeon size
        int SizeLeft = MostLeft;
        int SizeRight = MostRight;
        int SizeUp = MostUp;
        int SizeDown = MostDown;

        if (MostLeft < 8) SizeLeft -= 8 + 1;// If size is below 0 add 1 to offset (-2 > 2 is a difference of 4 but -2x -> 2x is 5 spaces)
        else SizeLeft -= 8;

        SizeRight += 8;

        if (MostDown < 8) SizeDown -= 8 + 1;
        else SizeDown -= 8;

        SizeUp += 8;

        DungeonWidth = SizeRight - SizeLeft;
        DungeonLength = SizeUp - SizeDown;
        DungeonOffset = new Vector3(SizeLeft + 1, 0, SizeDown + 1);

        Tiles = new CellularAutomata.Tile[DungeonWidth, DungeonLength];
        //print("MostLeft: " + MostLeft + " MostRight: " + MostRight + " MostDown: " + MostDown + " MostUp: " + MostUp);
        //print("SizeLeft: " + SizeLeft + " SizeRight: " + SizeRight + " SizeDown: " + SizeDown + " SizeUp: " + SizeUp);
        //print("Dungeon Size: " + DungeonWidth + " " + DungeonLength);

        foreach (GrammarNode node in Nodes)
        {
            CellularAutomata cell = node.gameObject.GetComponent<CellularAutomata>();
            Vector3 pos = cell.transform.position;
            for (int i = 0; i < 17; ++i)
            {
                for (int j = 0; j < 17; ++j)
                {
                    Tiles[(int)pos.x + Mathf.Abs(MostLeft) + i, (int)pos.z + Mathf.Abs(MostDown) + j] = cell.GetTile(i, j);
                }
            }
        }

    }

    public void ResetFloodCheck()
    {
        for (int i = 0; i < DungeonWidth; ++i)
        {
            for (int j = 0; j < DungeonLength; ++j)
            {
                Tiles[i, j].Flood = false;
            }
        }
    }

    public int CheckDistance(float _sx, float _sy, float _tx, float _ty)
    {
        Vector2 _Cursor = new Vector2(_sx, _sy);
        Vector2 _Target = new Vector2(_tx, _ty);
        int Distance = 0;
        while (!_Cursor.Equals(_Target))
        {
            if(_Cursor.x != _Target.x)
            {
                int dif = (int)(_Target.x - _Cursor.x);
                _Cursor.x += dif / Mathf.Abs(dif);
                Distance += 1;
            }
            else if(_Cursor.y != _Target.y)
            {
                int dif = (int)(_Target.y - _Cursor.y);
                _Cursor.y += dif / Mathf.Abs(dif);
                Distance += 1;
            }
        }
        return Distance;
    }

    public int FloodSearch(int _sx, int _sy, int _tx, int _ty)
    {
        ResetFloodCheck();
        Vector2 _TargetTile = new Vector2(_tx, _ty);
        Vector2 _Cursor = Vector2.zero;
        Queue<Vector2> SearchNodes = new Queue<Vector2>();
        SearchNodes.Enqueue(new Vector2(_sx, _sy));
        int MapCoverage = 0;

        while (SearchNodes.Count > 0)
        {
            MapCoverage += 1;
            _Cursor = SearchNodes.Dequeue();
            if (_Cursor != _TargetTile)
            {
                if (_Cursor.x - 1 > 0) SearchNodes.Enqueue(new Vector2(_Cursor.x - 1, _Cursor.y));
                if (_Cursor.x + 1 < DungeonWidth) SearchNodes.Enqueue(new Vector2(_Cursor.x + 1, _Cursor.y));
                if (_Cursor.y - 1 > 0) SearchNodes.Enqueue(new Vector2(_Cursor.x, _Cursor.y - 1));
                if (_Cursor.y + 1 < DungeonLength) SearchNodes.Enqueue(new Vector2(_Cursor.x, _Cursor.y + 1));
            }
            else
            {
                return MapCoverage;
            }
        }

        return MapCoverage;
    }

    public void RepositionNodes()
    {
        foreach (GrammarNode node in Nodes)
        {
            if (node.Symbol == GrammarRuleSet.Symbol.entrance)
            {
                node.Reposition();
                break;
            }
        }

    }

    public void RegenerateRooms()
    {
        foreach (GrammarNode Node in Nodes)
        {
            Node.gameObject.GetComponent<CellularAutomata>().GenerateRoom();
        }
    }

    public void RenameNodes()
    {
        foreach (GrammarNode Node in Nodes)
        {
            Node.gameObject.name = Node.Symbol.ToString();
        }
    }

    public void RemoveMarks()
    {
        foreach (GrammarNode node in Nodes)
        {
            node.Marked = false;
        }
    }

    public int GetTotalTiles()
    {
        if (TotalTiles == 0)
        {
            foreach (GrammarNode node in Nodes)
            {
                TotalTiles += node.gameObject.GetComponent<CellularAutomata>().TotalTiles;
            }
            return TotalTiles;
        }
        else
        {
            return TotalTiles;
        }
    }

    public void AddNode(GrammarNode _NewNode)
    {
        Nodes.Add(_NewNode);
        _NewNode.ID = NodeID;
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
            if (Connect.Node1 == _NodeInConnection || Connect.Node2 == _NodeInConnection)
            {
                MarkedConnections.Add(Connect);
            }
        }
        //Remove the matching connections
        foreach (NodeConnection Marked in MarkedConnections)
        {
            NodeConnections.Remove(Marked);
        }
        MarkedConnections.Clear();
    }

    public int GetNodeID()
    {
        return NodeID;
    }

    public List<Vector2> GetMonsterPositions()
    {
        List<Vector2> Monsters = new List<Vector2>();
        for (int i = 0; i < DungeonWidth; ++i)
        {
            for (int j = 0; j < DungeonLength; ++j)
            {
                if(Tiles[i,j].Monster)
                {
                    Monsters.Add(new Vector2(i, j));
                }
            }
        }
        return Monsters;
    }

    public List<Vector2> GetTreasurePositions()
    {
        List<Vector2> Treasure = new List<Vector2>();
        for (int i = 0; i < DungeonWidth; ++i)
        {
            for (int j = 0; j < DungeonLength; ++j)
            {
                if (Tiles[i, j].Reward)
                {
                    Treasure.Add(new Vector2(i, j));
                }
            }
        }
        return Treasure;
    }

    public Vector2 GetEntrancePosition()
    {
        for (int i = 0; i < DungeonWidth; ++i)
        {
            for (int j = 0; j < DungeonLength; ++j)
            {
                if (Tiles[i, j].Entrance)
                {
                    return new Vector2(i, j);
                }
            }
        }
        return Vector2.zero;
    }

    public Vector2 GetExitPosition()
    {
        for (int i = 0; i < DungeonWidth; ++i)
        {
            for (int j = 0; j < DungeonLength; ++j)
            {
                if (Tiles[i, j].Exit)
                {
                    return new Vector2(i, j);
                }
            }
        }
        return Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if (DebugMode && Application.isEditor && this.enabled)
        {
            foreach (NodeConnection connection in NodeConnections)
            {
                Gizmos.DrawLine(connection.Node1.transform.position, connection.Node2.transform.position);
            }
            foreach (GrammarNode node in Nodes)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node.transform.position, 0.5f);
                UnityEditor.Handles.Label(node.transform.position + transform.up, node.name);
            }           
        }
        if (DebugTiles && Tiles != null)
        {
            for (int i = 0; i < DungeonWidth; ++i)
            {
                for (int j = 0; j < DungeonLength; ++j)
                {
                    if (Tiles[i, j].Monster)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(new Vector3(i + DungeonOffset.x, 0, j + DungeonOffset.z), 0.5f);
                    }
                    else if (Tiles[i, j].Reward)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(new Vector3(i + DungeonOffset.x, 0, j + DungeonOffset.z), 0.5f);
                    }
                    else if (Tiles[i, j].State == 1)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(new Vector3(i + DungeonOffset.x, 0, j + DungeonOffset.z), 0.5f);
                    }

                    else if (Tiles[i, j].State == 0)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(new Vector3(i + DungeonOffset.x, 0, j + DungeonOffset.z), 0.5f);
                    }
                }
            }
        }
    }
}
