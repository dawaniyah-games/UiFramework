using UnityEngine;
using UnityEngine.UIElements;

namespace UiFramework.Editor.Window
{
    // Colours and small building blocks shared by the UI Setup Manager panels, so every section looks the same.
    public static class UiEditorStyles
    {
        public const float FieldLabelWidth = 170.0f;

        public static readonly Color OkColor = new Color(0.4f, 0.8f, 0.4f);
        public static readonly Color WarningColor = new Color(0.95f, 0.75f, 0.3f);
        public static readonly Color ErrorColor = new Color(0.9f, 0.35f, 0.35f);
        public static readonly Color MutedColor = new Color(0.6f, 0.6f, 0.6f);

        // A rounded box with a bold title and an optional one-line explanation.
        public static VisualElement CreateSection(string title, string subtitle)
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
                hint.style.color = MutedColor;
                hint.style.fontSize = 11;
                hint.style.marginBottom = 6;
                section.Add(hint);
            }

            return section;
        }

        public static Label CreateLine(string text, Color? color)
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

        public static Label CreateHint(string text)
        {
            Label label = CreateLine(text, MutedColor);
            label.style.fontSize = 11;
            return label;
        }

        // Aligns a field's label with the other fields in the panel.
        public static void SetLabelWidth(VisualElement field)
        {
            Label label = field.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = FieldLabelWidth;
                label.style.width = FieldLabelWidth;
            }
        }

        public static Button CreateButton(string text, string tooltip, System.Action onClick, bool bold)
        {
            Button button = new Button(onClick)
            {
                text = text,
                tooltip = tooltip
            };
            button.style.height = 24;

            if (bold)
            {
                button.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            return button;
        }
    }
}
