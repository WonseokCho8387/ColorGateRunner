# Test History

This file preserves the former feature catalog, Step/Iteration acceptance
mappings, regression evidence, execution counts, hashes, structural checks,
and manual findings moved from the monolithic `TEST_PLAN.md`.

- Source: `Docs/TEST_PLAN.md`
- Source SHA-256: `57B69633EDB91FD0CB5F256500A552C7485A7F517589A967BD0E1FAC7920B057`
- Preservation rule: the historical content below is copied verbatim. Terms,
  counts, hashes, and recorded outcomes have not been normalized or corrected.
- Current validation policy is defined only by `TEST_PLAN.md`.

---

## Feature and Historical Acceptance Catalog

The sections below retain detailed tests and evidence for implemented features
and past iterations. They are not a checklist to paste into every prompt.

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

## Editor test-build menu acceptance

### Automated

- `EnabledBuildScenes_ContainThePlayableScene` requires the menu to use the
  enabled `Assets/Scenes/SampleScene.unity` entry.
- Android and WebGL build-option cases require their expected ignored output
  paths and `BuildOptions.Development`.
- The WebGL portrait-template case requires `540 x 960`, the project template,
  exact `9:16` frame rules, and Unity width/height template macros.
- The WebGL settings case applies the temporary portrait settings and verifies
  that the prior width, height, and template values are restored.
- Focused EditMode result: 5/5 passed.
- Full EditMode result after the portrait correction: 283/283 passed.
- The original Unity editor must compile `ColorGateRunner.Editor` without
  errors and discover both AndroidPlayer and WebGLSupport modules.

### Build pipeline

- Android output is `Builds/Test/Android/ColorGateRunner.apk`.
- WebGL output is `Builds/Test/WebGL`.
- Android test builds must force APK output only for the duration of the build
  and restore the prior App Bundle setting in `finally`.
- Missing enabled scenes, missing platform support, failed target switching,
  and non-successful `BuildReport` results must fail explicitly.
- An isolated validation invocation reached the Android Player backend after
  successful scripts and shader compilation. It stopped making progress
  before APK output and was terminated; this is not an Android build pass.
- A separate isolated WebGL invocation completed successfully and produced a
  124,340,289-byte player. The generated `index.html` contained `width=540`,
  `height=960`, `aspect-ratio: 9 / 16`, and the two viewport-constraining
  dimensions. The validation project's prior `960 x 600` and default-template
  PlayerSettings were restored after the build.

### Manual

- In the original editor, run
  `Tools > Color Gate Runner > Test Builds > Build Android APK`, install the
  APK on an Android device, and complete the current human-play checklist.
- Run `Build WebGL`, serve `Builds/Test/WebGL` through a local web server, and
  complete the same flow in a supported browser.
- Require the WebGL test build to use a `540 x 960` canvas and the
  `ColorGateRunnerPortrait` project template. The generated page must preserve
  a `9:16` canvas inside square, landscape, and portrait host areas without
  stretching.
- Require WebGL width, height, and template PlayerSettings to be restored
  after the build path exits. Android options remain unchanged.
- Run `Build Android + WebGL` after both individual commands pass to confirm
  sequential platform switching and output replacement.

## Iteration 6 Hidden-only Experiment acceptance (originally named Flicker)

### EditMode / Core

- Validate every `HiddenSettings` range and ordering rule.
- Require identical Hidden gate IDs for the same seed and settings.
- Require eligible progress bounds, minimum gate cooldown, maximum
  occurrences, and no non-judgment/Goal selection.
- Require `FirstOccurrenceGuaranteed` to produce one occurrence when an
  eligible gate exists and to remain safe when none exists.
- Require modifier selection to remain fixed after plan/View binding.
- Require no hide before the minimum visible duration, no hide before the ETA
  threshold, hide when both are satisfied, one hide only, and no reappearance.
- Require target color data and ordinary Player → Echo → Shield → Failure
  judgment priority to remain unchanged.
- Require Retry and Replay to reproduce selection while clearing runtime
  visibility state.
- Require Hidden not to increase gate count or combine with another modifier.

### PlayMode

- Launcher can select a Hidden-only condition and disables Booster.
- Countdown does not advance Hidden observation time.
- Playing advances visible time; the initial color, symbol, and `HIDDEN`
  identity are readable.
- Meeting both visible-time and ETA conditions begins the transition. On
  completion, target color and symbol are hidden while the neutral silhouette,
  judgment opening, collider, and `HIDDEN` identity remain.
- Direct player match, existing Echo match, Shield defense, and ordinary
  failure flow remain available after hiding; Echo and Shield are not both
  consumed.
- Retry/Replay reset the visual state and reproduce the same selected gates.
- Repeated Lab entry, Back to Lab, listener counts, fixed six-gate pool,
  two-pass Scene Builder, Missing Script, and Missing Reference checks remain
  required.

### Regression and human review

- Run full EditMode and PlayMode suites.
- Rerun Stage 1-11 campaign simulation and the existing Step 10 matrix.
  Campaign rows and all five Step 10 hashes must remain unchanged.
- Confirm packages and ProjectSettings have no unintended changes and run
  `git diff --check`.
- Human review records concept comprehension, observation duration, hide
  timing, memory interval, transition clarity, retained gate position,
  Echo/Shield comprehension, portrait readability, and repeat-entry fatigue.
- Automated evidence does not establish fun, fairness, readability, comfort,
  comprehension, satisfaction, or motivation.

### Iteration 6 final automated evidence

- Original Editor script compilation: no C# errors.
- EditMode: 262/262.
- PlayMode: 134/134.
- Post-Builder PlayMode: 134/134 after the command rebuilt the scene twice.
- Campaign: 220 rows, 176,044 modeled runs, with exact baseline Summary, JSON,
  and CSV hashes.
- Step 10 hashes:
  - CSV `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`
  - JSON `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`
  - Summary `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`
  - Comparison `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`
  - Shortlist `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`
- The only warning class was the pre-existing Unity API deprecation in
  `Step10_1PlayModeTests`; it is not introduced by Hidden.

## Iteration 7 Hidden migration and color-cycling Flicker acceptance

### Baseline gate

- Clean worktree and `git diff --check`.
- EditMode `262/262`; PlayMode `134/134`.
- All Echo and former-Flicker targeted cases pass.
- Campaign Summary/JSON/CSV and all five Step 10 hashes match Iteration 6.
- Package manifest/lock and ProjectSettings have no diff.

Any mismatch blocks implementation.

### Hidden migration

- Numeric values remain `GateModifierType.Hidden = 1 << 4` and
  `MechanicExperimentType.Hidden = 5`; new Flicker uses new values.
