# Color Gate Runner — Codex Handoff

이 문서는 2026-09-12에 Git, 코드, 생성 Scene, Stage Catalog, 저장 파일,
기존 Player 산출물과 새 자동 검증 결과를 직접 대조해 만든 인수인계 스냅샷이다.
현재 수치와 승인 상태는 `CURRENT_STATUS.md`, 절차는 `AGENTS.md`, 검증 범위는
`TEST_PLAN.md`를 우선한다. 뒤쪽 문서의 과거 Iteration 기록은 현재 상태를
덮어쓰지 않는다.

## 1. Git과 환경 기준

- 브랜치: `main`
- 구현 기준 HEAD: `d937b3a242b04df6d211de7dfe92abfca82175e3`
- 구현 기준 커밋: `feat: refine lobby shell and page visuals`
- Unity: `6000.5.1f1`
- 프로젝트 사본: `D:\AIPrototype\ColorGateRunner\ColorGateRunner`
- 검증 backend: Unity batch/headless
- 스냅샷 문서 커밋은 구현 기준 HEAD보다 새롭지만
  `Docs/CURRENT_STATUS.md`와 이 파일만 변경한다.
- 최초 검사에서 untracked 파일은 없었다. Git은
  `ProjectSettings/ProjectSettings.asset`을 working-copy normalization 때문에
  modified로 표시했지만 working bytes와 HEAD blob은 동일했고 승인된 SHA-256
  `BFF9843B9363FF1C20B113108D623026E777311CAEF6372B07C92690049717BC`도
  일치했다.
- Unity 프로세스 없이 `Temp/UnityLockfile`만 남아 있었다. 검증을 막던 이
  stale 임시 잠금만 제거했으며 프로젝트 파일은 변경하지 않았다.
- Push하지 않았다.

## 2. 실제 실행 흐름

```text
Boot
-> 첫 실행이면 Account Choice -> Guest
-> Frontend Lobby/Home
-> PLAY
-> Campaign PreRun / Item Selection
-> Gameplay
-> Clear Celebration -> Rewards
   또는 Failure -> Continue / Give Up confirmation / Consequence / Retry or Lobby
```

재실행은 Boot에서 저장을 복구한 뒤 Frontend Lobby로 직접 간다. Frontend
출발은 Campaign 내부 Lobby를 우회하고 PreRun으로 이동한다. SampleScene
직접 실행, Developer Console과 Experiment Lab 진입은 Campaign Lobby를
fallback으로 사용한다. 하나의 Boot 생성 `DontDestroyOnLoad` AppRoot가
Profile, Save, Settings, Progression, Economy, Continue와 Scene navigation
서비스를 소유한다.

## 3. 구현된 Gameplay 시스템

- Unity 비의존 Core의 명시적 seed 기반 결정론적 Gate/Stage 생성, Retry,
  목표 진행, 난이도와 speed profile
- Red/Blue/Green 중심의 2색·3색 Campaign; enum과 공용 시각 자산은
  Yellow/Purple/Cyan까지 6색을 지원하지만 현재 Campaign에는 4–6색 진행이
  없다.
- Shield, Booster, Camouflage, Fog, Ice, Echo Provider, Hidden, Flicker
- 모든 26개 Campaign Stage의 단일 scalar-distance Spline 이동 권위:
  runner, camera, gate, Goal, Fog, Ice와 도시 배치가 같은 pose를 소비
- 카메라 `0.3s` half-life, `24°` yaw / `16°` pitch 제한, world-up/no-roll;
  runner visual child의 `10°` yaw / `6°` lean 선행 조향
- 6 Gate pool, 고정 gate-break pool, 50 Ice ribbon panel, 24 city body/glow,
  10 Fog bank, 24 wisp/96 rain, 6 Echo membrane의 고정 용량 표현
