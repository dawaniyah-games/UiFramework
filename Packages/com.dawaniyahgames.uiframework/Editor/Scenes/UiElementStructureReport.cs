using System.Collections.Generic;
using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    public enum UiElementStructureIssueCode
    {
        NoUiElement,
        ExtraRootObjects,
        NoCanvas,
        CanvasMissingComponents,
        NoContentRoot,
        ContentRootNotStretched,
        ContentRootInactive,
        CanvasInsidePrefab
    }

    public enum UiCanvasStatus
    {
        Matches,
        Differs,
        NoCanvas,
        Excluded
    }

    public sealed class UiElementStructureIssue
    {
        public UiElementStructureIssueCode Code { get; }

        // One short plain-language line for the editor UI, e.g. "'Bar_bg' doesn't fill the screen".
        public string Summary { get; }

        // Full explanation for the Console and validation reports.
        public string Message { get; }
        public bool IsFixable { get; }
        public bool IsWarning { get; }

        // Name of the scene object the issue is about (used to describe fixes after the scene is closed).
        public string ObjectName { get; }

        // Short description of what FixStructure does for this issue. Empty when not fixable.
        public string FixDescription { get; }

        public UiElementStructureIssue(UiElementStructureIssueCode code, string summary, string message, bool isFixable, bool isWarning, string objectName, string fixDescription)
        {
            Code = code;
            Summary = string.IsNullOrEmpty(summary) ? message : summary;
            Message = message;
            IsFixable = isFixable;
            IsWarning = isWarning;
            ObjectName = objectName ?? string.Empty;
            FixDescription = isFixable ? (fixDescription ?? string.Empty) : string.Empty;
        }

        public override string ToString()
        {
            return Code + ": " + Message;
        }
    }

    public sealed class UiElementStructureReport
    {
        private readonly List<UiElementStructureIssue> issues = new List<UiElementStructureIssue>();

        public string SceneName { get; }

        // Scene objects found during validation. Only valid while the validated scene stays open.
        public GameObject ElementRoot { get; internal set; }
        public Canvas Canvas { get; internal set; }
        public RectTransform ContentRoot { get; internal set; }

        public UiElementStructureReport(string sceneName)
        {
            SceneName = sceneName ?? string.Empty;
        }

        public IReadOnlyList<UiElementStructureIssue> Issues
        {
            get
            {
                return issues.AsReadOnly();
            }
        }

        public bool HasIssues
        {
            get
            {
                return issues.Count > 0;
            }
        }

        public bool HasFixableIssues
        {
            get
            {
                for (int i = 0; i < issues.Count; i++)
                {
                    if (issues[i].IsFixable)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool HasUnfixableIssues
        {
            get
            {
                for (int i = 0; i < issues.Count; i++)
                {
                    if (!issues[i].IsFixable && !issues[i].IsWarning)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Has(UiElementStructureIssueCode code)
        {
            return Find(code) != null;
        }

        public bool HasFixable(UiElementStructureIssueCode code)
        {
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i].Code == code && issues[i].IsFixable)
                {
                    return true;
                }
            }

            return false;
        }

        public UiElementStructureIssue Find(UiElementStructureIssueCode code)
        {
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i].Code == code)
                {
                    return issues[i];
                }
            }

            return null;
        }

        public string GetCodesSummary()
        {
            List<string> codes = new List<string>();

            for (int i = 0; i < issues.Count; i++)
            {
                string code = issues[i].Code.ToString();
                if (!codes.Contains(code))
                {
                    codes.Add(code);
                }
            }

            return string.Join(", ", codes.ToArray());
        }

        // Short bullet list for tooltips and dialogs.
        public string GetSummaryText()
        {
            List<string> lines = new List<string>(issues.Count);

            for (int i = 0; i < issues.Count; i++)
            {
                lines.Add("• " + issues[i].Summary);
            }

            return string.Join("\n", lines.ToArray());
        }

        public string GetIssuesText()
        {
            List<string> lines = new List<string>(issues.Count);

            for (int i = 0; i < issues.Count; i++)
            {
                UiElementStructureIssue issue = issues[i];
                string suffix = issue.IsWarning ? " (warning)" : (issue.IsFixable ? string.Empty : " (not auto-fixable)");
                lines.Add("- " + issue.Code + ": " + issue.Message + suffix);
            }

            return string.Join("\n", lines.ToArray());
        }

        internal void Add(UiElementStructureIssue issue)
        {
            if (issue != null)
            {
                issues.Add(issue);
            }
        }
    }

    // One Canvas/CanvasScaler value that differs from the global canvas settings, e.g. ("ScreenMatchMode", "MatchWidthOrHeight", "Expand").
    public sealed class UiCanvasSettingDifference
    {
        public string Setting { get; }
        public string Current { get; }
        public string Target { get; }

        public UiCanvasSettingDifference(string setting, string current, string target)
        {
            Setting = setting;
            Current = current;
            Target = target;
        }

        public override string ToString()
        {
            return Setting + ": " + Current + " → " + Target;
        }
    }

    public sealed class UiElementSceneInfo
    {
        public string ScenePath { get; }
        public string SceneName { get; }

        // False when the scene could not be opened (e.g. in Play Mode); the other values are then empty.
        public bool IsAvailable { get; internal set; }
        public bool HasUiElement { get; internal set; }
        public string ShowPreset { get; internal set; }
        public string HidePreset { get; internal set; }
        public UiElementStructureReport Structure { get; internal set; }
        public UiCanvasStatus CanvasStatus { get; internal set; }
        public string CanvasDiff { get; internal set; }
        public IReadOnlyList<UiCanvasSettingDifference> CanvasDifferences { get; internal set; }

        public UiElementSceneInfo(string scenePath)
        {
            ScenePath = scenePath ?? string.Empty;
            SceneName = System.IO.Path.GetFileNameWithoutExtension(ScenePath);
            Structure = new UiElementStructureReport(SceneName);
            CanvasStatus = UiCanvasStatus.NoCanvas;
            CanvasDiff = string.Empty;
            CanvasDifferences = new List<UiCanvasSettingDifference>().AsReadOnly();
        }
    }
}
