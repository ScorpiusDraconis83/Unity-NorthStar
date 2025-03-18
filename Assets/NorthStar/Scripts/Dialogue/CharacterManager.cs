// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Handles telling dialouge to play on the correct character
    /// </summary>
    public class CharacterManager
    {
        public static CharacterManager Instance { get; } = new();

        private Dictionary<string, Subtitle> m_keySubtitlePairs = new();

        public void RegisterSubtitleObject(Subtitle subtitle)
        {
            m_keySubtitlePairs[subtitle.Id] = subtitle;
        }

        public void DeRegisterSubtitleObject(Subtitle subtitle)
        {
            if (m_keySubtitlePairs.TryGetValue(subtitle.Id, out var value) && value == subtitle)
            {
                _ = m_keySubtitlePairs.Remove(subtitle.Id);
            }
        }

        public void PlayDialogue(string id, TextObject textObject)
        {
            if (m_keySubtitlePairs.TryGetValue(id, out var subtitle) && subtitle != null)
            {
                subtitle.DisplayText(textObject);
            }
            else
            {
                Debug.Log("No Subtitle object for " + id);
            }
        }
    }
}
