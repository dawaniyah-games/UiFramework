using System.Collections.Generic;
using System.Text;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    // Validate / Fix across element scenes the caller has already inspected. Pure editor logic: no dialogs or prompts
    // here, the UI decides what to ask. Each method returns a short summary and logs the full report to the Console.
    public static class UiElementSceneBatchOperations
    {
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

        public static List<UiElementSceneInfo> GetScenesToFix(IList<UiElementSceneInfo> infos)
        {
            List<UiElementSceneInfo> toFix = new List<UiElementSceneInfo>();

            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i].IsAvailable && infos[i].Structure.HasFixableIssues)
                {
                    toFix.Add(infos[i]);
                }
            }

            return toFix;
        }

        // Fixes every given scene (FixStructure re-validates each one first, so slightly stale infos are safe) and
        // updates each info's Structure with what is left afterwards.
        public static string FixStructure(IList<UiElementSceneInfo> toFix, UiGlobalCanvasSettings settings)
        {
            int fixedCount = 0;
            List<string> leftOpen = new List<string>();

            try
            {
                for (int i = 0; i < toFix.Count; i++)
                {
                    UiElementSceneInfo info = toFix[i];
                    EditorUtility.DisplayProgressBar("Fixing Element Scene Structure", info.SceneName, (float)i / toFix.Count);

                    bool isOpen = UiElementSceneAccess.IsSceneOpen(info.ScenePath);
                    bool done = UiElementSceneAccess.WithScene(info.ScenePath, true, scene =>
                    {
                        info.Structure = UiElementStructureFixer.FixStructure(scene, settings);
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

            string summary = "Fix Structure: fixed " + fixedCount + " scene(s).";
            string remaining = DescribeRemaining(toFix);

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

        public static void LogFixResult(string sceneName, UiElementStructureReport remaining)
        {
            if (remaining.HasIssues)
            {
                Debug.LogWarning("[UiFramework] Fixed structure of '" + sceneName + "'. Remaining issues:\n" + remaining.GetIssuesText());
                return;
            }

            Debug.Log("[UiFramework] Fixed structure of '" + sceneName + "'; it now follows the UI element scene contract.");
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
    }
}
