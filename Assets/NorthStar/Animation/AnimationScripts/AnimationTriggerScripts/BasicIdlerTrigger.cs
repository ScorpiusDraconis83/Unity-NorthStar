// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class BasicIdlerTrigger : MonoBehaviour
{
    [Header("Plays Idle animation, until triggered")]
    [Header("then plays a one off animation before transitioning back into idle")]
    [Space(25)]
    //DEBUG
    public bool showDebug = false;
    public TextMesh debugText;

    //

    private enum AnimState { Idling, Active }

    private AnimState m_animState = AnimState.Idling;
    private NPC_IKController m_nPCController;
    [Tooltip("Use distance from NPC to trigger")]
    public bool triggerByDistance = true;
    public float triggerDistance = 5f;
    [Tooltip("Use cone of vision angle from NPC to trigger")]
    public bool triggerByAngle = true;
    public float triggerAngle = 60f;


    [Tooltip("How many times can the animation be triggered? Negative numbers means no limit")]
    public int triggercount = 1;
    private bool isTriggerValid = true;

    [Tooltip("Does the trigger animation reset to idle when you leave the trigger area")]
    public bool useExitTriggerToReset = true;

    [Tooltip("Does the trigger animation have a cool down before you can trigger it again? Negative number = no cooldown")]
    public float activeTriggerCooldown = 15;
    private float activeCooldownCounter = 0;

    private void Start()
    {
        m_nPCController = GetComponent<NPC_IKController>();
    }

    private void SwitchAnimState(AnimState nextState)
    {
        if (showDebug) Debug.Log("Entered state: " + nextState + " from: " + m_animState);

        switch (nextState)
        {
            case AnimState.Idling:
                GetComponent<Animator>().SetTrigger("TriggerIdling");
                GetComponent<Animator>().ResetTrigger("TriggerActiveClip");
                break;

            case AnimState.Active:
                activeCooldownCounter = 0;
                triggercount--;
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
                if (activeTriggerCooldown > 0) //if using timer to return to start, then count down
                    ActiveCounter(); //if using cool down counter
                break;
        }

        if (isTriggerValid)
            CheckForTrigger();
        else
            CheckForTriggerReset(); //Check to see if you have exited the trigger area before trying to trigger the animation again
    }

    private void ActiveCounter()
    {
        if (activeCooldownCounter < activeTriggerCooldown)
            activeCooldownCounter += Time.deltaTime;
        else
        {
            SwitchAnimState(AnimState.Idling);
        }
    }

    private void CheckForTrigger()
    {
        if (triggercount > 0) //if can trigger the animation 
        {
            if (m_nPCController.IsWithinDistance(triggerDistance) && m_nPCController.IsWithinAngle(triggerAngle)) //if within trigger
            {
                if (showDebug) Debug.Log("Trigger zone entered");

                isTriggerValid = false; //Set this false to prevent animation triggering immediately after ending
                SwitchAnimState(AnimState.Active);
            }
        }
    }

    private void CheckForTriggerReset()
    {
        if (!m_nPCController.IsWithinDistance(triggerDistance) || !m_nPCController.IsWithinAngle(triggerAngle)) //if not in trigger zone
        {
            if (showDebug) Debug.Log("Trigger Zone Exited");
            if (useExitTriggerToReset)
            {
                isTriggerValid = true;
                SwitchAnimState(AnimState.Idling);
            }
        }
    }

    public void TransitionEnded() //Called by the Transition state in the Animation State machine
    {
        if (showDebug) Debug.Log("State Machine : ActiveEnded");
        SwitchAnimState(AnimState.Idling);
    }
}
