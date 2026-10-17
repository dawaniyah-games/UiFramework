using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UiFramework.Tweening.Tests
{
    // Runs the real show / hide presets frame by frame on a 1000 x 2000 canvas.
    public class TweenPresetsPlayModeTests
    {
        private const float canvasWidth = 1000f;
        private const float canvasHeight = 2000f;
        private const float tolerance = 0.5f;
        private const int maxFrames = 3000;

        private readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i] != null)
                {
                    UnityEngine.Object.Destroy(created[i]);
                }
            }

            created.Clear();
        }

        // ---------- Slides: the name is the direction of travel, for show and for hide ----------

        [UnityTest]
        public IEnumerator SlideRight_MovesRight_OnShowAndOnHide()
        {
            yield return AssertSlide(TweenPresets.SlideRightKey, Vector2.right);
        }

        [UnityTest]
        public IEnumerator SlideLeft_MovesLeft_OnShowAndOnHide()
        {
            yield return AssertSlide(TweenPresets.SlideLeftKey, Vector2.left);
        }

        [UnityTest]
        public IEnumerator SlideUp_MovesUp_OnShowAndOnHide()
        {
            yield return AssertSlide(TweenPresets.SlideUpKey, Vector2.up);
        }

        [UnityTest]
        public IEnumerator SlideDown_MovesDown_OnShowAndOnHide()
        {
            yield return AssertSlide(TweenPresets.SlideDownKey, Vector2.down);
        }

        private IEnumerator AssertSlide(string preset, Vector2 direction)
        {
            RectTransform root = CreateContentRoot();
            float distance = direction.x != 0f ? canvasWidth : canvasHeight;
            Func<float> along = () => Vector2.Dot(root.anchoredPosition, direction);

            TweenPresets.PrepareShow(preset, root.gameObject);
            Assert.AreEqual(-distance, along(), tolerance, "Show must start just past the opposite edge.");

            List<float> showSamples = new List<float>();
            yield return Run(TweenPresets.PlayAsync(preset, root.gameObject, true), along, showSamples);
            AssertMonotonicIncreasing(showSamples, preset + " show");
            Assert.AreEqual(0f, along(), tolerance, "Show must end at the authored position.");

            List<float> hideSamples = new List<float>();
            yield return Run(TweenPresets.PlayAsync(preset, root.gameObject, false), along, hideSamples);
            AssertMonotonicIncreasing(hideSamples, preset + " hide");
            Assert.AreEqual(distance, along(), tolerance, "Hide must keep moving the same way and end just past the named edge.");
        }

        // ---------- Edge fade: every show fades in, every hide fades out ----------

        [UnityTest]
        public IEnumerator EveryShowAndHide_FadesInAndOut()
        {
            string[] presets =
            {
                TweenPresets.SlideRightKey, TweenPresets.SlideLeftKey, TweenPresets.SlideUpKey, TweenPresets.SlideDownKey,
                TweenPresets.ClickPopKey, TweenPresets.SettlePopKey, TweenPresets.ShakeKey, TweenPresets.ScaleKey, TweenPresets.FadeKey
            };

            for (int i = 0; i < presets.Length; i++)
            {
                RectTransform root = CreateContentRoot();
                string preset = presets[i];

                TweenPresets.PrepareShow(preset, root.gameObject);
                CanvasGroup group = root.GetComponent<CanvasGroup>();
                Assert.IsNotNull(group, preset + ": show must fade, so it needs a CanvasGroup.");
                Assert.AreEqual(0f, group.alpha, 0.0001f, preset + ": show starts transparent (no flash).");

                yield return Run(TweenPresets.PlayAsync(preset, root.gameObject, true), null, null);
                Assert.AreEqual(1f, group.alpha, 0.0001f, preset + ": show ends fully visible.");
                Assert.AreEqual(1f, root.localScale.x, 0.0001f, preset + ": show ends at full size.");

                yield return Run(TweenPresets.PlayAsync(preset, root.gameObject, false), null, null);
                Assert.AreEqual(0f, group.alpha, 0.0001f, preset + ": hide ends fully transparent.");
            }
        }

        [UnityTest]
        public IEnumerator SlideHide_StaysFullyVisibleWhileItStartsMoving()
        {
            RectTransform root = CreateContentRoot();
            TweenPresets.PrepareShow(TweenPresets.SlideLeftKey, root.gameObject);
            yield return Run(TweenPresets.PlayAsync(TweenPresets.SlideLeftKey, root.gameObject, true), null, null);

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            List<float> alpha = new List<float>();
            yield return Run(TweenPresets.PlayAsync(TweenPresets.SlideLeftKey, root.gameObject, false), () => -group.alpha, alpha);

            AssertMonotonicIncreasing(alpha, "hide alpha (never fades back in)");
            Assert.AreEqual(-1f, alpha[alpha.Count / 3], 0.0001f, "The fade only happens at the end of the hide.");
        }

        [UnityTest]
        public IEnumerator ClickPop_AsButtonFeedback_DoesNotFade()
        {
            RectTransform button = CreateContentRoot();

            yield return Run(TweenPresets.ClickPop(button), null, null);

            CanvasGroup group = button.GetComponent<CanvasGroup>();
            Assert.IsTrue(group == null || Mathf.Approximately(group.alpha, 1f), "Direct feedback calls must not fade the button.");
        }

        // ---------- Scale (subtle zoom + fade) ----------

        [UnityTest]
        public IEnumerator Scale_ShowEndsAtFullSize_AndRepeatedCyclesDontShrink()
        {
            RectTransform root = CreateContentRoot();

            for (int cycle = 0; cycle < 3; cycle++)
            {
                TweenPresets.PrepareShow(TweenPresets.ScaleKey, root.gameObject);
                Assert.Less(root.localScale.x, 1f, "Show starts slightly smaller.");

                yield return Run(TweenPresets.PlayAsync(TweenPresets.ScaleKey, root.gameObject, true), null, null);
                Assert.AreEqual(1f, root.localScale.x, 0.0001f, "Cycle " + cycle + ": show must end at the real size, not 0.9.");
                Assert.AreEqual(1f, root.GetComponent<CanvasGroup>().alpha, 0.0001f);

                yield return Run(TweenPresets.PlayAsync(TweenPresets.ScaleKey, root.gameObject, false), null, null);
                Assert.AreEqual(0f, root.GetComponent<CanvasGroup>().alpha, 0.0001f, "Hide fades out completely.");
            }
        }

        // ---------- Fade ----------

        [UnityTest]
        public IEnumerator Fade_ShowAndHide_AreSmoothAndComplete()
        {
            RectTransform root = CreateContentRoot();
            TweenPresets.PrepareShow(TweenPresets.FadeKey, root.gameObject);
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            Assert.AreEqual(0f, group.alpha, 0.0001f);

            List<float> showSamples = new List<float>();
            yield return Run(TweenPresets.PlayAsync(TweenPresets.FadeKey, root.gameObject, true), () => group.alpha, showSamples);
            AssertMonotonicIncreasing(showSamples, "fade in");
            Assert.AreEqual(1f, group.alpha, 0.0001f);

            List<float> hideSamples = new List<float>();
            yield return Run(TweenPresets.PlayAsync(TweenPresets.FadeKey, root.gameObject, false), () => -group.alpha, hideSamples);
            AssertMonotonicIncreasing(hideSamples, "fade out");
            Assert.AreEqual(0f, group.alpha, 0.0001f);
        }

        // ---------- Interruptions ----------

        [UnityTest]
        public IEnumerator NewShow_StopsARunningHide_AndRestoresTheLayout()
        {
            RectTransform root = CreateContentRoot();
            Task hide = TweenPresets.PlayAsync(TweenPresets.SlideRightKey, root.gameObject, false);

            yield return null;
            yield return null;
            yield return null;

            TweenPresets.PrepareShow(TweenPresets.FadeKey, root.gameObject);
            Task show = TweenPresets.PlayAsync(TweenPresets.FadeKey, root.gameObject, true);

            yield return Run(Task.WhenAll(hide, show), null, null);

            Assert.AreEqual(0f, root.anchoredPosition.x, tolerance, "The interrupted slide must not keep moving the element.");
            Assert.AreEqual(1f, root.GetComponent<CanvasGroup>().alpha, 0.0001f);
        }

        [UnityTest]
        public IEnumerator ClickPop_RetriggeredMidPop_ReturnsToRestScale()
        {
            RectTransform button = CreateContentRoot();
            List<float> samples = new List<float>();

            Task first = TweenPresets.ClickPop(button);
            yield return null;
            yield return null;
            Task second = TweenPresets.ClickPop(button);

            yield return Run(Task.WhenAll(first, second), () => button.localScale.x, samples);

            Assert.AreEqual(1f, button.localScale.x, 0.0001f, "Spamming the pop must not grow the button.");
            Assert.LessOrEqual(Max(samples), 1.081f);
        }

        [UnityTest]
        public IEnumerator Shake_RetriggeredMidShake_ReturnsToRestPosition()
        {
            RectTransform target = CreateContentRoot();
            Vector3 rest = target.localPosition;

            Task first = TweenPresets.ShakeLocal(target);
            yield return null;
            yield return null;
            Task second = TweenPresets.ShakeLocal(target);

            yield return Run(Task.WhenAll(first, second), null, null);

            Assert.AreEqual(rest.x, target.localPosition.x, 0.0001f);
            Assert.AreEqual(rest.y, target.localPosition.y, 0.0001f);
        }

        // ---------- Curves ----------

        [Test]
        public void Curves_HaveTheShapeTheirNamesSay()
        {
            Assert.AreEqual(0.875f, TweenCurves.EaseOut.Evaluate(0.5f), 0.001f, "EaseOut = 1 - (1 - t)^3");
            Assert.AreEqual(0.125f, TweenCurves.EaseIn.Evaluate(0.5f), 0.001f, "EaseIn = t^3");

            float previous = 0f;
            for (int i = 1; i <= 100; i++)
            {
                float t = i / 100f;
                float value = TweenCurves.EaseInOut.Evaluate(t);
                Assert.Greater(value, previous - 0.00001f, "EaseInOut must never go backwards.");

                if (i > 5 && i < 95)
                {
                    Assert.Greater(value - previous, 0.0005f, "EaseInOut must not pause mid-way (t = " + t + ").");
                }

                previous = value;
            }
        }

        // ---------- Helpers ----------

        private RectTransform CreateContentRoot()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            ((RectTransform)canvasObject.transform).sizeDelta = new Vector2(canvasWidth, canvasHeight);
            created.Add(canvasObject);

            GameObject rootObject = new GameObject("Root", typeof(RectTransform));
            rootObject.transform.SetParent(canvasObject.transform, false);

            RectTransform root = (RectTransform)rootObject.transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            return root;
        }

        private static IEnumerator Run(Task task, Func<float> sample, List<float> samples)
        {
            int frames = 0;

            while (!task.IsCompleted)
            {
                if (sample != null && samples != null)
                {
                    samples.Add(sample());
                }

                frames++;
                Assert.Less(frames, maxFrames, "The animation never finished.");
                yield return null;
            }

            if (sample != null && samples != null)
            {
                samples.Add(sample());
            }

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }

        private static void AssertMonotonicIncreasing(List<float> samples, string what)
        {
            Assert.Greater(samples.Count, 2, what + ": expected several frames of motion.");

            for (int i = 1; i < samples.Count; i++)
            {
                Assert.GreaterOrEqual(samples[i], samples[i - 1] - 0.001f, what + ": moved backwards at frame " + i + ".");
            }
        }

        private static float Max(List<float> samples)
        {
            float max = float.MinValue;
            for (int i = 0; i < samples.Count; i++)
            {
                max = Mathf.Max(max, samples[i]);
            }

            return max;
        }
    }
}
