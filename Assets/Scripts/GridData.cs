using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
         
         // Split the rect
         var inputRect = new Rect(rect);
         inputRect.height = rect.height * 0.25f;

         var buttonRect = new Rect(rect);
         buttonRect.height = rect.height * 0.25f;
         buttonRect.y = rect.y + rect.height * 0.5f;

         var boolRect = new Rect(rect);
         boolRect.height = rect.height * 0.25f;
         boolRect.y = rect.y + rect.height * 0.75f;

         
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
         value.Flags = (GridElementFlags)EditorGUI.EnumFlagsField(buttonRect, value.Flags);
         value.OpenForEdit = EditorGUI.Toggle(boolRect, value.OpenForEdit);
         
         return value;
     }
#endif

    [Button]
    public void InterpretFromCSV()
    {
        
    }

    [Button]
    public void ClearAll()
    {
        for (int x = 0; x < gridSeed.GetLength(0); x++)
        {
            for (int y = 0; y < gridSeed.GetLength(1); y++)
            {
                gridSeed[x, y] = null;
            }
        }
    }
    
    [Button]
    public void SetAllFlags(GridElementFlags flags)
    {
        for (int x = 0; x < gridSeed.GetLength(0); x++)
        {
            for (int y = 0; y < gridSeed.GetLength(1); y++)
            {
                if (gridSeed[x, y] == null)
                {
                    gridSeed[x, y] = new GridSeed("", GridElementFlags.Empty);
                }
                else
                {
                    gridSeed[x, y].Flags |= GridElementFlags.Empty;
                }
            }
        }
    }

    [Button]
    public void SetFlags(GridElementFlags flags)
    {
        for (int x = 0; x < gridSeed.GetLength(0); x++)
        {
            for (int y = 0; y < gridSeed.GetLength(1); y++)
            {
                if (gridSeed[x, y] != null && gridSeed[x, y].OpenForEdit)
                {
                    gridSeed[x, y].Flags = flags;
                }
            }
        }
    }

    [Button]
    public void Evaluate()
    {
        for (int x = 0; x < gridSeed.GetLength(0); x++)
        {
            for (int y = 0; y < gridSeed.GetLength(1); y++)
            {
                if (gridSeed[x, y] != null && !gridSeed[x, y].Content.IsNullOrWhitespace() && gridSeed[x, y].Content != "")
                {
                    //A filled string cannot be empty
                    gridSeed[x, y].Flags &= ~GridElementFlags.Empty;
                }
            }
        }
    }
    
}