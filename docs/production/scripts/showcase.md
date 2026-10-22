# The showcase app in the Unity editor

| | |
| --- | --- |
| Length | about 1 min |
| Docs slot | Showcase tab, "The showcase app in the Unity editor" |
| Project | This repository's host project, `Assets/Scenes/Bootstrapper.unity` |
| Recording | OBS for the whole editor (beats 1 and 4), Unity Recorder for the Game view at 1080 × 1920 (beats 2 and 3) |

## Before you record

- [ ] Editor in the `Tutorial` layout, Game view 1080 × 1920, Console cleared.
- [ ] Reset the showcase's saved data (Settings popup → reset, or a fresh play) so balances and owned items start fresh.

## Script

**1. The editor** (0:00)
- SCREEN (OBS): The whole editor in Play mode on the Home tab. In the Hierarchy, expand the loaded scenes: `MainMenuUiElement`, `TopBarUiElement`, `NavBarUiElement`.
- SAY: "This is the showcase app that comes with the framework: a complete mobile game UI, built only from what you've seen in the tutorial. Every screen is its own scene, loaded when it's needed."

**2. The tabs** (0:12)
- SCREEN (Recorder): Home → Store → Leaderboard (wait for the podium to rise) → Profile (the XP bar fills).
- SAY: "Four tabs under a shared top bar and nav bar. The tabs slide in the direction you're going, and the bars never move."

**3. The popups** (0:30)
- SCREEN (Recorder): Home → claim the daily reward: the Reward popup with confetti, then the coins fly to the top bar. Store → tap an item: the confirm sheet → Buy. Tap the settings button: the Settings popup, flip a toggle, close.
- SAY: "Popups sit on top of whatever is open: a reward with confetti, coins flying into the top bar, a purchase sheet, settings."

**4. Change it** (0:48)
- SCREEN (OBS): Stop. UI Setup Manager → Elements → `StoreUiElement` → change **Show** to **Scale**. Play → Store: it now scales in.
- SAY: "And every bit of it is set up in the same window. Change an animation, press Play, done. Clone the repository and try it."
- ON SCREEN: `github.com/dawaniyah-games/UiFramework`
