using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxRange))]
public class MinMaxRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var minRect = new Rect(position.x + EditorGUIUtility.labelWidth+1, position.y, 50, position.height);
        var maxRect = new Rect(position.x + EditorGUIUtility.labelWidth + 88, position.y, 50, position.height);

        // Draw fields with labels
        EditorGUI.LabelField(minRect, new GUIContent("Min"));
        minRect.x += 30;
        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("min"), GUIContent.none);

        EditorGUI.LabelField(maxRect, new GUIContent("Max"));
        maxRect.x += 35;
        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("max"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}