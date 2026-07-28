# Decisions

## Milestone 1

Status: Accepted historical baseline. Replaced decisions are marked
`Superseded`.

- A tap while Ready starts the run without toggling the initial Red color.
- Taps while Playing toggle the player color between Red and Blue.
- Gameplay input is ignored while Dead, but the restart button remains active.
- A gate resolves when the player crosses its one-shot trigger plane.
- **Superseded by Step 4:** Speed updates immediately after reaching scores
  5, 10, 15, and so on.
- Restart reuses the configured seed.
- The initial seed is 12345.
- The deterministic pseudo-random number generator is xorshift32.
- Seed 0 is normalized to the fixed non-zero state `0x6D2B79F5`.
- Each xorshift32 step uses unchecked 32-bit unsigned arithmetic in this exact order:
  `state ^= state << 13`, `state ^= state >> 17`, `state ^= state << 5`.
- The least-significant raw bit maps to a gate color: `0` is Red and `1` is Blue.
- After applying the four-identical-color limit, the fixed first 32 gate colors for seed 12345 are:
  `Red, Blue, Red, Red, Blue, Red, Red, Red, Red, Blue, Red, Blue, Red, Blue, Blue, Red, Blue, Red, Red, Red, Blue, Blue, Red, Blue, Blue, Blue, Blue, Red, Red, Blue, Blue, Blue`.
- The endless run has no finish condition in Milestone 1.
- Existing unused Unity packages remain unchanged during Milestone 1.
- The graybox Red is sRGB `#E63946` and Blue is sRGB `#2D7FF9`.
- The player starts at world position `(0, 1, 0)`.
- The camera starts at world position `(0, 8, -10)` with fixed Euler rotation
  `(20, 0, 0)`.
- **Superseded by Step 4:** Gates use fixed six-unit spacing and a five-object
  pre-created pool.
- Movement translates the player and camera forward using the current Core
  session speed. It does not use forces, smoothing, or camera rotation.
- A gate resolves when the player's collider crosses that gate's one-shot
  trigger plane. `GateView` forwards the assigned color to `GameSession` and
  does not calculate the result.
- The generated scene root is named `ColorGateRunner_Graybox`.
- The scene-builder command is idempotent because it removes only the single
  named generated root, recreates its fixed hierarchy, updates generated
  materials at stable asset paths, and validates the rebuilt scene. Its batch
  entry point builds twice before validation to detect duplicate output.
- Post-processing is disabled in the graybox scene and on its camera.

## Step 4 playtest iteration

Status: Accepted historical baseline. Replaced decisions are marked
`Superseded`.

- Human playtesting found the initial Ready presentation ambiguous, base speed
  4 substantially too slow, fixed six-unit spacing overly mechanical, judgment
  feedback unclear, and the retry screen insufficiently motivating.
- **Superseded by Step 5:** Speed is
  `min(12, 6 + elapsedPlayingSeconds * 0.08 + score * 0.12)`.
- Elapsed gameplay time advances only while Playing. Ready and Dead do not
  advance it, and Retry resets it to zero.
- `GameSession.Advance(deltaSeconds)` is the sole time input to Core. Negative
  delta values are rejected with `ArgumentOutOfRangeException`.
- **Superseded by Step 5:** Gate spacing uses only `5.5`, `6`, `7`, and `8` units. A seed-derived offset
  selects the start of a repeating four-value pattern independent of the color
  PRNG, so the existing color golden sequence remains stable.
- Retry rewinds both color and spacing sequences using the configured seed.
- The five-object gate pool remains fixed; no gate is instantiated during
  normal gameplay.
- Ready uses a dimmed full-screen overlay with `COLOR GATE`, a one-line
  color-matching instruction, and a button-like `TAP TO START` visual. The
  full gameplay surface remains the actual input target.
- The score HUD uses a framed top-safe panel, a `SCORE` label, and a large
  numeric value. The numeric transform pulses for 0.2 seconds without storing
  state in UI text.
