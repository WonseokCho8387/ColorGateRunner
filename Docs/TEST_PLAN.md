# Test Plan

## How to use this document

`CURRENT_STATUS.md` owns the latest approved counts, hashes, Build Settings,
persistence snapshots, and Authoritative Baseline. This document owns **what
must be validated and when**.

For every iteration:

1. Run the Quality Graph `inspect` preflight and record whether Unity is open,
   which project copy is in scope, and which backend can provide evidence.
2. During Phase A, inspect the baseline, affected contracts, and risks. Do not
   run the full EditMode suite, full PlayMode suite, Builders, or Simulations
   merely to begin an iteration.
3. Select the lowest Validation Tier that covers the actual change. Escalate
   when a shared controller, shared prefab, persistence boundary, stage rule,
   or deterministic input is affected.
4. Implement the approved scope through one maker.
5. After implementation, run the selected tier's required validation once.
   Focused tests may be run earlier while developing, but the complete tier is
   not repeated unless a failure or subsequent edit invalidates its evidence.
6. Run the independent QC route required by the Quality Graph risk class.
7. Record final counts and artifact evidence in `CURRENT_STATUS.md`. Move
   iteration-specific acceptance mapping and final evidence to
   `TEST_HISTORY.md`; do not append them here.
8. Use `TEST_CATALOG.md` to locate current automated tests and structural
   checks. Do not copy the entire catalog into a Codex prompt.

Phase A may run a focused baseline test only when the investigated area
requires execution evidence. A full pre-implementation suite is allowed only
when the Authoritative Baseline is suspect or investigation cannot proceed
without it; the reason must be reported before execution.

## Validation Tiers

### Tier 0 — Documentation only

- Run `git diff --check` and appropriate Markdown formatting, link, path, and
  contract checks.
- Verify that edited claims agree with their authoritative documents.
- Do not run Unity runtime tests unless a documentation claim cannot otherwise
  be verified.
- Do not update runtime counts or hashes without executing the corresponding
  suites.

### Tier 1 — Isolated presentation or UI hotfix

- Run focused EditMode and/or PlayMode tests for the changed behavior.
- When generated Scene or UI content changes, run each affected Builder twice
  and run the focused post-Builder tests.
- Run the full EditMode and PlayMode suites only when the change reaches a
  shared controller, shared prefab, Scene-wide infrastructure, navigation
  authority, or another cross-feature boundary.
- Simulation may be omitted when no gameplay rule, stage data, timing,
  deterministic input, or simulation source changed.

### Tier 2 — Scene flow, Product Save, Settings, or controller integration

- Run focused tests for the changed behavior.
- Run each affected Builder twice.
- Run the full EditMode and PlayMode suites after the final Builder pass.
- Preserve and compare Product Save, Guest ID, and Campaign PlayerPrefs as
  applicable.
- Run only an affected simulation when the change touches a gameplay execution
  boundary; otherwise reproduce no simulation solely for a product-shell
  change.

### Tier 3 — Gameplay Core, stage data, timing, balance, or determinism

- Run focused tests for the changed behavior.
- Run the full EditMode and PlayMode suites.
- Run every affected Builder and its post-Builder validation.
- Run the complete Campaign deterministic simulation.
- Run the complete Step 10 matrix when Experiment contracts, shared mechanic
  rules, player profiles, or simulation inputs may be affected.
- Compare every required artifact hash against the Authoritative Baseline.

When a change spans tiers, use the highest applicable tier. If implementation
expands beyond the assumptions used to choose the tier, reclassify before final
validation.

## Quality Graph Routes

Quality Graph risk routing is independent of the deterministic Validation Tier.
The Tier selects project tests; the Route selects independent evidence.

### Route Q0 — Deterministic only

- Documentation and isolated pure logic with no demonstrated semantic,
  integration, visual, device, or release risk.
- Run the selected Validation Tier with one maker. No checker is required.

### Route Q1 — Independent behavior checker

- Shared controller, persistence, stage generation, Builder, navigation,
  timing, policy consistency, or cross-feature applicability changes.
- After deterministic gates, a fresh-context checker receives the acceptance
  contract, diff, environment manifest and relevant black-box evidence.
- The checker reports pass, fail, or insufficient evidence and does not edit.

### Route Q2 — Rendered or release-facing QC

- Scene, shader, material, VFX, camera, UI, animation, mobile/WebGL, or other
  user-visible output.
- Q1 applies, plus graphics-capable captures or a Player artifact at the
  relevant resolution/backend. Headless evidence cannot satisfy this route.
- Human review remains required for feel, readability, taste, fairness and
  device comfort.

Use one checker by default. Pass a compact task packet rather than the complete
conversation. Allow one targeted repair; a repeated failure or product-intent
conflict returns to the user. When a validator becomes a completion gate, seed
isolated mutations representative of its failure class and prove the validator
rejects them.

## Standard Regression Suite

### Before implementation — Phase A inspection

- Run `Tools/QualityGraph/Invoke-QualityPreflight.ps1 -Mode inspect`.
- Verify `git status --short` and `git diff --check`.
- Verify the repository and HEAD against the Authoritative Baseline in
  `CURRENT_STATUS.md`.
- Inspect Package manifest/lock and meaningful `ProjectSettings` state.
- Capture the Product Save, Guest ID, and real Editor Campaign PlayerPrefs
  snapshots when the selected tier can touch them.
- Inspect the relevant implementation, tests, Builders, Scenes, and documents.
- Run only focused inspection or tests that are necessary to resolve an
  uncertainty.
- Do not run full EditMode, full PlayMode, Builders, or Simulations during
  Phase A unless the baseline is suspect or execution evidence is required to
  continue.
- Stop before implementation when the baseline differs unexpectedly.

### After implementation — one tiered validation pass

- Run `batch-validate` preflight before Headless Unity and `finalize` preflight
  before final documentation and Git staging. Both require the primary Editor
  closed. Label isolated copies explicitly.
- Run targeted tests for every new or changed behavior.
- Run the additional suites, Builders, simulations, and artifact comparisons
  required by the selected Validation Tier.
- Require Unity compilation and non-zero execution for every requested test
  result.
- Run `git diff --check`.
- Verify no Missing Script or Missing Reference.
- Verify no unrelated Package or ProjectSettings change.
- Compare the required persistence snapshots before and after validation.
- Update documentation only after validation succeeds.
- If code, Scene, generated assets, or configuration changes after validation,
  rerun the invalidated portion of the tier before reporting completion.

## Cross-cutting safety rules

### Campaign PlayerPrefs

- Every PlayMode fixture that can touch Campaign progression captures the
  complete key set before setup: `ColorGateRunner.Stage.HighestUnlocked` and
  Stage Records 1 through 13.
- Preserve each key's existence and exact integer or string value.
- Restore the snapshot in exception-safe cleanup after success, setup failure,
  test failure, and teardown failure.
- Delete only keys that did not exist before the fixture, then call
  `PlayerPrefs.Save()`.
- Compare the real Editor before/after key count and SHA-256 when running the
  full PlayMode suite.
- Never use unconditional cleanup deletion or guessed defaults.

### Product Save and identity

- Test default creation, validation, dirty state, roundtrip, and reload when
  the save contract changes.
