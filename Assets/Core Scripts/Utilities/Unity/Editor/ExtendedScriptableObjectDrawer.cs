// Modifications made by Luiz Wendt, Rob Tranquillo
// Released under the MIT Licence as held at https://opensource.org/licenses/MIT

// Must be placed within a folder named "Editor"
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using com.spacepuppy;

using NoxCore.Utilities;

/// <summary>
/// Extends how ScriptableObject object references are displayed in the inspector
/// Shows you all values under the object reference
/// Also provides a button to create a new ScriptableObject if property is null.
/// todo: enable custom editors for scriptable objects
/// </summary>
[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ExtendedScriptableObjectDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight;
        if (property.objectReferenceValue == null)
        {
            return totalHeight;
        }
        if (!IsThereAnyVisibileProperty(property))
            return totalHeight;
        if (property.isExpanded)
        {
            var data = property.objectReferenceValue as ScriptableObject;
            if (data == null) return EditorGUIUtility.singleLineHeight;
            SerializedObject serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name == "m_Script") continue;
                    var subProp = serializedObject.FindProperty(prop.name);
                    float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
                    totalHeight += height;
                }
                while (prop.NextVisible(false));
            }
            // Add a tiny bit of height if open for the background
            totalHeight += EditorGUIUtility.standardVerticalSpacing;
        }
        return totalHeight;
    }

    protected float GetFieldHeight(List<FieldInfo> fieldsToDraw, ScriptableObject data)
    {
        float totalHeight = 0;

        foreach (var f in fieldsToDraw)
        {
            foreach (var runtimeprop in data.GetType().GetProperties())
            {
                totalHeight += 14 + EditorGUIUtility.standardVerticalSpacing;
            }

            totalHeight += EditorGUIUtility.standardVerticalSpacing;
        }

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var fieldsToDraw = GetDrawnFields(property);

        EditorGUI.BeginProperty(position, label, property);
        if (property.objectReferenceValue != null)
        {
            if (IsThereAnyVisibileProperty(property))
            {

                property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property.isExpanded, property.displayName, true);
            }
            else
            {
                EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property.displayName);
                property.isExpanded = false;
            }

            EditorGUI.PropertyField(new Rect(EditorGUIUtility.labelWidth + 14, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), property, GUIContent.none, true);
            if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
            if (property.objectReferenceValue == null) EditorGUIUtility.ExitGUI();

            if (property.isExpanded)
            {
                var data = (ScriptableObject)property.objectReferenceValue;

                float fieldHeight = GetFieldHeight(fieldsToDraw, data);

                // Draw a background that shows us clearly which fields are part of the ScriptableObject
                GUI.Box(new Rect(0, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, Screen.width, position.height + fieldHeight - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

                EditorGUI.indentLevel++;
                
                SerializedObject serializedObject = new SerializedObject(data);

                // Iterate over all the values and draw them
                SerializedProperty prop = serializedObject.GetIterator();
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (prop.NextVisible(true))
                {
                    do
                    {
                        // Don't bother drawing the class file
                        if (prop.name == "m_Script") continue;
                        float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);

                        y += height + EditorGUIUtility.standardVerticalSpacing;
                        
                        // not right but close?
                        //var runtimeProp = data.GetType().GetProperty(prop.displayName);
                        /*
                        TypeInfo t = data.GetType().GetTypeInfo();
                        IEnumerable<PropertyInfo> pList = t.DeclaredProperties;

                        foreach(PropertyInfo pInfo in pList)
                        {
                            MethodInfo mInfo = pInfo.GetGetMethod();

                            object value = mInfo.Invoke(serializedObject.targetObject, null);

                            EditorGUI.LabelField(new Rect(position.x, y, position.width, height), "Runtime: " + prop.displayName, value.ToString());

                            y += height + EditorGUIUtility.standardVerticalSpacing;
                        }
                        */
                        //var runtimePropValue = runtimeProp.GetValue(data, null);
                        
                        //if (runtimePropValue == null) runtimePropValue = "";
                        
                        // grab property
                    
                    }
                    while (prop.NextVisible(false));
                }

                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                /*
                foreach (var f in fieldsToDraw)
                {
                    //var fieldValue = f.GetValue(data);

                    foreach (var runtimeprop in data.GetType().GetProperties())
                    {
                        var runtimePropValue = runtimeprop.GetValue(data, null);

                        if (runtimePropValue == null) runtimePropValue = "";

                        float height = 14;

                        EditorGUI.LabelField(new Rect(position.x, y, position.width, height), runtimeprop.Name, runtimePropValue.ToString());
                        y += height + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
                */
                /*
                foreach (var f in fieldsToDraw)
                {
                    var fieldType = f.FieldType;

                    var fieldName = ObjectNames.NicifyVariableName(f.Name);
                    var fieldLabel = new GUIContent(fieldName);

                    var fieldValue = f.GetValue(obj);

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
                */

                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();

                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUI.ObjectField(new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight), property);
            if (GUI.Button(new Rect(position.x + position.width - 58, position.y, 58, EditorGUIUtility.singleLineHeight), "Create"))
            {
                string selectedAssetPath = "Assets";
                if (property.serializedObject.targetObject is MonoBehaviour)
                {
                    MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour)property.serializedObject.targetObject);
                    selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
                }
                Type type = fieldInfo.FieldType;
                if (type.IsArray) type = type.GetElementType();
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];
                property.objectReferenceValue = CreateAssetWithSavePrompt(type, selectedAssetPath);
            }
        }

        if (GUI.changed) property.serializedObject.ApplyModifiedProperties();
        EditorGUI.EndProperty();
    }

    // Creates a new ScriptableObject via the default Save File panel
    ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
    {
        path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "New " + type.Name + ".asset", "asset", "Enter a file name for the ScriptableObject.", path);
        if (path == "") return null;
        ScriptableObject asset = ScriptableObject.CreateInstance(type);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        EditorGUIUtility.PingObject(asset);
        return asset;
    }

    public bool IsThereAnyVisibileProperty(SerializedProperty property)
    {
        var data = (ScriptableObject)property.objectReferenceValue;
        SerializedObject serializedObject = new SerializedObject(data);

        SerializedProperty prop = serializedObject.GetIterator();

        while (prop.NextVisible(true))
        {
            if (prop.name == "m_Script") continue;
            return true; //if theres any visible property other than m_script
        }
        return false;
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