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
- Initial movement speed: 4 units per second.
- Speed increases by 0.15 after every 5 points.
- Maximum speed: 9 units per second.
- Collision with the wrong gate ends the game.
- Correct gates must never end the game.
- Input is ignored after death.

## Gate generation

- Gate colors are generated from a deterministic seed.
- No more than four consecutive gates may use the same color.
- Minimum distance between gates: 6 units.
- Default test seed: 12345.

## Restart

- Restart resets score, speed, player color, gate sequence and player position.
- The same test seed produces the same gate sequence.

## Out of scope

- Ads
- IAP
- Online ranking
- Cosmetics
- Daily rewards
- Multiple worlds