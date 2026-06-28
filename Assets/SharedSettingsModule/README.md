# Shared Settings Module

Small first-step setting window for Unity.

## Demo

1. In Unity, run `Tools > Shared Settings > Setup Ideal Mobile Demo`.
2. Press Play.
3. The setting window slides up from the bottom and stops at the center.
4. Press `Escape`, click the overlay, or click `X` to close it.

The demo uses `Assets/resourceBox/UI Bundle/UiElegant.png` and tries to assign `UiElegant_146` as the first panel skin.

Leave `Panel Size` as `(0, 0)` to draw the selected sprite at its original aspect ratio scaled up for UI. Set a custom size only when you intentionally want to stretch or fit it yourself.

The ideal mobile demo creates a visible Canvas with a `960 x 540` reference resolution for landscape phone games. The settings window opens on that canvas, so you can inspect and reuse the Canvas Scaler settings in other projects.

For crisp pixel art, keep the source texture on Point filtering and no compression. If the Unity Game view itself is zoomed below or above 1x, the editor preview can still look softened.

## Code

```csharp
using SharedSettingsModule;

SharedSettingsModal.Show(panelSprite);
```

This version intentionally contains only the window shell. Sliders, buttons, audio, language, credits, and theme switching can be added one small step at a time.
