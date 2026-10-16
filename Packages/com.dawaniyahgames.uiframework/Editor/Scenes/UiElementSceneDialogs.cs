using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    internal static class UiElementSceneDialogs
    {
        private const int maxDialogLines = 30;

        // Asks before a structure fix. When the plan includes a non-stretched content root, offers both modes:
        // wrapping (keeps the layout, default) and resetting the existing RectTransform.
        public static bool ConfirmStructureFix(string title, string intro, IList<string> lines, bool offersContentRootReset, out UiElementStructureFixMode mode)
        {
            mode = UiElementStructureFixMode.WrapContent;
            string message = BuildMessage(intro, lines);

            if (!offersContentRootReset)
            {
                return EditorUtility.DisplayDialog(title, message, "Fix", "Cancel");
            }

            message += "\n\nNon-stretched content roots: 'Fix (keep layout)' wraps them in a new stretched Root. 'Fix (stretch existing)' resets their RectTransform instead, which changes their size and position.";

            int choice = EditorUtility.DisplayDialogComplex(title, message, "Fix (keep layout)", "Cancel", "Fix (stretch existing)");
            if (choice == 1)
            {
                return false;
            }

            mode = choice == 2 ? UiElementStructureFixMode.ResetContentRoot : UiElementStructureFixMode.WrapContent;
            return true;
        }

        public static bool Confirm(string title, string intro, IList<string> lines, string ok)
        {
            return EditorUtility.DisplayDialog(title, BuildMessage(intro, lines), ok, "Cancel");
        }

        public static void Inform(string title, string intro, IList<string> lines)
        {
            EditorUtility.DisplayDialog(title, BuildMessage(intro, lines), "OK");
        }

        public static void AppendUnfixable(List<string> lines, List<string> unfixable)
        {
            if (unfixable.Count == 0)
            {
                return;
            }

            lines.Add(string.Empty);
            lines.Add("Not fixed automatically:");
            for (int i = 0; i < unfixable.Count; i++)
            {
                lines.Add(unfixable[i]);
            }
        }

        public static void LogRemaining(string sceneName, UiElementStructureReport remaining)
        {
            if (remaining.HasIssues)
            {
                Debug.LogWarning("[UiFramework] Fixed structure of '" + sceneName + "'. Remaining issues:\n" + remaining.GetIssuesText());
                return;
            }

            Debug.Log("[UiFramework] Fixed structure of '" + sceneName + "'; it now follows the UI element scene contract.");
        }

        // Dialogs get unreadable past a few dozen lines; the full list is always logged to the Console as well.
        public static string BuildMessage(string intro, IList<string> lines)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(intro);

            if (lines == null || lines.Count == 0)
            {
                return builder.ToString();
            }

            builder.Append("\n");

            int shown = lines.Count < maxDialogLines ? lines.Count : maxDialogLines;
            for (int i = 0; i < shown; i++)
            {
                builder.Append("\n");
                builder.Append(lines[i]);
            }

            if (lines.Count > shown)
            {
                builder.Append("\n... and " + (lines.Count - shown) + " more (see the Console).");
            }

            return builder.ToString();
        }
    }
}
