using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CellularAutomata))]
public class CellularAutomataEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CellularAutomata Automata = (CellularAutomata)target;

        if (GUILayout.Button("Generate Room"))
        {
            Automata.GenerateRoom();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Restart"))
        {
            Automata.FillRandom();
            Automata.DrawRoom();
        }
        if (GUILayout.Button("Iterate"))
        {
            Automata.Iterate();
        }
        if (GUILayout.Button("Change States"))
        {
            Automata.ChangeStates();
            Automata.DrawRoom();
        }
        GUILayout.EndHorizontal();
    }

}
