using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GrammarDungeon))]
public class GrammarDungeonEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GrammarDungeon Dungeon = (GrammarDungeon)target;

        if (GUILayout.Button("Process Rules"))
        {
            Dungeon.ProcessNodes();
        }
        if (GUILayout.Button("Reposition Rooms"))
        {
            Dungeon.RepositionNodes();
        }
        if (GUILayout.Button("Regenerate Rooms"))
        {
            Dungeon.RegenerateRooms();
        }
    }

}
