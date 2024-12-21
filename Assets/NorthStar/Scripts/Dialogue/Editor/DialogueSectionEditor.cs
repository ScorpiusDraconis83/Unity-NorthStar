// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;

namespace NorthStar
{
    /// <summary>
    /// Editor for Dialouge sections, shows its text data at a glance
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DialogueSection))]
    public class DialogueSectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var dialogueSection = target as DialogueSection;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.LabelField("Dialogue: ");
            foreach (var textObject in dialogueSection.TextObjects)
            {
                if (textObject.Text == null)
                { continue; }
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
