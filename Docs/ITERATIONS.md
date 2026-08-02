Iteration 0

Completed

Experiment Lab

Goal

Continue

Stage4

---

Findings

3 Tap works

4 Tap rare

Rhythm > Random

Mechanics unlock by Stage

---

Next

Experiment Runtime Flow

Clone

Flicker

---

Iteration 1

Completed

Experiment Runtime Flow

Stage4 Runtime / Simulation Color-Cycle Parity

Countdown

Failed / Completed Results

Retry Same Test

Replay

Back to Lab

---

Findings

Stage4 gate 5 failure was a simulation rule mismatch, not balance.

Runtime used three active colors while simulation calculated the intro with two.

One shared forward-cycle rule removes the invalid mismatch.

Fixed result listeners avoid duplicate event subscriptions on reentry.

---

Validation

EditMode 217 / 217

PlayMode 111 / 111

Stage4 Perfect / No Item / First Attempt 0% → 100%

Step10 report hashes unchanged

---

Human Feedback Next

Countdown readiness

Failure cause readability

Retry Same Test clarity

Back to Lab clarity

---

Next

Clone

Flicker

Pattern Generator

---

Iteration 2

Completed

Campaign Movement Speed 2x

Campaign Gate / Booster / Goal Spatial Scale 2x

Experiment Lab Isolation Preserved

---

Human Feedback

Campaign speed did not feel strong enough during play.

---

Decision

Double campaign speed and world distances together.

Keep cadence and time-based decision windows unchanged.

Keep Experiment Lab values unchanged.

---

Validation

EditMode 217 / 217

PlayMode 111 / 111

Scene Builder two-build structural validation passed

All 100 campaign profile / item rows kept identical non-spatial metrics

Four next-gate distance metrics scaled exactly 2x

Step10 report hashes unchanged

---

Human Feedback Next

Perceived speed on portrait mobile

Gate readability

Booster exit comfort

Continue and Goal approach comfort

---

Next

Clone

Flicker

Pattern Generator

---

Iteration 3

Completed

Clone-only Experiment

---

Implemented Contract

Source indices 3 and 6

Exactly two same-color Clone Gates

GapSeconds 0.45 at normal Experiment speed

Independent Source and Clone judgments

Shared Shield mismatch protection

Ordinary Camouflage hide, reveal, and judgment

Booster disabled for Clone-only Launcher runs

Distinct Clone Miss result cause

Campaign and Step10 matrix unchanged

---

Findings

Clone fits the existing deterministic sequence, judgment path, and six-View
pool without a second runtime or collision system.

Clone insertion does not consume the ordinary gate color PRNG, so existing
Step10 plans remain byte-identical.

Camouflage is visibility only. Hidden and revealed Clone still uses ordinary
judgment and Shield protection.

---

Validation

EditMode 231 / 231

PlayMode 117 / 117

Scene Builder two-build structural validation passed

Campaign simulation summary, JSON, and CSV hashes unchanged

All five Step10 report hashes unchanged

Clone Perfect simulation replay identical at the same seed

ProjectSettings and package hashes unchanged

---

Human Feedback Next

Clone relationship recognition

0.45-second gap readability

Clone hazard recognition

Shield and Camouflage presentation clarity

Portrait mobile transparency and screen complexity

---

Deferred

Flicker

Clone color variation

Multiple or chained Clone

Combined-mechanic balancing

Campaign Clone

Pattern Generator

---

Iteration 4

Status

Approved / In Progress

---

Contract

StageCatalogAsset single source

Pure Core Adapter and IStageCatalog

Shared GateModifier

EchoOfferCoordinator

Echo Modifier replacing Clone Gate

Camouflage ETA Reveal

StageSpeedProfile

Stage-local Shield / Booster

Campaign Stage 6–11

Simulation-selected color and pacing values

---

Baseline

EditMode 231 / 231

PlayMode 117 / 117

Stage 1–5 simulation hashes unchanged

Step10 report hashes unchanged

---

Human Feedback Required

Echo frequency and comprehension

Echo / Shield readability

Camouflage reveal lead

Stage speed feel

Stage 6–11 difficulty and finale quality

## Iteration 4 completion

Result

