using System;
using System.Collections.Generic;
using UiFramework.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Checks a UI element scene against the contract (see DESIGN.md §7.1):
    //
    //   <Name>Root   plain Transform with the UiElement, the only root GameObject in the scene
    //   └─ Canvas    child index 0: root Canvas + CanvasScaler + GraphicRaycaster
    //      └─ Root   the only Canvas child: stretched RectTransform, the animation target
    //
    // Names are not checked, only structure.
    public static class UiElementStructureValidator
    {
        private const int MaxListedNames = 6;

        public static bool ValidateStructure(Scene scene, out UiElementStructureReport report)
        {
            report = new UiElementStructureReport(scene.IsValid() ? scene.name : string.Empty);

            if (!scene.IsValid() || !scene.isLoaded)
            {
                report.Add(new UiElementStructureIssue(UiElementStructureIssueCode.NoUiElement, "Scene isn't loaded", "Scene is not loaded, so it could not be validated.", false, false, string.Empty, null));
                return false;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            bool hasUiElement;
            GameObject elementRoot = FindElementRoot(scene.name, roots, out hasUiElement);

            if (!hasUiElement)
            {
                string noUiElementSummary;
                string noUiElementMessage = BuildNoUiElementMessage(roots, elementRoot, out noUiElementSummary);
                report.Add(new UiElementStructureIssue(UiElementStructureIssueCode.NoUiElement, noUiElementSummary, noUiElementMessage, false, false, elementRoot != null ? elementRoot.name : string.Empty, null));
            }

            if (elementRoot == null)
            {
                return false;
            }

            report.ElementRoot = elementRoot;

            for (int i = 0; i < roots.Length; i++)
            {
                GameObject other = roots[i];
                if (other == elementRoot)
                {
                    continue;
                }

                string state = other.activeSelf ? "active" : "inactive";
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.ExtraRootObjects,
                    "Extra object '" + other.name + "' at the scene root (" + state + "): move it inside the element or delete it",
                    "Extra root object '" + other.name + "' (" + state + "). Only '" + elementRoot.name + "' may be a root object; move it under the element or delete it manually.",
                    false,
                    false,
                    other.name,
                    null));
            }

            Canvas canvas = ValidateCanvasPlacement(elementRoot.transform, report);
            if (canvas == null)
            {
                return false;
            }

            report.Canvas = canvas;
            ValidateCanvasAndContent(canvas, report);
            return !report.HasIssues;
        }

        public static bool IsStretchedToParent(RectTransform rectTransform, out string difference)
        {
            List<string> diffs = new List<string>();

            if (!UiSceneValues.Approximately(rectTransform.anchorMin, Vector2.zero, UiSceneValues.UnitTolerance) || !UiSceneValues.Approximately(rectTransform.anchorMax, Vector2.one, UiSceneValues.UnitTolerance))
            {
                diffs.Add("anchors " + UiSceneValues.FormatVector(rectTransform.anchorMin) + "-" + UiSceneValues.FormatVector(rectTransform.anchorMax));
            }

            if (!UiSceneValues.Approximately(rectTransform.offsetMin, Vector2.zero, UiSceneValues.PositionTolerance) || !UiSceneValues.Approximately(rectTransform.offsetMax, Vector2.zero, UiSceneValues.PositionTolerance))
            {
                diffs.Add("offsets min " + UiSceneValues.FormatVector(rectTransform.offsetMin) + " max " + UiSceneValues.FormatVector(rectTransform.offsetMax));
            }

            if (!UiSceneValues.Approximately(rectTransform.pivot, new Vector2(0.5f, 0.5f), UiSceneValues.UnitTolerance))
            {
                diffs.Add("pivot " + UiSceneValues.FormatVector(rectTransform.pivot));
            }

            if (!UiSceneValues.Approximately(rectTransform.anchoredPosition, Vector2.zero, UiSceneValues.PositionTolerance))
            {
                diffs.Add("anchoredPosition " + UiSceneValues.FormatVector(rectTransform.anchoredPosition));
            }

            Vector3 scale = rectTransform.localScale;
            if (Mathf.Abs(scale.x - 1.0f) > UiSceneValues.UnitTolerance || Mathf.Abs(scale.y - 1.0f) > UiSceneValues.UnitTolerance || Mathf.Abs(scale.z - 1.0f) > UiSceneValues.UnitTolerance)
            {
                diffs.Add("scale " + UiSceneValues.FormatVector(scale));
            }

            if (Quaternion.Angle(rectTransform.localRotation, Quaternion.identity) > UiSceneValues.RotationToleranceDegrees)
            {
                diffs.Add("rotation " + UiSceneValues.FormatVector(rectTransform.localEulerAngles));
            }

            difference = string.Join(", ", diffs.ToArray());
            return diffs.Count == 0;
        }

        public static bool IsRootCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return false;
            }

            Transform parent = canvas.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<Canvas>() != null)
                {
                    return false;
                }

                parent = parent.parent;
            }

            return true;
        }

        private static GameObject FindElementRoot(string sceneName, GameObject[] roots, out bool hasUiElement)
        {
            hasUiElement = false;

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].GetComponent<UiElement>() != null)
                {
                    hasUiElement = true;
                    return roots[i];
                }
            }

            if (roots.Length == 0)
            {
                return null;
            }

            // No UiElement: guess the intended element root so the remaining checks still run.
            string expectedName = sceneName + "Root";
            for (int i = 0; i < roots.Length; i++)
            {
                if (string.Equals(roots[i].name, expectedName, StringComparison.Ordinal))
                {
                    return roots[i];
                }
            }

            if (roots.Length == 1)
            {
                return roots[0];
            }

            for (int i = 0; i < roots.Length; i++)
            {
                Transform rootTransform = roots[i].transform;
                if (rootTransform.childCount > 0 && rootTransform.GetChild(0).GetComponent<Canvas>() != null)
                {
                    return roots[i];
                }
            }

            return roots[0];
        }

        private static string BuildNoUiElementMessage(GameObject[] roots, GameObject elementRoot, out string summary)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                UiElement nested = roots[i].GetComponentInChildren<UiElement>(true);
                if (nested != null)
                {
                    summary = "The UiElement script is on '" + nested.name + "': move it to the root object";
                    return "UiElement '" + nested.GetType().Name + "' is on '" + GetPath(nested.transform) + "', but it must be on the scene's root object.";
                }
            }

            for (int i = 0; i < roots.Length; i++)
            {
                if (HasMissingScript(roots[i]))
                {
                    summary = "'" + roots[i].name + "' has a missing script: re-assign the UiElement script";
                    return "Root object '" + roots[i].name + "' has a missing script. The UiElement class may have been renamed, deleted or failed to compile; fix or re-assign it.";
                }
            }

            if (elementRoot == null)
            {
                summary = "The scene is empty";
                return "The scene has no root objects.";
            }

            summary = "No UiElement script: add it to '" + elementRoot.name + "'";
            return "No root GameObject has a UiElement component. Add your UiElement subclass to '" + elementRoot.name + "'.";
        }

        // A component whose script class can't be resolved comes back as a null entry. That is more reliable than
        // GetMonoBehavioursWithMissingScriptCount, which misses scripts whose MonoScript exists but has no class.
        private static bool HasMissingScript(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    return true;
                }
            }

            return GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0;
        }

        // Returns the root Canvas to keep validating (the one at index 0, or a misplaced one), or null.
        private static Canvas ValidateCanvasPlacement(Transform elementRoot, UiElementStructureReport report)
        {
            string rootName = elementRoot.name;
            bool rootIsInPrefab = PrefabUtility.IsPartOfPrefabInstance(elementRoot.gameObject);

            if (elementRoot.childCount == 0)
            {
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.NoCanvas,
                    "No Canvas yet",
                    "'" + rootName + "' has no children. Expected a Canvas as its first child.",
                    !rootIsInPrefab,
                    false,
                    rootName,
                    "Add a Canvas with an empty full-screen Root"));
                return null;
            }

            if (elementRoot.GetComponent<Canvas>() != null)
            {
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.NoCanvas,
                    "'" + rootName + "' has its own Canvas: move the Canvas to a child object",
                    "'" + rootName + "' has a Canvas itself. The element root must be a plain Transform with the Canvas as its first child.",
                    false,
                    false,
                    rootName,
                    null));
                return null;
            }

            Transform first = elementRoot.GetChild(0);
            Canvas firstCanvas = first.GetComponent<Canvas>();
            if (firstCanvas != null && IsRootCanvas(firstCanvas))
            {
                return firstCanvas;
            }

            for (int i = 1; i < elementRoot.childCount; i++)
            {
                Canvas candidate = elementRoot.GetChild(i).GetComponent<Canvas>();
                if (candidate != null && IsRootCanvas(candidate))
                {
                    report.Add(new UiElementStructureIssue(
                        UiElementStructureIssueCode.NoCanvas,
                        "The Canvas isn't the first child",
                        "Canvas '" + candidate.name + "' is child " + i + " of '" + rootName + "'; it must be the first child ('" + first.name + "' is first).",
                        !rootIsInPrefab,
                        false,
                        candidate.name,
                        "Move '" + candidate.name + "' to the top"));
                    return candidate;
                }
            }

            bool allUiChildren = true;
            for (int i = 0; i < elementRoot.childCount; i++)
            {
                Transform child = elementRoot.GetChild(i);
                if (!(child is RectTransform) || child.GetComponent<Canvas>() != null)
                {
                    allUiChildren = false;
                    break;
                }
            }

            if (allUiChildren)
            {
                int count = elementRoot.childCount;
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.NoCanvas,
                    "UI without a Canvas",
                    "'" + rootName + "' has " + count + " UI child(ren) (" + ListChildNames(elementRoot) + ") but no Canvas.",
                    !rootIsInPrefab,
                    false,
                    rootName,
                    "Add a Canvas and move " + DescribeCount(count) + " into it"));
                return null;
            }

            report.Add(new UiElementStructureIssue(
                UiElementStructureIssueCode.NoCanvas,
                "'" + first.name + "' is first, but it isn't a Canvas: rearrange the objects by hand",
                "First child '" + first.name + "' of '" + rootName + "' is not a Canvas, and '" + rootName + "' has non-UI children, so it cannot be wrapped automatically.",
                false,
                false,
                first.name,
                null));
            return null;
        }

        private static void ValidateCanvasAndContent(Canvas canvas, UiElementStructureReport report)
        {
            string canvasName = canvas.name;
            bool canvasIsInPrefab = PrefabUtility.IsPartOfPrefabInstance(canvas.gameObject);

            if (canvasIsInPrefab)
            {
                GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(canvas.gameObject);
                string prefabName = prefabRoot != null ? prefabRoot.name : canvasName;
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.CanvasInsidePrefab,
                    "The Canvas is part of prefab '" + prefabName + "': fix it inside the prefab",
                    "Canvas '" + canvasName + "' is part of prefab instance '" + prefabName + "', so its children cannot be restructured in the scene. Fix the hierarchy in the prefab, or unpack it.",
                    false,
                    false,
                    canvasName,
                    null));
            }

            bool missingScaler = canvas.GetComponent<CanvasScaler>() == null;
            bool missingRaycaster = canvas.GetComponent<GraphicRaycaster>() == null;
            if (missingScaler || missingRaycaster)
            {
                string missing = missingScaler && missingRaycaster ? "CanvasScaler and GraphicRaycaster" : (missingScaler ? "CanvasScaler" : "GraphicRaycaster");
                string missingFriendly = missingScaler && missingRaycaster ? "Canvas Scaler and Graphic Raycaster" : (missingScaler ? "Canvas Scaler" : "Graphic Raycaster");
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.CanvasMissingComponents,
                    "The Canvas has no " + missingFriendly,
                    "Canvas '" + canvasName + "' is missing " + missing + ".",
                    true,
                    false,
                    canvasName,
                    "Add " + missingFriendly + " to the Canvas"));
            }

            Transform canvasTransform = canvas.transform;
            int childCount = canvasTransform.childCount;

            if (childCount == 0)
            {
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.NoContentRoot,
                    "The Canvas is empty",
                    "Canvas '" + canvasName + "' has no children. Expected one stretched content root.",
                    true,
                    false,
                    canvasName,
                    "Add an empty full-screen Root"));
                return;
            }

            string wrapDescription = childCount == 1
                ? "Put '" + canvasTransform.GetChild(0).name + "' inside a new full-screen Root"
                : "Put the " + childCount + " objects inside a new full-screen Root (same order)";

            if (childCount > 1)
            {
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.NoContentRoot,
                    childCount + " objects sit directly under the Canvas, so only '" + canvasTransform.GetChild(0).name + "' animates",
                    "Canvas '" + canvasName + "' has " + childCount + " direct children (" + ListChildNames(canvasTransform) + "); only the first one would animate.",
                    !canvasIsInPrefab,
                    false,
                    canvasName,
                    wrapDescription));
                return;
            }

            Transform only = canvasTransform.GetChild(0);
            RectTransform contentRoot = only as RectTransform;
            string problem = null;
            string problemSummary = null;

            if (PrefabUtility.IsAnyPrefabInstanceRoot(only.gameObject) && !canvasIsInPrefab)
            {
                problemSummary = "'" + only.name + "' is a prefab, so it can't be the Root";
                problem = "The only child of Canvas '" + canvasName + "', '" + only.name + "', is a prefab instance, so it cannot be the content root.";
            }
            else if (contentRoot == null)
            {
                problemSummary = "'" + only.name + "' isn't a UI object";
                problem = "The only child of Canvas '" + canvasName + "', '" + only.name + "', has no RectTransform.";
            }
            else if (only.GetComponent<Canvas>() != null)
            {
                problemSummary = "'" + only.name + "' has its own Canvas, so it won't animate";
                problem = "The only child of Canvas '" + canvasName + "', '" + only.name + "', has its own Canvas, so the animation would skip it and animate its first child instead.";
            }

            if (problem != null)
            {
                report.Add(new UiElementStructureIssue(UiElementStructureIssueCode.NoContentRoot, problemSummary, problem, !canvasIsInPrefab, false, only.name, wrapDescription));
                return;
            }

            report.ContentRoot = contentRoot;

            string stretchDifference;
            if (!IsStretchedToParent(contentRoot, out stretchDifference))
            {
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.ContentRootNotStretched,
                    "'" + only.name + "' doesn't fill the screen, so slides and scales look wrong",
                    "Content root '" + only.name + "' is not stretched to the canvas (" + stretchDifference + ").",
                    !canvasIsInPrefab,
                    false,
                    only.name,
                    "Put '" + only.name + "' inside a new full-screen Root ('" + only.name + "' keeps its size and position)"));
            }

            if (!only.gameObject.activeSelf)
            {
                report.Add(new UiElementStructureIssue(
                    UiElementStructureIssueCode.ContentRootInactive,
                    "'" + only.name + "' is turned off, so nothing shows until it's turned on",
                    "Content root '" + only.name + "' is inactive; the element is invisible until something activates it.",
                    false,
                    true,
                    only.name,
                    null));
            }
        }


        private static string DescribeCount(int count)
        {
            return count == 1 ? "it" : "the " + count + " objects";
        }

        private static string ListChildNames(Transform parent)
        {
            List<string> names = new List<string>();
            int count = parent.childCount;

            for (int i = 0; i < count && i < MaxListedNames; i++)
            {
                names.Add(parent.GetChild(i).name);
            }

            string joined = string.Join(", ", names.ToArray());
            if (count > MaxListedNames)
            {
                joined += ", +" + (count - MaxListedNames) + " more";
            }

            return joined;
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            Transform parent = transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
