# Coding Standards & Rules

This is the single source of truth for how code is written in this repository: the package under `Packages/com.dawaniyahgames.uiframework/`, the sample project under `Assets/`, and the code templates the editor tools generate from. [`.editorconfig`](.editorconfig) enforces the mechanical parts in the IDE; this document covers those plus the rules a linter cannot check.

Other docs (README, CONTRIBUTING, AI-assistant instructions) link here instead of restating rules. If they disagree with this file, this file wins. Fix the other doc.

**Severity words:** **MUST** = required, PRs are blocked on it. **SHOULD** = default; deviate only with a reason stated in the PR. **MAY** = allowed.

---

## 1. Formatting

Enforced by `.editorconfig`.

| Rule | Value |
| --- | --- |
| Indentation | 4 spaces, no tabs |
| Line endings | LF in the repository (normalized by `.gitattributes` `* text=auto`). Working copies may use CRLF on Windows |
| Encoding | UTF-8, final newline, no trailing whitespace |
| Braces | Allman: every `{` on its own line |
| Braces on control blocks | **MUST** always be present, even for one-line `if`/`for`/`while` |
| Single-line blocks | Not allowed; `{ }` bodies are expanded onto separate lines |
| `using` directives | **MUST** be outside the namespace, at the top of the file, `System.*` first, no blank lines between groups |
| Namespaces | Block-scoped (`namespace X { }`). File-scoped namespaces need C# 10, which Unity 2021.3 (the package minimum) does not support |

## 2. Naming

| Symbol | Style | Example |
| --- | --- | --- |
| Types (class, struct, enum, interface) | `PascalCase` | `UiManager` |
| Interfaces | `I` + `PascalCase` | `IUiElement` |
| Methods, properties, events | `PascalCase` | `ShowStateByKey`, `IsInitialized` |
| Private / internal fields, including `const` and `static readonly` | `camelCase`, **no** `_` or `m_` prefix | `stateStack`, `globalConfigsGroupName` |
| Public / protected constants | `PascalCase` | `TweenPresets.FadeKey` |
| Parameters, locals | `camelCase` | `stateKey`, `sceneGuid` |
| Async methods returning `Task` | **SHOULD** end in `Async` | `LoadSceneIfNeededAsync` |

- The existing public async API (`ShowState`, `ShowStateByKey`, `HideUI`) predates the `Async` suffix rule. Keep those names for compatibility. New methods follow the rule.
- UI types use the `Ui` casing, never `UI`: `UiState`, `UiElement`, `UiConfig`.
- App-side states and elements **SHOULD** end in `UiState` / `UiElement`, e.g. `MainMenuUiState`, `StoreUiElement`.
- A state's `UiState` subclass name **MUST** equal its `stateKey` exactly (see [DESIGN.md §5.3](DESIGN.md#53-type--key-resolution)).

## 3. Language features

| Rule | Severity |
| --- | --- |
| No `var`. Always write the explicit type | **MUST** (error) |
| No target-typed `new()`. Write `new List<string>()` | **MUST** (error) |
| No expression-bodied methods, properties, indexers or accessors (`=>`). Use a block body | **MUST** (error) |
| Lambdas and local functions are allowed (callbacks, LINQ in editor code) | MAY |
| Use `this.` only to disambiguate a field from a parameter of the same name | SHOULD |
| `out` variables MAY be declared inline (`TryGetValue(key, out UiStateEntry entry)`) | MAY |
| String interpolation (`$"..."`) and concatenation are both fine | MAY |

## 4. Unity rules

### 4.1 Null checks on Unity objects
- **MUST NOT** use `?.`, `??` or `is null` on anything derived from `UnityEngine.Object` (GameObject, Component, ScriptableObject, MonoScript…). Unity overloads `==` to report destroyed objects as null. The C# null operators bypass that overload and treat destroyed objects as alive.
- Use `if (obj == null)` / `if (obj != null)` instead. The C# operators are fine for plain C# objects (`context`, DTOs, collections).

### 4.2 Serialized fields
- New inspector fields **MUST** be `[SerializeField] private Type fieldName;`. Expose them through a property if other code needs them.
- **MUST NOT** rename or remove a serialized field without `[FormerlySerializedAs("oldName")]`, because existing scenes and assets in user projects silently lose their data.
- Some legacy data classes have public fields (`UiConfig.entries`, `UiStateEntry.stateKey`, `UiEditorConfig.*`, `UiStateRegistry.States`). They are part of the serialized format. Do not rename them, and do not copy the pattern into new code.

### 4.3 MonoBehaviour
- Lifecycle methods appear in execution order: `Awake → OnEnable → Start → Update/LateUpdate → OnDisable → OnDestroy`.
- Override a `protected virtual` lifecycle method from the base class (e.g. `UiElement.Awake`) and call `base.Awake()`. Do not hide it with a new method.
- `UiElement.Populate` **MUST** be idempotent: the framework calls it again on the same instance every time a new state reuses that element's scene (see [DESIGN.md §6](DESIGN.md#6-shared-scenes-and-shared-elements)). Guard one-time setup with a flag, as `NavBarUiElement` does.

