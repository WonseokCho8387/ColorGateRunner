# itch.io Playtest v0.1 구현 명세

상태: 2026-10-06 로컬 구현 후보 검증 완료, 전체 v0.1 및 게시 준비는 미완료. 감사 보고서에 따라 진행하라는 요청과 동의 기반 UGS Analytics 승인에 따른 구현 범위다. 최종 검증 수치와 산출물은 `CURRENT_STATUS.md`의 Uncommitted candidate 항목이 소유한다. 게시 승인은 포함하지 않는다.

## 목표와 범위

- 기존 Boot → Frontend → Campaign Scene과 StageSession을 재사용한다.
- 별도의 플레이테스트 빌드에서만 계정 선택·경제·보유 아이템 화면을 우회한다.
- 경로는 `stage-01 → stage-02 → stage-03 → stage-06 → stage-09 → stage-21 → stage-18 → stage-24`다. 기존 Stage 정의, 시드, 속도, 판정과 제공 Shield를 유지한다.
- 재시도는 한 번의 선택으로 기존 Countdown부터 재시작한다. 비용·대기시간·구매·광고는 없다.
- 플레이테스트 진행은 세션 메모리에만 보관한다. 기존 Product 파일과 Campaign PlayerPrefs를 읽거나 수정하지 않으며 새로고침 시 테스트 경로가 처음부터 시작됨을 안내한다.
- Stage 1 안내는 입력과 Gate 통과 전 색상 일치를 연결한다. 안내만 표시한 것을 완료로 세지 않고, 유효 입력·직접 일치 성공·Stage 1 클리어를 모두 관찰한다. 첫 Gate 생성이나 타이밍은 변경하지 않는다.
- 실패 시 실제 판정 당시 플레이어 색과 Gate 목표 정보를 표시한다. 완료 후 자발적인 다음 단계 선택과 8단계의 명시적 종료를 제공한다.
- 데스크톱 브라우저부터 검증한다. 실물 모바일 검증 없이 모바일 지원을 주장하지 않는다.

## 소유권과 변경 파일

- Product의 `PlaytestSession`: 메모리 내 경로, 시도, 완료 및 온보딩 관찰 상태. Unity/UGS 비의존.
- `AppRoot`: 플레이테스트 세션 및 임시 Product 저장 구성. 기존 일반 빌드의 서비스 그래프 유지.
- `FrontendSceneController`: 플레이테스트 시작/재개/종료 안내. 직렬화된 Campaign 목적지 재사용.
- `StageSceneController`: 기존 시작·Retry·Clear·입력·판정 경계에서 플레이테스트 정책 적용. 별도 이동/판정 시스템을 만들지 않음.
- 기존 Frontend/Campaign Builder: 시작 화면과 비차단 안내의 명시적 참조 생성. 생성 Scene 수작업 수정 금지.
- 별도 WebGL 빌드 진입점: 일반 개발 빌드 계약 유지, 플레이테스트 define을 해당 Player 빌드에만 적용. 출처·설정·파일 해시·ZIP 제한 결과 기록.
- EditMode/PlayMode 테스트: 경로 순서, 완료/중복 처리, 무제한 반복, 저장 격리, 버튼 입력 누출, 정상 Campaign 회귀.

## Analytics와 개인정보

아래는 승인된 후속 연결 계약이며 현재 완료 기능이 아니다. 이번 후보에는 SDK, 동의 UI, 원격 이벤트 전송이 없다.

사용자는 동의 기반 UGS Analytics 및 개인정보 안내 준비를 승인했다. 현재 Cloud Project ID와 조직 연결은 비어 있고, 개인정보 문의/안내 URL도 아직 제공되지 않았다.

