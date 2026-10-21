using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // A number that counts up from zero when its element shows, and rolls to new values (currency counters).
    [RequireComponent(typeof(Text))]
    public class CountUpText : MonoBehaviour, IUiShowEffect
    {
        [SerializeField] private int targetValue;
        [Tooltip("string.Format pattern; {0} is the number. E.g. \"{0:N0}\" or \"+{0:N0}\".")]
        [SerializeField] private string format = "{0:N0}";
        [Tooltip("Count up from zero when the element shows. Off: the number only moves when SetValue animates it.")]
        [SerializeField] private bool countOnShow = true;
        [SerializeField] private float delay = 0.2f;
        [SerializeField] private float duration = 0.8f;

        private Text label;
        private float shownValue;
        private int lastWrittenValue = int.MinValue;
        private int run;

        public int Value
        {
            get
            {
                return targetValue;
            }
        }

        private Text Label
        {
            get
            {
                if (label == null)
                {
                    label = GetComponent<Text>();
                }

                return label;
            }
        }

        public void SetValue(int value, bool animate)
        {
            targetValue = value;
            int currentRun = ++run;

            if (!animate)
            {
                Write(value);
                return;
            }

            ShowcaseTween.Forget(AnimateAsync(shownValue, value, 0f, duration * 0.75f, currentRun));
        }

        public void PrepareShowEffect()
        {
            if (!countOnShow)
            {
                Write(targetValue);
                return;
            }

            run++;
            Write(0f);
        }

        public Task PlayShowEffectAsync()
        {
            if (!countOnShow)
            {
                return Task.CompletedTask;
            }

            int currentRun = ++run;
            return AnimateAsync(0f, targetValue, delay, duration, currentRun);
        }

        private async Task AnimateAsync(float from, float to, float startDelay, float seconds, int currentRun)
        {
            Func<bool> isCurrent = () => run == currentRun;
            Write(from);

            await ShowcaseTween.DelayAsync(this, startDelay, isCurrent);
            if (!isCurrent())
            {
                return;
            }

            await Tween.RunAsync(this, seconds, TweenCurves.EaseOut, true, CancellationToken.None, isCurrent, eased =>
            {
                Write(Mathf.LerpUnclamped(from, to, eased));
            });
        }

        private void Write(float value)
        {
            shownValue = value;
            int rounded = Mathf.RoundToInt(value);

            // Only touch the Text when the visible number changes; rebuilding it every frame is wasted work.
            if (rounded == lastWrittenValue)
            {
                return;
            }

            lastWrittenValue = rounded;
            Label.text = string.Format(CultureInfo.InvariantCulture, format, rounded);
        }
    }
}
