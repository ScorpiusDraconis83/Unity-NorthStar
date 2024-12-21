// Copyright (c) Meta Platforms, Inc. and affiliates.
using DG.Tweening;
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Triggers the attatched renderer to glow
    /// </summary>
    public class FtueGlow : MonoBehaviour
    {
        [SerializeField, AutoSet] private Renderer m_renderer;
        private Material m_material;
        private float m_opacity;
        private const string PULSE_KEY = "_USE_INTERACTION_PULSE";
        private const string OPACITY_KEY = "_Pulse_Opacity";
        private void Awake()
        {
            m_material = m_renderer.material;
            GlobalSettings.FtueSettings.SetupMaterialForFtue(m_material);
        }

        private void OnEnable()
        {
            GlobalSettings.FtueSettings.OnValidate += UpdatematerialSettings;
        }
        private void OnDisable()
        {
            GlobalSettings.FtueSettings.OnValidate -= UpdatematerialSettings;
        }

        private void UpdatematerialSettings()
        {
            GlobalSettings.FtueSettings.SetupMaterialForFtue(m_material);
        }

        private void Update()
        {
            m_material.SetFloat(OPACITY_KEY, Mathf.Lerp(0, GlobalSettings.FtueSettings.GetPulseValue(), m_opacity));
        }

        [ContextMenu("Start")]
        public void StartPulsing()
        {
            m_material.EnableKeyword(PULSE_KEY);
            _ = DOTween.Kill(this);
            _ = DOTween.Sequence().AppendInterval(GlobalSettings.FtueSettings.PulseDelay).Append(DOTween.To(() => m_opacity, x => m_opacity = x, 1f, GlobalSettings.FtueSettings.FadeInTime));
        }

        [ContextMenu("Stop")]
        public void StopPulsing()
        {
            _ = DOTween.Kill(this);
            DOTween.To(() => m_opacity, x => m_opacity = x, 0f, GlobalSettings.FtueSettings.FadeInTime).onComplete += () => { m_material.DisableKeyword(PULSE_KEY); };
        }
    }
}
