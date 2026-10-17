using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UiFramework.Tweening
{
    // Show / hide presets for UI elements, plus small feedback effects.
    //
    // Slides: the name is the direction of travel, for show AND hide.
    //   SlideRight show: enters from the left edge moving right. SlideRight hide: keeps moving right, leaves on the right.
    // Entrances use EaseOut (they slow down into place); exits are shorter and use EaseIn (they speed up as they go).
    // Every preset starts from and returns to the object's base pose (recorded once in UiTweenState), so repeated or
    // interrupted animations never drift, and a newer animation on the same object stops the older one.
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

        private const float slideShowDuration = 0.32f;
        private const float slideHideDuration = 0.24f;
        private const float fadeShowDuration = 0.22f;
        private const float fadeHideDuration = 0.16f;
        private const float zoomShowDuration = 0.26f;
        private const float zoomHideDuration = 0.18f;
        private const float zoomShowStartScale = 0.94f;
        private const float zoomHideEndScale = 0.96f;

        private const float clickPeak = 1.08f;
        private const float clickUp = 0.08f;
        private const float clickDown = 0.14f;

        private const float settlePeak = 1.04f;
        private const float settleUp = 0.10f;
        private const float settleDown = 0.18f;

        private const float defaultShakeAmplitude = 8f;
        private const float defaultShakeDuration = 0.32f;
        private const float shakeCycles = 3f;

        // Slide distance for objects without a RectTransform (local units).
        private const float nonUiSlideDistance = 10f;

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

        public static IReadOnlyList<string> GetPresetKeys()
        {
            return presetKeys.AsReadOnly();
        }

        // One plain sentence describing what the preset looks like when showing or hiding, for editor UI.
        public static string Describe(string presetKey, bool isShow)
        {
            switch (Normalize(presetKey))
            {
                case NoneKey:
                    return isShow ? "Appears instantly." : "Disappears instantly.";
                case FadeKey:
                    return isShow ? "Fades in." : "Fades out.";
                case ScaleKey:
                    return isShow ? "Fades in while growing slightly to full size." : "Fades out while shrinking slightly.";
                case SlideRightKey:
                    return isShow ? "Enters from the left edge, moving right." : "Keeps moving right and leaves past the right edge.";
                case SlideLeftKey:
                    return isShow ? "Enters from the right edge, moving left." : "Keeps moving left and leaves past the left edge.";
                case SlideUpKey:
                    return isShow ? "Enters from the bottom edge, moving up." : "Keeps moving up and leaves past the top edge.";
                case SlideDownKey:
                    return isShow ? "Enters from the top edge, moving down." : "Keeps moving down and leaves past the bottom edge.";
                case ClickPopKey:
                    return "A quick pop. Feedback for buttons; it doesn't hide anything.";
                case SettlePopKey:
                    return "A gentle pulse. Feedback; it doesn't hide anything.";
                case ShakeKey:
                    return "A short side-to-side shake (e.g. for errors). Feedback; it doesn't hide anything.";
                default:
                    return "Unknown preset; nothing plays.";
            }
        }

        public static Task PlayAsync(string presetKey, GameObject target, bool isShow)
        {
            if (target == null)
            {
                return Task.CompletedTask;
            }

            switch (Normalize(presetKey))
            {
                case FadeKey:
                    return PlayFadeAsync(target, isShow);
                case ScaleKey:
                    return PlayZoomAsync(target, isShow);
                case ClickPopKey:
                    return ClickPop(target.transform);
                case SettlePopKey:
                    return SettlePop(target.transform);
                case ShakeKey:
                    return ShakeLocal(target.transform);
                case SlideLeftKey:
                    return PlaySlideAsync(target, isShow, Vector2.left);
                case SlideRightKey:
                    return PlaySlideAsync(target, isShow, Vector2.right);
                case SlideUpKey:
                    return PlaySlideAsync(target, isShow, Vector2.up);
                case SlideDownKey:
                    return PlaySlideAsync(target, isShow, Vector2.down);
                default:
                    return Task.CompletedTask;
            }
        }

        // Puts target in the pose its show animation starts from. Call it right after the element is loaded, before
        // it is rendered, so it doesn't flash at its final pose. Also stops any animation still running on target and
        // undoes leftovers from an earlier hide (e.g. still faded out or off-screen).
        public static void PrepareShow(string presetKey, GameObject target)
        {
            if (target == null)
            {
                return;
            }

            string key = Normalize(presetKey);
            UiTweenState state = target.GetComponent<UiTweenState>();

            if (key == NoneKey && state == null)
            {
                return;
            }

            state = GetOrCreateTweenState(target);
            state.BeginAnimation(UiTweenChannels.All);
            RestoreBasePose(target, state);

            switch (key)
            {
                case FadeKey:
                    GetOrAddCanvasGroup(target).alpha = 0f;
                    break;
                case ScaleKey:
                    target.transform.localScale = state.BaseLocalScale * zoomShowStartScale;
                    GetOrAddCanvasGroup(target).alpha = 0f;
                    break;
                case SlideLeftKey:
                    SetSlideStart(target, state, Vector2.left);
                    break;
                case SlideRightKey:
                    SetSlideStart(target, state, Vector2.right);
                    break;
                case SlideUpKey:
                    SetSlideStart(target, state, Vector2.up);
                    break;
                case SlideDownKey:
                    SetSlideStart(target, state, Vector2.down);
                    break;
            }
        }

        public static async Task ClickPop(Transform t, CancellationToken ct = default)
        {
            await PopAsync(t, clickPeak, clickUp, clickDown, ct);
        }

        public static async Task SettlePop(Transform t, CancellationToken ct = default)
        {
            await PopAsync(t, settlePeak, settleUp, settleDown, ct);
        }

        // Smooth, fading side-to-side shake around where the object rests. Re-triggering mid-shake keeps the same
        // resting point, so the object never drifts.
        public static async Task ShakeLocal(Transform t, float amplitude = defaultShakeAmplitude, float duration = defaultShakeDuration, CancellationToken ct = default)
        {
            if (t == null)
            {
                return;
            }

            UiTweenState state = GetOrCreateTweenState(t.gameObject);
            Vector3 origin = state.BeginShake(t.localPosition);
            int run = state.BeginAnimation(UiTweenChannels.Position, true);

            await Tween.RunAsync(t, duration, TweenCurves.Linear, true, ct, () => state != null && state.IsCurrent(run, UiTweenChannels.Position), p =>
            {
                float fade = (1f - p) * (1f - p);
                float offset = Mathf.Sin(p * shakeCycles * 2f * Mathf.PI) * amplitude * fade;
                t.localPosition = origin + new Vector3(offset, 0f, 0f);
            });

            if (state != null && state.IsCurrent(run, UiTweenChannels.Position))
            {
                t.localPosition = origin;
                state.EndShake();
            }
        }

        private static async Task PopAsync(Transform t, float peak, float upDuration, float downDuration, CancellationToken ct)
        {
            if (t == null)
            {
                return;
            }

            UiTweenState state = GetOrCreateTweenState(t.gameObject);
            int run = state.BeginAnimation(UiTweenChannels.Scale);
            Func<bool> isCurrent = () => state != null && state.IsCurrent(run, UiTweenChannels.Scale);

            Vector3 restScale = state.BaseLocalScale;
            Vector3 peakScale = restScale * peak;
            Vector3 start = t.localScale;

            await Tween.RunAsync(t, upDuration, TweenCurves.EaseOut, true, ct, isCurrent, e =>
            {
                t.localScale = Vector3.LerpUnclamped(start, peakScale, e);
            });

            if (!isCurrent())
            {
                return;
            }

            await Tween.RunAsync(t, downDuration, TweenCurves.EaseInOut, true, ct, isCurrent, e =>
            {
                t.localScale = Vector3.LerpUnclamped(peakScale, restScale, e);
            });
        }

        private static Task PlaySlideAsync(GameObject target, bool isShow, Vector2 direction)
        {
            UiTweenState state = GetOrCreateTweenState(target);
            int run = state.BeginAnimation(UiTweenChannels.Position);
            Func<bool> isCurrent = () => state != null && state.IsCurrent(run, UiTweenChannels.Position);
            float duration = isShow ? slideShowDuration : slideHideDuration;
            AnimationCurve ease = isShow ? TweenCurves.EaseOut : TweenCurves.EaseIn;

            RectTransform rectTransform = target.transform as RectTransform;
            if (rectTransform != null)
            {
                Vector2 basePosition = state.BaseAnchoredPosition;
                Vector2 offset = GetSlideOffset(rectTransform, direction);

                // Show travels from the opposite edge to the base position; hide travels on, past the named edge.
                Vector2 from = isShow ? basePosition - offset : rectTransform.anchoredPosition;
                Vector2 to = isShow ? basePosition : basePosition + offset;
                rectTransform.anchoredPosition = from;

                return Tween.RunAsync(target, duration, ease, true, default, isCurrent, e =>
                {
                    rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
                });
            }

            Transform t = target.transform;
            Vector3 localOffset = new Vector3(direction.x, direction.y, 0f) * nonUiSlideDistance;
            Vector3 localFrom = isShow ? state.BaseLocalPosition - localOffset : t.localPosition;
            Vector3 localTo = isShow ? state.BaseLocalPosition : state.BaseLocalPosition + localOffset;
            t.localPosition = localFrom;

            return Tween.RunAsync(target, duration, ease, true, default, isCurrent, e =>
            {
                t.localPosition = Vector3.LerpUnclamped(localFrom, localTo, e);
            });
        }

        private static Task PlayFadeAsync(GameObject target, bool isShow)
        {
            UiTweenState state = GetOrCreateTweenState(target);
            int run = state.BeginAnimation(UiTweenChannels.Alpha);
            CanvasGroup group = GetOrAddCanvasGroup(target);

            float from = isShow ? 0f : group.alpha;
            float to = isShow ? state.BaseAlpha : 0f;
            group.alpha = from;

            return Tween.RunAsync(target, isShow ? fadeShowDuration : fadeHideDuration, isShow ? TweenCurves.EaseOut : TweenCurves.EaseIn, true, default,
                () => state != null && state.IsCurrent(run, UiTweenChannels.Alpha), e =>
                {
                    group.alpha = Mathf.LerpUnclamped(from, to, e);
                });
        }

        // "Scale" preset: a subtle zoom combined with a fade, so the element never pops in or out abruptly.
        private static Task PlayZoomAsync(GameObject target, bool isShow)
        {
            UiTweenState state = GetOrCreateTweenState(target);
            int run = state.BeginAnimation(UiTweenChannels.Scale | UiTweenChannels.Alpha);
            CanvasGroup group = GetOrAddCanvasGroup(target);
            Transform t = target.transform;

            Vector3 baseScale = state.BaseLocalScale;
            Vector3 fromScale = isShow ? baseScale * zoomShowStartScale : t.localScale;
            Vector3 toScale = isShow ? baseScale : baseScale * zoomHideEndScale;
            float fromAlpha = isShow ? 0f : group.alpha;
            float toAlpha = isShow ? state.BaseAlpha : 0f;

            t.localScale = fromScale;
            group.alpha = fromAlpha;

            return Tween.RunAsync(target, isShow ? zoomShowDuration : zoomHideDuration, isShow ? TweenCurves.EaseOut : TweenCurves.EaseIn, true, default,
                () => state != null && state.IsCurrent(run, UiTweenChannels.Scale | UiTweenChannels.Alpha), e =>
                {
                    t.localScale = Vector3.LerpUnclamped(fromScale, toScale, e);
                    group.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, e);
                });
        }

        private static void SetSlideStart(GameObject target, UiTweenState state, Vector2 direction)
        {
            RectTransform rectTransform = target.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = state.BaseAnchoredPosition - GetSlideOffset(rectTransform, direction);
                return;
            }

            target.transform.localPosition = state.BaseLocalPosition - new Vector3(direction.x, direction.y, 0f) * nonUiSlideDistance;
        }

        private static void RestoreBasePose(GameObject target, UiTweenState state)
        {
            Transform t = target.transform;
            t.localScale = state.BaseLocalScale;

            RectTransform rectTransform = t as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = state.BaseAnchoredPosition;
            }
            else
            {
                t.localPosition = state.BaseLocalPosition;
            }

            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = state.BaseAlpha;
            }
        }

        // Full canvas width or height in the direction of travel, so the element starts / ends just off-screen.
        private static Vector2 GetSlideOffset(RectTransform rectTransform, Vector2 direction)
        {
            Vector2 size = Vector2.zero;
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.transform as RectTransform;
                if (canvasRect != null)
                {
                    size = canvasRect.rect.size;
                }
            }

            if (size.x <= 0f)
            {
                size.x = Screen.width;
            }

            if (size.y <= 0f)
            {
                size.y = Screen.height;
            }

            return new Vector2(direction.x * size.x, direction.y * size.y);
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

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            return group;
        }

        private static string Normalize(string presetKey)
        {
            if (string.IsNullOrWhiteSpace(presetKey))
            {
                return NoneKey;
            }

            for (int i = 0; i < presetKeys.Count; i++)
            {
                if (string.Equals(presetKeys[i], presetKey, StringComparison.OrdinalIgnoreCase))
                {
                    return presetKeys[i];
                }
            }

            return presetKey;
        }
    }
}
