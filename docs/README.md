# UI Framework docs site

`docs/index.html` is the public documentation and marketing page for the framework: one self-contained HTML file with tabs (Overview, Get started, Tutorial, Concepts, Animations, Showcase, API, FAQ). Every heading and tutorial step has an id, so any part can be linked, e.g. `index.html#t-state` opens the Tutorial at step 7.

## Preview

Open `docs/index.html` in a browser. Screenshots load from `docs/images/`; code highlighting and fonts load from the internet, and the page still reads fine offline.

## Publish with GitHub Pages

Repository **Settings → Pages → Build and deployment → Deploy from a branch**, branch `main`, folder `/docs`. The site is then served at `https://dawaniyah-games.github.io/UiFramework/`.

## Screenshots

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

## Keeping it accurate

The page describes the framework as documented in `DESIGN.md` and `README.md`. When a change updates those (new preset, new editor button, API change), update the matching tab here in the same PR. Code samples on the page follow `CODING_STANDARDS.md`.
