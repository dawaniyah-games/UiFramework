using UiFramework.Tweening;
using UnityEngine;

namespace UiShowcase.Components
{
    public enum RevealMotion
    {
        Fade = 0,
        FadeUp = 1,
        FadeDown = 2,
        SlideFromLeft = 3,
        SlideFromRight = 4,
        PopIn = 5,
        Drop = 6
    }

    // Pose maths shared by RevealOnShow and StaggeredReveal. progress is linear 0..1; each motion eases it itself.
    // Only PopIn touches scale, so a revealed button can still use PressFeedback.
    public static class RevealPose
    {
        private const float FadePortion = 0.6f;
        private const float PopStartScale = 0.55f;

        public static void Apply(RectTransform target, CanvasGroup group, Vector2 basePosition, Vector3 baseScale,
            RevealMotion motion, float distance, float progress)
        {
            if (target == null)
            {
                return;
            }

            float eased = TweenCurves.EaseOut.Evaluate(progress);
            Vector2 offset = GetStartOffset(motion, distance);

            if (offset != Vector2.zero)
            {
                float travel = motion == RevealMotion.Drop ? ShowcaseCurves.BackOut.Evaluate(progress) : eased;
                target.anchoredPosition = basePosition + offset * (1f - travel);
            }

            if (motion == RevealMotion.PopIn)
            {
                target.localScale = baseScale * Mathf.LerpUnclamped(PopStartScale, 1f, ShowcaseCurves.BackOut.Evaluate(progress));
            }

            if (group != null)
            {
                group.alpha = TweenCurves.EaseOut.Evaluate(Mathf.Clamp01(progress / FadePortion));
            }
        }

        private static Vector2 GetStartOffset(RevealMotion motion, float distance)
        {
            switch (motion)
            {
                case RevealMotion.FadeUp:
                    return new Vector2(0f, -distance);
                case RevealMotion.FadeDown:
                case RevealMotion.Drop:
                    return new Vector2(0f, distance);
                case RevealMotion.SlideFromLeft:
                    return new Vector2(-distance, 0f);
                case RevealMotion.SlideFromRight:
                    return new Vector2(distance, 0f);
                default:
                    return Vector2.zero;
            }
        }
    }
}
