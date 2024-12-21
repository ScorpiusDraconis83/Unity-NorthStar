// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using Meta.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace NorthStar
{
    /// <summary>
    /// Keeps floating text from stacking
    /// </summary>
    public class FloatingTextLayout : Singleton<FloatingTextLayout>
    {
        private readonly List<FloatingText> m_texts = new();

        private static Vector3[] s_corners = new Vector3[4];
        private Camera m_mainCamera;

        public void RegisterText(FloatingText text)
        {
            if (m_texts.Contains(text)) return;
            m_texts.Add(text);
        }

        private void Start()
        {
            m_mainCamera = Camera.main;
        }

        public void UnregisterText(FloatingText text) => m_texts.Remove(text);

        private void Update()
        {
            if (!m_mainCamera) return;
            var mainCameraPosition = m_mainCamera.transform.position;

            // sort by distance
            m_texts.Sort(
                (lhs, rhs) =>
                    (lhs.transform.position - mainCameraPosition).sqrMagnitude < (rhs.transform.position - mainCameraPosition).sqrMagnitude ? -1 : 1);

            // loop over every text object (nearest first)
            using (ListPool<Rect>.Get(out var viewportRects))
            {
                var safety = 100;
                for (var i = 0; i < m_texts.Count; i++)
                {
                    // get the text object and skip it if it's not showing anything
                    var farText = m_texts[i];
                    if (!farText.Showing()) continue;
                    var farRect = (RectTransform)farText.transform;

                    // loop until we're sure the object is safely visible
                    var overlapping = true;
                    while (overlapping && safety-- > 0)
                    {
                        // move rect if it overlaps with a nearer rect
                        overlapping = FixOverlap(m_mainCamera, viewportRects, farRect);
                    }

                    // tell the text object to lerp to its new position
                    farText.LerpToPosition(farText.transform.position);

                    viewportRects.Add(GetViewportRect(m_mainCamera, farRect));
                }
            }
        }

        private static Rect GetViewportRect(Camera camera, RectTransform rect)
        {
            rect.GetWorldCorners(s_corners);
            Vector2 min = new(float.MaxValue, float.MaxValue);
            Vector2 max = new(float.MinValue, float.MinValue);

            for (var i = 0; i < 4; i++)
            {
                var nearViewport = camera.WorldToViewportPoint(s_corners[i]);
                min = Vector2.Min(min, nearViewport);
                max = Vector2.Max(max, nearViewport);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool GetHasOverlap(List<Rect> nearViewportRects, Rect farRect)
        {
            foreach (var nearRect in nearViewportRects)
            {
                if (nearRect.Overlaps(farRect)) return true;
            }
            return false;
        }

        private static bool FixOverlap(Camera camera, List<Rect> nearViewportRects, RectTransform far, float moveAmount = 0.25f)
        {
            var farRect = GetViewportRect(camera, far);
            if (!GetHasOverlap(nearViewportRects, farRect)) return false;

            far.position += Vector3.up * moveAmount;

            return true;
        }
    }
}