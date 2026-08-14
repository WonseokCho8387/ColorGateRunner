# Color Gate Runner — GPT Handoff

## Iteration 25 completed update

- Starting HEAD was `a8f7892`; completion commit name is
  `feat: add timed fog curtain`.
- Campaign Stages 12–14 now trigger one world-space Fog curtain when their
  first Fog gate becomes the next judgment. It follows at 1.5 seconds of
  effective travel distance, holds Alpha 1 for 1.5 seconds and fades for 0.5.
- Campaign gates keep their authored color and ordinary judgment behind the
  curtain. The same attempt cannot retrigger it; Pause and Continue countdown
  freeze it, Continue repositions it after respawn, and Retry resets it.
- Experiment Lab keeps the historical nearest-two Fog comparison.
- Campaign Builder passed twice, EditMode `421/421`, PlayMode `225/225`.
  Campaign and Step 10 hashes remain the Iteration 23 baseline because no
  deterministic mechanical input changed. Product save and backup remained
  byte- and timestamp-identical.
- Human visual review remains required. Ice approach/speed and Lobby layout
  are the next separate product candidates.

## Iteration 24 completed update

- Starting HEAD was `61147cd`; the completion commit is named
  `fix: recycle failed gates after continue`.
- The Stage 11 late-run stall was a fixed gate-pool exhaustion bug, not a Fog
  side effect. Continue deactivated each failed slot permanently; unlimited
  Continue could therefore exhaust all six visible slots and leave Goal
  completion unreachable.
- Continue now recycles the failed slot with the next authored gate. A focused
  regression Continues more than six times after the late Stage 11 sequence,
  resolves all 46 gates and clears after crossing Goal.
- Focused PlayMode passed `1/1`, full EditMode `418/418`, and full PlayMode
  `223/223`. Scene/Builder, Campaign data, Core rules, Product persistence and
  Step 10 inputs were unchanged, so their Iteration 23 baselines remain valid.

## Iteration 23 completed update

- Implementation base was `7858803`; the completion commit is named
  `feat: refine continue and clear economy flow`.
- Continue is unlimited per attempt. Coin prices are `900 -> 1,900 -> 2,900
  -> 4,900`, and every later Coin Continue repeats `4,900`. Retry resets the
  price ordinal. Ticket and rewarded-ad Continues do not advance it.
- The one successful rewarded-ad right per attempt remains, but no release
  provider is connected. The unavailable action stays hidden and never fakes
  success.
- Stage difficulty is now authored data: Stages 11, 14, and 17 are Hard;
  Stage 20 is VeryHard; all other current Stages are Normal. First-clear base
  rewards are `100 / 200 / 500` Coins, while existing Lobby milestone rewards
  remain separate and idempotent. Clear Result displays the two sources
  separately.
- Normal Start still spends one stored Heart atomically. A clear atomically
  records progression and reward and refunds exactly the Heart actually spent
  by that attempt. Unlimited-Heart and provided/free starts cannot request a
  refund.
- Clear Result removes Campaign Replay. `NEXT STAGE` opens the next Stage's
  PreRun selection directly; the final authored Stage exposes Lobby only.
- Continue countdown now synchronizes gate modifiers immediately and removes
  expired item/buff presentation before the countdown is visible.
- EditMode passed `418/418`, PlayMode passed `222/222`, and Campaign Builder
  passed twice. Campaign CSV/JSON/Summary SHA-256 are
  `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`,
  `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`,
  and `698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`.
  Step 10 remains unchanged.
- Lobby visual redesign, timed Fog veil, preplaced/rebalanced Ice, Shop/IAP
  runtime, and real rewarded-ad integration remain deferred.

## Iteration 22 completed update

- Unity now has `Tools > Color Gate Runner > Developer Console` for authored
  Stage 1-20 cheat launch, unlock-through, Campaign reset and exact Coins,
  Shield, Booster, Continue Ticket, Heart and unlimited-Heart configuration.
- Edit Mode changes persist before Play. Play Mode uses the active Product
  session and refreshes visible Frontend/PreRun/Failure values. Cheat Stage
  launch does not alter progression.
- Full EditMode passed `414/414`; full PlayMode passed `218/218`. No Scene,
  gameplay, Catalog or deterministic input changed, so Builder/Campaign/Step10
  baselines remain unchanged.
- Store initialization, receipt/server validation, Shop UI, Firebase runtime,
  real rewarded-ad provider and Stages 21-36 remain pending.

## Iteration 21 completed update

