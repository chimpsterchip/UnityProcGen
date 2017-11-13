using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonEvaluation : MonoBehaviour {

    public GrammarDungeon Dungeon;
    public CellularAutomata CellDungeon;

    public float SRC_Score;
    public float SRCB_Score;
    public float AreaControlScore;
    public float AreaControlBalScore;
    public float ExplorationScore;
    public float ExplorationBalScore;
  
    List<Vector2> Monsters;
    List<Vector2> Treasure;
    List<Vector2> ExitsAndMonsters;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float AreaControlBalanceEquation()
    {
        List<float> JValues = new List<float>();
        List<float> IValues = new List<float>();
        float JValue = 0f;
        float IValue = 0f;
        foreach(Vector2 I in ExitsAndMonsters)
        {
            foreach(Vector2 J in ExitsAndMonsters)
            {
                if (J != I)
                {
                    float ABS = Mathf.Abs(MapCoverageValue(I) - MapCoverageValue(J));
                    float MAX = Mathf.Max(MapCoverageValue(I), MapCoverageValue(J));
                    JValues.Add(ABS / MAX);
                }
            }
            JValue = 0f;
            foreach (float val in JValues)
            {
                JValue += val;
            }
            IValues.Add(JValue);
        }
        IValue = 0f;
        foreach (float val in IValues)
        {
            IValue += val;
        }
        float Count = ExitsAndMonsters.Count;
        float FinalValue = 1f - (1f / (Count * (Count - 1f)));
        FinalValue *= IValue;
        AreaControlBalScore = FinalValue;
        return FinalValue;
    }

    public float AreaControlEquation()
    {
        if (CellDungeon)
        {
            ExitsAndMonsters = CellDungeon.Monsters;
            ExitsAndMonsters.Add(CellDungeon.Entrance);
            ExitsAndMonsters.Add(CellDungeon.Exit);           
        }
        else if(Dungeon)
        {
            ExitsAndMonsters = Dungeon.GetMonsterPositions();
            ExitsAndMonsters.Add(Dungeon.GetEntrancePosition());
            ExitsAndMonsters.Add(Dungeon.GetExitPosition());
        }
        else
        {
            return -1337f;
        }
        List<float> Values = new List<float>();

        foreach(Vector2 N in ExitsAndMonsters)
        {
            Values.Add(MapCoverageValue(N));
        }
        float Sum = 0f;
        foreach (float val in Values)
        {
            Sum += val;
        }
        float FinalValue = 1f / (float)Dungeon.GetTotalTiles() * Sum;
        AreaControlScore = FinalValue;
        return FinalValue;
    }

    float MapCoverageValue(Vector2 N)
    {
        float MapCoverage = 0f;
        if (CellDungeon)
        {
            for (int i = 0; i < CellDungeon.RoomWidth; ++i)
            {
                for (int j = 0; j < CellDungeon.RoomHeight; ++j)
                {
                    if (CellDungeon.Tiles[i, j].State == 0)
                    {
                        float Value = AreaControlSafetyValue(new Vector2(i, j), N);
                        if (Value < 0.35f)
                        {
                            MapCoverage += 1;
                        }
                    }
                }
            }
        }
        else if (Dungeon)
        {
            for (int i = 0; i < Dungeon.DungeonWidth; ++i)
            {
                for (int j = 0; j < Dungeon.DungeonLength; ++j)
                {
                    if (Dungeon.Tiles[i, j].State == 0)
                    {
                        float Value = AreaControlSafetyValue(new Vector2(i, j), N);
                        if (Value < 0.35f)
                        {
                            MapCoverage += 1;
                        }
                    }
                }
            }
        }
        return MapCoverage;
    }

    float AreaControlSafetyValue(Vector2 T, Vector2 N)
    {
        List<float> Values = new List<float>();
        foreach (Vector2 M in ExitsAndMonsters)
        {
            if (M != N)
            {
                Values.Add(DistanceValue(T, N, M));
            }
        }
        float Min = 1000;
        foreach (float val in Values)
        {
            if (val < Min)
            {
                Min = val;
            }
        }
        return Min;
    }

    public float StrategicResourceControlBalance()
    {
        float TValue = 0f;
        List<float> TValues = new List<float>();
        float IValue = 0f;
        List<float> IValues = new List<float>();
        float JValue = 0f;
        List<float> JValues = new List<float>();

        foreach (Vector2 T in Treasure)
        {
            IValue = 0f;
            IValues.Clear();
            foreach (Vector2 I in Monsters)
            {
                JValue = 0f;
                JValues.Clear();
                foreach (Vector2 J in Monsters)
                {
                    if (!J.Equals(I)) JValues.Add(Mathf.Abs(SafetyValue(T, I) - SafetyValue(T, J)));
                }
                foreach (float val in JValues)
                {
                    JValue += val;
                }
                IValues.Add(JValue);
            }
            foreach (float val in IValues)
            {
                IValue += val;
            }
            TValues.Add(IValue);
        }
        foreach (float val in TValues)
        {
            TValue += val;
        }
        float FinalValue = 1f - (1f/(float)Treasure.Count * (float)Monsters.Count * (Monsters.Count - 1f)) * TValue;
        SRCB_Score = FinalValue;
        return FinalValue;
    }


    public float StrategicResourceControl()
    {
        if (CellDungeon)
        {
            Treasure = CellDungeon.Treasures;
            Monsters = CellDungeon.Monsters;
        }
        else if(Dungeon)
        {
            Treasure = Dungeon.GetTreasurePositions();
            Monsters = Dungeon.GetMonsterPositions();
        }
        else
        {
            return -1337f;
        }
        List<float> TreasureValue = new List<float>();

        foreach(Vector2 T in Treasure)
        {
            TreasureValue.Add(EnemySafety(T));
        }
        float Sum = 0;
        foreach(float val in TreasureValue)
        {
            Sum += val;
        }
        float FinalValue = (1f / (float)Treasure.Count) * Sum;
        SRC_Score = FinalValue;
        return FinalValue;
    }

    float EnemySafety(Vector2 T)
    {
        List<float> EnemyValue = new List<float>();
        foreach(Vector2 E in Monsters)
        {
            EnemyValue.Add(SafetyValue(T, E));
        }
        float Max = -1000;
        foreach (float val in EnemyValue)
        {
            if (val > Max)
            {
                Max = val;
            }
        }
        return Max;
    }
            
    public float SafetyValue(Vector2 T, Vector2 E)
    {
        List<float> Values = new List<float>();
        foreach(Vector2 Jnemy in Monsters)
        {
            if (Jnemy != E)
            {
                Values.Add(DistanceValue(T, E, Jnemy));
            }
        }
        float Min = 1000;
        foreach(float val in Values)
        {
            if(val < Min)
            {
                Min = val;
            }
        }
        return Min;
    }

    float DistanceValue(Vector2 K, Vector2 E, Vector2 J)
    {
        float DistKJ = Dungeon.CheckDistance(K.x, K.y, J.x, J.y);
        float DistKE = Dungeon.CheckDistance(K.x, K.y, E.x, E.y);
        float DistValue = (DistKJ - DistKE) / (DistKJ + DistKE);
        return Mathf.Max(0, DistValue);
    }

    public float ExplorationBalanceEquation()
    {
        //float ExStart = ExplorationValue(Dungeon.EntranceNode, Dungeon.ExitNode);
        //float ExEnd = ExplorationValue(Dungeon.ExitNode, Dungeon.EntranceNode);
        float ExStart = ExplorationValue(Dungeon.GetEntrancePosition(), Dungeon.GetExitPosition());
        float ExEnd = ExplorationValue(Dungeon.GetExitPosition(), Dungeon.GetEntrancePosition());
        float BalStart = Mathf.Abs(ExStart - ExEnd);
        BalStart = BalStart / Mathf.Max(ExStart, ExEnd);
        float BalEnd = Mathf.Abs(ExEnd - ExStart);
        BalEnd = BalStart / Mathf.Max(ExEnd, ExStart);
        float be = 1f - 1f/(2f * (2f - 1f));
        be = be * (BalStart + BalEnd);
        ExplorationBalScore = be;       
        return be;
    }

    public float ExplorationEquation()
    {
        //float Start2End = ExplorationValue(Dungeon.EntranceNode, Dungeon.ExitNode);
        //float End2Start = ExplorationValue(Dungeon.ExitNode, Dungeon.EntranceNode);
        float Start2End = ExplorationValue(Dungeon.GetEntrancePosition(), Dungeon.GetExitPosition());
        float End2Start = ExplorationValue(Dungeon.GetExitPosition(), Dungeon.GetEntrancePosition());
        Start2End = 1f / (2f - 1f) * (Start2End / Dungeon.GetTotalTiles());
        End2Start = 1f / (2f - 1f) * (End2Start / Dungeon.GetTotalTiles());
        float fe = (1f / 2f) * (Start2End + End2Start);
        ExplorationScore = fe;
        return fe;
    }

    public float ExplorationEquation(GrammarNode _StartNode, GrammarNode _EndNode)
    {
        int Start2End = ExplorationValue(_StartNode, _EndNode);
        int End2Start = ExplorationValue(_EndNode, _StartNode);
        Start2End = (1 / 2 - 1) * (Start2End / Dungeon.GetTotalTiles());
        End2Start = (1 / 2 - 1) * (End2Start / Dungeon.GetTotalTiles());
        float fe = (1 / 2) * (Start2End + End2Start);
        return fe;
    }

    int ExplorationValue(GrammarNode _StartNode, GrammarNode _EndNode)
    {
        Dungeon.RemoveMarks();
        int MapCoverage = 0;
        Queue<GrammarNode> SearchNodes = new Queue<GrammarNode>();
        SearchNodes.Enqueue(_StartNode);
        GrammarNode _Cursor = null;
        while (SearchNodes.Count > 0)
        {
            _Cursor = SearchNodes.Dequeue();
            _Cursor.Marked = true;
            if (_Cursor != _EndNode)
            {
                foreach (GrammarNode child in _Cursor.Children)
                {
                    if (!child.Marked)
                    {
                        SearchNodes.Enqueue(child);
                    }
                }
                foreach(GrammarNode parent in _Cursor.Parents)
                {
                    if(!parent.Marked)
                    {
                        SearchNodes.Enqueue(parent);
                    }
                }

                MapCoverage += _Cursor.gameObject.GetComponent<CellularAutomata>().TotalTiles;
            }
            else
            {
                return MapCoverage;              
            }            
        }
        return 0;
    }

    public float CellularExplorationBalanceEquation()
    {
        float ExStart = ExplorationValue(CellDungeon.Entrance, CellDungeon.Exit);
        float ExEnd = ExplorationValue(CellDungeon.Exit, CellDungeon.Entrance);
        float BalStart = Mathf.Abs(ExStart - ExEnd);
        BalStart = BalStart / Mathf.Max(ExStart, ExEnd);
        float BalEnd = Mathf.Abs(ExEnd - ExStart);
        BalEnd = BalStart / Mathf.Max(ExEnd, ExStart);
        float be = 1f - 1f / (2f * (2f - 1f));
        be = be * (BalStart + BalEnd);
        ExplorationBalScore = be;
        return be;
    }

    public float CellularExplorationEquation()
    {
        float Start2End = ExplorationValue(CellDungeon.Entrance, CellDungeon.Exit);
        float End2Start = ExplorationValue(CellDungeon.Exit, CellDungeon.Entrance);
        Start2End = 1f / (2f - 1f) * (Start2End / CellDungeon.TotalTiles);
        End2Start = 1f / (2f - 1f) * (End2Start / CellDungeon.TotalTiles);
        float fe = (1f / 2f) * (Start2End + End2Start);
        ExplorationScore = fe;
        return fe;
    }

    public float ExplorationEquation(Vector2 _StartNode, Vector2 _EndNode)
    {
        float Start2End = ExplorationValue(_StartNode, _EndNode);
        float End2Start = ExplorationValue(_EndNode, _StartNode);
        Start2End = 1f / (2f - 1f) * (Start2End / CellDungeon.TotalTiles);
        End2Start = 1f / (2f - 1f) * (End2Start / CellDungeon.TotalTiles);
        float fe = (1f / 2f) * (Start2End + End2Start);
        ExplorationScore = fe;
        return fe;
    }

    int ExplorationValue(Vector2 _StartNode, Vector2 _EndNode)
    {
        CellularAutomata.Tile[,] Tiles;
        float RoomWidth, RoomHeight;
        if (CellDungeon)
        {
            CellDungeon.RemoveFloodMarkers();
            Tiles = CellDungeon.Tiles;
            RoomWidth = CellDungeon.RoomWidth;
            RoomHeight = CellDungeon.RoomHeight;
            
        }
        else
        {
            Dungeon.ResetFloodCheck();
            Tiles = Dungeon.Tiles;
            RoomWidth = Dungeon.DungeonWidth;
            RoomHeight = Dungeon.DungeonLength;
        }
        
        int MapCoverage = 0;
        Queue<Vector2> SearchNodes = new Queue<Vector2>();
        SearchNodes.Enqueue(_StartNode);
        Vector2 _Cursor = Vector2.zero;
        while (SearchNodes.Count > 0)
        {
            _Cursor = SearchNodes.Dequeue();
            if (_Cursor.x < 0 || _Cursor.x > RoomWidth || _Cursor.y < 0 || _Cursor.y > RoomHeight)
            {
                continue;
            }
            Tiles[(int)_Cursor.x, (int)_Cursor.y].Flood = true;
            if (_Cursor != _EndNode)
            {
                if (_Cursor.x + 1 < RoomWidth)
                {
                    if (!Tiles[(int)_Cursor.x + 1, (int)_Cursor.y].Flood)
                    {
                        Tiles[(int)_Cursor.x + 1, (int)_Cursor.y].Flood = true;
                        SearchNodes.Enqueue(new Vector2(_Cursor.x + 1, _Cursor.y));
                    }
                }
                if (_Cursor.x - 1 > 0)
                {
                    if (!Tiles[(int)_Cursor.x - 1, (int)_Cursor.y].Flood)
                    {
                        Tiles[(int)_Cursor.x - 1, (int)_Cursor.y].Flood = true;
                        SearchNodes.Enqueue(new Vector2(_Cursor.x - 1, _Cursor.y));
                    }
                }
                if (_Cursor.y + 1 < RoomHeight)
                {
                    if (!Tiles[(int)_Cursor.x, (int)_Cursor.y + 1].Flood)
                    {
                        Tiles[(int)_Cursor.x, (int)_Cursor.y + 1].Flood = true;
                        SearchNodes.Enqueue(new Vector2(_Cursor.x, _Cursor.y + 1));
                    }
                }
                if (_Cursor.y - 1 > 0)
                {
                    if (!Tiles[(int)_Cursor.x, (int)_Cursor.y - 1].Flood)
                    {
                        Tiles[(int)_Cursor.x, (int)_Cursor.y - 1].Flood = true;
                        SearchNodes.Enqueue(new Vector2(_Cursor.x, _Cursor.y - 1));
                    }
                }

                MapCoverage += 1;
            }
            else
            {
                return MapCoverage;
            }
        }
        return 0;
    }

}