- 프로젝트 연결과 안내 정보가 없으면 실제 수집을 켜거나 작동한다고 표시하지 않는다.
- 허용/거절 선택은 동등하게 제공하고, 거절해도 동일하게 플레이한다.
- 선택 SDK 버전에 맞는 Unity Consent API를 사용한다. 동의 전 또는 철회 후 게임플레이 이벤트를 전송하지 않는다.
- SDK 자체 식별자/메타데이터가 있으므로 완전 익명이라고 설명하지 않는다. Profile/Guest ID·이메일·직접 수집 IP·자체 기기 ID·자유입력은 이벤트에 넣지 않는다.
- `game_ready`, `first_input`, `tutorial_start`, `tutorial_complete`, `stage_start`, `stage_fail`, `stage_complete`, `stage_retry`, `playtest_complete`를 소규모 관찰 경계로 삼는다. 정확한 Event Manager 스키마는 연결 전에 확정한다.
- 브라우저 종료 이벤트는 완료·이탈 통계의 필수 전송 경로로 사용하지 않는다. 마지막 수신 이벤트와 활성 경과시간으로 미완료 세션을 추정한다.
- 동의한 사용자만 측정하므로 모든 방문/로드 수를 대표한다고 주장하지 않는다.
- 철회/삭제 동작 및 실제 수신은 UGS 연결 후 별도 검증해야 한다.

## 검증과 완료 조건

- Tier 2 / Q2: 변경한 Builder 2회, 최종 EditMode·PlayMode 전체, 저장/Package/ProjectSettings 비교, 독립 checker, 그래픽/브라우저 근거.
- Stage 정의와 Core는 변경하지 않는다. 실행 경계가 달라지는 부분은 기존 Campaign 시뮬레이션 비교로 확인하며 Step 10은 영향을 평가한다.
- 재생성 파일의 공백 및 파일 ID 변경을 게임플레이 변경과 혼동하지 않는다.
- WebGL 로컬 HTTP 실행은 itch.io iframe 승인과 구분한다. 최종 배포 ZIP은 루트 index.html, 상대 경로, 파일 수/크기 제한을 다시 확인한다.
- 실제 itch.io 실행, 초심자 관찰, 개인정보 안내의 게시 정보, UGS 이벤트 수신이 확인되기 전에는 외부 초대 준비 완료로 보고하지 않는다.

## 현재 실행 근거

- HEAD `e48448d81a28f2f4338a4f2db79e6f74ca90391a`, primary 프로젝트, Unity `6000.5.1f1`.
- 2026-09-30 UTC 기존 개발 빌드 도구 성공: 보고 크기 `188,523,616 bytes`, 로그 `Artifacts/Validation/ItchPreparation-20260930/WebGL-build.log`.
- 빌드로 재생성된 19개 추적 파일을 위 근거 폴더에 복사 보관한 후 작업 전 버전으로 복원했다. 기존 WebGL define 차이는 유지했다.
- Product 저장/백업은 빌드 전후 SHA-256과 수정 시각이 동일했다. 패키지 내용도 동일했다.
- 2026-10-04 KST 로컬 HTTP의 Codex 내장 브라우저에서 최초 진입, Home, Stage 1 Countdown, 미입력 실패를 확인했다. 후처리 FSR shader 경고가 있어 렌더링 검증이 남아 있다. Stage 1 클리어·전체 경로·itch.io·실물 모바일은 아직 미검증이다.
- 후속 플레이테스트 후보는 일반 Home/PreRun을 우회한다. 실행 순서 오류로 첫 자동 시작이 다른 컴포넌트의 Awake보다 빨랐던 문제는 Start로 이관해 수정했다.
- 독립 검토 후 결과 버튼의 실제 활성 상태와 완료 메뉴까지 자동 테스트를 강화했다. 이는 브라우저에서 전체 경로를 사람이 완주한 근거는 아니다.
- URP가 재질을 재검증해도 기존 렌더 순서를 유지하도록 additive 재질의 queue offset을 명시했다. 변경 전후 패킹된 Player의 실제 재질 순서는 동일했으며 디스크/Builder 일관성을 보완한 것이다.
- 최신 브라우저/자동 테스트/패키징 결과와 남은 검증은 `CURRENT_STATUS.md` 및 `TEST_HISTORY.md`를 따른다. 초기 감사의 NOT VERIFIED를 과거 시점의 기록으로 보존한다.

## 제외

게임 밸런스 변경, 새 기믹, 새 튜토리얼 Scene, 광고/IAP/계정/통화 확장, 라이브 운영, 외부 게시·초대, 자동 커밋은 이번 요청에 포함하지 않는다.