- Replaced active Clone gameplay with one ordinary-gate Echo modifier.
- Moved the campaign to one Inspector-authored 11-stage catalog without
  duplicating generation, judgment, movement, or persistence systems.
- Added deterministic ETA Camouflage and sampled speed curves shared by
  runtime and simulation.
- Added teaching stages for local Shield, local Booster, Camouflage, Fog,
  Ice, and Echo.

Learning

- The first three-color Stage 8-10 candidate produced Average/None first-clear
  rates of `21.2%`, `20.0%`, and `18.5%`. Isolating the new mechanic with two
  colors raised them to `31.2%`, `30.5%`, and `27.9%` without changing the
  mechanic or speed contracts.
- Stage 6 is mechanically easier because its local Shield is working as
  intended. Stage 7 failures concentrate before the 30% Booster grant; human
  play must decide whether that opening communicates the coming mechanic.
- Exact Stage 1-5 row equality and exact Step 10 file hashes show that the
  expansion did not silently alter the previous campaign or experiment matrix.

Validation

- EditMode 248/248; PlayMode 125/125.
- Full Stage 1-11, profile, and item matrix completed under
  `Artifacts/Simulation/MechanicCampaign-*`.
- Human feedback remains the gate for Echo comprehension, reveal timing,
  speed feel, difficulty, and finale quality.

## Iteration 4 stage-progression hotfix

Human finding

- After Stage 1 clear, the lobby displayed Stage 2 but Play could start Stage
  1 again. The saved state correctly contained Stage 1 clear and
  `HighestUnlocked = 2`; the failure was after persistence.

Cause and correction

- Catalog-driven selection changed `_selectedStageNumber` in `ShowLobby()`
  without changing `_selectedStageId`.
- `PlayFromLobby()` correctly used stable IDs, but therefore used the stale
  Stage 1 ID. `ShowLobby()` now synchronizes both values from the same
  `StageDefinition`.

Validation

- EditMode 248/248; PlayMode 126/126.
- A PlayMode regression verifies Stage 2 and Stage 3 lobby displays start the
  corresponding sessions.
- The change does not alter stage data, deterministic generation, balance,
  packages, ProjectSettings, or saved-record format.

## Iteration 5 provided-item timing and high-speed progression

Human findings

- Stage 7 displayed `BOOSTER: PROVIDED` but activated the grant only after
  30% progress, unlike a selected Booster or the provided Stage 6 Shield.
- Around the same transition, Booster speed could carry the kinematic player
  completely past the gate Trigger between physics updates. The unresolved
  gate then blocked pool progression and left an endless empty track.
- Allowing item selection before the first item-teaching stage confused the
  campaign introduction.

Implemented

- Stage 7 now activates its provided Booster at `GO`.
- Stages 1-5 reject both item selections in catalog data and show `LOCKED` in
  the existing PreRun controls.
- Movement checks the current unresolved gate plane after advancing and calls
  the same one-shot `TryResolveCrossing()` used by the physics Trigger.
- Legacy item behavior tests now run on Stage 8, the first ordinary campaign
  stage after both provided-item teaching stages, instead of bypassing the new
  Stage 1-5 lock contract.

Validation and learning

- EditMode 249/249; PlayMode 128/128.
- Stage 7 starts at Booster speed `110`, resolves all 40 gates, and reaches
  Goal without growing the fixed six-gate pool.
- Stage 6-11 validation covers 120 matrix rows and 96,024 modeled runs.
  Stages 6 and 8-11 are row-identical to the prior baseline; only Stage 7 has
  the 20 intended contract changes.
- Stage 7 Average/None first-clear remains nearly flat
  (`47.6% -> 47.0%`) while Continue-clear rises
  (`84.8% -> 95.1%`). Earlier protection also shortens median clear time
  (`33.203s -> 31.356s`) and increases average Booster bypasses
  (`9.780 -> 15.774`).
- All Stage 6-11 Perfect rows clear and all continuity/pool violation counters
  remain zero. A fresh Step 10 matrix reproduced all five baseline hashes.
- Human play still owns the decision on comprehension, speed feel, comfort,
  difficulty, and fun.

## Iteration 5 build-test tooling follow-up

Need

- Human feedback now requires repeatable Android-device and browser builds
  without manually re-entering scenes, targets, and output paths.

