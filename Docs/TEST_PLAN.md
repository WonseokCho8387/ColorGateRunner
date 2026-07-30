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

## Step 9A mandatory simulation methodology

### Profiles

- Perfect: reaction `0`, variance `0`, no error.
- Expert: reaction `0.24s`, variance `0.05`, miss `0.004`, wrong `0.003`,
  density `0.01`, Green `0.005`, fatigue `0.002`.
- Average: reaction `0.42s`, variance `0.12`, miss `0.018`, wrong `0.012`,
  density `0.035`, Green `0.025`, fatigue `0.008`.
- Novice: reaction `0.62s`, variance `0.20`, miss `0.045`, wrong `0.03`,
  density `0.07`, Green `0.06`, fatigue `0.015`.
- Stress: reaction `0.46s`, variance `0.24`, miss `0.02`, wrong `0.018`,
  density `0.08`, Green `0.04`, fatigue `0.025`.

All values are provisional and live separately from stage balance. They do
not represent real players until local human telemetry calibrates them.

### Matrix and reproducibility

- Every Stage 1–5 × None/Shield/Booster/Both combination runs once with
  Perfect and 1,000 seeded runs for each other profile.
- Every stochastic case includes first-attempt and one-Continue outcomes.
- The simulator must use `StageSession`, `StageCatalog`,
  `DeterministicStageGateSequence`, item rules, Continue rules, and Goal
  completion rather than a second approximate gameplay implementation.
- Same simulation seed must reproduce results.

### Metrics and reports

Record run counts, first/Continue clear rates, P10/median/P90 completion time,
median failure progress, failure by gate/pattern, final reach/completion,
required/successful taps, missed/wrong inputs, average/peak input rate,
minimum/P10/average reaction margin, Shield consumption/survival, Booster
bypass count/percentage/primary-pattern percentage, first-three post-Booster
failure rate, Continue success, and estimated no-item duration.

Write untracked artifacts to `Artifacts/Simulation/`:

- `Step9A-SimulationSummary.md`
- `Step9A-SimulationResults.json`
- `Step9A-SimulationResults.csv`

The Markdown comparison includes duration, relative difficulty, profiles,
items, Continue, failure hotspots, Stage 2/3 before-after notes, Booster exit,
the provisional-model warning, and human-only criteria.

### Relative difficulty and adjustment boundary

Report whether the provisional model orders Stage 1 easiest, Stage 2 with
greater rhythm pressure, Stage 3 with more attention failures, Stage 4 with a
Green increase, and Stage 5 hardest. Contradictions trigger model inspection,
not forced tuning.

Only Stage 2 section length/cadence within reaction limits, Stage 3 pattern/run
values, Booster distance in its safe range, exit spacing, and result delay in
the documented range may be tuned automatically. Tap input, item purpose,
Lobby, failure rules, currency, hidden Assist, monetization, colors, and major
mechanics require human direction.

Telemetry is local CSV, development-only, disabled by default, identifier-free,
and never uploaded. Track stage/seed/items/Continue/time, gate/pattern/colors,
taps, margin/outcome, Shield/Booster, failure, completion, and final outcome.
Assist eligibility may track failures/progress/pattern/Continue, but no
automatic or hidden Assist activation is allowed.

Automated validation must not claim fun, excitement, visual satisfaction,
emotional achievement, replay motivation, or perceived fairness.

## Step 9A.1 continuity hotfix acceptance mapping

### EditMode automated

- Doubled movement scale:
  `StageCatalog_UsesIteration2DoubleSpeedScale`.
- Booster continuity:
  `BoosterExit_PreservesSequenceCursor`,
  `BoosterExit_PreservesGateIndices`,
  `BoosterExit_OverridesColorWithoutChangingPlannedPosition`,
  `BoosterExit_FirstGateMatchesCurrentColor`,
  `BoosterExit_SecondGateRequiresAtMostOneTap`,
  `BoosterExit_ThirdGateReturnsToAuthoredSequence`.
