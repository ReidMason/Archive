using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NoxCore.Utilities
{
    public static class UnityEditorExtensionMethods
    {
        public static T GetActualObject<T>(this SerializedProperty property) where T : class
        {
            var serializedObject = property.serializedObject;
            if (serializedObject == null)
            {
                return null;
            }
            var targetObject = serializedObject.targetObject;
            var field = targetObject.GetType().GetField(property.name);
            var obj = field.GetValue(targetObject);
            if (obj == null)
            {
                return null;
            }
            T actualObject = null;
            if (obj.GetType().IsArray)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                actualObject = ((T[])obj)[index];
            }
            else
            {
                actualObject = obj as T;
            }
            return actualObject;
        }
    }
}