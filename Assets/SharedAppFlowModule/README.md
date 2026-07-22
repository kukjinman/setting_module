# Shared App Flow Module

Reusable Unity UI flow for simple mobile app/game starts:

- Intro
- Login
- Home
- Gameplay entry screen
- Settings modal

Use `Tools > Shared Modules > App Flow > Setup Intro Login Home Gameplay Demo` to create a demo canvas in the current scene and save `Prefabs/Shared App Flow Root.prefab`.

The generated UI uses a `960 x 540` reference resolution and regular `Image`, `Button`, `Text`, and `RectTransform` objects so it can be edited visually in Unity.

The intro panel uses `Art/Intro/intro-logo.png`. While the logo is visible, the flow restores the current platform login. An authenticated iOS Game Center player goes directly to Home even after reinstalling the app. If platform authentication fails, the flow opens Login and offers Guest plus the current platform login.

iOS Game Center authentication is implemented by `SharedAppleGameCenterLoginProvider` and `Plugins/iOS/SharedGameCenterBridge.mm`. Enable the Game Center capability for the iOS app target. The provider times out safely if iOS doesn't return a cancellation callback.

Guest login works immediately. Android uses `SharedGooglePlayGamesLoginProvider`, which remains a safe placeholder until the Google Play Games SDK is installed.

`SharedLoginProvider` is the SDK boundary. To enable real Google Play Games authentication, replace the placeholder component with an SDK-backed implementation and return `SharedLoginResult.Succeeded(playerId, displayName)` only after the platform login succeeds.

Home, Collection, and Gameplay are separate panels in the generated demo. The Gameplay and Collection panels are entry placeholders; a production game can replace those transitions with scene loading or game-specific UI without changing the authentication flow.

## Customizing the home screen

The generated Home panel follows a full-screen game-art layout with a profile card and a compact bottom navigation dock. The artwork is intentionally black by default. The dock contains `PLAY`, `OPTIONS`, `QUIT`, and `COLLECTION`. A separate compact language selector sits at the bottom-right and opens its available Locale list upward.

Each generated Home panel has a `SharedHomeView` component. A game can replace its artwork and profile copy at runtime without changing the shared generator:

```csharp
using SharedAppFlowModule;
using UnityEngine;

public sealed class GameHomeTheme : MonoBehaviour
{
    [SerializeField] private SharedHomeView homeView;
    [SerializeField] private Sprite homeArtwork;

    private void Start()
    {
        homeView.SetArtwork(homeArtwork);
        homeView.SetProfile("P1", "Player Name");
    }
}
```

Calling `ClearArtwork()` restores the black placeholder. When Home opens, the default profile name is refreshed from the saved shared login automatically. After a game calls `SetProfile`, that explicit value is kept; call `UseSavedProfile()` to return to the automatic login name.

## Localization

The setup tool creates English (`en`) and Korean (`ko`) Locales plus a `Shared UI` String Table under `Assets/SharedModules/Generated/Localization`. The selected Locale is saved and restored by `SharedLanguageSelector`. Shared menu labels update through `SharedLocalizedText` when the Locale changes.

Games can add more Locales through Unity Localization and provide the corresponding `Shared UI` entries. Game-specific strings should remain in separate String Table Collections owned by the game project.

## Reusing the button design in game scenes

`SharedButtonVisual` is a public uGUI component that can style an existing `Button` in any game, game-over, or menu scene. It supplies consistent outline, shadow, surface highlight, depth rim, pointer animation, UI selection, and keyboard/gamepad Submit feedback without a TextMeshPro dependency.

Add it to the same GameObject as an existing `Image` and `Button`, then choose the role in code:

```csharp
using SharedAppFlowModule;
using UnityEngine;

public sealed class GameButtonTheme : MonoBehaviour
{
    [SerializeField] private SharedButtonVisual playButton;
    [SerializeField] private SharedButtonVisual homeIconButton;

    private void Awake()
    {
        playButton.Configure(SharedButtonVariant.Primary);
        homeIconButton.Configure(SharedButtonVariant.Navigation);
    }
}
```

Games can override the module palette without modifying the package. The optional fourth color controls label/icon contrast:

```csharp
visual.Configure(
    SharedButtonVariant.Primary,
    new Color(0.12f, 0.22f, 0.38f, 1f), // base
    new Color(0.2f, 0.92f, 0.78f, 1f),  // accent
    Color.white);                        // label/icon
```

Available roles are `Primary`, `Secondary`, `Navigation`, `Back`, and `Destructive`. Calling `Configure` repeatedly updates the existing visual layers and effects rather than adding duplicates. Decorative images do not receive raycasts, so existing `Button.onClick` listeners remain unchanged.

Runtime routing:

1. A saved Guest session goes directly from Intro to Home.
2. On iOS, Intro silently checks the current Game Center player. An already authenticated player goes to Home, including after reinstall. If iOS requires login UI, the flow goes to Login instead.
3. On Android, the same restore hook delegates to the Google Play Games provider.
4. The platform button on Login allows interactive sign-in; Guest remains available as a fallback.
5. Logout clears the local session and suppresses platform auto-login for that installation until the player explicitly signs in again. Reinstalling removes that local preference, so platform restoration can run again.
