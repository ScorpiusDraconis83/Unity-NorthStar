// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    public static class Extensions
    {
        /// <summary>
        /// Checks recursively if the transform is the parent of the target gameobject
        /// </summary>
        /// <param name="transform">The transform of the potential parent</param>
        /// <param name="target">The gameobject of the potential child</param>
        /// <returns>A boolean stating if the target is a child of the transform</returns>
        public static bool IsParentOf(this Transform transform, GameObject target)
        {
            if (transform.gameObject == target)
                return true;

            var found = false;
            for (var i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).IsParentOf(target))
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        /// <summary>
        /// Checks recursively if the array of gameobjects contains a parent of the target gameobject
        /// </summary>
        /// <param name="gameObjects">An array of potential parent gameobjects</param>
        /// <param name="target">A potential child gameobject</param>
        /// <returns>A boolean stating if the target is a child of one of the gameobjects in the array</returns>
        public static bool IsParentOf(this GameObject[] gameObjects, GameObject target)
        {
            foreach (var obj in gameObjects)
            {
                if (obj.transform.IsParentOf(target))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
