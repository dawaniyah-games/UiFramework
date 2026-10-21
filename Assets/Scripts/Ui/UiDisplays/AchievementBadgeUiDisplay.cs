using UiShowcase.App;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Displays
{
    public class AchievementBadgeUiDisplay : MonoBehaviour
    {
        [SerializeField] private Image badge;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject unlockedGlow;
        [SerializeField] private Text titleLabel;

        public void Bind(ShowcaseAchievement achievement, ShowcaseIconLibrary icons)
        {
            if (icons != null)
            {
                icon.sprite = icons.Get(achievement.Icon);
            }

            bool unlocked = achievement.IsUnlocked;
            badge.color = unlocked ? ShowcasePalette.WithAlpha(ShowcasePalette.Gold, 0.22f) : ShowcasePalette.SurfaceRaised;
            icon.color = unlocked ? ShowcasePalette.Gold : ShowcasePalette.WithAlpha(ShowcasePalette.TextMuted, 0.35f);
            titleLabel.text = achievement.Title;
            titleLabel.color = unlocked ? ShowcasePalette.Text : ShowcasePalette.TextMuted;
            lockIcon.SetActive(!unlocked);
            unlockedGlow.SetActive(unlocked);
        }
    }
}
