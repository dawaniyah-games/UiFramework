using System;
using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Displays
{
    // A store card (items grid and coin packs share it with different layouts).
    public class ProductCardUiDisplay : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Image iconGlow;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text detailLabel;
        [SerializeField] private GameObject pricePill;
        [SerializeField] private Text priceLabel;
        [SerializeField] private Image currencyIcon;
        [SerializeField] private GameObject ownedStamp;

        private Action<ProductCardUiDisplay> clicked;

        public ShowcaseProduct Product { get; private set; }

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnClicked);
            }
        }

        public void Bind(ShowcaseProduct product, ShowcaseIconLibrary icons, bool isOwned, Action<ProductCardUiDisplay> onClicked)
        {
            Product = product;
            clicked = onClicked;

            Color rarityColor = ShowcasePalette.ForRarity(product.Rarity);

            if (icons != null)
            {
                icon.sprite = icons.Get(product.Icon);
                currencyIcon.sprite = icons.Get(product.Currency == ShowcaseCurrency.Gems ? ShowcaseIcon.Gem : ShowcaseIcon.Coin);
            }

            icon.color = product.GrantsCoins > 0 ? ShowcasePalette.Gold : rarityColor;
            iconGlow.color = ShowcasePalette.WithAlpha(rarityColor, 0.35f);
            currencyIcon.color = ShowcasePalette.ForCurrency(product.Currency);

            nameLabel.text = product.Name;
            detailLabel.text = product.GrantsCoins > 0 ? "+" + product.GrantsCoins.ToString("N0") + " coins" : product.Rarity.ToString().ToUpperInvariant();
            detailLabel.color = product.GrantsCoins > 0 ? ShowcasePalette.Gold : rarityColor;
            priceLabel.text = product.Price.ToString("N0");

            SetOwned(isOwned, false);
        }

        public void SetOwned(bool isOwned, bool animate)
        {
            if (pricePill != null)
            {
                pricePill.SetActive(!isOwned);
            }

            if (ownedStamp == null)
            {
                return;
            }

            ownedStamp.SetActive(isOwned);
            if (isOwned && animate)
            {
                ShowcaseTween.Forget(TweenPresets.SettlePop(ownedStamp.transform));
                ShowcaseTween.Forget(TweenPresets.SettlePop(icon.transform));
            }
        }

        private void OnClicked()
        {
            if (clicked != null)
            {
                clicked(this);
            }
        }
    }
}
