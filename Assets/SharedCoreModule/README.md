# Shared Core Module

Small reusable base module for common game services.

First version includes only the pieces that are useful almost immediately:

- `SharedCoreRoot`: one persistent root object for shared managers.
- `SharedSaveManager`: small `PlayerPrefs` wrapper for simple values.
- `SharedAudioManager`: BGM/SFX playback and saved volume settings.
- `SharedAuthManager`: minimal saved-login state for guest login flow.
- `SharedHapticsManager`: saved vibration preference and mobile vibration entry point.

Use `Tools > Shared Modules > Core > Setup Core Root` to create the root in the current scene and save `Prefabs/Shared Core Root.prefab`.

Later candidates:

- Scene loading
- Localization
- Popup/toast manager
- Mobile back-button routing
- Haptics
- Ads and purchase wrappers

Those are intentionally not included yet because they depend on the game shape.
