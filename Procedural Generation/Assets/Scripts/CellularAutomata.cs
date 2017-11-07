using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{

    struct Tile
    {
        public int State;
        public int NewState;
        public bool locked;
    }

    struct NeighborData
    {
        public int NumNeighbors;
        public int AvailableNeighbors;
    }

    [System.Serializable]
    public struct TileConnections
    {
        public bool UP;
        public bool RIGHT;
        public bool DOWN;
        public bool LEFT;
    }

    public bool DebugMode = false;
    public int RoomWidth = 17;
    public int RoomHeight = 17;
    [Range(0f, 1f)]
    public float InitTileChance = 0.48f;
    public int WallThreshold = 5;
    public int FloorThreshold = 5;
    public GameObject Floor;
    public int ConnectorWidth = 7;
    public TileConnections Connectors;
    Tile[,] Tiles = new Tile[0, 0];
    List<GameObject> RoomTiles;



    // Use this for initialization
    void Awake()
    {
        Init();
    }

    void Init()
    {
        Tiles = new Tile[RoomWidth, RoomHeight];
        RoomTiles = new List<GameObject>();

        if (ConnectorWidth > RoomWidth)
        {
            ConnectorWidth = RoomWidth;
        }
        if (ConnectorWidth > RoomHeight)
        {
            ConnectorWidth = RoomHeight;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateRoom()
    {
        if (RoomTiles == null) Init();
        ClearRoom();
        FillRandom();

        CalculateConnectors();
        CreateConnectors();

        for (int i = 0; i < 4; ++i)
        {
            Iterate();
            ChangeStates();
        }

        DrawRoom();
    }

    public void DrawRoom()
    {
        float ScaleValue = 10 * Floor.transform.localScale.x;
        Vector3 AdjustVec = new Vector3(RoomWidth / 2 * ScaleValue, 0f, RoomHeight / 2 * ScaleValue);
        for (int i = 0; i < RoomWidth; ++i)
        {
            for (int j = 0; j < RoomHeight; ++j)
            {
                if (Tiles[i, j].State == 0)
                {
                    GameObject newTile = Instantiate(Floor);
                    newTile.transform.parent = this.gameObject.transform;
                    newTile.transform.localPosition = new Vector3(i * ScaleValue, 0f, j * ScaleValue) - AdjustVec;
                    RoomTiles.Add(newTile);
                    //RoomTiles.Add(Instantiate(Floor, new Vector3(i * (10 * Floor.transform.localScale.x), 0f, j * (10 * Floor.transform.localScale.z)), Quaternion.identity, this.transform));
                }
            }
        }
    }
    
    //Calculate where the tile will need to connect to another tile
    void CalculateConnectors()
    {
        if (!GetComponent<GrammarNode>())
        {
            Debug.LogWarning("No Grammar Node Attached To: " + gameObject.name);
            return;
        }
        else
        {
            List<GrammarNode> Children = GetComponent<GrammarNode>().GetChildren();
            foreach(GrammarNode Child in Children)
            {
                Vector3 Direction = Child.transform.position - transform.position;
                if(Direction.x > 0)
                {
                    Connectors.RIGHT = true;
                }
                else if(Direction.x < 0)
                {
                    Connectors.LEFT = true;
                }
                else if (Direction.z > 0)
                {
                    Connectors.UP = true;
                }
                else if (Direction.z < 0)
                {
                    Connectors.DOWN = true;
                }
            }
            List<GrammarNode> Parents = GetComponent<GrammarNode>().GetParents();
            foreach (GrammarNode Parent in Parents)
            {
                Vector3 Direction = Parent.transform.position - transform.position;
                if (Direction.x > 0)
                {
                    Connectors.RIGHT = true;
                }
                else if (Direction.x < 0)
                {
                    Connectors.LEFT = true;
                }
                else if (Direction.z > 0)
                {
                    Connectors.UP = true;
                }
                else if (Direction.z < 0)
                {
                    Connectors.DOWN = true;
                }
            }
        }
    }

    //Creates the openning to connect two tiles together
    void CreateConnectors()
    {
        //Set the border to walls and lock them
        for (int i = 0; i < RoomWidth; ++i)
        {
            Tiles[i, 0].NewState = 1;
            Tiles[i, 0].locked = true;
            Tiles[i, RoomHeight - 1].NewState = 1;
            Tiles[i, RoomHeight - 1].locked = true;
        }
        for (int i = 0; i < RoomHeight; ++i)
        {
            Tiles[0, i].NewState = 1;
            Tiles[0, i].locked = true;
            Tiles[RoomWidth - 1, i].NewState = 1;
            Tiles[RoomWidth - 1, i].locked = true;
        }
        //Create the entrances to the room
        int ConnectorPadding = (RoomWidth - ConnectorWidth) / 2;
        if (Connectors.DOWN)
        {
            for (int i = ConnectorPadding; i < RoomWidth - ConnectorPadding; ++i)
            {
                Tiles[i, 0].NewState = 0;
                Tiles[i, 1].NewState = 0;
                Tiles[i, 1].locked = true;
            }
        }
        if (Connectors.RIGHT)
        {
            for (int i = ConnectorPadding; i < RoomHeight - ConnectorPadding; ++i)
            {
                Tiles[RoomWidth - 1, i].NewState = 0;
                Tiles[RoomWidth - 2, i].NewState = 0;
                Tiles[RoomWidth - 2, i].locked = true;
            }
        }
        if (Connectors.UP)
        {
            for (int i = ConnectorPadding; i < RoomWidth - ConnectorPadding; ++i)
            {
                Tiles[i, RoomHeight - 1].NewState = 0;
                Tiles[i, RoomHeight - 2].NewState = 0;
                Tiles[i, RoomHeight - 2].locked = true;
            }
        }
        if (Connectors.LEFT)
        {
            for (int i = ConnectorPadding; i < RoomWidth - ConnectorPadding; ++i)
            {
                Tiles[0, i].NewState = 0;
                Tiles[1, i].NewState = 0;
                Tiles[1, i].locked = true;
            }
        }
    }

    //Loop through all the tiles and change their state
    public void Iterate()
    {
        for (int i = 0; i < RoomWidth; ++i)
        {
            for (int j = 0; j < RoomHeight; ++j)
            {
                NeighborData Neighbors = CheckNeighbors(i, j);
                if (!Tiles[i, j].locked)
                {
                    if (Tiles[i, j].State == 1) //If the tile is a wall
                    {
                        if (Neighbors.AvailableNeighbors <= 5 && Neighbors.NumNeighbors > 0)
                        {
                            Tiles[i, j].NewState = 1;
                        }
                        else if (Neighbors.NumNeighbors >= WallThreshold)
                        {
                            Tiles[i, j].NewState = 1;
                        }
                        else
                        {
                            Tiles[i, j].NewState = 0;
                        }
                    }
                    else if (Tiles[i, j].State == 0) //If the tile is a floor
                    {
                        if (Neighbors.AvailableNeighbors <= 5 && Neighbors.NumNeighbors > 1)
                        {
                            Tiles[i, j].NewState = 1;
                        }
                        else if (Neighbors.NumNeighbors >= FloorThreshold)
                        {
                            Tiles[i, j].NewState = 1;
                        }
                        else
                        {
                            Tiles[i, j].NewState = 0;
                        }
                    }
                }
            }
        }
    }

    public void ChangeStates()
    {
        for (int i = 0; i < RoomWidth; ++i)
        {
            for (int j = 0; j < RoomHeight; ++j)
            {
                Tiles[i, j].State = Tiles[i, j].NewState;
            }
        }
    }

    NeighborData CheckNeighbors(int x, int y)
    {
        NeighborData Data = new NeighborData();
        Data.AvailableNeighbors = 8;
        Data.NumNeighbors = 0;

        int OffsetX, OffsetY;
        OffsetX = 0;
        OffsetY = 1;

        for (int j = 0; j < 8; ++j)
        {
            if (x + OffsetX >= 0 && x + OffsetX < RoomWidth && y + OffsetY >= 0 && y + OffsetY < RoomHeight)
            {
                if (Tiles[x + OffsetX, y + OffsetY].State == 1)
                {
                    Data.NumNeighbors++;
                }
            }
            else
            {
                Data.AvailableNeighbors--;
            }
            switch (j)
            {
                case 0:
                    OffsetX += 1;
                    break;
                case 1:
                case 2:
                    OffsetY -= 1;
                    break;
                case 3:
                case 4:
                    OffsetX -= 1;
                    break;
                case 5:
                case 6:
                case 7:
                    OffsetY += 1;
                    break;
            }
        }
        return Data;
    }

    public void FillRandom()
    {
        float RandomChance;
        for (int i = 0; i < RoomWidth; ++i)
        {
            for (int j = 0; j < RoomHeight; ++j)
            {
                RandomChance = Random.Range(0f, 1f);
                Tiles[i, j].locked = false;
                if (RandomChance < InitTileChance)
                {
                    Tiles[i, j].State = 1;
                }
                else
                {
                    Tiles[i, j].State = 0;
                }
            }
        }
    }

    void ClearRoom()
    {
        foreach (GameObject obj in RoomTiles)
        {
            GameObject.Destroy(obj);
        }
        RoomTiles.Clear();
    }

    private void OnDrawGizmos()
    {
        if (DebugMode && Application.isPlaying && this.enabled)
        {
            float ScaleValue = 10 * Floor.transform.localScale.x;
            Vector3 AdjustVec = new Vector3(RoomWidth / 2 * ScaleValue, 0f, RoomHeight / 2 * ScaleValue);
            for (int i = 0; i < RoomWidth; ++i)
            {
                for (int j = 0; j < RoomHeight; ++j)
                {
                    UnityEditor.Handles.Label(transform.position - AdjustVec + new Vector3(i  * ScaleValue, 0f, j  * ScaleValue), CheckNeighbors(i, j).NumNeighbors.ToString() + "\n" + Tiles[i, j].State + " " + Tiles[i, j].NewState);
                    //UnityEditor.Handles.Label(new Vector3((float)i * 10, 0f, (float)j * 10) + transform.up,"Coords: " + i.ToString() + " " + j.ToString() + "\nState: " + Tiles[i,j].ToString() + " Neighbors: " + CheckNeighbors(i, j).ToString());
                }
            }
            //UnityEditor.Handles.Label(new Vector3(RoomWidth/2 * (10 * Floor.transform.localScale.x), 0f, 0f) + transform.up, "UP");
            //UnityEditor.Handles.Label(new Vector3(RoomWidth * (10 * Floor.transform.localScale.x), 0f, RoomHeight / 2 * (10 * Floor.transform.localScale.x)) + transform.up, "RIGHT");
            //UnityEditor.Handles.Label(new Vector3(RoomWidth / 2 * (10 * Floor.transform.localScale.x), 0f, RoomHeight * (10 * Floor.transform.localScale.x)) + transform.up, "DOWN");
            //UnityEditor.Handles.Label(new Vector3(0f, 0f, RoomHeight / 2 * (10 * Floor.transform.localScale.x)) + transform.up, "LEFT");

            UnityEditor.Handles.Label(transform.position - AdjustVec + new Vector3(RoomWidth / 2 * ScaleValue, 0f, -1f) + transform.up, "DOWN");
            UnityEditor.Handles.Label(transform.position - AdjustVec + new Vector3(RoomWidth * ScaleValue, 0f, RoomHeight / 2 * ScaleValue) + transform.up, "RIGHT");
            UnityEditor.Handles.Label(transform.position - AdjustVec + new Vector3(RoomWidth / 2 * ScaleValue, 0f, RoomHeight * ScaleValue) + transform.up, "UP");
            UnityEditor.Handles.Label(transform.position - AdjustVec + new Vector3(-1f, 0f, RoomHeight / 2 * ScaleValue) + transform.up, "LEFT");
        }
    }
}