- Continue state continuity:
  `Continue_PreservesStateAndConsumesFailedGate`,
  `Continue_PreservesElapsedTime`,
  `Continue_AdvancesProgressPastConsumedFailedGate`,
  `Continue_PreservesCurrentSpeed`,
  `Continue_PreservesCurrentColor`,
  `Continue_PreservesPendingSequenceCursor`,
  `Continue_FailedGateIsResolvedExactlyOnce`,
  `Continue_DoesNotRestoreShield`,
  `Continue_DoesNotRestartBooster`,
  `Continue_FinalSectionStateSurvives`.
- Simulation continuity:
  `Simulation_ContinuityMetricsReportNoReset` requires zero active-gate
  displacement, cursor resets, unexpected gate-index gaps/duplicates, and
  full-pool resets.

### PlayMode automated

- Booster live-stream integration:
  `BoosterExit_ActiveGatePositionsDoNotJump`,
  `BoosterExit_DoesNotIntroduceEmptyGateInterval`,
  `BoosterExit_FirstGateUsesCurrentPlayerColor`,
  `BoosterExit_OriginalPatternResumesAfterShortOverride`.
- Continue live-scene integration:
  `Continue_FreezesTrackDuringCleanRespawn`,
  `Continue_CountdownUsesSameGateLayout`,
  `Continue_UnaffectedGateTransformsRemainUnchanged`,
  `Continue_FailedGateCannotImmediatelyFailAgain`,
  `Continue_ResumesSameTimerProgressSpeedColorAndCursor`,
  `Continue_ResumesSameActiveSequence`,
  `Continue_ClearsCameraShakeAndDoesNotShowItemSelection`,
  `Continue_DoesNotRestoreShieldOrBooster`,
  `Continue_GateAndTrackPoolsDoNotResetOrGrow`.
- Repetition:
  `ThirtyRepeatedBoosterContinueTransitions_HaveNoTransformDrift` checks the
  fixed gate and track counts plus gate root rotation/scale after 30 complete
  transition cycles.

### Structural validation

- The existing idempotent scene builder must still report one generated root,
  one Stage controller, six gates, six track segments, one EventSystem,
  assigned references, no missing scripts, Portrait orientation, no active
  post-processing, and no package or unrelated ProjectSettings change.
- Both test result XML files must contain nonzero executed tests. Simulation
  JSON/CSV/Markdown must expose Booster/Continue distance, displacement,
  index, cursor, pool-reset, and post-transition failure metrics.

### Manual mobile check

- Confirm Booster effects ease out with no visible world freeze, gate jump, or
  empty interval.
- Confirm Continue visually resumes the exact failed location under the
  countdown overlay and that the failed gate cannot immediately fail again.
- Confirm the doubled world speed reads as intended on a portrait device;
  automated cadence preservation does not establish comfort or fairness.
- Confirm the first three gates after Booster and Continue are readable, the
  camera return is continuous, and no Shield/Booster presentation reappears.

## Iteration 2 campaign speed-scale acceptance mapping

### Automated

- `StageCatalog_UsesIteration2DoubleSpeedScale` verifies normal campaign
  speeds `28/40`, `32/44`, `34/46`, `32/48`, and `36/54`, Booster speeds
  `88`, `92`, `96`, `100`, and `104`, and Booster distances `320`, `360`,
  `400`, `420`, and `460`.
- Goal planning and Continue integration verify the 48-unit campaign initial
  lead and 40-unit Goal distance.
- `BoosterEnd_UsesSpeedAtCurrentStageProgress` advances through authored gate
  spacing and verifies the 0.35-second return to the current normal curve.
- EditMode and PlayMode suites must pass after the idempotent Scene Builder
  completes its two-build structural validation.
- Rerun all five campaign stages across Perfect, Expert, Average, Novice, and
  Stress profiles, all four start-item combinations, and deterministic plus
  1,000-run stochastic modes.
- Compare the immediately preceding baseline with the final campaign report.
  Non-spatial gameplay metrics must be identical; next-gate distance telemetry
  must scale by exactly 2.
- Rerun the Step 10 experiment matrix. All five report hashes must remain
  unchanged because Experiment Lab is outside this adjustment.

### Manual mobile check

