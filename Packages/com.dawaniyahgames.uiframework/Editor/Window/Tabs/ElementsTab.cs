using System.Collections.Generic;
using System.IO;
using UiFramework.Editor.Config;
using UiFramework.Editor.Scenes;
using UiFramework.Editor.Window.Elements;
using UiFramework.Editor.Window.Popups;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiFramework.Editor.Window.Tabs
{
    // Element list on the left (search, status, double-click to open), settings of the selected element on the right.
    public class ElementsTab : BaseVisualElementTab, IUiElementDetailActions
    {
        private readonly UiElementDetailPanel detailPanel;
        private readonly UiElementCreateForm createForm;

        public ElementsTab()
        {
            detailPanel = new UiElementDetailPanel(this);
            createForm = new UiElementCreateForm(CreateElement, CloseCreateForm);
        }

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

            fixAllButton = new ToolbarButton(FixAllStructure)
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
                    sceneInfos.Add(UiElementSceneAccess.InspectScene(scenePaths[i], canvasSettings));
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
        public void RefreshScene(string scenePath)
        {
            for (int i = 0; i < sceneInfos.Count; i++)
            {
                if (sceneInfos[i].ScenePath == scenePath)
                {
                    sceneInfos[i] = UiElementSceneAccess.InspectScene(scenePath, GetCanvasSettings());
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
                empty.style.color = UiEditorStyles.MutedColor;
                listView.Add(empty);
                return;
            }

            List<UiElementSceneInfo> visible = GetVisibleInfos();

            if (!string.IsNullOrWhiteSpace(searchQuery) || showOnlyIssues)
            {
                Label count = new Label(visible.Count + " of " + sceneInfos.Count);
                count.style.color = UiEditorStyles.MutedColor;
                count.style.fontSize = 11;
                count.style.marginBottom = 4;
                listView.Add(count);
            }

            if (visible.Count == 0)
            {
                Label none = new Label(string.IsNullOrWhiteSpace(searchQuery) ? "Every element follows the layout." : "No elements match '" + searchQuery.Trim() + "'.");
                none.style.whiteSpace = WhiteSpace.Normal;
                none.style.color = UiEditorStyles.MutedColor;
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

            Label name = new Label(info.SceneName + (UiElementSceneAccess.IsSceneOpen(scenePath) ? "  (open)" : string.Empty));
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
                color = UiEditorStyles.MutedColor;
                return "?";
            }

            if (!info.Structure.HasIssues)
            {
                color = UiEditorStyles.OkColor;
                return "✓";
            }

            if (info.Structure.HasUnfixableIssues)
            {
                color = UiEditorStyles.ErrorColor;
                return "✖";
            }

            color = UiEditorStyles.WarningColor;
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
                    summaryLabel.style.color = UiEditorStyles.OkColor;
                }
                else if (manual > 0)
                {
                    summaryLabel.style.color = UiEditorStyles.ErrorColor;
                }
                else if (fixable > 0)
                {
                    summaryLabel.style.color = UiEditorStyles.WarningColor;
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
                detailView.Add(createForm.Build());
                return;
            }

            UiElementSceneInfo info = FindInfo(selectedScenePath);
            if (info == null)
            {
                detailView.Add(UiEditorStyles.CreateLine("Select an element on the left, or create one with ➕ New Element.", UiEditorStyles.MutedColor));
                return;
            }

            detailPanel.Build(detailView, info);
        }

        // ---------- Actions (IUiElementDetailActions and toolbar) ----------

        public void OpenScene(string scenePath)
        {
            if (UiElementSceneAccess.IsSceneOpen(scenePath))
            {
                SelectSceneAsset(scenePath);
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            RenderSceneList();
        }

        public void SelectSceneAsset(string scenePath)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                return;
            }

            Selection.activeObject = sceneAsset;
            EditorGUIUtility.PingObject(sceneAsset);
        }

        public void FixStructure(UiElementSceneInfo info)
        {
            // Re-read the scene so the popup's Now / After preview reflects its current state.
            string scenePath = info.ScenePath;
            bool isOpen = UiElementSceneAccess.IsSceneOpen(scenePath);
            UiElementStructureReport current = null;
            UiElementChangeDialogModel dialogModel = null;

            UiElementSceneAccess.WithScene(scenePath, false, scene =>
            {
                UiElementStructureValidator.ValidateStructure(scene, out current);
                dialogModel = UiElementChangeDialogs.ForStructureFix(info.SceneName, current, !isOpen, false);
            });

            if (current == null || !current.HasFixableIssues)
            {
                RefreshScene(scenePath);
                return;
            }

            if (!UiElementChangeDialog.Show(dialogModel))
            {
                return;
            }

            UiGlobalCanvasSettings canvasSettings = GetCanvasSettings();
            UiElementStructureReport remaining = null;
            UiElementSceneAccess.WithScene(scenePath, true, scene =>
            {
                remaining = UiElementStructureFixer.FixStructure(scene, canvasSettings);
            });

            if (remaining != null)
            {
                UiElementSceneBatchOperations.LogFixResult(info.SceneName, remaining);
                SetActionStatus(remaining.HasIssues ? "Fixed " + info.SceneName + "; some issues need manual work." : "Fixed " + info.SceneName + ".");
            }

            RefreshScene(scenePath);
        }

        public void ShowManualWork(UiElementSceneInfo info)
        {
            if (UiElementChangeDialog.Show(UiElementChangeDialogs.ForManualWork(info)))
            {
                OpenScene(info.ScenePath);
            }
        }

        public void UseDefaultCanvas(UiElementSceneInfo info)
        {
            string scenePath = info.ScenePath;
            if (!UiElementChangeDialog.Show(UiElementChangeDialogs.ForUseDefaultCanvas(info, UiElementSceneAccess.IsSceneOpen(scenePath))))
            {
                return;
            }

            UiGlobalCanvasSettings canvasSettings = GetCanvasSettings();
            bool applied = UiElementCanvasUtility.EditCanvas(scenePath, "Use Default Canvas Settings", (canvas, scaler) =>
            {
                UiElementCanvasUtility.ApplyCanvasSettings(canvas, scaler, canvasSettings);
            });

            if (applied)
            {
                Debug.Log("[UiFramework] Applied the default canvas settings to '" + info.SceneName + "'.");
                SetActionStatus("Applied the default canvas settings to " + info.SceneName + ".");
            }

            RefreshScene(scenePath);
        }

        public void RefreshDetail()
        {
            RenderDetail();
        }

        private void FixAllStructure()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            List<UiElementSceneInfo> toFix = UiElementSceneBatchOperations.GetScenesToFix(sceneInfos);
            if (toFix.Count == 0)
            {
                SetActionStatus("No element has issues that can be fixed automatically.");
                return;
            }

            if (!UiElementChangeDialog.Show(UiElementChangeDialogs.ForFixAll(toFix)))
            {
                return;
            }

            string result = UiElementSceneBatchOperations.FixStructure(toFix, GetCanvasSettings());
            RefreshSceneList();
            SetActionStatus(result);
        }

        private void CreateElement(UiElementSceneCreator.Options options)
        {
            // The new script is attached after compiling, so a missing UiElement is expected and not listed here.
            options.ConfirmStructureFix = report =>
            {
                UiElementChangeDialogModel dialogModel = UiElementChangeDialogs.ForStructureFix(options.ElementName, report, true, true);
                dialogModel.Description = "This scene doesn't follow the UI element layout yet. " + dialogModel.Description + " You can also fix it later from the element's Structure section.";
                return UiElementChangeDialog.Show(dialogModel);
            };

            string scenePath = UiElementSceneCreator.Create(editorConfig, options);
            if (scenePath == null)
            {
                return;
            }

            selectedScenePath = scenePath;
            showCreatePanel = false;
            RefreshSceneList();
        }

        private void CloseCreateForm()
        {
            showCreatePanel = false;
            RenderList();
            RenderDetail();
        }

        private List<string> GetElementScenePaths()
        {
            return UiElementSceneAccess.FindElementScenePaths(editorConfig);
        }
    }
}