Implemented

- Added one shared Editor build path under
  `Tools > Color Gate Runner > Test Builds`.
- Android APK, WebGL, combined build, and output-folder commands use the
  enabled Build Settings scenes and ignored `Builds/Test` outputs.
- Test builds use Development mode. Android temporarily disables App Bundle
  output and restores the previous setting after success or failure.
- WebGL temporarily uses a `540 x 960` logical canvas and the project-owned
  `ColorGateRunnerPortrait` template. The template centers an exact `9:16`
  frame with neutral letterboxing, including inside square or landscape hosts,
  and the previous width, height, and template settings are restored afterward.
- Scene, module, platform-switch, and `BuildReport` failures are explicit.
  No gameplay, packages, or ProjectSettings values were changed.

Validation and learning

- The original editor compiled the menu without errors. AndroidPlayer and
  WebGLSupport are installed and were discovered by Unity.
- Five focused EditMode assertions pass for the scene list, both targets,
  deterministic output locations, Development mode, exact portrait frame, and
  temporary WebGL setting restoration. The full EditMode suite passes 283/283.
- An isolated Android invocation completed platform switching, scripts,
  Android shaders, and entered the Player backend. The copied-Library backend
  then stopped progressing before APK output, so it was stopped.
- A subsequent independent isolated WebGL invocation completed successfully
  and produced a 124,340,289-byte player. The generated page was checked for
  `540 x 960` canvas dimensions and exact `9:16` CSS, and the prior validation
  project settings were restored.
- Actual APK installation and WebGL browser play remain human acceptance work.
  The successful file build alone does not claim device input, browser resize,
  or gameplay compatibility.

## Iteration 6 Hidden-only Experiment (originally named Flicker)

Status

- Completed

Contract

- Ordinary-gate Hidden modifier, not a separate gate.
- Deterministic selection inside the existing Experiment sequence.
- Minimum readable duration plus shared effective-speed ETA hide condition.
- One-way target-color and symbol hide with retained neutral silhouette,
  judgment opening, and `HIDDEN` identity.
- Existing Player, Echo, Shield, Failure priority.
- Deterministic Retry/Replay reset.
- Booster disabled and modifier combinations deferred.
- Campaign Stages 1-11 and Step 10 matrix unchanged.

Human feedback required

- Hidden concept recognition.
- Readable observation duration.
- Hide lead and memory interval.
- Transition clarity.
- Gate-position retention.
- Echo/Shield comprehension.
- Portrait readability and repeated-entry fatigue.

Implementation

- Added the modifier now named `Hidden` and the settings now named
  `HiddenSettings`.
- Extended the existing deterministic Experiment sequence with an independent
  Hidden selection mask so gate count and existing color/spacing PRNG output
  remain unchanged.
- Added a pure-Core one-way visibility state used by the pooled gate View.
- Reused effective-speed ETA, existing materials, rich-text symbol alpha,
  ordinary judgment, failure, Retry/Replay, Launcher, and six-View pool.
- Disabled Booster only for the Hidden Launcher condition.

Validation and learning

- Original Editor compilation completed without C# errors.
- EditMode 262/262 and PlayMode 134/134 passed.
- The Scene Builder completed both consecutive builds, and its rebuilt scene
  passed PlayMode 134/134.
- The 220-row, 176,044-run Stage 1-11 campaign output reproduced all three
  baseline artifact hashes.
- All five Step 10 artifact hashes reproduced exactly.
- The architecture can express the approved one-way information hide without
  a second gate or judgment system. Human play must determine whether the
  default one-second observation, `0.65s` hide lead, `0.12s` transition, and
  retained marker are perceptually clear.

Deferred

- Campaign Hidden.
- Hidden combined on a gate with Camouflage, Fog, Ice, or Echo Provider.
- A color-changing mechanic, implemented later as the distinct Flicker.
- Repeating flash patterns.
- New failure flow, input, package, Pattern metadata, or Section metadata.

## Iteration 7 Hidden migration and color-cycling Flicker

Status

- Implemented / Human Review Pending

Play

- The existing memory mechanic must remain intact but its Flicker name
  conflicts with a newly approved visible color-cycling timing mechanic.

Analyze