- Confirm the campaign reads as materially faster on a portrait device.
- Confirm normal gates, Booster exit, Continue recovery, and Goal approach
  remain readable and comfortable.
- Record human feedback before changing cadence or presentation. Automated
  timing continuity does not establish perceived speed, comfort, fairness, or
  fun.

## Step 9C mobile UI readability acceptance mapping

### EditMode automated

- Color tile policy: `TwoColorHud_UsesStageOrder`,
  `ThreeColorHud_UsesStageOrderAfterIntroduction`,
  `StageFour_ShowsCompleteColorCycleFromStart`,
  `CurrentColor_RemainsAnActiveTile`, and `NextColor_FollowsActiveCycle`.
- Presentation policy: `PrimaryFlow_ActivatesExactlyOneRoot`,
  `Countdown_UsesGameplayHudWithCountdownOverlay`,
  `BoosterMeter_OnlyAppearsDuringActiveGameplay`,
  `ItemIcon_OnlyAppearsWhenRelevant`, and
  `ColorSymbolMapping_IsStable`.
- Existing progression mapping remains covered by
  `LobbyTierMapping_UsesExistingProgressionThresholds`.

### PlayMode automated

- Root and Lobby separation: `Lobby_ContainsNoGameplayHudElements`,
  `Lobby_ContainsNoBaseOrUpgradeButtons`,
  `Lobby_HasOneDominantPlayButton`, `Gameplay_HidesLobbyRoot`, and
  `Gameplay_ShowsOneColorHud`.
- Color HUD integration: `CurrentColorHud_UpdatesAfterTap`,
  `TwoColorStage_ShowsExactlyTwoColorTiles`, and
  `ThreeColorStage_ShowsExactlyThreeColorTiles`.
- Conditional item presentation:
  `BoosterBar_IsHiddenBeforeBooster`,
  `BoosterBar_IsVisibleOnlyDuringBooster`,
  `BlueHorizontalCenterBar_NoLongerExists`,
  `NormalGameplay_HasNoCentralSpeedLineObstruction`,
  `BoosterEffects_ClearAfterEnding`,
  `Continue_RestoresCorrectHudState`, and
  `Retry_RestoresCorrectHudState`.
- Layout and reuse: `ReferenceResolutions_PassOverlapBounds`,
  `PlayerUi_RemainsInsideSimulatedNotchSafeArea`, and
  `SceneBuilderRerun_CreatesNoDuplicateRoots`.
- `CaptureStep9CReferenceScreenshots` is opt-in visual evidence and executes
  only when `COLOR_GATE_CAPTURE_VISUALS=1`; normal automated validation does
  not claim pixel quality.

### Structural validation

- The builder runs twice, validates all seven unique flow roots, three unique
  color tiles and markers, one top Booster meter, one Shield icon, one
  EventSystem, six gates with left/right/top geometry and color symbols, six
  track segments, assigned references, no missing scripts, Portrait
  orientation, and no active post-processing.
- Obsolete `CurrentColorText`, `ColorCycleOrderText`, `NextColorText`, and
  Lobby tier-banner objects must be absent from the generated scene.

### Manual mobile check

- At each reference resolution and on one notched physical or simulated
  device, verify text is not clipped, Play is dominant, Lobby and gameplay
  HUD never leak into each other, and the upper-left color tile panel is
  readable with one glance.
- Verify the top gate crossbar and redundant symbols remain readable against
  track, Shield, and Booster effects at real device brightness.
- Verify Booster effects read as motion at screen edges without resembling UI
  or entering the center recognition corridor, and that no particles remain
  after exit, Retry, or Continue.
- Human review must still judge hierarchy, contrast, comfort, and visual
  quality; screenshots and bounds tests cannot establish those qualities.

## Step 10 experiment-lab acceptance mapping

### EditMode automated

- Stable identity and cycling:
  `SixColorIdentityAndSymbolMapping_IsStable`,
  `ColorCycle_IsCorrectForTwoThroughSixColors`,
  `VerticalStack_OrderStartsWithCurrentColor`, and
  `RequiredTapCount_UsesForwardCycle`.
