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

- `StageCatalog_ContainsTwentyThreeValidStages`
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
- `CatalogExpansion_UnlocksOneNewStageWithoutChangingSave`
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

- `StagesSixThroughTwentyThree_UseApprovedLearningSequence`
- `ShieldStage_ProvidesLocalChargeWhileSelectionIsLocked`
- `BoosterStage_ProvidesLocalChargeAtStageStart`
- `StartItems_AreLockedThroughGrantTrainingAndUnlockAtStageEight`
- `GateModifiers_AreDeterministicAndStayInsideAuthoredStages`
- `Ice_UsesAuthoredCampaignSpeedAndSharedSpacingMultipliers`
- `EchoStage_AcquiresConsumesAndRestartsDeterministically`
- `AuthoredSpeedCurves_AreSampledForDeterministicCoreUse`
- `HiddenBlock_DistributesIncreasingPressureAcrossWholeStages`

#### `CampaignDeferredMechanicStageTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/CampaignDeferredMechanicStageTests.cs`

- `CampaignStagesOneThroughTwenty_DeferHiddenAndFlicker`
- `CampaignStagesTwentyOneThroughTwentyThree_EnableOnlyHidden`
- `ExperimentCatalog_RetainsHiddenAndFlickerImplementations`

#### `CampaignLearningCurveStageTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/CampaignLearningCurveStageTests.cs`

- `Catalog_ContainsApprovedLearningBlockCurve`
- `MechanicBlocks_ProgressFromTwoColorIntroToThreeColorMastery`
- `FogBlock_UsesApprovedVisibilityDurationCurve`
- `IceBlock_UsesApprovedCampaignSpeedMultiplier`
- `IceRunwaySettings_RejectNonPositiveSpeed`
- `IceBlock_PrefersOneTapAndKeepsDoubleTapsBelowTenPercent`
- `IceTapRhythm_RetryReplaysTheSameColorsAndQuota`
- `LearningBlockPlans_AreDeterministicAndKeepModifiersIsolated`

### Hidden, Flicker, Clone, Echo, and ETA

#### `TimedFogCurtainTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/TimedFogCurtainTests.cs`

- `Trigger_FadesInHoldsFullOpacityThenFadesToZero`
- `Trigger_IsOneShotUntilAttemptReset`
- `Advance_RejectsNegativeTime`
- `Settings_RejectNonPositivePhaseDurations`

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
- `Continue_HasNoAttemptCapAndRemainsBoundedByFiniteStage`
- `Continue_CountChangesOnlyOnSuccessAndRetryResetsIt`
- `Continue_DoesNotRestoreConsumedShieldOrBooster`
- `Continue_GrantsSafeResumeSequenceAndProtection`
- `ContinuedClear_DoesNotReplaceBestRecords`
- `PostBooster_FirstGateMatchesAndSecondNeedsAtMostOneTap`
- `StageTwo_HasPerceptibleAuthoredCadenceSections`
- `StageThree_HasAuthoredExceptionSections`
- `SimulationProfiles_AreExplicitAndValid`
- `PerfectSimulation_IsDeterministicAndCompletes`
- `StochasticSimulation_ReplaysWithSameSeed`
- `Simulation_ContinueUseMetricsAreDeterministicAndStageBounded`
- `Simulation_DisabledContinueReportsZeroUseMetrics`
- `SimulationReport_EmitsContinueUseMetricsInAllFormats`
- `SimulationReport_ContainsRequiredMechanicalSections`

#### `AttemptContinuePolicyTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/AttemptContinuePolicyTests.cs`

