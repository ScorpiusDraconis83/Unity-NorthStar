// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUI;

namespace NorthStar
{
    /// <summary>
    /// Editor for the text manager
    /// </summary>
    [CustomEditor(typeof(TextManager))]
    public class TextManagerEditor : Editor
    {
        private SerializedProperty m_languageProperty;
        private SerializedProperty m_textAssetProperty;

        private SerializedProperty m_useSystemLanguage;

        private void OnEnable()
        {
            m_languageProperty = serializedObject.FindProperty("SelectedLanguage");
            m_textAssetProperty = serializedObject.FindProperty("m_xmlFile");
            m_useSystemLanguage = serializedObject.FindProperty("m_autoGetLanguage");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (DropdownButton(EditorGUILayout.GetControlRect(), new GUIContent(m_languageProperty.stringValue != "" ? m_languageProperty.stringValue : "Select Key"), FocusType.Passive))
            {
                var menu = new GenericMenu();
                foreach (var str in (serializedObject.targetObject as TextManager).GetLanguageOptionsLocal())
                {
                    menu.AddItem(new GUIContent(str), m_languageProperty.stringValue == str, () => Select(str));
                }
                menu.DropDown(EditorGUILayout.GetControlRect());
            }
            _ = EditorGUILayout.PropertyField(m_useSystemLanguage);
            _ = EditorGUILayout.PropertyField(m_textAssetProperty);
            _ = serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button(new GUIContent("Reimport")))
            {
                (serializedObject.targetObject as TextManager).CreateAssets();
            }
        }

        private void Select(string str)
        {
            m_languageProperty.stringValue = str;
            _ = serializedObject.ApplyModifiedProperties();
        }
    }
}
