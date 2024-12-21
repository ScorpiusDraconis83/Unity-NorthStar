// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class ClothAnimator : MonoBehaviour
    {
        public Cloth cloth;
        public SkinnedMeshRenderer clothMesh;
        public float clothFadeTime = 1f;
        public float furlSpeed = 10f;
        public float furlInput = 0;

        private float m_windCutoff = 2f;
        private float m_clothBlendValue = 0;

        private void Update()
        {
            var furlDirection = furlInput - Input.GetAxis("Vertical");
            // print(furlDirection);
            if (m_clothBlendValue >= 0 && m_clothBlendValue <= 100)
            {
                m_clothBlendValue += furlDirection * furlSpeed * Time.deltaTime;
                m_clothBlendValue = Mathf.Clamp(m_clothBlendValue, 0, 100);
                clothMesh.SetBlendShapeWeight(0, m_clothBlendValue);
            }

            var blendProgress = m_clothBlendValue * 0.01f;
            GetComponent<ClothWind>().windFactor = Mathf.Clamp01(1 - blendProgress * m_windCutoff);
        }
    }
}