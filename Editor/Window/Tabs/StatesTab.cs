using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UiFramework.Editor.CodeGeneration;
using UiFramework.Editor.Config;
using UiFramework.Editor.Data;

namespace UiFramework.Editor.Window.Tabs
{
    public class StatesTab : BaseVisualElementTab
    {
        public override string TabName
        {
            get
            {
                return "States";
            }
        }

        private UiEditorConfig editorConfig;
        private UiStateRegistry registry;
        private ScrollView stateListView;
        private ScrollView elementDetailView;
        private string selectedState;

        public void SetRegistry(UiStateRegistry registryInstance)
        {
            registry = registryInstance;
        }

        public override void OnCreateGUI(VisualElement root, UiEditorConfig config)
        {
            editorConfig = config;
            LoadRegistry();

            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            CreateTopBar(root);
            CreateContentArea(root);
        }

        private void CreateTopBar(VisualElement root)
        {
            VisualElement topBar = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 10,
                    paddingRight = 10
                }
            };

            Label titleLabel = new Label("UI States")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 13,
                    marginRight = 10
                }
            };

            TextField nameField = new TextField
            {
                value = string.Empty,
                tooltip = "Enter name of new UI state",
                style = { width = 160, marginRight = 8 }
            };

            Button createButton = new Button
            {
                text = "➕",
                tooltip = "Create new UI State",
                style =
                {
                    width = 28,
                    height = 24,
                    marginLeft = 4
                }
            };
            createButton.clicked += () =>
            {
                string name = nameField.value != null ? nameField.value.Trim() : string.Empty;

                if (string.IsNullOrWhiteSpace(name))
                {
                    EditorUtility.DisplayDialog("Invalid Name", "State name cannot be empty or whitespace.", "OK");
                    return;
                }

                UiStateGenerator.Generate(name, editorConfig.StatesPath, editorConfig.StateNamespace);
                AddToRegistry(name, new List<string>());
                AssetDatabase.Refresh();

                selectedState = name;
                RebuildStateList();
            };

            Button deleteButton = new Button
            {
                text = "🗑",
                tooltip = "Delete selected UI State"
            };
            deleteButton.style.width = 28;
            deleteButton.style.height = 24;
            deleteButton.style.marginLeft = 6;

            deleteButton.clicked += () =>
            {
                if (string.IsNullOrEmpty(selectedState))
                {
                    return;
                }

                if (EditorUtility.DisplayDialog("Delete UI State", "Are you sure you want to delete '" + selectedState + "'?", "Yes", "No"))
                {
                    UiStateMetadata metadata = registry.States.FirstOrDefault(s => s.StateName == selectedState);
                    if (metadata != null)
                    {
                        registry.States.Remove(metadata);
                        EditorUtility.SetDirty(registry);
                        AssetDatabase.SaveAssets();
                        Debug.Log("🗑 Deleted UIState: " + selectedState);

                        selectedState = null;
                        RebuildStateList();
                    }
                }
            };

            root.Add(topBar);
            topBar.Add(titleLabel);
            topBar.Add(nameField);
            topBar.Add(createButton);
            topBar.Add(deleteButton);
        }

        private void CreateContentArea(VisualElement root)
        {
            stateListView = new ScrollView();
            elementDetailView = new ScrollView();

            stateListView.mode = ScrollViewMode.Vertical;
            elementDetailView.mode = ScrollViewMode.Vertical;

            VisualElement container = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    marginTop = 8
                }
            };

            stateListView.style.width = new Length(30, LengthUnit.Percent);
            stateListView.style.marginRight = 8;
            stateListView.style.flexGrow = 0;
            stateListView.style.minHeight = 0;

            elementDetailView.style.flexGrow = 1;
            elementDetailView.style.minHeight = 0;

            container.Add(stateListView);
            container.Add(elementDetailView);
            root.Add(container);

            RebuildStateList();
        }

        private void RebuildStateList()
        {
            if (registry == null)
            {
                Debug.LogWarning("⚠️ Registry not set in StatesTab.");
                return;
            }

            stateListView.Clear();

            List<string> states = registry.States.Select(s => s.StateName).ToList();
            if (string.IsNullOrEmpty(selectedState) && states.Count > 0)
            {
                selectedState = states[0];
            }

            for (int i = 0; i < states.Count; i++)
            {
                string stateName = states[i];

                Button selectButton = new Button(() =>
                {
                    selectedState = stateName;
                    PopulateElementDetails();
                    RefreshStateSelectionVisuals(states);
                })
                {
                    text = stateName
                };

                if (selectedState == stateName)
                {
                    selectButton.AddToClassList("tab-button-selected");
                }

                stateListView.Add(selectButton);
            }

            PopulateElementDetails();
        }

        private void RefreshStateSelectionVisuals(List<string> allStates)
        {
            for (int i = 0; i < stateListView.childCount; i++)
            {
                VisualElement child = stateListView[i];
                Button button = child as Button;
                if (button == null)
                {
                    continue;
                }

                if (button.text == selectedState)
                {
                    button.AddToClassList("tab-button-selected");
                }
                else
                {
                    button.RemoveFromClassList("tab-button-selected");
                }
            }
        }

        private void PopulateElementDetails()
        {
            elementDetailView.Clear();

            UiStateMetadata metadata = registry.States.FirstOrDefault(s => s.StateName == selectedState);
            if (metadata == null)
            {
                return;
            }

            Label header = new Label("State: " + selectedState)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 6
                }
            };
            elementDetailView.Add(header);

            for (int i = 0; i < metadata.ElementSceneNames.Count; i++)
            {
                string sceneName = metadata.ElementSceneNames[i];

                VisualElement row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 4
                    }
                };

                Label icon = new Label("🎬");
                icon.style.marginRight = 4;

                Label label = new Label(sceneName);
                label.style.flexGrow = 1;
                label.style.fontSize = 13;

                Button removeButton = new Button(() =>
                {
                    metadata.ElementSceneNames.Remove(sceneName);
                    EditorUtility.SetDirty(registry);
                    AssetDatabase.SaveAssets();
                    PopulateElementDetails();
                })
                {
                    text = "✖",
                    tooltip = "Remove scene"
                };
                removeButton.style.width = 22;
                removeButton.style.height = 22;

                row.Add(icon);
                row.Add(label);
                row.Add(removeButton);
                elementDetailView.Add(row);
            }

            List<string> availableScenes = GetAvailableElementScenes()
                .Where(s => !metadata.ElementSceneNames.Contains(s))
                .ToList();

            if (availableScenes.Count > 0)
            {
                string selectedScene = availableScenes[0];

                VisualElement addRow = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginTop = 10
                    }
                };

                PopupField<string> dropdown = new PopupField<string>(availableScenes, 0);
                dropdown.style.flexGrow = 1;
                dropdown.style.marginRight = 6;
                dropdown.RegisterValueChangedCallback(evt =>
                {
                    selectedScene = evt.newValue;
                });

                Button addButton = new Button(() =>
                {
                    if (!metadata.ElementSceneNames.Contains(selectedScene))
                    {
                        metadata.ElementSceneNames.Add(selectedScene);
                        EditorUtility.SetDirty(registry);
                        AssetDatabase.SaveAssets();
                        PopulateElementDetails();
                    }
                })
                {
                    text = "Add",
                    tooltip = "Add selected scene"
                };
                addButton.style.height = 24;
                addButton.style.minWidth = 60;

                addRow.Add(dropdown);
                addRow.Add(addButton);
                elementDetailView.Add(addRow);
            }
            else
            {
                Label allAssigned = new Label("✅ All available scenes already assigned.");
                elementDetailView.Add(allAssigned);
            }
        }

        private void LoadRegistry()
        {
            if (registry != null)
            {
                return;
            }

            if (editorConfig == null || string.IsNullOrEmpty(editorConfig.StateRegistryPath))
            {
                Debug.LogWarning("⚠️ Cannot load registry, config is null or path missing.");
                return;
            }

            registry = AssetDatabase.LoadAssetAtPath<UiStateRegistry>(editorConfig.StateRegistryPath);
        }

        private List<string> GetAvailableElementScenes()
        {
            List<string> allScenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();

            List<string> filtered = allScenePaths
                .Where(path => path.StartsWith("Assets/Scenes/UiElements/"))
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            return filtered;
        }

        private void AddToRegistry(string stateName, List<string> elementSceneNames)
        {
            if (registry == null)
            {
                Debug.LogError("❌ Cannot add to registry, it's not loaded.");
                return;
            }

            bool exists = registry.States.Any(s => s.StateName == stateName);
            if (exists)
            {
                Debug.LogWarning("⚠️ State '" + stateName + "' already exists.");
                return;
            }

            UiStateMetadata newMeta = new UiStateMetadata
            {
                StateName = stateName,
                ElementSceneNames = elementSceneNames
            };

            registry.States.Add(newMeta);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }
    }
}
