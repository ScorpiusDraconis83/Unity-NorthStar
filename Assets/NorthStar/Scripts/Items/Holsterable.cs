// Copyright (c) Meta Platforms, Inc. and affiliates.
using Oculus.Interaction.HandGrab;
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// For the physically enabled item component that can be attached to a corrisponding <see cref="Holster"/> object
    /// </summary>
    public class Holsterable : MonoBehaviour
    {
        [SerializeField, HideInInspector] private PhysicsTransformer[] m_grabPoints;
        [SerializeField, HideInInspector] private Rigidbody[] m_bodies;
        [SerializeField, HideInInspector] private Vector3[] m_initialPosition;

        [SerializeField] private Transform m_holster;

        [Space(5)]
        public UnityEvent OnGrab;

        private float m_grabbedCount = 0;

        public bool IsGrabbed => m_grabbedCount > 0;
        [HideInInspector]
        public bool IsHolstered;

        protected virtual void Start()
        {
            for (var i = 0; i < m_grabPoints.Length; i++)
            {
                m_grabPoints[i].OnInteraction += Grabbed;
                m_grabPoints[i].OnEndInteraction += Released;
            }

            m_initialPosition = new Vector3[m_bodies.Length];
            for (var i = 0; i < m_bodies.Length; i++)
            {
                m_initialPosition[i] = m_holster.InverseTransformPoint(m_bodies[i].transform.position);
            }
        }

        protected virtual void Grabbed(HandGrabInteractor interactor)
        {
            m_grabbedCount++;

            if (m_grabbedCount > 2)
            {
                Debug.LogError("More than two grab count should be impossible!", gameObject);
            }

            OnGrab.Invoke();
        }
        protected virtual void Released(HandGrabInteractor interactor)
        {
            m_grabbedCount--;

            if (m_grabbedCount < 0)
            {
                Debug.LogError("Negative grab count should be impossible!", gameObject);
            }
        }

        /// <summary>
        /// Freeze all the rigidbodies and return them to their starting position relative to their assigned holster
        /// </summary>
        public void Freeze()
        {
            for (var i = 0; i < m_bodies.Length; i++)
            {
                m_bodies[i].isKinematic = true;
                m_bodies[i].transform.position = m_holster.TransformPoint(m_initialPosition[i]);
            }
        }

        public void Unfreeze()
        {
            for (var i = 0; i < m_bodies.Length; i++)
            {
                m_bodies[i].isKinematic = false;
            }
        }

        protected virtual void OnValidate()
        {
            m_grabPoints = transform.GetComponentsInChildren<PhysicsTransformer>();
            m_bodies = transform.GetComponentsInChildren<Rigidbody>();
        }
    }
}
