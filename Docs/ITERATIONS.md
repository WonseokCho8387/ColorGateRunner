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

## Iteration 11 — Campaign Progress Restore Test Isolation Hotfix

### Play

- In Unity Editor, Campaign could first show Stage 1 and later return to an
  older Stage 11 progression state. Product Guest and Campaign persistence
  were already separate by contract.
- Current Editor storage was partial: prior Records remained while Highest
  Unlocked and Stage 6 Record were absent. Their deleted values were unknown.

### Analyze

- Runtime Campaign entry synchronously reloads one device-wide
  `PlayerPrefsStageProgressStore`, selects the lowest uncleared unlocked Stage,
  and carries the displayed Stage's stable ID into PreRun and Gameplay.
- Two PlayMode fixtures deleted Highest Unlocked and Stage 6 Record in setup or
  teardown without snapshotting prior values. Windows Editor project copies
  with the same Company/Product names share that PlayerPrefs namespace.
- One stable PlayerPrefs snapshot cannot independently produce Stage 1 and
  Stage 11 across re-entry; the observed sequence requires state mutation.
  The confirmed mutation source was test cleanup, so runtime storage was not
  changed without a separate failing runtime regression.

### Design

- A test-assembly-only snapshot owns Highest plus Records 1-13 for each test.
  It captures existence and typed value, restores existing keys, deletes only
  originally absent keys, saves, and is idempotent through `IDisposable`.
- Fixture setup uses a guarded `finally` so an exception before setup completes
  restores immediately. Teardown also restores from `finally`, even if Scene
  or AppRoot cleanup fails.
- Production Campaign and Product persistence remain completely unchanged.

### Implementation

- Added the shared `CampaignPlayerPrefsSnapshot` and focused tests for existing
  integer/string keys, absent keys, all 13 Records, and exception restoration.
- Replaced destructive cleanup in Frontend and Product Boot PlayMode fixtures
  with full Campaign snapshot/restore.
- Added production-path PlayMode regression for Stage 11 first/re-entry,
  stable-ID parity, fresh Stage 1 clear and Stage 2 recreation, new Guest
  preservation, and missing/corrupt Highest fallback.

### Validation

- Focused Snapshot Utility `3/3`; focused Campaign restore `4/4`.
- Full EditMode `335/335`; full PlayMode `167/167`; post-Builder PlayMode
  `167/167`.
- Frontend Builder twice, Boot Builder twice, and Campaign Builder twice all
  succeeded without Missing Script/Reference or duplicate generation.
- Actual Editor Campaign PlayerPrefs remained 11 entries with identical hash
  `2A8AF71352FCCF9D80C8BDCB9BDC89FC3F61A7B356633C3423A847F576D0B6C9`
  before and after tests. Product save hash remained identical, so Guest ID was
  preserved.
- Campaign retained its three 260-row hashes. Step 10 retained all five hashes
  across 80 rows and 64,016 runs.
- Package manifest/lock, `ProjectSettings.asset`, Scenes, runtime Campaign
  code, Product Save Schema, Stage Catalog, and balance have no Hotfix change.

### Learning

- A separate project directory is not PlayerPrefs isolation in Unity Editor;
  Company/Product identity determines the shared namespace.
- Preservation tests are unsafe if cleanup destroys the state they claim to
  preserve. Test fixtures must restore the prior world, not reset it.
- A minimal test-only Hotfix can close the confirmed data-loss path without
  inventing a second save owner or changing runtime behavior without evidence.

### Deferred

- Previously deleted Highest and Stage 6 values cannot be recovered safely and
  are not synthesized.
- Profile-scoped Campaign persistence and migration remain a future
  `StageProgressService` Iteration.
- Full dependency injection of an in-memory Campaign store for Scene tests is
  a future testability improvement.

## Iteration 12 — Account Onboarding, Settings, and Gameplay Pause UX

### Play

- Boot entered a placeholder Title before Lobby, while first-run account
  choice, shared Settings, and an in-game Pause shell were not yet playable.
- Product Profile and Settings already had one recoverable schema-1 save owner;
  Campaign progression remained a separate device-wide PlayerPrefs system.
- Gameplay already exposed the deterministic attempt state, existing Retry,
  Booster haptic, pooled gate Views, and two attempt-owned ParticleSystems.

### Analyze

- Account choice completion describes Profile onboarding, not a preference.
  Keeping it outside Settings prevents Settings repair from reopening the flow.
- Publishing mutable service state before a save would create partial success.
  The minimum safe boundary is clone, mutate candidate, save, then perform one
  successful rebind.
- Pause cannot be presentation-only: a queued same-frame trigger could still
  resolve unless the judgment path checks coordinator state at mutation time.
- Scene-wide ParticleSystem search would capture unrelated effects. The
  Campaign Builder can serialize the two attempt-owned effects explicitly.

### Design

- Add optional `AccountChoiceCompleted` to schema-1 Profile; missing remains
  false and no schema bump is required.
- Keep `LocalProductSession` as a Unity-independent coordinator over the
  existing services, with no second disk or in-memory authority.
- Reuse one Settings panel contract in Frontend and Gameplay. Persist all four
  values, apply only Master to current audio, and gate Booster haptic calls at
  the source.
- Give one coordinator ownership of Pause, nested modal, and transition state.
  Freeze attempt-owned systems without `timeScale`; serialize Frontend return
  and registered effects through the Builder.

### Implementation

- Added Profile onboarding persistence, provider-availability truth, typed
  mutation results, transactional session mutation, and one AppRoot rebind.
- Reworked Frontend routing and Builder output for Account Choice, automatic
  returning-user Lobby, Account status, shared Settings, save errors, exit
  confirmation, and generic serialized Scene transition.
