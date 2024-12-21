// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Keeps the subtitles on screen
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private Transform m_targetPosition;

        [SerializeField] private Vector2 m_bottomLeft = new(-1, -1);
        [SerializeField] private Vector2 m_topRight = new(1, 1);
        [SerializeField] protected float m_fixedSize = 1;
        [SerializeField] private float m_interpolationSpeed = 5;
        [SerializeField] private float m_maxDistance = 100;
        [SerializeField, AutoSet] private Canvas m_canvas;
        [SerializeField, AutoSet] private CanvasGroup m_canvasGroup;

        private Camera m_camera;
        private Vector3 m_lastPosition;

        private void Awake()
        {
            m_camera = Camera.main;
            m_lastPosition = m_targetPosition.position;
        }

        private void Start()
        {
            if (FloatingTextLayout.Instance) FloatingTextLayout.Instance.RegisterText(this);
        }

        private void OnEnable()
        {
            if (FloatingTextLayout.Instance) FloatingTextLayout.Instance.RegisterText(this);
        }

        private void OnDestroy()
        {
            if (FloatingTextLayout.Instance) FloatingTextLayout.Instance.UnregisterText(this);
        }

        private void Update()
        {
            UpdatePosition();
            if (m_canvasGroup.alpha == 0) m_lastPosition = transform.position;

        }

        private Vector3 GetOffsetVector(Vector3 target)
        {
            var fromCenter = target - new Vector3(0.5f, 0.5f);
            Debug.DrawLine(m_camera.ViewportToWorldPoint(target), m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), Color.green);
            if (fromCenter.magnitude < GlobalSettings.ScreenSettings.ScreenRadius)
                return Vector3.zero;
            var fromCenterToRing = fromCenter.normalized * GlobalSettings.ScreenSettings.ScreenRadius;
            Debug.DrawLine(m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f)), m_camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f) + fromCenterToRing), Color.red);
            return -(fromCenter - fromCenterToRing);
        }

        private void UpdatePosition()
        {
            if (m_camera == null) return;
            transform.rotation = m_camera.transform.rotation;

            var distance = Vector3.Distance(m_camera.transform.position, m_targetPosition.position);
            var visible = distance < m_maxDistance;
            var z = distance - m_camera.nearClipPlane;
            var size = distance * m_fixedSize * m_camera.fieldOfView;
            var targetPosition = m_targetPosition.position + Vector3.up * (GlobalSettings.ScreenSettings.SubtitleUpOffset * size);
            transform.localScale = Vector3.one * size;

            var camSpacePoint = m_camera.WorldToViewportPoint(targetPosition);
            camSpacePoint.z = z;

            if (Vector3.Dot(targetPosition - m_camera.transform.position, m_camera.transform.forward) < 0)
            {
                // Flip viewspace position when behind camera
                camSpacePoint.x = 1f - camSpacePoint.x;
                camSpacePoint.y = 1f - camSpacePoint.y;
                visible = false;
            }

            transform.position = m_camera.ViewportToWorldPoint(camSpacePoint);

            // Corner offsets in viewport space
            var bl = m_camera.WorldToViewportPoint(transform.TransformPoint(m_bottomLeft)) - camSpacePoint;
            var tr = m_camera.WorldToViewportPoint(transform.TransformPoint(m_topRight)) - camSpacePoint;

            // Screen bounds reduced by floating text extents
            var marginMin = GlobalSettings.ScreenSettings.ScreenMinBounds - (Vector2)bl;
            var marginMax = GlobalSettings.ScreenSettings.ScreenMaxBounds - (Vector2)tr;
            camSpacePoint.x = Mathf.Clamp(camSpacePoint.x, marginMin.x, marginMax.x);
            camSpacePoint.y = Mathf.Clamp(camSpacePoint.y, marginMin.y, marginMax.y);

            var newPos = m_camera.ViewportToWorldPoint(camSpacePoint);
            var dir = newPos - m_camera.transform.position;
            newPos = m_camera.transform.position + dir.normalized * Vector3.Distance(m_camera.transform.position, targetPosition);
            transform.forward = dir;
            transform.position = newPos;
            m_canvas.enabled = visible;
        }

        public void SyncPosition()
        {
            UpdatePosition();
            m_lastPosition = transform.position;
        }

        public bool Showing() => m_canvasGroup.alpha > 0;

        public void LerpToPosition(Vector3 target)
        {
            // move transform to lerped position
            transform.position = Vector3.Lerp(m_lastPosition, target, Time.deltaTime * m_interpolationSpeed);

            // fix rotation and scale after move
            transform.rotation = m_camera.transform.rotation;
            transform.forward = -(m_camera.transform.position - transform.position);
            var distance = Vector3.Distance(m_camera.transform.position, m_targetPosition.position);
            var size = distance * m_fixedSize * m_camera.fieldOfView;
            transform.localScale = Vector3.one * size;

            m_lastPosition = transform.position;
        }

        private Vector3 ClampToView(Vector3 point)
        {
            if (m_camera == null) return point;
            var pos = m_camera.WorldToViewportPoint(point);

            pos.x = Mathf.Clamp01(pos.x);
            pos.y = Mathf.Clamp01(pos.y);

            return m_camera.ViewportToWorldPoint(pos);
        }

        private void OnDrawGizmosSelected()
        {
            var worldBottomLeft = transform.TransformPoint(m_bottomLeft);
            var worldTopRight = transform.TransformPoint(m_topRight);
            Gizmos.DrawSphere(worldTopRight, .1f);
            Gizmos.DrawSphere(worldBottomLeft, .1f);
        }
    }
}
