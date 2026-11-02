# itch.io 플레이테스트 빌드 v0.1 — 출시 준비도 감사

감사 완료: 2026-09-27 KST. 소스 및 산출물 점검 기간: 9월 25–27일. 대상 독자: GPT / Game Director.
진행 단계: **Play → Analyze에서 종료.** 본 문서는 점검 및 격차 분석이며, 승인된 설계·구현 명세·게시 허가가 아닙니다.

후속 기록: 이후 사용자가 구현 진행과 동의 기반 UGS 방향을 승인했습니다. 2026-10-06 로컬 구현 후보의 실제 완료·미완료 상태는 [CURRENT_STATUS.md](CURRENT_STATUS.md)의 Uncommitted candidate 항목과 [구현 명세](ITCH_IO_IMPLEMENTATION_SPEC.md)를 참고하세요. 아래 상태표는 감사 당시의 관찰을 보존합니다.

| 영역 | 상태 | 근거 | 필요한 조치 |
|---|---|---|---|
| 기존 게임플레이 기반 | READY | Catalog revision 16, 제작된 스테이지 26개, 테스트 XML 및 일치하는 시뮬레이션 산출물 | 기존 Core 규칙, 명시적 시드, 판정, 고정 풀 재사용 |
| 현재 외부 테스트용 WebGL 빌드 | BLOCKER | 로컬 산출물은 있으나 revision과 빌드 출처가 연결되지 않고 호스팅 브라우저 실행도 없음 | 출처를 추적할 수 있는 후보 빌드를 만들고 초대 전에 호스팅 스모크 테스트 통과 |
| 첫 플레이 온보딩 | NEEDS WORK | Account Choice → Home → PreRun 흐름, Campaign 입력/색상 맞추기 안내 없음 | Stage 1에 최소한의 게임 내 안내를 넣고 설명 없이 테스트 |
| 실패 후 제한 없는 복구 | BLOCKER | Heart 승인 실패로 시작이 거부될 수 있고, 일반 Retry는 행동 네 번 필요 | 플레이테스트 전용 비상업적 시도/복구 정책 승인 |
| 짧은 콘텐츠 경로 | NEEDS WORK | 필요한 기믹은 Stage ID 1–24에 흩어져 구현됨 | 프로덕션 Stage 정의를 바꾸지 않는 8개 스테이지 경로 승인 |
| 포인터 입력 및 세로 화면 레이아웃 | READY | `GameplayTapSurface`, Input System UI, 기존 9:16 HTML 템플릿 | 구현을 재사용하되 브라우저/기기 실행은 아직 NOT VERIFIED |
| WebGL 저장 | NEEDS WORK | 복구 가능한 파일 교체는 있으나 브라우저 저장 내구성 미검증 | IndexedDB 동기화, 저장 차단, 새로고침, iframe 동작 검증 |
| ZIP 구조 제한 | READY | 관찰한 폴더: 7개 파일, 188,152,604 bytes; 가장 큰 파일 130,783,243 bytes | 최종 ZIP 재검사, 디버그 전용 산출물 제외 |
| 전체 화면/모바일 브라우저 승인 | NEEDS WORK | 반응형 CSS는 있으나 호스팅/기기 실행 근거 없음 | 8절 테스트 매트릭스 실행, 테스트하지 않은 플랫폼은 지원한다고 홍보하지 않음 |
| 최소 게임플레이 데이터 수집 | BLOCKER | UGS Analytics 미설치, 개발용 CSV 비활성 및 업로드 안 됨 | 동의 기반 최소 수집 경로 승인 및 실제 수신 입증 |
| 개인정보 및 동의 | NEEDS WORK | 개인정보 URL이 비어 있고 분석 동의 UI 없음 | SDK 메타데이터 검토, 수집 전 안내·거절·철회·삭제 수단 제공 |
| 기존 회귀 검증 근거 | READY | EditMode 480개 통과, PlayMode 265개 통과와 의도적 무시 1개, 산출물 해시 8개 일치 | 기준선을 유지하고 구현 후 해당 검증 수행 |
| itch.io 페이지 자료와 문구 | NEEDS WORK | 타이틀 및 내부 캡처는 있으나 README에는 타이틀만 있음 | 간결하고 사실에 맞는 테스트 페이지와 적절한 스크린샷 준비 |
| 오디오 다듬기 | OPTIONAL | 출시용 BGM/SFX 미구현 | 피드백 이해에 필요할 때만 추가; 무음은 첫 테스트의 차단 사유 아님 |
| 광고, IAP, 계정, 경제 확장, 라이브 운영 | DEFER | 기존 공급자 기능 미제공; 사용자 요청에서 상업 범위 제외 | 수익화 OFF 유지, 해당 시스템 추가 금지 |

상태 정의: `READY`는 해당 제한된 요구사항에 충분한 근거가 있음을 뜻합니다. `NEEDS WORK`는 미완료이거나 승인·수용이 필요함을 뜻합니다. `BLOCKER`는 해결 전에는 해당 플레이테스트를 의미 있게 진행할 수 없음을 뜻합니다. `OPTIONAL`은 유용하지만 불필요함, `DEFER`는 v0.1 범위에서 제외함을 뜻합니다. **NOT VERIFIED**는 근거 한계 표시이며 여섯 번째 상태가 아닙니다. 아래 출시 차단 항목은 해결 작업을 시작하는 것 자체를 막는다는 뜻은 아닙니다.

## 1. 요약

**itch.io 소규모 플레이테스트를 위한 구현은 진행할 수 있습니다. 현재 제품 흐름은 요청된 플레이테스트 형태가 아닙니다.** 제품 아키텍처를 다시 만들거나 새 기믹을 추가할 필요 없이 기존 게임을 재사용할 수 있습니다. 주요 격차는 제한 없는 재시도, 색상 맞추기 규칙 교육, 비연속 Stage로 구성한 짧은 경로, 호스팅된 WebGL 검증, 개인정보를 검토한 최소 데이터 수집입니다.

목표 설정은 **itch.io HTML5 / Unity WebGL, 플레이테스트 / 시장 검증, 수익화 OFF, 가격 결제 없음**입니다. 1단계는 외부 테스터 약 10–20명만 접근할 수 있는 비공개 테스트로 권장합니다. 핵심 UX와 WebGL 문제를 해결한 뒤에만 2단계 공개 테스트로 진행합니다. 이번 감사에서 어느 단계도 설정하거나 게시하지 않았습니다.

### 점검 범위와 기준

- 작업공간: `D:\AIPrototype\ColorGateRunner`; Unity 프로젝트: 그 아래 `ColorGateRunner/`.
- 확인한 HEAD: `e48448d81a28f2f4338a4f2db79e6f74ca90391a` (`docs: refresh current project handoff`), branch `main`. 구현 기준: `d937b3a242b04df6d211de7dfe92abfca82175e3`. 두 기준 사이에는 `Docs/CURRENT_STATUS.md`와 `Docs/GPT_HANDOFF.md`만 변경되었습니다.
- `Tools/QualityGraph/Invoke-QualityPreflight.ps1 -Mode inspect`: ALLOW; Editor 닫힘; 프로젝트 잠금 없음; backend `not-applicable`. 이번 감사에서는 Unity 실행, 그래픽 backend, Player 세션, 새 캡처 생성을 하지 않았습니다.
- 기존에 추적 중인 파일 하나가 수정 표시됨: `ColorGateRunner/ProjectSettings/ProjectSettings.asset`. 내용 diff는 기존 WebGL define `APP_UI_EDITOR_ONLY`에 `SENTIS_ANALYTICS_ENABLED`를 추가한 것뿐이며 명시적으로 허용된 변동 예외입니다. 감사 중 수정·stage·정규화하지 않았습니다.
- 필수 기준 문서를 읽었습니다. 여기에는 `Docs/GPT_HANDOFF.md`, `Docs/FRONTEND_FLOW.md`, `Docs/PRODUCT_SYSTEMS.md`가 포함됩니다. 인수인계 문서는 설계/테스트 문서의 오래된 과거 문장들을 이미 지적합니다. 폐기된 수치나 규칙 대신 현재 기준선과 명시된 후속 반복 작업 계약을 사용했습니다.
- 여기서 “Play” 근거는 실행 경로 추적, 직렬화 Scene/Catalog, 기존 테스트 결과와 시뮬레이션, 기존 캡처 두 장의 점검입니다. **현재 빌드의 실제 브라우저 플레이 및 초심자 관찰은 NOT VERIFIED**입니다. 이해도에 관한 발견은 격차/가설이지 실제 플레이어에게서 관찰한 결과가 아닙니다.
- 이번 감사에서 추가한 파일은 이 보고서뿐입니다. 코드, Scene, 패키지, 설정, 테스트, 아트, 저장 데이터, 빌드 설정은 변경하지 않았습니다. Commit/Push도 하지 않았습니다.

### 근거 참조 표

아래 경로는 저장소 루트 기준입니다. 이 보고서 전체에서 `P`는 `ColorGateRunner/Assets/Game/Presentation/`, `C`는 `ColorGateRunner/Assets/Game/Core/`, `E`는 `ColorGateRunner/Assets/Game/Editor/`를 줄여 씁니다.

