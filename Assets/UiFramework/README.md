# UI Framework — High-Level System Design (DenDon)

## Architecture Overview

The UI framework composes application UI from Addressable scenes and exposes a small runtime API to push/pop logical UI states. It follows the project's MVC-S approach: the framework is the View layer's composition system and integrates with the app via `UiState` contexts and `IUiElement.Populate` calls.

## Core Components

1. UiManager (Assets/Scripts/UiFramework/Runtime/Manager/UiManager.cs)
   - Orchestrates a LIFO stack of `UiState` objects.
   - Public API: `ShowState<T>`, `ShowStateByKey`, `HideUI`, `GetCurrentState`.
   - Loads `UiConfig` (ScriptableObject) at Init and builds a type→stateKey map.
2. UiState (Assets/Scripts/UiFramework/Core/UiCompoenets/UiState.cs)
   - Loads addressable scenes additively via `Addressables.LoadSceneAsync`.
   - Discovers `IUiElement` components in loaded scenes and calls `Populate(context)`.
   - Handles unloading and cleanup (`UnloadUiState`, `Dispose`).
3. UiElement / IUiElement (Assets/Scripts/UiFramework/Core/UiCompoenets)
   - `IUiElement.Populate(object context)` is the contract for receiving runtime data.
   - Recommended: implement `UiElement : MonoBehaviour` and override `Populate`.
4. UiConfig (Assets/Scripts/UiFramework/Core/Config/UiConfig.cs)
   - ScriptableObject mapping `stateKey` → list of `AssetReference` scene entries.
5. Editor helpers (UiStateRegistry, UiStateGenerator)
   - Editor tooling can generate `UiState` shells or register scenes; see `Assets/Scripts/UiFramework/Editor`.

## Data Flow Example — Showing a Screen

- Call `UiManager.ShowState<MyState>(context)` or `UiManager.ShowStateByKey("MyState", context)`.
- UiManager looks up the `UiStateEntry` from `UiConfig` and constructs a `UiState` with the configured `AssetReference` scenes.
- `UiState.Init(context)` begins Addressables loads for each scene; on load complete it finds `IUiElement`s and calls `Populate(context)`.
- If `additive` is false, previous states are unloaded via `UnloadUiState` and the new `UiState` is pushed onto the stack.

## Important Patterns & Conventions

- `stateKey` naming: the framework expects `stateKey` values to align with `UiState` subclass names when using the generic API. `GetTypeForKey` resolves types by class name across assemblies.
- Addressables-first: UI is built from addressable scenes so that screens can be loaded/unloaded independently at runtime.
- LIFO state stack: `UiManager` maintains stacked states (push/pop) to support nested screens and modals.
- Context object: the `context` parameter is the primary mechanism for passing runtime data to UI elements — cast it to expected types in `Populate`.

## Integration Points & Dependencies

- Relies on Unity Addressables package: scenes are `AssetReference` items. Ensure addressables groups are set up.
- Designed to be called from the project's loading steps or services (e.g., after `Bootstrapper` completes). Typical integration point: call `await uiManager.Init()` during app startup.

## Developer Workflows (practical notes)

- Adding screens: create addressable scenes containing `UiElement` components, add `AssetReference` to `UiConfig` under a `stateKey`.
- Debugging: enable logs in `UiManager` and `UiState` to trace load/unload flow; inspect `UiConfig` entries in the inspector.
- Testing in editor: use `UiManager.ShowStateByKey` from a temporary debug MonoBehaviour or the console to validate scene loading and `Populate` invocations.

## Files To Inspect

- `Assets/Scripts/UiFramework/Runtime/Manager/UiManager.cs` — state orchestration
- `Assets/Scripts/UiFramework/Core/UiCompoenets/UiState.cs` — scene load/unload and element discovery
- `Assets/Scripts/UiFramework/Core/UiCompoenets/UiElement.cs` & `IUiElement.cs` — element contract
- `Assets/Scripts/UiFramework/Core/Config/UiConfig.cs` — mapping stateKey→scenes
- `Assets/Scripts/UiFramework/Editor/*` — generator and registry tools

## Do's & Don'ts

- Do: keep `stateKey` values consistent and descriptive. Prefer creating a `UiState` subclass named exactly like the `stateKey` if you use the generic API.
- Do: place each isolated UI screen in its own addressable scene to enable efficient unloading.
- Don't: rely on scene names alone — use `AssetReference`s stored in `UiConfig` so Addressables resolves properly.
- Don't: have multiple `UiManager` instances active; `SetInstance` is called during `Init` and the framework assumes a single runtime manager.

If you want, I can:

- Add a small Editor validation window that enumerates `UiConfig` entries and verifies each `AssetReference` resolves and contains `IUiElement` components.
- Add optional defensive logging and safer type-mapping (explicit type registration) to reduce runtime fragility.

---

Place this file next to the framework sources so engineers and AI agents can quickly understand and extend the UI framework.

## ASCII UML (high-level)

Below is a compact ASCII UML-style diagram showing the runtime relationships and typical flow between components.

UiManager
|
|-- stack of UiState (LIFO)
|
+-- UiState
| - StateName
| - uiElementScenes : List<AssetReference>
| - loadedScenes : Dictionary<string, SceneInstance>
| - activeUiElements : List<IUiElement>
| - Init(context) -> Addressables.LoadSceneAsync(...)
| - OnSceneLoadCompleted -> Populate(context) on IUiElement
| - UnloadUiState(keepScenes)
|
+-- UiElement (IUiElement) - Populate(object context) - MonoBehaviour (view logic)

