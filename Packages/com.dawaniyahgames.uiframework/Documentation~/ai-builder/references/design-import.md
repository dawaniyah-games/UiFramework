# From design to Unity art

Works for Figma, Photoshop, Illustrator, Adobe XD, Sketch, Penpot or a flat mockup. The goal is the fewest, smallest textures that reproduce the design at the reference resolution.

## 1. Map the design

- Read every frame and name it with its element scene: `Store` frame → `StoreUiElement`. Components and repeated groups → `*UiDisplay` templates.
- In Figma, use the connector when it's available: design context for structure and spacing, a screenshot per frame for checking, and asset export for the images. Write down the node id of each frame in the plan table so the next person finds it.
- Photoshop or other layered files: group layers by screen, then export (Export As / Generate Image Assets), or import a layered `.psb` with Unity's PSD Importer (`com.unity.2d.psdimporter`) when the layers already match the hierarchy.
- Only a flat image? Ask for the source file. If there isn't one, rebuild procedurally where you can and list which art must be cut by hand.

### What to ask the designer for

Send this list when the source arrives incomplete. It saves more time than any export trick:
- One frame per screen, popup and bar, at one width (the profile's frame width), named after the screen.
- Every state drawn or listed: normal, pressed, disabled, empty, loading, error, locked, long text.
- Components for repeated parts (cards, rows, buttons), so they become one `*UiDisplay`.
- Images that must be exported marked for export (Figma export settings / Photoshop layer names); everything else is assumed procedural.
- Real text in text layers, not outlined; the fonts named.
- Which parts are fixed and which grow (lists, long names), and what happens on a taller or wider screen.

## 2. Decide: export, or build in Unity?

| In the design | In Unity |
| --- | --- |
| Text, numbers, labels | TextMeshPro (or the profile's RTL text). Never an image, except the logo |
| Solid fills, simple linear gradients, rounded rectangles, outlines, pills | Procedural: Image with no sprite, a tiny 9-sliced rounded rect from the Common atlas, or the profile's procedural image component |
| Panels, buttons, frames with texture or ornaments | Sliced sprite: export the smallest piece that keeps the corners, set borders, Image Type = Sliced (Fill Center off for hollow frames) |
| Icons, illustrations, characters, badges | Sprite, trimmed, at the exact pixel size it shows on the 1080-wide canvas |
| Full-screen backgrounds | One sprite at the size it fills (edge to edge), its own feature atlas or ASTC 8x8 |
| Shadows and glows | Baked into the sprite only if they never change; otherwise a shared soft-glow sprite |
| Repeated identical icons (coins, gems) | One sprite in Common, reused |

Export only what Unity can't draw. Every exported image costs memory and atlas space.

## 3. Export rules

- **Scale:** export so 1 pixel in the file = 1 unit on the 1080-wide reference canvas. Scale = 1080 / frame width (360 → 3x, 390 → about 2.77x; round to the nearest export option and check the size in Unity).
- **Format:** PNG with transparency. JPG only for photos with no transparency.
- **Trim** transparent padding. Keep padding only where the sprite's pivot or a 9-slice needs it.
- **9-slice:** export the corner + 1-2 px of the middle (for example 96 x 96 for a panel with 40 px corners), never the full panel.
- **States:** one sprite per visual state that can't be a tint (Normal, Pressed, Disabled), named `_Pressed`, `_Disabled`. A colour change is a tint in the Button, not a new sprite.
- **Names and folders:** from the profile, for example `Assets/Art/UI/Store/Store_CoinPack_Small.png`. No spaces, no "copy", no "final2".
- **Figma export transparency:** check exported PNGs actually have alpha; frames with a fill export an opaque background.

## 4. Import

Apply the import settings in [optimization.md](optimization.md) right away (Texture Type Sprite, no mipmaps, no Read/Write, compression per platform), add the sprites to their feature atlas, and set 9-slice borders in the Sprite Editor.

Then build the scene from the frame: check it against the frame's screenshot at the reference size in the Game view, side by side.
