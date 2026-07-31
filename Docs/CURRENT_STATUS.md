# Current Status

Version

Iteration 7

(Hidden migration and color-cycling Flicker implemented / human review pending)

---

Previous Iteration 5 Result

- `StageCatalog.asset` is the single production source for all 11 campaign
  stages. Runtime, simulation, and tests consume its pure-Core adapter through
  `IStageCatalog`.
- Clone gameplay has been removed. Echo is a modifier on an ordinary gate and
  does not add gates, positions, collision paths, or a second generator.
- Camouflage reveal uses effective-speed ETA, transitions once, never hides
  again, and always keeps ordinary judgment active.
- Stages 1-5 lock both start items in catalog data and the PreRun UI.
- Stage 6 provides one local Shield at start; Stage 7 provides one local
  Booster at start. Both activate after the same countdown boundary as an
  equivalent selected item, and the picker prevents duplicate selection.
- Player movement crossing an unresolved gate plane reuses the existing
  one-shot `TryResolveCrossing()` path. This prevents high-speed Booster
  movement from tunneling past the thin physics Trigger without adding a
  second judgment system or increasing the six-gate pool.
- Stages 8, 9, 10, and 11 teach Camouflage, Fog, Ice, and Echo respectively.
  Stages 6-10 use two active colors to isolate the new mechanic; Stage 11
  returns to three colors for the finale.
- Stage speed Start/Max/Curve, cadence, active color count, mechanic,
  Echo/Camouflage values, seed, and local grant are Inspector-authored.
- The stage picker is catalog-driven and contains 11 stable-ID buttons in a
  two-column portrait layout.

Iteration 5 Validation

- EditMode: 249 / 249 passed.
- PlayMode: 128 / 128 passed.
- Stage 7 PlayMode starts Booster at `GO`, crosses all 40 planned gates at
  runtime speed, retains the fixed pool, and reaches Goal/StageCleared.
- The requested Stage 6-11 simulation subset contains 120 rows and 96,024
  modeled runs: six stages, five profiles, four item inputs, Perfect once and
  every stochastic condition 1,000 times.
- Stages 6 and 8-11 have zero changed rows versus the preceding campaign
  baseline. All 20 Stage 7 rows change only under the approved start-Booster
  contract.
- Stage 7 Average/None changes mechanically from first-clear
  `47.6% -> 47.0%`, Continue-clear `84.8% -> 95.1%`, median clear time
  `33.203s -> 31.356s`, and average Booster bypasses `9.780 -> 15.774`.
  These are not claims about fun or comfort.
- All 24 Stage 6-11 Perfect/item rows clear, and all 120 rows report zero
  Booster/Continue displacement, gate-index gap/duplicate, cursor reset, and
  full-pool reset violations.
- A fresh Step 10 run reproduced all five baseline hashes exactly.
- The full refreshed Stage 1-11 matrix is stored in the existing Mechanic
  Campaign artifacts.
- Core keeps `noEngineReferences: true`; no package or ProjectSettings feature
  change was introduced.

Stage Progression Hotfix

- Fixed a stale stable-ID reference in `ShowLobby()`. The lobby previously
  displayed the newly unlocked stage number but `PlayFromLobby()` could still
  start the prior stage ID.
- The displayed `StageDefinition.StageId` is now synchronized whenever lobby
  progression selects a stage.
- PlayMode explicitly verifies displayed Stage 2 starts Stage 2 and displayed
  Stage 3 starts Stage 3.

Test Build Tooling

- `Tools > Color Gate Runner > Test Builds` provides Android APK, WebGL,
  combined Android-plus-WebGL, and output-folder commands.
- Both platforms use the enabled `EditorBuildSettings` scenes and Development
  mode. Outputs stay under ignored `Builds/Test`; Android always produces an
  APK and restores the previous App Bundle setting afterward.
- WebGL test builds temporarily use a `540 x 960` logical canvas and the
  project-owned `ColorGateRunnerPortrait` template. Its browser container
  preserves an exact `9:16` aspect ratio with neutral letterboxing instead of
  stretching to a square or landscape host.
- The build path restores the prior WebGL width, height, and template settings
  after success or failure, so Android and persistent ProjectSettings remain
  unchanged.
- Missing scenes, missing platform support, platform-switch failure, and
  unsuccessful `BuildReport` results fail explicitly instead of reporting a
  false success.
- Build-menu and portrait-template EditMode coverage passes 5/5 in the focused
  suite. The full EditMode suite passes 283/283.
- An isolated Android invocation reached the real Player build after script
  and shader compilation, but its copied-Library backend stopped making
  progress before producing an APK. Actual APK installation remains a manual
  acceptance check from the original editor.
- A separate isolated WebGL build completed successfully and produced a
  124,340,289-byte player. Its generated page contains the expected `540 x 960`
  canvas macros and exact `9:16` frame rules, and the validation project's
  prior `960 x 600` default-template settings were restored afterward.
  Browser play, input, and host-page resizing remain human acceptance checks.

Authoritative Iteration 6 Result (now Hidden)

- The memory mechanic then named Flicker, now named Hidden, is a flag on the
  shared ordinary-gate `GateModifier`. It adds no gate, generator, collision
  path, judgment path, or pool object.
- Its settings, now `HiddenSettings`, validate the approved
  Inspector-authored fields. The
  Launcher converts its serialized values into the pure-Core settings and
  disables Booster for Hidden-only runs.
- The existing deterministic Experiment sequence selects stable Hidden gate
  IDs without changing color/spacing PRNG output, gate count, or Step 10.