| 참조 | 구체적 근거 |
|---|---|
| E1 | `AGENTS.md`; `Docs/CURRENT_STATUS.md` Authoritative Baseline; `Docs/TEST_PLAN.md` Phase A / Tier 0; `Docs/GPT_HANDOFF.md` |
| E2 | `P/BootSceneController.cs::InitializeAndContinue`; `P/AppRoot.cs::Initialize`, `CreateProductionGraph`; `ColorGateRunner/Assets/Scenes/Boot.unity` |
| E3 | `P/FrontendSceneController.cs::ApplyContext`, `ChooseGuest`, `PlayCampaign`; `E/FrontendSceneBuilder.cs::CreateTitlePage`; `ColorGateRunner/Assets/Scenes/Frontend.unity` |
| E4 | `P/StageSceneController.cs::StartSelectedStage`, `HandleGameplayTap`, `HandleGateCrossed`, `RetryToItemSelection`, `OpenNextStagePreRun`, `RequestFailureExit`, `ConfirmFailureExit` |
| E5 | `P/CampaignResultFlow.cs`; `P/StageResultSequenceView.cs::ShowFailureExitConfirmation`, `ShowFailureFinalChoice`; `ColorGateRunner/Assets/Scenes/SampleScene.unity` |
| E6 | `ColorGateRunner/Assets/Game/Resources/StageCatalog.asset`; `C/StageSession.cs`; `C/StageCatalog.cs`; `P/StageCatalogAsset.cs` |
| E7 | `E/TestBuildMenu.cs`; `ColorGateRunner/ProjectSettings/ProjectVersion.txt`, `EditorBuildSettings.asset`, `ProjectSettings.asset` |
| E8 | `ColorGateRunner/Assets/WebGLTemplates/ColorGateRunnerPortrait/index.html`; 설치된 Unity Default 템플릿; 관찰한 `ColorGateRunner/Builds/Test/WebGL/` |
| E9 | `ColorGateRunner/Assets/Game/Product/LocalSaveService.cs`, `SystemProductAdapters.cs`; `P/AppRoot.cs::CreateProductionGraph` |
| E10 | `P/GameplayTapSurface.cs::OnPointerClick`; `P/SafeAreaLayout.cs`; `P/StageSceneController.cs::OnApplicationFocus`, `OnApplicationPause`; `P/HapticFeedback.cs` |
| E11 | `P/DevelopmentTelemetry.cs`; `P/StageSceneController.cs`의 텔레메트리 호출; `SampleScene.unity`의 `developmentTelemetryEnabled: 0` 필드 |
| E12 | `ColorGateRunner/Packages/manifest.json`, `packages-lock.json`; `ColorGateRunner/ProjectSettings/UnityConnectSettings.asset`; `ColorGateRunner/Assets/Game/Resources/ProductLinkConfiguration.asset` |
| E13 | `Artifacts/Validation/EditMode-results.xml`, `PlayMode-results.xml`; `Artifacts/Simulation/MechanicCampaign-*`; `Artifacts/Experiment/Step10-*` |
| E14 | `Artifacts/VisualValidation/Step9C/`; `Artifacts/Validation/VisualQC/`; `README.md`; `Docs/GAME_DESIGN.md`의 한 줄 소개 |

## 2. 현재 첫 사용자 흐름

**상태: NEEDS WORK.** 아래는 소스에서 추적한 프로덕션 경로이며, 새 브라우저 녹화 자료는 아닙니다.

| 단계 | 신규 플레이어가 보게 되는 화면 | 근거 / 해석 |
|---|---|---|
| 호스트 실행 | itch.io에서 로딩 전에 실행 클릭을 요구할 수 있음 | 감사 시점에 호스트 페이지 없음; NOT VERIFIED |
| HTML 로딩 | 백분율 표시와 canvas가 보이고 로드 완료 후 안내가 사라짐; Unity 시작 promise가 거부되면 오류 문자열 표시 | E8; 로더 오류용 사용자 정의 복구/재로드 버튼 없음 |
| Boot | `LOADING`, 버전 표시; 로컬 서비스와 저장 데이터 초기화 후 Frontend로 자동 이동 | E2; 실패 시 원인과 Retry 표시, 기존 초기화 graph 재사용 |
| 계정 선택 | `CHOOSE ACCOUNT`, 로컬 Guest, `START AS GUEST` | E3; Google provider는 이용 불가/숨김 상태이며 실제 가입 요구가 아님 |
| Home | Guest, Coin/Heart, 챕터, Stage 카드, Play, 목적지 탭 다섯 개 | E3와 Iteration 60 기준; Shop/Rank/Collection이 테스트 목적을 흐릴 수 있음 |
| PreRun | 선택된 Stage와 아이템 선택 화면, Start | E4; Stage 1은 선택 아이템 두 개를 모두 허용하지 않지만 PreRun은 거침 |
| 카운트다운/플레이 | `3, 2, 1, GO`, 자동 전진; 포인터 입력으로 색상 변경 | E4, E6, E10; HUD에 현재/다음 색과 Gate 진행 표시 |
| Gate 일치 | 통과/파괴 효과, 파티클, 진행도 상승 | E4 `HandleGateCrossed`; 성공은 보이지만 이유를 명시적으로 가르치지는 않음 |
| Gate 불일치 | 이동이 실패하고 중립화된 실패 화면 후 `STAGE FAILED` | 실패 문구에는 남은 Gate와 Continue 안내가 있지만 기대 색과 플레이어 색의 불일치는 설명하지 않음 |
| 실패 복구 | Continue 제안 → Give Up → Heart 소모 확인 → 최종 Try Again → PreRun → Start | E4/E5; 현재 결과 큐에는 필수 추가 설명 페이지 없음 |
| 클리어 | 축하 연출 → 확정 보상 → Next Stage → 다음 PreRun → Start | E4/E5; 축하 유지 2.35초, crossfade 0.25초; 이동 전 보상 표시도 거침 |

첫 카운트다운 전 일반적인 조작은 **3회**입니다: Start as Guest, Play, Start. itch.io의 Click to Play가 추가되면 일반적으로 총 **4회**입니다. 기존 저장 사용자는 Guest를 건너뛰어 게임 내 2회입니다. 설정/동의 또는 추가 이동이 있으면 더 늘 수 있으며 분석 동의는 아직 없습니다.

실패 후 새 카운트다운까지는 보통 **4회 조작**입니다: Give Up, 확인, Try Again, Start. 초기 카운트다운도 다시 보며 Heart가 0이면 시작 권한이 거부될 수 있습니다. Continue는 Retry와 다른 기능이며, 실패한 시도를 유지합니다. 클리어 후 다음 카운트다운까지는 보통 **2회**입니다: Next Stage, Start.

교착 가능성은 구체적입니다. 로컬 초기화가 계속 실패하면 Boot에서 진행할 수 없습니다. Heart 부족, 아이템 부족, 저장 실패 시 Start가 거부됩니다. 다음 Stage 이동은 Catalog에서 해금 가능 여부에 달려 있습니다. 현재 제품 계약으로는 의도된 동작이나 앞의 두 상황은 비상업적 테스트에 지장을 줍니다. Lobby로 돌아가는 기능은 즉시 다시 시도할 수 있는 복구와 다릅니다.

주요 Play 버튼을 찾는 데 개발자 설명은 필요하지 않지만, **클릭/탭, 플레이어 색상, Gate 목표 색상, 실패의 관계가 충분히 명시되어 있지 않습니다.** `TAP TO SWITCH COLOR · HORIZONTAL S-CURVE` 문구는 Campaign 튜토리얼이 아니라 별도 Spline Lab에 있습니다. 내부 대체 Lobby의 `Learn the red and blue match.` 문구도 입력 방법을 충분히 설명하지 않으며, 일반 Frontend 경로는 그 대체 Lobby를 건너뜁니다.

## 3. 출시 차단 항목

아래 문제는 요청된 기준으로 외부 플레이어를 초대하는 데 장애가 됩니다. 불필요한 플랫폼 재설계를 요구하는 것은 아닙니다.

| ID | 상태 | 발견 내용 | 초대 전 필요한 조치 |
|---|---|---|---|
| B1 | BLOCKER | 현재 소스와 연결해 추적할 수 있고 호스팅에서 승인된 빌드가 없음. 더 최신인 로컬 산출물은 있으나 출처와 실제 시작은 NOT VERIFIED. | revision/설정과 빌드를 연결하고 최종 ZIP 검증. 실제 itch.io iframe에서 새 브라우저로 실행→입력→클리어→실패→재시도 확인 |
| B2 | BLOCKER | 반복 실패 시 유한 Heart가 소모되어 다음 시도가 막힐 수 있음. 복구 화면에는 테스트 범위 밖 Coin/아이템/Heart 선택도 노출됨. | 재화, 대기시간, 광고, 구매, 계정 행동 없이 계속 재시도할 수 있는 격리된 테스트 접근/복구 승인 및 구현 |
| B3 | BLOCKER | 외부 게임플레이 데이터 수집 경로 없음. 비활성 개발 CSV로는 원격 funnel 질문에 답할 수 없음. | 수집 provider/개인정보 경계 승인, 최소 이벤트 구현, 게임 진행을 막지 않으면서 실제 수신 확인 |

현재 C# 컴파일 오류, 누락 Scene, WebGL 모듈 부재, 지원되지 않는 런타임 API는 확인되지 않았습니다. 브라우저 실행이 확인되지 않았다는 것은 근거상의 관문이며, 게임이 충돌한다고 단정하는 것이 아닙니다. 4–5절의 온보딩과 경로 개선은 여전히 필요하지만, 감사에서 현재 게임 플레이 자체가 불가능하다고 판단한 것은 아닙니다.

## 4. 첫 플레이어 UX 격차

