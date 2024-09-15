using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using com.spacepuppy;

[CustomPropertyDrawer(typeof(ShowNonSerializedPropertyAttribute))]
public class ShowNonSerializedDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var fieldsToDraw = GetDrawnFields(property);

        var obj = property.serializedObject.targetObject;

        position.height = EditorGUIUtility.singleLineHeight;

        foreach (var f in fieldsToDraw)
        {
            var fieldType = f.FieldType;

            var fieldName = ObjectNames.NicifyVariableName(f.Name);
            var fieldLabel = new GUIContent(fieldName);

            var fieldValue = f.GetValue(obj);

            if (fieldType == typeof(float))
            {
                var val = (float)fieldValue;
                EditorGUI.BeginChangeCheck();
                val = EditorGUI.FloatField(position, fieldLabel, val);
                if (EditorGUI.EndChangeCheck())
                {
                    f.SetValue(obj, val);
                }
            }
            else if (fieldType == typeof(string))
            {
                var val = (string)fieldValue;
                EditorGUI.BeginChangeCheck();
                val = EditorGUI.TextField(position, fieldLabel, val);
                if (EditorGUI.EndChangeCheck())
                {
                    f.SetValue(obj, val);
                }
            }
            else if (fieldType == typeof(int))
            {
                var val = (int)fieldValue;
                EditorGUI.BeginChangeCheck();
                val = EditorGUI.IntField(position, fieldLabel, val);
                if (EditorGUI.EndChangeCheck())
                {
                    f.SetValue(obj, val);
                }
            }
            else if (fieldType == typeof(bool))
            {
                var val = (bool)fieldValue;
                EditorGUI.BeginChangeCheck();
                val = EditorGUI.Toggle(position, fieldLabel, val);
                if (EditorGUI.EndChangeCheck())
                {
                    f.SetValue(obj, val);
                }
            }

            position.y += EditorGUIUtility.singleLineHeight + 2;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var fieldsToDraw = GetDrawnFields(property);

        return EditorGUIUtility.singleLineHeight * fieldsToDraw.Count + 2 * (fieldsToDraw.Count - 1);
    }

    private static List<FieldInfo> GetDrawnFields(SerializedProperty property)
    {
        var baseType = property.serializedObject.targetObject.GetType();

        var fields = baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        var fieldsToDraw = new List<FieldInfo>();

        foreach (var f in fields)
        {
            var attr = f.GetCustomAttributes(typeof(ShowNonSerializedPropertyAttribute), true);
            if (attr.Length > 0)
            {
                fieldsToDraw.Add(f);
            }
        }

        return fieldsToDraw;
    }
}