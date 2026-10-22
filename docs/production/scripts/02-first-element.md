# Episode 2 · Create your first element

| | |
| --- | --- |
| Length | about 2 min |
| Docs slot | Tutorial step 2 (`#t-element`), "Step 2: Create your first element" |
| Starts with | End of Episode 1 |
| Ends with | `MainMenuUiElement`: its script, a params class and its scene |
| Screenshot | 📸 `setup-new-element.png` at beat 3 |

## Script

**1. What an element is** (0:00)
- SCREEN: UI Setup Manager, **Elements** tab, empty list.
- SAY: "Every piece of UI in this framework is an element: a screen, a bar or a popup. Each element is its own small scene with its own script. Our first one is the main menu."

**2. Open the form** (0:15)
- SCREEN: Click **➕ New Element**. The form opens on the right.
- SAY: "In the Elements tab, press New Element."

**3. Fill it in** (0:25)
- SCREEN: **Name**: type `MainMenuUiElement`. Leave **From Existing Scene** empty. Tick **Include Parameters Class**. Leave **Include Reference Class** off. Hover each field as it is mentioned.
- 📸 Take `setup-new-element.png` here, before pressing the button: the form filled in.
- SAY: "Name it Main Menu UI Element. Names end in UI Element, so you always know what you're looking at. Tick Include Parameters Class: that gives us a small class for the data we'll pass to this screen later. You could also turn a scene you already have into an element here, but we're starting fresh."
- ON SCREEN: `Name ends in UiElement`

**4. Create it** (0:55)
- SCREEN: Click **➕ Create UI Element**. Wait for Unity to compile (speed up). The element appears in the list with a ✓.
- SAY: "Press Create UI Element, and give Unity a moment to compile."

**5. What you got** (1:10)
- SCREEN: In the Project window, show `Assets/Scripts/Ui/UiElements/MainMenuUiElement.cs`, `MainMenuUiElementParams.cs`, and the scene `Assets/Scenes/UiElements/MainMenuUiElement.unity`. Open the script briefly, then back.
- SAY: "You get three things. The script. The parameters class. And a scene for this screen, already set up the way the framework expects, with the script attached for you. If a script with this name already exists, it is never overwritten."

**6. Wrap up** (1:45)
- SCREEN: The Elements tab with `MainMenuUiElement ✓` selected.
- SAY: "The green tick means the scene is ready. Next, we build the screen itself."
- ON SCREEN: `Next: Build a screen under Root`

## After recording

- [ ] Commit: `Tutorial episode 2 end`.
- [ ] `setup-new-element.png` saved.
