// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class DebugMaterialOptions : MonoBehaviour
    {
        [SerializeField] private GameObject m_waterGameObject;
        [SerializeField] private Material m_complexSkybox;
        [SerializeField] private Material m_simpleSkybox;
        [SerializeField] private Renderer m_shipRenderer;
        [Header("Use Context Menu to fetch reference setup from Ship Model")]
        [SerializeField] private Material[] m_customShipMaterials;
        [SerializeField] private Material[] m_standardShipMaterials;

        private const string MATERIALS_CATEGORY = "MATERIAL COMPLEXITY SETTINGS";

        private void Start()
        {
            var waterEnabled = DebugSystem.Instance.AddBool(MATERIALS_CATEGORY, "Water Enabled", true, false, (value) =>
            {
                WaterEnabled(value);
            });
            var useComplexSky = DebugSystem.Instance.AddBool(MATERIALS_CATEGORY, "Use Dynamic Sky Material", true, false, (value) =>
            {
                if (value)
                {
                    ComplexSkybox();
                }
                else
                {
                    SimpleSkybox();
                }
            });
            var useCustomShipMaterials = DebugSystem.Instance.AddBool(MATERIALS_CATEGORY, "Use Custom Ship Materials", true, false, (value) =>
            {
                if (value)
                {
                    CustomShipMaterials();
                }
                else
                {
                    StandardShipMaterials();
                }
            });
            var shipRendererActive = DebugSystem.Instance.AddBool(MATERIALS_CATEGORY, "Ship Renderer Enabled", true, false, (value) =>
            {
                RendererActive(m_shipRenderer, value);
            });
        }

        public void WaterEnabled(bool value)
        {
            if (m_waterGameObject == null)
            {
                return;
            }
            m_waterGameObject.SetActive(value);
        }

        public void RendererActive(Renderer renderer, bool value)
        {
            renderer.enabled = value;
        }

        [ContextMenu("Complex Skybox")]
        public void ComplexSkybox()
        {
            if (m_complexSkybox == null)
            {
                return;
            }
            RenderSettings.skybox = m_complexSkybox;
        }

        [ContextMenu("Simple Skybox")]
        public void SimpleSkybox()
        {
            if (m_simpleSkybox == null)
            {
                return;
            }
            RenderSettings.skybox = m_simpleSkybox;
        }

        [ContextMenu("Use Custom Ship Materials")]
        public void CustomShipMaterials()
        {
            if (m_shipRenderer == null)
            {
                return;
            }
            m_shipRenderer.materials = m_customShipMaterials;
        }

        [ContextMenu("Use Standard Ship Materials")]
        public void StandardShipMaterials()
        {
            if (m_shipRenderer == null)
            {
                return;
            }
            m_shipRenderer.materials = m_standardShipMaterials;
        }
    }
}
