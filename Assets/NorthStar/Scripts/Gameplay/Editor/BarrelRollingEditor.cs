// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Editor for the barrel rolling interaction
    /// </summary>
    [CustomEditor(typeof(BarrelRolling)), CanEditMultipleObjects]
    public class BarrelRollingEditor : Editor
    {
        protected virtual void OnSceneGUI()
        {
            var br = (BarrelRolling)target;

            EditorGUI.BeginChangeCheck();
            var newStartPos = Handles.PositionHandle(br.StartPosition, Quaternion.identity);
            var newEndPos = Handles.PositionHandle(br.EndPosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(br, "Change Barrel Start and End Positions");
                br.StartPosition = newStartPos;
                br.EndPosition = newEndPos;
            }
        }
    }
}
