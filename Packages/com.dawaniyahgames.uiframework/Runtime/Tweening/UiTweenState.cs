using System;
using UnityEngine;

namespace UiFramework.Tweening
{
    // What an animation drives. A new animation only stops older ones that use the same channels, so for example a
    // button pop (Scale) doesn't interrupt a slide (Position).
    [Flags]
    public enum UiTweenChannels
    {
        None = 0,
        Position = 1,
        Scale = 2,
        Alpha = 4,
        All = Position | Scale | Alpha
    }

    // Remembers an object's resting pose (recorded the first time it animates) and which animation currently owns
    // each channel.
    public sealed class UiTweenState : MonoBehaviour
    {
        [SerializeField] private bool initialized;
        [SerializeField] private Vector3 baseLocalPosition;
        [SerializeField] private Vector3 baseLocalScale;
        [SerializeField] private Vector2 baseAnchoredPosition;
        [SerializeField] private float baseAlpha = 1f;

        // Runtime-only bookkeeping (not serialized).
        private int lastRun;
        private int positionRun;
        private int scaleRun;
        private int alphaRun;
        private bool shaking;
        private Vector3 shakeOrigin;

        public bool Initialized
        {
            get
            {
                return initialized;
            }
        }

        public Vector3 BaseLocalPosition
        {
            get
            {
                return baseLocalPosition;
            }
        }

        public Vector3 BaseLocalScale
        {
            get
            {
                return baseLocalScale;
            }
        }

        public Vector2 BaseAnchoredPosition
        {
            get
            {
                return baseAnchoredPosition;
            }
        }

        public float BaseAlpha
        {
            get
            {
                return baseAlpha;
            }
        }

        public void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            baseLocalPosition = transform.localPosition;
            baseLocalScale = transform.localScale;

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null)
            {
                baseAnchoredPosition = rectTransform.anchoredPosition;
            }
            else
            {
                baseAnchoredPosition = Vector2.zero;
            }

            CanvasGroup group = GetComponent<CanvasGroup>();
            baseAlpha = group != null ? group.alpha : 1f;

            initialized = true;
        }

        // Claims the given channels for a new animation; older animations on those channels stop on their next frame.
        public int BeginAnimation(UiTweenChannels channels)
        {
            return BeginAnimation(channels, false);
        }

        public int BeginAnimation(UiTweenChannels channels, bool isShake)
        {
            lastRun++;

            if ((channels & UiTweenChannels.Position) != 0)
            {
                positionRun = lastRun;
                if (!isShake)
                {
                    shaking = false;
                }
            }

            if ((channels & UiTweenChannels.Scale) != 0)
            {
                scaleRun = lastRun;
            }

            if ((channels & UiTweenChannels.Alpha) != 0)
            {
                alphaRun = lastRun;
            }

            return lastRun;
        }

        public bool IsCurrent(int run, UiTweenChannels channels)
        {
            if ((channels & UiTweenChannels.Position) != 0 && positionRun != run)
            {
                return false;
            }

            if ((channels & UiTweenChannels.Scale) != 0 && scaleRun != run)
            {
                return false;
            }

            if ((channels & UiTweenChannels.Alpha) != 0 && alphaRun != run)
            {
                return false;
            }

            return true;
        }

        // A shake moves around its resting point. If a shake is re-triggered mid-shake it keeps the first resting
        // point instead of the current (offset) position.
        public Vector3 BeginShake(Vector3 currentLocalPosition)
        {
            if (!shaking)
            {
                shakeOrigin = currentLocalPosition;
                shaking = true;
            }

            return shakeOrigin;
        }

        public void EndShake()
        {
            shaking = false;
        }
    }
}
