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
