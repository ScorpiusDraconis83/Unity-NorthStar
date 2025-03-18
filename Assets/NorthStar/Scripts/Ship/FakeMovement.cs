// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using System.Collections;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Used to fake object movement in the world. What is this used for? The boat is the main use-case where we need
    /// consistent physics while giving the illusion of movement and rotation. We experimented early on with different
    /// forms of movement but each one had issues/drawbacks so we decided to fake it!
    /// 
    /// How does it work?
    /// 
    /// Essentially the parent of objects we want ot move simply doesn't move during Update() or FixedUpdate(). And then
    /// when LateUpdate() is called the object is moved the to the "fake" position and then after rending is completed it
    /// gets moved back. From the perspective of the physics engine and any scripts, the object has not moved
    /// 
    /// There are still several side-effects however since other scripts that get called during LateUpdate() or other late
    /// callbacks might get confused and we've had to compensate for that in several cases
    /// </summary>
    public class FakeMovement : MonoBehaviour
    {
        public Vector3 OriginalPosition { get; private set; }
        public Quaternion OriginalRotation { get; private set; }

        public Vector3 CurrentPosition;
        public Quaternion CurrentRotation;
        public Action OnSync;

        public Vector3 Forward => CurrentRotation * Vector3.forward;
        public Vector3 Right => CurrentRotation * Vector3.right;
        public Vector3 Up => CurrentRotation * Vector3.up;

        private bool m_shouldSync = false;

        public GameObject[] OtherObjectsToMove;

        private Vector3[] m_otherObjectsOriginalPositions;
        private Quaternion[] m_otherObjectsOriginalRotations;

        private void Start()
        {
            _ = StartCoroutine(EndOfFrameUpdater());
            Setup();
        }

        public void Setup()
        {
            OriginalPosition = transform.position;
            OriginalRotation = transform.rotation;
            CurrentPosition = transform.position;
            CurrentRotation = transform.rotation;
            m_otherObjectsOriginalPositions = new Vector3[OtherObjectsToMove.Length];
            m_otherObjectsOriginalRotations = new Quaternion[OtherObjectsToMove.Length];
            for (var i = 0; i < OtherObjectsToMove.Length; i++)
            {
                m_otherObjectsOriginalPositions[i] = OtherObjectsToMove[i].transform.position;
                m_otherObjectsOriginalRotations[i] = OtherObjectsToMove[i].transform.rotation;
            }
            OnSync?.Invoke();
        }

        private IEnumerator EndOfFrameUpdater()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (!enabled)
                    continue;
                EndOfFrameUpdate();
            }
        }

        private void EndOfFrameUpdate()
        {
            // Restore the original boat transform before the next frame starts
            transform.position = OriginalPosition;
            transform.rotation = OriginalRotation;

            if (m_shouldSync)
            {
                DoSync();
                m_shouldSync = false;
            }
        }

        private void LateUpdate()
        {
            transform.position = CurrentPosition;
            transform.rotation = CurrentRotation;
            for (var i = 0; i < OtherObjectsToMove.Length; i++)
            {
                OtherObjectsToMove[i].transform.position = m_otherObjectsOriginalPositions[i] + CurrentPosition;
                OtherObjectsToMove[i].transform.rotation = m_otherObjectsOriginalRotations[i] * CurrentRotation;
            }
        }

        public void Sync()
        {
            if (m_shouldSync)
                return;
            m_shouldSync = true;

        }

        public Vector3 ConvertPoint(Vector3 point)
        {
            return CurrentRotation * (point - transform.position) + CurrentPosition;
        }

        public Quaternion ConvertRotation(Quaternion rotation)
        {
            return rotation * Quaternion.Inverse(transform.rotation) * CurrentRotation;
        }

        private void DoSync()
        {
            transform.position = CurrentPosition;
            transform.rotation = CurrentRotation;
            OriginalPosition = transform.position;
            OriginalRotation = transform.rotation;
            OnSync?.Invoke();
        }
    }
}
