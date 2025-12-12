using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UiFramework.Editor.Config;
using UiFramework.Editor.Data;
using UiFramework.Editor.Window.Tabs;
using UnityEngine.AddressableAssets;
using UiFramework.Runtime;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace UiFramework.Editor.Window
{
    public class UiSetupEditorWindow : EditorWindow
    {
        private const string configKey = "UiFramework.Editor.ConfigAssetGUID";

        private UiEditorConfig configAsset;
        private ObjectField configField;
        private VisualElement tabContent;
        private Dictionary<IVisualElementTab, Button> tabButtonMap;

        private readonly List<IVisualElementTab> tabs = new()
        {
            new GeneralTab(),
            new ElementsTab(),
            new StatesTab()
        };

        [MenuItem("UiFramework/UI Setup Manager")]
        public static void ShowWindow()
        {
            UiSetupEditorWindow window = GetWindow<UiSetupEditorWindow>(false, "UI Setup Manager", true);
            window.minSize = new Vector2(800, 500);
        }

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            root.style.flexDirection = FlexDirection.Column;

            // Find stylesheet by name to avoid hardcoded paths
            StyleSheet styleSheet = null;
            string[] guids = AssetDatabase.FindAssets("UiSetupTabs t:StyleSheet");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith(".uss"))
                {
                    styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                    if (styleSheet != null)
                    {
                        break;
                    }
                }
            }

            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError("❌ Stylesheet 'UiSetupTabs.uss' not found in Packages or Assets.");
            }

            configField = new ObjectField("UI Config")
            {
                objectType = typeof(UiEditorConfig),
                allowSceneObjects = false
            };

            configField.RegisterValueChangedCallback(evt =>
            {
                configAsset = evt.newValue as UiEditorConfig;

                if (configAsset != null)
                {
                    SaveConfigAsset(configAsset);
                    SwitchTab(tabs[0], tabButtonMap);
                    ReloadUiStateRegistry();
                }
            });

            root.Add(configField);

            VisualElement tabsHeader = new VisualElement();
            tabsHeader.AddToClassList("tabs-header");

            tabContent = new VisualElement();
            tabContent.AddToClassList("tab-content");

            tabButtonMap = new Dictionary<IVisualElementTab, Button>();

            foreach (IVisualElementTab tab in tabs)
            {
                if (tab is GeneralTab generalTab)
                {
                    generalTab.SetLoadOrCreateCallback(() => LoadOrCreateConfig());
                    generalTab.SetRuntimeUiBuildCallback(() => { BuildRuntimeUiConfig(); });
                }

                Button btn = new Button(() => SwitchTab(tab, tabButtonMap))
                {
                    text = tab.TabName
                };

                btn.focusable = false;
                btn.AddToClassList("tab-button");
                tabButtonMap[tab] = btn;
                tabsHeader.Add(btn);
            }

            VisualElement separator = new VisualElement();
            separator.AddToClassList("separator-line");

            root.Add(tabsHeader);
            root.Add(separator);
            root.Add(tabContent);

            LoadSavedConfig(tabButtonMap);

            // Ensure a tab is visible even if no saved config exists
            if (configAsset == null)
            {
                SwitchTab(tabs[0], tabButtonMap);
            }
        }

        private void SwitchTab(IVisualElementTab tab, Dictionary<IVisualElementTab, Button> tabButtonMap = null)
        {
            tabContent.Clear();
            tab.OnCreateGUI(tabContent, configAsset);

            if (tabButtonMap == null)
            {
                return;
            }

            foreach (KeyValuePair<IVisualElementTab, Button> kv in tabButtonMap)
            {
                Button button = kv.Value;
                bool isSelected = kv.Key == tab;

                button.RemoveFromClassList("tab-button-selected");
                if (isSelected)
                {
                    button.AddToClassList("tab-button-selected");
                }
            }
        }

        private void SaveConfigAsset(UiEditorConfig config)
        {
            string path = AssetDatabase.GetAssetPath(config);
            string guid = AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString(configKey, guid);
        }

        private void LoadSavedConfig(Dictionary<IVisualElementTab, Button> tabButtonMap)
        {
            string guid = EditorPrefs.GetString(configKey, null);
            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UiEditorConfig config = AssetDatabase.LoadAssetAtPath<UiEditorConfig>(path);
                if (config != null)
                {
                    configAsset = config;

                    if (configField != null)
                    {
                        configField.value = configAsset;
                    }

                    ReloadUiStateRegistry();
                    SwitchTab(tabs[0], tabButtonMap);
                }
            }
        }

        public void LoadOrCreateConfig()
        {
            string path = "Assets/UiConfigs/UiEditorConfig.asset";
            UiEditorConfig config = AssetDatabase.LoadAssetAtPath<UiEditorConfig>(path);
            
            if (config == null)
            {
                string[] guids = AssetDatabase.FindAssets("UiEditorConfig t:ScriptableObject");
                if (guids.Length > 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    config = AssetDatabase.LoadAssetAtPath<UiEditorConfig>(path);
                }
            }

            if (config == null)
            {
                string folder = Path.GetDirectoryName(path);
                EnsureUnityFolderExists(folder);

                config = CreateInstance<UiEditorConfig>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                Debug.Log($"✅ Created new UiEditorConfig at {path}");
            }

            configAsset = config;
            configField.value = config;
            SaveConfigAsset(config);

            ReloadUiStateRegistry();
            SwitchTab(tabs[0], tabButtonMap);

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }

        private void ReloadUiStateRegistry()
        {
            UiStateRegistry registry = AssetDatabase.LoadAssetAtPath<UiStateRegistry>(configAsset.StateRegistryPath);
            foreach (IVisualElementTab tab in tabs)
            {
                if (tab is StatesTab statesTab)
                {
                    statesTab.SetRegistry(registry);
                }
            }
        }

        public void BuildRuntimeUiConfig()
        {
            if (configAsset == null)
            {
                Debug.LogError("❌ No config asset loaded to build runtime UI config.");
                return;
            }

            string registryPath = configAsset.StateRegistryPath;
            string configOutputPath = configAsset.RuntimeConfigOutputPath;

            UiStateRegistry registry = AssetDatabase.LoadAssetAtPath<UiStateRegistry>(registryPath);

            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<UiStateRegistry>();
                EnsureUnityFolderExists(Path.GetDirectoryName(registryPath));
                AssetDatabase.CreateAsset(registry, registryPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"🆕 Created new UiStateRegistry at: {registryPath}");
            }
            else
            {
                Debug.Log($"🔄 Using existing UiStateRegistry at: {registryPath}");
            }

            UiConfig config = AssetDatabase.LoadAssetAtPath<Runtime.UiConfig>(configOutputPath);
            if (config == null)
            {
                config = CreateInstance<Runtime.UiConfig>();
                EnsureUnityFolderExists(Path.GetDirectoryName(configOutputPath));
                AssetDatabase.CreateAsset(config, configOutputPath);
                Debug.Log($"🆕 Created new UiConfig at {configOutputPath}");
            }

            config.entries.Clear();

            for (int i = 0; i < registry.States.Count; i++)
            {
                UiStateMetadata state = registry.States[i];
                List<AssetReference> assetRefs = new List<AssetReference>();

                foreach (string sceneName in state.ElementSceneNames)
                {
                    string scenePath = AssetDatabase.FindAssets($"{sceneName} t:Scene")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == sceneName);

                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        AssetReference assetRef = new AssetReference(AssetDatabase.AssetPathToGUID(scenePath));
                        assetRefs.Add(assetRef);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Scene '{sceneName}' not found.");
                    }
                }

                config.AddOrUpdateEntry(state.StateName, assetRefs);
            }

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("✅ Built runtime UiConfig from UiStateRegistry.");

            // Ensure Addressables setup: add to Global Configs group and assign labels
            EnsureConfigIsAddressable(configOutputPath, new string[] { "UiConfig", "RuntimeUiConfig" }, "Global Configs");
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

            string[] parts = normalized.Split('/');
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

        private static void EnsureConfigIsAddressable(string assetPath, string[] labels, string groupName)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("⚠️ Addressables settings not found. Please enable Addressables in your project.");
                return;
            }

            // Ensure group exists
            AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group == null)
            {
                group = settings.CreateGroup(groupName, false, false, true, null);
                Debug.Log($"🆕 Created Addressables group '{groupName}'.");
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"❌ Could not resolve GUID for asset at {assetPath}");
                return;
            }

            AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
                entry.address = "UiConfig"; // Set a predictable address
                Debug.Log("✅ Registered UiConfig as Addressable entry.");
            }
            else if (entry.parentGroup != group)
            {
                settings.MoveEntry(entry, group);
                Debug.Log("🔄 Moved UiConfig entry to Global Configs group.");
            }

            // Ensure labels exist and are assigned
            for (int i = 0; i < labels.Length; i++)
            {
                string label = labels[i];
                IList<string> existingLabels = settings.GetLabels();
                if (existingLabels == null || !existingLabels.Contains(label))
                {
                    settings.AddLabel(label);
                }
                if (!entry.labels.Contains(label))
                {
                    entry.SetLabel(label, true, true);
                }
            }

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
            AssetDatabase.SaveAssets();
        }
    }
}
