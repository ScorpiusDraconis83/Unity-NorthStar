// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class IdleToIdleTrigger : MonoBehaviour
{
    [Header("Plays Idle animation, until triggered")]
    [Header("then optionally plays a transition animation")]
    [Header("into a new idle animation. Optionally transition back to original idle after time")]
    [Space(25)]
    //DEBUG
    public bool ShowDebug = false;
    public TextMesh DebugText;

    //---------

    //Angle and Distance checking variables
    [System.NonSerialized] public NPC_IKController NPCController;

    private enum AnimState { InactiveIdle, Transitioning, TransitionEnded, ActiveIdle }

    private AnimState m_animState = AnimState.InactiveIdle;

    [Tooltip("Use distance from NPC to trigger")]
    public float TriggerDistance = 5f;
    [Tooltip("Use cone of vision angle from NPC to trigger")]
    public float TriggerAngle = 60f;

    [Tooltip("Does this use a transition animation between the two Idles")]
    public bool UseTransitionAnimation = true;

    [Tooltip("Does the active animation have a time limit before it returns to idle? Negative is never returns")]
    public float ActiveIdlePlayTime = 15;
    private float m_activeIdleCounter = 0;

    [Tooltip("Does the active animation reset to idle when you leave the trigger area")]
    public bool UseExitTriggerToReset = true;


    [Tooltip("Can the animation be triggered more than once?")]
    public bool CanTriggerMultiple = false;
    private bool m_isTriggerValid = true;

    private void Start()
    {
        NPCController = GetComponent<NPC_IKController>();

        if (DebugText != null)
        {
            DebugText.gameObject.SetActive(ShowDebug);
        }
    }

    private void SwitchAnimState(AnimState nextState)
    {
        if (ShowDebug) Debug.Log("Entered state: " + nextState + " from: " + m_animState);

        switch (nextState)
        {
            case AnimState.InactiveIdle:
                GetComponent<Animator>().ResetTrigger("SetActiveIdle");
                GetComponent<Animator>().ResetTrigger("TriggerTransition");
                GetComponent<Animator>().SetTrigger("TriggerInactiveIdle");
                break;

            case AnimState.Transitioning:
                GetComponent<Animator>().ResetTrigger("TriggerInactiveIdle");
                GetComponent<Animator>().SetTrigger("TriggerTransition");
                GetComponent<Animator>().ResetTrigger("SetActiveIdle");
                break;

            case AnimState.ActiveIdle:
                GetComponent<Animator>().ResetTrigger("TriggerInactiveIdle");
                GetComponent<Animator>().ResetTrigger("TriggerTransition");
                GetComponent<Animator>().SetTrigger("SetActiveIdle");
                m_activeIdleCounter = 0;
                break;
        }
        m_animState = nextState;
    }

    private void Update()
    {
        switch (m_animState)
        {
            case AnimState.InactiveIdle:
                break;

            case AnimState.Transitioning:
                break;

            case AnimState.ActiveIdle:
                if (ActiveIdlePlayTime > 0) //if using timer to return to start, then count down
                    ActiveIdleCounter();
                break;
        }

        if (m_isTriggerValid)
            CheckForTrigger();
        else if (!m_isTriggerValid && CanTriggerMultiple)
            CheckForTriggerReset(); //Check to see if you have exited the trigger area before trying to trigger the animation again

        //Debug---
        if (ShowDebug && DebugText != null)
        {
            var debugIsInRange = NPCController.IsWithinDistance(TriggerDistance) && NPCController.IsWithinAngle(TriggerAngle);
            DebugText.text = m_animState.ToString() + "\n" + m_activeIdleCounter.ToString(m_activeIdleCounter % 1 == 0 ? "0" : "0.0" + "/" + ActiveIdlePlayTime + "\n Is in Range: " + debugIsInRange);
        }
        //--------
    }

    private void CheckForTrigger()
    {
        if (NPCController.IsWithinDistance(TriggerDistance) && NPCController.IsWithinAngle(TriggerAngle)) //if within trigger
        {
            if (ShowDebug) Debug.Log("Trigger zone entered");

            m_isTriggerValid = false; //Set this false to prevent animation triggering immediately after ending
            if (UseTransitionAnimation) //trigger the transition animation
            {
                SwitchAnimState(AnimState.Transitioning);
            }
            else //else go straight to the active idle
            {
                SwitchAnimState(AnimState.ActiveIdle);
            }
        }
    }

    private void CheckForTriggerReset()
    {
        if (!NPCController.IsWithinDistance(TriggerDistance) || !NPCController.IsWithinAngle(TriggerAngle)) //if not in trigger zone
        {
            if (ShowDebug) Debug.Log("Trigger Zone Exited");
            if (UseExitTriggerToReset)
            {
                m_isTriggerValid = true;
                SwitchAnimState(AnimState.InactiveIdle);
            }
        }
    }

    private void ActiveIdleCounter()
    {
        if (m_activeIdleCounter < ActiveIdlePlayTime)
            m_activeIdleCounter += Time.deltaTime;
        else
        {
            SwitchAnimState(AnimState.InactiveIdle);
        }
    }

    public void TransitionEnded() //Called by the Transition state in the Animation State machine
    {
        if (ShowDebug) Debug.Log("State Machine : TransitionStateEnded");
        SwitchAnimState(AnimState.ActiveIdle);
    }
}
