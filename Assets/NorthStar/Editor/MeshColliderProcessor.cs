// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Processes mesh into colliders
    /// </summary>
    public class MeshColliderProcessor : AssetPostprocessor
    {
        private void OnPostprocessModel(GameObject gameObject)
        {
            // Process all meshes in the imported model
            ProcessGameObjectHierarchy(gameObject);
        }

        private void ProcessGameObjectHierarchy(GameObject gameObject)
        {
            // Get all children including the root object
            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            var allObjects = new List<GameObject>();

            // Convert transforms to gameObjects
            for (var i = 0; i < transforms.Length; i++)
            {
                allObjects.Add(transforms[i].gameObject);
            }

            // Find all objects that have mesh renderers
            var meshObjects = new List<GameObject>();
            for (var i = 0; i < allObjects.Count; i++)
            {
                if (allObjects[i].GetComponent<MeshRenderer>() != null)
                {
                    meshObjects.Add(allObjects[i]);
                }
            }

            // Keep track of objects to destroy
            var objectsToDestroy = new List<GameObject>();

            foreach (var obj in meshObjects)
            {
                // Skip objects that end with _collider
                if (obj.name.EndsWith("_collider", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                // Look for a corresponding collider mesh
                GameObject colliderObj = null;
                for (var i = 0; i < allObjects.Count; i++)
                {
                    if (allObjects[i].name.Equals(obj.name + "_collider", System.StringComparison.OrdinalIgnoreCase))
                    {
                        colliderObj = allObjects[i];
                        break;
                    }
                }

                if (colliderObj != null)
                {
                    // Get or add MeshCollider component
                    var meshCollider = obj.GetComponent<MeshCollider>();
                    if (meshCollider == null)
                        meshCollider = obj.AddComponent<MeshCollider>();

                    // Assign the collider mesh
                    var colliderMeshFilter = colliderObj.GetComponent<MeshFilter>();
                    if (colliderMeshFilter != null && colliderMeshFilter.sharedMesh != null)
                    {
                        meshCollider.sharedMesh = colliderMeshFilter.sharedMesh;

                        // Add collider object to the destruction list
                        objectsToDestroy.Add(colliderObj);

                        Debug.Log($"Assigned collider mesh from {colliderObj.name} to {obj.name}");
                    }
                }
            }

            // Destroy all collider objects after processing
            foreach (var obj in objectsToDestroy)
            {
                Object.DestroyImmediate(obj);
                Debug.Log($"Removed collider object: {obj.name}");
            }
        }
    }
}