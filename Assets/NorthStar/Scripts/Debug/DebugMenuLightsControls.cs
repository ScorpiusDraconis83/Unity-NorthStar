// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class TestLightsDebugControls : MonoBehaviour
    {
        public GameObject DirectionalLight;
        public bool PointLightsOffByDefault = false;
        [Header("Point lights to control can be manually added, or populated in Context Menu")]
        public Light[] ChildLights;
        private const string LIGHTS_CATEGORY = "Lights Stress Test";
        [Header("These can be applied in Context Menu")]
        [Range(0f, 20f)]
        public float LightIntensity = 6f;
        [Range(0.25f, 20f)]
        public float LightRange = 2.1f;


        private void Start()
        {
            _ = DebugSystem.Instance.AddAction(LIGHTS_CATEGORY, "Set New Random Colors", () =>
            {
                RandomizeLightColors();
            });
            if (ChildLights.Length > 0)
            {
                var numberOfLightsActive = DebugSystem.Instance.AddInt(LIGHTS_CATEGORY, "Number Of Point Lights Active", 0, ChildLights.Length, ChildLights.Length, false, (value) =>
                {
                    EnableSomeLights(value);
                });
            }
            var directionalLightEnabled = DebugSystem.Instance.AddBool(LIGHTS_CATEGORY, "Directional Light", true, false, (value) =>
            {
                DirectionalLightEnabled(value);
            });
            var lightsIntensityValue = DebugSystem.Instance.AddFloat(LIGHTS_CATEGORY, "Light Intensity", 0f, 20f, LightIntensity, false, (value) =>
            {
                LightIntensity = value;
                UpdateLightIntensity();
            });
            var lightsRangeValue = DebugSystem.Instance.AddFloat(LIGHTS_CATEGORY, "Light Range", 0.25f, 20f, LightRange, false, (value) =>
            {
                LightRange = value;
                UpdateLightRanges();
            });
            if (PointLightsOffByDefault)
            {
                DisableAllLights();
            }
        }
        public void DirectionalLightEnabled(bool value)
        {
            if (DirectionalLight == null)
            {
                return;
            }
            DirectionalLight.SetActive(value);
        }

        [ContextMenu("Populate Lights Array")]
        public void PopulateLightsArray()
        {
            ChildLights = GetComponentsInChildren<Light>();
        }

        [ContextMenu("Update Light Ranges")]
        public void UpdateLightRanges()
        {
            foreach (var light in ChildLights)
            {
                light.range = LightRange;
            }
        }
        [ContextMenu("Update Light Intensity")]
        public void UpdateLightIntensity()
        {
            foreach (var light in ChildLights)
            {
                light.intensity = LightIntensity;
            }
        }
        [ContextMenu("Randomize Light Colors")]
        public void RandomizeLightColors()
        {
            foreach (var light in ChildLights)
            {
                //SendMessage is a slightly clunky way of doing this, but quick testing menu thing not intended to be exposed in final demo
                light.gameObject.SendMessage("SetRandomLightColor");
            }
        }

        private void EnableSomeLights(int value)
        {
            DisableAllLights();
            for (var i = 0; i < value; i++)
            {
                ChildLights[i].gameObject.SetActive(true);
            }
        }
        [ContextMenu("Enable Point Lights")]
        public void EnableAllLights()
        {
            foreach (var light in ChildLights)
            {
                light.gameObject.SetActive(true);
            }
        }
        [ContextMenu("Disable Point Lights")]
        public void DisableAllLights()
        {
            foreach (var light in ChildLights)
            {
                light.gameObject.SetActive(false);
            }
        }
    }
}
