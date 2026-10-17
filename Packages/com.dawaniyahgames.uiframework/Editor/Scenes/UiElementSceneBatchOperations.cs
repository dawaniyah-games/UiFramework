using System.Collections.Generic;
using System.Text;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Validate / Fix / Apply across element scenes. Each method returns a short summary for the editor UI and logs the
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
            int canvasDiffers = 0;
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
                lines.Append("\n  " + info.SceneName + ": " + structure + " | Canvas: " + DescribeCanvasStatus(info));

                if (info.Structure.HasIssues)
                {
                    withIssues++;
                    details.Append("\n" + info.SceneName + ":\n" + info.Structure.GetIssuesText());
                }
                else
                {
                    valid++;
                }

                if (info.CanvasStatus == UiCanvasStatus.Differs)
                {
                    canvasDiffers++;
                }
            }

            string summary = "Validated " + infos.Count + " element scene(s): " + valid + " follow the contract, " + withIssues + " have structure issues, " + canvasDiffers + " differ from the global canvas settings.";
            string message = "[UiFramework] " + summary + lines;

            if (details.Length > 0)
            {
                message += "\n\nDetails:" + details;
            }

            if (withIssues > 0 || canvasDiffers > 0)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }

            return summary;
        }

        public static string ApplyCanvasSettingsToAll(UiEditorConfig config, UiGlobalCanvasSettings settings)
        {
            if (settings == null)
            {
                return "No UiGlobalCanvasSettings asset.";
            }

            List<UiElementSceneInfo> infos = InspectAll(config, settings, "Checking Global Canvas Settings");
            if (infos == null)
            {
                return "Cancelled.";
            }

            if (infos.Count == 0)
            {
                return LogNoScenes(config);
            }

            return ApplyCanvasSettingsToAll(infos, settings);
        }

        public static string ApplyCanvasSettingsToAll(IList<UiElementSceneInfo> infos, UiGlobalCanvasSettings settings)
        {
            if (settings == null)
            {
                return "No UiGlobalCanvasSettings asset.";
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return "Cancelled.";
            }

            List<UiElementSceneInfo> toUpdate = new List<UiElementSceneInfo>();
            List<string> plan = new List<string>();
            int matching = 0;
            int excluded = 0;
            int noCanvas = 0;
            int unavailable = 0;

            for (int i = 0; i < infos.Count; i++)
            {
                UiElementSceneInfo info = infos[i];
                if (!info.IsAvailable)
                {
                    unavailable++;
                    continue;
                }

                switch (info.CanvasStatus)
                {
                    case UiCanvasStatus.Matches:
                        matching++;
                        break;
                    case UiCanvasStatus.Excluded:
                        excluded++;
                        break;
                    case UiCanvasStatus.NoCanvas:
                        noCanvas++;
                        break;
                    default:
                        toUpdate.Add(info);
                        plan.Add(info.SceneName + ": " + info.CanvasDiff);
                        break;
                }
            }

            if (toUpdate.Count == 0)
            {
                string nothing = "Apply Global Canvas Settings: nothing to update (matching " + matching + ", excluded " + excluded + ", no canvas " + noCanvas + ").";
                Debug.Log("[UiFramework] " + nothing);
                return nothing;
            }

            Debug.Log("[UiFramework] Apply Global Canvas Settings plan:\n  " + string.Join("\n  ", plan.ToArray()));

            if (!UiElementSceneDialogs.Confirm("Apply Global Canvas Settings", "Update the Canvas/CanvasScaler in " + toUpdate.Count + " scene(s)? Sorting order, cameras and other Canvas fields are not changed.", plan, "Apply"))
            {
                return "Cancelled.";
            }

            int updated = 0;
            List<string> leftOpen = new List<string>();

            try
            {
                for (int i = 0; i < toUpdate.Count; i++)
                {
                    UiElementSceneInfo info = toUpdate[i];
                    EditorUtility.DisplayProgressBar("Applying Global Canvas Settings", info.SceneName, (float)i / toUpdate.Count);

                    bool isOpen = UiElementSceneUtility.IsSceneOpen(info.ScenePath);
                    bool applied = false;

                    UiElementSceneUtility.WithScene(info.ScenePath, true, scene =>
                    {
                        UiElementStructureReport report;
                        UiElementSceneUtility.ValidateStructure(scene, out report);
                        if (report.Canvas == null)
                        {
                            return;
                        }

                        UiElementSceneUtility.ApplyCanvasSettings(report.Canvas, report.Canvas.GetComponent<CanvasScaler>(), settings);
                        applied = true;
                    });

                    if (applied)
                    {
                        updated++;
                        if (isOpen)
                        {
                            leftOpen.Add(info.SceneName);
                        }
                    }
                    else
                    {
                        noCanvas++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            string summary = "Apply Global Canvas Settings: updated " + updated + ", already matching " + matching + ", excluded " + excluded + ", no canvas " + noCanvas + (unavailable > 0 ? ", could not open " + unavailable : string.Empty) + ".";
            Debug.Log("[UiFramework] " + summary + DescribeLeftOpen(leftOpen));
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
            bool offersReset = false;

            for (int i = 0; i < infos.Count; i++)
            {
                UiElementSceneInfo info = infos[i];
                if (info.IsAvailable && info.Structure.HasFixableIssues)
                {
                    toFix.Add(info);
                    offersReset |= UiElementStructureFixer.OffersContentRootReset(info.Structure);
                }
            }

            if (toFix.Count == 0)
            {
                string nothing = "Fix Structure: no scene has auto-fixable issues.";
                Debug.Log("[UiFramework] " + nothing + DescribeRemaining(infos));
                return nothing;
            }

            List<string> wrapPlan = BuildFixPlan(toFix, UiElementStructureFixMode.WrapContent);
            Debug.Log("[UiFramework] Fix Structure plan:\n  " + string.Join("\n  ", wrapPlan.ToArray()));

            UiElementStructureFixMode mode;
            string intro = "Fix the structure of " + toFix.Count + " scene(s)? Objects are only created and reparented, never deleted. Issues that cannot be fixed automatically are reported in the Console.";
            if (!UiElementSceneDialogs.ConfirmStructureFix("Fix Structure In All Scenes", intro, wrapPlan, offersReset, out mode))
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
                        afterReports[info.ScenePath] = UiElementStructureFixer.FixStructure(scene, settings, mode);
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

            string summary = "Fix Structure: fixed " + fixedCount + " scene(s) (" + (mode == UiElementStructureFixMode.WrapContent ? "keep layout" : "stretch existing") + ").";
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

        public static List<string> BuildFixPlan(IList<UiElementSceneInfo> infos, UiElementStructureFixMode mode)
        {
            List<string> plan = new List<string>();

            for (int i = 0; i < infos.Count; i++)
            {
                List<string> changes = UiElementStructureFixer.DescribePlannedChanges(infos[i].Structure, mode);
                for (int c = 0; c < changes.Count; c++)
                {
                    plan.Add(infos[i].SceneName + ": " + changes[c]);
                }
            }

            return plan;
        }

        public static string DescribeCanvasStatus(UiElementSceneInfo info)
        {
            switch (info.CanvasStatus)
            {
                case UiCanvasStatus.Matches:
                    return "matches";
                case UiCanvasStatus.Differs:
                    return "differs (" + info.CanvasDiff + ")";
                case UiCanvasStatus.Excluded:
                    return string.IsNullOrEmpty(info.CanvasDiff) ? "excluded" : "excluded (" + info.CanvasDiff + ")";
                default:
                    return "no canvas";
            }
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

        private static string LogNoScenes(UiEditorConfig config)
        {
            string folder = config != null ? config.ElementsScenePath : "(no UiEditorConfig)";
            string message = "No element scenes found under '" + folder + "'.";
            Debug.LogWarning("[UiFramework] " + message);
            return message;
        }
    }
}
