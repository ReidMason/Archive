using System;
using System.Reflection;

using UnityEngine;

namespace NoxCore.Utilities
{
    public static class SOEditorUtils
    {
        public static Type GetReturnType(MemberInfo info)
        {
            if (info == null) return null;

            switch (info.MemberType)
            {
                case MemberTypes.Field:
                    return (info as FieldInfo).FieldType;
                case MemberTypes.Property:
                    return (info as PropertyInfo).PropertyType;
                case MemberTypes.Method:
                    return (info as MethodInfo).ReturnType;
            }
            return null;
        }

        public static object GetValue(this object obj, MemberInfo member, params object[] args)
        {
            switch (member.MemberType)
            {
                case System.Reflection.MemberTypes.Field:
                    var field = member as System.Reflection.FieldInfo;
                    return field.GetValue(obj);

                case System.Reflection.MemberTypes.Property:
                    {
                        var prop = member as System.Reflection.PropertyInfo;
                        var paramInfos = prop.GetIndexParameters();
                        if (prop.CanRead && ParameterSignatureMatches(args, paramInfos, false))
                        {
                            return prop.GetValue(obj, args);
                        }
                        break;
                    }
                case System.Reflection.MemberTypes.Method:
                    {
                        var meth = member as System.Reflection.MethodInfo;
                        var paramInfos = meth.GetParameters();
                        if (ParameterSignatureMatches(args, paramInfos, false))
                        {
                            return meth.Invoke(obj, args);
                        }
                        break;
                    }
            }

            return null;
        }

        private static bool ParameterSignatureMatches(object[] args, ParameterInfo[] paramInfos, bool allowOptional)
        {
            //if (args == null) args = ArrayUtil.Empty<object>();
            //if (paramInfos == null) ArrayUtil.Empty<ParameterInfo>();

            if (args == null) args = new object[0];
            if (paramInfos == null) paramInfos = new ParameterInfo[0];

            if (args.Length == 0 && paramInfos.Length == 0) return true;
            if (args.Length > paramInfos.Length) return false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                if (args[i].GetType().IsAssignableFrom(paramInfos[i].ParameterType))
                {
                    continue;
                }

                return false;
            }

            return paramInfos.Length == args.Length || (allowOptional && paramInfos[args.Length].IsOptional);
        }
    }
}