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
using UiFramework.Editor.Scenes;
using UiFramework.Editor.Window.Popups;
using UiFramework.Core;
using UiFramework.Tweening;

namespace UiFramework.Editor.Window.Tabs
{
    public class ElementsTab : BaseVisualElementTab
    {
        private const string defaultUiElementsGroupName = "UiElements";
        private const string defaultUiElementsLabel = "UiElements";
        private const float structureColumnWidth = 64.0f;
        private const float canvasColumnWidth = 64.0f;
        private const float actionsColumnWidth = 196.0f;
        private const float excludeColumnWidth = 52.0f;

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
            sceneListScrollView.mode = ScrollViewMode.VerticalAndHorizontal;
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

                UiCanvasSettings canvasSettings = UiEditorConfigUtility.GetCanvasSettingsOrDefaults(editorConfig);

                try
                {
                    for (int i = 0; i < scenePaths.Count; i++)
                    {
                        string scenePath = scenePaths[i];
                        string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                        EditorUtility.DisplayProgressBar("UI Elements", "Reading " + sceneName, (float)i / scenePaths.Count);

                        // One scene open per row: presets, structure and canvas status are read together.
                        UiElementSceneInfo info = UiElementSceneUtility.InspectScene(scenePath, canvasSettings);
                        sceneListScrollView.Add(CreateSceneRow(sceneName, scenePath, hasAddressables, settings, groupNames, info, canvasSettings));
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
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

            Label structureLabel = new Label("Structure");
            structureLabel.tooltip = "Whether the scene follows the UI element scene contract: <Name>Root → Canvas → one stretched Root.\n✓ valid  ⚠ fixable issues or warnings  ✖ issues that need manual work";
            structureLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            structureLabel.style.minWidth = structureColumnWidth;
            structureLabel.style.marginRight = 8;
            row.Add(structureLabel);

            Label canvasLabel = new Label("Canvas");
            canvasLabel.tooltip = "Whether the Canvas/CanvasScaler match the canvas settings (Canvas tab).\n✓ matches  ⚠ differs  — no canvas  Excluded = skipped by Apply To All";
            canvasLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            canvasLabel.style.minWidth = canvasColumnWidth;
            canvasLabel.style.marginRight = 8;
            row.Add(canvasLabel);

            Label actionsLabel = new Label("Contract");
            actionsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            actionsLabel.style.minWidth = actionsColumnWidth;
            actionsLabel.style.marginRight = 8;
            row.Add(actionsLabel);

            Label excludeLabel = new Label("Exclude");
            excludeLabel.tooltip = "Skip this scene in 'Apply Canvas Settings To All'.";
            excludeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            excludeLabel.style.minWidth = excludeColumnWidth;
            excludeLabel.style.marginRight = 8;
            row.Add(excludeLabel);

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

        private VisualElement CreateSceneRow(string sceneName, string sceneAssetPath, bool hasAddressables, AddressableAssetSettings settings, List<string> groupNames, UiElementSceneInfo info, UiCanvasSettings canvasSettings)
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

            string currentShowPreset = info.ShowPreset;
            string currentHidePreset = info.HidePreset;
            bool hasUiElement = info.HasUiElement;

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

            AddContractColumns(row, sceneAssetPath, info, canvasSettings);

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

        private void WriteElementAnimationPresets(string sceneAssetPath, string showPreset, string hidePreset)
        {
            if (string.IsNullOrWhiteSpace(sceneAssetPath))
            {
                return;
            }

            bool wroteAny = false;

            // Scenes already open in the editor are edited in place (dirty, not saved or closed).
            UiElementSceneUtility.WithScene(sceneAssetPath, true, scene =>
            {
                wroteAny = UiElementSceneUtility.WriteAnimationPresets(scene, showPreset, hidePreset);
            });

            if (!wroteAny)
            {
                Debug.LogWarning("[UiFramework] No UiElement found in scene: " + sceneAssetPath);
            }
        }

        private void AddContractColumns(VisualElement row, string sceneAssetPath, UiElementSceneInfo info, UiCanvasSettings canvasSettings)
        {
            UiElementStructureReport structure = info.Structure;

            Label structureStatus = new Label();
            structureStatus.style.minWidth = structureColumnWidth;
            structureStatus.style.marginRight = 8;
            structureStatus.style.unityTextAlign = TextAnchor.MiddleCenter;
            structureStatus.style.unityFontStyleAndWeight = FontStyle.Bold;

            if (!info.IsAvailable)
            {
                structureStatus.text = "?";
                structureStatus.tooltip = "The scene could not be opened (e.g. in Play Mode).";
            }
            else if (!structure.HasIssues)
            {
                structureStatus.text = "✓";
                structureStatus.tooltip = "Follows the UI element scene contract.";
                structureStatus.style.color = new Color(0.4f, 0.8f, 0.4f);
            }
            else if (structure.HasUnfixableIssues)
            {
                structureStatus.text = "✖";
                structureStatus.tooltip = structure.GetIssuesText();
                structureStatus.style.color = new Color(0.9f, 0.35f, 0.35f);
            }
            else
            {
                structureStatus.text = "⚠";
                structureStatus.tooltip = structure.GetIssuesText();
                structureStatus.style.color = new Color(0.95f, 0.75f, 0.3f);
            }

            row.Add(structureStatus);

            Label canvasStatus = new Label();
            canvasStatus.style.minWidth = canvasColumnWidth;
            canvasStatus.style.marginRight = 8;
            canvasStatus.style.unityTextAlign = TextAnchor.MiddleCenter;
            UpdateCanvasStatusLabel(canvasStatus, info, info.CanvasStatus == UiCanvasStatus.Excluded);
            row.Add(canvasStatus);

            VisualElement actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.minWidth = actionsColumnWidth;
            actions.style.marginRight = 8;

            Button fixButton = new Button(() =>
            {
                FixSceneStructure(sceneAssetPath, info, canvasSettings);
            })
            {
                text = "Fix Structure",
                tooltip = structure.HasFixableIssues ? "Fix the structure issues that can be fixed automatically (asks first)." : "Nothing to fix automatically."
            };
            fixButton.SetEnabled(info.IsAvailable && structure.HasFixableIssues);
            actions.Add(fixButton);

            bool canResetCanvas = info.IsAvailable && info.CanvasStatus != UiCanvasStatus.NoCanvas && !string.IsNullOrEmpty(info.CanvasDiff);
            Button resetButton = new Button(() =>
            {
                ResetSceneCanvas(sceneAssetPath, info, canvasSettings);
            })
            {
                text = "Reset Canvas",
                tooltip = canResetCanvas ? "Write the canvas settings into this scene's Canvas/CanvasScaler (asks first). Works on excluded scenes too." : "Nothing to reset."
            };
            resetButton.SetEnabled(canResetCanvas);
            actions.Add(resetButton);
            row.Add(actions);

            Toggle excludeToggle = new Toggle();
            excludeToggle.tooltip = "Skip this scene in 'Apply Canvas Settings To All'.";
            excludeToggle.value = canvasSettings != null && canvasSettings.IsExcluded(sceneAssetPath);
            excludeToggle.style.minWidth = excludeColumnWidth;
            excludeToggle.style.marginRight = 8;
            excludeToggle.SetEnabled(canvasSettings != null && EditorUtility.IsPersistent(canvasSettings));
            excludeToggle.RegisterValueChangedCallback(evt =>
            {
                if (canvasSettings.SetExcluded(sceneAssetPath, evt.newValue))
                {
                    AssetDatabase.SaveAssets();
                }

                UpdateCanvasStatusLabel(canvasStatus, info, evt.newValue);
            });
            row.Add(excludeToggle);
        }

        private static void UpdateCanvasStatusLabel(Label label, UiElementSceneInfo info, bool excluded)
        {
            label.style.color = StyleKeyword.Null;

            if (!info.IsAvailable)
            {
                label.text = "?";
                label.tooltip = "The scene could not be opened.";
                return;
            }

            if (info.CanvasStatus == UiCanvasStatus.NoCanvas)
            {
                label.text = "—";
                label.tooltip = "No root Canvas found under the element root.";
                return;
            }

            if (excluded)
            {
                label.text = "Excluded";
                label.tooltip = "Skipped by 'Apply Canvas Settings To All'." + (string.IsNullOrEmpty(info.CanvasDiff) ? " Matches the canvas settings." : "\nDiffers: " + info.CanvasDiff.Replace("; ", "\n"));
                label.style.color = Color.gray;
                return;
            }

            if (string.IsNullOrEmpty(info.CanvasDiff))
            {
                label.text = "✓";
                label.tooltip = "Matches the canvas settings.";
                label.style.color = new Color(0.4f, 0.8f, 0.4f);
                return;
            }

            label.text = "⚠";
            label.tooltip = info.CanvasDiff.Replace("; ", "\n");
            label.style.color = new Color(0.95f, 0.75f, 0.3f);
        }

        private void FixSceneStructure(string sceneAssetPath, UiElementSceneInfo info, UiCanvasSettings canvasSettings)
        {
            List<string> lines = UiElementStructureFixer.DescribePlannedChanges(info.Structure, UiElementStructureFixMode.WrapContent);
            UiElementSceneDialogs.AppendUnfixable(lines, UiElementStructureFixer.DescribeUnfixableIssues(info.Structure));

            bool isOpen = UiElementSceneUtility.IsSceneOpen(sceneAssetPath);
            string intro = "Fix the structure of '" + info.SceneName + "'? Objects are only created and reparented, never deleted." +
                (isOpen ? " The scene is open, so it is edited in place (with Undo) and not saved." : " The scene is saved afterwards.");

            UiElementStructureFixMode mode;
            if (!UiElementSceneDialogs.ConfirmStructureFix("Fix Structure", intro, lines, UiElementStructureFixer.OffersContentRootReset(info.Structure), out mode))
            {
                return;
            }

            UiElementStructureReport remaining = null;
            UiElementSceneUtility.WithScene(sceneAssetPath, true, scene =>
            {
                remaining = UiElementStructureFixer.FixStructure(scene, canvasSettings, mode);
            });

            if (remaining != null)
            {
                UiElementSceneDialogs.LogRemaining(info.SceneName, remaining);
            }

            RefreshSceneList();
        }

        private void ResetSceneCanvas(string sceneAssetPath, UiElementSceneInfo info, UiCanvasSettings canvasSettings)
        {
            List<string> lines = new List<string>(info.CanvasDiff.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries));
            bool isOpen = UiElementSceneUtility.IsSceneOpen(sceneAssetPath);
            string intro = "Reset the canvas in '" + info.SceneName + "' to the canvas settings? Sorting order and cameras are not changed." +
                (isOpen ? " The scene is open, so it is edited in place (with Undo) and not saved." : string.Empty);

            if (!UiElementSceneDialogs.Confirm("Reset Canvas", intro, lines, "Reset"))
            {
                return;
            }

            bool applied = false;
            UiElementSceneUtility.WithScene(sceneAssetPath, true, scene =>
            {
                UiElementStructureReport report;
                UiElementSceneUtility.ValidateStructure(scene, out report);
                if (report.Canvas == null)
                {
                    return;
                }

                UiElementSceneUtility.ApplyCanvasSettings(report.Canvas, report.Canvas.GetComponent<UnityEngine.UI.CanvasScaler>(), canvasSettings);
                applied = true;
            });

            if (applied)
            {
                Debug.Log("[UiFramework] Reset canvas settings in '" + info.SceneName + "' (" + info.CanvasDiff + ").");
            }

            RefreshSceneList();
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

            // Opening/creating the element scene replaces the open scenes, so give the user a chance to save first.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

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

            UiCanvasSettings canvasSettings = UiEditorConfigUtility.GetCanvasSettingsOrDefaults(editorConfig);

            if (sceneAsset == null)
            {
                UiElementStructureFixer.BuildContractHierarchy(rootGameObject, canvasSettings);
            }
            else
            {
                OfferStructureFixForExistingScene(openedScene, elementName, canvasSettings);
            }

            string finalScenePath = Path.Combine(scenePath, elementName + ".unity");
            Directory.CreateDirectory(scenePath);
            EditorSceneManager.SaveScene(openedScene, finalScenePath);
            AssetDatabase.Refresh();

            Debug.Log("\u2705 Created/updated scene: " + finalScenePath + ". Script will be attached to '" + rootName + "' after compilation.");
        }

        // Never restructures silently: fixable issues are listed and applied only after confirmation.
        private void OfferStructureFixForExistingScene(Scene scene, string elementName, UiCanvasSettings canvasSettings)
        {
            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);

            if (!report.HasFixableIssues)
            {
                return;
            }

            List<string> lines = UiElementStructureFixer.DescribePlannedChanges(report, UiElementStructureFixMode.WrapContent);
            List<string> unfixable = UiElementStructureFixer.DescribeUnfixableIssues(report);

            // The UiElement script is attached after compilation, so its absence is expected here.
            for (int i = unfixable.Count - 1; i >= 0; i--)
            {
                if (unfixable[i].StartsWith(UiElementStructureIssueCode.NoUiElement.ToString(), StringComparison.Ordinal))
                {
                    unfixable.RemoveAt(i);
                }
            }

            UiElementSceneDialogs.AppendUnfixable(lines, unfixable);

            UiElementStructureFixMode mode;
            string intro = "'" + elementName + "' does not follow the UI element scene contract yet. Apply these changes before saving? (You can also run 'Fix Structure' later from the Elements list.)";
            if (!UiElementSceneDialogs.ConfirmStructureFix("Fix Element Scene Structure", intro, lines, UiElementStructureFixer.OffersContentRootReset(report), out mode))
            {
                return;
            }

            UiElementStructureFixer.FixStructure(scene, canvasSettings, mode);
        }

        private List<string> GetElementScenePaths()
        {
            return UiElementSceneUtility.FindElementScenePaths(editorConfig);
        }
    }
}
