# Shared Setting Module

Reusable Unity package containing shared core services and an intro/login/home/settings UI flow.

## Install from Git

In Unity, open **Window > Package Manager**, press **+**, select **Add package from git URL**, and enter:

```text
https://github.com/kukjinman/setting_module.git#v1.0.0
```

Alternatively, add the dependency to the game's `Packages/manifest.json`:

```json
"com.sharedmodules.setting-module": "https://github.com/kukjinman/setting_module.git#v1.0.0"
```

For local development, use:

```json
"com.sharedmodules.setting-module": "file:../../setting_module"
```

## Create scene objects

After installation, use:

- **Tools > Shared Modules > Core > Setup Core Root**
- **Tools > Shared Modules > App Flow > Setup Intro Login Home Gameplay Demo**

Generated prefabs are saved under `Assets/SharedModules/Generated/Prefabs`, keeping installed package files unchanged.

The App Flow generator uses its bundled intro logo. Its optional pixel panel and font references fall back to standard UI when the third-party art package is not installed.

## Release

Commit `package.json` and package assets, then tag releases using semantic versions:

```text
v1.0.0
v1.0.1
v1.1.0
```

Projects referencing a tag remain on that version until their manifest entry is changed.
