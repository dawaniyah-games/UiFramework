using System.Threading.Tasks;
using UiFramework.Core;
using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    public class RewardPopupUiParams : UiPopulationParams
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public int Coins { get; set; }
        public int Gems { get; set; }
        public ShowcaseIcon Icon { get; set; }
    }

    // Additive reward popup: spinning rays, the reward icon drops in, amounts count up, confetti bursts. Claim flies the
    // coins and gems into the TopBar counters while the popup closes.
    public class RewardPopupUiElement : ShowcaseUiElement
    {
        [SerializeField] private ShowcaseIconLibrary icons;
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Text titleLabel;
        [SerializeField] private Text subtitleLabel;
        [SerializeField] private RectTransform coinsPill;
        [SerializeField] private CountUpText coinsCounter;
        [SerializeField] private GameObject gemsPill;
        [SerializeField] private CountUpText gemsCounter;
        [SerializeField] private Button claimButton;
        [SerializeField] private ConfettiBurst confetti;
        [SerializeField] private Vector2 confettiOrigin = new Vector2(0f, 120f);
        [SerializeField] private float confettiDelay = 0.35f;
        [SerializeField] private float coinsPillOffsetWithGems = -200f;

        private RewardPopupUiParams reward;
        private bool initialized;
        private bool claimed;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                initialized = true;
                ShowcaseIconLibrary.WarnIfMissing(icons, this);
                claimButton.onClick.AddListener(OnClaimClicked);
            }

            reward = context as RewardPopupUiParams;
            if (reward == null)
            {
                reward = new RewardPopupUiParams();
                reward.Title = "REWARD";
                reward.Coins = 100;
                reward.Icon = ShowcaseIcon.Gift;
            }

            claimed = false;
            claimButton.interactable = true;

            titleLabel.text = reward.Title;
            subtitleLabel.text = reward.Subtitle;
            if (icons != null)
            {
                rewardIcon.sprite = icons.Get(reward.Icon);
            }

            // Set the targets only: CountUpText counts up from zero when the popup shows.
            coinsCounter.SetValue(reward.Coins, false);
            gemsCounter.SetValue(reward.Gems, false);

            bool hasGems = reward.Gems > 0;
            gemsPill.SetActive(hasGems);
            coinsPill.anchoredPosition = new Vector2(hasGems ? coinsPillOffsetWithGems : 0f, coinsPill.anchoredPosition.y);
        }

        public override Task ShowAsync(string presetKey = null)
        {
            Task show = base.ShowAsync(presetKey);
            ShowcaseTween.Forget(PlayConfettiAsync());
            return show;
        }

        private async Task PlayConfettiAsync()
        {
            await ShowcaseTween.DelayAsync(this, confettiDelay, null);
            if (this == null)
            {
                return;
            }

            confetti.Play(confettiOrigin);
        }

        private void OnClaimClicked()
        {
            if (claimed)
            {
                return;
            }

            claimed = true;
            claimButton.interactable = false;

            Vector3 from = claimButton.transform.position;
            ShowcaseTween.Forget(ShowcaseRewards.CollectAsync(from, reward.Coins, reward.Gems));
            ShowcaseTween.Forget(ShowcaseNavigator.ClosePopupAsync());
        }
    }
}
