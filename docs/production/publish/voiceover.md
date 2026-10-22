# Voiceover read sheet

Every SAY line from the scripts, beat by beat, in recording order. Read at about 140 words a minute; the *fits* column compares the estimated read time with the gap to the next beat. Leave a two-second pause between beats so they are easy to cut. Generated from `docs/production/scripts/` by `docs/production/publish/build_kit.py`; edit the scripts, not this file.

## Episode 1 · Install and create the config

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. What we'll build | 0:00 | 15 s | 19 s | **+4 s** | In this series we build the UI for a small game called Coin Shop, from an empty project. A main menu, a store, a top bar and a nav bar shared by both, and a purchase popup. One short step per video. Let's start. |
| 2. Install the package | 0:15 | 25 s | 13 s | yes | Open the Package Manager, press plus, and choose Add package from git URL. Paste the address from the docs and press Add. Unity installs the framework, and Addressables comes with it. |
| 3. Turn on Addressables | 0:40 | 20 s | 15 s | yes | Every screen will be an Addressable scene, so open Addressables Groups once and create its settings. Set the play mode script to Use Asset Database, so screens load straight from your project when you press Play. |
| 4. Open the UI Setup Manager and create the config | 1:00 | 25 s | 20 s | yes | This window is where you'll build all of your UI. In the General tab, press Create Config. The config remembers where your scripts and scenes go. Set the two namespaces to your game's: Coin Shop dot UI dot Elements, and Coin Shop dot UI dot States. |
| 5. Check the canvas defaults | 1:25 | 25 s | 18 s | yes | Below are the Global Canvas Settings. Every new screen starts with these: a portrait phone at ten-eighty by nineteen-twenty, scaling with the screen. Change them here once, and every new screen follows. Existing screens are never changed behind your back. |
| 6. Wrap up | 1:50 | 10 s | 6 s | yes | That's the setup, and you only do it once. Next, we create our first screen. |

## Episode 2 · Create your first element

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. What an element is | 0:00 | 15 s | 15 s | yes | Every piece of UI in this framework is an element: a screen, a bar or a popup. Each element is its own small scene with its own script. Our first one is the main menu. |
| 2. Open the form | 0:15 | 10 s | 3 s | yes | In the Elements tab, press New Element. |
| 3. Fill it in | 0:25 | 30 s | 24 s | yes | Name it Main Menu UI Element. Names end in UI Element, so you always know what you're looking at. Tick Include Parameters Class: that gives us a small class for the data we'll pass to this screen later. You could also turn a scene you already have into an element here, but we're starting fresh. |
| 4. Create it | 0:55 | 15 s | 5 s | yes | Press Create UI Element, and give Unity a moment to compile. |
| 5. What you got | 1:10 | 35 s | 18 s | yes | You get three things. The script. The parameters class. And a scene for this screen, already set up the way the framework expects, with the script attached for you. If a script with this name already exists, it is never overwritten. |
| 6. Wrap up | 1:45 | 15 s | 6 s | yes | The green tick means the scene is ready. Next, we build the screen itself. |

## Episode 3 · Build a screen under Root

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. Open the scene | 0:00 | 15 s | 9 s | yes | Double-click the element to open its scene. Every element scene has the same simple shape, so let's look at it. |
| 2. The shape | 0:15 | 25 s | 16 s | yes | At the top, the element's root, with our script on it. Under it, one Canvas. And under the Canvas, one object called Root. Root is the important one: it's what moves when the screen animates in and out. |
| 3. The one rule | 0:40 | 15 s | 6 s | yes | So there's one rule: everything you build goes under Root. Nothing sits next to it. |
| 4. Build the background and title | 0:55 | 50 s | 15 s | yes | Right-click Root and add an image for the background. Stretch it to fill the screen. Then a title. Use normal anchors, exactly like any Unity UI: the canvas scaler takes care of different phone sizes. |
| 5. Add two buttons | 1:45 | 45 s | 14 s | yes | Now a Play button, and a Store button under it. We'll make the Store button work later in the series. Here it is in the Game view, on a phone-sized screen. |
| 6. Check the hierarchy | 2:30 | 20 s | 6 s | yes | Everything lives under Root. That's what keeps the animations working. Save the scene. |
| 7. Wrap up | 2:50 | 10 s | 5 s | yes | Our first screen is built. Next, we give it an animation. |