- Existing Flicker owns serialized enum values `16` and `5`, deterministic
  sequence selection, one-way visibility state, ordinary judgment, and pooled
  presentation.
- Experiment Gameplay Time already advances only while Playing and resets on
  Retry/Replay, so it can be the single absolute time source.

Design

- Rename the existing mechanic to Hidden while preserving enum numbers,
  serialized Launcher fields, seed-selected gate IDs, hide timing, and
  judgment.
- Add new numeric values for a visible two-/three-color Flicker modifier.
- Store deterministic cycle colors and phase in the gate plan; use one pure
  Core time calculation for View, collision judgment, tests, and simulation.
- Require minimum pooled-view exposure, retain Player/Echo/Shield/Failure,
  disable Booster, and defer Campaign, music, and modifier combinations.

Baseline

- Clean worktree; EditMode 262/262; PlayMode 134/134.
- Echo/former-Flicker targeted cases 12/12.
- Campaign three hashes and Step 10 five hashes exactly match Iteration 6.
- Package and ProjectSettings diffs are empty.

Implementation

- Preserved the former modifier/mechanic values `16` and `5` as Hidden and
  assigned new Flicker values `32` and `6`.
- Renamed the former settings, runtime visibility state, pooled View state,
  Launcher choice, marker, tests, and logs to Hidden without changing the
  hide contract.
- Mapped every former Launcher field name to Hidden with
  `FormerlySerializedAs`. New Flicker fields use a separate
  `colorCycleFlicker` prefix so old serialized values cannot bind to them.
- Extended the existing deterministic Experiment plan with fixed unique
  cycle colors, interval, deterministic phase offset, transition pulse,
  Gate ID, base color, and selection seed.
- Added one pure-Core absolute Gameplay Time phase calculation shared by
  presentation, collision judgment, simulator, and tests. Exact boundaries
  select the new phase.
- Kept Player, Echo, Shield, and ordinary failure behavior on the shared
  resolution path. Hidden and Flicker Launcher conditions disable Booster.
- Reused the fixed six-View pool and existing spacing/speed ETA calculation
  for minimum visible-cycle eligibility.

Validation

- EditMode 281/281 passed.
- PlayMode 143/143 passed before scene regeneration.
- Scene Builder succeeded twice; the regenerated scene then passed PlayMode
  143/143.
- Echo 27/27, Shield 29/29, Camouflage 9/9, Hidden 25/25, Flicker 27/27,
  and Experiment Runtime 5/5 related cases passed.
- The full Stage 1-11 campaign regression wrote 220 rows, including the
  requested Stage 1-5 coverage, and reproduced all three baseline hashes.
- Step 10 wrote 80 rows and 64,016 runs and reproduced all five baseline
  hashes.
- Missing MonoBehaviour, required references, pools, duplicate roots, Lab
  return/re-entry, Package manifest/lock, and ProjectSettings checks passed.

Learning

- `FormerlySerializedAs` is not enough if a new field reuses the exact former
  name. Keeping the new Flicker serialized namespace distinct is required to
  make the old-to-Hidden migration unambiguous.
- A View can be rebound to the next pooled gate immediately after crossing;
  collision tests must capture the pre-crossing presentation color and compare
  it with the session's recorded authoritative judgment color.

Human feedback required

- Hidden/Flicker concept separation.
- Two- and three-color switch speed and collision-boundary comprehension.
- Pulse, symbol synchronization, Echo/Shield explanation, portrait
  readability, and minimum observation at speed.

## Hidden lead-time and Flicker palette-order revision

Status

- Implemented / Human Review Pending

Play

- Hidden did not retain its target information long enough before hiding to
  create the intended memory challenge.
- Random two-/three-color Flicker cycles could jump against the player's
  forward color order, requiring an unachievable burst of taps near judgment
  and encouraging memorization through failure.

Analyze

- Hidden hide starts when minimum observation has elapsed and ETA reaches
  `HideLeadTimeSeconds`; increasing that lead hides earlier and lengthens the
  memory interval.
- Experiment player input already owns one active-palette order, but Flicker
  independently selected unique colors with a local seed.

Design

- Raise Hidden hide lead `0.65s -> 0.85s`; retain observation and transition.
- Derive Flicker cycles from the full active palette, begin at gate base color,
  and advance with the exact same Core next-color rule as player input.
