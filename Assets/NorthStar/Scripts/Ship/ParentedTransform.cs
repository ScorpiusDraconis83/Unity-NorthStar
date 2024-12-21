// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
namespace NorthStar
{
    /// <summary>
    /// Handles Registering objects to be moved with a platform
    /// </summary>
    public class ParentedTransform : MonoBehaviour
    {
        //private Matrix4x4 m_lastTransform;
        //private Matrix4x4 m_lastTransformFixedUpdate;
        //private Matrix4x4 m_lastManualUpdate;

        //[SerializeField] private float m_movementCutoff = 0.01f;
        //[SerializeField] private bool m_useFixedMatrix = true;
        //private List<MoveableObject> m_fixedUpdateObjects = new();
        //private List<MoveableObject> m_lateFixedUpdateObjects = new();
        //private List<MoveableObject> m_updateObjects = new();
        //private List<MoveableObject> m_lateUpdateObjects = new();
        //private List<MoveableObject> m_manualObjects = new();
        //private List<MoveableObject> m_velocityObjects = new();
        //private List<FixedJoint> m_joints = new();

        //private Queue<MoveableObject> m_registerQueue = new();
        //public void RegisterObject(MoveableObject target)
        //{
        //    if (target.Registered || target.updateMode == MoveableObject.UpdateMode.Disabled)
        //        return;
        //    switch (target.parentMethod)
        //    {
        //        case MoveableObject.ParentMethod.Parent:
        //            target.transform.SetParent(transform);
        //            break;
        //        case MoveableObject.ParentMethod.Joint:
        //            var joint = new FixedJoint
        //            {
        //                autoConfigureConnectedAnchor = true,
        //                connectedBody = target.Body
        //            };
        //            m_joints.Add(joint);
        //            break;
        //        case MoveableObject.ParentMethod.TrackVelocity:
        //            m_velocityObjects.Add(target);
        //            break;
        //        case MoveableObject.ParentMethod.TrackTransform:
        //        case MoveableObject.ParentMethod.TrackRigidBodyMove:
        //        case MoveableObject.ParentMethod.TrackRigidBodySet:
        //        case MoveableObject.ParentMethod.TrackTransformAndRigidbody:
        //            switch (target.updateMode)
        //            {
        //                case MoveableObject.UpdateMode.FixedUpdate:
        //                    m_fixedUpdateObjects.Add(target);
        //                    break;
        //                case MoveableObject.UpdateMode.LateFixedUpdate:
        //                    m_lateFixedUpdateObjects.Add(target);
        //                    break;
        //                case MoveableObject.UpdateMode.LateUpdate:
        //                    m_lateUpdateObjects.Add(target);
        //                    break;
        //                case MoveableObject.UpdateMode.Update:
        //                    m_updateObjects.Add(target);
        //                    break;
        //                case MoveableObject.UpdateMode.Manual:
        //                    m_manualObjects.Add(target);
        //                    break;
        //            }
        //            break;
        //    }
        //    target.Registered = true;
        //    target.RegisteredTo = this;
        //    target.OnRegisterCallback?.Invoke(this);
        //}

        //private void UnregisterObject(MoveableObject target)
        //{
        //    if (!target.Registered)
        //        return;
        //    switch (target.parentMethod)
        //    {
        //        case MoveableObject.ParentMethod.Parent:
        //            target.transform.SetParent(null);
        //            break;
        //        case MoveableObject.ParentMethod.Joint:
        //            foreach (var joint in m_joints)
        //            {
        //                if (joint.connectedBody == target.Body)
        //                {
        //                    Destroy(joint);
        //                    _ = m_joints.Remove(joint);
        //                    break;
        //                }
        //            }
        //            break;
        //        case MoveableObject.ParentMethod.TrackTransform:
        //        case MoveableObject.ParentMethod.TrackRigidBodyMove:
        //        case MoveableObject.ParentMethod.TrackRigidBodySet:
        //        case MoveableObject.ParentMethod.TrackTransformAndRigidbody:
        //            switch (target.updateMode)
        //            {
        //                case MoveableObject.UpdateMode.FixedUpdate:
        //                    _ = m_fixedUpdateObjects.Remove(target);
        //                    break;
        //                case MoveableObject.UpdateMode.LateFixedUpdate:
        //                    _ = m_lateFixedUpdateObjects.Remove(target);
        //                    break;
        //                case MoveableObject.UpdateMode.LateUpdate:
        //                    _ = m_lateUpdateObjects.Remove(target);
        //                    break;
        //                case MoveableObject.UpdateMode.Update:
        //                    _ = m_updateObjects.Remove(target);
        //                    break;
        //                case MoveableObject.UpdateMode.Manual:
        //                    _ = m_manualObjects.Remove(target);
        //                    break;
        //            }
        //            break;
        //    }
        //    target.Registered = false;
        //    target.RegisteredTo = null;
        //    target.OnUnregisterCallback?.Invoke(this);
        //}

        //// Start is called before the first frame update
        //private void Awake()
        //{
        //    m_lastTransform = transform.localToWorldMatrix;
        //    m_lastTransformFixedUpdate = transform.localToWorldMatrix;
        //    m_lastManualUpdate = transform.localToWorldMatrix;
        //}

