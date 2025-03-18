// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NPC_IKController : MonoBehaviour
{
    //DEBUG STUFF---------------------------
    public bool ShowDebug = true;
    private float[] m_targetAngles; //Store for Debug
    public Renderer RendMesh;
    //---------------------------------------

    [Header("Constraint Parents")]
    [Tooltip("The head and spine parent rig")]
    public Transform HeadLookRig;
    public Transform EyeLookRig;
    //public GameObject ArmIKParent; //TODO - Need to do something with this. Currently IK Rig is set up but weight is set to 0.

    [Header("'Look At' settings")]
    [Tooltip("Target objects to look at. Used to calculate distance and angle from Target")]
    public Transform[] LookAtTargets;
    private int m_currentTargetIndex = 0; //Used to set the current target to focus on
    public float HeadLookBaseWeight = 0.9f;

    [Tooltip("How fast to blend the head and spine into the look at constraint")]
    public float HeadLookLerpSpeed = 5;
    private float m_currentHeadLookWeight = 0; //the current weight of the look at constraint - lerps towards target weight
    private float m_targetHeadLookWeight = 0; //the goal weight of the look at constraint

    [Tooltip("How fast to blend the eyes into the look at constraint")]
    public float EyeLookLerpSpeed = 20; //Use a faster speed than the head to have the eyes lead the target 
    private float m_currentEyeLookWeight = 0; //the current weight of the look at constraint - lerps towards target weight
    private float m_targetEyeLookWeight = 0; //the goal weight of the look at constraint

    [Tooltip("Distance to start looking at target, -1 for always look")]
    public float TriggerDistance = 10f;

    [Tooltip("Cone angle target must be in to start looking at target")]
    public float ConeOfVisionAngle = 80f;

    [Tooltip("Reduce influence by this factor (Multiply next link in chain by this amount)")]
    public float ChainInfluenceFactor = 0.5f;

    private void Start()
    {
        if (ShowDebug)
            m_targetAngles = new float[LookAtTargets.Length];

        SetInitialHeadLookInfluences(); //Sets the weights of the chain of bones in the Head Look rig
        UpdateLookAtWeights(); //Set initial weight of the parent head and eye look rigs
    }

    private void Update()
    {
        if (ShowDebug)
        {
            for (var i = 0; i < m_targetAngles.Length; i++)
            {
                m_targetAngles[i] = GetAngleFromTarget(LookAtTargets[i]);
            }
        }

        CheckTargets(); //Check to see if close enough to target and in cone of vision
        if (LookAtTargets[m_currentTargetIndex] != null && m_currentHeadLookWeight != m_targetHeadLookWeight) //if target weight has changed, then lerp towards the new target
            UpdateLookAtWeights();
    }

    //Head Look At
    private void SetInitialHeadLookInfluences() //Set how far each bone in the chain turns to look at the player based on the Head look weight value
    {
        HeadLookRig.GetChild(HeadLookRig.childCount - 1).GetComponent<MultiAimConstraint>().weight = HeadLookBaseWeight;

        for (var i = 0; i < HeadLookRig.childCount - 1; i++)
        {
            var constraint = HeadLookRig.GetChild(i).GetComponent<MultiAimConstraint>();
            constraint.weight = HeadLookBaseWeight * (ChainInfluenceFactor / (HeadLookRig.childCount - 1 - i)); //reduce weights by factor
        }

        for (var i = 0; i < EyeLookRig.childCount; i++)
        {
            var constraint = EyeLookRig.GetChild(i).GetComponent<MultiAimConstraint>();
            constraint.weight = 1;
        }
    }

    private void SwitchLookAtTarget(int newTargetIndex)
    {
        m_currentTargetIndex = newTargetIndex;
        for (var i = 0; i < HeadLookRig.childCount; i++)
        {
            var constraint = HeadLookRig.GetChild(i).GetComponent<MultiAimConstraint>();
            var sources = constraint.data.sourceObjects;
            print(sources.Count);

            //TODO - find out how to access the number of sources in the constraint. Turn off all targets weight, then add new target weight.
            sources.SetWeight(0, 0);
            sources.SetWeight(1, 0);
            sources.SetWeight(newTargetIndex, 1);
            constraint.data.sourceObjects = sources;

            //TODO  - Set this by passing in properties from the target later
            TriggerDistance = -1;
        }

        for (var i = 0; i < EyeLookRig.childCount; i++)
        {
            var constraint = EyeLookRig.GetChild(i).GetComponent<MultiAimConstraint>();
            var sources = constraint.data.sourceObjects;
            print(sources.Count);

            //TODO - find out how to access the number of sources in the constraint. Turn off all targets weight, then add new target weight.
            sources.SetWeight(0, 0);
            sources.SetWeight(1, 0);
            sources.SetWeight(newTargetIndex, 1);
            constraint.data.sourceObjects = sources;

            //TODO  - Set this by passing in properties from the target later
            TriggerDistance = -1;
        }
    }

    private void CheckTargets()
    {
        var currentTarget = LookAtTargets[m_currentTargetIndex];
        if (GetAngleFromTarget(currentTarget) < ConeOfVisionAngle && IsTargetInRange(currentTarget))
        {
            m_targetHeadLookWeight = 1;
            m_targetEyeLookWeight = 1;
        }
        else
        {
            m_targetHeadLookWeight = 0;
            m_targetEyeLookWeight = 0;
        }

        if (ShowDebug)
        {
            RendMesh.material.color = GetAngleFromTarget(currentTarget) < ConeOfVisionAngle && IsTargetInRange(currentTarget)
                ? Color.green
                : GetAngleFromTarget(currentTarget) < ConeOfVisionAngle && !IsTargetInRange(currentTarget)
                ? Color.yellow
                : GetAngleFromTarget(currentTarget) >= ConeOfVisionAngle && IsTargetInRange(currentTarget) ? Color.magenta : Color.white;
        }
    }

    private void UpdateLookAtWeights() //set the look at strength
    {
        m_currentHeadLookWeight = Mathf.Lerp(m_currentHeadLookWeight, m_targetHeadLookWeight, Time.deltaTime * HeadLookLerpSpeed);
        HeadLookRig.GetComponent<Rig>().weight = m_currentHeadLookWeight;

        m_currentEyeLookWeight = Mathf.Lerp(m_currentEyeLookWeight, m_targetEyeLookWeight, Time.deltaTime * EyeLookLerpSpeed);
        EyeLookRig.GetComponent<Rig>().weight = m_currentEyeLookWeight;
    }

    private bool IsTargetInRange(Transform target) //Return if target is close enough and in the cone of vision
    {
        return TriggerDistance < 0 || Vector3.Distance(target.position, transform.position) < TriggerDistance;
    }

    private float GetAngleFromTarget(Transform target) //Get angle between the NPC forward and the targets position in degrees
    {
        var targetDirection = target.position - transform.position;
        var angle = Vector3.Angle(targetDirection, transform.forward);

        //Debug
        if (ShowDebug)
        {
            for (var i = 0; i < LookAtTargets.Length; i++)
            {
                var angleLineOffset = new Vector3(0, 0, 1);
                var angleLineColor = Color.red;
                if (m_targetAngles[i] < ConeOfVisionAngle) angleLineColor = Color.green;
                Debug.DrawLine(transform.position + angleLineOffset, LookAtTargets[i].position, angleLineColor);
            }
        }
        return angle;
    }

    //Use to check if NPC is within a certain distance from the player
    public bool IsWithinDistance(float dist)
    {
        var currentTarget = LookAtTargets[m_currentTargetIndex];
        return Vector3.Distance(currentTarget.position, transform.position) < dist;
    }
    //Use to check if player is within a certain angle from the NPC's forward vector
    public bool IsWithinAngle(float targetAngle)
    {
        var currentTarget = LookAtTargets[m_currentTargetIndex];
        var targetDirection = currentTarget.position - transform.position;
        var angle = Vector3.Angle(targetDirection, transform.forward);
        return angle < targetAngle;
    }

    //DEBUG Stuff
    private void OnDrawGizmos()
    {
        if (ShowDebug)
        {
            for (var i = 0; i < LookAtTargets.Length; i++)
            {
                if (LookAtTargets[i] != null)
                {
                    var influenceLineOffset = new Vector3(0, 0, 1.5f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position + influenceLineOffset, transform.forward * TriggerDistance + influenceLineOffset);

                    Gizmos.color = new Color(1, 1, 1, 0.1f);
                    if (IsTargetInRange(LookAtTargets[i]))
                        Gizmos.color = new Color(0, 1, 0, 0.1f);
                    Gizmos.DrawSphere(transform.position, TriggerDistance);
                }
            }
        }
    }
}
