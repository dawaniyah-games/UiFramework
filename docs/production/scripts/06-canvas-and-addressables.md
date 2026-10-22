# Episode 6 · Canvas and Addressables settings

| | |
| --- | --- |
| Length | about 2 min |
| Docs slot | Tutorial step 6 (`#t-canvas`), "Step 6: Canvas and Addressables settings" |
| Starts with | End of Episode 5 |
| Ends with | Two more elements, `TopBarUiElement` and `NavBarUiElement`, built and sorted above the screens |
| Screenshot | 📸 `setup-elements.png` at beat 5 |

## Script

**1. Two bars** (0:00)
- SCREEN: Elements tab → **➕ New Element** → `TopBarUiElement` → **➕ Create UI Element**. Again for `NavBarUiElement`. (Speed both up.)
- SAY: "Our game needs two bars that sit on top of every screen: a top bar with the player's coins, and a nav bar at the bottom. Two more elements, made exactly like the main menu."

**2. Build them** (0:20)
- SCREEN: Open `TopBarUiElement`: under `Root` add a strip image anchored to the top (height 160, `#20244A`) with a TMP text `Coins 1,250` (accent `#FFC83D`). Open `NavBarUiElement`: under `Root` a strip anchored to the bottom (height 200) with two TMP buttons, `HomeButton` (label `Home`) and `StoreButton` (label `Store`). Leave the rest of each screen empty: the screens underneath show through. (Speed up to 4×.)
- SAY: "A strip at the top with the coin count. A strip at the bottom with a Home and a Store button. Everything under Root, as always. The rest of each bar's screen stays empty, so the screen underneath shows through."

**3. Sorting order** (0:55)
- SCREEN: UI Setup Manager → select `TopBarUiElement` → **Canvas** section → **Sorting Order** `10`. Same for `NavBarUiElement`. Show `MainMenuUiElement` still at `0`.
- SAY: "Each element has its own canvas, and the Canvas section edits it right here. Sorting order decides what's on top. Screens stay at zero, bars go to ten. Later, popups go to a hundred."
- ON SCREEN: `Screens 0 · Bars 10 · Popups 100`

**4. The other canvas settings** (1:15)
- SCREEN: Hover **Render Mode**, **UI Scale Mode**, **Reference Resolution**, **Screen Match Mode** in the same section.
- SAY: "Render mode and scaling are here too, per element. They start from the global defaults, and Use Default Settings puts them back."

**5. Addressables** (1:30)
- SCREEN: Scroll to the **Addressables** section of `NavBarUiElement`. The element list on the left shows all three elements (`MainMenuUiElement`, `NavBarUiElement`, `TopBarUiElement`) with ✓, NavBar selected.
- 📸 Take `setup-elements.png` here: the Elements tab with the list, the search box, and the selected element's Structure, Animations, Canvas and Addressables sections.
- SAY: "The Addressables section shows whether this scene is Addressable, and its group. You don't have to set it by hand: when we build the config, every scene a state uses is made Addressable for you."

**6. Wrap up** (1:50)
- SAY: "Our pieces are ready. Next, we combine them into states."
- ON SCREEN: `Next: States and shared elements`

## After recording

- [ ] Commit: `Tutorial episode 6 end`.
- [ ] `setup-elements.png` saved.