- Fog의 구간 어두운 tone/비/안개, Ice의 Spline runway와 속도 규칙,
  Echo 색 전달, Hidden 네온 power-loss, Flicker frame/emblem dissolve
- Flicker 접근 안전 잠금 `24` units, `0.24s` 양색 통과 판정, 추격 화면 기준
  왼쪽 기둥 아래→위 / 상단 왼쪽→오른쪽 / 오른쪽 기둥 위→아래 전환
- 하단 중앙 가로 색상 순서 HUD, Pause, quick Continue, Goal/result 흐름
- Campaign Clear의 `2.35s` victory hold, `0.25s` reward crossfade, 최초
  `0.1s` 뒤 skip, 36개 고정 sprite streak의 세 차례 축포
- 22-part modular Runner: Red power kit, Blue guard kit, Green wing kit의
  `0.24s` fold/deploy 변신. 논리 색상과 판정은 탭 순간 즉시 바뀐다.
- 별도 Experiment Lab과 Spline Track Lab. 과거 Clone 실험은 폐기되고
  Echo가 대체했으며 현재 Modifier enum에는 Clone이 없다.

## 4. 구현된 Campaign Stage

Catalog는 revision `16`, stable ID `stage-01`부터 `stage-26`까지 정확히
26개다.

| Stage | 구간 | Gate 수 / 색 | 기믹 | 난이도·특기 |
|---|---|---:|---|---|
| 1–5 | 기본 색상 학습 | 24–36 / 2–3색 | 없음 | 기본 순환과 3색 도입 |
| 6 | Shield Training | 38 / 2색 | Shield 지급 | 아이템 선택 잠금 |
| 7 | Booster Timing | 40 / 2색 | Booster 지급 | 아이템 선택 잠금 |
| 8 | Item Application | 40 / 3색 | 소유 아이템 | Shield/Booster 선택 |
| 9–11 | Camouflage | 38/42/46 / 2–3색 | Camouflage | 11 Hard |
| 12–14 | Fog | 40/44/48 / 2–3색 | Fog | 14 Hard |
| 15–17 | Ice | 42/46/50 / 2–3색 | Ice | 17 Hard |
| 18–20 | Echo | 42/46/50 / 2–3색 | Echo Provider | 20 Very Hard |
| 21–23 | Hidden | 42/46/50 / 2–3색 | Hidden 7/9/11회 | 23 Hard |
| 24–26 | Flicker | 44/48/52 / 2–3색 | Flicker 6/8/10회 | 주기 1.35/1.17/0.99s, 26 Hard |

Stages 27–36은 Journey에 미래 milestone/Coming Soon으로만 보이며 Catalog나
실제 플레이 Stage로 구현되지 않았다. 동일 Gate에 여러 Modifier를 겹치는
규칙도 승인되지 않았다.

## 5. Frontend, Product, Save와 Boot

### Frontend

- 페이지: Account Choice, Shop, Rank, Home/Lobby, Journey, Collection
- 한 페이지 단위 가로 pager 순서: Shop → Rank → Home → Journey → Collection
- Shop/Journey의 세로 ScrollRect와 가로 swipe arbitration, 모든 비-Home
  Back의 Home 복귀
- 상단 Profile/Coin/Heart/Settings와 authored bottom dock/selected cursor
- 세 Lobby chapter/theme: Color Courtyard, Neon Garden, Sky Festival.
  milestone `0 / 6 / 12`에서 해금되고 선택 theme를 저장한다.
- Home은 테마 이미지, progress, 현재 Stage/mechanic, 단일 PLAY를 우선한다.
  Shield/Booster와 pending milestone read model은 inactive compatibility root에
  남아 있고 화면에는 표시하지 않는다.
- Journey는 짝수 Stage 보상 18개, chapter 선택, 마지막 live Stage 뒤 League
  terminal을 표시한다. 모든 현재 Catalog Stage를 클리어하면 stable shuffle
  bag으로 authored Stage를 무제한 재생하고, 새 미클리어 Stage가 추가되면
  정상 진행으로 돌아간다.
