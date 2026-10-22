using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    // Additive popup over any tab. Its show / hide preset (Fade) fades the dimmed backdrop; the panel itself pops in
    // with a RevealOnShow, so the backdrop never slides or scales with it.
    public class SettingsPopupUiElement : ShowcaseUiElement
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private ToggleSwitch musicToggle;
        [SerializeField] private ToggleSwitch soundToggle;
        [SerializeField] private ToggleSwitch vibrationToggle;
        [SerializeField] private ToggleSwitch notificationsToggle;
        [SerializeField] private Button replayIntroButton;
        [SerializeField] private Button resetButton;

        private bool initialized;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                Initialize();
            }

            RefreshToggles();
        }

        private void Initialize()
        {
            initialized = true;

            backdropButton.onClick.AddListener(Close);
            closeButton.onClick.AddListener(Close);
            replayIntroButton.onClick.AddListener(OnReplayIntroClicked);
            resetButton.onClick.AddListener(OnResetClicked);

            musicToggle.Changed += value =>
            {
                ShowcaseSession.Current.SetSetting(ShowcaseSetting.Music, value);
            };

            soundToggle.Changed += value =>
            {
                ShowcaseSession.Current.SetSetting(ShowcaseSetting.Sound, value);
            };

            notificationsToggle.Changed += value =>
            {
                ShowcaseSession.Current.SetSetting(ShowcaseSetting.Notifications, value);
            };

            vibrationToggle.Changed += value =>
            {
                ShowcaseSession.Current.SetSetting(ShowcaseSetting.Vibration, value);

                // No haptics in the editor, so turning vibration on "buzzes" the panel with the Shake preset instead.
                if (value)
                {
                    ShowcaseTween.Forget(TweenPresets.ShakeLocal(panel, 14f, 0.4f));
                }
            };
        }

        private void RefreshToggles()
        {
            ShowcaseSession session = ShowcaseSession.Current;
            musicToggle.SetIsOn(session.GetSetting(ShowcaseSetting.Music), false);
            soundToggle.SetIsOn(session.GetSetting(ShowcaseSetting.Sound), false);
            vibrationToggle.SetIsOn(session.GetSetting(ShowcaseSetting.Vibration), false);
            notificationsToggle.SetIsOn(session.GetSetting(ShowcaseSetting.Notifications), false);
        }

        private void Close()
        {
            ShowcaseTween.Forget(ShowcaseNavigator.ClosePopupAsync());
        }

        private void OnReplayIntroClicked()
        {
            ShowcaseTween.Forget(ShowcaseNavigator.RestartAsync());
        }

        private void OnResetClicked()
        {
            ShowcaseSession.Current.ResetData();
            RefreshToggles();
            Close();
        }
    }
}