- Added shared runtime Settings application, Gameplay Pause coordination,
  full-screen blocker/Dim, confirmation and error layers, focus/Back rules,
  exact judgment guards, registered effect pause/resume, and Frontend return.
- Kept Campaign PlayerPrefs, Product schema version, Stage data, balance,
  packages, and `ProjectSettings.asset` unchanged.

### Validation

- Full EditMode `352/352`; full post-Builder PlayMode `179/179`.
- Frontend and Boot Builders each passed two consecutive runs. The Campaign
  Builder's two-build command passed, including generated-reference and Scene
  structure checks.
- Campaign remained 260 rows with all three approved hashes. Step 10 remained
  80 rows and 64,016 runs with all five approved hashes.
- Actual Editor Campaign PlayerPrefs remained 12 entries with hash
  `B57BDC93138A3E1C376272C33FB042274E821B8BB9C0EC12A33B9480010C73CD`.
  Product save and Guest identity remained unchanged; its file hash was
  `69AA916EF1295FECE34A4DD9A1EC13A525DFDF477EC3EBAF9A7E85898AFF0191`.
- Package manifest/lock retained approved hashes. No Iteration 12
  `ProjectSettings.asset` change or deterministic artifact change occurred.

### Learning

- Backward-compatible optional fields can extend schema 1 safely when the
  missing-value behavior is intentional and independently validated.
- Clone-save-publish makes UI persistence failure atomic from the user's point
  of view without replacing existing service ownership.
- A small explicit Pause coordinator gives both UI routing and gameplay
  mutation paths one auditable authority while avoiding global time changes.

### Deferred

- Google or another provider, account linking, cloud sync, and profile-scoped
  Campaign progress.
- Audible Music/SFX targets, final Lobby/Campaign/Stage Detail/Results,
  Campaign-to-Frontend polish, economy, events, analytics, ads, and IAP.
- Final art, animation, localization, accessibility extensions, and device-
  specific audio/background integration.

### Human Review

- Confirm first-run Account Choice is clear and Guest feels trustworthy;
  returning-user auto-Lobby entry should not flash an intermediate page.
- Confirm Master feedback and four Settings controls are readable at 9:16.
  Music/SFX currently persist but intentionally produce no audible change.
- Confirm Pause button placement, alpha-0.85 full-screen concealment, Resume,
  Restart/Lobby confirmations, rapid Back/taps, and focus loss on devices.
- Confirm no gameplay or modifier motion visibly leaks through Pause and that
  continuing the same Attempt feels coherent after Resume.

## Iteration 13 — Lobby Consolidation and Unified Settings Access

### Play

- Returning users reached a placeholder Frontend Lobby and then saw the older
  Campaign Lobby again before PreRun. Pause existed but its button was outside
  the intended Gameplay HUD coordinate space.
- Settings exposed sliders whose current content-specific Music/SFX effects do
  not yet exist, while Notifications and product links had no truthful shell.

### Analyze

- Campaign PlayerPrefs must remain the only progression authority, so
  Frontend needs a narrow reader rather than store ownership.
- A Scene transition request must survive the Frontend-to-Campaign load but
  must not become persistent data or a global mutable locator. AppRoot is the
  existing lifecycle boundary suited to one-shot ownership.
- Reusing the existing PreRun/item selection and Pause coordinator avoids
  duplicate gameplay, navigation, and persistence systems.

### Design

- Add `IStageProgressReader`, a Frontend read model, and an AppRoot-owned
  one-shot stable Stage ID request consumed before Campaign Lobby activation.
- Compose one Settings panel for Frontend and Pause with Notifications,
  Music, SFX, Vibration, and HTTPS-only configured links. Preserve hidden
  Master and last non-zero Music/SFX values in schema 1.
- Move the existing Pause action under `GameplayHudRoot` within Safe Area and
  prove it does not overlap Stage/Shield HUD or gameplay input.

### Implementation

- Frontend Lobby now shows recommended Stage number, title, mechanic, cleared
  count, and `START STAGE`. Production launch enters existing PreRun directly;
  direct/development/Experiment/fallback routes retain Campaign Lobby.
- Product Settings gained backward-compatible Notification and last-non-zero
  fields. Toggle save remains transactional through the existing Product
  session, with Master as the only current audio runtime target.
- Added one `ProductLinkConfiguration`, HTTPS validation, replaceable URL
  opener, truthful unconfigured state, and a notification-not-delivered note.
- Both Builders use one Settings panel composition. Campaign Builder places
  Pause in the Gameplay HUD coordinate system without changing its coordinator.

### Validation

- Focused EditMode `31/31`; focused PlayMode `114/114`; isolated launch
  regression `1/1`.
- Full EditMode `361/361`; full post-Builder PlayMode `180/180`.
- Frontend, Boot, and Campaign Builders each passed twice. Missing Script,
  Missing Reference, duplicate generated root, EventSystem, pool, and listener
  regressions were not reported.
- Campaign reproduced 260 rows and all three approved hashes. Step 10
  reproduced 80 rows / 64,016 runs and all five approved hashes.
- Campaign PlayerPrefs retained Highest plus 12 records. Product save and the
  existing Guest identity were not written by validation. Packages remained
  unchanged.
- Package-managed WebGL defines may vary between `APP_UI_EDITOR_ONLY` and
  `SENTIS_ANALYTICS_ENABLED`; this volatility is excluded from the feature
  commit while all other ProjectSettings semantic changes remain failures.

### Learning

- A one-shot request at the composition root can bridge Scenes without turning
  navigation intent into saved state or a parallel service graph.
- A read-only adapter lets Frontend present authoritative progression without
  expanding its write authority.
- Toggle UI is more truthful than inactive precision sliders when no separate
  Music/SFX sources exist, provided persistence and current limitations remain
  explicit.

### Deferred

- Real notification permission/scheduling/delivery and real Music/SFX source
  routing.