### 4.4 Assets & meta files
- Every new file or folder under `Assets/` or `Packages/` **MUST** be committed together with its `.meta` file. Never hand-edit or regenerate GUIDs.
- Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or generated `*.csproj` / `*.sln`.
- The generated runtime `UiConfig` asset is build output. Change it through *Build Runtime UiConfig* in the editor window, never by hand (see [DESIGN.md §8](DESIGN.md#8-editor-pipeline)).

## 5. Async & threading

- Asynchronous framework code uses `System.Threading.Tasks.Task` and `await`. Continuations run on Unity's main thread via its `SynchronizationContext`.
- `async void` is allowed **only** for Unity messages (`Start`, button/event handlers) and top-level fire-and-forget entry points. Such a method **MUST** catch and log exceptions. An exception escaping `async void` is lost or crashes the player.
- **MUST NOT** block on a task: no `.Wait()`, `.Result` or `.GetAwaiter().GetResult()` on the main thread. Addressables completes on the main thread, so blocking it deadlocks.
- Awaiting an `AsyncOperationHandle` **MUST** be followed by a check of `handle.Status == AsyncOperationStatus.Succeeded` before reading `handle.Result`.
- Cleanup that must always run (scene release, stack updates, `loadingState` reset) belongs in `finally`.
- Runtime hot paths (`UiManager`, `UiState`, tweening) **SHOULD** use plain `for` loops and avoid LINQ and per-frame allocations. LINQ is fine in editor code and one-time init.

## 6. Errors & logging

- Public framework API **SHOULD** log and return rather than throw on bad input or missing setup (unknown state key, UiManager not initialized, failed Addressables load). This matches the existing `UiManager` behaviour, and a UI failure should not take down the game loop.
- Exceptions from user code (e.g. a `Populate` override) during a transition are caught and logged, and cleanup runs in `finally` (see `UiManager.LoadAndPushState`).
- Every log message **MUST** start with a `[ClassName]` tag: `Debug.LogError("[UiManager] State 'X' not found.");`
- Use `Debug.LogError` for broken setup or failed operations, `Debug.LogWarning` for recoverable or suspicious states, and `Debug.Log` only for operations a developer would want traced. No log spam in per-frame code.
- New log messages **MUST NOT** contain emoji. Older messages in the editor tools still do; replace them when you touch those lines.
- Error messages **SHOULD** say what to do next, e.g. "Ensure the UI Editor Window generated the runtime config…".

## 7. Architecture rules

Background for each rule is in [DESIGN.md](DESIGN.md).

1. **Assembly direction is fixed:** `Tweening ← Core ← Runtime ← Editor`. A lower assembly **MUST NOT** reference a higher one. `UiFramework.Editor` is Editor-platform only, and runtime assemblies **MUST NOT** reference `UnityEditor` outside `#if UNITY_EDITOR`.
2. **`IUiElement.Populate(object context)` is the core contract.** Do not change its signature. Add strongly-typed helpers on top of it instead.
3. **`UiManager` is the only owner of scene loading and unloading.** Elements and states **MUST NOT** call `Addressables.LoadSceneAsync` / `UnloadSceneAsync` or `SceneManager.UnloadSceneAsync` on UI scenes. Doing so breaks the reference counting.
4. **One `UiManager`.** Create it only through `UiManager.InitializeAsync()`. Never add it to a scene or instantiate it by hand.
5. **`UiState` subclasses are type markers.** The runtime always constructs a base `UiState` from `UiConfig`, so subclass constructors and overrides never run. Do not put behaviour there.
6. **Identify scenes by GUID (`AssetReference.AssetGUID`), never by scene name.**
7. **New tween presets** are added to `TweenPresets` (key constant + `presetKeys` list + `PlayAsync` branch) **and** to `UiElement.PrepareForShow`. If `PrepareForShow` is missed, the element flashes at its final pose before animating in.
8. **The package stays dependency-light:** Unity built-ins plus `com.unity.addressables`. A new package dependency needs a `package.json` change and maintainer approval.

## 8. Public API & versioning

The public surface is everything `public` or `protected` in `UiFramework.Core`, `UiFramework.Runtime` and `UiFramework.Tweening`, plus the serialized shape of `UiConfig` and `UiElement`, plus the template placeholders.

- Versioning follows [SemVer](https://semver.org/): **patch** for fixes, **minor** for backwards-compatible features, **major** for breaking changes to the public surface.
- Prefer additive changes. To retire a member, mark it `[System.Obsolete("Use X instead.")]` for at least one minor release before removing it.
- Bump `version` in `Packages/com.dawaniyahgames.uiframework/package.json` and add a dated `CHANGELOG.md` section **before** merging `dev → main`. The release workflow fails if the tag already exists.

## 9. Code generation templates

Templates live in `Packages/com.dawaniyahgames.uiframework/Editor/Templates/`.

- Template output **MUST** comply with this document: generated code is project code the day it is created.
- Placeholders are `[UiElementName]`, `[UiElementNamespace]`, `[UiReferenceName]`, `[UiReferenceNamespace]`, `[UiParamName]`, `[UiParamNamespace]`, `[UiStateName]` and `[UiStateNamespace]`. Renaming a placeholder is a breaking change, because user projects may have their own copies of the templates.

## 10. Comments & documentation

- Comment *why*, not *what*. Keep comment density close to the surrounding code; this codebase comments sparingly.
- Public API members **SHOULD** have `///` XML docs when added or changed.
- No commented-out code in the package. The sample project MAY keep short commented examples.
- A change to behaviour, public API, editor workflow or Addressables layout **MUST** update [DESIGN.md](DESIGN.md) and, where it affects users, [README.md](README.md) in the same PR.

## 11. Testing & verification

The repo has no automated tests yet. `com.unity.test-framework` is installed, and new tests go in `Packages/com.dawaniyahgames.uiframework/Tests/{Editor,Runtime}` with their own asmdefs.

Until tests exist, every runtime change **MUST** be checked in Play Mode with the sample project:

1. *UiFramework → UI Setup Manager → General → Build Runtime UiConfig*, then build Addressables content (or use *Use Asset Database* play mode).
2. Run the bootstrap scene and switch states with the NavBar, or use `UiNavigationSmokeTest` (keys `1` / `2` / `` ` ``).
3. Confirm: no console errors, no leftover scenes in the Hierarchy after switching, no UI flash, and shared elements (NavBar) do not re-animate.

## 12. Git workflow

The details are in [CONTRIBUTING.md](CONTRIBUTING.md).

- Branches: `feature/*` (also `fix/*`, `docs/*`) off `dev` → PR to `dev` → release PR `dev → main`.
- Commit messages use [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`, `ci:`, with an optional scope, e.g. `feat(editor): …`.
- One logical change per commit, and no unrelated reformatting in the same commit.

---

## Appendix A: Reference example

```csharp
using System.Collections.Generic;
using UiFramework.Core;
using UnityEngine;

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

        private readonly List<GameObject> spawnedSlots = new List<GameObject>();

        public int SlotCount
        {
            get
            {
                return spawnedSlots.Count;
            }
        }

        public override void Populate(object context = null)
        {
            base.Populate(context);

            // Populate runs again whenever a new state reuses this scene, so rebuild instead of appending.
            ClearSlots();

            InventoryUiParams data = context as InventoryUiParams;
            if (data == null || data.ItemIds == null)
            {
                return;
            }

            if (slotContainer == null || slotPrefab == null)
            {
                Debug.LogWarning("[InventoryUiElement] slotContainer or slotPrefab is not assigned.");
                return;
            }

            for (int i = 0; i < data.ItemIds.Count; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotContainer);
                slot.name = data.ItemIds[i];
                spawnedSlots.Add(slot);
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

## Appendix B: Known deviations in existing code

The rules above apply to all new and modified code. These pre-existing violations are tracked for cleanup. Fix them when you touch the file, in a separate `refactor:` commit.

| Location | Deviation |
| --- | --- |
| `Core/UiCompoenets/IUiElement.cs`, `UiElement.cs`, `UiElementReference.cs`, `UiPopulationParams.cs`; `Editor/Window/Tabs/IVisualElementTab.cs`, `BaseVisualElementTab.cs` | `using` inside namespace; `UiElement.Populate` and `UiPopulationParams` use single-line `{ }` |
| `Core/Config/UiConfig.cs`, `Editor/Data/UiStateRegistry.cs`, `Editor/Window/UiSetupEditorWindow.cs` | Target-typed `new()` |
| `Core/Config/UiConfig.cs` | Lives in the `UiFramework.Core` assembly but declares `namespace UiFramework.Runtime` (kept for compatibility) |
| `Runtime/Tweening/TweenPresets.cs` | Private constants in `PascalCase` |
| `Runtime/Manager/UiManager.cs` | `ResetUI()` blocks with `Task.Wait()` (deadlock risk, §5); emoji in log messages; `IsLastSceneActive<T>()` calls `Peek()` without checking for an empty stack |
| `Editor/CodeGeneration/UiElementScriptBinder.cs` | Declared in the global namespace; uses `?.` on a `MonoScript` (§4.1) |
| `Editor/**` | Emoji and missing `[ClassName]` tags in log messages |
| Folder `Core/UiCompoenets/` | Misspelled; renaming moves GUIDs/paths for users, so it is deferred to a major version |
