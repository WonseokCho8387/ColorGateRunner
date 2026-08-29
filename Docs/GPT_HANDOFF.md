# Color Gate Runner — Codex Handoff

이 문서는 새 Codex 작업 창에서 Color Gate Runner를 바로 이어가기 위한
최신 인수인계 문서다. 구현 상태가 충돌하면 `CURRENT_STATUS.md`, 공통 작업
절차는 `AGENTS.md`, 회귀 검증은 `TEST_PLAN.md`를 우선한다.

## 1. 인계 기준

- 기준일: 2026-08-29
- 브랜치: `main`
- 최신 구현: `feat: add flicker campaign stages` 완료 커밋
- 완료 Iteration: **Iteration 46 — Flicker Campaign Block**
- Push: 하지 않음
- Unity: `6000.5.1f1`
- Campaign Catalog: revision 13, stable Stage 1–26
- 표준 검증 기준: EditMode `455/455`, PlayMode `250/250`

### 현재 작업 트리 주의

Iteration 46 완료 시 승인된 파일만 커밋하고 작업 트리는 clean이어야 한다.
Unity가 재질, `.utmp`, `ProjectSettings.asset` 또는 WebGL 생성 파일을 다시
쓰면 먼저 승인 범위인지 확인한다. package-managed WebGL define 이외의
ProjectSettings 차이는 기준선으로 받아들이지 않는다.

## 2. 반드시 먼저 읽을 문서

1. `AGENTS.md`
2. `Docs/PROJECT_CHARTER.md`
3. `Docs/CURRENT_STATUS.md`
4. `Docs/GAME_DESIGN.md`
5. `Docs/ART_DIRECTION.md`
6. `Docs/TEST_PLAN.md`
7. `Docs/DECISIONS.md`
8. `Docs/FRONTEND_FLOW.md`
9. `Docs/PRODUCT_SYSTEMS.md`
10. `Docs/LOOP.md`
11. `Docs/ITERATIONS.md`

`CURRENT_STATUS.md` 앞부분의 Authoritative Baseline이 역사 섹션보다
우선한다. 과거 Iteration의 Continue 가격, Continue 횟수 제한, 저장
스키마, Fog/Ice 표현은 현재 계약으로 사용하면 안 된다.

## 3. 현재 실제 제품 흐름

첫 실행:

```text
Boot
-> Account Choice
-> Guest
-> Frontend Lobby
-> PLAY
-> PreRun / Item Selection
-> Gameplay
-> Clear 또는 Failure Result
-> Next Stage / Retry / Continue / Lobby
```

재실행은 Boot 이후 Frontend Lobby로 직접 진입한다. Frontend에서 시작하면
Campaign 내부 Lobby를 우회하고 PreRun으로 간다. 직접 SampleScene 실행,
개발 치트 진입과 Experiment Lab은 기존 Campaign Lobby를 fallback으로
사용한다.

Clear의 `NEXT STAGE`는 다음 Stage PreRun으로 직접 이동한다. Failure
Continue는 Ticket, 실제 광고 가능 여부, Coin 순서다. Retry는 새 Attempt로
돌아가 Heart와 선택 아이템을 다시 승인한다.

## 4. 현재 구현 완료 범위

### Campaign과 플레이

- Stage 1–26, 결정론적 Gate 생성과 Retry
- 전체 Campaign Spline 이동, 좌우 곡선과 완만한 고저차
- Spline 기반 러너, 카메라, Gate, Goal, Fog, Ice와 도시 배치
- 카메라 회전 관성: half-life `0.3s`, yaw `24°`, pitch `16°`, roll 없음
- 러너 visual-only Spline 선행 조향과 하단 중앙 가로 색상 순서 UI
- Continue: `READY 0.5s -> GO`, GO부터 즉시 플레이
- 24개 고정 도시 풀, Goal까지 재활용
- 6개 Gate 풀과 파괴 연출 풀
- Gate와 하단 색상 UI가 공유하는 6개 투명 이미지 emblem
- 설명 텍스트 없이 emblem과 색상이 전기 차단처럼 점멸 후 사라지는 Hidden 표현
- 24-unit 접근 안전 잠금, `0.12s` 양색 판정과 좌→우 네온 wipe를 갖춘
  marker-free Flicker 표현
