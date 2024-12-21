// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace NorthStar
{
    /// <summary>
    /// NPC rig controller that manages look-at and eye movement behaviours
    /// </summary>
    public class NpcRigController : MonoBehaviour
    {
        [Header("Constraint Parents")]
        [Tooltip("The head and spine parent rig")]
        [SerializeField] private Rig m_spineLookRig;
        [SerializeField] private bool m_spineEnabledOnStart;
        [SerializeField] private Rig m_headLookRig;
        [SerializeField] private bool m_headEnabledOnStart;
        [SerializeField] private Rig m_eyeLookRig;
        [SerializeField] private bool m_eyeEnabledOnStart;

        [Space(5)]
        [SerializeField] private Rig m_leftArmRig;
        [SerializeField] private Rig m_rightArmRig;

        [Space(5)]
        [SerializeField] private Rig m_leftLegRig;
        [SerializeField] private Rig m_rightLegRig;

        [Header("'Look At' settings")]
        [Tooltip("Target objects to look at. Used to calculate distance and angle from Target")]
        [SerializeField] private Transform m_targetTransform;

        [Tooltip("How fast to blend the spine into the look at constraint")]
        [SerializeField] private float m_spineLookLerpSpeed = 5;

        [Tooltip("How fast to blend the arms into the look at constraint")]
        [SerializeField] private float m_armLookLerpSpeed = 5;

        [Tooltip("How fast to blend the head and spine into the look at constraint")]
        [SerializeField] private float m_headLookLerpSpeed = 5;

        [Tooltip("How fast to blend the eyes into the look at constraint")]
        [SerializeField] private float m_eyeLookLerpSpeed = 100; // Use a faster speed than the head to have the eyes lead the target 

        [SerializeField] private float m_trackTargetLerpSpeed = 5;


        [Tooltip("Reduce influence by this factor (Multiply next link in chain by this amount)")]
        private float m_chainInfluenceFactor = 0.5f;

        #region Eye Wander
        [Header("Eye wander")]
        [SerializeField, Tooltip("Whether the eyes should wander periodically")]
        private bool m_eyeWander = true;
        //[SerializeField, Tooltip("The bones that directly control the eye position and rotation")]
        //private Transform[] m_eyeBones;
        [SerializeField, Tooltip("The bones that parents the eyes")]
        private Transform m_headBone;
        [SerializeField, Tooltip("X represents the minimum time the NPC will look at the target, while Y represents the maximum time")]
        private Vector2 m_lookAtDurationRange;
        [SerializeField, Tooltip("X represents the minimum time the NPC's eyes will wander away from the target, while Y represents the maximum time")]
        private Vector2 m_lookAwayDurationRange;

        [SerializeField, Range(0, 1), Tooltip("How likely is it that the NPC will look away from the player a second time instead of returning their gaze.")]
        private float m_lookAwayRepeatChance = 0.2f;
        [SerializeField, Range(0, 1), Tooltip("How likely is it that the NPC will blink when they eyes change target.")]
        private float m_lookBlinkChance = 0.9f;

        [SerializeField, Tooltip("The eyes will track this transform")]
        private Transform m_lookTransform;
        private float m_lookCountdown;
        private bool m_isLookingAtTarget; //Is the eye looking at the player or are their eyes darting away

        private Vector3 m_targetEyePos;
        private Vector3 m_previousEyePos;

        private float m_eyeLerp;
        private float m_eyeDistanceCompensation;
        #endregion

        [SerializeField, Space(10)]
        private BlendshapeManager m_blendshapeManager;

        [System.Flags]
        public enum IKRig
        {
            None = 0, // Custom name for "Nothing" option
            Spine = 1 << 0,
            Arm = 1 << 1,
            Head = 1 << 2,
            UpperBody = Spine | Head, // Custom combined name
            Eye = 1 << 3,
            //All = ~0, // Custom name for "Everything" option
        }

        private float m_currentSpineLookWeight = 0; // The current weight of the look at constraint - lerps towards target weight
        private float m_targetSpineLookWeight = 0; // The goal weight of the look at constraint
        private float m_spineLookCap = 1; //Whether spine look-at is enabled, controlled through timelines

        private float m_currentArmLookWeight = 0; // The current weight of the look at constraint - lerps towards target weight
        private float m_targetArmLookWeight = 0; // The goal weight of the look at constraint
        private float m_armLookCap = 1; //Whether arm look-at is enabled, controlled through timelines

        private float m_currentHeadLookWeight = 0; // The current weight of the look at constraint - lerps towards target weight
        private float m_targetHeadLookWeight = 0; // The goal weight of the look at constraint
        private float m_headLookCap = 1; //Whether head look-at is enabled, controlled through timelines

        private float m_currentEyeLookWeight = 0; // The current weight of the look at constraint - lerps towards target weight
        private float m_targetEyeLookWeight = 0; // The goal weight of the look at constraint
        private float m_eyeLookCap = 1; //The max value that the weight can go, used by timelines to control strength

        private Transform m_prevLookAtTarget;
        private Transform m_lookAtTarget;
        private MultiAimConstraint[] m_headLookConstraints;
        private MultiAimConstraint[] m_eyeLookConstraints;

        private TwoBoneIKConstraint m_leftArmIk;
        private TwoBoneIKConstraint m_rightArmIk;
        private TwoBoneIKConstraint m_leftLegIk;
        private TwoBoneIKConstraint m_rightLeftIk;

        public Transform LookAtTarget
        {
            get => m_lookAtTarget;

            set
            {
                if (m_lookAtTarget != value)
                {
                    m_prevLookAtTarget = m_lookAtTarget;
                    m_lookAtTarget = value;

                    if (m_lookAtTarget != null)
                    {
                        m_targetSpineLookWeight = 1;
                        m_targetArmLookWeight = 1;
                        m_targetHeadLookWeight = 1;
                        m_targetEyeLookWeight = 1;
                    }
                    else
                    {
                        m_targetSpineLookWeight = 0;
                        m_targetArmLookWeight = 0;
                        m_targetHeadLookWeight = 0;
                        m_targetEyeLookWeight = 0;
                    }
                }
            }
        }

        private void Awake()
        {
            m_headLookConstraints = m_headLookRig.GetComponentsInChildren<MultiAimConstraint>();
            m_eyeLookConstraints = m_eyeLookRig.GetComponentsInChildren<MultiAimConstraint>();
        }

        private void Start()
        {
            //SetInitialHeadLookInfluences(); //Sets the weights of the chain of bones in the Head Look rig
            UpdateLookAtWeights(); //Set initial weight of the parent head and eye look rigs

            m_spineLookCap = m_spineEnabledOnStart ? 1 : 0;
            m_headLookCap = m_headEnabledOnStart ? 1 : 0;
            m_eyeLookCap = m_eyeEnabledOnStart ? 1 : 0;
        }

        private void Update()
        {
            UpdateLookAtWeights();

            if (m_eyeWander && m_lookTransform)
            {
                if (m_lookCountdown <= 0)
                {
                    m_isLookingAtTarget = !m_isLookingAtTarget;
                    m_previousEyePos = m_targetEyePos;

                    if (m_isLookingAtTarget && Random.value < m_lookAwayRepeatChance)
                    {
                        m_isLookingAtTarget = false; //Set the NPC to look away if they roll a random repeat
                    }

                    if (!m_isLookingAtTarget)
                    {
                        //We generate an elipsoid as we want to allow more horizontal eye movement than vertical, otherwise too much of eye's whites gets revealed
                        var pointInElipsoid = Random.insideUnitSphere; //TODO: investigate if there is a straightforward way to get a uniform distribution within an elipsoid
                        pointInElipsoid.y *= 0.5f;

                        m_targetEyePos = m_headBone.position + m_headBone.forward * 3 + pointInElipsoid;
                        m_lookCountdown = Random.Range(m_lookAwayDurationRange.x, m_lookAwayDurationRange.y);
                    }
                    else
                    {
                        m_targetEyePos = m_targetTransform.position;
                        m_lookCountdown = Random.Range(m_lookAtDurationRange.x, m_lookAtDurationRange.y);
                    }

                    m_eyeLerp = 0;
                    m_eyeDistanceCompensation = Vector3.Distance(m_targetEyePos, m_previousEyePos) * 0.1f; //TODO: calculate the angle the eye would have to travel and use that to get a constant speed

                    if (Random.value < m_lookBlinkChance) // 90% of wanders/resets, make the NPC blink
                    {
                        m_blendshapeManager.Blink(); //Feels a bit more natural if we make the NPC blink each time their eyes wander/reset
                    }
                }
                m_lookCountdown -= Time.deltaTime;
                m_eyeLerp += Time.deltaTime / m_eyeDistanceCompensation;

                if (m_isLookingAtTarget)
                {
                    m_targetEyePos = m_targetTransform.position;
                }

                m_lookTransform.position = Vector3.Lerp(m_previousEyePos, m_targetEyePos, m_eyeLerp);
            }
        }

        /// <summary>
        /// Set the look at strength
        /// </summary>
        private void UpdateLookAtWeights()
        {
            if (m_lookAtTarget != null)
                m_targetTransform.position = Vector3.Lerp(m_targetTransform.position, m_lookAtTarget.position, Time.deltaTime * m_trackTargetLerpSpeed);

            m_currentSpineLookWeight = Mathf.Lerp(m_currentSpineLookWeight, Mathf.Min(m_targetSpineLookWeight, m_spineLookCap), Time.deltaTime * m_spineLookLerpSpeed);
            m_spineLookRig.weight = m_currentSpineLookWeight;

            m_currentHeadLookWeight = Mathf.Lerp(m_currentHeadLookWeight, Mathf.Min(m_targetHeadLookWeight, m_headLookCap), Time.deltaTime * m_headLookLerpSpeed);
            m_headLookRig.weight = m_currentHeadLookWeight;

            m_currentEyeLookWeight = Mathf.Lerp(m_currentEyeLookWeight, Mathf.Min(m_targetEyeLookWeight, m_eyeLookCap), Time.deltaTime * m_eyeLookLerpSpeed);
            m_eyeLookRig.weight = m_currentEyeLookWeight;
        }

        public void SetWeightCap(float weight, IKRig rig)
        {
            if (rig.HasFlag(IKRig.Spine))
            {
                m_spineLookCap = weight;
            }
            if (rig.HasFlag(IKRig.Arm))
            {
                m_armLookCap = weight;
            }
            if (rig.HasFlag(IKRig.Head))
            {
                m_headLookCap = weight;
            }
            if (rig.HasFlag(IKRig.Eye))
            {
                m_eyeLookCap = weight;
            }
        }

        private void OnDrawGizmos()
        {
            if (m_targetTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(m_targetTransform.position, 0.15f);
            }
            if (m_lookAtTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(m_lookAtTarget.position, 0.15f);
            }
        }
    }
}