- Shop은 로컬 catalog 11개를 pouch/box/chest/cart 볼륨 이미지와 수량으로
  보여 주는 read-only preview다. 모든 action은 `STORE OFFLINE`이다.
- Rank와 Collection은 각각 고유 배경을 가진 navigable `COMING SOON` 화면이며
  가짜 데이터나 backend는 없다.

### Product와 저장

- Product save schema `3`: Profile, Settings, stable-ID CampaignProgress,
  Economy, LobbyProgress, revision/write metadata
- primary/temp/backup validation, atomic/recoverable write, schema migration,
  corrupt-primary recovery와 future-schema safe failure
- 현재 실제 Editor save: revision `742`, Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, highest unlocked `stage-05`, 4 records,
  45,900 Coins, Shield 8, Booster 8, Heart 4, Continue Ticket 0, selected theme
  empty. SHA-256
  `936ADE5FDE839A4F140001310FE82A5CAB70532D498350CEAADF36AAF130C75F`.
- 새 검증 전후 저장 bytes와 timestamp가 동일했다.
- Heart 5 capacity, 30분 recharge, offline 회복, timed unlimited Hearts
- Stage 시작의 Heart/소유 아이템 원자 소비, 성공 시 유한 Heart 환급
- Shield/Booster 0 stock quick-buy `900 Coins`
- Continue Ticket → 실제 rewarded-ad 권리 → Coin 순서. 현재 광고 provider가
  없어 광고 action은 숨긴다. Coin 비용 `900/1900/2900/4900`, 이후 4900 반복
- 최초 clear 보상 Normal/Hard/Very Hard `100/200/500 Coins`
- 짝수 Stage milestone의 idempotent 자동 보상과 transaction ledger
- 로컬 catalog: Coin pack 6개, bundle 5개. Starter만 account-limited
- Settings: Notifications, Music, SFX, Vibration, configured links. 실제 content
  BGM/SFX와 notification delivery는 아직 없다.
- Editor-only Developer Console: Stage launch/unlock, Campaign/Economy 독립
  reset, 재화·아이템·Heart·unlimited Hearts 설정

## 6. Scene과 Build Settings

현재 Scene 파일은 다음 세 개이며 Build Settings에서 모두 enabled/unique다.

1. `Assets/Scenes/Boot.unity`
2. `Assets/Scenes/Frontend.unity`
3. `Assets/Scenes/SampleScene.unity`

Boot는 serialized destination으로 Frontend를 연다. Frontend에는 AppRoot를
복제하지 않고 Boot의 persistent graph를 사용한다. SampleScene은 Campaign과
Experiment Lab을 포함한다.

## 7. 자동 검증 상태

2026-09-12 KST, 구현 HEAD에서 Quality Graph batch/headless preflight 후
`Tools/Validate.ps1`을 새로 실행했다.

- EditMode: `480/480` passed, failed 0, skipped 0
- PlayMode: `265/265` available passed, failed 0. 전체 discovered 266 중
  `LobbyPagesVisualCapture_WritesPortraitEvidence` 1개는 graphics opt-in이라
  headless suite에서 의도적으로 ignored
- Missing Script, Missing Reference, duplicate generated root 관련 실패 없음
- 검증 중 Unity가 WarpCyan/WarpGold를 render queue 3000으로 다시 직렬화한
  transient 차이는 승인 기준 3010으로 복원했다.