- A correct judgment starts in the resolution frame: a reusable particle burst
  and player scale response run for at most 0.25 seconds without pausing play.
- **Superseded by Step 5:** An incorrect judgment stops movement in the resolution frame, marks the gate
  and player with the failure material, shows game over immediately, and runs a
  fixed-position camera shake for 0.22 seconds. Camera rotation never changes,
  and the exact pre-shake position is restored.
- Core owns only the current score. `GameSceneController` owns the current best
  presentation and talks to `IBestScoreStore`; `PlayerPrefsBestScoreStore` is
  the production adapter and tests replace it with memory storage.
- **Superseded by Step 5:** Game over shows `GAME OVER`, current score, best score, and one explicit
  `RETRY` action. Retry returns to Ready and does not also start gameplay.
- BPM-based gate placement and all audio timing are deferred.
- Additional colors, obstacles, power-ups, combos, ratings, missions, and skins
  are deferred.

## Step 5 autonomous gameplay-feel iteration

Status: Accepted historical baseline. Replaced decisions are marked
`Superseded`.

- Playtesting found that Step 4 acceleration was technically continuous but
  not perceptually obvious, gate gaps still looked mechanical, immediate game
  over obscured the failed gate, and Retry required an unnecessary second tap.
- **Superseded by Step 6:** Speed is deterministic and ease-out:
  `min(12, 6 + (12 - 6) * (1 - exp(-0.1 * elapsedPlayingSeconds)) + score * 0.04)`.
  Time advances only while Playing; negative deltas remain rejected. Restart
  resets elapsed time and speed.
- **Superseded by Step 6:** A dedicated spacing xorshift32 stream is seeded with
  `normalizedSeed XOR 0x9E3779B9`. Each gap starts at
  `speed * 0.85 + 0.5`, rounds upward to 0.5 units, and adds a deterministic
  short/medium/long band offset of `0`, `1.5`, or `3` units. A band may repeat
  at most twice. Restart rewinds colors and spacing.
- The persistent top HUD is parented under `SafeAreaRoot`. `SafeAreaLayout`
  converts `Screen.safeArea` to normalized anchors and reapplies them only
  when the safe area or resolution changes.
- **Superseded by Step 6:** A lethal mismatch locks Core in Dead immediately, stops movement and input,
  highlights the failed gate, tilts and neutralizes the player, shakes only
  the fixed camera position, and delays the game-over panel by 0.85 seconds.
- **Superseded by Step 6:** Retry performs `Restart` followed by `StartRun`: it resets the complete run
  and presentation and resumes Playing immediately without showing Ready.
- **Superseded by Step 6:** A single non-stacking shield is granted at score 3. It consumes one
  mismatched gate, awards no point, and keeps Playing; the next mismatch is
  lethal. Retry clears it. The HUD shows active and unavailable states.
- **Superseded visually by Step 6:** Completed positive scores are inserted into a descending local Top 5.
  Duplicates are allowed, only five are retained, missing/corrupt PlayerPrefs
  data falls back to an empty list, and Best remains at least Top 5 rank 1.
  Persistence remains outside Core behind `IScoreHistoryStore`.
- Ready keeps the full-screen tap target and adds a subtle reusable text pulse;
  no imported visual assets are used. Correct gates reuse the existing player
  pulse, score pulse, and bounded ParticleSystem burst.
- Live-changing gate colors, visibility/fog modifiers, currency, paid or
  ad-based Continue, online leaderboards, extra colors, moving/fake gates,
  combos, audio, haptics, and BPM-based placement remain deferred.

## Step 6 perceptible game-feel and infinite-track iteration

Status: Accepted historical baseline. Replaced decisions are marked
`Superseded`.

- Step 5 human playtesting found that numerically valid acceleration and gap
  variance were not perceptible, shield state was effectively invisible,
  shield use had no recovery moment, defeat changed too abruptly, immediate
  Retry lacked preparation, Top 5 lacked hierarchy, and the fixed 1000-unit
  floor disappeared during long runs. Same-color runs with one exception gate
  produced the strongest excitement.
