// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Editor for text objects, shows the text in the localisation file
    /// </summary>
    [CustomEditor(typeof(TextObject))]
    public class TextObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var textObject = target as TextObject;
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(true);
            _ = EditorGUILayout.TextArea(textObject.Text);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Reload"))
                textObject.ForceReload();
        }
    }
}
