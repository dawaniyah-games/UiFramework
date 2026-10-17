using System.Collections.Generic;
using UiFramework.Editor.Scenes;

namespace UiFramework.Editor.Window.Popups
{
    // Builds the content of each confirmation popup, so the wording lives in one place and the tabs only decide
    // when to ask.
    public static class UiElementChangeDialogs
    {
        // Single-scene structure fix. The report's scene must still be open (the Now / After preview reads it).
        public static UiElementChangeDialogModel ForStructureFix(string sceneName, UiElementStructureReport report, bool willBeSaved, bool hideMissingUiElement)
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

            UiElementChangeDialogModel model = new UiElementChangeDialogModel
            {
                WindowTitle = "Fix Structure",
                Heading = "Fix the structure of " + sceneName + "?",
                Description = "For the show/hide animation to move the whole screen, everything has to sit inside one full-screen Root under the Canvas.",
                Footer = willBeSaved ? "Nothing is deleted or resized. The scene will be saved." : "Nothing is deleted or resized. The scene is open, so you can undo with Ctrl+Z. Save the scene to keep the changes.",
                ConfirmLabel = "Fix"
            };
            model.Entries.Add(entry);
            return model;
        }

        public static UiElementChangeDialogModel ForFixAll(IList<UiElementSceneInfo> toFix)
        {
            UiElementChangeDialogModel model = new UiElementChangeDialogModel
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
                List<string> changes = UiElementStructureFixer.DescribePlannedChanges(structure);
                List<string> manualWork = UiElementStructureFixer.SummarizeManualWork(structure);

                UiElementChangeEntry entry = new UiElementChangeEntry
                {
                    Title = toFix[i].SceneName,
                    Subtitle = (changes.Count == 1 ? "1 change" : changes.Count + " changes") + (manualWork.Count > 0 ? ", " + manualWork.Count + " manual" : string.Empty)
                };
                entry.Changes.AddRange(changes);
                entry.ManualWork.AddRange(manualWork);
                model.Entries.Add(entry);
            }

            return model;
        }

        public static UiElementChangeDialogModel ForUseDefaultCanvas(UiElementSceneInfo info, bool sceneIsOpen)
        {
            UiElementChangeEntry entry = new UiElementChangeEntry { Title = info.SceneName };
            for (int i = 0; i < info.CanvasDifferences.Count; i++)
            {
                entry.SettingChanges.Add(info.CanvasDifferences[i]);
            }

            UiElementChangeDialogModel model = new UiElementChangeDialogModel
            {
                WindowTitle = "Use Default Settings",
                Heading = "Use the default canvas settings for " + info.SceneName + "?",
                Description = "These values change to the Global Canvas Settings (General tab).",
                Footer = "Sorting order, cameras and everything else stay as they are." +
                    (sceneIsOpen ? " The scene is open, so you can undo with Ctrl+Z. Save the scene to keep the change." : " The scene will be saved."),
                ConfirmLabel = "Use Defaults"
            };
            model.Entries.Add(entry);
            return model;
        }

        public static UiElementChangeDialogModel ForManualWork(UiElementSceneInfo info)
        {
            UiElementChangeEntry entry = new UiElementChangeEntry { Title = info.SceneName };
            entry.ManualWork.AddRange(UiElementStructureFixer.SummarizeManualWork(info.Structure));

            UiElementChangeDialogModel model = new UiElementChangeDialogModel
            {
                WindowTitle = "Structure",
                Heading = info.SceneName + " needs a manual fix",
                Description = "These can't be fixed automatically. Open the scene and change them by hand.",
                ConfirmLabel = "Open Scene",
                CancelLabel = "Close"
            };
            model.Entries.Add(entry);
            return model;
        }

        private static string DescribeScenes(int count)
        {
            return count == 1 ? "1 scene" : count + " scenes";
        }
    }
}