- `CoinPrices_AdvanceBySuccessfulCoinOrdinal`
- `CoinFirst_PreservesRewardedAdRight`
- `RewardedAdFirst_PreservesFirstCoinPrice`
- `CoinAdCoin_UsesOnlySuccessfulCoinOrdinals`
- `RewardedAd_IsConsumedOnlyByOneCompletedResult`
- `NonNegativeCoreContinueCount_HasNoTotalCap`
- `Retry_UsesFreshAttemptPolicy`
- `NonCompletedAdResult_DoesNotMutatePolicy`
- `UnavailableAdService_IsTruthfulAndNeverSucceeds`
- `CoinOffer_RequiresPriceAndDoesNotMutatePolicy`

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
- `StageClearRewardPolicy_UsesDifficultyAndKeepsMilestoneSeparate`
- `StageClear_SaveFailureDoesNotPublishPartialProgression`
- `ConsumeStartItems_BothSelected_DecrementsAtomically`
- `ConsumeStartItems_None_IsNoOpWithoutSave`
- `ConsumeStartItems_Insufficient_IsAtomicAndDoesNotSave`
- `ConsumeStartItems_SaveFailure_DoesNotPublishDecrement`
- `ConsumeStartItems_RepeatedAttemptsConsumeAgain`
- `PurchaseStartItemWithCoins_IsAtomicAndIdempotent`
- `PurchaseStartItemWithCoins_InsufficientFundsDoesNotSave`
- `PurchaseStartItemWithCoins_SaveFailureDoesNotPublish`
- `SpendContinueCoins_IsAtomicAndIdempotent`
- `SpendContinueCoins_InvalidRequestDoesNotSave`
- `SpendContinueCoins_InsufficientFundsDoesNotSave`
- `SpendContinueCoins_SaveFailureDoesNotPublish`
- `SpendContinueCoins_SaveFailureCanRetrySameTransactionOnce`
- `SpendContinueCoins_SameTransactionDifferentAmountIsReplay`
- `SpendContinueCoins_ReloadKeepsTransactionIdempotent`
- `StageStartReceipt_ClearRefundsConsumedHeartAndRewardsOnce`
- `StageStartReceipt_UnlimitedOrFreeStartCannotCreateHeartRefund`
- `StageClear_HeartRefundAndRewardsAreAtomicOnSaveFailure`
- `StageClear_RejectsHeartRefundTokenWithoutStoredHeartSpend`

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
- `LobbyThemeOne_AmbientContainsRealTransparentPixels`

#### `Theme01GameplayArtTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/Theme01GameplayArtTests.cs`

- `Theme01GameplayArt_PreservesSourceExportsAndPbrMaps`

#### `TestBuildMenuTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/EditMode/TestBuildMenuTests.cs`

- `EnabledBuildScenes_AreBootFrontendCampaign`
- `BuildOptions_UseDevelopmentModeAndExpectedOutput`
- `WebGlPortraitTemplate_UsesExactNineBySixteenFrame`
- `WebGlPortraitSettings_ApplyAndRestoreEditorValues`
- `AndroidGradle_AlignsKotlinLibrariesIdempotently`
- `AndroidGradle_ReplacesLegacyPrefixAlignment`

## PlayMode automated

### Campaign flow, generated Scene, and persistence isolation

#### `GrayboxScenePlayModeTests.cs`

Source: `ColorGateRunner/Assets/Game/Tests/PlayMode/GrayboxScenePlayModeTests.cs`

