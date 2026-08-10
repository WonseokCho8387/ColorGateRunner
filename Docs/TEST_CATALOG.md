# Test Catalog

## How to use this catalog

This is a search index for the current automated tests and structural
validation entry points. Test method names are read from the current
repository fixtures; historical execution counts and artifact hashes are
intentionally excluded.

- Choose validation scope from `TEST_PLAN.md`.
- Search this file by feature term, fixture name, or test method name.
- Use `CURRENT_STATUS.md` for the latest approved execution evidence.
- Use `TEST_HISTORY.md` for former Step/Iteration evidence.
- The test source remains authoritative if this index becomes stale.

## EditMode automated

### Gameplay Core and deterministic sequencing

#### `GameSessionTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/GameSessionTests.cs`

- `NewSession_IsReady`
- `NewSession_ColorIsRed`
- `NewSession_ScoreIsZero`
- `NewSession_UsesUpdatedBaseSpeed`
- `Ready_StartsPlaying`
- `StartRun_DoesNotToggleInitialColor`
- `Red_TogglesToBlue`
- `Blue_TogglesToRed`
- `Toggle_WhenReady_IsIgnored`
- `Toggle_WhenDead_IsIgnored`
- `MatchingGate_AddsExactlyOne`
- `MatchingGate_KeepsSessionPlaying`
- `MismatchingGate_AddsNothing`
- `MismatchingGate_ChangesStateToDead`
- `GateResolution_WhenNotPlaying_IsIgnored`
- `Speed_IncreasesWithScore`
- `Speed_NeverExceedsFifteen`
- `Advance_WhilePlaying_IncreasesElapsedTime`
- `Advance_WhileReady_DoesNotIncreaseElapsedTime`
- `Advance_WhileDead_DoesNotIncreaseElapsedTime`
- `Advance_NegativeDelta_IsRejected`
- `Speed_IncreasesWithElapsedTime`
- `NonlinearSpeed_HasMeasuredProgression`
- `NonlinearSpeed_EarlyAccelerationExceedsLateAcceleration`
- `MovementSpeed_ContinuesIncreasingAfterTenSeconds`
- `MaximumSpeed_IsReachedOnlyAtLateStage`
- `EncounterInterval_DecreasesAcrossProgression`
- `Shield_AutomaticScoreGrantIsRemoved`
- `ShieldPickup_ActivatesAndProtectsExactlyOneMismatch`
- `Restart_ResetsShield`
- `Shield_MatchingWhileActiveDoesNotCreateASecondCharge`
- `ShieldBreak_EntersRecoveryAndReducesSpeed`
- `ShieldRecovery_MismatchIsIgnoredWithoutScore`
- `ShieldRecovery_ExpiresDeterministicallyAndSpeedRecovers`
- `Restart_ClearsShieldRecovery`
- `Countdown_DoesNotAdvanceTimeAndCompletesOnce`
- `ThirdColor_IsInactiveBeforeMilestone`
- `ThirdColor_IntroducesExactlyOnceAtMilestone`
- `ThirdColor_TutorialGatesFollowIntroduction`
- `ThreeColorCycle_OrderIsStable`
- `Restart_ResetsThirdColorIntroduction`
- `SpeedPresentation_StagesIncreaseVisualOutputs`
- `SameAdvanceSequence_ProducesSameSpeed`
- `Restart_ReturnsReady`
- `Restart_ResetsColorToRed`
- `Restart_ResetsScore`
- `Restart_ResetsSpeed`
- `Restart_ResetsElapsedTime`
- `Restart_ReplaysSequence`
- `Restart_ReplaysColorAndSpacingSequence`

#### `DeterministicGateSequenceTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/DeterministicGateSequenceTests.cs`

- `Seed12345_MatchesIndependentReference`
- `GeneratedSequence_RunLengthNeverExceedsFour`
- `SameSeed_ProducesSameSequence`
- `SeedZero_UsesDocumentedFallback`
- `SpacingSequence_AlwaysMeetsReactionTimeMinimum`
- `SpacingSequence_IsDeterministic`
- `SpacingSequence_DifferentSeedProducesDifferentSequence`
- `SpacingSequence_UsesThreeBandsWithoutThreeIdenticalGaps`
- `CoreAssembly_DoesNotReferenceUnityEngine`

#### `GatePatternSequenceTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/GatePatternSequenceTests.cs`

- `PatternSequence_SameSeedReplaysIdentically`
- `PatternSequence_DifferentSeedChangesPlans`
- `PatternSequence_UsesDistinctFairTimingAndNoImmediateRepeat`
- `PatternSequence_ColorRunNeverExceedsFour`
- `SameColorBait_ContainsOneReadableException`
- `PatternSequence_DiagnosticReportsFirstThirtyAndDistribution`
- `PatternSequence_EncounterCadenceDecreasesFromEarlyToLate`
- `PatternSequence_DoesNotRepeatEitherRecentPattern`
- `PatternSequence_OnlyUsesActiveColors`
- `ThreeColorSequence_NeverRequiresMoreThanOneTap`
- `ShieldPickupPlan_IsDeterministicAndAppearsInEarlyRange`
- `PatternSequence_DiagnosticReportsFirstFortyCadenceValues`

