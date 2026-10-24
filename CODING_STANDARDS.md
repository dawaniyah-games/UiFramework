# Coding Standards & Rules

This is the single source of truth for how code is written in this repository: the package under `Packages/com.dawaniyahgames.uiframework/`, the sample project under `Assets/`, and the code templates the editor tools generate from. [`.editorconfig`](.editorconfig) enforces the mechanical parts in the IDE; this document covers those plus the rules a linter cannot check.

Other docs (README, CONTRIBUTING, AI-assistant instructions) link here instead of restating rules. If they disagree with this file, this file wins. Fix the other doc.

**Severity words:** **MUST** = required, PRs are blocked on it. **SHOULD** = default; deviate only with a reason stated in the PR. **MAY** = allowed.

**Checked automatically:** `bash tools/check-standards.sh` (run from the repository root, or pass the files to check) checks the rules marked *(checked)* below. It runs on every pull request and every push to `dev` / `main` ([`.github/workflows/standards.yml`](.github/workflows/standards.yml)). Legacy files that predate a rule are listed in [`tools/standards-baseline.txt`](tools/standards-baseline.txt); new code never goes in there.

---

## 0. How we work

The package's main consumer is **DenDon**, a multiplayer Ludo game in Unity. Everything here must look like it was written by the DenDon team: don't invent new styles, patterns or abstractions, and when in doubt, copy the nearest existing code.

- Read the existing code path before you edit. Don't guess the architecture.
- Keep changes small and aimed at the root cause. Don't refactor, rename or reformat code you weren't asked to touch.
- Prefer extending what exists over adding parallel abstractions, new base classes or "manager" layers.
- Keep public APIs, serialized fields, scene structure and asset GUIDs as they are unless the task says otherwise. Consumers wire these in scenes and prefabs, so a rename silently breaks them (§8).
- Every change goes through **implement → self-review** (bugs, regressions, missing validation, style drift) **→ commit prep**. Don't push or publish unless asked.
- After editing, make sure every assembly compiles (Core, Runtime, Tweening, Editor and the test assemblies), run `tools/check-standards.sh`, and run the tests (§11). Say clearly what you verified and what you didn't.
- Hand-off report: what changed and why, files touched, verification done (and not done), consumer impact with migration steps (§8), and the package version.

## 1. Formatting

Enforced by `.editorconfig`.

| Rule | Value |
| --- | --- |
| Indentation | 4 spaces, no tabs *(checked)* |
| Line endings | LF in the repository (normalized by `.gitattributes` `* text=auto`). Working copies may use CRLF on Windows. Shell scripts are always LF |
| Encoding | UTF-8, final newline, no trailing whitespace *(checked)* |
| Braces | Allman: every `{` on its own line, for types, methods, control blocks and lambdas with a block body. `else` / `catch` / `finally` start a new line *(checked)* |
| Braces on control blocks | **MUST** always be present, even for one-line `if`/`for`/`while` and early returns *(checked)* |
| Single-line blocks | Not allowed; `{ }` bodies are expanded onto separate lines, and statements are never packed onto one line *(checked)* |
| `using` directives | **MUST** be outside the namespace, at the top of the file, `System.*` first, no blank lines between groups *(checked)*. A few old files still have them inside; don't copy that, and don't mass-fix it either |
| Namespaces | Block-scoped (`namespace X { }`). File-scoped namespaces need C# 10, which Unity 2021.3 (the package minimum) does not support |

## 2. Naming

| Symbol | Style | Example |
| --- | --- | --- |
| Types (class, struct, enum, interface) | `PascalCase` | `UiManager` |
| Interfaces | `I` + `PascalCase` | `IUiElement` |
| Methods, properties, events | `PascalCase` | `ShowStateByKey`, `IsInitialized` |
| Private / internal fields, including `static readonly` | `camelCase`, **no** `_` or `m_` prefix *(checked)* | `stateStack`, `loadingState`, `closing` |
| Constants, at any access level, including private, local and test constants | `PascalCase` *(checked)* | `TweenPresets.FadeKey`, `SlideShowDuration`, `GlobalConfigsGroupName` |
| Serialized fields | `[SerializeField] private` + `camelCase` (§4.2) | `titleText`, `showAnimationPreset` |
| Booleans | Read as statements | `isVisible`, `hasStarted`, `closing`, `IsInitialized` |
| Parameters, locals | `camelCase` | `stateKey`, `sceneGuid` |
| Async methods returning `Task` | **SHOULD** end in `Async` | `ShowAsync`, `ResetUIAsync` |

