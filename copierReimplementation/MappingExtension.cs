using System;
using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public static class MappingExtension
    {
        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void RecursiveCopy(Object destinationObject, Object originalObject)
        {
            if (originalObject == null) destinationObject = null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(destinationObject.GetType()))
            {
                destinationObject = originalObject;
                return;
            }
            CopyFields(originalObject, destinationObject, typeToReflect);
            CopyBaseTypeFields(originalObject, destinationObject, typeToReflect);
        }

        private static void CopyBaseTypeFields(object originalObject, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                CopyBaseTypeFields(originalObject, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                if (originalFieldValue == null)
                {
                    continue;
                }
                object clonedFieldValue;
                if (IsPrimitive(originalFieldValue.GetType()))
                {
                    clonedFieldValue = originalFieldValue;
                }
                else
                {
                    clonedFieldValue = CloneMethod.Invoke(originalFieldValue, null);
                }
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
                RecursiveCopy(clonedFieldValue, originalFieldValue);
            }
        }

        static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }
        public static void CopyFrom<T>(this T source, T template)
        {
            RecursiveCopy(source, template);
        }
    }
}