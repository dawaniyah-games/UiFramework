using System.Threading.Tasks;
using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UiShowcase.Displays;
using UiShowcase.States;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    // Shared by every tab state: it stays loaded and on screen while the tabs change underneath it, so it never
    // re-animates. It listens to the session to update counters that are already visible.
    public class TopBarUiElement : ShowcaseUiElement, IRewardFlyTarget
    {
        [SerializeField] private Button avatarButton;
        [SerializeField] private Text initialsLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text levelLabel;
        [SerializeField] private Text levelBadgeLabel;
        [SerializeField] private CountUpText coinsCounter;
        [SerializeField] private CountUpText gemsCounter;
        [SerializeField] private RectTransform coinIcon;
        [SerializeField] private RectTransform gemIcon;
        [SerializeField] private Button coinsPlusButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private ToastUiDisplay toast;
        [SerializeField] private CoinFlyLayer flyLayer;
        [SerializeField] private Sprite coinSprite;
        [SerializeField] private Sprite gemSprite;

        private bool initialized;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            // Every tab state lists this scene, so Populate runs on each tab switch: wire up once, refresh every time.
            if (!initialized)
            {
                Initialize();
            }

            ShowcaseSession session = ShowcaseSession.Current;
            initialsLabel.text = ShowcaseCatalog.GetInitials(session.PlayerName);
            nameLabel.text = session.PlayerName;
            levelLabel.text = session.PlayerTitle;
            levelBadgeLabel.text = session.Level.ToString();
            coinsCounter.SetValue(session.GetBalance(ShowcaseCurrency.Coins), false);
            gemsCounter.SetValue(session.GetBalance(ShowcaseCurrency.Gems), false);
        }

        public Task FlyRewardAsync(ShowcaseCurrency currency, Vector3 fromWorldPosition, int amount)
        {
            bool isGems = currency == ShowcaseCurrency.Gems;
            int pieces = Mathf.Clamp(isGems ? amount : amount / 25, 3, isGems ? 8 : 12);
            int perPiece = amount / pieces;
            int remainder = amount - perPiece * pieces;
            int landed = 0;

            return flyLayer.FlyAsync(isGems ? gemSprite : coinSprite, ShowcasePalette.ForCurrency(currency), fromWorldPosition,
                isGems ? gemIcon : coinIcon, pieces, () =>
                {
                    landed++;
                    int chunk = landed == pieces ? perPiece + remainder : perPiece;
                    ShowcaseSession.Current.Add(currency, chunk);
                });
        }

        private void Initialize()
        {
            initialized = true;

            avatarButton.onClick.AddListener(OnAvatarClicked);
            coinsPlusButton.onClick.AddListener(OnCoinsPlusClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);

            ShowcaseSession.Current.CurrencyChanged += OnCurrencyChanged;
            ShowcaseSession.Current.ProductUnlocked += OnProductUnlocked;
            ShowcaseRewards.RegisterFlyTarget(this);
        }

        private void OnDestroy()
        {
            if (!initialized)
            {
                return;
            }

            ShowcaseSession.Current.CurrencyChanged -= OnCurrencyChanged;
            ShowcaseSession.Current.ProductUnlocked -= OnProductUnlocked;
            ShowcaseRewards.UnregisterFlyTarget(this);
        }

        private void OnCurrencyChanged(ShowcaseCurrency currency, int balance)
        {
            bool isGems = currency == ShowcaseCurrency.Gems;
            CountUpText counter = isGems ? gemsCounter : coinsCounter;
            counter.SetValue(balance, true);
            ShowcaseTween.Forget(TweenPresets.ClickPop(isGems ? gemIcon : coinIcon));
        }

        private void OnProductUnlocked(ShowcaseProduct product)
        {
            toast.Show("Unlocked " + product.Name + "!");
        }

        private void OnAvatarClicked()
        {
            ShowcaseTween.Forget(ShowcaseNavigator.ShowTabAsync(ShowcaseTab.Profile));
        }

        private void OnCoinsPlusClicked()
        {
            StoreUiParams context = new StoreUiParams();
            context.OpenCoinsPage = true;
            ShowcaseTween.Forget(ShowcaseNavigator.ShowTabAsync(ShowcaseTab.Store, context));
        }

        private void OnSettingsClicked()
        {
            ShowcaseTween.Forget(ShowcaseNavigator.OpenPopupAsync<SettingsUiState>(null));
        }
    }
}
