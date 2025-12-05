using UnityEngine;

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
    }
}