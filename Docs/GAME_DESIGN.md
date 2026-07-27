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
2. A gate using one of the currently active colors approaches.
3. Tapping cycles the player's color through the active color set.
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
- Colors: Red and Blue at launch; Green unlocks at score 15.
- Each passed gate adds 1 point.
- Initial movement speed: 6 units per second.
- Movement speed follows a deterministic staged progression through
  6, 9, 10.5, 12.5, 14, and 15 units per second at 0, 5, 10, 20, 30, and
  45 seconds, with a 0.01 units-per-point contribution.
- Maximum speed: 15 units per second.
- Encounter cadence progresses independently from movement speed. The base
  interval decreases through 1.65, 1.32, 1.15, 1.0, 0.87, and 0.82 seconds
  at the same time checkpoints. Beat multipliers vary each authored pattern,
  while a 0.75-second minimum reaction time and 0.5-unit safety margin remain.
- A visible pooled shield pickup appears deterministically between early gate
  decisions. It protects exactly one wrong gate, awards no score, and enters
  one second of reduced-speed invulnerability. A wrong gate after recovery
  ends the game.
- Correct gates must never end the game.
- Input is ignored after death.

## Gate generation

- Gate colors and authored beat patterns are generated from a deterministic
  seed.
- No more than four consecutive gates may use the same color.
- Early patterns use two colors and simple steady/compression/release beats.
  Syncopation, burst, bait, and mixed patterns unlock later. Green is
  introduced once at score 15 with a readable announcement and two slower
  tutorial gates before normal three-color generation.
- Default test seed: 12345.

## Infinite track

- Six pre-created 40-unit straight segments recycle behind the player.
- Adjacent end/start anchors remain coincident.
- A disabled 16-degree curve experiment is retained separately until safe
  path following is implemented.

## Restart

- The first title tap and Retry both enter the same configurable three-second
  `3, 2, 1, GO` countdown. Retry resets score, elapsed Playing time, movement
  and cadence progression, active color count, player color, shield pickup,
  patterns, feedback, player position, and track.
- The same test seed produces the same gate colors and layout.

## Out of scope

- Ads
- IAP
- Online ranking
- Cosmetics
- Daily rewards
- Multiple worlds
