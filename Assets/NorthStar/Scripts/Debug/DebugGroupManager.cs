// Copyright (c) Meta Platforms, Inc. and affiliates.
using Oculus.Interaction;
using UnityEngine;

namespace NorthStar.DebugUtilities
{
    public class DebugGroupManager : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IInteractableView))]
        private UnityEngine.Object m_decrementInteractableView;
        private IInteractableView m_DecrementInteractableView { get; set; }

        [SerializeField, Interface(typeof(IInteractableView))]
        private UnityEngine.Object m_incrementInteractableView;
        private IInteractableView m_IncrementInteractableView { get; set; }

        protected bool m_started = false;

        protected virtual void Awake()
        {
            m_DecrementInteractableView = m_decrementInteractableView as IInteractableView;
            m_IncrementInteractableView = m_incrementInteractableView as IInteractableView;

            //Disable all groups barring the first
            for (var i = 1; i < m_groups.Length; i++)
            {
                m_groups[i].SetActive(false);
            }
        }

        protected virtual void Start()
        {
            this.BeginStart(ref m_started);

            this.AssertField(m_DecrementInteractableView, nameof(m_DecrementInteractableView));
            this.AssertField(m_IncrementInteractableView, nameof(m_IncrementInteractableView));

            this.EndStart(ref m_started);
        }

        protected virtual void OnEnable()
        {
            if (m_started)
            {
                m_DecrementInteractableView.WhenStateChanged += DecrementOnStateChange;
                m_IncrementInteractableView.WhenStateChanged += IncrementOnStateChange;
            }
        }

        protected virtual void OnDisable()
        {
            if (m_started)
            {
                m_DecrementInteractableView.WhenStateChanged -= DecrementOnStateChange;
                m_IncrementInteractableView.WhenStateChanged -= IncrementOnStateChange;
            }
        }

        [SerializeField]
        private GameObject[] m_groups;

        private int m_groupIndex = 0;

        private void DecrementOnStateChange(InteractableStateChangeArgs args)
        {
            //If button pressed
            if (args.NewState == InteractableState.Select)
            {
                m_groups[m_groupIndex].SetActive(false);

                //Wrap index
                m_groupIndex--;
                if (m_groupIndex < 0)
                {
                    m_groupIndex = m_groups.Length - 1;
                }

                m_groups[m_groupIndex].SetActive(true);
            }
        }

        private void IncrementOnStateChange(InteractableStateChangeArgs args)
        {
            //If button pressed
            if (args.NewState == InteractableState.Select)
            {
                m_groups[m_groupIndex].SetActive(false);

                //Wrap index
                m_groupIndex++;
                if (m_groupIndex >= m_groups.Length)
                {
                    m_groupIndex = 0;
                }

                m_groups[m_groupIndex].SetActive(true);
            }
        }
    }
}