- `NormalFlow_OpensLobbyRatherThanStageSelect`
- `CampaignCamera_CurvesWithBoundedRotationInertia`
- `CampaignRunner_SteersVisualWithoutChangingPathAuthority`
- `ThemeOneGameplayVisuals_UseImportedArtBloomAndFixedBreakPool`
- `Play_OpensItemSelection`
- `DirectCampaignPreRunBack_ReturnsToCampaignLobby`
- `StagesOneThroughFive_ShowLockedItemsAndRejectToggles`
- `StageEight_StartConsumesSelectedInventoryBeforeCountdown`
- `StageEight_ZeroStockOffersQuickBuyAndCanStartItemless`
- `StageEight_QuickBuyPurchasesOneAndAutoSelects`
- `StageEight_QuickBuyInsufficientCoinsKeepsModalAndInventory`
- `StageEight_QuickBuySaveFailureSpendsNothingAndCanRetry`
- `StartItemSaveFailure_BlocksCountdownAndShowsError`
- `InventoryChangedAfterSelection_BlocksStartAndNormalizes`
- `BackBeforeStart_DoesNotConsumeSelectedItem`
- `Retry_StartConsumesSelectedItemForNewAttempt`
- `Retry_NormalizesSelectedItemWhenStockReachesZero`
- `ProvidedItem_StartsWithoutProductInventory`
- `MissingProductSession_CannotGrantFreeSelectableItem`
- `DebugStagePicker_RemainsHidden`
- `ColorHud_UpdatesCurrentAndNextAfterTap`
- `CampaignHiddenGate_HidesEmblemWithoutTextAndKeepsJudgment`
- `CampaignHiddenGate_NaturallyErasesDuringLiveApproach`
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
- `HorizontalColorStrip_CurrentAndNextSwapSides`
- `HorizontalColorStrip_RapidTapsRetargetToAuthoritativeColor`
- `BoosterCamera_LowersAndMovesCloserBehindPlayer`
- `ResetProgress_CancellationChangesNothing`
- `ResetProgress_ConfirmationReturnsLobbyToStageOne`
- `HorizontalColorStrip_HasSixReusableSlots`
- `ThreeColorStrip_ShowsPreviousCurrentAndNextAroundCenter`
- `ExperimentLauncher_IsHiddenFromNormalFlow`
- `ExperimentPlay_DoesNotChangeNormalProgression`
- `ExperimentLauncher_StartsCountdownWithFixedPoolSession`
- `ExperimentColorStack_SupportsThreeThroughSixColors`
- `ExperimentRapidTaps_EndAtCorrectCurrentTile`
- `IceGatePositions_RemainContinuousDuringMovement`
- `Camouflage_RemainsNeutralThenRevealsWithoutMoving`
- `Fog_KeepsExactlyTwoGatesFullyReadableWithoutPoolGrowth`
- `CampaignFog_UsesOneTimedCurtainAtAdaptiveDistance`
- `CampaignFog_PauseContinueAndRetryPreserveLifecycle`
- `CampaignIce_PreplacesFixedRunwayAndKeepsTrackStable`
- `CampaignIceMastery_RuntimePlanKeepsDoubleTapsBelowTenPercent`
- `ExperimentIce_ChangesWholeTrackAndKeepsColorJudgment`
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
- `SecondFailure_StillOffersContinueBelowCap`
- `ContinueSources_AdThenCoinKeepsFirstCoinPrice`
- `ContinueSources_CoinAdCoinEscalateWithoutTotalCap`
- `CoinContinueFailure_KeepsFailureFrozen`
- `InsufficientCoins_OpensTruthfulPopupAndKeepsFailureFrozen`
- `RewardedFailureAndStaleDuplicateCallbackAreOneShot`
- `RewardedCallbackAfterRetryCannotResumeNewAttempt`
- `ContinueRequestsBeforeFailureDoNotSpendOrShowAd`
- `UnavailableProvidersHideContinueOffersButKeepRetry`
- `Retry_StillOpensItemSelection`
- `Retry_RetainsItemSelection`
- `Restart_ReplaysIdenticalGateLayout`
- `StageClear_PersistsRecordAndUnlock`
- `ContinuedClear_UnlocksButDoesNotReplaceBestRecords`
- `ClearContinue_ReturnsToUpdatedLobby`
- `CampaignClear_HidesReplayAndNextOpensPreRunDirectly`
- `LobbyPlay_UsesStableIdOfDisplayedProgressionStage`
- `LobbyProgression_ChangesAtMilestones`
- `ThirtyRepeatedFlows_LeaveNoDuplicateOrStaleVisuals`
- `EveryStage_AllItemCombinationsCanInitialize`
- `StageSeven_StartBoosterCrossesEveryGateAndReachesGoal`
- `StageSelectUi_HasOneButtonPerCatalogEntry`
- `StageEight_IsCleanThreeColorItemApplicationRuntime`
- `CampaignCamouflageGate_RevealsFromEtaAndStaysJudged`
- `CampaignEchoProvider_ActivatesColoredPlayerShell`
- `CampaignMechanicIntro_BindsModifierToRuntimeGateView`
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
- `NormalGameplay_HasNoWorldWarpParticles`
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
- `LastHeart_ClearRefundsAndShowsFirstClearRewards`
- `TogglePresentation_MatchesAfterSaveReloadAndInPause`
- `CampaignStart_UsesProductInventoryAndPersistsSpend`
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
- `ContinueCountdown_AppliesCamouflageBeforeFirstFrame`
- `Continue_UnaffectedGateTransformsRemainUnchanged`
- `Continue_FailedGateCannotImmediatelyFailAgain`
- `Continue_ResumesSameTimerProgressSpeedColorAndCursor`
- `Continue_ResumesSameActiveSequence`
- `Continue_ClearsCameraShakeAndDoesNotShowItemSelection`
- `Continue_RecyclesFailedSlotWithoutResettingPools`
- `Continue_DoesNotRestoreShieldOrBooster`
- `StageEleven_RepeatedContinuesPastGateThirtyFourReachGoal`
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

### Iteration 21 commerce coverage

- `ProductFoundationTests` covers schema-2 migration, exact 11-product local
  catalog, atomic Heart/item start, recharge and rollback behavior, timed
  unlimited Hearts, idempotent grants and Continue Ticket spending.