### Campaign catalog, stage runtime, and mechanics

#### `StageCatalogArchitectureTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/StageCatalogArchitectureTests.cs`

- `ResourceAsset_BuildsExpandedCatalogAndPreservesLegacyStages`
- `SpeedProfile_UsesDeterministicSampleInterpolation`
- `LinearSpeedProfile_PreservesLegacyFloatOperationOrder`
- `InMemoryCatalog_ResolvesDisplayNumberWithoutIndexAssumption`

#### `StageSessionTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/StageSessionTests.cs`

- `StageCatalog_ContainsThirteenValidStages`
- `StageIds_AreUnique`
- `StageNumbers_AreSequential`
- `StagesOneToThree_DoNotAllowGreen`
- `StagesFourAndFive_AllowGreen`
- `EveryStage_HasPositiveTargetAndFinalSection`
- `Booster_IsShorterThanEstimatedStageDistance`
- `NoSelection_InitializesWithoutItems`
- `ShieldSelection_ActivatesAfterCountdown`
- `BoosterSelection_ActivatesAfterCountdown`
- `BoosterBypass_DoesNotConsumeShield`
- `BoosterEnd_UsesSpeedAtCurrentStageProgress`
- `FinalGate_EntersStageFinishing`
- `Goal_ClearsOnlyOnce`
- `StageFinishing_IgnoresFailureJudgment`
- `Clear_UnlocksOnlyNextStage`
- `BetterLowerTime_ReplacesBestTime`
- `BoosterClear_DoesNotOverwriteNoItemBest`
- `CorruptPersistence_FallsBackSafely`
- `Persistence_RoundTrips`
- `StageSequence_IsDeterministic`
- `StageFour_IntroducesGreenAtConfiguredGate`
- `Retry_RetainsSelectedItemsAndReplaysLayout`
- `NegativeAdvance_IsRejected`
- `Countdown_DoesNotAdvanceElapsedTime`

#### `CampaignMechanicStageTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/CampaignMechanicStageTests.cs`

- `StagesSixThroughThirteen_UseOnePrimaryMechanicEach`
- `ShieldStage_ProvidesLocalChargeAndDisablesDuplicateItem`
- `BoosterStage_ProvidesLocalChargeAtStageStart`
- `StagesOneThroughFive_RejectAllStartItems`
- `GateModifiers_AreDeterministicAndStayInsideAuthoredStages`
- `Ice_UsesSharedSpeedAndSpacingMultipliers`
- `EchoStage_AcquiresConsumesAndRestartsDeterministically`
- `AuthoredSpeedCurves_AreSampledForDeterministicCoreUse`

#### `CampaignHiddenFlickerStageTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/CampaignHiddenFlickerStageTests.cs`

- `Catalog_ContainsApprovedHiddenAndFlickerStages`
- `Plans_AreDeterministicIsolatedAndDoNotChangeGateCount`
- `FlickerPlans_UsePlayerOrderAndMeetBoosterExposure`
- `FlickerJudgment_UsesExactCampaignGameplayTimeBoundary`
- `FlickerStage_BoosterUsesExistingAutoPassAndPreservesShield`
- `Retry_ReproducesCampaignTargetsCyclesAndPhases`

### Hidden, Flicker, Clone, Echo, and ETA

#### `HiddenModifierTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/HiddenModifierTests.cs`

- `HiddenMigration_PreservesFormerFlickerEnumValues`
- `HiddenMigration_FormerSerializedNamesMapOnlyToHidden`
- `HiddenMigration_PreservesFormerFlickerSeedSelections`
- `HiddenSettings_DefaultsMatchApprovedContract`
- `HiddenSettings_RejectInvalidValues`
- `HiddenSelection_IsDeterministicAndRespectsBoundsCooldownAndMaximum`
- `FirstOccurrenceGuaranteed_SelectsOneWhenChanceIsZero`
- `NoEligibleGate_IsHandledWithoutFallbackSelection`
- `HiddenSelection_DoesNotIncreaseGateCountOrMixModifiers`
- `HiddenVisibility_RequiresObservationAndEtaThenNeverReappears`
- `HiddenVisibility_ResetClearsRuntimeState`
- `HiddenPlan_UsesOrdinaryPlayerShieldAndFailurePriority`
- `HeldEcho_ResolvesHiddenWithoutConsumingShield`

#### `FlickerModifierTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/FlickerModifierTests.cs`