- Production Terms, Privacy, and Support URLs.
- Campaign-to-Frontend return, Stage Detail, economy, events, analytics, ads,
  IAP, cloud/account linking, and profile-scoped Campaign progression.

### Human Review

- Confirm recommended Stage information is immediately understandable and the
  direct transition to PreRun does not feel abrupt.
- Confirm toggle wording, unconfigured-link truth, Pause button reachability,
  HUD separation, and 9:16 Android/WebGL readability.
- Automation does not determine visual polish, wording comprehension, audio
  expectation, transition satisfaction, or touch ergonomics.

## Iteration 14 — PreRun Back and Settings Toggle Hotfix

### Play

- Frontend-launched PreRun Back exposed the older Campaign Lobby instead of
  returning to the consolidated Frontend Lobby.
- Toggle ON rendered over an always-visible OFF label, while OFF alone looked
  correct.

### Analyze

- PreRun Back was directly bound to `ShowLobby` and discarded the already
  consumed launch origin.
- Builder output created two coincident state Text objects and relied on
  Toggle graphic visibility for only the ON object.

### Design

- Record a Scene-local Frontend origin only after one-shot context consumption
  and reuse the existing serialized Frontend transition path.
- Replace the paired state objects with one shared-controller-owned label per
  toggle and one synchronization method for all four settings.

### Implementation

- PreRun Back now branches on the recorded origin. Frontend return blocks
  duplicate input; failure stays in PreRun, exposes its diagnostic, restores
  the Back button, and permits retry. Direct/development fallback is unchanged.
- The shared Builder emits `StateLabel` only. Settings refreshes all four
  labels after open, click, successful Apply, re-entry, and save reload.

### Validation

- EditMode `361/361`; post-Builder PlayMode `184/184`.
- Frontend/Boot Builder completed twice and Campaign Builder completed its
  internal two-pass validation. No legacy toggle state object, Missing Script,
  Missing Reference, duplicate root, EventSystem, or listener regression was
  reported.
- Campaign 260-row hashes and Step 10 80-row / 64,016-run hashes all match.
  Campaign progress, Product Guest identity, Packages, and non-volatile
  ProjectSettings semantics remain preserved.

### Learning

- A consumed navigation request can provide a precise transient return origin
  without Scene-name inference or persistent transport.
- Toggle state should have one visual owner; overlapping complementary labels
  make the valid ON state inherently ambiguous.

### Deferred

- Clear/Failure result navigation, final Campaign/Stage Detail pages, real
  notification delivery, and content-specific Music/SFX routing are unchanged.

### Human Review

- Verify Frontend is the first visible frame after PreRun Back and rapid taps
  do not feel stuck.
- Verify ON/OFF color, spacing, and touch readability on portrait Android and
  WebGL. Automation does not judge those qualities.

## Iteration 15 — Automatic Lobby Progression Foundation

### Play

- The deterministic Stage loop was stable, but clearing a Stage did not feed a
  visible long-term home progression. Campaign progress also remained in a
  legacy device-wide store separate from Product save.

### Analyze

- Building 36 Stages before establishing reward and Lobby ownership would
  multiply migration risk. The smallest launch-oriented slice is one atomic
  progression authority plus a visible automatic milestone loop.

### Design

- Upgrade Product save to schema 2 and import legacy PlayerPrefs once without
  deleting them. Store Campaign records by stable Stage ID.
- Apply first-clear and even-Stage Lobby rewards with transaction IDs. Separate
  reward application from one-time presentation acknowledgement.
- Show three planned themes, six visible steps per theme, Coins, inventory and
  the next target. Keep visuals structural until the art iteration.

### Implementation

- Added Campaign, Economy and Lobby save sections, migration/validation/clone
  support, a Progression service and atomic Product session mutations.
- Added Product-backed Campaign read/write adapter. Stage clear now saves
  record, unlock and rewards together; production Boot performs one-time
  legacy import.
- Added generated Lobby progression presentation and Frontend-origin result
  return so earned progress is visible immediately.

### Validation

- Product-focused EditMode `25/25`; Frontend PlayMode `16/16`.
- Full EditMode `365/365`; full post-Builder PlayMode `186/186`.
- Frontend Builder completed twice. Missing Script/reference, duplicate root,
  EventSystem and Build Settings validation passed.
- Tier 2 omitted deterministic simulations because Stage data, generation,
  timing, judgment and balance inputs did not change.
- Campaign PlayerPrefs fixtures restored exact state. The real Editor Product
  save remained schema 1/revision 18 during validation and kept its Guest ID;
  migration will occur on the next production Boot.

### Learning

- Reward application and reward presentation must be separate persisted facts;
  otherwise reopening Lobby risks duplicate grants or lost celebration state.
- Stable transaction IDs make both legacy backfill and repeated result events
  safe without a second economy authority.

### Deferred

- Stage 14-36 authoring, final Lobby art/animation, Continue pricing and ad
  policy, item consumption, Hearts, Shop/IAP, analytics and providers.

### Human Review

- Confirm the placeholder Lobby hierarchy makes Coins, next upgrade and the
  newly unlocked step immediately understandable at portrait 9:16.
- Confirm automatic rewards feel meaningful; exact values remain tunable and
  automation does not establish motivation, polish or economy satisfaction.

## Iteration 16 — Campaign Act 2: Stage 14–20

### Play

- The first 13 Stages and core loop were mechanically stable, but campaign
  production had stopped at the prototype introduction sequence.
- The product needed authored forward progress using the approved mechanic set
  before adding Continue economy, Shop or another gimmick.

### Analyze

- Campaign UI and progression already derive from the Stage Catalog, so a new
  campaign layer would duplicate ownership.
- Modifier flags can technically coexist, but Echo, Hidden, Flicker and
  visibility modifiers have no approved same-gate presentation arbitration.