- Theme 1 러너, Gate, Goal, 도로, 도시 Blender/FBX 원본
- 거친 비발광 아스팔트, Cyan emissive edge와 Bloom
- 10-bank Fog, 고정-capacity wisp/rain과 Fog 구간 어두운 tone

### 아이템과 경제

- Shield와 Booster 제공 학습 Stage 6/7
- Stage 8+ 소유 아이템 선택과 Start 시 원자 소비
- 재고 0일 때 Shield/Booster 1개를 `900 Coins`로 즉시 구매
- Heart 5개, 30분 recharge, offline 회복, timed unlimited Hearts
- Continue Ticket
- Coin Continue: `900 -> 1900 -> 2900 -> 4900`, 이후 `4900` 반복
- Continue 총 횟수 제한 없음
- 실제 rewarded-ad provider가 있을 때 Attempt당 성공 1회 계약
- 현재 provider가 없으므로 광고 버튼은 의도적으로 숨김
- Normal/Hard/Very Hard 최초 보상 `100/200/500 Coins`
- 성공한 유한 Heart Attempt는 소비 Heart 1개를 원자 환급
- 2 Stage마다 자동 Lobby milestone과 별도 보상

### Frontend와 Lobby

- Boot, persistent AppRoot, Guest Profile와 Account Choice
- schema-3 Product Save, backup/recovery/migration
- Profile/Coin/Heart/Settings 상단 영역
- Theme/발전 진행 중심 영역
- 현재 Stage 카드와 단일 `PLAY` 하단 영역
- Shield/Booster 재고 chip
- Theme 1 `COLOR COURTYARD` 배경, reactor, ambient frame
- Theme당 6개, 총 18개 자동 milestone 데이터 구조
- Theme 1의 6개 energy node 활성화
- 공용 네온 UI skin, 9-slice surface와 의미 아이콘
- Editor Developer Console에서 Stage, Coin, 아이템, Heart 조정

### 플랫폼 기반

- Unity IAP `5.4.2` 설치
- Unity Services Core `1.18.0`
- Google Play billing mode
- Android application ID `com.wscho.colorgaterunner`
- Firebase Console 프로젝트는 생성됐지만 Unity SDK와 설정 파일은 없음
- Test Build 메뉴는 빌드 전 Catalog/Campaign/Frontend/Boot를 재생성하고
  이전 출력을 제거한다. Android Kotlin stdlib 충돌도 생성 Gradle 단계에서
  정렬하며 WebGL에서 시작한 통합 빌드에도 callback이 등록된다.
- 최신 실빌드: Android APK 68,774,305 bytes, WebGL 폴더 155,566,553
  bytes. 설치와 브라우저 인간 플레이는 아직 필요하다.

## 5. 기믹 구현 상태

| 기믹 | Core/테스트 | Experiment | Campaign | 현재 상태 |
|---|---|---|---|---|
| 색상 순환/일치 | 완료 | 완료 | Stage 1–26 | 핵심 규칙 완료 |
| Shield | 완료 | 완료 | Stage 6+, 선택 가능 | 비주얼·경제 연결 완료 |
| Booster | 완료 | 완료 | Stage 7+, 선택 가능 | 3D Warp와 카메라 연결 완료 |
| Camouflage | 완료 | 완료 | Stage 9–11 | 입문/연습/숙련 완료 |
| Fog | 완료 | 완료 | Stage 12–14 | Storm Fog 적용, 인간 비주얼 검증 필요 |
| Ice | 완료 | 완료 | Stage 15–17 | Spline ribbon과 재질 적용, 인간 비주얼 검증 필요 |
| Echo Provider | 완료 | 완료 | Stage 18–20 | Gate membrane/색 전달 적용, 인간 검증 필요 |
| Hidden | 완료 | 완료 | Stage 21–23 | 전 구간 7/9/11회·네온 차단 소거 표현, 인간 검증 필요 |
| Flicker | 완료 | 완료 | Stage 24–26 | 6/8/10회·거리 잠금·양색 wipe, 인간 검증 필요 |

