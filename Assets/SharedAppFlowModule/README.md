# Shared App Flow Module

Reusable Unity UI flow for simple mobile app/game starts:

- Intro
- Login
- Home
- Gameplay entry screen
- Settings modal

Use `Tools > Shared Modules > App Flow > Setup Intro Login Home Demo` to create a demo canvas in the current scene and save `Prefabs/Shared App Flow Root.prefab`.

The generated UI uses a `960 x 540` reference resolution and regular `Image`, `Button`, `Text`, and `RectTransform` objects so it can be edited visually in Unity.

The intro panel uses `Art/Intro/intro-logo.png`. While the logo is visible, the flow restores the current platform login. An authenticated iOS Game Center player goes directly to Home even after reinstalling the app. If platform authentication fails, the flow opens Login and offers Guest plus the current platform login.

iOS Game Center authentication is implemented by `SharedAppleGameCenterLoginProvider` and `Plugins/iOS/SharedGameCenterBridge.mm`. Enable the Game Center capability for the iOS app target. The provider times out safely if iOS doesn't return a cancellation callback.

Guest login works immediately. Android uses `SharedGooglePlayGamesLoginProvider`, which remains a safe placeholder until the Google Play Games SDK is installed.

`SharedLoginProvider` is the SDK boundary. To enable real Google Play Games authentication, replace the placeholder component with an SDK-backed implementation and return `SharedLoginResult.Succeeded(playerId, displayName)` only after the platform login succeeds.

Home and Gameplay are separate panels in the generated demo. The Gameplay panel is an entry placeholder; a production game can replace that transition with scene loading without changing the authentication flow.

Runtime routing:

1. A saved Guest session goes directly from Intro to Home.
2. On iOS, Intro silently checks the current Game Center player. An already authenticated player goes to Home, including after reinstall. If iOS requires login UI, the flow goes to Login instead.
3. On Android, the same restore hook delegates to the Google Play Games provider.
4. The platform button on Login allows interactive sign-in; Guest remains available as a fallback.
5. Logout clears the local session and suppresses platform auto-login for that installation until the player explicitly signs in again. Reinstalling removes that local preference, so platform restoration can run again.
