using System;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;

namespace UiShowcase.Components
{
    public enum FillAxis
    {
        Horizontal = 0,
        Vertical = 1
    }

    // A progress bar that fills from empty every time its element shows. The fill is a child RectTransform whose
    // anchors are driven (0 to value), so any sliced or rounded sprite keeps its shape. Children of the fill that are
    // anchored to its far edge ride along (the leaderboard avatars sit on top of their podium pillars this way).
    public class FillBar : MonoBehaviour, IUiShowEffect
    {
        [SerializeField] private RectTransform fill;
        [SerializeField] private FillAxis axis = FillAxis.Horizontal;
        [Range(0f, 1f)]
        [SerializeField] private float fillAmount = 1f;
        [Tooltip("Fill from empty when the element shows. Off: the bar only moves when SetValue animates it.")]
        [SerializeField] private bool fillOnShow = true;
        [SerializeField] private float delay = 0.15f;
        [SerializeField] private float duration = 0.7f;

        private float shownAmount;
        private int run;

        public float Value
        {
            get
            {
                return fillAmount;
            }
        }

        public void SetValue(float value, bool animate)
        {
            fillAmount = Mathf.Clamp01(value);
            int currentRun = ++run;

            if (!animate)
            {
                Apply(fillAmount);
                return;
            }

            ShowcaseTween.Forget(AnimateAsync(shownAmount, fillAmount, 0f, currentRun));
        }

        public void PrepareShowEffect()
        {
            if (!fillOnShow)
            {
                return;
            }

            run++;
            Apply(0f);
        }

        public Task PlayShowEffectAsync()
        {
            if (!fillOnShow)
            {
                return Task.CompletedTask;
            }

            int currentRun = ++run;
            return AnimateAsync(0f, fillAmount, delay, currentRun);
        }

        private async Task AnimateAsync(float from, float to, float startDelay, int currentRun)
        {
            Func<bool> isCurrent = () => run == currentRun;
            Apply(from);

            await ShowcaseTween.DelayAsync(this, startDelay, isCurrent);
            if (!isCurrent())
            {
                return;
            }

            await Tween.RunAsync(this, duration, TweenCurves.EaseOut, true, CancellationToken.None, isCurrent, eased =>
            {
                Apply(Mathf.LerpUnclamped(from, to, eased));
            });
        }

        private void Apply(float amount)
        {
            shownAmount = amount;
            if (fill == null)
            {
                return;
            }

            fill.anchorMin = Vector2.zero;
            fill.anchorMax = axis == FillAxis.Horizontal ? new Vector2(amount, 1f) : new Vector2(1f, amount);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }
    }
}
