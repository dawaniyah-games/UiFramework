namespace UiFramework.Editor.Elements
{
    using TMPro;
    using UiFramework.Core;
    using UiFramework.Editor.States;
    using UiFramework.Runtime.Manager;
    using UnityEngine;
    using UnityEngine.UI;

    // Auto-generated: UI Element
    public class SettingsUiElement : UiElement
    {
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private Button saveButton;
        // If you need setup logic, uncomment and use:
        public override void Populate(object context = null)
        {
            base.Populate(context);
            if (context is SettingsContext settingsContext)
            {
                playerNameInput.text = settingsContext.playerName;
            }

            HandleButtons();
        }

        private void HandleButtons()
        {
            backButton.onClick.AddListener(OnBackClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        private async void OnBackClicked()
        {
            Debug.Log("Back button clicked! Hiding Settings State...");
            await UiManager.HideUI();
        }

        private async void OnSaveClicked()
        {
            Debug.Log("Save button clicked! Saving settings...");
            SettingsContext context = new SettingsContext
            {
                playerName = playerNameInput.text,
                volumeLevel = 0.5f // Example volume level
            };
            await UiManager.HideUI();
            await UiManager.ShowState<UserProfileUiState>(context, additive: true);
        }
    }
}
public class SettingsContext
{
    public string playerName;
    public float volumeLevel; //for test

}