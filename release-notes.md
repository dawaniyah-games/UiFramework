## UI Framework v1.3.14

The version submitted to the Unity Asset Store, with fixes from a pre-submission review. A patch on top of v1.3.13.

### Highlights

**Player builds load the UI in new projects**
- The UI Setup Manager created the `GlobalConfigs` and `UiElements` Addressables groups without build settings, so the content build left them out and a player couldn't load the runtime UiConfig. Play Mode with *Use Asset Database* hid it. New groups now copy the project's default group settings, and existing groups are repaired the next time you build the runtime UiConfig or make an element Addressable.

**Honest status in the UI Setup Manager**
- *Build Runtime UiConfig* says "successfully" only when it worked; otherwise it points to the Console.

**More robust start-up**
- `UiManager.InitializeAsync()` no longer fails when another plugin has an assembly with a missing dependency.
- No more "Unloading UI state" log on every screen change.

**Asset Store package**
- The package ships its own changelog for Asset Store users, and its README and AI build guide no longer refer to git installs, the repository's license or internal projects.
- *Create* menu entries for the editor assets moved to *UI Framework*; the runtime `UiConfig` has none (the UI Setup Manager builds it).

### Upgrade notes
- No runtime API or serialized field change. `UiSetupEditorWindow.BuildRuntimeUiConfig()` now returns `bool`.
- If you built your UI with an earlier version, click *Build Runtime UiConfig* once so the Addressables groups get their build settings, then rebuild Addressables content.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.14/CHANGELOG.md)

### Installation

Coming soon to the Unity Asset Store. Until the listing is live, use Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.14
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.14"
  }
}
```
