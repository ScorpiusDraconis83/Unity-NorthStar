// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    public class RopePoint
    {
        public Vector3 Position;
        public Collider Collider;
    }

    public class RaycastRope : MonoBehaviour
    {
        [SerializeField] private List<RopePoint> m_points = new();
        [SerializeField] private Transform m_origin;
        [SerializeField] public Rigidbody Target;
        [SerializeField] private float m_maxLength = 100;
        [SerializeField] private LayerMask m_layerMask;
        [SerializeField] private ConfigurableJoint m_joint;
        private LineRenderer m_lineRenderer;
        private RopePoint m_startPoint = new();

        private void Awake()
        {
            m_lineRenderer = GetComponent<LineRenderer>();
            m_joint.connectedBody = Target;
            m_startPoint.Position = ToRopeSpace(m_origin.position);
            m_points.Add(m_startPoint);
        }

        private void Update()
        {
            m_points[0].Position = ToRopeSpace(m_origin.position);
            m_lineRenderer.SetPosition(0, m_points[0].Position);
            var toLastPosition = FromRopeSpace(m_points[^1].Position) - Target.position;
            if (Physics.Raycast(Target.position, toLastPosition, out var hit, toLastPosition.magnitude * .99f, m_layerMask, QueryTriggerInteraction.Ignore))
            {
                var newPoint = new RopePoint
                {
                    Position = ToRopeSpace(hit.point),
                    Collider = hit.collider
                };
                m_points.Add(newPoint);
                m_lineRenderer.SetPosition(m_lineRenderer.positionCount - 1, ToRopeSpace(hit.point + hit.normal * .01f));
                m_lineRenderer.positionCount++;
            }

            float distanceInRope = 0;

            if (m_points.Count > 1)
            {
                var toLastLastPosition = FromRopeSpace(m_points[^2].Position) - Target.position;
                if (!Physics.Raycast(Target.position, toLastLastPosition, toLastLastPosition.magnitude * .99f, m_layerMask, QueryTriggerInteraction.Ignore))
                {
                    m_points.RemoveAt(m_points.Count - 1);
                    m_lineRenderer.positionCount--;
                }

                for (var i = 1; i < m_points.Count; i++)
                {
                    distanceInRope += Vector3.Distance(m_points[i].Position, m_points[i - 1].Position);
                }
            }
            //DoSlipping();

            var remainingDistance = m_maxLength - distanceInRope;
            SetJointLimit(remainingDistance);
            m_joint.anchor = m_points[^1].Position;
            m_lineRenderer.SetPosition(m_lineRenderer.positionCount - 1, ToRopeSpace(Target.position));
        }

        public Vector3 ToRopeSpace(Vector3 point)
        {
            return transform.InverseTransformPoint(point);
        }

        public Vector3 FromRopeSpace(Vector3 point)
        {
            return transform.TransformPoint(point);
        }

        public void TieAt(Vector3 position)
        {
            m_lineRenderer.SetPosition(m_lineRenderer.positionCount - 1, ToRopeSpace(position));
            enabled = false;
        }

        private void DoSlipping()
        {
            var index = m_points.Count - 2;
            while (index >= 1)
            {
                var onSurface = m_points[index];
                var offSurface = m_points[index + 1];
                var closestPoint = onSurface.Collider.ClosestPoint(offSurface.Position);

                if (Physics.Linecast(closestPoint, onSurface.Position))
                    break;

                onSurface.Position = closestPoint;
                m_lineRenderer.SetPosition(index, onSurface.Position);

                index--;
            }
        }

        private void SetJointLimit(float distance)
        {
            var limit = new SoftJointLimit
            {
                limit = distance
            };
            m_joint.linearLimit = limit;
        }

        private void OnDrawGizmos()
        {
            if (m_points.Count == 0)
                return;
            var toLastPosition = FromRopeSpace(m_points[^1].Position) - Target.position;
            Gizmos.color = Color.green;
            Gizmos.DrawRay(Target.position, toLastPosition);
            Gizmos.DrawSphere(FromRopeSpace(m_points[^1].Position), 0.1f);

            if (m_points.Count > 1)
            {
                var toLastLastPosition = FromRopeSpace(m_points[^2].Position) - Target.position;
                Gizmos.color = Color.red;
                Gizmos.DrawRay(Target.position, toLastLastPosition);
                Gizmos.DrawSphere(FromRopeSpace(m_points[^2].Position), 0.01f);
            }
        }

        public void ResetRope()
        {
            m_points.Clear();
            m_lineRenderer.positionCount = 2;
            m_startPoint.Position = m_origin.position;
            m_points.Add(m_startPoint);
            SetJointLimit(m_maxLength);
        }

    }
}