# Shared Setting Module

Reusable Unity package containing shared core services and an intro/login/home/settings UI flow.

## Install from Git

In Unity, open **Window > Package Manager**, press **+**, select **Add package from git URL**, and enter:

```text
https://github.com/kukjinman/setting_module.git?path=/Assets/SharedCoreModule#v1.0.0
https://github.com/kukjinman/setting_module.git?path=/Assets/SharedAppFlowModule#v1.0.0
```

Alternatively, add the dependency to the game's `Packages/manifest.json`:

```json
"com.sharedmodules.core-module": "https://github.com/kukjinman/setting_module.git?path=/Assets/SharedCoreModule#v1.0.0",
"com.sharedmodules.setting-module": "https://github.com/kukjinman/setting_module.git?path=/Assets/SharedAppFlowModule#v1.0.0"
```

The current repository version is stored in [`VERSION`](VERSION). Release changes are recorded in [`CHANGELOG.md`](CHANGELOG.md), and the release process is documented in [`VERSIONING.md`](VERSIONING.md). Production games should use an immutable tag; use `main` only for development verification.

For local development, use:

```json
"com.sharedmodules.core-module": "file:../../setting_module/Assets/SharedCoreModule",
"com.sharedmodules.setting-module": "file:../../setting_module/Assets/SharedAppFlowModule"
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
https://github.com/kukjinman/setting_module.git?path=/Assets/SharedCoreModule#main
https://github.com/kukjinman/setting_module.git?path=/Assets/SharedAppFlowModule#main
```

릴리스 게임에서는 변경되지 않는 태그 버전을 권장합니다.

```text
https://github.com/kukjinman/setting_module.git?path=/Assets/SharedCoreModule#v1.0.0
https://github.com/kukjinman/setting_module.git?path=/Assets/SharedAppFlowModule#v1.0.0
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

Intro, Login, Home, Gameplay/Collection 자리 표시자, Settings UI와 필요한 `EventSystem`이 생성됩니다. App Flow 프리팹은 다음 경로에 저장됩니다.

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

저장된 Guest 또는 플랫폼 로그인이 있으면 Intro에서 로그인 복원을 확인한 후 Home으로 바로 이동할 수 있습니다. 로그아웃은 `OPTIONS > LOGOUT`에서 실행할 수 있습니다.

Home은 전체 화면 게임 이미지와 하단 메뉴 구조로 생성됩니다. 게임 이미지는 기본적으로 검정색이며, 왼쪽 아래에는 로그인 이름을 반영하는 Profile 카드가, 그 옆에는 `PLAY`, `OPTIONS`, `QUIT`, `COLLECTION` 버튼이 모여 있습니다. 현재 언어를 표시하는 언어 버튼은 Balatro 스타일로 오른쪽 아래에 따로 배치됩니다.

Home의 `SharedHomeView`를 이용하면 게임별 이미지를 런타임에 교체할 수 있습니다.

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

`ClearArtwork()`를 호출하면 다시 검정 자리 표시자로 돌아갑니다. `COLLECTION`은 게임별 컬렉션을 붙일 수 있는 자리 표시자 화면으로 이동하고, `QUIT`은 빌드된 앱을 종료합니다.

언어 버튼을 누르면 지원 Locale 목록이 버튼 위에 열립니다. 기본으로 English(`en`)와 한국어(`ko`) Locale 및 `Shared UI` String Table이 생성되고, 선택한 Locale 코드는 `PlayerPrefs`에 저장됩니다. 각 게임은 Unity Localization 설정에 Locale을 추가하고 같은 `Shared UI` 키의 번역을 채우거나 별도의 게임 전용 String Table을 추가할 수 있습니다.

Home의 공용 메뉴 버튼은 `OPTIONS`로 표시됩니다. Balatro 스타일의 간단한 중앙 메뉴가 열리며 다음 화면으로 이동할 수 있습니다.

```text
OPTIONS
|- SETTINGS
|  |- MASTER VOLUME
|  |- MUSIC VOLUME
|  |- SOUND EFFECTS
|  `- VIBRATION
|- STATS
|- CREDITS
|- LOGOUT
`- BACK
```

Settings의 볼륨 슬라이더는 `SharedAudioManager`에 즉시 반영되고 `PlayerPrefs`에 저장됩니다. Settings, Stats, Credits 안의 `BACK`은 Options 목록으로 돌아가고 Options 목록의 `BACK`은 모달을 닫습니다. Stats의 초기 값과 Credits의 스튜디오 문구는 게임 프로젝트에서 실제 데이터에 맞게 수정해야 합니다.

음악과 효과음은 게임 프로젝트의 `AudioClip`을 `SharedCoreRoot.Instance.Audio`로 재생합니다. 예를 들어 다음 컴포넌트의 필드에 Project 창의 오디오 파일을 연결할 수 있습니다.

```csharp
using SharedCoreModule;
using UnityEngine;

public sealed class GameAudioExample : MonoBehaviour
{
    [SerializeField] private AudioClip homeMusic;
    [SerializeField] private AudioClip buttonSound;

    private void Start()
    {
        SharedCoreRoot.Instance.Audio.PlayBgm(homeMusic);
    }

    public void PlayButtonSound()
    {
        SharedCoreRoot.Instance.Audio.PlaySfx(buttonSound);
    }
}
```

`PlayBgm`은 BGM Source에서 반복 재생되며 `MUSIC VOLUME`의 영향을 받습니다. `PlaySfx`는 SFX Source에서 한 번 재생되며 `SOUND EFFECTS`의 영향을 받습니다. 두 종류 모두 `MASTER VOLUME`이 함께 적용됩니다.

게임 효과의 의미와 강도에 맞는 네이티브 햅틱을 요청할 수 있습니다.

```csharp
using SharedCoreModule;

SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Selection); // 메뉴 또는 카드 선택
SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Light);     // 가벼운 터치
SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Medium);    // 카드 확정
SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Heavy);     // 강한 충돌 또는 큰 점수
SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Success);   // 승리 또는 보상
SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Warning);   // 경고
SharedCoreRoot.Instance.Haptics.Play(SharedHapticType.Error);     // 실패
```

기존 `Vibrate()`는 호환성을 위해 유지되며 `Heavy` 햅틱을 호출합니다. Settings의 `VIBRATION`이 OFF이면 모든 햅틱 요청이 무시됩니다. 설정값은 `PlayerPrefs`에 저장되며 다음 실행 때 복원됩니다.

iOS는 `UISelectionFeedbackGenerator`, `UIImpactFeedbackGenerator`, `UINotificationFeedbackGenerator`를 타입에 맞게 사용합니다. Android는 시스템 `performHapticFeedback` 상수를 사용하며 Android 11(API 30) 이상에서는 Success와 Error를 각각 `CONFIRM`, `REJECT`로 구분합니다. 구형 Android에서는 지원되는 Click 또는 Long Press 피드백으로 대체됩니다. Android의 시스템 햅틱 설정과 기기별 하드웨어 특성을 따르며 Unity Editor에서는 아무 동작도 하지 않습니다.

패키지를 업데이트하기 전에 이미 App Flow 프리팹을 생성했다면 새 홈 레이아웃이 기존 로컬 프리팹에 자동 반영되지 않습니다. 커스텀 UI 수정 사항을 보관한 후 Setup을 다시 실행하여 프리팹을 재생성합니다.

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