- Test supported migration, future-schema rejection, corrupt-primary recovery,
  and backup recovery according to `PRODUCT_SYSTEMS.md`.
- Preserve Guest ID unless identity replacement is explicitly approved.
- Product services must not copy, reset, migrate, or write Campaign
  progression unless a dedicated progression migration is approved.
- Save failures must be reported truthfully and must not appear successful.
- Save-failure fixtures must arm the exact next write after composition, then
  assert the user-visible failure state and unchanged authority. They must not
  require a diagnostic log unless the runtime contract explicitly emits one.

### Determinism and simulation

- Preserve deterministic seed, Retry, Replay, Continue, stable Stage-ID, and
  planned sequence contracts.
- Compare artifacts only against the approved baseline; do not rewrite a hash
  merely because current output differs.
- Run all applicable player profiles and start-item combinations at Tier 3.
- Verify Gate and Track pools do not grow unexpectedly.
- Change only values explicitly approved as balance-tunable.
- Report mechanical results without claiming fun, fairness, readability, or
  satisfaction.

### Builder, Scene, and UI structure

- Run each affected Builder twice consecutively when required by the selected
  tier.
- Perform post-Builder tests after the final pass.
- Require one expected generated root for each Builder-owned structure and all
  required serialized references.
- Verify EventSystem, AppRoot, router, overlay, pooled object, state-label, and
  listener counts as applicable.
- Verify Build Settings order, enabled state, uniqueness, and serialized Scene
  destinations.
- Verify Safe Area and portrait layout contracts.
- Repeated navigation must not duplicate persistent or Scene objects.
- UI input must not leak into gameplay.

## Validation by change type

### Documentation-only

- Use Tier 0.
- Preserve historical evidence and do not change runtime counts or hashes.

### Isolated visual, copy, or local UI presentation

- Start at Tier 1.
- Cover visibility, hierarchy, label synchronization, input routing, and the
  affected Builder-owned output.
- Escalate to Tier 2 when navigation, shared Settings state, persistent UI,
  Scene loading, or a shared controller changes.

### Scene, navigation, Builder, or generated UI

- Use Tier 2 unless the edit is demonstrably isolated presentation covered by
  Tier 1.
- Validate two Builder passes, final generated structure, Build Settings,
  serialized destinations, repeated navigation, transition blockers, and
  post-Builder PlayMode behavior.

### Product Save, Profile, Settings, account state, or identity

- Use Tier 2.
- Cover creation, validation, migration/rejection, recovery, transactional
  failure behavior, Guest ID stability, and Campaign-progress separation.
- When Frontend and Gameplay expose Settings, they must use the same
  authoritative Settings state.

### Gameplay Core, stage catalog, mechanic, timing, balance, or simulation

- Use Tier 3.
- Validate deterministic planning and replay, stage progression, stable IDs,
  terminal states, item and modifier interactions, pool stability, Campaign
  simulation, and affected Step 10 artifacts.

### Test isolation or tooling

- Choose the tier of the production boundary the tooling can mutate.
- For Campaign PlayMode isolation, preserve the complete real Editor
  PlayerPrefs snapshot even when production code is unchanged.
- For build tooling, validate destination selection, enabled Scene order,
  output paths, failure reporting, and non-zero test execution.

## Human review

Automation can validate state, timing, bounds, hierarchy, visibility flags,
and deterministic results. Human review is still required for:

- fun, fairness, pacing feel, cognitive load, and satisfaction;
- mobile readability, notch and rounded-corner comfort, and touch ergonomics;
- transition feel, wording comprehension, and visual hierarchy;
- whether Pause dimming prevents useful future-Gate scouting;
- Android/WebGL platform-specific Back, focus, audio, vibration, and input
  behavior;
- final visual polish.

Human findings must be recorded separately from automated acceptance. A passing
automated suite must not be described as proof of fun, readability, comfort,
fairness, or polish.

## Active Feature Acceptance Map

### Gameplay Core and deterministic runtime

- Initial, Restart, Retry, Continue, Countdown, color switching, scoring,
  difficulty, gate spacing, authored patterns, judgment, and terminal-state
  transitions remain deterministic and one-shot.
- Retry replays the approved layout; Continue consumes the failed plan once,
  immediately recycles that fixed-pool slot with a later plan, and preserves
  the approved timer, speed, color, sequence, pool, and item state.
- Continue has no total count cap. Coin sources use `900 / 1900 / 2900 / 4900`
  by successful-Coin ordinal and repeat `4900` thereafter. One completed
  rewarded-ad right remains independent of Coin use, Ticket/ad sources do not
  advance the Coin ordinal, and Retry resets the attempt-local policy.
- Coin spending must be atomic and idempotent before Core resume. Insufficient
  funds, save failure, non-completed ads, duplicate callbacks and stale
  callbacks preserve the frozen failure state. Unavailable providers expose
  no rewarded action and cannot simulate success.
- On the first Continue-countdown frame, retained modifier visuals and safe
  colors must match resumed gameplay. Consumed Shield, Booster, Echo, local
  grants, and their presentation must already be inactive.
- Gate and Track pools remain fixed and recycled objects reset their runtime
  state.
- Goal is part of deterministic planning, appears continuously, and clears the
  stage once.

### Campaign, progression, and items

- Catalog IDs remain unique and stable; Lobby display, PreRun, Gameplay,
  persistence, Retry, and Next Stage use the same stable Stage ID.
- Schema 3 Product Save owns runtime Stage unlock and record writes by stable
  Stage ID. Legacy device-wide Campaign PlayerPrefs are imported once and then
  retained unchanged as a rollback source; Campaign progression remains scoped
  to the local Product profile and does not derive from Guest identity.
- Locked stages cannot start; Clear unlocks only the next Stage; Retry returns
  through the existing PreRun/item-selection contract.
- Campaign Clear hides Replay. Next Stage opens the unlocked next stable Stage
  directly in PreRun; final-Stage and unavailable/unpersisted destinations
  return to Lobby.
- A first Clear grants `100 / 200 / 500` base Coins for Normal/Hard/Very Hard
  and presents any even-Stage milestone reward separately. Replays do not
  duplicate either reward.
- Catalog revision 11 contains 23 contiguous stable IDs. Stages 1–5 remain
  unchanged; Stages 6/7 provide their named item with selection locked;
  Stage 8 is clean three-color item application. Stages 9–20 contain isolated
  Camouflage, Fog, Ice and Echo intro/practice/mastery blocks with 2/2/3
  colors. Stages 21–23 contain the isolated Hidden 2/2/3-color block; Flicker
  remains Experiment-only until Stage 24+.
- Every Stage 1–23 initializes under all four requested item inputs. Runtime
  selection is rejected through Stage 7 and allowed from Stage 8 onward.
- A successful Stage 8+ Start atomically persists one decrement for each
  selected owned item before Countdown. No selection and Back before Start do
  not consume; duplicate Start consumes once; Retry starts a new attempt and
  consumes retained selections again.
- Zero stock on an allowed Stage 8+ item exposes a fixed `900`-Coin one-item
  quick buy. Confirmed Product success grants exactly one item, spends once
  under an idempotent transaction ID and auto-selects it; shortage and save
  failure keep PreRun/modal state truthful with no partial spend or grant.
  Stale insufficient inventory at `START` follows the existing no-partial-
  decrement rule. Stage 6/7 provided items remain free and non-consuming.
  Missing Product context disables selectable items and quick buy without
  blocking no-item or provided-item starts.
