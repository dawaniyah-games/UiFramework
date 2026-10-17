using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    // A lightweight copy of a scene hierarchy, used to show "Now" and "After" in the fix dialog.
    public sealed class UiElementPreviewNode
    {
        private readonly List<UiElementPreviewNode> children = new List<UiElementPreviewNode>();

        public string Name { get; set; }
        public bool IsNew { get; set; }
        public bool IsCanvas { get; set; }
        public bool FillsParent { get; set; }

        // A prefab instance, a non-UI object or an object with its own Canvas: FixStructure wraps it instead of using it as Root.
        public bool CannotBeContentRoot { get; set; }
        public string Note { get; set; }

        public List<UiElementPreviewNode> Children
        {
            get
            {
                return children;
            }
        }

        public UiElementPreviewNode Clone()
        {
            UiElementPreviewNode copy = new UiElementPreviewNode
            {
                Name = Name,
                IsNew = IsNew,
                IsCanvas = IsCanvas,
                FillsParent = FillsParent,
                CannotBeContentRoot = CannotBeContentRoot,
                Note = Note
            };

            for (int i = 0; i < children.Count; i++)
            {
                copy.children.Add(children[i].Clone());
            }

            return copy;
        }
    }

    // Builds the "Now" hierarchy from an open scene and simulates what FixStructure will do to it.
    public sealed class UiElementFixPreview
    {
        private const int maxDepth = 3;
        private const int maxChildrenShown = 6;

        public UiElementPreviewNode Before { get; private set; }
        public UiElementPreviewNode After { get; private set; }

        // The report's scene must still be open.
        public static UiElementFixPreview Build(UiElementStructureReport report)
        {
            if (report == null || report.ElementRoot == null)
            {
                return null;
            }

            UiElementFixPreview preview = new UiElementFixPreview();
            preview.Before = Capture(report.ElementRoot.transform, 0);
            preview.After = Simulate(preview.Before, report);
            return preview;
        }

        private static UiElementPreviewNode Capture(Transform transform, int depth)
        {
            RectTransform rectTransform = transform as RectTransform;
            string unused;

            UiElementPreviewNode node = new UiElementPreviewNode
            {
                Name = transform.name,
                IsCanvas = transform.GetComponent<Canvas>() != null,
                FillsParent = rectTransform != null && UiElementStructureValidator.IsStretchedToParent(rectTransform, out unused),
                CannotBeContentRoot = rectTransform == null || transform.GetComponent<Canvas>() != null || PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)
            };

            if (depth >= maxDepth)
            {
                return node;
            }

            int childCount = transform.childCount;
            for (int i = 0; i < childCount && i < maxChildrenShown; i++)
            {
                node.Children.Add(Capture(transform.GetChild(i), depth + 1));
            }

            if (childCount > maxChildrenShown)
            {
                node.Children.Add(new UiElementPreviewNode { Name = "… " + (childCount - maxChildrenShown) + " more", FillsParent = true });
            }

            return node;
        }

        private static UiElementPreviewNode Simulate(UiElementPreviewNode before, UiElementStructureReport report)
        {
            UiElementPreviewNode root = before.Clone();
            bool canvasCreated = false;

            if (report.HasFixable(UiElementStructureIssueCode.NoCanvas))
            {
                int misplacedIndex = report.Canvas != null ? FindCanvasChild(root, report.Canvas.name) : -1;

                if (misplacedIndex > 0)
                {
                    UiElementPreviewNode moved = root.Children[misplacedIndex];
                    root.Children.RemoveAt(misplacedIndex);
                    root.Children.Insert(0, moved);
                    moved.Note = "moved to the top";
                }
                else
                {
                    UiElementPreviewNode canvas = new UiElementPreviewNode { Name = UiElementStructureFixer.CanvasObjectName, IsNew = true, IsCanvas = true };
                    canvas.Children.AddRange(root.Children);
                    root.Children.Clear();
                    root.Children.Add(canvas);
                    canvasCreated = true;
                }
            }

            if (root.Children.Count == 0 || !root.Children[0].IsCanvas)
            {
                return root;
            }

            UiElementPreviewNode canvasNode = root.Children[0];

            if (report.HasFixable(UiElementStructureIssueCode.CanvasMissingComponents))
            {
                canvasNode.Note = "missing components added";
            }

            // After a new Canvas is created, FixStructure re-validates, so the content rules apply to what was moved in.
            bool needsWrap = report.HasFixable(UiElementStructureIssueCode.NoContentRoot) ||
                (canvasCreated && (canvasNode.Children.Count != 1 || canvasNode.Children[0].CannotBeContentRoot));
            bool contentNotFilling = report.HasFixable(UiElementStructureIssueCode.ContentRootNotStretched) ||
                (canvasCreated && canvasNode.Children.Count == 1 && !canvasNode.Children[0].FillsParent);

            if (needsWrap || contentNotFilling)
            {
                Wrap(canvasNode);
            }

            return root;
        }

        private static void Wrap(UiElementPreviewNode canvasNode)
        {
            List<string> names = new List<string>(canvasNode.Children.Count);
            for (int i = 0; i < canvasNode.Children.Count; i++)
            {
                names.Add(canvasNode.Children[i].Name);
            }

            UiElementPreviewNode wrapper = new UiElementPreviewNode
            {
                Name = UiElementStructureFixer.GetWrapperName(names),
                IsNew = true,
                FillsParent = true,
                Note = "fills the screen"
            };

            wrapper.Children.AddRange(canvasNode.Children);
            canvasNode.Children.Clear();
            canvasNode.Children.Add(wrapper);
        }

        private static int FindCanvasChild(UiElementPreviewNode root, string canvasName)
        {
            for (int i = 0; i < root.Children.Count; i++)
            {
                if (root.Children[i].IsCanvas && root.Children[i].Name == canvasName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
