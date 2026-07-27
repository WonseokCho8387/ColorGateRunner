# Decisions

## Milestone 1

Status: Accepted

- A tap while Ready starts the run without toggling the initial Red color.
- Taps while Playing toggle the player color between Red and Blue.
- Gameplay input is ignored while Dead, but the restart button remains active.
- A gate resolves when the player crosses its one-shot trigger plane.
- Speed updates immediately after reaching scores 5, 10, 15, and so on.
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
- Gates use fixed six-unit spacing and a five-object pre-created pool.
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