- Former serialized Launcher Flicker fields load as Hidden through explicit
  Unity migration metadata and never become new Flicker settings.
- Hidden seed `12345` selects gate IDs `[6, 11, 18, 23]`; seed `98765`
  selects `[6, 9, 13, 20]`.
- Existing observation, ETA hide, transition, neutral silhouette, ordinary
  Player/Echo/Shield/Failure judgment, and Retry/Replay reset remain.
- Launcher and marker use `Hidden` / `HIDDEN`.

### New Flicker Core/EditMode

- Validate all settings, including cycle count `2/3`, active-color capacity,
  positive switch interval, nonnegative pulse, and minimum visible cycles.
- Same seed and Gate ID reproduce selected gates, `CycleColors`, and phase
  offset. The first cycle color equals the plan base color; remaining colors
  are unique active-palette colors.
- Two colors alternate exactly; three colors rotate all three; Goal and other
  modifiers are excluded; gate count is unchanged.
- Eligible bounds, cooldown, maximum, guaranteed first occurrence, and
  minimum expected exposure are enforced without fallback substitution.
- Gameplay Time `0`, interval-minus-epsilon, exact interval boundary,
  multiple intervals, phase-zero, randomized deterministic phase, and
  float-boundary cases use the same pure calculator.
- Collision-time color, rather than base color or prior View color, drives
  Player → Echo → Shield → Failure. Echo and Shield never both consume.
- Countdown/Failed/Cleared freeze Gameplay Time; Retry/Replay reset it and
  reproduce the same phase at the same time.
- Flicker simulation checks call the same Core active-color calculation.

### New Flicker PlayMode

- Hidden and Flicker are separate Launcher choices; both disable Booster.
- Countdown does not advance phase; Playing changes color and matching symbol.
- Two- and three-color display, exact boundary, short pulse, persistent
  `FLICKER` marker, stable transform/collider, and no neutral interval pass.
- Player, held Echo, Shield, and ordinary Failed flow use the collision-time
  active color.
- Retry/Replay initial phase, Back to Lab, Lab reentry, pooled View reuse,
  listener count, fixed pools, and modifier-state clearing pass.
- Scene Builder succeeds twice with no Missing Script, Missing Reference, or
  duplicate generated object.

### Regression and human review

- Run full EditMode and PlayMode suites, Stage 1-5 campaign matrix, existing
  Step 10 matrix, targeted Echo/Shield/Camouflage/Hidden/Flicker/runtime-flow
  cases, Retry/Replay, and Lab reentry.
- Preserve all campaign and Step 10 hashes. Preserve package manifest/lock and
  meaningful ProjectSettings state. Run `git diff --check`.
- Human review records Hidden concept continuity, Flicker concept recognition,
  `0.50s` two-color speed, three-color difficulty, collision-time judgment
  comprehension, color/symbol sync, pulse clarity, marker interference,
  Echo/Shield explanations, 9:16 readability, and fast-speed observation.
- Automation does not establish fun, fairness, readability, comfort,
  comprehension, satisfaction, or motivation.

### Iteration 7 final automated evidence

- EditMode `281/281`.
- PlayMode `143/143`; after two successful Scene Builder runs, PlayMode
  `143/143` again.
- Related cases: Echo `27/27`, Shield `29/29`, Camouflage `9/9`, Hidden
  `25/25`, Flicker `27/27`, Experiment Runtime `5/5`.
- Full Stage 1-11 campaign output: 220 rows, including the requested Stage 1-5
  coverage; Summary
  `22B85883B00B5A4404D8E352A8C1CFD896ADF03A9DA03B1D64C3B0102AAA33B0`,
  JSON
  `F5A57219244BDE8BDF61CC97A68D3EE2C05D3800F21384D0121568CAE15562CC`,
  CSV
  `D88D4B15E17D35971094CAF70038BBC7D580426DEBD267B104CA7F1A1FE76850`.
- Step 10 output: 80 rows and 64,016 runs; CSV
  `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`,
  JSON
  `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`,
  Summary
  `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`,
  Comparison
  `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`,
  Shortlist
  `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`.
- Final rebuilt-scene tests report no Missing MonoBehaviour, required
  reference, pool growth, duplicate root, listener, or Lab reentry failure.
- Package manifest/lock and ProjectSettings have no intended change.

## Hidden lead-time and Flicker palette-order revision acceptance

### Core/EditMode

- Hidden defaults to `HideLeadTimeSeconds = 0.85` while readable duration
  remains `1.00s` and transition remains `0.12s`.
- Hidden does not start above ETA `0.85`, starts exactly at the boundary after
  minimum observation, and retains ordinary one-way hide/reset behavior.
- `FlickerSettings` has no authored cycle-count value or `2/3` validation.
- For every supported Experiment color count `3-6`, a Flicker plan contains
  every active color exactly once and derives its count from that palette.
- The first cycle color equals the gate base color. Every subsequent color,
  including wraparound, equals the same Core next-color result used by player
  input and therefore requires one forward tap from the prior phase.
- Same seed reproduces gate targets and phase offsets. Cycle order no longer
  consumes seeded random values; Goal exclusion, eligible bounds, cooldown,
  maximum occurrences, minimum exposure, and gate count remain unchanged.
- Exact Gameplay Time boundaries, Player/Echo/Shield/Failure priority,
  Retry/Replay, simulation sharing, and no mixed modifier behavior retain
  regression coverage.

### PlayMode and human review

- Launcher exposes no two-/three-color cycle-count control. Selected Experiment
  color count determines the Flicker cycle.
- Three- and six-color runs display the full ordered palette with matching
  symbols, pulse, collision-time judgment, and pooled-View reset.
- Human review records whether `0.85s` Hidden memory time is appropriately
  harder, whether Flicker can be followed with one tap per switch, and whether
  collision-boundary response remains achievable at `0.50s`.
- Automation does not establish difficulty quality, learnability, mastery,
  readability, fairness, satisfaction, or fun.

### Final automated evidence

- Baseline EditMode `283/283`; baseline PlayMode `143/143`.
- Final EditMode `288/288`; final PlayMode `144/144`.
- Focused Hidden/Flicker results: EditMode `14/14` and `20/20`; PlayMode
  `6/6` and `10/10`.
- Scene Builder succeeded twice; generated Launcher data contains Hidden
  `0.85` and no Flicker cycle-count field. Post-Builder PlayMode is `144/144`.
