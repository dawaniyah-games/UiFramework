# Short narration (reel style)

One line per beat, in the same order as the episode scripts, so each line still lands on its footage. Conversational, no filler. Replaces the SAY lines when the voiceover is generated. Voice: the designed "UiFramework Narrator" (ElevenLabs v4), mastered to -16 LUFS. Reviewed against each beat's footage on 23 Oct 2026.

## 01 · Install and create the config
1. Let's build the whole UI for a small game, Coin Shop. A menu, a store, shared bars and a popup. One quick step per video.
2. Package Manager, plus, Add package from git URL. Paste the link from the docs, and hit Add. Addressables comes with it.
3. Open Addressables Groups and create the settings. Then set Play Mode Script to Use Asset Database.
4. Now open the UI Setup Manager. Press Create Config, and set your two namespaces.
5. Every new screen starts from these canvas defaults: a portrait phone, ten-eighty by nineteen-twenty.
6. That's the setup. You only do it once.

## 02 · Create your first element
1. Every screen, bar or popup is an element: its own scene, with its own script.
2. Elements tab, New Element.
3. Name it Main Menu UI Element, and tick Include Parameters Class.
4. Create, and let Unity compile.
5. You get the script, a parameters class, and a ready-made scene with the script already attached.
6. Green tick. Ready to build.

## 03 · Build a screen under Root
1. Double-click to open its scene.
2. Every element has the same shape. Root is the part that animates.
3. So, one rule: everything goes under Root.
4. Add a background and stretch it to fill. Then a title. Just normal Unity UI.
5. Two buttons: Play and Store. And here it is on a phone screen.
6. Everything's under Root. Save.
7. Screen done. Next, let's animate it.

## 04 · Choose animations
1. Select the menu and find Animations.
2. Show: Scale. It grows in as it fades in.
3. Hide: Fade. That's saved straight into the scene.
4. You can see all ten presets in the docs.
5. Next: keeping your scenes healthy.

## 05 · Validate and fix scenes
1. Scenes break. So let's break one on purpose.
2. Drag the title out of Root. Save, and refresh.
3. Warning sign. Validate All tells you exactly what's wrong.
4. Fix Structure shows you a preview before it changes anything.
5. Confirm, and it's valid again. Fixes never delete anything, and Undo works.
6. Fix All does every scene at once.

## 06 · Canvas and Addressables settings
1. Two more elements: a top bar and a nav bar. Both sit on every screen.
2. Coins at the top. Home and Store at the bottom. Everything under Root.
3. Sorting order decides what's on top. Screens zero, bars ten, popups a hundred.
4. Render mode and scaling live here too.
5. Addressables? That's set for you when you build the config.
6. The pieces are ready. Next: states.

## 07 · States and shared elements
1. One more screen: the store. Three items, and it slides in from the right.
2. Here's the key idea. A state is just a list of elements.
3. Create the main menu state, and add the menu, the top bar and the nav bar.
4. Then the store state: the store, with the same two bars.
5. The bars are shared. Switch screens, and they stay put. No reload, no flicker.
6. Next: one button that builds it all.

## 08 · Build the runtime config
1. In the General tab, press Build Runtime UI Config.
2. It writes your states into the config, and makes every scene Addressable for you.
3. Changed a state or a scene? Press it again.
4. Next: a bit of code, and Play.

## 09 · Show states from code
1. A start-up scene: Bootstrap, with a camera and an event system.
2. Two lines: Initialize, then Show State. No UI manager to set up.
3. Press Play. The menu and both bars, each in its own scene.
4. The Store button: one line shows the store state. Wire it up, and save.
5. Same for the nav bar: Home and Store.
6. Tap Store. It slides in, and the bars never move.
7. Last step: a popup.

## 10 · Popups and overlays
1. A popup is just another state, shown on top.
2. A new element: a dimmed backdrop, a panel and two buttons. Sorting order a hundred.
3. Pass in the item's name and price, and show them in Populate. Hide UI closes it.
4. Give it a state, and build the config again.
5. Buy shows the popup state with additive set to true: put it on top, keep what's below.
6. Tap Buy. The popup's on top, and the store's still there. Cancel, and it's gone.
7. And that's Coin Shop: screens, shared bars and a popup, with barely any code. The showcase app goes way further. Link below.

## Showcase
1. A full mobile game UI, built only with UI Framework. Every screen is its own scene.
2. Four tabs under shared bars. The tabs slide, the bars stay.
3. Popups on top: rewards, coins flying, a purchase sheet, settings.
4. All set up in one window. Clone it, and press Play.

## Full tutorial intro
Here's the whole Coin Shop UI, from an empty project to a finished game UI. Use the chapters to jump to any step.
