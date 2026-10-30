# UI_PROFILE.md template

Copy this to `UI_PROFILE.md` at the project root and fill it in once. Every AI tool and every teammate reads it instead of asking again. The defaults suit a portrait mobile game, so a new mobile project can usually keep them.

Read the real values from the project where you can:

| Value | Where it lives |
| --- | --- |
| Orientation | `ProjectSettings/ProjectSettings.asset`: `defaultScreenOrientation` (0 = portrait, 3 = landscape left), `allowedAutorotateTo*` |
| Android min SDK | `AndroidMinSdkVersion` |
| Draw outside the safe area | `androidRenderOutsideSafeArea` |
| Default texture compression | `m_BuildTargetDefaultTextureCompressionFormat` (`03000000` = ASTC) |
| Canvas settings | `Assets/UiConfigs/UiGlobalCanvasSettings.asset` (`renderMode` 0 = Overlay, 1 = Camera; `uiScaleMode` 1 = Scale With Screen Size; `screenMatchMode` 0 = Match, 1 = Expand, 2 = Shrink) |
| Script and scene paths, namespaces | `UiEditorConfig.asset` |
| Sprite packer | `ProjectSettings/EditorSettings.asset`: `m_SpritePackerMode` (0 = disabled; set Sprite Atlas V2 - Enabled) |
| Unity version | `ProjectSettings/ProjectVersion.txt` |

---

```markdown
# UI profile: <Game name>

Read by AI assistants and the team before any UI work. Change it here, not in prompts.

## Targets
- Platforms: Android (min SDK 25) and iOS
- Orientation: portrait only
- Unity: 6000.3 (UI Framework needs Unity 6, 6000.0+)
- Test devices (Device Simulator): a tall 20:9 Android phone, a 19.5:9 iPhone with a notch or Dynamic Island,
  a small 16:9 phone (iPhone SE class), a 4:3 tablet (iPad class), and one low-end Android (2-3 GB RAM)
- Low-end budget is the bar: if it runs there, it ships

## Canvas (UiGlobalCanvasSettings)
- Render mode: Screen Space - Overlay
- Canvas Scaler: Scale With Screen Size, reference 1080 x 1920, Screen Match Mode Expand, reference PPU 100
- Pixel Perfect: off
- Sorting order: screens 0, shared bars 10, popups 100+, toasts 200
- Safe area: the app draws behind notches (androidRenderOutsideSafeArea = 1). Backgrounds go edge to edge;
  bars and buttons sit inside the project's safe-area container (see Responsiveness)

## Design source
- Tool: Figma (file link or key here). Frame width: 360 / 375 / 390 / 1080 (pick one)
- Export scale: reference width / frame width (1080 / 360 = 3x)
- Text language and direction: e.g. Arabic RTL with RTLTextMeshPro (Farsi off), numbers in plain TextMeshProUGUI
- Fonts: <font assets>; never bake text into images except the logo

## Art pipeline
- Folders: Assets/Art/UI/<Feature>/ for sprites, Assets/Art/UI/Atlases/<Feature>.spriteatlasv2
- Sprite names: <Feature>_<Thing>[_<State>] in PascalCase, e.g. Store_CoinPack_Small, Common_Button_Green_Pressed
- One atlas per feature or element scene, plus one Common atlas for shared buttons, frames and icons
- Atlas max size 2048, padding 4, tight packing off for 9-sliced sprites, rotation off
- Compression: ASTC 6x6 by default; 4x4 for small detailed icons; 8x8 for large soft backgrounds
- Procedural shapes: <none | FlexibleImage | rounded-rect 9-slice> for panels, pills, gradients and outlines
- Shared popup look: <reference scene, backdrop colour, panel style>

## Budgets (per screen, on the low-end device)
Starting points. Measure your heaviest existing screen on the low-end device and set the real numbers here.
- Draw calls (Batches in Stats) for one full screen: <= 40; popup on top: <= 20 more
- Atlases touched by one screen: <= 3 (feature + Common + font)
- Full-screen transparent layers stacked: <= 2
- No frame over 16 ms when opening or closing a screen

## Code
- Project rules: AGENTS.md, .github/copilot-instructions.md, .editorconfig (these win for game code)
- UI scripts: Assets/Scripts/Ui/UiElements, UiStates, UiDisplays, Components (helpers)
- Namespaces: <Game>.Ui.Elements, <Game>.Ui.States
- How the game shows UI: e.g. TaskRunner.Start(UiManager.ShowState<ShopUiState>()), or a navigator class
- Approved responsive helpers (Components/): e.g. SafeAreaFitter, FitWidthScaler, MaxSizeLayoutElement

## Not designed yet, build to match
- Empty, loading, error and locked states follow <reference scene>
```
