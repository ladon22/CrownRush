# Crown Rush

체스 말을 스와이프로 이동시켜 진영을 방어하는 턴제 디펜스 로그라이트 모바일 게임입니다. Unity 개인 프로젝트로, 기획부터 프로그래밍까지 전부 혼자 작업했습니다.

- 플랫폼: Android
- 엔진/언어: Unity, C#
- 개발 형태: 개인 프로젝트 (1인)

## 플레이 영상
https://youtube.com/shorts/KT6_VUsvrk8

## 다운로드
https://play.google.com/store/apps/details?id=com.bearhand.projectchess

## 주요 구현
- **턴제 게임 루프** — `TurnManager`가 플레이어 턴/적 턴을 코루틴으로 전환하며 진행, 적 턴 시작 시 다음 턴 행동을 미리 계산해 `Intent`로 표시
- **체스 기물 기반 디펜스** — 각 기물의 이동 규칙을 살린 스와이프 입력으로 방어선을 구성하는 턴제 전투
- **적 AI** — `EnemyBase`를 기반으로 몬스터 유형별 행동을 하위 클래스에서 확장
- **입력 락 스택** — `InputController`가 연출/턴 전환 중 입력을 스택 기반으로 잠궈 중복 입력 및 오조작 방지
- **Firebase 연동** — 로그인, 랭킹 리더보드 (`FireBaseManager`)
- **광고 시스템** — AdMob 보상형 광고 연동 (`AdManager`)
- **튜토리얼 힌트 시스템** — 특정 조건에서 단계별로 힌트를 노출하는 `TutorialHintManager`

## 폴더 구조
| 폴더 | 내용 |
|---|---|
| `Controller/` | 입력 처리, 카메라 |
| `Manager/` | 턴 진행, 적 스폰, 아이템, 광고, Firebase 등 매니저 |
| `Enemy/` | 적 유형별 행동 |
| `Item/` | 아이템 데이터/로직 |
| `Data/`, `Scriptable/` | 게임 데이터 에셋 |
| `UI/` | 인게임/팝업 UI |
