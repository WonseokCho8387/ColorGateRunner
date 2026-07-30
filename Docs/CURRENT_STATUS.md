# Current Status

Version

Iteration 4

(Echo Modifier / Configurable Campaign Expansion complete)

---

Authoritative Iteration 4 Result

- `StageCatalog.asset` is the single production source for all 11 campaign
  stages. Runtime, simulation, and tests consume its pure-Core adapter through
  `IStageCatalog`.
- Clone gameplay has been removed. Echo is a modifier on an ordinary gate and
  does not add gates, positions, collision paths, or a second generator.
- Camouflage reveal uses effective-speed ETA, transitions once, never hides
  again, and always keeps ordinary judgment active.
- Stage 6 provides one local Shield at start; Stage 7 provides one local
  Booster at 30% progress. The item picker prevents duplicate selection.
- Stages 8, 9, 10, and 11 teach Camouflage, Fog, Ice, and Echo respectively.
  Stages 6-10 use two active colors to isolate the new mechanic; Stage 11
  returns to three colors for the finale.
- Stage speed Start/Max/Curve, cadence, active color count, mechanic,
  Echo/Camouflage values, seed, and local grant are Inspector-authored.
- The stage picker is catalog-driven and contains 11 stable-ID buttons in a
  two-column portrait layout.

Final Validation

- EditMode: 248 / 248 passed.
- PlayMode: 125 / 125 passed.
- Stage 1-5 final simulation CSV rows exactly match the immediately preceding
  Echo-expansion baseline.
- All five Step 10 files are byte-identical to their baseline.
- The final 1,000-run matrix covers every Stage 1-11, player profile, and
  start-item combination in new Mechanic Campaign artifacts.
- Core keeps `noEngineReferences: true`; no package or ProjectSettings feature
  change was introduced.

Next Iteration

Human mobile play only: evaluate Echo frequency/comprehension, Echo versus
Shield readability, Camouflage lead time, Stage 6-11 pacing, curve feel, Ice
comfort, and Stage 11 finale quality before approving another balance pass.

---

Completed / Historical Milestones

Stage 1~5

Experiment Lab

Continue

Retry

Goal

Booster

Shield

Camouflage

Fog

Ice

Experiment Countdown

Experiment Failed / Completed Results

Experiment Retry / Replay

Stage 4 Runtime / Simulation Color-Cycle Parity

Campaign Movement / Spatial Scale 2x

Superseded Clone-only Experiment

Superseded Clone Source / Relationship Metadata

Superseded Clone Failure Cause

Superseded Clone Shield / Camouflage Integration

---

Current Decisions

Maximum Tap = 4

Mechanics unlock by Stage

Experiment uses deterministic stages

Experiment uses one explicit StageFlowState

Experiment Retry / Replay preserves condition and seed

Experiment never reads or writes campaign progress

Campaign speed and authored world distances scale together

Campaign cadence and approximate stage duration remain unchanged

Experiment Lab movement values remain unchanged

Echo uses ordinary gates and never changes gate count

Echo Camouflage uses ordinary ETA hide, reveal, and judgment

Echo offers are deterministic and non-stacking

---

Iteration 4 Delivered

Architecture Foundation

Echo Modifier

Camouflage ETA Reveal

Stage Speed Profiles

Campaign Stages 6–11

---

Known Issues

Iteration 3 Clone Gate concept was rejected by human feedback and is
superseded by the approved Echo Modifier contract below.

Echo frequency, Echo/Shield readability, Camouflage reveal lead, and Stage
6–11 pacing require human mobile play.

Shield visual

Campaign 2x speed feel and comfort require human mobile playtest
