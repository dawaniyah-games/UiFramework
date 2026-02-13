using UnityEngine;

namespace UiFramework.Tweening
{
    public sealed class UiTweenState : MonoBehaviour
    {
        [SerializeField] private bool initialized;
        [SerializeField] private Vector3 baseLocalPosition;
        [SerializeField] private Vector3 baseLocalScale;
        [SerializeField] private Vector2 baseAnchoredPosition;

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

            initialized = true;
        }
    }
}