- `FlickerSettings_DefaultsMatchApprovedContract`
- `FlickerLauncher_HasNoAuthoredCycleCountSetting`
- `FlickerSettings_RejectInvalidValues`
- `FlickerPlanning_IsDeterministicAndRespectsSelectionRules`
- `FlickerPlanning_UsesFullPaletteInPlayerInputOrder`
- `ExperimentPlayerInput_UsesTheSamePaletteOrder`
- `FlickerPlanning_FirstOccurrenceGuaranteeAndNoFallbackAreExplicit`
- `FlickerPlanning_DoesNotIncreaseGateCountOrMixModifiers`
- `FlickerCycle_TwoColorsUseExactBoundaryAndRepeat`
- `FlickerCycle_ThreeColorsUseEveryColorInOrder`
- `FlickerCycle_PhaseOffsetAndPulseAreDeterministic`
- `FlickerCycle_RandomizePhaseDisabledUsesZero`
- `FlickerJudgment_UsesCollisionTimeColorInsteadOfBaseColor`
- `FlickerJudgment_UsesEchoBeforeShield`
- `FlickerJudgment_UsesShieldThenOrdinaryFailure`
- `FlickerRetry_ResetsGameplayTimeAndReplaysPlans`
- `FlickerSimulation_UsesCollisionTimeCycleCalculation`

#### `Iteration3CloneTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Iteration3CloneTests.cs`

- `EchoExperiment_DoesNotIncreaseGateCount`
- `EchoExperiment_FirstProviderIsDeterministic`
- `ProviderAcquisition_RequiresDirectPlayerMatch`
- `ProviderAcquisition_StoresEffectiveColor`
- `ResolutionPriority_PlayerThenEchoThenShield`
- `EchoUse_DoesNotIncreaseCompletionRequirement`
- `Retry_ClearsEchoStateAndReproducesProvider`
- `EchoProviderCanAlsoCarryCamouflageWithoutColorLeak`

#### `EchoOfferCoordinatorTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/EchoOfferCoordinatorTests.cs`

- `Settings_RejectInvalidRanges`
- `FirstOffer_IsDeterministicAndGuaranteed`
- `PendingOrActiveEcho_BlocksAdditionalOffers`
- `AcquireRequiresPlayerMatch_AndStoresEffectiveColor`
- `EchoConsumption_DoesNotConsumeOnDifferentColor`
- `ProviderRole_IsLockedUntilGateRelease`
- `GoalNeverBecomesEchoProvider`
- `Restart_ReproducesOfferAndClearsState`

#### `CamouflageEtaTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/CamouflageEtaTests.cs`

- `FasterSpeed_RevealsFromFartherDistanceAtSameLeadTime`
- `Booster_RevealsEarlierInDistanceThanNormalSpeed`
- `IceEffectiveSpeed_IsIncludedInEta`
- `StoppedFlow_DoesNotRevealDistantGate`

### Experiment Lab, simulation policy, and mobile UI

#### `Step9ACoreTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Step9ACoreTests.cs`

- `Lobby_SelectsLowestUnlockedUnclearedStage`
- `Lobby_AllClearedKeepsFinalStage`
- `LobbyVisualTier_MapsProgression`
- `TwoColorCycle_OrderAndNextColorAreStable`
- `ThreeColorCycle_OrderAndNextColorAreStable`
- `Continue_IsAvailableOnlyOncePerAttempt`
- `Continue_DoesNotRestoreConsumedShieldOrBooster`
- `Continue_GrantsSafeResumeSequenceAndProtection`
- `ContinuedClear_DoesNotReplaceBestRecords`
- `PostBooster_FirstGateMatchesAndSecondNeedsAtMostOneTap`
- `StageTwo_HasPerceptibleAuthoredCadenceSections`
- `StageThree_HasAuthoredExceptionSections`
- `SimulationProfiles_AreExplicitAndValid`
- `PerfectSimulation_IsDeterministicAndCompletes`
- `StochasticSimulation_ReplaysWithSameSeed`
- `SimulationReport_ContainsRequiredMechanicalSections`

#### `Step9A1ContinuityTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Step9A1ContinuityTests.cs`

- `StageCatalog_UsesIteration2DoubleSpeedScale`
- `BoosterExit_PreservesSequenceCursor`
- `BoosterExit_PreservesGateIndices`
- `BoosterExit_OverridesColorWithoutChangingPlannedPosition`
- `BoosterExit_FirstGateMatchesCurrentColor`
- `BoosterExit_SecondGateRequiresAtMostOneTap`
- `BoosterExit_ThirdGateReturnsToAuthoredSequence`
- `Continue_PreservesStateAndConsumesFailedGate`
- `Continue_PreservesElapsedTime`
- `Continue_AdvancesProgressPastConsumedFailedGate`
- `Continue_PreservesCurrentSpeed`
- `Continue_PreservesCurrentColor`
- `Continue_PreservesPendingSequenceCursor`
- `Continue_FailedGateIsResolvedExactlyOnce`
- `Continue_DoesNotRestoreShield`
- `Continue_DoesNotRestartBooster`
- `Continue_FinalSectionStateSurvives`
- `Simulation_ContinuityMetricsReportNoReset`

