// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// basic script that just makes a Gameobject adopt the same position and rotation of another transform
    /// </summary>
    public class FollowObject : MonoBehaviour
    {
        public Transform FollowTarget;

        private bool m_followActive = true;

        private void Update()
        {
            if (m_followActive)
            {
                transform.position = FollowTarget.position;
                transform.rotation = FollowTarget.rotation;
            }
        }
    }
}