- **Superseded by Step 7:** Speed uses
  `min(14, 6 + (14 - 6) * (1 - exp(-0.14 * elapsed)) + score * 0.02)`.
  Four perceptual stages begin at 0, 5, 12, and 25 seconds. They control
  movement, camera FOV (`60/65/70/74`), trail intensity
  (`0.1/0.35/0.7/1`), speed-line rate (`0/18/40/70`), and an explicit HUD
  stage label.
- **Superseded by Step 7:** Gate generation uses a separate seeded pattern stream with authored
  `Steady`, `ShortShortLong`, `LongShortLong`, `Compression`, `Release`,
  `SameColorBait`, and `SingleColorBreak` patterns. The same pattern cannot be
  selected twice consecutively. Gaps are expressed as 0.9–1.9 seconds and
  converted to distance using current speed plus the 0.5-unit safety margin.
- `SameColorBait` intentionally produces base/base/base/exception/base and is
  unavailable during the first onboarding gates. Global color runs remain
  capped at four.
- The finite floor is replaced by six pooled 40-unit straight track segments.
  A segment whose end is more than 20 units behind the player moves to the
  farthest segment's end anchor. Normal play performs no Instantiate/Destroy.
- A 16-degree, eight-piece gentle curve exists as the inactive
  `CurveSegmentExperiment`. It is intentionally isolated until path-following
  player, camera, and gate placement can be validated without weakening the
  infinite straight-track guarantee.
- **Superseded by Step 7:** Score 3 grants one non-stacking shield, shown by a persistent
  rotating player ring, animated HUD state, centered `SHIELD` message, and
  reusable activation burst.
- **Superseded by Step 7:** Shield absorption enters authoritative `ShieldRecovery` for 2 seconds:
  0.12-second presentation hit-stop, 65% initial speed multiplier that
  recovers smoothly, blinking player, invulnerability, `SHIELD BREAK`
  messaging, distinct burst, and no score from protected mismatches.
- Lethal failure is authoritative immediately but presented over 0.9 seconds.
  The player progressively moves, drops, tilts, shrinks, and neutralizes while
  the camera shake eases and the failed gate stays marked. Game Over appears
  after the readable animation point.
- **Superseded by Step 7:** Retry performs Core Restart, enters a configurable five-second `Countdown`,
  displays `5–1` and `GO`, ignores gameplay input, and does not advance score,
  movement, or elapsed gameplay time. It transitions to Playing exactly once.
- **Superseded visually by Step 7:** Game Over separates `CURRENT`, a starred `BEST`, `NEW BEST`, and a framed
  `TOP 5`. Positive completed scores remain descending, capped at five, and
  duplicate scores rank after existing equal scores. The current row receives
  a visible marker, `RANK n` label, and a 0.6-second reusable pulse.
- Fog, live-changing gate colors, reduced visibility, currencies, paid or
  ad-based Continue, ads, online leaderboards, additional power-up types,
  extra player colors, audio, haptics, and BPM synchronization remain deferred.

## Step 7 cadence progression and replay motivation

Status: Superseded by Step 8 for the main player-facing mode. The deterministic
cadence, pattern, shield, and infinite-track code remains available for a
future optional Endless mode, but is not exposed by the Step 8 scene.

- Step 6 human playtesting found that initial acceleration was perceptible,
  but speed sensation fell after the opening camera movement. Speed-scaled
  spacing cancelled encounter-cadence growth, two colors made patterns
  repetitive, the visible shield state and break were clear but automatic
  acquisition lacked anticipation, two-second recovery and five-second
  countdowns were too long, and Game Over score hierarchy was unsuccessful.
  Death timing and the infinite straight track were successful. Camera motion
  communicated speed more strongly than track markers.
