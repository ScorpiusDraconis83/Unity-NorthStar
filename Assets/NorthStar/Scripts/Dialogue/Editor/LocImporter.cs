// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEditor;
using UnityEngine;

namespace NorthStar
{
    public class LocImporter : EditorWindow
    {

        private DefaultAsset m_pyFile;
        private TextAsset m_xmlFile;
        private DefaultAsset m_exelFile;


        [MenuItem("Tools/Loc Importer")]
        public static void ShowWindow()
        {
            _ = GetWindow(typeof(LocImporter));
        }

        private void OnGUI()
        {
            if (m_pyFile == null)
            {
                var path = "";
                foreach (var i in AssetDatabase.FindAssets("spreadsheetExporter"))
                {
                    path = AssetDatabase.GUIDToAssetPath(i);
                }

                var pyFile = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
                if (pyFile != null)
                {
                    m_pyFile = pyFile;
                }
            }

            EditorGUI.BeginDisabledGroup(true);
            m_pyFile = EditorGUILayout.ObjectField(m_pyFile, typeof(DefaultAsset), true) as DefaultAsset;
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Install Requirements"))
                InstallRequirements();
            if (m_pyFile == null)
            {
                GUILayout.Label("Error no python file");
                return;
            }

            m_exelFile = EditorGUILayout.ObjectField(m_exelFile, typeof(DefaultAsset), true) as DefaultAsset;
            m_xmlFile = EditorGUILayout.ObjectField(m_xmlFile, typeof(TextAsset), true) as TextAsset;

            if (m_xmlFile != null && m_exelFile != null)
            {
                if (GUILayout.Button("Run"))
                    Run();
            }
        }

        private void Run()
        {
            var dataPath = Application.dataPath.Replace("Assets", "");
            var args = $"{dataPath + AssetDatabase.GetAssetPath(m_pyFile)} \"{dataPath + AssetDatabase.GetAssetPath(m_exelFile)}\" \"{dataPath + AssetDatabase.GetAssetPath(m_xmlFile)}\"";
            Debug.Log(args);
            var process = System.Diagnostics.Process.Start("python", args);
            process.WaitForExit();
        }

        private void InstallRequirements()
        {
            _ = System.Diagnostics.Process.Start("pip", "install openpyxl");
            _ = System.Diagnostics.Process.Start("pip", "install argparse");
        }

    }
}
