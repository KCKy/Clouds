using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Useful.EditorExtensions
{
    public class ShowInlineAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ShowInlineAttribute))]
    public class ShowInline : PropertyDrawer
    {
        static readonly float FieldLabelWidth = 14;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object boxedValue = property.boxedValue;
            var fields = boxedValue?.GetType().GetFields();
            int fieldCount = fields?.Length ?? 1;
            float fieldWidth = position.width / (fieldCount + 1);

            float currentX = position.x;
            EditorGUI.LabelField(new(currentX, position.y, fieldWidth, position.height), new GUIContent(label.text));
            currentX += fieldWidth;

            if (fields == null)
            {
                Rect nullRect = new Rect(currentX, position.y, fieldWidth, position.height);
                EditorGUI.LabelField(nullRect, new GUIContent("<null>"));
                return;
            }

            foreach (FieldInfo field in fields)
            {
                SerializedProperty fieldProperty = property.FindPropertyRelative(field.Name);

                if (fieldProperty != null)
                {
                    Rect labelRect = new Rect(currentX, position.y, FieldLabelWidth, position.height);
                    EditorGUI.LabelField(labelRect, new GUIContent(fieldProperty.displayName[..1], fieldProperty.displayName), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });

                    Rect fieldRect = new Rect(currentX + FieldLabelWidth, position.y, fieldWidth - FieldLabelWidth, position.height);
                    EditorGUI.PropertyField(fieldRect, fieldProperty, GUIContent.none);
                }

                currentX += fieldWidth;
            }
        }
    }
#endif
}