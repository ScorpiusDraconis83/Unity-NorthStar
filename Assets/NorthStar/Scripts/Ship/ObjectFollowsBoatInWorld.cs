// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Moves an object alongside the boat without it being a child
    /// </summary>
    public class ObjectFollowsBoatInWorld : MonoBehaviour
    {
        private Vector3 m_position;
        private Quaternion m_rotation;

        [SerializeField] private bool m_useRotation = true;

        private void Start()
        {
            m_position = BoatController.Instance.transform.InverseTransformPoint(transform.position);
            m_rotation = Quaternion.Inverse(BoatController.Instance.transform.rotation) * transform.rotation;
            transform.parent = null;
        }

        private void OnEnable()
        {
            BoatController.Instance.MovementSource.OnSync += OnSync;
        }

        private void OnDisable()
        {
            BoatController.Instance.MovementSource.OnSync -= OnSync;
        }

        private void Update()
        {
            if (BoatController.Instance is null)
                return;
            transform.position = BoatController.WorldToBoatSpace(BoatController.Instance.transform.TransformPoint(m_position));
            if (m_useRotation)
                transform.rotation = BoatController.WorldToBoatSpace(BoatController.Instance.transform.rotation * m_rotation);
        }

        private void OnSync()
        {
            transform.position = BoatController.Instance.transform.TransformPoint(m_position);
            if (m_useRotation)
                transform.rotation = BoatController.Instance.transform.rotation * m_rotation;
        }
    }
}
