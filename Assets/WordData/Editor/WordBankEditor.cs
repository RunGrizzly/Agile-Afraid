using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WordBankFromTextAsset))]
public class WordBankEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WordBankFromTextAsset wordBank = (WordBankFromTextAsset)target;
        if (GUILayout.Button("Rebuild Bank"))
        {
            wordBank.RebuildBank();
        }

        if (GUILayout.Button("Filter Bank"))
        {
            wordBank.FilterBank();
        }

    }
}