UiConfig (ScriptableObject)

- entries: List<UiStateEntry>
  - UiStateEntry { stateKey: string, uiElementScenes: List<AssetReference> }

Notes:

- `UiManager` maps `Type` <-> `stateKey` via `GetTypeForKey(stateKey)` (type name matching).
- `UiState` loads scenes additively; each scene contains root GameObjects implementing `IUiElement`.

## Onboarding Quick Reference (for new developers)

- Where to start:

  - Read `Assets/Scripts/UiFramework/README.md` (this file).
  - Open `Assets/Scripts/UiFramework/Core/Config/UiConfig.cs` in the Editor and inspect `UiConfig` ScriptableObject assigned to your `UiManager` GameObject.
  - Inspect example states (search `UiState` subclasses in `Assets/Scripts/Ui` or project UI folders).

- Fast checklist to show a UI state in code:
  - Ensure `UiManager` has been initialized (call `await uiManager.Init()` during bootstrap).
  - Call `await UiManager.ShowStateByKey("MyStateKey", context)` or `await UiManager.ShowState<MyState>(context)`.

## Editor Tool: TA Tutorial — Creating a new UiState and UiElements

This step-by-step guide is written for Technical Artists (TA) or non-programmers who need to create new screens using the Editor tools and minimal C#.

Prerequisites:

- Unity Editor with Addressables package configured.
- `UiConfig` ScriptableObject instance created and assigned to `UiManager` in a scene (project bootstrap scene).

Step 1 — Prepare the scene

1. Create a new Scene in Unity for your screen (e.g., `UI_PlayerProfile.unity`).
2. Design UI visually using UI Toolkit or Unity UI; add root GameObjects for each logical element.
3. For each root element that needs runtime data, add a MonoBehaviour that inherits `UiElement` (see example below) or add an existing `UiElement` script.
4. Mark the Scene as Addressable: open Addressables Groups window, drag the scene into a group and create an `AssetReference`.

Step 2 — Create a UiElement script (small C#; TA can copy/paste)

1. Create a script under `Assets/Scripts/Ui/UIDisplayes/` (or similar) named `PlayerProfileUiElement.cs`.
2. Example (follow `.editorconfig` rules):

```csharp
using UiFramework.Core;
using TMPro;

public class PlayerProfileUiElement : UiElement
{
      [SerializeField] private TextMeshProUGUI nameLabel;

      public override void Populate(object context = null)
      {
            if (context is PlayerProfileContext p)
            {
                  nameLabel.text = p.DisplayName ?? "Player";
            }
            else
            {
                  nameLabel.text = "Player";
            }
      }
}

public class PlayerProfileContext
{
      public string DisplayName { get; set; }
}
```

Step 3 — Add the scene to `UiConfig`

1. Open your `UiConfig` ScriptableObject in the inspector (Assets → find `UiConfig`).
2. Add a new entry with:
   - `stateKey`: choose `PlayerProfileState` (we recommend the `State` suffix to match class names),
   - `uiElementScenes`: add the `AssetReference` to `UI_PlayerProfile` scene.

Step 4 — (Optional) Create a `UiState` subclass

1. If you need custom code when entering the state, create a subclass:

```csharp
using UiFramework.Core;

public class PlayerProfileState : UiState
{
      public PlayerProfileState(List<UnityEngine.AddressableAssets.AssetReference> scenes)
            : base("PlayerProfileState", scenes)
      {
      }
}
```

2. Note: the framework can work without a custom subclass — `UiState` default behavior is usually sufficient.

Step 5 — Show the state at runtime (test)

1. Create a small debug MonoBehaviour (Editor-only if you prefer) with a button that calls:

```csharp
await UiManager.ShowStateByKey("PlayerProfileState", new PlayerProfileContext { DisplayName = "Alex" });
```

2. Enter Play Mode and click the button — the Addressable scene should load and the UI elements' `Populate` methods should be executed.

Step 6 — Validate and iterate

- If UI does not appear, check these:
  - `UiConfig` entry `stateKey` matches your chosen class name (or use `ShowStateByKey`).
  - The scene is addressable and `AssetReference` is correct.
  - Console logs for Addressables load errors (missing groups, build not updated).

Editor Tools (where to look)

- `Assets/Scripts/UiFramework/Editor/UiStateRegistry.cs` — registry of states used by generator.
- `Assets/Scripts/UiFramework/Editor/CodeGeneration/UiStateGenerator.cs` — helper to create `UiState` class shells; use this if you want boilerplate classes generated.

TA tips

- Keep UI scenes small and focused so individual screens load quickly.
- Prefer one logical `UiElement` script per functional block (header, list, footer) to maximize reuse.
- Use `context` as a plain DTO (POCO) with only the data the UI needs; avoid passing large service objects.

## Suggested Editor Validation (manual)

1. In Editor, open `UiConfig` and iterate entries:
   - For each `AssetReference` click `Select` to open the referenced scene and visually confirm `IUiElement` components are present on root GameObjects.
2. Run a quick Play Mode smoke test that calls `UiManager.ShowStateByKey` for each entry and confirms scenes load without Addressable errors.

---

If you want, I can implement an Editor Window that automates the validation steps (non-destructive) and optionally scaffolds `UiState` classes using the generator. Mark this in the todo list if you'd like me to proceed.