- `GrayboxScenePlayModeTests` covers empty-Heart PreRun blocking, Ticket-first
  Continue success and Ticket save-failure frozen-state preservation.
- Campaign generated-structure coverage includes one Ticket button and label,
  required controller references, no duplicate generated root and one
  EventSystem.

### Iteration 22 Developer Console coverage

- `ProductFoundationTests.DevelopmentEconomy_UpdatesExactValuesAndKeepsCommerceLedger`
- `ProductFoundationTests.DevelopmentEconomy_InvalidValuesDoNotSaveOrPublish`
- `ProductFoundationTests.DevelopmentCampaignReset_PreservesEconomyAndResetsLobby`
- `ProductFoundationTests.DevelopmentEconomyReset_PreservesCampaignProgress`
- `DeveloperConsoleTests.WindowMenu_IsRegisteredOnApprovedPath`
- `DeveloperConsoleTests.BuildSettings_KeepBootFrontendCampaignOrder`

### Iteration 23 Continue and clear economy coverage

- `AttemptContinuePolicyTests` covers uncapped Continue capacity, the
  900/1,900/2,900/4,900-repeat Coin ordinal, mixed Coin/ad order and Retry
  reset.
- `Step9ACoreTests` covers repeated Core Continue and a finite, Stage-sized
  simulation histogram instead of a product cap.
- `StageCatalogArchitectureTests` verifies the approved Normal, Hard and Very
  Hard assignments in the generated Catalog asset.
- `ProductFoundationTests` covers difficulty rewards, one-use Heart refund
  receipts, free/unlimited exclusions, invalid tokens and atomic save failure.
- `GrayboxScenePlayModeTests` covers failure wallet/shortage presentation,
  uncapped mixed sources and direct next-Stage PreRun without Campaign Replay.
- `Step9A1PlayModeTests` covers modifier presentation from the first Continue
  countdown frame while consumed Attempt buffs remain absent.
- `FrontendFlowPlayModeTests` covers the last-Heart clear refund and visible
  first-clear reward breakdown across the Frontend/Campaign boundary.

## References

- `TEST_PLAN.md` — validation policy and tier selection.
- `CURRENT_STATUS.md` — current counts, hashes, snapshots, and baseline.
- `TEST_HISTORY.md` — preserved historical mappings and evidence.
- `Docs/TEST_PLAN.md` in the source repository — original catalog source.

## Iteration 29 current coverage

- `LobbyConsolidationEditModeTests.HeartText_FormatsFullRechargeAndUnlimitedStates`
  fixes the full, recharge and timed-unlimited Lobby Heart labels.
- `ProductFoundationTests.StageStart_ConsumesHeartAndRechargesOfflineWithoutRollbackGain`
  also fixes the Product-calculated next Heart recharge UTC.
- `LobbyConsolidationEditModeTests.FrontendReader_ReusesLobbyProgressionAndReturnsStableStageId`
  also verifies the authored difficulty projection.
- `FrontendFlowPlayModeTests.GuestChoicePersistsThenLobbyBackRequestsExit`
  verifies removed duplicate labels, top controls, Heart visibility, Stage
  difficulty and the bottom `PLAY` action in the generated Frontend Scene.
- Existing Frontend progression and clear-return tests verify the compact
  theme/upgrades/reward strings and unchanged navigation.

## Iteration 30 current coverage

- `FrontendSceneBuilderTests.LobbyThemeCatalog_HasCompleteThemeOneAndFutureFallbacks`
  verifies the exact three-theme catalog, Theme 1 Sprite paths/import settings
  and palette-only Theme 2/3 fallbacks.
- `FrontendFlowPlayModeTests.LobbyThemeOne_ShowsLayeredNonBlockingArtwork`
  verifies active Background/Midground/Foreground Sprites, non-raycast
  decoration, Product-derived upgrade-node count and an interactive `PLAY`.
- `FrontendFlowPlayModeTests.LobbyProgression_ShowsEconomyThemeAndAcknowledgesReward`
  also verifies that two applied milestones activate exactly two visual nodes.

## Iteration 31 current coverage

- `FrontendSceneBuilderTests.LobbyThemeOne_AmbientContainsRealTransparentPixels`
  decodes the Lobby ambient PNG and rejects an opaque baked checkerboard.
- `Theme01GameplayArtTests.Theme01GameplayArt_PreservesSourceExportsAndPbrMaps`
  verifies the Blender source, five FBX imports and fifteen UV/PBR maps.
