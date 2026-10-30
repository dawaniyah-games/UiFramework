# UI Framework: System Design

> Package `com.dawaniyahgames.uiframework` · describes **v1.3.10** · last reviewed 2026-10-30
>
> This document describes how the framework works *today*. If you change behaviour, update the matching section in the same PR ([CODING_STANDARDS.md §10](CODING_STANDARDS.md#10-comments--documentation)).

---

## 1. Purpose

The framework builds a game's UI from **Addressable scenes** that are loaded additively at runtime. Each screen is a named **UI state** made of one or more **element scenes**. `UiManager` keeps a LIFO stack of states. It loads the scenes a state needs, hands runtime data to the scene's elements, plays show/hide animations, and unloads scenes nothing references any more.

Goals:
- Screens load and unload independently, so memory follows what is on screen.
- Elements shared between screens (a nav bar, a currency header) stay loaded and do not re-animate on every switch.
- Designers and TAs can author screens through an editor window with minimal C#.

Non-goals: input routing, navigation history beyond the stack, data binding, localisation.

## 2. Vocabulary

| Term | Meaning |
| --- | --- |
| **Element scene** | A Unity scene containing one root GameObject `<Name>Root` with a `UiElement` component. It is the unit of loading. |
| **UiElement** | `MonoBehaviour` implementing `IUiElement`. Receives data through `Populate(object context)` and owns its show/hide animation presets. |
| **State** | A named set of element scenes, identified by a **state key** (string). At runtime it is a `UiState` instance. |
| **State key** | The string identifying a state in `UiConfig`. It **must** equal the name of the matching `UiState` subclass if you use `ShowState<T>()`. |
| **Context** | The `object` passed to `ShowState*`, forwarded unchanged to every element's `Populate`. Usually a `UiPopulationParams` subclass. |
| **Shared scene / element** | A scene used by more than one state that is already loaded when the next state needs it. |

## 3. Package layout & assemblies

```
Packages/com.dawaniyahgames.uiframework/
├── Runtime/Tweening/   → UiFramework.Tweening   Tween, TweenCurves, TweenPresets, UiTweenState
├── Core/               → UiFramework.Core       IUiElement, UiElement, UiState, UiConfig,
│                                                UiPopulationParams, UiElementReference
├── Runtime/            → UiFramework.Runtime    UiManager
└── Editor/             → UiFramework.Editor     UiSetupEditorWindow + tabs, code generators,
                                                 UiEditorConfig, UiStateDefinition, Addressables utility
```

Dependency direction (an arrow means "references"):

```
UiFramework.Editor ──► UiFramework.Runtime ──► UiFramework.Core ──► UiFramework.Tweening
        │                     │                       │
        └─────────────────────┴───────────────────────┴──► Unity.Addressables, Unity.ResourceManager
UiFramework.Editor ──► Unity.Addressables.Editor   (Editor platform only)
```

| Assembly | Namespaces | Notes |
| --- | --- | --- |
| `UiFramework.Tweening` | `UiFramework.Tweening` | No dependencies. Its folder sits inside `Runtime/`, but it is a separate assembly that Core depends on. |
| `UiFramework.Core` | `UiFramework.Core`, **and `UiFramework.Runtime` for `UiConfig`/`UiStateEntry`** | The namespace mismatch is historical. Moving `UiConfig` would change type identity for user assets. |
| `UiFramework.Runtime` | `UiFramework.Runtime.Manager` | Holds `UiManager` only. |
| `UiFramework.Editor` | `UiFramework.Editor.*` | Editor-only. `UiElementScriptBinder` is currently in the global namespace. |

## 4. Data model

| Asset | Type | Where it lives | Who writes it | Read at |
| --- | --- | --- | --- | --- |
| Editor config | `UiEditorConfig` (Editor SO) | `Assets/UiConfigs/UiEditorConfig.asset` | Developer, via the General tab | Edit time |
| Global canvas settings | `UiGlobalCanvasSettings` (Editor SO), referenced by `UiEditorConfig.GlobalCanvasSettings` | `Assets/UiConfigs/UiGlobalCanvasSettings.asset` (auto-created with defaults when missing) | Developer, via the General tab (Global Canvas Settings section) | Edit time only. Defaults for new element scenes (and a Canvas created by Fix Structure) and for an element's *Use Default Settings*. Editing them never changes existing scenes |
| State definition | `UiStateDefinition` (Editor SO), **one per state** | `UiEditorConfig.StateDefinitionsPath` (default `Assets/UiConfigs/UiStateDefinitions/`) | States tab | Edit time |
| State registry *(legacy)* | `UiStateRegistry` (Editor SO) | `UiEditorConfig.StateRegistryPath` | Older versions | Edit time, only if no definitions exist |
| **Runtime config** | `UiConfig` (runtime SO) | `UiEditorConfig.RuntimeConfigOutputPath` (default `Assets/UiConfigs/RuntimeUiConfig.asset`) | **Generated** by *Build Runtime UiConfig*. Do not hand-edit | Runtime, via Addressables |
| Element scenes | `.unity` | `UiEditorConfig.ElementsScenePath` (default `Assets/Scenes/UiElements/`) | Elements tab, then designers | Runtime, via Addressables |
| Generated scripts | `.cs` | `ElementsScriptPath`, `StatesPath` | Generators, then developers | Compile time |

`UiConfig` shape:

```
UiConfig
└── entries : List<UiStateEntry>
    └── UiStateEntry { stateKey : string, uiElementScenes : List<AssetReference> }
```

`UiStateDefinition` stores scene **names**. The build step resolves each name to a scene asset path and then to a GUID-based `AssetReference`. Element scene names must therefore be unique across the project.

## 5. Runtime

### 5.1 Initialization

```csharp
await UiManager.InitializeAsync();
```

1. If an instance already exists, it is returned as-is.
2. A `GameObject("UiManager")` is created and marked `DontDestroyOnLoad`, and it becomes the static instance. **`IsInitialized` is `true` from this point**, before the config has loaded.
3. `UiConfig` is loaded with `Addressables.LoadAssetAsync<UiConfig>("UiConfig")`, using the address set by the build step (§9). If it fails, an error is logged and the manager stays empty: every `ShowState*` then logs "not found".
4. For each entry, `stateKey → UiStateEntry` is cached and the `UiState` subclass is resolved (§5.3).

The `defaultState` parameter of `InitializeAsync` is currently unused. There is no synchronous initializer. `UiManager` is never placed in a scene.

The static instance is cleared at `RuntimeInitializeLoadType.SubsystemRegistration`, before any scene loads in each Play session. With *Enter Play Mode Options* and domain reload turned off, statics survive between sessions, so without this the second session would still hold the destroyed manager from the first. Every Play session starts uninitialized and creates its manager on the first `InitializeAsync`. The manager's other state lives on the instance, so it goes with it. No other runtime static holds state: `TweenCurves`, `TweenPresets`' key list and `UiTransition.Default` are read-only.

### 5.2 Showing a state

`ShowState<T>(context, additive, transition)` resolves `T` to its key and delegates to `ShowStateByKey(key, context, additive, transition)`, which calls `LoadAndPushStateAsync`:

```
ShowStateByKey(key, ctx, additive, transition)
│
├─ newState   = new UiState(key, entry.uiElementScenes)      ← always the base class
├─ loadingState = newState                                    ← GetCurrentState() returns it while loading
├─ visibleIds = GameObject IDs of every element in every state on the stack
│
├─ LoadStateScenesAsync(newState)            for each distinct scene GUID, in config order:
│     ├─ LoadSceneIfNeededAsync              reuse a loaded scene │ await an in-flight load │ Addressables.LoadSceneAsync(Additive)
│     ├─ refCount[guid]++
│     ├─ newState.RegisterLoadedScene        find every IUiElement in the scene (incl. inactive) → Populate(ctx)
│     └─ PrepareForShow()                    on new elements not in visibleIds (sets the "before" pose: no flash)
│
├─ if !additive && stack not empty:
│     ├─ sharedIds = newState elements ∩ visibleIds
│     ├─ await WhenAll( hide every stacked state except sharedIds,
│     │                 show newState except sharedIds )     ← overlapping transition
│     └─ finally: pop every old state → refCount-- per scene → unload at 0 → Dispose
│                 stack = [ newState ]; loadingState = null
│
└─ else (additive, or empty stack):
      ├─ finally: push newState; loadingState = null
      └─ show newState except sharedIds
```

Notes:
- **Non-additive** (default) replaces the whole stack. **Additive** pushes on top, for modals and overlays. The states below stay loaded and visible.
- The new state's scenes are ref-counted *before* old states are released, so shared scenes never unload and reload between states.
- Exceptions in loading or transitions are logged. Cleanup and the stack update always run (`finally`).
- Within a state, all element animations run in parallel. In a non-additive switch, the stacked states are hidden one after another, while the new state's show runs at the same time.

### 5.3 Type ↔ key resolution

At init, `GetTypeForKey(stateKey)` scans all loaded assemblies for a type assignable to `UiState` whose **simple class name equals the key**. If none is found it falls back to `typeof(UiState)`.

Consequences:
- `ShowState<MainMenuUiState>()` works only if `UiConfig` has `stateKey = "MainMenuUiState"`.
- **`UiState` subclasses are type tokens only.** The runtime always instantiates the base `UiState` with the scenes from `UiConfig`. Subclass constructors, fields and overrides never run, and the scene list in the generated constructor is ignored.
- Two subclasses with the same simple name in different namespaces collide, and the first one found wins.
- Keys without a subclass all map to `typeof(UiState)`, so `ShowState<UiState>()` is meaningless. Use `ShowStateByKey` for those.

### 5.4 Hiding

`HideUI(transition)` pops the top state, plays its hide animations (or `transition.HidePreset`), releases its scene references (unloading at zero) and disposes it. It does **not** replay show animations on the state underneath.

### 5.5 Queries

| API | Returns |
| --- | --- |
| `UiManager.IsInitialized` | `true` once `InitializeAsync` has created the instance (see §5.1) |
| `UiManager.GetCurrentState()` | The state being loaded, if any, otherwise the top of the stack, otherwise `null` |
| `UiManager.IsLastSceneActive<T>()` | Whether the top state's key is `T`'s key (`false` when the stack is empty) |
| `UiManager.ResetUIAsync()` | Hides and unloads every state (`ResetUI()` is obsolete) |
| `UiManager.TryGetUiStateKeyName<T>(out key)` | The key registered for `T` |

## 6. Shared scenes and shared elements

Scenes are identified by **AssetReference GUID** and reference-counted in `UiManager`:

- `loadedScenesByGuid`: the `SceneInstance` for every loaded UI scene.
- `sceneRefCountsByGuid`: how many states on the stack (plus the one loading) use that scene.
- `loadingHandlesByGuid`: in-flight loads, so concurrent requests for the same scene share one load.

A state counts a GUID at most once, even if `UiConfig` lists it twice. A scene unloads only when its count reaches zero.

When a new state uses an already-loaded scene, its elements are:
- **re-populated**: `Populate(newContext)` runs again on the same instance, so `Populate` must be idempotent;
- **not prepared, hidden or shown**: their GameObject IDs are in `sharedIds`, so they stay on screen without re-animating.

## 7. Animation & transitions

### 7.1 Animation target

`UiElement.Awake` picks the GameObject to animate:
1. The element's **first child**. If the element has no children, nothing animates.
2. If that child has a `Canvas`, the target is the first descendant without a `Canvas` (depth-first) instead. This animates the content and never the Canvas root, so the layout and the animation do not fight.

Only that one object animates, so its siblings stay still. Slides move it by the canvas size, and scale/pop presets scale around its pivot. That is why every element scene must follow this **element scene contract**:

```
<Name>Root   plain Transform (no RectTransform) with the UiElement subclass; the ONLY root GameObject in the scene
└─ Canvas    child index 0: root Canvas + CanvasScaler + GraphicRaycaster, values from UiGlobalCanvasSettings
   └─ Root   the ONLY direct child of Canvas, active, no Canvas of its own; RectTransform anchors (0,0)-(1,1),
      │      offsets 0, pivot (0.5,0.5), anchoredPosition 0, scale 1, rotation 0. This is the animation target
      └─ ... all screen content (nested canvases are fine here)
```

- The tools check **structure, not names**: `IngameRoot` or `ClubsUiParentGroup` are fine as the content root. Generated scenes use `Canvas` and `Root`.
- The content root may carry a Graphic (e.g. a full-screen background Image).
- Canvas values (render mode, pixel perfect, scaler mode, reference resolution, match mode/value, reference PPU) come from `UiGlobalCanvasSettings` and are written at **authoring time only**. Sorting order/layer, override sorting, world camera, plane distance and shader channels are never touched, because consumers use sorting order to stack popups.

Validation issue codes (`UiElementStructureIssueCode`):

| Code | Meaning | Auto-fix |
| --- | --- | --- |
| `NoUiElement` | No root GameObject has a `UiElement` | No |
| `ExtraRootObjects` | Other root objects besides the element root (one issue per object, with active state) | No. Never deleted automatically |
| `NoCanvas` | Child 0 of the element root is not a root Canvas | Yes, when the root has no children, only UI (RectTransform) children, or the Canvas is just not first |
| `CanvasMissingComponents` | No CanvasScaler / GraphicRaycaster | Yes, adds them |
| `NoContentRoot` | Canvas has 0 or 2+ children, or its only child is a prefab instance, has no RectTransform, or has its own Canvas | Yes, wraps the children in a new stretched `Root` (original order kept) |
| `ContentRootNotStretched` | The content root's anchors/offsets/pivot/position/scale/rotation differ from the contract | Yes, wraps it in a new full-screen `Root` (never resizes it) |
| `ContentRootInactive` | The content root is inactive | Warning only |
| `CanvasInsidePrefab` | The Canvas is part of a prefab instance, so its children can't be restructured in the scene | No. Fix it in the prefab |

`UiElementStructureFixer.FixStructure` only creates and reparents objects (Undo-recorded, never deletes), so serialized references survive. Reparenting keeps each child's local RectTransform values. The new `Root` is stretched with zero offsets, so the layout is unchanged. Content that doesn't fill the screen (`ContentRootNotStretched`) is always **wrapped** in a new full-screen `Root`, never resized: its own size and anchors still apply, the screen looks identical, and the whole screen (including anything added later) animates together. A box that was meant to be full screen also stays correct when wrapped; stretch it by hand if you want to drop the extra parent.

### 7.2 Presets

Presets are strings stored on each `UiElement` (`showAnimationPreset`, `hideAnimationPreset`) and chosen in the **Elements** tab, which shows a one-line description of each choice (`TweenPresets.Describe`). All presets run on unscaled time. **Every show and hide fades** (Fade and Scale over their whole duration, the others at their edges), so nothing pops in or out. **Entrances use ease-out** (they slow down into place); **exits are shorter and use ease-in** (they speed up as they leave), the usual pattern for UI that feels calm rather than jumpy.

| Key | Show | Hide |
| --- | --- | --- |
| `None` | appears instantly | disappears instantly |
| `Fade` | alpha 0 → 1, 0.22 s (adds a `CanvasGroup` if missing) | alpha → 0, 0.16 s |
| `Scale` | subtle zoom **with fade**: scale 0.94× → 1×, alpha 0 → 1, 0.26 s | scale → 0.96×, alpha → 0, 0.18 s |
| `SlideRight` / `SlideLeft` / `SlideUp` / `SlideDown` | **moves in the named direction**: enters from the opposite edge (SlideRight enters from the left), 0.32 s, fading in during the first 40% | **keeps moving in the named direction** and leaves past that edge (SlideRight leaves on the right), 0.24 s, fading out during the last 40% |
| `ClickPop`, `SettlePop`, `Shake` | pop 1.08× / pulse 1.04× / smooth fading side-to-side shake, fading in during the first 40% | the same motion, fading out during the last 40% |

- **Slides:** the name is always the direction of travel, for show and for hide. The distance is the canvas width or height (falling back to `Screen` size), so a screen enters and leaves fully off-screen.
- **Edge fades:** slides, pops and shake fade in during the first 40% of a show and out during the last 40% of a hide, so the element is fully visible while it moves but never appears or vanishes abruptly. This applies only when they run as a show / hide (`PlayAsync`); calling `TweenPresets.ClickPop` / `SettlePop` / `ShakeLocal` directly as button feedback never fades.
- **Base pose:** `UiTweenState` (added on demand) records the target's resting position, anchored position, scale and alpha the first time it animates. Every preset starts from and returns to that pose, so repeated, re-triggered or interrupted animations never drift. (Before 1.1.0, Scale used the current size as its base, so a shown screen ended at 0.9× and shrank further every cycle.)
- **No skipped frames:** `Tween.RunAsync` advances by at most `Tween.MaxFrameStep` (1/30 s) per frame. A long frame, like the one where a scene loads, slows the animation slightly instead of jumping past half of it.
- **One animation per channel:** a new animation stops older ones on the same object that drive the same thing (position, scale, alpha). A button pop doesn't stop a slide, but a new show stops a running hide.
- **No flash:** `PrepareForShow()` → `TweenPresets.PrepareShow` puts the element in its show start pose right after its scene loads, in the same continuation as `Populate`. It also undoes leftovers from an earlier hide (faded out, off-screen).
- Stacked states hide in parallel during a non-additive switch, while the new state shows.

### 7.3 Per-call animation override (`UiTransition`)

Each element's show / hide presets are set in the Elements tab. A call can replace them **for that call only**, without changing the configuration:

```csharp
// Show the Store with a zoom this time, whatever its elements are configured to do.
await UiManager.ShowState<StoreUiState>(context, transition: UiTransition.ShowWith(TweenPresets.ScaleKey));

// Close the top state by sliding it out to the right.
await UiManager.HideUI(UiTransition.HideWith(TweenPresets.SlideRightKey));

// Both sides in a non-additive switch: new screen slides in moving left, old screen leaves moving left.
await UiManager.ShowStateByKey("ProfileUiState", context, false, new UiTransition(TweenPresets.SlideLeftKey, TweenPresets.SlideLeftKey));
```

- `ShowPreset` replaces the show animation (and its start pose, via `UiElement.PrepareForShow(string)`) of every element that **appears** in the call. `HidePreset` replaces the hide animation of every element that **disappears** in it: the states leaving in a non-additive switch, or the popped state in `HideUI`.
- `null` (or `UiTransition.Default`) keeps each element's own animation. Shared elements that stay on screen are still not animated.
- Preset names are the `TweenPresets.*Key` constants and are matched ignoring case. An unknown name logs a `[UiTransition]` warning and falls back to the elements' own animations, so a typo never breaks navigation.
- Subclasses that override `PrepareForShow()` keep working: `UiManager` calls the parameterless overload when there is no override, and `PrepareForShow(string)` only when there is one.

### 7.4 Extending

Adding a preset means changing `TweenPresets` only: the key constant, the `presetKeys` list, the `PlayAsync` branch, its start pose in `PrepareShow`, and its text in `Describe`. Use `Tween.RunAsync` with a `UiTweenState` run so it can be interrupted cleanly. `ShowAsync`, `HideAsync` and `PrepareForShow` stay `virtual` for per-element custom animation.

## 8. Editor pipeline

Menu: **Window → UI Framework → UI Setup Manager** (`UiSetupEditorWindow`). The selected `UiEditorConfig` is remembered per machine in `EditorPrefs["UiFramework.Editor.ConfigAssetGUID"]`.

| Tab | What it does |
| --- | --- |
| **General** | Project configuration. Shows the configured paths. *Create Config* finds or creates `UiEditorConfig`. ***Build Runtime UiConfig*** generates the runtime config (below). **Global Canvas Settings** section: pick and edit the `UiGlobalCanvasSettings` asset inline. |
| **Elements** | Same layout as States. **Top bar**: search (part of the name, any case; names that start with the text first; Enter selects the first result) and *➕ New Element*. **Toolbar**: *Refresh*, *Validate All* (logs one line per scene with its issue codes), *Fix All Structure…* (confirms the planned changes, fixes every scene with fixable issues, logs what still needs manual work), and an *Only show elements with issues* filter. A **summary** shows OK / fixable / manual counts. **Left**: scrolling list of elements with a status icon (✓ / ⚠ / ✖), "(open)" when loaded; click to select, double-click to open the scene. **Right**, the selected element's settings: *Open Scene* / *Select Asset*; **Structure** (issue summaries, *Fix Structure…*, *What to fix by hand…*); **Animations** (show / hide presets, written straight into the scene's `UiElement`); **Canvas** (this scene's Render Mode, Sorting Order, UI Scale Mode, Reference Resolution, Screen Match Mode / Match, Reference PPU, edited in place via `UiElementCanvasUtility.EditCanvas`, plus *Use Default Settings…*); **Addressables** (toggle, group, labels). Each scene is opened once per refresh (`UiElementSceneAccess.InspectScene`); actions on one element re-read only that scene. **➕ New Element** shows the create form in the right panel: *Create* generates `<Name>.cs` from `UiElementTemplate.txt` (optionally `<Name>Params` / `<Name>Reference`). A script that already exists is kept and never overwritten; a warning says so. A **new** scene gets `<Name>Root → Canvas → Root` built per the contract. An **existing** scene has its roots reparented under `<Name>Root`, then the user is offered a structure fix. The script is attached after recompilation (`UiElementScriptBinder`, via `EditorPrefs` keys `UiElementAutoBind_*`): the binder waits one `EditorApplication.delayCall` after the reload, attaches and saves the scene, then re-reads that scene in every open Elements tab (`UiSetupEditorWindow.RefreshElementScene`). Scenes that can't be read during Play show as unavailable and are read again when Play ends (`ElementsTab.RefreshUnavailableScenes`). |
| **States** | Creates and edits `UiStateDefinition` assets (key + element scene names) and generates `<Key>.cs` from `UiStateTemplate.txt` into `StatesPath`. Search box in the top bar filters the state list (same matching as Elements, via `UiSearchFilter`). *Add element scene* is a search box above a scrolling list of every matching unassigned scene, with a match count (Enter adds the first). |

Confirmations (`UiElementChangeDialog`): every structure fix and *Use Default Settings* asks first in one popup with a one-line question and a short reason. It then shows **What will change** (short bullets, or a *Setting · Now → After* table for canvas values), a **Now / After** hierarchy preview for a single-scene structure fix (new objects highlighted; built by `UiElementFixPreview`, which simulates `FixStructure`), and a collapsed **Needs manual work** list. A footer line says whether the scene is saved or left open for undo. Bulk actions list one collapsible entry per scene. Each issue has a short `Summary` for this UI and tooltips. The full `Message` goes to the Console.

Scene editing rules (`UiElementSceneAccess.WithScene`): a scene **already open** in the editor is edited in place with Undo and left dirty. It is never saved or closed for you. A scene that is **not open** is opened additively, saved if changed, and closed again. Bulk actions call `SaveCurrentModifiedScenesIfUserWantsTo()` first. No scene is modified on import, refresh or window open.

**Build Runtime UiConfig**:
1. Load or create `UiConfig` at `RuntimeConfigOutputPath` and **clear all entries**.
2. For each `UiStateDefinition` (sorted by key, duplicates skipped with a warning): resolve each scene name to a path, make the scene Addressable (§9), and add an `AssetReference(guid)`.
3. If there are no definitions, fall back to the legacy `UiStateRegistry`.
4. Make the `UiConfig` asset Addressable (§9).

Run it after **any** change to states or element scenes, then rebuild Addressables content for player builds.

## 9. Addressables layout

| What | Group | Address | Labels |
| --- | --- | --- | --- |
| Runtime `UiConfig` | `GlobalConfigs` | `UiConfig` (**the runtime loads by this key**) | `UiConfig`, `RuntimeUiConfig` |
| Element scenes | `UiElements` | scene file name | `UiElements` |

A legacy group named `Global Configs` (with a space) is renamed to `GlobalConfigs` automatically on build. Groups and labels that do not exist yet are created.

## 10. Invariants

These hold today. A change that breaks one is a design change and needs this document updated.

1. Exactly one `UiManager` exists, created by `InitializeAsync` and kept alive with `DontDestroyOnLoad`. Each Play session starts without one, also with domain reload disabled (§5.1).
2. Only `UiManager` loads or unloads UI scenes. A UI scene is loaded ⇔ its ref count is > 0 or it is in flight.
3. Scenes are keyed by Addressables GUID, never by scene name.
4. After a non-additive show completes, the stack holds exactly one state.
5. Every element in a newly loaded scene gets `Populate(context)` before its first frame is shown.
6. Elements already visible when a state is shown are never prepared, hidden or shown by that transition.

## 11. Known limitations

| Area | Limitation |
| --- | --- |
| Concurrency | Overlapping `ShowState*` / `HideUI` calls are not serialized. Callers should await one before starting the next, as the sample's `ShowcaseNavigator` does by queueing every call (§14). |
| `HideUI` | Does not skip elements shared with the state underneath, so a shared element (e.g. NavBar) plays its hide animation even though its scene stays loaded. |
| Init | `IsInitialized` is `true` before the config finishes loading, and stays `true` if loading fails. |
| Type resolution | Reflection over all assemblies at init, matched by simple class name (§5.3). |
| Non-`UiElement` `IUiElement`s | Receive `Populate` but never animate. |
| Samples | The package ships no samples, on purpose. `InitializeAsync` always loads the Addressable `UiConfig`, so a project has exactly one config: a sample with its own would clash with the project's or silently become it, and Addressables entries don't travel with a `Samples~` import. The working sample is the showcase app in the host project under `Assets/` (§14), available by cloning the repository. |

## 12. Authoring a new screen (quick guide)

1. **Window → UI Framework → UI Setup Manager → General → Create Config** (first time only). Adjust the paths and namespaces on the `UiEditorConfig` asset. Set `ElementNamespace` / `StateNamespace` to your game's namespaces; the defaults (`UiFramework.Editor.*`) are placeholders.
2. **Elements tab** → name it (e.g. `ProfileUiElement`) → *Create*. The scene is generated as `ProfileUiElementRoot → Canvas → Root` with the global canvas settings. Put **all** content under `Root` (§7.1) and pick show/hide presets. The row's Structure column should stay ✓.
3. Override `Populate`, cast `context`, and keep it idempotent:
   ```csharp
   public override void Populate(object context = null)
   {
       base.Populate(context);

       ProfileUiElementParams data = context as ProfileUiElementParams;
       if (data == null)
       {
           return;
       }

       nameLabel.text = data.DisplayName;
   }
   ```
4. **States tab** → create a state key (e.g. `ProfileUiState`), add the element scenes it shows (shared ones like `NavBarUiElement` included), and generate the state class if you want `ShowState<T>()`.
5. **General → Build Runtime UiConfig**.
6. At runtime: `await UiManager.ShowState<ProfileUiState>(new ProfileUiElementParams { DisplayName = "Alex" });`

If nothing appears, check the following: the key matches the class name; *Build Runtime UiConfig* ran after the last change; the Addressables play mode script / content build is current; the console shows no `[UiManager]` errors.

## 13. Key files

| File | Responsibility |
| --- | --- |
| `Runtime/Manager/UiManager.cs` | Stack, scene loading and ref counting, transitions, public API |
| `Core/UiCompoenets/UiState.cs` | Per-state scene list, acquired GUIDs, element discovery and `Populate` |
| `Core/UiCompoenets/UiElement.cs` | Element base: animation target, show/hide/prepare (optionally with a per-call preset) |
| `Core/Transitions/UiTransition.cs` | Optional per-call animation override for `ShowState*` / `HideUI` |
| `Core/Config/UiConfig.cs` | Runtime state key → scenes map |
| `Runtime/Tweening/TweenPresets.cs` | Preset catalogue and implementations |
| `Editor/Window/UiSetupEditorWindow.cs` | Window host, *Build Runtime UiConfig*, config Addressables setup |
| `Editor/Window/Tabs/GeneralTab.cs`, `ElementsTab.cs`, `StatesTab.cs` | Tabs: configuration and default canvas settings; element list, toolbar and actions; states |
| `Editor/Window/UiSearchFilter.cs` | Name search used by the Elements and States lists |
| `Editor/Config/UiGlobalCanvasSettings.cs`, `UiEditorConfigUtility.cs` | Global canvas settings asset; find/create the active config and settings |
| `Editor/Scenes/UiElementSceneAccess.cs` | Safe scene access (open scenes in place, others opened and closed), single-open inspection, preset read/write |
| `Editor/Scenes/UiElementStructureValidator.cs` | Structure checks and issue codes |
| `Editor/Scenes/UiElementCanvasUtility.cs` | Canvas compare / apply / edit (only the default-settings fields) |
| `Editor/Scenes/UiElementSceneCreator.cs` | New element workflow: script + scene + contract hierarchy (asks for fixes through a callback) |
| `Editor/Scenes/UiElementStructureFixer.cs` | Structure fixes and contract hierarchy for new scenes |
| `Editor/Scenes/UiElementSceneBatchOperations.cs` | Validate All / Fix All Structure logic (no UI) |
| `Editor/Scenes/UiElementFixPreview.cs` | Now / After preview of a structure fix |
| `Editor/Window/Popups/UiElementChangeDialog.cs`, `UiElementChangeDialogs.cs` | The confirmation popup, and the content of each one (fix, fix all, use defaults, manual work) |
| `Editor/Window/Elements/UiElementDetailPanel.cs`, `UiElementCreateForm.cs` | Element settings panel and the New Element form (the Elements tab owns the actions via `IUiElementDetailActions`) |
| `Editor/Window/UiEditorStyles.cs` | Shared colours and section building blocks |
| `Tests/Editor/*` | EditMode tests: structure validation and fixes, fix preview, canvas settings, scene access, search |
| `Tests/Runtime/*` | PlayMode tests: animation presets frame by frame (slide directions, base pose, interruptions, curves) |
| `Editor/CodeGeneration/*` | Script generation and auto-binding |
| `Editor/Templates/*.txt` | Code templates (must follow [CODING_STANDARDS.md](CODING_STANDARDS.md)) |

## 14. Sample app (host project)

The host project under `Assets/` is a small mobile-game shell built only from framework features. It is the Play Mode check for runtime changes (CODING_STANDARDS §11) and a reference for app code on top of the framework. It is not part of the package.

**Run it:** play `Assets/Scenes/Bootstrapper.unity`. The element scenes below live in `ElementsScenePath` and follow the scene contract; their art (sprites, icons, `ShowcaseIconLibrary`) is in `Assets/Art/Showcase/`. Edit them like any other element scene, and run *Build Runtime UiConfig* after changing states or scenes. `BootStrapper` keeps one camera and one EventSystem under a `DontDestroyOnLoad` `PersistentRoot`; element scenes never carry their own. The element canvases are **Screen Space – Camera** (the sample's `UiGlobalCanvasSettings` defaults to it), saved without a camera because they can't reference one in another scene. `UiComponents/UiCanvasCamera` binds them: the bootstrap registers its camera, and every scene that loads afterwards has its camera-less Screen Space – Camera canvases assigned that camera on `SceneManager.sceneLoaded`, before its first frame. All canvases share the camera and the plane distance (100), so sorting order still stacks them (screens 0, TopBar / NavBar 10, popups 100+), and world positions match across canvases (the coin fly-in relies on this).

| State | Element scenes | How it is shown |
| --- | --- | --- |
| `IntroUiState` | `IntroUiElement` | First, by `BootStrapper`. Its fake loading ends by entering Home |
| `MainMenuUiState` (Home) / `StoreUiState` / `LeaderboardUiState` / `ProfileUiState` | screen + `TopBarUiElement` + `NavBarUiElement` | Non-additive. The TopBar and NavBar are shared: they stay loaded and never re-animate between tabs |
| `SettingsUiState` / `RewardUiState` / `ConfirmPurchaseUiState` | one popup scene each | Additive over the current tab, closed with `HideUI` |

Patterns it demonstrates (`Assets/Scripts/Ui/`):

- **Serialized navigation** (`App/ShowcaseNavigator`): the only caller of `ShowState*` / `HideUI`. Calls are queued so they never overlap (§11), tab requests coalesce (only the last tap wins), and one popup is open at a time.
- **Per-call transitions** (§7.3): tab switches pass `new UiTransition(SlideLeft, SlideLeft)` (or `SlideRight`) by tab order, whatever the screens' own presets are. Leaving the intro passes none, so every element plays its configured preset.
- **Context to every element** (§2): tab states get `TabUiParams` (the NavBar reads the tab from it); the Store also accepts `StoreUiParams`; popups take `RewardPopupUiParams` / `ConfirmPurchaseUiParams`.
- **Shared re-population** (§6): the TopBar wires itself up once and refreshes on every `Populate`. When the TopBar "+" re-shows the Store while it is on screen, the Store scene is shared and only re-populated, so `StoreUiElement` switches its page in place instead.
- **Custom element animation** (§7.4): `ShowcaseUiElement` overrides `PrepareForShow` (both overloads) and `ShowAsync` to run every *show effect* component in the scene next to the element's preset. Designers add them in the Inspector: `RevealOnShow` (one object), `StaggeredReveal` (children in turn), `FillBar`, `CountUpText`. Idle motion (`LoopMotion`), press feedback (`PressFeedback`, using `ClickPop`), and effects (`ShimmerSweep`, `ConfettiBurst`, `CoinFlyLayer`, `SegmentedControl`, `ToggleSwitch`) are separate components built on `Tween.RunAsync`. Reusable sub-widgets (cards, rows, the toast) are `*UiDisplay` components in `UiDisplays/`, spawned from inactive templates in the scene.
- **Popups** fade their root (the dimmed backdrop) with the Fade preset and move only the panel, with a `RevealOnShow` on it, so the backdrop never slides.
- **Data** lives in `App/ShowcaseSession` (balances, owned items, settings). Elements read it in `Populate` and listen to its events to update what is already visible. The framework itself does no data binding (§1).
