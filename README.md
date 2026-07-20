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

## 새 게임 프로젝트에 적용하기

Git URL을 등록하는 작업은 패키지의 코드와 에디터 도구를 설치하는 단계입니다. 설치만으로 씬에 UI나 Core 오브젝트가 생성되지는 않습니다. 패키지를 설치한 다음 시작 씬에서 Setup 메뉴를 한 번 실행해야 합니다.

### 1. Unity 버전 확인

이 패키지는 Unity 6(`6000.0` 이상)을 대상으로 합니다.

### 2. 패키지 설치

Unity에서 **Window > Package Manager**를 열고 **+ > Add package from git URL**을 선택한 후 다음 주소를 입력합니다.

개발 중 최신 `main` 브랜치를 사용하려면:

```text
https://github.com/kukjinman/setting_module.git#main
```

릴리스 게임에서는 변경되지 않는 태그 버전을 권장합니다.

```text
https://github.com/kukjinman/setting_module.git#v1.0.0
```

### 3. 씬 생성

게임 프로젝트에 다음 두 씬을 만듭니다.

```text
Assets/Scenes/StartScene.unity
Assets/Scenes/GameScene.unity
```

- `StartScene`: Intro, Login, Home, Settings와 공용 Core를 담당합니다.
- `GameScene`: 실제 게임 플레이를 담당합니다.

### 4. StartScene에 Core 생성

`StartScene`을 연 상태에서 다음 메뉴를 실행합니다.

```text
Tools > Shared Modules > Core > Setup Core Root
```

현재 씬에 `Shared Core Root`가 생성됩니다. 이 오브젝트는 저장, 로그인 상태, BGM/SFX 기능을 제공하며 씬 전환 후에도 `DontDestroyOnLoad`로 유지됩니다. 따라서 `GameScene`에 Core를 다시 배치하지 않습니다.

게임 프로젝트 전용 프리팹도 다음 경로에 자동 생성됩니다.

```text
Assets/SharedModules/Generated/Prefabs/Shared Core Root.prefab
```

### 5. StartScene에 App Flow 생성

같은 `StartScene`에서 다음 메뉴를 실행합니다.

```text
Tools > Shared Modules > App Flow > Setup Intro Login Home Gameplay Demo
```

Intro, Login, Home, Gameplay 자리 표시자, Settings UI와 필요한 `EventSystem`이 생성됩니다. App Flow 프리팹은 다음 경로에 저장됩니다.

```text
Assets/SharedModules/Generated/Prefabs/Shared App Flow Root.prefab
```

Setup은 씬 오브젝트 생성과 프리팹 저장을 함께 처리합니다. Setup을 실행한 후 생성된 프리팹을 씬에 다시 드래그할 필요는 없습니다. 씬만 저장하면 됩니다.

완성된 시작 씬은 대략 다음과 같습니다.

```text
StartScene
|- Shared Core Root
|- Shared App Flow Root
`- EventSystem
```

Setup 메뉴는 게임을 실행할 때마다 사용하는 기능이 아닙니다. 최초 구성 또는 프리팹을 새로 생성해야 할 때만 실행합니다. 이미 수정한 Root가 있는 상태에서 Setup을 다시 실행하면 기존 오브젝트 교체 여부를 묻기 때문에 커스텀 UI를 덮어쓰지 않도록 주의합니다.

### 6. PLAY 버튼을 GameScene에 연결

기본 `PLAY` 버튼은 같은 StartScene 안의 Gameplay 자리 표시자 패널을 표시합니다. 실제 게임에서는 이 이벤트를 제거하고 `GameScene`을 로드하도록 연결합니다.

게임 프로젝트의 `Assets/Scripts/GameSceneLoader.cs` 등에 다음 컴포넌트를 추가할 수 있습니다.

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameSceneLoader : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadHome()
    {
        SceneManager.LoadScene("StartScene");
    }
}
```

`StartScene`의 빈 GameObject에 이 컴포넌트를 추가한 후, Home의 `PLAY` 버튼 `On Click()`에서 기존 화면 전환 이벤트 대신 `GameSceneLoader.LoadGame()`을 연결합니다. 게임 종료 또는 나가기 버튼에는 `LoadHome()`을 연결합니다.

### 7. Build Profiles 등록

**File > Build Profiles**의 Scene List에 다음 순서로 씬을 등록합니다.

```text
0: StartScene
1: GameScene
```

`Shared Core Root`가 먼저 생성되어야 하므로 게임은 `StartScene`부터 시작해야 합니다. Unity Editor에서 `GameScene`만 직접 실행하면 `SharedCoreRoot.Instance`가 존재하지 않을 수 있습니다.

### 8. 실행 흐름 확인

로그인 기록이 없는 첫 실행의 기본 흐름은 다음과 같습니다.

```text
Intro -> Login -> Guest 또는 플랫폼 로그인 -> Home -> PLAY -> GameScene
```

저장된 Guest 또는 플랫폼 로그인이 있으면 Intro에서 로그인 복원을 확인한 후 Home으로 바로 이동할 수 있습니다. 로그인 화면을 다시 확인하려면 Home의 `LOGOUT` 버튼을 사용합니다.

Home 왼쪽 상단의 플랫폼 아이콘 버튼은 현재 플랫폼 로그인을 실행합니다. iOS에서는 `GC`로 표시되어 Apple Game Center를 호출하고, Android에서는 `PG`로 표시되어 Google Play Games 로그인 공급자를 호출합니다. Android 공급자는 SDK가 연결되기 전까지 placeholder로 동작합니다.

이 버튼은 Home 화면에 들어왔을 때 표시되고 5초 후 자동으로 사라집니다. Home 화면의 아무 곳이나 터치하거나 클릭하면 다시 나타나며, 마지막 입력으로부터 5초 후 다시 사라집니다. 시간 정지 상태에서도 동작하도록 unscaled time을 사용합니다.

패키지를 업데이트하기 전에 이미 App Flow 프리팹을 생성했다면 새 버튼이 기존 로컬 프리팹에 자동 추가되지 않습니다. 커스텀 UI 수정 사항을 보관한 후 Setup을 다시 실행하여 프리팹을 재생성하거나, 기존 Home Panel에 `SharedAppFlowButton`을 직접 추가하고 Action을 `LoginWithPlatform`으로 설정합니다.

### Setup, 패키지 프리팹, 생성된 프리팹의 관계

- Git 패키지: 공용 Runtime 코드, Editor Setup 도구와 원본 에셋을 제공합니다.
- Setup 메뉴: 현재 씬에 사용할 Root를 만들고 게임 프로젝트용 프리팹을 생성합니다.
- `Assets/SharedModules/Generated/Prefabs`: 각 게임이 직접 수정하고 사용하는 로컬 프리팹입니다.
- 패키지를 업데이트하면 Runtime 코드 변경은 반영되지만 이미 생성한 로컬 프리팹은 자동 재생성되지 않습니다.

권장 프로젝트 구조는 다음과 같습니다.

```text
Assets/
|- Scenes/
|  |- StartScene.unity
|  `- GameScene.unity
|- Scripts/
|  `- GameSceneLoader.cs
`- SharedModules/Generated/Prefabs/
   |- Shared Core Root.prefab
   `- Shared App Flow Root.prefab
```

## Release

Commit `package.json` and package assets, then tag releases using semantic versions:

```text
v1.0.0
v1.0.1
v1.1.0
```

Projects referencing a tag remain on that version until their manifest entry is changed.
