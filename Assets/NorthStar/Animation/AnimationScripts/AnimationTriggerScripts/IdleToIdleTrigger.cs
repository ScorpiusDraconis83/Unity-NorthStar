// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class IdleToIdleTrigger : MonoBehaviour
{
    [Header("Plays Idle animation, until triggered")]
    [Header("then optionally plays a transition animation")]
    [Header("into a new idle animation. Optionally transition back to original idle after time")]
    [Space(25)]
    //DEBUG
    public bool showDebug = false;
    public TextMesh debugText;

    //---------

    //Angle and Distance checking variables
    private NPC_IKController NPCController;

    private enum AnimState { InactiveIdle, Transitioning, TransitionEnded, ActiveIdle }

    private AnimState animState = AnimState.InactiveIdle;

    [Tooltip("Use distance from NPC to trigger")]
    public float triggerDistance = 5f;
    [Tooltip("Use cone of vision angle from NPC to trigger")]
    public float triggerAngle = 60f;

    [Tooltip("Does this use a transition animation between the two Idles")]
    public bool useTransitionAnimation = true;

    [Tooltip("Does the active animation have a time limit before it returns to idle? Negative is never returns")]
    public float activeIdlePlayTime = 15;
    private float activeIdleCounter = 0;

    [Tooltip("Does the active animation reset to idle when you leave the trigger area")]
    public bool useExitTriggerToReset = true;


    [Tooltip("Can the animation be triggered more than once?")]
    public bool canTriggerMultiple = false;
    private bool isTriggerValid = true;

    private void Start()
    {
        NPCController = GetComponent<NPC_IKController>();

        if (debugText != null)
        {
            debugText.gameObject.SetActive(showDebug);
        }
    }

    private void SwitchAnimState(AnimState nextState)
    {
        if (showDebug) Debug.Log("Entered state: " + nextState + " from: " + animState);

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
                activeIdleCounter = 0;
                break;
        }
        animState = nextState;
    }

    private void Update()
    {
        switch (animState)
        {
            case AnimState.InactiveIdle:
                break;

            case AnimState.Transitioning:
                break;

            case AnimState.ActiveIdle:
                if (activeIdlePlayTime > 0) //if using timer to return to start, then count down
                    ActiveIdleCounter();
                break;
        }

        if (isTriggerValid)
            CheckForTrigger();
        else if (!isTriggerValid && canTriggerMultiple)
            CheckForTriggerReset(); //Check to see if you have exited the trigger area before trying to trigger the animation again

        //Debug---
        if (showDebug && debugText != null)
        {
            var debugIsInRange = NPCController.IsWithinDistance(triggerDistance) && NPCController.IsWithinAngle(triggerAngle);
            debugText.text = animState.ToString() + "\n" + activeIdleCounter.ToString(activeIdleCounter % 1 == 0 ? "0" : "0.0" + "/" + activeIdlePlayTime + "\n Is in Range: " + debugIsInRange);
        }
        //--------
    }

    private void CheckForTrigger()
    {
        if (NPCController.IsWithinDistance(triggerDistance) && NPCController.IsWithinAngle(triggerAngle)) //if within trigger
        {
            if (showDebug) Debug.Log("Trigger zone entered");

            isTriggerValid = false; //Set this false to prevent animation triggering immediately after ending
            if (useTransitionAnimation) //trigger the transition animation
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
        if (!NPCController.IsWithinDistance(triggerDistance) || !NPCController.IsWithinAngle(triggerAngle)) //if not in trigger zone
        {
            if (showDebug) Debug.Log("Trigger Zone Exited");
            if (useExitTriggerToReset)
            {
                isTriggerValid = true;
                SwitchAnimState(AnimState.InactiveIdle);
            }
        }
    }

    private void ActiveIdleCounter()
    {
        if (activeIdleCounter < activeIdlePlayTime)
            activeIdleCounter += Time.deltaTime;
        else
        {
            SwitchAnimState(AnimState.InactiveIdle);
        }
    }

    public void TransitionEnded() //Called by the Transition state in the Animation State machine
    {
        if (showDebug) Debug.Log("State Machine : TransitionStateEnded");
        SwitchAnimState(AnimState.ActiveIdle);
    }
}
