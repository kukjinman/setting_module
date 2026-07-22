# 버전 및 태그 관리

이 저장소는 Shared Core Module과 Shared App Flow Module을 하나의 릴리스 버전으로 묶어 관리합니다. 두 패키지는 항상 같은 Git 태그에서 가져옵니다.

## 버전이 기록되는 위치

- `VERSION`: 저장소의 현재 릴리스 버전
- 루트 및 각 모듈의 `package.json`: Unity Package Manager에 표시되는 패키지 버전
- `Assets/SharedAppFlowModule/package.json`의 `com.sharedmodules.core-module`: 두 모듈 사이의 호환 버전
- 루트 `CHANGELOG.md`: 저장소 전체의 버전별 변경 이력과 릴리스 날짜
- 각 패키지의 `CHANGELOG.md`: Unity Package Manager에서 확인할 수 있는 모듈별 변경 이력
- Git annotated tag: 배포되는 소스 스냅샷 (`v1.0.0` 형식)

소비하는 게임 프로젝트에서는 다음 두 파일에 사용 버전이 기록됩니다.

- `Packages/manifest.json`: 설치할 Git 태그가 URL의 `#v1.0.0` 형태로 저장됨
- `Packages/packages-lock.json`: Unity가 실제로 해석한 패키지 소스와 리비전이 잠금 정보로 저장됨

두 파일을 게임 프로젝트의 Git에 함께 커밋해야 같은 버전을 재현할 수 있습니다.

## 게임 프로젝트에서 설치

두 모듈은 반드시 같은 태그를 사용합니다.

```json
{
  "dependencies": {
    "com.sharedmodules.core-module": "https://github.com/kukjinman/setting_module.git?path=/Assets/SharedCoreModule#v1.0.0",
    "com.sharedmodules.setting-module": "https://github.com/kukjinman/setting_module.git?path=/Assets/SharedAppFlowModule#v1.0.0"
  }
}
```

`main` 브랜치는 개발 확인용으로만 사용하고 출시 빌드에서는 태그를 사용합니다.

## 새 버전 릴리스

1. 작업 트리가 깨끗한지 확인합니다.
2. 버전을 일괄 변경합니다.

   ```bash
   python3 scripts/release.py set X.Y.Z
   ```

3. 루트 및 각 패키지의 `CHANGELOG.md`에 `## [X.Y.Z] - YYYY-MM-DD` 항목과 변경 사항을 작성합니다.
4. 버전 일관성을 검사합니다.

   ```bash
   python3 scripts/release.py check
   ```

5. 변경 사항을 검토하고 릴리스 커밋을 만듭니다.
6. 작업 트리가 깨끗한 상태에서 annotated tag를 생성합니다.

   ```bash
   python3 scripts/release.py tag
   ```

7. 커밋과 태그를 원격에 올립니다.

   ```bash
   git push origin main
   git push origin vX.Y.Z
   ```

이미 배포한 태그는 이동하거나 재사용하지 않습니다. 수정이 필요하면 patch 버전을 올려 새 태그를 만듭니다.
