using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileRack))]
public class TileRackEditor : Editor
{

    public override void OnInspectorGUI()
    {
        TileRack t = (TileRack)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Add Random Tile"))
        {
            t.AddRandomTile();
        }

        if (GUILayout.Button("Empty Rack"))
        {
            t.EmptyRack();
        }
    }

}
