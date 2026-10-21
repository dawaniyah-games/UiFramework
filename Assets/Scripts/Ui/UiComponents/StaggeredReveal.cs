using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;

namespace UiShowcase.Components
{
    // Reveals this object's active children one after another every time the element shows: lists, grids, rows of
    // stats. Works with children spawned in Populate too. Position the children yourself (no LayoutGroup), because a
    // layout pass would fight the motion.
    public class StaggeredReveal : MonoBehaviour, IUiShowEffect
    {
        [SerializeField] private RevealMotion motion = RevealMotion.FadeUp;
        [SerializeField] private float delay;
        [Tooltip("Seconds between one child and the next.")]
        [SerializeField] private float stagger = 0.06f;
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private float distance = 50f;

        private readonly List<Entry> entries = new List<Entry>();
        private int run;

        public void PrepareShowEffect()
        {
            run++;
            CollectChildren();
            ApplyAll(0f);
        }

        public async Task PlayShowEffectAsync()
        {
            int currentRun = ++run;
            if (entries.Count == 0)
            {
                CollectChildren();
            }

            ApplyAll(0f);

            Func<bool> isCurrent = () => run == currentRun;
            List<Task> tasks = new List<Task>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                tasks.Add(PlayEntryAsync(entries[i], delay + i * stagger, isCurrent));
            }

            await Task.WhenAll(tasks);
        }

        private async Task PlayEntryAsync(Entry entry, float startDelay, Func<bool> isCurrent)
        {
            await ShowcaseTween.DelayAsync(this, startDelay, isCurrent);
            if (!isCurrent() || entry.Target == null)
            {
                return;
            }

            await Tween.RunAsync(entry.Target, duration, TweenCurves.Linear, true, CancellationToken.None, isCurrent, progress =>
            {
                Apply(entry, progress);
            });
        }

        private void CollectChildren()
        {
            // Put known children back first, so a child caught mid-animation is never recorded at an offset pose.
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                if (entry.Target != null)
                {
                    entry.Target.anchoredPosition = entry.BasePosition;
                    entry.Target.localScale = entry.BaseScale;
                }
            }

            List<Entry> previous = new List<Entry>(entries);
            entries.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = transform.GetChild(i) as RectTransform;
                if (child == null || !child.gameObject.activeSelf)
                {
                    continue;
                }

                Entry entry = FindEntry(previous, child);
                if (entry == null)
                {
                    entry = new Entry(child, ShowcaseTween.GetOrAddCanvasGroup(child.gameObject), child.anchoredPosition, child.localScale);
                }

                entries.Add(entry);
            }
        }

        private void ApplyAll(float progress)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Apply(entries[i], progress);
            }
        }

        private void Apply(Entry entry, float progress)
        {
            RevealPose.Apply(entry.Target, entry.Group, entry.BasePosition, entry.BaseScale, motion, distance, progress);
        }

        private static Entry FindEntry(List<Entry> list, RectTransform target)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Target == target)
                {
                    return list[i];
                }
            }

            return null;
        }

        private sealed class Entry
        {
            public Entry(RectTransform target, CanvasGroup group, Vector2 basePosition, Vector3 baseScale)
            {
                Target = target;
                Group = group;
                BasePosition = basePosition;
                BaseScale = baseScale;
            }

            public RectTransform Target { get; }
            public CanvasGroup Group { get; }
            public Vector2 BasePosition { get; }
            public Vector3 BaseScale { get; }
        }
    }
}
