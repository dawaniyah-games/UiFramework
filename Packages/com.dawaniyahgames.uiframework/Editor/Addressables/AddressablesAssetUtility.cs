using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
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

        // Creates the project's default Addressables settings (Assets/AddressableAssetsData) when there are none yet.
        public static AddressableAssetSettings GetOrCreateSettings()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                return settings;
            }

            settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[UiFramework] Could not create the Addressables settings. Create them in Window > Asset Management > Addressables > Groups, then build the runtime UiConfig again.");
                return null;
            }

            Debug.Log("[UiFramework] Created the project's Addressables settings in " + AddressableAssetSettingsDefaultObject.kDefaultConfigFolder + ", so the runtime UiConfig and element scenes can be loaded.");
            return settings;
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
                Debug.LogWarning("[UiFramework] Addressables settings not found, so nothing was changed. Create them in Window > Asset Management > Addressables > Groups.");
                return;
            }

            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("[UiFramework] Cannot make an asset Addressable: its path is empty.");
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError("[UiFramework] No asset found at " + assetPath + ", so it was not made Addressable. Check that the path exists and has been imported.");
                return;
            }

            AddressableAssetGroup group = GetOrCreateGroup(settings, groupName);
            if (group == null)
            {
                Debug.LogError("[UiFramework] Could not find or create the Addressables group '" + groupName + "'. Check the Addressables Groups window.");
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
                Debug.LogWarning("[UiFramework] Addressables settings not found, so nothing was changed. Create them in Window > Asset Management > Addressables > Groups.");
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

        // Returns the group, creating it with the build settings of the project's default group when it doesn't exist.
        public static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
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
            if (group == null)
            {
                List<AddressableAssetGroupSchema> schemasToCopy = settings.DefaultGroup != null ? settings.DefaultGroup.Schemas : null;
                group = settings.CreateGroup(groupName, false, false, true, schemasToCopy);
                Debug.Log("[UiFramework] Created Addressables group '" + groupName + "'.");
            }

            EnsureBuildSchemas(settings, group);
            return group;
        }

        // A group without a BundledAssetGroupSchema is left out of the Addressables build, so its assets load in the
        // editor but not in a player. Versions before 1.3.14 created the UiFramework groups that way; this repairs them.
        public static void EnsureBuildSchemas(AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            if (settings == null || group == null || group.ReadOnly)
            {
                return;
            }

            bool changed = false;

            if (!group.HasSchema<BundledAssetGroupSchema>())
            {
                AddSchemaLikeDefaultGroup<BundledAssetGroupSchema>(settings, group);
                changed = true;
            }

            if (!group.HasSchema<ContentUpdateGroupSchema>())
            {
                AddSchemaLikeDefaultGroup<ContentUpdateGroupSchema>(settings, group);
                changed = true;
            }

            if (changed)
            {
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.GroupSchemaAdded, group, true);
                AssetDatabase.SaveAssets();
                Debug.Log("[UiFramework] Added the build settings to the Addressables group '" + group.Name + "', so its assets are included in player builds.");
            }
        }

        private static void AddSchemaLikeDefaultGroup<TSchema>(AddressableAssetSettings settings, AddressableAssetGroup group) where TSchema : AddressableAssetGroupSchema
        {
            TSchema source = settings.DefaultGroup != null && settings.DefaultGroup != group ? settings.DefaultGroup.GetSchema<TSchema>() : null;
            if (source != null)
            {
                group.AddSchema(source, false);
            }
            else
            {
                group.AddSchema<TSchema>(false);
            }
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