- Remove the authored `2/3` cycle-count setting while preserving target
  selection, phase, timing, judgment, retry, View, and Campaign boundaries.

Implementation

- Added `ExperimentDefinition.GetNextColor` and reused it for both
  `ExperimentSession.TryCycleColor` and Flicker plan construction.
- Flicker plans now contain all selected Experiment colors exactly once in
  forward input order. Seeded random color selection was removed.
- Updated Core, Launcher, and `SampleScene` Hidden defaults to `0.85s`.
- Removed the Launcher and Core Flicker cycle-count setting. Plan
  `CycleColorCount` remains a derived value for phase calculation.

Validation

- Baseline: EditMode 283/283; PlayMode 143/143.
- Final: EditMode 288/288; PlayMode 144/144.
- Hidden/Flicker focused cases: EditMode 14/14 and 20/20; PlayMode 6/6 and
  10/10.
- Scene Builder succeeded twice and post-Builder PlayMode passed 144/144.
- Stage 1-11 Campaign produced 220 rows with all three hashes unchanged.
- Step 10 produced 80 rows and 64,016 runs with all five hashes unchanged.
- Package manifest/lock, ProjectSettings, and `git diff --check` remain
  regression boundaries for final handoff.

Learning

- A Core field initializer does not replace an older value already serialized
  into a Scene. Balance-default changes must update both the code default and
  existing authored assets, then verify a regenerated Scene.
- Sharing the next-color rule makes Flicker response demand legible: each
  phase change is one forward tap if the player is tracking the displayed
  color. Automation cannot establish whether `0.50s` feels achievable.

Human feedback required

- Whether Hidden `0.85s` produces a useful memory interval.
- Whether full-palette Flicker remains followable from three through six
  colors and feels skill-based at the collision boundary.

## Campaign Stage 12 Hidden / Stage 13 Flicker

Status

- Implemented / Human Review Pending

Play

- Campaign currently ends at Stage 11. Hidden and Flicker are fully testable
  only through isolated Experiment Launcher conditions.
- Stage 6+ already establishes selectable Shield and Booster. Disabling
  Booster only for Flicker would make the stage-selection contract
  inconsistent and would hide the real interaction that needs evaluation.

Analyze

- Campaign `GatePlan` lacks Flicker cycle metadata, while
  `ExperimentGatePlan` already owns deterministic cycle and phase data.
- Campaign View binding currently routes Camouflage/Fog only; Hidden/Flicker
  presentation and collision-time judgment remain Experiment-only.
- The campaign simulator resolves a static planned color before advancing its
  travel clock, so Flicker requires an explicit shared collision-time input.
- Catalog-driven lobby buttons scale with `StageCatalog.Count`; the Resource
  asset, builder upgrade threshold, serialized Scene arrays, and tests must
  move together from 11 to 13.

Design

- Add Stage 12 Hidden Memory and Stage 13 Flicker Flow without changing Stage
  1-11 data.
- Share modifier selection, cycle metadata, phase calculation, and judgment
  color between Experiment, Campaign runtime, View, simulation, and tests.
- Keep Booster selectable and unchanged. Validate minimum exposure using the
  fastest effective arrival speed and retain deterministic later encounters.
- No mixed modifiers, new colors, new input, music integration, or duplicate
  generator.

Baseline

- Clean worktree and `git diff --check`.
- EditMode `288/288`; PlayMode `144/144`.
- Stage 1-11 Campaign reproduced all three Iteration 7 hashes.
- Step 10 reproduced 80 rows, 64,016 runs, and all five Iteration 7 hashes.

Implementation

- Added Catalog stages `stage-12` and `stage-13`, revision 5 Resource data,
  and catalog-driven Scene buttons.
- Extracted shared `FlickerGatePlan` and deterministic occurrence/cycle/phase
  planning so Experiment and Campaign do not own parallel systems.
- Extended `GatePlan`, `StageDefinition`, and `StageSession` with Campaign
  Hidden/Flicker data and exact Gameplay Time judgment.
- Campaign View binding now reuses the existing Hidden/Flicker presentation
  and resets it through the existing six-gate pool.
- Simulation supplies collision Gameplay Time to the same `GatePlan`
  judgment-color path used by runtime.