- Determinism:
  `ExperimentDefinitions_RemainDeterministic`,
  `ExperimentRestart_ReplaysPlansAndColor`, and
  `ShortlistGeneration_IsDeterministic`.
- Mechanics:
  `Camouflage_RevealsAfterOneInterveningGate`,
  `Fog_ExposesExactlyTwoNearestUnpassedGates`,
  `Fog_DoesNotChangeGatePlans`,
  `Ice_ChangesSpeedButPreservesColorJudgment`, and
  `Ice_ExitRestoresCorrectNormalSpeed`.
- Repeated-tap model:
  `RepeatedTapPlayerProfiles_AreValid`,
  `TapWindowMetrics_HandleZeroAndRepeatedTaps`, and
  `RiskClassification_AppliesDocumentedThresholds`,
  `ConditionRisk_CombinesAverageAndExpertHardExclusions`, and
  `SixColorBoundary_IsHighRiskAndBoundaryOnly`.

Each non-Perfect profile explicitly owns first-tap mean/variance, repeated-tap
mean/variance, comfortable burst, burst error growth, confirmation time, and
feedback dependency. Perfect performs the exact required tap count.

### Simulation matrix

- Conditions are color counts `3/4/5/6` crossed with
  `None/Camouflage/Fog/Ice`.
- First pass uses no items: one Perfect run and 1,000 deterministic runs for
  Expert, Average, Novice, and Stress per condition: 64,016 total.
- Mechanic isolation pairs the same seed, planned colors, required tap counts,
  gate count, cadence, and section order.
- Report run count, completion, median time, 0/1/2/3/4+ tap distribution,
  average/min/P10 required interval, peak taps/s, longest burst, failure
  categories, final reach, and mechanic-specific margins.
- Risk flags are: P10 interval `<0.14s`; 3+ tap gates `>20%`; Average
  completion `<20%`; burst failures `>50%` of failures; Expert completion
  `<50%`. Average completion below 20% or Expert completion below 50% is a
  hard exclusion and always classifies the condition as High Risk. Otherwise
  one caution flag is Caution and two or more are High Risk. A Boundary Only
  diagnostic remains High Risk and is explicitly ineligible as a normal-stage
  candidate.
- Shortlist selection is deterministic and yields one informative condition
  from each mechanic family for human review. It does not declare a winner.
- Only the five shortlisted conditions run Shield, Booster, and Both across
  the same five profiles (60,015 additional model runs). Item protection uses
  the same `ExperimentItemRules` as the development runtime.
- One allowed tuning iteration introduced explicit 4+ tap pressure gates only
  in the final section for five- and six-color conditions. No input direction,
  normal-stage rule, or fixed mechanic rule was tuned.

### PlayMode automated

- Phase A:
  `Lobby_VerticalDecorationsAreAbsent`,
  `VerticalStack_CurrentColorStaysAtTop`,
  `VerticalStack_RapidTapsRetargetToAuthoritativeColor`,
  `VerticalStack_HasSixReusableSlots`,
  `BoosterCamera_LowersAndMovesCloserBehindPlayer`,
  `Booster_CameraReturnsToExactBaseline`,
  `ResetProgress_CancellationChangesNothing`, and
  `ResetProgress_ConfirmationReturnsLobbyToStageOne`.
- Development integration:
  `ExperimentLauncher_IsHiddenFromNormalFlow`,
  `ExperimentPlay_DoesNotChangeNormalProgression`,
  `Camouflage_RemainsNeutralThenRevealsWithoutMoving`,
  `Fog_KeepsExactlyTwoGatesFullyReadableWithoutPoolGrowth`,
  `Ice_ChangesFloorPresentationAndKeepsColorJudgment`, and
  `RepeatedExperimentStarts_CreateNoDuplicates`.

### Structural and visual validation

- The builder runs twice and requires one Experiment launcher, six fixed color
  tiles, six generated color materials, one Reset confirmation, one gate pool,
  and one track pool. Lobby accent-block names must be absent.
- `CaptureStep10ReferenceScreenshots` is opt-in with
  `COLOR_GATE_CAPTURE_STEP10=1` and writes the 16 requested captures under
  `Artifacts/VisualValidation/Step10/`.

