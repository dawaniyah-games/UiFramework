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

        // Returns config.CanvasSettings, assigning an existing UiCanvasSettings asset or creating one with defaults
        // at UiCanvasSettings.DefaultAssetPath when the field is empty. Returns null only while the AssetDatabase
        // is busy (importing/compiling) and no asset exists yet.
        public static UiCanvasSettings GetOrCreateCanvasSettings(UiEditorConfig config)
        {
            if (config != null && config.CanvasSettings != null)
            {
                return config.CanvasSettings;
            }

            UiCanvasSettings settings = AssetDatabase.LoadAssetAtPath<UiCanvasSettings>(UiCanvasSettings.DefaultAssetPath);

            if (settings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:UiCanvasSettings");
                if (guids.Length > 0)
                {
                    settings = AssetDatabase.LoadAssetAtPath<UiCanvasSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }

            if (settings == null)
            {
                if (EditorApplication.isUpdating || EditorApplication.isCompiling)
                {
                    return null;
                }

                EnsureFolderExists(Path.GetDirectoryName(UiCanvasSettings.DefaultAssetPath));
                settings = ScriptableObject.CreateInstance<UiCanvasSettings>();
                AssetDatabase.CreateAsset(settings, UiCanvasSettings.DefaultAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[UiFramework] Created UiCanvasSettings with default values at " + UiCanvasSettings.DefaultAssetPath);
            }

            if (config != null)
            {
                config.CanvasSettings = settings;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            return settings;
        }

        // Like GetOrCreateCanvasSettings, but falls back to an unsaved instance with default values instead of null.
        public static UiCanvasSettings GetCanvasSettingsOrDefaults(UiEditorConfig config)
        {
            UiCanvasSettings settings = GetOrCreateCanvasSettings(config);
            if (settings != null)
            {
                return settings;
            }

            UiCanvasSettings transientSettings = ScriptableObject.CreateInstance<UiCanvasSettings>();
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