- The Campaign selection UI contains exactly one generated entry per Catalog
  Stage, and Lobby current-stage/title bounds remain separated at every
  approved portrait reference resolution.
- Shield, Booster, local grants, Echo, Continue, Goal, and failure priority
  preserve their documented interactions and do not grow pools.

### Hidden, Flicker, Clone, Echo, and experiment mechanics

- Modifier selection, gate identity, target color, judgment, Retry, Replay,
  and visibility state remain deterministic.
- Hidden and Flicker do not apply to Goal, do not increase gate count, and do
  not combine with another modifier unless an explicit contract says so.
- Clone preserves its Source relationship, separate judgment, configured gap,
  Shield behavior, and ordinary Camouflage behavior.
- Experiment Lab remains development-only, does not mutate normal progression,
  and reuses the fixed runtime object graph.

### Frontend, onboarding, Settings, and Pause

- Build Settings remain Boot -> Frontend -> Campaign with unique enabled
  entries and serialized destinations rather than Scene-name assumptions.
- Boot creates one persistent AppRoot; direct Frontend without it remains
  blocked as `BOOT REQUIRED`.
- Completed onboarding reaches Lobby without a Title flash; failed persistence
  leaves public state unchanged and exposes retryable feedback.
- Frontend `START STAGE` consumes one stable Stage launch request and enters
  PreRun without exposing the legacy Campaign Lobby.
- PreRun Back returns to Frontend only when the Frontend launch context was
  consumed; direct or missing-context entry retains the Campaign Lobby
  fallback. Load failure remains recoverable and repeated Back creates one
  load.
- Frontend and Pause share one Settings contract. Notifications, Music, SFX,
  and Vibration each use one synchronized `StateLabel`; Music/SFX restore the
  last non-zero value.
- Pause freezes the approved attempt state, owns nested modal priority, blocks
  duplicate actions, and does not leak UI input into gameplay.

### Product Save and links

- Fresh creation, schema-1 roundtrip, deterministic schema-0 migration,
  future-schema rejection, corrupt-primary recovery, backup recovery, and
  transactional failure behavior remain covered.
- Guest ID remains stable and is not a device identifier.
- Product services do not own Campaign progression.
- Product links accept only absolute HTTPS destinations and do not open when
  unconfigured.

### Monetization platform baseline

- Approved package and ProjectSettings hashes match `CURRENT_STATUS.md`.
- Unity IAP compiles without introducing a live or simulated-success purchase
  path before the purchase contract is implemented.
- Android application ID remains exactly `com.wscho.colorgaterunner` and
  Google Play billing mode remains selected.
- Firebase SDK/configuration, Store products and paid reward grants remain
  absent until their separately approved iterations.

### Generated structure

- Boot, Frontend, and Campaign Builders remain idempotent.
- Generated Scenes contain the required references, one expected root and
  EventSystem, fixed pools, valid Safe Areas, and no Missing Script or Missing
  Reference.
- Repeated Builder runs and navigation do not create duplicate roots,
  listeners, labels, routers, overlays, AppRoots, or pooled objects.

### Iteration 21 local commerce rewards and Hearts

- Schema 2 migrates to schema 3 with 5 Hearts while preserving identity,
  Campaign progress, Coins and owned start items.
- Stage start atomically authorizes one Heart plus selected items; empty
  Hearts or save failure keeps PreRun open and publishes no partial spend.
- A successful Clear atomically refunds the finite Heart consumed by that
  attempt. Unlimited-Heart attempts have no spend to refund, and failure,
  Retry, or Lobby departure does not refund the failed attempt.
- Hearts recover one per 30 minutes to a cap of 5, tolerate offline elapsed
  time and do not gain from backwards clock movement. Unlimited duration
  stacks and suppresses consumption before expiry.
- Approved commerce grants are order-ID idempotent, Starter is account-limited
  and unknown products never mutate state.
- Continue Ticket is offered before available rewarded ad and Coin. Spend
  failure keeps the failure snapshot frozen. The former shared maximum of
  three Continues is historical and is superseded by Iteration 23.
- Campaign Builder runs twice after the new failure action is generated, then
  the full post-Builder PlayMode suite runs.

### Iteration 22 Editor Developer Console

- Product EditMode coverage must verify exact Coin/item/Ticket/Heart/timed-
  Heart edits, preservation of commerce ledger during an exact edit, invalid
  value non-publication, Campaign-only reset and Economy-only reset.
- Editor coverage must verify the menu registration and preserve enabled Build
  Settings order. Tests use isolated saves and never invoke the real console
  mutation path.
- Full EditMode and PlayMode suites are required because Product save and
  shared Presentation refresh/launch boundaries changed. Builders and
  deterministic simulations are omitted when generated Scenes, Core, Stage
  data and simulation inputs remain unchanged.

### Iteration 23 result-loop closure

- Failure wallet coverage verifies live Coin and Heart values, the uncapped
  `900 / 1900 / 2900 / 4900+` Coin schedule, Ticket/ad independence, Retry
  reset, and intentional ad-action hiding while no real provider exists.
- Insufficient Coin coverage verifies the frozen failure snapshot and a
  truthful local unavailable notice with no working or simulated Shop action.
- Continue coverage verifies modifier/safe-color synchronization and consumed
  buff/local-grant presentation removal before the first countdown frame.
- Clear coverage verifies the `100 / 200 / 500` first-clear base reward by
  difficulty, separate milestone reward presentation, duplicate-clear
  idempotency, finite-Heart refund, and no refund for unlimited Heart.
- Campaign result coverage verifies Replay hidden, direct next unlocked PreRun,
  final-Stage Lobby fallback, and Lobby fallback when next unlock persistence
  is unavailable. Experiment Lab Replay remains available.
- Final validated evidence: EditMode `418/418`, PlayMode `222/222`, and the
  Campaign Builder passed twice before the post-Builder suite.
- Two Campaign simulations produced 400 rows each and byte-identical outputs:
  Summary `698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`,
  JSON `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`, and
  CSV `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`.
- Step 10 simulation artifacts remain unchanged.

### Iteration 24 Continue gate-pool hotfix

- A late-stage regression must consume more failed gates than the visible
  fixed-pool size, verify every later authored gate remains available, and
  cross Goal into Stage Cleared.
- Recycling a failed slot must not grow/reset Gate or Track pools, re-expose
  the consumed plan, or alter unaffected active-gate transforms.
- Final evidence: focused Stage 11 PlayMode `1/1`, full EditMode `418/418`,
  full PlayMode `223/223`. No Builder or deterministic simulation input was
  changed; Iteration 23 artifacts remain authoritative.
- Final Lobby layout, Fog curtain redesign, and authored Ice approach remain
  deferred human/product work rather than acceptance claims for this iteration.

### Iteration 25 Campaign timed Fog curtain

- Campaign Fog must trigger once when the first Fog gate becomes the next
  judgment, follow at 1.5 seconds of effective travel distance, hold Alpha 1
  for 1.5 seconds, and fade linearly to 0 over 0.5 seconds.
