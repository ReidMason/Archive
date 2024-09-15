using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using com.spacepuppy;

namespace NoxCore.Utilities
{ 
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects()]
    public class SOEditor : Editor
    {
        private List<ShownPropertyInfo> _shownFields;
        private ConstantlyRepaintEditorAttribute _constantlyRepaint;

        protected virtual void OnEnable()
        {
            var tp = this.target.GetType();
            var fields = tp.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
            foreach (var f in fields)
            {
                var attribs = f.GetCustomAttributes(typeof(ShowNonSerializedPropertyAttribute), false) as ShowNonSerializedPropertyAttribute[];
                if (attribs != null && attribs.Length > 0)
                {
                    if (_shownFields == null) _shownFields = new List<ShownPropertyInfo>();
                    var attrib = attribs[0];
                    _shownFields.Add(new ShownPropertyInfo()
                    {
                        Attrib = attrib,
                        MemberInfo = f,
                        Label = new GUIContent(attrib.Label ?? f.Name, attrib.Tooltip)
                    });
                }
            }

            _constantlyRepaint = tp.GetCustomAttributes(typeof(ConstantlyRepaintEditorAttribute), false).FirstOrDefault() as ConstantlyRepaintEditorAttribute;
        }

        public override void OnInspectorGUI()
        {
            if (_shownFields != null && UnityEngine.Application.isPlaying)
            {
                GUILayout.Label("Runtime Values", EditorStyles.boldLabel);

                foreach (var info in _shownFields)
                {
                    var value = SOEditorUtils.GetValue(this.target, info.MemberInfo);

                    EditorGUI.LabelField(EditorGUILayout.GetControlRect(true), info.Label, value.ToString());
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        
        /*


        public SerializedPropertyType GetPropertyType(System.Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");

            if (tp.IsEnum) return SerializedPropertyType.Enum;

            var code = System.Type.GetTypeCode(tp);
            switch (code)
            {
                case System.TypeCode.SByte:
                case System.TypeCode.Byte:
                case System.TypeCode.Int16:
                case System.TypeCode.UInt16:
                case System.TypeCode.Int32:
                    return SerializedPropertyType.Integer;
                case System.TypeCode.Boolean:
                    return SerializedPropertyType.Boolean;
                case System.TypeCode.Single:
                    return SerializedPropertyType.Float;
                case System.TypeCode.String:
                    return SerializedPropertyType.String;
                case System.TypeCode.Char:
                    return SerializedPropertyType.Character;
                default:
                    {
                        if (TypeUtil.IsType(tp, typeof(Color)))
                            return SerializedPropertyType.Color;
                        else if (TypeUtil.IsType(tp, typeof(UnityEngine.Object)))
                            return SerializedPropertyType.ObjectReference;
                        else if (TypeUtil.IsType(tp, typeof(LayerMask)))
                            return SerializedPropertyType.LayerMask;
                        else if (TypeUtil.IsType(tp, typeof(Vector2)))
                            return SerializedPropertyType.Vector2;
                        else if (TypeUtil.IsType(tp, typeof(Vector3)))
                            return SerializedPropertyType.Vector3;
                        else if (TypeUtil.IsType(tp, typeof(Vector4)))
                            return SerializedPropertyType.Vector4;
                        else if (TypeUtil.IsType(tp, typeof(Quaternion)))
                            return SerializedPropertyType.Quaternion;
                        else if (TypeUtil.IsType(tp, typeof(Rect)))
                            return SerializedPropertyType.Rect;
                        else if (TypeUtil.IsType(tp, typeof(AnimationCurve)))
                            return SerializedPropertyType.AnimationCurve;
                        else if (TypeUtil.IsType(tp, typeof(Bounds)))
                            return SerializedPropertyType.Bounds;
                        else if (TypeUtil.IsType(tp, typeof(Gradient)))
                            return SerializedPropertyType.Gradient;
                    }
                    return SerializedPropertyType.Generic;

            }
        }



        public object DefaultPropertyField(Rect position, GUIContent label, object value, System.Type valueType)
        {
            SerializedPropertyType propertyType = SerializedPropertyType.Generic;
            if (valueType != null) propertyType = (valueType.IsInterface) ? SerializedPropertyType.ObjectReference : EditorHelper.GetPropertyType(valueType);

            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    EditorGUI.BeginChangeCheck();
                    int num1 = EditorGUI.IntField(position, label, ConvertUtil.ToInt(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return num1;
                    }
                    else
                        break;
                case SerializedPropertyType.Boolean:
                    EditorGUI.BeginChangeCheck();
                    bool flag2 = EditorGUI.Toggle(position, label, ConvertUtil.ToBool(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return flag2;
                    }
                    else
                        break;
                case SerializedPropertyType.Float:
                    EditorGUI.BeginChangeCheck();
                    float num2 = EditorGUI.FloatField(position, label, ConvertUtil.ToSingle(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return num2;
                    }
                    else
                        break;
                case SerializedPropertyType.String:
                    EditorGUI.BeginChangeCheck();
                    string str1 = EditorGUI.TextField(position, label, ConvertUtil.ToString(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return str1;
                    }
                    else
                        break;
                case SerializedPropertyType.Color:
                    EditorGUI.BeginChangeCheck();
                    Color color = EditorGUI.ColorField(position, label, ConvertUtil.ToColor(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return color;
                    }
                    else
                        break;
                case SerializedPropertyType.ObjectReference:
                    EditorGUI.BeginChangeCheck();
                    object obj = EditorGUI.ObjectField(position, label, value as UnityEngine.Object, valueType, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return obj;
                    }
                    break;
                case SerializedPropertyType.LayerMask:
                    EditorGUI.BeginChangeCheck();
                    LayerMask mask = (value is LayerMask) ? (LayerMask)value : (LayerMask)ConvertUtil.ToInt(value);
                    mask = SPEditorGUI.LayerMaskField(position, label, mask);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return mask;
                    }
                    break;
                case SerializedPropertyType.Enum:
                    if (valueType.GetCustomAttributes(typeof(System.FlagsAttribute), false).Any())
                    {
                        EditorGUI.BeginChangeCheck();
                        var e = SPEditorGUI.EnumFlagField(position, label, ConvertUtil.ToEnumOfType(valueType, value));
                        if (EditorGUI.EndChangeCheck())
                        {
                            return e;
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        var e = SPEditorGUI.EnumPopupExcluding(position, label, ConvertUtil.ToEnumOfType(valueType, value));
                        if (EditorGUI.EndChangeCheck())
                        {
                            return e;
                        }
                    }
                    break;
                case SerializedPropertyType.Vector2:
                    EditorGUI.BeginChangeCheck();
                    var v2 = EditorGUI.Vector2Field(position, label, ConvertUtil.ToVector2(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return v2;
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    EditorGUI.BeginChangeCheck();
                    var v3 = EditorGUI.Vector3Field(position, label, ConvertUtil.ToVector3(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return v3;
                    }
                    break;
                case SerializedPropertyType.Vector4:
                    EditorGUI.BeginChangeCheck();
                    var v4 = EditorGUI.Vector4Field(position, label.text, ConvertUtil.ToVector4(value));
                    if (EditorGUI.EndChangeCheck())
                    {
                        return v4;
                    }
                    break;
                case SerializedPropertyType.Rect:
                    EditorGUI.BeginChangeCheck();
                    Rect rect = (value is Rect) ? (Rect)value : new Rect();
                    rect = EditorGUI.RectField(position, label, rect);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return rect;
                    }
                    break;
                case SerializedPropertyType.ArraySize:
                    EditorGUI.BeginChangeCheck();
                    int num3 = EditorGUI.IntField(position, label, ConvertUtil.ToInt(value), EditorStyles.numberField);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return num3;
                    }
                    break;
                case SerializedPropertyType.Character:
                    bool changed = GUI.changed;
                    GUI.changed = false;
                    string str2 = EditorGUI.TextField(position, label, new string(ConvertUtil.ToChar(value), 1));
                    if (GUI.changed)
                    {
                        if (str2.Length == 1)
                            return str2[0];
                        else
                            GUI.changed = false;
                    }
                    GUI.changed = GUI.changed | changed;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    EditorGUI.BeginChangeCheck();
                    AnimationCurve curve = value as AnimationCurve;
                    curve = EditorGUI.CurveField(position, label, curve);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return curve;
                    }
                    break;
                case SerializedPropertyType.Bounds:
                    EditorGUI.BeginChangeCheck();
                    Bounds bnds = (value is Bounds) ? (Bounds)value : new Bounds();
                    bnds = EditorGUI.BoundsField(position, label, bnds);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return bnds;
                    }
                    break;
                case SerializedPropertyType.Gradient:
                    EditorGUI.BeginChangeCheck();
                    Gradient grad = value as Gradient;
                    grad = SPEditorGUI.GradientField(position, label, grad);
                    if (EditorGUI.EndChangeCheck())
                    {
                        return grad;
                    }
                    break;
                default:
                    EditorGUI.PrefixLabel(position, label);
                    break;
            }

            return value;
        }
        */
        /*

            private List<GUIDrawer> _headerDrawers;
            private SPEditorAddonDrawer[] _addons;

            private List<ShownPropertyInfo> _shownFields;
            private ConstantlyRepaintEditorAttribute _constantlyRepaint;

            protected virtual void OnEnable()
            {
                var tp = this.target.GetType();
                var fields = tp.GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static);
                foreach (var f in fields)
                {
                    var attribs = f.GetCustomAttributes(typeof(ShowNonSerializedPropertyAttribute), false) as ShowNonSerializedPropertyAttribute[];
                    if (attribs != null && attribs.Length > 0)
                    {
                        if (_shownFields == null) _shownFields = new List<ShownPropertyInfo>();
                        var attrib = attribs[0];
                        _shownFields.Add(new ShownPropertyInfo()
                        {
                            Attrib = attrib,
                            MemberInfo = f,
                            Label = new GUIContent(attrib.Label ?? f.Name, attrib.Tooltip)
                        });
                    }
                }

                _constantlyRepaint = tp.GetCustomAttributes(typeof(ConstantlyRepaintEditorAttribute), false).FirstOrDefault() as ConstantlyRepaintEditorAttribute;
            }

            protected virtual void OnDisable()
            {

            }

            void OnInspectorGUIComplete(SerializedObject obj, bool validate)
            {
                if (obj == null || obj.targetObject == null) return;

                int id = obj.targetObject.GetInstanceID();
                if (_usedHandlers.ContainsKey(id))
                {
                    var lst = _usedHandlers.Lists[id];
                    if (validate)
                    {
                        HandlerInfo info;
                        for (int i = 0; i < lst.Count; i++)
                        {
                            info = lst[i];
                            info.handler.OnValidate(obj.FindProperty(info.propPath));
                        }
                    }
                    lst.Clear();
                }
            }

            public sealed override void OnInspectorGUI()
            {
                if (!(this.target is SPComponent) && !SpacepuppySettings.UseSPEditorAsDefaultEditor)
                {
                    base.OnInspectorGUI();
                    return;
                }

                //this.OnBeforeSPInspectorGUI();

                EditorGUI.BeginChangeCheck();

                //draw header infobox if needed
                this.DrawDefaultInspectorHeader();
                this.OnSPInspectorGUI();
                this.DrawDefaultInspectorFooters();

                if (EditorGUI.EndChangeCheck())
                {
                    //do call onValidate
                    PropertyHandlerValidationUtility.OnInspectorGUIComplete(this.serializedObject, true);
                    this.OnValidate();
                }
                else
                {
                    PropertyHandlerValidationUtility.OnInspectorGUIComplete(this.serializedObject, false);
                }

                if (_shownFields != null && UnityEngine.Application.isPlaying)
                {
                    GUILayout.Label("Runtime Values", EditorStyles.boldLabel);

                    foreach (var info in _shownFields)
                    {
                        var cache = SPGUI.DisableIf(info.Attrib.Readonly);

                        var value = DynamicUtil.GetValue(this.target, info.MemberInfo);
                        EditorGUI.BeginChangeCheck();
                        value = SPEditorGUILayout.DefaultPropertyField(info.Label, value, DynamicUtil.GetReturnType(info.MemberInfo));
                        if (EditorGUI.EndChangeCheck())
                        {
                            DynamicUtil.SetValue(this.target, info.MemberInfo, value);
                        }

                        cache.Reset();
                    }
                }
            }

            protected virtual void OnBeforeSPInspectorGUI()
            {

            }

            protected virtual void OnSPInspectorGUI()
            {
                this.DrawDefaultInspector();
            }

            protected virtual void OnValidate()
            {

            }

            private void DrawDefaultInspectorHeader()
            {
                if (_headerDrawers == null)
                {
                    _headerDrawers = new List<GUIDrawer>();
                    if (serializedObject.targetObject != null)
                    {
                        var componentType = serializedObject.targetObject.GetType();
                        if (TypeUtil.IsType(componentType, typeof(Component), typeof(ScriptableObject)))
                        {
                            var attribs = (from o in componentType.GetCustomAttributes(typeof(ComponentHeaderAttribute), true)
                                           let a = o as ComponentHeaderAttribute
                                           where a != null
                                           orderby a.order
                                           select a).ToArray();
                            foreach (var attrib in attribs)
                            {
                                var dtp = ScriptAttributeUtility.GetDrawerTypeForType(attrib.GetType());
                                if (dtp != null)
                                {
                                    if (TypeUtil.IsType(dtp, typeof(DecoratorDrawer)))
                                    {
                                        var decorator = System.Activator.CreateInstance(dtp) as DecoratorDrawer;
                                        DynamicUtil.SetValue(decorator, "m_Attribute", attrib);
                                        _headerDrawers.Add(decorator);
                                    }
                                    else if (TypeUtil.IsType(dtp, typeof(ComponentHeaderDrawer)))
                                    {
                                        var drawer = System.Activator.CreateInstance(dtp) as ComponentHeaderDrawer;
                                        drawer.Init(attrib, componentType);
                                        _headerDrawers.Add(drawer);
                                    }
                                }
                            }



                            _addons = SPEditorAddonDrawer.GetDrawers(this.serializedObject);
                        }
                    }
                }

                for (int i = 0; i < _headerDrawers.Count; i++)
                {
                    var drawer = _headerDrawers[i];
                    if (drawer is DecoratorDrawer)
                    {
                        var decorator = drawer as DecoratorDrawer;
                        var h = decorator.GetHeight();
                        Rect position = EditorGUILayout.GetControlRect(false, h);
                        decorator.OnGUI(position);
                    }
                    else if (drawer is ComponentHeaderDrawer)
                    {
                        var compDrawer = drawer as ComponentHeaderDrawer;
                        var h = compDrawer.GetHeight(this.serializedObject);
                        Rect position = EditorGUILayout.GetControlRect(false, h);
                        compDrawer.OnGUI(position, this.serializedObject);
                    }
                }

                if (_addons != null)
                {
                    foreach (var d in _addons)
                    {
                        if (!d.IsFooter) d.OnInspectorGUI();
                    }
                }
            }

            private void DrawDefaultInspectorFooters()
            {
                if (_addons != null)
                {
                    foreach (var d in _addons)
                    {
                        if (d.IsFooter) d.OnInspectorGUI();
                    }
                }
            }
        */
        private class ShownPropertyInfo
        {
            public ShowNonSerializedPropertyAttribute Attrib;
            public MemberInfo MemberInfo;
            public GUIContent Label;
        }
    }
}