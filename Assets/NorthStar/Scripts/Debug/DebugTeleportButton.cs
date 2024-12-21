// Copyright (c) Meta Platforms, Inc. and affiliates.
using Oculus.Interaction;
using UnityEngine;

namespace NorthStar.DebugUtilities
{
    public class DebugTeleportButton : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IInteractableView))]
        private Object _interactableView;
        private IInteractableView InteractableView { get; set; }

        private bool m_started;

        [SerializeField]
        private Transform m_cameraTransform;
        [SerializeField]
        private Transform m_bodyTransform;

        protected virtual void Awake()
        {
            InteractableView = _interactableView as IInteractableView;
        }

        protected virtual void Start()
        {
            this.BeginStart(ref m_started);

            this.AssertField(InteractableView, nameof(InteractableView));

            this.EndStart(ref m_started);
        }

        protected virtual void OnEnable()
        {
            if (m_started)
            {
                InteractableView.WhenStateChanged += OnStateChange;
            }
        }

        protected virtual void OnDisable()
        {
            if (m_started)
            {
                InteractableView.WhenStateChanged -= OnStateChange;
            }
        }

        private void OnStateChange(InteractableStateChangeArgs args)
        {
            //If button pressed
            if (args.NewState == InteractableState.Select)
            {
                m_bodyTransform.position += m_cameraTransform.forward;
            }
        }
    }
}