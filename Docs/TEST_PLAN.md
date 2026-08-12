# Test Plan

## How to use this document

`CURRENT_STATUS.md` owns the latest approved counts, hashes, Build Settings,
persistence snapshots, and Authoritative Baseline. This document owns **what
must be validated and when**.

For every iteration:

1. During Phase A, inspect the baseline, affected contracts, and risks. Do not
   run the full EditMode suite, full PlayMode suite, Builders, or Simulations
   merely to begin an iteration.
2. Select the lowest Validation Tier that covers the actual change. Escalate
   when a shared controller, shared prefab, persistence boundary, stage rule,
   or deterministic input is affected.
3. Implement the approved scope.
4. After implementation, run the selected tier's required validation once.
   Focused tests may be run earlier while developing, but the complete tier is
   not repeated unless a failure or subsequent edit invalidates its evidence.
5. Record final counts and artifact evidence in `CURRENT_STATUS.md`. Move
   iteration-specific acceptance mapping and final evidence to
   `TEST_HISTORY.md`; do not append them here.
6. Use `TEST_CATALOG.md` to locate current automated tests and structural
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

## Standard Regression Suite

### Before implementation — Phase A inspection

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
- Retry replays the approved layout; Continue consumes the failed gate once
  and preserves the approved timer, speed, color, sequence, pool, and item
  state.
- Continue is capped at three successful uses per attempt. Coin sources use
  `300 / 600 / 900` successful-Coin ordinals, one completed rewarded-ad right
  remains independent of Coin use, and Retry resets the attempt-local policy.
- Coin spending must be atomic and idempotent before Core resume. Insufficient
  funds, save failure, non-completed ads, duplicate callbacks and stale
  callbacks preserve the frozen failure state. Unavailable providers expose
  no rewarded action and cannot simulate success.
- Gate and Track pools remain fixed and recycled objects reset their runtime
  state.
- Goal is part of deterministic planning, appears continuously, and clears the
  stage once.

### Campaign, progression, and items

- Catalog IDs remain unique and stable; Lobby display, PreRun, Gameplay,
  persistence, Retry, and Next Stage use the same stable Stage ID.
- Schema 2 Product Save owns runtime Stage unlock and record writes by stable
  Stage ID. Legacy device-wide Campaign PlayerPrefs are imported once and then
  retained unchanged as a rollback source; Campaign progression remains scoped
  to the local Product profile and does not derive from Guest identity.
- Locked stages cannot start; Clear unlocks only the next Stage; Retry returns
  through the existing PreRun/item-selection contract.
- Catalog revision 7 contains 20 contiguous stable IDs. Stages 1–5 remain
  unchanged; Stages 6/7 provide their named item with selection locked;
  Stage 8 is clean three-color item application. Stages 9–20 contain isolated
  Camouflage, Fog, Ice and Echo intro/practice/mastery blocks with 2/2/3
  colors. Hidden and Flicker remain Experiment-only until Stages 21+.
- Every Stage 1–20 initializes under all four requested item inputs. Runtime
  selection is rejected through Stage 7 and allowed from Stage 8 onward.
- A successful Stage 8+ Start atomically persists one decrement for each
  selected owned item before Countdown. No selection and Back before Start do
  not consume; duplicate Start consumes once; Retry starts a new attempt and
  consumes retained selections again.
- Zero stock disables only the affected toggle. Stale insufficient inventory
  and save failure remain in PreRun with truthful status and no partial
  published decrement. Stage 6/7 provided items remain free and non-consuming.
  Missing Product context disables selectable items without blocking no-item
  or provided-item starts.
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
