// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    public class LookAtBehaviour : PlayableBehaviour
    {
        public NpcRigController NpcRigController;
        [Range(0, 1)]
        public float Weight = 1;
        public NpcRigController.IKRig Rig = NpcRigController.IKRig.Spine;
    }
}
