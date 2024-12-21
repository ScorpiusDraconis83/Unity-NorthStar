// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Plays sounds when a level is pulled
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LeverSounds : MonoBehaviour
    {
        private AudioSource m_audioSource;

        [Tooltip("For now just a basic sound, later we'll want a fancier solution")]
        public AudioClip basicInteractionSound;

        public float timeBetweenSoundTriggers = 1f;

        private float m_lastTriggerTime = 0f;

        private void Start()
        {
            m_audioSource = GetComponent<AudioSource>();
        }

        public void LeverInteractedWith()
        {
            //We'll want the type of sound made by the lever to change based on the interaction the player is having with it, playing the whole time the player is moving it
            //For now as a basic first pass this just plays the full audioclip if it's been more than the set limit time since the last interaction
            if (Time.time - m_lastTriggerTime >= timeBetweenSoundTriggers && basicInteractionSound != null)
            {
                m_audioSource.clip = basicInteractionSound;
                m_audioSource.Play();
                m_lastTriggerTime = Time.time;
            }
        }

    }
}
