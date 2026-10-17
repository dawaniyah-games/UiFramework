using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UiFramework.Core;
using UiFramework.Tests.Support;
using UnityEngine;
using UnityEngine.TestTools;

namespace UiFramework.Tweening.Tests
{
    // A per-call UiTransition replaces an element's configured animation for that call only.
    public class UiTransitionPlayModeTests
    {
        private const float canvasWidth = 1000f;
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

        // ---------- UiTransition ----------

        [Test]
        public void Transition_ResolvesPresetNamesIgnoringCase()
        {
            UiTransition transition = new UiTransition("slideleft", "SCALE");

            Assert.AreEqual(TweenPresets.SlideLeftKey, transition.ShowPreset);
            Assert.AreEqual(TweenPresets.ScaleKey, transition.HidePreset);
        }

        [Test]
        public void Transition_EmptyOrUnknownPreset_KeepsTheElementsOwnAnimation()
        {
            LogAssert.Expect(LogType.Warning, new Regex("Unknown show animation 'Wobble'"));

            UiTransition unknown = UiTransition.ShowWith("Wobble");

            Assert.IsNull(unknown.ShowPreset);
            Assert.IsNull(UiTransition.HideWith("  ").HidePreset);
            Assert.IsNull(UiTransition.Default.ShowPreset);
            Assert.IsNull(UiTransition.Default.HidePreset);
        }

        [Test]
        public void Transition_ShowWithAndHideWith_SetOnlyOneSide()
        {
            Assert.AreEqual(TweenPresets.FadeKey, UiTransition.ShowWith(TweenPresets.FadeKey).ShowPreset);
            Assert.IsNull(UiTransition.ShowWith(TweenPresets.FadeKey).HidePreset);
            Assert.AreEqual(TweenPresets.FadeKey, UiTransition.HideWith(TweenPresets.FadeKey).HidePreset);
            Assert.IsNull(UiTransition.HideWith(TweenPresets.FadeKey).ShowPreset);
        }

        // ---------- UiElement with an override ----------

        [UnityTest]
        public IEnumerator Show_WithOverride_PlaysTheOverrideInsteadOfTheConfiguredPreset()
        {
            RectTransform root;
            TestUiElement element = CreateElement(TweenPresets.FadeKey, TweenPresets.FadeKey, out root);

            element.PrepareForShow(TweenPresets.SlideLeftKey);
            Assert.AreEqual(canvasWidth, root.anchoredPosition.x, tolerance, "SlideLeft starts past the right edge.");

            List<float> samples = new List<float>();
            yield return Run(element.ShowAsync(TweenPresets.SlideLeftKey), () => -root.anchoredPosition.x, samples);

            AssertMonotonicIncreasing(samples, "override show (moving left)");
            Assert.AreEqual(0f, root.anchoredPosition.x, tolerance);
        }

        [UnityTest]
        public IEnumerator Show_WithoutOverride_PlaysTheConfiguredPreset()
        {
            RectTransform root;
            TestUiElement element = CreateElement(TweenPresets.FadeKey, TweenPresets.FadeKey, out root);

            element.PrepareForShow(null);
            Assert.AreEqual(0f, root.GetComponent<CanvasGroup>().alpha, 0.0001f, "The configured Fade starts transparent.");
            Assert.AreEqual(0f, root.anchoredPosition.x, tolerance, "No slide was configured.");

            yield return Run(element.ShowAsync(), null, null);
            Assert.AreEqual(1f, root.GetComponent<CanvasGroup>().alpha, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Hide_WithOverride_PlaysTheOverrideInsteadOfTheConfiguredPreset()
        {
            RectTransform root;
            TestUiElement element = CreateElement(TweenPresets.FadeKey, TweenPresets.FadeKey, out root);

            yield return Run(element.HideAsync(TweenPresets.SlideRightKey), null, null);

            Assert.AreEqual(canvasWidth, root.anchoredPosition.x, tolerance, "SlideRight hide leaves past the right edge.");
        }

        // ---------- Helpers ----------

        // <Name>Root (TestUiElement) -> Canvas (1000 x 2000) -> stretched Root, with the given configured presets.
        private TestUiElement CreateElement(string showPreset, string hidePreset, out RectTransform root)
        {
            GameObject elementObject = new GameObject("TestUiElementRoot");
            created.Add(elementObject);

            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            ((RectTransform)canvasObject.transform).sizeDelta = new Vector2(canvasWidth, 2000f);
            canvasObject.transform.SetParent(elementObject.transform, false);

            GameObject rootObject = new GameObject("Root", typeof(RectTransform));
            rootObject.transform.SetParent(canvasObject.transform, false);
            root = (RectTransform)rootObject.transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            // Awake picks the animation target, so the hierarchy must exist before the component is added.
            TestUiElement element = elementObject.AddComponent<TestUiElement>();
            SetPreset(element, "showAnimationPreset", showPreset);
            SetPreset(element, "hideAnimationPreset", hidePreset);
            return element;
        }

        private static void SetPreset(UiElement element, string fieldName, string value)
        {
            FieldInfo field = typeof(UiElement).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, "UiElement." + fieldName + " not found.");
            field.SetValue(element, value);
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
    }
}
