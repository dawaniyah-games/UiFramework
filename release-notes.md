## UI Framework v1.2.0

Two animation improvements on top of v1.1.0.

### Highlights

**Per-call animation override (`UiTransition`)**
- Play a different show / hide animation for one call, without changing the elements' configured presets:

```csharp
// Show the Store with a zoom this time.
await UiManager.ShowState<StoreUiState>(context, transition: UiTransition.ShowWith(TweenPresets.ScaleKey));

// Close the top state by sliding it out to the right.
await UiManager.HideUI(UiTransition.HideWith(TweenPresets.SlideRightKey));

// Both sides of a non-additive switch.
await UiManager.ShowStateByKey("ProfileUiState", context, false, new UiTransition(TweenPresets.SlideLeftKey, TweenPresets.SlideLeftKey));
```

- `null` keeps each element's own animation. An unknown preset name logs a warning and falls back to the configured animation, so a typo never breaks navigation.

**Every show and hide fades**
- Slides, pops and shake now fade in during the first 40% of a show and out during the last 40% of a hide (Fade and Scale already fade throughout). Elements are fully visible while they move, but never pop in or out, which makes transitions easier on the eyes.
- Pops and shake used as a show / hide now actually show and hide the element. Calling `ClickPop` / `SettlePop` / `ShakeLocal` directly as button feedback is unchanged (no fade).

### Upgrade notes
- No breaking changes. The new `transition` parameter is optional and comes last, so existing `ShowState` / `ShowStateByKey` / `HideUI` calls compile unchanged.
- Subclasses that override `UiElement.PrepareForShow()` keep working. Override the new `PrepareForShow(string presetKey)` only if you need custom preparation for per-call overrides.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.2.0/CHANGELOG.md)

### Installation

Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.2.0
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.2.0"
  }
}
```
