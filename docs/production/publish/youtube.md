# YouTube publishing sheet

Everything to paste when uploading each video: title, description (with chapters and links), tags, thumbnail, captions, playlist and end screen. Generated from the scripts by `build_kit.py`; after the edit, check every chapter time against the final timeline (YouTube needs the first chapter at 0:00 and each chapter at least 10 s long).

**Playlist:** *UiFramework Tutorial* (already created on the channel, unlisted, manual order) (Episodes 1 to 10 in order, then the full tutorial and the showcase).  
**Upload as:** Unlisted, check it, then Public. Category *Education*, language English, captions from `captions/`.  
**Never use `<` or `>` in a title or description:** YouTube rejects them, so code stays in the docs and the pinned comment links there.

## Episode 1 · Install and create the config

| | |
| --- | --- |
| Thumbnail | `thumbnails/01.png` |
| Captions | `captions/01.srt` (timed from the script; nudge after the edit) |
| Target length | about 2 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-config` (`docs/index.html`) |
| End screen | Next episode (Episode 2) + subscribe |

**Title**

```text
Install and create the config · Unity UI Framework Tutorial 1/10
```

**Description**

```text
Start the Coin Shop tutorial: install UI Framework from its git URL, turn on Addressables, and create the config the UI Setup Manager works from. You do this once per project.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-config

Chapters
0:00 What we'll build
0:15 Install the package
0:40 Turn on Addressables
1:00 Open the UI Setup Manager and create the config
1:25 Check the canvas defaults
1:50 Wrap up

Next: Episode 2 · Create your first element

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, Unity package manager, git URL package, Addressables setup
```

**Pinned comment**

```text
Package URL for Window → Package Manager → + → Add package from git URL: https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework  If Unity asks about Legacy Bundles when you create the Addressables settings, press Ignore.
```

## Episode 2 · Create your first element

| | |
| --- | --- |
| Thumbnail | `thumbnails/02.png` |
| Captions | `captions/02.srt` (timed from the script; nudge after the edit) |
| Target length | about 2 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-element` (`docs/index.html`) |
| End screen | Next episode (Episode 3) + subscribe |

**Title**

```text
Create your first element · Unity UI Framework Tutorial 2/10
```

**Description**

```text
Every screen, bar and popup is an element: its own small scene with its own script. Create the Main Menu element in one click and see the three files you get.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-element

Chapters
0:00 What an element is
0:15 Open the form
0:25 Fill it in
0:55 Create it
1:10 What you got
1:45 Wrap up

Previous: Episode 1 · Install and create the config
Next: Episode 3 · Build a screen under Root

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, Unity scenes, UI element, code generation
```

## Episode 3 · Build a screen under Root

| | |
| --- | --- |
| Thumbnail | `thumbnails/03.png` |
| Captions | `captions/03.srt` (timed from the script; nudge after the edit) |
| Target length | about 3 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-build` (`docs/index.html`) |
| End screen | Next episode (Episode 4) + subscribe |

**Title**

```text
Build a screen under Root · Unity UI Framework Tutorial 3/10
```

**Description**

```text
Open the Main Menu scene and build it: background, title and two buttons. The one rule: everything goes under Root, because Root is what animates.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-build

Chapters
0:00 Open the scene
0:15 The shape
0:40 The one rule
0:55 Build the background and title
1:45 Add two buttons
2:30 Check the hierarchy
2:50 Wrap up

Previous: Episode 2 · Create your first element
Next: Episode 4 · Choose animations

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, TextMeshPro, Unity canvas, UI layout
```

## Episode 4 · Choose animations

| | |
| --- | --- |
| Thumbnail | `thumbnails/04.png` |
| Captions | `captions/04.srt` (timed from the script; nudge after the edit) |
| Target length | about 1 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-animate` (`docs/index.html`) |
| End screen | Next episode (Episode 5) + subscribe |

**Title**

```text
Choose animations · Unity UI Framework Tutorial 4/10
```

**Description**

