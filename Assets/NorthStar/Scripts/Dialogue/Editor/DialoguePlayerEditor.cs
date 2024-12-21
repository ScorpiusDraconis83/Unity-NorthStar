// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;

namespace NorthStar
{
    /// <summary>
    /// Editor for dialouge players, shows its text data at a glance
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DialoguePlayer))]
    public class DialoguePlayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var dialogueSectionRef = serializedObject.FindProperty("m_dialogueSection");
            var dialogueSection = dialogueSectionRef.objectReferenceValue as DialogueSection;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Dialogue: ");
            foreach (var textObject in dialogueSection.TextObjects)
            {
                _ = EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel(textObject.Text.name);

                var style = EditorStyles.textArea;
                style.wordWrap = true;
                _ = EditorGUILayout.TextArea(textObject.Text.Text, style);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
