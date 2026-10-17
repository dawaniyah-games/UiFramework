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
    // Element list on the left (search, status, double-click to open), settings of the selected element on the right.
    public class ElementsTab : BaseVisualElementTab
    {
        private const string defaultUiElementsGroupName = "UiElements";
        private const string defaultUiElementsLabel = "UiElements";
        private const float fieldLabelWidth = 170.0f;

        private static readonly Color okColor = new Color(0.4f, 0.8f, 0.4f);
        private static readonly Color warningColor = new Color(0.95f, 0.75f, 0.3f);
        private static readonly Color errorColor = new Color(0.9f, 0.35f, 0.35f);
        private static readonly Color mutedColor = new Color(0.6f, 0.6f, 0.6f);

        public override string TabName
        {
            get
            {
                return "Elements";
            }
        }

        private UiEditorConfig editorConfig;
        private ScrollView listView;
        private ScrollView detailView;
        private Label summaryLabel;
        private Label actionStatusLabel;
        private ToolbarButton fixAllButton;
        private string searchQuery = string.Empty;
        private bool showOnlyIssues = false;
        private bool showCreatePanel = false;
        private string selectedScenePath;
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

            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;

            CreateTopBar(root);
            root.Add(CreateToolbar());
            CreateSummaryRow(root);
            CreateContentArea(root);

            RefreshSceneList();
        }

        // ---------- Layout ----------

        private void CreateTopBar(VisualElement root)
        {
            VisualElement topBar = new VisualElement();
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.alignItems = Align.Center;
            topBar.style.paddingTop = 4;
            topBar.style.paddingBottom = 4;
            topBar.style.paddingLeft = 10;
            topBar.style.paddingRight = 10;

            Label title = new Label("UI Elements");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 13;
            title.style.marginRight = 10;
            topBar.Add(title);

            ToolbarSearchField searchField = new ToolbarSearchField();
            searchField.tooltip = "Search by name or part of the name. Press Enter to select the first result.";
            searchField.style.flexGrow = 1;
            searchField.style.maxWidth = 360;
            searchField.value = searchQuery;
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchQuery = evt.newValue ?? string.Empty;
                RenderList();
            });
            searchField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    List<UiElementSceneInfo> results = GetVisibleInfos();
                    if (results.Count > 0)
                    {
                        Select(results[0].ScenePath);
                    }
                }
            });
            topBar.Add(searchField);

            Button newButton = new Button(() =>
            {
                showCreatePanel = true;
                RenderList();
                RenderDetail();
            })
            {
                text = "➕ New Element",
                tooltip = "Create a new UI element (script + scene)"
            };
            newButton.style.marginLeft = 8;
            newButton.style.height = 24;
            topBar.Add(newButton);

            root.Add(topBar);
        }

        private Toolbar CreateToolbar()
        {
            Toolbar toolbar = new Toolbar();

            toolbar.Add(new ToolbarButton(() =>
            {
                RefreshSceneList();
                SetActionStatus(string.Empty);
            })
            {
                text = "Refresh",
                tooltip = "Re-read every element scene."
            });

            toolbar.Add(new ToolbarButton(() =>
            {
                RefreshSceneList();
                SetActionStatus(UiElementSceneBatchOperations.LogValidationReport(sceneInfos) + " Details in the Console.");
            })
            {
                text = "Validate All",
                tooltip = "Re-check every element scene's structure and log a report to the Console."
            });

            fixAllButton = new ToolbarButton(() =>
            {
                string result = UiElementSceneBatchOperations.FixStructureInAll(sceneInfos, GetCanvasSettings());
                RefreshSceneList();
                SetActionStatus(result);
            })
            {
                text = "Fix All Structure…",
                tooltip = "Fix every structure issue that can be fixed automatically, in all scenes. Shows the changes first. Never deletes or resizes objects."
            };
            toolbar.Add(fixAllButton);

            toolbar.Add(new ToolbarSpacer { flex = true });

            ToolbarToggle onlyIssuesToggle = new ToolbarToggle
            {
                text = "Only show elements with issues",
                value = showOnlyIssues
            };
            onlyIssuesToggle.RegisterValueChangedCallback(evt =>
            {
                showOnlyIssues = evt.newValue;
                RenderList();
            });
            toolbar.Add(onlyIssuesToggle);

            return toolbar;
        }

        private void CreateSummaryRow(VisualElement root)
        {
            VisualElement summaryRow = new VisualElement();
            summaryRow.style.flexDirection = FlexDirection.Row;
            summaryRow.style.flexWrap = Wrap.Wrap;
            summaryRow.style.marginTop = 4;
            summaryRow.style.marginLeft = 10;

            summaryLabel = new Label(string.Empty);
            summaryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            summaryLabel.style.marginRight = 12;
            summaryRow.Add(summaryLabel);

            actionStatusLabel = new Label(string.Empty);
            actionStatusLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            actionStatusLabel.style.whiteSpace = WhiteSpace.Normal;
            summaryRow.Add(actionStatusLabel);

            root.Add(summaryRow);
        }

        private void CreateContentArea(VisualElement root)
        {
            VisualElement container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexGrow = 1;
            container.style.marginTop = 8;

            listView = new ScrollView(ScrollViewMode.Vertical);
            listView.style.width = new Length(30, LengthUnit.Percent);
            listView.style.minWidth = 200;
            listView.style.marginRight = 8;
            listView.style.flexGrow = 0;
            listView.style.minHeight = 0;

            detailView = new ScrollView(ScrollViewMode.Vertical);
            detailView.style.flexGrow = 1;
            detailView.style.minHeight = 0;
            detailView.style.paddingRight = 8;

            container.Add(listView);
            container.Add(detailView);
            root.Add(container);
        }

        // ---------- Data ----------

        // Opens every element scene once and caches what the tab shows.
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

                    // One scene open per element: presets, structure and canvas are read together.
                    sceneInfos.Add(UiElementSceneUtility.InspectScene(scenePaths[i], canvasSettings));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (FindInfo(selectedScenePath) == null)
            {
                selectedScenePath = sceneInfos.Count > 0 ? sceneInfos[0].ScenePath : null;
            }

            RenderSceneList();
        }

        // Re-reads a single scene after an action on it instead of reopening the whole list.
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

        private UiElementSceneInfo FindInfo(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return null;
            }

            for (int i = 0; i < sceneInfos.Count; i++)
            {
                if (sceneInfos[i].ScenePath == scenePath)
                {
                    return sceneInfos[i];
                }
            }

            return null;
        }

        private List<UiElementSceneInfo> GetVisibleInfos()
        {
            List<UiElementSceneInfo> results = UiSearchFilter.Filter(sceneInfos, info => info.SceneName, searchQuery);

            if (showOnlyIssues)
            {
                results.RemoveAll(info => !NeedsAttention(info));
            }

            return results;
        }

        private static bool NeedsAttention(UiElementSceneInfo info)
        {
            return !info.IsAvailable || info.Structure.HasIssues;
        }

        private UiGlobalCanvasSettings GetCanvasSettings()
        {
            return UiEditorConfigUtility.GetCanvasSettingsOrDefaults(editorConfig);
        }

        private void SetActionStatus(string text)
        {
            if (actionStatusLabel != null)
            {
                actionStatusLabel.text = text ?? string.Empty;
            }
        }

        private void Select(string scenePath)
        {
            selectedScenePath = scenePath;
            showCreatePanel = false;
            RenderList();
            RenderDetail();
        }

        // ---------- Rendering (no scene is opened here) ----------

        private void RenderSceneList()
        {
            RenderList();
            RenderDetail();
            UpdateSummary();
        }

        private void RenderList()
        {
            if (listView == null)
            {
                return;
            }

            listView.Clear();

            if (sceneInfos.Count == 0)
            {
                string folder = editorConfig != null ? editorConfig.ElementsScenePath : "(no UI Config selected)";
                Label empty = new Label("No UI element scenes in '" + folder + "' yet. Use ➕ New Element to create one.");
                empty.style.whiteSpace = WhiteSpace.Normal;
                empty.style.color = mutedColor;
                listView.Add(empty);
                return;
            }

            List<UiElementSceneInfo> visible = GetVisibleInfos();

            if (!string.IsNullOrWhiteSpace(searchQuery) || showOnlyIssues)
            {
                Label count = new Label(visible.Count + " of " + sceneInfos.Count);
                count.style.color = mutedColor;
                count.style.fontSize = 11;
                count.style.marginBottom = 4;
                listView.Add(count);
            }

            if (visible.Count == 0)
            {
                Label none = new Label(string.IsNullOrWhiteSpace(searchQuery) ? "Every element follows the layout." : "No elements match '" + searchQuery.Trim() + "'.");
                none.style.whiteSpace = WhiteSpace.Normal;
                none.style.color = mutedColor;
                listView.Add(none);
                return;
            }

            for (int i = 0; i < visible.Count; i++)
            {
                listView.Add(CreateListButton(visible[i]));
            }
        }

        private Button CreateListButton(UiElementSceneInfo info)
        {
            string scenePath = info.ScenePath;
            Button button = new Button();
            button.style.flexDirection = FlexDirection.Row;
            button.style.justifyContent = Justify.FlexStart;
            button.style.height = 24;
            button.tooltip = scenePath + (info.Structure.HasIssues ? "\n" + info.Structure.GetSummaryText() : string.Empty) + "\nDouble-click to open the scene.";

            Color iconColor;
            Label icon = new Label(GetStatusIcon(info, out iconColor));
            icon.style.color = iconColor;
            icon.style.width = 18;
            icon.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.Add(icon);

            Label name = new Label(info.SceneName + (UiElementSceneUtility.IsSceneOpen(scenePath) ? "  (open)" : string.Empty));
            name.style.unityTextAlign = TextAnchor.MiddleLeft;
            button.Add(name);

            if (!showCreatePanel && scenePath == selectedScenePath)
            {
                button.AddToClassList("tab-button-selected");
            }

            button.clickable.clickedWithEventInfo += evt =>
            {
                if (GetClickCount(evt) >= 2)
                {
                    OpenScene(scenePath);
                    return;
                }

                Select(scenePath);
            };

            return button;
        }

        private static int GetClickCount(EventBase evt)
        {
            IMouseEvent mouse = evt as IMouseEvent;
            if (mouse != null)
            {
                return mouse.clickCount;
            }

            IPointerEvent pointer = evt as IPointerEvent;
            return pointer != null ? pointer.clickCount : 1;
        }

        private static string GetStatusIcon(UiElementSceneInfo info, out Color color)
        {
            if (!info.IsAvailable)
            {
                color = mutedColor;
                return "?";
            }

            if (!info.Structure.HasIssues)
            {
                color = okColor;
                return "✓";
            }

            if (info.Structure.HasUnfixableIssues)
            {
                color = errorColor;
                return "✖";
            }

            color = warningColor;
            return "⚠";
        }

        private void UpdateSummary()
        {
            int ok = 0;
            int fixable = 0;
            int manual = 0;

            for (int i = 0; i < sceneInfos.Count; i++)
            {
                UiElementSceneInfo info = sceneInfos[i];

                if (info.IsAvailable && !NeedsAttention(info))
                {
                    ok++;
                }

                if (info.Structure.HasFixableIssues)
                {
                    fixable++;
                }

                if (info.Structure.HasUnfixableIssues)
                {
                    manual++;
                }
            }

            if (summaryLabel != null)
            {
                summaryLabel.text = sceneInfos.Count + " element(s) · " + ok + " OK · " + fixable + " fixable · " + manual + " need manual work";

                if (sceneInfos.Count > 0 && ok == sceneInfos.Count)
                {
                    summaryLabel.style.color = okColor;
                }
                else if (manual > 0)
                {
                    summaryLabel.style.color = errorColor;
                }
                else if (fixable > 0)
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
        }

        // ---------- Detail panel ----------

        private void RenderDetail()
        {
            if (detailView == null)
            {
                return;
            }

            detailView.Clear();

            if (showCreatePanel)
            {
                detailView.Add(CreateNewElementPanel());
                return;
            }

            UiElementSceneInfo info = FindInfo(selectedScenePath);
            if (info == null)
            {
                Label hint = new Label("Select an element on the left, or create one with ➕ New Element.");
                hint.style.color = mutedColor;
                detailView.Add(hint);
                return;
            }

            detailView.Add(CreateDetailHeader(info));

            if (!info.IsAvailable)
            {
                Label unavailable = new Label("This scene couldn't be read (for example while in Play Mode). Press Refresh to try again.");
                unavailable.style.whiteSpace = WhiteSpace.Normal;
                unavailable.style.color = warningColor;
                detailView.Add(unavailable);
                return;
            }

            UiGlobalCanvasSettings canvasSettings = GetCanvasSettings();
            detailView.Add(CreateStructureSection(info, canvasSettings));
            detailView.Add(CreateAnimationSection(info));
            detailView.Add(CreateCanvasSection(info, canvasSettings));
            detailView.Add(CreateAddressablesSection(info));
        }

        private VisualElement CreateDetailHeader(UiElementSceneInfo info)
        {
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.flexWrap = Wrap.Wrap;
            header.style.marginBottom = 6;

            Label title = new Label(info.SceneName);
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 15;
            title.style.marginRight = 10;
            header.Add(title);

            bool isOpen = UiElementSceneUtility.IsSceneOpen(info.ScenePath);
            if (isOpen)
            {
                Label openBadge = new Label("open in editor");
                openBadge.style.color = okColor;
                openBadge.style.unityFontStyleAndWeight = FontStyle.Italic;
                openBadge.style.marginRight = 10;
                openBadge.tooltip = "Changes made here are applied to the open scene (with Undo). Save the scene to keep them.";
                header.Add(openBadge);
            }

            VisualElement spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            header.Add(spacer);

            Button openButton = new Button(() =>
            {
                OpenScene(info.ScenePath);
            })
            {
                text = isOpen ? "Show Scene" : "Open Scene",
                tooltip = isOpen ? "The scene is already open. Selects it in the Project window." : "Open this scene to edit it (asks to save your current scenes first)."
            };
            openButton.style.height = 26;
            openButton.style.minWidth = 110;
            openButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(openButton);

            Button selectButton = new Button(() =>
            {
                SelectSceneAsset(info.ScenePath);
            })
            {
                text = "Select Asset",
                tooltip = "Highlight the scene asset in the Project window."
            };
            selectButton.style.height = 26;
            header.Add(selectButton);

            VisualElement wrapper = new VisualElement();
            wrapper.Add(header);

            Label path = new Label(info.ScenePath);
            path.style.color = mutedColor;
            path.style.fontSize = 11;
            path.style.marginBottom = 4;
            wrapper.Add(path);

            return wrapper;
        }

        private static VisualElement CreateSection(string title, string subtitle)
        {
            VisualElement section = new VisualElement();
            section.style.marginTop = 10;
            section.style.paddingLeft = 10;
            section.style.paddingRight = 10;
            section.style.paddingTop = 8;
            section.style.paddingBottom = 8;
            section.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.12f);
            section.style.borderTopLeftRadius = 4;
            section.style.borderTopRightRadius = 4;
            section.style.borderBottomLeftRadius = 4;
            section.style.borderBottomRightRadius = 4;

            Label header = new Label(title);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.fontSize = 13;
            section.Add(header);

            if (!string.IsNullOrEmpty(subtitle))
            {
                Label hint = new Label(subtitle);
                hint.style.whiteSpace = WhiteSpace.Normal;
                hint.style.color = mutedColor;
                hint.style.fontSize = 11;
                hint.style.marginBottom = 6;
                section.Add(hint);
            }

            return section;
        }

        private static Label CreateLine(string text, Color? color)
        {
            Label label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginTop = 2;

            if (color.HasValue)
            {
                label.style.color = color.Value;
            }

            return label;
        }

        private static void SetLabelWidth(VisualElement field)
        {
            Label label = field.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = fieldLabelWidth;
                label.style.width = fieldLabelWidth;
            }
        }

        private VisualElement CreateStructureSection(UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            VisualElement section = CreateSection("Structure", "<Name>Root → Canvas → one full-screen Root that holds all content, so show/hide animations move the whole screen.");
            UiElementStructureReport structure = info.Structure;

            if (!structure.HasIssues)
            {
                section.Add(CreateLine("✓ Follows the UI element layout.", okColor));
                return section;
            }

            for (int i = 0; i < structure.Issues.Count; i++)
            {
                UiElementStructureIssue issue = structure.Issues[i];
                Color color = issue.IsFixable || issue.IsWarning ? warningColor : errorColor;
                section.Add(CreateLine("• " + issue.Summary + (issue.IsFixable ? string.Empty : "  (fix by hand)"), color));
            }

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 8;

            if (structure.HasFixableIssues)
            {
                Button fixButton = new Button(() =>
                {
                    FixSceneStructure(info.ScenePath, info, canvasSettings);
                })
                {
                    text = "Fix Structure…",
                    tooltip = "Shows exactly what will change before anything happens."
                };
                fixButton.style.height = 24;
                fixButton.style.unityFontStyleAndWeight = FontStyle.Bold;
                buttons.Add(fixButton);
            }

            if (structure.HasUnfixableIssues)
            {
                Button detailsButton = new Button(() =>
                {
                    ShowManualIssues(info.ScenePath, info);
                })
                {
                    text = "What to fix by hand…"
                };
                detailsButton.style.height = 24;
                buttons.Add(detailsButton);
            }

            section.Add(buttons);
            return section;
        }

        private VisualElement CreateAnimationSection(UiElementSceneInfo info)
        {
            VisualElement section = CreateSection("Animations", "Played on the whole screen (the full-screen Root) when this element is shown or hidden.");

            if (!info.HasUiElement)
            {
                section.Add(CreateLine("Add the UiElement script to the root object to choose animations.", warningColor));
                return section;
            }

            List<string> presetKeys = new List<string>(TweenPresets.GetPresetKeys());
            if (presetKeys.Count == 0)
            {
                presetKeys.Add(TweenPresets.NoneKey);
            }

            string currentShow = string.IsNullOrWhiteSpace(info.ShowPreset) || !presetKeys.Contains(info.ShowPreset) ? TweenPresets.NoneKey : info.ShowPreset;
            string currentHide = string.IsNullOrWhiteSpace(info.HidePreset) || !presetKeys.Contains(info.HidePreset) ? TweenPresets.NoneKey : info.HidePreset;

            DropdownField showDropdown = new DropdownField("Show", presetKeys, currentShow);
            DropdownField hideDropdown = new DropdownField("Hide", presetKeys, currentHide);
            SetLabelWidth(showDropdown);
            SetLabelWidth(hideDropdown);

            showDropdown.RegisterValueChangedCallback(evt =>
            {
                WriteElementAnimationPresets(info.ScenePath, evt.newValue, hideDropdown.value);
                info.ShowPreset = evt.newValue;
            });

            hideDropdown.RegisterValueChangedCallback(evt =>
            {
                WriteElementAnimationPresets(info.ScenePath, showDropdown.value, evt.newValue);
                info.HidePreset = evt.newValue;
            });

            section.Add(showDropdown);
            section.Add(hideDropdown);
            return section;
        }

        private VisualElement CreateCanvasSection(UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            VisualElement section = CreateSection("Canvas", "This element's own Canvas. New elements start with the Global Canvas Settings (General tab).");
            UiCanvasSnapshot canvas = info.Canvas;
            string path = info.ScenePath;

            if (canvas == null)
            {
                section.Add(CreateLine("No Canvas yet. Use Fix Structure above to create one.", warningColor));
                return section;
            }

            EnumField renderMode = new EnumField("Render Mode", canvas.RenderMode);
            SetLabelWidth(renderMode);
            renderMode.RegisterValueChangedCallback(evt =>
            {
                RenderMode value = (RenderMode)evt.newValue;
                EditCanvas(path, (c, s) => { c.renderMode = value; });
            });
            section.Add(renderMode);

            IntegerField sortingOrder = new IntegerField("Sorting Order") { value = canvas.SortingOrder, isDelayed = true };
            sortingOrder.tooltip = "Higher values draw on top. Use it to stack popups above other screens.";
            SetLabelWidth(sortingOrder);
            sortingOrder.RegisterValueChangedCallback(evt =>
            {
                int value = evt.newValue;
                EditCanvas(path, (c, s) => { c.sortingOrder = value; });
            });
            section.Add(sortingOrder);

            if (!canvas.HasScaler)
            {
                section.Add(CreateLine("No Canvas Scaler. Use Fix Structure above to add one.", warningColor));
            }
            else
            {
                EnumField scaleMode = new EnumField("UI Scale Mode", canvas.UiScaleMode);
                SetLabelWidth(scaleMode);
                scaleMode.RegisterValueChangedCallback(evt =>
                {
                    UnityEngine.UI.CanvasScaler.ScaleMode value = (UnityEngine.UI.CanvasScaler.ScaleMode)evt.newValue;
                    EditCanvas(path, (c, s) => { s.uiScaleMode = value; });
                });
                section.Add(scaleMode);

                if (canvas.UiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    section.Add(CreateResolutionRow(path, canvas.ReferenceResolution));

                    EnumField matchMode = new EnumField("Screen Match Mode", canvas.ScreenMatchMode);
                    SetLabelWidth(matchMode);
                    matchMode.RegisterValueChangedCallback(evt =>
                    {
                        UnityEngine.UI.CanvasScaler.ScreenMatchMode value = (UnityEngine.UI.CanvasScaler.ScreenMatchMode)evt.newValue;
                        EditCanvas(path, (c, s) => { s.screenMatchMode = value; });
                    });
                    section.Add(matchMode);

                    if (canvas.ScreenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                    {
                        FloatField match = new FloatField("Match (0 = width, 1 = height)") { value = canvas.MatchWidthOrHeight, isDelayed = true };
                        SetLabelWidth(match);
                        match.RegisterValueChangedCallback(evt =>
                        {
                            float value = Mathf.Clamp01(evt.newValue);
                            EditCanvas(path, (c, s) => { s.matchWidthOrHeight = value; });
                        });
                        section.Add(match);
                    }
                }

                FloatField pixelsPerUnit = new FloatField("Reference Pixels Per Unit") { value = canvas.ReferencePixelsPerUnit, isDelayed = true };
                SetLabelWidth(pixelsPerUnit);
                pixelsPerUnit.RegisterValueChangedCallback(evt =>
                {
                    float value = Mathf.Max(0.01f, evt.newValue);
                    EditCanvas(path, (c, s) => { s.referencePixelsPerUnit = value; });
                });
                section.Add(pixelsPerUnit);
            }

            VisualElement defaultsRow = new VisualElement();
            defaultsRow.style.flexDirection = FlexDirection.Row;
            defaultsRow.style.alignItems = Align.Center;
            defaultsRow.style.marginTop = 8;

            bool matchesDefaults = info.CanvasDifferences.Count == 0;
            Button useDefaults = new Button(() =>
            {
                ApplyDefaultCanvasSettings(path, info, canvasSettings);
            })
            {
                text = "Use Default Settings…",
                tooltip = "Set this Canvas to the Global Canvas Settings (General tab). Shows the changes first."
            };
            useDefaults.style.height = 24;
            useDefaults.SetEnabled(!matchesDefaults);
            defaultsRow.Add(useDefaults);

            Label defaultsNote = new Label(matchesDefaults ? "✓ Same as the default settings." : info.CanvasDifferences.Count + " value(s) differ from the defaults.");
            defaultsNote.style.color = matchesDefaults ? okColor : mutedColor;
            defaultsNote.style.marginLeft = 8;
            defaultsRow.Add(defaultsNote);

            section.Add(defaultsRow);
            return section;
        }

        private VisualElement CreateResolutionRow(string path, Vector2 resolution)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;

            IntegerField width = new IntegerField("Reference Resolution") { value = Mathf.RoundToInt(resolution.x), isDelayed = true };
            SetLabelWidth(width);
            width.style.flexGrow = 1;
            row.Add(width);

            Label times = new Label("×");
            times.style.marginLeft = 4;
            times.style.marginRight = 4;
            row.Add(times);

            IntegerField height = new IntegerField { value = Mathf.RoundToInt(resolution.y), isDelayed = true };
            height.style.width = 80;
            row.Add(height);

            width.RegisterValueChangedCallback(evt =>
            {
                Vector2 value = new Vector2(Mathf.Max(1, evt.newValue), height.value);
                EditCanvas(path, (c, s) => { s.referenceResolution = value; });
            });

            height.RegisterValueChangedCallback(evt =>
            {
                Vector2 value = new Vector2(width.value, Mathf.Max(1, evt.newValue));
                EditCanvas(path, (c, s) => { s.referenceResolution = value; });
            });

            return row;
        }

        private void EditCanvas(string scenePath, Action<Canvas, UnityEngine.UI.CanvasScaler> edit)
        {
            // Scaler fields are only shown when the Canvas has a Canvas Scaler, so edits never see a missing one.
            bool edited = UiElementSceneUtility.EditCanvas(scenePath, "Change Canvas", edit);

            if (!edited)
            {
                Debug.LogWarning("[UiFramework] Couldn't change the Canvas of '" + Path.GetFileNameWithoutExtension(scenePath) + "': it has no root Canvas.");
            }

            RefreshScene(scenePath);
        }

        private VisualElement CreateAddressablesSection(UiElementSceneInfo info)
        {
            VisualElement section = CreateSection("Addressables", "Element scenes must be Addressable so the UiManager can load them at runtime.");
            string scenePath = info.ScenePath;
            string sceneName = info.SceneName;

            AddressableAssetSettings settings;
            bool hasAddressables = AddressablesAssetUtility.TryGetSettings(out settings);
            if (!hasAddressables)
            {
                section.Add(CreateLine("Addressables isn't set up in this project (Window → Asset Management → Addressables → Groups).", warningColor));
                return section;
            }

            List<string> groupNames = AddressablesAssetUtility.GetGroupNames(settings);
            if (groupNames.Count == 0)
            {
                groupNames.Add(defaultUiElementsGroupName);
            }
            else if (!groupNames.Contains(defaultUiElementsGroupName))
            {
                groupNames.Insert(0, defaultUiElementsGroupName);
            }

            AddressableAssetEntry entry = null;
            bool isAddressable = AddressablesAssetUtility.IsAssetAddressable(scenePath, out entry);

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

            string selectedGroup = defaultUiElementsGroupName;
            if (isAddressable && entry != null && entry.parentGroup != null)
            {
                selectedGroup = entry.parentGroup.Name;
            }

            if (!groupNames.Contains(selectedGroup))
            {
                selectedGroup = groupNames[0];
            }

            Toggle addressableToggle = new Toggle("Addressable") { value = isAddressable };
            SetLabelWidth(addressableToggle);
            section.Add(addressableToggle);

            DropdownField groupDropdown = new DropdownField("Group", groupNames, selectedGroup);
            SetLabelWidth(groupDropdown);
            groupDropdown.SetEnabled(isAddressable);
            section.Add(groupDropdown);

            VisualElement labelsRow = new VisualElement();
            labelsRow.style.flexDirection = FlexDirection.Row;
            labelsRow.style.alignItems = Align.Center;
            labelsRow.style.marginLeft = 3;

            Label labelsCaption = new Label("Labels");
            labelsCaption.style.width = fieldLabelWidth;
            labelsCaption.style.minWidth = fieldLabelWidth;
            labelsRow.Add(labelsCaption);

            Label labelsSummary = new Label(BuildLabelsSummary(selectedLabels));
            labelsSummary.style.flexGrow = 1;
            labelsRow.Add(labelsSummary);

            Button labelsButton = new Button();
            labelsButton.text = "Edit Labels…";
            labelsButton.SetEnabled(isAddressable);
            labelsButton.clicked += () =>
            {
                UnityEditor.PopupWindow.Show(labelsButton.worldBound, new AddressablesLabelsPopup(settings, selectedLabels, _ =>
                {
                    labelsSummary.text = BuildLabelsSummary(selectedLabels);
                    AddressablesAssetUtility.EnsureAssetIsAddressable(scenePath, groupDropdown.value, sceneName, selectedLabels.ToArray());
                }));
            };
            labelsRow.Add(labelsButton);
            section.Add(labelsRow);

            addressableToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                {
                    AddressablesAssetUtility.EnsureAssetIsAddressable(scenePath, groupDropdown.value, sceneName, selectedLabels.ToArray());
                }
                else
                {
                    AddressablesAssetUtility.RemoveAssetFromAddressables(scenePath);
                }

                RenderDetail();
            });

            groupDropdown.RegisterValueChangedCallback(evt =>
            {
                AddressablesAssetUtility.EnsureAssetIsAddressable(scenePath, evt.newValue, sceneName, selectedLabels.ToArray());
            });

            if (!isAddressable)
            {
                section.Add(CreateLine("Not Addressable yet: the UiManager can't load it until you turn this on (or run Build Runtime UiConfig).", warningColor));
            }

            return section;
        }

        private VisualElement CreateNewElementPanel()
        {
            VisualElement section = CreateSection("New UI Element", "Generates the UiElement script and a scene laid out as <Name>Root → Canvas → Root, using the Global Canvas Settings (General tab). Put all content under Root.");

            SceneAsset selectedScene = null;

            TextField nameField = new TextField("Name")
            {
                tooltip = "Class and scene name, e.g. ProfileUiElement"
            };
            SetLabelWidth(nameField);
            section.Add(nameField);

            ObjectField sceneField = new ObjectField("From Existing Scene (optional)")
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                tooltip = "Turn an existing scene into an element. Its root objects are moved under <Name>Root, and you are offered a structure fix."
            };
            SetLabelWidth(sceneField);
            sceneField.RegisterValueChangedCallback(evt =>
            {
                selectedScene = evt.newValue as SceneAsset;
            });
            section.Add(sceneField);

            Toggle paramsToggle = new Toggle("Include Parameters Class") { value = includeParams };
            SetLabelWidth(paramsToggle);
            paramsToggle.RegisterValueChangedCallback(evt =>
            {
                includeParams = evt.newValue;
            });
            section.Add(paramsToggle);

            Toggle referenceToggle = new Toggle("Include Reference Class") { value = includeReference };
            SetLabelWidth(referenceToggle);
            referenceToggle.RegisterValueChangedCallback(evt =>
            {
                includeReference = evt.newValue;
            });
            section.Add(referenceToggle);

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = 8;

            Button cancel = new Button(() =>
            {
                showCreatePanel = false;
                RenderList();
                RenderDetail();
            })
            {
                text = "Cancel"
            };
            cancel.style.minWidth = 90;
            cancel.style.height = 26;
            buttons.Add(cancel);

            Button create = new Button(() =>
            {
                string elementName = nameField.value != null ? nameField.value.Trim() : string.Empty;

                if (selectedScene != null && string.IsNullOrEmpty(elementName))
                {
                    elementName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(selectedScene));
                }

                if (string.IsNullOrEmpty(elementName))
                {
                    EditorUtility.DisplayDialog("New UI Element", "Enter a name for the new element.", "OK");
                    return;
                }

                CreateUiElementFromScene(elementName, selectedScene);

                string scenesFolder = editorConfig != null ? editorConfig.ElementsScenePath : string.Empty;
                selectedScenePath = (scenesFolder.TrimEnd('/', '\\') + "/" + elementName + ".unity").Replace("\\", "/");
                showCreatePanel = false;
                RefreshSceneList();
            })
            {
                text = "➕ Create UI Element"
            };
            create.style.minWidth = 160;
            create.style.height = 26;
            create.style.unityFontStyleAndWeight = FontStyle.Bold;
            buttons.Add(create);

            section.Add(buttons);
            return section;
        }

        // ---------- Actions ----------

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

            if (UiElementChangeDialog.Show(dialogModel))
            {
                OpenScene(sceneAssetPath);
            }
        }

        // Builds the single-scene Fix Structure dialog. The report's scene must still be open (for the preview).
        private static UiElementChangeDialogModel BuildFixDialog(string sceneName, UiElementStructureReport report, bool willBeSaved, bool hideMissingUiElement)
        {
            UiElementChangeEntry entry = new UiElementChangeEntry { Title = sceneName };
            entry.Changes.AddRange(UiElementStructureFixer.DescribePlannedChanges(report));

            UiElementFixPreview preview = UiElementFixPreview.Build(report);
            if (preview != null)
            {
                entry.Before = preview.Before;
                entry.After = preview.After;
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

            UiElementChangeDialogModel dialogModel = new UiElementChangeDialogModel
            {
                WindowTitle = "Fix Structure",
                Heading = "Fix the structure of " + sceneName + "?",
                Description = "For the show/hide animation to move the whole screen, everything has to sit inside one full-screen Root under the Canvas.",
                Footer = willBeSaved ? "Nothing is deleted or resized. The scene will be saved." : "Nothing is deleted or resized. The scene is open, so you can undo with Ctrl+Z. Save the scene to keep the changes.",
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

            if (!UiElementChangeDialog.Show(dialogModel))
            {
                return;
            }

            UiElementStructureReport remaining = null;
            UiElementSceneUtility.WithScene(sceneAssetPath, true, scene =>
            {
                remaining = UiElementStructureFixer.FixStructure(scene, canvasSettings);
            });

            if (remaining != null)
            {
                UiElementSceneDialogs.LogRemaining(info.SceneName, remaining);
                SetActionStatus(remaining.HasIssues ? "Fixed " + info.SceneName + "; some issues need manual work (see the row)." : "Fixed " + info.SceneName + ".");
            }

            RefreshScene(sceneAssetPath);
        }


        private void ApplyDefaultCanvasSettings(string sceneAssetPath, UiElementSceneInfo info, UiGlobalCanvasSettings canvasSettings)
        {
            bool isOpen = UiElementSceneUtility.IsSceneOpen(sceneAssetPath);

            UiElementChangeEntry entry = new UiElementChangeEntry { Title = info.SceneName };
            for (int i = 0; i < info.CanvasDifferences.Count; i++)
            {
                entry.SettingChanges.Add(info.CanvasDifferences[i]);
            }

            UiElementChangeDialogModel dialogModel = new UiElementChangeDialogModel
            {
                WindowTitle = "Use Default Settings",
                Heading = "Use the default canvas settings for " + info.SceneName + "?",
                Description = "These values change to the Global Canvas Settings (General tab).",
                Footer = "Sorting order, cameras and everything else stay as they are." +
                    (isOpen ? " The scene is open, so you can undo with Ctrl+Z. Save the scene to keep the change." : " The scene will be saved."),
                ConfirmLabel = "Use Defaults"
            };
            dialogModel.Entries.Add(entry);

            if (!UiElementChangeDialog.Show(dialogModel))
            {
                return;
            }

            bool applied = UiElementSceneUtility.EditCanvas(sceneAssetPath, "Use Default Canvas Settings", (canvas, scaler) =>
            {
                UiElementSceneUtility.ApplyCanvasSettings(canvas, scaler, canvasSettings);
            });

            if (applied)
            {
                Debug.Log("[UiFramework] Applied the default canvas settings to '" + info.SceneName + "'.");
                SetActionStatus("Applied the default canvas settings to " + info.SceneName + ".");
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

            if (!UiElementChangeDialog.Show(dialogModel))
            {
                return;
            }

            UiElementStructureFixer.FixStructure(scene, canvasSettings);
        }

        private List<string> GetElementScenePaths()
        {
            return UiElementSceneUtility.FindElementScenePaths(editorConfig);
        }
    }
}
