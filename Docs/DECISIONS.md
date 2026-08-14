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

## Step 10 color capacity and gate mechanic lab

Status: Active development-only experiment policy.

- Tap-to-cycle remains active through six-color experiments.
- Three through six colors are development experiments. Yellow, Purple, and
  Cyan are appended to the stable enum and are not normal Stage 1–5 content.
- The Step 9C fixed color-row presentation is Superseded. The active HUD is a
  current-first vertical stack with fixed tile identities. Rapid taps interrupt
  and retarget all tiles from their current visual transforms; the Core color
  changes immediately and no input queue delays it.
- Lobby progression keeps its data-derived palette tier, but the unexplained
  tall gray side blocks are removed. Only a quiet background treatment remains.
- Booster presentation uses normal `(0,8,-10)/(20,0,0)/60` and a chase target
  relative offset `(0,5.4,-7.4)`, rotation `(14,0,0)`, FOV `78`, with
  `0.22s` blend in and `0.35s` blend out. Retry, Continue, exit, and failure
  converge on the exact baseline without cumulative drift.
- Development Reset Progress requires confirmation. It deletes only the
  highest-unlocked key and the five stage-record keys, returns the Lobby to
  Stage 1, and preserves unrelated PlayerPrefs. Cancel performs no write.
- Camouflage reveals when one unpassed gate remains before it. The sequence
  index, spacing, and transform are unchanged.
- Fog fully reveals the nearest two unpassed gates. Farther gates are neutral
  silhouettes; their plans remain unchanged.
- Ice increases speed while retaining color judgment. Initial experimental
  values are speed `1.45x`, spacing `1.30x`, entry `0.30s`, exit `0.35s`,
  gates `12–19`.
- The 16-condition matrix pairs the same seed and planned sequence. Perfect
  runs once and Expert/Average/Novice/Stress run 1,000 seeds each, for 64,016
  first-pass runs.
- Repeated-tap profile fields are explicit and provisional. Risk labels use
  the documented interval, tap-burst, completion, and failure-share thresholds.
  Simulation shortlists human-test candidates but cannot determine fun.
- Experiment play and reset testing are separately controlled. Creating or
  leaving an experiment session does not read or modify normal progression.

Deferred: Flicker Gate, Clone Gate, permanent four-through-six-color
progression, reverse-cycle input, swipe-to-previous-color input, combined
mechanics, and production adaptive difficulty.

## Step 10.1 human-playtest fixes and Experiment Lab access

Status: Active. The Continue timing and Stage 4 color-activation details below
Supersede only the conflicting Step 9A.1 and Step 9C statements; their other
continuity and readability decisions remain historical and active.

- The experiment launcher already existed under
  `DevelopmentDebugRoot/ExperimentLauncherPanel`, but no Lobby action selected
  the Development flow. A secondary `EXPERIMENT LAB` Lobby button is now the
  explicit access path in the Unity Editor and Development Builds. It is hidden
  in release builds, opens the existing launcher, returns to the Lobby on
  leave, and never writes normal stage progression.
- Stage 4 makes Red, Blue, and Green available to the input cycle and HUD from
  the initial countdown. Gate-color availability is separate deterministic
  data: Green first appears at zero-based gate index `8` (the ninth gate).
  The first eight authored gates are `Red, Blue, Blue, Blue, Blue, Red, Red,
  Blue`; seven of eight transitions need zero or one tap. Their cadence remains
  at the generous Stage 4 starting cadence of `1.45s`. Stage 1–3 color rules
  are unchanged.
- A `StageGoalPlan` deterministically sums the configured initial lead, every
  authored gate spacing, and the 20-unit finish offset. The Goal is activated
  once at run setup and is never repositioned when the final gate resolves,
  Booster ends, or Continue is used. Stage 4 places it `826.9061` units ahead
  of the player; the camera far plane is 500 units, so it enters view
  continuously from that range.
- The generated Goal is a neutral marathon finish made from two posts, an
  overhead crossbar and `FINISH` banner, and a floor line. It has no
  color-match or collision requirement.
- The Step 9A.1 rule that delayed failed-gate consumption until `GO` and kept
  the visual failure snapshot through the countdown is Superseded. Selecting
  Continue now consumes the failed gate exactly once immediately, deactivates
  that pooled view, preserves the same `StageSession` and sequence, and places
  the player no closer than 24 units before the next unresolved gate.
  Rotation, scale, current-color material, Rigidbody linear/angular velocity,
  camera shake/FOV, and Booster presentation are reset before the countdown.
  Other gate and track transforms stay frozen; no pool is rebuilt.

## Iteration 1 Experiment Runtime Flow and Stage 4 simulation parity

Status: Active.

- Experiment uses the existing `StageFlowState` as its single Core flow source:
  `Countdown`, `Playing`, `Failed`, and `StageCleared` as the experiment's
  completed state. The former mutable failure/completion booleans are replaced
  by properties derived from that state.
- Experiment launch and every Retry/Replay begin at the shared three-second
  countdown. Movement, color input, gate judgment, item activation, and elapsed
  experiment time advance only while `Playing`.
- Failed and completed experiments reuse the generated campaign result roots.
  Failure offers `Retry Same Test` and `Back to Lab`; completion offers
  `Replay` and `Back to Lab`. Experiment Continue, rewards, scores, grades, and
  statistics screens remain absent.
- Retry and Replay restart the same `ExperimentSession` definition. Color
  count, mechanic, seed, selected items, settings, and deterministic gate
  sequence are preserved; transient runtime and presentation state reset.
