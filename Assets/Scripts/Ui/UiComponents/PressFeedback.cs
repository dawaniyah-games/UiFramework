using System.Threading;
using UiFramework.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // Squeezes a button while it is held and pops it on release, using the framework's ClickPop preset. Both claim the
    // Scale channel of the same UiTweenState, so rapid taps never fight or drift.
    public class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("What scales. Empty: this object.")]
        [SerializeField] private Transform target;
        [SerializeField] private float pressedScale = 0.93f;
        [SerializeField] private float pressDuration = 0.08f;

        private Selectable selectable;
        private UiTweenState tweenState;

        private void Awake()
        {
            if (target == null)
            {
                target = transform;
            }

            selectable = GetComponent<Selectable>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable != null && !selectable.IsInteractable())
            {
                return;
            }

            UiTweenState state = GetTweenState();
            int run = state.BeginAnimation(UiTweenChannels.Scale);
            Vector3 from = target.localScale;
            Vector3 to = state.BaseLocalScale * pressedScale;

            ShowcaseTween.Forget(Tween.RunAsync(target, pressDuration, TweenCurves.EaseOut, true, CancellationToken.None,
                () => state != null && state.IsCurrent(run, UiTweenChannels.Scale),
                eased =>
                {
                    target.localScale = Vector3.LerpUnclamped(from, to, eased);
                }));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Always pop back, even if the button became non-interactable while it was held.
            GetTweenState();
            ShowcaseTween.Forget(TweenPresets.ClickPop(target));
        }

        private UiTweenState GetTweenState()
        {
            if (tweenState == null)
            {
                tweenState = target.GetComponent<UiTweenState>();
                if (tweenState == null)
                {
                    tweenState = target.gameObject.AddComponent<UiTweenState>();
                }
            }

            tweenState.EnsureInitialized();
            return tweenState;
        }
    }
}