- Local implementation now contains schema-3 Hearts, timed unlimited Hearts,
  Continue Tickets, exact approved commerce products and idempotent reward
  grants. Stage start authorizes Heart plus selected items atomically.
- Failure hierarchy is Ticket -> available rewarded ad -> Coin. Actual Unity
  IAP store initialization, receipt validation, Shop/Lobby redesign and
  Firebase Functions/App Check are still not implemented.
- Campaign Builder passed twice, full EditMode passed `408/408`, and full
  post-Builder PlayMode passed `218/218`. The actual Editor Product save stayed
  byte-, timestamp- and hash-exact at schema 2 revision 29; schema-3 migration
  was validated only with isolated saves.
- Campaign and Step 10 hashes remain the previously approved values because
  gameplay Core, Stage data and deterministic simulation inputs did not
  change. Iteration 21 is the current authoritative implementation baseline.

## Iteration 20 prior update (historical snapshot)

- Current completion base is `13afabb`; the new completion commit is named
  `chore: establish monetization platform baseline`.
- Unity IAP `5.4.2` is installed for standard Google Play billing. Android
  application ID is `com.wscho.colorgaterunner`.
- Runtime purchasing is not implemented. There is no Store connection,
  product catalog, receipt grant, Firebase Unity SDK/config or Shop page yet.
- Approved Coin amounts: 1,000 / 5,000 / 10,000 / 25,000 / 50,000 / 100,000.
- Approved bundle intent:
  - Starter: Shield 1, Booster 1, Continue 1, unlimited Hearts 30 minutes.
  - Small: Shield 2, Booster 2, Coins 500, unlimited Hearts 1 hour.
  - Medium: Shield 4, Booster 4, Coins 1,000, unlimited Hearts 3 hours.
  - Large: Shield 10, Booster 10, Coins 5,000, unlimited Hearts 6 hours.
  - Extra Large: Shield 13, Booster 13, Continue 3, Coins 10,000, unlimited
    Hearts 12 hours.
- Hearts, timed unlimited Heart state and owned Continue inventory do not yet
  exist. Product IDs, prices and bundle repeatability also remain undecided.
- Firebase phase one is Functions + App Check; Analytics and Crashlytics are
  deferred.
- Lobby reference direction: persistent top wallet/Heart/Settings, dominant
  themed Home scene, persistent bottom navigation, scrollable Shop cards,
  future Journey path and Collection grid. Use original neon sci-fi art, not
  copied third-party characters or branding.
- Final validation: EditMode `400/400`, PlayMode `215/215`; Campaign and Step
  10 hashes remain unchanged because gameplay and simulation inputs did not
  change.
- This section records the prior Iteration 20 state; Iteration 21 above
  supersedes its missing-model statements.

이 문서에 적힌 커밋, 완료 기능, 남은 작업은 인수인계 시점의 요약이다.
저장소의 CURRENT_STATUS.md와 충돌하면 CURRENT_STATUS.md를 우선한다.

## 1. GPT의 역할

GPT는 Game Director 및 Product/UX 설계자로 행동한다.

- 직접 구현하지 않는다.
- 사용자 플레이 피드백을 분석한다.
- 다음 Iteration의 범위와 설계 계약을 정한다.
- Codex가 실행할 짧은 프롬프트를 작성한다.
- 기존 문서와 구현 계약을 존중한다.
- 확인되지 않은 구현 상태를 추측하지 않는다.

## 2. 프로젝트

- Unity 기반 모바일 Portrait 게임
- 핵심 플레이: 플레이어 색상을 바꾸며 다가오는 Gate를 통과
- 현재 Campaign Stage 1–20 구현, Stage Catalog revision 7
- 현재 Campaign 기믹:
  Shield, Booster, Camouflage, Fog, Ice, Echo
- Hidden과 Flicker는 Experiment Lab에 구현되어 있지만 Campaign 배치는
  각각 Stage 21–23과 24–26으로 연기
- 목표 플랫폼:
  Android와 WebGL
- 기본 화면비:
  9:16 Portrait

## 3. 현재 Git 상태

- 브랜치: main
- Iteration 19 구현 기반 커밋: 5e0eb3c
- 실제 최신 HEAD는 Docs/CURRENT_STATUS.md를 따른다.
  feat: add continue economy policy

최신 HEAD와 테스트 수는 항상 저장소의
Docs/CURRENT_STATUS.md를 권위자로 사용한다.

## 4. 현재 제품 흐름

첫 실행:

