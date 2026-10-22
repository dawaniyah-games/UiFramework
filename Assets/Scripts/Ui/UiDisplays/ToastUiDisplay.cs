using System;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Displays
{
    // A short message that drops in, waits and slides away. A new message restarts it from wherever it is.
    [RequireComponent(typeof(RectTransform))]
    public class ToastUiDisplay : MonoBehaviour
    {
        [SerializeField] private Text messageLabel;
        [SerializeField] private float holdSeconds = 1.8f;
        [Tooltip("How far above its resting place the toast starts and ends, in canvas pixels.")]
        [SerializeField] private float travel = 140f;

        private RectTransform rectTransform;
        private CanvasGroup group;
        private Vector2 basePosition;
        private int run;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            group = ShowcaseTween.GetOrAddCanvasGroup(gameObject);
            group.blocksRaycasts = false;
            group.interactable = false;
            basePosition = rectTransform.anchoredPosition;

            rectTransform.anchoredPosition = basePosition + new Vector2(0f, travel);
            group.alpha = 0f;
        }

        public void Show(string message)
        {
            if (messageLabel != null)
            {
                messageLabel.text = message;
            }

            ShowcaseTween.Forget(PlayAsync(++run));
        }

        private async Task PlayAsync(int currentRun)
        {
            Func<bool> isCurrent = () => run == currentRun;
            Vector2 hidden = basePosition + new Vector2(0f, travel);
            Vector2 start = rectTransform.anchoredPosition;
            float startAlpha = group.alpha;

            await Tween.RunAsync(this, 0.4f, TweenCurves.Linear, true, CancellationToken.None, isCurrent, progress =>
            {
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(start, basePosition, ShowcaseCurves.BackOut.Evaluate(progress));
                group.alpha = Mathf.Lerp(startAlpha, 1f, TweenCurves.EaseOut.Evaluate(Mathf.Clamp01(progress * 2f)));
            });

            await ShowcaseTween.DelayAsync(this, holdSeconds, isCurrent);
            if (!isCurrent())
            {
                return;
            }

            await Tween.RunAsync(this, 0.25f, TweenCurves.EaseIn, true, CancellationToken.None, isCurrent, eased =>
            {
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(basePosition, hidden, eased);
                group.alpha = 1f - eased;
            });
        }
    }
}
