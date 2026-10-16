using System.Collections.Generic;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Works on the active scene in place: Undo-recorded, never saved automatically.
    internal static class UiElementSceneMenu
    {
        private const string menuRoot = "UiFramework/Element Scene/";
        private const string validateItem = menuRoot + "Validate Active Scene";
        private const string fixItem = menuRoot + "Fix Active Scene Structure";
        private const string resetItem = menuRoot + "Reset Active Scene Canvas";

        [MenuItem(validateItem, false, 100)]
        private static void ValidateActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            UiCanvasSettings settings = GetSettings();

            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);

            string diff;
            UiCanvasStatus status = UiElementSceneUtility.GetCanvasStatus(report, settings, scene.path, out diff);

            string structure = report.HasIssues ? report.GetCodesSummary() + "\n" + report.GetIssuesText() : "OK";
            string canvas = status == UiCanvasStatus.NoCanvas ? "no canvas" : (string.IsNullOrEmpty(diff) ? "matches" : diff) + (status == UiCanvasStatus.Excluded ? " (excluded)" : string.Empty);
            string message = "[UiFramework] " + scene.name + ": structure " + structure + "\nCanvas: " + canvas;

            if (report.HasIssues || status == UiCanvasStatus.Differs)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        [MenuItem(fixItem, false, 101)]
        private static void FixActiveSceneStructure()
        {
            Scene scene = SceneManager.GetActiveScene();
            UiCanvasSettings settings = GetSettings();

            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);

            List<string> unfixable = UiElementStructureFixer.DescribeUnfixableIssues(report);

            if (!report.HasFixableIssues)
            {
                UiElementSceneDialogs.Inform("Fix Structure", report.HasIssues ? "Nothing can be fixed automatically in '" + scene.name + "':" : "'" + scene.name + "' already follows the contract.", unfixable);
                return;
            }

            List<string> lines = UiElementStructureFixer.DescribePlannedChanges(report, UiElementStructureFixMode.WrapContent);
            UiElementSceneDialogs.AppendUnfixable(lines, unfixable);

            UiElementStructureFixMode mode;
            if (!UiElementSceneDialogs.ConfirmStructureFix("Fix Structure", "Apply these changes to '" + scene.name + "'? They can be undone, and the scene is not saved.", lines, UiElementStructureFixer.OffersContentRootReset(report), out mode))
            {
                return;
            }

            UiElementStructureReport remaining = UiElementStructureFixer.FixStructure(scene, settings, mode);
            UiElementSceneDialogs.LogRemaining(scene.name, remaining);
        }

        [MenuItem(resetItem, false, 102)]
        private static void ResetActiveSceneCanvas()
        {
            Scene scene = SceneManager.GetActiveScene();
            UiCanvasSettings settings = GetSettings();

            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);

            if (report.Canvas == null)
            {
                UiElementSceneDialogs.Inform("Reset Canvas", "'" + scene.name + "' has no root Canvas under the element root. Run 'Fix Active Scene Structure' first.", null);
                return;
            }

            CanvasScaler scaler = report.Canvas.GetComponent<CanvasScaler>();
            string diff;
            if (UiElementSceneUtility.CanvasSettingsMatch(report.Canvas, scaler, settings, out diff))
            {
                UiElementSceneDialogs.Inform("Reset Canvas", "The canvas in '" + scene.name + "' already matches the canvas settings.", null);
                return;
            }

            List<string> lines = new List<string>(diff.Split(new string[] { "; " }, System.StringSplitOptions.RemoveEmptyEntries));
            if (!UiElementSceneDialogs.Confirm("Reset Canvas", "Reset the canvas in '" + scene.name + "' to the canvas settings? Sorting order and cameras are not changed. The scene is not saved.", lines, "Reset"))
            {
                return;
            }

            UiElementSceneUtility.ApplyCanvasSettings(report.Canvas, scaler, settings);
            Debug.Log("[UiFramework] Reset canvas settings in '" + scene.name + "' (" + diff + ")." + (scaler == null ? " CanvasScaler is missing; run 'Fix Active Scene Structure' to add it." : string.Empty));
        }

        [MenuItem(validateItem, true)]
        [MenuItem(fixItem, true)]
        [MenuItem(resetItem, true)]
        private static bool ActiveSceneHasUiElement()
        {
            return UiElementSceneUtility.HasUiElementOnRoot(SceneManager.GetActiveScene());
        }

        private static UiCanvasSettings GetSettings()
        {
            return UiEditorConfigUtility.GetCanvasSettingsOrDefaults(UiEditorConfigUtility.FindActiveConfig());
        }
    }
}