- Stage 1-11 Campaign: 220 rows; Summary/JSON/CSV hashes remain
  `22B85883B00B5A4404D8E352A8C1CFD896ADF03A9DA03B1D64C3B0102AAA33B0`,
  `F5A57219244BDE8BDF61CC97A68D3EE2C05D3800F21384D0121568CAE15562CC`,
  and
  `D88D4B15E17D35971094CAF70038BBC7D580426DEBD267B104CA7F1A1FE76850`.
- Step 10: 80 rows and 64,016 runs; all five hashes remain identical to the
  Iteration 7 evidence above.

## Iteration 8 Campaign Hidden and Flicker stages

### Baseline

- Require a clean worktree and `git diff --check`.
- Full EditMode must reproduce `288/288`; full PlayMode must reproduce
  `144/144`.
- Stage 1-11 Campaign Summary/JSON/CSV hashes must reproduce the Iteration 7
  values before implementation.
- Step 10 must reproduce 80 rows, 64,016 runs, and all five Iteration 7 hashes.

### Core and EditMode

- Catalog contains 13 stable stages; all Stage 1-11 definitions remain byte-
  equivalent through the Adapter.
- Stage 12/13 IDs, display numbers, titles, gate counts, three-color palettes,
  speed curves, cadence, item availability, seeds, and primary mechanics
  match the approved contract.
- Hidden/Flicker never apply to Goal and never increase gate count.
- Same seed and Retry reproduce target gate IDs. Flicker also reproduces cycle
  colors and phase offsets.
- Stage 12 applies only Hidden and retains the `0.85s` ETA hide contract.
- Stage 13 applies only Flicker and cycles Red -> Blue -> Green -> Red using
  the same next-color rule as player input.
- Flicker collision judgment uses the exact supplied Campaign Gameplay Time,
  including exact switch-boundary tests.
- Simulator and runtime call the same judgment-color calculator. Player,
  Echo, Booster, Shield, and Failure remain mutually ordered and do not double
  consume defenses.
- Minimum cycle visibility is tested at normal maximum speed and selectable
  Booster speed. Item choice must not alter planned targets or phases.
- Retry/Continue preserve the documented Campaign progress contract while
  Retry resets Gameplay Time and View state.

### PlayMode

- Lobby exposes exactly 13 catalog-driven buttons and Stage 11 -> 12 -> 13
  progression uses stable IDs.
- Stage 12 displays/hides ordinary pooled gates and resets correctly on Retry,
  Continue, replay, pool reuse, and return to Lobby.
- Stage 13 starts phases only in Playing, synchronizes color and symbol,
  pulses at boundaries, and judges the displayed collision-time color.
- Shield and Booster are selectable in both stages. Booster activates at
  `GO`, uses the ordinary auto-pass path, and does not suppress later planned
  Hidden/Flicker presentation.
- Goal completion, Continue, Retry, next-stage navigation, Back to Lobby,
  repeated scene entry, fixed pool size, and listener counts remain valid.
- Scene Builder runs twice; require no Missing Script, Missing Reference,
  duplicate controller/root/EventSystem/button/listener, or stale modifier
  View state.

### Simulation and regression

- Run all five player profiles and every allowed start-item combination for
  Stages 1-13 with 1,000 runs per row.
- Perfect exact-input profiles must clear both new stages. Report rather than
  tune any adjacent non-Perfect first-attempt change of five percentage points
  or more.
- The Stage 1-11 prefix must exactly match all three existing campaign
  artifacts and hashes. Stage 12-13 results receive new 13-stage artifacts;
  existing baseline artifacts are not overwritten.
- Step 10, Echo, Shield, Camouflage, Hidden, Flicker, Experiment runtime,
  package manifest/lock, ProjectSettings, and `git diff --check` remain
  regression boundaries.

### Human feedback

- Stage 12: Hidden concept clarity, `0.85s` memory difficulty, Booster
  interaction, marker readability, and failure comprehension.
- Stage 13: three-color forward rotation comprehension, `0.50s` timing,
  collision-boundary readability, sufficient post-Booster encounters,
  color/symbol synchronization, Shield feedback, and portrait readability.
- Automation cannot establish fun, fairness, comfort, mastery, satisfaction,
  or final balance.

### Final automated evidence

- Baseline EditMode `288/288`; baseline PlayMode `144/144`.
- New focused results: Core/EditMode `6/6`; Campaign PlayMode `3/3`.
- Final EditMode `294/294`; final PlayMode `147/147`.
- Scene Builder completed twice; post-Builder PlayMode `147/147`.
- Stage 1-13 matrix contains 260 rows. The first 220 Stage 1-11 JSON rows have
  zero differences from the previous result.
- Perfect clears Stage 12/13 for None, Shield, Booster, and Shield+Booster.
  Booster/Continue index-gap, duplicate, reset, displacement, and pool-reset
  regression counters are zero.
- Stage 11/12/13 Average+None first-attempt clear is
  `26.9% / 16.2% / 17.6%`; human review is required before tuning.
