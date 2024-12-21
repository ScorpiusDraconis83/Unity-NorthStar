// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// Overrides timeline playback speed
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class PlayableDirectorSpeedController : MonoBehaviour
    {
        public PlayableDirector pd;
        public bool m_overrideTimelineSpeed;

        [Range(0.0f, 2.0f)]
        public float m_timelineSpeed = 1.0f;

        private void Start()
        {
            if (pd == null)
            {
                pd = GetComponent<PlayableDirector>();
            }
            if (m_overrideTimelineSpeed)
            {
                pd.playableGraph.GetRootPlayable(0).SetSpeed(m_timelineSpeed);
            }
        }

        public void SetSpeed(float speed)
        {
            m_timelineSpeed = speed;
        }

        public void OverrideTimelineSpeed(bool b)
        {
            m_overrideTimelineSpeed = b;
        }

        private void Update()
        {
            if (m_overrideTimelineSpeed)
            {
                pd.playableGraph.GetRootPlayable(0).SetSpeed(m_timelineSpeed);
            }

        }
    }
}
