using System;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;

namespace UiShowcase.Components
{
    // Small helpers on top of the framework's Tween.RunAsync, shared by the showcase components.
    public static class ShowcaseTween
    {
        // Waits on unscaled time. Ends early when owner is destroyed or keepRunning turns false, so check your own
        // "still current" condition after awaiting it.
        public static Task DelayAsync(UnityEngine.Object owner, float seconds, Func<bool> keepRunning)
        {
            if (seconds <= 0f)
            {
                return Task.CompletedTask;
            }

            return Tween.RunAsync(owner, seconds, TweenCurves.Linear, true, CancellationToken.None, keepRunning, NoOp);
        }

        // Runs a task without awaiting it (button handlers, effects that outlive their trigger) and logs failures,
        // so exceptions are never lost.
        public static async void Forget(Task task)
        {
            if (task == null)
            {
                return;
            }

            try
            {
                await task;
            }
            catch (Exception exception)
            {
                Debug.LogError("[ShowcaseTween] Background task failed: " + exception);
            }
        }

        public static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = target.AddComponent<CanvasGroup>();
            }

            return group;
        }

        private static void NoOp(float progress)
        {
        }
    }

    public static class ShowcaseCurves
    {
        // Ease-out that overshoots by about 7% and settles back. Only for small pieces (badges, icons, panels): the
        // framework presets never overshoot full-screen motion, and neither should you.
        public static readonly AnimationCurve BackOut =
            new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 3.5f),
                new Keyframe(0.62f, 1.07f, 0f, 0f),
                new Keyframe(1f, 1f, 0f, 0f));
    }
}
