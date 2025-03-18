// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Calls a unity event when something in the provided layer collides with it
    /// </summary>
    public class CollisionTrigger : MonoBehaviour
    {
        public UnityEvent OnCollisionEnterEvents;
        public bool ReadyToFire = false;
        public bool FireOnceOnly = true;
        public LayerMask LayersToFireEvent;

        public void SetReadyToFire(bool ready)
        {
            ReadyToFire = ready;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!ReadyToFire)
            {
                return;
            }
            //check if the colliding object is in the LayerMask
            if (LayersToFireEvent == (LayersToFireEvent | (1 << collision.gameObject.layer)))
            {
                OnCollisionEnterEvents.Invoke();
                if (FireOnceOnly)
                {
                    ReadyToFire = false;
                }
            }
        }
    }
}
