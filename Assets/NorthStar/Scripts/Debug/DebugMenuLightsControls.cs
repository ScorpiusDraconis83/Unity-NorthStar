// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public class TestLightsDebugControls : MonoBehaviour
    {
        public GameObject directionalLight;
        public bool pointLightsOffByDefault = false;
        [Header("Point lights to control can be manually added, or populated in Context Menu")]
        public Light[] childLights;
        private const string LIGHTS_CATEGORY = "Lights Stress Test";
        [Header("These can be applied in Context Menu")]
        [Range(0f, 20f)]
        public float lightIntensity = 6f;
        [Range(0.25f, 20f)]
        public float lightRange = 2.1f;


        private void Start()
        {
            _ = DebugSystem.Instance.AddAction(LIGHTS_CATEGORY, "Set New Random Colors", () =>
            {
                RandomizeLightColors();
            });
            if (childLights.Length > 0)
            {
                var numberOfLightsActive = DebugSystem.Instance.AddInt(LIGHTS_CATEGORY, "Number Of Point Lights Active", 0, childLights.Length, childLights.Length, false, (value) =>
                {
                    EnableSomeLights(value);
                });
            }
            var directionalLightEnabled = DebugSystem.Instance.AddBool(LIGHTS_CATEGORY, "Directional Light", true, false, (value) =>
            {
                DirectionalLightEnabled(value);
            });
            var lightsIntensityValue = DebugSystem.Instance.AddFloat(LIGHTS_CATEGORY, "Light Intensity", 0f, 20f, lightIntensity, false, (value) =>
            {
                lightIntensity = value;
                UpdateLightIntensity();
            });
            var lightsRangeValue = DebugSystem.Instance.AddFloat(LIGHTS_CATEGORY, "Light Range", 0.25f, 20f, lightRange, false, (value) =>
            {
                lightRange = value;
                UpdateLightRanges();
            });
            if (pointLightsOffByDefault)
            {
                DisableAllLights();
            }
        }
        public void DirectionalLightEnabled(bool value)
        {
            if (directionalLight == null)
            {
                return;
            }
            directionalLight.SetActive(value);
        }

        [ContextMenu("Populate Lights Array")]
        public void PopulateLightsArray()
        {
            childLights = GetComponentsInChildren<Light>();
        }

        [ContextMenu("Update Light Ranges")]
        public void UpdateLightRanges()
        {
            foreach (var light in childLights)
            {
                light.range = lightRange;
            }
        }
        [ContextMenu("Update Light Intensity")]
        public void UpdateLightIntensity()
        {
            foreach (var light in childLights)
            {
                light.intensity = lightIntensity;
            }
        }
        [ContextMenu("Randomize Light Colors")]
        public void RandomizeLightColors()
        {
            foreach (var light in childLights)
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
                childLights[i].gameObject.SetActive(true);
            }
        }
        [ContextMenu("Enable Point Lights")]
        public void EnableAllLights()
        {
            foreach (var light in childLights)
            {
                light.gameObject.SetActive(true);
            }
        }
        [ContextMenu("Disable Point Lights")]
        public void DisableAllLights()
        {
            foreach (var light in childLights)
            {
                light.gameObject.SetActive(false);
            }
        }
    }
}
