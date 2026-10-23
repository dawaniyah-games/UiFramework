using System;
using System.Linq;
using UiFramework.Editor.Window;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UiFramework.Editor.CodeGeneration
{
    // A freshly generated element script only becomes a type after compiling; this attaches it to <Name>Root then.
    public class UiElementScriptBinder : AssetPostprocessor
    {
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            string name = EditorPrefs.GetString("UiElementAutoBind_Name", null);
            string scriptPath = EditorPrefs.GetString("UiElementAutoBind_ScriptPath", null);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(scriptPath))
            {
                return;
            }

            EditorPrefs.DeleteKey("UiElementAutoBind_Name");
            EditorPrefs.DeleteKey("UiElementAutoBind_ScriptPath");

            string scriptGuid = AssetDatabase.FindAssets($"{name} t:MonoScript", new string[] { scriptPath }).FirstOrDefault();

            if (string.IsNullOrEmpty(scriptGuid))
            {
                return;
            }

            string scriptAssetPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
            if (monoScript == null)
            {
                return;
            }

            Type type = monoScript.GetClass();
            if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return;
            }

            // Attach after the reload has fully finished: saving the scene from inside the reload callback makes
            // Unity's import pass see the file change and ask to reload the "externally modified" scene.
            EditorApplication.delayCall += () => AttachAndSave(name, type);
        }

        private static void AttachAndSave(string name, Type type)
        {
            GameObject elementRoot = GameObject.Find(name + "Root");
            if (elementRoot == null || elementRoot.GetComponent(type) != null)
            {
                return;
            }

            Undo.AddComponent(elementRoot, type);

            // The scene was created and saved by the element creator; save it again so the new element isn't left with unsaved changes.
            UnityEngine.SceneManagement.Scene scene = elementRoot.scene;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!string.IsNullOrEmpty(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log($"[UiFramework] Attached {type.Name} to {elementRoot.name} after script reload and saved the scene.");
            UiSetupEditorWindow.RefreshElementScene(scene.path);
        }
    }
}
