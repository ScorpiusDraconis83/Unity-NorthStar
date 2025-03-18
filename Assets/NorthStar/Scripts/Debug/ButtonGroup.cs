// Copyright (c) Meta Platforms, Inc. and affiliates.
using Oculus.Interaction;
using UnityEngine;

namespace NorthStar.DebugUtilities
{
    public class ButtonGroup : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IInteractableView))]
        private Object m_decrementInteractableView;
        private IInteractableView DecrementInteractableView { get; set; }

        [SerializeField, Interface(typeof(IInteractableView))]
        private Object m_incrementInteractableView;
        private IInteractableView IncrementInteractableView { get; set; }

        protected bool m_started = false;

        protected virtual void Awake()
        {
            DecrementInteractableView = m_decrementInteractableView as IInteractableView;
            IncrementInteractableView = m_incrementInteractableView as IInteractableView;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref m_started);

            this.AssertField(DecrementInteractableView, nameof(DecrementInteractableView));
            this.AssertField(IncrementInteractableView, nameof(IncrementInteractableView));

            this.EndStart(ref m_started);
        }

        protected virtual void OnEnable()
        {
            if (m_started)
            {
                DecrementInteractableView.WhenStateChanged += DecrementOnStateChange;
                IncrementInteractableView.WhenStateChanged += IncrementOnStateChange;
            }
        }

        protected virtual void OnDisable()
        {
            if (m_started)
            {
                DecrementInteractableView.WhenStateChanged -= DecrementOnStateChange;
                IncrementInteractableView.WhenStateChanged -= IncrementOnStateChange;
            }
        }

        protected virtual void DecrementOnStateChange(InteractableStateChangeArgs args)
        {
        }

        protected virtual void IncrementOnStateChange(InteractableStateChangeArgs args)
        {
        }
    }
}