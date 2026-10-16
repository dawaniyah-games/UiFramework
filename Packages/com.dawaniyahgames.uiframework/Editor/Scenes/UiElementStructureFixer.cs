using System.Collections.Generic;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Brings a UI element scene in line with the contract (see UiElementSceneUtility). Every change is
    // Undo-recorded. Objects are only created and reparented, never deleted, so serialized references survive.
    public static class UiElementStructureFixer
    {
        public const string CanvasObjectName = "Canvas";
        public const string ContentRootObjectName = "Root";

        private const string alternateContentRootObjectName = "ContentRoot";
        private const string undoName = "Fix UI Element Structure";
        private const string uiLayerName = "UI";

        public static UiElementStructureReport FixStructure(Scene scene, UiCanvasSettings settings)
        {
            return FixStructure(scene, settings, UiElementStructureFixMode.WrapContent);
        }

        // Applies every fixable issue, then re-validates and returns the report of what remains.
        public static UiElementStructureReport FixStructure(Scene scene, UiCanvasSettings settings, UiElementStructureFixMode mode)
        {
            UiElementStructureReport report;
            UiElementSceneUtility.ValidateStructure(scene, out report);

            if (report.ElementRoot == null || !report.HasFixableIssues)
            {
                return report;
            }

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);

            try
            {
                if (report.HasFixable(UiElementStructureIssueCode.NoCanvas))
                {
                    FixCanvasPlacement(report, settings);

                    // Content checks only run once a Canvas exists, so validate again before continuing.
                    UiElementSceneUtility.ValidateStructure(scene, out report);
                }

                Canvas canvas = report.Canvas;
                if (canvas != null)
                {
                    if (report.HasFixable(UiElementStructureIssueCode.CanvasMissingComponents))
                    {
                        AddMissingCanvasComponents(canvas, settings);
                    }

                    bool wrap = report.HasFixable(UiElementStructureIssueCode.NoContentRoot) ||
                        (mode == UiElementStructureFixMode.WrapContent && report.HasFixable(UiElementStructureIssueCode.ContentRootNotStretched));

                    if (wrap)
                    {
                        WrapCanvasChildren(canvas.transform);
                    }
                    else if (mode == UiElementStructureFixMode.ResetContentRoot &&
                        report.HasFixable(UiElementStructureIssueCode.ContentRootNotStretched) &&
                        report.ContentRoot != null)
                    {
                        StretchToParent(report.ContentRoot, true);
                    }
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroup);
            }

            UiElementStructureReport remaining;
            UiElementSceneUtility.ValidateStructure(scene, out remaining);
            return remaining;
        }

        // Human-readable list of what FixStructure will change, built from a report (works after the scene closed).
        public static List<string> DescribePlannedChanges(UiElementStructureReport report, UiElementStructureFixMode mode)
        {
            List<string> changes = new List<string>();
            if (report == null)
            {
                return changes;
            }

            bool wrapsCanvasChildren = report.HasFixable(UiElementStructureIssueCode.NoContentRoot);

            for (int i = 0; i < report.Issues.Count; i++)
            {
                UiElementStructureIssue issue = report.Issues[i];
                if (!issue.IsFixable)
                {
                    continue;
                }

                if (issue.Code == UiElementStructureIssueCode.ContentRootNotStretched)
                {
                    if (wrapsCanvasChildren)
                    {
                        continue;
                    }

                    if (mode == UiElementStructureFixMode.ResetContentRoot)
                    {
                        changes.Add("Reset '" + issue.ObjectName + "' to stretch the full canvas (anchors 0-1, offsets 0, pivot 0.5, scale 1, rotation 0). Its children keep their RectTransforms, but '" + issue.ObjectName + "' itself changes size/position.");
                        continue;
                    }
                }

                changes.Add(issue.FixDescription);
            }

            return changes;
        }

        public static List<string> DescribeUnfixableIssues(UiElementStructureReport report)
        {
            List<string> lines = new List<string>();
            if (report == null)
            {
                return lines;
            }

            for (int i = 0; i < report.Issues.Count; i++)
            {
                UiElementStructureIssue issue = report.Issues[i];
                if (!issue.IsFixable)
                {
                    lines.Add(issue.Code + ": " + issue.Message + (issue.IsWarning ? " (warning)" : string.Empty));
                }
            }

            return lines;
        }

        public static bool OffersContentRootReset(UiElementStructureReport report)
        {
            return report != null &&
                report.HasFixable(UiElementStructureIssueCode.ContentRootNotStretched) &&
                !report.HasFixable(UiElementStructureIssueCode.NoContentRoot);
        }

        // Builds Canvas -> Root under an element root that has no children yet (new element scenes).
        public static RectTransform BuildContractHierarchy(GameObject elementRoot, UiCanvasSettings settings)
        {
            if (elementRoot == null)
            {
                return null;
            }

            Canvas canvas = CreateCanvas(elementRoot.transform, settings);
            return CreateContentRoot(canvas.transform, ContentRootObjectName);
        }

        private static void FixCanvasPlacement(UiElementStructureReport report, UiCanvasSettings settings)
        {
            Transform elementRoot = report.ElementRoot.transform;

            if (report.Canvas != null)
            {
                Undo.RegisterChildrenOrderUndo(elementRoot, undoName);
                report.Canvas.transform.SetSiblingIndex(0);
                return;
            }

            List<Transform> uiChildren = GetChildren(elementRoot);
            Canvas canvas = CreateCanvas(elementRoot, settings);

            for (int i = 0; i < uiChildren.Count; i++)
            {
                ReparentKeepingLocalLayout(uiChildren[i], canvas.transform);
            }
        }

        private static void AddMissingCanvasComponents(Canvas canvas, UiCanvasSettings settings)
        {
            GameObject canvasObject = canvas.gameObject;

            if (canvasObject.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = Undo.AddComponent<CanvasScaler>(canvasObject);
                UiElementSceneUtility.ApplyCanvasSettings(null, scaler, settings);
            }

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                Undo.AddComponent<GraphicRaycaster>(canvasObject);
            }
        }

        private static void WrapCanvasChildren(Transform canvasTransform)
        {
            List<Transform> children = GetChildren(canvasTransform);
            string rootName = ContentRootObjectName;

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].name == ContentRootObjectName)
                {
                    rootName = alternateContentRootObjectName;
                    break;
                }
            }

            RectTransform contentRoot = CreateContentRoot(canvasTransform, rootName);

            for (int i = 0; i < children.Count; i++)
            {
                ReparentKeepingLocalLayout(children[i], contentRoot);
            }
        }

        private static Canvas CreateCanvas(Transform elementRoot, UiCanvasSettings settings)
        {
            GameObject canvasObject = CreateGameObject(CanvasObjectName, elementRoot, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            UiElementSceneUtility.ApplyCanvasSettings(canvas, canvasObject.GetComponent<CanvasScaler>(), settings);
            return canvas;
        }

        private static RectTransform CreateContentRoot(Transform canvasTransform, string name)
        {
            GameObject rootObject = CreateGameObject(name, canvasTransform, typeof(RectTransform));
            RectTransform rectTransform = rootObject.GetComponent<RectTransform>();
            StretchToParent(rectTransform, false);
            return rectTransform;
        }

        // Creates a GameObject as the first child of parent, in parent's scene, registered for Undo.
        private static GameObject CreateGameObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject created = new GameObject(name, components);

            // new GameObject() lands in the active scene; the target scene may be one opened additively.
            if (created.scene != parent.gameObject.scene)
            {
                SceneManager.MoveGameObjectToScene(created, parent.gameObject.scene);
            }

            int uiLayer = LayerMask.NameToLayer(uiLayerName);
            if (uiLayer >= 0)
            {
                created.layer = uiLayer;
            }

            created.transform.SetParent(parent, false);
            created.transform.SetAsFirstSibling();
            Undo.RegisterCreatedObjectUndo(created, undoName);
            return created;
        }

        private static void StretchToParent(RectTransform rectTransform, bool recordUndo)
        {
            if (recordUndo)
            {
                Undo.RecordObject(rectTransform, undoName);
            }

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;

            if (recordUndo && PrefabUtility.IsPartOfPrefabInstance(rectTransform))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(rectTransform);
            }
        }

        // Equivalent to SetParent(newParent, worldPositionStays: false) with Undo. The new parents created here are
        // stretched with zero offsets, so the child's local RectTransform values keep the same on-screen layout.
        private static void ReparentKeepingLocalLayout(Transform child, Transform newParent)
        {
            RectTransform rectTransform = child as RectTransform;

            Vector3 localPosition = child.localPosition;
            Quaternion localRotation = child.localRotation;
            Vector3 localScale = child.localScale;
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;
            Vector2 pivot = Vector2.zero;
            Vector2 sizeDelta = Vector2.zero;
            Vector3 anchoredPosition3D = Vector3.zero;

            if (rectTransform != null)
            {
                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
                pivot = rectTransform.pivot;
                sizeDelta = rectTransform.sizeDelta;
                anchoredPosition3D = rectTransform.anchoredPosition3D;
            }

            Undo.SetTransformParent(child, newParent, undoName);
            Undo.RecordObject(child, undoName);

            if (rectTransform != null)
            {
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                rectTransform.pivot = pivot;
                rectTransform.sizeDelta = sizeDelta;
                rectTransform.anchoredPosition3D = anchoredPosition3D;
            }
            else
            {
                child.localPosition = localPosition;
            }

            child.localRotation = localRotation;
            child.localScale = localScale;

            if (PrefabUtility.IsPartOfPrefabInstance(child))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(child);
            }
        }

        private static List<Transform> GetChildren(Transform parent)
        {
            List<Transform> children = new List<Transform>(parent.childCount);

            for (int i = 0; i < parent.childCount; i++)
            {
                children.Add(parent.GetChild(i));
            }

            return children;
        }
    }
}