#### `Step9CUiPolicyTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Step9CUiPolicyTests.cs`

- `TwoColorHud_UsesStageOrder`
- `ThreeColorHud_UsesStageOrderAfterIntroduction`
- `StageFour_ShowsCompleteColorCycleFromStart`
- `StageFive_ShowsThreeTilesImmediately`
- `NextColor_FollowsActiveCycle`
- `CurrentColor_RemainsAnActiveTile`
- `VerticalStack_OrderStartsWithCurrentColor`
- `RequiredTapCount_UsesForwardCycle`
- `LobbyTierMapping_UsesExistingProgressionThresholds`
- `PrimaryFlow_ActivatesExactlyOneRoot`
- `Countdown_UsesGameplayHudWithCountdownOverlay`
- `BoosterMeter_OnlyAppearsDuringActiveGameplay`
- `ItemIcon_OnlyAppearsWhenRelevant`
- `ColorSymbolMapping_IsStable`

#### `Step10ExperimentTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Step10ExperimentTests.cs`

- `SixColorIdentityAndSymbolMapping_IsStable`
- `ColorCycle_IsCorrectForTwoThroughSixColors`
- `ExperimentDefinitions_RemainDeterministic`
- `ExperimentRestart_ReplaysPlansAndColor`
- `Camouflage_RevealsAtConfiguredEta`
- `Fog_ExposesExactlyTwoNearestUnpassedGates`
- `Fog_DoesNotChangeGatePlans`
- `Ice_ChangesSpeedButPreservesColorJudgment`
- `Ice_ExitRestoresCorrectNormalSpeed`
- `RepeatedTapPlayerProfiles_AreValid`
- `TapWindowMetrics_HandleZeroAndRepeatedTaps`
- `RiskClassification_AppliesDocumentedThresholds`
- `ConditionRisk_CombinesAverageAndExpertHardExclusions`
- `SixColorBoundary_IsHighRiskAndBoundaryOnly`
- `ShortlistGeneration_IsDeterministic`

#### `Step10_1CoreTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Step10_1CoreTests.cs`

- `Stage4_ActiveColorsAreCompleteAtRunStart`
- `Stage4_GreenGatesBeginAtConfiguredLaterIndex`
- `Stage4_ColorCycleDoesNotMutateMidRun`
- `Stage4_EarlyTutorialPrimarilyRequiresZeroOrOneTap`
- `Stage4_PerfectFirstAttemptUsesRuntimeColorCycle`
- `Stage4_SharedTapCalculationMatchesRuntimeCycle`
- `ExperimentCountdown_BlocksInputProgressAndJudgment`
- `ExperimentCountdown_CompletesOnceIntoPlaying`
- `ExperimentMismatch_EntersFailedOnce`
- `ExperimentFinalGate_EntersCompletedOnce`
- `ExperimentRestart_PreservesConditionAndResetsRuntime`
- `Goal_IsPartOfDeterministicStagePlanning`
- `Continue_ConsumesFailedGateImmediately`
- `Continue_SelectsGateAfterConsumedFailure`
- `ExperimentLauncherOptions_MapToExistingDefinitions`

### Product Save, Frontend, Lobby, and Pause

#### `ProductFoundationTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/ProductFoundationTests.cs`

- `ProductAssembly_RemainsUnityIndependent`
- `FreshInitialization_CreatesOneStableGuestAndOrderedSteps`
- `InitializationFailure_StopsAndRetryUsesSameServices`
- `SaveRoundtrip_PreservesProfileSettingsAndIncrementsRevision`
- `Save_PreparesRevisionAndUtcBeforeTempSerialization`
- `FailedReplacement_PreservesPreviousPrimary`
- `CorruptPrimary_RecoversValidBackup`
- `BothCorrupt_ProducesTypedBlockingError`
- `FutureSchema_BlocksWithoutFallbackOrOverwrite`
- `SupportedSchemaZero_MigratesDeterministically`
- `InvalidSettings_ResetToDocumentedDefaults`
- `SaveFiles_StayInsideInjectedDirectory`
- `RecoverablePolicy_DoesNotClaimAtomicReplacement`
- `AccountLink_IsExplicitlyUnsupported`
- `SchemaOneWithoutAccountChoiceField_DefaultsToIncomplete`
- `SchemaOneWithoutUnifiedSettingsFields_UsesSafeDefaults`
- `InvalidSettingsReset_DoesNotResetAccountChoice`
- `ProductSession_SaveFailureLeavesAllPublishedStateUntouched`
- `ProductSession_SuccessRebindsOnceAndPersistsClampedSettings`
- `ProductSession_DisablingVolumesPreservesLastNonZeroValues`
- `ProductSession_AccountAndSettingsSurviveSaveReload`
- `SchemaOneLoad_UpgradesProgressionEconomyAndLobbyDefaults`
- `LegacyCampaignImport_IsAtomicIdempotentAndBackfillsRewards`
- `StageClear_FirstClearRewardsOnceAndAcknowledgesLobby`
- `StageClear_SaveFailureDoesNotPublishPartialProgression`