- Movement and encounter progression are separate deterministic values.
  Score-zero movement speeds at 0, 5, 10, 20, 30, and 45 seconds are
  `6`, `9`, `10.5`, `12.5`, `14`, and `15`. Linear interpolation between
  checkpoints keeps speed continuous, the score contribution is
  `0.01 * score`, and the result is capped at 15.
- Base target encounter intervals at the same checkpoints are `1.65`, `1.32`,
  `1.15`, `1.0`, `0.87`, and `0.82` seconds. A pattern interval is
  `max(0.75, targetInterval * beatMultiplier)`. Physical distance is that
  interval multiplied by movement speed plus a 0.5-unit margin, rounded up to
  0.5 units. The minimum guaranteed reaction time is therefore 0.75 seconds.
- For a Steady beat, the measured rounded distances and actual reaction times
  at 0/5/10/20/30/45 seconds are respectively
  `10.5/1.667`, `12.5/1.333`, `13/1.190`, `13/1.000`,
  `13/0.893`, and `13/0.833` (distance/time).
- Authored beat multipliers are: Steady `1,1,1,1`; Compression
  `1.35,1.15,0.95,0.75`; Release `0.75,0.95,1.15,1.35`; Syncopation
  `1,0.65,1.35,1`; Burst `0.7,0.7,1.5`; SameColorBait
  `1,1,1,0.8,1.2`; SingleColorBreak `0.9,0.9,0.9,0.9,1.3`; and late
  ThreeColorFlow `0.9,0.9,0.9,1.15`.
- Early play unlocks only Steady, Compression, and Release. Middle play adds
  Syncopation, Burst, and SameColorBait. Late play adds SingleColorBreak and,
  after Green is active, ThreeColorFlow. Neither of the two most recent named
  patterns may be selected, repeated bait base colors are shifted, and global
  same-color runs remain capped at four.
- Normal three-color generation may keep the same color or advance by exactly
  one position in the Red, Blue, Green cycle. It never requests a two-tap
  transition at normal late-stage cadence. The slower first Green tutorial
  gate is the only intentional introduction that may teach a two-tap change.
- The Step 7 seed-12345 diagnostic first 40 generated colors are:
  `Red, Red, Blue, Red, Red, Blue, Red, Blue, Blue, Red, Blue, Red, Red,
  Red, Red, Blue, Green, Green, Red, Blue, Green, Red, Red, Blue, Green,
  Green, Green, Green, Red, Blue, Green, Red, Red, Red, Blue, Green, Red,
  Blue, Green, Red`. The associated pattern, beat multiplier, target interval,
  rounded distance, and actual reaction time are emitted by
  `PatternSequence_DiagnosticReportsFirstFortyCadenceValues`.
- Green unlocks exactly once at score 15. Before then, generation and input use
  only Red and Blue. The introduction shows `NEW COLOR`, changes the compact
  cycle indicator to `RED > BLUE > GREEN`, and queues slower Green then Red
  tutorial gates. Retry restores the two-color state.
- Automatic score-based shield granting is removed. The first visible shield
  pickup uses the configured seed to choose plan index 6 through 8, is placed
  halfway between gate decisions, rotates and bobs, uses one fixed pooled
  object, and cannot stack. Collection activates the existing shield effect.
- Shield hit-stop remains 0.12 seconds. Authoritative invulnerability is
  exactly 1 second and begins at 70% of the underlying movement speed before
  restoring it continuously. The underlying elapsed progression remains
  intact.
- The first title tap and Retry both use one Core `Countdown` path with a
  configurable default of 3 seconds and the visual sequence `3, 2, 1, GO`.
  Countdown accepts no color input and advances no movement, elapsed gameplay,
  score, cadence, or gate state.
- Game Over uses one Safe-Area result card. `THIS RUN` is the largest number
  and repeats a subtle independent pulse. Best uses a gold badge and accent,
  `NEW BEST!` has a dedicated pulse, and five fixed rank rows use aligned
  rank/score/marker columns. The current run has a blue row highlight and
  `YOU`; the gameplay HUD is hidden while results are visible.