### Manual evaluation

- At 1080x1920, 1170x2532, 1080x2400, 1440x3200, a notched Safe Area, and at
  least one physical phone, evaluate six-tile readability, rapid-tap comfort,
  Booster gate readability, Camouflage comprehension, Fog planning horizon,
  and Ice entry/exit continuity.
- Human review must decide whether any candidate is understandable, visually
  satisfying, comfortable, fair, fun, or replayable. Automated evidence must
  not make those claims.

## Step 10.1 human-playtest fixes acceptance mapping

### EditMode automated

- Stage 4 activation and tutorial:
  `Stage4_ActiveColorsAreCompleteAtRunStart`,
  `Stage4_GreenGatesBeginAtConfiguredLaterIndex`,
  `Stage4_ColorCycleDoesNotMutateMidRun`, and
  `Stage4_EarlyTutorialPrimarilyRequiresZeroOrOneTap`.
- Finish planning:
  `Goal_IsPartOfDeterministicStagePlanning` verifies that a replay of the same
  stage definition produces the same finish distance after all planned gates.
- Continue semantics:
  `Continue_ConsumesFailedGateImmediately` and
  `Continue_SelectsGateAfterConsumedFailure` require one consumption, the next
  global gate index, and the existing two-gate safe-color policy.
- Experiment selections:
  `ExperimentLauncherOptions_MapToExistingDefinitions` covers 4-color
  None/Camouflage/Fog/Ice, 5-color None, and 6-color None against the existing
  immutable experiment catalog.
- Existing continuity simulation remains required:
  `Simulation_ContinuityMetricsReportNoReset` treats the single failed-gate
  increment as intended consumption, while still requiring zero sequence
  reset, duplicate index, pool reset, or unaffected-transform displacement.

### PlayMode automated

- Stage 4 scene integration:
  `Stage4_HudShowsThreeColorsImmediately`,
  `Stage4_EarlyGatesDoNotUseGreen`, and
  `Stage4_LaterGateUsesGreen`.
- Finish continuity:
  `Goal_IsVisibleAtLongRangeAndApproachesContinuously` and
  `Goal_DoesNotPopOrTeleportAtFinalGate`.
- Continue presentation:
  `Continue_ResetsPlayerToCleanUprightPose` and
  `Continue_ConsumesFailedGateAndLeavesReadableNextGate`, plus the retained
  Step 9A.1 frozen-track, sequence, item, and fixed-pool tests.
- Tester access:
  `ExperimentLab_OpensFromLobbyInDevelopmentFlow`,
  `ExperimentLab_ShortlistedConditionsLaunchWithoutProgression`, and
  `ExperimentLab_IsHiddenForReleasePolicy`.

### Structural validation

- Run the idempotent scene builder twice. Require one `ExperimentLabButton`,
  one existing `ExperimentLauncherPanel`, one Goal root with exactly one left
  post, right post, crossbar, FINISH banner, and floor line, one fixed six-gate
  pool, one fixed six-segment track pool, assigned controller references, and
  no missing scripts.
- Both test result XML files must execute nonzero tests. Run the Stage 1–5
  deterministic/stochastic simulation and verify the Step 10 experiment report
  hashes and source data remain unchanged.

### Manual mobile check

- Confirm all three Stage 4 tiles are legible before `GO`, the delayed Green
  gate is understood without a perceived control change, and the early
  zero/one-tap teaching rhythm is readable.
- Confirm the finish structure becomes recognizable as it enters the 500-unit
  camera range and never visibly pops or shifts during Booster, Continue, or
  final-gate resolution.
- Confirm Continue immediately looks like a clean respawn, the failed gate is
  absent ahead, the next gate is comfortably readable, and the fixed camera
  rotation remains stable.
- In the Editor or a Development Build, open Lobby > `EXPERIMENT LAB`, launch
  every shortlisted condition without Inspector edits, verify the active label,
  leave to the Lobby, and confirm the button is absent from a release build.

## Human Playtest

Fun

1~5

Difficulty

1~5

Replay

