// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    public class TeleportHotspotCollection : MonoBehaviour
    {
        [SerializeField] private Transform m_playerHead;
        public List<TeleportWaypoint> Targets;
        private void Start()
        {
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out TeleportWaypoint target))
                {
                    Targets.Add(target);
                    target.OnHide();
                }
            }
        }

        private void Update()
        {
            var sqrMinDistance = GlobalSettings.PlayerSettings.TeleporterToCloseDespawnDistance * GlobalSettings.PlayerSettings.TeleporterToCloseDespawnDistance;
            foreach (var target in Targets)
            {
                var toPlayer = target.transform.position - m_playerHead.position;
                if (toPlayer.sqrMagnitude < sqrMinDistance)
                {
                    target.gameObject.SetActive(false);
                }
                else
                {
                    target.gameObject.SetActive(true);
                }
            }
        }

        public void Show()
        {
            foreach (var target in Targets)
            {
                target.OnShow();
            }
        }

        public void Hide()
        {
            foreach (var target in Targets)
            {
                target.OnHide();
            }
        }
    }
}
