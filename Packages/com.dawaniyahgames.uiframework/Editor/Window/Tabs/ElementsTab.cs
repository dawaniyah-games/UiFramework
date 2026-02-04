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