- `git diff --check`, package/project hash와 사용자 save 보존을 별도 확인했다.
- 이 문서-only snapshot은 gameplay/Stage/generation 입력을 바꾸지 않으므로
  Campaign/Step 10 시뮬레이션을 재생성하지 않았다. 현재 승인 기준은:
  - Campaign 520 rows: Summary
    `0C07357C04BDDDDBEFD35D621966967B33D0F68099F0D65ED8FFCAE7BF2347CA`,
    JSON `E2B18725FE63BD7C93109C0EE9CC08777CC2BA814B05A4B38E61F9C6ADFB807B`,
    CSV `5502E9A93461A49D28BD3040666733778A768F59B611FC61B7FDCBDB39F7AB84`
  - Step 10 80 rows / 64,016 runs: CSV
    `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`,
    JSON `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`,
    Summary `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`,
    Comparison `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`,
    Shortlist `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`
- 최신 graphics-capable evidence는 Iteration 60 Frontend D3D11 1/1,
  1080×1920 중심 14 captures(720×1280 Home 포함)다. Campaign의 최신 rendered
  evidence는 Iteration 59 D3D11 1/1, 19 captures다. 이번 headless 실행은 새
  visual evidence가 아니다.

## 8. Android와 WebGL 빌드 상태

Test Build 메뉴와 Android/WebGL build automation은 구현되어 있다. 빌드 전
Catalog, Campaign, Frontend, Boot를 재생성하고 이전 test output을 제거한다.

- Android APK 존재: `Builds/Test/Android/ColorGateRunner.apk`,
  68,774,305 bytes, `2026-08-28T10:26:16Z`, SHA-256
  `885E3CC8FE7693590AA3CF8DC543A488E3D53F48A804C786EFE73CB6109BFA91`
- WebGL 존재: `Builds/Test/WebGL`, 6 files / 155,566,553 bytes, newest
  `2026-08-28T10:29:18Z`; `index.html` SHA-256
  `2B3ED6A0BA943D4FDF32606ED4ABAE1CBB252F5DCBF4A322F96A9B3204B78988`

두 산출물은 Iteration 45 시점이며 Iterations 46–60의 Flicker 수정, 결과
연출, pager/Journey/League, modular runner와 Lobby/UI 개선을 포함하지 않는다.
따라서 파일은 존재하고 당시 빌드는 성공했지만 **현재 HEAD의 모바일/WebGL
빌드 상태로 간주할 수 없다**. 현재 HEAD APK/AAB/WebGL 생성, Android 설치,
HTTP WebGL 실행은 미검증이다.

## 9. 알려진 버그와 리스크

- 새 자동 suite에서 재현된 현재 code defect는 없다.
- 과거 Clone 실패, Stage 26 무게이트, Flicker 좌우/주기/Booster 제한,
  전환 후 원래 색 gate-break, Continue pool exhaustion, Lobby overflow는 이미
  수정된 역사이며 현재 버그 목록에 넣지 않는다.
- 가장 큰 현재 리스크는 Iteration 60 코드와 일치하는 Player build가 없다는
  점이다. 플랫폼 시작·입력·safe area·성능 결함은 아직 발견되지 않은 것이지
  없다고 증명된 것이 아니다.
- Unity batch validation이 Warp 재질 render queue를 일시적으로 3010에서
  3000으로 직렬화하는 도구 부작용이 반복된다. 이번에도 복원했으며 runtime
  feature bug는 아니지만 매 검증 후 Git diff 확인이 필요하다.

## 10. Deferred 시스템

- 실제 Unity IAP 초기화, provider price, Google Play 상품, pending order,
  receipt 검증, save-before-confirm, 구매 복구
- Firebase Unity SDK/설정, Functions/App Check 영수증 경계
- 실제 rewarded-ad provider와 consent/privacy 흐름
- Google/Apple 계정, cloud save와 conflict resolution
- 실제 leaderboard/rank backend와 collection 데이터/보상 시스템
- analytics/crash provider, remote config, 운영 dashboard
- production BGM/SFX, mobile notification permission/scheduling/delivery
- Terms/Privacy/Support 최종 URL, Android signing/AAB/release pipeline
- Campaign Stages 27+, 4–6색 진행, 복합 Modifier, banking/loop/inversion,
  Race Mode, BPM 기반 배치

