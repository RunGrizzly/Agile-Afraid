using UnityEditor;
using UnityEngine;

public class GridDataEditorWindow : EditorWindow
{
    private GridData targetObject;

    // Fixed window size
    private const int windowWidth = 400;
    private const int windowHeight = 300;

    public static void Open(GridData obj)
    {
        var window = GetWindow<GridDataEditorWindow>("My Scriptable Object");
        window.targetObject = obj;

        // Set fixed window size
        window.minSize = new Vector2(windowWidth, windowHeight);
        window.maxSize = new Vector2(windowWidth, windowHeight);

        window.Show();
    }

    private void OnGUI()
    {
        // if (targetObject == null)
        // {
        //     EditorGUILayout.LabelField("No object assigned.");
        //     return;
        // }
        //
        // // Draw your ScriptableObject fields here, for example:
        // EditorGUI.BeginChangeCheck();
        //
        // targetObject.someInt = EditorGUILayout.IntField("Some Int", targetObject.someInt);
        // targetObject.someString = EditorGUILayout.TextField("Some String", targetObject.someString);
        //
        // if (EditorGUI.EndChangeCheck())
        // {
        //     EditorUtility.SetDirty(targetObject);
        // }
    }
}