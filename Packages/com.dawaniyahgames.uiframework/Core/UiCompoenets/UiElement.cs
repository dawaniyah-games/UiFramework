using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;

namespace UiFramework.Core
{
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

        public virtual void Populate(object context = null)
        {
        }

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
            // The start poses live in TweenPresets next to the animations, so show and prepare always agree.
            TweenPresets.PrepareShow(showAnimationPreset, GetAnimationTargetGameObject());
        }

        // Prepares for a show that plays presetKey instead of the configured animation (a per-call UiTransition).
        // UiManager calls the parameterless overload when there is no override, so existing overrides keep working.
        public virtual void PrepareForShow(string presetKey)
        {
            if (string.IsNullOrWhiteSpace(presetKey))
            {
                PrepareForShow();
                return;
            }

            TweenPresets.PrepareShow(presetKey, GetAnimationTargetGameObject());
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
    }
}
