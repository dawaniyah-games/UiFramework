# Episode 3 · Build a screen under Root

| | |
| --- | --- |
| Length | about 3 min |
| Docs slot | Tutorial step 3 (`#t-build`), "Step 3: Build a screen under Root" |
| Starts with | End of Episode 2 |
| Ends with | The Main Menu: background, title "Coin Shop", Play and Store buttons |
| Screenshot | 📸 `scene-hierarchy.png` at beat 6 |

## Before you record

- [ ] Game view set to **1080 × 1920 Portrait**.

## Script

**1. Open the scene** (0:00)
- SCREEN: Elements tab, double-click **MainMenuUiElement**. The scene opens. Expand the whole hierarchy.
- SAY: "Double-click the element to open its scene. Every element scene has the same simple shape, so let's look at it."

**2. The shape** (0:15)
- SCREEN: Click each object in turn: `MainMenuUiElementRoot`, then `Canvas`, then `Root`.
- SAY: "At the top, the element's root, with our script on it. Under it, one Canvas. And under the Canvas, one object called Root. Root is the important one: it's what moves when the screen animates in and out."
- ON SCREEN: `Root → Canvas → Root → your content`

**3. The one rule** (0:40)
- SCREEN: Highlight `Root` in the Hierarchy.
- SAY: "So there's one rule: everything you build goes under Root. Nothing sits next to it."
- ON SCREEN: `Everything goes under Root`

**4. Build the background and title** (0:55)
- SCREEN: Right-click `Root` → UI → Image, name it `Background`, stretch it full screen (anchor preset Alt + stretch-stretch), colour `#12142A`. Right-click `Root` → UI → Text - TextMeshPro, name it `Title`, text `Coin Shop`, size 120, centred, anchored to the top. (Speed up the typing and colour picking in the edit.)
- SAY: "Right-click Root and add an image for the background. Stretch it to fill the screen. Then a title. Use normal anchors, exactly like any Unity UI: the canvas scaler takes care of different phone sizes."

**5. Add two buttons** (1:45)
- SCREEN: Right-click `Root` → UI → Button - TextMeshPro, name it `PlayButton`, label `Play`, anchored centre. Duplicate it, name it `StoreButton`, label `Store`, move it below. Accent colour `#FFC83D`. Switch to the Game view to show the result.
- SAY: "Now a Play button, and a Store button under it. We'll make the Store button work later in the series. Here it is in the Game view, on a phone-sized screen."

**6. Check the hierarchy** (2:30)
- SCREEN: Scene view and Hierarchy side by side, everything expanded: `MainMenuUiElementRoot → Canvas → Root → Background, Title, PlayButton, StoreButton`.
- 📸 Take `scene-hierarchy.png` here: Hierarchy and Scene view, the full tree visible.
- SAY: "Everything lives under Root. That's what keeps the animations working. Save the scene."

**7. Wrap up** (2:50)
- SCREEN: Ctrl + S. The Game view with the finished menu.
- SAY: "Our first screen is built. Next, we give it an animation."
- ON SCREEN: `Next: Choose animations`

## After recording

- [ ] Commit: `Tutorial episode 3 end`.
- [ ] `scene-hierarchy.png` saved.
