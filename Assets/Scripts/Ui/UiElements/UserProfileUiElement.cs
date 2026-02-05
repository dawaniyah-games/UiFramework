namespace UiFramework.Editor.Elements
{
    using UiFramework.Core;
    using UiFramework.Runtime.Manager;
    using UnityEngine;
    using UnityEngine.UI;

    // Auto-generated: UI Element
    public class UserProfileUiElement : UiElement
    {
        [SerializeField] private TMPro.TMP_Text userNameText;
        [SerializeField] private Button goToMenuButton;
        // If you need setup logic, uncomment and use:
        public override void Populate(object context = null)
        {
            base.Populate(context);
            if (context is SettingsContext settingsContext)
            {
                userNameText.text = "Welcome " + settingsContext.playerName + "! :)";
            }
            goToMenuButton.onClick.AddListener(OnGoToMenuClicked);
        }

        private async void OnGoToMenuClicked()
        {
            await UiManager.HideUI();
        }
    }
}
