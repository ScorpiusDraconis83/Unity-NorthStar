// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections;
using UnityEngine;

public class NPC_Animator : MonoBehaviour
{
    private Animator animator;
    private bool hasBumped = false;
    private float bumpCooldown = 1f;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private IEnumerator BumpCooldownCoroutine()
    {
        yield return new WaitForSeconds(bumpCooldown);
        hasBumped = false;
    }

    public void TryBump(Vector3 bumpPosition) //Call this with the player position to use the blended directional bump animation
    {
        Debug.Log("Bumping disabled");
        /*
        if(!hasBumped)
        {
            hasBumped = true;
            StartCoroutine(BumpCooldownCoroutine());
            Vector2 relativePos = new Vector2(transform.position.x - bumpPosition.x, transform.position.z-bumpPosition.z).normalized;
            animator.SetFloat("bumpX", relativePos.x);
            animator.SetFloat("bumpY", relativePos.y);
            animator.SetTrigger("bumpTrigger");
        }
        */
    }
}
