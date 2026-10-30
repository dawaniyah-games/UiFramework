---
name: uiframework-ai-builder
description: Build Unity game UI with UI Framework (com.dawaniyahgames.uiframework) the way a professional team does - from a Figma, Photoshop or other design to optimized, responsive mobile screens, with element scenes, states, sprite atlases, inspector-first responsiveness and the project's code standards. Use whenever someone asks to build, import, rebuild, optimize or fix UI screens, popups or HUD in a Unity project, or to turn a design into Unity UI, even if they don't name UI Framework. Reads the project's UI_PROFILE.md so nobody has to repeat target devices, optimization and naming rules in every prompt.
---

# UI Framework AI builder

You are the UI engineer and technical artist on a Unity mobile game team. You build UI that a team can keep working on: every screen in its own scene, art packed and compressed for phones, layout that adapts in the Inspector, and code that looks like the rest of the project.

Raw AI UI goes wrong in the same ways every time: one giant Canvas, a 2,000-line script full of `SetActive`, hard-coded positions, a texture per icon, and a layout that breaks on the first tall phone or iPad. This skill exists to never do that.

## Step 0: load the project profile (every time)

1. Look for `UI_PROFILE.md` at the project root (next to `Assets/`). It holds the target devices, canvas settings, art pipeline, budgets and naming for this project.
2. If it's missing, create it from [references/project-profile.md](references/project-profile.md). Fill in what you can read from the project itself (`ProjectSettings/ProjectSettings.asset`, `UiGlobalCanvasSettings.asset`, `UiEditorConfig.asset`, `Packages/manifest.json`). Ask the user only for what you couldn't read, in one short message. Then commit it with the work.
3. Read the project's own rules too: `AGENTS.md`, `CLAUDE.md`, `.github/copilot-instructions.md`, `.editorconfig`, `CODING_STANDARDS.md`. For game code, the project's rules win over this skill. The UI Framework rules in [references/code-rules.md](references/code-rules.md) apply wherever the project is silent.

Never ask the user to repeat what the profile already says.

### Is UI Framework set up?

- Check `Packages/manifest.json` for `com.dawaniyahgames.uiframework`. If the project uses another UI system (UI Toolkit, NGUI, a custom screen manager) and the user didn't ask for UI Framework, ask before switching; otherwise follow the project's system and still apply steps 2, 4 and 6.
- Not installed and the user wants it: add `https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework` (pin `#vX.Y.Z`), then have the user open **Window → UI Framework → UI Setup Manager → General → Create Config**, set the paths and namespaces from the profile, and set **Global Canvas Settings** to the profile's canvas values before the first element is created.

### Audit the canvases (existing projects)

Canvas values are copied into each scene when it is created, so older scenes drift. Read the `CanvasScaler` of every scene in `ElementsScenePath` (UI scale mode, reference resolution, screen match mode, match value, render mode) and compare it with the profile. Report every scene that differs; don't fix them silently. The TA fixes each one with **Elements tab → select the element → Canvas → Use Default Settings…** and checks the screen on the profile devices afterwards, because its anchors may have been tuned around the old values.

## Step 1: plan the screens before touching anything

Turn the request or design into a table and show it to the user before building:

| Design frame / request | Element scene (`*UiElement`) | Shared? | State(s) (`*UiState`) | Additive? | Context (`<Element>Params`) |
| --- | --- | --- | --- | --- | --- |

Rules:
- One screen, bar or popup = one element scene. A bar used by several screens (TopBar, NavBar) is its own shared element, listed in every state that shows it, so it stays loaded and doesn't re-animate.
- Full screens are non-additive states. Popups, sheets and toasts are additive states closed with `HideUI()`.
- Repeated parts (cards, rows, list items, pills) are `*UiDisplay` components on templates inside the element scene, not separate scenes.
- Reuse existing elements and displays before adding new ones. Search `ElementsScenePath` and the `UiDisplays` folders first.
- List what the design doesn't cover (empty, loading, error, locked, long text, RTL) and say how you'll build each one.

## Step 2: bring the art in (design to atlas)

Follow [references/design-import.md](references/design-import.md): what to export and what to draw procedurally, export scale for the reference resolution, 9-slicing, trimming, naming and folders. Then [references/optimization.md](references/optimization.md) for import settings, one sprite atlas per feature, compression per platform and Addressables placement.

If a Figma connector is available, read the frames with it (design context, screenshots, asset export) instead of guessing from a picture.

## Step 3: build the element scenes

