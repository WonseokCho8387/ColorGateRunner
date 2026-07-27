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

Status: Active

- Step 5 human playtesting found that numerically valid acceleration and gap
  variance were not perceptible, shield state was effectively invisible,
  shield use had no recovery moment, defeat changed too abruptly, immediate
  Retry lacked preparation, Top 5 lacked hierarchy, and the fixed 1000-unit
  floor disappeared during long runs. Same-color runs with one exception gate
  produced the strongest excitement.
- Speed now uses
  `min(14, 6 + (14 - 6) * (1 - exp(-0.14 * elapsed)) + score * 0.02)`.
  Four perceptual stages begin at 0, 5, 12, and 25 seconds. They control
  movement, camera FOV (`60/65/70/74`), trail intensity
  (`0.1/0.35/0.7/1`), speed-line rate (`0/18/40/70`), and an explicit HUD
  stage label.
- Gate generation uses a separate seeded pattern stream with authored
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
- Score 3 still grants one non-stacking shield, now shown by a persistent
  rotating player ring, animated HUD state, centered `SHIELD` message, and
  reusable activation burst.
- Shield absorption enters authoritative `ShieldRecovery` for 2 seconds:
  0.12-second presentation hit-stop, 65% initial speed multiplier that
  recovers smoothly, blinking player, invulnerability, `SHIELD BREAK`
  messaging, distinct burst, and no score from protected mismatches.
- Lethal failure is authoritative immediately but presented over 0.9 seconds.
  The player progressively moves, drops, tilts, shrinks, and neutralizes while
  the camera shake eases and the failed gate stays marked. Game Over appears
  after the readable animation point.
- Retry performs Core Restart, enters a configurable five-second `Countdown`,
  displays `5–1` and `GO`, ignores gameplay input, and does not advance score,
  movement, or elapsed gameplay time. It transitions to Playing exactly once.
- Game Over separates `CURRENT`, a starred `BEST`, `NEW BEST`, and a framed
  `TOP 5`. Positive completed scores remain descending, capped at five, and
  duplicate scores rank after existing equal scores. The current row receives
  a visible marker, `RANK n` label, and a 0.6-second reusable pulse.
- Fog, live-changing gate colors, reduced visibility, currencies, paid or
  ad-based Continue, ads, online leaderboards, additional power-up types,
  extra player colors, audio, haptics, and BPM synchronization remain deferred.