- Several tests and the rollback PlayerPrefs snapshot still fixed the catalog
  at 13 entries. The Campaign Builder itself already scales from Catalog count.
- `TEST_PLAN.md` still described PlayerPrefs as runtime progression authority,
  conflicting with the implemented schema-2 Product save contract.

### Design

- Create a seven-Stage mastery act: clean three-color recovery, then isolated
  Camouflage, Fog, Ice, Echo, Hidden and Flicker revisits.
- Use a difficulty wave: shorter recovery at 14, short pressure at 17, Echo
  relief at 18, and Hidden/Flicker finale at 19–20.
- Keep existing items selectable but do not repeat local tutorial grants or
  introduce inventory consumption.

### Implementation

- Catalog revision 6 adds stable IDs `stage-14` through `stage-20`, authored
  curves, patterns, seeds and single-mechanic settings.
- Stage Catalog Builder upgrades catalogs below 20 entries. Campaign Builder
  regenerates the Scene with 20 selection buttons.
- Expanded Catalog validity and rollback snapshot bounds to 20. Added focused
  Act 2 curve, determinism and modifier-isolation tests.
- Added a normalized gap between Campaign Lobby current-stage and title labels
  after the reference-resolution test exposed a one-pixel overlap.
- Corrected `TEST_PLAN.md` to name schema-2 Product Save as runtime Campaign
  authority and legacy PlayerPrefs as an untouched rollback/import source.

### Validation

- Focused Act 2 EditMode `3/3`; full EditMode `368/368`; final post-Builder
  PlayMode `186/186`.
- Campaign Builder completed two consecutive builds and structural validation.
  Required references, 20 Stage entries, generated-root uniqueness,
  EventSystem and Build Settings checks passed.
- Campaign simulation expanded from 260 to 400 rows. The prior Stage 1–13 CSV
  rows and JSON result prefix are byte-exact. New full hashes are recorded in
  `CURRENT_STATUS.md`.
- Step 10 was not rerun: no shared mechanic rule, Experiment contract, player
  profile or simulation input changed.
- Packages and non-volatile ProjectSettings semantics remain unchanged.

### Learning

- Existing mechanics can support a second act when their rhythm role and
  pacing context change; novelty does not require another runtime subsystem.
- Difficulty curves benefit from recovery and relief beats. A strictly rising
  gate count would turn content volume into fatigue.
- Exact-touch UI anchors are not a safe gap across portrait resolutions.

### Deferred

- Continue Coin pricing and one-ad-per-attempt policy, item consumption,
  Hearts, Shop/IAP, final Lobby visuals, Stages 21–36 and release packaging.

### Human Review

- Play Stages 14–20 in order and judge whether each returning mechanic feels
  like mastery rather than repetition.
- Check Stage 14 recovery, Stage 17 pressure, Stage 18 relief and Stage 20
  finale fatigue on portrait Android and WebGL.
- Automation does not establish fun, readability, comfort, fairness or final
  balance. Mechanical Average/No Item first-clear rates range from `12.6%` to
  `24.8%` in this act.

## Iteration 17 - Campaign Learning-Curve Redistribution

### Play

- Sequential play showed that the prior Campaign introduced almost one new
  mechanic per Stage. Players had little time to recognize, practice and
  master a mechanic before the next one appeared.
- Shield and Booster demonstrations were useful early convenience lessons, but
  Hidden and Flicker arrived before the preceding visibility and movement
  mechanics had enough Campaign practice.

### Analyze

- The existing Catalog, deterministic generator, runtime binding and Campaign
  Builder already support repeated isolated-mechanic Stages. A new level or
  mechanic system was unnecessary.
- Stable Stage IDs already own saved progression. Reusing those IDs and changing
  only authored definitions preserves Product-save identity and avoids an
  unapproved reset or migration.
- Hidden and Flicker retain complete Lab coverage, so removing them from the
  current Campaign does not remove their implementation or regression safety.

### Design

- Preserve Stages 1-5 exactly. Keep Stage 6 as the locked provided-Shield
  lesson and Stage 7 as the locked provided-Booster lesson.
- Make Stage 8 a clean three-color recovery and ordinary selectable-item Stage.
- Allocate three Stages to each current Campaign mechanic: Camouflage 9-11,
  Fog 12-14, Ice 15-17 and Echo 18-20. Every block follows a two-color
  introduction, two-color practice and three-color mastery shape.
- Defer Hidden to Stages 21-23 and Flicker to Stages 24-26 rather than
  compressing them into the first 20 Stages.

### Implementation

- Catalog revision 7 redistributes all post-foundation Campaign definitions
  without changing the 20 stable IDs or Product-save ownership.
- Campaign-only Hidden/Flicker placement tests were retired while their Core,
  Experiment and PlayMode mechanic coverage remains active.
- Campaign Builder regenerated the 20-entry Scene from the revised Catalog.

### Validation

- Full EditMode passed `367/367`; full post-Builder PlayMode passed `195/195`.
- Campaign Builder completed two consecutive passes with its structural checks.
- The complete 400-row Campaign simulation ran twice and produced byte-identical
  outputs. Summary SHA-256 is
  `D07BC7A2808DD4A31E65FA618F20EC3981F782568614510E1AF020146C4820DE`,
  JSON is
  `6FF5187DDE11EFA38C27C4AD96CD3145315D41BB33178CCC8F2F73F683CB3D0E`,
  and CSV is
  `522053519887272534EABEB1769EAD4709C12D865514EA1068CFE899FD97B37A`.
- The prior Stage 1-5 CSV 100 rows and the first 100 JSON results are exact.
  All reported continuity, sequence and fixed-pool violation counters remain
  zero.
- Step 10 was not rerun because no Experiment contract, shared mechanic rule,
  player profile or simulation input changed. Its approved artifacts remain
  untouched.