- The same attempt cannot retrigger the curtain. Pause and Continue countdown
  freeze its elapsed time and Alpha; Continue repositions the same state after
  clean respawn and Retry resets it.
- Campaign Fog gates retain their authored material and ordinary judgment.
  Experiment Lab retains its nearest-two comparison and Step 10 contract.
- Builder validation requires exactly one `FogCurtain`, one valid controller
  reference and the existing fixed Gate/Track pools after two builds.
- Final evidence: EditMode `421/421`, post-Builder PlayMode `225/225`, Campaign
  Builder twice. Deterministic simulations are omitted because no mechanical
  input or result changed.

### Iteration 26 Fog visibility curve

- Campaign Fog must fade alpha `0 -> 1` over `0.5 seconds`, remain fully
  opaque for the Catalog-authored Stage duration, and fade `1 -> 0` over
  `0.5 seconds`.
- Stage 12 / 13 / 14 full-opacity values must be exactly `5 / 6 / 7 seconds`.
  Invalid non-positive phase durations must be rejected.
- Pause, Continue countdown, one-shot triggering, adaptive positioning, Retry
  reset, gate judgment and Experiment Lab behavior retain Iteration 25 rules.
- Final evidence: Stage Catalog revision 9, Campaign Builder twice, EditMode
  `423/423`, PlayMode `225/225`, and two byte-identical 400-row Campaign runs
  matching the existing Summary / JSON / CSV hashes. Step 10 is omitted because
  Experiment inputs and behavior are unchanged.

### Iteration 27 Preplaced Ice runway

- Campaign Stages 15–17 must author a positive `2.0` non-Booster Ice speed
  multiplier. The existing Ice spacing multiplier, gate sequence, seed and
  color judgment remain unchanged.
- Campaign Scene must contain exactly 50 prebuilt runway panels. Before
  Countdown, active panels must match every authored Ice plan and span its
  deterministic approach; the normal six-segment track material must remain
  unchanged. Runtime creation or pool growth is forbidden.
- Continue preserves panel positions and activation. Retry clears and rebuilds
  the same deterministic runway. Experiment Lab retains whole-track Cyan and
  `1.45` speed behavior.
- Builder validation requires one complete runway, the existing one Fog View,
  fixed Gate/Track pools, unique generated roots, references, EventSystem and
  Build Settings after two consecutive builds.
- Final evidence: Stage Catalog revision 10, focused EditMode `8/8`, focused
  PlayMode `4/4`, full EditMode `425/425`, post-Builder PlayMode `226/226`, and
  two byte-identical 400-row Campaign runs. Step 10 is omitted because its
  Experiment inputs and behavior are unchanged.

### Iteration 28 Ice tap rhythm

- Every Campaign Ice gate requires exactly one or two forward taps from the
  preceding authored target; zero and three-or-more are rejected.
- Stage 15 / 16 exact Ice quotas are `12 single + 0 double` and
  `14 single + 0 double`. Stage 17 is `14 single + 1 double`, so its two-tap
  rate is `6.67%` and strictly below 10%.
- Retry must reproduce every target color and the selected two-tap gate.
  Non-Ice stages, Continue recovery, Experiment Lab, speed, spacing and runway
  presentation remain unchanged.
- Tier 3 requires focused Core/runtime coverage, full EditMode/PlayMode and two
  complete Campaign simulations. Builder is omitted when no generated Scene or
  Catalog asset changes. Step 10 is omitted only while Experiment inputs and
  behavior remain unchanged.
- Final evidence: focused EditMode `13/13`, focused PlayMode `1/1`, full
  EditMode `429/429`, PlayMode `227/227`, and two byte-identical 400-row
  Campaign runs with zero continuity/pool violations.

## References

- `CURRENT_STATUS.md` — latest Authoritative Baseline, counts, hashes, Build
  Settings, and persistence snapshots.
- `TEST_CATALOG.md` — searchable current EditMode, PlayMode, Builder, and
  structural validation catalog.
- `TEST_HISTORY.md` — preserved Step/Iteration acceptance mapping, regression
  evidence, execution counts, hashes, and manual findings from the former
  monolithic plan.
- `PRODUCT_SYSTEMS.md` — Product Save, Profile, Settings, Guest identity, and
  recovery contracts.
- `GAME_DESIGN.md` — gameplay, Campaign, mechanic, and determinism contracts.
- `FRONTEND_FLOW.md` — Boot, Frontend, navigation, onboarding, and return-flow
  contracts.
- `AGENTS.md` — Standard Iteration Protocol and repository operating rules.

### Iteration 29 Lobby visual hierarchy

- Frontend Builder must produce no `LobbyTitle` or duplicate
  `LobbyAccountText`, and must serialize one Heart label plus required panel
  references after two consecutive builds.
- Lobby PlayMode must prove top Profile/Settings placement, visible Coin/Heart
  state, difficulty in the Stage card, one bottom primary `PLAY` action and
  unchanged Boot-to-Lobby / Lobby-to-Campaign / clear-return flow.
- Heart formatting covers full, next-recharge and unlimited countdown states.
  Full Product save and backup must remain exact.
- Tier 2 requires focused and full EditMode/PlayMode plus Frontend Builder
  twice. Campaign and Step 10 simulations are omitted only while gameplay,
  generated Campaign content, timing, judgment and Experiment inputs remain
  unchanged.

### Iteration 30 Theme 1 visual slice

- Frontend Builder must import three Theme 1 images as non-mipmapped Sprites,
  preserve alpha for Midground/Foreground, assign one three-entry catalog and
  serialize all three decorative layer references after two builds.
- Theme 1 PlayMode must prove all artwork layers are active, non-null and
  non-raycast; milestone energy-node activation must equal Product Lobby
  progress and the existing `PLAY` action must remain interactive.
- Themes 2 and 3 must retain valid palette-only fallbacks rather than showing
  unavailable illustrated content as complete.
- Tier 2 requires focused and full EditMode/PlayMode plus Frontend Builder
  twice. Campaign and Step 10 simulations are omitted only while gameplay,
  Stage data, timing, generation, judgment and Experiment inputs remain
  unchanged.

### Iteration 31 Neon gameplay visual slice

- EditMode must prove the editable Blender source, five imported FBX models
  and Runner / Gate / Environment BaseColor, Emission, Normal,
  MetallicSmoothness and UV layout maps all import successfully.
- The Lobby ambient PNG must decode with a majority of genuinely transparent
  pixels and retain visible high-alpha energy pixels.
- Campaign PlayMode must prove the imported `ColorShell` runner, Goal portal,
  city backdrop, enabled camera post-processing, one global Volume and the
  fixed six-view gate-break pool. Existing gate-part and whole-track Ice reset
  tests remain authoritative after imported renderer ownership changes.
- Campaign Builder must run twice and retain one generated root, one
  EventSystem, valid references, six Gate slots, six Track slots and Build
  Settings order. Missing Script, Missing Reference and prefab-parenting
  warnings are failures.
- Tier 2 requires full EditMode and post-Builder PlayMode. Campaign and Step 10
  simulations are omitted because Stage data, generation, timing, judgment,
  balance and Experiment inputs are unchanged.

### Iteration 32 Neon UI skin

- EditMode must prove all seven common surfaces and ten semantic icons import
  as genuine-alpha Sprites with mipmaps disabled. Common surfaces require the
  approved non-zero 9-slice border; icons require zero border.