- Booster remains selectable and unchanged. Exposure eligibility uses
  Booster speed without making item-dependent plans.

Validation

- Focused new Core/EditMode `6/6`; focused Campaign PlayMode `3/3`.
- Full EditMode `294/294`.
- Full PlayMode `147/147`; second Scene Builder run followed by another
  `147/147`.
- Stage 1-13 simulation: 260 rows. Stage 1-11 prefix has zero differences;
  Perfect clears Stage 12/13 in all four item combinations; continuity error
  counters are zero.
- New Summary/JSON/CSV hashes:
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 retained all five hashes. Package manifest/lock and ProjectSettings
  remain unchanged; `git diff --check` passes.

Learning

- Booster does not need a Flicker-specific disable. Planning against its
  fastest arrival speed plus later deterministic occurrences preserves the
  real item contract and still exposes the mechanic after early bypasses.
- A collision-time modifier must pass explicit Gameplay Time through runtime
  and simulation; advancing an independent simulator timer would create a
  second judgment system.
- Average/None first-clear drops from Stage 11 `26.9%` to Stage 12 `16.2%`.
  This exceeds the review threshold but is evidence for human play, not
  authority for automatic balance changes.

Human feedback required

- Stage 12 Hidden concept, `0.85s` memory demand, and Stage 11 -> 12 jump.
- Stage 13 forward rotation, `0.50s` boundary response, and post-Booster
  teaching frequency.
- Shield/Booster feedback, marker legibility, and 9:16 portrait readability.

## Iteration 9 — Commercial Flow Foundation 1

### Play

- The playable entry was the generated Campaign/Experiment `SampleScene`.
  There was no Boot Scene, persistent composition root, production Scene
  transition, local profile, settings persistence, or product-save owner.
- Existing Stage progression already persisted independently through
  `PlayerPrefsStageProgressStore` using the highest-unlocked and per-display-
  number record keys.

### Analyze

- Reusing the Stage PlayerPrefs as a new product save would create two owners
  and an unsafe migration. This Iteration therefore leaves progression fully
  untouched and gives the new save only Profile and Settings.
- Unity-specific path, JSON, and Scene APIs can remain adapters around a pure
  Product assembly. A single persistent `AppRoot` is sufficient without
  service-level singletons or a global mutable locator.
- Build Settings already provide an explicit destination list. The Builder
  can serialize the first active non-Boot path before inserting Boot at index
  0, avoiding a Scene-name hardcode.

### Design

- Boot owns presentation and transition only. AppRoot owns the service graph
  and ordered initialization pipeline.
- Save writes clone the requested snapshot, apply schema/revision/write UTC,
  validate, serialize to temp, decode and validate temp, then replace primary.
- Native platforms prefer atomic replacement. Platforms such as WebGL use an
  explicitly reported backup-based recoverable policy.
- Load accepts current schema, deterministically migrates schema 0, recovers a
  corrupt primary from a valid backup, and blocks unknown future schema.

### Implementation

- Added the Unity-free `ColorGateRunner.Product` assembly, local save models,
  contracts, services, system file adapter, and initialization pipeline.
- Added Unity JSON/path/platform adapters, one persistent `AppRoot`, Boot
  controller, Loading/version/error/Retry UI, and Safe Area support.
- Added an idempotent Boot Builder, Boot-first Build Settings management, and
  enabled-scene test-build coverage.
- Added EditMode coverage for initialization, profile identity, revision/write
  ordering, roundtrip, failure preservation, backup recovery, future schema,
  migration, settings policy, injected paths, and assembly purity.
- Added PlayMode coverage for Boot structure, success transition, existing
  profile load, Retry, duplicate rejection, and Stage PlayerPrefs preservation.

### Validation

- EditMode `313/313`; PlayMode `152/152`.
- Boot Builder twice and Campaign Scene Builder twice succeeded. PlayMode
  remained `152/152` after each Builder boundary.
- Campaign 260-row and Step 10 64,016-run artifacts reproduced every approved
  hash. Gameplay, Stage Catalog, items, modifiers, Retry/Continue, and Goal
  behavior are unchanged.
- Package manifest/lock are unchanged. ProjectSettings changes only by adding
  Boot first while retaining the active Campaign Scene.

