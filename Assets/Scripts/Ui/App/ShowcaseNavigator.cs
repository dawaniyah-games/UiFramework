using System;
using System.Threading.Tasks;
using UiFramework.Core;
using UiFramework.Runtime.Manager;
using UiFramework.Tweening;
using UiShowcase.States;
using UnityEngine;

namespace UiShowcase.App
{
    // The one place the showcase changes UI state. It shows how an app typically sits on top of UiManager:
    // - Calls run one at a time. UiManager does not serialize overlapping ShowState / HideUI calls (DESIGN.md §11), so
    //   every request waits for the previous one to finish.
    // - Tab requests coalesce: tapping three tabs quickly shows only the last one.
    // - Tab switches pass a UiTransition so screens slide in the direction of the tab, whatever their own presets are.
    // - Popups are additive states pushed on top of the current tab and closed with HideUI.
    public static class ShowcaseNavigator
    {
        private static Task queue = Task.CompletedTask;
        private static ShowcaseTab currentTab;
        private static bool hasTab;
        private static int tabRequestVersion;
        private static int openPopups;

        public static event Action<ShowcaseTab> TabShown;

        public static bool HasOpenPopup
        {
            get
            {
                return openPopups > 0;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            queue = Task.CompletedTask;
            hasTab = false;
            tabRequestVersion = 0;
            openPopups = 0;
            TabShown = null;
        }

        public static Task ShowIntroAsync()
        {
            return EnqueueAsync(ShowIntroInternalAsync);
        }

        // Leaves the intro for the Home tab. No UiTransition: every element plays its own configured animation, so
        // the TopBar slides down, the NavBar slides up and Home zooms in at the same time.
        public static Task EnterAppAsync()
        {
            return ShowTabAsync(ShowcaseTab.Home, null);
        }

        public static Task ShowTabAsync(ShowcaseTab tab)
        {
            return ShowTabAsync(tab, null);
        }

        public static Task ShowTabAsync(ShowcaseTab tab, TabUiParams context)
        {
            int version = ++tabRequestVersion;
            return EnqueueAsync(() => ShowTabInternalAsync(tab, context, version));
        }

        // Pushes T on top of the current state. The screens below stay loaded and visible.
        // One popup at a time: a second request (a double tap) is ignored. Pushing the same popup state twice would
        // stack two states on one shared scene, and closing the top one would hide the scene the other still uses.
        public static Task OpenPopupAsync<T>(object context) where T : UiState
        {
            return EnqueueAsync(async () =>
            {
                if (openPopups > 0)
                {
                    return;
                }

                await UiManager.ShowState<T>(context, true);
                openPopups++;
            });
        }

        public static Task ClosePopupAsync()
        {
            return EnqueueAsync(async () =>
            {
                if (openPopups <= 0)
                {
                    return;
                }

                openPopups--;
                await UiManager.HideUI();
            });
        }

        // Back to the intro, replacing the whole stack (tabs, TopBar, NavBar and any popup).
        public static Task RestartAsync()
        {
            return EnqueueAsync(ShowIntroInternalAsync);
        }

        private static async Task ShowIntroInternalAsync()
        {
            hasTab = false;
            openPopups = 0;
            await UiManager.ShowState<IntroUiState>(null, false);
        }

        private static async Task ShowTabInternalAsync(ShowcaseTab tab, TabUiParams context, int version)
        {
            // A newer tab request is queued behind this one; skip straight to it.
            if (version != tabRequestVersion)
            {
                return;
            }

            if (hasTab && tab == currentTab && openPopups == 0 && context == null)
            {
                return;
            }

            TabUiParams tabContext = context != null ? context : new TabUiParams();
            tabContext.Tab = tab;

            UiTransition transition = hasTab ? GetTabTransition(currentTab, tab) : UiTransition.Default;

            switch (tab)
            {
                case ShowcaseTab.Store:
                    await UiManager.ShowState<StoreUiState>(tabContext, false, transition);
                    break;
                case ShowcaseTab.Leaderboard:
                    await UiManager.ShowState<LeaderboardUiState>(tabContext, false, transition);
                    break;
                case ShowcaseTab.Profile:
                    await UiManager.ShowState<ProfileUiState>(tabContext, false, transition);
                    break;
                default:
                    await UiManager.ShowState<MainMenuUiState>(tabContext, false, transition);
                    break;
            }

            // A non-additive show replaces the whole stack, so any popup that was open is gone too.
            currentTab = tab;
            hasTab = true;
            openPopups = 0;

            if (TabShown != null)
            {
                TabShown(tab);
            }
        }

        // Moving to a tab on the right slides everything left (like swiping), and the other way round.
        // The shared TopBar and NavBar stay on screen and are not animated by the transition.
        private static UiTransition GetTabTransition(ShowcaseTab from, ShowcaseTab to)
        {
            // Same tab again (e.g. the Store with a different page): its scene is shared, so nothing animates anyway.
            if (from == to)
            {
                return UiTransition.Default;
            }

            string preset = (int)to > (int)from ? TweenPresets.SlideLeftKey : TweenPresets.SlideRightKey;
            return new UiTransition(preset, preset);
        }

        private static Task EnqueueAsync(Func<Task> operation)
        {
            Task next = RunAfterAsync(queue, operation);
            queue = next;
            return next;
        }

        private static async Task RunAfterAsync(Task previous, Func<Task> operation)
        {
            // previous never faults: every queued operation catches its own exceptions below.
            await previous;

            try
            {
                await operation();
            }
            catch (Exception exception)
            {
                Debug.LogError("[ShowcaseNavigator] Navigation failed: " + exception);
            }
        }
    }
}