#### `FrontendContextTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/FrontendContextTests.cs`

- `GuestProfileAndSettings_BindToImmutableDisplayContext`
- `MissingProfile_ProducesBootRequiredError`
- `MissingOptionalSettings_HidesSettingsFeature`

#### `FrontendPageRouterTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/FrontendPageRouterTests.cs`

- `InitialPage_IsAccountChoice`
- `ShowPages_ActivatesOneLogicalPageAndSamePageIsSafe`
- `Transition_RejectsDuplicateNavigationAndBack`
- `TransitionBlocker_ClearsAfterCompletion`
- `BackFromLobby_RequestsExitConfirmation`
- `Modal_ConsumesBackBeforePageNavigation`
- `BootRequiredModal_IsBlocking`
- `AccountChoiceBack_RequestsExitConfirmation`
- `InvalidPage_IsRejectedExplicitly`

#### `LobbyConsolidationEditModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/LobbyConsolidationEditModeTests.cs`

- `FrontendReader_ReusesLobbyProgressionAndReturnsStableStageId`
- `FrontendReader_WhenUnlockedStagesAreClearedSelectsHighest`
- `LaunchContext_IsNonPersistentOneShotAndCancellationIsExact`
- `ProductLinks_AllowOnlyAbsoluteHttps`

#### `GameplayPauseCoordinatorTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/GameplayPauseCoordinatorTests.cs`

- `PauseAvailability_MatchesAttemptStates`
- `PauseResumeAndRepeatedRequests_AreIdempotent`
- `Back_ClosesModalThenResumesThenPauses`
- `Transition_BlocksResumeUntilCompletionAndReportsFailureModal`
- `BoosterHaptic_DoesNotInvokeDeviceWhenVibrationIsDisabled`

### Builders and test-build tooling

#### `BootSceneBuilderTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/BootSceneBuilderTests.cs`

- `Destination_IsFirstActiveNonBootScene`
- `Destination_DoesNotDependOnSampleSceneName`
- `Destination_RejectsBootOnlyConfiguration`
- `BuildSceneList_PutsOneActiveBootAtIndexZero`
- `SerializedDestination_RemainsExplicitWhenOrderChanges`
- `ExplicitDestination_AcceptsActiveFrontendScene`
- `ExplicitDestination_RejectsInvalidScene`

#### `FrontendSceneBuilderTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/FrontendSceneBuilderTests.cs`

- `CampaignSelection_UsesFirstActiveNonBootNonFrontendScene`
- `CampaignSelection_HasNoSampleSceneNameDependency`
- `BuildSceneList_IsExactlyBootFrontendCampaign`
- `BuildSceneList_RejectsInvalidCampaign`
- `CampaignBuilder_SelectsSerializedFrontendWithoutNameHardcode`
- `CampaignBuilder_RejectsBootOrSelfAsFrontend`

#### `TestBuildMenuTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/TestBuildMenuTests.cs`

- `EnabledBuildScenes_AreBootFrontendCampaign`
- `BuildOptions_UseDevelopmentModeAndExpectedOutput`
- `WebGlPortraitTemplate_UsesExactNineBySixteenFrame`
- `WebGlPortraitSettings_ApplyAndRestoreEditorValues`

## PlayMode automated

### Campaign flow, generated Scene, and persistence isolation

#### `GrayboxScenePlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/GrayboxScenePlayModeTests.cs`

