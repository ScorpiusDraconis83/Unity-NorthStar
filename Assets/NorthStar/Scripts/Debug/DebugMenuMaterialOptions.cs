// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class DebugMaterialOptions : MonoBehaviour
    {
        public GameObject waterGameObject;
        public Material complexSkybox;
        public Material simpleSkybox;
        public Renderer shipRenderer;
        [Header("Use Context Menu to fetch reference setup from Ship Model")]
        public Material[] referenceShipMaterials;
        public Material[] customShipMaterials;
        public Material[] standardShipMaterials;

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
                RendererActive(shipRenderer, value);
            });
        }

        public void WaterEnabled(bool value)
        {
            if (waterGameObject == null)
            {
                return;
            }
            waterGameObject.SetActive(value);
        }

        public void RendererActive(Renderer renderer, bool value)
        {
            renderer.enabled = value;
        }

        [ContextMenu("Complex Skybox")]
        public void ComplexSkybox()
        {
            if (complexSkybox == null)
            {
                return;
            }
            RenderSettings.skybox = complexSkybox;
        }

        [ContextMenu("Simple Skybox")]
        public void SimpleSkybox()
        {
            if (simpleSkybox == null)
            {
                return;
            }
            RenderSettings.skybox = simpleSkybox;
        }

        [ContextMenu("Fetch Reference Ship Materials")]
        public void FetchShipMaterials()
        {
            if (shipRenderer == null)
            {
                return;
            }
            referenceShipMaterials = shipRenderer.sharedMaterials;
        }

        [ContextMenu("Use Custom Ship Materials")]
        public void CustomShipMaterials()
        {
            if (shipRenderer == null)
            {
                return;
            }
            shipRenderer.materials = customShipMaterials;
        }

        [ContextMenu("Use Standard Ship Materials")]
        public void StandardShipMaterials()
        {
            if (shipRenderer == null)
            {
                return;
            }
            shipRenderer.materials = standardShipMaterials;
        }
    }
}
