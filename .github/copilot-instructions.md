# UI Framework: AI Assistant Instructions

Unity UPM package `com.dawaniyahgames.uiframework` (source in `Packages/com.dawaniyahgames.uiframework/`), plus a sample host project in `Assets/`. Unity 2021.3+ (the host project uses Unity 6), Addressables, C# 9.

## Read first: these are authoritative

- **[CODING_STANDARDS.md](../CODING_STANDARDS.md)**: all code you write or generate MUST follow it.
- **[DESIGN.md](../DESIGN.md)**: architecture, lifecycle and invariants. Check your change against §10 (Invariants) and §11 (Known limitations).

Don't restate or override those documents here. If they and this file disagree, they win.

## Non-negotiable rules (the most commonly broken)

1. **No `var`, no target-typed `new()`, no expression-bodied members (`=>`)**, and braces on every block, Allman style.
2. `using` directives go **outside** a block-scoped `namespace`.
3. Private fields, including `const`, are `camelCase`, with no `_` or `m_`. Inspector fields are `[SerializeField] private`.
4. Never use `?.` / `??` on `UnityEngine.Object` types. Use `== null`.
5. Never `.Wait()` / `.Result` a Task. `async void` only for Unity messages and event handlers, with try/catch.
6. Logs start with `[ClassName]` (runtime) or `[UiFramework]` (editor tooling) and contain no emoji.
9. UI element scenes follow the contract `<Name>Root → Canvas → one stretched Root` (DESIGN.md §7.1). Editor tools edit open scenes in place (Undo, no save), never delete objects, and never touch Canvas sorting.
7. Assembly direction is `Tweening ← Core ← Runtime ← Editor`. Never reference upward, and never use `UnityEditor` in runtime code.
8. Never rename a serialized field without `[FormerlySerializedAs]`. Never change `IUiElement.Populate(object context)`.

## Mental model (details in DESIGN.md)

- `UiManager.InitializeAsync()` creates a DontDestroyOnLoad singleton and loads `UiConfig` from Addressables (address `UiConfig`).
- `ShowStateByKey(key, context, additive, transition)` builds a **base** `UiState` from `UiConfig`, loads its element scenes by GUID with ref counting, calls `Populate(context)` on every `IUiElement`, then plays overlapping hide/show transitions. Elements already on screen are skipped.
- `UiState` subclasses are **only type markers** for `ShowState<T>()`. Their class name must equal the state key, and their code never runs.
- `Populate` can run several times on the same instance (shared scenes), so it must be idempotent.
- An optional `UiTransition` overrides the show / hide animation for one call only; never change the elements' configured presets for a one-off.
- The runtime `UiConfig` asset is **generated** by *UiFramework → UI Setup Manager → General → Build Runtime UiConfig* from `UiStateDefinition` assets. Don't hand-edit it.

## When changing things

- New tween preset: only in `TweenPresets` (key + list + `PlayAsync` + `PrepareShow` + `Describe`), run via `Tween.RunAsync` with a `UiTweenState` run. Slides move in their named direction on show and hide.
- New or changed generated code: edit `Editor/Templates/*.txt`. The output must satisfy CODING_STANDARDS.
- Behaviour, API or editor workflow change: update DESIGN.md (and README.md if user-facing) and add a `CHANGELOG.md` `[Unreleased]` entry in the same change.
- Version bumps: `package.json` `version`, SemVer. Release steps are in CONTRIBUTING.md.
- Run the EditMode and PlayMode tests in `Packages/com.dawaniyahgames.uiframework/Tests/`, and verify runtime changes in Play Mode with the sample project (CODING_STANDARDS §11).
