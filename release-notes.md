## UI Framework v1.3.7

Fixes from the Unity Asset Store review. A patch on top of v1.3.6.

### Highlights

**Unity 6.5 and newer**
- The package compiles again on Unity 6.5+ (tested by the Asset Store on 6000.6). `UiManager` no longer calls `GetInstanceID()`, which Unity 6.5 turned into a compile error. No behaviour change.

**Asset Store package**
- The package no longer ships its own `LICENSE.md` or a `license` entry in `package.json`. The Asset Store's End User License Agreement covers the Asset Store copy; this repository stays MIT licensed.
- The documentation link points straight at `https://www.dawaniyahgames.com/uiframework/`.

### Upgrade notes
- No API, serialized field, scene or asset change. Update the version and you're done.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.7/CHANGELOG.md)

### Installation

From the Unity Asset Store: add *UI Framework* to your assets, then install it from **Window → Package Manager → My Assets**.

Or Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.7
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.7"
  }
}
```
