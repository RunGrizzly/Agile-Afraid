using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New GridData", menuName = "Data/New GridData", order = 1)]
public class GridData: SerializedScriptableObject
{
 #if UNITY_EDITOR
     [TableMatrix(DrawElementMethod ="DrawLetterGrid",SquareCells = true, ResizableColumns = false)]
 #endif
    public GridSeed[,] gridSeed = new GridSeed[4,3];
    
 #if UNITY_EDITOR
     static GridSeed DrawLetterGrid(Rect rect, GridSeed value)
     {
         if (value == null)
         {
             value = new GridSeed("", GridElementFlags.None);
         }
         
         Color elementColor = Color.gray;

         if ((value.Flags & GridElementFlags.Start) != 0)
         {
             elementColor = Color.green;
         }
         else if ((value.Flags & GridElementFlags.End) != 0)
         {
             elementColor = Color.yellow;
         }
         else if ((value.Flags & GridElementFlags.Blocked) != 0)
         {
             elementColor = Color.red;
         }
         else if (value.Flags == GridElementFlags.None)
         {
             elementColor = Color.gray;
         }

         
         // // Full rect for the element (passed in)
         // EditorGUI.DrawRect(rect, elementColor);
         
         // Split the rect
         var inputRect = new Rect(rect);
         inputRect.height = rect.height * 0.25f;

         var buttonRect = new Rect(rect);
         buttonRect.height = rect.height *0.25f;
         buttonRect.y = rect.y + rect.height*0.75f;
         
         // elementRect.x /= 4;
         // elementRect.y /= 4;
         //
         // elementRect.width /= 4;
         // elementRect.height /= 4;
         
         // Draw text field in top half
         var customStyle = new GUIStyle(EditorStyles.textField)
         {
             normal = { background = null },
             active = { background = null },
             hover = { background = null },
             focused = { background = null }
         };
         
         EditorGUI.DrawRect(rect,elementColor);
         
         value.Content = EditorGUI.TextField(inputRect, value.Content.ToString(), customStyle);
         
         // Draw button in bottom half
         // if (GUI.Button(buttonRect, "⋮"))
         // {
         //     Debug.Log("Right-click menu triggered for: " + value.Content);
         //
         //     var menu = new UnityEditor.GenericMenu();
         //     menu.AddItem(new GUIContent("Set As Start Seed"), false, () => value.Flags = GridElementFlags.start);
         //     menu.AddItem(new GUIContent("Set As End Seed"), false, () => value.Flags = GridElementFlags.end);
         //     menu.AddItem(new GUIContent("Set As Neutral Seed"), false, () => value.Flags = GridElementFlags.none);
         //     menu.AddItem(new GUIContent("Set As Blocked Seed"), false, () => value.Flags = GridElementFlags.blocked);
         //     menu.ShowAsContext();
         // }
         
         // Draw enum flags as dropdown
         value.Flags = (GridElementFlags)EditorGUI.EnumFlagsField(buttonRect, value.Flags);
         
         return value;
     }
#endif

    [Button]
    public void InterpretFromCSV()
    {
        
    }
    
}