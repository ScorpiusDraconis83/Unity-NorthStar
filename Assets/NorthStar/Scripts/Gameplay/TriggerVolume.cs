// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Calls a unity event when something enters its trigger
    /// </summary>
    public class TriggerVolume : MonoBehaviour
    {
        public GameObject triggeringObject;
        public UnityEvent onTriggerEnterEvents;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == triggeringObject)
            {
                Debug.Log("Object " + triggeringObject.name + " has entered trigger " + this.gameObject.name + " firing OnTriggerEvent events");
                onTriggerEnterEvents.Invoke();
            }
        }
    }
}
