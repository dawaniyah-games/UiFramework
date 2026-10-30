# UI Framework: AI Assistant Instructions

Unity UPM package `com.dawaniyahgames.uiframework` (source in `Packages/com.dawaniyahgames.uiframework/`), plus a sample host project in `Assets/`. Unity 6 (6000.0) or newer, Addressables 2.x, C# 9.

## Read first: these are authoritative

- **[CODING_STANDARDS.md](../CODING_STANDARDS.md)**: all code you write or generate MUST follow it.
- **[DESIGN.md](../DESIGN.md)**: architecture, lifecycle and invariants. Check your change against §10 (Invariants) and §11 (Known limitations).

Don't restate or override those documents here. If they and this file disagree, they win.

## Non-negotiable rules (the most commonly broken)

`bash tools/check-standards.sh` checks most of these; CI runs it on every PR.

1. **No `var`, no target-typed `new()`, no expression-bodied members (`=>`)**, and braces on every block, Allman style.
2. `using` directives go **outside** a block-scoped `namespace`.
3. Private fields are `camelCase`, with no `_` or `m_`. Constants are `PascalCase` at every access level. Inspector fields are `[SerializeField] private`.
4. Never use `?.` / `??` on `UnityEngine.Object` types. Use `== null`.
5. Never `.Wait()` / `.Result` a Task. `async void` only for Unity messages and event handlers, with try/catch. No try/catch for validation: guard up front.
6. Logs read `"[ClassName] What happened. What to do about it."` (`[UiFramework]` in editor tooling), with no emoji and no duplicates.
7. Assembly direction is `Tweening ← Core ← Runtime ← Editor`. Never reference upward, never use `UnityEditor` in runtime code, and never reference game code (ServiceLocator, Signals, Photon, TaskRunner, TMP, localisation).
8. The public API and the package's serialized fields are frozen; changing them is a breaking change that needs approval and a consumer impact note. Never change `IUiElement.Populate(object context)`.
9. UI element scenes follow the contract `<Name>Root → Canvas → one stretched Root` (DESIGN.md §7.1). Editor tools edit open scenes in place (Undo, no save), never delete objects, and never touch Canvas sorting. Generators never overwrite an existing file.
10. Commit subjects: imperative, sentence case, no prefix, about 72 characters. Don't push unless asked.

## Mental model (details in DESIGN.md)

- `UiManager.InitializeAsync()` creates a DontDestroyOnLoad singleton and loads `UiConfig` from Addressables (address `UiConfig`).
- `ShowStateByKey(key, context, additive, transition)` builds a **base** `UiState` from `UiConfig`, loads its element scenes by GUID with ref counting, calls `Populate(context)` on every `IUiElement`, then plays overlapping hide/show transitions. Elements already on screen are skipped.
- `UiState` subclasses are **only type markers** for `ShowState<T>()`. Their class name must equal the state key, and their code never runs.
- `Populate` can run several times on the same instance (shared scenes), so it must be idempotent.
- An optional `UiTransition` overrides the show / hide animation for one call only; never change the elements' configured presets for a one-off.
- The runtime `UiConfig` asset is **generated** by *Window → UI Framework → UI Setup Manager → General → Build Runtime UiConfig* from `UiStateDefinition` assets. Don't hand-edit it.

## When changing things

- New tween preset: only in `TweenPresets` (key + list + `PlayAsync` + `PrepareShow` + `Describe`), run via `Tween.RunAsync` with a `UiTweenState` run. Slides move in their named direction on show and hide.
- New or changed generated code: edit `Editor/Templates/*.txt`. The output must satisfy CODING_STANDARDS.
- Behaviour, API or editor workflow change: update DESIGN.md (and README.md if user-facing) and add a `CHANGELOG.md` `[Unreleased]` entry in the same change.
- Version bumps: `package.json` `version`, SemVer. Release steps are in CONTRIBUTING.md.
- The root `CHANGELOG.md` is the source of truth. After editing it, copy it into `Packages/com.dawaniyahgames.uiframework/`; `tools/check-standards.sh` fails when the copies differ. Never add a `LICENSE` file to the package: the Asset Store rejects packages with their own license (the root `LICENSE.md` covers the repository).
- Run `bash tools/check-standards.sh` and the EditMode and PlayMode tests in `Packages/com.dawaniyahgames.uiframework/Tests/`, and verify runtime changes in Play Mode with the sample project (CODING_STANDARDS §11).
