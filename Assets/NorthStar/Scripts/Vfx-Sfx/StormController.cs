// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controller for the storm effects system, manages spawning lightning VFX, sounds and adjusting global lighting and reflection dynamically
    /// </summary>
    public class StormController : MonoBehaviour
    {
        [SerializeField] private bool m_strikePositionsRelativeToBoat = true;
        [Tooltip("Tracked object that moves to the 'real' location of the boats fake movement")]
        [SerializeField] private Transform m_boatTrackerTransform;
        [SerializeField] private LightningFlashScriptableObject[] m_lightningFlashScriptableObjects;
        [SerializeField] private EnvironmentSystem m_environmentSystem;
        [Tooltip("Enabled but inactive particle systems that will be moved and have then have a single play event sent to them")]
        [SerializeField] private ParticleSystem[] m_lightningEffectsPool;
        private int m_nextEffectIndex;
        [Tooltip("Enabled but inactive audiosources that will be moved and have then have a single play event sent to them")]
        [SerializeField] private AudioSource[] m_audioSourcesPool;
        private int m_nextAudioSourceIndex;
        [SerializeField] private ReflectionSources[] m_reflectionSources;
        [Tooltip("Speed of sound used for delay of sound playing on lightning strikes, in units per second. Earth sea level is 343m/s")]
        [SerializeField, Range(10f, 1000f)] private float m_speedOfSound = 343f;
        [Tooltip("it will currently cause visual issues if multiple concurrent lightning strikes happen.")]
        [SerializeField] private bool m_allowConcurrentStrikes = false;
        private bool m_lightningStrikeInProgress = false;
        [Tooltip("Time that must elapse after a strike before a new one can occur. Ignored if allowConcurrentStrikes is true.")]
        [SerializeField, Range(0f, 10f)] private float m_minTimeBetweenStrikes = 0.2f;
        [Tooltip("Multiplier to increase the range of lightning strikes")]
        [SerializeField, Range(1f, 3f)] private float m_strikeDistanceMultiplier = 1.0f;

        private void Awake()
        {
            foreach (var lightningFlashSO in m_lightningFlashScriptableObjects)
            {
                lightningFlashSO.Setup();
            }
        }

        [ContextMenu("Do a random lightning strike")]
        public void DoRandomLightningStrike()
        {
            if (!m_allowConcurrentStrikes && !m_lightningStrikeInProgress)
            {
                var lfso = m_lightningFlashScriptableObjects[Random.Range(0, m_lightningFlashScriptableObjects.Length)];
                _ = StartCoroutine(DoLightningStrike(lfso));
            }
        }

        public void DoSpecificLightningStrike(LightningFlashScriptableObject lfso)
        {
            if (!m_allowConcurrentStrikes && !m_lightningStrikeInProgress)
            {
                _ = StartCoroutine(DoLightningStrike(lfso));
            }
        }

        private IEnumerator DoLightningStrike(LightningFlashScriptableObject lfso)
        {
            m_lightningStrikeInProgress = true;
            //Start environment transition
            m_environmentSystem.SetOneOffTransitionTime(lfso.strikeWarmupTime);
            m_environmentSystem.SetProfile(lfso.strikeEnvironmentProfile);
            //Go through all the blended reflection probes and look for reflections in the scriptable object that apply to that probe location
            for (var i = 0; i < m_reflectionSources.Length; i++)
            {
                if (m_reflectionSources[i].blendedReflectionProbe is not null)
                {
                    var refTextures = lfso.GetTextures(m_reflectionSources[i].reflectionLocation);
                    if (refTextures is not null)
                    {
                        UpdateBlendedReflectionProbe(m_reflectionSources[i].blendedReflectionProbe, refTextures.noFlashTexture, refTextures.flashTexture, lfso.strikeWarmupTime);
                    }
                }
            }
            yield return new WaitForSeconds(lfso.strikeWarmupTime);
            //move the lightning strike particle effect
            var lightningStrikeDir = Quaternion.AngleAxis(lfso.strikeDirection, Vector3.up) * Vector3.forward;
            var lightningStrikePos = new Vector3();
            if (m_strikePositionsRelativeToBoat && m_boatTrackerTransform is not null)
            {
                //Don't want to rotate the lightning strikes relative to boat at this time as it will break the light direction in the environment profile so this line is disabled at this time
                //lightningStrikeDir = Quaternion.AngleAxis(lfso.strikeDirection, Vector3.up) * BoatController.Instance.MovementSource.CurrentRotation * Vector3.forward;
                lightningStrikePos = m_boatTrackerTransform.position + (lightningStrikeDir * lfso.strikeDistance * m_strikeDistanceMultiplier);
                lightningStrikePos.y = 0f;
            }
            else
            {
                lightningStrikePos = lightningStrikeDir * lfso.strikeDistance;
            }
            var lightningEffect = m_lightningEffectsPool[m_nextEffectIndex];
            m_nextEffectIndex++;
            if (m_nextEffectIndex >= m_lightningEffectsPool.Length)
            {
                m_nextEffectIndex = 0;
            }
            lightningEffect.transform.position = lightningStrikePos;
            lightningEffect.Play();
            //Move audio source, then set it to play on a delay
            var lightningAudioSource = m_audioSourcesPool[m_nextAudioSourceIndex];
            if (m_nextAudioSourceIndex >= m_audioSourcesPool.Length)
            {
                m_nextEffectIndex = 0;
            }
            lightningAudioSource.transform.position = lightningStrikePos;
            _ = StartCoroutine(PlayAudioDelayed(lightningAudioSource, lfso.strikeDistance * m_strikeDistanceMultiplier / m_speedOfSound, lfso.strikeAudioClips[Random.Range(0, lfso.strikeAudioClips.Length)]));
            yield return new WaitForSeconds(lfso.strikeDuration);
            //start environment transition
            m_environmentSystem.SetOneOffTransitionTime(lfso.strikeCooldownTime);
            m_environmentSystem.SetProfile(lfso.postStrikeEnvironmentProfile);
            //update reflections
            for (var i = 0; i < m_reflectionSources.Length; i++)
            {
                if (m_reflectionSources[i].blendedReflectionProbe is not null)
                {
                    var refTextures = lfso.GetTextures(m_reflectionSources[i].reflectionLocation);
                    if (refTextures is not null)
                    {
                        UpdateBlendedReflectionProbe(m_reflectionSources[i].blendedReflectionProbe, refTextures.flashTexture, refTextures.noFlashTexture, lfso.strikeCooldownTime);
                    }
                }
            }
            yield return new WaitForSeconds(lfso.strikeCooldownTime + m_minTimeBetweenStrikes);
            m_lightningStrikeInProgress = false;
        }

        private IEnumerator PlayAudioDelayed(AudioSource audioSource, float delay, AudioClip audioClip)
        {
            yield return new WaitForSeconds(delay);
            audioSource.PlayOneShot(audioClip);
        }

        private void UpdateBlendedReflectionProbe(BlendedReflectionProbe brp, Cubemap srcCubemap, Cubemap dstCubemap, float blendTime)
        {
            brp.CubemapSrc = srcCubemap;
            brp.CubemapDst = dstCubemap;
            brp.StartBlendOverTime(blendTime);
        }
    }

    [System.Serializable]
    public class ReflectionSources
    {
        [SerializeField] public ReflectionLocation reflectionLocation;
        [SerializeField] public BlendedReflectionProbe blendedReflectionProbe;
    }

    public enum ReflectionLocation
    {
        World,
        WholeBoat,
        ForeDeck,
        MainDeck,
        QuarterDeck,
        TopDeck,
        Dock,
        Raft,
        Spare001,
        Spare002,
        Spare003
    }
}
