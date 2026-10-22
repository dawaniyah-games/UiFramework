# Prompts for Claude in the desktop app (Computer use)

Use these to have Claude take the docs screenshots and help with the tutorial recordings by controlling Unity on this computer.

**How to use them**

1. Open the **Claude desktop app** → Settings → This computer → turn on **Computer use**.
2. Start a **new chat for each prompt** below and paste the whole prompt. Approve access to **Unity**, **Unity Hub** and **Snipping Tool** when asked.
3. When a prompt finishes, check `D:\Repos\UiFramework\docs\images\` and run `git status` in `D:\Repos\UiFramework`: only new `.png` files should appear. If anything else changed, discard it (`git restore <file>`).
4. Tick the items in [README.md](README.md) (or tell Claude Code, which keeps the tracker).

Prompt 1 and 2 capture all twelve missing screenshots. Prompt 3 is the tutorial briefing: paste it once, then ask for one episode at a time.

---

## Prompt 1 · Showcase screenshots (play mode)

```text
You are helping me capture screenshots of a Unity game UI for a documentation website. Use computer use to drive the Unity editor on this computer.

PROJECT
- Unity project: D:\Repos\UiFramework (open it from Unity Hub if it isn't open; use the Unity 6 version Unity Hub suggests for it).
- Scene to play: Assets/Scenes/Bootstrapper.unity
- It is a mobile game UI showcase: an intro, then four tabs (Home, Store, Ranks, Profile) in a bottom nav bar, a top bar with currencies and a settings button, and popups.

RULES (important)
- Do NOT save any scene, asset or project setting. Do not install packages. If Unity asks to save, choose "Don't Save".
- Only create PNG files in D:\Repos\UiFramework\docs\images\ with exactly the names below. Overwrite nothing else.
- If you see compile errors, an unexpected dialog, or anything you are unsure about, stop and ask me.

SETUP
1. Open Assets/Scenes/Bootstrapper.unity.
2. In the Game view, set the resolution to the 1080x1920 portrait entry (it may be called "Full HD portrait (1080x1920)"; if it's missing, add 1080 x 1920 with the + at the bottom of the resolution list).
3. Maximize the Game view (hover it and press Shift+Space) and set its Scale so the whole phone screen is visible and as large as possible. Turn off any Gizmos/Stats overlays.
4. Press Play. Wait for the intro to finish and the Home tab to appear.

HOW TO SAVE EACH SCREENSHOT
- Open Snipping Tool, choose Rectangle mode, select exactly the phone screen inside the Game view (no editor chrome, no Game view toolbar), then Save (Ctrl+S) as PNG to D:\Repos\UiFramework\docs\images\ with the exact file name.
- Wait for animations to finish before each capture, unless the step says otherwise.

SCREENSHOTS
1. showcase-home.png: the Home tab after its reveal animation has finished (season card, daily reward, quests visible).
2. showcase-store.png: tap Store in the bottom nav bar; the Items page with the offer banner visible.
3. showcase-profile.png: tap Profile; wait until the XP bar has finished filling.
4. showcase-settings.png: open the Settings popup (gear button in the top bar, or from Profile); make sure at least one toggle is on; capture the popup over the screen. Close it.
5. showcase-purchase.png: go to Store, tap an item you can afford; capture the confirm-purchase bottom sheet once it has slid in. Then cancel/close it without buying.
6. showcase-reward.png: go to Home and claim the daily reward (the CLAIM button); capture the Reward popup while the confetti is in the air (take it about half a second after it appears; retry if you miss it: stop Play, open Settings → reset data, or restart Play).

FINISH
- Stop Play mode. Do not save. Tell me the six file paths and show me each image so I can check them.
```

---

## Prompt 2 · Editor screenshots (UI Setup Manager)

```text
You are helping me capture screenshots of a Unity editor tool for a documentation website. Use computer use to drive the Unity editor on this computer.

PROJECT
- Unity project: D:\Repos\UiFramework (open it from Unity Hub if needed).
- The tool is the "UI Setup Manager" window: menu UiFramework → UI Setup Manager. It has three tabs: General, Elements, States.

RULES (important)
- Do NOT save any scene, asset, script or setting, and never press "Build Runtime UiConfig", "Fix Structure…", "Fix All Structure…", "Create Config" or "➕ Create UI Element" to completion. If a dialog would change something, press Cancel. If Unity asks to save, choose "Don't Save".
- Only create PNG files in D:\Repos\UiFramework\docs\images\ with exactly the names below.
- If you see compile errors or anything unexpected, stop and ask me.

EDITOR LOOK (set once, for every screenshot)
- Dark editor theme, Preferences → UI Scaling 100%.
- Layout: Hierarchy left, Scene/Game centre, Inspector right with the UI Setup Manager docked as a tab beside it, Project and Console at the bottom. Make the UI Setup Manager wide enough that nothing in it is cut off.

HOW TO SAVE EACH SCREENSHOT
- Snipping Tool → Window mode or Rectangle mode as the step says → Save (Ctrl+S) as PNG to D:\Repos\UiFramework\docs\images\ with the exact name.

SCREENSHOTS
1. setup-general.png: UI Setup Manager → General tab. Show the config paths, namespaces and the "Global Canvas Settings" section, scrolled so all of it is visible. Capture the UI Setup Manager window (rectangle around it).
2. setup-new-element.png: Elements tab → press "➕ New Element". In the form type Name: StoreUiElement, tick "Include Parameters Class", leave the rest. Capture the UI Setup Manager with the filled form. Then press Cancel (do NOT create it).
3. setup-elements.png: Elements tab, close the form if open, select StoreUiElement in the list. The right side shows its Structure, Animations, Canvas and Addressables sections; scroll so as many sections as possible are visible. Capture the whole UI Setup Manager (list on the left, details on the right).
4. scene-hierarchy.png: double-click StoreUiElement in the list to open its scene. In the Hierarchy expand StoreUiElementRoot → Canvas → Root and its first-level children. Frame the Scene view on the UI (select Root, press F). Capture the Hierarchy and Scene view together (rectangle). Do not change anything in the scene.
5. setup-states.png: States tab, select StoreUiState so its element scenes (StoreUiElement, TopBarUiElement, NavBarUiElement) are listed. Capture the UI Setup Manager.
6. setup-fix-dialog.png (this one needs a temporary change that must be undone):
   a. Open the StoreUiElement scene. In the Hierarchy, under Canvas → Root, drag ONE child object (for example the first one) out so it sits directly under Canvas, next to Root.
   b. In the UI Setup Manager press Refresh; StoreUiElement now shows a warning. Select it and press "Fix Structure…".
   c. A confirmation dialog shows "What will change" with a Now / After preview of the hierarchy. Capture this dialog (Window mode).
   d. Press Cancel in the dialog. Then press Ctrl+Z in the Scene/Hierarchy until the object is back under Root exactly as before. Do NOT save. If the scene still shows unsaved changes, close it with "Don't Save" (File → Open the scene again and choose Don't Save when asked).
   e. Press Refresh: StoreUiElement must show the green tick again.

FINISH
- Make sure no scene is left modified (no * on the scene tab); if one is, discard the changes without saving.
- Tell me the six file paths and show me each image so I can check them.
```

---

## Prompt 3 · Tutorial briefing (paste once, then ask for one episode at a time)

```text
You are my assistant for recording a 10-episode Unity video tutorial for "UI Framework", a Unity package that builds game UI from Addressable scenes. I record the screen myself with OBS and record the voice later. Use computer use to drive Unity when I ask, but NEVER while I'm recording unless I say so: your mouse moves too fast for a tutorial.

WHERE THINGS ARE
- Full scripts (SCREEN / SAY / ON SCREEN beats, code, screenshot moments): D:\Repos\UiFramework\docs\production\scripts\ (files 01-…md to 10-…md). Read the episode's file before helping with it.
- Production plan and tracker: D:\Repos\UiFramework\docs\production\README.md
- The tutorial project is a NEW, separate Unity 6 project called CoinShop (not the UiFramework repository). We build one small game UI in it, step by step:
  Main Menu (title, Play, Store), Store (three item cards), shared TopBar (coin count) and NavBar (Home, Store), and a Confirm Purchase popup. Namespaces: CoinShop.Ui.Elements and CoinShop.Ui.States. Colours: background #12142A, panel #20244A, accent #FFC83D, text white.
- The package is installed from: https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework
- The tool window: UiFramework → UI Setup Manager (tabs General, Elements, States).

THE EPISODES (each starts exactly where the previous ended)
1. Install and create the config: install the package by git URL; Window → Asset Management → Addressables → Groups → Create Addressables Settings, Play Mode Script = Use Asset Database; UI Setup Manager → General → 🔄 Create Config; set the two namespaces; show Global Canvas Settings (Screen Space - Overlay, Scale With Screen Size, 1080 x 1920, Expand).
2. Create your first element: Elements → ➕ New Element → MainMenuUiElement, tick Include Parameters Class → ➕ Create UI Element; it makes the script, MainMenuUiElementParams and a scene shaped MainMenuUiElementRoot → Canvas → Root.
3. Build a screen under Root: open the scene; everything goes under Root; add a full-screen Background image, a "Coin Shop" title (TextMeshPro), a Play and a Store button.
4. Choose animations: select the element → Animations: Show = Scale, Hide = Fade.
5. Validate and fix scenes: drag the title out of Root (next to it), Refresh → warning; Validate All; Fix Structure… shows Now/After (it wraps both objects in a new ContentRoot); apply, then undo and drag the title back by hand.
6. Canvas and Addressables settings: create TopBarUiElement (top strip, coin text) and NavBarUiElement (bottom strip, Home and Store buttons); set both Canvas Sorting Order to 10 (screens 0, popups 100); show the Addressables section.
7. States and shared elements: create StoreUiElement (three item cards, show/hide Slide Left); States tab: MainMenuUiState = MainMenu + TopBar + NavBar; StoreUiState = Store + TopBar + NavBar.
8. Build the runtime config: General → ⚙️ Build Runtime UiConfig; show RuntimeUiConfig and the UiElements Addressables group.
9. Show states from code: Bootstrap scene with an EventSystem and GameBootstrap.cs (UiManager.InitializeAsync, ShowState<MainMenuUiState>); Store button and NavBar buttons call ShowState; Play: the bars stay while screens change.
10. Popups and overlays: ConfirmPurchaseUiElement (dimmed backdrop + panel, Sorting Order 100, Include Parameters Class with ItemName and Price); ConfirmPurchaseUiState with only that scene; rebuild the config; Store's Buy button calls ShowState<ConfirmPurchaseUiState>(purchase, additive: true); Buy/Cancel call UiManager.HideUI().

HOW YOU CAN HELP
- "Prepare episode N": bring the CoinShop project to the exact START state of episode N by doing the previous episodes' steps off camera (follow the scripts), then tell me it's ready. Don't do any of episode N itself.
- "Check episode N": after I record, compare the project with the episode's END state in its script and list anything missing or different.
- "Rehearse episode N": read me the episode's SCREEN steps one at a time, in order, while I perform them; wait for me to say "next".
- "Screenshot for episode N": take the 📸 screenshot that the script marks, save it as PNG to D:\Repos\UiFramework\docs\images\ with the name the script gives.

RULES
- Never touch the D:\Repos\UiFramework Unity project unless I ask; work in CoinShop.
- Don't commit, push, delete or rename anything outside CoinShop.
- If something in Unity doesn't match the script (a button with another name, an error), stop and tell me exactly what you see.

Reply "Ready" and wait for my first request.
```

---

## Tutorial descriptions (for YouTube and the docs)

Short descriptions for each video, ready to paste into the YouTube description box. Add the docs link at the end: `Docs: https://dawaniyah-games.github.io/UiFramework/#tutorial`

| Video | Description |
| --- | --- |
| Episode 1 · Install and create the config | Start the Coin Shop tutorial: install UI Framework from its git URL, turn on Addressables, and create the config the UI Setup Manager works from. You do this once per project. |
| Episode 2 · Create your first element | Every screen, bar and popup is an element: its own small scene with its own script. Create the Main Menu element in one click and see the three files you get. |
| Episode 3 · Build a screen under Root | Open the Main Menu scene and build it: background, title and two buttons. The one rule: everything goes under Root, because Root is what animates. |
| Episode 4 · Choose animations | Pick how the Main Menu appears and leaves from ten tuned presets. No code, and you can still override it for a single call later. |
| Episode 5 · Validate and fix scenes | Break the menu on purpose and watch the tools catch it: Validate All, the issues filter, and Fix Structure with its before / after preview. |
| Episode 6 · Canvas and Addressables settings | Add a top bar and a nav bar, and stack everything with sorting orders: screens 0, bars 10, popups 100. See where each scene's Addressables settings live. |
| Episode 7 · States and shared elements | A state is what the player sees at one moment: a list of elements. Build the Main Menu and Store states, sharing the two bars so they never reload. |
| Episode 8 · Build the runtime config | One button turns your states into the config the game loads and makes every scene Addressable. Rebuild after every change. |
| Episode 9 · Show states from code | Two lines start the UI, one line switches screens. Press Play: the store slides in while the shared bars stay put. |
| Episode 10 · Popups and overlays | Finish Coin Shop with a purchase popup: a state shown on top with additive: true, filled with data in Populate, closed with HideUI. |
| The whole tutorial | Build a small game UI in Unity from an empty project in about twelve minutes: screens, shared bars and a popup, each in its own scene, set up in the editor with a few lines of code. Chapters below. |
| The showcase app | A complete mobile game UI built only with UI Framework: an intro, four tabs under shared bars, and reward, purchase and settings popups. Clone the repository and press Play. |
