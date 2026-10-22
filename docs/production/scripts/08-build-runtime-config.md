# Episode 8 · Build the runtime config

| | |
| --- | --- |
| Length | about 1 min |
| Docs slot | Tutorial step 8 (`#t-runtime`), "Step 8: Build the runtime config" |
| Starts with | End of Episode 7 |
| Ends with | `RuntimeUiConfig` built; every element scene Addressable in the `UiElements` group |
| Screenshot | none |

## Script

**1. One button** (0:00)
- SCREEN: UI Setup Manager → **General** tab → **⚙️ Build Runtime UiConfig**. The message **✅ Built runtime UiConfig successfully.** appears.
- SAY: "Back in the General tab, press Build Runtime UiConfig."

**2. What it did** (0:10)
- SCREEN: Project window: click `Assets/UiConfigs/RuntimeUiConfig.asset`, show its state entries in the Inspector. Then Window → Asset Management → Addressables → **Groups**: open the `UiElements` group and show the four element scenes in it; then open the `GlobalConfigs` group and find the config entry with the address `UiConfig`.
- SAY: "It wrote the runtime config: every state and its scenes. It made every one of those scenes Addressable, in a group called UI Elements, so the game can load them when it needs them and unload them when it doesn't. And it registered the config itself, in its own Global Configs group, so the game can find it at start-up."

**3. The rule** (0:35)
- SCREEN: Back on the General tab, the build button highlighted.
- SAY: "One rule to remember: after any change to your states or scenes, press this button again. The config is generated, so never edit it by hand."
- ON SCREEN: `Changed a state or scene? Build again.`

**4. Wrap up** (0:50)
- SAY: "Everything is ready. Next, a few lines of code, and we press Play."
- ON SCREEN: `Next: Show states from code`

## After recording

- [ ] Commit: `Tutorial episode 8 end`.
