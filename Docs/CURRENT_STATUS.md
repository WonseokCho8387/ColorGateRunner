# Current Status

Version

Iteration 11

(Campaign Progress Restore Hotfix: PlayMode PlayerPrefs isolation and
restart/re-entry regression coverage implemented)

---

Campaign Progress Restore Hotfix Result

- Campaign progress remains device-wide Unity `PlayerPrefs` owned only by
  `PlayerPrefsStageProgressStore`. It is not linked to the Product Guest and
  is not copied, reset, migrated, or written into `product-save.json`.
- Investigation found that two PlayMode fixtures deleted the real Editor
  `HighestUnlocked` and Stage 6 Record keys during cleanup without preserving
  their previous values. Windows Editor project copies share this PlayerPrefs
  namespace when Company and Product names match.
- All Campaign-affecting PlayMode fixtures now capture Highest Unlocked plus
  every Stage 1-13 Record before setup work and restore exact key existence,
  integer/string values, and `PlayerPrefs.Save()` in exception-safe cleanup.
- Regression coverage now proves Stage 11 first entry and re-entry, Lobby /
  PreRun / Gameplay stable-ID parity, fresh Stage 1 clear followed by Stage 2
  after controller recreation, new Guest preservation of existing Campaign
  progress, and missing/corrupt Highest fallback without Record deletion.
- Values deleted before this investigation cannot be reconstructed from
  evidence. The missing prior Highest Unlocked and Stage 6 Record are not
  guessed or automatically repaired.
- Profile-scoped Campaign progress remains deferred to the separately approved
  `StageProgressService` Iteration. A future testability improvement may inject
  an in-memory Campaign store, but this Hotfix does not alter runtime storage.

Iteration 11 Validation

- Snapshot Utility tests: `3/3`; Campaign restore regression tests: `4/4`.
- EditMode: `335/335`; PlayMode: `167/167`; post-Builder PlayMode: `167/167`.
- Frontend Builder, Boot Builder, and Campaign Builder each succeeded twice.
  Builder and PlayMode validation found no duplicate generated objects,
  Missing Script, Missing Reference, or required-reference failure.
- The actual Editor Campaign PlayerPrefs snapshot retained 11 existing entries
  and SHA-256
  `2A8AF71352FCCF9D80C8BDCB9BDC89FC3F61A7B356633C3423A847F576D0B6C9`
  before and after the test runs. The Product save file hash also remained
  unchanged, preserving the current Guest ID.
- Campaign simulation remains 260 rows with all three approved hashes. Step 10
  remains 80 rows / 64,016 runs with all five approved hashes.
- `ProjectSettings.asset`, Package manifest/lock, Scene assets, runtime
  Campaign code, Stage Catalog, balance, and Product Save Schema are unchanged.

---

Commercial Flow Foundation Frontend Iteration 2 Result

- `Frontend.unity` is Build Index 1 between Boot and the existing Campaign
  `SampleScene`. Boot serializes Frontend as its destination; Frontend
  separately serializes the existing active Campaign path. Runtime does not
  hardcode the `SampleScene` name.
- One `FrontendPageRouter` owns the Title/Lobby Page, blocking Modal, and
  Transition state. Page Views do not activate one another or load Scenes.
- Title shows the initialized Guest display name, truthful `GUEST` state,
  version, account-unavailable notice, and read-only Settings summary. Empty
  Terms/Privacy/Support actions are hidden.
- Placeholder Lobby shows `CONTINUE CAMPAIGN` without reading or estimating
  Stage progress. Currency, Event, Notification, and Lobby-theme slots remain
  inactive and consume no layout space.
- Frontend contains no AppRoot. It captures an immutable display Context from
  the Boot-initialized AppRoot. Direct Frontend execution without AppRoot
  displays blocking `BOOT REQUIRED` and disables Campaign entry.
- Lobby Back returns to Title, Title Back opens Exit Confirmation, a
  cancellable Modal consumes Back, and Transition state ignores Back and
  blocks duplicate input.
