# UI Framework - Copilot Instructions

## Package Information

**Package Name**: `com.mohamedmoghazy.uiframework`  
**Version**: 1.0.0  
**Repository**: https://github.com/mohamedmoghazy/UiFramework

### Installation (UPM)

Via Package Manager:

```
https://github.com/mohamedmoghazy/UiFramework.git?path=/Assets/UiFramework
```

Via manifest.json:

```json
"com.mohamedmoghazy.uiframework": "https://github.com/mohamedmoghazy/UiFramework.git?path=/Assets/UiFramework"
```

## Architecture Overview

This is a Unity UI framework using **Addressables-based scene composition** with a LIFO state stack. UI screens are loaded as additive scenes and populated with runtime data through the `IUiElement.Populate(object context)` contract.

**Core Flow**: `UiManager.ShowState<T>(context)` → looks up state in `UiConfig` → `UiState.Init()` loads scenes via Addressables → discovers `IUiElement` components → calls `Populate(context)` on each.

## Key Components

- **`UiManager`** (`Assets/UiFramework/Runtime/Manager/UiManager.cs`): Singleton orchestrator with LIFO state stack. API: `ShowState<T>()`, `ShowStateByKey()`, `HideUI()`, `GetCurrentState()`.
- **`UiState`** (`Assets/UiFramework/Core/UiCompoenets/UiState.cs`): Loads addressable scenes additively, discovers `IUiElement` components, handles scene unloading.
- **`UiConfig`** (`Assets/UiFramework/Core/Config/UiConfig.cs`): ScriptableObject mapping `stateKey` (string) → `List<AssetReference>` scenes. Lives at `Assets/UiFramework/Runtime/UiConfig.asset`.
- **`IUiElement` / `UiElement`** (`Assets/UiFramework/Core/UiCompoenets/`): Interface/base class for UI components. Override `Populate(object context)` to receive runtime data.

## Critical Patterns

### Type-to-Key Mapping

`UiManager` resolves types via reflection: `GetTypeForKey()` searches all assemblies for a `UiState` subclass matching the `stateKey` by name. **When using `ShowState<T>()`, the class name MUST match the `stateKey` in `UiConfig`** (e.g., `ShowState<MainMenuState>()` requires `stateKey = "MainMenuState"`).

### Context Object Pattern

Runtime data flows through the `context` parameter (type `object`). UI elements cast it to expected types in `Populate()`. Example:

```csharp
public class MyElement : UiElement {
    public override void Populate(object context = null) {
        if (context is MyData data) {
            // Use data
        }
    }
}
```

### Non-Additive Unloading

When `additive = false` (default), `UiManager` loads the new state's scenes BEFORE unloading previous states, then preserves new state's scenes during unload (`UnloadAllPreviousStates(keepScenes)`). This prevents Addressables from prematurely releasing shared assets.

## Namespaces

- **Runtime**: `UiFramework.Runtime`, `UiFramework.Runtime.Manager`
- **Core**: `UiFramework.Core` (contains `UiState`, `IUiElement`, `UiElement`)
- **Editor**: `UiFramework.Editor.*` (code generation, windows, config)

Generated code uses configurable namespaces from `UiEditorConfig`: `ElementNamespace`, `StateNamespace`.

## Editor Workflows

### Adding New UI Screens

1. Open **Window → UiFramework/UI Setup Manager** (`UiSetupEditorWindow`)
2. Use **Elements** tab to generate `UiElement` scripts (optionally with `Params` and `Reference` classes)
3. Create addressable scene(s) containing the `UiElement` MonoBehaviours
4. Use **States** tab to register scenes under a `stateKey` in `UiConfig`
5. Call `UiManager.ShowStateByKey("MyStateKey", context)` at runtime

### Code Generation

- **`UiElementGenerator`**: Creates `UiElement` subclass from template (`Assets/UiFramework/Editor/Templates/UiElementTemplate.txt`). Can generate companion `UiElementReference` and `UiPopulationParams` classes.
- **`UiStateGenerator`**: Creates `UiState` subclass from template (note: base constructor requires `(string name, List<AssetReference>)`).
- Templates use placeholders like `[UiElementName]`, `[UiElementNamespace]`, `[UiStateName]`, `[UiStateNamespace]`.

### Editor Config Paths

`UiEditorConfig` (`Assets/UiConfigs/UiEditorConfig.asset`) centralizes:

- Script output paths: `ElementsScriptPath`, `StatesPath`
- Scene paths: `ElementsScenePath`
- Namespaces: `ElementNamespace`, `StateNamespace`
- Runtime config: `RuntimeConfigOutputPath` (where `UiConfig.asset` is generated/updated)

## File Structure

