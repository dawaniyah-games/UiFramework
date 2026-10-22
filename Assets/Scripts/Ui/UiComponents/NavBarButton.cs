using System;
using System.Threading;
using UiFramework.Tweening;
using UiShowcase.App;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // One NavBar tab: the selected icon lifts, grows and lights up; the others dim.
    public class NavBarButton : MonoBehaviour
    {
        [SerializeField] private ShowcaseTab tab;
        [SerializeField] private Button button;
        [SerializeField] private RectTransform icon;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text label;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float selectedLift = 10f;
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float duration = 0.3f;

        private Vector2 iconBasePosition;
        private bool hasBasePose;
        private float shown;
        private int run;

        public event Action<ShowcaseTab> Clicked;

        public ShowcaseTab Tab
        {
            get
            {
                return tab;
            }
        }

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }

            CaptureBasePose();
        }

        public void SetSelected(bool selected, bool animate)
        {
            CaptureBasePose();
            int currentRun = ++run;
            float target = selected ? 1f : 0f;

            if (!animate)
            {
                Apply(target);
                return;
            }

            float from = shown;
            ShowcaseTween.Forget(Tween.RunAsync(this, duration, TweenCurves.Linear, true, CancellationToken.None,
                () => run == currentRun,
                progress =>
                {
                    // Selecting overshoots a touch, deselecting just settles.
                    AnimationCurve curve = selected ? ShowcaseCurves.BackOut : TweenCurves.EaseOut;
                    Apply(Mathf.LerpUnclamped(from, target, curve.Evaluate(progress)));
                }));
        }

        private void CaptureBasePose()
        {
            if (hasBasePose || icon == null)
            {
                return;
            }

            iconBasePosition = icon.anchoredPosition;
            hasBasePose = true;
        }

        private void Apply(float amount)
        {
            shown = amount;
            Color color = Color.LerpUnclamped(normalColor, selectedColor, Mathf.Clamp01(amount));

            if (icon != null)
            {
                icon.anchoredPosition = iconBasePosition + new Vector2(0f, selectedLift * amount);
                icon.localScale = Vector3.one * Mathf.LerpUnclamped(1f, selectedScale, amount);
            }

            if (iconImage != null)
            {
                iconImage.color = color;
            }

            if (label != null)
            {
                label.color = color;
            }
        }

        private void OnButtonClicked()
        {
            if (Clicked != null)
            {
                Clicked(tab);
            }
        }
    }
}
