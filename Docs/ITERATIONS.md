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
