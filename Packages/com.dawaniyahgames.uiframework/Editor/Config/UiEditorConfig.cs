using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UiFramework.Editor.Config
{
    [CreateAssetMenu(fileName = "UiEditorConfig", menuName = "Scripts/UiFramework/Editor Config")]
    public class UiEditorConfig : ScriptableObject
    {
        public string UiSetupAssetPath = "Assets/UiConfigs/UiSetup.asset";
        public string ElementsScriptPath = "Assets/Scripts/Ui/UiElements";
        public string ElementsScenePath = "Assets/Scenes/UiElements";
        public string StatesPath = "Assets/Scripts/Ui/UiStates";
        public string StateRegistryPath = "Assets/UiConfigs/UiStateRegistry.asset";
        public string RuntimeConfigOutputPath = "Assets/UiConfigs/RuntimeUiConfig.asset";
        public string ElementNamespace = "UiFramework.Editor.Elements";
        public string StateNamespace = "UiFramework.Editor.States";

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(UiSetupAssetPath) || UiSetupAssetPath == "Assets/UiConfigs/UiSetup.asset")
            {
                UiSetupAssetPath = GetDefaultConfigPath("UiSetup", "Assets/UiConfigs/UiSetup.asset");
            }

            if (string.IsNullOrEmpty(StateRegistryPath) || StateRegistryPath == "Assets/UiConfigs/UiStateRegistry.asset")
            {
                StateRegistryPath = GetDefaultConfigPath("UiStateRegistry", "Assets/UiConfigs/UiStateRegistry.asset");
            }

            if (string.IsNullOrEmpty(RuntimeConfigOutputPath) || RuntimeConfigOutputPath == "Assets/UiConfigs/RuntimeUiConfig.asset")
            {
                RuntimeConfigOutputPath = GetDefaultConfigPath("RuntimeUiConfig", "Assets/UiConfigs/RuntimeUiConfig.asset");
            }
        }

        private string GetDefaultConfigPath(string assetName, string fallback)
        {
            string[] guids = AssetDatabase.FindAssets($"{assetName} t:ScriptableObject");
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }

            return fallback;
        }
#endif
    }
}
