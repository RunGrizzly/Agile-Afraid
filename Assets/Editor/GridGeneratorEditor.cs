using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{

    //  public GUISkin gridSkin;

    public override void OnInspectorGUI()
    {



        int selGridInt = 0;

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

        GUI.skin = t.editorSkin;



        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical("Box");



        selGridInt = GUILayout.SelectionGrid(selGridInt, BrainControl.Get().grid.letterBlocks.Cast<LetterBlock>().ToList().Select(x => x.name).ToArray(), t.letterBlocks.GetLength(0));

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();


        // foreach(LetterBlock block in BrainControl.Get().grid.letterBlocks)
        // {
        //     if (GUILayout.Button(block.name))
        //         {

        //         }
        // }



    }

}