- Frontend PlayMode must prove the primary account action uses a sliced surface
  and non-raycast icon, and Lobby Coin/Heart icons exist without replacing
  Product-backed text.
- Campaign PlayMode must prove Pause, failure/modal and item-action surfaces
  use the shared skin and non-raycast semantic icons.
- Frontend and Campaign Builders must each run twice and preserve required
  references, unique roots, EventSystem and Build Settings order.
- Tier 2 requires full EditMode and post-Builder PlayMode. Campaign and Step 10
  simulations are omitted while Stage data, generation, timing, judgment,
  balance and Experiment inputs remain unchanged.

### Iteration 33 Theme 1 artwork axis correction

- Campaign Builder must normalize the imported Runner, Gate, Track, Goal
  portal and City roots to local position zero, identity rotation and unit
  scale after parenting.
- Campaign PlayMode and Builder validation must reject a Track that is not
  approximately `6.5-8` units wide, no more than `3` units high and at least
  `39.5` units long on forward Z. The Track length must exceed its height by
  more than ten times.
- Campaign Builder must run twice and retain all existing unique-root,
  reference, EventSystem, pool and Build Settings contracts.
- Tier 2 requires full EditMode and post-Builder PlayMode. Campaign and Step 10
  simulations are omitted because collision, Stage data, deterministic
  generation, timing, judgment, balance and Experiment inputs are unchanged.

### Iteration 34 Recycled Track continuity

- Campaign Builder must author every 40-unit Track segment with start/end
  anchors at local Z `-20 / +20`, and reject a non-40-unit span or initial
  seam gap/overlap on both consecutive passes.
- PlayMode must recycle the fixed six-segment pool beyond one kilometer and
  prove every segment retains its span, every ordered seam remains exact and
  the live coverage stays ahead of the player without pool growth.
- Tier 2 requires full EditMode and post-Builder PlayMode. Campaign and Step 10
  simulations are omitted because Stage data, deterministic gate generation,
  timing, judgment, balance and Experiment inputs are unchanged.

### Iteration 35 Protection field and Warp Booster

- Core coverage must prove an attempt-level block returns no Echo Provider and
  does not alter the deterministic first-offer slot. Campaign Stage 18 and the
  Echo Experiment must generate zero providers with normal Shield and preserve
  existing deterministic offers without Shield.
- PlayMode must prove both protection roots use one valid field renderer and
  the `ColorGateRunner/ProtectionField` shader. Held Echo field RGB must equal
  the exact stored runner-material RGB. Shield and Echo consumption must each
  play its fixed collapse emitter while hiding the consumed field; emitters
  must be registered in the attempt pause set.
- Warp Booster must contain exactly one non-emitting controller root and two
  centered Circle emitters, Cyan and Gold, with 1.5-second lifetime, speed 35,
  Stretch rendering, radius five and at most 160 emitted particles combined.
  Existing activation, pause, end-clear and normal-play hidden behavior remain
  active regressions.
- Tier 3 requires Campaign Builder twice, full EditMode/post-Builder PlayMode,
  two Campaign simulations and two Step 10 simulations. Campaign differences
  are permitted only for Shield-bearing Echo Stage rows; every no-Shield row
  must match the prior baseline. Step 10 artifacts must remain deterministic.
- Builder validation must reject missing, looping or play-on-awake protection
  collapse emitters and must retain their fixed 36-particle capacity.

### Iteration 36 Runner readability and Bloom

- EditMode must prove the imported Runner contains exactly the required rear
  readability vocabulary and that all semantic gameplay materials use HDR
  emission while Theme 1 dark alloy remains non-emissive.
- Campaign PlayMode must prove the generated Runner exposes those rear parts,
  the active player material has HDR emission, the Track remains non-emissive,
  camera post-processing is enabled and the global Bloom Volume remains active.
- Campaign Builder must run twice and reject missing/duplicated or chase-camera-
  reversed rear parts, invalid material contrast, or Bloom weaker than threshold
  `0.8`, intensity `0.85`, scatter `0.58` with no more than four iterations.
- Tier 2 requires focused and full EditMode/PlayMode plus Campaign Builder
  twice. Campaign and Step 10 simulations are omitted because Stage data,
  deterministic generation, timing, judgment, balance, collision and
  Experiment inputs are unchanged.

#### Runner glass color follow-up

- Builder and PlayMode must prove exactly one `RunnerColorView` binds the
  existing `ColorShell` HDR accent and `HullShell` glass renderer.
- The glass shared material must be non-emissive with smoothness at least
  `0.9`; its property-block base color must follow the selected semantic
  runner material within serialized color tolerance.

### Iteration 37 Spline Track Lab

- EditMode must prove distance evaluation follows a curved Spline while
  preserving a horizontal up vector and height offset.
- Builder validation must prove exactly one complete Lab, one generated
  two-submesh road, meaningful horizontal X/yaw variation, hidden initial Lab
  visuals, and six unchanged straight Campaign track segments.
- PlayMode must enter the Lab, move runner/camera/gates/Goal along the curve,
  resolve all 12 gates, clear, then Exit to Lobby with the straight track
  active and pooled gate rotations reset.
- Campaign Builder runs twice. Full EditMode and post-Builder PlayMode are
  required. Campaign and Step 10 simulations are omitted because Core rules,
  Stage data, deterministic generation, timing, judgment, balance and existing
  Experiment inputs are unchanged.

### Iteration 38 World-space protection and Warp correction

- EditMode must load `ColorGateRunner/ProtectionField`, inspect its compiler
  messages and reject every error. Builder must apply the same rejection before
  regenerating the protection material and two field instances.
- Campaign Builder must produce one non-emitting Booster controller under the
  Player, never the camera, plus exactly two Cyan/Gold Box emitters positioned
  forward of the runner. Both layers must simulate in World Space, use velocity-
  aligned Stretch rendering and preserve the combined 160-particle cap.
- PlayMode must prove normal play emits no Warp particles, Booster activation
  uses the Player-owned hierarchy, and existing pause/end-clear behavior stops
  and clears every child particle system.
- Tier 2 requires the focused shader test, Campaign Builder twice, full
  EditMode and full post-Builder PlayMode. Campaign and Step 10 simulations are
  omitted because Core rules, Stage data, deterministic generation, timing,
  judgment, balance and Experiment inputs are unchanged.

### Iteration 40 Campaign Spline conversion and continuous city

- Builder validation must create exactly one complete Campaign Spline path,
  one two-submesh full-route road and one paired 24-slot Theme 1 city pool. It
  must retain the separate Spline Lab and fixed legacy six-slot Track pool for
  Experiment compatibility while disabling the legacy road in Campaign.
- Campaign PlayMode must prove runner, camera, pooled gates and Goal use one
  scalar path distance; later Stages contain horizontal and vertical path
  variation without roll; Fog and Ice consume path poses; and Continue restores
  the same route state without losing gates or Goal completion.
- City coverage must prove every active building remains outside the minimum
  lateral clearance, the fixed pool does not grow, deterministic placements
  recycle behind the runner and visual coverage extends through the Goal.
- Campaign Builder runs twice. Full EditMode and post-Builder PlayMode, two
  Campaign simulations and two Step 10 simulations are required. All approved
  deterministic artifact hashes must remain exact because spatial presentation
  does not change Core plans, timing, judgment or balance.

