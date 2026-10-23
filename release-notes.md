## UI Framework v1.3.1

Editor fixes for creating elements, and the tutorial videos in the docs. A patch on top of v1.3.0.

### Highlights

**Creating elements**
- Creating an element no longer brings up Unity's "Open scene(s) modified externally" dialog: the script is attached and the scene saved after the script reload.
- The Elements tab shows a new element as ready (✓) as soon as its script is attached, without pressing *Refresh*.
- The Elements tab reads its scenes again when Play Mode ends, instead of showing them as unavailable until *Refresh*.

**Docs**
- The tutorial step and showcase video slots now play the published Coin Shop tutorial videos.

### Upgrade notes
- No breaking changes. Editor-only fixes: no runtime or public API change.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.1/CHANGELOG.md)

### Installation

Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.1
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.1"
  }
}
```