- Result actions use the controller's existing fixed button listeners with
  flow-aware handlers. No runtime listener registration or second result-flow
  object graph is introduced.
- Experiment start, failure, completion, Retry, Replay, and Back to Lab do not
  load or save normal campaign progress. Back to Lab returns directly to the
  existing development launcher.
- The Stage 4 Perfect first-attempt failure at zero-based gate index 5 was a
  simulator/runtime rule mismatch. Runtime used the Step 10.1 three-color
  `Red, Blue, Green` input cycle from countdown, while the simulator retained
  the superseded two-color intro calculation and applied its one computed tap
  through the three-color runtime session, landing on Green instead of Red.
- `StageDefinition` now owns the pure active-color count, next-color, and
  required-tap calculations. Runtime input, HUD policy, safe-color selection,
  and simulation share that rule. No gate count, speed, cadence, timing window,
  profile accuracy, or Stage 4 exception was changed.
- Clone, Flicker, Pattern/Section metadata, and Experiment Continue remain
  deferred.

## Iteration 2 campaign speed-scale playtest adjustment

Status: Active.

- Human playtesting found that the campaign did not communicate enough speed.
- Normal campaign starting/maximum speeds are doubled to `28/40`, `32/44`,
  `34/46`, `32/48`, and `36/54` units per second.
- Campaign Booster speeds are doubled to `88`, `92`, `96`, `100`, and `104`.
  Booster travel distances are doubled with them to `320`, `360`, `400`,
  `420`, and `460` units.
- Gate cadence is unchanged. Since authored spacing is speed multiplied by
  cadence, gate spacing doubles with movement speed. Campaign initial lead
  becomes 48 units and Goal distance becomes 40 units.
- This preserves the existing time-based input windows, approximate stage
  duration, and approximate Booster duration while increasing world motion.
- Experiment Lab remains isolated at its existing movement values and
  24-unit initial lead.
- Automated validation may establish deterministic timing and continuity, but
  not perceived speed, comfort, fairness, or fun. Those remain human
  playtest judgments.

## Iteration 3 Clone-only Experiment contract

Status: Superseded by Iteration 4 Echo Modifier Design. Retained as history.

- Clone extends the existing deterministic Experiment gate sequence. No
  second generator, runtime flow, collision system, or Pattern Generator is
  introduced.
- `CloneSettings` explicitly owns one-based non-Clone Source indices
  `[3, 6]` and `GapSeconds = 0.45`. Exactly two Clone Gates are inserted,
  immediately after those Sources, in that order.
- Source selection is not random. Seed still determines the ordinary
  Experiment gate colors and spacing; the explicit Clone relationship and
  insertion locations replay identically for every retry.
- A Clone copies its Source target color and is a separate, one-shot judgment
  included in Experiment completion. It cannot be a Goal or Source and cannot
  create another Clone. One Source owns at most one Clone.
- Clone gap distance is the Experiment's normal base movement speed at that
  Source multiplied by `0.45` seconds. Booster does not affect the calculation
  and is disabled by the Clone-only Launcher condition.
- Definitions with fewer than six non-Clone gates reject these settings
  explicitly; they never select replacement Sources.
- Shield uses the shared mismatch protection path. One active Shield consumes
  once, passes the mismatched Clone, and preserves Playing.
- Camouflage remains presentation visibility only: Clone hides and reveals
  under the ordinary rule, then uses normal judgment and failure behavior.
- Clone failure uses the existing `Failed` flow but carries a distinct failure
  cause for the result presentation.
- Retry/Replay reset gate View, visibility, reaction, and judgment state while
  preserving the same seed, definition, Source/Clone relationships, colors,
  and positions.
- Campaign stages and the 16-condition Step 10 matrix remain unchanged.
  Flicker, color-changing Clone, multiple/recursive Clone, Clone chains,
  combined-mechanic balancing, Pattern/Section metadata, and Experiment
  Continue remain deferred.

## Iteration 4 Echo Modifier and configurable campaign expansion

Status: Approved implementation contract.

- `StageCatalogAsset` is the only production campaign-stage value source.
  Runtime and Editor simulation consume the same adapter output through
  `IStageCatalog`; Core keeps `noEngineReferences: true`.
- Unity `AnimationCurve` data is converted once into deterministic pure-Core
  curve data. Runtime, ETA, and simulation use the same evaluator.
- None, Camouflage, Fog, Ice, and Echo Provider share one minimal GateModifier
  representation. This is not approval for Flicker or a general modifier
  composition framework.
- `EchoOfferCoordinator` owns deterministic candidates, one locked active
  offer, one held Echo, cooldown, acquisition count, and Retry/Replay reset.
  Presentation only reports stable gate IDs and pool lifecycle events.
- Echo is an ordinary gate modifier. It adds no judgment gate and stores the
  effective color only after an exact player-color pass.
- Judgment priority is Player, Echo, Shield, Failure. Echo-assisted and
  Shielded passes never acquire Echo.
- Camouflage reveal begins when deterministic ETA is at or below its
  stage-authored lead time, fades over its authored transition, and never
  hides again.
- Stage base speed is
  `Lerp(StartSpeed, MaxSpeed, Curve(progress))`. Booster and Ice apply after
  base speed, and Booster is not capped by `MaxSpeed`.
- Stage 6 grants one stage-local Shield at start. The former Stage 7
  30%-progress Booster activation is superseded by the human-feedback
  decision below.
