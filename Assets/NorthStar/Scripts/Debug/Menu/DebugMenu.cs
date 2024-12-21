// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Debug menu UI, creates the visual interface for the debug system
    /// </summary>
    public class DebugMenu : MonoBehaviour
    {
        [SerializeField] private Transform m_contentRoot;
        [SerializeField] private TextMeshProUGUI m_categoryLabel;

        private int m_categoryIndex;
        private bool m_needsRebuild;

        private const string CATEGORIES_PREFAB_NAME = "DebugMenuCategory";

        private void OnEnable()
        {
            // Listen rebuild events (triggered when the debug menu options change)
            DebugSystem.Instance.OnNeedsRebuild += OnNeedsRebuild;
            Rebuild();
            UpdatePages();
        }

        private void OnDisable()
        {
            if (DebugSystem.Instance)
                DebugSystem.Instance.OnNeedsRebuild -= OnNeedsRebuild;
        }

        private void Update()
        {
            if (m_needsRebuild)
            {
                Rebuild();
                UpdatePages();
                m_needsRebuild = false;
            }
        }

        private void OnNeedsRebuild()
        {
            m_needsRebuild = true;
        }

        /// <summary>
        /// Cycle to the next debug page
        /// </summary>
        public void NextPage()
        {
            if (m_contentRoot.childCount == 0) return;
            m_categoryIndex = (m_categoryIndex + 1) % DebugSystem.Instance.Categories.Count;
            UpdatePages();
        }

        /// <summary>
        /// Cycle to the previous debug page
        /// </summary>
        public void PreviousPage()
        {
            if (m_contentRoot.childCount == 0) return;
            m_categoryIndex = (m_categoryIndex - 1) < 0 ? DebugSystem.Instance.Categories.Count - 1 : m_categoryIndex - 1;
            UpdatePages();
        }

        private void UpdatePages()
        {
            if (DebugSystem.Instance.Categories.Count > 0)
            {
                var i = 0;
                foreach (var category in DebugSystem.Instance.Categories)
                {
                    if (category.Content != null)
                    {
                        category.Content.SetActive(i == m_categoryIndex);
                        i++;
                    }
                }
                m_categoryLabel.text = DebugSystem.Instance.Categories[m_categoryIndex].Name;
            }
        }

        /// <summary>
        /// Fully rebuild all UI components using data from the active DebugSystem
        /// </summary>
        private void Rebuild()
        {
            foreach (var category in DebugSystem.Instance.Categories)
            {
                if (category.Content) Destroy(category.Content);

                if (category.Prefab)
                {
                    var categoryPage = Instantiate(category.Prefab, m_contentRoot);
                    category.Content = categoryPage;
                }
                else
                {
                    var categoryPage = Instantiate(Resources.Load<GameObject>(CATEGORIES_PREFAB_NAME), m_contentRoot);
                    category.Content = categoryPage;

                    foreach (var property in category.Properties)
                    {
                        var instance = Instantiate(Resources.Load<GameObject>(property.PrefabName), categoryPage.transform);
                        property.ConnectToPrefab(instance);
                    }
                }
            }
        }

    }
}
