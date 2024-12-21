// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Manages the silver curse VFX on the player's hand(s) throughout the game
    /// </summary>
    public class HandCurseManager : Singleton<HandCurseManager>
    {
        private static readonly int s_playerCurse = Shader.PropertyToID("_PlayerCurse");

        [SerializeField] private SkinnedMeshRenderer m_cursedRightHandRenderer;
        [SerializeField] private SkinnedMeshRenderer m_cursedLeftHandRenderer;
        private Material m_leftHandMaterial, m_rightHandMaterial;

        [SerializeField] private bool m_showLeftCurse = true;
        public bool ShowLeftCurse
        {
            get => m_showLeftCurse;
            set
            {
                m_showLeftCurse = value;
                UpdateCurse();
            }
        }

        [SerializeField] private bool m_showRightCurse = true;
        public bool ShowRightCurse
        {
            get => m_showRightCurse;
            set
            {
                m_showRightCurse = value;
                UpdateCurse();
            }
        }

        public void SetCurseAmount(float amount, bool updateVisibility = true)
        {
            if (updateVisibility)
                m_showLeftCurse = m_showRightCurse = amount > 0;
            m_curseAmount = amount;
            UpdateCurse();
        }

        [Range(0, 1)]
        [SerializeField] private float m_curseAmount = 0.5f;
        public float CurseAmount
        {
            get => m_curseAmount;
            set
            {
                m_curseAmount = value;
                UpdateCurse();
            }
        }

        private void Start()
        {
            UpdateCurse();
        }

        private void UpdateCurse()
        {
            if (!Application.isPlaying) return;

            const string CURSE_KEYWORD = "_USESILVERCURSE";

            if (m_cursedRightHandRenderer)
            {
                if (m_rightHandMaterial == null)
                    m_rightHandMaterial = m_cursedRightHandRenderer.material;
                if (ShowRightCurse)
                    m_rightHandMaterial.EnableKeyword(CURSE_KEYWORD);
                else
                    m_rightHandMaterial.DisableKeyword(CURSE_KEYWORD);
                m_rightHandMaterial.SetFloat(s_playerCurse, CurseAmount);
            }

            if (m_cursedLeftHandRenderer)
            {
                if (m_leftHandMaterial == null)
                    m_leftHandMaterial = m_cursedLeftHandRenderer.material;
                if (ShowLeftCurse)
                    m_leftHandMaterial.EnableKeyword(CURSE_KEYWORD);
                else
                    m_leftHandMaterial.DisableKeyword(CURSE_KEYWORD);
                m_leftHandMaterial.SetFloat(s_playerCurse, CurseAmount);
            }
        }
    }
}