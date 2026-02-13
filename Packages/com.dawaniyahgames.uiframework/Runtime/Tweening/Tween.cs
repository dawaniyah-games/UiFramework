using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UiFramework.Tweening
{
    public static class Tween
    {
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

        public static async Task ScaleAsync(Transform t, Vector3 from, Vector3 to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                t.localScale = to;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;
            t.localScale = from;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                t.localScale = Vector3.LerpUnclamped(from, to, e);
                await Task.Yield();
            }

            t.localScale = to;
        }

        public static async Task MoveLocalAsync(Transform t, Vector3 from, Vector3 to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                t.localPosition = to;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;
            t.localPosition = from;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                t.localPosition = Vector3.LerpUnclamped(from, to, e);
                await Task.Yield();
            }

            t.localPosition = to;
        }

        public static async Task RotateLocalAsync(Transform t, Quaternion from, Quaternion to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                t.localRotation = to;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;
            t.localRotation = from;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                t.localRotation = Quaternion.SlerpUnclamped(from, to, e);
                await Task.Yield();
            }

            t.localRotation = to;
        }

        public static async Task FadeCanvasGroupAsync(CanvasGroup cg, float from, float to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                cg.alpha = to;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;
            cg.alpha = from;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                cg.alpha = Mathf.LerpUnclamped(from, to, e);
                await Task.Yield();
            }

            cg.alpha = to;
        }

        public static async Task FadeGraphicAsync(Graphic g, float fromA, float toA, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                Color c0 = g.color;
                c0.a = toA;
                g.color = c0;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                Color c = g.color;
                c.a = Mathf.LerpUnclamped(fromA, toA, e);
                g.color = c;
                await Task.Yield();
            }

            Color final = g.color;
            final.a = toA;
            g.color = final;
        }

        public static async Task ColorGraphicAsync(Graphic g, Color from, Color to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                g.color = to;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;
            g.color = from;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                g.color = Color.LerpUnclamped(from, to, e);
                await Task.Yield();
            }

            g.color = to;
        }

        public static async Task ColorSpriteAsync(SpriteRenderer sr, Color from, Color to, float duration,
            AnimationCurve ease, bool unscaled, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                sr.color = to;
                return;
            }

            float start = unscaled ? Time.unscaledTime : Time.time;
            float end = start + duration;
            sr.color = from;

            while ((unscaled ? Time.unscaledTime : Time.time) < end)
            {
                ct.ThrowIfCancellationRequested();
                float now = unscaled ? Time.unscaledTime : Time.time;
                float a = Mathf.InverseLerp(start, end, now);
                float e = ease != null ? ease.Evaluate(a) : a;
                sr.color = Color.LerpUnclamped(from, to, e);
                await Task.Yield();
            }

            sr.color = to;
        }
    }
}
