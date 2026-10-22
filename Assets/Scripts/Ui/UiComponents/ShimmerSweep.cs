using UiFramework.Tweening;
using UnityEngine;

namespace UiShowcase.Components
{
    // Sweeps a light streak across this object every few seconds (the "shiny card" effect). Put a Mask or RectMask2D on
    // this object so the streak is clipped to it, and make the streak a direct child anchored at the centre.
    [RequireComponent(typeof(RectTransform))]
    public class ShimmerSweep : MonoBehaviour
    {
        [SerializeField] private RectTransform streak;
        [SerializeField] private float interval = 3.2f;
        [SerializeField] private float sweepDuration = 0.9f;
        [SerializeField] private float startDelay = 0.6f;

        private RectTransform area;
        private float enabledAt;

        private void OnEnable()
        {
            area = (RectTransform)transform;
            enabledAt = Time.unscaledTime;
            Place(0f);
        }

        private void Update()
        {
            float elapsed = Time.unscaledTime - enabledAt - startDelay;
            if (elapsed < 0f)
            {
                return;
            }

            float progress = Mathf.Clamp01(Mathf.Repeat(elapsed, interval) / sweepDuration);
            Place(TweenCurves.EaseInOut.Evaluate(progress));
        }

        private void Place(float progress)
        {
            if (streak == null)
            {
                return;
            }

            // Start and end fully outside the area, so the streak never pops in or out of view. A tilted streak reaches
            // further sideways than its width.
            float angle = streak.localEulerAngles.z * Mathf.Deg2Rad;
            float halfExtent = Mathf.Abs(Mathf.Cos(angle)) * streak.rect.width * 0.5f + Mathf.Abs(Mathf.Sin(angle)) * streak.rect.height * 0.5f;
            float reach = area.rect.width * 0.5f + halfExtent;
            streak.anchoredPosition = new Vector2(Mathf.LerpUnclamped(-reach, reach, progress), streak.anchoredPosition.y);
        }
    }
}
