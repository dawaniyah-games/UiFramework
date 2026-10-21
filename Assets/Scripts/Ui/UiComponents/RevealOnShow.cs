using System;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;

namespace UiShowcase.Components
{
    // Plays a small entrance on this object every time its element shows. No code: add it in the Inspector and pick
    // the motion and delay. Chain delays across siblings to choreograph a screen.
    // It drives position, scale (PopIn) and a CanvasGroup; don't put a LoopMotion that drives the same thing on the
    // same object (put the loop on a child).
    [RequireComponent(typeof(RectTransform))]
    public class RevealOnShow : MonoBehaviour, IUiShowEffect
    {
        [SerializeField] private RevealMotion motion = RevealMotion.FadeUp;
        [SerializeField] private float delay;
        [SerializeField] private float duration = 0.45f;
        [Tooltip("How far the object travels, in canvas pixels (FadeUp, FadeDown, Slide*, Drop).")]
        [SerializeField] private float distance = 60f;

        private RectTransform rectTransform;
        private CanvasGroup group;
        private Vector2 basePosition;
        private Vector3 baseScale;
        private bool hasBasePose;
        private int run;

        public void PrepareShowEffect()
        {
            EnsureBasePose();
            run++;
            ApplyPose(0f);
        }

        public async Task PlayShowEffectAsync()
        {
            EnsureBasePose();
            int currentRun = ++run;
            Func<bool> isCurrent = () => run == currentRun;
            ApplyPose(0f);

            await ShowcaseTween.DelayAsync(this, delay, isCurrent);
            if (!isCurrent())
            {
                return;
            }

            await Tween.RunAsync(this, duration, TweenCurves.Linear, true, CancellationToken.None, isCurrent, ApplyPose);
        }

        // Recorded on the first show, after Populate has placed the object, so spawned rows reveal from where they sit.
        private void EnsureBasePose()
        {
            if (hasBasePose)
            {
                return;
            }

            rectTransform = (RectTransform)transform;
            group = ShowcaseTween.GetOrAddCanvasGroup(gameObject);
            basePosition = rectTransform.anchoredPosition;
            baseScale = rectTransform.localScale;
            hasBasePose = true;
        }

        private void ApplyPose(float progress)
        {
            RevealPose.Apply(rectTransform, group, basePosition, baseScale, motion, distance, progress);
        }
    }
}
