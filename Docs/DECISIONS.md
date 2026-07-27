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

Status: Active

- Playtesting found that Step 4 acceleration was technically continuous but
  not perceptually obvious, gate gaps still looked mechanical, immediate game
  over obscured the failed gate, and Retry required an unnecessary second tap.
- Speed is deterministic and ease-out:
  `min(12, 6 + (12 - 6) * (1 - exp(-0.1 * elapsedPlayingSeconds)) + score * 0.04)`.
  Time advances only while Playing; negative deltas remain rejected. Restart
  resets elapsed time and speed.
- A dedicated spacing xorshift32 stream is seeded with
  `normalizedSeed XOR 0x9E3779B9`. Each gap starts at
  `speed * 0.85 + 0.5`, rounds upward to 0.5 units, and adds a deterministic
  short/medium/long band offset of `0`, `1.5`, or `3` units. A band may repeat
  at most twice. Restart rewinds colors and spacing.
- The persistent top HUD is parented under `SafeAreaRoot`. `SafeAreaLayout`
  converts `Screen.safeArea` to normalized anchors and reapplies them only
  when the safe area or resolution changes.
- A lethal mismatch locks Core in Dead immediately, stops movement and input,
  highlights the failed gate, tilts and neutralizes the player, shakes only
  the fixed camera position, and delays the game-over panel by 0.85 seconds.
- Retry performs `Restart` followed by `StartRun`: it resets the complete run
  and presentation and resumes Playing immediately without showing Ready.
- A single non-stacking shield is granted at score 3. It consumes one
  mismatched gate, awards no point, and keeps Playing; the next mismatch is
  lethal. Retry clears it. The HUD shows active and unavailable states.
- Completed positive scores are inserted into a descending local Top 5.
  Duplicates are allowed, only five are retained, missing/corrupt PlayerPrefs
  data falls back to an empty list, and Best remains at least Top 5 rank 1.
  Persistence remains outside Core behind `IScoreHistoryStore`.
- Ready keeps the full-screen tap target and adds a subtle reusable text pulse;
  no imported visual assets are used. Correct gates reuse the existing player
  pulse, score pulse, and bounded ParticleSystem burst.
- Live-changing gate colors, visibility/fog modifiers, currency, paid or
  ad-based Continue, online leaderboards, extra colors, moving/fake gates,
  combos, audio, haptics, and BPM-based placement remain deferred.
