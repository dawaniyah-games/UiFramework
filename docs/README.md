# UI Framework docs site

`docs/index.html` is the public documentation and marketing page for the framework: one self-contained HTML file with tabs (Overview, Get started, Tutorial, Concepts, Animations, Showcase, API, FAQ). Every heading and tutorial step has an id, so any part can be linked, e.g. `index.html#t-state` opens the Tutorial at step 7.

## Preview

Open `docs/index.html` in a browser. Screenshots load from `docs/images/`; code highlighting and fonts load from the internet, and the page still reads fine offline.

## Publish with GitHub Pages

Repository **Settings → Pages → Build and deployment → Deploy from a branch**, branch `main`, folder `/docs`. The site is then served at `https://dawaniyah-games.github.io/UiFramework/`.

## Screenshots

**The full production plan is in [production/README.md](production/README.md)**: a tracker of every missing screenshot and video, the recording setup, and a script for every video (the tutorial builds one small game UI, Coin Shop, from an empty project). The tables below stay as a quick reference.

Every screenshot slot shows a labelled placeholder until its file exists. Two slots (the hero phone and the Elements tab) show an illustration instead. Add a PNG with the exact name below to `docs/images/` and it appears; no HTML changes needed.

Capture the editor at 100% UI scaling in the dark editor theme. Capture phone screens from the Game view at 1080 x 1920 portrait.

**Most important first:** `editor-overview.png` is the hero of the Overview tab and the image that shows at a glance that this is a Unity tool. Take it like this:

1. Open `Assets/Scenes/Bootstrapper.unity` and arrange the editor as Hierarchy (left), Game view (centre, set to 1080x1920 Portrait), and UI Setup Manager docked on the right with the Elements tab open.
2. Press Play and wait for the Home tab. In the Hierarchy, expand the loaded UI scenes (MainMenuUiElement, TopBarUiElement, NavBarUiElement) and DontDestroyOnLoad.
3. Capture the whole editor window (Windows: <kbd>Win</kbd>+<kbd>Shift</kbd>+<kbd>S</kbd>, window mode), about 16:9, at least 1600 px wide.

| File | Where it appears | What to capture |
| --- | --- | --- |
| `editor-overview.png` | Overview hero | The whole Unity editor in Play mode, as described above (done: from the 2026-10-21 recording) |
| `showcase-home.png` | Showcase gallery | Showcase Home tab |
| `showcase-intro.png` | Showcase gallery | Intro screen while the loading bar fills (done) |
| `showcase-store.png` | Showcase gallery | Store tab, Items page |
| `showcase-leaderboard.png` | Showcase gallery | Leaderboard after the podium has risen (done) |
| `showcase-profile.png` | Showcase gallery | Profile tab |
| `showcase-reward.png` | Showcase gallery | Reward popup with confetti |
| `showcase-purchase.png` | Showcase gallery | Confirm-purchase bottom sheet |
| `showcase-settings.png` | Showcase gallery | Settings popup |
| `setup-general.png` | Tutorial step 1 | UI Setup Manager, General tab with Global Canvas Settings |
| `setup-new-element.png` | Tutorial step 2 | Elements tab, New Element form |
| `scene-hierarchy.png` | Tutorial step 3 | An element scene: Hierarchy (Root → Canvas → Root → content) next to the Scene view |
| `setup-elements.png` | Tutorial step 4 | Elements tab with an element selected (Structure, Animations, Canvas, Addressables) |
| `setup-fix-dialog.png` | Tutorial step 5 | Fix Structure confirmation with the Now / After preview |
| `setup-states.png` | Tutorial step 7 | States tab with a state selected and its element scenes |

## Videos

Each tutorial step has a video slot, and the "Video series" list at the top of the Tutorial tab is built from them. To publish a video, upload it to YouTube and paste its id into the slot's `data-video-id`:

```html
<div class="video compact" data-video-id="dQw4w9WgXcQ" data-title="Step 2: Create your first element" data-length="2 min"></div>
```

The Showcase tab has one more slot (`data-title="The showcase app in the Unity editor"`) for the showcase video.

The slot turns into an embedded player (privacy-enhanced `youtube-nocookie.com`), and the step shows "Watch" in the list. The large slot at the top of the Tutorial tab is for a full walkthrough.

Planned episodes, in order (one per tutorial step):

1. Install and create the config
2. Create your first element
3. Build a screen under Root
4. Choose animations
5. Validate and fix scenes
6. Canvas and Addressables settings
7. States and shared elements
8. Build the runtime config
9. Show states from code
10. Popups and overlays

## Live 3D explainer

"Watch a screen come together" on the Overview (`#explainer`) is drawn by the page itself: the showcase screen in 3D, pulled apart into its element scenes. It is built to explain itself rather than just move:

- **Next** (or a step on the right) runs one real call: `ShowState` to MainMenu, to Store, a `RewardUiState` popup with `additive: true`, then `HideUI`. Nothing plays on its own.
- The strip above the figure says what that call did to every scene: *loaded and populated*, *kept, populated again* (shared scenes in a non-additive switch), *kept, untouched* (a push or a pop) or *unloaded*. The same word is on each layer's label and on the scene chips.
- Pointing at a layer (or tapping it) lifts it and turns the strip into a description of that scene.
- **Pulled apart / As the player sees it** opens or closes the stack.

