using System;
using System.Threading;
using UiFramework.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // An on / off switch: the knob slides, the track changes colour and the knob gives a small pulse.
    // Needs a raycast-target Graphic on this object (the track) to receive clicks.
    public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image track;
        [SerializeField] private RectTransform knob;
        [SerializeField] private Color onColor = Color.green;
        [SerializeField] private Color offColor = Color.gray;
        [Tooltip("Knob x offset from the centre when on (and minus this when off).")]
        [SerializeField] private float knobTravel = 28f;
        [SerializeField] private float duration = 0.22f;
        [SerializeField] private bool isOn;

        private float shown;
        private int run;

        public event Action<bool> Changed;

        public bool IsOn
        {
            get
            {
                return isOn;
            }
        }

        public void SetIsOn(bool value, bool animate)
        {
            isOn = value;
            int currentRun = ++run;
            float target = value ? 1f : 0f;

            if (!animate)
            {
                Apply(target);
                return;
            }

            float from = shown;
            ShowcaseTween.Forget(Tween.RunAsync(this, duration, TweenCurves.EaseOut, true, CancellationToken.None,
                () => run == currentRun,
                eased =>
                {
                    Apply(Mathf.LerpUnclamped(from, target, eased));
                }));

            if (knob != null)
            {
                ShowcaseTween.Forget(TweenPresets.SettlePop(knob));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SetIsOn(!isOn, true);

            if (Changed != null)
            {
                Changed(isOn);
            }
        }

        private void Apply(float amount)
        {
            shown = amount;

            if (knob != null)
            {
                knob.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-knobTravel, knobTravel, amount), knob.anchoredPosition.y);
            }

            if (track != null)
            {
                track.color = Color.LerpUnclamped(offColor, onColor, amount);
            }
        }
    }
}
