using UnityEngine;

namespace UiFramework.Editor.Config
{
    [CreateAssetMenu(fileName = "UiEditorConfig", menuName = "Scripts/UiFramework/Editor Config")]
    public class UiEditorConfig : ScriptableObject
    {
        public string UiSetupAssetPath = "Assets/Scripts/UiFramework/Editor/UiSetup.asset";
        public string ElementsScriptPath = "Assets/Scripts/Ui/UiElements";
        public string ElementsScenePath = "Assets/Scenes/UiElements";
        public string StatesPath = "Assets/Scripts/Ui/UiStates";
        public string StateRegistryPath = "Assets/Scripts/UiFramework/Editor/UiStateRegistry.asset";
        public string RuntimeConfigOutputPath = "Assets/Scripts/UiFramework/Runtime/UiConfig.asset";
        public string ElementNamespace = "UiFramework.Editor.Elements";
        public string StateNamespace = "UiFramework.Editor.States";
    }
}