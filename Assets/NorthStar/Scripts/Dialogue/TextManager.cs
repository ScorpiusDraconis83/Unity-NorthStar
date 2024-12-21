// Copyright (c) Meta Platforms, Inc. and affiliates.
using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace NorthStar
{
    /// <summary>
    /// Imports and localizes text data
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Text Manager")]
    public class TextManager : ScriptableObject
    {
        #region Singleton
        private const string ASSETPATH = "TextManager";
        private static TextManager s_instance;
        public static TextManager Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = Resources.Load(ASSETPATH) as TextManager;
                return s_instance;
            }
        }
        public static string GetText(TextObject textAsset)
        {
            return Instance.GetTextLocal(textAsset);
        }
        #endregion
        [SerializeField] private bool m_autoGetLanguage;
        public string SelectedLanguage;
        private bool m_loaded = false;
        private Dictionary<string, string> m_textDatabase;
        [SerializeField] private TextAsset m_xmlFile;
        public delegate void OnReloadCallback();
        public OnReloadCallback OnReload;


        [ContextMenu("Load")]
        public void LoadXml()
        {
            if (m_autoGetLanguage)
                SelectedLanguage = GetSystemLanguage();
            var language = SelectedLanguage;
            m_textDatabase = new Dictionary<string, string>();
            var xml = new XmlDocument();
            xml.LoadXml(m_xmlFile.text);
            foreach (XmlNode textData in xml.GetElementsByTagName("TextObject"))
            {
                var id = textData.Attributes["Id"].Value;
                if (m_textDatabase.ContainsKey(id))
                {
                    Debug.LogError($"Multiple Entries of same id: {id}");
                    continue;
                }
                for (var i = 0; i < textData.ChildNodes.Count; i++)
                {
                    var languageOption = textData.ChildNodes[i];
                    if (languageOption.Attributes["Language"].Value == language)
                    {
                        m_textDatabase.Add(id, languageOption.InnerText);
                    }
                }
            }
            m_loaded = true;
            OnReload?.Invoke();
        }

        public List<string> GetLanguageOptionsLocal()
        {
            var list = new List<string>();

            var xml = new XmlDocument();
            xml.LoadXml(m_xmlFile.text);

            var languageOptionsElement = xml.GetElementsByTagName("SupportedLanguages")[0];
            foreach (XmlNode languageOption in languageOptionsElement)
            {
                list.Add(languageOption.Attributes["Value"].Value);
            }
            return list;
        }

        private string GetSystemLanguage()
        {
            var language = Application.systemLanguage.ToString();
            return GetLanguageOptionsLocal().Contains(language) ? language : SelectedLanguage;
        }

        public List<string> GetTextIdsLocal()
        {
            var list = new List<string>();

            var xml = new XmlDocument();
            xml.LoadXml(m_xmlFile.text);

            var languageOptionsElement = xml.GetElementsByTagName("TextObject");
            foreach (XmlNode languageOption in languageOptionsElement)
            {
                list.Add(languageOption.Attributes["Id"].Value);
            }
            return list;
        }
#if UNITY_EDITOR
        [ContextMenu("CreateAssets")]
        public void CreateAssets()
        {
            AssetDatabase.Refresh();
            var assetList = GetTextIdsLocal();
            var parentFolder = "Assets/NorthStar/Data/Dialogue/DialogueText";
            foreach (var asset in assetList)
            {
                var folder = asset.Split('_')[0].Replace("B", "Beat ");
                if (!AssetDatabase.IsValidFolder($"{parentFolder}/" + folder))
                    _ = AssetDatabase.CreateFolder(parentFolder, folder);
                var path = $"{parentFolder}/{folder}/{asset}.asset";
                if (!File.Exists(path))
                    AssetDatabase.CreateAsset(CreateInstance<TextObject>(), path);
            }
            AssetDatabase.SaveAssets();
        }
#endif

        public string GetTextLocal(TextObject textObject)
        {
            if (!m_loaded)
                LoadXml();
            if (m_textDatabase == null)
                LoadXml();
            if (!m_textDatabase.ContainsKey(textObject.name))
            {
                Debug.LogError($"Not finding text object {textObject.name} in database");
                return "_ERROR_";
            }
            return m_textDatabase.ContainsKey(textObject.name) ? m_textDatabase[textObject.name] : "_Empty_";
        }
        private void OnValidate()
        {
            m_loaded = false;
        }
    }
}