- Campaign expands sequentially through Stage 11 using stable IDs and
  data-bound UI. Existing Stage 1–5 and Step 10 outputs are regression
  baselines, not values to be silently updated.
- Simulation candidate selection may tune only documented stage-authored
  colors, speed profile, cadence, and mechanic settings. No runtime adaptive
  balancing or player-profile manipulation is allowed.

## Iteration 4 implementation outcome

Status: Implemented and validated.

- The final simulation-informed candidate uses two active colors in Stages
  6-10 and three in Stage 11. This is authored catalog data, not adaptive
  difficulty.
- Stage 6-11 speed profiles and cadence remain initial human-play candidates.
  Automated evidence did not authorize further tuning.
- Clone types, Source relationships, gap insertion, Clone failure, and Clone
  presentation are absent from active code. Historical Clone text remains
  only as a superseded decision record.

## Iteration 5 provided-item timing and high-speed progression

Status: Approved human-feedback correction.

- `PROVIDED` means the item becomes active immediately after the same
  countdown boundary as a player-selected item.
- Stage 6 keeps one provided Shield at start. Stage 7 now provides one
  Booster at start; the former 30% activation contract is superseded.
- Stages 1-5 reject Shield and Booster selection in both catalog data and the
  PreRun UI. Stage 6-11 retain their existing allowed-item values, with
  duplicate selection blocked for a provided item.
- Gate judgment remains one-shot through `StageGateView.TryResolveCrossing`.
  Player movement crossing an unresolved gate plane is a deterministic
  fallback for a missed physics Trigger and reuses that same judgment path.
- The fixed six-gate pool, authored gate sequence, Booster speed/distance,
  stage curves, and Goal contract are unchanged.

## Iteration 6 Flicker-only Experiment (renamed to Hidden in Iteration 7)

Historical naming note: every `Flicker` reference in this Iteration 6 decision
means the one-way memory mechanic now named `Hidden`, not the later
color-cycling Flicker.

Status: Approved implementation contract.

- Extend the shared `GateModifier` representation with Flicker and extend the
  existing deterministic Experiment sequence; do not add a second generator,
  collision path, judgment system, or Pattern/Section framework.
- `FlickerSettings` owns Enabled, eligible progress bounds, occurrence chance,
  minimum gate cooldown, reveal duration, hide lead time, transition duration,
  maximum occurrences, and first-occurrence guarantee. Invalid values fail
  explicitly rather than being silently changed at runtime.
- Selection is stable for the same seed and settings, excludes non-judgment
  targets, respects eligible bounds, cooldown, and maximum count, and
  guarantees one eligible occurrence only when configured.
- View binding never changes selection. Countdown does not advance readable
  time. During Playing, hide starts only after
  `VisibleElapsed >= RevealDurationSeconds` and
  `ETA <= HideLeadTimeSeconds`.
- Hide is one-way and presentation-only. Target color and symbol disappear;
  neutral silhouette, judgment opening, and Flicker identity remain.
- Existing Player → Echo → Shield → Failure priority is authoritative.
  Flicker does not add a pass, defense, or failure cause.
- Retry/Replay rebuild the same deterministic plans and reset all per-View
  Flicker runtime state.
- Flicker-only Launcher runs disable Booster and do not mix Flicker with
  Camouflage, Fog, Ice, Echo Provider, or campaign stages.
- Campaign Stages 1-11, the Step 10 matrix, packages, and ProjectSettings are
  regression boundaries.

Implementation outcome: completed and validated with the shared modifier,
existing deterministic sequence, pure-Core one-way visibility state, shared
ETA, ordinary judgment, and fixed View pool. EditMode 262/262, PlayMode
134/134, post-Builder PlayMode 134/134, all three campaign hashes, and all five
Step 10 hashes passed their required boundaries.

## Iteration 7 Hidden migration and color-cycling Flicker

Status: Approved implementation contract.

- The Iteration 6 `Flicker` decision is retained historically but renamed to
  `Hidden` in active code and current documentation. Its one-way visibility
  behavior, ordinary judgment, deterministic selection, and Retry/Replay
  behavior do not change.
- `GateModifierType.Hidden` retains numeric value `1 << 4` and
  `MechanicExperimentType.Hidden` retains numeric value `5`. The new Flicker
  uses `1 << 5` and mechanic value `6`. Enum insertion or reordering that
  moves existing values is forbidden.
- Unity Launcher fields formerly named for the old Flicker migrate to Hidden
  through `FormerlySerializedAs`. Unity migration attributes remain outside
  the pure Core assembly.
- New Flicker is an ordinary-gate modifier using the existing Experiment
  sequence, flow, collision, judgment, and six-View pool. It adds no gate,
  second generator, or Flicker-specific result flow.
- A Flicker plan owns deterministic unique two- or three-color cycles,
  switch interval, phase offset, transition pulse, base color, Gate ID, and
  the existing seed context. Runtime switching consumes no random values.
- `ExperimentSession.ElapsedPlayingSeconds` is the single time source for
  Core calculation, View presentation, collision-time judgment, tests, and
  Flicker simulation checks. `Time.time`, realtime, coroutines, View
  activation time, and material readback are not judgment inputs.
- Exact switch boundaries use the new phase. Player → Echo → Shield → Failure
  remains authoritative against that collision-time color.
- Deterministic target selection reuses the existing sequence-side policy,
  excludes Goal/non-judgment and any other modifier, and also requires the
  configured minimum visible cycles under the existing speed/spacing and
  six-View exposure model.
