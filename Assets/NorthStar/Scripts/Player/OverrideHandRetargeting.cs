// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using Oculus.Interaction;
using Oculus.Movement.AnimationRigging;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Updates the retargeted hand positions to match the simulated physical hands
    /// </summary>
    [RequireComponent(typeof(OVRBody)), RequireComponent(typeof(RetargetingLayer))]
    public class OverrideHandRetargeting : MonoBehaviour
    {
        [SerializeField] private Transform m_geometry;
        [SerializeField] private Transform m_leftHand;
        [SerializeField] private Transform m_rightHand;

        [SerializeField] private Transform m_leftHandInactiveTarget, m_rightHandInactiveTarget;
        [SerializeField] private float m_inactiveHandLerpSpeed = 1.0f;

        [SerializeField, AutoSet] private RetargetingLayer m_layer;
        [SerializeField, AutoSet] private OVRBody m_body;
        private CustomRetargetingProcessorCorrectHand m_correctHandProcessor;
        private SyntheticHandRetargetingProcessor m_syntheticHandProcessor;

        private MaterialPropertyBlockEditor m_leftHandMaterialEditor;
        private MaterialPropertyBlockEditor m_rightHandMaterialEditor;

        private void Start()
        {
            m_leftHandMaterialEditor = m_leftHand.GetComponentInChildren<MaterialPropertyBlockEditor>();
            m_rightHandMaterialEditor = m_rightHand.GetComponentInChildren<MaterialPropertyBlockEditor>();

            // Search for a RetargetingProcessorCorrectHand
            foreach (var processor in m_layer.RetargetingProcessors)
            {
                if (processor is CustomRetargetingProcessorCorrectHand)
                {
                    m_correctHandProcessor = processor as CustomRetargetingProcessorCorrectHand;
                }
                else if (processor is SyntheticHandRetargetingProcessor)
                {
                    m_syntheticHandProcessor = processor as SyntheticHandRetargetingProcessor;
                    if (m_leftHand is not null) m_syntheticHandProcessor.LeftHandVisual = m_leftHand.GetComponentInChildren<HandVisual>();
                    if (m_rightHand is not null) m_syntheticHandProcessor.RightHandVisual = m_rightHand.GetComponentInChildren<HandVisual>();
                }
            }
        }

        private void SetHandOpacity(MaterialPropertyBlockEditor editor, float opacity, float outlineOpacity)
        {
            editor.FloatProperties.Clear();

            editor.FloatProperties.Add(new MaterialPropertyFloat
            {
                name = "_Opacity",
                value = opacity
            });

            editor.FloatProperties.Add(new MaterialPropertyFloat
            {
                name = "_OutlineOpacity",
                value = outlineOpacity
            });
        }

        private void Update()
        {
            if (BodyPositions.Instance.BodyTrackingActive)
            {
                if (m_leftHand is not null)
                {
                    if (OVRInput.IsControllerConnected(OVRInput.Controller.LHand) || OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
                    {
                        UpdateHandProcessor(m_correctHandProcessor.LeftHandProcessor, m_leftHand);
                        m_leftHandInactiveTarget.SetPositionAndRotation(m_leftHand.position, m_leftHand.rotation);
                    }
                    else
                    {
                        UpdateHandProcessor(m_correctHandProcessor.LeftHandProcessor, m_leftHandInactiveTarget, Time.deltaTime * m_inactiveHandLerpSpeed);
                    }
                }

                if (m_rightHand is not null)
                {
                    if (OVRInput.IsControllerConnected(OVRInput.Controller.RHand) || OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
                    {
                        UpdateHandProcessor(m_correctHandProcessor.RightHandProcessor, m_rightHand);
                        m_rightHandInactiveTarget.SetPositionAndRotation(m_rightHand.position, m_rightHand.rotation);
                    }
                    else
                    {
                        UpdateHandProcessor(m_correctHandProcessor.RightHandProcessor, m_rightHandInactiveTarget, Time.deltaTime * m_inactiveHandLerpSpeed);
                    }
                }

                m_geometry.gameObject.SetActive(true);

                m_leftHandMaterialEditor.Renderers[0].gameObject.SetActive(false);
                m_rightHandMaterialEditor.Renderers[0].gameObject.SetActive(false);
            }
            else
            {
                m_geometry.gameObject.SetActive(false); // Hide body if calibration/tracking has failed
                m_leftHandMaterialEditor.Renderers[0].gameObject.SetActive(true);
                m_rightHandMaterialEditor.Renderers[0].gameObject.SetActive(true);
                SetHandOpacity(m_leftHandMaterialEditor, 0.79f, 1.0f);
                SetHandOpacity(m_rightHandMaterialEditor, 0.79f, 1.0f);
            }
        }

        private void UpdateHandProcessor(CustomRetargetingProcessorCorrectHand.HandProcessor handProcessor, Transform target, float lerp = 1.0f)
        {
            handProcessor.UseCustomHandTargetPosition = true;
            handProcessor.UseCustomHandTargetRotation = true;
            handProcessor.UseWorldHandPosition = true;

            var currentTargetPosition = handProcessor.CustomHandTargetPosition ?? target.position;
            handProcessor.CustomHandTargetPosition = Vector3.Lerp(currentTargetPosition, target.position, lerp);

            var currentTargetRotation = handProcessor.CustomHandTargetRotation ?? target.rotation;
            handProcessor.CustomHandTargetRotation = Quaternion.Slerp(currentTargetRotation, target.rotation, lerp);
        }
    }
}
