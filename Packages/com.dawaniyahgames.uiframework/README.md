# UI Framework: Manual

UI Framework builds your game UI from **Addressable scenes** that are loaded additively and managed on a **LIFO state stack**. Each screen (a *state*) is made of one or more *element scenes*, such as a top bar, a nav bar and the page itself. One call shows a screen or pushes a popup: `UiManager` loads the scenes the state needs, passes your data to every element through a single `Populate(object context)` call, plays show and hide animations, and unloads the scenes nothing uses any more. Elements shared by consecutive states, such as a nav bar, stay loaded and don't re-animate.

The **UI Setup Manager** editor window generates element scripts, element scenes, state classes and the runtime config, and checks every element scene against the structure the framework expects.

This file is the offline manual. The same material, with screenshots and video tutorials, is online at <https://www.dawaniyahgames.com/uiframework/>.

**Contents**

1. [Requirements and compatibility](#1-requirements-and-compatibility)
2. [Installation](#2-installation)
3. [Concepts](#3-concepts)
4. [Quick start](#4-quick-start)
5. [The UI Setup Manager](#5-the-ui-setup-manager)
6. [Element scenes](#6-element-scenes)
7. [Writing elements](#7-writing-elements)
8. [Showing and hiding states](#8-showing-and-hiding-states)
9. [Animations](#9-animations)
10. [Addressables](#10-addressables)
11. [Enter Play Mode options](#11-enter-play-mode-options)
12. [Known limitations](#12-known-limitations)
13. [Troubleshooting](#13-troubleshooting)
14. [Building UI with an AI assistant](#14-building-ui-with-an-ai-assistant)
15. [Support and links](#15-support-and-links)

## 1. Requirements and compatibility

| | |
| --- | --- |
| Unity | 2022.3 LTS or newer, including Unity 6 |
| Dependencies | Addressables (`com.unity.addressables` 1.21.0 or newer) and uGUI (`com.unity.ugui`). Package Manager installs them automatically |
| Render pipelines | Built-in, URP and HDRP. The framework only uses uGUI canvases, so it doesn't depend on the render pipeline |
| Platforms | Every platform Unity and Addressables support. There is no native code |
| Play Mode | Works with *Enter Play Mode Options* and domain reload disabled (see [section 11](#11-enter-play-mode-options)) |

The package contains C# scripts, editor tools, uss/uxml styles, code templates and tests. It contains no art, no third-party code and no samples (see [section 3](#no-samples-on-purpose)).

## 2. Installation

**From the Asset Store:** add *UI Framework* to your assets, then in Unity open **Window → Package Manager → My Assets**, select *UI Framework* and click **Install**.

**From git:** open **Window → Package Manager → + → Add package from git URL…** and enter:

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
```

Append `#vX.Y.Z` to pin a release.

After installing, open **Window → UI Framework → UI Setup Manager**.

## 3. Concepts

| Term | Meaning |
| --- | --- |
| **Element scene** | A Unity scene with one root GameObject, `<Name>Root`, that carries a `UiElement` component. It is the unit of loading and unloading. |
| **UiElement** | Your `MonoBehaviour` for that scene. It receives data in `Populate(object context)` and owns its show and hide animations. |
| **State** | A named set of element scenes that make up one screen or popup, for example `StoreUiState` = Store + TopBar + NavBar. |
| **State key** | The string that names a state in the runtime config. To call `ShowState<T>()`, the key must equal the class name of `T`. |
| **Context** | The object you pass to `ShowState*`. Every element of the state receives the same instance in `Populate`. Usually a `UiPopulationParams` subclass. |
| **Shared element** | An element whose scene is already loaded when the next state needs it, such as a nav bar. It is populated again but stays on screen without re-animating. |
| **Runtime config** | The `UiConfig` asset that maps state keys to element scenes. *Build Runtime UiConfig* generates it, and `UiManager` loads it from Addressables at startup. |

### How a screen change works

1. `UiManager.ShowState<StoreUiState>(context)` looks up the state's element scenes in `UiConfig`.
2. It loads each scene additively through Addressables, unless it is already loaded, and counts how many states use it.
3. It calls `Populate(context)` on every element in the new scenes, then puts new elements in their "before" pose so nothing flashes.
4. It hides the old state and shows the new one at the same time. Elements on screen in both stay where they are.
5. Scenes that no state uses any more are unloaded.

A normal call replaces the whole stack. `additive: true` pushes the state on top of the current one instead (for popups and overlays), and `HideUI()` pops it again.

### No samples, on purpose

The package ships no sample scenes or sample config. `UiManager.InitializeAsync()` always loads the Addressable with the address `UiConfig`, so a project has exactly one config. A sample with its own config would clash with yours or silently replace it, and Addressables entries don't travel with a sample import. Build your config from scratch with the UI Setup Manager, as below.

A complete showcase app built with the framework (a mobile-game shell with tabs, a shared top bar and nav bar, and popups) is in the GitHub repository, not in this package. Clone the repository, open it in Unity 6 and play `Assets/Scenes/Bootstrapper.unity`.

## 4. Quick start

1. Open **Window → UI Framework → UI Setup Manager**.
2. **General** → *Create Config*. This creates `Assets/UiConfigs/UiEditorConfig.asset`. Select it and set the script and scene folders, and the namespaces for generated element and state scripts. The default namespaces (`UiFramework.Editor.Elements`, `UiFramework.Editor.States`) are placeholders: use your game's.
3. **Elements** → *New Element* → enter a name ending in `UiElement`, for example `MainMenuUiElement` → *Create*. This generates `MainMenuUiElement.cs` and a scene with the hierarchy `MainMenuUiElementRoot → Canvas → Root`. Unity recompiles, and the script is attached to the root for you.
4. Open the scene and build the screen **under `Root`**. Back in the Elements tab, pick the element's show and hide animations.
5. **States** → create a state, for example `MainMenuUiState`. Add the element scenes it shows and generate the state class (needed for `ShowState<T>()`).
6. **General** → ***Build Runtime UiConfig***. Run it again after every change to states or element scenes.
7. Make sure the Addressables content is current: build it (**Window → Asset Management → Addressables → Groups → Build → New Build**), or use the *Use Asset Database* play mode script while developing.
8. Initialize the manager once and show the state:

```csharp
using System;
using UiFramework.Runtime.Manager;
using UnityEngine;
using MyGame.Ui.States;

namespace MyGame.Ui
{
    public class GameBootstrap : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                await UiManager.InitializeAsync();
                await UiManager.ShowState<MainMenuUiState>();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameBootstrap] Could not show the main menu. {exception}", this);
            }
        }
    }
}
```

`UiManager` creates itself and survives scene loads. Don't add it to a scene. Your bootstrap scene still needs a camera and an `EventSystem`; element scenes should not carry their own.

## 5. The UI Setup Manager

Open it from **Window → UI Framework → UI Setup Manager**. The selected `UiEditorConfig` is remembered per machine.

### General tab

- **Create Config** finds or creates the `UiEditorConfig` asset. The tab shows its paths: element scripts, element scenes, state scripts, state definitions and the runtime config output.
- **Build Runtime UiConfig** clears and regenerates the runtime `UiConfig` from your state definitions. It makes every element scene and the config Addressable (see [section 10](#10-addressables)). Never edit the generated `UiConfig` by hand.
- **Global Canvas Settings** are the defaults written into new element scenes: render mode, pixel perfect, UI scale mode, reference resolution, screen match mode and value, and reference pixels per unit. Changing them never changes existing scenes. Use *Use Default Settings* on an element for that.

### Elements tab

- **Search** filters the list by part of the name, in any case. Enter selects the first result.
- **New Element** shows the create form. *Include Parameters Class* also generates `<Name>Params`, a `UiPopulationParams` subclass for the data you pass to this screen. *Include Reference Class* generates `<Name>Reference`, an optional typed holder you can fill in for your own code; the framework doesn't call it. You can also convert an existing scene into an element: its root objects are moved under `<Name>Root` and a structure fix is offered. **A script that already exists is never overwritten.**
- **Toolbar**: *Refresh*, *Validate All* (logs one line per scene with its issues), *Fix All Structure…* (confirms, then fixes every fixable scene) and *Only show elements with issues*.
- Each element shows ✓ (valid), ⚠ (fixable) or ✖ (needs manual work). Double-click to open the scene.
- **Element settings**: *Open Scene* and *Select Asset*; **Structure** (issues, *Fix Structure…*, *What to fix by hand…*); **Animations** (show and hide presets); **Canvas** (this scene's canvas values, and *Use Default Settings…*); **Addressables** (toggle, group and labels).

Every fix asks first. The confirmation lists what will change, shows a *Now / After* hierarchy preview, and lists anything that needs manual work. Fixes only create and reparent objects. They never delete anything, and they are recorded for Undo. A scene that is already open is edited in place and left unsaved; a closed scene is opened, saved and closed again.

### States tab

Create a state, add the element scenes it shows (shared ones like a nav bar included), and generate its class from the template. Each state is stored as one `UiStateDefinition` asset, so team members editing different states don't conflict.

## 6. Element scenes

Every element scene must have exactly this structure, which the tools create and check:

```
<Name>Root   plain Transform with your UiElement subclass; the ONLY root GameObject in the scene
└─ Canvas    child 0: Canvas + CanvasScaler + GraphicRaycaster
   └─ Root   the ONLY child of Canvas: active, stretched to full screen, no Canvas of its own
      └─ ... all the screen's content (nested canvases are fine here)
```

`Root` is what animates. Slides move it by the canvas size and scale presets scale it around its pivot, so anything placed next to `Root` under the Canvas would not animate with the screen. The tools check the structure, not the names: a content root called `Panel` is fine.

The validator reports these issues:

| Issue | Meaning | Fix Structure |
| --- | --- | --- |
| No UiElement | No root object has a `UiElement` | Manual |
| Extra root objects | The scene has other root objects (cameras, lights, EventSystems…) | Manual: they are never deleted for you |
| No Canvas | Child 0 of the element root is not a Canvas | Automatic in most cases |
| Canvas missing components | No CanvasScaler or GraphicRaycaster | Automatic: adds them |
| No content root | The Canvas has no single content child | Automatic: wraps the children in a new `Root` |
| Content root not stretched | The content root doesn't fill the canvas | Automatic: wraps it in a full-screen `Root` without resizing it |
| Content root inactive | The content root is disabled | Warning only |
| Canvas inside prefab | The Canvas belongs to a prefab instance | Manual: fix it in the prefab |

Sorting order, override sorting, world camera and plane distance are never changed by the tools, so you can use sorting order to stack popups over screens.

## 7. Writing elements

Override `Populate` to read the context. **`Populate` must be idempotent**: when the next state reuses an element's scene, `Populate` runs again on the same instance with the new context. Rebuild instead of appending, and guard one-time setup with a flag.

```csharp
using System.Collections.Generic;
using UiFramework.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MyGame.Ui.Elements
{
    public class InventoryUiParams : UiPopulationParams
    {
        public List<string> ItemIds { get; set; }
    }

    public class InventoryUiElement : UiElement
    {
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Button closeButton;

        private readonly List<GameObject> spawnedSlots = new List<GameObject>();
        private bool isWired;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!isWired)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
                isWired = true;
            }

            ClearSlots();

            InventoryUiParams data = context as InventoryUiParams;
            if (data == null || data.ItemIds == null)
            {
                return;
            }

            for (int i = 0; i < data.ItemIds.Count; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotContainer);
                slot.name = data.ItemIds[i];
                spawnedSlots.Add(slot);
            }
        }

        private async void OnCloseClicked()
        {
            try
            {
                await UiFramework.Runtime.Manager.UiManager.HideUI();
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[InventoryUiElement] Could not close. {exception}", this);
            }
        }

        private void ClearSlots()
        {
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                if (spawnedSlots[i] != null)
                {
                    Destroy(spawnedSlots[i]);
                }
            }

            spawnedSlots.Clear();
        }
    }
}
```

Context can be any object: a `UiPopulationParams` subclass, a plain class, or a primitive such as `context is int coins`. Elements that don't need it can ignore it.

`UiElement` members you can override:

| Member | Purpose |
| --- | --- |
| `Populate(object context)` | Receive the state's data. Called before the element's first visible frame |
| `ShowAsync(string presetKey = null)` / `HideAsync(string presetKey = null)` | Play the show or hide animation. Override for custom animation, and call `base` to keep the preset |
| `PrepareForShow()` / `PrepareForShow(string presetKey)` | Put the element in its "before" pose right after its scene loads |
| `Awake()` | Protected virtual. Call `base.Awake()`, which picks the animation target |

## 8. Showing and hiding states

All calls are static on `UiFramework.Runtime.Manager.UiManager` and return a `Task`.

| API | What it does |
| --- | --- |
| `InitializeAsync()` | Creates the manager (once) and loads `UiConfig` from Addressables |
| `ShowState<T>(object context = null, bool additive = false, UiTransition transition = null)` | Shows the state whose key equals `T`'s class name |
| `ShowStateByKey(string stateKey, object context = null, bool additive = false, UiTransition transition = null)` | The same, by key. Use it for states without a generated class |
| `HideUI(UiTransition transition = null)` | Pops and hides the top state, and unloads scenes nothing uses any more |
| `ResetUIAsync()` | Hides and unloads every state |
| `GetCurrentState()` | The state being loaded, otherwise the top of the stack, otherwise `null` |
| `IsLastSceneActive<T>()` | Whether the top state is `T` |
| `TryGetUiStateKeyName<T>(out string key)` | The key registered for `T` |
| `IsInitialized` | `true` once `InitializeAsync` has created the manager |

- **Non-additive** (the default) replaces the whole stack: use it for full screens and tabs.
- **Additive** pushes on top and leaves the states below loaded and visible: use it for popups, then close them with `HideUI()`.
- Await each call before starting the next one (see [section 12](#12-known-limitations)). From button handlers, use `async void` with `try / catch`, as in the example above.
- Errors never throw into your code. An unknown key, a missing config or a failed load logs one clear `[UiManager]` error and returns.

## 9. Animations

### Presets

Each element has a show and a hide preset, chosen in the Elements tab. Presets run on unscaled time, so UI keeps moving while the game is paused. Every show and hide fades, entrances ease out and exits ease in.

| Preset | Show | Hide |
| --- | --- | --- |
| `None` | Appears instantly | Disappears instantly |
| `Fade` | Fades in (0.22 s) | Fades out (0.16 s) |
| `Scale` | Zooms from 0.94× with a fade (0.26 s) | Zooms to 0.96× with a fade (0.18 s) |
| `SlideRight`, `SlideLeft`, `SlideUp`, `SlideDown` | Moves in the named direction, entering from the opposite edge (0.32 s) | Keeps moving in the named direction and leaves past that edge (0.24 s) |
| `ClickPop`, `SettlePop`, `Shake` | A pop, a gentle pulse or a short shake, fading in | The same motion, fading out |

Every animation starts from and returns to the element's resting pose, so repeated or interrupted animations never drift. A new animation stops older ones on the same object that drive the same property.

### Per-call override

Pass a `UiTransition` to change the animation of one call without changing the elements' configuration:

```csharp
// Show the Store with a zoom this time.
await UiManager.ShowState<StoreUiState>(context, transition: UiTransition.ShowWith(TweenPresets.ScaleKey));

// Close the top popup by sliding it out to the right.
await UiManager.HideUI(UiTransition.HideWith(TweenPresets.SlideRightKey));

// Tab switch: the new screen slides in moving left, the old one leaves moving left.
await UiManager.ShowStateByKey("ProfileUiState", context, false, new UiTransition(TweenPresets.SlideLeftKey, TweenPresets.SlideLeftKey));
```

Preset names are the `TweenPresets.*Key` constants, matched ignoring case. An unknown name logs a warning and falls back to the elements' own animations. Shared elements that stay on screen are never animated.

### Button feedback and custom animation

`TweenPresets.ClickPop(transform)`, `SettlePop(transform)` and `ShakeLocal(transform)` play the pop, pulse and shake on any transform, for example on a button press. `UiFramework.Tweening.Tween` offers `RunAsync` and helpers such as `ScaleAsync`, `MoveLocalAsync`, `FadeCanvasGroupAsync` and `FadeGraphicAsync` for your own effects. For a fully custom show or hide, override `ShowAsync` / `HideAsync` on your element.

## 10. Addressables

*Build Runtime UiConfig* sets up Addressables for you:

| What | Group | Address | Labels |
| --- | --- | --- | --- |
| Runtime `UiConfig` | `GlobalConfigs` | `UiConfig` (the runtime loads it by this address) | `UiConfig`, `RuntimeUiConfig` |
| Element scenes | `UiElements` | the scene file name | `UiElements` |

Missing groups and labels are created. Element scene names must be unique in the project, because states refer to scenes by name until the config is built.

For player builds, build Addressables content after *Build Runtime UiConfig*. In the Editor, the *Use Asset Database* play mode script needs no build.

## 11. Enter Play Mode options

The framework supports **Enter Play Mode Options** with domain reload disabled. `UiManager` clears its static instance when each Play session starts, so every session begins uninitialized and creates a fresh manager on the first `InitializeAsync`. No other static holds state.

## 12. Known limitations

| Area | Limitation |
| --- | --- |
| Overlapping calls | `ShowState*` and `HideUI` calls are not queued for you. Await one before starting the next, or queue them in one navigator class |
| `HideUI` and shared elements | When a popped state shares an element with the state below, that element still plays its hide animation, although its scene stays loaded |
| Initialization | `IsInitialized` is `true` as soon as the manager exists, before the config finishes loading, and stays `true` if loading fails (an error is logged) |
| State classes | `UiState` subclasses only name the state; the runtime always creates the base `UiState`, so their constructors and overrides don't run. Two subclasses with the same class name in different namespaces collide |
| Custom `IUiElement` | Elements that implement `IUiElement` without deriving from `UiElement` receive `Populate` but don't animate |
| One config | A project has one runtime `UiConfig`, loaded by the address `UiConfig` |
| Not included | Input routing, navigation history beyond the stack, data binding and localisation are out of scope |

## 13. Troubleshooting

**Nothing appears when I call `ShowState<T>()`.** Check that the state key equals the class name of `T`, that *Build Runtime UiConfig* ran after your last change, that the Addressables content is built (or the play mode script is *Use Asset Database*), and that the Console shows no `[UiManager]` errors.

**`[UiManager] State '…' not found`.** The key is not in the runtime config. Create the state in the States tab, then run *Build Runtime UiConfig*.

**The screen flashes or part of it doesn't animate.** Run *Validate All*. Content placed next to `Root` instead of under it doesn't animate; *Fix Structure* wraps it for you.

**A nav bar re-animates on every tab switch.** It must be the same element scene in both states, and the switch must be non-additive.

**Data from the previous screen shows up.** `Populate` runs again on shared elements; clear what it built last time (see [section 7](#7-writing-elements)).

**The menu item isn't where a video shows it.** Since v1.3.6 the window is under **Window → UI Framework → UI Setup Manager**. Older tutorial videos show a top-level *UiFramework* menu.

## 14. Building UI with an AI assistant

`Documentation~/ai-builder/` (in the package folder on disk) is a guide for coding assistants such as Claude, ChatGPT / Codex and Copilot. It covers planning screens as element scenes and states, importing art from Figma or Photoshop as compressed sprite atlases, responsive layouts in the Inspector, and the coding standards. Copy `Documentation~/ai-builder/references/project-profile.md` to `UI_PROFILE.md` at your project root, fill it in once, then point your assistant at `Documentation~/ai-builder/SKILL.md` from your `AGENTS.md`, `CLAUDE.md` or Copilot instructions.

## 15. Support and links

- Documentation and video tutorials: <https://www.dawaniyahgames.com/uiframework/>
- Questions and bug reports: <https://github.com/dawaniyah-games/UiFramework/issues> and <https://github.com/dawaniyah-games/UiFramework/discussions>
- Email: info@dawaniyahgames.com
- Source and the showcase app: <https://github.com/dawaniyah-games/UiFramework>
- Version history: [CHANGELOG.md](CHANGELOG.md)