- Booster is disabled in Hidden-only and Flicker-only Launcher runs.
- Campaign Flicker, music/BPM/DSP integration, rhythm scoring, combinations
  with Hidden/Camouflage/Fog/Ice/Echo Provider, speed-based interval
  adjustment, new colors, new input, and Stage 6-11 changes are not approved.
- Automated evidence may verify determinism and mechanics but cannot establish
  timing readability, fairness, comfort, satisfaction, or fun.

## WebGL test-build portrait frame

- The WebGL test-build menu owns a `540 x 960` logical canvas and a custom
  project template that preserves an exact `9:16` DOM aspect ratio.
- The canvas scales inside the available browser or iframe area without
  stretching. Unused horizontal or vertical space remains a neutral
  letterbox.
- WebGL width, height, and template settings are temporary build inputs and
  are restored after success or failure. Android build settings are
  unaffected.
- This is test-build presentation policy, not a Campaign, camera, safe-area,
  gameplay, or persistent ProjectSettings balance change.

## Hidden lead-time and Flicker palette-order revision

Status: Approved from portrait human-play feedback.

- Hidden default `HideLeadTimeSeconds` changes from `0.65` to `0.85`. Its
  `1.00s` minimum readable duration and `0.12s` transition remain unchanged.
- Level difficulty treats ETA oppositely for the two visibility mechanics:
  Camouflage becomes harder as reveal-to-arrival time shrinks; Hidden becomes
  harder as hide-to-arrival memory time grows.
- The Flicker-authored `CycleColorCount` setting and its `2/3` restriction are
  removed. A plan derives its cycle from every currently active color.
- The plan starts with its effective base color and advances through the same
  active-palette order as player tap-to-cycle input. The same Core next-color
  rule must feed both systems, so every Flicker transition is one forward tap
  from the previous displayed color.
- Seeded target selection, deterministic phase offset, `0.50s` interval,
  collision-time judgment, Player/Echo/Shield/Failure priority, Retry/Replay,
  minimum exposure, View behavior, and Booster exclusion remain unchanged.
- This revision does not apply Flicker to Campaign, mix modifiers, introduce
  colors, change player input, or claim that automated tests establish
  readability, mastery, fairness, satisfaction, or fun.

## Campaign Hidden and Flicker stage expansion

Status: Implemented / Human Review Pending.

- The earlier Campaign-Flicker deferral is superseded only for two isolated
  Campaign stages: Stage 12 Hidden and Stage 13 Flicker.
- Stage IDs are `stage-12` and `stage-13`; prior stable IDs and Stage 1-11
  authored data remain unchanged.
- Stage 12 has 50 gates, three active colors, base speed `46 -> 68`, cadence
  `1.05 -> 0.78s`, and the approved Hidden defaults.
- Stage 13 has 52 gates, three active colors, base speed `46 -> 68`, cadence
  `1.08 -> 0.80s`, and the approved full-active-palette Flicker cycle.
- Campaign and Experiment share one deterministic Hidden/Flicker planning and
  Flicker phase-calculation path. A second Campaign-only modifier system is
  forbidden.
- Campaign Flicker presentation and judgment use
  `StageSession.ElapsedPlayingSeconds`; material readback, View timers, and
  simulator-local phase formulas are forbidden.
- Shield and Booster remain selectable in both stages. Booster is not disabled
  and receives no modifier-specific exception: it activates at `GO` and
  auto-passes gates through the existing shared outcome.
- Minimum Flicker exposure is evaluated against the fastest effective arrival
  speed, including Booster. Target planning must retain later non-bypassed
  teaching opportunities without changing targets based on selected items.
- Goal application, mixed modifiers, new colors, music/BPM/DSP, rhythm score,
  and speed-based switch-interval adjustment remain out of scope.
- Automated tests own determinism and regression. Human portrait play owns
  comprehension, memory difficulty, timing readability, and satisfaction.

## Commercial Flow Foundation 1

Status: Implemented.

- Production initialization is Guest-first and offline. A Guest is created
  once when absent, uses a generated local UUID and injected UTC clock, and is
  restored by Profile ID on later launches. Device identifiers are forbidden.
- One persistent `AppRoot` is the composition root. It owns one explicit
  Clock/Profile/LocalAccount/Settings/Save/Initialization graph. Individual
  service singletons and a mutable global service locator are forbidden.
- `ColorGateRunner.Product` is Unity-independent. Persistent data paths,
  Unity JSON, runtime platform selection, Boot UI, and Scene loading remain in
  Unity adapters/presentation.
- Product save schema 1 owns Profile, Settings, root/Profile SaveRevision, and
  LastWriteUtc. It does not own Stage progression.
- SaveRevision and LastWriteUtc are applied to the candidate snapshot before
  temp serialization and validation. Successful replacement copies exactly
  that validated candidate back to the caller.
- Primary/temp/backup is the local write policy. Atomic replacement is used
  only where supported; WebGL selects and reports backup-based recoverable
  replacement instead of claiming atomic success.
- Corrupt primary may recover from a valid backup. Corrupt primary plus backup
  is a typed blocking error. Unknown future schema blocks without fallback,
  empty replacement, or overwrite. Schema 0 is the only supported migration
  entry in this Iteration.
