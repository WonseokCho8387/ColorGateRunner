# Test Plan

## EditMode automated

### Initial and restart values

- Initial player color is Red.
- Restart restores the player color to Red.
- `NewSession_UsesUpdatedBaseSpeed`: speed starts at 6.
- `Restart_ResetsSpeed`: Retry restores speed to 6.
- `Restart_ResetsElapsedTime`: Retry restores elapsed Playing time to zero.

### Color switching

- Red toggles to Blue.
- Blue toggles to Red.
- Input after death does not change color.

### Scoring

- Passing a matching gate adds exactly 1 point.
- Passing a mismatching gate adds no score.
- Score resets to 0 on restart.

### Difficulty

- `Advance_WhilePlaying_IncreasesElapsedTime`.
- `Advance_WhileReady_DoesNotIncreaseElapsedTime`.
- `Advance_WhileDead_DoesNotIncreaseElapsedTime`.
- `Advance_NegativeDelta_IsRejected`.
- `Speed_IncreasesWithElapsedTime`.
- `NonlinearSpeed_HasMeasuredProgression`.
- `NonlinearSpeed_EarlyAccelerationExceedsLateAcceleration`.
- `Speed_IncreasesWithScore`.
- `Speed_NeverExceedsFourteen`.
- `SameAdvanceSequence_ProducesSameSpeed`.
- `SpeedPresentation_StagesIncreaseVisualOutputs`.

### Gate generation

- Seed 12345 always creates the same sequence.
- No sequence contains more than four identical colors in a row.
- Restart with the same seed recreates the sequence.
- `SpacingSequence_AlwaysMeetsReactionTimeMinimum`.
- `SpacingSequence_IsDeterministic`.
- `SpacingSequence_DifferentSeedProducesDifferentSequence`.
- `SpacingSequence_UsesThreeBandsWithoutThreeIdenticalGaps`.
- `Restart_ReplaysColorAndSpacingSequence`.

### Shield

- `Shield_MilestoneActivatesAndProtectsExactlyOneMismatch`.
- `Shield_MatchingWhileActiveDoesNotCreateASecondCharge`.
- `Restart_ResetsShield`.
- `ShieldBreak_EntersRecoveryAndReducesSpeed`.
- `ShieldRecovery_MismatchIsIgnoredWithoutScore`.
- `ShieldRecovery_ExpiresDeterministicallyAndSpeedRecovers`.
- `Restart_ClearsShieldRecovery`.

### Countdown

- `Countdown_DoesNotAdvanceTimeAndCompletesOnce`.

### Authored gate patterns

- `PatternSequence_SameSeedReplaysIdentically`.
- `PatternSequence_DifferentSeedChangesPlans`.
- `PatternSequence_UsesDistinctFairTimingAndNoImmediateRepeat`.
- `PatternSequence_ColorRunNeverExceedsFour`.
- `SameColorBait_ContainsOneReadableException`.
- `PatternSequence_DiagnosticReportsFirstThirtyAndDistribution`.

### State transitions

- Ready can transition to Playing.
- Playing can transition to Dead.
- Dead cannot return to Playing without Restart.
- Restart returns the game to Ready.

## PlayMode automated

- Ready clarity:
  `Ready_ShowsDimmedStartOverlay`,
  `Ready_ShowsTitleAndStartInstruction`,
  `GameplayTap_HidesReadyOverlay`.
- Score HUD:
  `ScoreHud_HasLabelAndFramedPanel`,
  `MatchingTrigger_IncrementsHudScore`,
  `CorrectGate_PulsesScore`.
- Judgment feedback:
  `CorrectGate_TriggersFeedback`,
  `IncorrectGate_TriggersFailureFeedback`,
  `CameraShake_RestoresOriginalPosition`.
- Game-over and best score:
  `GameOver_ShowsCurrentScore`,
  `GameOver_ShowsBestScore`,
  `HigherScore_UpdatesBestScore`,
  `LowerScore_DoesNotReplaceBestScore`.
- Retry:
  `Retry_EntersCountdown`,
  `Retry_ClearsFailureFeedback`,
  `Retry_DoesNotAlsoStartGameplay`.
- Deterministic layout:
  `GateSpacing_RespectsReactionTimeMinimum`,
  `Restart_ReplaysIdenticalGateLayout`,
  `GatePool_ObjectCountDoesNotGrow`.
- Step 5 presentation and persistence:
  `SafeArea_CalculatesNormalizedAnchors`,
  `Shield_ProtectsOneMismatchThenNextMismatchFails`,
  `ScoreHistory_SortsTruncatesAndAllowsDuplicates`,
  `ScoreHistory_CorruptedDataFallsBackToEmpty`,
  `ScoreHistory_RejectsZeroAndLowSixthScore`,
  `GameOver_ShowsCompletedRunInTopScores`,
  `BestScore_IsConsistentWithTopScoreRankOne`.
- Step 6 perceptible feedback and state sequencing:
  `Countdown_DoesNotAdvanceGameplayOrAcceptInput`,
  `Countdown_CompletesOnceIntoPlaying`,
  `Countdown_RepeatedRetryDoesNotOverlap`,
  `SpeedStages_ChangeFovTrailAndSpeedLines`,
  `ShieldAcquisition_ShowsPersistentPlayerAndHudFeedback`,
  `ShieldBreak_ShowsDistinctRecoveryFeedback`,
  `Failure_AnimatesProgressivelyBeforeGameOver`.
- Infinite track and curve experiment:
  `TrackPool_LongRunRecyclesWithoutGapsOrGrowth`,
  `CurveExperiment_IsPresentButDisabledForLongRunSafety`.
- Score hierarchy:
  `ScoreHistory_RankPolicyHandlesBestDuplicateAndRejection`,
  `GameOver_ShowsCompletedRunInTopScores`.

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
- Complete retry behavior:
  `Retry_EntersCountdown`,
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
- Fixed pre-created gate and track pools:
  `Scene_HasFixedGatePool`,
  `TrackPool_LongRunRecyclesWithoutGapsOrGrowth`.
- Unique Step 6 UI and reusable feedback objects:
  `Scene_HasNoDuplicateStep4PresentationObjects`.

## Manual mobile check

- Tap works across the full playable screen.
- Score and shield UI are not clipped by notches or rounded corners on at
  least one Android device or simulator profile.
- Text is readable.
- Retry starts its countdown with one tap.
- No visible stutter occurs during gate spawning.
- Thirty consecutive restarts do not crash the app.
- The 0.9-second animated failure reveal is long enough to identify the wrong gate but
  does not feel sluggish.
- Opening acceleration is perceptible through movement, FOV, trail, speed
  lines, and stage labeling.
- Pattern timing and exception-color gates are visibly distinct rather than
  only numerically different.
- Shield acquisition, persistent state, break, and recovery cannot be confused
  with a correct gate.
- The five-second Retry countdown feels preparatory rather than frustrating.
- The framed Top 5, current row marker, Best badge, and New Best message have a
  clear visual hierarchy.

## Step 4 feedback status and deferred work

The Step 3 feedback ideas below are now active Step 4 automated acceptance:

- The correct-gate scale punch and particle burst are visible and finish within 0.35 seconds.
- A wrong gate causes a brief camera shake and visibly desaturates the player.

Still deferred:

- BPM-based or music-synchronized gate placement.
- Additional colors, obstacles, power-ups, combos, rating systems, missions,
  and skins.
