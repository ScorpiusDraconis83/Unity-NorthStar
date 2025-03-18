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
        public GameObject TriggeringObject;
        public UnityEvent OnTriggerEnterEvents;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == TriggeringObject)
            {
                Debug.Log("Object " + TriggeringObject.name + " has entered trigger " + gameObject.name + " firing OnTriggerEvent events");
                OnTriggerEnterEvents.Invoke();
            }
        }
    }
}
