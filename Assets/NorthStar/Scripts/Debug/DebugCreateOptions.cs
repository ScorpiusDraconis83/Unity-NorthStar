// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using Meta.Utilities.Environment;
using Meta.Utilities.Narrative;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NorthStar
{
    public class DebugCreateOptions : UiMenu
    {
        [SerializeField] private EnvironmentProfile[] m_environmentProfiles;
        [Serializable]
        private class SceneLoadTarget
        {
            public string Name;
            public TaskID Task;
            public string SceneName;
        }
        [SerializeField] private SceneLoadTarget[] m_startBeatNarrativeSequences;
        [SerializeField] private DebugMenu m_debugMenu;

        private const string CALIBRATION_CATEGORY = "Player Calibration";
        private const string HAND_LIMITS_CATEGORY = "Hand Limits";
        private const string SCENE_LOADING_CATEGORY = "Available Scenes";
        private const string ENVIRONMENT_PROFILE_CATEGORY = "Environment Profiles";
        private const string NARRATIVE_CATEGORY = "Narrative Settings";
        private const string PHYSICS_SIM_CATEGORY = "Physics Simulation";

        protected override void OnStart()
        {
            DebugSystem.Instance.ResetOptions();
            AddPlayerHandLimits();
            AddEnvironmentProfileOptions();
            AddNarrativeOptions();
            AddPhysicsSimulationOptions();
        }

        private void AddPhysicsSimulationOptions()
        {
            _ = DebugSystem.Instance.AddAction(PHYSICS_SIM_CATEGORY, "Set fixed timestep to 36 hz", () =>
            {
                Time.fixedDeltaTime = 1.0f / 36.0f;
            });

            _ = DebugSystem.Instance.AddAction(PHYSICS_SIM_CATEGORY, "Set fixed timestep to 50 hz", () =>
            {
                Time.fixedDeltaTime = 1.0f / 50.0f;
            });

            _ = DebugSystem.Instance.AddAction(PHYSICS_SIM_CATEGORY, "Set fixed timestep to 72 hz", () =>
            {
                Time.fixedDeltaTime = 1.0f / 72.0f;
            });

            _ = DebugSystem.Instance.AddAction(PHYSICS_SIM_CATEGORY, "Set fixed timestep to 100 hz", () =>
            {
                Time.fixedDeltaTime = 1.0f / 100.0f;
            });

            _ = DebugSystem.Instance.AddWatch(PHYSICS_SIM_CATEGORY, "Current fixed timestep", () =>
            {
                return $"{Mathf.RoundToInt(1.0f / Time.fixedDeltaTime)} hz";
            });
        }

        private void AddPlayerCalibrationOptions()
        {
            var standingValue = DebugSystem.Instance.AddBool(CALIBRATION_CATEGORY, "Seated", false, true, (value) =>
            {
                GlobalSettings.PlayerSettings.Seated = value;
                GlobalSettings.PlayerSettings.PlayerCalibrationChanged();
            });

            var heightValue = DebugSystem.Instance.AddFloat(CALIBRATION_CATEGORY, "Height", 100, 300, 175, true, (value) =>
            {
                GlobalSettings.PlayerSettings.Height = value;
                GlobalSettings.PlayerSettings.PlayerCalibrationChanged();
            });

            var seatedHeightValue = DebugSystem.Instance.AddFloat(CALIBRATION_CATEGORY, "Seated Height", 100, 300, 92, true, (value) =>
            {
                GlobalSettings.PlayerSettings.SeatedHeight = value;
                GlobalSettings.PlayerSettings.PlayerCalibrationChanged();
            });

            _ = DebugSystem.Instance.AddAction(CALIBRATION_CATEGORY, "Calibrate Height", () =>
            {
                heightValue.Value = Camera.main.transform.localPosition.y * 100; // TODO: clean this up
            });

            _ = DebugSystem.Instance.AddAction(CALIBRATION_CATEGORY, "Calibrate Seated Height", () =>
            {
                seatedHeightValue.Value = Camera.main.transform.localPosition.y * 100; // TODO: clean this up
            });

            _ = DebugSystem.Instance.AddWatch(CALIBRATION_CATEGORY, "Camera Height", () =>
            {
                return $"{Camera.main.transform.localPosition.y * 100:F1} cm";
            });
        }

        private void AddPlayerHandLimits()
        {
            _ = DebugSystem.Instance.AddWatch(HAND_LIMITS_CATEGORY, "Left Hand Angles", () =>
            {
                var leftHandAngles = BodyPositions.Instance.GetRelativeHandAngles(HumanBodyBones.LeftHand);
                return $"({leftHandAngles.x:F1}, {leftHandAngles.y:F1}, {leftHandAngles.z:F1})";
            });

            _ = DebugSystem.Instance.AddWatch(HAND_LIMITS_CATEGORY, "Left Hand Within Limits", () =>
            {
                return $"({BodyPositions.Instance.IsHandWithinLimits(HumanBodyBones.LeftHand)})";
            });

            _ = DebugSystem.Instance.AddWatch(HAND_LIMITS_CATEGORY, "Right Hand Angles", () =>
            {
                var rightHandAngles = BodyPositions.Instance.GetRelativeHandAngles(HumanBodyBones.RightHand);
                return $"({rightHandAngles.x:F1}, {rightHandAngles.y:F1}, {rightHandAngles.z:F1})";
            });

            _ = DebugSystem.Instance.AddWatch(HAND_LIMITS_CATEGORY, "Right Hand Within Limits", () =>
            {
                return $"({BodyPositions.Instance.IsHandWithinLimits(HumanBodyBones.RightHand)})";
            });

        }

        private void AddSceneLoadOptions()
        {
            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                //Go through Build Settings, find out the name for each scene, and then add a button to the debug menu to load it in
                //Code for getting scene name from path came from here: https://stackoverflow.com/a/40901893
                var buildIndex = i;
                var sceneName = GetSceneNameByBuildIndex(buildIndex);
                _ = DebugSystem.Instance.AddAction(SCENE_LOADING_CATEGORY, sceneName, () =>
                {
                    LoadScreen.Instance.Load(sceneName, false);
                });
            }
        }

        private void AddEnvironmentProfileOptions()
        {
            foreach (var profile in m_environmentProfiles)
            {
                AddEnvironmentProfileOption(profile);
            }
            _ = DebugSystem.Instance.AddAction(ENVIRONMENT_PROFILE_CATEGORY, $"Make next transition instant", () =>
            {
                EnvironmentSystem.Instance.SetOneOffTransitionTime(0f);
            });
        }

        private void AddNarrativeOptions()
        {
            _ = DebugSystem.Instance.AddAction(NARRATIVE_CATEGORY, "Skip Task", () =>
            {
                foreach (var task in TaskManager.CurrentTasks)
                {
                    TaskManager.HandlerForTask(task.ID).Skip();
                }
            });

            for (var i = 0; i < m_startBeatNarrativeSequences.Length; i++)
            {
                AddStartBeatNarrativeOption(m_startBeatNarrativeSequences[i]);
            }
        }

        private void AddStartBeatNarrativeOption(SceneLoadTarget sequence)
        {
            _ = DebugSystem.Instance.AddAction(NARRATIVE_CATEGORY, $"Start from {sequence.Name}", () =>
            {
                LoadScreen.Instance.LoadWithTaskID(sequence.SceneName, sequence.Task);
            });
        }

        private void AddEnvironmentProfileOption(EnvironmentProfile profile)
        {
            _ = DebugSystem.Instance.AddAction(ENVIRONMENT_PROFILE_CATEGORY, $"{profile.name}", () =>
            {
                EnvironmentSystem.Instance.SetProfile(profile);
            });
        }

        private string GetSceneNameByBuildIndex(int buildIndex)
        {
            return GetSceneNameFromScenePath(SceneUtility.GetScenePathByBuildIndex(buildIndex));
        }

        private string GetSceneNameFromScenePath(string scenePath)
        {
            // Unity's asset paths always use '/' as a path separator
            var sceneNameStart = scenePath.LastIndexOf("/", StringComparison.Ordinal) + 1;
            var sceneNameEnd = scenePath.LastIndexOf(".", StringComparison.Ordinal);
            var sceneNameLength = sceneNameEnd - sceneNameStart;
            return scenePath.Substring(sceneNameStart, sceneNameLength);
        }

    }
}