- Campaign entry loads the existing Scene once and retains its current Lobby,
  PreRun, Gameplay, result, progress, and return contracts. No Campaign-to-
  Frontend route was added.
- The approved portrait/Input baseline was first restored in isolated commit
  `c191826`; this Iteration does not modify `ProjectSettings.asset` again.

Iteration 10 Validation

- EditMode: `335/335` passed. PlayMode: `160/160` passed. The same counts pass
  after all final Builder runs.
- Frontend Builder succeeded twice, Boot Builder succeeded twice, and the
  Campaign Builder completed its two-build command successfully. Required
  references, Safe Area, one EventSystem, one primary Page, unique generated
  roots, and no Missing Script were validated.
- Stage 1-13 Campaign simulation remains 260 rows and preserves Summary/JSON/
  CSV hashes `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and preserves all five approved hashes. Package
  manifest/lock hashes remain unchanged. The only ProjectSettings change in
  this Iteration is adding Frontend to `EditorBuildSettings.asset`.
- Human review remains required for 9:16 small-screen readability, Boot-to-
  Title and Lobby-to-Campaign transition feel, Android Back behavior, UI input
  leakage on devices, and the intentional new-Lobby/old-Lobby UX handoff.

Deferred after Iteration 10

- Final Lobby, Campaign Page, Stage Detail, Gameplay/Results shells,
  StageProgressService integration, Campaign-to-Frontend navigation, external
  account/cloud, economy, events, analytics, ads, IAP, and final visual art.

---

Commercial Flow Foundation Iteration 1 Result

- `Boot.unity` is Build Index 0 and initializes one persistent `AppRoot`
  before loading the first active non-Boot Build Settings Scene. The serialized
  destination is `Assets/Scenes/SampleScene.unity`; runtime contains no
  `SampleScene` name hardcode.
- `AppRoot` owns one explicit service graph: Clock, Profile, Local Account,
  Settings, Local Save, and the ordered initialization pipeline. Individual
  services are not singletons and no mutable service locator was added.
- The new `ColorGateRunner.Product` assembly has no Unity reference.
  `Application.persistentDataPath`, `JsonUtility`, Scene loading, and Boot UI
  stay in the Unity Presentation adapter.
- A fresh local product save creates exactly one Guest with an injected-capable
  UUID and UTC clock. Reload preserves the Profile ID and updates Last Played.
  Device identifiers are neither read nor stored.
- Product save schema 1 contains Profile, Settings, SaveRevision, and
  LastWriteUtc. Candidate revision and timestamp are applied before temporary
  serialization and validation. Primary/temp/backup, corrupt-primary recovery,
  supported schema-0 migration, and future-version blocking are implemented.
- Desktop/native uses atomic replace when supported. WebGL explicitly selects
  backup-based recoverable replacement and never reports an atomic result.
- `PlayerPrefsStageProgressStore` remains the sole Stage progression owner.
  Product save has no Stage section, does not read/write progression keys, and
  performs no copy, migration, reset, or new Stage result write.
- Boot shows Loading, version, a blocking local-error panel, and Retry. Retry
  reuses the existing service graph; returning to Boot rejects duplicate
  `AppRoot` creation.

Iteration 9 Validation

- EditMode: `313/313` passed.
- PlayMode: `152/152` passed; the same `152/152` passed after Boot Builder and
  again after two Campaign Scene Builder runs.
- Boot Builder succeeded twice. Campaign Scene Builder succeeded twice. Both
  validate required references, Safe Area, one EventSystem, and no Missing
  Script; PlayMode validates the existing Campaign Scene after regeneration.
- Stage 1-13 Campaign simulation produced 260 rows and reproduced Summary,
  JSON, and CSV hashes `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 reproduced 80 rows, 64,016 runs, and all five Iteration 8 hashes.
- Package manifest and lock hashes remain unchanged. The only approved
  ProjectSettings diff is the Boot-first `EditorBuildSettings` Scene list.
