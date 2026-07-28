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
- `MovementSpeed_ContinuesIncreasingAfterTenSeconds`.
- `MaximumSpeed_IsReachedOnlyAtLateStage`.
- `EncounterInterval_DecreasesAcrossProgression`.
- `Speed_IncreasesWithScore`.
- `Speed_NeverExceedsFifteen`.
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

### Shield and pickup

- `Shield_AutomaticScoreGrantIsRemoved`.
- `ShieldPickup_ActivatesAndProtectsExactlyOneMismatch`.
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
- `PatternSequence_EncounterCadenceDecreasesFromEarlyToLate`.
- `PatternSequence_DoesNotRepeatEitherRecentPattern`.
- `PatternSequence_OnlyUsesActiveColors`.
- `ThreeColorSequence_NeverRequiresMoreThanOneTap`.
- `ShieldPickupPlan_IsDeterministicAndAppearsInEarlyRange`.
- `PatternSequence_DiagnosticReportsFirstFortyCadenceValues`.

### Third-color progression

- `ThirdColor_IsInactiveBeforeMilestone`.
- `ThirdColor_IntroducesExactlyOnceAtMilestone`.
- `ThirdColor_TutorialGatesFollowIntroduction`.
- `ThreeColorCycle_OrderIsStable`.
- `Restart_ResetsThirdColorIntroduction`.

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
- Step 7 cadence, progression, and replay presentation:
  `FirstStart_UsesUnifiedThreeSecondCountdown`,
  `Retry_UsesSameUnifiedThreeSecondCountdown`,
  `DeveloperDiagnostics_IsHiddenByDefaultAndReportsPacing`,
  `ShieldPickup_IsVisiblePooledAndDoesNotGrow`,
  `ShieldRecovery_UsesOneSecondInvulnerability`,
  `ThirdColor_IntroducesAtMilestoneWithTutorialGate`,
  `ThirdColor_IsAbsentBeforeMilestone`,
  `Retry_ResetsThirdColorIntroduction`,
  `ResultCard_HasFiveRowsAndStrongScoreHierarchy`,
  `ResultCard_CurrentScorePulsesWithoutPulsingWholeCard`,
  `ResultCard_CurrentRunRowHasDistinctHighlight`,
  `ResultCard_IsContainedWithinPortraitSafeRegion`.
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
- Exactly one fixed `ShieldPickupView`, one diagnostics panel, one result card,
  one result Safe Area, and exactly five rank-row containers are generated.

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
- The unified three-second first-start and Retry countdown feels readable
  without interrupting replay flow.
- Movement speed still feels like it develops after 10 seconds, and encounter
  decisions become perceptibly more frequent through 45 seconds.
- Beat patterns feel intentional rather than random near/far spacing.
- `NEW COLOR`, its two tutorial gates, and the three-color cycle indicator are
  understandable without sustained explanatory text.
- The pooled shield pickup is visible early enough to create anticipation.
- The one-second shield recovery feels sharp rather than low-pressure.
- The central result card makes THIS RUN dominant, Best distinct, the current
  Top 5 row obvious, and Retry easy to reach.
- The developer diagnostics panel is absent during normal player-facing play.

## Step 4 feedback status and deferred work

The Step 3 feedback ideas below are now active Step 4 automated acceptance:

- The correct-gate scale punch and particle burst are visible and finish within 0.35 seconds.
- A wrong gate causes a brief camera shake and visibly desaturates the player.

Still deferred:

- BPM-based or music-synchronized gate placement.
- A fourth color, obstacles, additional power-ups, combos, rating systems,
  missions, and skins.

## Step 8 active acceptance mapping

The Step 7 main-flow checks above are historical. Their deterministic Endless
fixtures remain useful, but the generated SampleScene now uses the following
stage-specific acceptance criteria.

### EditMode automated

- Catalog validity and ordering:
  `StageCatalog_ContainsFiveValidStages`, `StageIds_AreUnique`,
  `StageNumbers_AreSequential`,
  `EveryStage_HasPositiveTargetAndFinalSection`.
- Color onboarding:
  `StagesOneToThree_DoNotAllowGreen`, `StagesFourAndFive_AllowGreen`,
  `StageFour_IntroducesGreenAtConfiguredGate`.