### 구현되지 않은 기믹과 모드

- **Stage 27–36의 새 기믹**: 승인된 새 Gate Modifier가 없다. 먼저 기존
  여섯 Modifier의 재등장과 교대 조합으로 충분한지 인간 플레이로 판단해야
  한다. 같은 Gate에 여러 Modifier를 중첩하는 규칙도 아직 승인되지 않았다.
- **4–6색 영구 Campaign 진행**: 구현/배치되지 않았다. 현재 학습 곡선은
  2/3색 중심이다.
- **Banking, loop, inversion**: Campaign Spline에 없다. 현재는 world-up,
  no-roll 계약이다.
- **Race Mode**: 하나의 center Spline에서 3–5개 평행 경로와 non-collision
  Ghost AI를 만드는 방향만 승인 후보로 남아 있고 구현되지 않았다.
- **Endless Mode**: 재사용 가능한 과거 코드가 있지만 메인 제품 흐름에
  노출되지 않는다.
- **BPM 기반 Gate 배치**: 오디오와 함께 미구현이다.

현재 enum에 존재하는 Gate Modifier는 `Camouflage`, `Fog`, `Ice`,
`EchoProvider`, `Hidden`, `Flicker`뿐이다. 새 창에서 문서 근거 없이
새 기믹을 이미 계획된 기능처럼 취급하면 안 된다.

## 6. 비주얼 퀄업 잔여 작업

### 바로 인간 플레이로 확인할 항목

- Stage 12–14: Fog 밀도, 6개 bank 이음새, 커브 뒤가 과하게 보이는지,
  Gate 색상 판독 난이도
- Stage 15–17: 아스팔트/빙판 재질 대비, ribbon 이음새, 커브·경사 밀착,
  `2x` 속도에서 shimmer와 모바일 aliasing
- Stage 18–20: Echo Gate membrane 인지, Gate 색과 획득 Shield 색의 정확한
  전달, Bloom 과다 여부
- Stage 21–23: 6개 emblem 구분, Gate와의 조화, Hidden 기억 시간,
  Stage 20→21 학습 전환, 후반 반복과 3색 숙련 피로도
- Stage 24–26: 24-unit 잠금의 대응 여유, `0.12s` 좌→우 wipe 인지,
  전환 중 두 색 판정의 납득도와 6/8/10회 압박 곡선
- Stage 14–20: 카메라 관성, crest에서 다음 Gate 가시성, 멀미 가능성
- Shield/Booster: 기기별 투명 shader와 Warp 상·하단 밀도
- 도시: Goal까지 pop-in, 반복감, 트랙 침범 여부

### 아직 제작이 필요한 비주얼

- Theme 2와 Theme 3의 최종 Lobby 환경 그림 및 6개 발전 상태
- Theme 전환/해금 보상 연출과 최초 진입 연출
- PreRun, Clear, Failure 화면의 최종 icon hierarchy와 보상 애니메이션
- 최종 폰트/다국어/접근성 대응과 Safe Area 기기별 조정
- 실제 BGM, UI SFX, Gate/Shield/Booster/Goal SFX
- 추가 도시 modular art와 후반 Theme 변주
- Android 실기기 Bloom, 투명 overdraw와 성능 quality tier

Theme 1과 공용 UI skin은 더 이상 primitive-only graybox는 아니지만,
자동 테스트가 최종 시각 품질을 증명하지는 않는다.

## 7. Lobby와 제품 기능 잔여 작업

### Lobby/Home

- Theme 2/3 최종 아트와 milestone 연동
- Theme 해금 시 보상 표시, 새 Theme 소개와 전환 피드백
- 상단 Coin/Heart `+` 진입과 실제 Shop 연결
- Home/Shop용 persistent bottom navigation
- 작은 화면, notch, 긴 localized price/text 대응
- Profile/Guest 표시의 최종 UX와 Google action 정책

### Shop과 IAP