- Human review remains required for Boot presentation, transition feel,
  understandable save-error text, restart persistence on Android/WebGL, and
  preservation of real device Stage progress.

Deferred after Iteration 9

- Title, Frontend/Lobby redesign, Campaign Page, Stage Detail, Gameplay shell,
  Results redesign, StageProgressService integration/migration, external
  login/cloud, economy, events, analytics, ads, and IAP.

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
- Human-play feedback raised the Hidden default hide lead from `0.65s` to
  `0.85s`. The `1.00s` minimum observation and `0.12s` transition remain.
  Level tuning treats shorter Camouflage reveal time as harder but longer
  Hidden memory time as harder.
- Flicker remains an ordinary gate modifier. Its plan fixes the base color,
  every active-palette color in player tap order, interval, deterministic
  phase offset, pulse, Gate ID, and selection seed without changing gate
  count. The authored `2/3` cycle-count setting is removed.
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

Hidden/Flicker Human-Feedback Revision Validation

- Baseline EditMode `283/283` and PlayMode `143/143` passed.
- Final EditMode `288/288` and PlayMode `144/144` passed. Hidden/Flicker
  focused coverage is EditMode `14/14` and `20/20`, PlayMode `6/6` and
  `10/10`.
- Scene Builder completed twice. The regenerated Launcher serializes Hidden
  `0.85s` and contains no Flicker cycle-count field; post-Builder PlayMode is
  `144/144`.
- Stage 1-11 Campaign simulation wrote 220 rows and retained all three hashes.
  Step 10 wrote 80 rows and 64,016 runs and retained all five hashes.
- Package manifest/lock and ProjectSettings have no intended change.

Authoritative Iteration 8 Result

- Campaign now contains 13 stable catalog stages. Stage 12 `HIDDEN MEMORY`
  has 50 gates and Stage 13 `FLICKER FLOW` has 52; both use Red, Blue, Green
  and retain selectable Shield and Booster.
- Stage 12 uses the approved Hidden `0.85s` lead and deterministically selects
  Gate IDs `8, 19, 22, 26`. Stage 13 deterministically selects Flicker Gate
  IDs `8, 14, 19, 22`.
- `FlickerGatePlan` and `DeterministicModifierPlanner` are shared by
  Experiment and Campaign. Runtime, View, tests, and simulation use the same
  absolute Gameplay Time phase calculation.
- Campaign Flicker collision resolves the active color at
  `StageSession.ElapsedPlayingSeconds`. Booster remains the existing
  start-at-`GO` auto-pass and receives no Hidden/Flicker exception.
- Flicker exposure is accepted only when six pooled-view spacings provide at
  least three `0.50s` cycles at the authored Booster speed. Item choice does
  not alter targets, cycles, or phase.
- Catalog revision 5 and the rebuilt Scene contain 13 buttons. Scene Builder
  completed twice and post-Builder PlayMode passed.
- Final EditMode is `294/294`; PlayMode is `147/147` before and after the
  second Builder run.
- Stage 1-13 simulation wrote 260 rows. Its first 220 Stage 1-11 rows are
  exactly equal to the previous JSON results. Perfect clears Stage 12 and 13
  under all four item combinations.
- New 13-stage artifact hashes are Summary
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  JSON
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and CSV
  `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and 64,016 runs with all five hashes unchanged.
  Package manifest/lock retain their prior hashes and ProjectSettings has no
  diff.
- Average/None first-attempt clear is `26.9%` at Stage 11, `16.2%` at Stage
  12, and `17.6%` at Stage 13. This mechanical drop is not auto-tuned and
  requires human difficulty review.

Next Iteration

Perform portrait-device human play of Stage 12 and 13. Record Hidden memory
difficulty, Flicker collision-boundary readability, whether post-Booster
Flicker encounters are sufficient, and whether the Stage 11 -> 12 difficulty
step is too large. Do not mix modifiers or auto-tune values from simulation
alone.

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
