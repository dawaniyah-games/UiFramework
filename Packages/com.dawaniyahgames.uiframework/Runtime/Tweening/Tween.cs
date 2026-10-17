using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UiFramework.Tweening
{
    public static class Tween
    {
        // The most an animation advances in one frame. A long frame (for example the one where a scene loads) then
        // slows the animation a little instead of skipping most of it, which is what made motion look like a jump.
        public const float MaxFrameStep = 1.0f / 30.0f;

        // Shared animation loop. apply receives the eased progress (0..1) once per frame and is always called with 1
        // at the end. Stops early, without applying the end value, when owner is destroyed or keepRunning returns
        // false (a newer animation took over the same object).
        public static async Task RunAsync(UnityEngine.Object owner, float duration, AnimationCurve ease, bool unscaled,
            CancellationToken ct, Func<bool> keepRunning, Action<float> apply)
        {
            if (apply == null)
            {
                return;
            }

            if (duration <= 0f)
            {
                apply(1f);
                return;
            }

            float elapsed = 0f;

            while (true)
            {
                await Task.Yield();

                if (owner == null || (keepRunning != null && !keepRunning()))
                {
                    return;
                }

                ct.ThrowIfCancellationRequested();

                float delta = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                elapsed += Mathf.Min(delta, MaxFrameStep);

                float progress = Mathf.Clamp01(elapsed / duration);
                apply(ease != null ? ease.Evaluate(progress) : progress);

                if (progress >= 1f)
                {
                    return;
                }
            }
        }

        public static async Task PopScaleAsync(
            Transform t,
            float peakScale,
            float upDuration,
            float downDuration,
            AnimationCurve easeUp,
            AnimationCurve easeDown,
            bool unscaled = true,
            CancellationToken ct = default)
        {
            Vector3 baseScale = t.localScale;
            Vector3 peak = baseScale * peakScale;

            await ScaleAsync(t, baseScale, peak, upDuration, easeUp, unscaled, ct);
            await ScaleAsync(t, peak, baseScale, downDuration, easeDown, unscaled, ct);
        }

        public static Task ScaleAsync(Transform t, Vector3 from, Vector3 to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            t.localScale = from;
            return RunAsync(t, duration, ease, unscaled, ct, null, e =>
            {
                t.localScale = Vector3.LerpUnclamped(from, to, e);
            });
        }

        public static Task MoveLocalAsync(Transform t, Vector3 from, Vector3 to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            t.localPosition = from;
            return RunAsync(t, duration, ease, unscaled, ct, null, e =>
            {
                t.localPosition = Vector3.LerpUnclamped(from, to, e);
            });
        }

        public static Task RotateLocalAsync(Transform t, Quaternion from, Quaternion to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            t.localRotation = from;
            return RunAsync(t, duration, ease, unscaled, ct, null, e =>
            {
                t.localRotation = Quaternion.SlerpUnclamped(from, to, e);
            });
        }

        public static Task FadeCanvasGroupAsync(CanvasGroup cg, float from, float to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            cg.alpha = from;
            return RunAsync(cg, duration, ease, unscaled, ct, null, e =>
            {
                cg.alpha = Mathf.LerpUnclamped(from, to, e);
            });
        }

        public static Task FadeGraphicAsync(Graphic g, float fromA, float toA, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            return RunAsync(g, duration, ease, unscaled, ct, null, e =>
            {
                Color c = g.color;
                c.a = Mathf.LerpUnclamped(fromA, toA, e);
                g.color = c;
            });
        }

        public static Task ColorGraphicAsync(Graphic g, Color from, Color to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            g.color = from;
            return RunAsync(g, duration, ease, unscaled, ct, null, e =>
            {
                g.color = Color.LerpUnclamped(from, to, e);
            });
        }

        public static Task ColorSpriteAsync(SpriteRenderer sr, Color from, Color to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            sr.color = from;
            return RunAsync(sr, duration, ease, unscaled, ct, null, e =>
            {
                sr.color = Color.LerpUnclamped(from, to, e);
            });
        }
    }
}
