using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DungeonEvaluation))]
public class DungeonEvaluationEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DungeonEvaluation Evaluator = (DungeonEvaluation)target;

        if (GUILayout.Button("Strategic Resource Control Evaluation"))
        {
            Evaluator.StrategicResourceControl();
            Evaluator.StrategicResourceControlBalance();
        }

        if (GUILayout.Button("Area Control Evaluation"))
        {
            Evaluator.AreaControlEquation();
            Evaluator.AreaControlBalanceEquation();
        }

        if (GUILayout.Button("Exploration Evaluation"))
        {
            if (Evaluator.CellDungeon)
            {
                Evaluator.CellularExplorationEquation();
                Evaluator.CellularExplorationBalanceEquation();
            }
            else
            {
                Evaluator.ExplorationEquation();
                Evaluator.ExplorationBalanceEquation();
            }
        }

    }

}
