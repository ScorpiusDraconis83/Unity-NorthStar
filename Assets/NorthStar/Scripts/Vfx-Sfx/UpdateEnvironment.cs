// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Simple utility class to update environment lighting on demand
    /// </summary>
    public class UpdateEnvironment : MonoBehaviour
    {
        //Just a basic little script to ask Unity to queue an environment update.
        //If too many things are trying to do this at once this request might get ignored
        [ContextMenu("DynamicGI.UpdateEnvironment")]
        public void DoEnvironmentUpdate()
        {
            DynamicGI.UpdateEnvironment();
        }

        public void DoEnvironmentUpdateAfterDelay(float delay)
        {
            _ = StartCoroutine(DoEnvironmentUpdateAfterDelayCoroutine(delay));
        }

        private IEnumerator DoEnvironmentUpdateAfterDelayCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            DoEnvironmentUpdate();
        }

    }
}