| 요구사항 | 상태 | 근거와 최소 추가 요소 |
|---|---|---|
| 목표를 이해함 | NEEDS WORK | E3/E4는 Stage와 진행도는 보여주지만 완전한 색 맞추기 규칙을 눈에 띄게 안내하지 않음. 첫 플레이 중 짧은 목표/입력 안내 추가 |
| 입력으로 플레이어 색을 조절함을 이해함 | NEEDS WORK | E10은 왼쪽 포인터 클릭을 전달하고 E4는 즉시 색을 바꿈. 러너/현재 색 표시와 연결된 클릭/탭 안내 필요 |
| Gate 목표 색을 식별함 | NEEDS WORK | 색상/엠블럼 자산과 Gate 표현은 존재. 배경 장식과 구별하도록 첫 Gate를 잠시 강조할 필요가 있는지 검토 |
| 통과 전 색을 맞춰야 함을 이해함 | NEEDS WORK | E6 판정은 교차 순간의 색을 비교하지만 HUD가 이 시간 관계를 알려주지 않음. 피할 수 없는 첫 판정 전에 가르침 |
| 올바른 행동을 알아봄 | READY | 성공 Gate 파괴 효과, 파티클과 진행도 사용 가능. 초심자가 이를 이해하는지는 NOT VERIFIED |
| 잘못된 행동을 이해함 | NEEDS WORK | E4/E5는 Stage 실패/Continue를 보여주나 불일치 원인을 설명하지 않음. 기대 색과 현재 색을 잠시 보여 실패에서 학습하게 함 |
| 즉시 재시도함 | NEEDS WORK | 네 번 조작과 카운트다운 필요. B2 정책 승인 후 테스트에서는 바로 Retry할 수 있게 함 |
| 자발적으로 계속함 | NEEDS WORK | Next Stage는 있으나 보상/아이템 화면과 전체 Catalog 이후 League는 제안한 단기 테스트를 구성하지 않음. 명확한 선택형 다음 단계와 종료점 필요 |

### Stage 1을 인터랙티브 튜토리얼로 쓸 수 있는가?

**기반 상태: READY.** 점검한 아키텍처에서는 별도 튜토리얼 Scene이 필요하지 않습니다. Stage 1은 두 색, Gate 24개, modifier 없음, 선택 아이템 없음, 첫 6개 Gate 도입 구간, 고정 시드 `10101`, 간격 1.55→1.25초로 이미 격리되어 있습니다. `StageSession`은 Red에서 시작하며 결정론적 색 순환과 판정을 담당합니다. 기존 UI overlay와 피드백을 같은 Stage/Scene 안에서 활용할 수 있습니다.

**교육 수용 기준은 NEEDS WORK입니다.** 최소한 (1) 입력과 색 맞추기를 연결한 짧은 안내, (2) 첫 필수 색 변경에서 러너와 목표 Gate의 분명한 관계, (3) 입력 반영 및 일치 성공에 대한 피드백, (4) 이해 가능한 첫 불일치와 직접 재시도, (5) 관찰 가능한 온보딩 완료 기준이 필요합니다. 안내 문구를 단순히 보여준 것을 완료로 간주해서는 안 됩니다. 애니메이션, 일시정지, Gate 변경, 별도 튜토리얼 상태 머신을 지금 결정하지 마세요. 타이밍/생성을 바꾸는 통제된 연출은 Game Director의 명시적 승인과 해당 회귀 검증 단계가 필요합니다.

Stage 1의 Perfect/no-item 시뮬레이션 클리어 시간은 33.828초입니다. 잠정 Novice 프로필의 최초 클리어율은 0.211(1,000회 모델 실행), Average는 0.525입니다. 이는 **실제 플레이어 측정값이 아니며** 난이도나 공정성의 증거가 아닙니다. 두 색만으로 온보딩이 충분하다고 가정하지 말아야 한다는 신호이며, 특히 Heart가 5개라 재시도가 제한되는 점을 고려해야 합니다. 근거: E6/E13.

## 5. 권장 플레이테스트 Stage 구성

**경로 통합 상태: NEEDS WORK. 후보 평가에서 기존 Stage 정의 상태: READY.** 아래 순서로 이미 존재하는 2색 Stage 8개를 권합니다. 안정 ID를 바꾸거나 프로덕션 Catalog 순서를 재작성하지 마세요. Green 학습을 건너뛰어 첫 테스트를 요청된 기믹 진행에 집중합니다.

| 테스트 순서 | 기존 ID / 이름 | Gate 수 | 기믹 / 목적 | Perfect 무선택 클리어(초) | 상태 |
|---:|---|---:|---|---:|---|
| 1 | `stage-01` TWO-COLOR BASICS | 24 | 기본 색상 맞추기, 제안된 Stage 내 온보딩 | 33.828 | READY |
| 2 | `stage-02` RHYTHM CONTRAST | 28 | 변하는 간격에서 규칙 적용 | 34.453 | READY |
| 3 | `stage-03` EXCEPTION COLOR | 30 | 반복 리듬 암기 대신 의도된 예외 읽기 | 36.698 | READY |
| 4 | `stage-06` SHIELD TRAINING | 38 | 구매 없이 Stage에 제공되는 Shield 1회 사용 | 38.960 | READY |
| 5 | `stage-09` CAMOUFLAGE INTRO | 38 | 지연 후 드러나는 목표 색 첫 경험 | 40.859 | READY |
| 6 | `stage-21` HIDDEN INTRO | 42 | 색이 사라진 후 Gate 색 기억, 7회 발생 | 43.436 | READY |
| 7 | `stage-18` ECHO INTRO | 42 | 저장된 일치 색 획득/소비, Shield와 구별 | 43.436 | READY |
| 8 | `stage-24` FLICKER INTRO | 44 | 변하는 목표 읽기, 6회 발생, 1.35초 전환 주기 | 44.604 | READY |

근거: E6 Catalog revision 16; E13 Campaign CSV에서 `Perfect`, `shield=False`, `booster=False`. Stage 6은 Stage 규칙으로 제공되는 Shield를 포함합니다. 여기서 “no-item”은 보유 아이템 선택이 없다는 의미입니다. Perfect 8개 행 모두 클리어합니다. 런타임 테스트에는 Echo/Flicker 풀과 가시성 검증이 있습니다. 저장된 XML상 전체 테스트가 통과했습니다. **이 경로가 브라우저에서 안정적인지는 NOT VERIFIED입니다.**

합계는 **모델상 성공 플레이 316.274초(5분 16초)**이며, 8회의 카운트다운, 메뉴, 교육, 축하 연출, 재시도는 포함하지 않습니다. 따라서 성공 세션 5–10분은 가능성이 있지만 신규 플레이어의 실제 소요 시간으로 입증된 것은 아닙니다. Intro Stage라도 후반 캠페인 속도이며 중간 연습 단계를 건너뜁니다. 초심자에게 이미 균형이 맞는 순서라고 주장하지 마세요.

중요한 경로 격차가 있습니다. 현재 `OpenNextStagePreRun`은 위 표가 아니라 기존 진행/해금/League 규칙을 따릅니다. 현재 일반 Campaign만으로는 8개 안에 전부 도달할 수 없습니다. 승인된 제한적 플레이테스트 경로/접근 정책이 필요하며, 건너뛴 Campaign Stage를 클리어 처리하거나 최고 해금 Stage/저장 데이터를 부풀리면 안 됩니다. Stage 21 다음 Stage 18로 돌아가는 순서도 최고 Stage 숫자가 부정확한 진행도 지표임을 보여줍니다.

| 제외 항목 / 변형 | 상태 | 이유 |
|---|---|---|
| Stage 4–5 / Green 도입 | DEFER | 8개 Stage 테스트의 질문에는 색 학습 추가가 불필요. 필요하면 후속 테스트에서 다룸 |
| Stage 7 Booster 및 Stage 8 인벤토리 적용 | DEFER | Booster는 색 맞추기를 우회하며 보유 아이템 선택이 경제 요소를 도입해 학습 결과를 흐릴 수 있음 |
| Fog 12–14 및 Ice 15–17 | DEFER | 첫 테스트에 필요한 것보다 시야/속도 변수가 많음 |
| Hard/mastery 블록, Stage 25–26, League/endless, Stage 27+ | DEFER | 더 길거나 어렵거나 아직 없는 콘텐츠; 초기 funnel에 불필요 |
| 기믹 Stage에서 Shield/Booster 선택 | DEFER | 선택 인벤토리는 경로에서 제외. 현재 Shield 선택은 Echo Provider를 숨겨 Echo 학습을 무효화함. Stage 6 제공 Shield는 유지 |
| 세션이 길 때 기억/가시성 학습 중 하나 제거 | OPTIONAL | 짧은 파일럿 후 Director가 Camouflage 또는 Hidden을 선택 가능. 조용히 Stage 밸런스를 바꾸지 말 것 |

## 6. WebGL 호환성

### 런타임 및 도구 체인