- The existing public async API (`ShowState`, `ShowStateByKey`, `HideUI`, and `TweenPresets.ClickPop` / `SettlePop` / `ShakeLocal`) predates the `Async` suffix rule. Keep those names for compatibility. Every other method returning a `Task`, private ones included, follows the rule.
- UI types use the `Ui` casing, never `UI`, in new names: `UiState`, `UiElement`, `UiConfig` *(checked for type names)*.
- Suffixes: `*UiState` (states), `*UiElement` (scene elements), `*UiDisplay` (a reusable sub-widget MonoBehaviour, e.g. a card or a row), `*Context` or `*Params` (populate data), `*Signal` / `*SignalParam` (game side only).
- A state's `UiState` subclass name **MUST** equal its `stateKey` exactly (see [DESIGN.md §5.3](DESIGN.md#53-type--key-resolution)).
- Namespaces: `UiFramework.Core`, `UiFramework.Runtime.*`, `UiFramework.Tweening`, `UiFramework.Editor.*`. Generated consumer code lands in `UiFramework.Editor.States` / `UiFramework.Editor.Elements` by default (`UiEditorConfig`); don't change that default without a migration plan.
- Don't rename existing folders or files (including the misspelled `Core/UiCompoenets`) without approval. GUIDs, asmdefs and consumer references depend on them.

## 3. Language features

| Rule | Severity |
| --- | --- |
| No `var`. Always write the explicit type | **MUST** (error, *checked*) |
| No target-typed `new()`. Write `new List<string>()` | **MUST** (error, *checked*) |
| No expression-bodied methods, properties, indexers or accessors (`=>`). Use a block body | **MUST** (error, *checked*) |
| No try/catch for parsing or validation: check up front (null / empty / range / format guards) and reject bad input early. try/catch only around real async or IO boundaries (scene loads, Addressables handles, file writes), and it logs with context. Never swallow an exception silently | **MUST** |
| Lambdas and local functions are allowed (callbacks, LINQ in editor code) | MAY |
| Use `this.` only to disambiguate a field from a parameter of the same name | SHOULD |
| `out` variables MAY be declared inline (`TryGetValue(key, out UiStateEntry entry)`) | MAY |
| String interpolation (`$"..."`) and concatenation are both fine | MAY |

## 4. Unity rules

### 4.1 Null checks on Unity objects
- **MUST NOT** use `?.`, `??` or `is null` on anything derived from `UnityEngine.Object` (GameObject, Component, ScriptableObject, MonoScript…). Unity overloads `==` to report destroyed objects as null. The C# null operators bypass that overload and treat destroyed objects as alive.
- Use `if (obj == null)` / `if (obj != null)` instead. The C# operators are fine for plain C# objects (`context`, DTOs, collections).

### 4.2 Serialized fields
- New inspector fields **MUST** be `[SerializeField] private Type fieldName;`. No public fields for inspector wiring. Expose them through a property if other code needs them.
- The package's serialized fields are frozen: consumers' scenes, prefabs and assets store them by name, and a rename silently drops the data. Renaming one is a breaking change (§8). Once it is approved, rename it cleanly and give the migration steps in the consumer impact note; don't leave `[FormerlySerializedAs]` attributes behind.
- The sample (`Assets/`) owns its scenes, so it MAY rename its own serialized fields, as long as the same change updates its scenes.
- Some legacy data classes have public fields (`UiConfig.entries`, `UiStateEntry.stateKey`, `UiEditorConfig.*`, `UiStateRegistry.States`). They are part of the serialized format. Do not rename them, and do not copy the pattern into new code.

