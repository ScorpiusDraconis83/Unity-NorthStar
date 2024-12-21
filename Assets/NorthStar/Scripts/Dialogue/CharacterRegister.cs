// Copyright (c) Meta Platforms, Inc. and affiliates.
using System.Collections.Generic;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Stores all the characters to make finding them in game easier
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Character Register")]
    public class CharacterRegister : ScriptableObject
    {
        private static CharacterRegister s_instance;

        public static CharacterRegister Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = Resources.Load("CharacterRegister") as CharacterRegister;
                return s_instance;
            }
        }

        public List<string> CharacterNames = new();
    }
}
