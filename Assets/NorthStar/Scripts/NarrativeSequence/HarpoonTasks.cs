// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// A task condition that checks if targets have been struck by the harpoon
    /// </summary>
    public class HitTargets : TaskCondition
    {
        [SerializeField] private HarpoonTarget[] m_targets = new HarpoonTarget[0];
        private enum AndOr
        {
            And, OR
        }
        [SerializeField] private AndOr m_andOr;
        public override string Description => $"Hit {(m_targets.Length > 1 ? "All targets" : "Target")} with a harpoon";
        public override bool IsComplete(TaskHandler handler)
        {
            var hitAll = true;
            foreach (var target in m_targets)
            {
                if (!target.HasBeenHit)
                {
                    hitAll = false;
                }
                else if (m_andOr == AndOr.OR)
                {
                    return true;
                }
            }
            return hitAll;
        }
    }

    /// <summary>
    /// A task condition that checks if targets have been reeled in by the harpoon
    /// </summary>
    public class ReeledTargets : TaskCondition
    {
        [SerializeField] private HarpoonTarget[] m_targets = new HarpoonTarget[0];
        private enum AndOr
        {
            And, OR
        }
        [SerializeField] private AndOr m_andOr;
        public override string Description => $"Reel {(m_targets.Length > 1 ? "All targets" : "Target")} with a harpoon";
        public override bool IsComplete(TaskHandler handler)
        {
            var reelAll = true;
            foreach (var target in m_targets)
            {
                if (!target.HasBeenReeled)
                {
                    reelAll = false;
                }
                else if (m_andOr == AndOr.OR)
                {
                    return true;
                }
            }
            return reelAll;
        }
    }

    /// <summary>
    /// A task condition that checks if a target object is being aimed at by the harpoon
    /// </summary>
    public class ObjectAimedAt : TaskCondition
    {
        [SerializeField] private Transform m_object;
        [SerializeField] private Transform m_target;
        [SerializeField, Range(0f, 1f)] private float m_threshold;
        [SerializeField] private bool m_objectInBoatSpace;
        [SerializeField] private bool m_targetInBoatSpace;
        public override string Description => "Object points somewhat towards target";
        public override bool IsComplete(TaskHandler handler)
        {
            var objectPos = GetPos(m_object, m_objectInBoatSpace);
            var targetPos = GetPos(m_target, m_targetInBoatSpace);

            var toTarget = (targetPos - objectPos).normalized;
            return Vector3.Dot(toTarget, GetRotation(m_object, m_objectInBoatSpace) * Vector3.forward) >= m_threshold;
        }

        private Vector3 GetPos(Transform target, bool inBoatSpace)
        {
            if (!inBoatSpace)
                return target.position;
            var pos = BoatController.WorldToBoatSpace(target.position);
            return pos;
        }

        private Quaternion GetRotation(Transform target, bool inBoatSpace)
        {
            if (!inBoatSpace)
                return target.rotation;
            var rot = BoatController.WorldToBoatSpace(target.rotation);
            return rot;
        }
    }

    /// <summary>
    /// A task condition that waits until a particular event broadcaster is triggered
    /// </summary>
    [Serializable]
    public class WaitForEventBroadcastCondition : TaskCondition
    {
        [Dropdown(typeof(EventBroadcaster), "m_id")] public string Id;

        private bool m_completed;

        public override string Description => Id == "" ? "Listen for defined event broadcast" : $"Listen for {Id} event";


        public override bool IsComplete(TaskHandler handler) => m_completed;


        public override void OnHandlerDestroy(TaskHandler handler)
        {
            UnSubscribe();
        }

        public override void OnTaskStarted(TaskHandler handler)
        {
            m_completed = false;
        }

        public override void OnHandlerStart(TaskHandler handler)
        {
            ResolveConnection();
        }

        private void ResolveConnection()
        {
            if (!EventBroadcaster.Events.ContainsKey(Id))
            {
                EventBroadcaster.Events.Add(Id, null);
            }
            EventBroadcaster.Events[Id] += EventHandler;
        }

        private void UnSubscribe()
        {
            if (EventBroadcaster.Events.TryGetValue(Id, out var action))
            {
                action -= EventHandler;
            }
        }

        private void EventHandler()
        {
            m_completed = true;

            UnSubscribe();

            if (TaskManager.DebugLogs)
            {
                Debug.Log("WaitForEventCondition.EventHandler(); "
                          + "condition is now completed; event link unsubscribed");
            }
        }
    }

    /// <summary>
    /// A task condition that checks if a rope has been spooled
    /// </summary>
    public class RopeIsSpooledTask : TaskCondition
    {
        [SerializeField] private RopeSystem m_ropeSystem;
        [SerializeField, Range(0, 1)] private float m_min = 0;
        [SerializeField, Range(0, 1)] private float m_max = 1;

        public override bool IsComplete(TaskHandler handler)
        {
            var spooled = m_ropeSystem.TotalAmountSpooled;
            return spooled >= m_min && spooled <= m_max;
        }
    }

    /// <summary>
    /// A task condition that checks if a rope has been tied
    /// </summary>
    public class RopeIsTiedTask : TaskCondition
    {
        [SerializeField] private RopeSystem m_ropeSystem;

        public override bool IsComplete(TaskHandler handler) => m_ropeSystem.Tied;
    }
}
