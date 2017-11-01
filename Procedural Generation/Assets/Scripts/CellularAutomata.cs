using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularAutomata : MonoBehaviour {

    struct Tile
    {
        public int State;
        public int NewState;
    }

    struct NeighborData
    {
        public int NumNeighbors;
        public int AvailableNeighbors;
    }

    public bool DebugMode = false;
    public int RoomWidth = 5;
    public int RoomHeight = 5;
    [Range(0f, 1f)]
    public float InitTileChance = 0.5f;
    public int WallThreshold = 4;
    public int FloorThreshold = 5;
    public GameObject Floor;
    Tile[,] Tiles = new Tile[0, 0];
    List<GameObject> RoomTiles;



    // Use this for initialization
    void Start() {
        Tiles = new Tile[RoomWidth, RoomHeight];
        RoomTiles = new List<GameObject>();
        FillRandom();
        DrawRoom();
    }

    // Update is called once per frame
    void Update() {

    }

    public void GenerateRoom()
    {
        ClearRoom();
        FillRandom();
        for(int i = 0; i < 3; ++i)
        {
            Iterate();
            ChangeStates();
        }
        DrawRoom();
    }

    public void DrawRoom()
    {
        ClearRoom();
        for (int i = 0; i < RoomWidth; ++i)
        {
            for (int j = 0; j < RoomHeight; ++j)
            {
                if (Tiles[i, j].State == 0)
                {
                    RoomTiles.Add(Instantiate(Floor, new Vector3((float)i * 10, 0f, (float)j * 10), Quaternion.identity, this.transform));
                }
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
                if (Tiles[i, j].State == 1) //If the tile is a wall
                {
                    if(Neighbors.AvailableNeighbors <= 5 && Neighbors.NumNeighbors > 0)
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
        for(int i = 0; i < RoomWidth; ++i )
        {
            for(int j = 0; j < RoomHeight; ++j)
            {
                RandomChance = Random.Range(0f, 1f);
                if(RandomChance < InitTileChance)
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
        foreach(GameObject obj in RoomTiles)
        {
            GameObject.Destroy(obj);
        }
        RoomTiles.Clear();
    }

    private void OnDrawGizmos()
    {
        if (DebugMode && Application.isPlaying && this.enabled)
        {
            for (int i = 0; i < RoomWidth; ++i)
            {
                for (int j = 0; j < RoomHeight; ++j)
                {
                    UnityEditor.Handles.Label(new Vector3((float)i * 10, 0f, (float)j * 10) + transform.up, CheckNeighbors(i, j).NumNeighbors.ToString() + "\n" + Tiles[i,j].State + " " + Tiles[i,j].NewState);
                    //UnityEditor.Handles.Label(new Vector3((float)i * 10, 0f, (float)j * 10) + transform.up,"Coords: " + i.ToString() + " " + j.ToString() + "\nState: " + Tiles[i,j].ToString() + " Neighbors: " + CheckNeighbors(i, j).ToString());
                }
            }
        }
    }
}
