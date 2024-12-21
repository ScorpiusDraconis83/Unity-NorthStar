// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.IO;
using uLipSync;
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    public class MenuItems : MonoBehaviour
    {
        [MenuItem("Assets/Create/Data/Baked Lipsync")]
        private static void CreateMultipleLipsyncData()
        {
            // Sort through every selected object
            foreach (var obj in Selection.objects)
            {
                // Find the selected audio clip from the user
                var audioObject = obj as AudioClip;
                if (audioObject == null)
                {
                    Debug.LogWarning("You must select an audio clip!");
                    return;
                }

                CreateLipsyncData(audioObject);
            }
        }

        private static void CreateLipsyncData(AudioClip audioObject)
        {

            // Locate where in the project the asset is
            var assetPath = AssetDatabase.GetAssetPath(audioObject);

            if (assetPath is null or "")
            {
                Debug.LogWarning("The audio clip can not be found in the asset database.");
                return;
            }

            // Remove the last part of the asset path (I.E. name.wav)
            var pathSplit = assetPath.Split("/");
            assetPath = "";
            for (var i = 0; i < pathSplit.Length - 1; i++)
            {
                assetPath = Path.Join(assetPath, pathSplit[i]);
            }

            // Create a simple material asset
            var lipsyncData = ScriptableObject.CreateInstance<BakedData>();
            lipsyncData.audioClip = audioObject;
            lipsyncData.name = audioObject.name;

            string[] guids;

            // TODO: create a more dynamic way to get relevant voice profile
            switch (lipsyncData.name[^1])
            {
                case 'A':
                    guids = AssetDatabase.FindAssets("Audrey-profile");

                    break;
                case 'B':
                    guids = AssetDatabase.FindAssets("Bessie-profile");

                    break;
                case 'T':
                    guids = AssetDatabase.FindAssets("Thomas-profile");

                    break;
                default:
                    Debug.LogWarning("Audio clip name is not formatted correctly.");

                    Destroy(lipsyncData);
                    return;
            }

            if (guids.Length == 0)
            {
                Debug.LogError("Could not find needed lipsync profiles!");
                Destroy(lipsyncData);
                return;
            }

            Profile bakedProfile = null;
            // Check all GUIDs to get the lipsync profiles
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<Profile>(path);

                if (profile != null)
                {
                    bakedProfile = profile;
                    break; // Assume first match is correct
                }
            }

            if (bakedProfile == null)
            {
                Debug.LogError("Could not find needed lipsync profiles!");
                Destroy(lipsyncData);
                return;
            }

            lipsyncData.profile = bakedProfile;

            AssetDatabase.CreateAsset(lipsyncData, Path.Join(assetPath, lipsyncData.name + ".asset"));

            // Print the path of the created asset
            Debug.Log(AssetDatabase.GetAssetPath(lipsyncData));
        }
    }
}
