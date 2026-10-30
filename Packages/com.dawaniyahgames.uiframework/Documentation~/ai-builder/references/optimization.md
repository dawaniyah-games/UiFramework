# Mobile UI optimization

Defaults for Android and iOS UGUI. The profile's budgets decide when a screen is done.

## Sprite import settings

| Setting | Value |
| --- | --- |
| Texture Type | Sprite (2D and UI), Sprite Mode Single (Multiple only for real sheets) |
| Generate Mip Maps | Off |
| Read/Write | Off |
| sRGB | On; Alpha Is Transparency on for PNGs with alpha |
| Max Size | The smallest power of two that holds the sprite at its on-canvas size. Not 2048 for a 120 px icon |
| Filter | Bilinear |
| Android / iOS override | On. ASTC 6x6 default; 4x4 for small detailed icons and thin outlines; 8x8 for large soft backgrounds |
| Compression in atlases | Set it on the atlas: the atlas's settings decide what ships |

Use a Preset (Assets/UI/Presets) for these settings and a Preset Manager rule on the UI art folder, so every new sprite gets them without anyone remembering.

ASTC covers current Android and iOS devices. If the profile supports old Android GPUs without ASTC, Unity decompresses on load there; check Build Settings → Android → Texture Compression and the profile's minimum device before choosing ETC2 instead.

## Sprite atlases

- Turn packing on: **Project Settings → Editor → Sprite Packer Mode = Sprite Atlas V2 - Enabled**. With packing disabled, every sprite is its own texture and every icon breaks the batch.
- One atlas per feature or element scene (`Store`, `Profile`, `Clubs`), one `Common` atlas for what several screens share (buttons, frames, coin and gem icons, the rounded-rect slice). A screen should touch its feature atlas, Common and the font, and nothing else.
- Atlas settings: max 2048 x 2048, padding 4, Allow Rotation off and Tight Packing off (both break UI and 9-slices), mipmaps off, the platform compression from the table above.
- Big full-screen backgrounds don't go in a feature atlas with small icons; give them their own atlas or keep them single.
- Never put one sprite in two atlases.

## Addressables

UI scenes are Addressable, so their sprites travel in bundles:

- UI Framework puts every element scene in the `UiElements` group. If that group packs scenes separately, a sprite atlas that isn't Addressable is copied into every scene bundle that uses it. Make the atlases Addressable in their own group (for example `UiAtlases`, packed separately so each atlas is one bundle), so each one ships and loads once.
- Run **Addressables → Analyze → Check Duplicate Bundle Dependencies** after adding art. A texture listed twice is shipped twice.
- Don't load sprites by name from `Resources` for UI. Reference them in the scene or through Addressables.

## Canvas and batching

- One root Canvas per element scene (the contract). Add a nested Canvas only for a part that changes every frame or very often (timers, counters, progress bars, scrolling lists) so it rebuilds alone.
- Changing any Graphic marks its Canvas dirty. Don't animate colour or size on big static canvases every frame; animate the `Root` or a nested Canvas.
- Order siblings so the same atlas and the same material are next to each other; text and images interleaved break batches.
- Masks: RectMask2D for rectangular clipping (scroll views); Mask (stencil) only for non-rectangular shapes.
- Raycast Target off on every Graphic that isn't touched (backgrounds, icons, labels). Only buttons, toggles, inputs and scroll areas keep it.
- Hide a heavy element by disabling its Canvas component or through states, not by deleting and recreating objects.

## Overdraw and fill rate

- At most two full-screen transparent layers on top of each other (background + popup backdrop).
- No invisible Images (alpha 0) left enabled to catch clicks; use an empty Graphic-free raycast target or the backdrop that is already there.
- Hollow frames: Sliced with Fill Center off.

## Layout cost

- Layout groups and Content Size Fitters recalculate when children change. Fine for static screens and lists that change on populate; avoid nested layout groups on content that updates every frame.
- Long lists (more than about 30 rows) reuse row objects instead of instantiating all of them.

## Text

- TextMeshPro everywhere. Shared font assets; one material per style (outline, shadow) used across screens.
- Fixed text (titles, buttons) can use a static font atlas with just the characters it needs; player names and chat need a dynamic atlas or a fallback.
- Avoid Auto Size on text that changes often; set a size and use Overflow Ellipsis or the profile's max-size helper.

## Measuring

Open the Stats overlay (Game view) or the Frame Debugger, and the Profiler on the low-end device:
- Batches for the screen against the budget
- Which atlases and textures the screen draws (Frame Debugger)
- Canvas.SendWillRenderCanvases and Canvas rebuilds when the screen opens
