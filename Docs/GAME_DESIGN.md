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
- Countdown
- Playing
- ShieldRecovery
- Dead

## Rules

- Initial player color: Red
- Colors: Red and Blue only
- Each passed gate adds 1 point.
- Initial movement speed: 6 units per second.
- Speed follows a deterministic ease-out from 6 toward 14 using growth rate
  0.14, with a modest 0.02 units-per-point contribution.
- Maximum speed: 14 units per second.
- A shield is granted once at score 3. It protects exactly one wrong gate,
  awards no score, and enters two seconds of reduced-speed invulnerability.
  A wrong gate after recovery ends the game.
- Correct gates must never end the game.
- Input is ignored after death.

## Gate generation

- Gate colors are generated from a deterministic seed.
- No more than four consecutive gates may use the same color.
- Gate spacing and color relationships come from deterministic authored
  patterns. Time-to-gate values range from 0.9 to 1.9 seconds and convert to
  distance using current speed plus a 0.5-unit safety margin.
- Default test seed: 12345.

## Infinite track

- Six pre-created 40-unit straight segments recycle behind the player.
- Adjacent end/start anchors remain coincident.
- A disabled 16-degree curve experiment is retained separately until safe
  path following is implemented.

## Restart

- Retry resets score, elapsed Playing time, speed, player color, shield,
  patterns, feedback, player position, and track, then runs a configurable
  five-second countdown before Playing.
- The same test seed produces the same gate colors and layout.

## Out of scope

- Ads
- IAP
- Online ranking
- Cosmetics
- Daily rewards
- Multiple worlds
