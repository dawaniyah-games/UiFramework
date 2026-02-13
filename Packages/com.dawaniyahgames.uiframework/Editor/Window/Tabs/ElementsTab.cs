using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.SceneManagement;
using UiFramework.Editor.Config;
using UiFramework.Editor.CodeGeneration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
using System;
using UnityEditor.AddressableAssets.Settings;
using UiFramework.Editor.Addressables;
using UiFramework.Editor.Window.Popups;
using UiFramework.Core;
using UiFramework.Tweening;

namespace UiFramework.Editor.Window.Tabs
{
    public class ElementsTab : BaseVisualElementTab
    {
        private const string defaultUiElementsGroupName = "UiElements";
        private const string defaultUiElementsLabel = "UiElements";

        public override string TabName
        {
            get
            {
                return "Elements";
            }
        }

        private UiEditorConfig editorConfig;
        private ScrollView sceneListScrollView;
        private bool includeParams = false;
        private bool includeReference = false;

        public override void OnCreateGUI(VisualElement root, UiEditorConfig config)
        {
            editorConfig = config;

            if (root == null)
            {
                return;
            }

            root.style.flexGrow = 1;

            Label header = new Label("UI Elements")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18,
                    marginTop = 18,
                    marginBottom = 18
                }
            };
            root.Add(header);

            sceneListScrollView = new ScrollView();
            sceneListScrollView.mode = ScrollViewMode.Vertical;
            sceneListScrollView.style.flexGrow = 1;
            sceneListScrollView.style.minHeight = 0;
            root.Add(sceneListScrollView);
            RefreshSceneList();

            SceneAsset selectedScene = null;