Boot
→ Account Choice
→ Guest로 시작
→ Frontend Lobby
→ START STAGE
→ PreRun / Item Selection
→ Gameplay
→ Clear / Failure Result
→ Frontend Lobby

이후 실행:

Boot
→ Frontend Lobby
→ START STAGE
→ PreRun / Item Selection
→ Gameplay

Frontend에서 진입하면 기존 Campaign Lobby는 우회한다.

직접 SampleScene 실행이나 개발 진입에서는 기존 Campaign Lobby를
fallback으로 유지한다.

## 5. 현재 구현된 제품 기능

- Boot Scene
- Persistent AppRoot
- Local Guest Profile
- Account Choice 완료 상태
- 이후 실행 시 Lobby 자동 진입
- Frontend 통합 Lobby
- 추천 Stage 읽기 전용 표시
- Stable Stage ID 기반 one-shot Campaign Launch Context
- Product Save 및 Settings
- Product Save schema 2 기반 Campaign Progress
- 기존 Campaign PlayerPrefs 1회 자동 이관
- Coin, Shield/Booster Inventory 기반 Economy 기초
- Stage 최초 클리어 보상과 2 Stage마다 자동 Lobby 발전
- 36 Stage를 위한 18개 Lobby milestone, 3개 Theme 구조
- Lobby Coin, Inventory, Theme, 다음 발전 목표 표시
- Gameplay Pause
- Pause Dim
- Resume / Restart / Lobby 복귀
- Frontend와 Pause 공용 Settings UI
- 알림, 음악, 효과음, 진동 설정 저장
- 이용약관, 개인정보, 지원 링크 Configuration 경계
- PlayMode 테스트의 Campaign PlayerPrefs 격리
- Campaign Stage 1–20 학습 곡선 재배치
  - Stage 1–5: 색상과 리듬 기초
  - Stage 6: 제공 Shield, 선택 잠금
  - Stage 7: 제공 Booster, 선택 잠금
  - Stage 8: 첫 3색 적용 및 첫 아이템 선택
  - Stage 9–11: Camouflage 입문/연습/숙련
  - Stage 12–14: Fog 입문/연습/숙련
  - Stage 15–17: Ice 입문/연습/숙련
  - Stage 18–20: Echo 입문/연습/숙련
- Stage 8+ PreRun의 Product Shield/Booster 보유량 표시와 실제 소비
  - 보유량 0인 아이템은 선택 불가
  - START 성공 시 선택 아이템을 각각 1개씩 하나의 저장으로 소비
  - 저장 실패 시 PreRun 유지, 아이템 효과와 Countdown 시작 금지
  - Retry는 새 Attempt이므로 다시 소비하고 Back은 소비하지 않음
  - Stage 6/7 제공 아이템은 무료이며 Inventory를 소비하지 않음
  - ProductSession 부재 시 선택 아이템을 무료 제공하지 않음
- Failure Result Continue Economy
  - 성공한 Coin Continue 순서대로 `300 / 600 / 900` Coin
  - 한 Attempt의 Coin/광고 합산 Continue는 최대 3회
  - 성공한 보상형 광고 Continue는 Attempt당 1회이며 Coin을 먼저 써도 유지
  - Retry는 새 Attempt이므로 가격 순서와 광고 권리를 초기화
  - 결제/광고 요청 중에는 Failure Result를 고정하고 중복 요청을 차단
  - 부족한 잔액, 저장 실패, 광고 실패/취소/미지원은 소비 없이 Failure 유지
  - 실제 광고 provider가 없는 release에서는 광고 버튼을 숨기며 무료 성공을
    흉내 내지 않음

## 6. 최근 해결한 문제

- Frontend Lobby와 Campaign Lobby가 연속으로 나타나던 문제
- Gameplay Pause 버튼이 거의 보이지 않던 좌표계 문제
- PreRun Back이 항상 기존 Campaign Lobby로 가던 문제
- Settings ON/OFF 라벨이 겹치던 문제
- PlayMode 테스트가 실제 Editor Campaign 진행을 삭제하던 문제
- Unity Package가 관리하는 WebGL define 때문에 작업이 반복 중단되던 문제

## 7. 현재 저장 정책

### Iteration 19 검증 기준

- EditMode `400/400`
- PlayMode 및 Post-Builder PlayMode `215/215`
- Frontend/Campaign Builder 2회 및 검증 성공
- Campaign simulation `400` rows, 연속 2회 byte-identical
- Campaign artifact는 Iteration 19의 승인 해시로 갱신됐고 Step 10
  결정론적 artifact는 변경되지 않음
- Campaign Summary SHA-256:
  `BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`