```text
Pick how the Main Menu appears and leaves from ten tuned presets. No code, and you can still override it for a single call later.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-animate

Chapters
0:00 Select the element
0:12 Pick a show animation
0:30 Pick a hide animation
0:42 Where to see them all
0:55 Wrap up

Previous: Episode 3 · Build a screen under Root
Next: Episode 5 · Validate and fix scenes

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, UI animation, tween presets, screen transitions
```

## Episode 5 · Validate and fix scenes

| | |
| --- | --- |
| Thumbnail | `thumbnails/05.png` |
| Captions | `captions/05.srt` (timed from the script; nudge after the edit) |
| Target length | about 2 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-validate` (`docs/index.html`) |
| End screen | Next episode (Episode 6) + subscribe |

**Title**

```text
Validate and fix scenes · Unity UI Framework Tutorial 5/10
```

**Description**

```text
Break the menu on purpose and watch the tools catch it: Validate All, the issues filter, and Fix Structure with its before / after preview.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-validate

Chapters
0:00 Why it matters
0:12 Break it
0:30 See the problem
0:55 Preview the fix
1:20 Apply it, then undo
1:50 Wrap up

Previous: Episode 4 · Choose animations
Next: Episode 6 · Canvas and Addressables settings

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, scene validation, editor tooling, hierarchy
```

## Episode 6 · Canvas and Addressables settings

| | |
| --- | --- |
| Thumbnail | `thumbnails/06.png` |
| Captions | `captions/06.srt` (timed from the script; nudge after the edit) |
| Target length | about 2 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-canvas` (`docs/index.html`) |
| End screen | Next episode (Episode 7) + subscribe |

**Title**

```text
Canvas and Addressables settings · Unity UI Framework Tutorial 6/10
```

**Description**

```text
Add a top bar and a nav bar, and stack everything with sorting orders: screens 0, bars 10, popups 100. See where each scene's Addressables settings live.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-canvas

Chapters
0:00 Two bars
0:20 Build them
0:55 Sorting order
1:15 The other canvas settings
1:30 Addressables
1:50 Wrap up

Previous: Episode 5 · Validate and fix scenes
Next: Episode 7 · States and shared elements

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, sorting order, Canvas Scaler, nav bar
```

## Episode 7 · States and shared elements

| | |
| --- | --- |
| Thumbnail | `thumbnails/07.png` |
| Captions | `captions/07.srt` (timed from the script; nudge after the edit) |
| Target length | about 2 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-state` (`docs/index.html`) |
| End screen | Next episode (Episode 8) + subscribe |

**Title**

```text
States and shared elements · Unity UI Framework Tutorial 7/10
```

**Description**

```text
A state is what the player sees at one moment: a list of elements. Build the Main Menu and Store states, sharing the two bars so they never reload.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-state

Chapters
0:00 One more screen
0:30 What a state is
0:45 Create MainMenuUiState
1:15 Create StoreUiState
1:30 Shared elements
1:55 Wrap up

Previous: Episode 6 · Canvas and Addressables settings
Next: Episode 8 · Build the runtime config

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, UI states, shared UI, state machine
```

## Episode 8 · Build the runtime config

| | |
| --- | --- |
| Thumbnail | `thumbnails/08.png` |
| Captions | `captions/08.srt` (timed from the script; nudge after the edit) |
| Target length | about 1 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-runtime` (`docs/index.html`) |
| End screen | Next episode (Episode 9) + subscribe |

**Title**

```text
Build the runtime config · Unity UI Framework Tutorial 8/10
```

**Description**

```text
One button turns your states into the config the game loads and makes every scene Addressable. Rebuild after every change.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-runtime

Chapters
0:00 One button
0:10 What it did
0:35 The rule
0:50 Wrap up

Previous: Episode 7 · States and shared elements
Next: Episode 9 · Show states from code

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, Addressables groups, runtime config
```

## Episode 9 · Show states from code

| | |
| --- | --- |
| Thumbnail | `thumbnails/09.png` |
| Captions | `captions/09.srt` (timed from the script; nudge after the edit) |
| Target length | about 3 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-code` (`docs/index.html`) |
| End screen | Next episode (Episode 10) + subscribe |

**Title**

```text
Show states from code · Unity UI Framework Tutorial 9/10
```

**Description**

```text
Two lines start the UI, one line switches screens. Press Play: the store slides in while the shared bars stay put.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-code

