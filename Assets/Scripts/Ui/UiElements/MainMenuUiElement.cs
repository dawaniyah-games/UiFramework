using System;
using System.Collections.Generic;
using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UiShowcase.Displays;
using UiShowcase.States;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    // Home tab: season hero card, daily reward and daily quests.
    public class MainMenuUiElement : ShowcaseUiElement
    {
        [SerializeField] private ShowcaseIconLibrary icons;
        [SerializeField] private Text greetingLabel;
        [SerializeField] private Button playButton;
        [SerializeField] private Button dailyRewardButton;
        [SerializeField] private Text dailyRewardSubtitle;
        [SerializeField] private Image dailyRewardActionBackground;
        [SerializeField] private Text dailyRewardActionLabel;
        [SerializeField] private GameObject dailyRewardBadge;
        [SerializeField] private RectTransform questContainer;
        [SerializeField] private QuestRowUiDisplay questTemplate;
        [SerializeField] private Text questSummaryLabel;
        [SerializeField] private float questRowSpacing = 156f;
        [SerializeField] private int dailyRewardCoins = 150;

        private static readonly Color[] questAccents =
        {
            ShowcasePalette.Gold, ShowcasePalette.Cyan, ShowcasePalette.Pink
        };

        private readonly List<QuestRowUiDisplay> questRows = new List<QuestRowUiDisplay>();
        private bool initialized;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                initialized = true;
                ShowcaseIconLibrary.WarnIfMissing(icons, this);
                questTemplate.gameObject.SetActive(false);
                playButton.onClick.AddListener(OnPlayClicked);
                dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);
                ShowcaseSession.Current.DataReset += RefreshDailyReward;
            }

            ShowcaseSession session = ShowcaseSession.Current;
            greetingLabel.text = GetGreeting() + ", " + session.PlayerName.Split(' ')[0];
            RefreshDailyReward();
            RebuildQuests();
        }

        private void OnDestroy()
        {
            if (initialized)
            {
                ShowcaseSession.Current.DataReset -= RefreshDailyReward;
            }
        }

        // Populate can run again on the same instance, so the list is rebuilt rather than appended to.
        private void RebuildQuests()
        {
            ShowcaseDisplays.DestroyAll(questRows);

            IReadOnlyList<ShowcaseQuest> quests = ShowcaseCatalog.Quests;
            int completed = 0;

            for (int i = 0; i < quests.Count; i++)
            {
                QuestRowUiDisplay row = ShowcaseDisplays.Spawn(questTemplate, questContainer, new Vector2(0f, -i * questRowSpacing));
                row.Bind(quests[i], icons, questAccents[i % questAccents.Length]);
                questRows.Add(row);

                if (quests[i].IsComplete)
                {
                    completed++;
                }
            }

            questSummaryLabel.text = completed + " / " + quests.Count + " done";
        }

        private void RefreshDailyReward()
        {
            bool claimed = ShowcaseSession.Current.DailyRewardClaimed;
            dailyRewardSubtitle.text = claimed ? "Come back tomorrow for more" : "Tap to collect " + dailyRewardCoins + " coins";
            dailyRewardActionLabel.text = claimed ? "CLAIMED" : "CLAIM";
            dailyRewardActionBackground.color = claimed ? ShowcasePalette.SurfaceRaised : ShowcasePalette.Green;
            dailyRewardBadge.SetActive(!claimed);
        }

        private void OnPlayClicked()
        {
            RewardPopupUiParams reward = new RewardPopupUiParams();
            reward.Title = "VICTORY!";
            reward.Subtitle = "Neon Nights · Level 7 cleared";
            reward.Coins = 250;
            reward.Gems = 3;
            reward.Icon = ShowcaseIcon.Trophy;
            ShowcaseTween.Forget(ShowcaseNavigator.OpenPopupAsync<RewardUiState>(reward));
        }

        private void OnDailyRewardClicked()
        {
            ShowcaseSession session = ShowcaseSession.Current;
            if (session.DailyRewardClaimed)
            {
                // A "no" shake, using the framework's preset directly as feedback.
                ShowcaseTween.Forget(TweenPresets.ShakeLocal(dailyRewardButton.transform));
                return;
            }

            session.ClaimDailyReward();
            RefreshDailyReward();

            RewardPopupUiParams reward = new RewardPopupUiParams();
            reward.Title = "DAILY REWARD";
            reward.Subtitle = "Day 4 streak · keep it going!";
            reward.Coins = dailyRewardCoins;
            reward.Gems = 0;
            reward.Icon = ShowcaseIcon.Gift;
            ShowcaseTween.Forget(ShowcaseNavigator.OpenPopupAsync<RewardUiState>(reward));
        }

        private static string GetGreeting()
        {
            int hour = DateTime.Now.Hour;
            if (hour < 12)
            {
                return "Good morning";
            }

            return hour < 18 ? "Good afternoon" : "Good evening";
        }
    }
}
