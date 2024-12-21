// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Manages NPC blend shapes for expressions and blinking
    /// </summary>
    [ExecuteAlways]
    public class BlendshapeManager : MonoBehaviour
    {
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        public float[] BlendShapeWeights;

        [Header("Blinking")]
        [SerializeField, Tooltip("Whether the skinnedmesh should blink using blendshapes during set times")]
        private bool m_autoBlinking = true;
        [SerializeField, Tooltip("The index for the blinking blendshape")]
        private int m_blinkingBlendshapeIndex = 0;
        [SerializeField, Tooltip("X represents the minimum time between blinks, while Y represents the maximum time")]
        private Vector2 m_blinkCoolDownRange;
        [SerializeField, Tooltip("How fast and for how long the eyes shut")]
        private AnimationCurve m_blinkCurve;
        private float m_blinkCountdown;

        public int BlendShapeCount => SkinnedMeshRenderer.sharedMesh.blendShapeCount;

        private void Awake()
        {
            ResetPositions(SkinnedMeshRenderer);
        }

        private void Update()
        {
#if UNITY_EDITOR
            //While the editor is running but not playing, preview the blendshapes while it is directly selected in the inspector
            if (!Application.IsPlaying(gameObject) && (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(gameObject.scene) || !Selection.gameObjects.Any(a => a == gameObject)))
            {
                //Otherwise if in edit mode and there is no timeline director selected, reset transforms and return early
                ResetPositions(SkinnedMeshRenderer);
                return;
            }
#endif

            if (SkinnedMeshRenderer != null)
            {
                //Weights are applied last to avoid conflicting with multiple clips on the same timeline
                for (var i = 0; i < BlendShapeWeights.Length; i++)
                {
                    if (BlendShapeWeights[i] != -1)
                    {
                        SkinnedMeshRenderer.SetBlendShapeWeight(i, BlendShapeWeights[i]);
                    }
                }
            }

            if (m_autoBlinking)
            {
                m_blinkCountdown -= Time.deltaTime;
                var lastFrameTime = m_blinkCurve.keys[^1].time; //Get the time of the last keyframe in the blink curve, assuming they are sorted timewise
                if (m_blinkCountdown <= lastFrameTime)
                {
                    BlendShapeWeights[m_blinkingBlendshapeIndex] = m_blinkCurve.Evaluate(lastFrameTime - m_blinkCountdown);

                    if (m_blinkCountdown <= 0)
                    {
                        m_blinkCountdown = Random.Range(m_blinkCoolDownRange.x, m_blinkCoolDownRange.y) + lastFrameTime; //reset the countdown, including the time for the blink's duration
                        BlendShapeWeights[m_blinkingBlendshapeIndex] = 0;
                    }
                }
            }
        }

        private void ResetPositions(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (!enabled)
            {
                return;
            }

            if (skinnedMeshRenderer != null)
            {
                BlendShapeWeights = new float[BlendShapeCount];
            }
            else
            {
                BlendShapeWeights = new float[0];
                Debug.LogError("BlendshapeManager must have a reference to a skinnedMesh", gameObject);
            }
        }

        //Allow external scripts to force the eyes to blink (such as when they should be moving)
        public void Blink()
        {
            var lastFrameTime = m_blinkCurve.keys[^1].time;
            m_blinkCountdown = lastFrameTime;
        }
    }
}
