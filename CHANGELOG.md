# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