### Iteration 41 Spline camera inertia and quick Continue

- PlayMode must prove later Campaign curves create measurable camera rotation
  lag, cap it at `12°`, keep a stable upright view and return Booster framing to
  the normal local chase pose on the inertial path basis.
- Continue must keep the restored world frozen during `READY` for `0.5s`, begin
  gameplay at `GO`, keep GO non-blocking for `0.25s`, preserve the same gate
  layout and leave initial Stage entry on the existing three-second countdown.
- Campaign Builder runs twice. Full EditMode and post-Builder PlayMode, two
  Campaign simulations and two Step 10 simulations are required. All existing
  artifact hashes must remain exact; package and ProjectSettings baselines
  must remain unchanged.

### Iteration 42 Mechanic surface readability

- Builder validation requires exactly six Fog sections, one valid fixed
  50-slot Ice mesh pool and a complete shared-shader Echo field on each of the
  six pooled gates.
- PlayMode proves Fog sections span the adaptive band, active Ice slots own
  multi-row Spline ribbons using a non-emissive smooth material, the Campaign
  road uses rough non-emissive mapping, and Echo transfers the provider's exact
  color before hiding its gate field.
- Campaign Builder runs twice. Full EditMode and post-Builder PlayMode plus two
  Campaign and two Step 10 simulations are required. Existing artifact hashes,
  packages and ProjectSettings must remain exact.

### Iteration 43 Storm Fog and curve readability

- Builder validation requires ten fixed Fog banks, one 24-particle wisp system,
  one 96-particle rain system and one shared-profile scoped tone Volume. Both
  particle systems simulate in World Space and the inactive/reset state must
  have zero tone weight.
- PlayMode must prove Fog alpha drives banks, particles and tone together;
  Pause and Continue READY freeze the particle systems, GO resumes them, and
  Retry clears the weather. Camera lag remains upright and bounded by `24°`
  yaw / `16°` pitch. Runner visual steering must be measurable while its root
  remains the exact Spline pose.
- The bottom color strip must keep current centered, next on the right and
  previous on the left for three colors, retain a balanced two-color layout,
  reuse fixed slots for six colors and remain inside the portrait safe area.
- Campaign Builder runs twice. Full EditMode and post-Builder PlayMode plus two
  Campaign and two Step 10 simulations are required. All existing artifact
  hashes must remain exact; packages and ProjectSettings may not be changed.

### Iteration 44 Hidden Campaign block and current test builds

- Catalog revision 11 must contain exactly 23 contiguous stable IDs. Stages
  21 / 22 / 23 must be Hidden-only intro / practice / mastery with 2 / 2 / 3
  active colors, 42 / 46 / 50 gates and Normal / Normal / Hard difficulty.
- Hidden selection must remain deterministic, never apply to Goal or combine
  with another modifier, and reuse ordinary target-color judgment. Runtime
  Campaign coverage must prove the target hides while the `HIDDEN` marker and
  judgment path remain active.
- Loading a save whose highest Stage is 20 and whose Stage 20 record is cleared
  must expose Stage 21 without writing either Product save or legacy
  PlayerPrefs. The repair is capped and does not synthesize records or rewards.
- Each Android or WebGL Test Build command must refresh Stage Catalog,
  Campaign, Frontend and Boot before packaging and remove the previous target
  output first. A failed build may not leave a stale output that appears new.
- Generated Android Gradle must place dependency alignment after `plugins`,
  force Kotlin stdlib `1.8.22`, exclude split jdk7/jdk8 artifacts and upgrade
  the former pre-`plugins` cached block idempotently.
- Final evidence: focused build-tool EditMode `7/7`; full EditMode `449/449`;
  full PlayMode `248/248`; Campaign Builder twice. Two Campaign simulations
  produced 460 byte-identical rows: Summary
  `4E3B9ACE68ABDFF9540F9A26D7085C664C1658DAC133D73AE7E87BE3A5A1D07B`,
  JSON `3C3233C5CA3DA50B1272ADE1FE451D718CAA4E148598C81220881530B942A68D`,
  CSV `9CFB8DE0CFC54483E518F91290F998FEF57CBC5E0722CC4A0B2BDC744EF43611`.
  Two Step 10 runs retain all five authoritative hashes.
- Actual menu-driven Development builds must produce a fresh Android APK and
  a WebGL folder containing `index.html`, data, framework, loader and WASM with
  a `540 x 960` exact-`9:16` canvas. Installation, touch/browser interaction,
  performance and visual feel remain human review rather than automated pass.

### Iteration 45 Hidden feel, color emblems and cross-target builds

- Catalog revision 12 retains 23 stable entries. EditMode must prove exact
  whole-run Hidden occurrence IDs for Stages 21–23, increasing `7 / 9 / 11`
  pressure, stage-local hide leads, `0.18s` transitions and late-run coverage.
- Theme 1 art coverage must import six transparent single-sprite emblems at
  512 pixels per unit. Builder and PlayMode must bind the same color-indexed
  sprites to every pooled gate and bottom color-order HUD slot.
- Hidden runtime coverage must prove there is no `HIDDEN` or instructional
  marker, the emblem visibly fades and contracts during ordinary live approach,
  the neutral frame remains, and judgment still uses the memorized target.
  Echo/Flicker marker behavior and Flicker's color/emblem synchronization must
  remain intact.
- Android Gradle coverage must prove the post-generation interface is present
  even under a WebGL Editor target, while retaining idempotent Kotlin force and
  jdk7/jdk8 exclusion behavior. A real combined command started on WebGL must
  produce both fresh outputs successfully.
- Campaign Builder runs twice. Focused Hidden EditMode/PlayMode and build-tool
  EditMode, full EditMode and post-Builder PlayMode, two Campaign simulations
  and two Step 10 simulations are required. All eight authoritative artifact
  hashes, package files, ProjectSettings and persistence snapshots must remain
  exact.
- Final evidence: Hidden EditMode `10/10`, Hidden PlayMode `7/7`, live approach
  `1/1`, WebGL-start build-tool EditMode `7/7`, full EditMode `450/450`, full
  post-Builder PlayMode `249/249`, two byte-identical 460-row Campaign runs and
  two byte-identical 80-row / 64,016-run Step 10 runs. The real combined build
  produced a 68,774,305-byte APK and a 155,566,553-byte WebGL folder with an
  exact `540 x 960` / `9:16` canvas.

### Iteration 45A Hidden neutral-frame correction

- A fully hidden Experiment or Campaign gate must use the exact neutral shared
  material used by hidden Camouflage, not the assigned color-emissive material
  with only base-color property overrides.
- Hidden observation and transition keep the existing emblem fade/contract,
  timing and geometry. Judgment retains the memorized target color, and Retry
  must restore the assigned color material before observation restarts.
- Focused `HiddenPlayModeTests` must cover final neutral material and Retry
  restoration. Campaign Hidden tests must cover forced and live-approach final
  neutral material while retaining ordinary judgment.
- Final evidence: focused Hidden PlayMode `7/7`, full EditMode `450/450` and
  full PlayMode `249/249`. No Builder, simulation or test build is required
  because Scene structure, deterministic content, timing, balance, generation
  and judgment do not change.

