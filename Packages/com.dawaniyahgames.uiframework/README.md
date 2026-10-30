# UI Framework

A Unity UI framework that builds screens from **Addressable scenes** loaded additively and managed on a **LIFO state stack**. Each screen (a *state*) is made of one or more element scenes. `UiManager` loads the scenes a state needs, passes your data to every element through a single `Populate(object context)` call, plays show / hide animations, and unloads scenes nothing uses any more. Elements shared by consecutive states, such as a nav bar, stay loaded and don't re-animate.

The **UI Setup Manager** editor window generates the element scripts, element scenes, state classes and the runtime config for you.

## Requirements

- Unity 2022.3 or later
- Addressables 1.21.0 or later and uGUI (installed automatically as dependencies)

## Installation

**From the Asset Store:** add *UI Framework* to your assets, then in Unity open **Window → Package Manager → My Assets**, select *UI Framework* and click **Install**.

**From git:** open **Window → Package Manager → + → Add package from git URL…** and enter:

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
```

Append `#vX.Y.Z` to pin a [release](https://github.com/dawaniyah-games/UiFramework/releases).

## Quick start

1. Open **Window → UI Framework → UI Setup Manager**.
2. **General** → *Create Config*. Set your script and scene paths and namespaces on the new `UiEditorConfig`.
3. **Elements** → enter a name (e.g. `MainMenuUiElement`) → *Create*. This generates the script and the scene. Build the screen under `MainMenuUiElementRoot → Canvas → Root` and pick its show / hide animations in the list.
4. **States** → create a state (e.g. `MainMenuUiState`), add its element scenes and generate the state class.
5. **General** → ***Build Runtime UiConfig***. Run it again after every state or scene change.

Then initialize the manager and show a state. `UiManager` creates itself: don't add it to a scene. It loads the generated `UiConfig` from Addressables (address `UiConfig`), so build Addressables content first, or use the *Use Asset Database* play mode.

```csharp
using System;
using UiFramework.Editor.States;
using UiFramework.Runtime.Manager;
using UnityEngine;

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

`UiFramework.Editor.States` is the default namespace for generated states; use the one set on your `UiEditorConfig`. `ShowState<T>(context)` passes `context` to every element's `Populate`, and `additive: true` pushes the state on top of the current one (for popups) instead of replacing the stack.

## No samples, on purpose

This package ships no sample scenes or sample config. `UiManager.InitializeAsync()` always loads the Addressable with the address `UiConfig`, so a project has exactly one config. A sample with its own config would clash with yours or silently replace it, and Addressables entries don't travel with a sample import. Build your config from scratch with the UI Setup Manager as above.

To see a complete app built with the framework, clone the [GitHub repository](https://github.com/dawaniyah-games/UiFramework), open it in Unity 6 and play `Assets/Scenes/Bootstrapper.unity`.

## Links

- [Documentation](https://dawaniyahgames.com/uiframework/): getting started, a step-by-step UI Setup Manager tutorial, animations and the API
- [GitHub repository](https://github.com/dawaniyah-games/UiFramework): source, showcase app, [issues](https://github.com/dawaniyah-games/UiFramework/issues) and [discussions](https://github.com/dawaniyah-games/UiFramework/discussions)
- [CHANGELOG.md](CHANGELOG.md)
