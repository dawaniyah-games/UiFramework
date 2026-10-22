using System.Collections.Generic;
using UiShowcase.App;
using UiShowcase.Components;
using UiShowcase.Displays;
using UiShowcase.States;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    // Profile tab: avatar with a spinning ring, XP bar, stats that count up and an achievements grid that pops in.
    public class ProfileUiElement : ShowcaseUiElement
    {
        [SerializeField] private ShowcaseIconLibrary icons;
        [SerializeField] private Text initialsLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text levelLabel;
        [SerializeField] private FillBar experienceBar;
        [SerializeField] private Text experienceLabel;
        [SerializeField] private CountUpText winsCounter;
        [SerializeField] private CountUpText streakCounter;
        [SerializeField] private CountUpText starsCounter;
        [SerializeField] private RectTransform badgeGrid;
        [SerializeField] private AchievementBadgeUiDisplay badgeTemplate;
        [SerializeField] private Text achievementsSummary;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Vector2 badgeCellSize = new Vector2(300f, 196f);
        [SerializeField] private Vector2 badgeSpacing = new Vector2(30f, 16f);

        private readonly List<AchievementBadgeUiDisplay> badges = new List<AchievementBadgeUiDisplay>();
        private bool initialized;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                initialized = true;
                ShowcaseIconLibrary.WarnIfMissing(icons, this);
                badgeTemplate.gameObject.SetActive(false);
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            ShowcaseSession session = ShowcaseSession.Current;
            initialsLabel.text = ShowcaseCatalog.GetInitials(session.PlayerName);
            nameLabel.text = session.PlayerName;
            titleLabel.text = session.PlayerTitle;
            levelLabel.text = "Level " + session.Level;
            experienceLabel.text = session.Experience.ToString("N0") + " / " + session.ExperienceToNextLevel.ToString("N0") + " XP";

            // Set the targets only: FillBar and CountUpText animate from zero when the element shows.
            experienceBar.SetValue((float)session.Experience / session.ExperienceToNextLevel, false);
            winsCounter.SetValue(session.Wins, false);
            streakCounter.SetValue(session.BestStreak, false);
            starsCounter.SetValue(session.Stars, false);

            RebuildBadges();
        }

        private void RebuildBadges()
        {
            ShowcaseDisplays.DestroyAll(badges);

            IReadOnlyList<ShowcaseAchievement> achievements = ShowcaseCatalog.Achievements;
            int unlocked = 0;

            for (int i = 0; i < achievements.Count; i++)
            {
                Vector2 position = ShowcaseDisplays.GridPosition(i, 3, badgeCellSize, badgeSpacing);
                AchievementBadgeUiDisplay badge = ShowcaseDisplays.Spawn(badgeTemplate, badgeGrid, position);
                badge.Bind(achievements[i], icons);
                badges.Add(badge);

                if (achievements[i].IsUnlocked)
                {
                    unlocked++;
                }
            }

            achievementsSummary.text = unlocked + " / " + achievements.Count;
        }

        private void OnSettingsClicked()
        {
            ShowcaseTween.Forget(ShowcaseNavigator.OpenPopupAsync<SettingsUiState>(null));
        }
    }
}