- Deterministic layout and pacing:
  `StageSequence_IsDeterministic`,
  `Booster_IsShorterThanEstimatedStageDistance`,
  `Retry_RetainsSelectedItemsAndReplaysLayout`,
  `NegativeAdvance_IsRejected`,
  `Countdown_DoesNotAdvanceElapsedTime`.
- Start items:
  `NoSelection_InitializesWithoutItems`,
  `ShieldSelection_ActivatesAfterCountdown`,
  `BoosterSelection_ActivatesAfterCountdown`,
  `BoosterBypass_DoesNotConsumeShield`,
  `BoosterEnd_UsesSpeedAtCurrentStageProgress`.
- Goal and terminal state:
  `FinalGate_EntersStageFinishing`, `Goal_ClearsOnlyOnce`,
  `StageFinishing_IgnoresFailureJudgment`.
- Progress storage:
  `Clear_UnlocksOnlyNextStage`, `BetterLowerTime_ReplacesBestTime`,
  `BoosterClear_DoesNotOverwriteNoItemBest`,
  `CorruptPersistence_FallsBackSafely`, `Persistence_RoundTrips`.

### PlayMode automated

- Stage and item flow:
  `StageSelect_ShowsFiveEntries`, `LockedStage_CannotStart`,
  `SelectedStage_OpensItemScreen`,
  `FourItemCombinations_StartCountdown`,
  `Start_CreatesExactlyOneCountdown`,
  `Failure_RetryReturnsToItemSelection`, `Retry_RetainsItemSelection`,
  `NextStage_OpensNextItemSelection`.
- Item activation and Booster boundary:
  `ShieldSelected_ActivatesAfterGo`,
  `ShieldUnselected_RemainsInactive`,
  `BoosterSelected_ActivatesAfterGo`,
  `Booster_BypassesMismatchedGate`,
  `Booster_DoesNotConsumeShield`,
  `Booster_CameraEffectsReturnToNormal`,
  `ItemButtons_AreNotRuntimeConsumables`.
- Stage resolution:
  `FinalGate_RevealsGoal`, `GoalCrossing_ClearsStage`,
  `NoFailureAfterGoalBecomesAvailable`, `Clear_ShowsOneResultPanel`,
  `Failure_StopsMovementImmediately`,
  `FailurePanel_ShowsProgressAndItems`,
  `StageClear_PersistsPerStageRecordAndUnlock`.
- Main-flow removals and onboarding:
  `RuntimeShieldPickup_IsNotExposed`,
  `StageOneToThree_DoNotUnlockGreenByProgress`,
  `StageFour_IntroducesGreen`.
- Reuse and repeatability:
  `GateReaction_ResetsWhenRecycled`,
  `GatePool_ObjectCountDoesNotGrow`,
  `TrackPool_ObjectCountDoesNotGrow`,
  `Restart_ReplaysIdenticalGateLayout`,
  `EveryStage_AllItemCombinationsCanInitialize`.

### Structural validation

- `GrayboxSceneBuilder.BuildGrayboxSceneFromCommandLine` runs the builder
  twice and then checks one generated root, one Stage controller, zero exposed
  legacy controllers, five stage buttons, one item screen, one countdown, one
  HUD, one Goal, one clear panel, one fail panel, one EventSystem, six pooled
  gates, six track segments, zero runtime Shield pickups, assigned references,
  no missing scripts, Portrait orientation, no Volume, and camera
  post-processing disabled.
- PlayMode mirrors the key checks in
  `RequiredSerializedReferences_AreAssigned`,
  `Scene_HasNoMissingMonoBehaviours`, and
  `GeneratedUi_HasNoDuplicateRoots`.

### Manual mobile check

- Confirm all five stage entries and both item toggles are comfortably
  reachable within the physical device safe area.
- Time no-item clears for all five stages and verify the intended 25–40 second
  range; automated tests validate rules but do not claim the pacing is fun.
- Confirm the 3/2/1/GO transition communicates exactly when selected items
  activate.
- Confirm Booster feels substantially faster without making its gate bypass or
  retained Shield ambiguous.
- Confirm Goal arrival, Stage Clear, Next Stage, Retry-to-selection, and
  retained toggles are understandable without developer explanation.
- Confirm Stage 4 Green onboarding is readable and Stages 1–3 never imply a
  score-based color unlock.
- Confirm thirty retries and long pooled-track movement show no stutter,
  duplicates, gaps, or lingering camera/effect state.
- Direct color buttons and optional Endless access remain deferred and require
  later human validation if introduced.
