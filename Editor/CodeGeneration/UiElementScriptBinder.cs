using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;

public class UiElementScriptBinder : AssetPostprocessor
{
    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        string name = EditorPrefs.GetString("UiElementAutoBind_Name", null);
        string scriptPath = EditorPrefs.GetString("UiElementAutoBind_ScriptPath", null);

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(scriptPath))
            return;

        EditorPrefs.DeleteKey("UiElementAutoBind_Name");
        EditorPrefs.DeleteKey("UiElementAutoBind_ScriptPath");

        var scriptGuid = AssetDatabase.FindAssets($"{name} t:MonoScript", new[] { scriptPath }).FirstOrDefault();
        if (string.IsNullOrEmpty(scriptGuid)) return;

        var scriptAssetPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
        var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptAssetPath);
        var type = monoScript?.GetClass();

        if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
        {
            var rootGO = GameObject.Find(name + "Root");
            if (rootGO != null && rootGO.GetComponent(type) == null)
            {
                Undo.AddComponent(rootGO, type);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log($"✅ Auto-attached {type.Name} to {rootGO.name} after script reload.");
            }
        }
    }
}