- Create elements and states only through **Window → UI Framework → UI Setup Manager** (Elements tab → Create; States tab → create the state, add its scenes, generate the class; General → **Build Runtime UiConfig**). Never hand-write scene, prefab or `.meta` YAML, and never edit the generated `UiConfig`.
- Every scene follows the contract: `<Name>Root → Canvas → Root → (all content)`. Nothing sits next to `Root` under the Canvas. Check the row stays ✓ in the Elements tab (Validate All).
- If you can drive the Unity Editor (an editor MCP or bridge), build the hierarchy through it. If you can't, write a **scene build sheet** per element for the technical artist: the hierarchy, each object's components, anchors/pivot/size, sprite, and which `[SerializeField]` it wires to. The TA builds it in minutes and nothing is guessed.
- In the Create form, tick **Include Parameters Class** when the element takes data: it generates `<Element>Params : UiPopulationParams` next to the script. New scenes are made Addressable in the `UiElements` group automatically.
- Render mode comes from the profile. Screen Space - Overlay needs nothing else. Screen Space - Camera scenes can't reference the bootstrap camera, so the project needs one binder that assigns it as scenes load (the UI Framework sample's `UiCanvasCamera` is the pattern); never put a camera in an element scene.
- Pick show/hide presets in the Elements tab (popups: the backdrop fades, the panel moves). No animation code for standard transitions. For choreography inside a screen (staggered cards, a bar filling, a count-up), use the project's Inspector components if it has them (the sample's `RevealOnShow`, `StaggeredReveal`, `FillBar`, `CountUpText` through an `IUiShowEffect` element); otherwise propose adding that pattern once instead of animating per screen in code.

## Step 4: make it responsive in the Inspector

Responsiveness is technical-art work in the Inspector, not code. Climb [references/responsiveness.md](references/responsiveness.md) one rung at a time and stop at the first one that works:

1. Canvas Scaler from the profile (never per-scene tweaks).
2. Anchors and pivots.
3. Layout groups, Layout Element, Content Size Fitter.
4. Aspect Ratio Fitter and 9-slicing.
5. Scene structure: split or regroup objects, a different container, a separate element.
6. Only then: one small, generic, reusable helper component in the game project (never in the framework), shown to the user first.

Never fix a layout by setting positions or sizes from gameplay code.

## Step 5: write the code

- Element script: override `Populate(object context)`, cast with `as`, return early on null, and make it **idempotent** (it runs again whenever a state reuses the scene). `[SerializeField] private` fields, wired in the scene.
- Context: the generated `<Element>Params` class (or the project's existing `*Params` / `*Context` type). No services, singletons or game managers inside elements unless the project's architecture already does it there.
- Navigation: `await UiManager.ShowState<StoreUiState>()`, `ShowState<ConfirmUiState>(context, additive: true)`, `await UiManager.HideUI()`. `UiManager` doesn't queue overlapping calls, so one place in the app changes UI state and runs requests one at a time. If the project has a navigator or task runner, go through it; if it has none, propose one modelled on the sample's `ShowcaseNavigator` (a queue, tab requests that coalesce, popups counted and closed with `HideUI`).
- Follow [references/code-rules.md](references/code-rules.md) and the project's own rules.

## Step 6: verify and hand off

Run the checks in [references/verification.md](references/verification.md): structure ✓, Build Runtime UiConfig, Device Simulator on every device in the profile, the performance budget (draw calls, atlases, overdraw), and Play Mode transitions. You usually can't run Unity yourself: say exactly which checks the user should run, and never claim a check you didn't do.

Finish with the hand-off report: what was built and why, files and scenes touched, art imported (atlas, sizes, formats), what was verified and what wasn't, open questions and the states the design didn't cover.

## Never

- One Canvas holding every screen, or showing screens with `SetActive` instead of states.
- Calling `Addressables.LoadSceneAsync`, `UnloadSceneAsync` or `SceneManager` on UI scenes. `UiManager` owns them.
- Text, solid fills or simple gradients exported as images.
- Sprites outside an atlas, mipmaps on UI sprites, Read/Write enabled, uncompressed textures on mobile.
- Positions, sizes, anchors or safe area set from gameplay code.
- Changing one scene's Canvas Scaler to make that screen fit. Canvas values come from the profile, the same for every scene.
- `var`, target-typed `new()`, `=>` members, `?.` / `??` on Unity objects, untagged logs (see code-rules).
- Inventing a new pattern when the project already has one.
