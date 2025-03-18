// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using Meta.Utilities.Environment;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controls diving bell movement and behaviour
    /// </summary>
    public class DivingBell : MonoBehaviour
    {
        [SerializeField] private Transform m_door, m_windowAnchor;
        [SerializeField, AutoSet] private FakeMovement m_fakeMovement;
        [SerializeField] private BaseJointInteractable<float> m_interactable;
        [SerializeField] private float m_maxDistance;
        [SerializeField] private ReflectionProbe m_reflectionProbe;

        [SerializeField] private bool m_useFakeMovement;

        private bool m_entered = false;
        private Vector3 m_startPos = Vector3.zero;

        public void OnEnter()
        {
            if (m_entered)
                return;

            m_startPos = transform.position;
            if (m_useFakeMovement)
            {
                if (BoatController.Instance != null)
                {
                    BoatController.Instance.MovementSource.Sync();
                    BoatController.Instance.enabled = false;
                }
                m_fakeMovement.enabled = true;
                m_fakeMovement.Setup();
            }
            m_entered = true;

            m_door.localEulerAngles = new Vector3(-90, 0, 0);

            EnvironmentSystem.Instance.ToggleOceanPlaneClipping(true);
        }

        public void OnExit()
        {
            if (!m_entered)
                return;

            if (m_useFakeMovement)
            {
                if (BoatController.Instance != null)
                {
                    BoatController.Instance.enabled = true;
                }
                m_fakeMovement.Sync();
                m_fakeMovement.enabled = false;
            }
            m_door.localEulerAngles = new Vector3(-90, 0, -106.74f);
            m_entered = false;

            EnvironmentSystem.Instance.ToggleOceanPlaneClipping(false);
            m_reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
        }

        private void Update()
        {
            if (!m_entered)
                return;

            if (m_useFakeMovement)
            {
                m_fakeMovement.CurrentPosition = m_startPos + Vector3.down * (m_maxDistance * m_interactable.Value);
            }
            EnvironmentSystem.Instance.SetOceanClipPlane(new Plane(m_windowAnchor.forward, m_windowAnchor.position));
        }
    }
}
