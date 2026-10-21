using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UiFramework.Editor.CodeGeneration;
using UiFramework.Editor.Config;
using UiFramework.Editor.Data;
using UiFramework.Core;

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
        private ScrollView stateListView;
        private ScrollView elementDetailView;
        private readonly List<MonoScript> stateScripts = new List<MonoScript>();
        private readonly Dictionary<string, UiStateDefinition> definitionsByKey = new Dictionary<string, UiStateDefinition>(StringComparer.Ordinal);
        private MonoScript selectedStateScript;
        private string searchQuery = string.Empty;

        public void SetRegistry(UiStateRegistry registryInstance)
        {
            // Legacy no-op: states are now stored as per-state UiStateDefinition assets.
        }

        public override void OnCreateGUI(VisualElement root, UiEditorConfig config)
        {
            editorConfig = config;

            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            CreateTopBar(root);
            CreateContentArea(root);

            ReloadStateScriptsAndDefinitions();
            RebuildStateList();
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
                AssetDatabase.Refresh();

                // The new script won't become a type until the next compile; still refresh the list.
                ReloadStateScriptsAndDefinitions();
                selectedStateScript = FindStateScriptByClassName(name);

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
                if (selectedStateScript == null)
                {
                    return;
                }

                string stateKey = GetSelectedStateKey();
                UiStateDefinition selectedDefinition = GetSelectedDefinition();
                string definitionPath = selectedDefinition != null ? AssetDatabase.GetAssetPath(selectedDefinition) : null;
                string scriptPath = AssetDatabase.GetAssetPath(selectedStateScript);

                if (selectedDefinition != null)
                {
                    int option = EditorUtility.DisplayDialogComplex(
                        "Delete UI State",
                        "Delete config for '" + stateKey + "'?\n\nYou can also delete the UiState script file.",
                        "Config Only",
                        "Cancel",
                        "Config + Script");

                    if (option == 1)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(definitionPath))
                    {
                        AssetDatabase.SaveAssets();
                        AssetDatabase.DeleteAsset(definitionPath);
                    }

                    if (option == 2)
                    {
                        if (!string.IsNullOrEmpty(scriptPath))
                        {
                            AssetDatabase.DeleteAsset(scriptPath);
                        }
                    }
                }
                else
                {
                    bool deleteScript = EditorUtility.DisplayDialog(
                        "Delete UI State",
                        "No config found for '" + stateKey + "'.\n\nDelete the UiState script file?",
                        "Delete Script",
                        "Cancel");

                    if (!deleteScript)
                    {
                        return;
                    }

                    if (!string.IsNullOrEmpty(scriptPath))
                    {
                        AssetDatabase.DeleteAsset(scriptPath);
                    }
                }

                selectedStateScript = null;
                ReloadStateScriptsAndDefinitions();
                RebuildStateList();
            };

            ToolbarSearchField searchField = new ToolbarSearchField();
            searchField.tooltip = "Search states by name or part of the name. Press Enter to select the first result.";
            searchField.style.flexGrow = 1;
            searchField.style.maxWidth = 360;
            searchField.style.marginRight = 12;
            searchField.value = searchQuery;
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchQuery = evt.newValue ?? string.Empty;
                RebuildStateList();
            });
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                {
                    return;
                }

                List<MonoScript> results = GetVisibleStateScripts();
                if (results.Count > 0)
                {
                    selectedStateScript = results[0];
                    RebuildStateList();
                }
            });

            Label newLabel = new Label("New state:");
            newLabel.style.marginRight = 4;

            root.Add(topBar);
            topBar.Add(titleLabel);
            topBar.Add(searchField);
            topBar.Add(newLabel);
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
        }

        private void RebuildStateList()
        {
            stateListView.Clear();

            if (stateScripts == null || stateScripts.Count == 0)
            {
                stateListView.Add(new Label("No UiState scripts found."));
                PopulateElementDetails();
                return;
            }

            if (selectedStateScript == null)
            {
                selectedStateScript = stateScripts[0];
            }

            List<MonoScript> visible = GetVisibleStateScripts();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                Label count = new Label(visible.Count + " of " + stateScripts.Count);
                count.style.color = new Color(0.6f, 0.6f, 0.6f);
                count.style.fontSize = 11;
                count.style.marginBottom = 4;
                stateListView.Add(count);

                if (visible.Count == 0)
                {
                    Label none = new Label("No states match '" + searchQuery.Trim() + "'.");
                    none.style.whiteSpace = WhiteSpace.Normal;
                    none.style.color = new Color(0.6f, 0.6f, 0.6f);
                    stateListView.Add(none);
                }
            }

            for (int i = 0; i < visible.Count; i++)
            {
                MonoScript script = visible[i];
                if (script == null)
                {
                    continue;
                }

                Type stateType = script.GetClass();
                string stateName = stateType != null ? stateType.Name : script.name;

                Button selectButton = new Button(() =>
                {
                    selectedStateScript = script;
                    PopulateElementDetails();
                    RefreshStateSelectionVisuals();
                })
                {
                    text = stateName
                };

                if (selectedStateScript == script)
                {
                    selectButton.AddToClassList("tab-button-selected");
                }

                stateListView.Add(selectButton);
            }

            PopulateElementDetails();
        }

        private List<MonoScript> GetVisibleStateScripts()
        {
            return UiSearchFilter.Filter(stateScripts, GetScriptClassName, searchQuery);
        }

        private void RefreshStateSelectionVisuals()
        {
            for (int i = 0; i < stateListView.childCount; i++)
            {
                VisualElement child = stateListView[i];
                Button button = child as Button;
                if (button == null)
                {
                    continue;
                }

                string selectedName = GetSelectedStateKey();
                if (!string.IsNullOrEmpty(selectedName) && button.text == selectedName)
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

            if (selectedStateScript == null)
            {
                elementDetailView.Add(new Label("Select a state to edit."));
                return;
            }

            string stateKey = GetSelectedStateKey();

            Label header = new Label("State: " + stateKey)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 14,
                    marginBottom = 6
                }
            };
            elementDetailView.Add(header);

            UiStateDefinition selectedDefinition = GetSelectedDefinition();
            if (selectedDefinition == null)
            {
                Label missing = new Label("No config found for this UiState.")
                {
                    style =
                    {
                        marginBottom = 6
                    }
                };
                elementDetailView.Add(missing);

                Button createConfigButton = new Button(() =>
                {
                    string folder = editorConfig != null ? editorConfig.StateDefinitionsPath : "Assets/UiConfigs/UiStateDefinitions";
                    UiStateDefinition created = UiStateDefinitionUtility.CreateNew(folder, stateKey);
                    if (created != null)
                    {
                        EditorGUIUtility.PingObject(created);
                        Selection.activeObject = created;
                    }

                    ReloadStateScriptsAndDefinitions();
                    PopulateElementDetails();
                    RefreshStateSelectionVisuals();
                })
                {
                    text = "Create Config"
                };
                createConfigButton.style.width = 140;
                createConfigButton.style.height = 26;
                elementDetailView.Add(createConfigButton);
                return;
            }

            for (int i = 0; i < selectedDefinition.ElementSceneNames.Count; i++)
            {
                string sceneName = selectedDefinition.ElementSceneNames[i];

                VisualElement row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        marginBottom = 4
                    }
                };

                Label label = new Label(sceneName);
                label.style.flexGrow = 1;
                label.style.fontSize = 13;

                Button removeButton = new Button(() =>
                {
                    selectedDefinition.ElementSceneNames.Remove(sceneName);
                    EditorUtility.SetDirty(selectedDefinition);
                    AssetDatabase.SaveAssets();
                    PopulateElementDetails();
                })
                {
                    text = "✖",
                    tooltip = "Remove scene"
                };
                removeButton.style.width = 22;
                removeButton.style.height = 22;

                row.Add(label);
                row.Add(removeButton);
                elementDetailView.Add(row);
            }

            List<string> availableScenes = GetAvailableElementScenes()
                .FindAll(s => !selectedDefinition.ElementSceneNames.Contains(s));

            if (availableScenes.Count > 0)
            {
                elementDetailView.Add(CreateAddSceneSearch(selectedDefinition, availableScenes));
            }
            else
            {
                Label allAssigned = new Label("✅ All available scenes already assigned.");
                elementDetailView.Add(allAssigned);
            }
        }

        // "Add element scene" search box: type part of a name, matching scenes are suggested below; click one to add it.
        private VisualElement CreateAddSceneSearch(UiStateDefinition definition, List<string> availableScenes)
        {
            const int MaxSuggestions = 8;

            VisualElement container = new VisualElement();
            container.style.marginTop = 12;

            Label caption = new Label("Add element scene");
            caption.style.unityFontStyleAndWeight = FontStyle.Bold;
            caption.style.marginBottom = 2;
            container.Add(caption);

            ToolbarSearchField sceneSearch = new ToolbarSearchField();
            sceneSearch.tooltip = "Type part of a scene name, then click a suggestion (or press Enter to add the first one).";
            sceneSearch.style.width = StyleKeyword.Auto;
            sceneSearch.style.marginLeft = 0;
            container.Add(sceneSearch);

            VisualElement suggestions = new VisualElement();
            suggestions.style.marginTop = 2;
            container.Add(suggestions);

            Action<string> addScene = sceneName =>
            {
                if (!definition.ElementSceneNames.Contains(sceneName))
                {
                    definition.ElementSceneNames.Add(sceneName);
                    EditorUtility.SetDirty(definition);
                    AssetDatabase.SaveAssets();
                }

                PopulateElementDetails();
            };

            Action<string> showSuggestions = query =>
            {
                suggestions.Clear();
                List<string> matches = UiSearchFilter.Filter(availableScenes, name => name, query);

                if (matches.Count == 0)
                {
                    Label none = new Label("No unassigned scene matches '" + query.Trim() + "'.");
                    none.style.color = new Color(0.6f, 0.6f, 0.6f);
                    suggestions.Add(none);
                    return;
                }

                for (int i = 0; i < matches.Count && i < MaxSuggestions; i++)
                {
                    string sceneName = matches[i];
                    Button suggestion = new Button(() =>
                    {
                        addScene(sceneName);
                    })
                    {
                        text = "➕  " + sceneName,
                        tooltip = "Add " + sceneName + " to this state"
                    };
                    suggestion.style.unityTextAlign = TextAnchor.MiddleLeft;
                    suggestions.Add(suggestion);
                }

                if (matches.Count > MaxSuggestions)
                {
                    Label more = new Label("+" + (matches.Count - MaxSuggestions) + " more, keep typing to narrow down");
                    more.style.color = new Color(0.6f, 0.6f, 0.6f);
                    more.style.fontSize = 11;
                    suggestions.Add(more);
                }
            };

            sceneSearch.RegisterValueChangedCallback(evt =>
            {
                showSuggestions(evt.newValue ?? string.Empty);
            });

            sceneSearch.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                {
                    return;
                }

                List<string> matches = UiSearchFilter.Filter(availableScenes, name => name, sceneSearch.value);
                if (matches.Count > 0)
                {
                    addScene(matches[0]);
                }
            });

            showSuggestions(string.Empty);
            return container;
        }

        private void ReloadStateScriptsAndDefinitions()
        {
            stateScripts.Clear();
            definitionsByKey.Clear();

            LoadDefinitions();
            LoadStateScripts();

            if (selectedStateScript != null && !stateScripts.Contains(selectedStateScript))
            {
                selectedStateScript = stateScripts.Count > 0 ? stateScripts[0] : null;
            }
        }

        private void LoadDefinitions()
        {
            string folder = editorConfig != null ? editorConfig.StateDefinitionsPath : string.Empty;
            List<UiStateDefinition> definitions = UiStateDefinitionUtility.FindAll(folder);

            for (int i = 0; i < definitions.Count; i++)
            {
                UiStateDefinition definition = definitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.StateKey))
                {
                    continue;
                }

                if (!definitionsByKey.ContainsKey(definition.StateKey))
                {
                    definitionsByKey.Add(definition.StateKey, definition);
                }
            }
        }

        private void LoadStateScripts()
        {
            if (editorConfig == null || string.IsNullOrEmpty(editorConfig.StatesPath))
            {
                return;
            }

            string normalizedFolder = editorConfig.StatesPath.Replace("\\", "/").Trim();
            if (!AssetDatabase.IsValidFolder(normalizedFolder))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:MonoScript", new string[] { normalizedFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null)
                {
                    continue;
                }

                Type type = script.GetClass();
                if (type == null)
                {
                    continue;
                }

                if (type == typeof(UiState))
                {
                    continue;
                }

                if (!typeof(UiState).IsAssignableFrom(type))
                {
                    continue;
                }

                if (!stateScripts.Contains(script))
                {
                    stateScripts.Add(script);
                }
            }

            stateScripts.Sort(CompareScriptsByClassName);
        }

        private int CompareScriptsByClassName(MonoScript a, MonoScript b)
        {
            string aName = GetScriptClassName(a);
            string bName = GetScriptClassName(b);
            return string.Compare(aName, bName, StringComparison.Ordinal);
        }

        private string GetScriptClassName(MonoScript script)
        {
            if (script == null)
            {
                return string.Empty;
            }

            Type type = script.GetClass();
            if (type == null)
            {
                return script.name;
            }

            return type.Name;
        }

        private string GetSelectedStateKey()
        {
            if (selectedStateScript == null)
            {
                return string.Empty;
            }

            Type type = selectedStateScript.GetClass();
            if (type == null)
            {
                return selectedStateScript.name;
            }

            return type.Name;
        }

        private UiStateDefinition GetSelectedDefinition()
        {
            string key = GetSelectedStateKey();
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            UiStateDefinition definition;
            if (definitionsByKey.TryGetValue(key, out definition))
            {
                return definition;
            }

            return null;
        }

        private MonoScript FindStateScriptByClassName(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return null;
            }

            string trimmed = className.Trim();
            for (int i = 0; i < stateScripts.Count; i++)
            {
                MonoScript script = stateScripts[i];
                if (GetScriptClassName(script) == trimmed)
                {
                    return script;
                }
            }

            return null;
        }

        private List<string> GetAvailableElementScenes()
        {
            List<string> scenes = new List<string>();

            string scenesFolder = editorConfig != null ? editorConfig.ElementsScenePath : string.Empty;
            if (string.IsNullOrEmpty(scenesFolder))
            {
                scenesFolder = "Assets/Scenes/UiElements";
            }

            string normalized = scenesFolder.Replace("\\", "/").Trim();
            if (!AssetDatabase.IsValidFolder(normalized))
            {
                return scenes;
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { normalized });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string sceneName = Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(sceneName))
                {
                    continue;
                }

                if (!scenes.Contains(sceneName))
                {
                    scenes.Add(sceneName);
                }
            }

            scenes.Sort(StringComparer.Ordinal);
            return scenes;
        }
    }
}
