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

Status: Active

- Human playtesting found the initial Ready presentation ambiguous, base speed
  4 substantially too slow, fixed six-unit spacing overly mechanical, judgment
  feedback unclear, and the retry screen insufficiently motivating.
- Speed is
  `min(12, 6 + elapsedPlayingSeconds * 0.08 + score * 0.12)`.
- Elapsed gameplay time advances only while Playing. Ready and Dead do not
  advance it, and Retry resets it to zero.
- `GameSession.Advance(deltaSeconds)` is the sole time input to Core. Negative
  delta values are rejected with `ArgumentOutOfRangeException`.
- Gate spacing uses only `5.5`, `6`, `7`, and `8` units. A seed-derived offset
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
- An incorrect judgment stops movement in the resolution frame, marks the gate
  and player with the failure material, shows game over immediately, and runs a
  fixed-position camera shake for 0.22 seconds. Camera rotation never changes,
  and the exact pre-shake position is restored.
- Core owns only the current score. `GameSceneController` owns the current best
  presentation and talks to `IBestScoreStore`; `PlayerPrefsBestScoreStore` is
  the production adapter and tests replace it with memory storage.
- Game over shows `GAME OVER`, current score, best score, and one explicit
  `RETRY` action. Retry returns to Ready and does not also start gameplay.
- BPM-based gate placement and all audio timing are deferred.
- Additional colors, obstacles, power-ups, combos, ratings, missions, and skins
  are deferred.