로컬 모델은 완료됐지만 실제 구매는 전혀 연결되지 않았다.

- 6개 Coin product ID:
  `coins_1000`, `coins_5000`, `coins_10000`, `coins_25000`,
  `coins_50000`, `coins_100000`
- 5개 bundle product ID:
  `bundle_starter`, `bundle_small`, `bundle_medium`, `bundle_large`,
  `bundle_xlarge`
- Starter는 로컬 account-limited이며 나머지는 반복 가능 모델
- 모든 현재 catalog entry는 Google Play consumable 모델

남은 구현:

1. Google Play Console 상품과 localized price metadata 구성
2. Unity IAP 초기화, 연결/복구/metadata 상태 모델
3. pending order 수신과 중복 delivery 방지
4. 검증된 order를 Product transaction ledger에 먼저 저장
5. 저장 성공 후에만 store order confirm
6. Firebase Functions + App Check 기반 영수증 검증 경계
7. Shop scroll page, special offer/bundle/coin card와 오류/재시도 UX
8. 네트워크 없음, 취소, pending, 중복 callback, save 실패 회귀 테스트

화면 가격은 reference image나 코드 상수로 하드코딩하지 않고 Store
metadata를 사용해야 한다.

### 광고

- 실제 rewarded-ad SDK/provider 선정 및 adapter 구현
- availability, load, show, success/cancel/fail/stale callback 처리
- Attempt당 성공 1회, Coin-first 시 광고 권리 유지 계약 보존
- provider가 unavailable이면 버튼을 계속 숨김
- 광고/개인정보 동의와 release configuration 확정

### 아직 없는 Lobby 목적지

- Leaderboard/competitive page
- Journey/progression page
- Collection page

초기 출시에서 필수로 만들 필요는 없다. Home과 Shop만 먼저 활성화하고,
나머지는 빈 기능처럼 보이지 않게 숨기거나 명확히 비활성화한다.

## 8. 기타 출시 전 잔여 작업

- 실제 Music/SFX AudioSource와 음원 연결
- 모바일 notification permission, scheduling과 delivery
- Terms, Privacy, Support 실제 HTTPS URL 확정
- Google/platform account와 cloud save는 아직 없음
- Firebase Analytics/Crashlytics와 Remote Config는 아직 없음
- Local/remote analytics와 개인정보 정책 결정
- Android signing, AAB, target device 성능/발열/메모리 검증
- 새 APK 설치와 WebGL HTTP browser 실행, touch/focus/audio/resize 회귀
- 저장 migration, offline clock, 결제 pending/재전달과 복구 QA
- 36 Stage 전 구간의 실제 플레이 난이도/피로도/반복감 검증
- Store/광고/개인정보 관련 release checklist와 운영 문서

## 9. 권장 제작 순서

### P0 — 현재 플레이 빌드 검증

필요할 때 Test Build 메뉴로 Android APK와 WebGL을 새로 생성해 Stage
12–26을 인간 플레이한다. Fog, Ice, Echo, 카메라, 색상 UI, Hidden 기억
난이도와 Flicker 거리 잠금/wipe를 확인하고 결함과 취향 조정을 분리한다.

### P1 — Stage 21–26 인간 플레이 조정

- Hidden 기억 압박과 Flicker 접근 안전 잠금/wipe를 모바일과 WebGL에서
  확인한다.
- 자동 검증으로 판단할 수 없는 가독성, 피로도, Bloom과 체감 속도만
  근거로 최소한의 밸런스/표현 조정 Iteration을 승인한다.

### P2 — Stage 27–36 Act 3 설계와 제작

먼저 21–26 인간 플레이 결과를 본다. 권장 초안은 새 Modifier를 바로
추가하는 것이 아니라 기존 기믹을 Gate 단위로 교대 재사용하는 것이다.

- 27–29: Camouflage/Fog/Ice 재숙련
- 30–32: Echo/Hidden/Flicker 재숙련
- 33–35: 서로 다른 Gate 구간의 기믹 교대와 압축
- 36: Campaign finale