- Save-failure fixture setup was stabilized so its intended failure occurs at
  the save boundary. This is validation robustness only and changes no product
  behavior.

### Learning

- Content volume does not create a learning curve by itself. Repetition needs
  an explicit introduction, practice and mastery role before novelty advances.
- Stable content IDs allow pre-release balance redistribution without creating
  a second progression authority, but saved completion still refers to those
  IDs and must never be silently reset.
- Removing a mechanic from Campaign placement is safe only when its independent
  implementation and Lab regression coverage remain intact.

### Deferred

- Hidden Stages 21-23, Flicker Stages 24-26, Stages 27-36, item consumption,
  Continue economy/ads, final Lobby art, Shop/IAP and release packaging.

### Human Review

- Play Stages 8-20 in order and judge whether each three-Stage block gives
  enough time to understand, practice and master its mechanic.
- Confirm the two-color introduction/practice followed by three-color mastery
  reads as progression rather than repetition, especially at the 11-12,
  14-15 and 17-18 block transitions.
- Automated simulations establish determinism and mechanical outcomes, not
  learning, fun, readability, comfort or final balance.

## Iteration 18 - Consumable Start Items

### Play

- Lobby already displayed owned Shield and Booster counts, but PreRun selection
  did not spend them. The visible inventory therefore had no gameplay cost or
  ownership consequence.
- The Stage 6/7 teaching grants still needed to remain free so players could
  learn each item before Stage 8 introduced ordinary selection.

### Analyze

- Product save schema 2 already owns Shield/Booster counts and idempotent
  transaction IDs. A new save schema or second inventory service was
  unnecessary.
- The existing Start boundary is the smallest truthful consume point. Charging
  at toggle time would make exploration costly, while charging after countdown
  would allow gameplay to begin before persistence succeeds.
- Retry is a new Attempt through the retained PreRun selection, so retaining a
  toggle must not retain a previously spent item charge.

### Design

- Consume one unit of every manually selected item atomically when `START` is
  accepted on Stage 8 or later.
- Keep Stage 6/7 provided grants free and selection-locked. Permit no-item play
  without Product, but never substitute free selected items for a missing
  Product session.
- Keep PreRun visible on shortage or save failure, show the failure truthfully,
  and publish no partial Product or gameplay state.
- Use one idempotent request per Start so repeated input consumes and starts at
  most once. Retry creates a fresh request and consumes retained selections
  again.

### Implementation

- `LocalProductSession.ConsumeStartItems` performs the cloned, validated and
  atomic Product mutation before the existing countdown path.
- Campaign presentation uses an inventory gateway rather than introducing
  Product ownership into gameplay Core. Generated PreRun references and state
  synchronize with the authoritative owned counts.
- Product save schema, Catalog, deterministic item effects and StageSession
  rules remain unchanged.

### Validation

- Targeted Product EditMode passed `30/30`; targeted item-consumption PlayMode
  passed `5/5`.
- Campaign Builder completed two consecutive passes and structural validation.
- Full EditMode passed `372/372`; final post-Builder PlayMode passed `207/207`.
- Package manifest/lock, non-volatile ProjectSettings, actual Product save,
  Guest identity and player balances remained unchanged by validation.
- Tier 2 omitted Campaign and Step 10 simulation reruns because Product
  authorization does not alter Core item behavior, stage data, deterministic
  inputs or Experiment contracts. All approved hashes remain preserved.

### Learning

- A displayed inventory becomes meaningful only when the gameplay entry point
  and persistence boundary agree on when ownership changes.
- Atomic multi-item consumption is simpler for the player and safer for
  recovery than independently publishing two selected-item writes.
- Retry UX must state that a retained selection is a new purchase/use for the
  next Attempt rather than implying the previous charge carries forward.

### Deferred

- Continue Coin pricing, rewarded ads, Shop/IAP, Hearts, unlimited-time items,
  final Lobby art and release provider integration.

### Human Review

- Confirm owned counts, selected state, Start failure and Retry re-consumption
  are understandable on a portrait device without reading implementation
  language.
- Confirm the free Stage 6/7 teaching flow and first paid-use Stage 8 transition
  feel fair. Automated tests do not establish clarity, value or satisfaction.

## Iteration 19 - Continue Economy and Attempt Policy

### Play

- Failure exposed Continue as a mechanic but did not present truthful Coin or
  rewarded-ad choices. The visible Coin balance therefore had no Continue use,
  and a run could not express the approved escalating recovery cost.

### Analyze

- Core already owned failure, safe resume and Retry. Product already owned
  Coins, atomic persistence and idempotent transaction IDs. The smallest safe
  design was an attempt-local authorization policy between those authorities,
  without moving economy into gameplay Core.

### Design

- Allow three Continues per Attempt. Price successful Coin Continues at
  300/600/900 by Coin ordinal, independently allow one successfully completed
  rewarded ad, and reset both rights on Retry.
- Preserve the ad right when Coins are used first and preserve the 300-Coin
  first price when the ad is used first. Keep the failure state frozen on
  insufficient funds, save failure, ad failure, cancellation or unavailability.
- Hide rewarded Continue when no real provider exists; never present an
  unavailable release action as successful.

### Implementation

- `StageSession` now owns the Continue count and three-use cap while retaining
  the existing derived continued-run contract and resume invariants.
- Product performs atomic, idempotent Continue Coin spends. Presentation owns
  the attempt policy, source authorization, stale ad-callback protection and
  truthful offer visibility.
- Campaign simulation supports up to three authorized Continues and exports
  total, average, maximum and per-use-count histogram metrics.

### Validation

- Campaign Builder and structural validation completed twice.
- Full EditMode passed `400/400`; final post-Builder PlayMode passed `215/215`.
- Two complete 400-row Campaign simulations were byte-identical. Summary
  SHA-256 is
  `BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`,
  JSON is
  `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`,
  and CSV is
  `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`.