| 요구사항 | 상태 | 확인한 값 / 근거 | 의미 / 조치 |
|---|---|---|---|
| Unity 및 설치 모듈 | READY | Unity `6000.5.1f1 (0d9463e84828)`; 대응하는 `Editor/Data/PlaybackEngines/WebGLSupport` 존재 | 설치 파일로 빌드 가능성은 확인. 새 빌드 성공은 NOT VERIFIED |
| Scene 순서/목적지 | READY | `EditorBuildSettings.asset`에서 Boot → Frontend → SampleScene 활성화; E2/E3에 직렬화 목적지 있음 | Boot를 첫 Scene으로 유지하고 SampleScene 직접 실행/개발자 진입을 배포하지 말 것 |
| Core 게임플레이 이식성 | READY | 순수 Core와 Product 경계 존재; 런타임 소스 검색에서 사용자 정의 thread/socket/native import 없음 | 기존 규칙 사용. 소스 검색 결과만으로 컴파일된 패키지 전체 검증이 완료된 것은 아님 |
| 파일 API | NEEDS WORK | E9는 UTF-8 `FileStream`, `Flush(true)`, Copy/Move 사용. WebGL은 `File.Replace` 대신 `RecoverableOnly` 선택 | 가상 파일시스템 지원은 가능성 있으나 브라우저 저장 내구성/오류 처리는 테스트 필요 |
| 영속 경로 | NEEDS WORK | E2에서 `Application.persistentDataPath` 아래 `product-save.json` 기록 | 브라우저/origin별 저장이며 PC 파일 저장이나 기기 간 백업이 아님 |
| Threads | READY | `webGLThreadsSupport: 0`; 사용자 정의 `Thread`/`Task.Run` 게임플레이 경로를 찾지 못함 | 꺼진 상태 유지; cross-origin isolation 요구사항을 추가하지 말 것 |
| 네트워크 | READY | 런타임 사용자 정의 `UnityWebRequest`, `HttpClient`, socket 또는 서비스 초기화 호출을 찾지 못함 | 게임플레이는 분석/네트워크 실패와 독립 가능. 실제 패키지 네트워크 트래픽은 NOT VERIFIED |
| Plugin/SDK | NEEDS WORK | IAP 5.4.2, Services Core 1.18.0, URP 17.5.0, Input System 1.19.0, Splines 2.9.0; AI 패키지도 있음. Assets 아래 네이티브 `.dll/.so/.a/.bundle/.aar/.jar/.jslib`는 찾지 못함 | 감사만을 이유로 패키지 추가/삭제 금지. 최종 Player stripping과 런타임 트래픽 확인; 패키지 존재가 서비스 사용 증거는 아님 |
| Shader/그래픽 | NEEDS WORK | ProtectionField, Flicker frame 및 emblem dissolve는 shader model 3.0; URP Bloom과 투명 layer | D3D11 근거로 WebGL2 shader가 검증되지는 않음. 브라우저에서 색/투명도/pink material/dissolve/프레임 확인 |
| 오디오 | OPTIONAL | 게임 콘텐츠에 출시용 AudioSource/AudioClip 또는 오디오 자산 구현 없음; Music/SFX는 설정값으로 저장 | v0.1 무음 가능. 추가한다면 사용자 동작 후 재생 및 focus/mute 확인 |
| 햅틱 | DEFER | `HapticFeedback.cs`는 `UNITY_ANDROID || UNITY_IOS`에서만 진동 호출 | WebGL 터치는 진동 피드백에 의존하지 말 것 |
| 앱 종료 | NEEDS WORK | `UnityFrontendExitHandler.RequestExit`에서 `Application.Quit`; `webGLCloseOnQuit: 0` | 브라우저 탭이 닫힌다고 약속하지 말 것. 테스트 종료 상태를 명확히 안내; 탭 종료 동작은 NOT VERIFIED |
| 브라우저 포커스 | NEEDS WORK | E10은 focus/background 이탈 시 Pause 요청; `runInBackground: 0` | iframe focus, 탭 숨김, 복귀 검증. 소스상 의도는 있으나 callback 발생은 플랫폼에 따라 다름 |

### 저장 호환성: 범위를 좁힌 실제 위험

`AppRoot.CreateProductionGraph`는 WebGL에서 명시적으로 복구 가능한 파일 교체 방식을 선택합니다. primary/temp/backup 검증과 미래 schema 차단도 이미 있습니다. 이는 유용한 기반이며 Cloud save나 계정 시스템은 필요하지 않습니다.

다만 HTML 설정은 `autoSyncPersistentDataPath`를 활성화하지 **않습니다**. 설치된 Unity 6000.5.1f1의 `WebGLSupport/BuildTools/prejs/IdbFs.js`는 IDBFS를 `autoPersist: !!Module['autoSyncPersistentDataPath']` 설정으로 mount합니다. `BuildTools/lib/FileSystem.js`도 자동 동기화 opt-in과 수동 동기화를 설명합니다. 프로젝트 자체 JavaScript 저장 bridge는 찾지 못했습니다. `FileStream.Flush(true)`만으로 IndexedDB transaction 완료가 입증되지는 않습니다. **실제 새로고침 후 저장 유실은 NOT VERIFIED**이며, template/adaptor를 작게 고쳐야 하는지 결정하기 전에 검사/테스트해야 합니다.

