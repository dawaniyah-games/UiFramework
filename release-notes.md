## UI Framework v1.3.12

Fixes from the Unity Asset Store review (v1.3.10), an AI build guide (v1.3.11) and an offline manual (v1.3.12). A patch on top of v1.3.9.

### Highlights

**Unity 6 and newer**
- The package now requires Unity 6 (6000.0 LTS) or newer, with Addressables 2.2.2+ and uGUI 2.0. Projects on Unity 2022.3 or 2023 stay on v1.3.11.

**Offline manual**
- The package README is now a full manual: concepts, quick start, every UI Setup Manager tab, the element scene contract, the runtime API, animations, Addressables, limitations and troubleshooting.
- The misspelled folder `Core/UiCompoenets` is now `Core/UiComponents` (GUIDs unchanged).

**Unity 6.5 and newer**
- The package compiles again on Unity 6.5+ (tested by the Asset Store on 6000.6). `UiManager` no longer calls `GetInstanceID()`, which Unity 6.5 turned into a compile error. No behaviour change.

**Asset Store package**
- The package no longer ships its own `LICENSE.md` or a `license` entry in `package.json`. The Asset Store's End User License Agreement covers the Asset Store copy; this repository stays MIT licensed.
- The documentation link points straight at `https://www.dawaniyahgames.com/uiframework/`.

**Building UI with an AI assistant**
- The package ships an AI build guide in `Documentation~/ai-builder/` for Claude, ChatGPT / Codex and Copilot: screens as element scenes and states, art from Figma or Photoshop as compressed sprite atlases, Inspector-first responsiveness, and the coding standards. Copy its `UI_PROFILE.md` template to your project once. See "Building UI with an AI assistant" in the README.
- The README quick start now says to build under `Canvas → Root` (it said `Content`).

### Upgrade notes
- No API, serialized field, scene or asset change. Update the version and you're done.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.12/CHANGELOG.md)

### Installation

Coming soon to the Unity Asset Store. Until the listing is live, use Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.12
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.12"
  }
}
```