        //private void Update()
        //{
        //    while (m_registerQueue.Count > 0)
        //    {
        //        RegisterObject(m_registerQueue.Dequeue());
        //    }
        //    TrackObjects(m_updateObjects, Matrix4x4.Inverse(m_lastTransform));
        //}

        //private void FixedUpdate()
        //{
        //    TrackObjects(m_fixedUpdateObjects, Matrix4x4.Inverse(m_useFixedMatrix ? m_lastTransformFixedUpdate : m_lastTransform));
        //    _ = StartCoroutine(LateFixedUpdate());
        //}

        //private void LateUpdate()
        //{
        //    TrackObjects(m_lateUpdateObjects, Matrix4x4.Inverse(m_lastTransform));

        //    m_lastTransform = transform.localToWorldMatrix;
        //}

        //private IEnumerator LateFixedUpdate()
        //{
        //    yield return new WaitForFixedUpdate();
        //    TrackObjects(m_lateFixedUpdateObjects, Matrix4x4.Inverse(m_useFixedMatrix ? m_lastTransformFixedUpdate : m_lastTransform));
        //    m_lastTransformFixedUpdate = transform.localToWorldMatrix;
        //}

        //private void TrackObjects(List<MoveableObject> moveObjects, Matrix4x4 inverseTransform)
        //{
        //    foreach (var moveableObject in moveObjects)
        //    {
        //        switch (moveableObject.parentMethod)
        //        {
        //            case MoveableObject.ParentMethod.TrackTransform:
        //                {
        //                    var target = moveableObject.transform;

        //                    var localPosition = inverseTransform.MultiplyPoint(target.position);
        //                    var worldPosition = transform.localToWorldMatrix.MultiplyPoint(localPosition);

        //                    var inverseRotation = inverseTransform.rotation;
        //                    var worldRotation = target.rotation * inverseRotation * transform.localToWorldMatrix.rotation;

        //                    target.SetPositionAndRotation(worldPosition, worldRotation);
        //                    break;
        //                }
        //            case MoveableObject.ParentMethod.TrackRigidBodyMove:
        //                {
        //                    var target = moveableObject.Body;

        //                    var localPosition = inverseTransform.MultiplyPoint(target.position);
        //                    var worldPosition = transform.localToWorldMatrix.MultiplyPoint(localPosition);

        //                    var inverseRotation = inverseTransform.rotation;
        //                    var worldRotation = target.rotation * inverseRotation * transform.localToWorldMatrix.rotation;

        //                    target.MovePosition(worldPosition);
        //                    target.MoveRotation(worldRotation);
        //                    break;
        //                }
        //            case MoveableObject.ParentMethod.TrackRigidBodySet:
        //                {
        //                    var target = moveableObject.Body;

        //                    var localPosition = inverseTransform.MultiplyPoint(target.position);
        //                    var worldPosition = transform.localToWorldMatrix.MultiplyPoint(localPosition);

        //                    var inverseRotation = inverseTransform.rotation;
        //                    var worldRotation = target.rotation * inverseRotation * transform.localToWorldMatrix.rotation;

        //                    target.position = (worldPosition);
        //                    target.rotation = (worldRotation);
        //                    break;
        //                }
        //            case MoveableObject.ParentMethod.TrackTransformAndRigidbody:
        //                {
        //                    var target = moveableObject.transform;

        //                    var localPosition = inverseTransform.MultiplyPoint(target.position);
        //                    var worldPosition = transform.localToWorldMatrix.MultiplyPoint(localPosition);

        //                    var inverseRotation = inverseTransform.rotation;
        //                    var worldRotation = target.rotation * inverseRotation * transform.localToWorldMatrix.rotation;

        //                    target.SetPositionAndRotation(worldPosition, worldRotation);
        //                    moveableObject.Body.position = worldPosition;
        //                    moveableObject.Body.rotation = worldRotation;
        //                    break;
        //                }
        //        }
        //        moveableObject.OnSync?.Invoke();
        //    }
        //}

        //public void ManualSync()
        //{
        //    TrackObjects(m_manualObjects, Matrix4x4.Inverse(m_lastManualUpdate));
        //    m_lastManualUpdate = transform.localToWorldMatrix;
        //}

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.TryGetComponent(out MoveableObject moveableObject))
        //    {
        //        m_registerQueue.Enqueue(moveableObject);
        //    }
        //    else
        //    {
        //        if (other.attachedRigidbody != null)
        //        {
        //            if (other.attachedRigidbody.TryGetComponent(out moveableObject))
        //            {
        //                m_registerQueue.Enqueue(moveableObject);
        //            }
        //        }
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (other.TryGetComponent(out MoveableObject moveableObject))
        //    {
        //        UnregisterObject(moveableObject);
        //    }
        //    else
        //    {
        //        if (other.attachedRigidbody != null)
        //        {
        //            if (other.TryGetComponent(out moveableObject))
        //            {
        //                UnregisterObject(moveableObject);
        //            }
        //        }
        //    }
        //}
    }
}