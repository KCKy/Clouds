using System;
using System.Reflection;
using UnityEditor;

#if UNITY_EDITOR

namespace Useful.Extensions
{
    public static class SerializedPropertyExtension
    {
        public static T Value<T>(this SerializedProperty serializedProperty)
        {
            if (serializedProperty == null)
                throw new ArgumentNullException(nameof(serializedProperty));

            UnityEngine.Object targetObject = serializedProperty.serializedObject.targetObject;
            if(targetObject == null)
                return default;

            Type targetObjectClassType = targetObject.GetType();
            FieldInfo field = targetObjectClassType.GetField(serializedProperty.propertyPath);

            if (field == null)
                return default;
            object value = field.GetValue(targetObject);

            if (value is T result)
                return result;
            return default;
        }
    }
}

#endif
