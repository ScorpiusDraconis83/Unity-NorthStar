// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Helper for getting the closest point to a line
    /// </summary>
    public class LineSegment : MonoBehaviour
    {
        [SerializeField] private Vector3 a, b;

        //[SerializeField] Transform testCase;
        public Vector3 ClosestPoint(Vector3 point, bool inWorldSpace = true)
        {
            point = inWorldSpace ? transform.InverseTransformPoint(point) : point;
            var endPoint = ClosestPoint(a, b, point);
            return inWorldSpace ? transform.TransformPoint(endPoint) : endPoint;
        }

        public float DistanceFromLine(Vector3 point)
        {
            return Vector3.Distance(point, ClosestPoint(point));
        }

        public static Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 point)
        {
            var direction = (a - b).normalized;
            var c = point - b;
            var distanceAlongLine = Mathf.Clamp(Vector3.Dot(direction, c), 0, Vector3.Distance(a, b));
            var endPoint = b + direction * distanceAlongLine;
            return endPoint;
        }
        public Quaternion GetRotationRelative(Quaternion rotation)
        {
            var axis = (b - a).normalized;
            var ra = new Vector3(rotation.x, rotation.y, rotation.z);
            var p = Vector3.Project(ra, axis);
            var twist = new Quaternion(p.x, p.y, p.z, rotation.w);
            return twist.normalized;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Selection.gameObjects.IsParentOf(gameObject))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.TransformPoint(a), transform.TransformPoint(b));
            }
            //var rot = GetRotationRelative(testCase.rotation);
            //Gizmos.DrawRay(testCase.position, rot * Vector3.forward);
            //Gizmos.DrawRay(testCase.position, rot * Vector3.up);
        }
#endif
    }
}
