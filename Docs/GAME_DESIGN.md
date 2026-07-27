# Game Design

## One-line pitch

A one-tap color-switching runner where the player must match the
character color to each approaching gate.

## Target

- Platform: Android first
- Orientation: Portrait
- Session length: 20–60 seconds
- Input: Single tap
- Audience: Casual mobile players

## Core loop

1. Player automatically moves forward.
2. A red or blue gate approaches.
3. Tapping toggles the player's color.
4. Matching the gate awards one point.
5. A mismatch ends the run.
6. Restart must take no more than two taps.

## Player states

- Ready
- Playing
- Dead

## Rules

- Initial player color: Red
- Colors: Red and Blue only
- Each passed gate adds 1 point.
- Initial movement speed: 6 units per second.
- Speed increases continuously by 0.08 per elapsed Playing second and by 0.12
  per point.
- Maximum speed: 12 units per second.
- Collision with the wrong gate ends the game.
- Correct gates must never end the game.
- Input is ignored after death.

## Gate generation

- Gate colors are generated from a deterministic seed.
- No more than four consecutive gates may use the same color.
- Gate spacing uses the deterministic values 5.5, 6, 7, and 8 units.
- Default test seed: 12345.

## Restart

- Restart resets score, elapsed Playing time, speed, player color, color and
  spacing sequences, feedback, and player position.
- The same test seed produces the same gate colors and layout.

## Out of scope

- Ads
- IAP
- Online ranking
- Cosmetics
- Daily rewards
- Multiple worlds
