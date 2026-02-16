## What's Changed

Merged `dev` → `main`


### Version Information
- **Version**: v1.0.39
- **Package**: com.dawaniyahgames.uiframework

### Installation


#### Via Package Manager UI
```
  https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.0.39
```

#### Via manifest.json
```json
{
  "dependencies": {
      "com.dawaniyahgames.uiframework": "https://github.com/dawaniyah-games/UiFramework.git?path=Packages/com.dawaniyahgames.uiframework#v1.0.39"
  }
}
```

### Animation & Transition Improvements (v1.0.37)
- Per-element animation system: each UI element can have its own show/hide preset (Fade, Scale, Slide, etc.)
- SlideUp/Down/Left/Right now match movement direction (e.g. SlideUp moves up from below)
- Shared UI elements (e.g. NavBar) animate only once, not on every state switch
- Overlapping transitions: new UI animates in while old UI animates out (no empty gap)
- Animation target auto-detection: animations run on the first child under a Canvas
