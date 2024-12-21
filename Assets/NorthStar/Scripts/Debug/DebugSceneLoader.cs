// Copyright (c) Meta Platforms, Inc. and affiliates.
using Oculus.Interaction;
using UnityEngine.SceneManagement;

namespace NorthStar.DebugUtilities
{
    public class DebugSceneLoader : ButtonGroup
    {
        protected override void DecrementOnStateChange(InteractableStateChangeArgs args)
        {
            //If button pressed
            if (args.NewState == InteractableState.Select)
            {
                var currentIndex = SceneManager.GetActiveScene().buildIndex;
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = SceneManager.sceneCountInBuildSettings - 1;
                }

                _ = SceneManager.LoadSceneAsync(currentIndex);
            }
        }

        protected override void IncrementOnStateChange(InteractableStateChangeArgs args)
        {
            //If button pressed
            if (args.NewState == InteractableState.Select)
            {
                var currentIndex = SceneManager.GetActiveScene().buildIndex;
                currentIndex++;
                if (currentIndex >= SceneManager.sceneCountInBuildSettings)
                {
                    currentIndex = 0;
                }

                _ = SceneManager.LoadSceneAsync(currentIndex);
            }
        }
    }
}