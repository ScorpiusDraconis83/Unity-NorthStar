// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Tries to keep one point attached to the boat and one to the world
    /// </summary>
    public class Gangplank : MonoBehaviour
    {
        [SerializeField] private Transform m_otherEndPoint;
        [SerializeField, AutoSet] private FakeMovement m_movement;
        private Vector3 m_position;
        private Quaternion m_rotation;
        private void Awake()
        {
            m_position = transform.position;
            m_rotation = transform.rotation;
        }
        private void Update()
        {
            transform.position = m_position;
            transform.rotation = m_rotation;
            var point = BoatController.WorldToBoatSpace(transform.position);

            var toEndPoint = m_otherEndPoint.position - point;
            toEndPoint.Normalize();
            var up = Vector3.Cross(transform.forward, toEndPoint);
            m_movement.CurrentPosition = point;
            m_movement.CurrentRotation = Quaternion.LookRotation(transform.forward, -up);
        }
    }
}