새 저장 공간, 클리어 저장 후 일반 새로고침, 클리어 직후 즉시 닫기, IndexedDB 거부, private browsing, 사이트 데이터 삭제, 호스팅 iframe 저장을 테스트하세요. Unity 최신 browser 안내는 Safari iframe IndexedDB 제약을 특별히 언급합니다. 데스크톱 파일시스템 테스트 성공만으로 저장을 추정하지 마세요. Director 승인과 사전 안내가 있다면 1회 세션 비상업 테스트에서 저장이 안 되는 것은 허용할 수 있지만 게임이 계속 작동해야 합니다. 저장 오류 때문에 Boot나 Retry가 영구히 막혀서는 안 됩니다. [Unity 6000.5 브라우저 호환성](https://docs.unity.com/en-us/engine/6000.5/manual/platform-specific/webgl/intro/browsercompatibility)

### 게시 설정

아래 값은 직렬화된 설정 또는 빌드 도구 동작을 관찰한 것이며 감사 중 바꾸지 않았습니다. Enum 값은 [Unity reference source](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/PlayerSettingsWebGL.bindings.cs)와 대조했습니다.

| 설정 | 현재 값 | 상태 | v0.1 권장 |
|---|---|---|---|
| Development Build | `TestBuildMenu.CreateBuildOptions`: `BuildOptions.Development` | NEEDS WORK | 외부 후보는 non-Development로 빌드. 승인된 별도 구성으로 기존 내부 테스트 빌드 계약/테스트 유지 |
| Compression Format | `0` = Brotli; 관찰한 로컬 출력은 `.data/.wasm/.js` 비압축 확장자를 사용 | NEEDS WORK | non-Development 출력 후보로 Brotli 유지, 생성 확장자와 CDN header 확인. 현재 설정만 보고 실제 build 옵션을 단정하지 말 것 |
| Decompression Fallback | `0` / off | READY | 호스팅 header가 정확하다면 브라우저 기본 압축 해제 사용 가능. itch.io는 `.br`를 지원하므로 fallback 활성화가 자동으로 필수는 아님 |
| Strip Engine Code | `1` / on | READY | 유지하고 후보 빌드의 save serialization/Scene 실행 확인 |
| Exception support | `1` / ExplicitlyThrownExceptionsOnly | READY | 스모크 테스트 후 첫 외부 후보에 유지. 실패 조사 시 별도 진단 빌드는 더 상세한 exception 설정을 쓸 수 있음 |
| Debug symbols / diagnostics | 모두 `0` | READY | 외부 archive에서 디버그 전용 출력 제외. 관찰 폴더에는 Burst `DoNotShip` 파일이 있음 |
| Texture compression | 점검한 UI import는 `textureFormat: -1`, WebGL override 비활성; WebGL format 고정 설정은 찾지 못함 | NEEDS WORK | build report의 실제 format 확인. 실제 데스크톱/모바일 대상과 alpha 품질에 근거해 선택; 증거 없이 DXT/ETC2/ASTC라 하지 말 것 |
| Memory | 초기 32MB; 최대 2048MB; growth mode 2 (geometric), step 0.2, cap 96MB; legacy size 필드 32MB | NEEDS WORK | 최초 로드/peak heap/브라우저 메모리 측정. 최대치는 상한이지 선할당량이 아님. 초기 heap/상한 변경 전 profile 필요 |
| Data caching | `1` / on | READY | 후보에서 유지하되 cold/warm 로드와 cache 거부 테스트. Asset cache가 Product save 지속성 증거는 아님 |
| Template | `PROJECT:ColorGateRunnerPortrait` | READY | 기존 경량 template 재사용, 새 템플릿 체계 불필요 |
| Orientation | `defaultScreenOrientation: 0` (portrait) | NEEDS WORK | 네이티브 orientation 값이 브라우저 회전을 고정하지 않음. letterbox 포함 세로 play area 유지 후 회전 테스트 |
| Resolution / canvas | Web 기본 540×960; HTML width/height 일치; CSS는 9:16 frame에 맞춤 | READY | 반응형 맞춤 유지. 실제 render 크기는 CSS/device pixel ratio 영향을 받을 수 있으므로 선명도/성능 측정 |
| Fullscreen | 프로젝트 fullscreen 버튼 및 custom HTML의 instance 참조 없음 | NEEDS WORK | 먼저 itch.io fullscreen 제어 사용; 진입/복귀 시 viewport/canvas 크기 변경 확인 |
| Wasm / threads | Wasm linker; threads off; table/BigInt flag 0 | READY | 불필요한 WebAssembly 기능 요구를 추가하지 말 것 |

**Default template 검토:** 설치된 Default template로 게임을 시작할 수 있으므로 Unity/itch.io가 별도 템플릿을 필수로 요구하지 않습니다. 그러나 수정하지 않은 Default는 데스크톱 canvas를 지정된 pixel 크기로 고정하고 모바일 canvas를 viewport에 늘려 맞춥니다. 짧은 데스크톱 iframe, 가로형 휴대폰, 전체 화면에서도 이 프로젝트의 9:16 구성을 유지하지 못합니다. 그대로면 요청한 반응형 세로 화면 조건에 부적합합니다. 프로젝트에는 이미 화면 맞춤/letterbox template이 있어 이를 유지하는 것이 가장 작은 경로입니다. 근거: 설치본 `WebGLSupport/BuildTools/WebGLTemplates/Base/Default/index.html`, `TemplateData/style.css` 및 E8 대조.

Custom template에는 top-window 이동 가정이 없고 상대 경로를 사용하므로 iframe 호환 가능성은 있습니다. CSS `safe-area-inset-*` 처리와 검증된 mobile visual-viewport 대응은 없으며 `vh/vw`를 씁니다. `SafeAreaLayout`은 Unity `Screen.safeArea`에 반응하지만 브라우저가 notch/chrome의 유용한 영역값을 주는지는 NOT VERIFIED입니다.

## 7. itch.io 패키징 준수

새 빌드는 만들지 않았습니다. 기존 menu는 Builder 소유 콘텐츠를 재생성하고 asset을 저장하고 platform 전환 및 출력 덮어쓰기를 수행합니다. 과거 검증 때 Warp material도 다시 직렬화되었습니다. 이 도구를 주 프로젝트에서 실행하면 감사의 변경 제한을 위반합니다. 기존 파일에서 크기만 관찰했습니다.

**관찰한 로컬 출력은 승인된 기준선이나 출처 증거를 대체하지 않습니다.** `CURRENT_STATUS.md`에는 8월 28일 6개 파일, 155,566,553 bytes 산출물이 기록되어 있습니다. 현재 폴더에는 9월 12일 생성된 7개 파일이 있으며 가장 최신은 `2026-09-12T15:21:10Z`, 전체 **188,152,604 bytes (188.15 decimal MB / 약 179.44 MiB)**입니다. Index SHA-256은 `FC6F9BABE15686D6646807E2E8DB9B5F8D256856F1C3D0590861C35665E797FF`. 소스 revision과 대응하는 manifest는 찾지 못했습니다. 그러므로 HEAD 새 빌드 성공으로 보지 말고 최신이지만 검증 안 된 로컬 근거로 보고합니다.

| 요구사항 | 상태 | 근거 / 다음 확인 |
|---|---|---|
| HTML5 / Unity WebGL 출력 | READY | `index.html`, loader, framework, Wasm, data 파일 존재 |
| ZIP archive | NEEDS WORK | 감사 대상은 폴더이며 배포 ZIP 미검증. `WebGL/` 상위 폴더 대신 그 안의 파일들을 압축 |
| ZIP root에 `index.html` | NEEDS WORK | 현재 directory root에는 있음; 최종 archive entry 검사 |
| 상대 asset 경로 | READY | index에서 `Build/WebGL.loader.js`, `.framework.js`, `.data`, `.wasm` 참조; `StreamingAssets` 상대 경로 |
| 대소문자 경로 | READY | 명시적 entry-point 경로와 실제 파일명이 일치. 최종 runtime request는 호스팅 검증 필요 |
| UTF-8 파일명 | READY | 관찰 파일명은 ASCII 호환이고 HTML은 UTF-8 선언. 최종 archive encoding 확인 |
| 추출 후 1,000개 이하 파일 | READY | 현재 폴더 7개. 최종 ZIP 재확인 |
| 추출 후 전체 500MB 이하 | READY | 현재 폴더 합계 188,152,604 bytes |
| 파일당 200MB 이하 | READY | 최대 `Build/WebGL.wasm` 130,783,243 bytes; data 56,494,460 bytes |
| 반응형 viewport | READY | CSS가 사용 가능한 viewport에서 9:16으로 너비/높이 제한. 실제 browser 동작은 NOT VERIFIED |
| iframe embedding | NEEDS WORK | 소스 구조는 적합해 보이나 호스팅 실행/저장/focus NOT VERIFIED |
| fullscreen 크기 변경 | NEEDS WORK | CSS는 iframe viewport 변경에 반응할 것으로 보이나 itch.io 실제 전체 화면 전환 NOT VERIFIED |
| 배포 전용 콘텐츠 | NEEDS WORK | `ColorGateRunner_BurstDebugInformation_DoNotShip/Data/Plugins/lib_burst_generated.txt` 제외. 검증 빌드에서 달리 확인되지 않는 한 런타임 `StreamingAssets/UnityServicesProjectConfiguration.json`은 보존 |
| 압축 / 응답 header | NEEDS WORK | 현재 파일은 plain; 향후 Brotli 출력은 호스트에서 대응 `Content-Encoding`과 Wasm MIME 확인 |

이 제한과 경로 규칙은 사용자 요구 및 [itch.io HTML5 안내](https://itch.io/docs/creators/html5)에 부합합니다. itch.io는 `.br` 확장자로 Brotli를 처리하고 gzip도 지원한다고 안내합니다. fallback은 호스팅 선택이며 필수 checkbox가 아닙니다. 관찰 폴더의 추출 크기는 압축 ZIP 크기나 측정된 네트워크 전송량이 아닙니다.

## 8. 브라우저 / 기기 테스트 매트릭스

**아래 항목 중 검증된 v0.1 후보로 실행한 것은 없습니다.** 승인 시 버전, 하드웨어, build hash, 호스팅 URL, cold/warm 로드 시간, console 오류, 메모리, 실패 상세를 기록해야 합니다. 엔진 호환성만으로 게임을 인증할 수는 없습니다.

| 대상 | 상태 | 근거 상태 | 필요한 검사 / rollout 역할 |
|---|---|---|---|
| Windows Chrome 최신 stable | NEEDS WORK | NOT VERIFIED | 1단계 기본: cold load, Guest/진입, 좌클릭, 8개 Stage, 실패/재시도/다음, fullscreen, 탭 이탈/복귀, 동의 및 이벤트 |
| Windows Edge 최신 stable | NEEDS WORK | NOT VERIFIED | 주요 흐름 및 호스팅 저장/네트워크 반복. Chromium 동등성을 가정하지 말고 확인 |
| Windows Firefox 최신 stable | NEEDS WORK | NOT VERIFIED | 엔진 시작, shader, 입력, fullscreen, cache, 동의/이벤트; 시험 전 experimental로 표기 |
| macOS Safari 최신 stable | NEEDS WORK | NOT VERIFIED | iframe 저장 제약, loader 오류, focus, 메모리, 저장 차단 상태; 테스트 전 지원 주장 금지 |
| macOS Chrome 최신 stable | OPTIONAL | NOT VERIFIED | 테스터가 사용하면 두 번째 데스크톱 OS로 유용; 새 플랫폼 기반을 요구하지 않음 |
| Android Chrome 물리 중급폰 | NEEDS WORK | NOT VERIFIED | 탭당 터치 1회, 길게 누름/drag 취소, 페이지 scroll, portrait/landscape, 주소창 resize, 앱 전환/복귀, 5–10분 성능 |
| iOS Safari notch 있는 실기기 | NEEDS WORK | NOT VERIFIED | 메모리 압박, WebGL2 shader, iframe IndexedDB, 터치, 브라우저 chrome 변동, 회전, safe area, 백그라운드 종료 |
| Tablet/소셜 앱 내장 브라우저 | DEFER | NOT VERIFIED | v0.1 지원 홍보 금지; 검증된 browser 사용을 안내 |

데스크톱 viewport 사례: 기준 540×960, 더 짧은 360×640 iframe, 창 1366×768, 넓은 1920×1080 fullscreen, 좁은 resize 창. 이는 CSS viewport 테스트이지 내부 render buffer가 540×960을 유지한다는 뜻이 아닙니다.

모바일 사례: portrait, 세로 화면 letterbox 상태의 landscape, fullscreen 진입/이탈, 브라우저 chrome 축소/확장, 플레이/카운트다운 중간 인터럽트, 색 변경 없이 UI 탭, UI 밖 gameplay 탭. `GameplayTapSurface`는 `IPointerClickHandler`를 사용하며 좌클릭만 받습니다. 구조상 마우스/터치를 지원하지만 raw touch-down이 아니라 유효한 pointer click 뒤 전달됩니다. drag/cancel 및 multi-touch는 기기 테스트가 필요합니다.

대상 browser 승인 기준 제안: 시작에서 막히지 않음, 누락/pink 시각 오류 없음, 클릭/탭 1회에 논리 색 변경 1회, Retry/Pause/Next로 의도치 않은 입력 없음, focus 이탈 시 잠복 실패 없이 시도 정지, 주요 조작 접근 가능, 재화 대기 없음, 분석 거절/네트워크 오류가 플레이를 막지 않음. 타이밍/가독성/멀미는 사람이 판단해야 합니다.

실기기 테스트를 할 수 없다면 **데스크톱 우선 제한 테스트**를 권합니다. 실제 지원 기기 통과 전까지 itch.io Mobile Friendly를 끄고 모바일은 별도 탐색 cohort로 다룰 수 있습니다. Unity가 browser 계열을 지원한다고 명시했다는 이유만으로 iOS/Android 지원을 주장하지 마세요.

## 9. Analytics 준비도

**상태: 원격 게임플레이 데이터 목표에 BLOCKER.** UGS 게임 분석 구현은 확인되지 않았습니다.

- `com.unity.services.analytics`가 manifest 및 lock에 없습니다. `com.unity.modules.unityanalytics`는 엔진 module이며 UGS Analytics package가 설치됐다는 증거가 아닙니다. Services Core는 IAP 경로로 설치되어 있습니다.
- `UnityConnectSettings.asset`에서 Unity Connect 비활성, `UnityAnalyticsSettings.m_Enabled: 0`. 초기화 flag만으로 이 근거가 뒤집히지 않습니다. Dashboard project 연결, schema/environment 설정, 보존 기간, 서비스 권한 상태는 **NOT VERIFIED**입니다.
- 변동 가능한 `SENTIS_ANALYTICS_ENABLED` define은 package 관리 값이며 요청된 게임 이벤트 pipeline의 증거가 아닙니다. 생성된 `UnityServicesProjectConfiguration.json`도 Analytics 활성화를 입증하지 않습니다.
- `DevelopmentTelemetry`에는 로컬 유사 이벤트가 있습니다: `run-start`, `tap`, `gate`, `run-end` cleared/failed. Editor/Development에서만 활성 compile되고 Scene에서 꺼져 있으며 persistentDataPath에 로컬 CSV만 씁니다. 업로드/동의 경로가 없어 현재 형태로는 외부 사용자를 측정할 수 없습니다.
- 색상 변경 수락, 성공한 Start/countdown, Gate mismatch, Goal clear, 수락된 Retry에 신뢰할 만한 이벤트 지점이 있습니다. 나중에 observer를 붙여도 게임 규칙을 Analytics로 옮기거나 모든 Gate를 기록할 필요는 없습니다.
- 기존 문서는 별도 승인 없는 외부 분석 SDK 추가를 허용하지 않습니다. 이번 감사는 UGS를 평가했을 뿐 package 설치나 SDK 활성화 승인이 아닙니다. Unity는 현재 WebGL을 지원 플랫폼으로 안내하나 iframe 및 최종 SDK 통합은 **NOT VERIFIED**입니다. [Unity Analytics 설정 안내](https://docs.unity.com/en-us/analytics/get-started/get-started)

최소한 필요한 것은 provider/구성의 명시적 승인, 동의 처리, 소규모 lifecycle observer, 안정적인 이벤트 정의, 수신 검증입니다. 질문에 답하기 위해 계정/로그인, 광고 ID, 상업 backend, cloud save는 필요하지 않습니다.

## 10. Analytics 이벤트 권고

아래 권고 custom event는 모두 **NEEDS WORK**입니다. 소스 hook만 있고 구현, 동의 후 전송, 중복 방지는 없습니다. 정확한 schema/API 세부사항은 이후 승인된 명세에 남깁니다.

| 제안 이벤트 | 상태 | 권고 및 소스 지점 | 최소 유용 parameter / 의미 |
|---|---|---|---|
| `session_start` | NEEDS WORK | Boot 및 플레이테스트 진입이 사용 가능한 뒤 custom event `playtest_ready` 사용 권장. 모호한 이름으로 SDK lifecycle start를 중복 기록하지 말 것 | build/version, route version, 일시적 session 연결값, active elapsed 0. 모든 page 방문이 아니라 동의 후 플레이 준비 완료한 session 수 |
| `first_input` | NEEDS WORK | 유지. Guest/Play/동의/UI click이 아니라 **수락된 첫 gameplay 색 변경**을 뜻함. E4 `HandleGameplayTap`에서 Core cycle 성공 후 기록 | mouse/touch 구분, route position, stage ID, 첫 입력까지 경과시간. 페이지 session당 1회 |
| `tutorial_start` | OPTIONAL | 항상 Stage 1에서 시작하고 `stage_start`가 tutorial version을 담으면 중복. 시작 지점이 다를 때만 유지 | 유지한다면 tutorial version |
| `tutorial_complete` | NEEDS WORK | 관찰 가능한 기준을 승인한 경우에만 유지. 현재 milestone 없음 | tutorial version, Stage 1 attempt/retry 횟수, active elapsed. 안내 dismiss만으로 완료 처리하지 말 것 |
| `stage_start` | NEEDS WORK | 유지. E4의 성공적인 authorization/countdown 시작 시도당 1회. GO 시점 사용도 가능하나 countdown abandonment를 놓치므로 의미를 일관되게 결정 | `stage_id`, `route_position`, `attempt_index`, `retry_count` |
| `stage_fail` | NEEDS WORK | 유지. mismatch 실패 전환 1회만, 매 frame 아님. 계획/index를 알 수 있는 `HandleGateCrossed` 사용 | `stage_id`, `gate_index`, mechanic enum/name, `retry_count`, route/attempt, 시도 active elapsed. plan index와 일치하도록 gate index 0부터 정의 |
| `stage_complete` | NEEDS WORK | 유지. Goal clear 지점 1회. 기존 `run-end:cleared` hook 사용 가능. gameplay clear와 저장 실패 구분 | `stage_id`, `clear_time`, `retry_count`, route/attempt. clear time은 active gameplay만 포함하고 pause/countdown 제외 |
| `retry` | NEEDS WORK | 기존 Product 어휘에 맞춰 `stage_retry` 권고. 새 시도가 승인될 때 1회 기록하고 authorization 실패 UI 연타는 제외 | 이전/새 attempt ordinal, retry count, route position이 있는 `stage_id` |
| `session_complete` | NEEDS WORK | 이름을 `playtest_complete`로 변경. 브라우저 종료가 아니라 8개 route의 명시적 종료점에서 기록 | 마지막 Stage ID, 완료 최고 **route position**, 총 active play time, 완료 Stage 수. 진행 지표로 숫자가 가장 높은 Stage ID 사용 금지 |
| 제한된 active-session checkpoint | OPTIONAL | milestone만으로 시간 정보가 부족할 때에 한해 active play 60초 등 간격으로 `playtest_checkpoint` 1회. frame/Gate별 기록 금지 | active elapsed, 현재 route/attempt. 짧은 세션에 맞춘 제한 수량 |

공통 필드는 build ID, schema/tutorial/route version, session 연결값, 필요시 stage ID, attempt ordinal, 누적 active elapsed로 제한합니다. `retry_count`는 이 페이지 session 동안 해당 route stage에서 수락된 Retry 횟수이며 0부터 정의합니다. 불필요한 영구 사용자 필드는 피하세요. 무작위 **메모리 안 session token**은 기기 ID가 아닙니다. 교차 세션 지문으로 저장하지 마세요. 영구 식별값을 새로 만들기보다는 메타데이터 검토 후 SDK가 제공하는 적절한 session 연결값을 사용하세요.

Unity Analytics는 `gameStarted`, `gameEnded`, `clientDevice` 같은 표준 lifecycle/device event를 자동 전송합니다. 어떤 custom event가 중복인지, SDK가 기본으로 전송하는 것이 무엇인지 확인해야 합니다. [Unity 이벤트 문서](https://docs.unity.com/en-us/analytics/events/record-event)

**브라우저 종료 시 `session_complete` 전송은 신뢰할 수 없습니다.** Unity 문서상 일반적인 탭 종료 lifecycle에서 Web은 `OnApplicationQuit`을 지원하지 않습니다. flush 또는 visibility/unload callback은 best effort이지 수신 보장이 아닙니다. 중요한 event는 플레이 중 전송하고 collection은 게임을 막지 않게 합니다. 명시된 inactivity 기간이 지나면 마지막 수신 stage/attempt를 기준으로 이탈을 추정하고 미완료 세션 시간은 하한으로 취급하세요. [Unity OnApplicationQuit](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/monobehaviour/onapplicationquit)

| 확인할 질문 | 상태 | 측정 및 한계 |
|---|---|---|
| 몇 명이 게임을 로드했나? | NEEDS WORK | `playtest_ready`는 동의하고 플레이 가능한 상태에 도달한 수. 호스트 페이지 방문/실행은 별도 분모임. 로더 실패와 동의 거절을 성공 또는 누락 플레이로 단정하지 말 것. 모든 방문자 funnel은 별도 승인된 호스트 측 측정이 필요 |
| 첫 입력을 한 사람은 몇 명인가? | NEEDS WORK | 측정된 session 중 `first_input`이 있는 비율을 ready session과 비교 |
| 온보딩 완료자는 몇 명인가? | NEEDS WORK | Stage 1 시작 대비 `tutorial_complete`; 기준/version을 명시 |
| 어느 Stage에서 이탈하나? | NEEDS WORK | 마지막 미완료 route stage를 inactivity 후 추정. 확인 가능한 기술/로드 실패, 전송 누락, 자발 이탈을 구분. 상관관계만으로 원인을 입증하지 못함 |
| 어느 Stage에서 반복 실패하나? | NEEDS WORK | route position/attempt별 fail/retry 횟수. 기존 Campaign 저장 데이터와 첫 session 진행을 섞지 말 것 |
| 실패 후 Retry하나? | NEEDS WORK | 같은 측정 session에서 `stage_fail` 뒤 수락된 `stage_retry`/`stage_start` 확인 |
| 첫 session에서 어디까지 가나? | NEEDS WORK | session 내 완료한 최고 route position. 영구 ID나 기존 저장 가정 없이 “평생 첫 플레이어 session” 확인 불가능. 첫 테스트 참가자를 모집하고 한계 공개 |
| session 길이는 어느 정도인가? | NEEDS WORK | 수신 milestone의 active elapsed로 추정; 선택형 coarse checkpoint가 미수신 tail을 보완하고 route 완료는 완전 route 시간 제공 |

Profile/Guest ID, save 원문, transaction ID, 이름, 이메일, 직접 수집한 IP, 자체 생성 device ID, 자유 입력 텍스트는 업로드하지 마세요. 로컬 CSV의 모든 tap/Gate stream도 Analytics에 넣지 않습니다. 중복 callback, 거부된 Start, Retry 연타, Scene 전환, 동의 거절, 오프라인, 광고 차단, 재전송을 확인한 뒤 funnel 결론을 내세요.

## 11. 개인정보 / 동의 요구사항

**상태: NEEDS WORK; 이 경계가 승인되기 전까지 수집은 계속 꺼둡니다.** 현재 privacy URL은 비어 있고 analytics preference가 없습니다. Notification 동의와 analytics 동의를 혼용해서는 안 됩니다.

플레이테스트 전에는 **게임플레이 분석 허용 / 분석 없이 플레이** 선택을 동등하게 제공할 것을 권합니다. 목적, 이벤트 범주, provider, 저장/보존 방식을 간략하게 설명하고 실제 안내문을 연결하세요. 거부해도 게임을 계속할 수 있어야 합니다. 추후 철회 제어와 provider에 적합한 데이터 삭제 경로를 제공하세요. 이는 신중한 제품 요구사항 제안이지 모든 관할권 법률에 충분하다는 주장은 아닙니다.

Unity 6.2+ 및 Analytics SDK 6.1+에서 Unity는 예전 `StartDataCollection` 예제를 무조건 복사하는 대신 Developer Data의 `EndUserConsent` / Analytics intent 제어를 문서화합니다. 데이터 삭제 전에는 먼저 동의를 비활성화해야 합니다. 최종 선택 SDK/version에 맞는지 확인하고 동의 전/철회 후 event가 나가지 않는지 검증하세요. [Unity SDK 개인정보 제어](https://docs.unity.com/en-us/analytics/privacy-and-consent/manage-data-privacy)

**“이벤트 parameter에 PII가 없음”은 익명 수집을 뜻하지 않습니다.** Unity는 Analytics 서비스가 개인정보를 처리한다고 밝힙니다. 선택 전 SDK 생성 식별자, 기본 device event, provider가 처리하는 IP 등 네트워크 metadata, 보존 지역/기간 및 삭제 절차를 검토해야 합니다. 그 검토 없이 빌드를 “완전 익명”이라고 부르지 마세요. 연결 불가능한 엄격한 익명성이 필요한데 SDK가 이를 충족하지 못하면 UGS를 미루고 적절한 최소 대안을 Director 승인으로 정하세요. [Unity Analytics 개인정보 개요](https://docs.unity.com/en-us/analytics/privacy-and-consent/privacy-overview)

| 요구사항 | 상태 | 승인/검수 기준 |
|---|---|---|
| 게임 내 안내 및 작동하는 privacy 링크 | NEEDS WORK | 현재 `ProductLinkConfiguration.asset`의 Terms/Privacy/Support가 비어 있음; 테스트 설명과 실제 동작에 맞는 문구 필요 |
| Analytics 강제 없이 동의 | NEEDS WORK | 거절해도 게임 가능; 동의는 계정 선택과 분리 |
| 철회 / 삭제 | NEEDS WORK | 선택 SDK에서 작동 검증. PlayerPrefs 전체 삭제는 대안이 아님 |
| 금지한 사용자 지정 ID/문자 없음 | READY | 이번 감사는 전송하지 않았음; 구현 승인 시 parameter 제한 유지 |
| Provider metadata/보존 검토 | NEEDS WORK | Dashboard/계약 설정 NOT VERIFIED; 수집 전 책임자가 확인 |
| 동의를 위한 광고, IAP, 계정 연동 없음 | DEFER | 이런 시스템을 분석의 선행 조건으로 끌어들이지 말 것 |

10–20명 제한 cohort에서는 동의한 표본 수와 전송 손실을 보고하세요. 분석 거절/차단 요청을 게임 이탈 근거로 간주하지 마세요. 선택형 피드백 창구에는 수집 내용을 안내하고 자유입력은 텔레메트리에 넣지 않습니다.

## 12. itch.io 페이지 자료 준비도

| 자료 | 상태 | 있는 것 / 없는 것 |
|---|---|---|
| 게임 타이틀 | READY | `README.md`와 HTML title에 Color Gate Runner 표기. 페이지에는 Playtest v0.1 표기 |
| 별도 게임 로고 | OPTIONAL | 점검한 자산에서 바로 쓸 수 있도록 지정된 단독 로고를 찾지 못함. 이 테스트에는 텍스트 타이틀이면 충분 |
| 315:250 커버 / 권장 630×500 | NEEDS WORK | 해당 비율로 확정된 구성/출력 없음. 기존 UI/background art가 곧바로 usable cover인 것은 아님. 새 아트는 만들지 않음 |
| 게임플레이 스크린샷 3–5장 | NEEDS WORK | `Artifacts/VisualValidation/Step9C/`에 후보 있음: `03-Gameplay-TwoColor.png`, `04A-Runner-Red-Power.png`, `04C-Runner-Blue-Stable.png`, `04D-Runner-Green-Wing.png`, `05-Shield-Active.png`. 최종 선택은 v0.1 범위와 hosted 화면을 반영 |
| 짧은 설명 | NEEDS WORK | `Docs/GAME_DESIGN.md`에 한 줄 소개가 있으나 보유 아이템 강조는 비상업 경로에 맞지 않음; README에는 타이틀뿐 |
| 조작 안내 | NEEDS WORK | 포인터 입력은 구현됐으나 게시 가능한 완결 안내문 없음. 클릭/탭으로 플레이어 색을 바꾸고 Gate 통과 전 맞추도록 설명 |
| 플레이테스트 고지 | NEEDS WORK | 완성된 페이지 문구 없음. 초기 빌드, 예상 세션 길이, 테스트 브라우저 범위, 임시 로컬 저장을 설명 |
| 피드백 요청 | NEEDS WORK | 구체적인 플레이테스트 질문/창구 없음. 규칙 이해, 실패 이해, 계속하고 싶은지를 물되 telemetry에 입력 글을 수집하지 않음 |
| 개인정보/분석 안내 | NEEDS WORK | provider와 동의 결정 미완료. 실제 활성화 상태와 일치하는 문구 게시 |

기존 캡처 두 장을 직접 보았습니다. `03-Gameplay-TwoColor.png`에는 Stage 1, Gate 진행도, 현재/다음 색 strip이 있지만 명시적인 입력/맞추기 안내는 없습니다. 과거 그래픽 근거일 뿐 호스팅 승인 자료는 아닙니다. `10-Failed-Continue-Offer.png`에는 광고 제안과 크게 주입된 Coin 잔액이 보입니다. 현재 프로덕션의 광고 provider는 이용 불가이므로 이 fixture 캡처가 실제 광고가 있음을 뜻하지 않으며, 무수익화 테스트 홍보물로 부적절합니다. 내부 테스트 캡처를 검토 없이 재사용하지 마세요.

기존 캡처와 art 파일은 원본 자료로 유용합니다. 최종 커버, crop, 스크린샷은 실제 단기 release 기준으로 검수해야 합니다. 감사에서 홍보 아트를 만들거나 승인하지 않았습니다.

## 13. 기존 테스트 기준선

**상태: 점검한 회귀 근거 READY; 현재 WebGL 실행 NEEDS WORK.** `TEST_PLAN.md` Phase A/Tier 0은 무차별 전체 suite 재실행 대신 점검을 허용합니다. 이번 감사에서는 테스트, Builder, 시뮬레이션을 실행하지 않았으므로 이로 인해 생성 Scene/material이 바뀌지 않았습니다.

| 확인 항목 | 상태 | 관찰한 근거 |
|---|---|---|
| EditMode | READY | `Artifacts/Validation/EditMode-results.xml`: 총 480, 통과 480, 실패 0, skip 0; 종료 `2026-09-11T23:48:06Z` (한국 시각 9월 12일) |
| PlayMode | READY | `PlayMode-results.xml`: 총 266, 통과 265, 실패 0, skip 1; 종료 `2026-09-11T23:53:03Z`. 무시 항목은 문서화된 선택형 graphics capture이며 실패가 아님 |
| Compilation | READY | 이전 suite와 기준 handoff에 성공 기록; 새 compile은 수행하지 않았음 |
| Scene/build 구조 | READY | Build Settings 확인; 저장된 coverage에 `TestBuildMenuTests.EnabledBuildScenes_AreBootFrontendCampaign`, `WebGlPortraitTemplate_UsesExactNineBySixteenFrame`, Boot 테스트 있음 |
| Missing Script/reference/중복 | READY | 저장 baseline에서 관련 실패 없음. E1에 Boot/Frontend/Campaign 구조 검사 및 과거 Builder 2회 통과 기록. 새 Builder 검증은 수행하지 않음 |
| Campaign 결정론 | READY | 저장 artifact SHA-256 3개를 다시 계산해 기준과 일치 확인; 520행. 시뮬레이션 재실행은 안 함 |
| Step 10 결정론 | READY | SHA-256 다섯 개 재계산 후 기준과 일치 확인; 문서상 80행 / 64,016회. 재실행은 안 함 |
| Packages | READY | manifest/lock hash가 기준과 일치, 신규 차이 없음 |
| ProjectSettings | READY | 기존에 허용된 WebGL define만 추가. 나머지 내용 차이 없음. define 차이 때문에 파일 전체 hash는 다름 |
| 개발 저장 데이터 안전 | READY | Unity 실행/저장 데이터 수정 안 함. 과거 snapshot 유지; 현재 실사용 save hash/timestamp는 이번 감사에서 독립 검증하지 않아 NOT VERIFIED |
| 현재 브라우저 build smoke | NEEDS WORK | 로컬 파일만 있음; 출처, browser load, iframe 동작 NOT VERIFIED |

다시 계산한 hash:

```text
Campaign Summary  0C07357C04BDDDDBEFD35D621966967B33D0F68099F0D65ED8FFCAE7BF2347CA
Campaign JSON     E2B18725FE63BD7C93109C0EE9CC08777CC2BA814B05A4B38E61F9C6ADFB807B
Campaign CSV      5502E9A93461A49D28BD3040666733778A768F59B611FC61B7FDCBDB39F7AB84
Step10 CSV        04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04
Step10 JSON       5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00
Step10 Summary    611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27
Step10 Comparison 00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B
Step10 Shortlist  068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6
Package manifest  5E44864DB10C6A0A47806035C801D2271F6F48B0BF8BF6F699B88C71D6758C96
Package lock      FF4B3AC486B926719707FF2447580CEB1CE33D7C0851BABA229A3CD73C48CBBE
ProjectSettings   72C74AACD6534ACF72782988B0A484892024E30C1BEE2D7E9423D262C90194A5
```

마지막 hash는 관찰한 작업 파일 값이지 **새 승인 기준선이 아닙니다.** `CURRENT_STATUS.md`에는 `BFF9843B9363FF1C20B113108D623026E777311CAEF6372B07C92690049717BC`가 남아 있습니다. 허용된 define 차이가 다른 원인입니다. 관련 없는 ProjectSettings/Package 변경은 승인되지 않았습니다.

구현 후에는 entry/recovery/route observer 변경 시 Tier 2가 적절해 보입니다. 튜토리얼이 Core timing, Stage data, generation을 바꿀 때만 Tier 3로 올립니다. 생성 UI 변경은 문서화된 Builder 검증이 필요합니다. 테스트를 유지하세요. 특히 내부 빌드 메뉴 테스트는 Development 모드를 의도적으로 확인하므로 외부 빌드를 편하게 만들겠다는 이유로 기존 계약을 바꾸지 마세요. Browser/시각 검증과 독립 checker는 이후 승인 검증에 속하며 이번 감사에서 통과했다고 주장하지 않습니다.

## 14. 위험

| 위험 | 상태 | 영향 / 범위 제한 대응 |
|---|---|---|
| 초심자가 입력을 이해하기 전에 실패 | NEEDS WORK | 설명 없는 miss가 불공정하게 보일 수 있음. 최소 안내 후 개발자 설명 없이 관찰 |
| 8개 Stage가 연습 단계를 압축 | NEEDS WORK | 이름에 Intro가 있어도 중간 단계 생략 후 초보자에게 적절하다는 보장 없음. 순서/시간 pilot |
| 경제가 재시도 의향을 오염 | BLOCKER | Heart 대기/유료 복구 때문에 한 번 더 할 의지를 측정할 수 없음. 테스트 전 B2 해결 |
| 로컬 빌드가 오래됐거나 설정이 다름 | BLOCKER | 플레이 결과가 감사 소스에서 나온 것인지 알 수 없음. B1 출처 확인으로 해결 |
| 익명 분석 요구가 SDK 개인정보 보장 범위를 넘음 | NEEDS WORK | provider metadata/동의 결정. 사용자 지정 PII를 뺐다는 이유로 완전 익명을 주장하지 않음 |
| 이벤트 미전송 / 종료 시 손실 | BLOCKER | 로컬 CSV나 종료 시에만 보내는 이벤트는 funnel 질문에 답할 수 없음. 플레이 중 전달과 제한된 추정으로 B3 해결 |
| 초기 다운로드가 크거나 기기 메모리 압박 | NEEDS WORK | 로컬 추출 파일 188MB는 제한 이내지만 로드 이탈을 부를 수 있음. 최적화 전 cold 전송/시작 및 non-Development 압축 측정 |
| 브라우저 저장 이용 불가 | NEEDS WORK | 메모리 쓰기를 영속 저장으로 혼동하지 말 것. 적절하면 세션 내 저장만 허용할지 승인 |
| Graphics backend별 시각 의미 차이 | NEEDS WORK | Bloom, 투명도, 작은 엠블럼의 WebGL 실제 검사 필요. headless 통과는 가독성 증거가 아님 |
| 제품 범위가 상업화로 확대 | DEFER | 핵심 게임을 검증하는 데 Shop/광고/IAP/계정이 필요하지 않음 |

## 15. v0.1 필수 항목

| 요구사항 | 상태 | 출시 승인 기준 |
|---|---|---|
| 출처 추적 가능한 WebGL 후보 및 호스팅 실행 | BLOCKER | B1 해결; Boot 진입 확인, 치명 오류 없음, 최종 ZIP 검증 |
| 비상업적 반복 시도 | BLOCKER | B2 해결; Heart timer/통화/구매가 Retry를 막지 않음 |
| 최소 첫 플레이 가르침 | NEEDS WORK | 신규 테스터가 입력 방법과 Gate 통과 전 색 맞추기를 설명하고 첫 실패 후 도움 없이 복구 |
| 짧은 경로 승인 및 명시적 종료 | NEEDS WORK | 경로/접근/완료가 프로덕션 진행도에 새거나 Stage 생략 클리어를 요구하지 않음 |
| 이해 가능한 실패/Retry/다음 단계 | NEEDS WORK | 직접 복구와 자발적 진행. 사용 불가 action을 작동하는 것처럼 표시하지 않음 |
| 동의 후 유용한 이벤트 수신 | BLOCKER | B3 해결; 테스트 Dashboard/receiver가 호스팅 browser의 ready/input/fail/retry/complete 이벤트 중복 없이 받음 |
| 개인정보 선택/안내 | NEEDS WORK | 실제 provider 고지, 분석 없이 플레이, 철회/삭제 경로, 금지 데이터 검사 완료 |
| 지원 브라우저 범위 명시 | NEEDS WORK | 적어도 기본 데스크톱 browser에서 전체 경로 통과; 주장 범위를 테스트한 기기/browser로 제한 |
| 회귀 기준선 및 저장 안전 보존 | READY | 기존 기준 활용. 변경 후 해당 suite와 보호 상태 검사 필요 |
| 제한된 배포, 결제 없음 | NEEDS WORK | 담당자가 1단계 비공개/제한 접근을 10–20명용으로 설정; 게임 내 계정/결제 추가 없음 |
| 간결하고 사실에 맞는 페이지 | NEEDS WORK | 타이틀, 조작, 초기 테스트 설명, 브라우저 예상 범위, 피드백, 개인정보 link; 실제 범위에 맞는 screenshot |

## 16. v0.1 권장 항목

| 요구사항 | 상태 | 도움 되는 이유 |
|---|---|---|
| 로더 실패/도움 문구 개선 | NEEDS WORK | 시작 불가와 게임 이탈을 구분. 재로드/지원 browser 안내 |
| 적절한 커버 및 깨끗한 캡처 3–5장 | NEEDS WORK | 내부 fixture/개발자/경제 화면으로 테스터를 오도하지 않음 |
| 실물 Android/iOS 탐색 테스트 | NEEDS WORK | 세로 화면 게임에 유용하나 측정 전 지원 주장 보류 |
| 짧고 구조화된 피드백 질문 | NEEDS WORK | 이벤트 수치와 이해도/동기를 보완. 서술 입력을 telemetry에 기록하지 않음 |
| 첫 session과 재방문 구분 | NEEDS WORK | 기존 저장이 진입/Stage 선택을 바꿀 수 있음. 사용자 save를 지우지 않고 관찰 분리 |
| 최소 사운드 cue | OPTIONAL | 피드백을 강화할 수 있으나 무음에서도 시각 피드백이 충분해야 함 |
| 드문 session checkpoint | OPTIONAL | milestone만으로 미완료 세션 길이가 부족할 때 추정 보완 |

## 17. 시장 검증까지 보류

이 절의 항목은 모두 **DEFER** 상태입니다: 광고 및 보상형 광고, 실시간 IAP/store 연동, 계정 생성/외부 로그인, cloud save, 새 화폐/상업 진행 시스템, 라이브 운영, remote config, leaderboard/rank backend, collection 보상, 새 기믹, 추가 Campaign/mastery 블록, League 확장, 알림 전달, Android/iOS native store 출시, 프로덕션 오디오/BPM 시스템, 광범위한 태블릿/내장 브라우저 지원.

기존 상업/로컬 Product 코드를 자동으로 제거하라는 의미는 아닙니다. 현재 경계에서 제한적인 플레이테스트 설정을 우선 검토하세요. Save 초기화, transaction test 약화, 초기화 오류를 성공으로 꾸며내는 행위는 금지합니다. 이 문서에서 외부 데이터 기능으로 평가한 것은 향후 최소 analytics adapter 하나뿐이며 scope/개인정보 승인이 필요합니다.

## 18. 권장 다음 구현 범위

**상태: NEEDS WORK. Director 검토를 위한 제안이며 구현 승인이나 최종 구현 명세가 아닙니다.**

초기 WebGL 후보의 출처 추적과 검증을 일찍 수행하면서 **플레이테스트 진입, Stage 1 교육, 제한 없는 복구**에 범위를 한정할 것을 권고합니다. 기존 Boot, AppRoot, Campaign Scene, StageSession, Builder ownership을 사용합니다. 측정 가능한 결과는 외부 신규 테스터가 Stage 1에 진입해 의도적으로 색을 바꾸고, 성공/실패 한 번을 이해한 뒤 경제 마찰 없이 재시도하는 것입니다. Shop, 별도 튜토리얼 Scene, 새 제품 framework부터 시작하지 않습니다.

명세 전에 Director 결정이 필요합니다.

1. 제안한 8개 Stage 순서를 승인하고 Camouflage와 Hidden을 모두 첫 cohort에 포함할지 결정합니다.
2. 격리된 플레이테스트 시도/진행 정책을 승인합니다: 재시도 무제한, Stage 6 제공 Shield, 선택 인벤토리 제외, 생략 Stage에 대한 프로덕션 save 변경 금지.
3. 관찰 가능한 Stage 1 온보딩 완료 기준 및 첫 Gate/timing에 개입 허용 여부를 정합니다. 이번 분석은 밸런스 조정을 승인하지 않습니다.
4. 데스크톱 우선 지원을 승인하거나 초대 전 모바일 검증을 필수화합니다.
5. 동의 기반 pseudonymous UGS 수집이 목표 개인정보 제한에 맞는지 판단합니다. 아니라면 적합한 익명 대안을 정합니다. Services Core가 이미 있다는 이유로 provider를 설치하지 않습니다.

결정 후 파일/검증 단계를 명시한 소규모 구현 명세를 별도로 작성합니다. 먼저 재현 가능한 후보 빌드/호스팅 smoke test를 확립한 뒤 진입/복구/교육을 검증하고, 승인된 짧은 경로와 최소 event observer를 연결하며 15절 조건 충족 후 제한 cohort를 진행합니다. 소스/런타임 결과와 사람의 피드백으로 후속 작업을 정합니다. 이 감사에서는 구현, SDK 활성화, 업로드, 초대, commit을 수행하지 않았습니다.

아래 판정은 **플레이테스트 구현을 시작할 준비가 됐는지**에만 해당하며 게시 준비나 Game Director 검토를 건너뛸 허가가 아닙니다.

GO — itch.io Playtest v0.1 구현 진행 준비 완료