Each element scene has its own colour from the showcase palette (MainMenu purple, Store cyan, TopBar gold, NavBar green, Reward popup pink). Screens and the popup move on a spring, and scenes pulse when they load or are populated again. It holds still under reduced motion and only draws while it is on screen.

The scenes, states and narration live in `buildExplainer` in `index.html` and follow the showcase app (DESIGN.md §14) and the shared-scene rules (DESIGN.md §6). If either changes, update the `steps` list there.

## Layer story (scroll)

"UI is only the view" (`#layers`) is told by scrolling, with no buttons. A 3D drawing stays pinned while six short paragraphs scroll past it. Like a product page that recolours one laptop in place, nothing appears from nowhere: the pieces of the tangle are the pieces of the final stack.

1. **One big layer**: Home, Store, Profile, Reward and six singletons (`GameManager`, `UIManager`, `PopupManager`, `EconomyManager`, `AudioManager`, `BackendClient`) on one red plate, tangled by 22 dependency lines.
2. **Lift the screens out**: the four screens rise onto the UI scenes plate.
3. **One door**: `UIManager` rises and becomes `UiManager`; `PopupManager` flies into it and merges.
4. **Your app**: `GameManager` rises and becomes the Navigator.
5. **Game systems**: `EconomyManager` and `AudioManager` become Economy and Audio.
6. **Backend**: what's left, `BackendClient`, becomes the Backend; then a dot follows a Buy tap down to the backend and back up through UiManager to the Reward popup.

Everything is tied to scroll position, including colour: a piece blends from tangle red to its layer colour as it rises, a red line fades as soon as either end leaves (the plate label counts the tangled links down), and solid wires join neighbouring layers once both ends are in place. The layer the current paragraph is about lifts a little and glows. Scrolling back reverses all of it.

To fill the screen, the section runs edge to edge (the drawing takes about 60% of the width at full viewport height) and a camera (the SVG `viewBox`) frames whatever is on stage: close on the tangle at the start, pulling back as layers stack. Motion that is not tied to scroll makes the states readable at rest: in the tangle the red calls race and the pieces jitter; pieces rise in an arc, plates drop in and every landing sends a ripple; calm calls flow along the clean wires; and the finished stack shows a labelled "Buy tap" going down and "Reward" coming back up. Under reduced motion all of that holds still. On phones the drawing pins to the top half and the text scrolls below it.

The pieces, layers, tangle and wires live in `buildLayerStory` in `index.html`; colours are the `--ls-*` tokens (light and dark). The layers are a recommended structure, not something the framework enforces: it provides the one entry point and the params.

## Made for teams (merge story)

"Made for teams" (`#teams`) draws two git graphs side by side, in `buildMergeStory` / `drawMergePanel`. In both, Lina (Store screen) and Omar (Reward popup) branch off `dev` at the same time. **Other tools**: both commit to a shared `ScreenRegistry`; Lina merges, Omar's merge ends in a pulsing conflict. **UI Framework**: each commits only their own `…UiElement` scene and `…UiState` asset; both merge, then the generated config is rebuilt. It plays when scrolled into view and loops; under reduced motion it shows the finished state.

## Interactive figures

The Overview uses [Hairline](https://hairline.lucasmarkes.com/figures) (MIT, by Lucas Marques): line drawings that answer the pointer and play on their own until it arrives. The page loads it from jsDelivr, pinned to `@lucasmarkes/hairline@0.5.1`. Offline, the figure boxes hide and the text around them still reads.

A figure is an element with `data-figure` set to the figure's function name, inside a `.fig` card; the `.fig-readout` shows the figure's live caption:

```html
<figure class="fig">
    <div class="fig-plate" data-figure="stack" aria-label="What the figure shows"></div>
    <div class="fig-bar"><span class="fig-live"></span><span class="fig-hint">Point up and down</span><output class="fig-readout">&ndash;</output></div>
    <figcaption class="fig-body">...</figcaption>
</figure>
```

| Figure | Where | What it stands for |
| --- | --- | --- |
| `exploded` | Hero | A screen built from layered element scenes |
| `settle` | The problem | A tangled UI settling into a state tree |
| `stack` | Four ideas | The state stack |
| `hub` | Four ideas | Shared, reference-counted scenes |
| `elevator` | Four ideas | Moving between states with `ShowState` |
| `format` | Four ideas | The scene contract and Fix Structure |

Figures sit on the dark stage (`--stage`) in both themes, like the showcase app. Each card has one neon accent, `--fig-c`, set per figure in the `.fig:has([data-figure="..."])` rules (purple, cyan, gold, green, pink, orange from the page palette); it colours the lit strokes, the live dot, the readout and the hover glow. Strokes are 1.9 px (2.1 in the hero). All 33 figures are listed in the [Hairline README](https://github.com/lucasmarkes/hairline).

## Keeping it accurate

The page describes the framework as documented in `DESIGN.md` and `README.md`. When a change updates those (new preset, new editor button, API change), update the matching tab here in the same PR. Code samples on the page follow `CODING_STANDARDS.md`.
