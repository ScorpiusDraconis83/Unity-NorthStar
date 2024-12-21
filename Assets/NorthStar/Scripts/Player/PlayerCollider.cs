// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
namespace NorthStar
{
    /// <summary>
    /// Moves a colliders to the height and position of the tracked head
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerCollider : MonoBehaviour
    {
        private CapsuleCollider m_playerCollider;
        [SerializeField] private Transform m_head;

        private void Awake()
        {
            m_playerCollider = GetComponent<CapsuleCollider>();
        }

        // Update is called once per frame
        private void Update()
        {
            m_playerCollider.height = Mathf.Max(m_playerCollider.radius, m_head.localPosition.y);
            m_playerCollider.center = new Vector3(m_head.localPosition.x, m_head.localPosition.y / 2, m_head.localPosition.z);
        }
    }
}