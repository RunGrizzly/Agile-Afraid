using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WordRequest))]
public class WordRequestDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var wordRect = new Rect(position.x, position.y, 80, position.height);
        var placeRect = new Rect(position.x + wordRect.width + 10, position.y, 185, position.height);
        var buttonRect = new Rect(position.x + wordRect.width + 10 + placeRect.width + 10, position.y, 50, position.height);

        // Draw fields - pass GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(wordRect, property.FindPropertyRelative("word"), GUIContent.none);
        EditorGUI.PropertyField(placeRect, property.FindPropertyRelative("placementType"), GUIContent.none);

        //Buttons for player actions
        if (GUI.Button(buttonRect, "Place"))
        {
            //  Debug.Log("Enum value = " + property.FindPropertyRelative("placementType").enumValueIndex);



            GridTools.WordIntoLine(BrainControl.Get().grid, new WordRequest(property.FindPropertyRelative("word").stringValue, (PlacementType)property.FindPropertyRelative("placementType").enumValueIndex), true);
        }

        EditorGUI.EndProperty();
    }

}