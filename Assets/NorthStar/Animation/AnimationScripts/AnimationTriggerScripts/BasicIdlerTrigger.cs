// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class BasicIdlerTrigger : MonoBehaviour
{
    [Header("Plays Idle animation, until triggered")]
    [Header("then plays a one off animation before transitioning back into idle")]
    [Space(25)]
    //DEBUG
    public bool ShowDebug;
    public TextMesh DebugText;

    //

    private enum AnimState { Idling, Active }

    private AnimState m_animState = AnimState.Idling;
    private NPC_IKController m_npcController;
    public float TriggerDistance = 5f;
    public float TriggerAngle = 60f;


    [Tooltip("How many times can the animation be triggered? Negative numbers means no limit")]
    public int TriggerCount = 1;
    private bool m_isTriggerValid = true;

    [Tooltip("Does the trigger animation reset to idle when you leave the trigger area")]
    public bool UseExitTriggerToReset = true;

    [Tooltip("Does the trigger animation have a cool down before you can trigger it again? Negative number = no cooldown")]
    public float ActiveTriggerCooldown = 15;
    private float m_activeCooldownCounter;

    private void Start()
    {
        m_npcController = GetComponent<NPC_IKController>();
    }

    private void SwitchAnimState(AnimState nextState)
    {
        if (ShowDebug) Debug.Log("Entered state: " + nextState + " from: " + m_animState);

        switch (nextState)
        {
            case AnimState.Idling:
                GetComponent<Animator>().SetTrigger("TriggerIdling");
                GetComponent<Animator>().ResetTrigger("TriggerActiveClip");
                break;

            case AnimState.Active:
                m_activeCooldownCounter = 0;
                TriggerCount--;
                GetComponent<Animator>().SetTrigger("TriggerActiveClip");
                GetComponent<Animator>().ResetTrigger("TriggerIdling");
                break;
        }
        m_animState = nextState;
    }

    private void Update()
    {
        switch (m_animState)
        {
            case AnimState.Idling:
                break;

            case AnimState.Active:
                if (ActiveTriggerCooldown > 0) //if using timer to return to start, then count down
                    ActiveCounter(); //if using cool down counter
                break;
        }

        if (m_isTriggerValid)
            CheckForTrigger();
        else
            CheckForTriggerReset(); //Check to see if you have exited the trigger area before trying to trigger the animation again
    }

    private void ActiveCounter()
    {
        if (m_activeCooldownCounter < ActiveTriggerCooldown)
            m_activeCooldownCounter += Time.deltaTime;
        else
        {
            SwitchAnimState(AnimState.Idling);
        }
    }

    private void CheckForTrigger()
    {
        if (TriggerCount > 0) //if can trigger the animation 
        {
            if (m_npcController.IsWithinDistance(TriggerDistance) && m_npcController.IsWithinAngle(TriggerAngle)) //if within trigger
            {
                if (ShowDebug) Debug.Log("Trigger zone entered");

                m_isTriggerValid = false; //Set this false to prevent animation triggering immediately after ending
                SwitchAnimState(AnimState.Active);
            }
        }
    }

    private void CheckForTriggerReset()
    {
        if (!m_npcController.IsWithinDistance(TriggerDistance) || !m_npcController.IsWithinAngle(TriggerAngle)) //if not in trigger zone
        {
            if (ShowDebug) Debug.Log("Trigger Zone Exited");
            if (UseExitTriggerToReset)
            {
                m_isTriggerValid = true;
                SwitchAnimState(AnimState.Idling);
            }
        }
    }

    public void TransitionEnded() //Called by the Transition state in the Animation State machine
    {
        if (ShowDebug) Debug.Log("State Machine : ActiveEnded");
        SwitchAnimState(AnimState.Idling);
    }
}