이 배치는 아직 승인된 Catalog 계약이 아니다. 같은 Gate의 Modifier 중첩,
4색 이상, 새 기믹은 별도 인간 승인 후 진행한다.

### P3 — Lobby 완성

- Theme 2/3 아트와 18 milestone 완성
- 해금/보상/Theme 전환 연출
- Home/Shop navigation shell
- PreRun/Result 최종 비주얼과 오디오

### P4 — 실제 수익화 연결

- IAP provider foundation과 Firebase 검증
- Shop UI
- rewarded-ad provider
- pending/recovery/offline/save-failure QA

실제 결제와 광고는 병렬로 한 번에 붙이지 말고 각각 독립 Iteration으로
완료한다.

### P5 — Launch hardening

- 전체 36 Stage 플레이/밸런스
- Android/WebGL 실기기와 성능
- 법률/지원 URL, 개인정보와 플랫폼 설정
- 오디오, notification, crash/analytics 정책
- release build, save migration과 commerce recovery 검증

Race Mode, Leaderboard, Journey와 Collection은 기본 Campaign/Shop 출시
기준선을 통과한 뒤의 확장 기능으로 둔다.

## 10. 유지해야 할 핵심 계약

- Gameplay Core는 Unity 비의존 구조를 유지한다.
- 모든 gameplay random은 명시적 seed를 사용한다.
- Campaign scalar distance와 Spline pose가 유일한 공간 권위자다.
- 현재 Stage 1–26 stable ID와 저장 기록을 재설계 과정에서 지우지 않는다.
- Product Save schema 3와 transaction ledger가 경제 권위자다.
- AppRoot, EventSystem, Scene destination과 저장 권위자를 중복 생성하지 않는다.
- unavailable provider는 성공을 흉내 내지 않고 action을 숨긴다.
- 새 Package/ProjectSettings 변경은 별도 승인 없이 추가하지 않는다.
- Builder 소유 구조는 Builder를 수정하고 두 번 재생성한다.
- 자동화는 fun, readability, visual quality를 증명하지 않는다.
- 작업 완료 시 explicit file staging만 사용하고 Push하지 않는다.

## 11. 현재 결정론적 기준

Campaign 520 rows:

- Summary: `BDCAAFE843F1FB81BE8569BE8F01424ABD8FD4CEFED56F89C5889EB4892A4EB4`
- JSON: `748AF7A9D8368125E9AFF1943DC4B6A225E9F48E72A043507A7049FECBE85F7A`
- CSV: `627B5B4A4CCD8DBB2C3942343550118F2EE8E141BD683597FF14F1A0F3098C73`

Step 10, 80 rows / 64,016 runs:

- CSV: `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`
- JSON: `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`
- Summary: `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`
- Comparison: `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`
- Shortlist: `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`

## 12. 새 Codex 창의 첫 작업

새 창에서는 다음 순서로 시작한다.

1. 이 문서와 `AGENTS.md`, `CURRENT_STATUS.md`를 읽는다.
2. `git status --short`로 위의 Editor 생성 차이를 재확인한다.
3. 사용자의 최신 인간 플레이 피드백을 받는다.
4. Stage 21–26 인간 플레이 피드백이 있으면 Hidden과 Flicker 결함/취향을
   분리해 다음 조정 Iteration의 Phase A를 수행한다.
5. 피드백이 없으면 Stage 27–36 Act 3의 기존 기믹 교대 재사용안을 먼저
   설계하고, 새 Modifier나 같은 Gate 중첩은 별도 결정으로 남긴다.
6. Phase A 보고 후 승인 전에는 구현하지 않는다.

첫 요청 예시:

```text
Color Gate Runner 프로젝트를 Docs/GPT_HANDOFF.md 기준으로 이어간다.
AGENTS.md의 절차와 CURRENT_STATUS.md의 기준선을 사용한다.

먼저 현재 작업 트리와 Iteration 46 구현 상태를 확인하고,
Stage 21–26 인간 플레이 결과를 바탕으로 다음 조정 또는 Stage 27–36
설계 Iteration의 Phase A만 진행한다. 보고 후 중단하고 승인 전에는 코드를
수정하지 않는다.
```