- Step 10 inputs and artifacts remained unchanged.

### Learning

- Continue availability is a transaction protocol, not only a button state:
  authorization, persistence, attempt capacity and gameplay resume must agree
  before the failed run changes.
- Separating Coin ordinal from total Continue count keeps mixed Coin/ad order
  predictable and makes the player's unused recovery rights explicit.

### Deferred

- Production rewarded-ad provider integration, Shop/IAP, Hearts, unlimited
  boosters, final economy balance and release packaging.

### Human Review

- Verify on portrait devices that Coin price, balance shortage, ad availability,
  three-use exhaustion and Retry reset are understandable without technical
  language. Automation does not determine fairness, value or frustration.

## Iteration 20 — Monetization Platform Baseline

### Play / Input

- Unity IAP was installed and a Firebase console project was created.
- The product owner approved Google Play billing, Android ID
  `com.wscho.colorgaterunner`, six Coin amounts, five bundles and Firebase
  Functions plus App Check.
- Five reference screens supplied Home, Shop, Journey and Collection layout
  hierarchy.

### Analyze

- The install included Unity IAP `5.4.2`, Google Play BillingMode and Unity
  Services Core, but no Store integration, product catalog or Firebase SDK.
- Unity had also upgraded four unrelated packages; those changes were outside
  the approved scope.
- The project still used a Unity template Android application ID.
- Hearts, timed unlimited Hearts and owned Continue inventory do not exist, so
  mixed bundles cannot yet be granted truthfully.

### Design

- Establish the package/application identity baseline first.
- Record commercial catalog and UI direction as approved intent while keeping
  runtime purchase, Shop and missing reward models in later iterations.
- Preserve the local-first schema-2 economy and require save-before-confirm
  plus order-id idempotency for future purchases.

### Implementation

- Kept Unity IAP `5.4.2` and Unity Services Core `1.18.0`.
- Restored unrelated Navigation, Rider, Visual Studio and Visual Scripting
  packages to their previous approved versions.
- Set Android application ID to `com.wscho.colorgaterunner` and retained
  Google Play billing mode.

### Validation

- Final Unity compilation succeeded.
- EditMode passed `400/400`; PlayMode passed `215/215`.
- No Scene or gameplay input changed, so Builders, Campaign simulation and
  Step 10 were not rerun. Their existing hashes remain authoritative.
- The actual Editor Product save last-write time predates final validation and
  its schema, Guest ID, progress and economy were preserved.

### Learning

- Installing a package through Unity can update unrelated packages; the
  manifest/lock diff must be normalized before accepting a new baseline.
- ProjectSettings edited while Unity is open can be overwritten on Editor
  shutdown. Application identity was therefore reapplied with Unity closed
  and the final full suite repeated.

### Deferred / Human Review

- Product IDs, prices, repeatability, Heart/timed entitlement and Continue
  inventory contracts.
- Store integration, Firebase SDK/config, server validation, Shop UI and
  internal-track purchase tests.
- Human review of final original Lobby/Shop art and portrait navigation.

## Iteration 21 — Local Commerce Rewards and Hearts

### Play / Analyze

- Approved bundles referenced Hearts and Continue inventory that the save did
  not own, so a real purchase grant lacked deterministic local semantics.

### Design / Implementation

- Added schema-3 Hearts, timed unlimited Hearts, Continue Tickets, exact local
  product definitions and atomic/idempotent Product mutations.
- Stage start authorizes Heart plus selected items in one save. Failure UI adds
  Ticket before the existing real-ad and Coin offers.

### Validation / Learning

- Builder passed twice; EditMode passed `408/408`; post-Builder PlayMode passed
  `218/218`. The actual Editor save stayed byte-, timestamp- and hash-exact.
- A normal Editor-side Test Runner bridge was required because the installed
  Unity license did not provide the headless entitlement. Temporary validation
  files and generated test scenes were removed before commit.

### Deferred / Human Review

- Store connection, receipt/server validation, purchase confirmation, Shop UI,
  Firebase integration and final original Heart/Ticket visuals.

## Iteration 22 — Editor Developer Console

### Play / Analyze

- Existing in-game development controls were difficult to reach from the
  production Frontend flow and could not configure the full Product wallet.

### Design / Implementation

- Added one Editor Window for Stage launch/unlock, Campaign reset and exact
  Economy/Heart configuration before or during Play.
- Split the prior broad development reset into Campaign-only and Economy-only
  atomic Product operations. Play Stage is a transient unlock bypass.

### Validation / Learning

- Full EditMode passed `414/414`; full PlayMode passed `218/218`. Tests used
  isolated saves and the developer save remained unchanged.
- Editor tooling can safely share Product authority without shipping cheat UI
  or inventing a parallel persistence format.

### Deferred / Human Review

- In-run gate-index teleport, fine-grained Best-record editing, remote dev
  console, and a full account wipe remain excluded.
- Human review should confirm the Window labels and Play/Unlock distinction are
  clear during ordinary Unity use.

## Iteration 23 - Continue and Clear Economy Flow

### Play / Analyze

- Twenty-Stage human play exposed four connected loop problems: Continue
  pricing stopped after three uses, Continue countdown briefly showed stale
  modifier and consumed-buff presentation, the failure wallet did not explain
  affordability, and clear actions returned through redundant or misleading
  routes.
- The last-Heart clear also exposed that start-time charging had no success
  settlement, while the existing first-clear Coin grant was not visible on the
  result screen.

### Design

- Remove the total Continue cap. Price successful Coin uses at
  900/1,900/2,900/4,900 and repeat 4,900 thereafter, while keeping ad and
  Ticket use outside the Coin ordinal and resetting the Attempt policy on
  Retry.
