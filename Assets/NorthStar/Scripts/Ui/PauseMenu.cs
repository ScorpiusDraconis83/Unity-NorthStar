// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controller for the pause menu
    /// </summary>
    public class PauseMenu : UiMenu
    {
        public void ExitGame()
        {
            Application.Quit();
        }

        public void LoadFreeSail()
        {
            LoadScreen.Instance.Load("FreeSailing", false);
        }

        public void LoadStory()
        {
            LoadScreen.Instance.Load("Beat1", true);
        }

    }
}