- `PlayerPrefsStageProgressStore` remains the sole authority for Highest
  Unlocked, Stage clear, Clear Count, Best Time, and Best No-Item Time until a
  separately approved Product Iteration 2. No copy, migration, reset, shadow
  read, or new product-save Stage write is allowed now.
- Boot is Build Index 0. The Boot Builder serializes the first active non-Boot
  Build Settings path before reordering; Boot cannot target itself and runtime
  does not hardcode `SampleScene`.
- Until Frontend Iteration 2, successful Boot initialization enters the
  existing Campaign Scene. Title, Lobby redesign, page routing, login/cloud,
  economy, content, events, analytics, ads, and IAP remain deferred.

## Commercial Flow Foundation Frontend 2

Status: Implemented.

- Player entry is now Boot -> Frontend Title -> placeholder Lobby -> existing
  Campaign Scene. Build Settings contain exactly Boot, Frontend, and Campaign
  in that order, with independent serialized destination paths.
- `FrontendPageRouter` is the sole owner of Page, blocking Modal, and
  Transition state. Primary Page Views neither activate peers nor load Scenes.
- Frontend contains no AppRoot and creates no fallback service graph. Its Scene
  composition boundary reads the Boot-initialized AppRoot once and exposes an
  immutable display Context. Missing AppRoot is a blocking `BOOT REQUIRED`
  state and Campaign remains unavailable.
- Placeholder Lobby does not read Stage PlayerPrefs or display guessed Stage
  numbers/progress. `CONTINUE CAMPAIGN` enters the existing Campaign Lobby,
  which remains the sole UI reader of current campaign progression.
- Empty Currency, Event, Notification, Lobby-theme, and legal actions remain
  hidden. Account linking is reported as unavailable and never as linked.
- Lobby Back returns to Title; Title Back requests exit confirmation; Modal
  consumes Back when cancellable; Back and repeated navigation are ignored
  while transitioning.
- Campaign Scene, `StageSceneController`, Stage data, gameplay, progression,
  and Campaign-to-Frontend navigation are unchanged. The temporary transition
  from the new Lobby to the existing Campaign Lobby is accepted.
- Existing Product, Core, and Package boundaries remain unchanged. The only
  Iteration-specific ProjectSettings change is the Frontend entry in
  `EditorBuildSettings.asset`.

## Campaign Progress Restore Test Isolation Hotfix

Status: Implemented.

- Campaign progression remains device-wide `PlayerPrefs` owned exclusively by
  `PlayerPrefsStageProgressStore`. Product Guest creation does not create,
  reset, copy, migrate, or scope Campaign progress.
- PlayMode tests must never unconditionally delete real Editor Campaign keys.
  Tests that touch progression capture Highest Unlocked and all Stage 1-13
  Records before setup, then restore exact existence and values and call
  `PlayerPrefs.Save()` on success or failure.
- The prior cleanup behavior that deleted Highest Unlocked and Stage 6 Record
  is classified as a test-isolation defect, not authority to change runtime
  load behavior or the Campaign save format.
- Unknown values deleted before the defect was discovered are not guessed,
  synthesized, migrated, or automatically repaired.
- Stage 11 restore, fresh Stage 1 -> 2 restart, new Guest preservation, stable
  Stage ID parity, and missing/corrupt Highest fallback are required PlayMode
  regressions.
- Runtime `StageProgressStore`, `StageSceneController`, PlayerPrefs keys,
  Record format, Product Save Schema, Guest Profile, Stage Catalog, Scenes,
  balance, ProjectSettings, and Packages remain unchanged.
- Profile-scoped progression remains deferred to a separately approved
  `StageProgressService` Iteration. An injected memory store for Campaign tests
  is a future testability option, not part of this Hotfix.

## Account Onboarding, Settings, and Gameplay Pause UX

Status: Implemented / Human Review Pending.

- `AccountChoiceCompleted` is an optional schema-1 `LocalProfileData` field.
  It is not a Settings field, so Settings repair/reset cannot reopen account
  onboarding. Older schema-1 saves intentionally interpret absence as false
  and show Account Choice once.
- `LocalProductSession` is a thin mutation coordinator, not a persistence
  authority. It clones the current Product snapshot, delegates to the existing
  Save service, and publishes/rebinds Profile and Settings only after success.
  A failed write changes no public Profile, Settings, or onboarding state.
- Account Choice cannot enter Lobby until Guest choice persistence succeeds.
  No external provider is integrated; unavailable Google UI is hidden rather
  than pretending success.
- Returning users skip Account Choice and enter the placeholder Lobby. Lobby
  system Back requests exit confirmation and does not return to Title.
- Frontend and Gameplay use the same Settings panel and mutation path. Master,
  Music, SFX, and Vibration persist; only Master has an audio runtime target.
  Vibration false blocks the Booster haptic request itself.
- `GameplayPauseCoordinator` exclusively owns Pause, nested modal, and Scene
  transition state. Pause is allowed during Countdown, Playing, and Shield
  Recovery and freezes attempt-owned time, movement, input, judgment, mechanic
  presentation, camera feedback, and registered effects without `timeScale`.
- Judgment checks the coordinator immediately before resolving, including
  manually queued same-frame paths. Only Builder-registered attempt effects
  are paused; Scene-wide ParticleSystem discovery is forbidden.
- Resume preserves the Attempt. Restart uses the existing Retry-to-PreRun
  path. Confirmed Lobby loads the Builder-serialized Frontend destination and
  records no result; load failure remains paused and recoverable.
- Campaign progression ownership, Product schema version, balance, Packages,
  and `ProjectSettings.asset` remain unchanged.