- New 13-stage Summary/JSON/CSV hashes are
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and
  `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and 64,016 runs with all five Iteration 7 hashes.
- Package manifest
  `2DD47B08B54B22B90AC931E7BE86F2C49E99994029F683ED177233B60E77A941`
  and lock
  `0CCE79313E478B8C892DD1D9A299F66BA9DEAB61D62AE0B05525DB3AA08E6CC7`
  are unchanged. ProjectSettings has no diff and `git diff --check` passes.

## Iteration 9 — Commercial Flow Foundation 1

### Product/EditMode contract

- Product assembly has no Unity reference and `noEngineReferences: true`.
- Initialization records Clock/paths, Save load/recovery, Profile, Settings,
  dirty persistence, and Complete in order; failure stops later steps and
  Retry reuses the same services.
- Fresh data creates one Guest from injected ID/clock; reload preserves ID and
  does not store a device identifier.
- Current-schema roundtrip preserves Profile and Settings. SaveRevision and
  LastWriteUtc must already be present when temp bytes are serialized and
  decoded for validation.
- Failed replacement preserves the prior primary. Corrupt primary restores a
  valid backup. Two corrupt files return a typed blocking error. Unknown future
  schema does not fall back or overwrite. Schema 0 migration is deterministic.
- Invalid settings reset as one documented section to master/music/SFX 1,
  vibration on, and language `system`. Test files remain inside the injected
  temporary directory.
- Recoverable-only replacement returns `Recoverable`, never `Atomic`.
- Destination selection uses the first active non-Boot Scene, rejects Boot-
  only input, contains no SampleScene-name assumption, deduplicates Boot at
  index 0, preserves other Scene order/enabled state, and retains a valid
  explicit path after order changes.

### Boot/PlayMode contract

- Boot contains its generated root, AppRoot, controller, Safe Area, Loading,
  version, error/Retry references, and exactly one EventSystem.
- Success loads the serialized active Campaign path only after Profile and
  Local Account are populated. Failure remains in Boot and shows the error.
- Retry succeeds through the same graph, hides the error, loads once, and does
  not duplicate the button listener or AppRoot.
- Returning to Boot creates neither a second service graph nor a second
  persistent AppRoot.
- Boot and Guest creation preserve pre-seeded highest-unlocked and Stage-record
  PlayerPrefs exactly. Product code contains no Stage progression reference.

### Final automated evidence

- EditMode `313/313`; PlayMode `152/152`.
- Boot Builder run 1 and run 2 succeeded. Existing Campaign Scene Builder run
  1 and run 2 succeeded. Post-Boot and post-Campaign-Builder PlayMode each
  passed `152/152`.
- Builder validation and PlayMode cover required references, one EventSystem,
  Safe Area, no duplicate roots, no Missing Script, and valid Campaign entry.
- Campaign 260-row Summary/JSON/CSV hashes are
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 CSV/JSON/Summary/Comparison/Shortlist hashes remain
  `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`,
  `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`,
  `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`,
  `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`,
  and `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`.
- Package manifest/lock hashes remain
  `2DD47B08B54B22B90AC931E7BE86F2C49E99994029F683ED177233B60E77A941`
  and `0CCE79313E478B8C892DD1D9A299F66BA9DEAB61D62AE0B05525DB3AA08E6CC7`.
  The only ProjectSettings diff is Boot-first EditorBuildSettings.

### Human acceptance

- Check 9:16 Boot readability, no loading flicker, Campaign transition feel,
  understandable local-error/Retry messaging, and no duplicate audio/input.
- On Android and WebGL, check fresh Guest creation, restart identity, forced-
  close recovery behavior, and preservation of real Stage progress.
- Automation does not claim visual polish, messaging comprehension, device-
  specific persistence durability, or transition satisfaction.

## Iteration 10 — Commercial Flow Foundation Frontend 2

### EditMode contract

- Router initial Page is Title; Title and Lobby transitions are exclusive and
  same-Page requests are safe.
- Scene Transition rejects duplicate Page, Back, and Scene requests and clears
  its blocker on completion or failure.
- Modal owns Back before Page navigation; Lobby Back reaches Title and Title
  Back requests Exit Confirmation.
- Guest Profile, Account state, version, and Settings bind to an immutable
  display Context. Missing Profile is `BOOT REQUIRED`; missing optional
  Settings produces a hidden feature rather than a null action.
- Frontend Campaign selection uses the first active non-Boot/non-Frontend
  Scene without a `SampleScene` name dependency. Final Build Settings are
  exactly Boot, Frontend, Campaign and all are enabled and unique.
- Explicit Boot destination accepts active Frontend and rejects Boot, disabled,
  or empty paths.

### PlayMode contract

- Successful Boot creates one AppRoot, loads Frontend once, shows Title, Guest
  state, version, one primary Page, and one EventSystem.
- Title -> Lobby, repeated input, Lobby Back, Account notice, Settings/Modal
  Back, Exit Confirmation, and hidden legal/future slots remain stable.
- Direct Frontend entry has no AppRoot, shows blocking `BOOT REQUIRED`, and
  cannot load Campaign.
- Failed Campaign load clears Loading and Transition blocker, reports one
  error Modal, and does not repeat the load.
- Campaign entry loads the existing Scene once, initializes its existing Lobby,
  preserves Stage PlayerPrefs, and retains one persistent AppRoot.
- Frontend re-entry does not duplicate AppRoot, EventSystem, Router listener,
  or primary Page; repeated Title/Lobby navigation remains stable.

### Final automated evidence

- EditMode `335/335`; PlayMode `160/160`. Post-Builder results are also
  `335/335` and `160/160`.
- Frontend Builder run 1 and 2, Boot Builder run 1 and 2, and Campaign
  Builder's two-build command all succeeded.
- Campaign simulation remains 260 rows. Summary/JSON/CSV hashes are
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows. CSV/JSON/Summary/Comparison/Shortlist hashes are
  `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`,
  `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`,
  `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`,
  `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`,
  and `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`.
- Package manifest/lock hashes remain
  `2DD47B08B54B22B90AC931E7BE86F2C49E99994029F683ED177233B60E77A941`
  and `0CCE79313E478B8C892DD1D9A299F66BA9DEAB61D62AE0B05525DB3AA08E6CC7`.
  `ProjectSettings.asset` is unchanged after the approved baseline restore;
  only `EditorBuildSettings.asset` adds Frontend at Index 1.

### Human acceptance

- Check Boot -> Title and Lobby -> existing Campaign Lobby transition clarity.
- Check 9:16 small-screen hierarchy, Guest/account/settings wording, Back
  behavior, rapid-tap blocking, and no UI-to-gameplay input leakage on Android
  and WebGL.
- Automation does not claim final visual polish, transition satisfaction,
  wording comprehension, or device-specific Back/input quality.

## Iteration 11 — Campaign Progress Restore Test Isolation Hotfix

### Persistence isolation contract

- Every PlayMode fixture that can touch Campaign progression captures the
  complete device-wide Campaign PlayerPrefs set before setup work:
  `ColorGateRunner.Stage.HighestUnlocked` and Stage Records 1 through 13.
- The snapshot stores key existence plus the integer Highest value or string
  Record value. Cleanup restores existing keys to their exact values, deletes
  only keys absent before the test, and calls `PlayerPrefs.Save()`.
- Setup failure and teardown failure paths must still restore the snapshot.
  Unconditional cleanup deletion and replacement with guessed defaults are
  forbidden.
- Snapshot Utility tests cover an existing integer key, existing string key,
  absent key, all 13 Records, and an exception thrown inside the protected
  scope. The outer fixture snapshot must also preserve the real Editor state.

### Campaign restore regression contract

- Highest 11 with cleared Records 1-10 selects Stage 11 on first entry and
  re-entry. Lobby, PreRun, and Gameplay use the same stable Stage ID.
- A completely fresh Campaign selects Stage 1. Clearing Stage 1 and recreating
  the Campaign controller selects Stage 2 and never jumps to Stage 11.
- Creating a new Product Guest neither changes Campaign keys nor changes first
  entry/re-entry selection. Campaign progress remains device-wide and separate
  from Product Guest identity in this Iteration.
- Missing or corrupt Highest safely falls back to Stage 1 and does not delete
  otherwise valid Stage Records.

### Final automated evidence

- Snapshot Utility `3/3`; restore scenarios `4/4`; full EditMode `335/335`;
  full PlayMode `167/167`; post-Builder PlayMode `167/167`.
- Frontend Builder run 1/2, Boot Builder run 1/2, and Campaign Builder run 1/2
  succeed with no Missing Script, Missing Reference, or duplicate generation.
- Before/after actual Editor Campaign PlayerPrefs snapshots must have identical
  key counts and SHA-256. Product save hash and Guest identity must remain
  unchanged.
- Campaign Summary/JSON/CSV and Step 10 CSV/JSON/Summary/Comparison/Shortlist
  must retain all eight approved hashes.
- `ProjectSettings.asset`, Packages, Scenes, runtime progression code, Product
  Save Schema, Stage Catalog, and Campaign balance must have no Hotfix diff.

### Deferred

- Unknown values deleted before the Hotfix are not reconstructed or guessed.
- Profile-scoped Campaign progress and migration remain deferred to a separate
  `StageProgressService` Iteration.
- Fully replacing real PlayerPrefs with an injected memory store during tests
  is a future testability improvement, not part of this minimal Hotfix.

## Iteration 12 — Account Onboarding, Settings, and Gameplay Pause UX

### Product and onboarding acceptance

- Schema-1 Profile roundtrip preserves `AccountChoiceCompleted`; a missing
  field remains backward-compatible `false` without a schema-version change.
- Settings validation/repair cannot reset onboarding completion.
- Guest choice saves before navigation. Save failure leaves Profile, Settings,
  onboarding, Page, and public service state unchanged and exposes retryable
  feedback.
- The thin Product session publishes the saved snapshot through one rebind
  path and does not become a second persistence authority.
- Provider absence hides the release action and cannot claim a linked state.
- First entry shows Account Choice; successful Guest choice reaches Lobby;
  re-entry with completed onboarding goes directly to Lobby without a Title
  flash. Direct Frontend entry without AppRoot remains `BOOT REQUIRED`.

### Settings acceptance

- Frontend and Gameplay use the same Settings panel contract. Cancel discards
  draft UI values; Apply persists Master, Music, SFX, and Vibration.
- Master alone updates `AudioListener.volume`. Music and SFX roundtrip without
  pretending to control unavailable content targets.
- Vibration false prevents the Booster haptic callback itself; true permits
  the existing callback.
- Failed persistence keeps the panel open and retains prior public Settings.

### Gameplay Pause acceptance

- The coordinator owns exactly one base Pause state, nested Settings or
  confirmation modal, and transition state. Repeated Back/tap requests do not
  duplicate layers or Scene loads.
- Countdown, Playing, and Shield Recovery can pause. Attempt time, movement,
  input, judgment, recycling, Flicker/Hidden timing and presentation, camera
  feedback, and registered attempt VFX remain frozen.
- A same-frame queued manual or physics judgment checks Pause immediately
  before mutation and records no result.
- Resume preserves the Attempt; confirmed Restart uses existing Retry-to-
  PreRun; confirmed Lobby uses the serialized Frontend path and records no
  result. Load failure remains paused and recoverable.
- Only the Campaign Builder's explicit attempt Effect Root registrations are
  paused. Scene-wide ParticleSystem discovery is prohibited.
- Focus loss requests Pause; focus gain does not resume automatically.

### Final automated evidence

- Full EditMode `352/352`; full post-Builder PlayMode `179/179`.
- Frontend Builder twice, Boot Builder twice, and Campaign Builder twice all
  succeeded. Missing Script, Missing Reference, duplicate generated roots,
  EventSystem duplication, and pooled-state regressions were not reported.
- Campaign remains 260 rows with approved Summary/JSON/CSV hashes
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and 64,016 runs with approved CSV/JSON/Summary/
  Comparison/Shortlist hashes
  `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`,
  `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`,
  `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`,
  `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`,
  and `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`.
- Actual Editor Campaign PlayerPrefs remained 12 entries with canonical hash
  `B57BDC93138A3E1C376272C33FB042274E821B8BB9C0EC12A33B9480010C73CD`.
  Product save hash remained
  `69AA916EF1295FECE34A4DD9A1EC13A525DFDF477EC3EBAF9A7E85898AFF0191`
  and the existing Guest ID was unchanged.
- Package manifest/lock retain hashes
  `2DD47B08B54B22B90AC931E7BE86F2C49E99994029F683ED177233B60E77A941`
  and `0CCE79313E478B8C892DD1D9A299F66BA9DEAB61D62AE0B05525DB3AA08E6CC7`.
  `ProjectSettings.asset` has no Iteration 12 change.

### Human acceptance and deferred scope

- Review first-run Account Choice clarity, Guest trust, direct-to-Lobby
  continuity, Settings readability, Pause Dim coverage, confirmation wording,
  rapid Back/tap handling, and Android background behavior at 9:16.
- Music/SFX audible effect, Google/account linking, cloud/profile-scoped
  Campaign progress, final Lobby/Campaign/Stage Detail/Results, economy,
  events, analytics, ads, and IAP remain deferred.

## Iteration 13 — Lobby Consolidation and Unified Settings

### Focused acceptance

- A read-only Frontend progress adapter selects the lowest uncleared unlocked
  stable Stage ID and cannot write PlayerPrefs.
- The AppRoot launch request queues once, cancels only the matching request,
  consumes once, and is never serialized.
- Production `START STAGE` bypasses Campaign Lobby and opens existing PreRun
  for the same Stage ID. Missing context keeps the existing Lobby fallback.
- Failed Scene loading removes the pending launch request and reports one
  recoverable error.
- Frontend and Pause use one Settings controller and field set. Music/SFX OFF
  writes zero; ON restores last non-zero or `1.0`; Master remains preserved.
- Schema-1 saves missing Notification and last-non-zero fields use `false`,
  `1.0`, and `1.0` without changing Profile/onboarding state.
- Product links accept only absolute HTTPS, use an injectable opener, and do
  not open when unconfigured. Notification UI states that delivery is absent.
- Pause button is active under Gameplay HUD, at least 44 by 44, inside Safe
  Area, clear of Stage/Shield HUD, and does not mutate player color through the
  gameplay tap surface.

### Final automated evidence

- Focused EditMode `31/31`; focused Frontend/Gameplay PlayMode `114/114` after
  isolating Stage 1-13 records; targeted stable-ID launch regression `1/1`.
- Full EditMode `361/361`; full post-Builder PlayMode `180/180`.
- Frontend Builder run 1/2 and 2/2, Boot Builder run 1/2 and 2/2, and Campaign
  Builder's two-build command succeeded. Build Settings remain Boot,
  Frontend, Campaign with unique enabled entries.
- No Missing Script, required-reference, duplicate-root, EventSystem, pooled-
  state, or repeated-listener failure was reported.
- Campaign simulation reproduced 260 rows and Summary/JSON/CSV hashes
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 reproduced 80 rows / 64,016 runs and all five approved hashes.
- Campaign PlayerPrefs still contains Highest plus 12 existing Stage records;
  PlayMode snapshot tests restored exact state. Product save file timestamp and
  Guest ID were unchanged by validation.
- Package hashes remain approved. Only environment-dependent presence/order
  of `APP_UI_EDITOR_ONLY` and `SENTIS_ANALYTICS_ENABLED` is allowed in WebGL
  defines, and `ProjectSettings.asset` is excluded from the feature commit.

### Human review and deferred

- Review recommended-Stage hierarchy, transition directly into PreRun,
  Notifications/Music/SFX/Vibration wording, disabled prototype links, Pause
  reachability, and 9:16 Android/WebGL layout.
- Notification delivery, real Music/SFX AudioSources, configured legal/support
  URLs, Campaign-to-Frontend return, Stage Detail, economy, events, analytics,
  ads, IAP, and profile-scoped Campaign progress remain deferred.

## Iteration 14 — PreRun Back and Settings Toggle Hotfix

### Focused acceptance

- Frontend `START STAGE` consumes its one-shot stable Stage request once,
  enters PreRun, and Back loads the serialized Frontend destination without a
  frame of legacy Campaign Lobby.
- Missing context and direct SampleScene entry return PreRun Back to the
  existing Campaign Lobby. Development and Experiment paths are unchanged.
- Repeated Back while a Frontend return is pending produces one load. Failure
  keeps PreRun active, restores Back, and permits retry without fallback.
- Notifications, Music, SFX, and Vibration each contain exactly one
  `StateLabel`. ON shows only `ON`; OFF shows only `OFF`; opening, clicking,
  applying, cancelling/re-entering, Frontend/Pause movement, and save reload
  synchronize immediately.
- Two Builder passes leave no `OnLabel`, `OffLabel`, duplicate state label,
  duplicate listener, root, or EventSystem.

### Final automated evidence

- Full EditMode `361/361`; full post-Builder PlayMode `184/184`.
- Frontend Builder completed twice and rebuilt Boot twice; Campaign Builder's
  internal two-pass build and validation completed successfully.
- Frontend and Campaign Scenes each contain four `StateLabel` objects and zero
  legacy `OnLabel`/`OffLabel` objects. Missing Script/Reference scans are clean.
- Campaign simulation reproduced 260 rows and all three approved hashes.
  Step 10 reproduced 80 rows / 64,016 runs and all five approved hashes.
- Campaign PlayerPrefs snapshot tests restored exact state. Product save
  timestamp predates validation, its Guest ID is unchanged, and Package hashes
  remain approved. Only allowed package-managed WebGL define volatility was
  observed in ProjectSettings.

### Human review

- Confirm PreRun Back feels immediate, the Frontend Lobby is the first visible
  return state, and ON/OFF text remains readable at 9:16 on Android and WebGL.
- Automation does not determine transition feel, visual polish, or touch
  ergonomics.

## Iteration 15 — Automatic Lobby Progression Foundation

### Focused acceptance

- Schema 1 loads as dirty schema 2 with safe Campaign, Economy and Lobby
  defaults; future-schema and recovery rules remain unchanged.
- Legacy stable-ID Campaign import is one-time, atomic and reward-idempotent.
- Stage clear grants first-clear/milestone rewards once, acknowledges only
  presentation state, and publishes no partial state after save failure.
- Frontend Lobby shows Product wallet, inventory, theme and next milestone,
  then acknowledges the pending presentation without reapplying rewards.
- Production-like Frontend tests import legacy PlayerPrefs while exact fixture
  snapshots restore the user's Editor keys.

### Final automated evidence

- Product-focused EditMode `25/25`; focused Frontend PlayMode `16/16`.
- Full EditMode `365/365`; full post-Builder PlayMode `186/186`.
- Frontend Builder passed twice. Generated Frontend required-reference,
  missing-script, unique-root, EventSystem and Build Settings checks passed.
- Campaign and Step 10 simulations were omitted by Tier 2 because no Stage
  data, deterministic generation, timing, judgment or balance input changed.
- The Editor Product save was not written by tests. Before real migration it
  remained schema 1/revision 18 with Guest ID unchanged. Package hashes and
  non-volatile ProjectSettings semantics match the approved baseline.

### Human review

- Review placeholder Lobby hierarchy, automatic upgrade comprehension, reward
  value, return transition feel and portrait readability. Automated evidence
  does not prove motivation, polish, fairness or economy satisfaction.

## Iteration 16 — Campaign Act 2: Stage 14–20

### Focused acceptance

- Catalog revision 6 resolves 20 unique contiguous stable IDs and preserves
  every Stage 1–13 definition.
- Stage 14–20 match the approved gate counts, speed/cadence curves, three-color
  palette and primary mechanics.
- Generated plans are deterministic. Stage 14 has no modifier; each later
  Stage contains only its named modifier. Hidden and Flicker remain bounded to
  four deterministic occurrences.
- Campaign Builder generates 20 buttons and keeps Lobby current-stage/title
  bounds separate across the approved portrait reference resolutions.

### Final automated evidence

- Focused Act 2 EditMode `3/3`; full EditMode `368/368`; final post-Builder
  PlayMode `186/186`.
- Campaign Builder completed two consecutive generation passes and structural
  validation with no Missing Script, Missing Reference, duplicate root,
  EventSystem or Build Settings failure.
- Campaign simulation produced 400 rows: 20 rows for each Stage 1–20. Prior
  Stage 1–13 CSV rows and the JSON results prefix match the approved 260-row
  baseline byte-for-byte.
- Full hashes: Summary
  `FAA0ED46B5B6F941C78C487CA8DC4E30752FE8DB1B95626C36B5E5DB02F85E5B`,
  JSON `A23476F2C21425192706EBB28A9529266425AFE2A543A848B6B29A3789A2BEA0`,
  CSV `91388698740E141C8B711BC8586D2D36172E6EA9FB7F25201AC22603CF8EB3B2`.
- Step 10 remained at its approved baseline because no Experiment contract,
  shared mechanic rule, player profile or simulation input changed.

### Human review

- Play Stages 14–20 consecutively on portrait Android and WebGL. Judge rhythm
  variety, returning-mechanic mastery, Stage 14 recovery, Stage 17 pressure,
  Stage 18 relief and Stage 20 fatigue.
- Automation does not prove fun, readability, fairness, comfort or final
  balance.

## Iteration 17 - Campaign Learning-Curve Redistribution

### Focused acceptance

- Catalog revision 7 contains 20 contiguous stable IDs. Stages 1-5 remain
  exact foundation content; Stage 6 is locked provided Shield, Stage 7 is
  locked provided Booster and Stage 8 is a clean three-color selectable-item
  Stage.
- Camouflage Stages 9-11, Fog 12-14, Ice 15-17 and Echo 18-20 each follow
  two-color introduction, two-color practice and three-color mastery. No
  current Campaign Stage authors Hidden or Flicker.
- Hidden and Flicker continue to pass their independent Core, Experiment and
  PlayMode contracts while Campaign placement is deferred to Stages 21-26.
- Product-save progression remains bound to the existing stable IDs. The
  redistribution introduces no save reset, record remap or migration.

### Final automated evidence

- Full EditMode: `367/367` passed.
- Full post-Builder PlayMode: `195/195` passed.
- Campaign Builder completed two consecutive passes. Generated Catalog/Scene
  references, 20 selection entries, unique roots, EventSystem and Build
  Settings validation passed.
- Two complete Campaign simulation runs each produced 400 rows and were
  byte-identical. Approved hashes are:
  - Summary:
    `D07BC7A2808DD4A31E65FA618F20EC3981F782568614510E1AF020146C4820DE`
  - JSON:
    `6FF5187DDE11EFA38C27C4AD96CD3145315D41BB33178CCC8F2F73F683CB3D0E`
  - CSV:
    `522053519887272534EABEB1769EAD4709C12D865514EA1068CFE899FD97B37A`
- The prior Stage 1-5 CSV 100 rows and the first 100 JSON result objects match
  exactly. Continuity, cursor, gate-index, duplicate and fixed-pool violation
  counters remain zero.
- Step 10 artifacts were not rewritten because Experiment contracts, shared
  mechanic rules, profiles and simulation inputs are unchanged.
- The Product save-failure test fixture was stabilized to fail at its intended
  persistence boundary. This changes test robustness only, not runtime save
  behavior.

### Human review

- Play the four three-Stage mechanic blocks consecutively on portrait Android
  and WebGL. Judge comprehension, repetition fatigue, block transitions and
  the two-color-to-three-color mastery step.
- Automation does not establish whether the revised curve teaches effectively
  or feels fun, fair, readable and comfortable.

## Iteration 18 - Consumable Start Items

### Focused acceptance

- Stage 8+ consumes exactly one owned unit for each manually selected Shield
  and Booster when `START` succeeds. Both selections publish as one atomic
  Product mutation.
- Retry retains toggles but a new successful Start consumes those selections
  again. Duplicate Start input consumes once and produces one countdown.
- Insufficient inventory and Product save failure retain PreRun and publish no
  partial balance, transaction, selection or gameplay transition.
- Stage 6/7 provided items remain free and selection-locked. Missing Product
  context never grants a selected item for free; no-item and provided-item
  paths remain available.
- Product schema, gameplay Core, Campaign Catalog and deterministic stage data
  remain unchanged.

### Final automated evidence

- Targeted Product EditMode: `30/30` passed.
- Targeted item-consumption PlayMode: `5/5` passed.
- Campaign Builder completed two consecutive passes and its generated-reference,
  unique-root, EventSystem, Build Settings and missing-reference validation.
- Full EditMode: `372/372` passed.
- Final post-Builder PlayMode: `207/207` passed.
- Package manifest/lock, non-volatile ProjectSettings and the actual Product
  save remained unchanged. The existing Guest ID and owned balances were
  preserved.
- Validation Tier 2 omitted Campaign and Step 10 simulation reruns. No Core
  item effect, Stage data, deterministic simulation input or Experiment
  contract changed, so their approved hashes remain authoritative.

### Human review

- Verify portrait PreRun makes owned quantity, selected consumption and Retry
  re-consumption understandable, and that shortage/save failure feedback does
  not look like a successful Start.
- Judge whether the free Stage 6/7 lessons make Stage 8's first inventory use
  feel expected and fair. Automation does not determine product value or UX
  clarity.

## Iteration 19 - Continue Economy and Attempt Policy

### Focused acceptance

- Core permits exactly three successful Continues per Attempt, increments only
  on valid resume and resets on Retry while preserving elapsed time, speed,
  color, sequence cursor and failed-gate resolution invariants each time.
- Coin Continue uses 300/600/900 prices by successful Coin ordinal. A completed
  rewarded ad is independently available once per Attempt; mixed source order
  preserves the unused right and expected next Coin price.
- Coin spend is atomic and transaction-idempotent across duplicate calls,
  reload and save-failure retry. Insufficient funds and failed persistence do
  not publish a balance or transaction change.
- Missing release ad service hides rewarded Continue. Failed, cancelled,
  unavailable and stale ad results do not resume or consume the ad right.
- Simulation reports total, average, maximum and 0/1/2/3 Continue-use histogram
  values; disabling Continue reports zero uses deterministically.

### Final automated evidence

- Campaign Builder completed two consecutive passes and structural validation.
- Full EditMode: `400/400` passed.
- Final post-Builder PlayMode: `215/215` passed.
- Two 400-row Campaign simulation runs were byte-identical. Approved hashes:
  - Summary:
    `BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`
  - JSON:
    `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`
  - CSV:
    `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`
- Step 10 definitions, inputs and approved artifacts remained unchanged.

### Human review

- Confirm Coin cost escalation, independent ad right, failure feedback, cap and
  Retry reset read clearly on portrait Android and WebGL.
- Automation proves state, persistence and deterministic contracts, not whether
  Continue pricing feels fair or whether an ad offer is desirable.

## Iteration 20 — Monetization Platform Baseline

- Full final EditMode: `400/400` passed.
- Full final PlayMode: `215/215` passed.
- Compilation included Unity IAP `5.4.2` and Unity Services Core `1.18.0`.
- The first sandboxed attempt could not reach Unity's licensing IPC and was
  terminated without test evidence. The approved external validation run
  connected successfully and produced both non-zero result files.
- Builders were omitted because no generated Scene/UI changed. Campaign and
  Step 10 simulations were omitted because gameplay, content, timing,
  generation and simulation inputs were unchanged.
- Final manifest, lock and ProjectSettings hashes are owned by the active
  baseline in `CURRENT_STATUS.md`.
- Actual Editor Product save: schema 2, revision 29, Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, Highest Stage 14, 13 records, 1,500
  Coins, 3 Shields, 3 Boosters; SHA-256
  `9850956C9219B35ABD6DC5049BE59FA48309A3A17CF72B7CA158986EDA85153E`.
  Its last-write time was earlier than the two final validation processes.

## Iteration 21 — Local Commerce Rewards and Hearts

- Campaign Builder completed two consecutive passes with the generated
  Ticket-first failure action and required-reference validation.
- Full EditMode passed `408/408`; full post-Builder PlayMode passed `218/218`.
- New coverage includes schema migration, exact catalog rewards, atomic
  Heart/item start, recharge/rollback and unlimited policy, idempotent commerce
  grants, Ticket spending, empty-Heart UX and frozen Ticket-failure UX.
- Actual Editor Product save remained schema 2 revision 29 with identical
  length, timestamp and SHA-256
  `9850956C9219B35ABD6DC5049BE59FA48309A3A17CF72B7CA158986EDA85153E`.
- Campaign and Step 10 simulations were omitted because Core gameplay, Stage
  data, timing, generation and deterministic inputs did not change.

## Iteration 22 — Editor Developer Console

- Full EditMode: `414/414` passed.
- Full PlayMode: `218/218` passed.
- No Builder-owned Scene changed; Builders were omitted. No Core, Catalog,
  timing, generation or simulation input changed; Campaign and Step 10 were
  omitted and retain their prior approved hashes.
- Actual developer save remained schema 3 revision 30, Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, SHA-256
  `77AF3ED2CFBB13591E2119FD77F94C68A5BFC488CF4DB62C2FDCC52C8B443B50`.

## Iteration 23 - Continue and Clear Economy Flow

### Focused acceptance

- Continue has no Attempt cap. Successful Coin uses follow
  900/1,900/2,900/4,900-repeat; ad and Ticket resumes preserve the Coin
  ordinal, and Retry resets it.
- Continue countdown presents active modifiers from its first frame and never
  re-shows consumed Attempt buffs.
- Failure shows Coin and Heart balances. Insufficient Coins open a truthful
  popup and leave the failed Attempt frozen without a spend.
- Campaign clear hides Replay and opens the next Stage PreRun directly. The
  final Stage retains Lobby only; Experiment Replay remains unchanged.
- A normally consumed Heart refunds on clear exactly once. Free or unlimited
  starts cannot create a refund, and save failure publishes no partial refund,
  record, unlock or reward.
- First-clear base rewards are Normal 100, Hard 200 and Very Hard 500 Coins.
  Stages 11/14/17 are Hard and Stage 20 is Very Hard; Lobby milestones remain
  separately additive.

### Final automated evidence

- Campaign Builder completed two consecutive passes with structural,
  required-reference, unique-root, EventSystem and Build Settings validation.
- Full EditMode: `418/418` passed.
- Final post-Builder PlayMode: `222/222` passed.
- Two full Campaign simulation runs were byte-identical. Approved hashes:
  - Summary:
    `698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`
  - JSON:
    `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`
  - CSV:
    `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`
- Step 10 definitions, inputs and approved artifacts remained unchanged.
- Validation compared against base commit `7858803`.

### Human review

- Confirm on portrait Android and WebGL that failure balances, shortage,
  repeated 4,900 pricing, Continue countdown continuity, clear reward breakdown
  and direct next-Stage flow are understandable.
- Automation verifies state and persistence contracts, not pricing fairness,
  reward satisfaction or visual polish.

## Iteration 24 — Continue Gate-Pool Recycle Hotfix

- Base HEAD: `61147cd`
- Focused Stage 11 late-Continue regression: `1/1`
- Full EditMode: `418/418`
- Full PlayMode: `223/223`
- Builder: omitted; no Scene or Builder changed.
- Campaign/Step 10: omitted; no Core, Catalog, generation, timing, balance,
  judgment, seed or simulation input changed. Iteration 23 hashes preserved.
- Product save and backup: byte-identical to the captured pre-test snapshots.

## Iteration 25 — Timed Fog Curtain

- Base HEAD: `a8f7892`
- Campaign Builder: two consecutive passes with one required Fog curtain.
- Full EditMode: `421/421`
- Full post-Builder PlayMode: `225/225`
- Campaign/Step 10 simulations: omitted because Core, Catalog, gate plans,
  generator, judgment, seed, balance and simulation inputs are unchanged.
- Package hashes and non-volatile ProjectSettings semantics remained unchanged.
- Product save and backup hashes, byte lengths and timestamps were identical
  to the pre-test snapshot.

## Iteration 26 — Fog Visibility Curve

- Base HEAD: `74a9953`
- Campaign Builder: two consecutive passes with Stage Catalog revision 9.
- Full EditMode: `423/423`
- Full post-Builder PlayMode: `225/225`
- Two Campaign simulations: 400 rows each, byte-identical Summary / JSON / CSV
  hashes equal to the Iteration 23 baseline.
- Step 10: omitted because Experiment inputs and behavior are unchanged.
- Package and ProjectSettings baselines were preserved. Product save and backup
  remained byte- and timestamp-identical to their pre-test snapshots.

## Iteration 27 — Preplaced Ice Runway

- Base HEAD: `4c3ce13`.
- Campaign Builder: two consecutive successful passes with Stage Catalog
  revision 10, exactly one 50-panel Ice runway, one valid Fog View, fixed
  six-slot Gate/Track pools, unique roots, references, EventSystem and Build
  Settings validation.
- Focused Ice EditMode: `8/8`; focused Ice PlayMode: `4/4`.
- Full EditMode: `425/425`; final post-Builder PlayMode: `226/226`.
- Campaign simulation: two 400-row runs, byte-identical. SHA-256:
  - Summary: `86D55FB085FE51135BFA5E0F5D17D0242166C1F9DBD3EE03F58485A33BFC27B8`
  - JSON: `3CE081477E7A431939D4C8AB6D0140FFBB6C22D6A6850E7551499E4C28BC28C2`
  - CSV: `9D6E7903B31D59C3307DBE49A1E1FCF705A299FE37571299F17FAD63DF464B95`
- Booster/Continue displacement, index-gap, duplicate-index, cursor-reset and
  full-pool-reset counters are all zero. Step 10 was omitted because
  Experiment Ice inputs and behavior remain at `1.45`.
- Product save remained schema 3 revision 202, 13,580 bytes, timestamp- and
  SHA-identical at
  `49217BC97CF2C38CE11DA6052EEED5788F7DA002BF2A08F51CDB43F2002ADB6F`.
