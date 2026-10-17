using System;
using System.IO;
using UiFramework.Editor.CodeGeneration;
using UiFramework.Editor.Config;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UiFramework.Editor.Scenes
{
    // Creates a UI element: the UiElement script (from the templates) and its scene laid out per the contract.
    // The UI decides how to ask before restructuring an existing scene; this class only calls the given callback.
    public static class UiElementSceneCreator
    {
        private const string autoBindNameKey = "UiElementAutoBind_Name";
        private const string autoBindScriptPathKey = "UiElementAutoBind_ScriptPath";

        public sealed class Options
        {
            public string ElementName { get; set; }

            // Optional: turn this scene into the element instead of starting from an empty one.
            public SceneAsset ExistingScene { get; set; }
            public bool IncludeParams { get; set; }
            public bool IncludeReference { get; set; }

            // Asked when an existing scene doesn't follow the contract; return true to fix it.
            public Func<UiElementStructureReport, bool> ConfirmStructureFix { get; set; }
        }

        // Returns the saved scene path, or null when the user cancelled saving their open scenes.
        public static string Create(UiEditorConfig config, Options options)
        {
            string scriptPath = config != null ? config.ElementsScriptPath : string.Empty;
            string scenesPath = config != null ? config.ElementsScenePath : string.Empty;
            string elementNamespace = config != null ? config.ElementNamespace : string.Empty;
            string elementName = options.ElementName;

            // Opening/creating the element scene replaces the open scenes, so give the user a chance to save first.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return null;
            }

            UiElementGenerator.Generate(elementName, scriptPath, elementNamespace, options.IncludeParams, options.IncludeReference);
            AssetDatabase.Refresh();

            // UiElementScriptBinder attaches the script to <Name>Root once it has compiled.
            EditorPrefs.SetString(autoBindNameKey, elementName);
            EditorPrefs.SetString(autoBindScriptPathKey, scriptPath);

            Scene scene = options.ExistingScene != null
                ? EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(options.ExistingScene), OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);

            GameObject elementRoot = CreateElementRoot(scene, elementName);
            AttachScriptIfCompiled(scene, elementRoot, elementName, scriptPath);

            UiGlobalCanvasSettings canvasSettings = UiEditorConfigUtility.GetCanvasSettingsOrDefaults(config);
            if (options.ExistingScene == null)
            {
                UiElementStructureFixer.BuildContractHierarchy(elementRoot, canvasSettings);
            }
            else
            {
                OfferStructureFix(scene, canvasSettings, options.ConfirmStructureFix);
            }

            string scenePath = (scenesPath.TrimEnd('/', '\\') + "/" + elementName + ".unity").Replace("\\", "/");
            Directory.CreateDirectory(scenesPath);
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            Debug.Log("[UiFramework] Created UI element scene " + scenePath + ". The script is attached to '" + elementRoot.name + "' after compilation.");
            return scenePath;
        }

        // Puts every existing root object under a new <Name>Root.
        private static GameObject CreateElementRoot(Scene scene, string elementName)
        {
            GameObject elementRoot = new GameObject(elementName + "Root");
            GameObject[] roots = scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != elementRoot)
                {
                    roots[i].transform.SetParent(elementRoot.transform);
                }
            }

            return elementRoot;
        }

        // A freshly generated script only becomes a type after compiling; then UiElementScriptBinder attaches it.
        // If it is already compiled (e.g. re-creating an element), attach it right away.
        private static void AttachScriptIfCompiled(Scene scene, GameObject elementRoot, string elementName, string scriptPath)
        {
            string[] guids = AssetDatabase.FindAssets(elementName + " t:MonoScript", new string[] { scriptPath });

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (Path.GetFileNameWithoutExtension(assetPath) != elementName)
                {
                    continue;
                }

                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath);
                Type type = monoScript != null ? monoScript.GetClass() : null;

                if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type) && elementRoot.GetComponent(type) == null)
                {
                    elementRoot.AddComponent(type);
                    EditorSceneManager.MarkSceneDirty(scene);
                    Debug.Log("[UiFramework] Attached script '" + type.Name + "' to '" + elementRoot.name + "'.");
                }
            }
        }

        // Never restructures silently: only fixes when the callback confirms.
        private static void OfferStructureFix(Scene scene, UiGlobalCanvasSettings canvasSettings, Func<UiElementStructureReport, bool> confirm)
        {
            UiElementStructureReport report;
            UiElementStructureValidator.ValidateStructure(scene, out report);

            if (!report.HasFixableIssues || confirm == null || !confirm(report))
            {
                return;
            }

            UiElementStructureFixer.FixStructure(scene, canvasSettings);
        }
    }
}