- Treat Continue countdown as resumed state for gate modifiers and as consumed
  state for former Attempt buffs.
- Show Coin and Heart balances on failure, preserve the frozen failure on
  shortage, hide Campaign Replay and route `NEXT STAGE` directly to PreRun.
- Refund a normally charged Heart only on successful clear. Grant first-clear
  base Coins by Stage difficulty: 100 Normal, 200 Hard and 500 Very Hard.

### Implementation

- Core Continue capacity is no longer capped; finite Stage gate count remains
  the simulation bound. The attempt policy owns the Coin ordinal independently
  of total Continue count.
- Campaign presentation synchronizes modifier/buff state before Continue
  countdown, exposes the failure wallet and shortage popup, and separates
  direct next-Stage flow from Lobby and Experiment Replay.
- Product start receipts carry a one-use Heart refund token into the atomic
  clear transaction. Catalog data owns difficulty for Stages 11, 14, 17 and
  20; result UI shows base and Lobby milestone rewards separately.

### Validation

- Campaign Builder and structural validation completed twice.
- Full EditMode passed `418/418`; final post-Builder PlayMode passed `222/222`.
- Two complete Campaign simulations were byte-identical. Approved SHA-256:
  - Summary:
    `698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`
  - JSON:
    `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`
  - CSV:
    `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`
- Step 10 inputs and approved artifacts remained unchanged.

### Learning

- Continue count and Coin price ordinal are separate concepts. Keeping them
  separate allows unlimited mixed-source recovery without charging an ad or
  Ticket as though it were a Coin purchase.
- Start-time Heart charging needs an explicit clear settlement so crash/quit
  remains loss-safe while a successful last-Heart run can continue naturally.
- A reward contract is incomplete when persistence succeeds but the result UI
  cannot explain what was granted.

### Deferred / Human Review

- Production rewarded ads, Shop/IAP UI, Lobby layout cleanup, zero-stock
  start-item purchase and Fog/Ice redesign remain deferred.
- Human review should judge price pressure, direct next-Stage comprehension,
  failure wallet readability, countdown visual continuity and difficulty
  reward satisfaction. Automation does not determine fairness or value.

## Iteration 24 — Continue Gate-Pool Recycle Hotfix

### Play / Analyze

- Human play on Stage 11 reported that continuing near gate 34 eventually
  left no gates and crossing Goal did not end the run.
- Inspection showed that each Continue permanently deactivated the failed
  `StageGateView`. The fixed pool has six slots, so unlimited Continue exposed
  the former capped-policy assumption. Fog was not the cause.

### Design / Implementation

- Preserve the consumed-plan and clean-respawn contracts, but recycle the
  failed slot through the ordinary fixed-pool path so it receives a later
  authored plan.
- Avoid invalid velocity writes when the presentation Rigidbody is kinematic.

### Validation / Learning

- A focused Stage 11 test performs more than six late Continues, resolves all
  46 gates and crosses Goal into Stage Cleared.
- Focused PlayMode passed `1/1`; EditMode passed `418/418`; PlayMode passed
  `223/223`. The pool remained fixed and unaffected gates retained transforms.
- Unlimited recovery policies require presentation-pool tests beyond the old
  maximum number of recovery actions.

### Deferred / Human Review

- Fog and Ice presentation redesign remain separate approved product work.
- Human replay should confirm Stage 11 feels continuous at and beyond gate 34.

## Iteration 25 — Timed Fog Curtain

### Play / Analyze

- Human play found that nearest-two Fog exposed obstacle spacing and made its
  authored end predictable, so it read like another gate modifier rather than
  a temporary visibility event.
- Campaign and Experiment shared the same gate-level neutralization, while the
  requested product behavior could remain presentation-only.

### Design / Implementation

- Campaign Fog triggers one speed-adaptive curtain per attempt when the first
  Fog gate becomes the next judgment. It holds Alpha 1 for 1.5 seconds and
  fades over 0.5 seconds at 1.5 seconds of travel distance ahead.
- Pause and Continue countdown freeze state; Continue repositions it after
  respawn and Retry resets it. Campaign gates retain authored colors.
- Builder creates exactly one transparent `FogCurtain`; Experiment Lab keeps
  the prior nearest-two diagnostic behavior.

### Validation / Learning

- New pure-state tests cover hold, fade, one-shot, reset and invalid time.
- Campaign PlayMode covers adaptive distance, authored gate material,
  non-retrigger, Pause/Continue preservation and Retry reset.
- Builder passed twice, EditMode `421/421`, and PlayMode `225/225`.
- A visual mechanic can change presentation without invalidating deterministic
  judgment, but the product and diagnostic Experiment contracts must be named
  separately.

### Deferred / Human Review

- Human review owns curtain softness, opacity, depth and readability on device.
- Preplaced/rebalanced Ice and the final Lobby layout remain separate work.

## Iteration 26 — Fog Visibility Curve

### Play / Analyze

- Human review found the curtain appeared too abruptly and its 1.5-second full
  opacity window ended before it created meaningful visibility pressure.
- The duration was hard-coded inside the view, preventing an authored learning
  curve across the existing Fog intro / practice / mastery block.

### Design / Implementation

- Added Catalog-owned `FogCurtainSettings` with positive fade-in, full-opacity
  and fade-out phases. Stage 12 / 13 / 14 author `0.5 + 5/6/7 + 0.5 seconds`.
- The state now fades alpha `0 -> 1`, holds, then fades `1 -> 0`. Existing
  one-shot, adaptive position, Pause, Continue and Retry behavior remains.
- Stage Catalog revision advanced to 9 and the Resource/Scene were rebuilt.

### Validation / Learning

- Builder passed twice, EditMode `423/423`, and PlayMode `225/225`.
- Two Campaign 400-row runs were byte-identical and retained all three approved
  hashes. Step 10 remained unchanged because Experiment behavior is unchanged.
