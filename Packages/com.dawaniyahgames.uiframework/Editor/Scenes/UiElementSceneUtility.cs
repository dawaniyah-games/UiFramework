using System;
using System.Collections.Generic;
using System.Globalization;
using UiFramework.Core;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Helpers for the UI element scene contract (see DESIGN.md §7.1):
    //
    //   <Name>Root   plain Transform with the UiElement, the only root GameObject in the scene
    //   └─ Canvas    child index 0: root Canvas + CanvasScaler + GraphicRaycaster, values from UiGlobalCanvasSettings
    //      └─ Root   the only Canvas child: stretched RectTransform, the animation target
    //
    // Names are not checked, only structure.
    public static class UiElementSceneUtility
    {
        private const float positionTolerance = 0.01f;
        private const float unitTolerance = 0.0001f;
        private const float rotationToleranceDegrees = 0.01f;
        private const int maxListedNames = 6;

        // ---------- Scene access ----------

        public static bool IsSceneOpen(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                return false;
            }

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            return scene.IsValid() && scene.isLoaded;
        }

        // Runs action on the scene at scenePath.
        // - Already loaded in the editor: runs in place. When save is true the scene is marked dirty but is not
        //   saved or closed, so the user keeps control (and Undo) over their open scene.
        // - Otherwise: opens it additively, runs action, saves when save is true, and always closes it again.
        // Returns false when the scene could not be opened.
        public static bool WithScene(string scenePath, bool save, Action<Scene> action)
        {
            if (string.IsNullOrEmpty(scenePath) || action == null)
            {
                return false;
            }

            Scene existing = SceneManager.GetSceneByPath(scenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                action(existing);

                if (save)
                {
                    EditorSceneManager.MarkSceneDirty(existing);
                }

                return true;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[UiFramework] Cannot open scene '" + scenePath + "' while in Play Mode.");
                return false;
            }

            // A scene can be listed in the Hierarchy but unloaded; keep it listed after we are done.
            bool wasInHierarchy = existing.IsValid();

            Scene scene;
            try
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
            catch (Exception exception)
            {
                Debug.LogError("[UiFramework] Failed to open scene '" + scenePath + "': " + exception.Message);
                return false;
            }

            try
            {
                action(scene);

                if (save)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, !wasInHierarchy);
            }

            return true;
        }

        public static List<string> FindElementScenePaths(UiEditorConfig config)
        {
            List<string> scenes = new List<string>();

            if (config == null || string.IsNullOrEmpty(config.ElementsScenePath) || !AssetDatabase.IsValidFolder(config.ElementsScenePath.TrimEnd('/')))
            {
                return scenes;
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { config.ElementsScenePath.TrimEnd('/') });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase) && !scenes.Contains(path))
                {
                    scenes.Add(path);
                }
            }

            scenes.Sort(StringComparer.Ordinal);
            return scenes;
        }

        // Reads animation presets, structure and canvas status in a single scene open.
        public static UiElementSceneInfo InspectScene(string scenePath, UiGlobalCanvasSettings settings)
        {
            UiElementSceneInfo info = new UiElementSceneInfo(scenePath);

            bool opened = WithScene(scenePath, false, scene =>
            {
                string showPreset;
                string hidePreset;
                info.HasUiElement = TryReadAnimationPresets(scene, out showPreset, out hidePreset);
                info.ShowPreset = showPreset;
                info.HidePreset = hidePreset;

                UiElementStructureReport report;
                ValidateStructure(scene, out report);
                info.Structure = report;

                string diff;
                List<UiCanvasSettingDifference> differences;
                info.CanvasStatus = GetCanvasStatus(report, settings, scenePath, out diff, out differences);
                info.CanvasDiff = diff;
                info.CanvasDifferences = differences.AsReadOnly();
            });

            info.IsAvailable = opened;
            return info;
        }

        // ---------- Animation presets ----------

        public static bool TryReadAnimationPresets(Scene scene, out string showPreset, out string hidePreset)
        {
            showPreset = null;
            hidePreset = null;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                UiElement[] elements = roots[i].GetComponentsInChildren<UiElement>(true);
                if (elements == null || elements.Length == 0)
                {
                    continue;
                }

                SerializedObject serializedElement = new SerializedObject(elements[0]);
                SerializedProperty showProp = serializedElement.FindProperty("showAnimationPreset");
                SerializedProperty hideProp = serializedElement.FindProperty("hideAnimationPreset");

                showPreset = showProp != null ? showProp.stringValue : null;
                hidePreset = hideProp != null ? hideProp.stringValue : null;
                return true;
            }

            return false;
        }

        // Writes the presets to every UiElement in the scene. Returns false when the scene has none.
        public static bool WriteAnimationPresets(Scene scene, string showPreset, string hidePreset)
        {
            bool wroteAny = false;
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                UiElement[] elements = roots[i].GetComponentsInChildren<UiElement>(true);
                for (int e = 0; e < elements.Length; e++)
                {
                    SerializedObject serializedElement = new SerializedObject(elements[e]);
                    SerializedProperty showProp = serializedElement.FindProperty("showAnimationPreset");
                    SerializedProperty hideProp = serializedElement.FindProperty("hideAnimationPreset");

                    if (showProp != null)
                    {
                        showProp.stringValue = showPreset;
                    }

                    if (hideProp != null)
                    {
                        hideProp.stringValue = hidePreset;
                    }

                    serializedElement.ApplyModifiedProperties();
                    wroteAny = true;
                }
            }

            return wroteAny;
        }

        // ---------- Canvas settings ----------

        // Writes only renderMode, pixelPerfect and the CanvasScaler scale values. Sorting, cameras, plane distance and
        // shader channels are never touched. Either component may be null.
        public static void ApplyCanvasSettings(Canvas canvas, CanvasScaler scaler, UiGlobalCanvasSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            if (canvas != null)
            {
                Undo.RecordObject(canvas, "Apply Global Canvas Settings");
                canvas.renderMode = settings.RenderMode;
                canvas.pixelPerfect = settings.PixelPerfect;
                RecordPrefabOverrides(canvas);
                EditorUtility.SetDirty(canvas);
            }

            if (scaler != null)
            {
                Undo.RecordObject(scaler, "Apply Global Canvas Settings");
                scaler.uiScaleMode = settings.UiScaleMode;
                scaler.referenceResolution = settings.ReferenceResolution;
                scaler.screenMatchMode = settings.ScreenMatchMode;

                if (settings.ScreenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                {
                    scaler.matchWidthOrHeight = settings.MatchWidthOrHeight;
                }

                scaler.referencePixelsPerUnit = settings.ReferencePixelsPerUnit;
                RecordPrefabOverrides(scaler);
                EditorUtility.SetDirty(scaler);
            }
        }

        public static bool CanvasSettingsMatch(Canvas canvas, CanvasScaler scaler, UiGlobalCanvasSettings settings, out string diff)
        {
            List<UiCanvasSettingDifference> differences = GetCanvasDifferences(canvas, scaler, settings);
            diff = JoinDifferences(differences);
            return differences.Count == 0;
        }

        public static List<UiCanvasSettingDifference> GetCanvasDifferences(Canvas canvas, CanvasScaler scaler, UiGlobalCanvasSettings settings)
        {
            List<UiCanvasSettingDifference> differences = new List<UiCanvasSettingDifference>();

            if (settings == null)
            {
                differences.Add(new UiCanvasSettingDifference("GlobalCanvasSettings", "missing", "assign in the General tab"));
                return differences;
            }

            if (canvas == null)
            {
                differences.Add(new UiCanvasSettingDifference("Canvas", "missing", "use Fix Structure"));
                return differences;
            }

            if (canvas.renderMode != settings.RenderMode)
            {
                differences.Add(new UiCanvasSettingDifference("RenderMode", canvas.renderMode.ToString(), settings.RenderMode.ToString()));
            }

            if (canvas.pixelPerfect != settings.PixelPerfect)
            {
                differences.Add(new UiCanvasSettingDifference("PixelPerfect", canvas.pixelPerfect.ToString(), settings.PixelPerfect.ToString()));
            }

            if (scaler == null)
            {
                differences.Add(new UiCanvasSettingDifference("CanvasScaler", "missing", "use Fix Structure"));
                return differences;
            }

            if (scaler.uiScaleMode != settings.UiScaleMode)
            {
                differences.Add(new UiCanvasSettingDifference("UiScaleMode", scaler.uiScaleMode.ToString(), settings.UiScaleMode.ToString()));
            }

            if (!Approximately(scaler.referenceResolution, settings.ReferenceResolution, positionTolerance))
            {
                differences.Add(new UiCanvasSettingDifference("ReferenceResolution", FormatResolution(scaler.referenceResolution), FormatResolution(settings.ReferenceResolution)));
            }

            if (scaler.screenMatchMode != settings.ScreenMatchMode)
            {
                differences.Add(new UiCanvasSettingDifference("ScreenMatchMode", scaler.screenMatchMode.ToString(), settings.ScreenMatchMode.ToString()));
            }

            if (settings.ScreenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight &&
                Mathf.Abs(scaler.matchWidthOrHeight - settings.MatchWidthOrHeight) > unitTolerance)
            {
                differences.Add(new UiCanvasSettingDifference("MatchWidthOrHeight", FormatNumber(scaler.matchWidthOrHeight), FormatNumber(settings.MatchWidthOrHeight)));
            }

            if (Mathf.Abs(scaler.referencePixelsPerUnit - settings.ReferencePixelsPerUnit) > unitTolerance)
            {
                differences.Add(new UiCanvasSettingDifference("ReferencePixelsPerUnit", FormatNumber(scaler.referencePixelsPerUnit), FormatNumber(settings.ReferencePixelsPerUnit)));
            }

            return differences;
        }

        private static string JoinDifferences(List<UiCanvasSettingDifference> differences)
        {
            List<string> parts = new List<string>(differences.Count);

            for (int i = 0; i < differences.Count; i++)
            {
                parts.Add(differences[i].ToString());
            }

            return string.Join("; ", parts.ToArray());
        }

        // Uses the Canvas found by ValidateStructure, so the scene must still be open.
        public static UiCanvasStatus GetCanvasStatus(UiElementStructureReport report, UiGlobalCanvasSettings settings, string scenePath, out string diff)
        {
            List<UiCanvasSettingDifference> differences;
            return GetCanvasStatus(report, settings, scenePath, out diff, out differences);
        }

        public static UiCanvasStatus GetCanvasStatus(UiElementStructureReport report, UiGlobalCanvasSettings settings, string scenePath, out string diff, out List<UiCanvasSettingDifference> differences)
        {
            diff = string.Empty;
            differences = new List<UiCanvasSettingDifference>();

            if (report == null || report.Canvas == null)
            {
                return UiCanvasStatus.NoCanvas;
            }

            differences = GetCanvasDifferences(report.Canvas, report.Canvas.GetComponent<CanvasScaler>(), settings);
            diff = JoinDifferences(differences);
            bool matches = differences.Count == 0;

            if (settings != null && settings.IsExcluded(scenePath))
            {
                return UiCanvasStatus.Excluded;
            }

            return matches ? UiCanvasStatus.Matches : UiCanvasStatus.Differs;
        }

        // ---------- Structure validation ----------

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

            if (!Approximately(rectTransform.anchorMin, Vector2.zero, unitTolerance) || !Approximately(rectTransform.anchorMax, Vector2.one, unitTolerance))
            {
                diffs.Add("anchors " + FormatVector(rectTransform.anchorMin) + "-" + FormatVector(rectTransform.anchorMax));
            }

            if (!Approximately(rectTransform.offsetMin, Vector2.zero, positionTolerance) || !Approximately(rectTransform.offsetMax, Vector2.zero, positionTolerance))
            {
                diffs.Add("offsets min " + FormatVector(rectTransform.offsetMin) + " max " + FormatVector(rectTransform.offsetMax));
            }

            if (!Approximately(rectTransform.pivot, new Vector2(0.5f, 0.5f), unitTolerance))
            {
                diffs.Add("pivot " + FormatVector(rectTransform.pivot));
            }

            if (!Approximately(rectTransform.anchoredPosition, Vector2.zero, positionTolerance))
            {
                diffs.Add("anchoredPosition " + FormatVector(rectTransform.anchoredPosition));
            }

            Vector3 scale = rectTransform.localScale;
            if (Mathf.Abs(scale.x - 1.0f) > unitTolerance || Mathf.Abs(scale.y - 1.0f) > unitTolerance || Mathf.Abs(scale.z - 1.0f) > unitTolerance)
            {
                diffs.Add("scale " + FormatVector(scale));
            }

            if (Quaternion.Angle(rectTransform.localRotation, Quaternion.identity) > rotationToleranceDegrees)
            {
                diffs.Add("rotation " + FormatVector(rectTransform.localEulerAngles));
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

        private static void RecordPrefabOverrides(UnityEngine.Object target)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
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

            for (int i = 0; i < count && i < maxListedNames; i++)
            {
                names.Add(parent.GetChild(i).name);
            }

            string joined = string.Join(", ", names.ToArray());
            if (count > maxListedNames)
            {
                joined += ", +" + (count - maxListedNames) + " more";
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

        private static bool Approximately(Vector2 a, Vector2 b, float tolerance)
        {
            return Mathf.Abs(a.x - b.x) <= tolerance && Mathf.Abs(a.y - b.y) <= tolerance;
        }

        private static string FormatNumber(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string FormatResolution(Vector2 value)
        {
            return FormatNumber(value.x) + "x" + FormatNumber(value.y);
        }

        private static string FormatVector(Vector2 value)
        {
            return "(" + FormatNumber(value.x) + ", " + FormatNumber(value.y) + ")";
        }

        private static string FormatVector(Vector3 value)
        {
            return "(" + FormatNumber(value.x) + ", " + FormatNumber(value.y) + ", " + FormatNumber(value.z) + ")";
        }
    }
}
