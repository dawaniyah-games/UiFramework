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
using UiFramework.Tweening;

namespace UiFramework.Editor.Window.Tabs
{
    public class ElementsTab : BaseVisualElementTab
    {
        private const string defaultUiElementsGroupName = "UiElements";
        private const string defaultUiElementsLabel = "UiElements";
        private const float nameColumnWidth = 200.0f;
        private const float statusColumnWidth = 72.0f;

        private static readonly Color okColor = new Color(0.4f, 0.8f, 0.4f);
        private static readonly Color warningColor = new Color(0.95f, 0.75f, 0.3f);
        private static readonly Color errorColor = new Color(0.9f, 0.35f, 0.35f);

        public override string TabName
        {
            get
            {
                return "Elements";
            }
        }

        private UiEditorConfig editorConfig;
        private ScrollView sceneListScrollView;
        private Label summaryLabel;
        private Label actionStatusLabel;
        private ToolbarButton fixAllButton;
        private ToolbarButton applyAllButton;
        private bool showOnlyIssues = false;
        private readonly List<UiElementSceneInfo> sceneInfos = new List<UiElementSceneInfo>();
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
                    marginBottom = 8
                }
            };
            root.Add(header);

            root.Add(CreateToolbar());

            VisualElement summaryRow = new VisualElement();
            summaryRow.style.flexDirection = FlexDirection.Row;
            summaryRow.style.flexWrap = Wrap.Wrap;
            summaryRow.style.marginTop = 4;
            summaryRow.style.marginBottom = 6;

            summaryLabel = new Label(string.Empty);
            summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            summaryLabel.style.marginRight = 12;
            summaryRow.Add(summaryLabel);

            actionStatusLabel = new Label(string.Empty);
            actionStatusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            actionStatusLabel.style.whiteSpace = WhiteSpace.Normal;
            summaryRow.Add(actionStatusLabel);
            root.Add(summaryRow);

            sceneListScrollView = new ScrollView();
            sceneListScrollView.mode = ScrollViewMode.VerticalAndHorizontal;
            sceneListScrollView.style.flexGrow = 1;
            sceneListScrollView.style.minHeight = 0;
            root.Add(sceneListScrollView);

            root.Add(CreateNewElementSection());

            RefreshSceneList();
        }

        private Toolbar CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            ToolbarButton refreshButton = new ToolbarButton(() =>
            {
                RefreshSceneList();
                SetActionStatus(string.Empty);
            })
            {
                text = "Refresh",
                tooltip = "Re-read every element scene."
            };
            toolbar.Add(refreshButton);

            ToolbarButton validateButton = new ToolbarButton(() =>
            {
                RefreshSceneList();
                SetActionStatus(UiElementSceneBatchOperations.LogValidationReport(sceneInfos) + " Details in the Console.");
            })
            {
                text = "Validate All",
                tooltip = "Re-check every element scene against the scene contract and the global canvas settings, and log a report to the Console."
            };
            toolbar.Add(validateButton);

            fixAllButton = new ToolbarButton(() =>
            {
                string result = UiElementSceneBatchOperations.FixStructureInAll(sceneInfos, GetCanvasSettings());
                RefreshSceneList();
                SetActionStatus(result);
            })
            {
                text = "Fix All Structure…",
                tooltip = "Fix every structure issue that can be fixed automatically, in all scenes. Shows the planned changes first. Never deletes objects."
            };
            toolbar.Add(fixAllButton);

            applyAllButton = new ToolbarButton(() =>
            {
                string result = UiElementSceneBatchOperations.ApplyCanvasSettingsToAll(sceneInfos, GetCanvasSettings());
                RefreshSceneList();
                SetActionStatus(result);
            })
            {
                text = "Apply Canvas To All…",
                tooltip = "Write the global canvas settings (General tab) into every scene whose Canvas differs. Excluded scenes are skipped. Shows the changes first."
            };
            toolbar.Add(applyAllButton);

            toolbar.Add(new ToolbarSpacer { flex = true });

            ToolbarToggle onlyIssuesToggle = new ToolbarToggle
            {
                text = "Only show scenes with issues",
                value = showOnlyIssues
            };
            onlyIssuesToggle.RegisterValueChangedCallback(evt =>
            {
                showOnlyIssues = evt.newValue;
                RenderSceneList();
            });
            toolbar.Add(onlyIssuesToggle);

            return toolbar;
        }

        private VisualElement CreateNewElementSection()
        {
            Foldout foldout = new Foldout
            {
                text = "Create New UI Element",
                value = true
            };
            foldout.style.marginTop = 8;
            foldout.style.unityFontStyleAndWeight = FontStyle.Bold;

            VisualElement body = new VisualElement();
            body.style.unityFontStyleAndWeight = FontStyle.Normal;
            foldout.Add(body);

            Label hint = new Label("Generates the UiElement script and a scene laid out as <Name>Root → Canvas → Root, using the global canvas settings from the General tab. Put all content under Root.");
            hint.style.whiteSpace = WhiteSpace.Normal;
            hint.style.fontSize = 11;
            hint.style.marginBottom = 6;
            body.Add(hint);

            SceneAsset selectedScene = null;

            ObjectField sceneField = new ObjectField("Optional Existing Scene")
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                tooltip = "Turn an existing scene into an element. Its root objects are moved under <Name>Root, and you are offered a structure fix.",
                style = { marginBottom = 6 }
            };
            sceneField.RegisterValueChangedCallback(evt =>
            {
                selectedScene = evt.newValue as SceneAsset;
            });
            body.Add(sceneField);

            TextField nameField = new TextField("New Element Name")
            {
                style = { marginTop = 4, marginBottom = 4 }
            };
            body.Add(nameField);

            Toggle paramsToggle = new Toggle("Include Parameters Class") { value = false };

            paramsToggle.RegisterValueChangedCallback(evt =>
            {
                includeParams = evt.newValue;
            });
            body.Add(paramsToggle);

            Toggle referenceToggle = new Toggle("Include Reference Class") { value = false };

            referenceToggle.RegisterValueChangedCallback(evt =>
            {
                includeReference = evt.newValue;
            });

            body.Add(referenceToggle);

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
                text = "➕ Create UI Element"
            };
            createButton.style.marginTop = 6;
            body.Add(createButton);

            return foldout;
        }

        // Opens every element scene once and caches what the list shows.
        private void RefreshSceneList()
        {
            sceneInfos.Clear();

            List<string> scenePaths = GetElementScenePaths();
            UiGlobalCanvasSettings canvasSettings = GetCanvasSettings();

            try
            {
                for (int i = 0; i < scenePaths.Count; i++)
                {
                    string sceneName = Path.GetFileNameWithoutExtension(scenePaths[i]);
                    EditorUtility.DisplayProgressBar("UI Elements", "Reading " + sceneName, (float)i / scenePaths.Count);

                    // One scene open per row: presets, structure and canvas status are read together.
                    sceneInfos.Add(UiElementSceneUtility.InspectScene(scenePaths[i], canvasSettings));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            RenderSceneList();
        }

        // Re-reads a single scene after a per-row action instead of reopening the whole list.
        private void RefreshScene(string scenePath)
        {
            for (int i = 0; i < sceneInfos.Count; i++)
            {
                if (sceneInfos[i].ScenePath == scenePath)
                {
                    sceneInfos[i] = UiElementSceneUtility.InspectScene(scenePath, GetCanvasSettings());
                    break;
                }
            }

            RenderSceneList();
        }

        // Rebuilds the rows from the cached scene info; does not open any scene.
        private void RenderSceneList()
        {
            if (sceneListScrollView == null)
            {
                return;
            }

            sceneListScrollView.Clear();
            UpdateSummary();

            if (sceneInfos.Count == 0)
            {
                string folder = editorConfig != null ? editorConfig.ElementsScenePath : "(no UI Config selected)";
                Label noSceneLabel = new Label("⚠️ No UI element scenes found in '" + folder + "'. Create one below.")
                {
                    style = { color = Color.yellow, marginBottom = 6 }
                };
                sceneListScrollView.Add(noSceneLabel);
                return;
            }

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

            UiGlobalCanvasSettings canvasSettings = GetCanvasSettings();
            sceneListScrollView.Add(CreateSceneListHeaderRow());

            int shown = 0;
            for (int i = 0; i < sceneInfos.Count; i++)
            {
                UiElementSceneInfo info = sceneInfos[i];
                if (showOnlyIssues && !NeedsAttention(info))
                {
                    continue;
                }

                sceneListScrollView.Add(CreateSceneRow(info.SceneName, info.ScenePath, hasAddressables, settings, groupNames, info, canvasSettings));
                shown++;
            }

            if (shown == 0)
            {
                Label allGood = new Label("✓ All " + sceneInfos.Count + " scenes follow the scene contract and match the global canvas settings.");
                allGood.style.color = okColor;
                allGood.style.marginTop = 6;
                sceneListScrollView.Add(allGood);
            }
        }

        private void UpdateSummary()
        {
            int ok = 0;
            int fixable = 0;
            int manual = 0;
            int canvasDiffers = 0;

            for (int i = 0; i < sceneInfos.Count; i++)
            {
                UiElementSceneInfo info = sceneInfos[i];
                UiElementStructureReport structure = info.Structure;

                if (info.IsAvailable && !NeedsAttention(info))
                {
                    ok++;
                }

                if (structure.HasFixableIssues)
                {
                    fixable++;
                }

                if (structure.HasUnfixableIssues)
                {
                    manual++;
                }

                if (info.CanvasStatus == UiCanvasStatus.Differs)
                {
                    canvasDiffers++;
                }
            }

            if (summaryLabel != null)
            {
                summaryLabel.text = sceneInfos.Count + " scene(s) · " + ok + " OK · " + fixable + " fixable · " + manual + " need manual work · " + canvasDiffers + " canvas differ";

                if (sceneInfos.Count > 0 && ok == sceneInfos.Count)
                {
                    summaryLabel.style.color = okColor;
                }
                else if (manual > 0)
                {
                    summaryLabel.style.color = errorColor;
                }
                else if (fixable + canvasDiffers > 0)
                {
                    summaryLabel.style.color = warningColor;
                }
                else
                {
                    summaryLabel.style.color = StyleKeyword.Null;
                }
            }

            if (fixAllButton != null)
            {
                fixAllButton.SetEnabled(fixable > 0);
            }

            if (applyAllButton != null)
            {
                applyAllButton.SetEnabled(canvasDiffers > 0);
            }
        }

        private static bool NeedsAttention(UiElementSceneInfo info)
        {
            return !info.IsAvailable || info.Structure.HasIssues || info.CanvasStatus == UiCanvasStatus.Differs;
        }

        private void SetActionStatus(string text)
        {
            if (actionStatusLabel != null)
            {
                actionStatusLabel.text = text ?? string.Empty;
            }
        }

        private UiGlobalCanvasSettings GetCanvasSettings()
        {
            return UiEditorConfigUtility.GetCanvasSettingsOrDefaults(editorConfig);
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

            row.Add(CreateHeaderLabel("Scene", "Click a scene name to select it, double-click to open it.", nameColumnWidth));
            row.Add(CreateHeaderLabel("Show", "Animation preset to play when this UI element is shown.", 110));
            row.Add(CreateHeaderLabel("Hide", "Animation preset to play when this UI element is hidden/unloaded.", 110));
            row.Add(CreateHeaderLabel("Structure", "Scene contract: <Name>Root → Canvas → one stretched Root.\n✓ OK   ⚠ Fix = click to fix   ✖ = needs manual work (click for details)", statusColumnWidth));
            row.Add(CreateHeaderLabel("Canvas", "Canvas/Canvas Scaler compared to the global canvas settings (General tab).\n✓ matches   ⚠ Reset = click to reset   — no canvas   Excluded = skipped by Apply Canvas To All", statusColumnWidth));
            row.Add(CreateHeaderLabel("Addressable", "If enabled, the scene will be registered in Addressables for loading at runtime.", 90));
            row.Add(CreateHeaderLabel("Group", null, 140));

            Label labelsLabel = CreateHeaderLabel("Labels", null, 0);
            labelsLabel.style.flexGrow = 1;
            row.Add(labelsLabel);

            return row;
        }

        private static Label CreateHeaderLabel(string text, string tooltip, float minWidth)
        {
            Label label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginRight = 8;

            if (minWidth > 0.0f)
            {
                label.style.minWidth = minWidth;
            }

            if (!string.IsNullOrEmpty(tooltip))
            {
                label.tooltip = tooltip;
            }

            return label;
        }

        private VisualElement CreateSceneRow(string sceneName, string sceneAssetPath, bool hasAddressables, AddressableAssetSettings settings, List<string> groupNames, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
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

            row.Add(CreateSceneNameLabel(sceneName, sceneAssetPath));

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
                info.ShowPreset = evt.newValue;
            });

            hideDropdown.RegisterValueChangedCallback(evt =>
            {
                if (!hasUiElement)
                {
                    return;
                }

                WriteElementAnimationPresets(sceneAssetPath, showDropdown.value, evt.newValue);
                info.HidePreset = evt.newValue;
            });

            row.Add(CreateStructureCell(sceneAssetPath, info, canvasSettings));
            row.Add(CreateCanvasCell(sceneAssetPath, info, canvasSettings));

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
                RenderSceneList();
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
                        RenderSceneList();
                    }
                }));
            });
            labelsButton.text = "Labels";
            labelsButton.SetEnabled(hasAddressables);
            labelsButton.style.marginRight = 8;
            row.Add(labelsButton);

            Button moreButton = new Button(() =>
            {
                ShowRowMenu(sceneAssetPath, info, canvasSettings, hasAddressables && isAddressable);
            })
            {
                text = "⋯",
                tooltip = "More actions for this scene"
            };
            moreButton.style.width = 28;
            row.Add(moreButton);

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

                RenderSceneList();
            });

            return row;
        }

        private Label CreateSceneNameLabel(string sceneName, string sceneAssetPath)
        {
            bool isOpen = UiElementSceneUtility.IsSceneOpen(sceneAssetPath);

            Label nameLabel = new Label(isOpen ? sceneName + "  (open)" : sceneName);
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.minWidth = nameColumnWidth;
            nameLabel.style.marginRight = 8;
            nameLabel.style.flexGrow = 0;
            nameLabel.tooltip = sceneAssetPath + "\nClick to select the scene asset, double-click to open it." +
                (isOpen ? "\nOpen in the editor: fixes are applied in place (with Undo) and not saved automatically." : string.Empty);

            nameLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount >= 2)
                {
                    OpenScene(sceneAssetPath);
                    return;
                }

                SelectSceneAsset(sceneAssetPath);
            });

            return nameLabel;
        }

        private VisualElement CreateStructureCell(string sceneAssetPath, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            UiElementStructureReport structure = info.Structure;

            if (!info.IsAvailable)
            {
                return CreateStatusCell("?", "The scene could not be opened (e.g. in Play Mode).", null, null);
            }

            if (!structure.HasIssues)
            {
                return CreateStatusCell("✓", "Follows the UI element scene contract.", okColor, null);
            }

            if (structure.HasFixableIssues)
            {
                return CreateStatusCell(
                    structure.HasUnfixableIssues ? "✖ Fix" : "⚠ Fix",
                    structure.GetSummaryText() + "\n\nClick to fix what can be fixed automatically (you'll see the changes first).",
                    structure.HasUnfixableIssues ? errorColor : warningColor,
                    () =>
                    {
                        FixSceneStructure(sceneAssetPath, info, canvasSettings);
                    });
            }

            return CreateStatusCell(
                structure.HasUnfixableIssues ? "✖" : "⚠",
                structure.GetSummaryText() + "\n\nClick for details.",
                structure.HasUnfixableIssues ? errorColor : warningColor,
                () =>
                {
                    ShowManualIssues(sceneAssetPath, info);
                });
        }

        private VisualElement CreateCanvasCell(string sceneAssetPath, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            if (!info.IsAvailable)
            {
                return CreateStatusCell("?", "The scene could not be opened.", null, null);
            }

            if (info.CanvasStatus == UiCanvasStatus.NoCanvas)
            {
                return CreateStatusCell("—", "No root Canvas under the element root. Use Fix Structure to create one.", null, null);
            }

            bool differs = !string.IsNullOrEmpty(info.CanvasDiff);
            string diffText = differs ? info.CanvasDiff.Replace("; ", "\n") : string.Empty;
            Action reset = null;

            if (differs)
            {
                reset = () =>
                {
                    ResetSceneCanvas(sceneAssetPath, info, canvasSettings);
                };
            }

            if (info.CanvasStatus == UiCanvasStatus.Excluded)
            {
                string tooltip = "Excluded from 'Apply Canvas To All' (change this in the ⋯ menu)." + (differs ? "\nDiffers:\n" + diffText + "\n\nClick to reset this scene anyway." : " Matches the global canvas settings.");
                return CreateStatusCell("Excluded", tooltip, Color.gray, reset);
            }

            if (!differs)
            {
                return CreateStatusCell("✓", "Matches the global canvas settings.", okColor, null);
            }

            return CreateStatusCell("⚠ Reset", diffText + "\n\nClick to write the global canvas settings into this scene (you'll see the changes first).", warningColor, reset);
        }

        // A clickable cell looks like a button; a read-only one looks like plain text.
        private static VisualElement CreateStatusCell(string text, string tooltip, Color? color, Action onClick)
        {
            VisualElement cell;

            if (onClick != null)
            {
                cell = new Button(onClick) { text = text };
            }
            else
            {
                Label label = new Label(text);
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                cell = label;
            }

            cell.tooltip = tooltip;
            cell.style.width = statusColumnWidth;
            cell.style.marginRight = 8;
            cell.style.unityFontStyleAndWeight = FontStyle.Bold;

            if (color.HasValue)
            {
                cell.style.color = color.Value;
            }

            return cell;
        }

        private void ShowRowMenu(string sceneAssetPath, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings, bool isAddressable)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Open Scene"), false, () =>
            {
                OpenScene(sceneAssetPath);
            });
            menu.AddItem(new GUIContent("Select Scene Asset"), false, () =>
            {
                SelectSceneAsset(sceneAssetPath);
            });
            menu.AddSeparator(string.Empty);

            AddMenuItem(menu, "Fix Structure…", info.IsAvailable && info.Structure.HasFixableIssues, () =>
            {
                FixSceneStructure(sceneAssetPath, info, canvasSettings);
            });

            bool canResetCanvas = info.IsAvailable && info.CanvasStatus != UiCanvasStatus.NoCanvas && !string.IsNullOrEmpty(info.CanvasDiff);
            AddMenuItem(menu, "Reset Canvas To Settings…", canResetCanvas, () =>
            {
                ResetSceneCanvas(sceneAssetPath, info, canvasSettings);
            });

            bool canExclude = canvasSettings != null && EditorUtility.IsPersistent(canvasSettings);
            bool excluded = canExclude && canvasSettings.IsExcluded(sceneAssetPath);
            GUIContent excludeContent = new GUIContent("Exclude From 'Apply Canvas To All'");
            if (canExclude)
            {
                menu.AddItem(excludeContent, excluded, () =>
                {
                    SetExcluded(info, canvasSettings, !excluded);
                });
            }
            else
            {
                menu.AddDisabledItem(excludeContent, excluded);
            }

            AddMenuItem(menu, "Log Validation Details", info.IsAvailable, () =>
            {
                Debug.Log("[UiFramework] " + info.SceneName + ": " + (info.Structure.HasIssues ? "\n" + info.Structure.GetIssuesText() : "structure OK") + "\nCanvas: " + UiElementSceneBatchOperations.DescribeCanvasStatus(info));
            });

            menu.AddSeparator(string.Empty);
            AddMenuItem(menu, "Remove From Addressables", isAddressable, () =>
            {
                AddressablesAssetUtility.RemoveAssetFromAddressables(sceneAssetPath);
                RenderSceneList();
            });

            menu.ShowAsContext();
        }

        private static void AddMenuItem(GenericMenu menu, string text, bool enabled, GenericMenu.MenuFunction action)
        {
            GUIContent content = new GUIContent(text);

            if (enabled)
            {
                menu.AddItem(content, false, action);
            }
            else
            {
                menu.AddDisabledItem(content);
            }
        }

        private void SetExcluded(UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings, bool excluded)
        {
            if (canvasSettings.SetExcluded(info.ScenePath, excluded))
            {
                AssetDatabase.SaveAssets();
            }

            if (info.CanvasStatus != UiCanvasStatus.NoCanvas)
            {
                if (excluded)
                {
                    info.CanvasStatus = UiCanvasStatus.Excluded;
                }
                else
                {
                    info.CanvasStatus = string.IsNullOrEmpty(info.CanvasDiff) ? UiCanvasStatus.Matches : UiCanvasStatus.Differs;
                }
            }

            RenderSceneList();
        }

        private void OpenScene(string sceneAssetPath)
        {
            if (UiElementSceneUtility.IsSceneOpen(sceneAssetPath))
            {
                SelectSceneAsset(sceneAssetPath);
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(sceneAssetPath, OpenSceneMode.Single);
            RenderSceneList();
        }

        private static void SelectSceneAsset(string sceneAssetPath)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneAssetPath);
            if (sceneAsset == null)
            {
                return;
            }

            Selection.activeObject = sceneAsset;
            EditorGUIUtility.PingObject(sceneAsset);
        }

        private void ShowManualIssues(string sceneAssetPath, UiElementSceneInfo info)
        {
            UiElementChangeEntry entry = new UiElementChangeEntry { Title = info.SceneName };
            entry.ManualWork.AddRange(UiElementStructureFixer.SummarizeManualWork(info.Structure));

            UiElementChangeDialogModel dialogModel = new UiElementChangeDialogModel
            {
                WindowTitle = "Structure",
                Heading = info.SceneName + " needs a manual fix",
                Description = "These can't be fixed automatically. Open the scene and change them by hand.",
                ConfirmLabel = "Open Scene",
                CancelLabel = "Close"
            };
            dialogModel.Entries.Add(entry);

            bool unused;
            if (UiElementChangeDialog.Show(dialogModel, out unused))
            {
                OpenScene(sceneAssetPath);
            }
        }

        // Builds the single-scene Fix Structure dialog. The report's scene must still be open (for the preview).
        private static UiElementChangeDialogModel BuildFixDialog(string sceneName, UiElementStructureReport report, bool willBeSaved, bool hideMissingUiElement)
        {
            UiElementChangeEntry entry = new UiElementChangeEntry { Title = sceneName };
            entry.Changes.AddRange(UiElementStructureFixer.DescribePlannedChanges(report, UiElementStructureFixMode.WrapContent));

            bool offersStretch = UiElementStructureFixer.OffersContentRootReset(report);
            if (offersStretch)
            {
                entry.AlternativeChanges = UiElementStructureFixer.DescribePlannedChanges(report, UiElementStructureFixMode.ResetContentRoot);
            }

            UiElementFixPreview preview = UiElementFixPreview.Build(report);
            if (preview != null)
            {
                entry.Before = preview.Before;
                entry.After = preview.AfterWrap;
                entry.AlternativeAfter = preview.AfterStretch;
            }

            for (int i = 0; i < report.Issues.Count; i++)
            {
                UiElementStructureIssue issue = report.Issues[i];
                bool hidden = hideMissingUiElement && issue.Code == UiElementStructureIssueCode.NoUiElement;
                if (!issue.IsFixable && !hidden)
                {
                    entry.ManualWork.Add(issue.Summary);
                }
            }

            string stretchTarget = offersStretch ? report.Find(UiElementStructureIssueCode.ContentRootNotStretched).ObjectName : null;

            UiElementChangeDialogModel dialogModel = new UiElementChangeDialogModel
            {
                WindowTitle = "Fix Structure",
                Heading = "Fix the structure of " + sceneName + "?",
                Description = "For the show/hide animation to move the whole screen, everything has to sit inside one full-screen Root under the Canvas.",
                OptionLabel = offersStretch ? "Stretch '" + stretchTarget + "' to fill the screen instead (changes its size and position)" : null,
                Footer = willBeSaved ? "Nothing is deleted. The scene will be saved." : "Nothing is deleted. The scene is open, so you can undo with Ctrl+Z. Save the scene to keep the changes.",
                ConfirmLabel = "Fix"
            };
            dialogModel.Entries.Add(entry);
            return dialogModel;
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

        private void FixSceneStructure(string sceneAssetPath, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            // Re-read the scene so the dialog's Now / After preview reflects its current state.
            bool isOpen = UiElementSceneUtility.IsSceneOpen(sceneAssetPath);
            UiElementStructureReport current = null;
            UiElementChangeDialogModel dialogModel = null;

            UiElementSceneUtility.WithScene(sceneAssetPath, false, scene =>
            {
                UiElementSceneUtility.ValidateStructure(scene, out current);
                dialogModel = BuildFixDialog(info.SceneName, current, !isOpen, false);
            });

            if (current == null || !current.HasFixableIssues)
            {
                RefreshScene(sceneAssetPath);
                return;
            }

            bool stretchInstead;
            if (!UiElementChangeDialog.Show(dialogModel, out stretchInstead))
            {
                return;
            }

            UiElementStructureFixMode mode = stretchInstead ? UiElementStructureFixMode.ResetContentRoot : UiElementStructureFixMode.WrapContent;
            UiElementStructureReport remaining = null;
            UiElementSceneUtility.WithScene(sceneAssetPath, true, scene =>
            {
                remaining = UiElementStructureFixer.FixStructure(scene, canvasSettings, mode);
            });

            if (remaining != null)
            {
                UiElementSceneDialogs.LogRemaining(info.SceneName, remaining);
                SetActionStatus(remaining.HasIssues ? "Fixed " + info.SceneName + "; some issues need manual work (see the row)." : "Fixed " + info.SceneName + ".");
            }

            RefreshScene(sceneAssetPath);
        }

        private void ResetSceneCanvas(string sceneAssetPath, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            bool isOpen = UiElementSceneUtility.IsSceneOpen(sceneAssetPath);

            UiElementChangeEntry entry = new UiElementChangeEntry { Title = info.SceneName };
            for (int i = 0; i < info.CanvasDifferences.Count; i++)
            {
                entry.SettingChanges.Add(info.CanvasDifferences[i]);
            }

            UiElementChangeDialogModel dialogModel = new UiElementChangeDialogModel
            {
                WindowTitle = "Reset Canvas",
                Heading = "Reset the canvas of " + info.SceneName + "?",
                Description = "These values change to match the Global Canvas Settings (General tab).",
                Footer = "Sorting order, cameras and everything else stay as they are." +
                    (isOpen ? " The scene is open, so you can undo with Ctrl+Z. Save the scene to keep the change." : " The scene will be saved."),
                ConfirmLabel = "Reset Canvas"
            };
            dialogModel.Entries.Add(entry);

            bool unused;
            if (!UiElementChangeDialog.Show(dialogModel, out unused))
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
                Debug.Log("[UiFramework] Reset global canvas settings in '" + info.SceneName + "' (" + info.CanvasDiff + ").");
                SetActionStatus("Reset canvas in " + info.SceneName + ".");
            }

            RefreshScene(sceneAssetPath);
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

            UiGlobalCanvasSettings canvasSettings = UiEditorConfigUtility.GetCanvasSettingsOrDefaults(editorConfig);

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
        private void OfferStructureFixForExistingScene(Scene scene, string elementName, UiGlobalCanvasSettings canvasSettings)
        {
            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);

            if (!report.HasFixableIssues)
            {
                return;
            }

            // The UiElement script is attached after compilation, so its absence is expected here and not listed.
            UiElementChangeDialogModel dialogModel = BuildFixDialog(elementName, report, true, true);
            dialogModel.Description = "This scene doesn't follow the UI element layout yet. " + dialogModel.Description + " You can also fix it later from the Elements list.";

            bool stretchInstead;
            if (!UiElementChangeDialog.Show(dialogModel, out stretchInstead))
            {
                return;
            }

            UiElementStructureFixer.FixStructure(scene, canvasSettings, stretchInstead ? UiElementStructureFixMode.ResetContentRoot : UiElementStructureFixMode.WrapContent);
        }

        private List<string> GetElementScenePaths()
        {
            return UiElementSceneUtility.FindElementScenePaths(editorConfig);
        }
    }
}
