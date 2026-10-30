# Verification and hand-off

You usually can't run Unity. Do the checks you can (read the files, run the project's scripts), and give the user the exact list for the rest. Never say "verified" for a check you didn't run.

## Checks for the user (or for you, with an editor bridge)

1. **Structure:** UI Setup Manager → Elements → Validate All. Every new scene is ✓ (`<Name>Root → Canvas → Root`).
2. **Canvas:** the new scenes' Canvas Scaler matches the profile (UI scale mode, reference resolution, match mode and value).
3. **Config:** General → Build Runtime UiConfig. No errors.
4. **Compile:** no console errors or warnings from the new scripts.
5. **Wiring:** every `[SerializeField]` on the new components is assigned (no "is not assigned" warnings when the screen opens).
6. **Devices:** Device Simulator on every device in the profile, portrait (and landscape if the profile allows it). Nothing clipped or overlapping, nothing under the notch or home indicator, touch targets big enough, text not cut.
7. **Long content:** longest translation or name, the biggest numbers, empty lists, RTL if the profile has it.
8. **Transitions:** open and close each state in Play Mode. No flash, no leftover scenes in the Hierarchy, shared bars don't re-animate, popups close with Back.
9. **Performance (low-end device):** Batches within the budget, the screen draws only its feature atlas + Common + font (Frame Debugger), no Canvas rebuild every frame (Profiler), Addressables Analyze shows no duplicate textures.
10. **Project checks:** the project's standards script or linter, if it has one (UI Framework repo: `bash tools/check-standards.sh`).

## Hand-off report

```
What I built: <screens / states / helpers, one line each, and why>
Files: <scripts, scenes, atlases, profile changes>
Art: <sprites imported, atlas names, sizes, compression>
Verified: <what you actually checked>
Not verified, please run: <numbered checks from the list above>
Not in the design, built to match: <empty / loading / error / locked / RTL ...>
Open questions: <anything you assumed>
```
