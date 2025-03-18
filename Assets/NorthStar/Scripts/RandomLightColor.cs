// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    [RequireComponent(typeof(Light))]
    public class RandomLightColor : MonoBehaviour
    {
        //This script just changes a lights color to a new random color
        //nothing much exposed right now since it's just for some quick testing
        public bool RandomlyChange = true;

        private Light m_thisLight;

        private void Start()
        {
            m_thisLight = GetComponent<Light>();
            InvokeNextChange();
        }

        private void InvokeNextChange()
        {
            if (RandomlyChange)
            {
                Invoke(nameof(SetRandomLightColor), Random.Range(1f, 10f));
            }
        }

        [ContextMenu("Set New Random Color")]
        public void SetRandomLightColor()
        {
            if (m_thisLight == null)
            {
                m_thisLight = GetComponent<Light>();
            }

            Color newColor = new Vector4(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
            m_thisLight.color = newColor;
            InvokeNextChange();
        }
    }
}