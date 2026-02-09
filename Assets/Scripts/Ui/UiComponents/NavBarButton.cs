namespace UiFramework.Editor.Elements
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public enum NavBarState
    {
        MainMenu = 0,
        Store = 1
    }

    public class NavBarButtonClickedSignalParam
    {
        public NavBarState NewState { get; }

        public NavBarButtonClickedSignalParam(NavBarState newState)
        {
            NewState = newState;
        }
    }

    public class OnNavBarButtonClickedSignal
    {
        public NavBarButtonClickedSignalParam Parameter { get; private set; }

        private readonly Action<OnNavBarButtonClickedSignal> callback;

        public OnNavBarButtonClickedSignal(Action<OnNavBarButtonClickedSignal> callback)
        {
            this.callback = callback;
        }

        public void Dispatch(NavBarButtonClickedSignalParam param)
        {
            Parameter = param;

            if (callback != null)
            {
                callback(this);
            }
        }
    }

    public class NavBarButton : MonoBehaviour
    {
        [SerializeField] private NavBarState buttonState;
        [SerializeField] private Button button;
        [SerializeField] private Animator animator;

        private OnNavBarButtonClickedSignal onNavBarButtonClickedSignal;

        private bool isSelected;

        public void Init(NavBarState currentState, OnNavBarButtonClickedSignal onNavBarButtonClickedSignal)
        {
            this.onNavBarButtonClickedSignal = onNavBarButtonClickedSignal;

            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClicked);
                button.onClick.AddListener(OnButtonClicked);
            }

            if (currentState == buttonState)
            {
                Select();
            }
            else
            {
                DeSelect();
            }
        }

        public NavBarState GetButtonState()
        {
            return buttonState;
        }

        public void Select()
        {
            if (isSelected)
            {
                return;
            }

            isSelected = true;
            if (animator != null)
            {
                animator.ResetTrigger("DeSelected");
                animator.SetTrigger("Selected");
            }
        }

        public void DeSelect()
        {
            if (!isSelected)
            {
                return;
            }

            isSelected = false;
            if (animator != null)
            {
                animator.ResetTrigger("Selected");
                animator.SetTrigger("DeSelected");
            }
        }

        public bool IsSelected()
        {
            return isSelected;
        }

        private void OnButtonClicked()
        {
            if (onNavBarButtonClickedSignal == null)
            {
                return;
            }

            onNavBarButtonClickedSignal.Dispatch(new NavBarButtonClickedSignalParam(buttonState));
        }
    }
}