            ObjectField sceneField = new ObjectField("Optional Existing Scene")
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                style = { marginBottom = 6 }
            };
            sceneField.RegisterValueChangedCallback(evt =>
            {
                selectedScene = evt.newValue as SceneAsset;
            });
            root.Add(sceneField);

            TextField nameField = new TextField("New Element Name")
            {
                style = { marginTop = 10, marginBottom = 4 }
            };
            root.Add(nameField);

            Toggle paramsToggle = new Toggle("Include Parameters Class") { value = false };

            paramsToggle.RegisterValueChangedCallback(evt =>
            {
                includeParams = evt.newValue;
            });
            root.Add(paramsToggle);

            Toggle referenceToggle = new Toggle("Include Reference Class") { value = false };

            referenceToggle.RegisterValueChangedCallback(evt =>
            {
                includeReference = evt.newValue;
            });

            root.Add(referenceToggle);

            Button createButton = new Button(() =>
            {
                string manualName = nameField.value != null ? nameField.value.Trim() : string.Empty;
                string sceneName = string.Empty;

                if (selectedScene != null)
                {
                    string selectedPath = AssetDatabase.GetAssetPath(selectedScene);
                    sceneName = Path.GetFileNameWithoutExtension(selectedPath);
                }
                else
                {
                    sceneName = manualName;
                }

                if (!string.IsNullOrEmpty(sceneName))
                {
                    CreateUiElementFromScene(sceneName, selectedScene);
                    RefreshSceneList();
                }
            })
            {
                text = "\u2795 Create UI Element"
            };
            root.Add(createButton);
        }

        private void RefreshSceneList()
        {
            if (sceneListScrollView == null)
            {
                return;
            }

            sceneListScrollView.Clear();

            List<string> scenePaths = GetElementScenePaths();
            if (scenePaths.Count == 0)
            {
                Label noSceneLabel = new Label("\u26a0\ufe0f No UI element scenes found.")
                {
                    style = { color = Color.yellow, marginBottom = 6 }
                };
                sceneListScrollView.Add(noSceneLabel);
            }
            else
            {
                AddressableAssetSettings settings;
                bool hasAddressables = AddressablesAssetUtility.TryGetSettings(out settings);
                List<string> groupNames = hasAddressables ? AddressablesAssetUtility.GetGroupNames(settings) : new List<string>();

                sceneListScrollView.Add(CreateSceneListHeaderRow());

                if (groupNames.Count == 0)
                {
                    groupNames.Add(defaultUiElementsGroupName);
                }
                else if (!groupNames.Contains(defaultUiElementsGroupName))
                {
                    groupNames.Insert(0, defaultUiElementsGroupName);
                }

                for (int i = 0; i < scenePaths.Count; i++)
                {
                    string scenePath = scenePaths[i];
                    string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    sceneListScrollView.Add(CreateSceneRow(sceneName, scenePath, hasAddressables, settings, groupNames));
                }
            }
        }

        private VisualElement CreateSceneListHeaderRow()
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 6;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;

            Label nameLabel = new Label("Scene");
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.minWidth = 180;
            nameLabel.style.flexGrow = 0;
            row.Add(nameLabel);

            Label showLabel = new Label("Show");
            showLabel.tooltip = "Animation preset to play when this UI element is shown.";
            showLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            showLabel.style.minWidth = 110;
            showLabel.style.marginRight = 8;
            row.Add(showLabel);

            Label hideLabel = new Label("Hide");
            hideLabel.tooltip = "Animation preset to play when this UI element is hidden/unloaded.";
            hideLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            hideLabel.style.minWidth = 110;
            hideLabel.style.marginRight = 8;
            row.Add(hideLabel);

            Label addressableLabel = new Label("Addressable");
            addressableLabel.tooltip = "If enabled, the scene will be registered in Addressables for loading at runtime.";
            addressableLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            addressableLabel.style.minWidth = 90;
            addressableLabel.style.marginRight = 8;
            row.Add(addressableLabel);

            Label groupLabel = new Label("Group");
            groupLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            groupLabel.style.minWidth = 140;
            groupLabel.style.marginRight = 8;
            row.Add(groupLabel);

            Label labelsLabel = new Label("Labels");
            labelsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelsLabel.style.flexGrow = 1;
            labelsLabel.style.marginRight = 8;
            row.Add(labelsLabel);

            return row;
        }

        private VisualElement CreateSceneRow(string sceneName, string sceneAssetPath, bool hasAddressables, AddressableAssetSettings settings, List<string> groupNames)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 6;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.15f);
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = Color.gray;

            Label nameLabel = new Label(sceneName);
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.minWidth = 180;
            nameLabel.style.flexGrow = 0;
            row.Add(nameLabel);

            List<string> presetKeys = new List<string>(TweenPresets.GetPresetKeys());
            if (presetKeys.Count == 0)
            {
                presetKeys.Add(TweenPresets.NoneKey);
            }

            string currentShowPreset;
            string currentHidePreset;
            bool hasUiElement = TryReadElementAnimationPresets(sceneAssetPath, out currentShowPreset, out currentHidePreset);

            if (string.IsNullOrWhiteSpace(currentShowPreset) || !presetKeys.Contains(currentShowPreset))
            {
                currentShowPreset = TweenPresets.NoneKey;
            }

            if (string.IsNullOrWhiteSpace(currentHidePreset) || !presetKeys.Contains(currentHidePreset))
            {
                currentHidePreset = TweenPresets.NoneKey;
            }

            DropdownField showDropdown = new DropdownField(presetKeys, currentShowPreset);
            showDropdown.tooltip = "Show animation preset for this UI element scene.";
            showDropdown.style.minWidth = 110;
            showDropdown.style.marginRight = 8;
            showDropdown.SetEnabled(hasUiElement);
            row.Add(showDropdown);

            DropdownField hideDropdown = new DropdownField(presetKeys, currentHidePreset);
            hideDropdown.tooltip = "Hide animation preset for this UI element scene.";
            hideDropdown.style.minWidth = 110;
            hideDropdown.style.marginRight = 8;
            hideDropdown.SetEnabled(hasUiElement);
            row.Add(hideDropdown);

            showDropdown.RegisterValueChangedCallback(evt =>
            {
                if (!hasUiElement)
                {
                    return;
                }

                WriteElementAnimationPresets(sceneAssetPath, evt.newValue, hideDropdown.value);
                RefreshSceneList();
            });

            hideDropdown.RegisterValueChangedCallback(evt =>
            {
                if (!hasUiElement)
                {
                    return;
                }

                WriteElementAnimationPresets(sceneAssetPath, showDropdown.value, evt.newValue);
                RefreshSceneList();
            });

            AddressableAssetEntry entry = null;
            bool isAddressable = hasAddressables && AddressablesAssetUtility.IsAssetAddressable(sceneAssetPath, out entry);

            HashSet<string> selectedLabels = new HashSet<string>(StringComparer.Ordinal);
            if (entry != null && entry.labels != null)
            {
                foreach (string label in entry.labels)
                {
                    if (!string.IsNullOrEmpty(label))
                    {
                        selectedLabels.Add(label);
                    }
                }
            }

            if (selectedLabels.Count == 0 && !string.IsNullOrEmpty(defaultUiElementsLabel))
            {
                selectedLabels.Add(defaultUiElementsLabel);
            }

            Toggle addressableToggle = new Toggle();
            addressableToggle.tooltip = "Mark scene as Addressable";
            addressableToggle.value = isAddressable;
            addressableToggle.SetEnabled(hasAddressables);
            addressableToggle.style.marginRight = 8;
            addressableToggle.style.minWidth = 90;
            row.Add(addressableToggle);

            string selectedGroup = "Write";

            if (!string.IsNullOrEmpty(defaultUiElementsGroupName))
            {
                selectedGroup = defaultUiElementsGroupName;
            }
            
            if (isAddressable && entry != null && entry.parentGroup != null)
            {
                selectedGroup = entry.parentGroup.Name;
            }

            if (!groupNames.Contains(selectedGroup))
            {
                selectedGroup = defaultUiElementsGroupName;

                if (!groupNames.Contains(selectedGroup))
                {
                    selectedGroup = groupNames[0];
                }
            }

            DropdownField groupDropdown = new DropdownField(groupNames, selectedGroup);
            groupDropdown.style.minWidth = 140;
            groupDropdown.style.marginRight = 8;
            groupDropdown.SetEnabled(hasAddressables);
            row.Add(groupDropdown);

            groupDropdown.RegisterValueChangedCallback(evt =>
            {
                if (!hasAddressables)
                {
                    return;
                }

                if (!addressableToggle.value)
                {
                    return;
                }

                string address = sceneName;
                string groupName = evt.newValue;
                string[] labels = selectedLabels.ToArray();
                AddressablesAssetUtility.EnsureAssetIsAddressable(sceneAssetPath, groupName, address, labels);
                RefreshSceneList();
            });

            Label labelsSummary = new Label(BuildLabelsSummary(selectedLabels));
            labelsSummary.style.flexGrow = 1;
            labelsSummary.style.marginRight = 8;
            labelsSummary.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(labelsSummary);

            Button labelsButton = new Button(() =>
            {
                if (!hasAddressables)
                {
                    return;
                }

                Rect rect = labelsSummary.worldBound;
                UnityEditor.PopupWindow.Show(rect, new AddressablesLabelsPopup(settings, selectedLabels, _ =>
                {
                    labelsSummary.text = BuildLabelsSummary(selectedLabels);

                    if (addressableToggle.value)
                    {
                        string address = sceneName;
                        string groupName = groupDropdown.value;
                        string[] labels = selectedLabels.ToArray();
                        AddressablesAssetUtility.EnsureAssetIsAddressable(sceneAssetPath, groupName, address, labels);
                        RefreshSceneList();
                    }
                }));
            });
            labelsButton.text = "Labels";
            labelsButton.SetEnabled(hasAddressables);
            labelsButton.style.marginRight = 8;
            row.Add(labelsButton);

            Button removeButton = new Button(() =>
            {
                AddressablesAssetUtility.RemoveAssetFromAddressables(sceneAssetPath);
                RefreshSceneList();
            });
            removeButton.text = "Remove";
            removeButton.SetEnabled(hasAddressables && isAddressable);
            row.Add(removeButton);

            addressableToggle.RegisterValueChangedCallback(evt =>
            {
                if (!hasAddressables)
                {
                    return;
                }

                if (evt.newValue)
                {
                    string address = sceneName;
                    string groupName = groupDropdown.value;
                    string[] labels = selectedLabels.ToArray();
                    AddressablesAssetUtility.EnsureAssetIsAddressable(sceneAssetPath, groupName, address, labels);
                }
                else
                {
                    AddressablesAssetUtility.RemoveAssetFromAddressables(sceneAssetPath);
                }

                RefreshSceneList();
            });

            return row;
        }

        private bool TryReadElementAnimationPresets(string sceneAssetPath, out string showPreset, out string hidePreset)
        {
            showPreset = null;
            hidePreset = null;

            if (string.IsNullOrWhiteSpace(sceneAssetPath))
            {
                return false;
            }

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Additive);
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    UiElement[] elements = roots[i].GetComponentsInChildren<UiElement>(true);
                    if (elements == null || elements.Length == 0)
                    {
                        continue;
                    }

                    SerializedObject so = new SerializedObject(elements[0]);
                    SerializedProperty showProp = so.FindProperty("showAnimationPreset");
                    SerializedProperty hideProp = so.FindProperty("hideAnimationPreset");

                    showPreset = showProp != null ? showProp.stringValue : null;
                    hidePreset = hideProp != null ? hideProp.stringValue : null;
                    return true;
                }

                return false;
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private void WriteElementAnimationPresets(string sceneAssetPath, string showPreset, string hidePreset)
        {
            if (string.IsNullOrWhiteSpace(sceneAssetPath))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Additive);
            try
            {
                bool wroteAny = false;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    UiElement[] elements = roots[i].GetComponentsInChildren<UiElement>(true);
                    if (elements == null || elements.Length == 0)
                    {
                        continue;
                    }

                    for (int e = 0; e < elements.Length; e++)
                    {
                        SerializedObject so = new SerializedObject(elements[e]);
                        SerializedProperty showProp = so.FindProperty("showAnimationPreset");
                        SerializedProperty hideProp = so.FindProperty("hideAnimationPreset");

                        if (showProp != null)
                        {
                            showProp.stringValue = showPreset;
                        }

                        if (hideProp != null)
                        {
                            hideProp.stringValue = hidePreset;
                        }

                        so.ApplyModifiedProperties();
                        wroteAny = true;
                    }
                }

                if (!wroteAny)
                {
                    Debug.LogWarning("[UiFramework] No UiElement found in scene: " + sceneAssetPath);
                    return;
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private string BuildLabelsSummary(HashSet<string> selectedLabels)
        {
            if (selectedLabels == null || selectedLabels.Count == 0)
            {
                return "(no labels)";
            }

            List<string> labels = new List<string>(selectedLabels);
            labels.Sort(StringComparer.Ordinal);
            return string.Join(", ", labels.ToArray());
        }

        private void CreateUiElementFromScene(string elementName, SceneAsset sceneAsset)
        {
            string scriptPath = editorConfig != null ? editorConfig.ElementsScriptPath : string.Empty;
            string scenePath = editorConfig != null ? editorConfig.ElementsScenePath : string.Empty;
            string elementNamespace = editorConfig != null ? editorConfig.ElementNamespace : string.Empty;

            UiElementGenerator.Generate(elementName, scriptPath, elementNamespace, includeParams, includeReference);
            AssetDatabase.Refresh();

            EditorPrefs.SetString("UiElementAutoBind_Name", elementName);
            EditorPrefs.SetString("UiElementAutoBind_ScriptPath", scriptPath);

            Scene openedScene;

            if (sceneAsset != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(sceneAsset);
                openedScene = EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
            }
            else
            {
                openedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            }

            string rootName = elementName + "Root";
            GameObject rootGameObject = new GameObject(rootName);

            GameObject[] roots = openedScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject go = roots[i];
                if (go != rootGameObject)
                {
                    go.transform.SetParent(rootGameObject.transform);
                }
            }

            string expectedScriptFile = Path.Combine(scriptPath, elementName + ".cs");
            string[] guids = AssetDatabase.FindAssets(elementName + " t:MonoScript", new string[] { scriptPath });

            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) == elementName)
                {
                    MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                    Type type = monoScript != null ? monoScript.GetClass() : null;

                    if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
                    {
                        if (rootGameObject.GetComponent(type) == null)
                        {
                            rootGameObject.AddComponent(type);
                            EditorSceneManager.MarkSceneDirty(openedScene);
                            Debug.Log("✅ Attached script '" + type.Name + "' to '" + rootName + "'");
                        }
                    }
                }
            }

            string finalScenePath = Path.Combine(scenePath, elementName + ".unity");
            Directory.CreateDirectory(scenePath);
            EditorSceneManager.SaveScene(openedScene, finalScenePath);
            AssetDatabase.Refresh();

            Debug.Log("\u2705 Created/updated scene: " + finalScenePath + ". Script will be attached to '" + rootName + "' after compilation.");
        }

        private List<string> GetElementScenePaths()
        {
            if (editorConfig == null || string.IsNullOrEmpty(editorConfig.ElementsScenePath))
            {
                return new List<string>();
            }

            List<string> scenes = AssetDatabase.FindAssets("t:Scene", new string[] { editorConfig.ElementsScenePath })
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith(".unity"))
                .Distinct()
                .ToList();

            scenes.Sort(StringComparer.Ordinal);
            return scenes;
        }
    }
}
