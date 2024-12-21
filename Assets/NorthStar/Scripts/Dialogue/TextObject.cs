// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// References a section of text in the localisation file
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Text Object")]
    public class TextObject : ScriptableObject
    {
        public string Text => TextManager.GetText(this);

        public void ForceReload()
        {
            TextManager.Instance.LoadXml();
        }
    }
}
