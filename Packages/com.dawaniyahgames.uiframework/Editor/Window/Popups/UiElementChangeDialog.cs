using System.Collections.Generic;
using UiFramework.Editor.Scenes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UiFramework.Editor.Window.Popups
{
    // One scene's worth of content in a UiElementChangeDialog.
    public sealed class UiElementChangeEntry
    {
        private readonly List<string> changes = new List<string>();
        private readonly List<UiCanvasSettingDifference> settingChanges = new List<UiCanvasSettingDifference>();
        private readonly List<string> manualWork = new List<string>();

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public UiElementPreviewNode Before { get; set; }
        public UiElementPreviewNode After { get; set; }

        public List<string> Changes
        {
            get
            {
                return changes;
            }
        }

        public List<UiCanvasSettingDifference> SettingChanges
        {
            get
            {
                return settingChanges;
            }
        }

        public List<string> ManualWork
        {
            get
            {
                return manualWork;
            }
        }
    }

    public sealed class UiElementChangeDialogModel
    {
        private readonly List<UiElementChangeEntry> entries = new List<UiElementChangeEntry>();

        public string WindowTitle { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public string Footer { get; set; }
        public string ConfirmLabel { get; set; }
        public string CancelLabel { get; set; }

        public List<UiElementChangeEntry> Entries
        {
            get
            {
                return entries;
            }
        }
    }

    // Modal confirmation that shows what an action will change in plain language: a short list of changes,
    // a Now / After hierarchy preview or a settings table, and anything left for manual work.
    public sealed class UiElementChangeDialog : EditorWindow
    {
        private const int MaxPreviewDepth = 3;

        private static readonly Color newColor = new Color(0.45f, 0.85f, 0.45f);
        private static readonly Color mutedColor = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color manualColor = new Color(0.95f, 0.65f, 0.3f);

        private static bool lastConfirmed;

        private UiElementChangeDialogModel model;
        private ScrollView body;

        // Blocks until the dialog is closed. Returns true when the confirm button was pressed.
        public static bool Show(UiElementChangeDialogModel model)
        {
            lastConfirmed = false;

            UiElementChangeDialog dialog = CreateInstance<UiElementChangeDialog>();
            dialog.titleContent = new GUIContent(model.WindowTitle);
            dialog.model = model;
            dialog.BuildUi();

            Vector2 size = new Vector2(640.0f, EstimateHeight(model));
            Rect main = EditorGUIUtility.GetMainWindowPosition();
            dialog.minSize = new Vector2(480.0f, 240.0f);
            dialog.position = new Rect(main.x + (main.width - size.x) * 0.5f, main.y + (main.height - size.y) * 0.5f, size.x, size.y);
            dialog.ShowModal();

            return lastConfirmed;
        }

        private void BuildUi()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 16;
            root.style.paddingRight = 16;
            root.style.paddingTop = 14;
            root.style.paddingBottom = 12;

            Label heading = new Label(model.Heading);
            heading.style.unityFontStyleAndWeight = FontStyle.Bold;
            heading.style.fontSize = 15;
            heading.style.whiteSpace = WhiteSpace.Normal;
            heading.style.marginBottom = 4;
            root.Add(heading);

            if (!string.IsNullOrEmpty(model.Description))
            {
                Label description = new Label(model.Description);
                description.style.whiteSpace = WhiteSpace.Normal;
                description.style.color = mutedColor;
                description.style.marginBottom = 10;
                root.Add(description);
            }

            body = new ScrollView(ScrollViewMode.Vertical);
            body.style.flexGrow = 1;
            root.Add(body);
            RenderBody();

            if (!string.IsNullOrEmpty(model.Footer))
            {
                Label footer = new Label(model.Footer);
                footer.style.whiteSpace = WhiteSpace.Normal;
                footer.style.fontSize = 11;
                footer.style.color = mutedColor;
                footer.style.marginTop = 8;
                root.Add(footer);
            }

            VisualElement buttons = new VisualElement();
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.justifyContent = Justify.FlexEnd;
            buttons.style.marginTop = 10;

            Button cancel = new Button(Cancel) { text = string.IsNullOrEmpty(model.CancelLabel) ? "Cancel" : model.CancelLabel };
            cancel.style.minWidth = 100;
            cancel.style.height = 26;
            buttons.Add(cancel);

            Button confirm = new Button(Confirm) { text = model.ConfirmLabel };
            confirm.style.minWidth = 140;
            confirm.style.height = 26;
            confirm.style.unityFontStyleAndWeight = FontStyle.Bold;
            buttons.Add(confirm);
            root.Add(buttons);

            root.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Escape)
                {
                    Cancel();
                }
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    Confirm();
                }
            });
        }

        private void RenderBody()
        {
            body.Clear();
            bool single = model.Entries.Count == 1;

            for (int i = 0; i < model.Entries.Count; i++)
            {
                UiElementChangeEntry entry = model.Entries[i];
                VisualElement container = body;

                if (!single)
                {
                    Foldout foldout = new Foldout
                    {
                        text = string.IsNullOrEmpty(entry.Subtitle) ? entry.Title : entry.Title + "   ·   " + entry.Subtitle,
                        value = false
                    };
                    foldout.style.marginBottom = 2;
                    body.Add(foldout);
                    container = foldout;
                }

                RenderEntry(container, entry, single);
            }
        }

        private void RenderEntry(VisualElement container, UiElementChangeEntry entry, bool single)
        {
            List<string> changes = entry.Changes;
            UiElementPreviewNode after = entry.After;

            if (changes.Count > 0)
            {
                container.Add(CreateSectionLabel("What will change", null));
                for (int i = 0; i < changes.Count; i++)
                {
                    container.Add(CreateBullet(changes[i], null));
                }
            }

            if (entry.SettingChanges.Count > 0)
            {
                container.Add(CreateSectionLabel("What will change", null));
                for (int i = 0; i < entry.SettingChanges.Count; i++)
                {
                    container.Add(CreateSettingRow(entry.SettingChanges[i]));
                }
            }

            if (entry.Before != null && after != null)
            {
                VisualElement columns = new VisualElement();
                columns.style.flexDirection = FlexDirection.Row;
                columns.style.marginTop = 10;
                columns.Add(CreateTreeColumn("Now", entry.Before));
                columns.Add(CreateTreeColumn("After", after));
                container.Add(columns);
            }

            if (entry.ManualWork.Count > 0)
            {
                bool expanded = single && changes.Count == 0 && entry.SettingChanges.Count == 0;
                Foldout manual = new Foldout
                {
                    text = "Needs manual work (" + entry.ManualWork.Count + ")",
                    value = expanded
                };
                manual.style.marginTop = 10;
                manual.style.color = manualColor;

                for (int i = 0; i < entry.ManualWork.Count; i++)
                {
                    manual.Add(CreateBullet(entry.ManualWork[i], null));
                }

                container.Add(manual);
            }
        }

        private static Label CreateSectionLabel(string text, Color? color)
        {
            Label label = new Label(text);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 4;
            label.style.marginBottom = 2;

            if (color.HasValue)
            {
                label.style.color = color.Value;
            }

            return label;
        }

        private static Label CreateBullet(string text, Color? color)
        {
            Label label = new Label("•  " + text);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginLeft = 6;
            label.style.marginBottom = 2;

            if (color.HasValue)
            {
                label.style.color = color.Value;
            }

            return label;
        }

        private static VisualElement CreateSettingRow(UiCanvasSettingDifference difference)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginLeft = 6;
            row.style.marginBottom = 2;

            Label setting = new Label(FriendlySettingName(difference.Setting));
            setting.style.width = 200;
            row.Add(setting);

            Label current = new Label(FriendlyValue(difference.Current));
            current.style.width = 160;
            current.style.color = mutedColor;
            row.Add(current);

            Label arrow = new Label("→");
            arrow.style.width = 22;
            row.Add(arrow);

            Label target = new Label(FriendlyValue(difference.Target));
            target.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(target);

            return row;
        }

        private static VisualElement CreateTreeColumn(string title, UiElementPreviewNode root)
        {
            VisualElement column = new VisualElement();
            column.style.flexGrow = 1;
            column.style.flexBasis = 0;
            column.style.marginRight = 6;
            column.style.paddingLeft = 8;
            column.style.paddingRight = 8;
            column.style.paddingTop = 6;
            column.style.paddingBottom = 6;
            column.style.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.15f);
            column.style.borderTopLeftRadius = 4;
            column.style.borderTopRightRadius = 4;
            column.style.borderBottomLeftRadius = 4;
            column.style.borderBottomRightRadius = 4;

            Label header = new Label(title);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 4;
            column.Add(header);

            AddTreeRows(column, root, 0);
            return column;
        }

        private static void AddTreeRows(VisualElement column, UiElementPreviewNode node, int depth)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginLeft = depth * 14;

            Label name = new Label((depth > 0 ? "└ " : string.Empty) + node.Name);
            if (node.IsNew)
            {
                name.style.color = newColor;
                name.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            row.Add(name);

            string note = node.IsNew ? (string.IsNullOrEmpty(node.Note) ? "new" : "new, " + node.Note) : node.Note;
            if (!string.IsNullOrEmpty(note))
            {
                Label noteLabel = new Label("  " + note);
                noteLabel.style.color = node.IsNew ? newColor : mutedColor;
                noteLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noteLabel.style.fontSize = 11;
                row.Add(noteLabel);
            }

            column.Add(row);

            if (depth >= MaxPreviewDepth)
            {
                return;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                AddTreeRows(column, node.Children[i], depth + 1);
            }
        }

        private static string FriendlySettingName(string setting)
        {
            switch (setting)
            {
                case "UiScaleMode":
                    return "UI Scale Mode";
                case "MatchWidthOrHeight":
                    return "Match (Width ↔ Height)";
                case "ReferencePixelsPerUnit":
                    return "Reference Pixels Per Unit";
                default:
                    return ObjectNames.NicifyVariableName(setting);
            }
        }

        // Enum-style values ("ScaleWithScreenSize") get spaces; numbers and sentences are left alone.
        private static string FriendlyValue(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf(' ') >= 0 || !char.IsLetter(value[0]))
            {
                return value;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsDigit(value[i]))
                {
                    return value;
                }
            }

            return ObjectNames.NicifyVariableName(value);
        }

        private static float EstimateHeight(UiElementChangeDialogModel model)
        {
            float height = 190.0f;

            if (model.Entries.Count != 1)
            {
                height += 24.0f * model.Entries.Count;
            }
            else
            {
                UiElementChangeEntry entry = model.Entries[0];
                height += 22.0f * (entry.Changes.Count + entry.SettingChanges.Count) + (entry.ManualWork.Count > 0 ? 40.0f : 0.0f);

                if (entry.Before != null)
                {
                    height += 40.0f + 20.0f * Mathf.Max(CountRows(entry.Before, 0), CountRows(entry.After, 0));
                }
            }

            return Mathf.Clamp(height, 260.0f, 640.0f);
        }

        private static int CountRows(UiElementPreviewNode node, int depth)
        {
            if (node == null)
            {
                return 0;
            }

            int rows = 1;
            if (depth < MaxPreviewDepth)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    rows += CountRows(node.Children[i], depth + 1);
                }
            }

            return rows;
        }

        private void Confirm()
        {
            lastConfirmed = true;
            Close();
        }

        private void Cancel()
        {
            lastConfirmed = false;
            Close();
        }
    }
}
