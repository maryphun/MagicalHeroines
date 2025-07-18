using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnemyActionPattern))]
public class EnemyActionPatternDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.indentLevel++;

        EditorGUILayout.BeginVertical();

        // Draw the actionType field
        EditorGUILayout.PropertyField(property.FindPropertyRelative("actionType"));

        // Draw the ability field only if actionType is SpecialAbility
        if ((EnemyActionType)property.FindPropertyRelative("actionType").enumValueIndex == EnemyActionType.SpecialAbility)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("ability"));
        }

        // Draw the chance field
        EditorGUILayout.Slider(property.FindPropertyRelative("chance"), 0.0f, 1.0f);

        // Draw the hasThresholdCondition field
        EditorGUILayout.PropertyField(property.FindPropertyRelative("hasThresholdCondition"));

        // Draw the HpThreshold field only if hasThresholdCondition is true
        if (property.FindPropertyRelative("hasThresholdCondition").boolValue)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("HpThreshold"));
        }

        // Draw the SpThreshold field only if hasThresholdCondition is true
        if (property.FindPropertyRelative("hasThresholdCondition").boolValue)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("SpThreshold"));
        }

        EditorGUILayout.Space();

        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();

        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 1;
    }
}