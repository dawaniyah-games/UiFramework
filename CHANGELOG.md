# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.41] - 2026-10-16

### Added
- **UI element scene contract** (`<Name>Root → Canvas → one stretched Root`) with validation and one-click fixes (see DESIGN.md §7.1)
  - `UiGlobalCanvasSettings` asset (`Assets/UiConfigs/UiGlobalCanvasSettings.asset`, auto-created with defaults: Screen Space Overlay, Scale With Screen Size 1080x1920, Expand, 100 PPU), referenced from `UiEditorConfig.GlobalCanvasSettings`
  - New elements are generated with the full contract hierarchy and the global canvas settings; creating from an existing scene offers a structure fix
  - All tools live in the UI Setup Manager window. No separate menu items are added
  - General tab: Global Canvas Settings section. These are the defaults every new element scene starts with (instead of Unity's defaults); editing them never changes existing scenes
  - Elements tab redesigned like the States tab: search in the top bar, a scrolling element list on the left (status icon, "(open)", double-click opens the scene), and the selected element's settings on the right
  - Element settings: Open Scene / Select Asset; Structure (issue summaries, Fix Structure…, What to fix by hand…); Animations (show / hide presets); Canvas (Render Mode, Sorting Order, UI Scale Mode, Reference Resolution, Screen Match Mode / Match, Reference PPU, Use Default Settings…); Addressables (toggle, group, labels)
  - Elements toolbar: Refresh, Validate All, Fix All Structure…, "Only show elements with issues", and a status summary; ➕ New Element opens the create form in the settings panel
  - Search by name or part of the name in the Elements and States lists (any case, several words, names that start with the text first, Enter selects the first result); the States tab's "Add element scene" is now a search box with suggestions
  - Clear confirmation popup for every fix and for Use Default Settings: short "What will change" bullets, a Now / After hierarchy preview (new objects highlighted), a Setting · Now → After table for canvas values, collapsed "Needs manual work", and a one-line save/undo note
  - Short plain-language issue summaries in the popup and tooltips (full details still in the Console)
  - Fixes only create and reparent objects (Undo-recorded, never delete). Content that doesn't fill the screen is always wrapped in a new full-screen Root and never resized, so the layout is unchanged
  - Default canvas values are written only when a scene or its Canvas is created, or by an element's Use Default Settings; sorting order/layer, override sorting, world camera, plane distance and shader channels are never touched
- EditMode tests for the contract under `Tests/Editor`
- `com.unity.ugui` package dependency

### Fixed
- Elements tab no longer closes scenes the user already had open when reading or writing animation presets. Open scenes are edited in place and left unsaved
- Creating a UI element now asks to save modified open scenes before replacing them
- `UiFramework.Tweening` and `UiFramework.Editor` now reference `UnityEngine.UI` explicitly (required on Unity 2021.3/2022.3)

### Documentation
- `CODING_STANDARDS.md`: a single source of truth for code style and project rules (formatting, naming, Unity null semantics, async, logging, assembly boundaries, versioning, templates), with a list of known deviations
- `CLAUDE.md` pointing AI assistants at the standards and design docs

### Changed
- Rewrote `DESIGN.md` to match the current code: assembly graph, data model, runtime lifecycle, GUID ref-counted shared scenes, animation targets/presets, editor build pipeline, Addressables layout, invariants and known limitations
- `README.md`: fixed the out-of-order structure and the outdated initialization API (`UiManager.InitializeAsync()` takes no config)
- `CONTRIBUTING.md`: added a PR checklist, Conventional Commit types and the full release steps
- `.github/copilot-instructions.md`: condensed to point at the authoritative docs (it previously listed the version as 1.0.31 and gave wrong paths)
- Code templates now emit `using` directives outside the namespace, as the standards require, and document that `UiState` subclasses are type markers only
- `.editorconfig`: added rules for JSON/asmdef/USS/UXML/YAML/Markdown and block-scoped namespaces; `.gitattributes` now normalizes line endings (`* text=auto`)

## [1.0.40] - 2026-03-09

### Fixed
- Made `LoadAndPushState` in UiManager resilient to exceptions by adding try/catch/finally blocks
- Ensured old state cleanup always executes even if transitions or scene loading fails
- Prevented scene leaks when exceptions occur during state transitions

## [1.0.35] - 2026-02-06

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.35] - 2026-02-04

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.35] - 2026-01-28

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.35] - 2026-01-28

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.35] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.34] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.33] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.32] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.31] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.30] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.29] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.28] - 2025-12-12

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.27] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.26] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.25] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.24] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.23] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.22] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.21] - 2025-12-08

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.20] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.20] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.19] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.18] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.17] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.17] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.17] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.17] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.16] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.16] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.15] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.15] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.14] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.14] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.13] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.13] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.13] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.13] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.11] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

## [1.0.10] - 2025-12-05

### Changed
- Merged dev branch to main
- See GitHub release notes for detailed changes

### Added

### Changed

### Fixed

### Removed

## [1.0.0] - 2025-12-05

### Added

- Initial release of UI Framework
- Core components: `UiManager`, `UiState`, `UiConfig`, `IUiElement`, `UiElement`
- Addressables-based scene composition with LIFO state stack
- Runtime API: `ShowState<T>()`, `ShowStateByKey()`, `HideUI()`, `GetCurrentState()`
- Context-based UI population via `IUiElement.Populate(object context)`
- Editor window: `UiSetupEditorWindow` for visual configuration
- Code generation: `UiElementGenerator` and `UiStateGenerator`
- Template system for generating UI elements, states, references, and params
- Assembly definitions for Runtime, Core, and Editor namespaces
- Comprehensive documentation and Copilot instructions
- `.editorconfig` with strict C# coding standards

### Features

- Type-to-key mapping using reflection for generic API
- Non-additive state loading with scene preservation
- Automatic IUiElement discovery in loaded scenes
- EditorConfig integration with UPM-compatible paths
- Support for companion classes: `UiElementReference` and `UiPopulationParams`

### Dependencies

- Unity 2021.3 or later
- Unity Addressables 1.21.0+

[1.0.0]: https://github.com/dawaniyah-games/UiFramework/releases/tag/v1.0.0
