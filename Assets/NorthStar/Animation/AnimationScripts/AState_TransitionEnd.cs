// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class AnimationStateTriggers : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SendMessage("TransitionEnded");
    }
}