- A pure-Core visibility state requires both minimum readable time and shared
  effective-speed ETA, starts one transition, and never reveals target
  information again before judgment.
- The pooled View begins with target color/symbol plus a persistent `HIDDEN`
  identity. Hide blends to the existing neutral silhouette and removes only
  target symbol information; geometry, judgment opening, transform, collider,
  and marker remain.
- Player, held Echo, Shield, and ordinary failure reuse the existing priority.
  Retry/Replay reproduce selection and reset visibility state.
- Campaign Stages 1-11 and the 16-condition Step 10 matrix are unchanged.

The initial Hidden settings are Inspector-authored human-play candidates:
eligible progress `0.15-0.85`, chance `0.35`, cooldown `2`, visible duration
`1.00s`, hide lead `0.65s`, transition `0.12s`, maximum `4`, and guaranteed
first occurrence. Booster is disabled for this standalone condition.

Iteration 6 Validation

- Original open Editor: scripts compiled without C# errors.
- EditMode: 262/262 passed.
- PlayMode: 134/134 passed.
- Scene Builder: the command's two consecutive builds succeeded; the rebuilt
  isolated scene then passed PlayMode 134/134 again.
- Campaign simulation: 220 rows and 176,044 modeled runs. Summary, JSON, and
  CSV hashes are byte-identical to the preceding baseline.
- Step 10: all five CSV/JSON/summary/comparison/shortlist hashes are
  byte-identical to the approved baseline.
- Packages and ProjectSettings have no intended feature change. Automated
  results do not establish Hidden readability, comfort, comprehension,
  fairness, satisfaction, motivation, or fun.

Iteration 7 Baseline

- Clean worktree and `git diff --check`.
- EditMode 262/262 and PlayMode 134/134.
- Echo and former-Flicker targeted PlayMode cases 12/12.
- Campaign Summary/JSON/CSV and all five Step 10 hashes match Iteration 6.
- Package manifest/lock and ProjectSettings diffs are empty.

Iteration 7 Approved Contract

- Existing `Flicker = 16` gate modifier and mechanic value `5` become Hidden
  with identical selection, hide, judgment, View, and Retry/Replay behavior.
- New Flicker uses new enum values and cycles two or three unique active
  colors. The collision-time color from
  `ExperimentSession.ElapsedPlayingSeconds` is authoritative.
- Gate plans fix cycle colors, interval, deterministic phase, pulse, base
  color, Gate ID, and seed-derived data. View and judgment share the same Core
  active-color calculation.
- Minimum visible cycles are checked from existing deterministic speed,
  spacing, and six-View exposure. Booster is disabled.
- Player → Echo → Shield → Failure remains unchanged.
- Campaign, music/BPM/DSP, Hidden/Flicker or other modifier combinations,
  speed-based interval adjustment, new colors/input, and Stage 6-11 work are
  out of scope.

Iteration 7 Result

- The old modifier and mechanic numeric values `16` and `5` now deserialize
  as Hidden. New Flicker uses values `32` and `6`.
- Every former Launcher field has explicit `FormerlySerializedAs` migration
  metadata. New Flicker fields use a distinct `colorCycleFlicker` serialized
  prefix, so no old field name can bind to the new mechanic.
- Hidden preserves the former seed-selected gate IDs, ETA-based one-way hide,
  neutral silhouette, ordinary judgment, held Echo/Shield behavior, and
  Retry/Replay reset.
- Flicker remains an ordinary gate modifier. Its plan fixes the base color,
  two or three unique cycle colors, interval, deterministic phase offset,
  pulse, Gate ID, and selection seed without changing gate count.
- `FlickerCycleCalculator` is the single pure-Core phase path used by the
  View, collision-time judgment, simulation, and tests. An exact interval
  boundary uses the new phase.
- Experiment Gameplay Time advances only in Playing, freezes in Countdown,
  Failed, and StageCleared, and resets on Retry/Replay.
- Collision priority remains Player, held Echo, Booster if already present in
  a constructed session, Shield, then ordinary failure. The Launcher disables
  Booster for both Hidden-only and Flicker-only runs.
- The pooled View shows `HIDDEN` for the memory mechanic and `FLICKER` for the
  color cycle. Flicker color and symbol derive from the same Core color, with
  only a short brightness pulse above the immediate logical change.

Iteration 7 Validation

- EditMode: 281/281 passed.
- PlayMode: 143/143 passed before Scene Builder and 143/143 passed again after
  two consecutive successful Scene Builder runs.
- Related passed cases: Echo 27/27, Shield 29/29, Camouflage 9/9, Hidden
  25/25, Flicker 27/27, and Experiment Runtime 5/5.
- The full Stage 1-11 campaign regression wrote 220 matrix rows, including
  the requested Stage 1-5 coverage; Summary, JSON, and CSV hashes are
  byte-identical to the Iteration 6 baseline.
- Step 10 wrote 80 rows and 64,016 runs; all five
  CSV/JSON/summary/comparison/shortlist hashes are byte-identical.
- The rebuilt scene has no Missing MonoBehaviour, required-reference, pool,
  duplicate-root, or Lab return/re-entry failure in the final PlayMode suite.
- Package manifest/lock and ProjectSettings have no intended change.
- Automated validation does not establish switching comfort, concept
  comprehension, boundary readability, fairness, satisfaction, or fun.

Next Iteration

Perform portrait-device human play review of Hidden/Flicker concept
separation, `0.50s` two-color timing, three-color difficulty, collision
boundary comprehension, pulse/symbol clarity, Echo/Shield explanation, and
minimum observation at speed. Do not apply Flicker to Campaign or combine
modifiers until that feedback is reviewed.

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