- Developer pacing diagnostics are disabled by default. When explicitly
  enabled in Editor or development builds, the panel refreshes at 0.25-second
  intervals and shows movement speed, encounter interval, speed stage, active
  pattern, active color count, and shield/recovery state. It is not part of
  normal release presentation.
- The curve-track experiment remains isolated and disabled. Live-changing
  colors, fog, fake gates, currency, Continue, advertising, audio/BPM
  synchronization, online leaderboards, skins, other power-ups, and a fourth
  color remain deferred.

## Step 8 stage progression and start-item loop

Status: Superseded by Step 9A for the player-facing navigation, Continue,
readability, and simulation workflow. The five-stage definitions and start
items remain the foundation.

- Human review of Step 7 established that a score-only endless terminal did
  not provide a clear short-term objective or replay route. The main mode is
  now five finite stages with an explicit Stage Select, free start-item
  selection, one countdown, a visible Goal, and distinct clear/fail results.
- `StageCatalog` is the authoritative data source for five prototype stages.
  IDs are `stage-01` through `stage-05`; display numbers are sequential.
  Targets are 24, 28, 30, 32, and 36 gates. Final-pressure sections contain
  4, 5, 6, 6, and 8 gates.
- Stage 1 uses Steady/Release, Stage 2 adds cadence contrast, Stage 3 uses
  conservative bait/break patterns, Stage 4 safely introduces Green after
  eight two-color gates, and Stage 5 uses all three colors from the start.
  Stages 1–3 never generate Green and score 15 no longer changes their colors.
- Stage movement is deterministic and progress-based. Starting/maximum speeds
  are `7/10`, `8/11`, `8.5/11.5`, `8/12`, and `9/13.5`. Start/end target
  cadences are `1.55/1.25`, `1.45/1.10`, `1.40/1.00`, `1.45/1.00`, and
  `1.25/0.85` seconds. All remain above the 0.75-second reaction floor.
- Explicit active flow states are `StageSelect`, `PreRunSelection`,
  `Countdown`, `Playing`, `ShieldRecovery`, `StageFinishing`,
  `StageCleared`, and `Failed`. Panels mirror these states; panels are not the
  source of truth.
- Shield and Booster are free, unlimited start toggles. They activate only
  after `GO`. Retry returns to PreRun selection and retains both toggles.
  Runtime shield pickup is removed from the generated main scene.
- A selected Shield absorbs one mismatch, counts that gate as stage progress,
  and provides exactly one second of recovery. It cannot stack.
- A selected Booster uses stage speeds `22`, `23`, `24`, `25`, and `26` for
  deterministic distances `80`, `90`, `100`, `105`, and `115` units.
  During Booster, gate crossings advance stage progress without accuracy
  judgment and do not consume Shield. When its distance ends, speed returns
  over a deterministic 0.35-second ease to the normal value at current stage
  progress.
- Booster alone raises camera FOV from 60 to 70 and enables pooled speed lines
  and the existing player trail. Normal play keeps FOV at 60. Camera rotation
  is fixed. Booster and gate reactions are reset when the run or pooled gate
  is reused.
- The final gate enters `StageFinishing`; failure judgments are ignored from
  that point. A visible Goal is placed ten units after the final gate.
  Crossing it resolves clear exactly once.
- Per-stage persistence owns highest unlocked stage, cleared state, best time,
  best no-item time, and clear count. Lower time is better. Item-assisted
  clears may update overall best but never no-item best. Invalid serialized
  data safely becomes an empty record. Core contains only record policy;
  PlayerPrefs is isolated behind `IStageProgressStore`.
- Clear unlocks at most the next stage. Clear results show stage, time, used
  items, best time, Next Stage, Replay, and Stage Select. Failure results show
  progress, used items, Retry, and Stage Select.
- The generated root remains `ColorGateRunner_Graybox`. The builder destroys
  only the previous generated root and known default scene roots, then
  recreates fixed arrays and serialized references. Running it twice is the
  idempotency check.