- `GrayboxScenePlayModeTests.ThemeOneGameplayVisuals_UseImportedArtBloomAndFixedBreakPool`
  verifies the imported runner/city/Goal hierarchy, enabled post-processing,
  global Volume and fixed six-view break pool.
- Existing Ice whole-track material and Booster gate reset tests remain active
  against the imported Renderer ownership path.

## Iteration 32 current coverage

- `Theme01UiArtTests.Theme01UiArt_ImportsTransparentSpritesAndSliceBorders`
  verifies all seven common surfaces and ten semantic icons, their Sprite
  import settings, 9-slice borders and real transparent/visible pixels.
- `FrontendFlowPlayModeTests.ThemeOneUiSkin_UsesSlicedButtonsAndResourceIcons`
  verifies the Frontend primary surface, non-raycast action icon and Lobby
  Coin/Heart icons while Product-backed text remains in place.
- `GrayboxScenePlayModeTests.ThemeOneUiSkin_CoversGameplayButtonsPanelsAndIcons`
  verifies sliced Pause/failure presentation and semantic Pause/Shield icons
  across active and inactive Campaign UI roots.

## Iteration 33 current coverage

- `GrayboxScenePlayModeTests.ThemeOneImportedArtwork_RootsAreNormalizedAndTrackIsHorizontal`
  verifies identity local transforms for the five Theme 1 imported artwork
  roots and aggregate Track bounds extending forward on Z.
- `GrayboxSceneBuilder.ValidateTheme01ArtworkAxes` applies the same contract to
  both consecutive Campaign Builder passes so a regenerated vertical Track is
  rejected before the Scene is accepted.

## Iteration 34 current coverage

- `GrayboxScenePlayModeTests.TrackPool_RecyclesBeyondOneKilometerWithoutGapsOrOverlap`
  advances the fixed six-segment pool through 1.2 km and verifies each
  40-unit span, every ordered seam and forward coverage after every step.
- `GrayboxSceneBuilder.ValidateTrackPoolContinuity` rejects incorrect anchor
  spans and initial gap/overlap before accepting either Builder pass.

## Iteration 35 current coverage

- `EchoOfferCoordinatorTests.AttemptBlock_PreventsOfferWithoutChangingDeterministicSlot`
  fixes the offer API's attempt-block behavior.
- `CampaignMechanicStageTests.EchoStage_SelectedShieldBlocksProvidersForWholeAttempt`
  and `Iteration3EchoTests.ShieldedAttempt_HasNoEchoProviderForEntireRun`
  cover Campaign and Experiment exclusion while existing no-Shield Echo tests
  retain deterministic acquisition and consumption.
- `Iteration3EchoPlayModeTests.DirectMatchActivatesColoredEchoShell` verifies
  exact stored RGB on the active field, and
  `EchoConsumptionHidesShellWithoutUsingShield` verifies the real gate path
  hides the field and starts its collapse emitter. `ShieldedEchoAttempt_HasNoProvider`
  verifies generated initial pool output.
- `GrayboxScenePlayModeTests.ShieldBreak_EntersRecoveryAndRecoversPresentation`
  verifies Shield consumption hides the surface and starts its collapse
  emitter before recovery.
- `GrayboxScenePlayModeTests.ProtectionFields_UseTransparentHexShaderWithoutHidingRunner`
  verifies the shared field structure and shader.
- `Theme01GameplayArtTests.ProtectionFieldShader_HasNoCompilerErrors` rejects
  shader compiler errors before a pink fallback can be accepted.
- `GrayboxScenePlayModeTests.WarpBooster_UsesRunnerAlignedWorldSpaceVolumeWithinParticleCap`
  fixes the Player-owned hierarchy, two forward Box layers, World Space
  simulation, velocity alignment, semantic colors and hard capacity.
- `GrayboxSceneBuilder.ValidateProtectionAndWarpEffects` applies the structural
  and capacity contracts to both consecutive Builder passes.

## Iteration 36 current coverage

- `Theme01GameplayArtTests.CyberOrbRunner_HasReadableRearSilhouetteParts`
  verifies the imported FBX contains the dark hull, color panel, rear bumper,
  side fins, twin thrusters/glows, twin chevrons and rear light bar.
- `Theme01GameplayArtTests.GameplayColorMaterials_EmitWhileDarkAlloyDoesNot`
  verifies HDR semantic color emission and non-emissive Track dark alloy.
