using System;
using System.Collections.Generic;
using System.Threading;
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
    // Store tab: two pages (items, coin packs) switched in place with a slide, an offer banner, and product cards that
    // open the confirm-purchase sheet as an additive popup.
    public class StoreUiElement : ShowcaseUiElement
    {
        private const int ItemsPageIndex = 0;
        private const int CoinsPageIndex = 1;

        [SerializeField] private ShowcaseIconLibrary icons;
        [SerializeField] private SegmentedControl pageTabs;
        [SerializeField] private RectTransform itemsPage;
        [SerializeField] private RectTransform coinsPage;
        [SerializeField] private RectTransform itemsGrid;
        [SerializeField] private ProductCardUiDisplay itemCardTemplate;
        [SerializeField] private RectTransform coinPackList;
        [SerializeField] private ProductCardUiDisplay coinPackTemplate;
        [SerializeField] private Button offerButton;
        [SerializeField] private GameObject offerPricePill;
        [SerializeField] private GameObject offerOwnedStamp;
        [SerializeField] private Text offerPriceLabel;
        [SerializeField] private Vector2 gridCellSize = new Vector2(300f, 360f);
        [SerializeField] private Vector2 gridSpacing = new Vector2(30f, 30f);
        [SerializeField] private float coinPackSpacing = 226f;
        [SerializeField] private float pageSlideDistance = 180f;

        private readonly List<ProductCardUiDisplay> cards = new List<ProductCardUiDisplay>();
        private bool initialized;
        private int pageIndex;
        private int pageRun;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            StoreUiParams storeContext = context as StoreUiParams;
            int page = storeContext != null && storeContext.OpenCoinsPage ? CoinsPageIndex : ItemsPageIndex;

            // A second Populate on the same instance means the Store is already on screen and the new state shares
            // its scene (the TopBar "+" while on the Store). UiManager won't animate a shared element, so switch the
            // page in place instead of rebuilding what is visible.
            if (initialized)
            {
                pageTabs.Select(page, true);
                ShowcaseTween.Forget(SwitchPageAsync(page));
                return;
            }

            Initialize();
            RebuildCards();
            RefreshOffer(false);
            ShowPageImmediately(page);
        }

        private void Initialize()
        {
            initialized = true;
            ShowcaseIconLibrary.WarnIfMissing(icons, this);
            itemCardTemplate.gameObject.SetActive(false);
            coinPackTemplate.gameObject.SetActive(false);
            offerPriceLabel.text = ShowcaseCatalog.StarterBundle.Price.ToString();

            pageTabs.SelectionChanged += OnPageSelected;
            offerButton.onClick.AddListener(OnOfferClicked);
            ShowcaseSession.Current.ProductUnlocked += OnProductUnlocked;
            ShowcaseSession.Current.DataReset += OnDataReset;
        }

        private void OnDestroy()
        {
            if (!initialized)
            {
                return;
            }

            ShowcaseSession.Current.ProductUnlocked -= OnProductUnlocked;
            ShowcaseSession.Current.DataReset -= OnDataReset;
        }

        private void RebuildCards()
        {
            ShowcaseDisplays.DestroyAll(cards);
            ShowcaseSession session = ShowcaseSession.Current;

            IReadOnlyList<ShowcaseProduct> items = ShowcaseCatalog.Items;
            for (int i = 0; i < items.Count; i++)
            {
                Vector2 position = ShowcaseDisplays.GridPosition(i, 3, gridCellSize, gridSpacing);
                ProductCardUiDisplay card = ShowcaseDisplays.Spawn(itemCardTemplate, itemsGrid, position);
                card.Bind(items[i], icons, session.IsOwned(items[i]), OnCardClicked);
                cards.Add(card);
            }

            IReadOnlyList<ShowcaseProduct> packs = ShowcaseCatalog.CoinPacks;
            for (int i = 0; i < packs.Count; i++)
            {
                ProductCardUiDisplay card = ShowcaseDisplays.Spawn(coinPackTemplate, coinPackList, new Vector2(0f, -i * coinPackSpacing));
                card.Bind(packs[i], icons, false, OnCardClicked);
                cards.Add(card);
            }
        }

        private void RefreshOffer(bool animate)
        {
            bool owned = ShowcaseSession.Current.IsOwned(ShowcaseCatalog.StarterBundle);
            offerPricePill.SetActive(!owned);
            offerOwnedStamp.SetActive(owned);

            if (owned && animate)
            {
                ShowcaseTween.Forget(TweenPresets.SettlePop(offerOwnedStamp.transform));
            }
        }

        private void OnCardClicked(ProductCardUiDisplay card)
        {
            OpenPurchase(card.Product, card.transform);
        }

        private void OnOfferClicked()
        {
            OpenPurchase(ShowcaseCatalog.StarterBundle, offerButton.transform);
        }

        private void OpenPurchase(ShowcaseProduct product, Transform source)
        {
            if (ShowcaseSession.Current.IsOwned(product))
            {
                ShowcaseTween.Forget(TweenPresets.ShakeLocal(source));
                return;
            }

            ConfirmPurchaseUiParams context = new ConfirmPurchaseUiParams();
            context.Product = product;
            ShowcaseTween.Forget(ShowcaseNavigator.OpenPopupAsync<ConfirmPurchaseUiState>(context));
        }

        private void OnProductUnlocked(ShowcaseProduct product)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null && cards[i].Product == product)
                {
                    cards[i].SetOwned(true, true);
                }
            }

            if (product == ShowcaseCatalog.StarterBundle)
            {
                RefreshOffer(true);
            }
        }

        private void OnDataReset()
        {
            RebuildCards();
            RefreshOffer(false);
        }

        private void OnPageSelected(int index)
        {
            ShowcaseTween.Forget(SwitchPageAsync(index));
        }

        private void ShowPageImmediately(int index)
        {
            pageRun++;
            pageIndex = index;
            pageTabs.Select(index, false);

            SetPageVisible(itemsPage, index == ItemsPageIndex, 0f);
            SetPageVisible(coinsPage, index == CoinsPageIndex, 0f);
        }

        // In-element animation, independent of UiManager: the old page leaves towards one side and the new one comes in
        // from the other, then the new page's show effects (staggered cards) replay.
        private async Task SwitchPageAsync(int index)
        {
            if (index == pageIndex)
            {
                return;
            }

            int currentRun = ++pageRun;
            Func<bool> isCurrent = () => pageRun == currentRun;
            float direction = index > pageIndex ? -1f : 1f;
            RectTransform outgoing = GetPage(pageIndex);
            RectTransform incoming = GetPage(index);
            pageIndex = index;

            CanvasGroup outgoingGroup = ShowcaseTween.GetOrAddCanvasGroup(outgoing.gameObject);
            await Tween.RunAsync(outgoing, 0.16f, TweenCurves.EaseIn, true, CancellationToken.None, isCurrent, eased =>
            {
                outgoing.anchoredPosition = new Vector2(direction * pageSlideDistance * eased, 0f);
                outgoingGroup.alpha = 1f - eased;
            });

            if (!isCurrent())
            {
                return;
            }

            SetPageVisible(outgoing, false, 0f);
            SetPageVisible(incoming, true, -direction * pageSlideDistance);
            CanvasGroup incomingGroup = ShowcaseTween.GetOrAddCanvasGroup(incoming.gameObject);
            incomingGroup.alpha = 0f;

            Task effects = UiShowEffects.ReplayAsync(incoming);
            Task slide = Tween.RunAsync(incoming, 0.28f, TweenCurves.EaseOut, true, CancellationToken.None, isCurrent, eased =>
            {
                incoming.anchoredPosition = new Vector2(-direction * pageSlideDistance * (1f - eased), 0f);
                incomingGroup.alpha = eased;
            });

            await Task.WhenAll(effects, slide);
        }

        private RectTransform GetPage(int index)
        {
            return index == CoinsPageIndex ? coinsPage : itemsPage;
        }

        private static void SetPageVisible(RectTransform page, bool visible, float x)
        {
            page.gameObject.SetActive(visible);
            page.anchoredPosition = new Vector2(x, 0f);
            ShowcaseTween.GetOrAddCanvasGroup(page.gameObject).alpha = 1f;
        }
    }
}
