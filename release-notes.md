## UI Framework v1.1.0

A consistent layout for every UI element scene with one-click validation and fixes, a redesigned Elements tab with search, and much smoother animations.

### Highlights

**UI element scene contract**
- Every element scene follows `<Name>Root → Canvas → one full-screen Root`, so show/hide animations move the whole screen.
- New elements are created with this layout and the **Global Canvas Settings** (General tab) instead of Unity's canvas defaults.
- Validation shows each element's status; **Fix Structure** only creates and reparents objects (never deletes or resizes), with Undo.
- Every fix asks first in a clear popup: what will change, a **Now / After** hierarchy preview, and what still needs manual work.

**Elements and States tabs**
- Elements tab has the same layout as States: search, a list with status icons, and a settings panel per element (Open Scene, Structure, Animations, Canvas, Addressables).
- Search by name or part of the name in both tabs; "Add element scene" in States suggests matching scenes as you type.

**Smoother animations**
- Fixed: `Scale` ended at 0.9× and shrank further every show/hide cycle; `ClickPop` grew buttons when tapped repeatedly.
- Entrances ease out, exits are shorter and ease in; no more jump on the first frame after a scene loads; overlapping animations no longer fight.
- `Scale` is now a subtle zoom with a fade; `Shake` is a smooth wiggle instead of random jitter.
- Every show and hide fades: slides, pops and shake fade in at the start of a show and out at the end of a hide, so nothing pops in or out.
- Slides move in their named direction on show **and** hide (Slide Right enters from the left and leaves on the right).

**Per-call animation override**
- Play a different animation for one call without changing the configured presets:

```csharp
await UiManager.ShowState<StoreUiState>(context, transition: UiTransition.ShowWith(TweenPresets.ScaleKey));
await UiManager.HideUI(UiTransition.HideWith(TweenPresets.SlideRightKey));
```

### Upgrade notes
- No breaking API changes.
- **Deprecated:** `UiManager.ResetUI()`. Use `await UiManager.ResetUIAsync()` (`ResetUI()` no longer blocks the main thread).
- `TweenCurves.EaseIn` / `EaseOut` / `EaseInOut` now have the shapes their names describe, and preset timings changed slightly.
- `UiGlobalCanvasSettings` is created automatically at `Assets/UiConfigs/UiGlobalCanvasSettings.asset`. Existing scenes are only changed when you run Fix Structure or Use Default Settings.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.1.0/CHANGELOG.md)

### Installation

Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.1.0
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.1.0"
  }
}
```
