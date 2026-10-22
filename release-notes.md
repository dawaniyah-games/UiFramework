## UI Framework v1.3.0

A showcase app, a coding standards check, a docs site and editor fixes on top of v1.2.0.

### Highlights

**Showcase app**
- The sample project (`Assets/`) is now a complete mobile game UI built only with UI Framework: an intro, four tabs under a shared TopBar and NavBar, and Settings, Reward and Confirm-purchase popups.
- It demonstrates serialized navigation, per-call `UiTransition`s by tab order, context params, shared re-population and custom element animation (DESIGN.md §14). Clone the repository, open `Assets/Scenes/Bootstrapper.unity` and press Play.

**Docs site**
- `docs/index.html` (ready for GitHub Pages): overview, getting started, a ten-step UI Setup Manager tutorial, concepts, animated preset demos, the showcase, the API reference and an FAQ, with real Unity screenshots, interactive figures and a live 3D explainer of `ShowState` / `HideUI`.

**Editor fixes**
- Creating an element no longer overwrites an existing script with the same name: the generator keeps the file and logs a warning.
- The States tab's *Add element scene* list shows every matching unassigned scene in a scrolling list with a match count, instead of stopping at 8.
- The state generator no longer logs "Output directory does not exist" on every run.
- Editor log messages are tagged `[UiFramework]` and say what to do next.

**Coding standards check**
- `tools/check-standards.sh` checks the scriptable rules of CODING_STANDARDS.md and runs on every pull request. CODING_STANDARDS.md now includes the DenDon team rules.

### Upgrade notes
- No breaking changes. The public API and serialized fields are unchanged; the internal renames (PascalCase private constants, `Async` suffixes, `UiElementScriptBinder` moved to `UiFramework.Editor.CodeGeneration`) are not part of the public API.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.0/CHANGELOG.md)

### Installation

Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.0
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.0"
  }
}
```
