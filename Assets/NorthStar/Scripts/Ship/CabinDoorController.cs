// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections;
using Meta.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace NorthStar
{
    /// <summary>
    /// Handles the cabin door opening/closing. This is important as objects that cannot be seen from one side or the other may need to be disabled
    /// to maintain consistent performance during gameplay
    /// </summary>
    public class CabinDoorController : Singleton<CabinDoorController>
    {
        public UnityEvent DoorOpened;
        public UnityEvent DoorClosed;

        [SerializeField] private Transform m_boatArt;
        [SerializeField] private Transform[] m_outsideCabinArt;
        [SerializeField] private Transform m_cabinArt;
        [SerializeField] private Animation m_animation;

        [SerializeField] private float m_openYRotation = 100f;
        [SerializeField] private float m_closedYRotation = 180f;

        [SerializeField] private bool m_startOpen;
        [SerializeField] private bool m_showBoatInitially = true;
        [SerializeField] private bool m_showCabinInitially = true;
        [SerializeField] private bool m_hideCabinWhileOutside = true;
        [SerializeField] private bool m_hideBoatWhileInside = true;

        private void Start()
        {
            SetOpenInstant(m_startOpen, m_showBoatInitially, m_showCabinInitially);
        }

        public void SetOpenInstant(bool opened, bool showBoat, bool showCabin)
        {
            if (m_boatArt) m_boatArt.gameObject.SetActive(opened || showBoat || !m_hideBoatWhileInside);
            foreach (var t in m_outsideCabinArt)
            {
                t.gameObject.SetActive(opened || showBoat || !m_hideBoatWhileInside);
            }
            if (m_cabinArt != null)
            {
                m_cabinArt.gameObject.SetActive(opened || showCabin || !m_hideCabinWhileOutside);
            }

            var angles = transform.localEulerAngles;
            angles.y = opened ? m_openYRotation : m_closedYRotation;
            transform.localRotation = Quaternion.Euler(angles);
        }

        public void OpenDoor() => AnimateDoor(true, true, true);
        public void CloseDoor() => AnimateDoor(false, true, true);
        public void CloseDoorFromInside() => AnimateDoor(false, false, true);
        public void CloseDoorFromOutside() => AnimateDoor(false, true, false);

        private void AnimateDoor(bool opening, bool showBoat, bool showCabin)
        {
            StopAllCoroutines();
            _ = StartCoroutine(AnimateDoorCoroutine(opening, showBoat, showCabin));
        }

        private IEnumerator AnimateDoorCoroutine(bool opening, bool showBoat, bool showCabin)
        {
            // need to initially be showing both art
            if (m_boatArt) m_boatArt.gameObject.SetActive(true);
            foreach (var t in m_outsideCabinArt)
            {
                t.gameObject.SetActive(true);
            }
            if (m_cabinArt != null)
            {
                m_cabinArt.gameObject.SetActive(true);
            }

            // trigger the animation
            _ = m_animation.Play(opening ? "CabinDoorOpen" : "CabinDoorClose", PlayMode.StopAll);

            // wait until the animation is done
            while (m_animation.isPlaying) yield return null;

            if (m_boatArt) m_boatArt.gameObject.SetActive(opening || showBoat || !m_hideBoatWhileInside);
            foreach (var t in m_outsideCabinArt)
            {
                t.gameObject.SetActive(opening || showBoat || !m_hideBoatWhileInside);
            }
            if (m_cabinArt != null)
            {
                m_cabinArt.gameObject.SetActive(opening || showCabin || !m_hideCabinWhileOutside);
            }

            (opening ? DoorOpened : DoorClosed)?.Invoke();
        }
    }
}