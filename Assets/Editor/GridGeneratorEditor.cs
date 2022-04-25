using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {


        GridGenerator t = (GridGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Initialise Grid"))
        {
            t.StartCoroutine(t.Generate(t.gridDims));
        }

        if (GUILayout.Button("Force Update Nav"))
        {
            Brain.ins.eventManager.e_navUpdate.Invoke();
        }

        // if (GUILayout.Button("Build Filled Grid"))
        // {
        //     t.Generate();

        //     foreach (LetterBlock block in t.letterBlocks)
        //     {
        //         block.BuildFromDistribution(0, 100);
        //     }
        // }

        if (GUILayout.Button("Set All Empty"))
        {
            foreach (LetterBlock block in t.letterBlocks)
            {
                block.Empty();
            }
            //  Brain.ins.eventManager.e_navUpdate.Invoke();
        }

        if (GUILayout.Button("Set All Filled"))
        {
            foreach (LetterBlock block in t.letterBlocks)
            {
                block.BuildFromDistribution(0, 100);
            }
            // Brain.ins.eventManager.e_navUpdate.Invoke();
        }
    }

}
