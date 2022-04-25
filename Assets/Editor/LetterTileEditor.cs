using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(LetterTile))]
public class LetterTileEditor : Editor
{

    public override void OnInspectorGUI()
    {


        LetterTile t = (LetterTile)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Build Random"))
        {
            t.BuildWeighted();
        }

    }

}
