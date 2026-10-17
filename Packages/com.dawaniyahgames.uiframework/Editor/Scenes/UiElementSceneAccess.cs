using System;
using System.Collections.Generic;
using UiFramework.Core;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiFramework.Editor.Scenes
{
    // Opens UI element scenes safely and reads what the editor shows about them (presets, structure, canvas).
    // Scenes already open in the editor are used in place; others are opened additively and closed again.
    public static class UiElementSceneAccess
    {
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
                UiElementStructureValidator.ValidateStructure(scene, out report);
                info.Structure = report;

                if (report.Canvas != null)
                {
                    info.Canvas = UiCanvasSnapshot.Capture(report.Canvas);
                    info.CanvasDifferences = UiElementCanvasUtility.GetCanvasDifferences(report.Canvas, report.Canvas.GetComponent<CanvasScaler>(), settings).AsReadOnly();
                }
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
    }
}
