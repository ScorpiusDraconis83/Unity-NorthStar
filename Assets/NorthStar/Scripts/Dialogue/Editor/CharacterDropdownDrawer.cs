// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUI;

namespace NorthStar
{
    /// <summary>
    /// Gives a list of character names for a string field
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterDropdown))]
    public class CharacterDropdownDrawer : PropertyDrawer
    {
        private SerializedProperty m_property;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (DropdownButton(position, new GUIContent(property.stringValue != "" ? property.stringValue : "Select Key"), FocusType.Passive))
            {
                var dropdown = attribute as CharacterDropdown;
                m_property = property;
                var menu = new GenericMenu();

                foreach (var obj in CharacterRegister.Instance.CharacterNames)
                {
                    menu.AddItem(new GUIContent(obj), property.stringValue == obj, () => Select(obj));
                }

                menu.DropDown(position);
            }
        }

        private void Select(string data)
        {
            m_property.stringValue = data;
            _ = m_property.serializedObject.ApplyModifiedProperties();
        }
    }
}