### Learning

- A new product save can be introduced safely before progression integration
  only when ownership is explicit and the new schema contains no shadow copy.
- Applying metadata before temp serialization makes the validated bytes and
  in-memory success snapshot identical.
- Cross-platform save guarantees must be named honestly. Backup recoverability
  is a valid WebGL contract but is not atomic replacement.

### Deferred

- Title, Frontend shell, Lobby/Campaign/Stage Detail redesign, result screens,
  StageProgressService migration, cloud/login, economy, content, events,
  analytics, advertising, and IAP.

### Human Review

- Confirm Boot presentation is stable without flicker in 9:16 portrait.
- Confirm Campaign transition delay feels acceptable.
- Confirm local-save error and Retry text are understandable.
- Confirm Android and WebGL restarts preserve the Guest and existing Stage
  progression, including after a forced close.

## Iteration 10 — Commercial Flow Foundation Frontend 2

### Play

- Iteration 9 entered the existing Campaign Scene directly after Boot. There
  was no Title, standalone Frontend Scene, Page Router, or product-facing home
  shell.
- The Campaign Scene already opened its authoritative Lobby and required no
  external launch Context.

### Analyze

- Reusing the existing Campaign Lobby inside Frontend would couple new Page
  state to gameplay presentation. A small Frontend Scene can instead hand off
  to the unchanged Campaign Scene.
- Reading Stage PlayerPrefs for a placeholder recommendation would add a new
  progress reader before the approved progression integration. Generic
  `CONTINUE CAMPAIGN` avoids false information and duplicate ownership.
- AppRoot's existing Scene-lifetime boundary can provide a one-time immutable
  display Context without a new locator, singleton, or Product service.

### Design

- One Router owns Title/Lobby Page, Modal, and Transition state. One Scene
  Controller binds uGUI Views, Back input, AppRoot Context, and serialized
  Scene loading.
- Direct Frontend execution has no hidden bootstrap. It shows blocking
  `BOOT REQUIRED` and disables Campaign.
- Builder selection resolves Campaign from active Build Settings before
  producing exact Boot/Frontend/Campaign order and serializing both routing
  edges.

### Implementation

- Added `Frontend.unity`, pure Router and display Context, Frontend Scene
  Controller, uGUI Title/Lobby/Modal/Loading/blocker shell, and one EventSystem.
- Added truthful Guest/account/settings presentation, hidden empty legal and
  future slots, Back rules, duplicate transition protection, and typed Scene
  load failure display.
- Added an idempotent Frontend Builder and explicit Boot destination overload.
  Existing Campaign Scene and `StageSceneController` were not modified.

### Validation

- EditMode `335/335`; PlayMode `160/160`. Both counts pass after final Builder
  runs.
- Frontend Builder twice, Boot Builder twice, and Campaign Builder's two-build
  command succeeded. No duplicate root, Camera, Canvas, EventSystem, Page,
  listener, Missing Script, or Missing Reference was found.
- Campaign remains 260 rows with all three hashes unchanged. Step 10 remains
  80 rows with all five hashes unchanged.
- Package manifest/lock retain approved hashes. `ProjectSettings.asset` is
  unchanged after baseline commit `c191826`; `EditorBuildSettings.asset` adds
  Frontend at Index 1 only.

### Learning

- A placeholder frontend is safest when it admits missing information instead
  of becoming a temporary progress authority.
- A Scene boundary may resolve the one persistent composition root and then
  immediately reduce it to an immutable View Context; Pages need no service
  access.
- Explicit serialized routing edges keep Boot and Frontend independently
  testable while avoiding runtime Scene-name inference.

### Deferred

- Final Lobby, Campaign Page, Stage Detail, Gameplay/Results shell,
  StageProgressService, Campaign-to-Frontend navigation, account/cloud,
  economy, events, analytics, ads, IAP, and final art.

### Human Review

- Confirm Boot -> Title and Lobby -> existing Campaign Lobby transitions feel
  understandable rather than repetitive.
- Confirm Guest/account/settings wording and primary actions are readable at
  9:16 small mobile sizes.
- Confirm Android Back, rapid taps, Modal dismissal, and UI-to-gameplay input
  isolation on Android and WebGL devices.
