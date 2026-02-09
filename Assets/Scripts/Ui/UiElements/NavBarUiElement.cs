namespace UiFramework.Editor.Elements
{
    using System.Collections;
    using System.Collections.Generic;
    using UiFramework.Core;
    using UiFramework.Runtime.Manager;
    using UnityEngine;
    using UiFramework.Editor.States;
    
    // Auto-generated: UI Element
    public class NavBarUiElement : UiElement
    {
        [SerializeField] private List<NavBarButton> navButtons;

        private OnNavBarButtonClickedSignal onNavBarButtonClickedSignal;

        [SerializeField] private NavBarState defaultState = NavBarState.MainMenu;

        private NavBarState currentState;

        private bool applyQueued;
        private NavBarState pendingState;
        private bool initialized;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                Initialize();
            }
        }

        public void Initialize(List<NavBarButton> navButtonsOverride = null)
        {
            currentState = defaultState;
            pendingState = defaultState;

            if (navButtonsOverride != null && (navButtons == null || navButtons.Count == 0))
            {
                navButtons = navButtonsOverride;
            }

            if (navButtons == null || navButtons.Count == 0)
            {
                navButtons = new List<NavBarButton>(GetComponentsInChildren<NavBarButton>(true));
            }

            onNavBarButtonClickedSignal = new OnNavBarButtonClickedSignal(OnNavBarButtonClicked);

            for (int i = 0; i < navButtons.Count; i++)
            {
                NavBarButton btn = navButtons[i];
                if (btn == null)
                {
                    continue;
                }

                btn.Init(currentState, onNavBarButtonClickedSignal);
            }

            initialized = true;
        }

        private void OnNavBarButtonClicked(OnNavBarButtonClickedSignal signal)
        {
            pendingState = signal.Parameter.NewState;

            if (applyQueued)
            {
                return;
            }

            applyQueued = true;
            StartCoroutine(ApplyPendingSelectionEndOfFrame());
        }

        private IEnumerator ApplyPendingSelectionEndOfFrame()
        {
            yield return null;

            NavBarState previousState = currentState;
            currentState = pendingState;

            for (int i = 0; i < navButtons.Count; i++)
            {
                NavBarButton btn = navButtons[i];
                if (btn == null)
                {
                    continue;
                }

                if (btn.GetButtonState() == currentState)
                {
                    btn.Select();
                }
                else
                {
                    btn.DeSelect();
                }
            }

            if (previousState != currentState)
            {
                NavigateToState(currentState);
            }

            applyQueued = false;
        }

        private async void NavigateToState(NavBarState state)
        {
            if (!UiManager.IsInitialized)
            {
                Debug.LogWarning("[NavBarUiElement] UiManager not initialized. Navigation skipped.");
                return;
            }

            if (state == NavBarState.MainMenu)
            {
                if (!UiManager.IsLastSceneActive<MainMenuUiState>())
                {
                    await UiManager.ShowState<MainMenuUiState>(null, false);
                }
                return;
            }

            if (state == NavBarState.Store)
            {
                if (!UiManager.IsLastSceneActive<StoreUiState>())
                {
                    await UiManager.ShowState<StoreUiState>(null, false);
                }
                return;
            }
        }

        public void RequestState(NavBarState state)
        {
            pendingState = state;

            if (applyQueued)
            {
                return;
            }

            applyQueued = true;
            StartCoroutine(ApplyPendingSelectionEndOfFrame());
        }

        public void OnStoreButtonClicked()
        {
            RequestState(NavBarState.Store);
        }

        public void OnPlayButtonClicked()
        {
            RequestState(NavBarState.MainMenu);
        }
    }
}
