// Copyright (c) Meta Platforms, Inc. and affiliates.
using Meta.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace NorthStar
{
    /// <summary>
    /// Handles playing subtitles and timelines through unity events
    /// </summary>
    public class DialoguePlayer : MonoBehaviour
    {
        [SerializeField] private DialogueSection m_dialogueSection;
        [SerializeField, AutoSet] private PlayableDirector m_playableDirector;
        public UnityEvent OnCompleteSection;

        private int m_dialogueIndex = 0;

        private CharacterManager CharacterManager => CharacterManager.Instance;

        private void Awake()
        {
            LoadSection(m_dialogueSection);
        }
        public void AdvanceDialogue()
        {
            if (m_dialogueIndex >= m_dialogueSection.TextObjects.Count)
            {
                OnCompleteSection.Invoke();
                m_dialogueIndex = 0;
                return;
            }
            CharacterManager.PlayDialogue(m_dialogueSection.TextObjects[m_dialogueIndex].CharacterId, m_dialogueSection.TextObjects[m_dialogueIndex].Text);
            m_dialogueIndex++;
        }

        public void LoadSection(DialogueSection dialogueSection)
        {
            m_dialogueSection = dialogueSection;
            m_dialogueIndex = 0;
        }
        public void Play()
        {
            m_playableDirector.Play();
        }
    }
}
