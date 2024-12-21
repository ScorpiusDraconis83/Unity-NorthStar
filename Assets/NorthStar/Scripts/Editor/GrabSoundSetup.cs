// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Tool for setting up sounds when people grab objects
    /// </summary>
    public class GrabSoundSetup : EditorWindow
    {
        public PhysicsTransformer Grabbable;
        public bool SeperateGrabAndReleaseSounds;

        [MenuItem("Window/Grab Sound Setup")]
        public static void GetWindow()
        {
            _ = GetWindow(typeof(GrabSoundSetup));
        }
        private void OnGUI()
        {
            GUILayout.Label("Muli select all grab/release sounds and hit the button");
            Grabbable = EditorGUILayout.ObjectField(Grabbable, typeof(PhysicsTransformer), true) as PhysicsTransformer;
            SeperateGrabAndReleaseSounds = EditorGUILayout.Toggle("Seperate grab and release sounds", SeperateGrabAndReleaseSounds);
            if (GUILayout.Button(SeperateGrabAndReleaseSounds ? "Set up grab" : "Set up"))
            {
                if (Grabbable == null)
                {
                    return;
                }
                if (!Grabbable.TryGetComponent(out OnGrabEvents grabEvents))
                {
                    grabEvents = Grabbable.gameObject.AddComponent<OnGrabEvents>();
                }
                var grabSounds = CreateSoundPlayer(Grabbable.gameObject, "Grab Sounds");
                grabSounds.Clips = GetSelectedClips();
                UnityEventTools.AddPersistentListener(grabEvents.OnGrab, grabSounds.Play);

                if (!SeperateGrabAndReleaseSounds)
                {
                    UnityEventTools.AddPersistentListener(grabEvents.OnRelease, grabSounds.Play);
                }
            }

            if (SeperateGrabAndReleaseSounds)
            {
                if (GUILayout.Button("Set up release"))
                {
                    if (Grabbable == null)
                    {
                        return;
                    }
                    OnGrabEvents grabEvents = Grabbable.TryGetComponent(out grabEvents) ? grabEvents : Grabbable.gameObject.AddComponent<OnGrabEvents>();
                    var grabSounds = CreateSoundPlayer(Grabbable.gameObject, "Release Sounds");
                    grabSounds.Clips = GetSelectedClips();
                    UnityEventTools.AddPersistentListener(grabEvents.OnRelease, grabSounds.Play);
                }
            }
        }

        private SoundPlayer CreateSoundPlayer(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.parent = parent.transform;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var sound = go.AddComponent<SoundPlayer>();
            return sound;
        }

        private List<AudioClip> GetSelectedClips()
        {
            var clips = new List<AudioClip>();
            foreach (var obj in Selection.objects)
            {
                if (obj is AudioClip clip)
                {
                    clips.Add(clip);
                }
            }
            return clips;
        }
    }
}
