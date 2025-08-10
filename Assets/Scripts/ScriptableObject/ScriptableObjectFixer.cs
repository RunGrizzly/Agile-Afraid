// using UnityEditor;
// using UnityEngine;
//
// public class ScriptableObjectFixer : EditorWindow
// {
//     private bool applyGridDims = true;
//     private Vector2Int gridDims = new Vector2Int(5, 5);
//
//     private bool applyDeadZoneColorA = true;
//     private Color deadZoneColorA = Color.red;
//
//     private bool applyDeadZoneColorB = true;
//     private Color deadZoneColorB = Color.blue;
//
//     // Range sliders for all properties
//     private int applyRangeStart = 0;
//     private int applyRangeEnd = 0;
//
//     [MenuItem("Tools/Batch Edit LevelData")]
//     public static void ShowWindow()
//     {
//         GetWindow<ScriptableObjectFixer>("Batch Edit LevelData");
//     }
//
//     void OnGUI()
//     {
//         GUILayout.Label("Select Properties to Apply", EditorStyles.boldLabel);
//
//         applyGridDims = EditorGUILayout.Toggle("Apply GridDims", applyGridDims);
//         if (applyGridDims)
//         {
//             gridDims = EditorGUILayout.Vector2IntField("GridDims", gridDims);
//         }
//
//         EditorGUILayout.Space();
//
//         applyDeadZoneColorA = EditorGUILayout.Toggle("Apply DeadZone Color A", applyDeadZoneColorA);
//         if (applyDeadZoneColorA)
//         {
//             deadZoneColorA = EditorGUILayout.ColorField("DeadZone Color A", deadZoneColorA);
//         }
//
//         applyDeadZoneColorB = EditorGUILayout.Toggle("Apply DeadZone Color B", applyDeadZoneColorB);
//         if (applyDeadZoneColorB)
//         {
//             deadZoneColorB = EditorGUILayout.ColorField("DeadZone Color B", deadZoneColorB);
//         }
//
//         EditorGUILayout.Space();
//
//         GUILayout.Label("Apply Properties To Level Range (inclusive):", EditorStyles.boldLabel);
//
//         EditorGUILayout.BeginHorizontal();
//         applyRangeStart = EditorGUILayout.IntField("Start Level", Mathf.Max(0, applyRangeStart));
//         applyRangeEnd = EditorGUILayout.IntField("End Level", Mathf.Max(0, applyRangeEnd));
//         EditorGUILayout.EndHorizontal();
//
//         if (applyRangeEnd < applyRangeStart)
//         {
//             applyRangeEnd = applyRangeStart; // Clamp to start if invalid range
//         }
//
//         EditorGUILayout.Space();
//
//         if (GUILayout.Button("Apply to All SessionSettings Assets"))
//         {
//             ApplyToAllAssets();
//         }
//     }
//
//     private void ApplyToAllAssets()
//     {
//         string[] guids = AssetDatabase.FindAssets("t:SessionSettings");
//         int total = guids.Length;
//         int processed = 0;
//
//         foreach (string guid in guids)
//         {
//             string path = AssetDatabase.GUIDToAssetPath(guid);
//             SessionSettings settings = AssetDatabase.LoadAssetAtPath<SessionSettings>(path);
//
//             if (settings == null || settings.levels == null) continue;
//
//             bool changed = false;
//
//             int maxLevelIndex = settings.levels.Count - 1;
//
//             int startIndex = Mathf.Clamp(applyRangeStart, 0, maxLevelIndex);
//             int endIndex = Mathf.Clamp(applyRangeEnd, startIndex, maxLevelIndex);
//
//             for (int i = 0; i < settings.levels.Count; i++)
//             {
//                 var level = settings.levels[i];
//
//                 if (i >= startIndex && i <= endIndex)
//                 {
//                     if (applyGridDims)
//                     {
//                         if (level.gridData == null)
//                         {
//                             level.gridData = new GridData();
//                             changed = true;
//                         }
//                         if (level.gridData.GridDims != gridDims)
//                         {
//                             level.gridData.GridDims = gridDims;
//                             changed = true;
//                         }
//                     }
//
//                     if (applyDeadZoneColorA)
//                     {
//                         if (level.DeadZoneColorA != deadZoneColorA)
//                         {
//                             level.DeadZoneColorA = deadZoneColorA;
//                             changed = true;
//                         }
//                     }
//
//                     if (applyDeadZoneColorB)
//                     {
//                         if (level.DeadZoneColorB != deadZoneColorB)
//                         {
//                             level.DeadZoneColorB = deadZoneColorB;
//                             changed = true;
//                         }
//                     }
//                 }
//             }
//
//             if (changed)
//             {
//                 EditorUtility.SetDirty(settings);
//                 Debug.Log($"Updated LevelData in: {path}");
//             }
//
//             processed++;
//             if (processed % 10 == 0)
//             {
//                 EditorUtility.DisplayProgressBar("Applying LevelData Changes", $"Processing asset {processed} of {total}", (float)processed / total);
//             }
//         }
//
//         EditorUtility.ClearProgressBar();
//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
//         Debug.Log("✅ All SessionSettings updated.");
//     }
// }
