// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Allows the audio listner to ignorre fake movement
    /// </summary>
    public class PlayerAudioListener : MonoBehaviour
    {
        private void Start()
        {
            transform.parent = null;
        }
        [SerializeField] private Transform m_head;
        public bool PlayerOnBoat;
        private void Update()
        {
            if (PlayerOnBoat)
            {
                transform.position = BoatController.WorldToBoatSpace(m_head.position);
                transform.rotation = BoatController.WorldToBoatSpace(m_head.rotation);
            }
            else
            {
                transform.position = m_head.position;
                transform.rotation = m_head.rotation;
            }
        }
    }
}