### Iteration 45B Hidden power-down flicker

- The power-down envelope must be deterministic, remain inside `0..1`, contain
  three visible recovery segments and finish exactly at zero.
- Experiment runtime coverage must prove frame/emblem recovery remains
  synchronized and the completed gate uses the exact neutral shared material.
- Hidden timing, target, geometry, collider and judgment must remain unchanged.
  Retry continues to reset the presentation to full assigned color.
- Final evidence: focused Hidden EditMode `15/15`, focused Hidden PlayMode
  `8/8`, full EditMode `451/451` and full PlayMode `250/250`. No Builder,
  simulation or test build is required because no Scene, deterministic content,
  timing, balance, generation or judgment input changes.

### Iteration 46 Flicker campaign block

- Core tests must prove boundary precedence at 24 scalar path units, completion
  of a transition begun outside the boundary, exact `0.12s` progress and dual
  acceptance of previous/next colors for both player and held Echo.
- Catalog tests must prove revision 13 contains stable Stages 1–26; Stages
  24–26 are Flicker-only with exact 2/2/3-color, 6/8/10-occurrence, timing,
  difficulty and Shield-on/Booster-off contracts.
- PlayMode tests must reject a `FLICKER` marker, verify the left-to-right wipe,
  confirm the wipe has no collider, and cover Pause, Retry, transition and
  locked judgment behavior in Experiment and Campaign presentation.
- Stage Catalog Builder and Campaign Builder must each run twice. Full EditMode
  and post-Builder PlayMode, two Campaign simulations and two Step 10
  simulations are required because catalog, Scene structure, timing and
  judgment changed. Test builds remain excluded at the user's request.
- Final evidence: focused Flicker EditMode `22/22`, focused Flicker PlayMode
  `11/11`, full EditMode `455/455`, full PlayMode `250/250`, two byte-identical
  520-row Campaign runs, and two byte-identical 80-row / 64,016-run Step 10
  runs. All Builders passed twice and no missing/duplicate generated reference
  failure was reported.

### Iteration 47 Flicker path and emblem transform

- PlayMode must prove the next-color path rises on the left, crosses the top
  and descends on the right, uses no Collider and is removed after completion.
- The old and next emblem sprites must both remain active during transition,
  use the same Core progress and collapse to the committed single sprite on
  completion, Retry and pooled reuse.
- Campaign Builder must run twice. Shader compilation, generated references,
  unique root, EventSystem and Build Settings remain part of post-Builder full
  PlayMode validation.
- Final evidence: focused Flicker PlayMode `12/12`, full EditMode `455/455`
  and post-Builder full PlayMode `251/251`. Campaign/Step 10 simulations and
  player builds are excluded because deterministic inputs and judgment do not
  change and the user owns Android/WebGL test builds.

### Iteration 48 Flicker frame dissolve and break color

- Graphics-capable EditMode must load both Flicker shaders and reject every
  compiler error. A headless-only pass is insufficient because Iteration 47's
  emblem shader error reproduced only on the active D3D graphics compiler.
- PlayMode must prove the existing three neon meshes receive one transition
  material, progress in left/top/right order and restore the committed ordinary
  material on completion and pooled reuse. No `FlickerGatePath` or
  `LineRenderer` may remain.
- Gate-break material mapping must use `StageGateView.AssignedColor`, so both
  Campaign and Experiment share the committed current Flicker color rather
  than the authored initial plan color.
- Campaign Builder runs twice, followed by full EditMode and post-Builder
  PlayMode. Simulations and player builds remain excluded because Core timing,
  judgment, deterministic content and balance do not change.
- Final evidence: graphics Flicker shader EditMode `2/2`, focused Flicker
  PlayMode `13/13`, full EditMode `457/457`, full PlayMode `252/252`, and two
  Campaign Builder passes with no missing or duplicate generated reference.

### Iteration 50 Flicker screen order, timing and Booster access

- `CampaignMechanicStageTests.FlickerBlock_UsesIncreasingPressureAndAuthoredTiming`
  must require `1.35 / 1.17 / 0.99s`, Campaign Booster availability,
  deterministic `6 / 8 / 10` occurrences, first occurrence beyond authored
  Booster reach and identical occurrence IDs after Reset/Retry.
- `CampaignStagesTwentyFourThroughTwentySix_EnableOnlyFlicker` must retain
  Flicker-only modifier isolation, Shield availability and now require Booster
  availability.
- Flicker PlayMode must prove chase-view screen-left-up, top-left-to-right and
  screen-right-down progress on the existing meshes, with unchanged emblem,
  collider, transition and pooled-reset behavior.
- Stage Catalog Builder runs twice. Full EditMode and PlayMode plus two Campaign
  simulations are required because Catalog timing, item availability and
  deterministic target planning changed. Step 10 is omitted because Experiment
  inputs are unchanged; player builds remain user-owned.
- Final evidence: focused Campaign EditMode `15/15`, focused Flicker PlayMode
  `13/13`, full EditMode `457/457`, full PlayMode `255/255`, two identical
  520-row Campaign runs and no Missing Script/reference validation failure.

### Iteration 52 Campaign result experience

- Pure EditMode coverage must prove the clear page order, every failure page
  transition, empty and populated consequence queues, invalid transition
  rejection and the post-Continue remaining-gate calculation.
- PlayMode must prove Campaign Clear shows celebration before sequential
  transaction-backed reward rows, while Experiment retains its legacy result
  hierarchy. Empty reward rows remain hidden. The Product Heart refund must
  still succeed while no active reward row contains `HEART RETURNED`.
- Failure PlayMode must show the authoritative Continue cost and remaining
  gates, require Give Up confirmation, preserve the Heart count through that
  confirmation, traverse queued consequences and expose Try Again/Lobby only
  on the final page.
- Scene/Builder validation requires all result references, exactly 16 firework
  sparks, exactly five reward rows, no duplicate generated roots, one
  EventSystem and the approved Build Settings order. Campaign Builder runs
  twice and full PlayMode follows the generated Scene.
- Visual Q2 must run on a graphics-capable backend at `1080 x 1920`. It must
  capture celebration, rewards, Continue offer, Give Up confirmation and final
  choice. Continue and final-choice Retry buttons require a valid visible skin
  background and icon before capture; RenderTexture creation warnings fail the
  evidence gate.
- Final evidence: Campaign Builder passes 5 and 6 succeeded; EditMode
  `465/465`; PlayMode `257/257`; two D3D11 visual runs `1/1` each. Continue
  Offer images match SHA-256
  `114848C2ACBF940BA11F656DB0EB8B8B46A756CBD8E87A588E2EEB6F322F6F1F`
  and Final Choice images match
  `6C7C8F549CFD46435E5984897DFC0C61160FF9029B06770ECD98B943B90F17D4`
  across both runs. Package, ProjectSettings and Product-save hashes remain at
  the authoritative baseline.
- Campaign and Step 10 simulations are not required because the change owns
  no deterministic content, balance, timing, movement, generation or judgment
  input. Celebration satisfaction, pacing, device readability and touch
  comfort remain human review.

### Iteration 52A silent Heart refund copy

- The Product-backed Campaign clear test must prove the finite Heart is
  refunded while the visible result contains only earned Coin/item rows and no
  active text contains `HEART RETURNED`.
