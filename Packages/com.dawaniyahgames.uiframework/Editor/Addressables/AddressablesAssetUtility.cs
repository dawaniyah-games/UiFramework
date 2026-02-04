using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace UiFramework.Editor.Addressables
{
    public static class AddressablesAssetUtility
    {
        public static bool TryGetSettings(out AddressableAssetSettings settings)
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            return settings != null;
        }

        public static List<string> GetGroupNames(AddressableAssetSettings settings)
        {
            List<string> names = new List<string>();
            if (settings == null)
            {
                return names;
            }

            for (int i = 0; i < settings.groups.Count; i++)
            {
                AddressableAssetGroup group = settings.groups[i];
                if (group == null)
                {
                    continue;
                }

                names.Add(group.Name);
            }

            names.Sort(StringComparer.Ordinal);
            return names;
        }

        public static bool IsAssetAddressable(string assetPath, out AddressableAssetEntry entry)
        {
            entry = null;

            AddressableAssetSettings settings;
            if (!TryGetSettings(out settings))
            {
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            entry = settings.FindAssetEntry(guid);
            return entry != null;
        }

        public static void EnsureAssetIsAddressable(string assetPath, string groupName, string address, string[] labels)
        {
            AddressableAssetSettings settings;
            if (!TryGetSettings(out settings))
            {
                Debug.LogWarning("⚠️ Addressables settings not found. Please enable Addressables in your project.");
                return;
            }

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("❌ Asset path is empty.");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError("❌ Could not resolve GUID for asset at " + assetPath);
                return;
            }

            AddressableAssetGroup group = GetOrCreateGroup(settings, groupName);
            if (group == null)
            {
                Debug.LogError("❌ Failed to create/find Addressables group '" + groupName + "'.");
                return;
            }

            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            if (!string.IsNullOrEmpty(address))
            {
                entry.address = address;
            }

            EnsureLabels(settings, entry, labels);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
            AssetDatabase.SaveAssets();
        }

        public static void RemoveAssetFromAddressables(string assetPath)
        {
            AddressableAssetSettings settings;
            if (!TryGetSettings(out settings))
            {
                Debug.LogWarning("⚠️ Addressables settings not found. Please enable Addressables in your project.");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                return;
            }

            settings.RemoveAssetEntry(guid, true);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryRemoved, entry, true);
            AssetDatabase.SaveAssets();
        }

        public static string[] ParseLabels(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return new string[0];
            }

            string[] parts = csv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> labels = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                string label = parts[i] != null ? parts[i].Trim() : string.Empty;
                if (string.IsNullOrEmpty(label))
                {
                    continue;
                }

                labels.Add(label);
            }

            return labels.ToArray();
        }

        public static string DefaultAddressForAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return string.Empty;
            }

            return Path.GetFileNameWithoutExtension(assetPath);
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            if (settings == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(groupName))
            {
                return settings.DefaultGroup;
            }

            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group != null)
            {
                return group;
            }

            group = settings.CreateGroup(groupName, false, false, true, null);
            Debug.Log("🆕 Created Addressables group '" + groupName + "'.");
            return group;
        }

        private static void EnsureLabels(AddressableAssetSettings settings, AddressableAssetEntry entry, string[] labels)
        {
            if (settings == null || entry == null || labels == null)
            {
                return;
            }

            IList<string> existingLabels = settings.GetLabels();

            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                if (string.IsNullOrEmpty(label))
                {
                    continue;
                }

                if (existingLabels == null || !existingLabels.Contains(label))
                {
                    settings.AddLabel(label);
                }

                if (!entry.labels.Contains(label))
                {
                    entry.SetLabel(label, true, true);
                }
            }
        }
    }
}
