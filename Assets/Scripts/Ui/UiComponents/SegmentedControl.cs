using System;
using System.Collections.Generic;
using System.Threading;
using UiFramework.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // A row of tabs with a pill that slides (and stretches a little while it moves) to the selected one.
    // The indicator and the segment buttons must share a parent and a centre anchor.
    public class SegmentedControl : MonoBehaviour
    {
        [SerializeField] private List<Button> segments = new List<Button>();
        [SerializeField] private List<Text> labels = new List<Text>();
        [SerializeField] private RectTransform indicator;
        [SerializeField] private Color selectedLabelColor = Color.white;
        [SerializeField] private Color normalLabelColor = new Color(1f, 1f, 1f, 0.55f);
        [SerializeField] private float moveDuration = 0.32f;
        [Tooltip("Extra width at the middle of the move (0.3 = 30% wider).")]
        [SerializeField] private float stretch = 0.3f;

        private int selectedIndex = -1;
        private bool isListening;
        private float indicatorWidth;
        private int run;

        public event Action<int> SelectionChanged;

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
        }

        private void Awake()
        {
            Listen();
        }

        public void Select(int index, bool animate)
        {
            Listen();
            if (segments.Count == 0)
            {
                return;
            }

            selectedIndex = Mathf.Clamp(index, 0, segments.Count - 1);

            for (int i = 0; i < labels.Count; i++)
            {
                if (labels[i] != null)
                {
                    labels[i].color = i == selectedIndex ? selectedLabelColor : normalLabelColor;
                }
            }

            MoveIndicator(animate);
        }

        private void Listen()
        {
            if (isListening)
            {
                return;
            }

            isListening = true;
            if (indicator != null)
            {
                indicatorWidth = indicator.sizeDelta.x;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                int index = i;
                if (segments[i] != null)
                {
                    segments[i].onClick.AddListener(() =>
                    {
                        OnSegmentClicked(index);
                    });
                }
            }
        }

        private void OnSegmentClicked(int index)
        {
            if (index == selectedIndex)
            {
                return;
            }

            Select(index, true);

            if (SelectionChanged != null)
            {
                SelectionChanged(index);
            }
        }

        private void MoveIndicator(bool animate)
        {
            RectTransform segment = segments[selectedIndex] != null ? segments[selectedIndex].transform as RectTransform : null;
            if (indicator == null || segment == null)
            {
                return;
            }

            int currentRun = ++run;
            float fromX = indicator.anchoredPosition.x;
            float toX = segment.anchoredPosition.x;
            float y = indicator.anchoredPosition.y;

            if (!animate)
            {
                indicator.anchoredPosition = new Vector2(toX, y);
                indicator.sizeDelta = new Vector2(indicatorWidth, indicator.sizeDelta.y);
                return;
            }

            ShowcaseTween.Forget(Tween.RunAsync(indicator, moveDuration, TweenCurves.Linear, true, CancellationToken.None,
                () => run == currentRun,
                progress =>
                {
                    float eased = TweenCurves.EaseOut.Evaluate(progress);
                    float widthFactor = 1f + stretch * Mathf.Sin(progress * Mathf.PI);
                    indicator.anchoredPosition = new Vector2(Mathf.LerpUnclamped(fromX, toX, eased), y);
                    indicator.sizeDelta = new Vector2(indicatorWidth * widthFactor, indicator.sizeDelta.y);
                }));
        }
    }
}
