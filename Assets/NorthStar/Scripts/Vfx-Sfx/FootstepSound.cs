// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Assertions;

namespace NorthStar
{
    /// <summary>
    /// Plays footstep sounds for NPC's
    /// </summary>
    public class FootstepSound : MonoBehaviour
    {
        [SerializeField] private AudioSource m_audioSource = null;
        [SerializeField] private AudioClip[] m_lightStepAudioClips = new AudioClip[0];
        [Tooltip("Ideally audioclips shouldn't need volume scaling, but if needed can scale volume here"), Range(0f, 1f)]
        [SerializeField] private float m_lightStepVolumeMultiplier = 1f;
        [SerializeField] private AudioClip[] m_heavyStepAudioClips = new AudioClip[0];
        [Tooltip("Ideally audioclips shouldn't need volume scaling, but if needed can scale volume here"), Range(0f, 1f)]
        [SerializeField] private float m_heavyStepVolumeMultiplier = 1f;
        [Tooltip("If true, plays light footstep when generic footstep event is received. If false, plays heavy footstep")]
        [SerializeField] private bool m_fallbackToLightFootstep = true;

        private void OnEnable()
        {
            // Ensure we have an audio source
            Assert.IsNotNull(m_audioSource);

            // Ensure we do not have an empty audioclip array (Otherwise this component is pointless)
            Assert.AreNotEqual(0, m_lightStepAudioClips.Length);
            Assert.AreNotEqual(0, m_heavyStepAudioClips.Length);

            // Ensure no audio clips are null (Since the user is likely not expecting silence when the animation event occurs)
            for (var i = 0; i < m_lightStepAudioClips.Length; i++)
                Assert.IsNotNull(m_lightStepAudioClips[i]);
            for (var i = 0; i < m_heavyStepAudioClips.Length; i++)
                Assert.IsNotNull(m_heavyStepAudioClips[i]);
        }

        // Called from AnimationEvent
        private void LightFootstep()
        {
            //Debug.Log("Playing light footstep on " +gameObject.name);
            // Pick a random audio clip and play it. (Using one shot means clips can overlap)
            var index = Random.Range(0, m_lightStepAudioClips.Length);
            var audioClip = m_lightStepAudioClips[index];
            m_audioSource.PlayOneShot(audioClip, m_lightStepVolumeMultiplier);
        }
        private void HeavyFootstep()
        {
            //Debug.Log("Playing heavy footstep on " + gameObject.name);
            // Pick a random audio clip and play it. (Using one shot means clips can overlap)
            var index = Random.Range(0, m_heavyStepAudioClips.Length);
            var audioClip = m_heavyStepAudioClips[index];
            m_audioSource.PlayOneShot(audioClip, m_heavyStepVolumeMultiplier);
        }

        //For legacy support, if a generic footstep event is received, just play a light footstep sound
        private void Footstep()
        {
            if (m_fallbackToLightFootstep)
                LightFootstep();
            else
                HeavyFootstep();
        }

    }
}
