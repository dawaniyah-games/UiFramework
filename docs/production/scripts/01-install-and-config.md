# Episode 1 · Install and create the config

| | |
| --- | --- |
| Length | about 2 min |
| Docs slot | Tutorial step 1 (`#t-config`), "Step 1: Install and create the config" |
| Starts with | The empty `CoinShop` project (TextMeshPro Essentials imported) |
| Ends with | The package installed, Addressables set up, a `UiEditorConfig` with Coin Shop namespaces |
| Screenshot | 📸 `setup-general.png` at beat 5 |

## Before you record

- [ ] Editor in the `Tutorial` layout, Console cleared.
- [ ] The package URL copied to the clipboard: `https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework`

## Script

**1. What we'll build** (0:00)
- SCREEN: The finished Coin Shop running in the Game view (record it at the end of Episode 10 and drop it in here): Main Menu, tap Store, the Store slides in under the bars, tap an item, the popup appears.
- SAY: "In this series we build the UI for a small game called Coin Shop, from an empty project. A main menu, a store, a top bar and a nav bar shared by both, and a purchase popup. One short step per video. Let's start."
- ON SCREEN: `Coin Shop · built step by step`

**2. Install the package** (0:15)
- SCREEN: Window → Package Manager → **+** → **Add package from git URL…** → paste the URL → **Add**. Wait for the import (speed up in the edit).
- SAY: "Open the Package Manager, press plus, and choose Add package from git URL. Paste the address from the docs and press Add. Unity installs the framework, and Addressables comes with it."
- ON SCREEN: the URL, large, for two seconds.

**3. Turn on Addressables** (0:40)
- SCREEN: Window → Asset Management → Addressables → **Groups** → **Create Addressables Settings**. If a **Legacy Bundles Detected** dialog asks to convert bundles, press **Ignore** (the Universal 2D template has one URP shader bundle name; converting it would add an extra group). Then in the same window: **Play Mode Script → Use Asset Database**.
- SAY: "Every screen will be an Addressable scene, so open Addressables Groups once and create its settings. Set the play mode script to Use Asset Database, so screens load straight from your project when you press Play."

**4. Open the UI Setup Manager and create the config** (1:00)
- SCREEN: Menu **Window → UI Framework → UI Setup Manager**. Dock it next to the Inspector. **General** tab → **🔄 Create Config**. Click the created `UiEditorConfig` asset; in the Inspector set **Element Namespace** to `CoinShop.Ui.Elements` and **State Namespace** to `CoinShop.Ui.States`.
- SAY: "This window is where you'll build all of your UI. In the General tab, press Create Config. The config remembers where your scripts and scenes go. Set the two namespaces to your game's: Coin Shop dot UI dot Elements, and Coin Shop dot UI dot States."

**5. Check the canvas defaults** (1:25)
- SCREEN: Back in the **General** tab, scroll to **Global Canvas Settings**. Show Render Mode **Screen Space - Overlay**, UI Scale Mode **Scale With Screen Size**, Reference Resolution **1080 × 1920**, Screen Match Mode **Expand**. Hover over them slowly.
- 📸 Take `setup-general.png` here: the whole UI Setup Manager on the General tab, config paths and Global Canvas Settings visible. (The namespaces live on the `UiEditorConfig` in the Inspector, not in this tab.)
- SAY: "Below are the Global Canvas Settings. Every new screen starts with these: a portrait phone at ten-eighty by nineteen-twenty, scaling with the screen. Change them here once, and every new screen follows. Existing screens are never changed behind your back."

**6. Wrap up** (1:50)
- SCREEN: The UI Setup Manager on the General tab, still.
- SAY: "That's the setup, and you only do it once. Next, we create our first screen."
- ON SCREEN: `Next: Create your first element`

## After recording

- [ ] Commit the project: `Tutorial episode 1 end`.
- [ ] `setup-general.png` saved to `docs/images/`.
