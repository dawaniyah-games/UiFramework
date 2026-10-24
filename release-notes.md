## UI Framework v1.3.5

The package is ready for the Unity Asset Store. A patch on top of v1.3.4.

### Highlights

**Asset Store package**
- The package ships its own `README.md`, `LICENSE.md` and `CHANGELOG.md`, and `package.json` links to the docs site, the changelog and the license, as the Asset Store requires.
- The broken `samples` entry is removed from `package.json`. It pointed at `Samples~/BasicSetup`, which never existed.
- The package ships no samples, on purpose. `UiManager.InitializeAsync()` always loads the Addressable `UiConfig`, so a project has exactly one config, and a sample with its own would clash with yours or silently replace it. Build your config with the UI Setup Manager. The showcase app stays in this repository: clone it and play `Assets/Scenes/Bootstrapper.unity`.

**Repository**
- `tools/check-standards.sh` fails when the package's `LICENSE.md` or `CHANGELOG.md` differs from the root file. The root files stay the source of truth.

### Upgrade notes
- No breaking changes. Packaging and docs only: no runtime, editor or public API change.

Full details: [CHANGELOG.md](https://github.com/dawaniyah-games/UiFramework/blob/v1.3.5/CHANGELOG.md)

### Installation

From the Unity Asset Store: add *UI Framework* to your assets, then install it from **Window → Package Manager → My Assets**.

Or Package Manager → **+** → *Add package from git URL…*

```
https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.5
```

Or in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.3.5"
  }
}
```
