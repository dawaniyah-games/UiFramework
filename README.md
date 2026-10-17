# UI Framework

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com)
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

**Package Manager → + → Add package from git URL…**

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
```

To pin a version, append `#vX.Y.Z` (see [Releases](https://github.com/dawaniyah-games/UiFramework/releases)). Or add it to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.1.0"
  }
}
```

**Requirements:** Unity 2021.3+, Addressables 1.21.0+ (installed as a dependency).

## Quick start

### 1. Author your UI in the editor

1. Open **UiFramework → UI Setup Manager**.
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

## API at a glance

| Call | Effect |
| --- | --- |
| `await UiManager.InitializeAsync()` | Create the manager and load `UiConfig` (no-op if already initialized) |
| `await UiManager.ShowState<T>(context, additive)` | Show the state whose key equals `T`'s class name |
| `await UiManager.ShowStateByKey("Key", context, additive)` | Show a state by its key |
| `await UiManager.HideUI()` | Pop and unload the top state |
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
| [DESIGN.md](DESIGN.md) | How the framework works: lifecycle, scene ref counting, animation, editor pipeline, limitations |
| [CODING_STANDARDS.md](CODING_STANDARDS.md) | Code style and project rules (required for all contributions) |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Branching, PRs, releases |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

## License

MIT. See [LICENSE.md](LICENSE.md).

## Support

- 📝 [Issues](https://github.com/dawaniyah-games/UiFramework/issues)
- 💬 [Discussions](https://github.com/dawaniyah-games/UiFramework/discussions)