- The Step 7 `GameSession`, score milestone, Top 5, and runtime pickup code is
  retained only as non-exposed reusable legacy code. An optional Endless mode,
  direct color buttons, economies/consumables, additional colors, new
  mechanics, audio/BPM placement, ads, and online features are deferred.

## Step 9A lobby, Continue, readability, and simulation

Status: Superseded in part by Step 9A.1 for Booster/Continue transition
continuity and movement-scale tuning. Unrelated Step 9A decisions remain
active.

- Tap-to-cycle remains the active core input method. Direct color buttons
  remain deferred and require new evidence before reconsideration.
- Normal progression uses one current-stage Lobby rather than a player-facing
  stage browser. It selects the lowest unlocked uncleared stage, or Stage 5
  when all prototype stages are cleared. The old picker is Editor/development
  only and hidden by default.
- Lobby visual tiers are data-derived: Tier 0 initially, Tier 1 after Stage 2,
  Tier 2 after Stage 4, and Tier 3 after Stage 5. No currency, placement, or
  decoration economy is introduced.
- The gameplay HUD explicitly displays current color, active cycle order, and
  the next tap color. It is read-only and never resembles input buttons.
- Shield is visible during the initial countdown but protection begins only
  when Core completes it. Six thin generated ring segments replace the
  color-obscuring sphere. Continue never restores a consumed Shield; Retry
  reapplies retained selection.
- Booster uses exclusive 74-degree FOV, launch shake/pulse, speed lines,
  trail, and a platform-safe haptic request. A distance meter warns during the
  final 20%. Two post-Booster gates are reserved: current color, then at most
  one tap, each with at least 1.35 seconds. Normal FOV remains 60.
- Gates contain Left, Right, and Top parts. Booster impact separates/rotates
  the pooled parts and every transform/material resets before reuse.
- One free Continue is available per attempt. It preserves stage progress and
  color, bypasses item selection, starts one countdown, grants one second of
  post-GO protection, and uses the same two-gate safe sequence. Booster and
  start items are not restored. A second failure has only Retry and Lobby.
- Continued clears may unlock progression and increment clear count but never
  update overall or no-item Best. Continued clears are counted explicitly.
- Failure UI waits 1.0 seconds for the failure animation. Clear UI waits 1.2
  seconds for the finish animation and positive particles. Clear and Failed
  have different roots, hierarchy, palette, and primary actions.
- Stage 2 uses five authored sections: Steady, Compression, Release,
  Syncopation, Mixed Final. Stage 3 uses alternating Red/Blue-led exception
  sections and a Release/Burst finish.
- Gameplay-affecting development requires Core simulation using the same
  StageSession, stage definitions, gate plans, items, Continue, and completion
  rules. Perfect, Expert, Average, Novice, and Stress profiles are provisional
  mechanical models, not representations of real players.
- Development telemetry is local CSV, disabled by default outside explicit
  Editor/development use, contains no identifiers, and performs no upload.
- Adaptive Assist activation remains disabled. Only an eligibility snapshot
  is retained for future visible, optional assistance after calibration.
- Direct color buttons, player-facing stage browser, decoration economy,
  currency/item quantities, hidden adaptive difficulty, Assist activation,
  online analytics, additional power-ups/stages, and Endless release remain
  deferred.

## Step 9A.1 Booster exit and Continue continuity hotfix

Status: Active

- Playtesting exposed a visible empty interval after Booster and a scene-like
  restart after Continue. Both were caused by rebuilding the deterministic
  sequence and reactivating the complete six-gate pool at a new lead
  position.
- The previous Step 9A rule that forced both safe gates to at least 1.35
  seconds by changing their spacing is Superseded. Booster exit and Continue
  now preserve every active gate transform, global gate index, authored
  spacing, pattern metadata, stage timer, progress, and sequence cursor.
- One `StageSession` gate sequence is authoritative. Presentation consumes
  plans from that sequence for its fixed pool; Booster exit, Continue, Shield
  recovery, and protection never create or rewind a replacement sequence.
