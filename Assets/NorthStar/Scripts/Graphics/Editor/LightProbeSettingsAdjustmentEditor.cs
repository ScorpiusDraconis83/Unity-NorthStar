// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    [CustomEditor(typeof(LightProbeSettingsAdjustment))]
    public class LightProbeSettingsAdjustmentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            _ = DrawDefaultInspector();
            var settingsAdjustment = (LightProbeSettingsAdjustment)target;
            if (GUILayout.Button("Populate Child Renderers"))
            {
                settingsAdjustment.GetChildRenderers();
                EditorUtility.SetDirty(settingsAdjustment);
            }
        }
    }
}
