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

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UiFramework/Editor/Styles/UiSetupTabs.uss");
            
            if (styleSheet == null)
            {
                string[] guids = AssetDatabase.FindAssets("UiSetupTabs t:StyleSheet");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                }
            }

            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning("❌ Stylesheet 'UiSetupTabs.uss' not found.");
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

                    generalTab.SetRuntimeUiBuildCallback(() =>
                    {
                        BuildRuntimeUiConfig();
                    });
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
            const string path = "Assets/UiConfigs/UiEditorConfig.asset";
            UiEditorConfig config = AssetDatabase.LoadAssetAtPath<UiEditorConfig>(path);

            if (config == null)
            {
                string folder = Path.GetDirectoryName(path);
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string[] parts = folder.Split('/');
                    string current = "Assets";
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = $"{current}/{parts[i]}";

                        if (!AssetDatabase.IsValidFolder(next))
                        {
                            AssetDatabase.CreateFolder(current, parts[i]);
                        }

                        current = next;
                    }
                }

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
                Directory.CreateDirectory(Path.GetDirectoryName(registryPath));
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
                Directory.CreateDirectory(Path.GetDirectoryName(configOutputPath));
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
        }
    }
}
