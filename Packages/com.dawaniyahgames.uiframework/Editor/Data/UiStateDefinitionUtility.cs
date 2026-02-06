using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.Data
{
    public static class UiStateDefinitionUtility
    {
        public static List<UiStateDefinition> FindAll(string folderPath)
        {
            List<UiStateDefinition> results = new List<UiStateDefinition>();

            string normalizedFolder = NormalizeFolderPath(folderPath);
            string[] searchFolders = null;

            if (!string.IsNullOrEmpty(normalizedFolder) && AssetDatabase.IsValidFolder(normalizedFolder))
            {
                searchFolders = new string[] { normalizedFolder };
            }

            string[] guids = searchFolders != null
                ? AssetDatabase.FindAssets("t:UiStateDefinition", searchFolders)
                : AssetDatabase.FindAssets("t:UiStateDefinition");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                UiStateDefinition asset = AssetDatabase.LoadAssetAtPath<UiStateDefinition>(path);
                if (asset != null)
                {
                    results.Add(asset);
                }
            }

            results.Sort(CompareByKey);
            return results;
        }

        public static UiStateDefinition CreateNew(string folderPath, string stateKey)
        {
            if (string.IsNullOrWhiteSpace(stateKey))
            {
                return null;
            }

            string normalizedFolder = NormalizeFolderPath(folderPath);
            if (string.IsNullOrEmpty(normalizedFolder))
            {
                normalizedFolder = "Assets/UiConfigs/UiStateDefinitions";
            }

            EnsureUnityFolderExists(normalizedFolder);

            string safeName = stateKey.Trim();
            string assetPath = normalizedFolder.TrimEnd('/') + "/" + safeName + ".asset";

            UiStateDefinition existing = AssetDatabase.LoadAssetAtPath<UiStateDefinition>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            UiStateDefinition created = ScriptableObject.CreateInstance<UiStateDefinition>();
            created.StateKey = safeName;

            AssetDatabase.CreateAsset(created, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return created;
        }

        public static bool TryResolveScenePath(string sceneName, out string scenePath)
        {
            scenePath = null;

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            string trimmedName = sceneName.Trim();

            string[] guids = AssetDatabase.FindAssets(trimmedName + " t:Scene");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Path.GetFileNameWithoutExtension(path) == trimmedName)
                {
                    scenePath = path;
                    return true;
                }
            }

            return false;
        }

        private static int CompareByKey(UiStateDefinition a, UiStateDefinition b)
        {
            string aKey = a != null ? a.StateKey : string.Empty;
            string bKey = b != null ? b.StateKey : string.Empty;
            return string.Compare(aKey, bKey, StringComparison.Ordinal);
        }

        private static string NormalizeFolderPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return string.Empty;
            }

            string normalized = folderPath.Replace("\\", "/").Trim();

            while (normalized.EndsWith("/", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }

            return normalized;
        }

        private static void EnsureUnityFolderExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            string normalized = folderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string[] parts = normalized.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
