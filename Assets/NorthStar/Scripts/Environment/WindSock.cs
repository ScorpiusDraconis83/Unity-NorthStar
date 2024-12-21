// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controller for the wind socks seen on the boat. Sets the angle and cloth forces based on the current wind settings
    /// </summary>
    public class WindSock : MonoBehaviour
    {
        [SerializeField] private float m_windMultiplier = 1;
        [SerializeField] private float m_windTurbulence = 5;
        [SerializeField] private Transform m_hinge;

        private Cloth m_sockCloth;

        private void Awake()
        {
            m_sockCloth = GetComponentInChildren<Cloth>();
        }

        private void Update()
        {
            if (EnvironmentSystem.Instance is null) return;
            if (BoatController.Instance is null) return;

            // Use the built-in acceleration options in the cloth component to simulate wind effects
            m_sockCloth.externalAcceleration = EnvironmentSystem.Instance.WindVector * m_windMultiplier;
            m_sockCloth.randomAcceleration = EnvironmentSystem.Instance.WindVector * m_windTurbulence;

            var windDir = Quaternion.Inverse(BoatController.Instance.MovementSource.CurrentRotation) * EnvironmentSystem.Instance.WindVector;
            windDir.y = 0;
            var windSpeed = windDir.magnitude;

            if (windSpeed > 0.01f)
            {
                m_hinge.eulerAngles = new Vector3(0, Vector3.SignedAngle(-Vector3.forward, windDir / windSpeed, Vector3.up), 0);
            }
        }
    }
}
