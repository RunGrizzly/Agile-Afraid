using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LetterBlock))]
public class LetterBlockEditor : Editor
{

    public override void OnInspectorGUI()
    {


        LetterBlock t = (LetterBlock)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Build Block"))
        {
            t.BuildWeighted();
        }

        if (GUILayout.Button("Empty Block"))
        {
            t.Empty();
        }


    }

}
