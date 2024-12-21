// Copyright (c) Meta Platforms, Inc. and affiliates.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace NorthStar
{
    [Serializable]
    public class Dialogue
    {
        public TextObject Text;
        [CharacterDropdown] public string CharacterId;
    }
    /// <summary>
    /// Stores all the text assets for a dialouge section
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Dialogue Selection")]
    public class DialogueSection : ScriptableObject
    {
        public PlayableAsset TimelineAsset;
        public List<Dialogue> TextObjects;
    }
}