- `GrayboxScenePlayModeTests.ThemeOneGameplayVisuals_UseImportedArtBloomAndFixedBreakPool`
  additionally verifies the live generated rear parts, player HDR emission and
  non-emissive Track while retaining the existing post-processing, Volume and
  gate-break pool coverage.
- `GrayboxSceneBuilder.ValidateRunnerRearReadability`,
  `ValidateEmissiveContrast` and `ValidateBloomProfile` apply the asset-axis,
  material and four-iteration Bloom contracts to both Builder passes.
- `Theme01GameplayArtTests.GameplayColorMaterials_EmitWhileDarkAlloyDoesNot`
  also verifies the generated Runner Glass material is smooth and non-emissive.
- `GrayboxScenePlayModeTests.ThemeOneGameplayVisuals_UseImportedArtBloomAndFixedBreakPool`
  verifies `RunnerColorView` maps Blue into the glass property block without
  emission while the rear accent remains HDR emissive.

## Iteration 39 current coverage

- `ProductFoundationTests.PurchaseStartItemWithCoins_IsAtomicAndIdempotent`
  covers both item kinds, the fixed Coin exchange, exactly-one grant and ledger
  replay. Its insufficient-funds and save-failure companions prove no partial
  save or published state.
- `GrayboxScenePlayModeTests.StageEight_ZeroStockOffersQuickBuyAndCanStartItemless`
  verifies the zero-stock labels, modal and cancel/itemless path.
- `StageEight_QuickBuyPurchasesOneAndAutoSelects` covers Shield and Booster,
  including the following `START` consumption. The shortage and save-failure
  tests retain the modal, inventory and Coin truthfully.
- `ProtectionFields_UseTransparentHexShaderWithoutHidingRunner` now fixes the
  centered field/collapse-root pose, while
  `WarpBooster_UsesRunnerAlignedWorldSpaceVolumeWithinParticleCap` fixes
  prewarm, expanded upper-frame volumes and the unchanged 160-particle cap.
- `BoosterCamera_LowersAndMovesCloserBehindPlayer` covers the closer normal and
  Booster chase offsets. `Theme01GameplayArtTests.ProtectionFieldShader_HasNoCompilerErrors`
  remains the shader compiler guard and was additionally executed on D3D11.

## Iteration 44 current coverage

- `StageCatalogArchitectureTests` and `StageSessionTests` require revision 11's
  23 stable entries, Stage 23 Hard difficulty and capped old-endpoint unlock
  repair. `CampaignMechanicStageTests`, `CampaignLearningCurveStageTests` and
  `CampaignDeferredMechanicStageTests` cover Hidden's 2/2/3-color block,
  approved values, isolation and deterministic plans.
- `GrayboxScenePlayModeTests.CampaignHiddenGate_HidesTargetAndKeepsMarkerAndJudgment`
  proves the live Campaign View hides target information while retaining the
  marker and ordinary judgment. The generic Stage/runtime binding case now
  includes Stage 21.
- `TestBuildMenuTests` fixes enabled Scene order, Development outputs, exact
  WebGL portrait settings, Kotlin dependency placement/exclusions,
  idempotency and cached-prefix upgrade behavior.

## Iteration 45 current coverage

- `CampaignMechanicStageTests.HiddenBlock_DistributesIncreasingPressureAcrossWholeStages`
  fixes exact Stage 21–23 gate IDs, late-run distribution, increasing counts,
  stage-local leads and the shared transition.
- `Theme01UiArtTests.Theme01UiArt_ImportsTransparentSpritesAndSliceBorders`
  now checks all six emblem sprites, 512 pixels per unit and genuine alpha.
  Builder validation and the existing HUD cases require SpriteRenderer/Image
  references rather than Unicode Text symbols.
- `GrayboxScenePlayModeTests.CampaignHiddenGate_HidesEmblemWithoutTextAndKeepsJudgment`
  rejects a Hidden marker while retaining ordinary judgment.
  `CampaignHiddenGate_NaturallyErasesDuringLiveApproach` advances the real
  Campaign until the live observation, fade/contract and fully hidden states.
- `HiddenPlayModeTests` retains seven pause, timing, judgment, Echo, Shield and
  Retry cases under the text-free emblem presentation. Flicker coverage now
  verifies its active Core color and emblem sprite stay synchronized while its
  distinct `FLICKER` marker remains.
- `TestBuildMenuTests.AndroidGradle_AlignsKotlinLibrariesIdempotently` also
  requires the post-generation interface when tests compile for WebGL. The
  real combined build is the cross-target integration check.
