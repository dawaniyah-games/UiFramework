using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.Config
{
    public static class UiEditorConfigUtility
    {
        public const string ConfigAssetGuidPrefKey = "UiFramework.Editor.ConfigAssetGUID";

        // The UiEditorConfig selected in the UI Setup Manager window, or the first one in the project.
        public static UiEditorConfig FindActiveConfig()
        {
            string guid = EditorPrefs.GetString(ConfigAssetGuidPrefKey, null);
            if (!string.IsNullOrEmpty(guid))
            {
                UiEditorConfig saved = AssetDatabase.LoadAssetAtPath<UiEditorConfig>(AssetDatabase.GUIDToAssetPath(guid));
                if (saved != null)
                {
                    return saved;
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:UiEditorConfig");
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<UiEditorConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            return null;
        }

        // Returns config.GlobalCanvasSettings, assigning an existing UiGlobalCanvasSettings asset or creating one with defaults
        // at UiGlobalCanvasSettings.DefaultAssetPath when the field is empty. Returns null only while the AssetDatabase
        // is busy (importing/compiling) and no asset exists yet.
        public static UiGlobalCanvasSettings GetOrCreateCanvasSettings(UiEditorConfig config)
        {
            if (config != null && config.GlobalCanvasSettings != null)
            {
                return config.GlobalCanvasSettings;
            }

            UiGlobalCanvasSettings settings = AssetDatabase.LoadAssetAtPath<UiGlobalCanvasSettings>(UiGlobalCanvasSettings.DefaultAssetPath);

            if (settings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:UiGlobalCanvasSettings");
                if (guids.Length > 0)
                {
                    settings = AssetDatabase.LoadAssetAtPath<UiGlobalCanvasSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (settings == null)
            {
                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    return null;
                }

                EnsureFolderExists(Path.GetDirectoryName(UiGlobalCanvasSettings.DefaultAssetPath));
                settings = ScriptableObject.CreateInstance<UiGlobalCanvasSettings>();
                AssetDatabase.CreateAsset(settings, UiGlobalCanvasSettings.DefaultAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[UiFramework] Created UiGlobalCanvasSettings with default values at " + UiGlobalCanvasSettings.DefaultAssetPath);
            }

            if (config != null)
            {
                config.GlobalCanvasSettings = settings;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        // Like GetOrCreateCanvasSettings, but falls back to an unsaved instance with default values instead of null.
        public static UiGlobalCanvasSettings GetCanvasSettingsOrDefaults(UiEditorConfig config)
        {
            UiGlobalCanvasSettings settings = GetOrCreateCanvasSettings(config);
            if (settings != null)
            {
                return settings;
            }

            UiGlobalCanvasSettings transientSettings = ScriptableObject.CreateInstance<UiGlobalCanvasSettings>();
            transientSettings.hideFlags = HideFlags.DontSave;
            return transientSettings;
        }

        public static void EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            string normalized = folderPath.Replace("\\", "/").TrimEnd('/');
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
