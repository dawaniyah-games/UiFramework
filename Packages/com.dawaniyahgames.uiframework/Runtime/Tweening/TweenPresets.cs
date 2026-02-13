using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UiFramework.Tweening
{
    public static class TweenPresets
    {
        public const string NoneKey = "None";
        public const string FadeKey = "Fade";
        public const string ScaleKey = "Scale";
        public const string ClickPopKey = "ClickPop";
        public const string SettlePopKey = "SettlePop";
        public const string ShakeKey = "Shake";
        public const string SlideLeftKey = "SlideLeft";
        public const string SlideRightKey = "SlideRight";
        public const string SlideUpKey = "SlideUp";
        public const string SlideDownKey = "SlideDown";

        private const float DefaultDuration = 0.20f;

        private static readonly List<string> presetKeys = new List<string>
        {
            NoneKey,
            FadeKey,
            ScaleKey,
            ClickPopKey,
            SettlePopKey,
            ShakeKey,
            SlideLeftKey,
            SlideRightKey,
            SlideUpKey,
            SlideDownKey
        };

        private const float ClickPeak = 1.12f;
        private const float ClickUp = 0.08f;
        private const float ClickDown = 0.10f;

        private const float SettlePeak = 1.06f;
        private const float SettleUp = 0.06f;
        private const float SettleDown = 0.08f;

        public static IReadOnlyList<string> GetPresetKeys()
        {
            return presetKeys.AsReadOnly();
        }

        public static Task PlayAsync(string presetKey, GameObject target, bool isShow)
        {
            if (target == null)
            {
                return Task.CompletedTask;
            }

            if (string.IsNullOrWhiteSpace(presetKey) || string.Equals(presetKey, NoneKey, StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            if (string.Equals(presetKey, FadeKey, StringComparison.OrdinalIgnoreCase))
            {
                return PlayFadeAsync(target, isShow);
            }

            if (string.Equals(presetKey, ScaleKey, StringComparison.OrdinalIgnoreCase))
            {
                return PlayScaleAsync(target, isShow);
            }

            if (string.Equals(presetKey, ClickPopKey, StringComparison.OrdinalIgnoreCase))
            {
                return ClickPop(target.transform);
            }

            if (string.Equals(presetKey, SettlePopKey, StringComparison.OrdinalIgnoreCase))
            {
                return SettlePop(target.transform);
            }

            if (string.Equals(presetKey, ShakeKey, StringComparison.OrdinalIgnoreCase))
            {
                return ShakeLocal(target.transform);
            }

            if (string.Equals(presetKey, SlideLeftKey, StringComparison.OrdinalIgnoreCase))
            {
                return PlaySlideAsync(target, isShow, Vector2.left);
            }

            if (string.Equals(presetKey, SlideRightKey, StringComparison.OrdinalIgnoreCase))
            {
                return PlaySlideAsync(target, isShow, Vector2.right);
            }

            if (string.Equals(presetKey, SlideUpKey, StringComparison.OrdinalIgnoreCase))
            {
                return PlaySlideAsync(target, isShow, Vector2.up);
            }

            if (string.Equals(presetKey, SlideDownKey, StringComparison.OrdinalIgnoreCase))
            {
                return PlaySlideAsync(target, isShow, Vector2.down);
            }

            return Task.CompletedTask;
        }


        private static UiTweenState GetOrCreateTweenState(GameObject target)
        {
            UiTweenState state = target.GetComponent<UiTweenState>();
            if (state == null)
            {
                state = target.AddComponent<UiTweenState>();
            }

            state.EnsureInitialized();
            return state;
        }

        private static RectTransform FindRectTransformForAnimation(GameObject target)
        {
            if (target == null)
            {
                return null;
            }

            RectTransform self = target.GetComponent<RectTransform>();
            if (self != null)
            {
                return self;
            }

            return target.GetComponentInChildren<RectTransform>(true);
        }

        private static float GetCanvasWidth(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0.0f;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return 0.0f;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return 0.0f;
            }

            return canvasRect.rect.width;
        }

        private static float GetCanvasHeight(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return 0.0f;
            }

            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return 0.0f;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return 0.0f;
            }

            return canvasRect.rect.height;
        }

        private static async Task PlaySlideAsync(GameObject target, bool isShow, Vector2 direction)
        {
            if (target == null)
            {
                return;
            }

            RectTransform rectTransform = FindRectTransformForAnimation(target);
            UiTweenState tweenState = GetOrCreateTweenState(rectTransform != null ? rectTransform.gameObject : target);

            if (rectTransform != null)
            {
                float canvasWidth = GetCanvasWidth(rectTransform);
                float canvasHeight = GetCanvasHeight(rectTransform);

                Vector2 basePos = tweenState.BaseAnchoredPosition;
                Vector2 offset = Vector2.zero;

                Vector2 offscreenDirection = isShow ? -direction : direction;

                if (Mathf.Abs(offscreenDirection.x) > 0.0f)
                {
                    float w = canvasWidth;
                    if (w <= 0.0f)
                    {
                        w = Screen.width;
                    }

                    offset = new Vector2(offscreenDirection.x * w, 0.0f);
                }
                else
                {
                    float h = canvasHeight;
                    if (h <= 0.0f)
                    {
                        h = Screen.height;
                    }

                    offset = new Vector2(0.0f, offscreenDirection.y * h);
                }

                Vector2 from = isShow ? basePos + offset : basePos;
                Vector2 to = isShow ? basePos : basePos + offset;

                if (isShow)
                {
                    rectTransform.anchoredPosition = from;
                }
                else
                {
                    from = rectTransform.anchoredPosition;
                }

                float start = Time.unscaledTime;
                float end = start + DefaultDuration;

                while (Time.unscaledTime < end)
                {
                    float a = Mathf.InverseLerp(start, end, Time.unscaledTime);
                    float e = TweenCurves.EaseOut != null ? TweenCurves.EaseOut.Evaluate(a) : a;
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
                    await Task.Yield();
                }

                rectTransform.anchoredPosition = to;
                return;
            }

            Transform t = target.transform;
            Vector3 baseLocal = tweenState.BaseLocalPosition;
            Vector2 localOffscreenDirection = isShow ? -direction : direction;
            Vector3 localOffset = new Vector3(localOffscreenDirection.x, localOffscreenDirection.y, 0.0f) * 10.0f;
            Vector3 localFrom = isShow ? baseLocal + localOffset : t.localPosition;
            Vector3 localTo = isShow ? baseLocal : baseLocal + localOffset;

            await Tween.MoveLocalAsync(t, localFrom, localTo, DefaultDuration, TweenCurves.EaseOut, true, default);
        }

        public static async Task ClickPop(Transform t, CancellationToken ct = default)
        {
            await Tween.PopScaleAsync(
                t,
                ClickPeak,
                ClickUp,
                ClickDown,
                TweenCurves.EaseOut,
                TweenCurves.EaseIn,
                true,
                ct
            );
        }

        public static async Task SettlePop(Transform t, CancellationToken ct = default)
        {
            await Tween.PopScaleAsync(
                t,
                SettlePeak,
                SettleUp,
                SettleDown,
                TweenCurves.EaseOut,
                TweenCurves.BounceOut,
                true,
                ct
            );
        }

        public static async Task ShakeLocal(Transform t, float amplitude = 6f, float duration = DefaultDuration, CancellationToken ct = default)
        {
            if (t == null)
            {
                return;
            }

            Vector3 origin = t.localPosition;
            float start = Time.unscaledTime;
            float end = start + duration;

            while (Time.unscaledTime < end)
            {
                ct.ThrowIfCancellationRequested();
                float progress = Mathf.InverseLerp(start, end, Time.unscaledTime);
                float atten = 1f - progress;
                Vector3 offset = new Vector3(
                    (UnityEngine.Random.value * 2f - 1f) * amplitude * atten,
                    (UnityEngine.Random.value * 2f - 1f) * amplitude * atten,
                    0f
                );
                t.localPosition = origin + offset;
                await Task.Yield();
            }

            t.localPosition = origin;
        }

        private static Task PlayFadeAsync(GameObject target, bool isShow)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            float from = isShow ? 0.0f : 1.0f;
            float to = isShow ? 1.0f : 0.0f;

            return Tween.FadeCanvasGroupAsync(group, from, to, DefaultDuration, TweenCurves.EaseOut, true, default);
        }

        private static Task PlayScaleAsync(GameObject target, bool isShow)
        {
            Transform t = target.transform;
            Vector3 baseScale = t.localScale;
            Vector3 from = isShow ? baseScale * 0.9f : baseScale;
            Vector3 to = isShow ? baseScale : baseScale * 0.9f;

            return Tween.ScaleAsync(t, from, to, DefaultDuration, TweenCurves.EaseOut, true, default);
        }
    }
}
