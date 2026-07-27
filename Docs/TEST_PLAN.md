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

- Ready and Red scene initialization:
  `Scene_StartsInReady`, `Scene_StartsWithRedPlayer`.
- First-tap behavior:
  `FirstGameplayTap_StartsRun`,
  `FirstGameplayTap_DoesNotTogglePlayerColor`.
- Playing-tap behavior: `PlayingTap_TogglesPlayerColor`.
- Matching gate behavior:
  `MatchingTrigger_DoesNotEndRun`,
  `MatchingTrigger_IncrementsHudScore`.
- Mismatching gate behavior:
  `MismatchingTrigger_EndsRun`,
  `Death_StopsMovementImmediately`,
  `Death_ShowsGameOverPanel`.
- Complete restart behavior:
  `Restart_ReturnsStateToReady`,
  `Restart_RestoresPlayerPosition`,
  `Restart_RestoresRedColor`,
  `Restart_ResetsHudScore`,
  `Restart_ReplaysGateSequence`.
- Gate reuse behavior:
  `Gate_ResolvesOnlyOncePerActivation`,
  `GatePool_ObjectCountDoesNotGrow`.
- UI input routing:
  `CenterTap_IsAccepted`,
  `RestartClick_DoesNotAlsoTriggerGameplayTap`.
- Camera behavior:
  `CameraRotation_DoesNotChangeDuringPlay`,
  `PlayerAndNextTwoGates_AreInsideCameraViewportAtPortraitAspect`.

## Structural validation

- Unity compilation and non-zero test execution are enforced by
  `Tools/Validate.ps1`.
- Core's `noEngineReferences` assembly definition prevents UnityEngine
  references.
- Required scene references: `RequiredSerializedReferences_AreAssigned`.
- Missing scripts: `Scene_HasNoMissingMonoBehaviours`.
- Single generated root: `Scene_HasSingleGeneratedRoot`.
- Portrait orientation: `Scene_UsesPortraitOrientation`.
- No active post-processing volume:
  `Scene_HasNoActivePostProcessingVolume`.
- Camera post-processing disabled: `Camera_PostProcessingIsDisabled`.
- Fixed pre-created gate pool: `Scene_HasFixedGatePool`.

## Manual mobile check

- Tap works across the full playable screen.
- UI is not clipped by notches or rounded corners.
- Text is readable.
- Restart takes no more than two taps.
- No visible stutter occurs during gate spawning.
- Thirty consecutive restarts do not crash the app.

## Deferred beyond the Step 3 graybox

The following existing acceptance ideas require cosmetic feedback explicitly
excluded from Step 3. They are retained for a later milestone and are not part
of the current graybox acceptance:

- The correct-gate scale punch and particle burst are visible and finish within 0.35 seconds.
- A wrong gate causes a brief camera shake and visibly desaturates the player.