- Final evidence: focused PlayMode `1/1`, EditMode `465/465`, PlayMode
  `257/257`, and D3D11 visual Q2 `1/1` at `1080 x 1920`.
- Builder, simulations and Player builds are not required because no Scene,
  deterministic gameplay input, economy rule, save schema or package setting
  changes.

### Iteration 53 Lobby navigation and Shop layout

- EditMode must prove Shop Back returns Home, all five Featured and six Coin
  Vault cards map from the existing `CommerceProductCatalog`, reward summaries
  are non-empty and no provider price or successful purchase state is invented.
- PlayMode must prove Home and Shop share the persistent top/bottom shell, only
  Home and Shop are interactable, Home content is absent on Shop, all 11 cards
  exist in a scrollable content root, all `STORE OFFLINE` actions are disabled
  and Back restores Home without an exit modal.
- Frontend Builder runs twice. Generated-reference, unique-root, one-
  EventSystem and Build Settings checks remain mandatory.
- Visual QC runs on D3D11 at `1080 x 1920` and captures Home plus Shop. It may
  assess clipping, overlap and page separation, but cannot certify device touch
  comfort, store appeal or purchase comprehension.
- Final evidence: Frontend Builder `2/2`; focused Shop EditMode `2/2`; focused
  navigation PlayMode `1/1`; full EditMode `468/468`; full PlayMode `258`
  passed with the opt-in capture intentionally ignored; separate graphics
  capture `1/1`. Product-save, Package and ProjectSettings baselines remain
  unchanged. Campaign and Step 10 simulations and Player builds are excluded
  because no deterministic gameplay, balance, timing, judgment or release
  artifact changes.

### Iteration 54 Lobby pager, Shop art and Journey progression

- EditMode must prove the exact five-page order, Back-to-Home routing, all 11
  unique Shop hero mappings and semantic reward quantities, plus 18 Journey
  milestones and their Collected/Current/Locked/Coming Soon state boundaries.
- PlayMode must prove all five tabs navigate, horizontal drag moves exactly one
  adjacent page, vertical drag does not page, Rank/Collection are truthful,
  Journey exposes 18 views and Shop exposes 11 hero images without enabling a
  store action.
- Frontend Builder runs twice. Generated references, one generated root, one
  EventSystem, no Missing Script/reference and Build Settings order remain
  mandatory.
- Visual QC runs on D3D11 at `1080 x 1920` and captures Home, three Shop
  depths, Rank, two Journey positions and Collection. It checks clipping,
  overlap, package-volume hierarchy and truthful unavailable states, but not
  device gesture feel or commercial appeal.
- Final evidence: Builder `2/2`; EditMode `473/473`; PlayMode `259` passed,
  zero failed and one opt-in visual capture ignored; graphics capture `1/1`;
  fresh-context checker `PASS`. Campaign/Step 10 simulations and Player builds
  are excluded because gameplay inputs and release artifacts do not change.

### Iteration 55 Journey chapters, theme selection and League loop

- EditMode must prove chapter unlock boundaries `0 / 6 / 12`, latest-unlocked
  legacy fallback without a dirty load, atomic selection/save failure rollback,
  same-transaction chapter auto-selection and complete three-theme Sprite
  imports. League selection must be stable, visit every live Stage once per
  cycle and avoid an immediate boundary repeat.
- PlayMode must prove chapter selection changes and persists Home art, Journey
  has exactly three chapter views plus one League terminal, and a fully cleared
  live Catalog launches a stable `CampaignRunKind.League` request. Ordinary
  authored progression and all existing Campaign result behavior remain in the
  full suite.
- Frontend/Boot Builder runs twice after the final layout change. Campaign
  Builder runs its two-pass command because the integrated Campaign controller
  changes. Missing Script/reference, unique roots, EventSystem and Build
  Settings checks remain mandatory.
- Two full 520-row Campaign simulations must retain the approved Summary / JSON
  / CSV hashes. Step 10 is omitted while Experiment inputs stay unchanged.
- D3D11 visual QC at `1080 x 1920` captures all three Home themes, Shop depths,
  truthful placeholder pages, Journey chapter/Coming Soon positions, League
  locked/active and League Home. It can reject clipping or overlap but cannot
  certify theme appeal, touch comfort or replay variety.
- Final evidence: EditMode `480/480`; PlayMode `261` passed, zero failed and one
  opt-in visual ignore; D3D11 `1/1` with 13 captures; two byte-identical 520-row
  Campaign runs retaining all three approved hashes; developer save byte- and
  timestamp-exact; no Package or ProjectSettings feature diff; fresh-context
  read-only Quality Graph checker `PASS` with no blocking finding.

### Iteration 56 Campaign victory celebration pacing

- PlayMode must prove an immediate result tap cannot skip, skip becomes
  available at `0.1s`, manual and automatic exits both retain the `0.25s`
  crossfade, and navigation remains hidden until sequential rewards complete.
- Scene/Builder validation requires exactly 24 fixed sparks, two fixed emblem
  echoes, complete references, no duplicate generated root, one EventSystem
  and the approved Build Settings order. Campaign Builder runs twice.
- D3D11 visual Q2 at `1080 x 1920` captures separate impact and fireworks
  beats plus the reward page. It may reject missing materials, clipping,
  invisible sparks or a hard visual discontinuity, but not certify satisfaction
  or repeated-play pacing.
- Full EditMode and post-Builder PlayMode are required. Campaign and Step 10
  simulations are omitted because no Core rule, authored Stage data, movement,
  generation, judgment or Experiment input changes.
- Final evidence: Campaign Builder `2/2`; focused repaired PlayMode `1/1`;
  EditMode `480/480`; PlayMode `262` passed, zero failed and one intentional
  opt-in visual ignore; D3D11 `1/1` with 14 captures. Product save remains
  byte-exact; Package and ProjectSettings have no feature diff. A fresh-context
  read-only Quality Graph checker returned `PASS` with no blocking finding.

### Iteration 57 Campaign victory firework visibility

- Scene/Builder validation requires exactly 36 fixed streak objects and three
  fixed flash cores, with every Image referencing the normalized transparent
  VFX Sprite. Campaign Builder runs its two-pass command and retains all unique
  root, EventSystem, Build Settings and missing-reference checks.
- PlayMode must retain the Iteration 56 hold/skip/crossfade contract, prove a
  radial burst and its flash core are simultaneously active, and keep rewards
  hidden until the existing transition.
- D3D11 visual QC at `1080 x 1920` captures impact, expansion and final-burst
  beats before rewards. It may reject invisible or rectangular-placeholder
  sparks, missing/error materials, clipping or a missing late burst, but cannot
  certify subjective celebration impact.
- Campaign and Step 10 simulations remain omitted because Core rules, authored
  Stage data, movement, generation, judgment and Experiment inputs do not
  change.
- Final evidence: Campaign Builder `2/2`; focused PlayMode `1/1`; EditMode
  `480/480`; PlayMode `262` passed, zero failed and one intentional opt-in
  visual ignore out of `263`; D3D11 `1/1` with 15 captures. Product save is
  byte- and timestamp-exact; Package and ProjectSettings have no feature diff.
  A fresh-context read-only Quality Graph checker returned `PASS` with no
  blocking finding.
