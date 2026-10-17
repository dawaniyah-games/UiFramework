using System.Collections.Generic;
using System.Text;
using UiFramework.Editor.Config;
using UiFramework.Editor.Window.Popups;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    // Validate / Fix across element scenes. Each method returns a short summary for the editor UI and logs the
    // full report to the Console. The IList overloads reuse scene info the caller already inspected (the Elements tab
    // list), so scenes are not opened an extra time just to plan the operation.
    public static class UiElementSceneBatchOperations
    {
        public static string ValidateAll(UiEditorConfig config, UiGlobalCanvasSettings settings)
        {
            List<UiElementSceneInfo> infos = InspectAll(config, settings, "Validating Element Scenes");
            if (infos == null)
            {
                return "Validation cancelled.";
            }

            if (infos.Count == 0)
            {
                return LogNoScenes(config);
            }

            return LogValidationReport(infos);
        }

        public static string LogValidationReport(IList<UiElementSceneInfo> infos)
        {
            int valid = 0;
            int withIssues = 0;
            StringBuilder lines = new StringBuilder();
            StringBuilder details = new StringBuilder();

            for (int i = 0; i < infos.Count; i++)
            {
                UiElementSceneInfo info = infos[i];
                if (!info.IsAvailable)
                {
                    lines.Append("\n  " + info.SceneName + ": could not be opened");
                    continue;
                }

                string structure = info.Structure.HasIssues ? info.Structure.GetCodesSummary() : "OK";
                lines.Append("\n  " + info.SceneName + ": " + structure);

                if (info.Structure.HasIssues)
                {
                    withIssues++;
                    details.Append("\n" + info.SceneName + ":\n" + info.Structure.GetIssuesText());
                }
                else
                {
                    valid++;
                }
            }

            string summary = "Validated " + infos.Count + " element scene(s): " + valid + " follow the contract, " + withIssues + " have structure issues.";
            string message = "[UiFramework] " + summary + lines;

            if (details.Length > 0)
            {
                message += "\n\nDetails:" + details;
            }

            if (withIssues > 0)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }

            return summary;
        }

        public static string FixStructureInAll(UiEditorConfig config, UiGlobalCanvasSettings settings)
        {
            List<UiElementSceneInfo> infos = InspectAll(config, settings, "Checking Element Scene Structure");
            if (infos == null)
            {
                return "Cancelled.";
            }

            if (infos.Count == 0)
            {
                return LogNoScenes(config);
            }

            return FixStructureInAll(infos, settings);
        }

        // FixStructure re-validates each scene before changing it, so slightly stale infos only affect the plan text.
        public static string FixStructureInAll(IList<UiElementSceneInfo> infos, UiGlobalCanvasSettings settings)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return "Cancelled.";
            }

            List<UiElementSceneInfo> toFix = new List<UiElementSceneInfo>();

            for (int i = 0; i < infos.Count; i++)
            {
                UiElementSceneInfo info = infos[i];
                if (info.IsAvailable && info.Structure.HasFixableIssues)
                {
                    toFix.Add(info);
                }
            }

            if (toFix.Count == 0)
            {
                string nothing = "Fix Structure: no scene has auto-fixable issues.";
                Debug.Log("[UiFramework] " + nothing + DescribeRemaining(infos));
                return nothing;
            }

            List<string> wrapPlan = BuildFixPlan(toFix);
            Debug.Log("[UiFramework] Fix Structure plan:\n  " + string.Join("\n  ", wrapPlan.ToArray()));

            UiElementChangeDialogModel dialogModel = new UiElementChangeDialogModel
            {
                WindowTitle = "Fix All Structure",
                Heading = "Fix the structure of " + DescribeScenes(toFix.Count) + "?",
                Description = "Each scene gets one full-screen Root under its Canvas, so the whole screen animates. Expand a scene to see its changes.",
                Footer = "Nothing is deleted or resized. Scenes that aren't open are saved; open scenes stay unsaved so you can undo.",
                ConfirmLabel = "Fix " + DescribeScenes(toFix.Count)
            };

            for (int i = 0; i < toFix.Count; i++)
            {
                UiElementStructureReport structure = toFix[i].Structure;
                List<string> manualWork = UiElementStructureFixer.SummarizeManualWork(structure);
                List<string> changes = UiElementStructureFixer.DescribePlannedChanges(structure);

                UiElementChangeEntry entry = new UiElementChangeEntry
                {
                    Title = toFix[i].SceneName,
                    Subtitle = (changes.Count == 1 ? "1 change" : changes.Count + " changes") + (manualWork.Count > 0 ? ", " + manualWork.Count + " manual" : string.Empty)
                };
                entry.Changes.AddRange(changes);
                entry.ManualWork.AddRange(manualWork);

                dialogModel.Entries.Add(entry);
            }

            if (!UiElementChangeDialog.Show(dialogModel))
            {
                return "Cancelled.";
            }

            int fixedCount = 0;
            List<string> leftOpen = new List<string>();
            Dictionary<string, UiElementStructureReport> afterReports = new Dictionary<string, UiElementStructureReport>();

            try
            {
                for (int i = 0; i < toFix.Count; i++)
                {
                    UiElementSceneInfo info = toFix[i];
                    EditorUtility.DisplayProgressBar("Fixing Element Scene Structure", info.SceneName, (float)i / toFix.Count);

                    bool isOpen = UiElementSceneUtility.IsSceneOpen(info.ScenePath);
                    bool done = UiElementSceneUtility.WithScene(info.ScenePath, true, scene =>
                    {
                        afterReports[info.ScenePath] = UiElementStructureFixer.FixStructure(scene, settings);
                    });

                    if (done)
                    {
                        fixedCount++;
                        if (isOpen)
                        {
                            leftOpen.Add(info.SceneName);
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            List<UiElementSceneInfo> remainingInfos = new List<UiElementSceneInfo>();
            for (int i = 0; i < infos.Count; i++)
            {
                UiElementSceneInfo info = infos[i];
                UiElementStructureReport after;
                if (afterReports.TryGetValue(info.ScenePath, out after))
                {
                    info.Structure = after;
                }

                remainingInfos.Add(info);
            }

            string summary = "Fix Structure: fixed " + fixedCount + " scene(s).";
            string remaining = DescribeRemaining(remainingInfos);

            if (remaining.Length > 0)
            {
                Debug.LogWarning("[UiFramework] " + summary + DescribeLeftOpen(leftOpen) + remaining);
            }
            else
            {
                Debug.Log("[UiFramework] " + summary + DescribeLeftOpen(leftOpen));
            }

            return summary;
        }

        public static List<string> BuildFixPlan(IList<UiElementSceneInfo> infos)
        {
            List<string> plan = new List<string>();

            for (int i = 0; i < infos.Count; i++)
            {
                List<string> changes = UiElementStructureFixer.DescribePlannedChanges(infos[i].Structure);
                for (int c = 0; c < changes.Count; c++)
                {
                    plan.Add(infos[i].SceneName + ": " + changes[c]);
                }
            }

            return plan;
        }

        private static List<UiElementSceneInfo> InspectAll(UiEditorConfig config, UiGlobalCanvasSettings settings, string title)
        {
            List<string> scenePaths = UiElementSceneUtility.FindElementScenePaths(config);
            List<UiElementSceneInfo> infos = new List<UiElementSceneInfo>(scenePaths.Count);

            try
            {
                for (int i = 0; i < scenePaths.Count; i++)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(scenePaths[i]);
                    if (EditorUtility.DisplayCancelableProgressBar(title, name, (float)i / scenePaths.Count))
                    {
                        return null;
                    }

                    infos.Add(UiElementSceneUtility.InspectScene(scenePaths[i], settings));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return infos;
        }

        private static string DescribeRemaining(IList<UiElementSceneInfo> infos)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < infos.Count; i++)
            {
                List<string> lines = UiElementStructureFixer.DescribeUnfixableIssues(infos[i].Structure);
                for (int l = 0; l < lines.Count; l++)
                {
                    builder.Append("\n  " + infos[i].SceneName + ": " + lines[l]);
                }
            }

            if (builder.Length == 0)
            {
                return string.Empty;
            }

            return "\nNeeds manual attention:" + builder;
        }

        private static string DescribeLeftOpen(List<string> sceneNames)
        {
            if (sceneNames.Count == 0)
            {
                return string.Empty;
            }

            return "\nEdited in place and left unsaved (they were open in the editor): " + string.Join(", ", sceneNames.ToArray());
        }

        private static string DescribeScenes(int count)
        {
            return count == 1 ? "1 scene" : count + " scenes";
        }

        private static string LogNoScenes(UiEditorConfig config)
        {
            string folder = config != null ? config.ElementsScenePath : "(no UiEditorConfig)";
            string message = "No element scenes found under '" + folder + "'.";
            Debug.LogWarning("[UiFramework] " + message);
            return message;
        }
    }
}