## Lobby consolidation and unified Settings access

Status: Implemented / Human Review Pending.

- Frontend reads Campaign progression through `IStageProgressReader`; write
  operations remain available only through the existing
  `PlayerPrefsStageProgressStore` authority in Campaign.
- The consolidated Frontend Lobby recommends the lowest uncleared unlocked
  stable Stage ID and starts the existing Campaign PreRun/item-selection flow
  through an AppRoot-owned, non-persistent, one-shot launch request.
- The Campaign Lobby remains intact for direct Scene execution, development,
  Experiment Lab, and missing-context fallback. Production launch consumes
  the request before Campaign Lobby activation, so no intermediate Lobby frame
  is shown.
- Frontend and Pause use one Settings panel composition: Notifications,
  Music, SFX, Vibration, Terms, Privacy, and Support. Master Volume remains
  persisted and applied but is intentionally hidden from this UI.
- Music/SFX OFF writes zero; ON restores the last non-zero value or `1.0`.
  There are still no content-specific audio sources, so these values persist
  without an audible Music/SFX target.
- Notification preference persists with schema-1 default `false`, but no
  notification package, permission, scheduling, or delivery is implemented.
- One `ProductLinkConfiguration` owns external destinations. Only absolute
  HTTPS URLs are accepted; unconfigured prototype actions are disabled and do
  not open a browser.
- The existing Pause coordinator remains authoritative. The Pause button is
  placed inside the Gameplay HUD Safe Area, does not overlap Shield/HUD, and
  does not create a second pause or input system.
- `APP_UI_EDITOR_ONLY` and `SENTIS_ANALYTICS_ENABLED` are package-managed
  volatile WebGL defines. Their environment-dependent presence or ordering is
  allowed but excluded from feature commits; every other ProjectSettings
  semantic difference remains a failure.

## PreRun Back and Settings state-label hotfix

Status: Implemented / Human Review Pending.

- Frontend-origin PreRun is identified only by successful consumption of the
  existing one-shot Campaign launch context. The origin is Scene-local and
  non-persistent; Scene name and UI visibility are not inference inputs.
- Its Back action reuses the serialized Frontend path and existing transition
  loader. A pending load rejects repeated Back input, while failure preserves
  PreRun and restores retry. No launch context keeps the legacy Campaign Lobby
  fallback for direct and development entry.
- All four shared Settings toggles use the same one-label presentation path.
  One `StateLabel` changes between `ON` and `OFF`; Builder output contains no
  legacy state-label pair and no separate Toggle system.

## Automatic Lobby progression foundation

Status: Implemented / balance tunable.

- Product save schema 2 is the single runtime authority for Profile, Settings,
  stable-ID Campaign progress, Economy, and Lobby milestone state.
- Legacy Campaign PlayerPrefs are imported once on production Boot and are not
  deleted. No dual-write or shadow-read path is allowed after successful
  migration.
- One Stage clear mutation owns record, unlock, first-clear reward, and Lobby
  milestone application. Clone-save-publish atomicity and transaction IDs
  prevent partial state and duplicate rewards.
- Lobby progression is automatic and requires no player placement action.
  Eighteen milestones occur at even first-cleared Stages through Stage 36,
  grouped into three themes with six visible upgrades each.
- Starter rewards are 100 Coins per first clear; milestone rewards are 200
  Coins, or 300 at every third milestone, plus deterministic Shield/Booster
  inventory. These values remain explicitly balance-tunable.
- Lobby presentation acknowledgement is separate from reward application.
  Entering Lobby may mark milestones as presented but never reapply rewards.
- Frontend-origin results return to Frontend Lobby. Direct Scene, development,
  and Experiment paths keep their existing destinations.
- Ads, IAP, Hearts, Continue economy, item consumption, final Lobby art, and
  Stage 14-36 content are outside this Iteration.

## Campaign Act 2 Stage 14–20 decisions

Status: Approved and implemented.

- Stage 14–20 form a complete second campaign act using only existing colors,
  patterns, items and modifiers. This advances authored content instead of
  creating another prototype-only mechanic system.
- Stage IDs are `stage-14` through `stage-20`. Display numbers remain
  contiguous and all existing Stage 1–13 definitions and deterministic output
  remain unchanged.
- Stage 14 is a shorter no-modifier recovery stage. Stages 15–20 revisit
  Camouflage, Fog, Ice, Echo, Hidden and Flicker in that order.
- A Stage may enable only its named modifier in this batch. Although the Core
  representation supports flags, same-gate combinations are not approved
  because visibility and provider presentation arbitration is not yet owned.
- Red, Blue and Green remain the full active palette. A fourth color, new tap
  rule, new mechanic tutorial and repeated local item grant are excluded.
- Stage 17 supplies a short pressure spike, Stage 18 an Echo relief beat, and
  Stages 19–20 the memory/attention finale. Difficulty is a wave rather than a
  strictly monotonic gate-count increase.
- Campaign Builder remains the Scene authority and must generate one selection
  entry per Catalog Stage. The Lobby current-stage and title rectangles keep a
  normalized vertical gap so every approved portrait reference resolution
  passes overlap validation.
- This batch does not implement item consumption, Continue pricing, ads, IAP,
  Hearts, Shop, final Lobby art or Stages 21–36.

## Campaign learning-curve redistribution decisions

Status: Approved and implemented.

