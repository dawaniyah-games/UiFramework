# Episode 10 · Popups and overlays

| | |
| --- | --- |
| Length | about 3 min |
| Docs slot | Tutorial step 10 (`#t-popup`), "Step 10: Popups and overlays" |
| Starts with | End of Episode 9 |
| Ends with | The finished Coin Shop: tap an item, a confirm popup opens over the store, Buy or Cancel closes it |
| Screenshot | none (also record a clean 15-second run for Episode 1's intro) |

## The code

`ConfirmPurchaseUiElementParams.cs` (generated; fill it in)

```csharp
using UiFramework.Core;

namespace CoinShop.Ui.Elements
{
    public class ConfirmPurchaseUiElementParams : UiPopulationParams
    {
        public string ItemName { get; set; }
        public int Price { get; set; }
    }
}
```

`ConfirmPurchaseUiElement.cs`

```csharp
using System;
using TMPro;
using UiFramework.Core;
using UiFramework.Runtime.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace CoinShop.Ui.Elements
{
    public class ConfirmPurchaseUiElement : UiElement
    {
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button cancelButton;

        // UiElement.Awake finds the object to animate: override it and call base.Awake() first.
        protected override void Awake()
        {
            base.Awake();
            buyButton.onClick.AddListener(Close);
            cancelButton.onClick.AddListener(Close);
        }

        public override void Populate(object context = null)
        {
            base.Populate(context);

            ConfirmPurchaseUiElementParams purchase = context as ConfirmPurchaseUiElementParams;
            if (purchase == null)
            {
                return;
            }

            messageLabel.text = "Buy " + purchase.ItemName + " for " + purchase.Price + " coins?";
        }

        private async void Close()
        {
            try
            {
                await UiManager.HideUI();
            }
            catch (Exception exception)
            {
                Debug.LogError("[ConfirmPurchaseUiElement] Could not close the popup: " + exception);
            }
        }
    }
}
```

In `StoreUiElement.cs`, each item's `BuyButton` opens it:

```csharp
private async void OnBuyClicked()
{
    try
    {
        ConfirmPurchaseUiElementParams purchase = new ConfirmPurchaseUiElementParams();
        purchase.ItemName = "Neon Blade";
        purchase.Price = 250;
        await UiManager.ShowState<ConfirmPurchaseUiState>(purchase, additive: true);
    }
    catch (Exception exception)
    {
        Debug.LogError("[StoreUiElement] Could not open the purchase popup: " + exception);
    }
}
```

## Script

**1. What a popup is** (0:00)
- SCREEN: The store running in Play mode, then stop.
- SAY: "Last step: a popup. In this framework, a popup is just another state, shown on top of the current one."

**2. Make the popup element** (0:10)
- SCREEN: **➕ New Element** → `ConfirmPurchaseUiElement`, tick **Include Parameters Class** → **➕ Create UI Element**. Open it. Under `Root`: first a full-screen `Backdrop` image, black at 70 % alpha; then a centred `Panel` (`#20244A`) with a TMP `Message`, a `BuyButton` and a `CancelButton`. Animations: show **Scale**, hide **Fade**. Canvas **Sorting Order** `100`. (Speed up the building.)
- SAY: "A new element. The first thing under Root is a dimmed backdrop that covers the whole screen, then the panel with a message and two buttons. It scales in, it fades out, and its sorting order is a hundred, so it sits above the screens and the bars."
- ON SCREEN: `Sorting Order 100`

**3. Its data** (0:55)
- SCREEN: Open `ConfirmPurchaseUiElementParams.cs`, add `ItemName` and `Price`. Open `ConfirmPurchaseUiElement.cs`, paste the code, point at `Populate` and at `HideUI`. Wire the three fields on the root. Save.
- SAY: "The parameters class carries the item's name and price. Populate puts them into the message. And both buttons close the popup with Hide UI: it removes the top state, and whatever was underneath simply stays."

**4. Its state** (1:30)
- SCREEN: States tab → `ConfirmPurchaseUiState` → **➕** → add only `ConfirmPurchaseUiElement`. General tab → **⚙️ Build Runtime UiConfig**.
- SAY: "Its state has just this one scene. And because we changed the states, we build the config again."

**5. Open it from the store** (1:50)
- SCREEN: Open `StoreUiElement.cs`, paste `OnBuyClicked`, type the `ShowState` line on camera. Wire the first item's `BuyButton` to it (Awake + AddListener, as in Episode 9).
- SAY: "In the store, the Buy button fills in the parameters and shows the popup state, with additive set to true. Additive means: put it on top, and keep everything below it."
- ON SCREEN: `additive: true`

**6. Play it** (2:20)
- SCREEN: Play → Store → tap the first item's **Buy**: the backdrop fades in, the panel scales in, "Buy Neon Blade for 250 coins?". The store and the bars stay underneath. **Cancel**: it fades away. Buy again → **Buy**. Show the Hierarchy: the popup scene loads, then unloads after closing.
- SAY: "Play, go to the store, and tap Buy. The popup opens on top with the item's details, and the store stays right where it was. Cancel, and it's gone, scene and all."

**7. Series wrap up** (2:45)
- SCREEN: The finished Coin Shop running: Menu → Store → popup.
- SAY: "And that's Coin Shop: screens, shared bars and a popup, every piece in its own scene, set up in the editor, with a handful of lines of code. The showcase app in the repository goes much further, so open it and see what's possible. Thanks for watching."
- ON SCREEN: `Docs, showcase and source: github.com/dawaniyah-games/UiFramework`

## After recording

- [ ] Commit: `Tutorial episode 10 end`.
- [ ] Record a clean 15-second run (Menu → Store → popup → Cancel) for Episode 1's intro.
