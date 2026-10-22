using UiFramework.Core;

namespace UiShowcase.App
{
    // The bottom navigation tabs, in the order they appear. The order decides which way screens slide between tabs.
    public enum ShowcaseTab
    {
        Home = 0,
        Store = 1,
        Leaderboard = 2,
        Profile = 3
    }

    // Context for every tab state. UiManager hands the same object to every element of the state, so the shared
    // NavBar learns which tab is showing from the context the screen itself receives.
    public class TabUiParams : UiPopulationParams
    {
        public ShowcaseTab Tab { get; set; }
    }

    // A tab context can carry screen-specific data too: the TopBar's "+" opens the Store straight on the coins page.
    public class StoreUiParams : TabUiParams
    {
        public bool OpenCoinsPage { get; set; }
    }
}
