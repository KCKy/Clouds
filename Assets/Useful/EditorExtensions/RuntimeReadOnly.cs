using UnityEditor;
using UnityEngine;

namespace Useful.EditorExtensions
{
    public class RuntimeReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RuntimeReadOnlyAttribute))]
    public class RuntimeReadOnly : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, true);
    }
#endif
}