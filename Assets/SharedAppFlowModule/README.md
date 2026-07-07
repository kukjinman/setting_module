# Shared App Flow Module

Reusable Unity UI flow for simple mobile app/game starts:

- Intro
- Login
- Home
- Settings modal

Use `Tools > Shared Modules > App Flow > Setup Intro Login Home Demo` to create a demo canvas in the current scene and save `Prefabs/Shared App Flow Root.prefab`.

The generated UI uses a `960 x 540` reference resolution and regular `Image`, `Button`, `Text`, and `RectTransform` objects so it can be edited visually in Unity.

The intro panel uses `Art/Intro/intro-logo.png`. On play it shows the logo briefly, then routes to Home if a saved login exists or Login if not. Guest login stores a simple saved-login flag through `SharedAuthManager` when the core module exists, or directly through `PlayerPrefs` as a fallback.
