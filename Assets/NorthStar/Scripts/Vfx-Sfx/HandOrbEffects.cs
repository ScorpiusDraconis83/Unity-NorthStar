// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class HandOrbEffects : MonoBehaviour
    {
        public ParticleSystem[] handParticleSystems;

        //This could be simplified to having fewer variables to manage by using exact values in the animation curves, rather than using them as a multiplier
        //However scaling of animation curves in Unity Editor can be quite frustrating, so individual defined values for the min and max range has been found to
        //allow easier iteration on the effects without needing to change the whole curve.
        [Tooltip("Gravity effect on bubbles is negative since they are buoyant!")]
        [Range(-0.02f, 0f)]
        public float m_particleMinGravity = 0f;
        [Tooltip("Gravity effect on bubbles is negative since they are buoyant!")]
        [Range(-0.02f, 0f)]
        public float m_particleMaxGravity = -0.01f;
        public AnimationCurve m_particleGravityCurve;
        [Range(0f, 100f)]
        public float m_particleMinRateOverTime = 0f;
        [Range(0f, 100f)]
        public float m_particleMaxRateOverTime = 65f;
        public AnimationCurve m_particleRateOverTimeCurve;
        [Range(0f, 0.03f)]
        public float m_particleLowerMaxSize = 0.009f;
        [Range(0f, 0.03f)]
        public float m_particleUpperMaxSize = 0.02f;
        public AnimationCurve m_particleMaxSizeCurve;
        [Range(0f, 2f)]
        public float m_particleMinAttractionForceMultiplier = 0f;
        [Range(0f, 2f)]
        public float m_particleMaxAttractionForceMultiplier = 1f;
        [Tooltip("How the orbs particle attraction force should lerp between min and max intensity, sampling curve using the orbAttraction value")]
        public AnimationCurve m_particleAttractionForceMultiplierCurve;
        [Tooltip("Maximum speed the hand power can go up from 0 to 1 (per second value)")]
        [Range(0f, 5f)]
        public float m_powerWarmupRate = 2f;
        [Tooltip("Maximum speed the hand power can go down from 1 to 0 (per second value)")]
        [Range(0f, 5f)]
        public float m_powerCooldownRate = 0.5f;

        public Transform m_PalmTransformRef;
        private bool m_handOpen;
        private bool m_handNotClosed;
        private float m_handOpenness;
        private float m_targetHandPower;
        public float HandPower { get; private set; }

        public OrbAttraction m_orbAttraction;
        public Transform m_orbTransform;

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
            foreach (var ps in handParticleSystems)
            {
                //how much the particles are attracted to the orb is determined by the overall hand power
                ps.startSize = Mathf.Lerp(m_particleLowerMaxSize, m_particleUpperMaxSize, m_particleMaxSizeCurve.Evaluate(HandPower));
                ps.gravityModifier = Mathf.Lerp(m_particleMinGravity, m_particleMaxGravity, m_particleGravityCurve.Evaluate(HandPower));
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
            var targetDir = m_PalmTransformRef.position - m_orbTransform.position;
            var angle = Vector3.Angle(targetDir, m_PalmTransformRef.up);
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
                m_handNotClosed = true;
                if (m_handOpen)
                {
                    m_handOpenness = 1;
                }
                else
                {
                    m_handOpenness = 0.5f;
                }
            }
            else
            {
                m_handNotClosed = false;
                if (m_handOpen)
                {
                    m_handOpenness = 1;
                }
                else
                {
                    m_handOpenness = 0f;
                }
            }
        }
    }
}
