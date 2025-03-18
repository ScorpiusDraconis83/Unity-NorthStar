// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Holds settings for objects that want to move with moving platforms managed by the ParentedTransform script
    /// </summary>
    public class MoveableObject : MonoBehaviour
    {
        public enum UpdateMode
        {
            FixedUpdate,
            LateFixedUpdate,
            Update,
            LateUpdate,
            Manual,
            Disabled
        }
        [SerializeField] protected UpdateMode m_updateMode;
        public enum ParentMethod
        {
            TrackTransform,
            TrackRigidBodyMove,
            TrackRigidBodySet,
            TrackTransformAndRigidbody,
            TrackVelocity,
            Parent,
            Joint
        }
        [SerializeField] protected ParentMethod m_parentMethod;
        [HideInInspector] public Rigidbody Body;
        public bool Registered = false;
        [HideInInspector] public ParentedTransform RegisteredTo = null;

        public delegate void OnSyncCallback();
        public OnSyncCallback OnSync;

        private void OnValidate()
        {
            Body = GetComponent<Rigidbody>();
            if (Body == null && (m_parentMethod == ParentMethod.TrackRigidBodyMove || m_parentMethod == ParentMethod.Joint))
            {
                Debug.LogError("Missing RigidBody on tracked physics object");
            }
        }

        public delegate void RegisterCallback(ParentedTransform owner);
        public RegisterCallback OnRegisterCallback;
        public RegisterCallback OnUnregisterCallback;
    }
}