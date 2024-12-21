// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Handles telling dialouge to play on the correct character
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        private Dictionary<string, Subtitle> m_keySubtitlePairs = new();

        public void RegisterSubtitleObject(Subtitle subtitle)
        {
            if (m_keySubtitlePairs.ContainsKey(subtitle.Id))
            {
                m_keySubtitlePairs[subtitle.Id] = subtitle;
            }
            else
            {
                m_keySubtitlePairs.Add(subtitle.Id, subtitle);
            }
        }

        public void DeRegisterSubtitleObject(Subtitle subtitle)
        {
            if (m_keySubtitlePairs.ContainsKey(subtitle.Id))
            {
                if (m_keySubtitlePairs[subtitle.Id] == subtitle)
                {
                    _ = m_keySubtitlePairs.Remove(subtitle.Id);
                }
            }
        }

        public void PlayDialogue(string id, TextObject textObject)
        {
            if (m_keySubtitlePairs.ContainsKey(id))
            {
                m_keySubtitlePairs[id].DisplayText(textObject);
            }
            else
            {
                Debug.Log("No Subtitle object for " + id);
            }
        }
    }
}