- `NormalFlow_OpensLobbyRatherThanStageSelect`
- `Play_OpensItemSelection`
- `DirectCampaignPreRunBack_ReturnsToCampaignLobby`
- `StagesOneThroughFive_ShowLockedItemsAndRejectToggles`
- `DebugStagePicker_RemainsHidden`
- `ColorHud_UpdatesCurrentAndNextAfterTap`
- `ThreeColorHud_ShowsFullOrder`
- `Shield_DoesNotHidePlayerMaterialStructurally`
- `Shield_AppearsDuringCountdown`
- `ShieldBreak_EntersRecoveryAndRecoversPresentation`
- `Booster_LaunchRequestsHapticAndActivatesMeter`
- `Booster_BypassesMismatchedGate`
- `Booster_DoesNotConsumeShield`
- `Booster_WarningActivatesNearEnd`
- `Booster_CameraReturnsToExactBaseline`
- `Lobby_VerticalDecorationsAreAbsent`
- `VerticalStack_CurrentColorStaysAtTop`
- `VerticalStack_RapidTapsRetargetToAuthoritativeColor`
- `BoosterCamera_LowersAndMovesCloserBehindPlayer`
- `ResetProgress_CancellationChangesNothing`
- `ResetProgress_ConfirmationReturnsLobbyToStageOne`
- `VerticalStack_HasSixReusableSlots`
- `ExperimentLauncher_IsHiddenFromNormalFlow`
- `ExperimentPlay_DoesNotChangeNormalProgression`
- `ExperimentLauncher_StartsCountdownWithFixedPoolSession`
- `ExperimentColorStack_SupportsThreeThroughSixColors`
- `ExperimentRapidTaps_EndAtCorrectCurrentTile`
- `IceGatePositions_RemainContinuousDuringMovement`
- `Camouflage_RemainsNeutralThenRevealsWithoutMoving`
- `Fog_KeepsExactlyTwoGatesFullyReadableWithoutPoolGrowth`
- `Ice_ChangesFloorPresentationAndKeepsColorJudgment`
- `RepeatedExperimentStarts_CreateNoDuplicates`
- `FailureCameraShake_RestoresOriginalPositionAndRotation`
- `PostBooster_FirstGateMatchesCurrentColor`
- `Gate_HasLeftRightAndTopParts`
- `BoosterGateDestruction_ResetsAfterPooling`
- `NormalGate_RemainsIntact`
- `FailureAnimation_PrecedesFailurePanel`
- `Failure_StopsMovementImmediately`
- `ClearAnimation_PrecedesClearPanel`
- `ClearAndFailurePanels_HaveDistinctRootsAndHierarchy`
- `FinalGate_RevealsGoalAndGoalCrossingClearsStage`
- `Continue_BypassesItemSelectionAndStartsOneCountdown`
- `Continue_GrantsTemporaryProtection`
- `SecondFailure_RemovesContinueOption`
- `Retry_StillOpensItemSelection`
- `Retry_RetainsItemSelection`
- `Restart_ReplaysIdenticalGateLayout`
- `StageClear_PersistsRecordAndUnlock`
- `ContinuedClear_UnlocksButDoesNotReplaceBestRecords`
- `ClearContinue_ReturnsToUpdatedLobby`
- `LobbyPlay_UsesStableIdOfDisplayedProgressionStage`
- `LobbyProgression_ChangesAtMilestones`
- `ThirtyRepeatedFlows_LeaveNoDuplicateOrStaleVisuals`
- `EveryStage_AllItemCombinationsCanInitialize`
- `StageSeven_StartBoosterCrossesEveryGateAndReachesGoal`
- `StageSelectUi_HasOneButtonPerCatalogEntry`
- `CampaignCamouflageGate_RevealsFromEtaAndStaysJudged`
- `CampaignEchoProvider_ActivatesColoredPlayerShell`
- `CampaignHiddenGate_HidesAndUsesOrdinaryFailureFlow`
- `CampaignFlickerGate_UsesGameplayTimeForViewAndJudgment`
- `CampaignFlickerStage_AllowsShieldAndBoosterFromGo`
- `RuntimeGateAndTrackPools_DoNotGrow`
- `SafeArea_CalculatesNormalizedAnchorsAndContainsPlayerUi`
- `RequiredReferencesAndPools_AreStable`
- `Scene_HasNoMissingMonoBehaviours`
- `Lobby_ContainsNoGameplayHudElements`
- `Lobby_ContainsNoBaseOrUpgradeButtons`
- `Lobby_HasOneDominantPlayButton`
- `Gameplay_HidesLobbyRoot`
- `Gameplay_ShowsOneColorHud`
- `CurrentColorHud_UpdatesAfterTap`
- `TwoColorStage_ShowsExactlyTwoColorTiles`
- `ThreeColorStage_ShowsExactlyThreeColorTiles`
- `BoosterBar_IsHiddenBeforeBooster`
- `BoosterBar_IsVisibleOnlyDuringBooster`
- `BlueHorizontalCenterBar_NoLongerExists`
- `NormalGameplay_HasNoCentralSpeedLineObstruction`
- `BoosterEffects_ClearAfterEnding`
- `Continue_RestoresCorrectHudState`
- `Retry_RestoresCorrectHudState`
- `ReferenceResolutions_PassOverlapBounds`
- `PlayerUi_RemainsInsideSimulatedNotchSafeArea`
- `SceneBuilderRerun_CreatesNoDuplicateRoots`
- `PauseDuringCountdown_FreezesCountdownUntilResume`
- `PauseButton_IsVisibleClickableInsideSafeAreaAndDoesNotOverlapHud`
- `PauseDuringPlaying_FreezesTimeMovementInputAndJudgment`
- `Pause_OnlyPausesRegisteredAttemptEffectAndResumesOnce`
- `PauseRestart_UsesExistingRetryPathAndClearsPause`
- `PauseLobbyLoadFailure_StaysPausedAndShowsError`
- `FocusLossPausesOnceAndFocusGainDoesNotAutoResume`
- `GameplayBackPausesAndPauseBackResumesSameAttempt`

#### `CampaignPlayerPrefsSnapshotTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/CampaignPlayerPrefsSnapshotTests.cs`

- `Restore_PreservesExistingIntAndStringAndDeletesNewKey`
- `Dispose_RestoresAllThirteenRecords`
- `Dispose_RestoresAfterException`

#### `CampaignProgressRestorePlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/CampaignProgressRestorePlayModeTests.cs`

- `ExistingStageElevenProgress_RestoresSameStableStage`
- `FreshCampaign_ClearAndReinitialize_RestoresStageTwo`
- `NewProductGuest_PreservesExistingCampaignProgress`
- `MissingOrCorruptHighest_FallsBackWithoutDeletingRecords`

### Boot, Frontend, Lobby, Settings, and Pause

#### `ProductBootPlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/ProductBootPlayModeTests.cs`

- `BootScene_HasRequiredStructureAndOneEventSystem`
- `SuccessfulBoot_LoadsProfileBeforeFrontendTransition`
- `Retry_ReusesGraphHidesErrorAndDoesNotDuplicateListener`
- `ReturningToBoot_DoesNotCreateSecondServiceGraph`
- `BootAndGuestCreation_DoNotModifyStageProgressKeys`

#### `FrontendFlowPlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/FrontendFlowPlayModeTests.cs`

- `BootToFrontend_StartsAtAccountChoiceWithOneAppRootAndEventSystem`
- `GuestChoicePersistsThenLobbyBackRequestsExit`
- `LobbyProgression_ShowsEconomyThemeAndAcknowledgesReward`
- `CompletedAccountChoice_StartsDirectlyAtLobby`
- `AccountChoiceSaveFailure_RemainsOnChoicePage`
- `SharedSettings_SaveTogglesAndPreserveMasterVolume`
- `AccountAndSettingsModal_AreTruthfulAndConsumeBack`
- `TitleBack_RequestsExitOnlyAfterConfirmation`
- `DirectFrontendWithoutAppRoot_ShowsBootRequiredAndBlocksCampaign`
- `FailedCampaignLoad_ClearsBlockerAndShowsErrorOnce`
- `StartStage_BypassesCampaignLobbyAndPreservesProgress`
- `FrontendPreRunBack_ReturnsDirectlyToFrontendLobby`
- `FrontendPreRunBack_FailureStaysAndAllowsOneRetry`
- `FrontendStageClear_ReturnsToLobbyAndShowsReward`
- `TogglePresentation_MatchesAfterSaveReloadAndInPause`
- `FrontendReentry_DoesNotDuplicatePersistentOrSceneObjects`

### Hidden, Flicker, and Clone runtime

#### `HiddenPlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/HiddenPlayModeTests.cs`

- `HiddenLauncher_DisablesBoosterAndUsesFixedPool`
- `Countdown_DoesNotAdvanceHiddenObservation`
- `Pause_FreezesHiddenObservationAndHideTransition`
- `Hidden_HidesTargetOnlyAfterObservationAndEtaConditions`
- `Hidden_UsesOrdinaryShieldAndFailureFlows`
- `Hidden_UsesHeldEchoWithoutConsumingShield`
- `Retry_ReproducesHiddenSelectionAndResetsVisibility`

#### `FlickerPlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/FlickerPlayModeTests.cs`

- `FlickerLauncher_IsDistinctFromHiddenAndDisablesBooster`
- `Countdown_FreezesPhaseAndPlayingStartsPaletteCycle`
- `Pause_FreezesFlickerGameplayTimeAndDisplayedPhase`
- `ActivePaletteFlicker_UpdatesColorAndSymbolFromSamePhase`
- `FlickerBoundary_PulsesWithoutMovingOrDisablingGate`
- `Flicker_PlayerPassUsesExactGameplayTimeColor`
- `Flicker_EchoMatchConsumesEchoAndPreservesShield`
- `Flicker_ShieldAndFailureReuseOrdinaryFlow`
- `Retry_ReplaysCycleAndResetsGameplayPhase`
- `BackToLab_ReentryClearsPooledFlickerState`

#### `Iteration3ClonePlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/Iteration3ClonePlayModeTests.cs`

- `EchoLauncher_StartsFortyGateModifierCondition`
- `ProviderUsesOrdinaryGateAndShowsEchoMarker`
- `DirectMatchActivatesColoredEchoShell`
- `EchoConsumptionHidesShellWithoutUsingShield`
- `RetryClearsShellAndReproducesProvider`

### Continuity, Experiment Lab, and visual capture

#### `Step9A1PlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/Step9A1PlayModeTests.cs`

