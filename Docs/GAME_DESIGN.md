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
- Speed follows a deterministic ease-out from 6 toward 12 using growth rate
  0.1, with a modest 0.04 units-per-point contribution.
- Maximum speed: 12 units per second.
- A shield is granted once at score 3. It protects exactly one wrong gate
  without awarding score; a later wrong gate ends the game.
- Correct gates must never end the game.
- Input is ignored after death.

## Gate generation

- Gate colors are generated from a deterministic seed.
- No more than four consecutive gates may use the same color.
- Gate spacing comes from a separate deterministic seeded stream. Its minimum
  is based on current speed, 0.85 seconds of reaction time, and a 0.5-unit
  safety margin; short, medium, and long bands add controlled variation.
- Default test seed: 12345.

## Restart

- Retry resets score, elapsed Playing time, speed, player color, shield, color
  and spacing sequences, feedback, and player position, then starts Playing
  immediately.
- The same test seed produces the same gate colors and layout.

## Out of scope

- Ads
- IAP
- Online ranking
- Cosmetics
- Daily rewards
- Multiple worlds