## Episode 4 · Choose animations

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. Select the element | 0:00 | 12 s | 8 s | yes | Every element animates in and out on its own. Select the main menu and find its Animations section. |
| 2. Pick a show animation | 0:12 | 18 s | 11 s | yes | Pick how it appears. Each choice tells you in one line what it looks like. I'll take Scale: it grows in gently while it fades in. |
| 3. Pick a hide animation | 0:30 | 12 s | 8 s | yes | And how it leaves. Fade. The choice is saved straight into the scene. There's nothing else to set up. |
| 4. Where to see them all | 0:42 | 13 s | 12 s | yes | You can watch all ten presets moving on the Animations page of the docs. And from code, you can still pick a different one for a single call. |
| 5. Wrap up | 0:55 | 6 s | 5 s | yes | Next, we'll see how the tools keep every scene in shape. |

## Episode 5 · Validate and fix scenes

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. Why it matters | 0:00 | 12 s | 15 s | **+3 s** | In a real project, scenes get edited by lots of people, and sooner or later something ends up in the wrong place. Let's break our menu on purpose and watch the tools catch it. |
| 2. Break it | 0:12 | 18 s | 10 s | yes | I'll drag the title out of Root, so it sits next to it. Remember the rule: everything under Root. Save, and refresh the list. |
| 3. See the problem | 0:30 | 25 s | 15 s | yes | The menu now shows a warning sign: fixable. Validate All writes one clear line per scene to the Console. And this toggle shows only the scenes that need attention, which is handy when you have fifty. |
| 4. Preview the fix | 0:55 | 25 s | 21 s | yes | Press Fix Structure. Before it touches anything, it shows you exactly what will change. Two objects sit under the Canvas, so it will put both inside one new full-screen object called Content Root, in the same order. Nothing moves on screen; it all just animates together again. |
| 5. Apply it, then undo | 1:20 | 30 s | 24 s | yes | Confirm, and the scene is valid again. Fixes only create and move objects, never delete anything, and they undo like any other change. Here I'd rather put the title back where it belongs, so I undo and drag it into Root myself. Anything the tool can't fix safely is listed under What to fix by hand. |
| 6. Wrap up | 1:50 | 10 s | 6 s | yes | Fix All Structure does the same for every scene at once. Next, canvases and sorting. |

## Episode 6 · Canvas and Addressables settings

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. Two bars | 0:00 | 20 s | 15 s | yes | Our game needs two bars that sit on top of every screen: a top bar with the player's coins, and a nav bar at the bottom. Two more elements, made exactly like the main menu. |
| 2. Build them | 0:20 | 35 s | 17 s | yes | A strip at the top with the coin count. A strip at the bottom with a Home and a Store button. Everything under Root, as always. The rest of each bar's screen stays empty, so the screen underneath shows through. |
| 3. Sorting order | 0:55 | 20 s | 15 s | yes | Each element has its own canvas, and the Canvas section edits it right here. Sorting order decides what's on top. Screens stay at zero, bars go to ten. Later, popups go to a hundred. |
| 4. The other canvas settings | 1:15 | 15 s | 9 s | yes | Render mode and scaling are here too, per element. They start from the global defaults, and Use Default Settings puts them back. |
| 5. Addressables | 1:30 | 20 s | 15 s | yes | The Addressables section shows whether this scene is Addressable, and its group. You don't have to set it by hand: when we build the config, every scene a state uses is made Addressable for you. |
| 6. Wrap up | 1:50 | 10 s | 4 s | yes | Our pieces are ready. Next, we combine them into states. |

## Episode 7 · States and shared elements

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. One more screen | 0:00 | 30 s | 9 s | yes | One more screen: the store. Same steps as before, with three items to buy. It slides in from the right. |
| 2. What a state is | 0:30 | 15 s | 15 s | yes | Now the key idea. A state is what the player sees at one moment, written as a list of elements. The main menu state is the menu, plus the top bar, plus the nav bar. |
| 3. Create MainMenuUiState | 0:45 | 30 s | 17 s | yes | Type the name next to New state and press plus. It also creates a small class we'll use from code. Then add its scenes: start typing and pick from the list. The menu, the top bar and the nav bar. |
| 4. Create StoreUiState | 1:15 | 15 s | 5 s | yes | Same for the store: the store screen, with the same two bars. |
| 5. Shared elements | 1:30 | 25 s | 17 s | yes | Notice the bars are in both states. That's what shared means: when the player goes from the menu to the store, the bars stay loaded and on screen, and only the screen in the middle changes. No reloading, no flicker. |
| 6. Wrap up | 1:55 | 7 s | 6 s | yes | Next, one button turns all of this into the config the game loads. |

