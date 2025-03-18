// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// To keep this shader simpler and handling fog "properly" am just adjusting the color
    /// of the fake volumetric lights based on distance from the player
    /// This is fine since there is just this one instance of them clustered together, for more
    /// complicated scenes consider looking at a shader based solution
    /// </summary>
    public class FoggyFakeLightController : MonoBehaviour
    {
        public Material FakeLightMaterial;
        [SerializeField] private Color m_minColor;
        [SerializeField] private Color m_maxColor;
        [SerializeField] private Transform m_head;
        [Tooltip("Beyond this distance, material will have minColor")]
        [Range(0.1f, 200f)]
        [SerializeField] private float m_maxDistance = 20f;
        [Tooltip("Closer than this distance, material will have maxColor")]
        [Range(0.1f, 200f)]
        [SerializeField] private float m_minDistance = 20f;
        [Tooltip("If true, this will update every frame.")]
        public bool RecolorOnUpdate = true;

        private void Update()
        {
            if (!RecolorOnUpdate)
            {
                return;
            }
            if (m_head is null)
            {
                return;
            }
            var distance = Vector3.Distance(transform.position, m_head.position);
            var newColor = Color.Lerp(m_minColor, m_maxColor, (distance - m_minDistance) / (m_maxDistance - m_minDistance));
            FakeLightMaterial.SetColor("_color", newColor);
        }
    }
}
