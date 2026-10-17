using System;
using System.Collections.Generic;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Compares, applies and edits an element scene's Canvas / Canvas Scaler values. Only the values listed in
    // UiGlobalCanvasSettings are ever written; sorting, cameras, plane distance and shader channels are left alone.
    public static class UiElementCanvasUtility
    {
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

            if (!UiSceneValues.Approximately(scaler.referenceResolution, settings.ReferenceResolution, UiSceneValues.PositionTolerance))
            {
                differences.Add(new UiCanvasSettingDifference("ReferenceResolution", UiSceneValues.FormatResolution(scaler.referenceResolution), UiSceneValues.FormatResolution(settings.ReferenceResolution)));
            }

            if (scaler.screenMatchMode != settings.ScreenMatchMode)
            {
                differences.Add(new UiCanvasSettingDifference("ScreenMatchMode", scaler.screenMatchMode.ToString(), settings.ScreenMatchMode.ToString()));
            }

            if (settings.ScreenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight &&
                Mathf.Abs(scaler.matchWidthOrHeight - settings.MatchWidthOrHeight) > UiSceneValues.UnitTolerance)
            {
                differences.Add(new UiCanvasSettingDifference("MatchWidthOrHeight", UiSceneValues.FormatNumber(scaler.matchWidthOrHeight), UiSceneValues.FormatNumber(settings.MatchWidthOrHeight)));
            }

            if (Mathf.Abs(scaler.referencePixelsPerUnit - settings.ReferencePixelsPerUnit) > UiSceneValues.UnitTolerance)
            {
                differences.Add(new UiCanvasSettingDifference("ReferencePixelsPerUnit", UiSceneValues.FormatNumber(scaler.referencePixelsPerUnit), UiSceneValues.FormatNumber(settings.ReferencePixelsPerUnit)));
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

        // Changes the element scene's root Canvas / Canvas Scaler (Undo-recorded). Open scenes are edited in place and
        // left unsaved; closed scenes are opened, saved and closed. Returns false when the scene has no root Canvas.
        public static bool EditCanvas(string scenePath, string undoName, Action<Canvas, CanvasScaler> edit)
        {
            bool edited = false;

            UiElementSceneAccess.WithScene(scenePath, true, scene =>
            {
                UiElementStructureReport report;
                UiElementStructureValidator.ValidateStructure(scene, out report);
                if (report.Canvas == null)
                {
                    return;
                }

                CanvasScaler scaler = report.Canvas.GetComponent<CanvasScaler>();
                Undo.RecordObject(report.Canvas, undoName);
                if (scaler != null)
                {
                    Undo.RecordObject(scaler, undoName);
                }

                edit(report.Canvas, scaler);

                RecordPrefabOverrides(report.Canvas);
                EditorUtility.SetDirty(report.Canvas);
                if (scaler != null)
                {
                    RecordPrefabOverrides(scaler);
                    EditorUtility.SetDirty(scaler);
                }

                edited = true;
            });

            return edited;
        }

        private static void RecordPrefabOverrides(UnityEngine.Object target)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            }
        }
    }
}
