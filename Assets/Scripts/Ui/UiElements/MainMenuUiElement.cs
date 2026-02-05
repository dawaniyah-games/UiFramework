namespace UiFramework.Editor.Elements
{
    using UiFramework.Core;
    using UiFramework.Editor.States;
    using UiFramework.Runtime.Manager;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    // Auto-generated: UI Element
    public class MainMenuUiElement : UiElement
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;
        // If you need setup logic, uncomment and use:
        public override void Populate(object context = null)
        {
            base.Populate(context);
            SetupButtons();
        }
        private void SetupButtons()
        {
            // Clear previous listeners
            playButton.onClick.RemoveAllListeners();
            settingsButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();

            // Assign actions
            playButton.onClick.AddListener(OnPlayClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            exitButton.onClick.AddListener(OnExitClicked);
        }

        private async void OnPlayClicked()
        {
            Debug.Log("Play clicked! Loading LevelScene...");
            await UiManager.ShowState<UserProfileUiState>(null, additive: true);
            // SceneManager.LoadScene("LevelState"); 
        }

        private async void OnSettingsClicked()
        {
            Debug.Log("Settings clicked! Showing Settings State...");
            await UiManager.ShowState<SettingsUiState>(additive: true);
        }

        private void OnExitClicked()
        {
            Debug.Log("Exit clicked! Closing game...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}