1~5

Comments

## Iteration 1 Experiment Runtime Flow acceptance mapping

### EditMode automated

- Shared Stage 4 color cycle:
  `Stage4_PerfectFirstAttemptUsesRuntimeColorCycle` and
  `Stage4_SharedTapCalculationMatchesRuntimeCycle`.
- Experiment Core flow:
  `ExperimentCountdown_BlocksInputProgressAndJudgment`,
  `ExperimentCountdown_CompletesOnceIntoPlaying`,
  `ExperimentMismatch_EntersFailedOnce`, and
  `ExperimentFinalGate_EntersCompletedOnce`.
- Deterministic replay:
  `ExperimentRestart_PreservesConditionAndResetsRuntime`.

### PlayMode automated

- Countdown and Playing integration:
  `ExperimentRuntime_CountdownBlocksProgressThenStartsPlaying`.
- Failed result and deterministic Retry:
  `ExperimentRuntime_FailureShowsResultAndRetryReplaysCondition`.
- Completed result and deterministic Replay:
  `ExperimentRuntime_CompletionShowsResultAndReplayRestarts`.
- Campaign isolation and Lab return:
  `ExperimentRuntime_BackToLabDoesNotReadOrWriteProgress`.
- Reentry and fixed object graph:
  `ExperimentRuntime_ReentryKeepsSingleRuntimeObjectGraph`.
- The launcher integration check is now
  `ExperimentLauncher_StartsCountdownWithFixedPoolSession`.

### Regression evidence

- EditMode: 217 executed, 217 passed.
- PlayMode: 111 executed, 111 passed.
- The idempotent Scene Builder completed its two-build structural validation
  with no missing script or missing reference failure.
- Stage 1–5 deterministic/stochastic simulation was rerun for every documented
  profile and item combination. Only nine Stage 4 rows changed; Stage 1, 2, 3,
  and 5 rows were identical.
- Stage 4 Perfect/None changed from first-attempt clear `0%` with one gate-5
  failure to `100%` with zero failures. Average/None changed from `0.5%` to
  `32.9%` first-attempt clear after removal of the same invalid forced mismatch.
- All five Step 10 experiment report SHA-256 hashes remained unchanged.

### Manual evaluation

- Confirm countdown preparation and GO timing are readable.
- Confirm failure cause, Retry Same Test, Replay, and Back to Lab are
  immediately understandable.
- Confirm success and failure results are visually distinct and Retry does not
  feel unnecessarily slow.
- Automated evidence does not establish fun, comfort, fairness, satisfaction,
  or replay motivation.

## Iteration 3 Clone-only Experiment acceptance mapping

### EditMode automated

- `CloneSettings_UsesApprovedSourcesAndGap` verifies one-based Source indices
  `[3, 6]` and `GapSeconds = 0.45`.
- `CloneSettings_RejectsDefinitionsWithTooFewSources` requires a clear error
  for fewer than six non-Clone gates and no replacement Source.
- `CloneSequence_SameSeedReplaysIdentically` and
  `CloneSequence_SourceRelationshipsReplayIdentically` cover identity,
  placement, color, role, generation order, and Source relationship.
- `CloneSequence_InsertsExactlyTwoClonesAfterSourcesThreeAndSix` requires
  exactly one Clone after each approved Source, no intervening gate, no
  chains, and no Clone-owned Clone.
- `CloneSequence_UsesSourceColorAndConfiguredGap` requires the Source color
  and base normal speed multiplied by `0.45` seconds.
- `CloneSession_SourceAndCloneAreIndependentJudgments`,
  `CloneSession_CloneSuccessCountsTowardCompletion`, and
  `CloneSession_CloneMismatchEntersFailedWithDistinctCause` cover the shared
  Playing/Failed flow and one-shot judgment.
- `CloneSession_ShieldConsumesOnceAndPassesClone` requires the shared Shield
  path and continued Playing.
- `CloneCamouflage_UsesOrdinaryHideRevealAndJudgment` requires ordinary
  hiding/reveal, Source color after reveal, required judgment, and normal
  mismatch/Shield behavior.
