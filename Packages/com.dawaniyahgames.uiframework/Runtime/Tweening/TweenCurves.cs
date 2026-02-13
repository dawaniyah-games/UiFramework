using UnityEngine;

namespace UiFramework.Tweening
{
    public static class TweenCurves
    {
        public static readonly AnimationCurve Linear =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public static readonly AnimationCurve EaseIn =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public static readonly AnimationCurve EaseOut =
            new AnimationCurve(
                new Keyframe(0f, 0f, 2f, 0f),
                new Keyframe(1f, 1f, 0f, 0f));

        public static readonly AnimationCurve EaseInOut =
            new AnimationCurve(
                new Keyframe(0f, 0f, 2f, 0f),
                new Keyframe(0.5f, 0.5f, 0f, 0f),
                new Keyframe(1f, 1f, 0f, 2f));

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