- During the Booster final-20% warning and after Continue selection, only the
  next two unresolved, already-positioned gates may receive temporary color
  overrides. The first uses the current player color, the second uses either
  that color or the next cycle color and therefore needs at most one tap, and
  the third retains its original authored plan. `GatePlan.PlannedColor` and
  `HasTemporaryColorOverride` preserve telemetry distinction.
- Failure captures an authoritative in-memory snapshot containing stage ID,
  elapsed play time, normalized progress, traveled distance, normal speed,
  player color, sequence cursor, failed global gate index, final/Goal state,
  Continue state, selected/consumed Shield state, selected/completed Booster
  state, player and camera transforms, and every gate plan, transform,
  resolved state, part transform, and pool identity.
- Continue overlays `3, 2, 1, GO` on the frozen failure scene. It does not
  rebuild gates or tracks, reset the camera, reopen item selection, restore
  Shield, or restart Booster. At GO, the failed gate is counted/resolved
  exactly once, one second of protection begins, and normal recycling alone
  may later move that gate behind the player.
- Stage movement values are doubled at the user's direction. Starting/maximum
  speeds are `14/20`, `16/22`, `17/23`, `16/24`, and `18/27`; Booster speeds
  are `44`, `46`, `48`, `50`, and `52`. Booster distances are also doubled to
  `160`, `180`, `200`, `210`, and `230`, the initial lead becomes 24 units,
  and Goal distance becomes 20 units. Because authored spacing is computed
  from speed multiplied by cadence, this doubles spatial scale while
  preserving the existing time-based decision cadence and approximate
  Booster duration.
- Local development telemetry records both planned and effective colors plus
  the temporary-override flag. Simulation reports before/after next-gate
  distances, transform displacement, index continuity, sequence cursors, and
  first-three-gate post-transition failure rates. These are mechanical
  continuity measurements and do not claim perceptual smoothness.

## Step 9C mobile UI readability and art-direction integration

Status: Active

- The old player-facing UI hierarchy is Superseded. Generated UI now uses
  seven explicit roots: `LobbyRoot`, `PreRunRoot`, `GameplayHudRoot`,
  `CountdownRoot`, `ClearResultRoot`, `FailedResultRoot`, and
  `DevelopmentDebugRoot`. Countdown deliberately overlays the gameplay HUD;
  other player-facing flows show one primary root.
- The unexplained blue horizontal bar was the Booster distance meter. It was
  always present under `SafeAreaRoot` at near-full width. It now lives inside
  the compact top HUD, is labeled `BOOST`, and is active only while Core says
  Booster is active.
- The Step 9A decision to show current color, full cycle order, and next color
  as sentences is Superseded. `MobileUiPolicy` derives two or three ordered
  read-only tiles from `StageDefinition` and stage progress. Red/circle,
  Blue/square, and Green/triangle are fixed accessibility mappings. Current
  is largest and next is marked.
- Lobby tier thresholds remain unchanged. Only the derived active tier
  environment is visible; the `BASE` and `UP 1` through `UP 3` labels are
  removed. The development picker stays isolated and hidden.
- The recognition corridor is reserved for the runner and upcoming gates.
  Color tiles remain upper-left, stage/item information remains at the top,
  normal speed-line emission and the former central player trail are
  disabled. Booster uses two fixed pooled edge emitters and fully clears them
  on exit and flow reset.
- Gate logic is unchanged. A generated circle, square, or triangle repeats
  the tile mapping on each pooled gate's top crossbar for redundant color
  recognition.
- Safe Area and layout evidence targets `1080x1920`, `1170x2532`,
  `1080x2400`, and `1440x3200`, plus a simulated 1080x2400 top-notch inset.
  Screenshots are evidence for human review, not proof of visual quality.
- Stage definitions, movement scale, cadence, item powers, Continue behavior,
  deterministic sequences, simulation profiles, packages, input, and
  unrelated ProjectSettings remain unchanged.