- `CloneRestart_PreservesLayoutAndResetsRuntimeState` covers seed, placement,
  judgment, Shield, and visibility reset.

### PlayMode automated

- Launcher starts a Clone-only condition with Booster disabled.
- Countdown blocks Source and Clone judgment.
- Source success is followed by a separate Clone judgment.
- Clone View uses the Source color and distinguishable lower-intensity echo
  presentation without increasing the fixed gate or track pools.
- Clone failure shows the existing result flow and a distinct `CLONE MISS`
  cause; Retry Same Test reproduces placement and clears runtime state.
- Active Shield protects one Clone mismatch and updates the existing Shield
  presentation.
- Camouflage Clone hides/reveals exactly like an ordinary Camouflage gate,
  remains judgment-required, and resets on Retry/Replay.
- Replay, Back to Lab, repeated Lab entry, listener count, two-build Scene
  Builder stability, missing scripts, and missing references remain covered.

### Regression and simulation

- Rerun all campaign Stage 1–5 profile/item combinations and require exact
  equality with the immediately preceding baseline.
- Rerun the existing 16-condition Step 10 matrix and require all existing
  report artifacts to remain byte-identical.
- Run deterministic Clone-only validation twice with the same seed and after
  Retry/Replay; compare all generated identities, relationships, colors,
  positions, and outcomes.
- Automated results do not establish Clone readability, spacing comfort,
  tension, fairness, or fun.

### Manual mobile check

- Confirm Clone reads as the Source's real, hazardous echo rather than
  decoration or Shield.
- Confirm the Source/Clone relationship, judgment opening, and `0.45s`
  initial gap remain readable at portrait mobile scale.
- Confirm `CLONE MISS`, Shield protection, Camouflage reveal, Retry identity,
  and overall screen complexity are understandable.

### Regression evidence

- EditMode: 231 executed, 231 passed.
- PlayMode: 117 executed, 117 passed.
- The idempotent Scene Builder completed its two-build structural validation
  with no missing script or missing reference failure.
- Campaign simulation Markdown, JSON, and CSV SHA-256 hashes matched the
  immediately preceding Clone baseline exactly.
- All five existing Step 10 report SHA-256 hashes matched the immediately
  preceding Clone baseline exactly.
- Clone-only Perfect simulation produced the same completed result, time, and
  tap metrics when repeated with the same seed.
- ProjectSettings and package manifest/lock hashes remained unchanged.

## Iteration 4 Echo and campaign-expansion acceptance mapping

### Phase gates

- Architecture: one `StageCatalogAsset`, pure Adapter output, `IStageCatalog`
  runtime/simulation parity, deterministic curve parity, and exact Stage 1–5
  simulation/hash regression.
- Modifier foundation: shared None/Camouflage/Fog/Ice/Echo Provider metadata,
  stable Gate ID role locking, one pending offer across the active pool, and
  catalog-driven Stage UI without duplicate listeners.
- Echo: one held color, deterministic first/additional offers, cooldown and
  acquisition caps, unchanged gate count, effective-color acquisition, and
  Player → Echo → Shield → Fail priority.
- ETA: speed-aware reveal under normal, Booster, and Ice states; monotonic
  reveal; Retry reset; no hidden color leak.
- Speed profile: serialized Start/Max/Curve, identical Core evaluation at
  0/25/50/75/100%, modifier ordering, and unchanged Stage 1–5 results.
- Campaign: Stage 5→6 progression, Stage 11 completion, stage-local Shield and
  Booster grants, Primary Mechanic occurrence, Continue/Retry/Goal, and
  catalog ID-based UI/persistence.

### Simulation and regression

- Run every Stage 1–11 across Perfect, Expert, Average, Novice, and Stress
  with documented item/grant combinations.
- Perfect must clear every stage with exact input. Investigate adjacent
  non-Perfect first-attempt increases of 5 percentage points or more rather
  than forcing monotonicity.
- Preserve Stage 1–5 Markdown/JSON/CSV baseline hashes and all five Step 10
  hashes. Stage 6–11 reports use new artifact names.
