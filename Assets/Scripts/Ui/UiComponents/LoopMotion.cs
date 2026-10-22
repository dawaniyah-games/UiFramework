using UnityEngine;

namespace UiShowcase.Components
{
    public enum LoopMode
    {
        Pulse = 0,
        Float = 1,
        Spin = 2,
        Sway = 3,
        Breathe = 4
    }

    // Idle motion that runs while the object is active: a pulsing button glow, a floating icon, spinning rays.
    // Pick the mode, amplitude and speed in the Inspector. Stack several on one object when they drive different
    // things (Pulse = scale, Float = position, Spin / Sway = rotation, Breathe = alpha).
    // Put it on a child of anything that a RevealOnShow, StaggeredReveal or PressFeedback moves, not on the same object.
    public class LoopMotion : MonoBehaviour
    {
        [SerializeField] private LoopMode mode = LoopMode.Pulse;
        [Tooltip("Pulse: extra scale (0.05 = 5%). Float: pixels. Sway: degrees. Breathe: how far alpha dips (0..1). Spin: unused.")]
        [SerializeField] private float amplitude = 0.05f;
        [Tooltip("Cycles per second. Spin: degrees per second (negative turns clockwise).")]
        [SerializeField] private float speed = 0.5f;
        [Tooltip("Offsets the cycle (0..1) so neighbours don't move in lockstep.")]
        [SerializeField] private float phase;

        private RectTransform rectTransform;
        private CanvasGroup group;
        private Vector2 basePosition;
        private Vector3 baseScale;
        private Quaternion baseRotation;
        private float baseAlpha;

        private void OnEnable()
        {
            rectTransform = transform as RectTransform;
            baseScale = transform.localScale;
            baseRotation = transform.localRotation;

            if (rectTransform != null)
            {
                basePosition = rectTransform.anchoredPosition;
            }

            if (mode == LoopMode.Breathe)
            {
                group = ShowcaseTween.GetOrAddCanvasGroup(gameObject);
                baseAlpha = group.alpha;
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            float wave = Mathf.Sin((time * speed + phase) * 2f * Mathf.PI);

            switch (mode)
            {
                case LoopMode.Pulse:
                    transform.localScale = baseScale * (1f + amplitude * wave);
                    break;
                case LoopMode.Float:
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = basePosition + new Vector2(0f, amplitude * wave);
                    }

                    break;
                case LoopMode.Spin:
                    transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Repeat(time * speed + phase * 360f, 360f));
                    break;
                case LoopMode.Sway:
                    transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, amplitude * wave);
                    break;
                case LoopMode.Breathe:
                    if (group != null)
                    {
                        group.alpha = baseAlpha * (1f - amplitude * (0.5f + 0.5f * wave));
                    }

                    break;
            }
        }

        private void OnDisable()
        {
            transform.localScale = baseScale;
            transform.localRotation = baseRotation;

            if (rectTransform != null && mode == LoopMode.Float)
            {
                rectTransform.anchoredPosition = basePosition;
            }

            if (group != null)
            {
                group.alpha = baseAlpha;
            }
        }
    }
}
