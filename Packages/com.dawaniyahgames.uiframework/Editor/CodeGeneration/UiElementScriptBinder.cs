using System;
using System.Linq;
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

            GameObject elementRoot = GameObject.Find(name + "Root");
            if (elementRoot != null && elementRoot.GetComponent(type) == null)
            {
                Undo.AddComponent(elementRoot, type);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log($"[UiFramework] Attached {type.Name} to {elementRoot.name} after script reload.");
            }
        }
    }
}