Chapters
0:00 A start-up scene
0:20 Start the UI
0:55 First Play
1:20 Make the Store button work
1:55 The nav bar
2:15 Play it
2:45 Wrap up

Previous: Episode 8 · Build the runtime config
Next: Episode 10 · Popups and overlays

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, async await, C#, UI navigation
```

**Pinned comment**

```text
All the code from this video, ready to copy: https://dawaniyah-games.github.io/UiFramework/#t-code  The element scripts override Awake and call base.Awake() first, so the show and hide animations keep working.
```

## Episode 10 · Popups and overlays

| | |
| --- | --- |
| Thumbnail | `thumbnails/10.png` |
| Captions | `captions/10.srt` (timed from the script; nudge after the edit) |
| Target length | about 3 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#t-popup` (`docs/index.html`) |
| End screen | The showcase app + the full tutorial |

**Title**

```text
Popups and overlays · Unity UI Framework Tutorial 10/10
```

**Description**

```text
Finish Coin Shop with a purchase popup: a state shown on top with additive: true, filled with data in Populate, closed with HideUI.

This step in the docs: https://dawaniyah-games.github.io/UiFramework/#t-popup

Chapters
0:00 What a popup is
0:10 Make the popup element
0:55 Its data
1:30 Its state
1:50 Open it from the store
2:20 Play it
2:45 Series wrap up

Previous: Episode 9 · Show states from code

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, popup, modal, overlay
```

**Pinned comment**

```text
All the code from this video, ready to copy: https://dawaniyah-games.github.io/UiFramework/#t-popup  The element scripts override Awake and call base.Awake() first, so the show and hide animations keep working.
```

## The whole tutorial

| | |
| --- | --- |
| Thumbnail | `thumbnails/full.png` |
| Captions | `captions/full.srt` (timed from the script; nudge after the edit) |
| Target length | about 13:40 (voice-led estimate: voiceover plus 2.5 s per beat; the chapter times below use it) |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#tutorial` (`docs/index.html`) |
| End screen | Playlist + subscribe |

**Title**

```text
Build a Unity Game UI from Scratch · UI Framework Full Tutorial
```

**Description**

```text
Build a small game UI in Unity from an empty project in about sixteen minutes: screens, shared bars and a popup, each in its own scene, set up in the editor with a few lines of code. Chapters below.

Chapters
0:00 Intro
0:28 Install and create the config
1:44 Create your first element
3:01 Build a screen under Root
4:22 Choose animations
5:12 Validate and fix scenes
6:49 Canvas and Addressables settings
8:13 States and shared elements
9:28 Build the runtime config
10:17 Show states from code
11:44 Popups and overlays

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, full tutorial, beginner, step by step
```

## The showcase app

| | |
| --- | --- |
| Thumbnail | `thumbnails/showcase.png` |
| Captions | `captions/showcase.srt` (timed from the script; nudge after the edit) |
| Target length | about 1 min |
| Docs slot | Paste the YouTube id into `data-video-id` of the video slot in `#showcase` (`docs/index.html`) |
| End screen | Playlist + subscribe |

**Title**

```text
A Complete Mobile Game UI in Unity · UI Framework Showcase
```

**Description**

```text
A complete mobile game UI built only with UI Framework: an intro, four tabs under shared bars, and reward, purchase and settings popups. Clone the repository and press Play.

Chapters
0:00 The editor
0:12 The tabs
0:30 The popups
0:48 Change it

Install (Package Manager, Add package from git URL): https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
Source, showcase app and issues: https://github.com/dawaniyah-games/UiFramework

Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial

#Unity #UnityTutorial #GameUI
```

**Tags**

```text
Unity, Unity tutorial, Unity UI, game UI, Unity UI framework, Addressables, Unity 6, mobile game UI, UI architecture, Unity editor tools, Coin Shop, UiManager, showcase, mobile UI, UI demo
```
