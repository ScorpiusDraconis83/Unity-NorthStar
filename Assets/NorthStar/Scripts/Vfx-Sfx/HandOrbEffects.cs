// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class HandOrbEffects : MonoBehaviour
    {
        public ParticleSystem[] HandParticleSystems;

        //This could be simplified to having fewer variables to manage by using exact values in the animation curves, rather than using them as a multiplier
        //However scaling of animation curves in Unity Editor can be quite frustrating, so individual defined values for the min and max range has been found to
        //allow easier iteration on the effects without needing to change the whole curve.
        [Tooltip("Gravity effect on bubbles is negative since they are buoyant!")]
        [Range(-0.02f, 0f)]
        [SerializeField] private float m_particleMinGravity = 0f;
        [Tooltip("Gravity effect on bubbles is negative since they are buoyant!")]
        [Range(-0.02f, 0f)]
        [SerializeField] private float m_particleMaxGravity = -0.01f;
        [SerializeField] private AnimationCurve m_particleGravityCurve;
        [Range(0f, 100f)]
        [SerializeField] private float m_particleMinRateOverTime = 0f;
        [Range(0f, 100f)]
        [SerializeField] private float m_particleMaxRateOverTime = 65f;
        [SerializeField] private AnimationCurve m_particleRateOverTimeCurve;
        [Range(0f, 0.03f)]
        [SerializeField] private float m_particleLowerMaxSize = 0.009f;
        [Range(0f, 0.03f)]
        [SerializeField] private float m_particleUpperMaxSize = 0.02f;
        [SerializeField] private AnimationCurve m_particleMaxSizeCurve;
        [Range(0f, 2f)]
        [SerializeField] private float m_particleMinAttractionForceMultiplier = 0f;
        [Range(0f, 2f)]
        [SerializeField] private float m_particleMaxAttractionForceMultiplier = 1f;
        [Tooltip("How the orbs particle attraction force should lerp between min and max intensity, sampling curve using the orbAttraction value")]
        [SerializeField] private AnimationCurve m_particleAttractionForceMultiplierCurve;
        [Tooltip("Maximum speed the hand power can go up from 0 to 1 (per second value)")]
        [Range(0f, 5f)]
        [SerializeField] private float m_powerWarmupRate = 2f;
        [Tooltip("Maximum speed the hand power can go down from 1 to 0 (per second value)")]
        [Range(0f, 5f)]
        [SerializeField] private float m_powerCooldownRate = 0.5f;

        [SerializeField] private Transform m_palmTransformRef;
        private bool m_handOpen;
        private float m_handOpenness;
        private float m_targetHandPower;
        public float HandPower { get; private set; }
        public bool HandNotClosed { get; private set; }

        [SerializeField] private OrbAttraction m_orbAttraction;
        [SerializeField] private Transform m_orbTransform;

        private void Start()
        {
            if (m_orbAttraction is not null)
            {
                m_orbAttraction.AddHandToList(this);
            }
        }

        private void OnDestroy()
        {
            if (m_orbAttraction is not null)
            {
                m_orbAttraction.RemoveHandFromList(this);
            }
        }

        private void Update()
        {
            //For now doing all of this in update to avoid preemptive optimisation
            //If for whatever reason it has any significant cost can move to a coroutine that updates less regularly
            CalculateHandPower();
            UpdateParticleEffects();
        }

        private void UpdateParticleEffects()
        {
            foreach (var ps in HandParticleSystems)
            {
                //how much the particles are attracted to the orb is determined by the overall hand power
                var main = ps.main;
                main.startSizeMultiplier = Mathf.Lerp(m_particleLowerMaxSize, m_particleUpperMaxSize, m_particleMaxSizeCurve.Evaluate(HandPower));
                main.gravityModifierMultiplier = Mathf.Lerp(m_particleMinGravity, m_particleMaxGravity, m_particleGravityCurve.Evaluate(HandPower));
                var externalForces = ps.externalForces;
                externalForces.multiplier = Mathf.Lerp(m_particleMinAttractionForceMultiplier, m_particleMaxAttractionForceMultiplier, m_particleAttractionForceMultiplierCurve.Evaluate(HandPower));
                var emission = ps.emission;
                //number of particles is affected by how open the hand is
                emission.rateOverTime = Mathf.Lerp(m_particleMinRateOverTime, m_particleMaxRateOverTime, m_particleRateOverTimeCurve.Evaluate(m_handOpenness));
            }
        }

        private void CalculateHandPower()
        {
            //Calculate the power level of the players hand calling out to the orb
            //Power is based on how open the hand is, and how much the palm points towards the orb
            var targetDir = m_palmTransformRef.position - m_orbTransform.position;
            var angle = Vector3.Angle(targetDir, m_palmTransformRef.up);
            m_targetHandPower = angle / 180;
            m_targetHandPower *= m_handOpenness;
            if (HandPower < m_targetHandPower)
            {
                HandPower += m_powerWarmupRate * Time.deltaTime;
                if (HandPower > m_targetHandPower)
                {
                    HandPower = m_targetHandPower;
                }
            }
            else if (HandPower > m_targetHandPower)
            {
                HandPower -= m_powerCooldownRate * Time.deltaTime;
                if (HandPower < m_targetHandPower)
                {
                    HandPower = m_targetHandPower;
                }
            }
        }

        public void HandOpenSelected(bool value)
        {
            if (value)
            {
                m_handOpen = true;
                m_handOpenness = 1f;
            }
            else
            {
                m_handOpen = false;
            }
        }

        public void HandNotClosedSelected(bool value)
        {
            // Would be much cooler if these values were based on exactly how open the players fingers are
            // For now keeping it a bit more binary so we know effect feels good with controllers
            if (value)
            {
                HandNotClosed = true;
                m_handOpenness = m_handOpen ? 1 : 0.5f;
            }
            else
            {
                HandNotClosed = false;
                m_handOpenness = m_handOpen ? 1 : 0f;
            }
        }
    }
}
