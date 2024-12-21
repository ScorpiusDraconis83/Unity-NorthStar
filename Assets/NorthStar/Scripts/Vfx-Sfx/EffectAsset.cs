// Copyright (c) Meta Platforms, Inc. and affiliates.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Data object representing an effect with convenience methods for creation and playing
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Effect Asset")]
    public class EffectAsset : ScriptableObject
    {
        public GameObject VfxObjectPrefab;
        [Min(1)] public int PoolSize = 10;

        public void Play(Vector3 position, Quaternion rotation, bool inBoatSpace = false, float intensity = 1)
        {
            VfxManager.Spawn(this, position, rotation, inBoatSpace, intensity);
        }

#if UNITY_EDITOR


        [ContextMenu("Create")]
        private void CreateObject()
        {
            var asset = new GameObject(name);
            _ = asset.AddComponent<EffectObject>();

            var selections = Selection.objects;
            var clips = new List<AudioClip>();

            foreach (var obj in selections)
            {
                if (obj is AudioClip clip)
                {
                    clips.Add(clip);
                }
                if (obj is GameObject go)
                {
                    if (go.TryGetComponent(out ParticleSystem _))
                    {
                        var spawn = Instantiate(go);
                        spawn.transform.parent = asset.transform;
                        spawn.transform.localPosition = Vector3.zero;
                        spawn.transform.localRotation = Quaternion.identity;
                        spawn.transform.localScale = Vector3.one;
                    }
                }
            }
            //Add default Unity AudioSource component and set default values
            var audioSource = asset.AddComponent<AudioSource>();
            audioSource.spatialize = true;
            //TODO: SET AUDIO OUTPUT MIXER (audioSource.outputAudioMixerGroup)
            //Feels like it might be better to have a base prefab which is used to create prefab variants rather than setting all these things through code?
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.maxDistance = 100f;
            //Add MetaXRAudioSource component to ensure this audiosource plays nicely with the Meta Audio Spatialization
            var metaXRAudioSource = asset.AddComponent<MetaXRAudioSource>();
            metaXRAudioSource.EnableSpatialization = true;
            metaXRAudioSource.GainBoostDb = 10f;
            metaXRAudioSource.EnableAcoustics = true;
            metaXRAudioSource.ReverbSendDb = 0f;
            var sound = asset.AddComponent<SoundPlayer>();
            sound.Clips = clips;


            var path = AssetDatabase.GetAssetPath(this);
            path = path.Replace(".asset", ".prefab");
            Debug.Log(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(asset, path);
            VfxObjectPrefab = prefab;
            DestroyImmediate(asset);
        }
#endif
    }
}
