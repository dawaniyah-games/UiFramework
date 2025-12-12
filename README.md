# UI Framework

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.md)

A flexible Unity UI Framework using **Addressables-based scene composition** with LIFO state stack management. Build UI screens as additive scenes and populate them with runtime data through a simple contract-based system.

## Features

- 🎯 **Addressables Integration** - Load UI screens as additive scenes for efficient memory management
- 📚 **LIFO State Stack** - Push/pop UI states with automatic scene lifecycle management
- 🔄 **Context-Based Population** - Pass runtime data to UI elements via `IUiElement.Populate()`
- 🛠️ **Editor Tools** - Code generation and visual setup manager for rapid development
- 🎨 **Template System** - Generate UI elements and states from customizable templates
- ⚡ **Type-Safe API** - Generic `ShowState<T>()` with reflection-based type resolution

## Installation

### Via Package Manager (Git URL)

1. Open Unity Package Manager (Window → Package Manager)
2. Click the + button and select Add package from git URL...
3. Enter: `https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework`
4. Click Add

### Via manifest.json

Add this to your Packages/manifest.json:

```json
{
  "dependencies": {
        "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework"
  }
}
```

## Quick Start

### 1. Setup UI Manager

```csharp
using UiFramework.Runtime.Manager;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private UiManager uiManager;

    private async void Start()
    {
        await uiManager.Init();
        await UiManager.ShowStateByKey("MainMenu");
    }
}
```

### 2. Create UI Element

```csharp
using UiFramework.Core;
using UnityEngine;

public class MainMenuElement : UiElement
{
    public override void Populate(object context = null)
    {
        if (context is MenuData data)
        {
            // Use data to configure UI
        }
    }
}
```

### 3. Configure in Editor

1. Open `Window → UiFramework/UI Setup Manager`
2. Use **Elements** tab to generate UI element scripts
3. Create addressable scene with your UI elements
4. Use **States** tab to register scenes under a state key
5. Call `UiManager.ShowStateByKey("YourStateKey", context)` at runtime

## Core Architecture

```
UiManager (Singleton)
├── LIFO State Stack
│   └── UiState (Scene Loader)
│       ├── Addressables.LoadSceneAsync()
│       └── IUiElement.Populate(context)
│
├── UiConfig (ScriptableObject)
│   └── stateKey → List<AssetReference>
│
└── Type Resolution
    └── Reflection: Class Name → stateKey
```

## Key Components

| Component    | Path                              | Purpose                                    |
| ------------ | --------------------------------- | ------------------------------------------ |
| `UiManager`  | `Runtime/Manager/UiManager.cs`    | Orchestrates state stack and scene loading |
| `UiState`    | `Core/UiCompoenets/UiState.cs`    | Loads scenes and discovers UI elements     |
| `UiConfig`   | `Core/Config/UiConfig.cs`         | Maps state keys to scene references        |
| `IUiElement` | `Core/UiCompoenets/IUiElement.cs` | Contract for UI data population            |

## Documentation

- [Design Documentation](DESIGN.md) - Detailed architecture and system design
- [Copilot Instructions](.github/copilot-instructions.md) - Guide for AI coding assistants

## Requirements

- Unity 2021.3 or later
- Addressables package (1.21.0+)

## Code Standards

This project enforces strict C# coding standards via `.editorconfig`:

- ✅ No `var` keyword - always explicit types
- ✅ No target-typed `new()` - always specify type
- ✅ Braces required for all control blocks
- ✅ No expression-bodied members
- ✅ `camelCase` for private fields (no underscore prefix)
- ✅ Using directives outside namespace

## License

MIT License - see [LICENSE.md](LICENSE.md) file for details

## Contributing

Contributions are welcome! Please ensure code follows the `.editorconfig` standards.

## Support

- 📝 [Issues](https://github.com/dawaniyah-games/UiFramework/issues)
- 💬 [Discussions](https://github.com/dawaniyah-games/UiFramework/discussions)
