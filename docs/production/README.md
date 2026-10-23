# Docs content production plan

Everything the docs site (`docs/index.html`) still needs from Unity: real screenshots, the tutorial videos and their voiceover. Work through the phases in order and tick the boxes; every asset has one row in the tracker, so nothing is missed.

The tutorial videos build **one small game UI from an empty project**, called **Coin Shop**, one step per episode:

| | Coin Shop, by the end of the series |
| --- | --- |
| Screens | Main Menu (title, Play, Store), Store (three item cards) |
| Shared bars | TopBar (coin count), NavBar (Home, Store) |
| Popup | Confirm Purchase (dimmed backdrop, panel, Buy, Cancel) |
| States | `MainMenuUiState`, `StoreUiState`, `ConfirmPurchaseUiState` |

Each episode starts exactly where the previous one ended, so a viewer can build along and a single episode still makes sense on its own.

**Help from Claude in the desktop app:** [claude-desktop-prompts.md](claude-desktop-prompts.md) has paste-ready prompts that let Claude (with Computer use) capture all twelve missing screenshots in Unity, plus a briefing that lets it prepare, check and rehearse each episode with you, and a YouTube description for every video.

## Tracker

Status: `todo` → `recorded` → `voiced` → `edited` → `live` (screenshots: `todo` → `done`).

### Screenshots (`docs/images/`)

| File | Where it appears | Captured in | Status |
| --- | --- | --- | --- |
| `editor-overview.png` | Overview hero | done (2026-10-21) | done |
| `showcase-intro.png` | Showcase gallery | done | done |
| `showcase-leaderboard.png` | Showcase gallery | done | done |
| `showcase-home.png` | Showcase gallery | Phase 1 | done (2026-10-22) |
| `showcase-store.png` | Showcase gallery | Phase 1 | done (2026-10-22) |
| `showcase-profile.png` | Showcase gallery | Phase 1 | done (2026-10-22) |
| `showcase-reward.png` | Showcase gallery | Phase 1 | done (2026-10-22) |
| `showcase-purchase.png` | Showcase gallery | Phase 1 | done (2026-10-22) |
| `showcase-settings.png` | Showcase gallery | Phase 1 | done (2026-10-22) |
| `setup-general.png` | Tutorial step 1 | Episode 1, beat 5 | done (2026-10-22) |
| `setup-new-element.png` | Tutorial step 2 | Episode 2, beat 3 | done (2026-10-22) |
| `scene-hierarchy.png` | Tutorial step 3 | Episode 3, beat 6 | done (2026-10-22) |
| `setup-fix-dialog.png` | Tutorial step 5 | Episode 5, beat 4 | done (2026-10-22) |
| `setup-elements.png` | Tutorial step 4 (Elements tab) | Episode 6, beat 5 | done (2026-10-22) |
| `setup-states.png` | Tutorial step 7 | Episode 7, beat 5 | done (2026-10-22) |

### Videos (YouTube ids go into the `data-video-id` slots)

| Video | Script | Slot `data-title` | Target length | Status |
| --- | --- | --- | --- | --- |
| Episode 1 | [01-install-and-config.md](scripts/01-install-and-config.md) | Step 1: Install and create the config | 2 min | todo |
| Episode 2 | [02-first-element.md](scripts/02-first-element.md) | Step 2: Create your first element | 2 min | todo |
| Episode 3 | [03-build-the-screen.md](scripts/03-build-the-screen.md) | Step 3: Build a screen under Root | 3 min | todo |
| Episode 4 | [04-animations.md](scripts/04-animations.md) | Step 4: Choose animations | 1 min | todo |
| Episode 5 | [05-validate-and-fix.md](scripts/05-validate-and-fix.md) | Step 5: Validate and fix scenes | 2 min | todo |
| Episode 6 | [06-canvas-and-addressables.md](scripts/06-canvas-and-addressables.md) | Step 6: Canvas and Addressables settings | 2 min | todo |
| Episode 7 | [07-states.md](scripts/07-states.md) | Step 7: States and shared elements | 2 min | todo |
| Episode 8 | [08-build-runtime-config.md](scripts/08-build-runtime-config.md) | Step 8: Build the runtime config | 1 min | todo |
| Episode 9 | [09-show-from-code.md](scripts/09-show-from-code.md) | Step 9: Show states from code | 3 min | todo |
| Episode 10 | [10-popup.md](scripts/10-popup.md) | Step 10: Popups and overlays | 3 min | todo |
| Full walkthrough | [00-full-walkthrough.md](scripts/00-full-walkthrough.md) | (slot removed) | about 12 min (estimate 13:40) | dropped: the Tutorial tab opens with the showcase video as its intro, and every video stays short |
| Showcase | [showcase.md](scripts/showcase.md) | The showcase app in the Unity editor | about 1 min | todo |

When a video's real length differs from the slot, update `data-length` too.

## Phase 0: Set up once

### Tools

- [ ] **OBS Studio** for editor recordings. Canvas and output 1920 × 1080, 60 fps, recording format MKV (remux to MP4 after), encoder NVENC or x264 at CQ/CRF 18.
- [ ] **Unity Recorder** package (Package Manager → Unity Registry) for Game view captures at exactly 1080 × 1920: the showcase stills and the showcase video.
- [ ] A video editor that does zooms, speed ramps and captions (DaVinci Resolve, Premiere, CapCut, …).
- [ ] Windows Focus Assist on, notifications off, chat apps closed.

### The Unity editor look (same in every recording and screenshot)

