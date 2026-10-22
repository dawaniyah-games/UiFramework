using System.Threading;
using System.Threading.Tasks;
using UiFramework.Core;
using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    public class ConfirmPurchaseUiParams : UiPopulationParams
    {
        public ShowcaseProduct Product { get; set; }
    }

    // Additive bottom sheet. The element fades the backdrop (its Fade preset); the sheet slides up with a RevealOnShow.
    // Not enough currency shakes the Buy button with the framework's Shake preset.
    public class ConfirmPurchaseUiElement : ShowcaseUiElement
    {
        [SerializeField] private ShowcaseIconLibrary icons;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private Image productIcon;
        [SerializeField] private Image productGlow;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private Text priceLabel;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private Text errorLabel;

        private ShowcaseProduct product;
        private CanvasGroup errorGroup;
        private bool initialized;
        private bool isClosing;
        private int errorRun;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                initialized = true;
                ShowcaseIconLibrary.WarnIfMissing(icons, this);
                errorGroup = ShowcaseTween.GetOrAddCanvasGroup(errorLabel.gameObject);
                backdropButton.onClick.AddListener(Close);
                cancelButton.onClick.AddListener(Close);
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            ConfirmPurchaseUiParams data = context as ConfirmPurchaseUiParams;
            product = data != null ? data.Product : null;
            isClosing = false;
            errorRun++;
            errorGroup.alpha = 0f;

            if (product == null)
            {
                Debug.LogWarning("[ConfirmPurchaseUiElement] Shown without a product, so there is nothing to buy. Pass a ConfirmPurchaseUiParams with its Product set as the ShowState context.", this);
                return;
            }

            Color rarityColor = ShowcasePalette.ForRarity(product.Rarity);
            if (icons != null)
            {
                productIcon.sprite = icons.Get(product.Icon);
                currencyIcon.sprite = icons.Get(product.Currency == ShowcaseCurrency.Gems ? ShowcaseIcon.Gem : ShowcaseIcon.Coin);
            }

            productIcon.color = product.GrantsCoins > 0 && product.IsRepeatable ? ShowcasePalette.Gold : rarityColor;
            productGlow.color = ShowcasePalette.WithAlpha(rarityColor, 0.4f);
            currencyIcon.color = ShowcasePalette.ForCurrency(product.Currency);
            nameLabel.text = product.Name;
            descriptionLabel.text = product.Description;
            priceLabel.text = product.Price.ToString("N0");
        }

        private void OnBuyClicked()
        {
            if (product == null || isClosing)
            {
                return;
            }

            if (!ShowcaseSession.Current.TryPurchase(product))
            {
                ShowcaseTween.Forget(TweenPresets.ShakeLocal(buyButton.transform, 16f, 0.4f));
                ShowcaseTween.Forget(FlashErrorAsync("Not enough " + (product.Currency == ShowcaseCurrency.Gems ? "gems" : "coins") + " yet"));
                return;
            }

            isClosing = true;
            if (product.GrantsCoins > 0)
            {
                ShowcaseTween.Forget(ShowcaseRewards.CollectAsync(productIcon.transform.position, product.GrantsCoins, 0));
            }

            ShowcaseTween.Forget(ShowcaseNavigator.ClosePopupAsync());
        }

        private void Close()
        {
            if (isClosing)
            {
                return;
            }

            isClosing = true;
            ShowcaseTween.Forget(ShowcaseNavigator.ClosePopupAsync());
        }

        private async Task FlashErrorAsync(string message)
        {
            int currentRun = ++errorRun;
            errorLabel.text = message;

            await Tween.RunAsync(errorLabel, 0.2f, TweenCurves.EaseOut, true, CancellationToken.None, () => errorRun == currentRun, eased =>
            {
                errorGroup.alpha = eased;
            });

            await ShowcaseTween.DelayAsync(errorLabel, 1.6f, () => errorRun == currentRun);
            if (errorRun != currentRun)
            {
                return;
            }

            await Tween.RunAsync(errorLabel, 0.3f, TweenCurves.EaseIn, true, CancellationToken.None, () => errorRun == currentRun, eased =>
            {
                errorGroup.alpha = 1f - eased;
            });
        }
    }
}
