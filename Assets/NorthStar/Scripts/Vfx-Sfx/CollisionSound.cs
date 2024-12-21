// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Plays effects based on collision events. Collision types are mapped based on pairs of physics materials to keep things simple
    /// </summary>
    public class CollisionSound : MonoBehaviour
    {
        public UnityEvent OnPlaySound;
        private void OnCollisionEnter(Collision collision)
        {
            var contact = collision.GetContact(0);
            var effectAsset = CollisionMaterialPairs.GetEffect(contact.thisCollider.sharedMaterial, contact.otherCollider.sharedMaterial, out var curve);
            var intensity = curve.Evaluate(collision.relativeVelocity.magnitude);
            if (intensity > 0)
            {
                effectAsset.Play(contact.point, Quaternion.LookRotation(contact.normal), true, intensity);
                OnPlaySound.Invoke();
            }
        }
    }
}