### 4.3 MonoBehaviour
- Lifecycle methods appear in execution order: `Awake → OnEnable → Start → Update/LateUpdate → OnDisable → OnDestroy`.
- Override a `protected virtual` lifecycle method from the base class (e.g. `UiElement.Awake`) and call `base.Awake()`. Do not hide it with a new method.
- `UiElement.Populate` **MUST** be idempotent: the framework calls it again on the same instance every time a new state reuses that element's scene (see [DESIGN.md §6](DESIGN.md#6-shared-scenes-and-shared-elements)). Guard one-time setup with a flag, as `NavBarUiElement` does.
- Cache `GetComponent` results. No per-frame allocations, LINQ or `GetComponent` in hot paths (`Update`, tween callbacks).
- Null-guard Unity objects with `== null` and return early; avoid deep nesting.
- UI timers and tweens use unscaled time (`Time.unscaledDeltaTime`), so UI keeps moving when the game is paused.
- Responsiveness comes from the `CanvasScaler` and normal anchors only. The framework **MUST NOT** add runtime scripts that drive anchors, size, scale or safe areas.
- A static facade over one private instance (`UiManager`) is the manager pattern. Don't add new singletons casually.

### 4.4 Assets & meta files
- Every new file or folder under `Assets/` or `Packages/` **MUST** be committed together with its `.meta` file. Never hand-edit or regenerate GUIDs.
- Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or generated `*.csproj` / `*.sln`.
- The generated runtime `UiConfig` asset is build output. Change it through *Build Runtime UiConfig* in the editor window, never by hand (see [DESIGN.md §8](DESIGN.md#8-editor-pipeline)).

## 5. Async & threading

- Asynchronous framework code uses `System.Threading.Tasks.Task` and `await`. Continuations run on Unity's main thread via its `SynchronizationContext`. Don't introduce UniTask, Rx or coroutine-only APIs: consumers call the API from plain `Task` code.
- `async void` is allowed **only** for Unity messages (`Start`, button/event handlers) and top-level fire-and-forget entry points. Such a method **MUST** catch and log exceptions. An exception escaping `async void` is lost or crashes the player.
- **MUST NOT** block on a task: no `.Wait()`, `.Result` or `.GetAwaiter().GetResult()` on the main thread *(checked, except `.Result`)*. Addressables completes on the main thread, so blocking it deadlocks.
- Awaiting an `AsyncOperationHandle` **MUST** be followed by a check of `handle.Status == AsyncOperationStatus.Succeeded` before reading `handle.Result`.
- Cleanup that must always run (scene release, stack updates, `loadingState` reset) belongs in `finally`.
- Runtime hot paths (`UiManager`, `UiState`, tweening) **SHOULD** use plain `for` loops and avoid LINQ and per-frame allocations. LINQ is fine in editor code and one-time init.

## 6. Errors & logging

- Public framework API **SHOULD** log and return rather than throw on bad input or missing setup (unknown state key, UiManager not initialized, failed Addressables load). This matches the existing `UiManager` behaviour, and a UI failure should not take down the game loop.
- Exceptions from user code (e.g. a `Populate` override) during a transition are caught and logged, and cleanup runs in `finally` (see `UiManager.LoadAndPushStateAsync`).
- Every log message **MUST** start with a tag *(checked)*. Runtime code uses `[ClassName]`: `Debug.LogError("[UiManager] State 'X' not found in cache.");`. Editor tooling, whose messages are tool output for designers, uses `[UiFramework]`.
- Format: `"[Tag] What happened. What to do about it."` Pass the context object when it helps find the culprit: `Debug.LogWarning(message, this)`.
- Use `Debug.LogError` for broken setup or failed operations, `Debug.LogWarning` for recoverable or suspicious states, and `Debug.Log` only for operations a developer would want traced. No duplicate logs, no debug spam left in, and nothing logged per frame.
- Log messages **MUST NOT** contain emoji *(checked)*.
- Fail clearly: an unknown state key, a missing `UiConfig` or an invalid preset gets one clear log and a safe early return. Never silently tolerate a broken contract.

## 7. Architecture rules

Background for each rule is in [DESIGN.md](DESIGN.md).

1. **Assembly direction is fixed:** `Tweening ← Core ← Runtime ← Editor`. A lower assembly **MUST NOT** reference a higher one. `UiFramework.Editor` is Editor-platform only, and runtime assemblies **MUST NOT** reference `UnityEditor` outside `#if UNITY_EDITOR`.
2. **`IUiElement.Populate(object context)` is the core contract.** Do not change its signature. Add strongly-typed helpers on top of it instead.
3. **`UiManager` is the only owner of scene loading and unloading.** Elements and states **MUST NOT** call `Addressables.LoadSceneAsync` / `UnloadSceneAsync` or `SceneManager.UnloadSceneAsync` on UI scenes. Doing so breaks the reference counting.
4. **One `UiManager`.** Create it only through `UiManager.InitializeAsync()`. Never add it to a scene or instantiate it by hand.
5. **`UiState` subclasses are type markers.** The runtime always constructs a base `UiState` from `UiConfig`, so subclass constructors and overrides never run. Do not put behaviour there.
6. **Identify scenes by GUID (`AssetReference.AssetGUID`), never by scene name.**
7. **New tween presets** live entirely in `TweenPresets`: key constant + `presetKeys` list + `PlayAsync` branch + start pose in `PrepareShow` + text in `Describe`. Run them through `Tween.RunAsync` with a `UiTweenState` run so they can be interrupted, and start from / return to the base pose. Entrances ease out, exits ease in, and full-screen motion never overshoots or bounces.
8. **The package stays dependency-light:** Unity built-ins plus `com.unity.addressables`. A new package dependency needs a `package.json` change and maintainer approval.
9. **Editor layers:** `Editor/Scenes`, `Editor/Config`, `Editor/Data` and `Editor/CodeGeneration` hold editor *logic* and **MUST NOT** reference `Editor/Window` (UI). When logic needs a decision from the user, it takes a callback (e.g. `UiElementSceneCreator.Options.ConfirmStructureFix`); the window shows the dialog. Window classes stay small: one tab or panel per class, with popup wording in `UiElementChangeDialogs`.
10. **The framework stays game-agnostic.** It **MUST NOT** reference or depend on game concepts: ServiceLocator, services, DataReaders, Signals, Photon / Fusion, TaskRunner, FlexibleImage, RTLTextMeshPro, fonts, colours or localisation. Allowed dependencies: UnityEngine, UGUI, Addressables, and NUnit in tests.
11. **The element scene contract** (`<Name>Root → Canvas → Root`, [DESIGN.md §7.1](DESIGN.md#71-animation-target)) is what every consumer scene follows. Nothing may sit next to `Root` under the Canvas, or it won't animate. Validator and fixer changes keep the issue codes and messages stable and never auto-delete user objects: report them, and let the user move or delete them.

## 8. Public API & versioning

The public surface is everything `public` or `protected` in `UiFramework.Core`, `UiFramework.Runtime` and `UiFramework.Tweening`, plus the serialized shape of `UiConfig` and `UiElement`, plus the template placeholders.

- Versioning follows [SemVer](https://semver.org/): **patch** for fixes, **minor** for backwards-compatible features, **major** for breaking changes to the public surface. A breaking change needs explicit approval before it is made.
- Prefer additive changes. To retire a member, mark it `[System.Obsolete("Use X instead.")]` for at least one minor release before removing it.
- Bump `version` in `Packages/com.dawaniyahgames.uiframework/package.json` with every change to the package, and add a dated `CHANGELOG.md` section **before** merging `dev → main`. The release workflow fails if the tag already exists. Changes only to the sample (`Assets/`) don't bump the version.
- A change that affects consumers comes with a short **consumer impact** note (in the PR and the hand-off report): what changed in the API, and the exact migration steps for DenDon.

API the game depends on, frozen unless a breaking change is approved:

```
UiManager.InitializeAsync(UiState defaultState = null)
UiManager.ShowState<T>(object context = null, bool additive = false, UiTransition transition = null)
UiManager.ShowStateByKey(string stateKey, object context = null, bool additive = false, UiTransition transition = null)
UiManager.HideUI(UiTransition transition = null)
UiManager.ResetUIAsync(), UiManager.GetCurrentState(), UiManager.IsInitialized
UiState(string stateName, List<AssetReference> uiElementScenes), UiState.StateName
UiElement.Populate / ShowAsync(string presetKey = null) / HideAsync(string presetKey = null) / PrepareForShow(...)
UiElement serialized fields: showAnimationPreset, hideAnimationPreset
UiPopulationParams, UiElementReference, UiTransition.Default / ShowWith / HideWith
```

How the game calls it; stay compatible with these patterns:

```csharp
TaskRunner.Start(UiManager.ShowState<ShopUiState>(additive: false));                // from button handlers
TaskRunner.Start(UiManager.ShowState<ReportPopupUiState>(report, additive: true));  // popup with context
await UiManager.HideUI();                                                            // close a popup
// Context is a plain class or a primitive, read with "context as MyContext" or "context is int coins".
```

## 9. Code generation templates

Templates live in `Packages/com.dawaniyahgames.uiframework/Editor/Templates/`.

- Template output **MUST** comply with this document: generated code is project code the day it is created. The templates are checked like code *(checked)*.
- Generators **MUST NOT** overwrite an existing file: they log a warning and stop.
- Placeholders are `[UiElementName]`, `[UiElementNamespace]`, `[UiReferenceName]`, `[UiReferenceNamespace]`, `[UiParamName]`, `[UiParamNamespace]`, `[UiStateName]` and `[UiStateNamespace]`. Renaming a placeholder is a breaking change, because user projects may have their own copies of the templates.

## 10. Comments & documentation

- Comment *why*, or a non-obvious contract, in short plain sentences. Don't narrate the code line by line. Keep comment density close to the surrounding code; this codebase comments sparingly.
- Public API and anything consumers call **SHOULD** have a short `//` or `/// <summary>` explaining usage, as in `UiTransition.cs`.
- No commented-out code in the package, no TODO without context, and no "Auto-generated" banner on hand-written code. The sample project MAY keep short commented examples.
- A change to behaviour, public API, editor workflow or Addressables layout **MUST** update [DESIGN.md](DESIGN.md) and, where it affects users, [README.md](README.md) in the same PR.

## 11. Testing & verification

Automated tests live in `Packages/com.dawaniyahgames.uiframework/Tests/`:

- `Tests/Editor` (EditMode): the editor tools, e.g. scene validation and fixes, the fix preview, canvas settings, search.
- `Tests/Runtime` (PlayMode): animations played frame by frame, e.g. slide directions, base pose, interruptions, curve shapes.
- `Tests/Support`: test-only MonoBehaviours (Editor-only assemblies can't hold components).

Run them from *Window → General → Test Runner* (both EditMode and PlayMode tabs) before opening a PR, together with `bash tools/check-standards.sh`. A change to `UiManager` transitions, scene ref counting, `TweenPresets` or the structure validator **MUST** add or extend a test; other editor-tool changes **SHOULD**.

Tests use NUnit, follow every rule in this document, test one behaviour each, and clean up their temporary assets and scenes in `TearDown`.

Tests don't cover the full UiManager flow (Addressables loading, state switching). Every runtime change **MUST** still be checked in Play Mode with the sample project:

1. *Window → UI Framework → UI Setup Manager → General → Build Runtime UiConfig*, then build Addressables content (or use *Use Asset Database* play mode).
2. Run the bootstrap scene and switch states with the NavBar, or use `UiNavigationSmokeTest` (keys `1` / `2` / `` ` ``).
3. Confirm: no console errors, no leftover scenes in the Hierarchy after switching, no UI flash, and shared elements (NavBar) do not re-animate.

## 12. Git workflow

The details are in [CONTRIBUTING.md](CONTRIBUTING.md).

- Branches: `feature/*` (also `fix/*`, `docs/*`) off `dev` → PR to `dev` → release PR `dev → main`.
- Commit subjects are imperative, sentence case, with no prefix and no trailing period, about 72 characters or less, and describe the outcome: "Handle suspended and deleted accounts from any backend or Photon call", "Animate match chat bubbles and tidy player panels".
- Group commits by intent (runtime fix / editor tooling / tests / version bump), not by file, and keep unrelated reformatting out of them.
- Don't push or publish unless asked.

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

The rules above apply to all new and modified code. These pre-existing deviations stay until the file changes for a real reason; fix them then, in a separate commit, never as a mass clean-up. The ones `tools/check-standards.sh` would flag are listed in [`tools/standards-baseline.txt`](tools/standards-baseline.txt); remove the line there once the file is fixed.

| Location | Deviation |
| --- | --- |
| `Core/UiCompoenets/IUiElement.cs`; `Editor/Window/Tabs/IVisualElementTab.cs`, `BaseVisualElementTab.cs` | `using` inside the namespace |
| `Core/Config/UiConfig.cs` | Lives in the `UiFramework.Core` assembly but declares `namespace UiFramework.Runtime` (kept for compatibility) |
| `UiConfig.entries`, `UiStateEntry.*`, `UiEditorConfig.*`, `UiStateRegistry.*` | Public serialized fields (§4.2); part of the serialized format, so they stay until an approved breaking change |
| Folder `Core/UiCompoenets/` | Misspelled; renaming moves GUIDs/paths for users, so it is deferred to a major version |
