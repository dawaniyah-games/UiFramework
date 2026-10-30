# Responsiveness: the technical-art ladder

Responsive UI is set up in the Inspector by a technical artist (or by you through the editor), so designers can see it, change it and keep it working. Code is the last rung, not the first.

Climb one rung at a time. Before moving up, write down in one line why the rung below can't do it.

## Rung 1: Canvas Scaler (project-wide)

The profile's `UiGlobalCanvasSettings` (for example Scale With Screen Size, 1080 x 1920, Expand) applies to every element scene at creation. Never change it per scene to fix one screen. With Expand the canvas is never smaller than the reference: tall phones get extra height, tablets get extra width. Design for 1080 x 1920 and decide where the extra space goes.

Mixed modes are a common hidden bug: with Match Width Or Height 0.5, a 20:9 phone gets a canvas about 970 wide, narrower than the 1080 design, and content overlaps. Every scene uses the same mode (see the canvas audit in SKILL.md).

## Rung 2: anchors and pivots

| Object | Anchors | Pivot |
| --- | --- | --- |
| Full-screen background | Stretch both (0,0)-(1,1), offsets 0 | Center |
| Top bar | Top stretch (0,1)-(1,1), fixed height | (0.5, 1) |
| Bottom bar / NavBar | Bottom stretch (0,0)-(1,0), fixed height | (0.5, 0) |
| Corner button | That corner | That corner |
| Popup panel | Center, fixed size (or a stretch with margins) | Center |
| Content area between bars | Stretch both, top/bottom offsets = bar heights | Center |

Use the anchor presets with Alt (sets position) and Shift (sets pivot). Never leave a full-screen object with center anchors and a 1080 x 1920 size: it won't fill a tall phone.

## Rung 3: layout components

- Horizontal / Vertical / Grid Layout Group for rows, columns and grids; child force expand off unless you mean it.
- Layout Element for min, preferred and flexible sizes of children.
- Content Size Fitter only on the object that should grow with its content (a pill around a label, a list content); never on a child of a layout group that also controls its size.
- Scroll Rect + RectMask2D when content can be longer than the screen.

## Rung 4: aspect and slicing

- Aspect Ratio Fitter: Envelope Parent for backgrounds that must cover, Fit In Parent for art that must stay whole, Width Controls Height for cards.
- 9-sliced sprites for anything that changes size (panels, buttons, pills) so it stretches without blurring.
- Preserve Aspect on icons.

## Rung 5: scene structure

Many "it needs code" problems are hierarchy problems:
- Wrap content in a container with the right anchors instead of fighting each child.
- Split a crowded screen so the variable part scrolls.
- Move a bar into its own shared element.
- For a really different layout (tablet vs phone), build it as a second element scene and a second state, and let the code that already picks the state choose by device. That is one line in the navigator, not layout code.

## Rung 6: a generic helper (last resort)

When rungs 1-5 truly can't do it (the safe area, a column that must scale down on very narrow screens, text that should grow but stop at a maximum), add **one small, generic component** to the game project, not to UI Framework:

- It solves the general problem, not one screen: `SafeAreaFitter`, `FitWidthScaler`, `MaxSizeLayoutElement`, never `StoreHeaderFixer`.
- Its settings are `[SerializeField]` so the TA tunes it in the Inspector, and it works in Edit Mode (`[ExecuteAlways]`) so the result is visible without Play.
- It reacts to changes (`OnRectTransformDimensionsChange`, `OnEnable`, `OnValidate`), not by polling in `Update`.
- It follows the code rules and has a two-line comment saying what it's for and how to set it up.
- It goes in the profile's "approved responsive helpers" list, so the next screen reuses it instead of writing another.

Show the user the rung-5 alternative you tried and the helper's API before adding it.

### Safe area

UGUI has no built-in safe-area component, and the profile may draw behind notches. Handle it once per project with an approved helper (for example a `SafeAreaFitter` on a `SafeArea` container under `Root` that sets its anchors from `Screen.safeArea`). Backgrounds stay outside it, edge to edge; bars, buttons and text go inside. UI Framework itself never adds runtime layout or safe-area scripts.

## Check

Device Simulator on every profile device, plus resizing the Game view freely: nothing clipped, nothing overlapping, touch targets at least about 100 px on the 1080 canvas, nothing important under a notch or the home indicator.
