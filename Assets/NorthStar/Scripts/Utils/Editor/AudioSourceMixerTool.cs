// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace NorthStar
{
    /// <summary>
    /// Tool for finding and selecting audio mixers in the current scene
    /// </summary>
    public class AudioSourceMixerTool : EditorWindow
    {
        private List<Tuple<string, AudioSource>> m_sources;
        private bool m_showAlreadyAssinged = false;
        private Vector2 m_scrollPos = Vector2.zero;

        [MenuItem("Tools/NorthStar/Audio Mixer tool")]
        public static void GetWindow()
        {
            _ = GetWindow(typeof(AudioSourceMixerTool));
        }
        private void OnGUI()
        {
            if (m_sources == null)
            {
                Rebuild();
            }

            if (GUILayout.Button(m_showAlreadyAssinged ? "Hide already set up" : "Show already set up"))
            {
                m_showAlreadyAssinged = !m_showAlreadyAssinged;
                Rebuild();
            }
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);

            foreach (var source in m_sources)
            {
                if (source.Item2 == null)
                {
                    Rebuild();
                    break;
                }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    Selection.SetActiveObjectWithContext(source.Item2, source.Item2);
                }
                GUILayout.Label(source.Item1);
                var valueWas = source.Item2.outputAudioMixerGroup;
                source.Item2.outputAudioMixerGroup = EditorGUILayout.ObjectField(source.Item2.outputAudioMixerGroup, typeof(AudioMixerGroup), true, GUILayout.Width(300)) as AudioMixerGroup;
                if (source.Item2.outputAudioMixerGroup != valueWas)
                {
                    EditorUtility.SetDirty(source.Item2);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void Rebuild()
        {
            m_sources = new();
            var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var source in sources)
            {
                var path = source.gameObject.name;
                var parent = source.transform.parent;
                while (parent != null)
                {
                    path = $"{parent.name}/{path}";
                    parent = parent.parent;
                }
                if (source.outputAudioMixerGroup != null && !m_showAlreadyAssinged)
                    continue;

                m_sources.Add(new Tuple<string, AudioSource>(path, source));
            }

            m_sources = m_sources.OrderBy(x => x.Item1).ToList();
        }
    }
}