- `BoosterExit_ActiveGatePositionsDoNotJump`
- `BoosterExit_DoesNotIntroduceEmptyGateInterval`
- `BoosterExit_FirstGateUsesCurrentPlayerColor`
- `BoosterExit_OriginalPatternResumesAfterShortOverride`
- `Continue_FreezesTrackDuringCleanRespawn`
- `Continue_CountdownUsesSameGateLayout`
- `Continue_UnaffectedGateTransformsRemainUnchanged`
- `Continue_FailedGateCannotImmediatelyFailAgain`
- `Continue_ResumesSameTimerProgressSpeedColorAndCursor`
- `Continue_ResumesSameActiveSequence`
- `Continue_ClearsCameraShakeAndDoesNotShowItemSelection`
- `Continue_GateAndTrackPoolsDoNotResetOrGrow`
- `Continue_DoesNotRestoreShieldOrBooster`
- `ThirtyRepeatedBoosterContinueTransitions_HaveNoTransformDrift`

#### `Step10_1PlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/Step10_1PlayModeTests.cs`

- `Stage4_HudShowsThreeColorsImmediately`
- `Stage4_EarlyGatesDoNotUseGreen`
- `Stage4_LaterGateUsesGreen`
- `Goal_IsVisibleAtLongRangeAndApproachesContinuously`
- `Goal_DoesNotPopOrTeleportAtFinalGate`
- `Continue_ResetsPlayerToCleanUprightPose`
- `Continue_ConsumesFailedGateAndLeavesReadableNextGate`
- `ExperimentLab_OpensFromLobbyInDevelopmentFlow`
- `ExperimentLab_ShortlistedConditionsLaunchWithoutProgression`
- `ExperimentLab_IsHiddenForReleasePolicy`
- `ExperimentRuntime_CountdownBlocksProgressThenStartsPlaying`
- `ExperimentRuntime_FailureShowsResultAndRetryReplaysCondition`
- `ExperimentRuntime_CompletionShowsResultAndReplayRestarts`
- `ExperimentRuntime_BackToLabDoesNotReadOrWriteProgress`
- `ExperimentRuntime_ReentryKeepsSingleRuntimeObjectGraph`

#### `Step9CVisualCaptureTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/Step9CVisualCaptureTests.cs`

- `CaptureStep9CReferenceScreenshots`
- `CaptureStep10ReferenceScreenshots`

## Structural validation

### Test runner and compilation guard

- `Tools/Validate.ps1` enforces Unity compilation and non-zero test
  execution for requested suites.
- `Core` keeps `noEngineReferences` so UnityEngine references cannot
  enter the pure gameplay assembly.

### Builder and generation entry points

- `BootSceneBuilder.BuildBootSceneFromCommandLine`
- `FrontendSceneBuilder.BuildFrontendSceneFromCommandLine`
- `GrayboxSceneBuilder.BuildGrayboxSceneFromCommandLine`
- `GrayboxSceneBuilder.ValidateGeneratedScene`
- `StageCatalogAssetBuilder.CreateStageCatalogAssetFromCommandLine`
- `SimulationReportWriter.RunStep9ASimulationFromCommandLine`
- `ExperimentReportWriter.RunStep10ExperimentMatrixFromCommandLine`

### Required generated-structure checks

- Required serialized references are assigned.
- Generated Scenes contain no Missing Script or Missing Reference.
- Each Builder-owned structure has one expected generated root.
- Boot, Frontend, and Campaign generation is idempotent across two
  consecutive Builder passes.
- Build Settings contain unique enabled Boot, Frontend, and Campaign
  destinations in the approved order.
- EventSystem, AppRoot, router, overlay, controller, listener, and
  state-label counts match the active feature contract.
- Frontend and Campaign contain one `StateLabel` per Settings toggle
  and no legacy `OnLabel` or `OffLabel` objects.
- Portrait orientation and Safe Area contracts are valid.
- No active post-processing Volume exists and camera post-processing
  remains disabled.
- Gate and Track pools are fixed; repeated runtime and navigation do
  not grow pools or duplicate generated objects.
- UI input does not leak into gameplay.

### Persistence and deterministic artifact checks

- PlayMode fixtures that can touch progression snapshot and restore
  `ColorGateRunner.Stage.HighestUnlocked` and Stage Records 1–13.
- Product Save timestamp/content and Guest ID are preserved unless the
  approved change explicitly owns them.
- Campaign simulation artifacts are compared when Tier 3 or an affected
  gameplay execution boundary requires them.
- Step 10 artifacts are compared when Experiment contracts, shared
  mechanics, player profiles, or simulation inputs may be affected.
- Package manifest/lock and meaningful ProjectSettings state are checked
  against the Authoritative Baseline.

## References

- `TEST_PLAN.md` — validation policy and tier selection.
- `CURRENT_STATUS.md` — current counts, hashes, snapshots, and baseline.
- `TEST_HISTORY.md` — preserved historical mappings and evidence.
- `Docs/TEST_PLAN.md` in the source repository — original catalog source.