Rank/Collection Coming Soon, disabled Shop action과 숨겨진 ad action은 unavailable
기능을 성공처럼 보이지 않게 하는 승인된 현재 표현이며 버그가 아니다.

## 11. 자동으로 판단할 수 없는 Human Playtest

- Android와 HTTP WebGL의 startup, touch/focus/audio, notch/safe area, resize,
  pause/resume, 발열·메모리·프레임 유지
- Fog 밀도와 Gate 판독, Ice shimmer/미끄러짐, Echo 획득 이해,
  Hidden 기억 난이도, Flicker 전환 방향·24-unit 안전 잠금·양색 판정의 납득도
- 곡선 카메라 추종과 멀미, 빠른 구간 runner steering, 도시 pop-in/반복감
- Red/Blue/Green 형태 차이와 변신 역동성, 색 변경 반응의 즉시성
- 승리 hold/축포/skip 감정적 타이밍과 reward 전환
- Home 정보 위계와 긴 문자열, Shop 패키지 볼륨 인지, Journey chapter/theme
  선택과 League 진입 이해, Rank/Collection Coming Soon의 기대 관리
- Stage 1–26 난이도 곡선, 아이템 가치, Heart/Continue 비용의 체감 공정성

자동화 결과를 재미, 공정성, 가독성, 만족도나 최종 visual quality의 증거로
사용하면 안 된다.

## 12. 문서와 코드의 불일치

- 기존 `GPT_HANDOFF.md`는 Iteration 50, EditMode 457/PlayMode 255,
  Theme 2/3 미완성, persistent pager/Shop/Journey/Rank/Collection 미구현으로
  적혀 있었지만 실제 코드는 Iteration 60과 480/266이며 모두 위 상태로
  진행됐다. 이 파일에서 교정했다.
- `CURRENT_STATUS.md`에는 revision 740/Heart 5의 과거 save와 현재 schema-3
  snapshot이 혼재했고 Theme 2/3 fallback, visible inventory chip 같은 낡은
  문장이 남아 있었다. Authoritative Baseline을 실제 값으로 교정하고 과거
  evidence를 명시적으로 역사로 표시했다.
- `FRONTEND_FLOW.md` 상단 Status는 Journey, 추가 theme, Rank, Collection이
  pending/disabled라고 쓰지만 같은 문서의 Iteration 54–60 섹션과 실제
  Builder는 Journey/세 theme/Coming Soon pages가 구현됐음을 보여 준다.
- `TEST_PLAN.md`의 앞쪽 active acceptance map과 PlayerPrefs key 범위 일부는
  오래된 Catalog revision/Stage 범위를 가리킨다. 뒤쪽 Iteration 44–60
  acceptance와 현재 코드/Catalog revision 16이 실제 상태다.
- `GAME_DESIGN.md`, `DECISIONS.md`, `ITERATIONS.md`의 Clone, 이전 Continue,
  이전 저장 schema와 Stage 계획은 역사/폐기 기록이다. Authoritative Baseline과
  명시적인 supersession 없이 현재 계약으로 가져오면 안 된다.
- 사용자 요청에 따라 이번 변경 범위는 `CURRENT_STATUS.md`와 이 파일뿐이므로
  위 다른 문서의 stale top-level 문장은 발견 사항으로 남기고 수정하지 않았다.

## 13. 다음 권장 결정

새 기능을 시작하기 전에 현재 구현 HEAD에서 Android와 WebGL test player를
새로 만들고, 위 Human Playtest 항목 중 startup/input/safe area/performance와
Stages 12–26 및 Lobby/Journey/result 핵심 동선을 집중 검증할지 결정한다.
그 증거가 나온 뒤에 Stages 27+ 콘텐츠와 실제 commerce 중 어느 제품 축을
먼저 승인할지 선택하는 것이 안전하다. 이 인수인계 작업에서는 그 다음
iteration을 시작하지 않는다.
