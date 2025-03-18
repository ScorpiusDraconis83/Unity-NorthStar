// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    public class PlayableDirectorPauser : MonoBehaviour
    {
        [SerializeField] private PlayableDirector m_playableDirector;
        private bool m_playing;

        [ContextMenu("Play")]
        public void Play()
        {
            m_playing = true;
        }

        [ContextMenu("Pause")]
        public void Pause()
        {
            m_playing = false;
        }

        private void Update()
        {
            if (m_playableDirector.playableGraph.IsValid() && m_playableDirector.playableGraph.GetPlayableCount() > 0)
                m_playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(m_playing ? 1 : 0);
        }
    }
}