## Episode 8 · Build the runtime config

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. One button | 0:00 | 10 s | 4 s | yes | Back in the General tab, press Build Runtime UiConfig. |
| 2. What it did | 0:10 | 25 s | 26 s | **+1 s** | It wrote the runtime config: every state and its scenes. It made every one of those scenes Addressable, in a group called UI Elements, so the game can load them when it needs them and unload them when it doesn't. And it registered the config itself, in its own Global Configs group, so the game can find it at start-up. |
| 3. The rule | 0:35 | 15 s | 11 s | yes | One rule to remember: after any change to your states or scenes, press this button again. The config is generated, so never edit it by hand. |
| 4. Wrap up | 0:50 | 10 s | 6 s | yes | Everything is ready. Next, a few lines of code, and we press Play. |

## Episode 9 · Show states from code

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. A start-up scene | 0:00 | 20 s | 14 s | yes | The game needs one scene that starts everything. A new scene called Bootstrap, with a camera and an event system for button clicks, and an empty object for our start-up script. |
| 2. Start the UI | 0:20 | 35 s | 13 s | yes | Two lines start the UI. Initialize loads the config we built. Show State shows the main menu. That's it: you never put a UI manager in your scene, it creates itself. |
| 3. First Play | 0:55 | 25 s | 9 s | yes | Press Play. There's our menu, scaling in, with both bars. Look at the Hierarchy: each element was loaded as its own scene. |
| 4. Make the Store button work | 1:20 | 35 s | 11 s | yes | Now the Store button. When it's clicked, we show the store state. One line again. Drag the button into the field, and save the scene. |
| 5. The nav bar | 1:55 | 20 s | 8 s | yes | The nav bar does the same with two buttons: Home shows the menu state, Store shows the store state. |
| 6. Play it | 2:15 | 30 s | 17 s | yes | Press Play and tap Store. The store slides in, and the bars stay exactly where they are: they're shared, so they never reload. Home and back. Scenes load when a state needs them and unload when nothing uses them. |
| 7. Wrap up | 2:45 | 15 s | 9 s | yes | In a bigger game, send these calls through one navigator class, as the showcase app does. Last step: a popup. |

## Episode 10 · Popups and overlays

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. What a popup is | 0:00 | 10 s | 9 s | yes | Last step: a popup. In this framework, a popup is just another state, shown on top of the current one. |
| 2. Make the popup element | 0:10 | 45 s | 21 s | yes | A new element. The first thing under Root is a dimmed backdrop that covers the whole screen, then the panel with a message and two buttons. It scales in, it fades out, and its sorting order is a hundred, so it sits above the screens and the bars. |
| 3. Its data | 0:55 | 35 s | 15 s | yes | The parameters class carries the item's name and price. Populate puts them into the message. And both buttons close the popup with Hide UI: it removes the top state, and whatever was underneath simply stays. |
| 4. Its state | 1:30 | 20 s | 8 s | yes | Its state has just this one scene. And because we changed the states, we build the config again. |
| 5. Open it from the store | 1:50 | 30 s | 13 s | yes | In the store, the Buy button fills in the parameters and shows the popup state, with additive set to true. Additive means: put it on top, and keep everything below it. |
| 6. Play it | 2:20 | 25 s | 14 s | yes | Play, go to the store, and tap Buy. The popup opens on top with the item's details, and the store stays right where it was. Cancel, and it's gone, scene and all. |
| 7. Series wrap up | 2:45 | 22 s | 20 s | yes | And that's Coin Shop: screens, shared bars and a popup, every piece in its own scene, set up in the editor, with a handful of lines of code. The showcase app in the repository goes much further, so open it and see what's possible. Thanks for watching. |

## The whole tutorial

Only the intro is new voiceover; the rest is the episodes' audio.

| Beat | Start | Words | Read time | Line |
| --- | --- | --- | --- | --- |
| Intro | 0:00 | 59 | 25 s | In the next twelve minutes we build the UI for a small game from an empty Unity project: a main menu, a store, two bars they share, and a purchase popup. Every screen in its own scene, set up in the editor, with only a few lines of code. Follow along, or jump to any step with the chapters. |

## The showcase app

| Beat | Start | Gap | Read time | Fits | Line |
| --- | --- | --- | --- | --- | --- |
| 1. The editor | 0:00 | 12 s | 15 s | **+3 s** | This is the showcase app that comes with the framework: a complete mobile game UI, built only from what you've seen in the tutorial. Every screen is its own scene, loaded when it's needed. |
| 2. The tabs | 0:12 | 18 s | 10 s | yes | Four tabs under a shared top bar and nav bar. The tabs slide in the direction you're going, and the bars never move. |
| 3. The popups | 0:30 | 18 s | 9 s | yes | Popups sit on top of whatever is open: a reward with confetti, coins flying into the top bar, a purchase sheet, settings. |
| 4. Change it | 0:48 | 12 s | 10 s | yes | And every bit of it is set up in the same window. Change an animation, press Play, done. Clone the repository and try it. |
