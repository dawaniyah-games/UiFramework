using System.Collections.Generic;
using System.Threading;
using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;

namespace UiShowcase.Elements
{
    // Shared by every tab state. The selection follows the TabUiParams each tab state is shown with, so it is right
    // however the tab was reached (a NavBar tap, the TopBar avatar, the coins "+" button).
    public class NavBarUiElement : ShowcaseUiElement
    {
        [SerializeField] private List<NavBarButton> buttons = new List<NavBarButton>();
        [SerializeField] private RectTransform indicator;
        [SerializeField] private float indicatorDuration = 0.36f;
        [Tooltip("Extra width at the middle of the move, so the pill stretches like a drop of liquid.")]
        [SerializeField] private float indicatorStretch = 0.35f;

        private bool initialized;
        private bool hasSelection;
        private ShowcaseTab selectedTab;
        private float indicatorWidth;
        private int indicatorRun;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                Initialize();
            }

            TabUiParams tabContext = context as TabUiParams;
            if (tabContext != null)
            {
                // Animate only when the bar is already on screen; the first time it slides in already showing the tab.
                Select(tabContext.Tab, hasSelection);
            }
        }

        private void Initialize()
        {
            initialized = true;
            indicatorWidth = indicator.sizeDelta.x;

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Clicked += OnButtonClicked;
                buttons[i].SetSelected(false, false);
            }
        }

        private void OnButtonClicked(ShowcaseTab tab)
        {
            if (hasSelection && tab == selectedTab)
            {
                return;
            }

            // Move the pill at once so the tap feels instant; the screen follows when its scene is ready.
            Select(tab, true);
            ShowcaseTween.Forget(ShowcaseNavigator.ShowTabAsync(tab));
        }

        private void Select(ShowcaseTab tab, bool animate)
        {
            if (hasSelection && tab == selectedTab)
            {
                return;
            }

            selectedTab = tab;
            hasSelection = true;

            RectTransform target = null;
            for (int i = 0; i < buttons.Count; i++)
            {
                bool isSelected = buttons[i].Tab == tab;
                buttons[i].SetSelected(isSelected, animate);
                if (isSelected)
                {
                    target = (RectTransform)buttons[i].transform;
                }
            }

            if (target != null)
            {
                MoveIndicator(target.anchoredPosition.x, animate);
            }
        }

        private void MoveIndicator(float toX, bool animate)
        {
            int currentRun = ++indicatorRun;
            float fromX = indicator.anchoredPosition.x;
            float y = indicator.anchoredPosition.y;

            if (!animate)
            {
                indicator.anchoredPosition = new Vector2(toX, y);
                return;
            }

            ShowcaseTween.Forget(Tween.RunAsync(indicator, indicatorDuration, TweenCurves.Linear, true, CancellationToken.None,
                () => indicatorRun == currentRun,
                progress =>
                {
                    float widthFactor = 1f + indicatorStretch * Mathf.Sin(progress * Mathf.PI);
                    indicator.anchoredPosition = new Vector2(Mathf.LerpUnclamped(fromX, toX, TweenCurves.EaseOut.Evaluate(progress)), y);
                    indicator.sizeDelta = new Vector2(indicatorWidth * widthFactor, indicator.sizeDelta.y);
                }));
        }
    }
}
