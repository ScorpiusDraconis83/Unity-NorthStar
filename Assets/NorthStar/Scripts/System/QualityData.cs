// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using UnityEngine;
using static NorthStar.QualityControls;

namespace NorthStar
{
    [CreateAssetMenu(menuName = "Data/Quality Data")]
    public class QualityData : ScriptableObject
    {
        [field: SerializeField] public List<QualityPreset> Presets { get; private set; }

        public QualityPreset CurrentPreset
        {
            get
            {
                var headsetType = OVRPlugin.GetSystemHeadsetType();
                foreach (var preset in Presets)
                {
                    foreach (var targetHeadset in preset.Headsets)
                    {
                        if (targetHeadset == headsetType)
                        {
                            return preset;
                        }
                    }
                }

                return null;
            }
        }
    }
}
