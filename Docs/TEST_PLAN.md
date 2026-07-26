# Test Plan

## EditMode automated

### Initial and restart values

- Initial player color is Red.
- Restart restores the player color to Red.
- Restart restores speed to 4.

### Color switching

- Red toggles to Blue.
- Blue toggles to Red.
- Input after death does not change color.

### Scoring

- Passing a matching gate adds exactly 1 point.
- Passing a mismatching gate adds no score.
- Score resets to 0 on restart.

### Difficulty

- Speed starts at 4.
- Speed increases after every 5 points.
- Speed never exceeds 9.

### Gate generation

- Seed 12345 always creates the same sequence.
- No sequence contains more than four identical colors in a row.
- Restart with the same seed recreates the sequence.
- Gate spacing is never less than 6 units.

### State transitions

- Ready can transition to Playing.
- Playing can transition to Dead.
- Dead cannot return to Playing without Restart.
- Restart returns the game to Ready.

## PlayMode automated

- Matching player and gate does not trigger game over.
- Mismatching player and gate triggers game over.
- Restart returns the player to the starting position.
- UI score equals gameplay score.
- Game-over panel becomes visible after death.
- Game-over panel becomes visible within 0.5 seconds of death.
- Player movement stops in the same frame that death is resolved.
- Input is accepted on the central screen area.
- At a 9:16 aspect ratio, the player and next two gates are inside the camera viewport.
- Camera rotation does not change during gameplay.

## Structural validation

- Unity compiles without errors.
- Core gameplay code does not reference UnityEngine.
- EditMode and PlayMode test runs each execute at least one test.
- The gameplay scene contains no Missing Script components.
- Required serialized references in the gameplay scene are assigned.
- The Android orientation is Portrait.
- Post-processing is disabled in the Milestone 1 gameplay scene.

## Manual mobile check

- Tap works across the full playable screen.
- UI is not clipped by notches or rounded corners.
- Text is readable.
- Restart takes no more than two taps.
- No visible stutter occurs during gate spawning.
- Thirty consecutive restarts do not crash the app.
- The correct-gate scale punch and particle burst are visible and finish within 0.35 seconds.
- A wrong gate causes a brief camera shake and visibly desaturates the player.
- Game-over UI appears within 0.5 seconds of a wrong gate.
