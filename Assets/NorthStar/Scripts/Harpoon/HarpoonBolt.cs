// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// The projectile launched from the harpoon
    /// </summary>
    public class HarpoonBolt : MonoBehaviour
    {
        [SerializeField] public Rigidbody m_rigidbody;
        [SerializeField] private LineRenderer m_lineRenderer;
        [SerializeField] private float m_maxAirTime = 20;
        [SerializeField] private float m_airSpeed = 5;
        [SerializeField] private float m_colliderDelay = 0.2f;
        [SerializeField] private Collider m_collider;
        public bool UseVisualOffset;
        [SerializeField] private bool m_hideOnHit = false;
        [SerializeField] private Transform m_visual;
        [SerializeField] private Transform m_tip, m_end;
        [SerializeField] private float m_visualYOffset = .01f;
        [SerializeField] private AnimationCurve m_distanceOffsetCurve;
        [SerializeField] private LayerMask m_layerMask = int.MaxValue;
        [SerializeField] private EffectAsset m_hitParticles;

        private List<Vector3> m_points = new();

        private Vector3 m_firedPosition;
        private Vector3 m_landedPosition;


        private bool m_airborne = true;
        public bool IsAirborne => m_airborne;
        private float m_airTimeRemaining;
        private float m_colliderDelayTimeRemaining;

        private HarpoonTarget m_hitTarget;

        private void Start()
        {
            m_firedPosition = transform.position;
            m_points.Add(transform.position);
            m_airTimeRemaining = m_maxAirTime;
            m_colliderDelayTimeRemaining = m_colliderDelay;
            m_airborne = true;
            m_rigidbody.isKinematic = false;
            if (m_collider) m_collider.enabled = false;
        }

        private void Update()
        {
            if (m_airborne)
            {
                m_points.Add(transform.position);

                m_lineRenderer.positionCount = m_points.Count;
                m_lineRenderer.SetPositions(m_points.ToArray());
                transform.forward = m_rigidbody.velocity.normalized;

                // enable the collider after a certain time has passed
                if (m_collider && !m_collider.enabled && m_colliderDelayTimeRemaining > 0)
                {
                    m_colliderDelayTimeRemaining -= Time.deltaTime;
                    if (m_colliderDelayTimeRemaining <= 0)
                    {
                        m_collider.enabled = true;
                    }
                }

                //After too much time has passed, force the projectile to stop and be reeled in
                m_airTimeRemaining -= Time.deltaTime;
                if (m_airTimeRemaining < 0)
                {
                    m_airborne = false;
                    m_rigidbody.isKinematic = true;
                    m_landedPosition = transform.position;
                }
            }
            else
            {
                if (!UseVisualOffset)
                    return;
                var tipHeight = SampleHeight(m_tip.position);
                var endHeight = SampleHeight(m_end.position);

                var centreHeight = (tipHeight + endHeight) / 2;
                var pos = transform.position;
                pos.y = centreHeight + m_visualYOffset;
                m_visual.position = pos;

                var tipPos = m_tip.position;
                tipPos.y = tipHeight;

                var endPos = m_end.position;
                endPos.y = endHeight;

                m_visual.forward = tipPos - endPos;

                if (m_hitTarget != null)
                {
                    m_hitTarget.transform.position = tipPos;
                }
            }
        }

        private float SampleHeight(Vector3 pos)
        {
            var t = m_distanceOffsetCurve.Evaluate(HorizontalDistance(pos, m_firedPosition));
            if (Physics.Raycast(pos, Vector3.down, out var hit, float.PositiveInfinity, m_layerMask))
            {
                return Mathf.Lerp(hit.point.y, pos.y, t);
            }
            else
            {
                if (EnvironmentSystem.Instance is not null)
                {
                    return Mathf.Lerp(EnvironmentSystem.Instance.GetOceanHeight(pos), pos.y, t);
                }
            }
            return pos.y;

        }

        private float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b);
        }

        public void ReelBolt(float value)
        {
            if (!m_airborne)
            {
                transform.position = Vector3.Lerp(m_landedPosition, m_firedPosition, value);
            }

            if (value >= 0.95f)
            {
                if (m_hitTarget != null)
                {
                    if (m_hitTarget.TryGetComponent(out Rigidbody body))
                    {
                        body.isKinematic = false;
                    }
                    m_hitTarget.Reeled();
                }
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (m_airborne)
            {
                m_airborne = false;
                m_rigidbody.isKinematic = true;
                m_landedPosition = transform.position;

                if (collision.collider.attachedRigidbody != null)
                {
                    if (collision.collider.attachedRigidbody.TryGetComponent(out HarpoonTarget target))
                    {
                        target.Hit();
                        m_hitTarget = target;
                        if (target.Reelable)
                        {
                            if (m_hitTarget.TryGetComponent(out Rigidbody body))
                            {
                                body.isKinematic = true;
                            }
                        }
                    }
                }

                if (m_hitParticles != null)
                {
                    m_hitParticles.Play(m_tip.position, transform.rotation, true);
                }

                if (m_hideOnHit)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