```
Assets/UiFramework/
├── Core/
│   ├── Config/UiConfig.cs          # ScriptableObject: stateKey → scenes
│   └── UiCompoenets/
│       ├── UiState.cs              # Scene loader with Addressables
│       ├── IUiElement.cs           # Populate contract
│       ├── UiElement.cs            # Base MonoBehaviour
│       ├── UiElementReference.cs   # Base for typed references
│       └── UiPopulationParams.cs   # Base for population DTOs
├── Runtime/
│   ├── Manager/UiManager.cs        # State orchestration
│   └── UiConfig.asset              # Runtime configuration instance
└── Editor/
    ├── CodeGeneration/             # UiElementGenerator, UiStateGenerator
    ├── Templates/                  # .txt templates for code generation
    ├── Window/UiSetupEditorWindow.cs  # Main editor UI
    └── Config/UiEditorConfig.cs    # Editor paths/namespaces config
```

## Common Pitfalls

- **Mismatched names**: `ShowState<MyState>()` fails if `UiConfig` has `stateKey = "MyStateScreen"` instead of `"MyState"`. Use `ShowStateByKey()` or rename the class.
- **Context casting**: Always null-check and type-check `context` in `Populate()` — it's `object` type.
- **Addressables setup**: Scenes must be marked Addressable. If `Addressables.LoadSceneAsync()` fails silently, check the scene is in an addressables group.
- **Scene naming**: `loadedScenes` dictionary keys are `Scene.name` (runtime name), not asset path. Keep scene names unique.
- **Multiple UiManager instances**: Framework assumes singleton. Only call `Init()` once; subsequent calls clear the state stack.

## Testing & Debugging

- Enable debug logs in `UiManager`/`UiState` to trace scene loading and `Populate()` calls.
- Create a temporary test MonoBehaviour to call `UiManager.ShowStateByKey("TestState", testData)` in `Start()`.
- Inspect `Assets/UiConfigs/UiConfig.asset` in Inspector to verify `stateKey` entries and `AssetReference` assignments.
- Use `GetCurrentState()` to check active state at runtime.

## When Modifying

- **Adding fields to `UiConfig`**: Update `UiStateEntry` class; existing assets auto-upgrade via Unity serialization.
- **Changing `Populate()` signature**: Don't. It's the core contract. Create strongly-typed wrappers if needed.
- **New UI element patterns**: Create new templates in `Assets/UiFramework/Editor/Templates/` and update generators.
- **Custom state lifecycle**: Subclass `UiState` and override `Init()` or add cleanup logic in `UnloadUiState()`.

## Code Standards

This project follows strict C# coding standards defined in `.editorconfig`. **Always adhere to these rules when generating or modifying code:**

### Formatting & Style

- **Indentation**: 4 spaces (no tabs)
- **Line endings**: CRLF with final newline
- **Encoding**: UTF-8
- **Trailing whitespace**: Remove from all lines
- **Braces**: Always on new line for all constructs (Allman style)
- **Using directives**: Place **outside** namespace (at file top)

### Naming Conventions (Enforced as Errors/Warnings)

- **Interfaces**: `IPascalCase` (prefix with `I`) - **Warning**
- **Classes, structs, enums**: `PascalCase` - **Warning**
- **Methods, properties, events**: `PascalCase` - **Warning**
- **Private/internal fields**: `camelCase` (no underscore prefix) - **Error**
- **Parameters, local variables**: `camelCase`

### Language Features (Strict Rules)

- **`var` keyword**: **Never use** - always explicit types (`int x = 5;` not `var x = 5;`) - **Error**
- **Target-typed `new()`**: **Disallowed** - always use `new TypeName()` - **Error**
- **Braces**: **Always required** for all control blocks (if, for, while, etc.) - **Error**
- **Expression-bodied members**: **Disallowed** for methods, properties, indexers, accessors - **Error**
- **Single-line blocks/statements**: **Not preserved** - always expand to multiple lines

### Formatting Rules

- **New lines before**: open brace, else, catch, finally, object initializer members, anonymous type members, query clauses
- **Indentation**: case contents, switch labels, block contents (not braces)
- **Spaces**: after keywords in control flow, no spaces in parentheses or after casts
- **Import directives**: System directives first, no blank lines between groups

### Example

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace UiFramework.Core
{
    public class UiElementManager
    {
        private readonly List<IUiElement> elements = new List<IUiElement>();

        public int ElementCount
        {
            get
            {
                return elements.Count;
            }
        }

        public void AddElement(IUiElement element)
        {
            if (element == null)
            {
                Debug.LogError("Element cannot be null");
                return;
            }

            elements.Add(element);
        }

        public void PopulateAll(object context)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Populate(context);
            }
        }
    }
}
```

### Unity-Specific Conventions

- **SerializeField**: Use `[SerializeField] private TypeName fieldName;` for inspector-exposed fields
- **Namespaces**: Match folder structure where possible
- **MonoBehaviour lifecycle**: Follow Unity method order (Awake → OnEnable → Start → Update → OnDisable → OnDestroy)
- **Addressables**: Always null-check `AsyncOperationHandle` results before use
