using System;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Elements
{
    // Boot screen: logo choreography, a fake loading bar with rotating tips, then into the app.
    public class IntroUiElement : ShowcaseUiElement
    {
        [SerializeField] private FillBar loadingBar;
        [SerializeField] private Text percentLabel;
        [SerializeField] private Text tipLabel;
        [SerializeField] private float loadingSeconds = 2.6f;
        [SerializeField] private string[] tips =
        {
            "Every screen is its own Addressable scene",
            "Shared elements stay on screen between states",
            "Pick show and hide animations in the Elements tab"
        };

        private int loadingRun;
        private int shownTip = -1;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            // Populate runs on every show of this scene (Settings > Replay intro), so the loading restarts cleanly.
            loadingRun++;
            shownTip = -1;
            loadingBar.SetValue(0f, false);
            percentLabel.text = "0%";
            ShowTip(0, false);
        }

        public override async Task ShowAsync(string presetKey = null)
        {
            await base.ShowAsync(presetKey);

            // Not awaited: UiManager is still inside ShowState until this returns, and the loading ends by navigating
            // away, which must run after that call has finished (ShowcaseNavigator queues it behind).
            ShowcaseTween.Forget(RunLoadingAsync(loadingRun));
        }

        private async Task RunLoadingAsync(int run)
        {
            Func<bool> isCurrent = () => run == loadingRun;

            await Tween.RunAsync(this, loadingSeconds, TweenCurves.EaseInOut, true, CancellationToken.None, isCurrent, eased =>
            {
                loadingBar.SetValue(eased, false);
                percentLabel.text = Mathf.RoundToInt(eased * 100f) + "%";
                ShowTip(Mathf.Min(tips.Length - 1, Mathf.FloorToInt(eased * tips.Length)), true);
            });

            if (this == null || !isCurrent())
            {
                return;
            }

            await ShowcaseNavigator.EnterAppAsync();
        }

        private void ShowTip(int index, bool animate)
        {
            if (tips == null || tips.Length == 0 || index == shownTip)
            {
                return;
            }

            shownTip = index;
            tipLabel.text = tips[index];

            if (animate)
            {
                ShowcaseTween.Forget(TweenPresets.SettlePop(tipLabel.transform));
            }
        }
    }
}