- [ ] Unity 6, **dark** editor theme, editor UI scaling **100%** (Preferences → UI Scaling).
- [ ] Layout: **Hierarchy** left, **Scene / Game** centre, **Inspector** right with **UI Setup Manager** docked as a tab next to it, **Project** and **Console** at the bottom. Save it as a layout called `Tutorial`.
- [ ] Game view resolution: add **1080 × 1920 Portrait**.
- [ ] Close every window the episode doesn't use. Clear the Console before each take.

### The tutorial project (Coin Shop)

- [ ] New Unity 6 project from the **Universal 2D** (or 2D) template, named `CoinShop`.
- [ ] Import **TextMeshPro Essentials** off camera (Window → TextMeshPro → Import TMP Essential Resources).
- [ ] Commit the empty project to git (or copy it) so any episode can be re-recorded from the right starting point. After each episode, commit again with the episode number, e.g. `Tutorial episode 3 end`.
- [ ] Palette for the screens, so every take looks the same: background `#12142A`, panel `#20244A`, accent `#FFC83D`, text `#FFFFFF`.

### Recording rhythm

- Record **screen only** first, following the **SCREEN** lines. Move the mouse slowly, pause one second after every click, and leave a two-second still at the start and end of each beat.
- Record the **voiceover later**, one beat at a time, from the **SAY** lines (Phase 4). Read slowly: about 140 words a minute.
- In the edit, **zoom in** whenever a small editor control is clicked (the UI Setup Manager is small at 1080p), speed up waits (compiling, importing) to 4×, and add the **ON SCREEN** captions.

## Phase 1: Showcase screenshots (repository project)

Open this repository's project in Unity 6, `Assets/Scenes/Bootstrapper.unity`, Game view 1080 × 1920. Use Unity Recorder (Recorder → Image Sequence → single frame, Game view, 1080 × 1920, PNG) and save straight into `docs/images/` with the exact name.

- [ ] `showcase-home.png`: Home tab after the reveal animation has finished (season card, daily reward, quests).
- [ ] `showcase-store.png`: Store tab, Items page, offer banner visible.
- [ ] `showcase-profile.png`: Profile tab after the XP bar has filled.
- [ ] `showcase-reward.png`: Reward popup while the confetti is in the air.
- [ ] `showcase-purchase.png`: Confirm-purchase bottom sheet for an item you can afford.
- [ ] `showcase-settings.png`: Settings popup with at least one toggle on.

## Phase 2: Record the tutorial (Coin Shop project)

Record Episodes 1 to 10 in order, in one or two sessions. Each script lists its starting point, its end point and the screenshot to take during it (the tracker above). Take the screenshots with Windows + Shift + S (window mode) at the moment the script marks with 📸, crop to the editor window, and save them as PNG into `docs/images/`.

- [ ] Episode 1 recorded, 📸 `setup-general.png`
- [ ] Episode 2 recorded, 📸 `setup-new-element.png`
- [ ] Episode 3 recorded, 📸 `scene-hierarchy.png`
- [ ] Episode 4 recorded
- [ ] Episode 5 recorded, 📸 `setup-fix-dialog.png`
- [ ] Episode 6 recorded, 📸 `setup-elements.png`
- [ ] Episode 7 recorded, 📸 `setup-states.png`
- [ ] Episode 8 recorded
- [ ] Episode 9 recorded
- [ ] Episode 10 recorded

## Phase 3: Record the showcase video (repository project)

- [ ] Follow [scripts/showcase.md](scripts/showcase.md): Game view with Unity Recorder (movie, 1080 × 1920) plus one OBS take of the editor.

## Phase 4: Voiceover

- [ ] Quiet room, the same mic and distance every session, a test take checked on headphones first.
- [ ] Record each script's **SAY** lines beat by beat, with a pause between beats so they are easy to cut. Read from [publish/voiceover.md](publish/voiceover.md): it lists every beat with its read time and flags beats that run longer than their screen time.
- [ ] Episodes 1–10, the full walkthrough intro and outro, and the showcase.

## Phase 5: Edit

- [ ] For each episode: lay the voiceover over the screen recording, cut dead time, zoom on small controls, add the ON SCREEN captions, add the end card ("Next: …").
- [ ] Assemble the **full walkthrough** from the ten episodes as described in [00-full-walkthrough.md](scripts/00-full-walkthrough.md).
- [ ] Export 1920 × 1080 MP4 (H.264, 60 fps, high bitrate).
- [x] Thumbnails for all 12 videos are in [publish/thumbnails/](publish/thumbnails/) (1280 × 720).
- [ ] Captions: start from [publish/captions/](publish/captions/) (`.srt`, timed from the script beats); re-time them to the final edit before uploading.

## Phase 6: Publish and wire up

- [ ] Upload each video (the `/youtube-publish` skill does it; upload as unlisted first, check it, then make it public). Titles, descriptions, chapters, tags, playlist order, end screens and pinned comments are ready in [publish/youtube.md](publish/youtube.md); fix the chapter times to the final cut.
- [ ] Paste each YouTube id into its slot's `data-video-id` in `docs/index.html`, and fix `data-length` if needed.
- [ ] Open the page: the Tutorial tab's "Video series" list should say **Watch** for every episode, and every screenshot slot should show the image, not a placeholder.
- [ ] Add a `CHANGELOG.md` `[Unreleased]` line when the videos and screenshots go in.

Everything in `publish/` is generated by `python3 docs/production/publish/build_kit.py` (run from the repository root) from the scripts and screenshots; rerun it after editing a script.
