// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using Meta.Utilities;
using Meta.Utilities.Environment;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace NorthStar
{
    /// <summary>
    /// Detects the 'grab' teleport gesture on each hand
    /// </summary>
    public class GrabTeleport : MonoBehaviour
    {
        private static readonly int s_transition = Shader.PropertyToID("_Transition");
        [SerializeField] private ActiveStateSelector m_leftPalmOpenAction, m_rightPalmOpenAction;
        [SerializeField] private ActiveStateSelector m_leftGrabAction, m_rightGrabAction;
        [SerializeField] private HandGrabInteractor m_leftInteractor, m_rightInteractor;

        [SerializeField] private Transform m_head, m_leftAnchor, m_rightAnchor;
        [SerializeField] private Transform m_leftEye, m_rightEye;

        [SerializeField] public TeleportWaypoint LastTeleport;
        [SerializeField, AutoSet] private BodyPositions m_bodyPositions;
        [SerializeField, AutoSet] private ControllerMappings m_controllerMappings;
        [SerializeField] private LineRenderer m_lineRenderer;
        [SerializeField] private int m_curveSegments = 10;
        [SerializeField] private AnimationCurve m_curvePointDistribution;
        [SerializeField] private float m_warpDelay = 1f;
        [SerializeField] private ScreenFader m_screenFader;
        [SerializeField] private SoundPlayer m_teleportSound;
        [SerializeField] private SphereMaskRenderer m_sphereMaskRenderer;

        [SerializeField] private float m_criticalDistanceMin = 2f, m_criticalDistanceMax = 3f;

        private float m_warpTimer;

        [field: SerializeField] public UnityEvent<float> OnLeftHandTimerUpdate { get; private set; }
        [field: SerializeField] public UnityEvent<float> OnRightHandTimerUpdate { get; private set; }
        [field: SerializeField] public UnityEvent<TeleportWaypoint> OnTeleport { get; private set; }

        private bool m_useRightHand;
        private bool m_hasTarget = false;
        private TeleportWaypoint m_target;
        private Vector3 m_recordedHandLocalPosition;
        private Vector3 m_lastOffset = Vector3.zero;
        private float m_timer;

        private enum State
        {
            awaiting,
            aiming,
            locked,
            warping,
            recovery
        }
        [SerializeField] private State m_state;

        private void Start()
        {
            m_target = LastTeleport;
            DoWarp();
        }

        private void OnOpenHand(bool rightHand)
        {
            if (m_useRightHand != rightHand && m_state == State.aiming)
                m_useRightHand = rightHand;
            if (m_state != State.awaiting)
                return;
            m_state = State.aiming;
            m_useRightHand = rightHand;

            if (LastTeleport) LastTeleport.ShowConnections();
        }

        private void OnCloseHand(bool rightHand)
        {
            if (rightHand != m_useRightHand || m_state != State.aiming)
                return;
            if (!m_hasTarget)
                ResetState();
        }

        private void OnGrab(bool rightHand)
        {
            if (rightHand != m_useRightHand || m_state != State.aiming)
                return;
            if (!m_hasTarget || HandIsOccupied())
            {
                ResetState();
                return;
            }
            m_state = State.locked;
            m_recordedHandLocalPosition = rightHand ? m_rightAnchor.position : m_leftAnchor.position;
            m_timer = 0;
            m_target.OnLock();
        }

        private void OnGrabReleased(bool rightHand)
        {
            if (rightHand != m_useRightHand || m_state != State.locked)
                return;

            if (m_hasTarget)
            {
                m_timer = 0;
                m_state = State.aiming;
                m_target.OnSelect();
                m_lineRenderer.enabled = true;
                OnLeftHandTimerUpdate?.Invoke(0);
                OnRightHandTimerUpdate?.Invoke(0);
            }
            else
            {
                ResetState();
            }
        }

        private void ResetState()
        {
            if (m_hasTarget)
            {
                m_target.OnUnselect();
            }
            m_state = State.awaiting;
            m_hasTarget = false;
            LastTeleport.ShowConnections();
            m_lineRenderer.enabled = false;

            OnLeftHandTimerUpdate?.Invoke(0);
            OnRightHandTimerUpdate?.Invoke(0);
        }

        private void Awake()
        {
            m_leftPalmOpenAction.WhenSelected += () => { OnOpenHand(false); };
            m_rightPalmOpenAction.WhenSelected += () => { OnOpenHand(true); };
            m_controllerMappings.LeftStickForward += () => { OnOpenHand(false); };
            m_controllerMappings.RightStickForward += () => { OnOpenHand(true); };

            m_leftPalmOpenAction.WhenUnselected += () => { OnCloseHand(false); };
            m_rightPalmOpenAction.WhenUnselected += () => { OnCloseHand(true); };
            m_controllerMappings.LeftStickRelesed += () => { OnCloseHand(false); };
            m_controllerMappings.RightStickRelesed += () => { OnCloseHand(true); };

            m_leftGrabAction.WhenSelected += () => { OnGrab(false); };
            m_rightGrabAction.WhenSelected += () => { OnGrab(true); };
            m_controllerMappings.LeftStickRelesed += () => { OnGrab(false); };
            m_controllerMappings.RightStickRelesed += () => { OnGrab(true); };

            m_leftGrabAction.WhenUnselected += () => { OnGrabReleased(false); };
            m_rightGrabAction.WhenUnselected += () => { OnGrabReleased(true); };

            m_lastOffset = m_head.position;
            m_lastOffset.y = 0;
        }
        private struct TpTargetData
        {
            public TeleportWaypoint Target;
            public float HeadDot;
            public float ArmDot;
        }

        private bool HandIsOccupied()
        {
            return m_useRightHand ? m_rightInteractor.IsGrabbing : m_leftInteractor.IsGrabbing;
        }

        private void StartWarp()
        {
            m_state = State.warping;
            m_warpTimer = 0;
        }

        private void DoWarp()
        {
            var offset = m_head.position - transform.position;
            offset.y = 0;
            LastTeleport.OnHide();
            LastTeleport.HideConnections();
            foreach (var hand in m_bodyPositions.SyntheticHands)
            {
                if (hand.TryGetComponent(out PhysicalHand physicalHand))
                {
                    physicalHand.ForceDropHeldItem();
                }
            }
            LastTeleport = m_target.DoWarp(offset, transform, m_head);
            LastTeleport.ShowConnections();
            ResetState();
            m_state = State.recovery;
            OnTeleport?.Invoke(m_target);
        }

        private void SetLineTarget(Transform anchor, Transform target)
        {
            m_lineRenderer.useWorldSpace = false;
            m_lineRenderer.positionCount = m_curveSegments;

            var a = transform.InverseTransformPoint(anchor.position);
            var c = transform.InverseTransformPoint(target.position);
            var b = Vector3.Lerp(a, c, .5f);
            b.y = Mathf.Max(a.y, c.y);

            for (var i = 0; i < m_curveSegments; i++)
            {
                var t = (float)i / m_curveSegments;
                m_lineRenderer.SetPosition(i, BezierCurve(a, b, c, m_curvePointDistribution.Evaluate(t)));
            }
        }

        private static Vector3 BezierCurve(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            var ab = Vector3.Lerp(a, b, t);
            var bc = Vector3.Lerp(a, c, t);
            return Vector3.Lerp(ab, bc, t);
        }

        private void Update()
        {
            if (m_sphereMaskRenderer != null)
            {
                var distance = Vector3.Scale(m_head.position - LastTeleport.transform.position, new(1f, 0f, 1f)).magnitude;
                var targetIntensity = Mathf.InverseLerp(m_criticalDistanceMin, m_criticalDistanceMax, distance);
#if !UNITY_EDITOR   // This makes the simulator more difficult to use - disabled in editor
                m_sphereMaskRenderer.Intensity = targetIntensity;
#endif
            }

            var playerSettings = GlobalSettings.PlayerSettings;
            m_lineRenderer.enabled = m_state is State.aiming or State.locked && m_hasTarget;
            switch (m_state)
            {
                case State.awaiting:
                    {
                        break;
                    }
                case State.aiming:
                    {
                        if (HandIsOccupied())
                        {
                            ResetState();
                            break;
                        }
                        var shoulderPosition = m_bodyPositions.GetShoulderPosition(m_useRightHand);
                        var hand = m_useRightHand ? m_rightAnchor : m_leftAnchor;
                        var armDir = (hand.position - shoulderPosition).normalized;
                        var palmDir = m_controllerMappings.UsingControler() ? hand.forward : -hand.up;

                        Debug.DrawRay(shoulderPosition, armDir * 100, Color.magenta);
                        Debug.DrawLine(shoulderPosition, m_head.position, Color.magenta);

                        m_lineRenderer.material.SetFloat(s_transition, 0f);

                        var targets = new List<TpTargetData>();
                        if (m_hasTarget)
                        {
                            var toTarget = (m_target.LosCheckTarget.position - m_head.position).normalized;
                            var headDot = Vector3.Dot(toTarget, m_head.forward);
                            var armDot = Vector3.Dot(armDir, toTarget);
                            var palmDot = Vector3.Dot(palmDir, toTarget);

                            var headInRange = headDot > playerSettings.TeleporterHeadAngleBreak;
                            var armInRange = armDot > playerSettings.TeleporterArmAngleBreak;
                            var palmInRange = palmDot > playerSettings.TeleporterPalmAngleBreak;
                            var hasLos = !Physics.Linecast(m_head.position, m_target.LosCheckTarget.position, playerSettings.TeleporterLayers, QueryTriggerInteraction.Ignore);

                            SetLineTarget(m_useRightHand ? m_rightAnchor : m_leftAnchor, m_target.transform);

                            if (headInRange && armInRange && palmInRange && hasLos)
                                break;
                        }
                        foreach (var target in LastTeleport.Connections)
                        {
                            if (!target.gameObject.activeSelf)
                                continue;

                            var toTarget = (target.LosCheckTarget.position - m_head.position).normalized;
                            var headDot = Vector3.Dot(toTarget, m_head.forward);
                            var armDot = Vector3.Dot(armDir, toTarget);
                            var palmDot = Vector3.Dot(palmDir, toTarget);

                            if (headDot < GlobalSettings.PlayerSettings.TeleporterHeadAngleMin)
                                continue;
                            if (armDot < GlobalSettings.PlayerSettings.TeleporterArmAngleMin)
                                continue;
                            if (palmDot < GlobalSettings.PlayerSettings.TeleporterPalmAngleBreak)
                                continue;
                            if (Physics.Linecast(m_head.position, target.LosCheckTarget.position, playerSettings.TeleporterLayers, QueryTriggerInteraction.Ignore))
                                continue;

                            var newTarget = new TpTargetData()
                            {
                                Target = target,
                                HeadDot = headDot,
                                ArmDot = armDot,
                            };

                            targets.Add(newTarget);
                        }
                        if (targets.Count > 0)
                        {
                            targets.Sort((a, b) => a.ArmDot.CompareTo(b.ArmDot));
                            var bestTarget = targets[^1];
                            if (m_hasTarget && bestTarget.Target != m_target)
                            {
                                m_target.OnUnselect();
                            }
                            m_hasTarget = true;
                            m_target = bestTarget.Target;
                            m_target.OnSelect();
                        }
                        else
                        {
                            if (m_hasTarget)
                            {
                                m_target.OnUnselect();
                                m_hasTarget = false;
                            }
                        }
                        break;
                    }
                case State.locked:
                    {
                        if (HandIsOccupied())
                        {
                            ResetState();
                            break;
                        }

                        if (playerSettings.TeleportHandTriggerDelay > 0) // Use time based trigger
                        {
                            m_timer += Time.deltaTime;
                            var maxTimer = m_controllerMappings.UsingControler() ? 0 : playerSettings.TeleportHandTriggerDelay;
                            m_lineRenderer.material.SetFloat(s_transition, Mathf.Clamp01(m_timer / maxTimer));

                            if (m_timer >= maxTimer)
                            {
                                m_timer = 0;
                                StartWarp();
                            }

                            if (m_useRightHand)
                            {
                                OnRightHandTimerUpdate?.Invoke(Mathf.Clamp01(m_timer / maxTimer));
                            }
                            else
                            {
                                OnLeftHandTimerUpdate?.Invoke(Mathf.Clamp01(m_timer / maxTimer));
                            }
                        }
                        else if (playerSettings.TeleportHandMovementTriggerDistance > 0) // Use distance based trigger
                        {
                            var targetAnchor = m_useRightHand ? m_rightAnchor : m_leftAnchor;
                            var distance = Vector3.Distance(targetAnchor.position, m_recordedHandLocalPosition);
                            if (distance >= playerSettings.TeleportHandMovementTriggerDistance)
                            {
                                StartWarp();
                            }
                        }
                        else if (playerSettings.TeleportHandMovementTriggerDistance == 0)
                        {
                            StartWarp();
                        }

                        break;
                    }
                case State.warping:
                    {
                        m_warpTimer += Time.deltaTime;

                        if (m_warpTimer > m_warpDelay)
                        {
                            DoWarp();
                            m_teleportSound.Play();
                            m_warpTimer = m_warpDelay + LastTeleport.ExtraScreenFadeTime;
                        }
                        m_screenFader.TeleportFadeValue = m_warpTimer / m_warpDelay;
                        break;
                    }
                case State.recovery:
                    {
                        m_warpTimer -= Time.deltaTime;

                        if (m_warpTimer < 0)
                        {
                            ResetState();
                            m_warpTimer = 0;
                        }
                        m_screenFader.TeleportFadeValue = Mathf.Clamp01(m_warpTimer / m_warpDelay);
                        break;
                    }
            }
        }
    }
}
