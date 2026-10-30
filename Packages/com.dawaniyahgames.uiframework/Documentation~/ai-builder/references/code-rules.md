# Code rules for UI code

From UI Framework's CODING_STANDARDS.md (the DenDon team rules). The project's own `AGENTS.md`, `.editorconfig` and instructions win for game code; these apply wherever they are silent. Before writing anything, read the nearest existing element, display and state in the project and copy their shape.

## How to work
- Read the existing code path first. Don't guess the architecture.
- Small changes at the root cause. Don't rename, reformat or refactor what you weren't asked to touch.
- Extend what exists before adding a base class, a manager or a new pattern.
- Keep public APIs, serialized field names, scene structure and asset GUIDs as they are; scenes and prefabs store them by name.
- Implement → self-review (bugs, regressions, missing null checks, style drift) → hand-off report. Don't push or publish unless asked.

## Naming
| Thing | Style | Example |
| --- | --- | --- |
| Element (scene + script) | `<Name>UiElement` | `StoreUiElement` |
| State | `<Name>UiState`, class name = state key exactly | `StoreUiState` |
| Reusable sub-widget | `<Name>UiDisplay` | `CoinPackUiDisplay` |
| Populate data | `<Element>Params : UiPopulationParams` (what the Create form generates), or the project's existing `*Params` / `*Context` names | `StoreUiElementParams` |
| Generic layout helper | What it does | `SafeAreaFitter` |
| Types, methods, properties | PascalCase | `RefreshBalance` |
| Private fields, serialized fields | camelCase, no `_` or `m_` | `titleText` |
| Constants (any access level) | PascalCase | `MaxRows` |
| Booleans | Read as statements | `isLocked`, `hasRewards` |
| Async methods returning Task | end in `Async` | `LoadAvatarAsync` |
- Write `Ui`, not `UI`, in new type names.

## C#
- No `var`. No target-typed `new()`. No expression-bodied members (`=>`); use block bodies.
- Allman braces, braces on every `if` / `for` / `while`, one statement per line, 4 spaces.
- `using` directives at the top, outside the namespace, `System.*` first.
- Block-scoped namespaces (C# 9 on Unity 2022.3).
- No try/catch for validation or parsing: guard up front (null, empty, range) and return early. try/catch only around real async or IO boundaries, and it logs with context.

## Unity
- Never `?.`, `??` or `is null` on anything derived from `UnityEngine.Object`. Use `== null` / `!= null`.
- Inspector fields are `[SerializeField] private`. No public fields for wiring.
- Lifecycle methods in execution order; override `protected virtual` ones and call `base`.
- Cache `GetComponent`. No allocations, LINQ or `GetComponent` in `Update` or tween callbacks.
- UI timers and tweens use `Time.unscaledDeltaTime`.
- No layout, anchors, size or safe area from gameplay code (see responsiveness).

## UI Framework
- `Populate(object context)` is the only data entry point. Cast with `as` / `is`, return early on null, and keep it idempotent: clear what you spawned before spawning again; guard one-time setup with a flag.
- Show and hide only through `UiManager.ShowState<T>` / `ShowStateByKey` / `HideUI`, awaited and never overlapping (use the project's navigator or task runner if it has one).
- `UiState` subclasses are type markers. No behaviour in them.
- Elements never load or unload scenes; `UiManager` owns them.
- `async void` only for Unity messages and button handlers, and it catches and logs exceptions.
- Never block on a task (`.Wait()`, `.Result`).

## Logs
- Every message starts with a tag: `Debug.LogWarning("[StoreUiElement] coinPackTemplate is not assigned. Assign it in the scene.", this);`
- Say what happened and what to do. No emoji, no per-frame logs, no debug spam left in.

## Git
- Branch off `dev` (`feature/*`, `fix/*`), PR back to `dev`. Don't push or publish unless asked.
- Commit subjects: imperative, sentence case, no prefix, no trailing period, about 72 characters, describing the outcome ("Add the store coin packs screen").
- Commit every new asset with its `.meta`. Never edit GUIDs.

## Reference shape

```csharp
using System.Collections.Generic;
using UiFramework.Core;
using UnityEngine;

namespace MyGame.Ui.Elements
{
    public class StoreUiElementParams : UiPopulationParams
    {
        public List<CoinPackData> Packs { get; set; }
    }

    public class StoreUiElement : UiElement
    {
        [SerializeField] private Transform packContainer;
        [SerializeField] private CoinPackUiDisplay packTemplate;

        private readonly List<CoinPackUiDisplay> spawnedPacks = new List<CoinPackUiDisplay>();

        public override void Populate(object context = null)
        {
            base.Populate(context);

            // Runs again whenever a state reuses this scene, so rebuild instead of appending.
            ClearPacks();

            StoreUiElementParams data = context as StoreUiElementParams;
            if (data == null || data.Packs == null)
            {
                return;
            }

            if (packContainer == null || packTemplate == null)
            {
                Debug.LogWarning("[StoreUiElement] packContainer or packTemplate is not assigned.", this);
                return;
            }

            for (int i = 0; i < data.Packs.Count; i++)
            {
                CoinPackUiDisplay pack = Instantiate(packTemplate, packContainer);
                pack.gameObject.SetActive(true);
                pack.Bind(data.Packs[i]);
                spawnedPacks.Add(pack);
            }
        }

        private void ClearPacks()
        {
            for (int i = 0; i < spawnedPacks.Count; i++)
            {
                if (spawnedPacks[i] != null)
                {
                    Destroy(spawnedPacks[i].gameObject);
                }
            }

            spawnedPacks.Clear();
        }
    }
}
```
