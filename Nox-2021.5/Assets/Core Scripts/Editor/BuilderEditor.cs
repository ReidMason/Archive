using UnityEditor;
using UnityEngine;

using NoxCore.Builders;

[CustomEditor(typeof(Builder), true)]
public class BuilderEditor : Editor
{
    public Builder builder;
    public bool showSettings;

    public SerializedProperty _buildType;
    public SerializedProperty _enable;
    public SerializedProperty _buildOnce;

    public virtual void OnEnable()
    {
        builder = target as Builder;
        showSettings = false;

        _buildType = serializedObject.FindProperty("buildType");
        _enable = serializedObject.FindProperty("enable");
        _buildOnce = serializedObject.FindProperty("buildOnce");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUIUtility.labelWidth = 100;
        EditorGUI.indentLevel = 0;

        EditorGUILayout.PropertyField(_enable, new GUIContent("Build Immediate"));
        EditorGUILayout.PropertyField(_buildOnce, new GUIContent("Build Once"));

        if (GUILayout.Button((showSettings) ? "Hide Override Settings" : "Show Override Settings", GUILayout.MaxWidth(200)))
        {
            showSettings = !showSettings;
        }

        serializedObject.ApplyModifiedProperties();
    }
/*
    private Vector2 FixIfNaN(Vector2 v)
    {
        if (float.IsNaN(v.x))
        {
            v.x = 0;
        }
        if (float.IsNaN(v.y))
        {
            v.y = 0;
        }
        return v;
    }
*/
}