- Catalog revision 7 keeps the existing 20 contiguous stable Stage IDs while
  redistributing authored content into longer learning blocks. Product-save
  progression and records remain attached to the same stable IDs; no save
  reset, remap or migration is introduced.
- Stages 1-5 remain exact foundation content. Stage 6 provides only its local
  Shield and keeps selection locked; Stage 7 provides only its local Booster
  and keeps selection locked.
- Stage 8 is a clean three-color recovery Stage with selectable Shield and
  Booster. It contains no provided grant or gate modifier and is the first
  ordinary item-application Stage after the two provided-item demonstrations.
- Camouflage occupies Stages 9-11, Fog Stages 12-14, Ice Stages 15-17 and
  Echo Stages 18-20. Each block uses two colors for introduction and practice,
  then three colors for mastery.
- Hidden and Flicker remain implemented and testable Experiment mechanics but
  are not authored in the current 20-Stage Campaign. Their planned Campaign
  blocks are deferred to Hidden Stages 21-23 and Flicker Stages 24-26.
- This redistribution changes Catalog data and Campaign simulation output only.
  It does not change mechanic rules, judgment priority, item effects, Continue,
  Experiment definitions, player profiles or Step 10 simulation inputs.

## Consumable start-item decisions

Status: Approved and implemented.

- Product inventory is authoritative for manually selected start items. From
  Stage 8 onward, pressing `START` consumes exactly one owned Shield for a
  selected Shield and one owned Booster for a selected Booster.
- Shield and Booster are consumed together in one atomic Product mutation.
  Insufficient inventory, invalid state or save failure publishes no partial
  balance, transaction or gameplay change and leaves the player in PreRun.
- A successful consume is idempotent for one Start request. Duplicate input or
  listener invocation cannot charge twice or create a second countdown.
- Retry retains the selected toggles but creates a new Start request, so the
  retained items are consumed again when the player starts the new Attempt.
- Stage 6's provided Shield and Stage 7's provided Booster remain attempt-local
  teaching grants. Selection stays locked and Product inventory is never
  consumed for either Stage.
- Missing Product context never becomes a free selected-item fallback. A
  no-item Start and the Stage 6/7 provided-item paths remain available, while
  an owned-item selection requires an initialized Product session.
- Product save schema, Campaign Catalog, gameplay Core item effects and
  deterministic stage definitions remain unchanged. Continue economy, ads and
  Shop purchasing remain separate future decisions.

## Continue economy and attempt-policy decisions

Status: Approved and implemented.

- A gameplay Attempt permits at most three successful Continues. Core owns the
  authoritative `ContinueUseCount`; Retry creates a fresh Attempt and resets
  that count and all attempt-local Continue policy state.
- Coin Continue prices are 300, 600 and 900 Coins by successful Coin Continue
  ordinal. Rewarded-ad Continues do not advance the Coin price.
- One successfully completed rewarded ad is allowed per Attempt. Spending
  Coins first does not consume that ad right, and using the ad first does not
  change the first Coin price. Failed, cancelled or unavailable ads authorize
  nothing and consume no right.
- Coin spending is a Product-owned clone-save-publish mutation. Transaction
  IDs make a successful spend idempotent; insufficient funds, invalid input or
  save failure publishes neither a balance change nor a transaction ID.
- Continue resumes only after its source has been authorized. A failed Coin
  save or unsuccessful ad result leaves the failed run frozen and retryable.
- Release runtime exposes no fake ad success. When no real rewarded-ad provider
  is installed, the ad action is hidden; test fakes remain test-only.
- Campaign simulation models `AllowContinue` as authorization for up to three
  Continues and reports total, average, maximum and 0/1/2/3-use histogram
  metrics. Shop, IAP, Hearts and a production ad provider remain excluded.

## Iteration 20 — Monetization platform baseline

- Standard mobile billing uses Unity IAP `5.4.2` with Google Play rather than
  a direct-to-customer payment provider.
- Android application identity is `com.wscho.colorgaterunner`. Firebase and
  Google Play app registration must use the same identifier.
- IAP package installation is separated from runtime purchasing. No purchase
  action may appear successful until the Store returns a pending order, the
  order is validated, its idempotent reward is saved and the Store order is
  then confirmed.
- Six Coin amounts and five mixed bundles are approved as catalog intent.
  Product IDs, prices, bundle repeatability and the missing Heart/timed/
  Continue-inventory models remain follow-up decisions.
- Firebase phase one is Functions plus App Check. Analytics and Crashlytics
  are deferred. App Check enforcement begins only after legitimate-client
  metrics are observed.
- Supplied Lobby/Shop/Journey/Collection screenshots define hierarchy only.
  Color Gate Runner will use original neon sci-fi art and will not copy third-
  party characters, icons, branding or a pixel-exact layout.

## Iteration 21 — Local commerce rewards and Hearts

- Save schema 3 owns a 5-Heart cap, 30-minute recharge anchor, rollback-safe
  observed UTC, timed unlimited expiry, Continue Ticket inventory and local
  Starter-granted state. Schema 2 migrates with full Hearts and no wallet or
  progression loss.
- A normal Stage start consumes one Heart atomically with selected start items.
  Retry/replay is a new start; authorization or save failure never begins the
  attempt or publishes a partial spend.
- Six Coin products and five approved bundles use stable local IDs and are all
  consumables. Reward grants use order IDs in the transaction ledger; Starter
  is locally account-limited. Store receipt authority is not claimed here.
