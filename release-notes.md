## UI Framework v1.3.6

Fixes for the Unity Asset Store Validator. A patch on top of v1.3.5.

### Highlights

**UI Setup Manager menu**
- The window moved from the top-level **UiFramework** menu to **Window → UI Framework → UI Setup Manager**, because Asset Store packages may not add a top-level menu.
- The published tutorial videos still show the old menu. The docs' Tutorial tab says so.

**Unity version**
- The package now requires Unity 2022.3 or newer (was 2021.3), the oldest version the Asset Store accepts.

**Play Mode without domain reload**
- `UiManager` clears its static instance when a Play session starts, so with *Enter Play Mode Options* and domain reload turned off every session starts clean. Behaviour was already right; the reset is now explicit.

### Upgrade notes
- No breaking API change. Only the menu location changed: open the window from **Window → UI Framework → UI Setup Manager**. No runtime behaviour or public API change, and no scene, prefab or asset migration.
- Projects on Unity 2021.3 must upgrade to 2022.3 or newer before taking this version.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.6/CHANGELOG.md)

### Installation

From the Unity Asset Store: add *UI Framework* to your assets, then install it from **Window → Package Manager → My Assets**.

Or Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.6
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.6"
  }
}
```
