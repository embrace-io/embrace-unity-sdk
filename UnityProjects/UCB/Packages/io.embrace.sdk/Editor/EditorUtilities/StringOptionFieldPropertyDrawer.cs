using System;
using UnityEditor;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Add this attribute to a string field to display a popup with the given values in the inspector.
    /// </summary>
    public class StringOptionField : PropertyAttribute
    {
        public string[] values;

        public StringOptionField(params string[] values)
        {
            this.values = values;
        }
    }

    [CustomPropertyDrawer(typeof(StringOptionField))]
    public class StringOptionFieldPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect popupPosition = EditorGUI.PrefixLabel(position, label);

            string[] values = (attribute as StringOptionField)?.values ?? Array.Empty<string>();

            string currentValue = property.stringValue;

            int currentIndex = -1;
            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                for (int i = 0; i < values.Length; ++i)
                {
                    if (currentValue == values[i])
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }

            int newIndex = EditorGUI.Popup(popupPosition, currentIndex, values);

            if (newIndex != currentIndex)
            {
                property.stringValue = values[newIndex];
            }
        }
    }
}