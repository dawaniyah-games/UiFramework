using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UiFramework.Editor.Addressables;
using UiFramework.Editor.Scenes;
using UiFramework.Editor.Window.Popups;
using UiFramework.Tweening;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiFramework.Editor.Window.Elements
{
    // What the settings panel asks its owner to do; the owner (Elements tab) handles dialogs, scene opening and
    // refreshing its cached scene info.
    public interface IUiElementDetailActions
    {
        void OpenScene(string scenePath);
        void SelectSceneAsset(string scenePath);
        void FixStructure(UiElementSceneInfo info);
        void ShowManualWork(UiElementSceneInfo info);
        void UseDefaultCanvas(UiElementSceneInfo info);

        // Re-read one scene (after its Canvas was edited) and redraw.
        void RefreshScene(string scenePath);

        // Redraw the panel from cached info (after an Addressables change).
        void RefreshDetail();
    }

    // Settings of one UI element: header (Open Scene), Structure, Animations, Canvas and Addressables sections.
    // Builds UI from cached UiElementSceneInfo; edits are written to the scene and reported back via the actions.
    public sealed class UiElementDetailPanel
    {
        private const string DefaultUiElementsGroupName = "UiElements";
        private const string DefaultUiElementsLabel = "UiElements";

        private readonly IUiElementDetailActions actions;

        public UiElementDetailPanel(IUiElementDetailActions actions)
        {
            this.actions = actions;
        }

        public void Build(VisualElement container, UiElementSceneInfo info)
        {
            container.Add(CreateDetailHeader(info));

            if (!info.IsAvailable)
            {
                container.Add(UiEditorStyles.CreateLine("This scene couldn't be read (for example while in Play Mode). Press Refresh to try again.", UiEditorStyles.WarningColor));
                return;
            }

            container.Add(CreateStructureSection(info));
            container.Add(CreateAnimationSection(info));
            container.Add(CreateCanvasSection(info));
            container.Add(CreateAddressablesSection(info));
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

            bool isOpen = UiElementSceneAccess.IsSceneOpen(info.ScenePath);
            if (isOpen)
            {
                Label openBadge = new Label("open in editor");
                openBadge.style.color = UiEditorStyles.OkColor;
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
                actions.OpenScene(info.ScenePath);
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
                actions.SelectSceneAsset(info.ScenePath);
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
            path.style.color = UiEditorStyles.MutedColor;
            path.style.fontSize = 11;
            path.style.marginBottom = 4;
            wrapper.Add(path);

            return wrapper;
        }

        private VisualElement CreateStructureSection(UiElementSceneInfo info)
        {
            VisualElement section = UiEditorStyles.CreateSection("Structure", "<Name>Root → Canvas → one full-screen Root that holds all content, so show/hide animations move the whole screen.");
            UiElementStructureReport structure = info.Structure;

            if (!structure.HasIssues)
            {
                section.Add(UiEditorStyles.CreateLine("✓ Follows the UI element layout.", UiEditorStyles.OkColor));
                return section;
            }

            for (int i = 0; i < structure.Issues.Count; i++)
            {
                UiElementStructureIssue issue = structure.Issues[i];
                Color color = issue.IsFixable || issue.IsWarning ? UiEditorStyles.WarningColor : UiEditorStyles.ErrorColor;
                section.Add(UiEditorStyles.CreateLine("• " + issue.Summary + (issue.IsFixable ? string.Empty : "  (fix by hand)"), color));
            }

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.marginTop = 8;

            if (structure.HasFixableIssues)
            {
                Button fixButton = new Button(() =>
                {
                    actions.FixStructure(info);
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
                    actions.ShowManualWork(info);
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
            VisualElement section = UiEditorStyles.CreateSection("Animations", "Played on the whole screen (the full-screen Root) when this element is shown or hidden.");

            if (!info.HasUiElement)
            {
                section.Add(UiEditorStyles.CreateLine("Add the UiElement script to the root object to choose animations.", UiEditorStyles.WarningColor));
                return section;
            }

            List<string> presetKeys = new List<string>(TweenPresets.GetPresetKeys());
            if (presetKeys.Count == 0)
            {
                presetKeys.Add(TweenPresets.NoneKey);
            }

            string currentShow = string.IsNullOrWhiteSpace(info.ShowPreset) || !presetKeys.Contains(info.ShowPreset) ? TweenPresets.NoneKey : info.ShowPreset;
            string currentHide = string.IsNullOrWhiteSpace(info.HidePreset) || !presetKeys.Contains(info.HidePreset) ? TweenPresets.NoneKey : info.HidePreset;

            DropdownField showDropdown = CreatePresetDropdown("Show", presetKeys, currentShow);
            DropdownField hideDropdown = CreatePresetDropdown("Hide", presetKeys, currentHide);
            Label showDescription = CreatePresetDescription(TweenPresets.Describe(currentShow, true));
            Label hideDescription = CreatePresetDescription(TweenPresets.Describe(currentHide, false));

            showDropdown.RegisterValueChangedCallback(evt =>
            {
                WriteElementAnimationPresets(info.ScenePath, evt.newValue, hideDropdown.value);
                info.ShowPreset = evt.newValue;
                showDescription.text = TweenPresets.Describe(evt.newValue, true);
            });

            hideDropdown.RegisterValueChangedCallback(evt =>
            {
                WriteElementAnimationPresets(info.ScenePath, showDropdown.value, evt.newValue);
                info.HidePreset = evt.newValue;
                hideDescription.text = TweenPresets.Describe(evt.newValue, false);
            });

            section.Add(showDropdown);
            section.Add(showDescription);
            section.Add(hideDropdown);
            section.Add(hideDescription);

            Label directionNote = UiEditorStyles.CreateLine("Slides move in the direction in their name, both when showing and hiding: Slide Right enters from the left and leaves on the right.", UiEditorStyles.MutedColor);
            directionNote.style.fontSize = 11;
            directionNote.style.marginTop = 6;
            section.Add(directionNote);
            return section;
        }

        private static DropdownField CreatePresetDropdown(string label, List<string> presetKeys, string current)
        {
            DropdownField dropdown = new DropdownField(label, presetKeys, current);
            dropdown.formatListItemCallback = key => ObjectNames.NicifyVariableName(key);
            dropdown.formatSelectedValueCallback = key => ObjectNames.NicifyVariableName(key);
            UiEditorStyles.SetLabelWidth(dropdown);
            return dropdown;
        }

        private static Label CreatePresetDescription(string text)
        {
            Label description = new Label(text);
            description.style.whiteSpace = WhiteSpace.Normal;
            description.style.color = UiEditorStyles.MutedColor;
            description.style.fontSize = 11;
            description.style.marginLeft = UiEditorStyles.FieldLabelWidth + 6;
            description.style.marginBottom = 4;
            return description;
        }

        private VisualElement CreateCanvasSection(UiElementSceneInfo info)
        {
            VisualElement section = UiEditorStyles.CreateSection("Canvas", "This element's own Canvas. New elements start with the Global Canvas Settings (General tab).");
            UiCanvasSnapshot canvas = info.Canvas;
            string path = info.ScenePath;

            if (canvas == null)
            {
                section.Add(UiEditorStyles.CreateLine("No Canvas yet. Use Fix Structure above to create one.", UiEditorStyles.WarningColor));
                return section;
            }

            EnumField renderMode = new EnumField("Render Mode", canvas.RenderMode);
            UiEditorStyles.SetLabelWidth(renderMode);
            renderMode.RegisterValueChangedCallback(evt =>
            {
                RenderMode value = (RenderMode)evt.newValue;
                EditCanvas(path, (c, s) =>
                {
                    c.renderMode = value;
                });
            });
            section.Add(renderMode);

            IntegerField sortingOrder = new IntegerField("Sorting Order") { value = canvas.SortingOrder, isDelayed = true };
            sortingOrder.tooltip = "Higher values draw on top. Use it to stack popups above other screens.";
            UiEditorStyles.SetLabelWidth(sortingOrder);
            sortingOrder.RegisterValueChangedCallback(evt =>
            {
                int value = evt.newValue;
                EditCanvas(path, (c, s) =>
                {
                    c.sortingOrder = value;
                });
            });
            section.Add(sortingOrder);

            if (!canvas.HasScaler)
            {
                section.Add(UiEditorStyles.CreateLine("No Canvas Scaler. Use Fix Structure above to add one.", UiEditorStyles.WarningColor));
            }
            else
            {
                EnumField scaleMode = new EnumField("UI Scale Mode", canvas.UiScaleMode);
                UiEditorStyles.SetLabelWidth(scaleMode);
                scaleMode.RegisterValueChangedCallback(evt =>
                {
                    UnityEngine.UI.CanvasScaler.ScaleMode value = (UnityEngine.UI.CanvasScaler.ScaleMode)evt.newValue;
                    EditCanvas(path, (c, s) =>
                    {
                        s.uiScaleMode = value;
                    });
                });
                section.Add(scaleMode);

                if (canvas.UiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    section.Add(CreateResolutionRow(path, canvas.ReferenceResolution));

                    EnumField matchMode = new EnumField("Screen Match Mode", canvas.ScreenMatchMode);
                    UiEditorStyles.SetLabelWidth(matchMode);
                    matchMode.RegisterValueChangedCallback(evt =>
                    {
                        UnityEngine.UI.CanvasScaler.ScreenMatchMode value = (UnityEngine.UI.CanvasScaler.ScreenMatchMode)evt.newValue;
                        EditCanvas(path, (c, s) =>
                        {
                            s.screenMatchMode = value;
                        });
                    });
                    section.Add(matchMode);

                    if (canvas.ScreenMatchMode == UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                    {
                        FloatField match = new FloatField("Match (0 = width, 1 = height)") { value = canvas.MatchWidthOrHeight, isDelayed = true };
                        UiEditorStyles.SetLabelWidth(match);
                        match.RegisterValueChangedCallback(evt =>
                        {
                            float value = Mathf.Clamp01(evt.newValue);
                            EditCanvas(path, (c, s) =>
                            {
                                s.matchWidthOrHeight = value;
                            });
                        });
                        section.Add(match);
                    }
                }

                FloatField pixelsPerUnit = new FloatField("Reference Pixels Per Unit") { value = canvas.ReferencePixelsPerUnit, isDelayed = true };
                UiEditorStyles.SetLabelWidth(pixelsPerUnit);
                pixelsPerUnit.RegisterValueChangedCallback(evt =>
                {
                    float value = Mathf.Max(0.01f, evt.newValue);
                    EditCanvas(path, (c, s) =>
                    {
                        s.referencePixelsPerUnit = value;
                    });
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
                actions.UseDefaultCanvas(info);
            })
            {
                text = "Use Default Settings…",
                tooltip = "Set this Canvas to the Global Canvas Settings (General tab). Shows the changes first."
            };
            useDefaults.style.height = 24;
            useDefaults.SetEnabled(!matchesDefaults);
            defaultsRow.Add(useDefaults);

            Label defaultsNote = new Label(matchesDefaults ? "✓ Same as the default settings." : info.CanvasDifferences.Count + " value(s) differ from the defaults.");
            defaultsNote.style.color = matchesDefaults ? UiEditorStyles.OkColor : UiEditorStyles.MutedColor;
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
            UiEditorStyles.SetLabelWidth(width);
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
                EditCanvas(path, (c, s) =>
                {
                    s.referenceResolution = value;
                });
            });

            height.RegisterValueChangedCallback(evt =>
            {
                Vector2 value = new Vector2(width.value, Mathf.Max(1, evt.newValue));
                EditCanvas(path, (c, s) =>
                {
                    s.referenceResolution = value;
                });
            });

            return row;
        }

        private void EditCanvas(string scenePath, Action<Canvas, UnityEngine.UI.CanvasScaler> edit)
        {
            // Scaler fields are only shown when the Canvas has a Canvas Scaler, so edits never see a missing one.
            bool edited = UiElementCanvasUtility.EditCanvas(scenePath, "Change Canvas", edit);

            if (!edited)
            {
                Debug.LogWarning("[UiFramework] Couldn't change the Canvas of '" + Path.GetFileNameWithoutExtension(scenePath) + "': it has no root Canvas.");
            }

            actions.RefreshScene(scenePath);
        }

        private VisualElement CreateAddressablesSection(UiElementSceneInfo info)
        {
            VisualElement section = UiEditorStyles.CreateSection("Addressables", "Element scenes must be Addressable so the UiManager can load them at runtime.");
            string scenePath = info.ScenePath;
            string sceneName = info.SceneName;

            AddressableAssetSettings settings;
            bool hasAddressables = AddressablesAssetUtility.TryGetSettings(out settings);
            if (!hasAddressables)
            {
                section.Add(UiEditorStyles.CreateLine("Addressables isn't set up in this project (Window → Asset Management → Addressables → Groups).", UiEditorStyles.WarningColor));
                return section;
            }

            List<string> groupNames = AddressablesAssetUtility.GetGroupNames(settings);
            if (groupNames.Count == 0)
            {
                groupNames.Add(DefaultUiElementsGroupName);
            }
            else if (!groupNames.Contains(DefaultUiElementsGroupName))
            {
                groupNames.Insert(0, DefaultUiElementsGroupName);
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

            if (selectedLabels.Count == 0 && !string.IsNullOrEmpty(DefaultUiElementsLabel))
            {
                selectedLabels.Add(DefaultUiElementsLabel);
            }

            string selectedGroup = DefaultUiElementsGroupName;
            if (isAddressable && entry != null && entry.parentGroup != null)
            {
                selectedGroup = entry.parentGroup.Name;
            }

            if (!groupNames.Contains(selectedGroup))
            {
                selectedGroup = groupNames[0];
            }

            Toggle addressableToggle = new Toggle("Addressable") { value = isAddressable };
            UiEditorStyles.SetLabelWidth(addressableToggle);
            section.Add(addressableToggle);

            DropdownField groupDropdown = new DropdownField("Group", groupNames, selectedGroup);
            UiEditorStyles.SetLabelWidth(groupDropdown);
            groupDropdown.SetEnabled(isAddressable);
            section.Add(groupDropdown);

            VisualElement labelsRow = new VisualElement();
            labelsRow.style.flexDirection = FlexDirection.Row;
            labelsRow.style.alignItems = Align.Center;
            labelsRow.style.marginLeft = 3;

            Label labelsCaption = new Label("Labels");
            labelsCaption.style.width = UiEditorStyles.FieldLabelWidth;
            labelsCaption.style.minWidth = UiEditorStyles.FieldLabelWidth;
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

                actions.RefreshDetail();
            });

            groupDropdown.RegisterValueChangedCallback(evt =>
            {
                AddressablesAssetUtility.EnsureAssetIsAddressable(scenePath, evt.newValue, sceneName, selectedLabels.ToArray());
            });

            if (!isAddressable)
            {
                section.Add(UiEditorStyles.CreateLine("Not Addressable yet: the UiManager can't load it until you turn this on (or run Build Runtime UiConfig).", UiEditorStyles.WarningColor));
            }

            return section;
        }

        private static void WriteElementAnimationPresets(string sceneAssetPath, string showPreset, string hidePreset)
        {
            if (string.IsNullOrWhiteSpace(sceneAssetPath))
            {
                return;
            }

            bool wroteAny = false;

            // Scenes already open in the editor are edited in place (dirty, not saved or closed).
            UiElementSceneAccess.WithScene(sceneAssetPath, true, scene =>
            {
                wroteAny = UiElementSceneAccess.WriteAnimationPresets(scene, showPreset, hidePreset);
            });

            if (!wroteAny)
            {
                Debug.LogWarning("[UiFramework] No UiElement found in scene: " + sceneAssetPath);
            }
        }


        private static string BuildLabelsSummary(HashSet<string> selectedLabels)
        {
            if (selectedLabels == null || selectedLabels.Count == 0)
            {
                return "(no labels)";
            }

            List<string> labels = new List<string>(selectedLabels);
            labels.Sort(StringComparer.Ordinal);
            return string.Join(", ", labels.ToArray());
        }

    }
}
