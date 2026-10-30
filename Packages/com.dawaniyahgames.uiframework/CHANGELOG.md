# Changelog

All notable changes to UI Framework are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.3.14] - 2026-10-30

First release on the Unity Asset Store.

### Requirements
- Unity 6 (6000.0 LTS) or newer, with Addressables 2.2.2 or newer and uGUI 2.0. Tested on Unity 6.0 LTS and 6.6.
- Works with the Built-in Render Pipeline, URP and HDRP, because the framework only uses uGUI canvases.

### Features
- UI built from Addressable element scenes, loaded additively and managed on a LIFO state stack: `ShowState<T>()`, additive popups and overlays, `HideUI()`.
- Shared elements stay loaded between consecutive states and don't re-animate.
- One data contract: every element receives the context object in `Populate(object context)`.
- Show and hide presets per element (Fade, Scale, Slide, ClickPop, SettlePop, Shake) with overlapping transitions and per-call overrides.
- The UI Setup Manager (**Window → UI Framework → UI Setup Manager**) generates element scripts and scenes, state classes and the runtime config, and validates and fixes element scene structure.
- Works with *Enter Play Mode Options* and domain reload disabled.
- A full offline manual in `README.md`, and an AI build guide in `Documentation~/ai-builder/`.

### Fixed (compared with earlier versions published on GitHub)
- *Build Runtime UiConfig* creates the project's Addressables settings when there are none, and creates the `GlobalConfigs` and `UiElements` groups with the build settings of the project's default group, so the config and the element scenes are included in player builds. Groups created by earlier versions without those settings are repaired the next time you build the runtime UiConfig.
- *Build Runtime UiConfig* reports success only when the config was built and made Addressable; otherwise it says to check the Console.
- `UiManager.InitializeAsync()` no longer fails when another plugin in the project has an assembly with a missing dependency.
- `UiManager` no longer logs a message every time a state is unloaded.
