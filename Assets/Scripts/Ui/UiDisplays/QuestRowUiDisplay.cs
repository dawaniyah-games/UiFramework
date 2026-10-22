using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Displays
{
    public class QuestRowUiDisplay : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image iconBackground;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text progressLabel;
        [SerializeField] private FillBar progressBar;
        [SerializeField] private Image progressFill;
        [SerializeField] private Text rewardLabel;
        [SerializeField] private GameObject doneMark;

        public void Bind(ShowcaseQuest quest, ShowcaseIconLibrary icons, Color accent)
        {
            if (icons != null)
            {
                icon.sprite = icons.Get(quest.Icon);
            }

            icon.color = accent;
            iconBackground.color = ShowcasePalette.WithAlpha(accent, 0.18f);
            progressFill.color = accent;
            titleLabel.text = quest.Title;
            progressLabel.text = quest.Progress.ToString("N0") + " / " + quest.Goal.ToString("N0");
            rewardLabel.text = "+" + quest.RewardCoins;
            progressBar.SetValue(quest.Goal > 0 ? (float)quest.Progress / quest.Goal : 0f, false);

            if (doneMark != null)
            {
                doneMark.SetActive(quest.IsComplete);
            }
        }
    }
}
