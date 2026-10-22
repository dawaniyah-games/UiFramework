# Episode 5 · Validate and fix scenes

| | |
| --- | --- |
| Length | about 2 min |
| Docs slot | Tutorial step 5 (`#t-validate`), "Step 5: Validate and fix scenes" |
| Starts with | End of Episode 4 |
| Ends with | Same project, structure fixed again (✓) |
| Screenshot | 📸 `setup-fix-dialog.png` at beat 4 |

## Script

**1. Why it matters** (0:00)
- SCREEN: Elements tab, `MainMenuUiElement ✓`.
- SAY: "In a real project, scenes get edited by lots of people, and sooner or later something ends up in the wrong place. Let's break our menu on purpose and watch the tools catch it."

**2. Break it** (0:12)
- SCREEN: Open the MainMenu scene. Drag `Title` out of `Root` so it sits directly under `Canvas`, next to `Root`. Save. Back in the UI Setup Manager press **Refresh**.
- SAY: "I'll drag the title out of Root, so it sits next to it. Remember the rule: everything under Root. Save, and refresh the list."

**3. See the problem** (0:30)
- SCREEN: The row now shows ⚠. The summary line shows `1 fixable`. Click **Validate All**, then show the Console: one line for the scene with its issue. Tick **Only show elements with issues**, then untick it.
- SAY: "The menu now shows a warning sign: fixable. Validate All writes one clear line per scene to the Console. And this toggle shows only the scenes that need attention, which is handy when you have fifty."

**4. Preview the fix** (0:55)
- SCREEN: Select the element → **Fix Structure…**. The dialog shows **What will change** with the **Now** and **After** hierarchy side by side: now, `Root` and `Title` both sit under `Canvas`; after, both sit inside a new full-screen `ContentRoot`, in the same order. Hover both.
- 📸 Take `setup-fix-dialog.png` here: the dialog with the Now / After preview.
- SAY: "Press Fix Structure. Before it touches anything, it shows you exactly what will change. Two objects sit under the Canvas, so it will put both inside one new full-screen object called Content Root, in the same order. Nothing moves on screen; it all just animates together again."

**5. Apply it, then undo** (1:20)
- SCREEN: Confirm. The row is ✓ again. Show the scene: `Canvas → ContentRoot (new) → Root, Title`. Press Ctrl + Z: the old hierarchy is back. Then drag `Title` into the original `Root` by hand, save, **Refresh**: ✓.
- SAY: "Confirm, and the scene is valid again. Fixes only create and move objects, never delete anything, and they undo like any other change. Here I'd rather put the title back where it belongs, so I undo and drag it into Root myself. Anything the tool can't fix safely is listed under What to fix by hand."

**6. Wrap up** (1:50)
- SAY: "Fix All Structure does the same for every scene at once. Next, canvases and sorting."
- ON SCREEN: `Next: Canvas and Addressables settings`

## After recording

- [ ] Commit: `Tutorial episode 5 end` (the title must be back under the original Root, with a ✓).
- [ ] `setup-fix-dialog.png` saved.