- Failure priority is Continue Ticket, available real rewarded ad, then Coin.
  Tickets do not bypass Core's shared three-Continue attempt cap.
- Unity IAP initialization, receipt verification, Store confirmation, Shop UI
  and Firebase remain separate iterations.

## Iteration 22 — Editor Developer Console

- Development cheats live in one Editor-only Window menu, not in release UI.
- Cheat Stage launch bypasses unlock for that launch only; progression remains
  unchanged unless `UNLOCK THROUGH` is explicitly used.
- Campaign reset and Economy reset are separate operations. Campaign reset also
  resets derived Lobby milestone state, while Economy reset includes the local
  commerce transaction ledger so purchases can be retested intentionally.
- Edit Mode mutations target the same local Product save and Play Mode
  mutations require the active AppRoot. All writes reuse clone-save-publish.

## Iteration 23 - Continue and clear economy flow

Status: Approved and implemented.

- An Attempt has no product-imposed Continue count cap. Each successful Coin
  Continue advances only the Coin ordinal through 900, 1,900, 2,900 and 4,900
  Coins; the fourth price repeats for every later Coin Continue. Rewarded-ad
  and Continue Ticket resumes do not advance that ordinal. Retry starts a new
  Attempt and resets it.
- A successfully completed rewarded ad remains limited to once per Attempt.
  Release runtime still hides the action when no real provider is available.
- Continue countdown is already resumed gameplay state: active gate modifiers
  are visible on its first frame, while consumed Shield, Booster, Echo and
  mechanic-grant presentation stays absent for the entire countdown.
- Campaign failure owns a compact wallet readout for Coins and Hearts. An
  insufficient Coin request keeps failure frozen and opens a truthful shortage
  popup; it never spends, resumes or pretends that Shop is available.
- Campaign clear hides Replay. `NEXT STAGE` opens the next unlocked Stage's
  PreRun directly without a Lobby round trip; the final Stage offers Lobby
  only. Experiment Lab retains its separate Replay contract.
- A Heart charged at normal Stage start is refunded atomically when that
  Attempt clears. Unlimited-Heart and free starts cannot create a refund.
  Failure, Retry and abandonment retain the original charge.
- First-clear base Coin rewards are difficulty-owned: Normal 100, Hard 200
  and Very Hard 500. Stages 11, 14 and 17 are Hard; Stage 20 is Very Hard;
  all other current Campaign Stages are Normal. Existing Lobby milestone
  rewards remain additional and separately presented.
- First-clear reward, Heart refund, Stage record and unlock publish in one
  Product transaction. Repeated clear, invalid refund tokens and save failure
  cannot duplicate or partially publish value.
- Production rewarded-ad integration, Shop purchasing, Lobby relayout and the
  Fog/Ice redesign remain separate iterations.

## Iteration 24 — Continue recycles the failed fixed-pool slot

- A successful Continue consumes the failed authored plan exactly once, then
  immediately reuses that presentation slot for the next available plan.
- It must not permanently reduce the six-slot gate pool. This is required by
  the uncapped Continue policy approved in Iteration 23.
- This hotfix does not change Core Continue rules, stage balance, Fog,
  Campaign content, pricing or persistence.

## Iteration 25 — Campaign Fog is a one-shot timed curtain

- Campaign Fog no longer exposes exactly two readable gates. Its modifier
  triggers one presentation curtain when the first Fog gate becomes the next
  judgment in an attempt.
- Curtain distance is `effective speed * 1.5 seconds`. Alpha stays at 1 for
  1.5 seconds and fades linearly to 0 over 0.5 seconds.
- Pause and Continue countdown freeze the timer. Continue preserves and
  repositions the existing curtain; Retry resets the one-shot state.
- Experiment Lab preserves the historical nearest-two comparison so its Step
  10 experiment definition and artifacts are not silently redefined.
- Gate plans, judgment, Campaign balance, Catalog and persistence are unchanged.

## Iteration 26 — Fog visibility duration is authored per Stage

- Campaign Fog uses three explicit phases: `0.5 seconds` fade-in, a Stage-owned
  full-opacity duration, and `0.5 seconds` fade-out.
- Stage 12 / 13 / 14 author `5 / 6 / 7 seconds` at full opacity to increase
  visibility pressure across intro, practice and mastery.
- Stage Catalog owns the values through `FogCurtainSettings`; controller code
  must not infer them from Stage number or difficulty label.
- The Iteration 25 one-shot, adaptive-distance, Pause, Continue, Retry and
  Experiment Lab contracts remain unchanged.

## Iteration 27 — Campaign Ice uses a fixed preplaced runway

- Campaign Stages 15–17 author `IceRunwaySettings.SpeedMultiplier = 2.0`.
  Booster remains authoritative while active; the shared Ice spacing rule,
  deterministic plans and ordinary color judgment remain unchanged.
- One Builder-owned pool of exactly 50 Cyan panels is positioned for all
  authored Ice approach intervals before Countdown. Non-Ice and unused panels
  are inactive, and runtime allocation or growth is forbidden.
- The recycled normal track never changes material for Campaign Ice. Continue
  preserves runway state and Retry deterministically rebuilds it.
- Experiment Lab retains its historical whole-track `1.45` Ice comparison so
  existing diagnostic and Step 10 contracts are not silently rebalanced.
- A MonoBehaviour used as a generated Scene reference must live in a matching
  Unity script asset. The Fog View was separated from its pure state file after
  a clean reload exposed the old file/class reference instability; Fog
  behavior did not change.
