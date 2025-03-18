// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    public class Island : MonoBehaviour
    {
        [SerializeField] private float m_minDistance = 100, m_maxDistance = 1000f;
        [SerializeField] private float m_range = 10;
        [SerializeField] protected float m_spawnRange = 100;
        [SerializeField] private float m_minHeight = -100, m_maxHeight = 0;

        private void Start()
        {
            Reposition();
        }

        private void Update()
        {
            var pos = transform.position;
            pos.y = 0;
            var boatPos = BoatController.Instance.MovementSource.CurrentPosition;
            boatPos.y = 0;
            var distance = Vector3.Distance(pos, boatPos);
            pos.y = distance <= m_minDistance ? distance.ClampedMap(m_minDistance - m_range, m_minDistance, m_minHeight, m_maxHeight) : distance.ClampedMap(m_maxDistance, m_maxDistance + m_range, m_maxHeight, m_minHeight);
            transform.position = pos;

            var headingVector = Quaternion.Euler(0, BoatController.Instance.HeadingAngle, 0) * Vector3.forward;
            var toBoat = boatPos - pos;

            transform.rotation = Quaternion.LookRotation(toBoat, Vector3.up);

            if (distance > m_maxDistance + m_range && (Vector3.Dot(headingVector, -toBoat) < 0))
            {
                Reposition();
            }
        }

        private void Reposition()
        {
            var randDir = Random.onUnitSphere;
            randDir.y = 0;
            randDir.Normalize();
            var headingVector = Quaternion.Euler(0, BoatController.Instance.HeadingAngle, 0) * Vector3.forward;
            if (Vector3.Dot(headingVector, randDir) < 0)
                randDir = -randDir;

            var pos = BoatController.Instance.MovementSource.CurrentPosition + randDir * (m_maxDistance + m_range + Random.value * m_spawnRange);
            transform.position = pos;
        }
    }
}
