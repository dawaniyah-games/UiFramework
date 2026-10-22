# Episode 9 · Show states from code

| | |
| --- | --- |
| Length | about 3 min |
| Docs slot | Tutorial step 9 (`#t-code`), "Step 9: Show states from code" |
| Starts with | End of Episode 8 |
| Ends with | Press Play: the Main Menu appears; Store and the nav bar switch screens while the bars stay |
| Screenshot | none |

## Before you record

- [ ] Prepare the three scripts below in a text file so you can paste them; type only the highlighted lines on camera.
- [ ] Addressables **Play Mode Script** is still **Use Asset Database** (Episode 1).

## The code

`Assets/Scripts/GameBootstrap.cs`

```csharp
using System;
using CoinShop.Ui.States;
using UiFramework.Runtime.Manager;
using UnityEngine;

namespace CoinShop
{
    public class GameBootstrap : MonoBehaviour
    {
        // async void is fine for Unity messages, as long as exceptions are caught and logged.
        private async void Start()
        {
            try
            {
                await UiManager.InitializeAsync();
                await UiManager.ShowState<MainMenuUiState>();
            }
            catch (Exception exception)
            {
                Debug.LogError("[GameBootstrap] The UI failed to start. Check the Console for [UiManager] errors: " + exception);
            }
        }
    }
}
```

`MainMenuUiElement.cs` (add to the generated class)

```csharp
using System;
using CoinShop.Ui.States;
using UiFramework.Core;
using UiFramework.Runtime.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace CoinShop.Ui.Elements
{
    public class MainMenuUiElement : UiElement
    {
        [SerializeField] private Button storeButton;

        // UiElement.Awake finds the object to animate: override it and call base.Awake() first.
        protected override void Awake()
        {
            base.Awake();
            storeButton.onClick.AddListener(OnStoreClicked);
        }

        private async void OnStoreClicked()
        {
            try
            {
                await UiManager.ShowState<StoreUiState>();
            }
            catch (Exception exception)
            {
                Debug.LogError("[MainMenuUiElement] Could not open the store: " + exception);
            }
        }
    }
}
```

`NavBarUiElement.cs`: the same pattern with two buttons, `homeButton` → `ShowState<MainMenuUiState>()` and `storeButton` → `ShowState<StoreUiState>()`.

## Script

**1. A start-up scene** (0:00)
- SCREEN: File → New Scene (Basic). Save as `Assets/Scenes/Bootstrap.unity`. Add an empty object `GameBootstrap`. Check there is a camera; add GameObject → UI → **Event System**.
- SAY: "The game needs one scene that starts everything. A new scene called Bootstrap, with a camera and an event system for button clicks, and an empty object for our start-up script."

**2. Start the UI** (0:20)
- SCREEN: Create `GameBootstrap.cs`, paste the code, then type the two `await` lines on camera. Attach it to the `GameBootstrap` object.
- SAY: "Two lines start the UI. Initialize loads the config we built. Show State shows the main menu. That's it: you never put a UI manager in your scene, it creates itself."
- ON SCREEN: `await UiManager.ShowState<MainMenuUiState>();`

**3. First Play** (0:55)
- SCREEN: With `Bootstrap.unity` as the open scene (not an element scene), Game view 1080 × 1920, press **Play**. The Main Menu scales in with the two bars. In the Hierarchy, expand the loaded scenes: `MainMenuUiElement`, `TopBarUiElement`, `NavBarUiElement`. Stop.
- SAY: "Press Play. There's our menu, scaling in, with both bars. Look at the Hierarchy: each element was loaded as its own scene."

**4. Make the Store button work** (1:20)
- SCREEN: Open `MainMenuUiElement.cs`, paste the code, type the `ShowState<StoreUiState>()` line on camera. In the MainMenu scene, drag `StoreButton` into the new **Store Button** field on `MainMenuUiElementRoot`. Save.
- SAY: "Now the Store button. When it's clicked, we show the store state. One line again. Drag the button into the field, and save the scene."

**5. The nav bar** (1:55)
- SCREEN: Show `NavBarUiElement.cs` already pasted (same pattern, two buttons), wire `HomeButton` and `StoreButton`. Save.
- SAY: "The nav bar does the same with two buttons: Home shows the menu state, Store shows the store state."

**6. Play it** (2:15)
- SCREEN: Press **Play**. Tap **Store**: the store slides in, the menu leaves, the bars don't move. Tap **Home** in the nav bar, then **Store** again. Point at the Hierarchy: the bar scenes stay loaded; `MainMenuUiElement` unloads when you're in the store.
- SAY: "Press Play and tap Store. The store slides in, and the bars stay exactly where they are: they're shared, so they never reload. Home and back. Scenes load when a state needs them and unload when nothing uses them."

**7. Wrap up** (2:45)
- SAY: "In a bigger game, send these calls through one navigator class, as the showcase app does. Last step: a popup."
- ON SCREEN: `Next: Popups and overlays`

## After recording

- [ ] Commit: `Tutorial episode 9 end`.