- Builder runs twice; require no missing scripts/references or duplicate
  EventSystem, controller, stage button, listener, gate view, track, or player
  Echo shell.

### Manual

- Human review owns Echo frequency and understanding, Echo/Shield visual
  separation, ETA lead comfort, curve feel, stage-local grant comprehension,
  mobile readability, difficulty progression, and Stage 11 finale quality.

### Iteration 4 final automated evidence

- EditMode: 248 executed, 248 passed.
- PlayMode: 126 executed, 126 passed.
- Campaign-specific coverage includes catalog/adaptor validity, deterministic
  sampled curves, modifier placement, local Shield/Booster grants, Ice speed
  and spacing order, Echo acquire/consume/restart, catalog-bound buttons,
  Campaign Camouflage ETA reveal plus ordinary failure, and Campaign Echo
  player-shell activation.
- Every Stage 1-11 initializes under all four requested start-item
  combinations. Stage-provided items explicitly reject duplicate selection.
- The 1,000-run full matrix covers Perfect, Expert, Average, Novice, and Stress
  for every Stage 1-11 and all start-item combinations.
- The Stage 1-5 CSV prefix has zero differences from
  `EchoExpansion-BaselineResults.csv`.
- Step 10 SHA-256 values remain:
  - CSV `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`
  - JSON `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`
  - Summary `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`
  - Comparison `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`
  - Shortlist `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`
- Automated results do not establish fun, comfort, comprehension, fairness,
  satisfaction, or finale quality.

### Stage progression stable-ID regression

- `LobbyPlay_UsesStableIdOfDisplayedProgressionStage` requires the session
  started from a Stage 2 lobby to be Stage 2, then repeats the assertion for
  Stage 3.
- This protects catalog-driven display-number selection from diverging from
  the stable ID used by the Play action.

## Iteration 5 provided-item and high-speed progression acceptance

### EditMode

- `StagesOneThroughFive_RejectAllStartItems` requires catalog and session
  rejection of Shield, Booster, and Both for Stages 1-5.
- `BoosterStage_ProvidesLocalChargeAtStageStart` requires Stage 7 to activate
  its provided Booster immediately after countdown and reject a duplicate
  Booster selection.
- Catalog architecture tests require the serialized resource asset and its
  generated defaults to expose the same item availability.

### PlayMode

- `StagesOneThroughFive_ShowLockedItemsAndRejectToggles` requires disabled
  controls, `LOCKED` labels, and unchanged selection state.
- `StageSeven_StartBoosterCrossesEveryGateAndReachesGoal` requires
  `BOOSTER: PROVIDED`, Booster speed at `GO`, deterministic plane-crossing
  fallback through the existing one-shot gate path, all 40 judgments, Goal,
  and StageCleared.
- Existing Booster, Shield, Continue, Retry, UI, fixed-pool, scene-reference,
  and builder tests remain required. Legacy selectable-item tests run on an
  item-enabled campaign stage.

### Simulation and regression

- Run Stages 6-11 across Perfect, Expert, Average, Novice, and Stress with all
  four item inputs. Perfect runs once; every stochastic condition runs 1,000
  seeded attempts.
- Require zero Booster/Continue displacement, gate-index gap/duplicate,
  cursor-reset, and full-pool-reset counters.
- Stages 6 and 8-11 must stay row-identical to the immediately preceding
  baseline. Stage 7 differences are accepted only as effects of the approved
  start-Booster timing.
- Rerun Step 10 and require all five report hashes to remain exact.

### Final evidence

- EditMode: 249/249.
- PlayMode: 128/128.
- Stage 6-11: 120 rows, 96,024 modeled runs, zero continuity violations, and
  all 24 Perfect/item rows cleared.
- Stage 7 Average/None: first-clear `47.6% -> 47.0%`, Continue-clear
  `84.8% -> 95.1%`, median clear time `33.203s -> 31.356s`, average Booster
  bypasses `9.780 -> 15.774`.
- Step 10 CSV, JSON, summary, comparison, and shortlist hashes match the
  Iteration 4 baseline exactly.
- Automated evidence does not establish fun, comfort, comprehension,
  fairness, or satisfaction.
