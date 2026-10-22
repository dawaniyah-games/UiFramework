# Episode 7 · States and shared elements

| | |
| --- | --- |
| Length | about 2 min |
| Docs slot | Tutorial step 7 (`#t-state`), "Step 7: States and shared elements" |
| Starts with | End of Episode 6 |
| Ends with | `StoreUiElement` built; states `MainMenuUiState` and `StoreUiState` |
| Screenshot | 📸 `setup-states.png` at beat 5 |

## Script

**1. One more screen** (0:00)
- SCREEN: **➕ New Element** → `StoreUiElement`, **Include Parameters Class** off → **➕ Create UI Element**. Open it; under `Root` a background like the menu's, a title `Store`, and three item cards (panel `#20244A`, a TMP name and price, a `BuyButton` each). Show animation **Slide Left**, hide **Slide Left**. (Speed up to 4×.)
- SAY: "One more screen: the store. Same steps as before, with three items to buy. It slides in from the right."

**2. What a state is** (0:30)
- SCREEN: UI Setup Manager → **States** tab, empty.
- SAY: "Now the key idea. A state is what the player sees at one moment, written as a list of elements. The main menu state is the menu, plus the top bar, plus the nav bar."
- ON SCREEN: `State = a list of elements`

**3. Create MainMenuUiState** (0:45)
- SCREEN: Type `MainMenuUiState` next to **New state:** → **➕**. Select it → **Add element scene**: type `Main`, pick `MainMenuUiElement`; type `Top`, pick `TopBarUiElement`; type `Nav`, pick `NavBarUiElement`.
- SAY: "Type the name next to New state and press plus. It also creates a small class we'll use from code. Then add its scenes: start typing and pick from the list. The menu, the top bar and the nav bar."

**4. Create StoreUiState** (1:15)
- SCREEN: `StoreUiState` → **➕** → add `StoreUiElement`, `TopBarUiElement`, `NavBarUiElement`.
- SAY: "Same for the store: the store screen, with the same two bars."

**5. Shared elements** (1:30)
- SCREEN: Select `StoreUiState`, its three scenes listed. Hover the two bars in both states.
- 📸 Take `setup-states.png` here: the States tab with `StoreUiState` selected and its element scenes.
- SAY: "Notice the bars are in both states. That's what shared means: when the player goes from the menu to the store, the bars stay loaded and on screen, and only the screen in the middle changes. No reloading, no flicker."
- ON SCREEN: `Shared: stays on screen`

**6. Wrap up** (1:55)
- SAY: "Next, one button turns all of this into the config the game loads."
- ON SCREEN: `Next: Build the runtime config`

## After recording

- [ ] Commit: `Tutorial episode 7 end`.
- [ ] `setup-states.png` saved.
