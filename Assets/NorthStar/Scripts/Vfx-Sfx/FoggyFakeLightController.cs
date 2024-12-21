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
        public Material fakeLightMaterial;
        [SerializeField] private Color m_minColor;
        [SerializeField] private Color m_maxColor;
        [SerializeField] private Transform m_head;
        [Tooltip("Beyond this distance, material will have minColor")]
        [Range(0.1f, 200f)]
        [SerializeField] private float maxDistance = 20f;
        [Tooltip("Closer than this distance, material will have maxColor")]
        [Range(0.1f, 200f)]
        [SerializeField] private float minDistance = 20f;
        [Tooltip("If true, this will update every frame.")]
        public bool recolorOnUpdate = true;

        private void Update()
        {
            if (!recolorOnUpdate)
            {
                return;
            }
            if (m_head is null)
            {
                return;
            }
            var distance = Vector3.Distance(transform.position, m_head.position);
            var newColor = Color.Lerp(m_minColor, m_maxColor, (distance - minDistance) / (maxDistance - minDistance));
            fakeLightMaterial.SetColor("_color", newColor);
        }
    }
}
