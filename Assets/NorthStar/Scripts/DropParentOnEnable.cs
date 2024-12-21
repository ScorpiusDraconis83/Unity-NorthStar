// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Removes an objects parent when its enabled after the boat syncs its movement
    /// </summary>
    public class DropParentOnEnable : MonoBehaviour
    {
        private void Drop()
        {
            transform.parent = null;
        }
        private void OnEnable()
        {
            BoatController.Instance.MovementSource.OnSync += Drop;
        }

        private void OnDisable()
        {
            BoatController.Instance.MovementSource.OnSync -= Drop;
        }
    }
}
