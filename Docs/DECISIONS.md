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
- The exact golden sequence will be recorded only after the implementation and tests establish it.
- The endless run has no finish condition in Milestone 1.
- Existing unused Unity packages remain unchanged during Milestone 1.
- Post-processing will be disabled when the graybox scene is implemented.
