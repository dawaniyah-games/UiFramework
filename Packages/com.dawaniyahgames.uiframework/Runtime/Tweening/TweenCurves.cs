using UnityEngine;

namespace UiFramework.Tweening
{
    // Easing curves. Keyframe tangents are chosen so EaseOut and EaseIn are exact cubics:
    // EaseOut = 1 - (1 - t)^3 (starts fast, settles gently: use for things that appear),
    // EaseIn  = t^3           (starts gently, speeds up: use for things that leave).
    public static class TweenCurves
    {
        public static readonly AnimationCurve Linear =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public static readonly AnimationCurve EaseIn =
            new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(1f, 1f, 3f, 0f));

        public static readonly AnimationCurve EaseOut =
            new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 3f),
                new Keyframe(1f, 1f, 0f, 0f));

        // Smooth start and end with no pause in the middle.
        public static readonly AnimationCurve EaseInOut =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // Overshooting curves, kept for existing callers. The built-in presets don't use them: wobble reads as
        // motion sickness on full-screen UI.
        public static readonly AnimationCurve ElasticOut =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.4f, 1.2f),
                new Keyframe(0.7f, 0.9f),
                new Keyframe(1f, 1f));

        public static readonly AnimationCurve BounceOut =
            new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.55f, 1.05f),
                new Keyframe(0.75f, 0.95f),
                new Keyframe(0.9f, 1.02f),
                new Keyframe(1f, 1f));
    }
}