- Product save and backup remained byte- and timestamp-identical.
- A visibility mechanic's duration belongs in authored stage data when it is
  part of the learning curve, even if judgment remains mechanically unchanged.

### Deferred / Human Review

- Human play must compare the 5 / 6 / 7-second curve on a portrait device.
- Preplaced/rebalanced Ice and the final Lobby layout remain separate work.

## Iteration 27 — Preplaced Ice Runway

### Play / Analyze

- Human play found that Campaign Ice appeared by abruptly recoloring the whole
  floor. The terrain had no visible approach and the `1.45` speed increase was
  too weak for the intended Ice identity.
- Inspection confirmed that presentation followed only the next six-slot gate
  view, even though all deterministic gate plans already existed at Start.

### Design / Implementation

- Added Catalog-owned positive Ice runway speed and authored `2.0` for Stages
  15–17. Experiment definitions retain `1.45`.
- Added one Builder-owned fixed pool of 50 panels. Start maps each panel to its
  authored gate approach, activates only Ice plans and leaves the normal track
  material stable. Continue preserves it; Retry resets and rebuilds it.
- Split the Fog View into a matching Unity script asset after final Builder
  reload identified an unstable legacy generated reference. Fog timing and
  presentation behavior remain unchanged.

### Validation

- Builder passed twice. Focused Ice EditMode passed `8/8`; focused PlayMode
  passed `4/4`; full EditMode passed `425/425`; PlayMode passed `226/226`.
- Two complete 400-row Campaign runs were byte-identical. Summary / JSON / CSV
  SHA-256 are `86D55FB085FE51135BFA5E0F5D17D0242166C1F9DBD3EE03F58485A33BFC27B8`,
  `3CE081477E7A431939D4C8AB6D0140FFBB6C22D6A6850E7551499E4C28BC28C2`, and
  `9D6E7903B31D59C3307DBE49A1E1FCF705A299FE37571299F17FAD63DF464B95`.
  All continuity and pool-violation counters are zero. Step 10 was unchanged.
- Real Product save bytes, length, timestamp and SHA remained exact.

### Learning

- When the deterministic layout already exists, terrain telegraphing should be
  built from that authority before play rather than inferred frame by frame
  from a small presentation pool.
- File/class identity is part of Unity Scene reference stability, even when
  in-memory Builder execution previously appeared to succeed.

### Deferred / Human Review

- Final Ice material, edge transition, glow and environment integration remain
  visual work. Human play owns readability and fairness at `2.0` speed.
- Lobby hierarchy/redesign, Shop/IAP, Firebase and real rewarded ads remain
  separate iterations.

## Iteration 28 — Ice Tap Rhythm

### Play / Analyze

- Human play accepted the Fog and Ice presentation direction but found rapid
  color bursts inappropriate at `2.0` Ice speed. Static plan inspection showed
  Stage 17 required two taps on 6 of 15 Ice gates (`40%`), while Stage 15 / 16
  contained mostly zero-tap repeats instead of a readable one-tap rhythm.

### Design / Implementation

- Added an Ice-only deterministic tap budget to the Campaign Stage sequence.
  Every Ice gate now requires one or two forward taps, with a strict double-tap
  ratio below 10% and a seed-derived quota/position.
- Stage 15 / 16 now use one tap on all 12 / 14 Ice gates. Stage 17 uses one tap
  on 14 gates and two taps on one gate (`6.67%`). Retry is identical.
- Other Stage generators, Continue overrides, Experiment Lab, speed, spacing,
  runway presentation, stable IDs and Product state were not changed.

### Validation

- Focused EditMode passed `13/13`; focused Campaign PlayMode passed `1/1`.
  Full EditMode passed `429/429`; full PlayMode passed `227/227`.
- Two 400-row Campaign simulations were byte-identical. Summary / JSON / CSV
  hashes are `D3287E93FE0F022ABDF504EC3304076724BFBB3357A890DEFB0545E072CD9DB1`,
  `43C61E0CCB28B10211C6E201631EE2938D0160F72DA3A334AE0A459331276500`, and
  `4E1E87C9C1D5B27D63122744889DE066A3D22F2A93DF86CB6062DAC2165F5B99`.
  Continuity/pool counters are zero; Step 10 remained unchanged.
- Product save and backup bytes, timestamps and hashes were preserved. No
  Builder ran because no Scene, Catalog asset or generated hierarchy changed.

### Learning / Human Review

- A high-speed terrain mechanic needs a separate input-burst budget; random
  target distribution alone does not express the intended physical pressure.
- Human play must judge the Stage 17 rare double-tap and overall Ice cadence.
- Lobby visual hierarchy is the next approved, separate iteration.

## Iteration 29 — Lobby Visual Hierarchy

### Play / Analyze

- Human play found that the Lobby presented too many equally weighted text
  rows and did not show Heart availability alongside the other persistent
  resources.
- The approved reference established a top wallet, dominant center scene and
  one lower Stage action as the target information hierarchy.

### Design / Implementation

- Rebuilt the generated Frontend Lobby into top, center and bottom zones.
  Removed duplicate visible title/account copy, added Product-backed Heart
  countdown text and surfaced Stage difficulty in the compact action card.
- Kept stable-ID launch, automatic Lobby milestones, Settings, reward
  acknowledgement and all Product save ownership unchanged.

### Validation / Learning / Deferred

- Frontend Builder ran twice. Focused Lobby EditMode passed `8/8`, Product
  EditMode `58/58`, focused Frontend PlayMode `18/18`, full EditMode `430/430`
  and PlayMode `227/227`.
- Hierarchy can be completed before final art: fewer persistent labels and one
  primary action make later visual production safer and more measurable.
- Final art, Shop/IAP runtime, navigation modules and device visual review are
  deferred.
