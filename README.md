<p align="center"><img src="docs/images/logo.svg" width="96" alt="UI Framework logo"></p>

# UI Framework

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue.svg)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)

A Unity UI framework that builds screens from **Addressable scenes** loaded additively and managed on a **LIFO state stack**. Elements receive runtime data through a single `Populate(object context)` contract, and an editor window generates the scripts, scenes and runtime config for you.

## Features

- 🎯 **Addressables scene composition**: each UI element is its own scene, loaded and unloaded on demand
- 📚 **LIFO state stack**: replace the whole UI, or push modal/overlay states additively
- 🔗 **Shared elements**: scenes used by consecutive states (e.g. a NavBar) stay loaded and don't re-animate
- 🔄 **Context-based population**: pass any data object to every element in a state
- 🌀 **Per-element animation presets**: Fade, Scale, Slide{Up,Down,Left,Right}, ClickPop, SettlePop, Shake
- ⏩ **Overlapping transitions**: the new UI animates in while the old UI animates out, with no flash and no empty frame
- 🛠️ **Editor tooling**: generate elements and states from templates and build the runtime config automatically
- ✅ **Element scene contract**: new scenes are generated as `<Name>Root → Canvas → Root` with the global canvas settings, and existing scenes can be validated and fixed in one click from the Elements tab (Global Canvas Settings live in the General tab)

## Installation

**From the Unity Asset Store:** add *UI Framework* to your assets, then install it from **Window → Package Manager → My Assets**. It is the same package as the git URL below, without the sample app.

**From git: Package Manager → + → Add package from git URL…**

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
```

To pin a version, append `#vX.Y.Z` (see [Releases](https://github.com/dawaniyah-games/UiFramework/releases)). Or add it to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.6"
  }
}
```

**Requirements:** Unity 2022.3+, Addressables 1.21.0+ (installed as a dependency).

## Quick start

### 1. Author your UI in the editor

1. Open **Window → UI Framework → UI Setup Manager**.
2. **General** → *Create Config*. Set your script/scene paths and namespaces on the created `UiEditorConfig`.
3. **Elements** → enter a name (e.g. `MainMenuUiElement`) → *Create*. This generates the script and scene. Build your UI under `MainMenuUiElementRoot → Canvas → Content` and pick show/hide animation presets in the list.
4. **States** → create a state key (e.g. `MainMenuUiState`), add its element scenes, and generate the state class.
5. **General** → ***Build Runtime UiConfig***. Re-run it after every state or scene change.

### 2. Initialize and show a state

`UiManager` creates itself. Don't add it to a scene. It loads the generated `UiConfig` from Addressables (address `UiConfig`).

```csharp
using UiFramework.Runtime.Manager;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private async void Start()
    {
        if (!UiManager.IsInitialized)
        {
            await UiManager.InitializeAsync();
        }

        await UiManager.ShowState<MainMenuUiState>();
    }
}
```

### 3. Receive data in an element

```csharp
using UiFramework.Core;

public class MainMenuUiElement : UiElement
{
    public override void Populate(object context = null)
    {
        base.Populate(context);

        MenuData data = context as MenuData;
        if (data == null)
        {
            return;
        }

        // Configure the UI from data. May run more than once: keep it idempotent.
    }
}
```

## Sample app

This repository's host project (`Assets/`) is a small mobile-game shell built only from framework features. It has an intro, four tabs (Home, Store, Leaderboard, Profile) under a shared TopBar and NavBar, and Settings, Reward and Confirm-purchase popups, with animation on every screen.

1. Clone the repository and open it in Unity 6.
2. Open `Assets/Scenes/Bootstrapper.unity` and press Play (Addressables play mode *Use Asset Database*, or build Addressables content first). Use a portrait Game view (1080 x 1920).

Then open **Window → UI Framework → UI Setup Manager → Elements** and change a screen's show / hide animation to see the difference. To choreograph a screen without code, add `RevealOnShow`, `StaggeredReveal`, `FillBar`, `CountUpText` or `LoopMotion` to its objects in the Inspector. [DESIGN.md §14](DESIGN.md#14-sample-app-host-project) lists the patterns the sample demonstrates.

## API at a glance

| Call | Effect |
| --- | --- |
| `await UiManager.InitializeAsync()` | Create the manager and load `UiConfig` (no-op if already initialized) |
| `await UiManager.ShowState<T>(context, additive)` | Show the state whose key equals `T`'s class name |
| `await UiManager.ShowStateByKey("Key", context, additive)` | Show a state by its key |
| `await UiManager.ShowState<T>(context, transition: UiTransition.ShowWith(TweenPresets.ScaleKey))` | Optional: play a different show animation for this call only |
| `await UiManager.HideUI()` | Pop and unload the top state |
| `await UiManager.HideUI(UiTransition.HideWith(TweenPresets.SlideRightKey))` | Optional: play a different hide animation for this call only |
| `UiManager.GetCurrentState()` | Current (or currently loading) state |
| `UiManager.IsLastSceneActive<T>()` | Whether `T` is the top state |
| `await UiManager.ResetUIAsync()` | Hide and unload every state |

`additive: false` (default) replaces the stack. `additive: true` pushes on top, for modals and overlays.

## Common pitfalls

- **`ShowState<T>()` logs "No UI state key registered"**: the `UiState` class name must exactly match the state key.
- **"Failed to locate UiConfig"**: run *Build Runtime UiConfig*, and make sure Addressables content is built (or play mode uses the Asset Database).
- **Logic in a `UiState` subclass never runs**: subclasses are only type markers for `ShowState<T>()`, and the runtime uses the base `UiState`.
- **Element data duplicated after switching screens**: `Populate` runs again when a state reuses a shared scene, so make it idempotent.

## Documentation

| Doc | For |
| --- | --- |
| [Docs site](https://dawaniyahgames.com/uiframework/) | Overview, getting started, a step-by-step UI Setup Manager tutorial, animations, the showcase and the API ([how to add screenshots and videos](docs/README.md)) |
| [DESIGN.md](DESIGN.md) | How the framework works: lifecycle, scene ref counting, animation, editor pipeline, limitations |
| [CODING_STANDARDS.md](CODING_STANDARDS.md) | Code style and project rules (required for all contributions) |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Branching, PRs, releases |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

## License

MIT. See [LICENSE.md](LICENSE.md). The copy on the Unity Asset Store is covered by the Asset Store End User License Agreement instead.

## Support

- 📝 [Issues](https://github.com/dawaniyah-games/UiFramework/issues)
- 💬 [Discussions](https://github.com/dawaniyah-games/UiFramework/discussions)