- Campaign JSON SHA-256:
  `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`
- Campaign CSV SHA-256:
  `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`

최신 수치와 해시는 항상 `CURRENT_STATUS.md`를 우선한다.

- Product Profile, Settings, Campaign Progress, Economy, Lobby Progress:
  versioned product-save.json
- Product Save schema version은 2다.
- 첫 production Boot에서 기존 PlayerPrefs Campaign 기록을 stable Stage ID로
  1회 이관한다.
- 이관 후 Product Save가 Campaign 런타임 저장 권위자다.
- 기존 PlayerPrefs key는 rollback 안전을 위해 삭제하지 않지만 더 이상
  runtime dual-write 권위자가 아니다.
- Stage clear, unlock, 최초 보상, Lobby milestone은 하나의 저장 transaction으로
  적용한다.
- 선택한 Shield/Booster는 START에서 각각 1개씩 원자 소비하며 저장 성공
  후에만 Attempt를 시작한다. 이 변경은 schema 2 migration을 추가하지 않는다.

## 8. 현재 Settings 상태

표시 항목:

- 알림 ON/OFF
- 배경음악 ON/OFF
- 효과음 ON/OFF
- 진동 ON/OFF
- 이용약관
- 개인정보 보호정책
- 지원

현재 한계:

- 실제 모바일 알림 미구현
- 실제 Music/SFX AudioSource와 음원 미구현
- 법률 및 지원 URL 미확정
- Master Volume 데이터는 유지하지만 사용자 UI에서는 숨김

## 9. 지켜야 할 핵심 계약

- Gameplay Core는 Unity 비의존 구조를 유지한다.
- 현재 Stage 1–20 Catalog 데이터와 결정론적 결과를 임의로 변경하지 않는다.
- Campaign 진행과 Economy의 저장 권위자를 중복 생성하지 않는다.
- PlayerPrefs를 Scene 이동 Payload로 사용하지 않는다.
- Scene 이름이나 Build Index를 런타임에서 하드코딩하지 않는다.
- AppRoot와 EventSystem을 중복 생성하지 않는다.
- 새로운 Package와 ProjectSettings 변경은 명시적 승인 없이는 금지한다.
- 공통 절차와 검증은 저장소의 AGENTS.md와 TEST_PLAN.md가 소유한다.
- 현재 구현 상태와 테스트 기준은 CURRENT_STATUS.md가 소유한다.

## 10. 주요 저장소 문서

- AGENTS.md
- Docs/CURRENT_STATUS.md
- Docs/GAME_DESIGN.md
- Docs/FRONTEND_FLOW.md
- Docs/PRODUCT_SYSTEMS.md
- Docs/ART_DIRECTION.md
- Docs/DECISIONS.md
- Docs/TEST_PLAN.md
- Docs/ITERATIONS.md

GPT가 정확한 구현 세부 사항이 필요하면 사용자에게 해당 최신 문서를
요청한다.

## 11. 아직 남은 주요 작업

- Stage 1–20 학습 곡선 인간 플레이 검증
- Hidden Stage 21–23, Flicker Stage 24–26 제작
- Stage 27–36 후반 Campaign 설계와 제작
- Lobby 3개 Theme의 최종 아트, 애니메이션, 보상 연출
- Continue Economy 인간 플레이 검증과 실제 광고 SDK/provider 선정
- Shield/Booster Coin 구매와 Shop 진입 흐름
- Heart, 시간제 무제한 Booster/Heart, Shop 및 IAP
- 실제 BGM과 SFX 연결
- 실제 모바일 알림과 권한 처리
- 이용약관, 개인정보, 지원 URL 확정
- Stage 선택 또는 Campaign Page 필요성 검토
- PreRun, Gameplay, Result의 최종 비주얼
- 실제 Android/WebGL 기기 검증

## 12. GPT 응답 방식

사용자의 플레이 피드백을 우선 분석한다.

새 기능을 바로 Codex 프롬프트로 만들기 전에:

1. 문제가 버그인지 UX 문제인지 구분
2. 기존 기능과 중복되는지 확인
3. 가장 작은 다음 Iteration을 제안
4. 사용자 승인 후 Codex 프롬프트 작성

Codex 프롬프트에는 반복적인 Git, 테스트, Builder 규칙을 길게 복사하지
않는다.

대신 다음을 참조한다.

- AGENTS.md의 Standard Iteration Protocol
- CURRENT_STATUS.md의 Authoritative Baseline
- TEST_PLAN.md의 Standard Regression Suite
