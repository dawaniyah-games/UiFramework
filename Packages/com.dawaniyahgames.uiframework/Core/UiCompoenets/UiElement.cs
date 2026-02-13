namespace UiFramework.Core
{
    using System.Threading.Tasks;
    using UnityEngine;
    using UiFramework.Tweening;

    public abstract class UiElement : MonoBehaviour, IUiElement
    {
        [SerializeField] private string showAnimationPreset;
        [SerializeField] private string hideAnimationPreset;
        private Transform animationTarget;

        protected virtual void Awake()
        {
            if (animationTarget != null)
            {
                return;
            }

            if (transform.childCount <= 0)
            {
                return;
            }

            Transform root = transform.GetChild(0);
            if (root == null)
            {
                return;
            }

            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas != null)
            {
                animationTarget = FindFirstNonCanvasDescendant(root);
                return;
            }

            animationTarget = root;
        }

        public virtual void Populate(object context = null) { }

        public virtual Task ShowAsync(string presetKey = null)
        {
            string key = !string.IsNullOrWhiteSpace(presetKey) ? presetKey : showAnimationPreset;
            return TweenPresets.PlayAsync(key, GetAnimationTargetGameObject(), true);
        }

        public virtual Task HideAsync(string presetKey = null)
        {
            string key = !string.IsNullOrWhiteSpace(presetKey) ? presetKey : hideAnimationPreset;
            return TweenPresets.PlayAsync(key, GetAnimationTargetGameObject(), false);
        }

        public virtual void PrepareForShow()
        {
            GameObject target = GetAnimationTargetGameObject();
            if (target == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(showAnimationPreset) || string.Equals(showAnimationPreset, TweenPresets.NoneKey, System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(showAnimationPreset, TweenPresets.FadeKey, System.StringComparison.OrdinalIgnoreCase))
            {
                CanvasGroup group = target.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = target.AddComponent<CanvasGroup>();
                }

                group.alpha = 0.0f;
                return;
            }

            if (string.Equals(showAnimationPreset, TweenPresets.ScaleKey, System.StringComparison.OrdinalIgnoreCase))
            {
                UiTweenState tweenState = GetOrCreateTweenState(target);
                Vector3 baseScale = tweenState.BaseLocalScale;
                target.transform.localScale = baseScale * 0.9f;
                return;
            }

            if (string.Equals(showAnimationPreset, TweenPresets.SlideLeftKey, System.StringComparison.OrdinalIgnoreCase))
            {
                PrepareSlideForShow(target, Vector2.left);
                return;
            }

            if (string.Equals(showAnimationPreset, TweenPresets.SlideRightKey, System.StringComparison.OrdinalIgnoreCase))
            {
                PrepareSlideForShow(target, Vector2.right);
                return;
            }

            if (string.Equals(showAnimationPreset, TweenPresets.SlideUpKey, System.StringComparison.OrdinalIgnoreCase))
            {
                PrepareSlideForShow(target, Vector2.up);
                return;
            }

            if (string.Equals(showAnimationPreset, TweenPresets.SlideDownKey, System.StringComparison.OrdinalIgnoreCase))
            {
                PrepareSlideForShow(target, Vector2.down);
                return;
            }
        }

        private GameObject GetAnimationTargetGameObject()
        {
            if (animationTarget == null)
            {
                return null;
            }

            return animationTarget.gameObject;
        }

        private static Transform FindFirstNonCanvasDescendant(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            int childCount = root.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.GetComponent<Canvas>() == null)
                {
                    return child;
                }

                Transform found = FindFirstNonCanvasDescendant(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static UiTweenState GetOrCreateTweenState(GameObject target)
        {
            UiTweenState state = target.GetComponent<UiTweenState>();
            if (state == null)
            {
                state = target.AddComponent<UiTweenState>();
            }

            state.EnsureInitialized();
            return state;
        }

        private static RectTransform FindRectTransformForAnimation(GameObject target)
        {
            RectTransform self = target.GetComponent<RectTransform>();
            if (self != null)
            {
                return self;
            }

            return target.GetComponentInChildren<RectTransform>(true);
        }

        private static float GetCanvasWidth(RectTransform rectTransform)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return 0.0f;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return 0.0f;
            }

            return canvasRect.rect.width;
        }

        private static float GetCanvasHeight(RectTransform rectTransform)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return 0.0f;
            }

            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return 0.0f;
            }

            return canvasRect.rect.height;
        }

        private static void PrepareSlideForShow(GameObject target, Vector2 direction)
        {
            RectTransform rectTransform = FindRectTransformForAnimation(target);
            UiTweenState tweenState = GetOrCreateTweenState(rectTransform != null ? rectTransform.gameObject : target);

            Vector2 offscreenDirection = -direction;

            if (rectTransform != null)
            {
                float canvasWidth = GetCanvasWidth(rectTransform);
                float canvasHeight = GetCanvasHeight(rectTransform);

                Vector2 basePos = tweenState.BaseAnchoredPosition;
                Vector2 offset;

                if (Mathf.Abs(offscreenDirection.x) > 0.0f)
                {
                    float w = canvasWidth;
                    if (w <= 0.0f)
                    {
                        w = Screen.width;
                    }

                    offset = new Vector2(offscreenDirection.x * w, 0.0f);
                }
                else
                {
                    float h = canvasHeight;
                    if (h <= 0.0f)
                    {
                        h = Screen.height;
                    }

                    offset = new Vector2(0.0f, offscreenDirection.y * h);
                }

                rectTransform.anchoredPosition = basePos + offset;
                return;
            }

            Vector3 baseLocal = tweenState.BaseLocalPosition;
            Vector3 localOffset = new Vector3(offscreenDirection.x, offscreenDirection.y, 0.0f) * 10.0f;
            target.transform.localPosition = baseLocal + localOffset;
        }
    }
}
