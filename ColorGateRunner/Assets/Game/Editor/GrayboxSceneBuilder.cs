using System;
using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    public static class GrayboxSceneBuilder
    {
        internal const string ScenePath = "Assets/Scenes/SampleScene.unity";
        internal const string GeneratedRootName = "ColorGateRunner_Graybox";
        internal const string GeneratedMaterialsFolder =
            "Assets/Game/Generated/Materials";
        internal const string GeneratedSplineFolder =
            "Assets/Game/Generated/Spline";
        internal const string GeneratedProfilesFolder =
            "Assets/Game/Generated/Profiles";
        internal const string Theme01ModelsFolder =
            "Assets/Game/Art/Gameplay/Theme01/Models";
        internal const string Theme01TexturesFolder =
            "Assets/Game/Art/Gameplay/Theme01/Textures";
        internal const int GatePoolSize = 6;
        internal const int TrackPoolSize = 6;
        internal const int IceRunwayPanelCount = 50;
        internal const float TrackSegmentLength = 40f;

        internal static readonly Color RedColor = FromHex(0xE63946);
        internal static readonly Color BlueColor = FromHex(0x2D7FF9);
        internal static readonly Color GreenColor = FromHex(0x22C55E);
        internal static readonly Color YellowColor = FromHex(0xF4C430);
        internal static readonly Color PurpleColor = FromHex(0x9B5DE5);
        internal static readonly Color CyanColor = FromHex(0x00B8D9);
        internal static readonly Color NeutralColor = FromHex(0xD9D9D9);
        internal static readonly Color FailureColor = FromHex(0x6B7280);
        internal static readonly Color WarpGoldColor = FromHex(0xFF9D18);

        private static readonly Vector3 PlayerStartPosition =
            new Vector3(0f, 1f, 0f);
        private static readonly Vector3 CameraPosition =
            new Vector3(0f, 7f, -8.2f);
        private static readonly Vector3 CameraRotation =
            new Vector3(18f, 0f, 0f);

        [MenuItem("Tools/Color Gate Runner/Build Graybox Scene")]
        public static void BuildGrayboxScene()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Theme01UiSkinBuilder.EnsureAndConfigure();
            Sprite[] colorEmblems =
                Theme01UiSkinBuilder.LoadColorEmblems();
            Sprite victoryEmblem = Theme01UiSkinBuilder.LoadVictoryEmblem();
            Sprite coinIcon = Theme01UiSkinBuilder.LoadIcon("Coin");
            Sprite heartIcon = Theme01UiSkinBuilder.LoadIcon("Heart");
            Sprite shieldIcon = Theme01UiSkinBuilder.LoadIcon("Shield");
            Sprite boosterIcon = Theme01UiSkinBuilder.LoadIcon("Booster");
            string frontendPath = SelectFrontendScenePath(
                EditorBuildSettings.scenes);
            StageCatalogAsset stageCatalogAsset =
                StageCatalogAssetBuilder.EnsureAndConfigure();
            EnsureAssetFolder(GeneratedMaterialsFolder);
            EnsureAssetFolder(GeneratedSplineFolder);
            EnsureAssetFolder(GeneratedProfilesFolder);
            Material red = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Red.mat",
                RedColor,
                4.5f);
            Material blue = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Blue.mat",
                BlueColor,
                4.5f);
            Material green = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Green.mat",
                GreenColor,
                4.5f);
            Material yellow = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Yellow.mat",
                YellowColor,
                4.5f);
            Material purple = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Purple.mat",
                PurpleColor,
                4.5f);
            Material cyan = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Cyan.mat",
                CyanColor,
                5.5f);
            Material neutral = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Neutral.mat",
                NeutralColor,
                3f);
            Material failure = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Failure.mat",
                FailureColor,
                0f);
            Material darkAlloy = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Theme01DarkAlloy.mat",
                new Color(0.004f, 0.008f, 0.018f, 1f),
                0f);
            Material runnerGlass = CreateOrUpdateRunnerGlassMaterial(
                GeneratedMaterialsFolder + "/RunnerGlass.mat",
                BlueColor);
            Material roadMaterial = CreateOrUpdateMappedLitMaterial(
                GeneratedMaterialsFolder + "/CampaignRoad.mat",
                Theme01TexturesFolder + "/Road_BaseColor.png",
                Theme01TexturesFolder + "/Road_Normal.png",
                Theme01TexturesFolder + "/Road_MetallicSmoothness.png",
                Color.white,
                0.08f,
                0.22f);
            Material iceMaterial = CreateOrUpdateMappedLitMaterial(
                GeneratedMaterialsFolder + "/CampaignIce.mat",
                Theme01TexturesFolder + "/Ice_BaseColor.png",
                Theme01TexturesFolder + "/Ice_Normal.png",
                Theme01TexturesFolder + "/Ice_MetallicSmoothness.png",
                Color.white,
                0.04f,
                0.92f);
            Material fogCurtainMaterial = CreateOrUpdateTransparentMaterial(
                GeneratedMaterialsFolder + "/FogCurtain.mat",
                new Color(0.25f, 0.34f, 0.46f, 0.54f),
                Theme01TexturesFolder + "/Fog_Noise.png");
            Material fogWispMaterial = CreateOrUpdateAlphaParticleMaterial(
                GeneratedMaterialsFolder + "/FogWisps.mat",
                new Color(0.3f, 0.39f, 0.5f, 0.32f),
                Theme01TexturesFolder + "/Fog_Noise.png");
            Material rainMaterial = CreateOrUpdateAlphaParticleMaterial(
                GeneratedMaterialsFolder + "/FogRain.mat",
                new Color(0.46f, 0.62f, 0.78f, 0.42f),
                null);
            VolumeProfile fogWeatherProfile =
                CreateOrUpdateFogWeatherProfile(
                    GeneratedProfilesFolder + "/FogWeatherProfile.asset");
            Material protectionFieldMaterial =
                CreateOrUpdateProtectionFieldMaterial(
                    GeneratedMaterialsFolder + "/ProtectionField.mat");
            Material flickerFrameMaterial =
                CreateOrUpdateFlickerFrameMaterial(
                    GeneratedMaterialsFolder +
                    "/FlickerGateFrameDissolve.mat");
            Material flickerEmblemMaterial =
                CreateOrUpdateFlickerEmblemMaterial(
                    GeneratedMaterialsFolder + "/FlickerEmblemDissolve.mat");
            Material warpCyanMaterial = CreateOrUpdateAdditiveParticleMaterial(
                GeneratedMaterialsFolder + "/WarpCyan.mat",
                CyanColor);
            Material warpGoldMaterial = CreateOrUpdateAdditiveParticleMaterial(
                GeneratedMaterialsFolder + "/WarpGold.mat",
                WarpGoldColor);
            Material protectionPulseMaterial =
                CreateOrUpdateAdditiveParticleMaterial(
                    GeneratedMaterialsFolder + "/ProtectionPulse.mat",
                    Color.white);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemovePreviousSceneObjects(scene);

            GameObject root = new GameObject(GeneratedRootName);
            StageSceneController controller =
                root.AddComponent<StageSceneController>();
            Camera camera = CreateCamera(root.transform);
            CreateDirectionalLight(root.transform);
            CreateTheme01Volume(root.transform, camera);
            SplineCityPoolView cityPool =
                CreateTheme01City(root.transform, darkAlloy, blue, cyan);
            TrackPoolController trackPool =
                CreateTrackPool(root.transform, darkAlloy, cyan);
            CampaignSplinePathView campaignSplinePath =
                CreateCampaignSplinePath(root.transform, roadMaterial, cyan);
            SplineTrackLabView splineTrackLabView =
                CreateSplineTrackLab(root.transform, darkAlloy, cyan);
            IceRunwayView iceRunway =
                CreateIceRunway(root.transform, iceMaterial);
            Renderer playerRenderer;
            GameObject playerObject =
                CreatePlayer(
                    root.transform,
                    red,
                    darkAlloy,
                    cyan,
                    runnerGlass,
                    out playerRenderer,
                    out RunnerColorView runnerColorView,
                    out RunnerSteeringView runnerSteeringView);
            Rigidbody playerBody = playerObject.GetComponent<Rigidbody>();
            TrailRenderer trail = CreatePlayerTrail(playerObject.transform, blue);
            TimedFogCurtainView fogCurtain =
                CreateFogCurtain(
                    root.transform,
                    camera.transform,
                    fogCurtainMaterial,
                    fogWispMaterial,
                    rainMaterial,
                    fogWeatherProfile);
            ParticleSystem shieldCollapse;
            GameObject shieldVisual =
                CreateShieldVisual(
                    playerObject.transform,
                    protectionFieldMaterial,
                    protectionPulseMaterial,
                    out shieldCollapse);
            ParticleSystem echoCollapse;
            GameObject echoShellVisual =
                CreateEchoShellVisual(
                    playerObject.transform,
                    protectionFieldMaterial,
                    protectionPulseMaterial,
                    out echoCollapse);
            GameObject goal = CreateGoal(root.transform, darkAlloy, cyan);
            StageGateView[] gates =
                CreateGatePool(
                    root.transform,
                    controller,
                    darkAlloy,
                    neutral,
                    protectionFieldMaterial,
                    flickerFrameMaterial,
                    flickerEmblemMaterial,
                    colorEmblems[0]);
            GateBreakEffectPool gateBreakEffects =
                CreateGateBreakEffectPool(root.transform, cyan);
            ParticleSystem successParticles =
                CreateParticleSystem(
                    "SuccessParticles",
                    root.transform,
                    green,
                    false);
            ParticleSystem speedLines =
                CreateBoosterSpeedLines(
                    playerObject.transform,
                    warpCyanMaterial,
                    warpGoldMaterial);

            Canvas canvas = CreateCanvas(root.transform);
            GameplayTapSurface tapSurface =
                CreateTapSurface(canvas.transform);
            Transform safeArea = CreateSafeArea(canvas.transform);
            GameObject[] flowRoots =
            {
                CreateFlowRoot("LobbyRoot", safeArea),
                CreateFlowRoot("PreRunRoot", safeArea),
                CreateFlowRoot("GameplayHudRoot", safeArea),
                CreateFlowRoot("CountdownRoot", safeArea),
                CreateFlowRoot("ClearResultRoot", safeArea),
                CreateFlowRoot("FailedResultRoot", safeArea),
                CreateFlowRoot("DevelopmentDebugRoot", safeArea)
            };

            GameObject lobbyPanel;
            Text lobbyStageText;
            Text lobbyStageTitleText;
            Text lobbyStageDescriptionText;
            Text lobbyProgressText;
            Text lobbyTierText;
            Button lobbyPlayButton;
            Button experimentLabButton;
            GameObject[] lobbyTierRoots;
            Button resetProgressButton;
            GameObject resetProgressConfirmation;
            Button confirmResetProgressButton;
            Button cancelResetProgressButton;
            CreateLobbyUi(
                flowRoots[0].transform,
                out lobbyPanel,
                out lobbyStageText,
                out lobbyStageTitleText,
                out lobbyStageDescriptionText,
                out lobbyProgressText,
                out lobbyTierText,
                out lobbyPlayButton,
                out experimentLabButton,
                out lobbyTierRoots,
                out resetProgressButton,
                out resetProgressConfirmation,
                out confirmResetProgressButton,
                out cancelResetProgressButton);

            GameObject stageSelectPanel;
            Button[] stageButtons;
            Text[] stageSummaries;
            Button unlockAllButton;
            CreateStageSelectUi(
                flowRoots[6].transform,
                out stageSelectPanel,
                out stageButtons,
                out stageSummaries,
                out unlockAllButton);
            CreateExperimentLauncherUi(flowRoots[6].transform, controller);

            CreateSplineLabHud(
                safeArea,
                out GameObject splineLabHud,
                out Text splineLabTitle,
                out Text splineLabProgress,
                out Text splineLabInstruction,
                out Button splineLabRestart,
                out Button splineLabExit);

            GameObject itemPanel;
            Text selectedStageText;
            Button shieldButton;
            Text shieldButtonText;
            Button boosterButton;
            Text boosterButtonText;
            Text preRunStatusText;
            Button startButton;
            Button backButton;
            GameObject startItemPurchaseModal;
            Text startItemPurchaseTitle;
            Text startItemPurchaseMessage;
            Button startItemPurchaseConfirm;
            Text startItemPurchaseConfirmText;
            Button startItemPurchaseCancel;
            CreateItemUi(
                flowRoots[1].transform,
                out itemPanel,
                out selectedStageText,
                out shieldButton,
                out shieldButtonText,
                out boosterButton,
                out boosterButtonText,
                out preRunStatusText,
                out startButton,
                out backButton,
                out startItemPurchaseModal,
                out startItemPurchaseTitle,
                out startItemPurchaseMessage,
                out startItemPurchaseConfirm,
                out startItemPurchaseConfirmText,
                out startItemPurchaseCancel);

            GameObject countdownPanel;
            Text countdownText;
            CreateCountdownUi(
                flowRoots[3].transform,
                out countdownPanel,
                out countdownText);

            GameObject hud;
            Text hudStage;
            Text hudProgress;
            Image progressFill;
            GameObject hudShieldIcon;
            GameObject boosterMeterRoot;
            Image boosterMeter;
            GameObject boosterWarning;
            GameObject colorHudPanel;
            GameObject[] colorTiles;
            Image[] colorTileImages;
            Image[] colorTileEmblems;
            GameObject[] nextColorMarkers;
            CreateHud(
                flowRoots[2].transform,
                colorEmblems,
                out hud,
                out hudStage,
                out hudProgress,
                out progressFill,
                out hudShieldIcon,
                out boosterMeterRoot,
                out boosterMeter,
                out boosterWarning,
                out colorHudPanel,
                out colorTiles,
                out colorTileImages,
                out colorTileEmblems,
                out nextColorMarkers);

            GameObject clearPanel;
            Text clearTitle;
            Text clearDetails;
            Button clearContinueButton;
            Button replayButton;
            Button clearLobbyButton;
            GameObject clearCelebrationRoot;
            CanvasGroup clearCelebrationGroup;
            RectTransform clearVictoryEmblem;
            RectTransform[] clearVictoryEmblemEchoes;
            CanvasGroup[] clearVictoryEmblemEchoGroups;
            Button clearSkipButton;
            CanvasGroup clearSkipPromptGroup;
            GameObject clearRewardRoot;
            CanvasGroup clearRewardGroup;
            RectTransform[] clearFireworkSparks;
            CanvasGroup[] clearFireworkSparkGroups;
            GameObject[] clearRewardRows;
            CanvasGroup[] clearRewardRowGroups;
            Image[] clearRewardRowIcons;
            Text[] clearRewardRowTexts;
            CreateResultUi(
                flowRoots[4].transform,
                victoryEmblem,
                out clearPanel,
                out clearTitle,
                out clearDetails,
                out clearContinueButton,
                out replayButton,
                out clearLobbyButton,
                out clearCelebrationRoot,
                out clearCelebrationGroup,
                out clearVictoryEmblem,
                out clearVictoryEmblemEchoes,
                out clearVictoryEmblemEchoGroups,
                out clearSkipButton,
                out clearSkipPromptGroup,
                out clearRewardRoot,
                out clearRewardGroup,
                out clearFireworkSparks,
                out clearFireworkSparkGroups,
                out clearRewardRows,
                out clearRewardRowGroups,
                out clearRewardRowIcons,
                out clearRewardRowTexts);

            GameObject failPanel;
            Text failTitle;
            Text failDetails;
            Text failContinueStatus;
            Button ticketContinueButton;
            Button coinContinueButton;
            Button rewardedContinueButton;
            Button retryButton;
            Button failLobbyButton;
            GameObject insufficientCoinsPopup;
            Text insufficientCoinsMessage;
            Button insufficientCoinsCloseButton;
            GameObject failureContinueRoot;
            GameObject failureExitConfirmationRoot;
            Text failureExitMessage;
            Button failureExitConfirmButton;
            Button failureExitCancelButton;
            GameObject failureConsequenceRoot;
            Text failureConsequenceTitle;
            Text failureConsequenceMessage;
            Button failureConsequenceContinueButton;
            GameObject failureFinalChoiceRoot;
            Text failureFinalMessage;
            CreateFailureUi(
                flowRoots[5].transform,
                out failPanel,
                out failTitle,
                out failDetails,
                out failContinueStatus,
                out ticketContinueButton,
                out coinContinueButton,
                out rewardedContinueButton,
                out retryButton,
                out failLobbyButton,
                out insufficientCoinsPopup,
                out insufficientCoinsMessage,
                out insufficientCoinsCloseButton,
                out failureContinueRoot,
                out failureExitConfirmationRoot,
                out failureExitMessage,
                out failureExitConfirmButton,
                out failureExitCancelButton,
                out failureConsequenceRoot,
                out failureConsequenceTitle,
                out failureConsequenceMessage,
                out failureConsequenceContinueButton,
                out failureFinalChoiceRoot,
                out failureFinalMessage);

            StageResultSequenceView resultSequenceView =
                controller.gameObject.AddComponent<StageResultSequenceView>();
            resultSequenceView.Configure(
                clearCelebrationRoot,
                clearCelebrationGroup,
                clearVictoryEmblem,
                clearVictoryEmblemEchoes,
                clearVictoryEmblemEchoGroups,
                clearSkipButton,
                clearSkipPromptGroup,
                clearRewardRoot,
                clearRewardGroup,
                clearFireworkSparks,
                clearFireworkSparkGroups,
                clearRewardRows,
                clearRewardRowGroups,
                clearRewardRowIcons,
                clearRewardRowTexts,
                coinIcon,
                heartIcon,
                shieldIcon,
                boosterIcon,
                failureContinueRoot,
                failureExitConfirmationRoot,
                failureExitMessage,
                failureExitConfirmButton,
                failureExitCancelButton,
                failureConsequenceRoot,
                failureConsequenceTitle,
                failureConsequenceMessage,
                failureConsequenceContinueButton,
                failureFinalChoiceRoot,
                failureFinalMessage,
                retryButton,
                failLobbyButton);

            CreatePauseUi(
                canvas.transform,
                flowRoots[2].transform,
                out Button pauseButton,
                out GameObject pauseOverlayRoot,
                out Image pauseDim,
                out GameObject pausePanel,
                out Button pauseResumeButton,
                out Button pauseRestartButton,
                out Button pauseSettingsButton,
                out Button pauseLobbyButton,
                out GameObject pauseModalRoot,
                out Text pauseModalTitle,
                out Text pauseModalMessage,
                out Button pauseModalConfirm,
                out Text pauseModalConfirmText,
                out Button pauseModalCancel,
                out SettingsPanelController pauseSettingsPanel,
                out GameObject pauseTransitionBlocker);

            CreateEventSystem(root.transform);
            tapSurface.Configure(controller);
            controller.Configure(
                stageCatalogAsset,
                playerObject.transform,
                playerRenderer,
                runnerColorView,
                runnerSteeringView,
                playerBody,
                camera,
                red,
                blue,
                green,
                yellow,
                purple,
                cyan,
                failure,
                colorEmblems,
                tapSurface,
                trackPool,
                campaignSplinePath,
                cityPool,
                gates,
                goal,
                shieldVisual,
                echoShellVisual,
                successParticles,
                speedLines,
                trail,
                fogCurtain,
                iceRunway,
                flowRoots,
                lobbyPanel,
                lobbyStageText,
                lobbyStageTitleText,
                lobbyStageDescriptionText,
                lobbyProgressText,
                lobbyTierText,
                lobbyPlayButton,
                experimentLabButton,
                lobbyTierRoots,
                resetProgressButton,
                resetProgressConfirmation,
                confirmResetProgressButton,
                cancelResetProgressButton,
                stageSelectPanel,
                stageButtons,
                stageSummaries,
                unlockAllButton,
                itemPanel,
                selectedStageText,
                shieldButton,
                shieldButtonText,
                boosterButton,
                boosterButtonText,
                preRunStatusText,
                startButton,
                backButton,
                startItemPurchaseModal,
                startItemPurchaseTitle,
                startItemPurchaseMessage,
                startItemPurchaseConfirm,
                startItemPurchaseConfirmText,
                startItemPurchaseCancel,
                countdownPanel,
                countdownText,
                hud,
                hudStage,
                hudProgress,
                progressFill,
                hudShieldIcon,
                boosterMeterRoot,
                boosterMeter,
                boosterWarning,
                colorHudPanel,
                colorTiles,
                colorTileImages,
                colorTileEmblems,
                nextColorMarkers,
                clearPanel,
                clearTitle,
                clearDetails,
                clearContinueButton,
                replayButton,
                clearLobbyButton,
                failPanel,
                failTitle,
                failDetails,
                failContinueStatus,
                ticketContinueButton,
                coinContinueButton,
                rewardedContinueButton,
                retryButton,
                failLobbyButton,
                insufficientCoinsPopup,
                insufficientCoinsMessage,
                insufficientCoinsCloseButton);
            controller.ConfigureResultSequence(resultSequenceView);
            controller.ConfigurePause(
                pauseButton,
                pauseOverlayRoot,
                pauseDim,
                pausePanel,
                pauseResumeButton,
                pauseRestartButton,
                pauseSettingsButton,
                pauseLobbyButton,
                pauseModalRoot,
                pauseModalTitle,
                pauseModalMessage,
                pauseModalConfirm,
                pauseModalConfirmText,
                pauseModalCancel,
                pauseSettingsPanel,
                pauseTransitionBlocker,
                new[]
                {
                    successParticles,
                    speedLines,
                    shieldCollapse,
                    echoCollapse,
                    fogCurtain.FogWispParticles,
                    fogCurtain.RainParticles
                },
                frontendPath);
            controller.ConfigureThemeVisuals(gateBreakEffects);
            SplineTrackLabController splineLabController =
                splineTrackLabView.gameObject.AddComponent<
                    SplineTrackLabController>();
            splineLabController.Configure(
                controller,
                splineTrackLabView,
                trackPool,
                playerObject.transform,
                camera,
                gates,
                goal,
                gateBreakEffects,
                successParticles,
                splineLabHud,
                splineLabTitle,
                splineLabProgress,
                splineLabInstruction,
                splineLabRestart,
                splineLabExit);
            controller.ConfigureSplineTrackLab(splineLabController);

            for (int index = 0; index < flowRoots.Length; index++)
            {
                flowRoots[index].SetActive(index == 0);
            }
            lobbyPanel.SetActive(true);
            stageSelectPanel.SetActive(true);
            itemPanel.SetActive(true);
            startItemPurchaseModal.SetActive(false);
            countdownPanel.SetActive(true);
            hud.SetActive(true);
            clearPanel.SetActive(true);
            failPanel.SetActive(true);
            pauseOverlayRoot.SetActive(false);
            pausePanel.SetActive(false);
            pauseModalRoot.SetActive(false);
            pauseSettingsPanel.gameObject.SetActive(false);
            pauseTransitionBlocker.SetActive(false);
            splineLabHud.SetActive(false);
            splineTrackLabView.SetVisualsActive(false);
            goal.SetActive(false);
            shieldVisual.SetActive(false);
            unlockAllButton.gameObject.SetActive(false);
            speedLines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            successParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            trail.emitting = false;

            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.Portrait;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void BuildGrayboxSceneFromCommandLine()
        {
            BuildGrayboxScene();
            BuildGrayboxScene();
            ValidateGeneratedScene();
        }

        internal static string SelectFrontendScenePath(
            EditorBuildSettingsScene[] scenes)
        {
            if (scenes == null)
            {
                throw new InvalidOperationException(
                    "Build Settings scenes are unavailable.");
            }

            for (int index = 0; index < scenes.Length; index++)
            {
                EditorBuildSettingsScene candidate = scenes[index];
                if (candidate.enabled &&
                    !string.IsNullOrWhiteSpace(candidate.path) &&
                    !PathsEqual(candidate.path, BootSceneBuilder.ScenePath) &&
                    !PathsEqual(candidate.path, ScenePath))
                {
                    return candidate.path;
                }
            }

            throw new InvalidOperationException(
                "Campaign requires one active Frontend Scene.");
        }

        public static void ValidateGeneratedScene()
        {
            Scene scene =
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject generatedRoot = null;
            int generatedRootCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == GeneratedRootName)
                {
                    generatedRoot = roots[index];
                    generatedRootCount++;
                }
            }

            if (generatedRootCount != 1 || generatedRoot == null)
            {
                throw new InvalidOperationException(
                    $"Expected one {GeneratedRootName} root.");
            }

            StageSceneController[] controllers =
                generatedRoot.GetComponentsInChildren<StageSceneController>(true);
            GameSceneController[] legacyControllers =
                generatedRoot.GetComponentsInChildren<GameSceneController>(true);
            Camera[] cameras =
                generatedRoot.GetComponentsInChildren<Camera>(true);
            EventSystem[] eventSystems =
                generatedRoot.GetComponentsInChildren<EventSystem>(true);
            StageGateView[] gates =
                generatedRoot.GetComponentsInChildren<StageGateView>(true);
            ShieldPickupView[] pickups =
                generatedRoot.GetComponentsInChildren<ShieldPickupView>(true);
            TrackPoolController[] trackPools =
                generatedRoot.GetComponentsInChildren<TrackPoolController>(true);
            TimedFogCurtainView[] fogCurtains =
                generatedRoot.GetComponentsInChildren<TimedFogCurtainView>(true);
            RunnerSteeringView[] runnerSteeringViews =
                generatedRoot.GetComponentsInChildren<RunnerSteeringView>(true);
            IceRunwayView[] iceRunways =
                generatedRoot.GetComponentsInChildren<IceRunwayView>(true);
            ExperimentLauncher[] experimentLaunchers =
                generatedRoot.GetComponentsInChildren<ExperimentLauncher>(true);
            SplineTrackLabView[] splineTrackLabs =
                generatedRoot.GetComponentsInChildren<SplineTrackLabView>(true);
            SplineTrackLabController[] splineLabControllers =
                generatedRoot.GetComponentsInChildren<
                    SplineTrackLabController>(true);
            GateBreakEffectPool[] gateBreakPools =
                generatedRoot.GetComponentsInChildren<GateBreakEffectPool>(true);

            if (controllers.Length != 1 ||
                !controllers[0].HasRequiredReferences() ||
                legacyControllers.Length != 0)
            {
                throw new InvalidOperationException(
                    "Stage controller is duplicated, incomplete, or legacy mode is exposed. " +
                    $"Controllers={controllers.Length}, " +
                    $"Required={(controllers.Length == 1 && controllers[0].HasRequiredReferences())}, " +
                    $"IceRunways={iceRunways.Length}, " +
                    $"IceRequired={(iceRunways.Length == 1 && iceRunways[0].HasRequiredReferences())}, " +
                    $"IcePanels={(iceRunways.Length == 1 ? iceRunways[0].PanelCount : 0)}, " +
                    $"Fog={(controllers.Length == 1 && controllers[0].FogCurtain != null)}, " +
                    $"FogRequired={(controllers.Length == 1 && controllers[0].FogCurtain != null && controllers[0].FogCurtain.HasRequiredReferences())}, " +
                    $"Missing={DescribeMissingSerializedReferences(controllers)}, " +
                    $"Legacy={legacyControllers.Length}.");
            }
            RectTransform pauseDimRect =
                controllers[0].PauseDim.GetComponent<RectTransform>();
            if (pauseDimRect.anchorMin != Vector2.zero ||
                pauseDimRect.anchorMax != Vector2.one ||
                !controllers[0].PauseDim.raycastTarget ||
                !Mathf.Approximately(
                    controllers[0].PauseDim.color.a,
                    0.85f))
            {
                throw new InvalidOperationException(
                    "Pause Dim must cover and block the full viewport at alpha 0.85.");
            }
            if (!PathsEqual(
                controllers[0].FrontendScenePath,
                SelectFrontendScenePath(EditorBuildSettings.scenes)))
            {
                throw new InvalidOperationException(
                    "Campaign Frontend destination is not the active serialized Scene.");
            }
            if (cameras.Length != 1 || eventSystems.Length != 1)
            {
                throw new InvalidOperationException(
                    "Expected one camera and one EventSystem.");
            }
            if (gates.Length != GatePoolSize || pickups.Length != 0)
            {
                throw new InvalidOperationException(
                    "Fixed stage gate pool is invalid or runtime pickup remains.");
            }
            for (int index = 0; index < gates.Length; index++)
            {
                if (!gates[index].HasRequiredReferences())
                {
                    throw new InvalidOperationException(
                        $"Stage gate {index} is missing its Echo field or core references.");
                }
            }
            if (trackPools.Length != 1 ||
                !trackPools[0].HasRequiredReferences() ||
                trackPools[0].SegmentCount != TrackPoolSize)
            {
                throw new InvalidOperationException("Track pool is invalid.");
            }
            ValidateTrackPoolContinuity(trackPools[0]);
            for (int index = 0; index < trackPools[0].SegmentCount; index++)
            {
                if (trackPools[0].GetSegment(index).IsCurveExperiment)
                {
                    throw new InvalidOperationException(
                        "Campaign track must remain straight after Spline Lab generation.");
                }
            }
            if (fogCurtains.Length != 1 ||
                !fogCurtains[0].HasRequiredReferences() ||
                fogCurtains[0].SectionCount !=
                    TimedFogCurtainView.RequiredSectionCount)
            {
                throw new InvalidOperationException(
                    "Timed Fog curtain is missing or incomplete.");
            }
            ParticleSystem.MainModule fogWispMain =
                fogCurtains[0].FogWispParticles.main;
            ParticleSystem.MainModule rainMain =
                fogCurtains[0].RainParticles.main;
            ParticleSystemRenderer rainRenderer =
                fogCurtains[0].RainParticles.GetComponent<
                    ParticleSystemRenderer>();
            if (fogWispMain.maxParticles !=
                    TimedFogCurtainView.FogWispParticleCapacity ||
                rainMain.maxParticles !=
                    TimedFogCurtainView.RainParticleCapacity ||
                fogWispMain.simulationSpace !=
                    ParticleSystemSimulationSpace.World ||
                rainMain.simulationSpace !=
                    ParticleSystemSimulationSpace.World ||
                rainRenderer.renderMode !=
                    ParticleSystemRenderMode.Stretch ||
                fogCurtains[0].WeatherToneWeight > 0f)
            {
                throw new InvalidOperationException(
                    "Fog weather particles or tone volume are invalid.");
            }
            if (runnerSteeringViews.Length != 1 ||
                !runnerSteeringViews[0].HasRequiredReference)
            {
                throw new InvalidOperationException(
                    "Runner steering presentation is missing or incomplete.");
            }
            if (iceRunways.Length != 1 ||
                !iceRunways[0].HasRequiredReferences() ||
                iceRunways[0].PanelCount != IceRunwayPanelCount)
            {
                throw new InvalidOperationException(
                    "Fixed Ice runway pool is missing or incomplete.");
            }
            if (experimentLaunchers.Length != 1 ||
                !experimentLaunchers[0].HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Development experiment launcher is missing or incomplete.");
            }
            if (splineTrackLabs.Length != 1 ||
                !splineTrackLabs[0].HasRequiredReferences ||
                splineLabControllers.Length != 1 ||
                !splineLabControllers[0].HasRequiredReferences ||
                splineTrackLabs[0].VisualsActive ||
                splineLabControllers[0].HudRoot.activeSelf)
            {
                throw new InvalidOperationException(
                    "Spline Track Lab is duplicated, incomplete, or visible in Campaign.");
            }
            splineTrackLabs[0].EvaluatePose(
                splineTrackLabs[0].PathLength * 0.25f,
                0f,
                out Vector3 firstCurvePoint,
                out Quaternion firstCurveRotation);
            splineTrackLabs[0].EvaluatePose(
                splineTrackLabs[0].PathLength * 0.55f,
                0f,
                out Vector3 secondCurvePoint,
                out Quaternion secondCurveRotation);
            if (Mathf.Abs(firstCurvePoint.x - secondCurvePoint.x) < 2f ||
                Mathf.Abs(firstCurvePoint.y) > 0.01f ||
                Mathf.Abs(secondCurvePoint.y) > 0.01f ||
                Quaternion.Angle(firstCurveRotation, secondCurveRotation) < 5f)
            {
                throw new InvalidOperationException(
                    "Spline Lab must contain a horizontal S-curve with changing yaw.");
            }
            if (gateBreakPools.Length != 1 ||
                !gateBreakPools[0].HasRequiredReferences() ||
                gateBreakPools[0].Capacity != GatePoolSize)
            {
                throw new InvalidOperationException(
                    "Fixed gate break effect pool is missing or incomplete.");
            }

            string[] uniqueNames =
            {
                "Canvas",
                "SafeAreaRoot",
                "GameplayTapSurface",
                "LobbyRoot",
                "PreRunRoot",
                "GameplayHudRoot",
                "CountdownRoot",
                "ClearResultRoot",
                "FailedResultRoot",
                "DevelopmentDebugRoot",
                "SplineTrackLab",
                "SplineTrackVisuals",
                "SplineTrackSurface",
                "SplineTrackLabHud",
                "LobbyPanel",
                "LobbyCurrentStage",
                "LobbyStageTitle",
                "LobbyStageDescription",
                "LobbyPlayButton",
                "ExperimentLabButton",
                "ResetProgressButton",
                "ResetProgressConfirmation",
                "ConfirmResetProgressButton",
                "CancelResetProgressButton",
                "StageSelectPanel",
                "StageSelectTitle",
                "DeveloperUnlockAllButton",
                "ExperimentLauncherPanel",
                "ExperimentLauncherLabel",
                "ExperimentStartButton",
                "ExperimentLeaveButton",
                "PreRunItemPanel",
                "PreRunStatusText",
                "ShieldItemButton",
                "BoosterItemButton",
                "StartStageButton",
                "BackButton",
                "StartItemPurchaseModal",
                "StartItemPurchaseConfirmButton",
                "StartItemPurchaseCancelButton",
                "CountdownPanel",
                "CountdownText",
                "StageHud",
                "StageProgressFill",
                "ColorHudPanel",
                "BoosterMeter",
                "BoosterMeterLabel",
                "BoosterMeterTrack",
                "BoosterMeterFill",
                "BoosterEndWarning",
                "ShieldIcon",
                "ShieldVisual",
                "FogCurtain",
                "IceRunway",
                "Goal",
                "GoalPortalArtwork",
                "FinishLeftPost",
                "FinishRightPost",
                "FinishCrossbar",
                "FinishBanner",
                "FinishFloorLine",
                "StageClearPanel",
                "StageFailedPanel",
                "ClearContinueButton",
                "FailContinueStatusText",
                "TicketContinueButton",
                "CoinContinueButton",
                "RewardedContinueButton",
                "InsufficientCoinsPopup",
                "InsufficientCoinsCloseButton",
                "ReplayButton",
                "RetryButton",
                "PauseButton",
                "PauseOverlayRoot",
                "PauseDim",
                "PausePanel",
                "PauseResumeButton",
                "PauseRestartButton",
                "PauseSettingsButton",
                "PauseLobbyButton",
                "PauseModalRoot",
                "PauseModalConfirmButton",
                "PauseModalCancelButton",
                "SettingsPanel",
                "SettingsApplyButton",
                "SettingsCancelButton",
                "PauseTransitionBlocker",
                "SuccessParticles",
                "GateBreakEffectPool",
                "BoosterSpeedLines",
                "BoosterWarpCyan",
                "BoosterWarpGold",
                "EventSystem"
            };
            for (int index = 0; index < uniqueNames.Length; index++)
            {
                if (CountNamedTransforms(generatedRoot, uniqueNames[index]) != 1)
                {
                    throw new InvalidOperationException(
                        $"{uniqueNames[index]} is missing or duplicated.");
                }
            }
            for (int tileIndex = 0; tileIndex < 6; tileIndex++)
            {
                if (CountNamedTransforms(
                    generatedRoot,
                    $"ColorTile_{tileIndex}") != 1 ||
                    CountNamedTransforms(
                        generatedRoot,
                        $"ColorTileEmblem_{tileIndex}") != 1 ||
                    CountNamedTransforms(
                        generatedRoot,
                        $"ColorTileNextMarker_{tileIndex}") != 1)
                {
                    throw new InvalidOperationException(
                        "Color HUD tile set is missing or duplicated.");
                }
            }
            if (CountNamedTransforms(
                generatedRoot,
                "LobbyTier_0_Accent_0") != 0)
            {
                throw new InvalidOperationException(
                    "Meaningless Lobby accent blocks must not be generated.");
            }
            for (int stage = 1; stage <= StageCatalog.Count; stage++)
            {
                if (CountNamedTransforms(
                    generatedRoot,
                    $"StageButton_{stage:00}") != 1)
                {
                    throw new InvalidOperationException(
                        "Exactly five stage buttons are required.");
                }
            }
            for (int gateIndex = 0; gateIndex < gates.Length; gateIndex++)
            {
                if (gates[gateIndex].PartCount != 3 ||
                    gates[gateIndex].transform.Find("Left") == null ||
                    gates[gateIndex].transform.Find("Right") == null ||
                    gates[gateIndex].transform.Find("Top") == null)
                {
                    throw new InvalidOperationException(
                        "Every gate requires left, right, and top geometry.");
                }
            }

            Transform[] transforms =
                generatedRoot.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    transforms[index].gameObject) > 0)
                {
                    throw new InvalidOperationException(
                        $"Missing script on {transforms[index].name}.");
                }
            }

            if (PlayerSettings.defaultInterfaceOrientation !=
                UIOrientation.Portrait)
            {
                throw new InvalidOperationException(
                    "Default orientation is not Portrait.");
            }
            if (!HasActiveVolume(generatedRoot))
            {
                throw new InvalidOperationException(
                    "Theme 01 requires one active post-processing volume.");
            }
            if (!IsCameraPostProcessingEnabled(cameras[0]))
            {
                throw new InvalidOperationException(
                    "Theme 01 camera post-processing is disabled.");
            }
            ValidateTheme01ArtworkAxes(generatedRoot);
            ValidateRunnerRearReadability(generatedRoot);
            ValidateEmissiveContrast();
            ValidateBloomProfile();
            ValidateProtectionAndWarpEffects(generatedRoot);
        }

        private static void ValidateRunnerRearReadability(
            GameObject generatedRoot)
        {
            string[] requiredParts =
            {
                "HullShell",
                "ColorShell",
                "RearBumper",
                "RearThruster_L",
                "RearThruster_R",
                "RearThrusterGlow_L",
                "RearThrusterGlow_R",
                "RearChevronGlow_L",
                "RearChevronGlow_R",
                "RearLightBar",
                "SideFin_L",
                "SideFin_R"
            };
            for (int index = 0; index < requiredParts.Length; index++)
            {
                if (CountNamedTransforms(
                    generatedRoot,
                    requiredParts[index]) != 1)
                {
                    throw new InvalidOperationException(
                        $"Runner rear part {requiredParts[index]} is missing " +
                        "or duplicated.");
                }
            }

            Transform leftThruster = FindNamedTransform(
                generatedRoot.transform,
                "RearThrusterGlow_L");
            Transform rightThruster = FindNamedTransform(
                generatedRoot.transform,
                "RearThrusterGlow_R");
            Transform colorShell = FindNamedTransform(
                generatedRoot.transform,
                "ColorShell");
            RunnerColorView[] colorViews =
                generatedRoot.GetComponentsInChildren<RunnerColorView>(true);
            if (colorViews.Length != 1 ||
                !colorViews[0].HasRequiredReferences ||
                colorViews[0].EmissiveAccent.transform != colorShell ||
                colorViews[0].GlassShell.transform.name != "HullShell" ||
                colorViews[0].GlassShell.sharedMaterial == null ||
                colorViews[0].GlassShell.sharedMaterial.IsKeywordEnabled(
                    "_EMISSION") ||
                colorViews[0].GlassShell.sharedMaterial.GetFloat(
                    "_Smoothness") < 0.9f)
            {
                throw new InvalidOperationException(
                    "Runner glass color presentation is incomplete.");
            }
            if (leftThruster.position.z >= colorShell.position.z ||
                rightThruster.position.z >= colorShell.position.z ||
                leftThruster.position.x * rightThruster.position.x >= 0f ||
                !Mathf.Approximately(
                    Mathf.Abs(leftThruster.position.x),
                    Mathf.Abs(rightThruster.position.x)))
            {
                throw new InvalidOperationException(
                    "Runner rear silhouette does not face the chase camera. " +
                    $"Left={leftThruster.position}, " +
                    $"Right={rightThruster.position}, " +
                    $"Color={colorShell.position}.");
            }
        }

        private static void ValidateEmissiveContrast()
        {
            string[] emissiveNames =
            {
                "Red",
                "Blue",
                "Green",
                "Yellow",
                "Purple",
                "Cyan",
                "Neutral"
            };
            for (int index = 0; index < emissiveNames.Length; index++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(
                    GeneratedMaterialsFolder + "/" +
                    emissiveNames[index] + ".mat");
                if (material == null ||
                    !material.IsKeywordEnabled("_EMISSION") ||
                    material.GetColor("_EmissionColor").maxColorComponent <= 1f)
                {
                    throw new InvalidOperationException(
                        $"{emissiveNames[index]} must provide HDR emission.");
                }
            }

            Material dark = AssetDatabase.LoadAssetAtPath<Material>(
                GeneratedMaterialsFolder + "/Theme01DarkAlloy.mat");
            if (dark == null || dark.IsKeywordEnabled("_EMISSION") ||
                dark.GetColor("_EmissionColor").maxColorComponent > 0.001f)
            {
                throw new InvalidOperationException(
                    "Theme 01 dark alloy must remain non-emissive.");
            }
        }

        private static void ValidateBloomProfile()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(
                "Assets/Settings/SampleSceneProfile.asset");
            UnityEngine.Object bloom = Array.Find(
                assets,
                candidate => candidate != null &&
                    candidate.GetType().Name == "Bloom");
            if (bloom == null)
            {
                throw new InvalidOperationException(
                    "Theme 01 Bloom override is missing.");
            }

            SerializedObject serialized = new SerializedObject(bloom);
            SerializedProperty threshold = serialized.FindProperty(
                "threshold.m_Value");
            SerializedProperty intensity = serialized.FindProperty(
                "intensity.m_Value");
            SerializedProperty scatter = serialized.FindProperty(
                "scatter.m_Value");
            SerializedProperty iterations = serialized.FindProperty(
                "maxIterations.m_Value");
            if (threshold == null || intensity == null || scatter == null ||
                iterations == null || threshold.floatValue > 0.8f ||
                intensity.floatValue < 0.85f || scatter.floatValue < 0.58f ||
                iterations.intValue > 4)
            {
                throw new InvalidOperationException(
                    "Theme 01 Bloom readability profile is not configured.");
            }
        }

        private static void ValidateTheme01ArtworkAxes(GameObject generatedRoot)
        {
            string[] artworkNames =
            {
                "CyberOrbRunnerVisual",
                "GateArtwork",
                "TrackArtwork",
                "GoalPortalArtwork",
                "NeonCityBackdrop"
            };
            Transform[] transforms =
                generatedRoot.GetComponentsInChildren<Transform>(true);
            for (int nameIndex = 0;
                nameIndex < artworkNames.Length;
                nameIndex++)
            {
                bool found = false;
                for (int index = 0; index < transforms.Length; index++)
                {
                    Transform artwork = transforms[index];
                    if (artwork.name != artworkNames[nameIndex])
                    {
                        continue;
                    }

                    found = true;
                    if (artwork.localPosition.sqrMagnitude > 0.000001f ||
                        Quaternion.Angle(
                            artwork.localRotation,
                            Quaternion.identity) > 0.01f ||
                        (artwork.localScale - Vector3.one)
                            .sqrMagnitude > 0.000001f)
                    {
                        throw new InvalidOperationException(
                            $"Theme 01 artwork root is not normalized: " +
                            artwork.name + ".");
                    }
                }

                if (!found)
                {
                    throw new InvalidOperationException(
                        $"Theme 01 artwork root is missing: " +
                        artworkNames[nameIndex] + ".");
                }
            }

            Transform trackArtwork = FindNamedTransform(
                generatedRoot.transform,
                "TrackArtwork");
            Renderer[] renderers = trackArtwork == null
                ? Array.Empty<Renderer>()
                : trackArtwork.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException(
                    "Theme 01 track artwork has no renderers.");
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            if (bounds.size.x < 6.5f ||
                bounds.size.x > 8f ||
                bounds.size.y > 3f ||
                bounds.size.z < TrackSegmentLength - 0.5f ||
                bounds.size.z < bounds.size.y * 10f)
            {
                throw new InvalidOperationException(
                    $"Theme 01 track must be horizontal. Bounds={bounds.size}.");
            }
        }

        private static void ValidateProtectionAndWarpEffects(
            GameObject generatedRoot)
        {
            ProtectionFieldView[] fields =
                generatedRoot.GetComponentsInChildren<ProtectionFieldView>(true);
            if (fields.Length != 2)
            {
                throw new InvalidOperationException(
                    $"Expected two protection fields, found {fields.Length}.");
            }
            for (int index = 0; index < fields.Length; index++)
            {
                Renderer[] renderers =
                    fields[index].GetComponentsInChildren<Renderer>(true);
                if (!fields[index].HasRequiredReferences() ||
                    fields[index].RendererCount != 1 ||
                    renderers.Length != 1 ||
                    renderers[0].sharedMaterial == null ||
                    renderers[0].sharedMaterial.shader == null ||
                    renderers[0].sharedMaterial.shader.name !=
                        "ColorGateRunner/ProtectionField")
                {
                    throw new InvalidOperationException(
                        $"Protection field {fields[index].name} is incomplete.");
                }
                ParticleSystem collapse = fields[index].CollapseParticles;
                ParticleSystem.MainModule collapseMain = collapse.main;
                ParticleSystem.ShapeModule collapseShape = collapse.shape;
                ParticleSystemRenderer collapseRenderer =
                    collapse.GetComponent<ParticleSystemRenderer>();
                if (collapseMain.loop || collapseMain.playOnAwake ||
                    collapseMain.maxParticles != 36 ||
                    !Mathf.Approximately(
                        collapseMain.startLifetime.constant,
                        0.35f) ||
                    collapseShape.shapeType != ParticleSystemShapeType.Sphere ||
                    collapseRenderer.sharedMaterial == null)
                {
                    throw new InvalidOperationException(
                        $"Protection collapse {collapse.name} is incomplete.");
                }
            }

            Transform warpRoot = FindNamedTransform(
                generatedRoot.transform,
                "BoosterSpeedLines");
            Transform player = FindNamedTransform(
                generatedRoot.transform,
                "Player");
            ParticleSystem[] systems = warpRoot == null
                ? Array.Empty<ParticleSystem>()
                : warpRoot.GetComponentsInChildren<ParticleSystem>(true);
            if (systems.Length != 3)
            {
                throw new InvalidOperationException(
                    $"Warp Booster requires one root and two layers, found " +
                    systems.Length + ".");
            }
            if (player == null || warpRoot.parent != player)
            {
                throw new InvalidOperationException(
                    "Warp Booster must follow the runner, not the camera.");
            }
            int emittedCapacity = 0;
            for (int index = 0; index < systems.Length; index++)
            {
                if (systems[index].transform == warpRoot)
                {
                    continue;
                }
                ParticleSystem.MainModule main = systems[index].main;
                ParticleSystem.ShapeModule shape = systems[index].shape;
                ParticleSystemRenderer renderer =
                    systems[index].GetComponent<ParticleSystemRenderer>();
                emittedCapacity += main.maxParticles;
                if (!Mathf.Approximately(main.startLifetime.constant, 1.5f) ||
                    !Mathf.Approximately(main.startSpeed.constant, 35f) ||
                    main.simulationSpace !=
                        ParticleSystemSimulationSpace.World ||
                    shape.shapeType != ParticleSystemShapeType.Box ||
                    shape.scale.x < 6.3f ||
                    shape.scale.y < 3.1f ||
                    shape.scale.z < 18.9f ||
                    systems[index].transform.localPosition.z < 13.9f ||
                    renderer.renderMode != ParticleSystemRenderMode.Stretch ||
                    renderer.alignment !=
                        ParticleSystemRenderSpace.Velocity ||
                    renderer.sharedMaterial == null)
                {
                    throw new InvalidOperationException(
                        $"Warp layer {systems[index].name} is not configured.");
                }
            }
            if (emittedCapacity > 160)
            {
                throw new InvalidOperationException(
                    $"Warp Booster particle cap is {emittedCapacity}, max 160.");
            }
        }

        private static void RemovePreviousSceneObjects(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                GameObject root = roots[index];
                if (root.name == GeneratedRootName ||
                    root.name == "Main Camera" ||
                    root.name == "Directional Light" ||
                    root.name == "Global Volume" ||
                    root.name == "EventSystem")
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static Camera CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject(
                "GameplayCamera",
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.SetPositionAndRotation(
                CameraPosition,
                Quaternion.Euler(CameraRotation));
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;
            camera.tag = "MainCamera";
            return camera;
        }

        private static void CreateDirectionalLight(Transform parent)
        {
            GameObject lightObject = new GameObject(
                "DirectionalLight",
                typeof(Light));
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
        }

        private static void CreateTheme01Volume(
            Transform parent,
            Camera camera)
        {
            Type cameraDataType = FindLoadedType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            if (cameraDataType == null)
            {
                throw new InvalidOperationException(
                    "URP camera data type is unavailable.");
            }
            Component cameraData = camera.GetComponent(cameraDataType) ??
                camera.gameObject.AddComponent(cameraDataType);
            cameraDataType.GetProperty("renderPostProcessing")?.SetValue(
                cameraData,
                true);

            Type volumeType = FindLoadedType("UnityEngine.Rendering.Volume");
            if (volumeType == null)
            {
                throw new InvalidOperationException(
                    "Volume type is unavailable.");
            }
            ScriptableObject profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                "Assets/Settings/SampleSceneProfile.asset");
            if (profile == null)
            {
                throw new InvalidOperationException(
                    "Theme 01 post-processing profile is missing.");
            }
            GameObject volumeObject = new GameObject("Global Volume");
            volumeObject.transform.SetParent(parent, false);
            Component volume = volumeObject.AddComponent(volumeType);
            volumeType.GetProperty("isGlobal")?.SetValue(volume, true);
            volumeType.GetProperty("priority")?.SetValue(volume, 10f);
            volumeType.GetProperty("sharedProfile")?.SetValue(volume, profile);
        }

        private static SplineCityPoolView CreateTheme01City(
            Transform parent,
            Material darkMaterial,
            Material neonBlue,
            Material neonCyan)
        {
            GameObject city = InstantiateTheme01Model(
                "NeonCityBackdrop.fbx",
                parent,
                "NeonCityBackdrop");
            Renderer[] renderers = city.GetComponentsInChildren<Renderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                string name = renderers[index].name;
                renderers[index].sharedMaterial = name.Contains("Glow")
                    ? (index % 2 == 0 ? neonBlue : neonCyan)
                    : darkMaterial;
            }
            Transform[] bodies = new Transform[24];
            Transform[] glows = new Transform[24];
            int slot = 0;
            string[] sides = { "L", "R" };
            for (int side = 0; side < sides.Length; side++)
            {
                for (int index = 0; index < 12; index++)
                {
                    bodies[slot] = FindNamedTransform(
                        city.transform,
                        $"City_{sides[side]}_{index:00}");
                    glows[slot] = FindNamedTransform(
                        city.transform,
                        $"CityGlow_{sides[side]}_{index:00}");
                    if (bodies[slot] == null || glows[slot] == null)
                    {
                        throw new InvalidOperationException(
                            $"Neon City slot {sides[side]}_{index:00} " +
                            "is incomplete.");
                    }
                    slot++;
                }
            }
            SplineCityPoolView pool =
                city.AddComponent<SplineCityPoolView>();
            pool.Configure(bodies, glows);
            return pool;
        }

        private static GateBreakEffectPool CreateGateBreakEffectPool(
            Transform parent,
            Material material)
        {
            GameObject root = new GameObject(
                "GateBreakEffectPool",
                typeof(GateBreakEffectPool));
            root.transform.SetParent(parent, false);
            GateBreakEffectView[] views =
                new GateBreakEffectView[GatePoolSize];
            Vector3[] starts =
            {
                new Vector3(-2.5f, 1.5f, 0f),
                new Vector3(2.5f, 1.5f, 0f),
                new Vector3(-1.6f, 3f, 0f),
                new Vector3(0f, 3f, 0f),
                new Vector3(1.6f, 3f, 0f)
            };
            for (int slot = 0; slot < views.Length; slot++)
            {
                GameObject effectObject = new GameObject(
                    $"GateBreakEffect_{slot:00}",
                    typeof(GateBreakEffectView));
                effectObject.transform.SetParent(root.transform, false);
                Transform[] fragments = new Transform[starts.Length];
                Renderer[] renderers = new Renderer[starts.Length];
                for (int index = 0; index < starts.Length; index++)
                {
                    GameObject fragment = GameObject.CreatePrimitive(
                        PrimitiveType.Cube);
                    fragment.name = $"GlowFragment_{index:00}";
                    fragment.transform.SetParent(effectObject.transform, false);
                    fragment.transform.localPosition = starts[index];
                    fragment.transform.localScale = index < 2
                        ? new Vector3(0.45f, 1.25f, 0.28f)
                        : new Vector3(0.75f, 0.32f, 0.28f);
                    renderers[index] = fragment.GetComponent<Renderer>();
                    renderers[index].sharedMaterial = material;
                    fragments[index] = fragment.transform;
                    UnityEngine.Object.DestroyImmediate(
                        fragment.GetComponent<Collider>());
                }
                views[slot] = effectObject.GetComponent<GateBreakEffectView>();
                views[slot].Configure(fragments, renderers);
            }
            GateBreakEffectPool pool = root.GetComponent<GateBreakEffectPool>();
            pool.Configure(views);
            return pool;
        }

        private static TrackPoolController CreateTrackPool(
            Transform parent,
            Material roadMaterial,
            Material neonMaterial)
        {
            GameObject poolObject = new GameObject(
                "TrackPool",
                typeof(TrackPoolController));
            poolObject.transform.SetParent(parent, false);
            TrackSegmentView[] segments =
                new TrackSegmentView[TrackPoolSize];
            for (int index = 0; index < segments.Length; index++)
            {
                GameObject segment = new GameObject($"TrackSegment_{index:00}");
                segment.transform.SetParent(poolObject.transform, false);
                GameObject artwork = InstantiateTheme01Model(
                    "NeonTrackSegment.fbx",
                    segment.transform,
                    "TrackArtwork");
                ApplyThemeMaterials(artwork, roadMaterial, neonMaterial);

                Transform start = new GameObject("StartAnchor").transform;
                start.SetParent(segment.transform, false);
                start.localPosition = new Vector3(
                    0f,
                    0f,
                    -TrackSegmentLength * 0.5f);
                Transform end = new GameObject("EndAnchor").transform;
                end.SetParent(segment.transform, false);
                end.localPosition = new Vector3(
                    0f,
                    0f,
                    TrackSegmentLength * 0.5f);
                TrackSegmentView view =
                    segment.AddComponent<TrackSegmentView>();
                view.Configure(
                    start,
                    end,
                    false,
                    artwork.GetComponentsInChildren<Renderer>(true));
                segments[index] = view;
            }

            TrackPoolController pool =
                poolObject.GetComponent<TrackPoolController>();
            pool.Configure(segments, TrackSegmentLength, -20f, 20f);
            pool.ResetPool();
            return pool;
        }

        private static CampaignSplinePathView CreateCampaignSplinePath(
            Transform parent,
            Material surfaceMaterial,
            Material edgeMaterial)
        {
            GameObject pathObject = new GameObject(
                "CampaignSplinePath",
                typeof(SplineContainer),
                typeof(CampaignSplinePathView));
            pathObject.transform.SetParent(parent, false);
            SplineContainer container =
                pathObject.GetComponent<SplineContainer>();
            container.Spline = new Spline(
                new[]
                {
                    new float3(0f, 0f, 0f),
                    new float3(0f, 0f, 100f)
                },
                TangentMode.AutoSmooth,
                false);

            GameObject visualRoot = new GameObject(
                "CampaignSplineVisuals");
            visualRoot.transform.SetParent(pathObject.transform, false);
            GameObject surface = new GameObject(
                "CampaignSplineSurface",
                typeof(MeshFilter),
                typeof(MeshRenderer));
            surface.transform.SetParent(visualRoot.transform, false);
            MeshFilter meshFilter = surface.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = surface.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = new[]
            {
                surfaceMaterial,
                edgeMaterial
            };
            meshRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            CampaignSplinePathView view =
                pathObject.GetComponent<CampaignSplinePathView>();
            view.Configure(container, visualRoot, meshFilter);
            return view;
        }

        private static SplineTrackLabView CreateSplineTrackLab(
            Transform parent,
            Material surfaceMaterial,
            Material edgeMaterial)
        {
            GameObject labObject = new GameObject(
                "SplineTrackLab",
                typeof(SplineContainer),
                typeof(SplineTrackLabView));
            labObject.transform.SetParent(parent, false);
            SplineContainer container =
                labObject.GetComponent<SplineContainer>();
            float3[] knots =
            {
                new float3(0f, 0f, -8f),
                new float3(0f, 0f, 38f),
                new float3(11f, 0f, 83f),
                new float3(-12f, 0f, 130f),
                new float3(10f, 0f, 177f),
                new float3(-8f, 0f, 224f),
                new float3(0f, 0f, 286f)
            };
            container.Spline = new Spline(
                knots,
                TangentMode.AutoSmooth,
                false);

            GameObject visualRoot = new GameObject("SplineTrackVisuals");
            visualRoot.transform.SetParent(labObject.transform, false);
            GameObject surface = new GameObject(
                "SplineTrackSurface",
                typeof(MeshFilter),
                typeof(MeshRenderer));
            surface.transform.SetParent(visualRoot.transform, false);
            MeshFilter meshFilter = surface.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = surface.GetComponent<MeshRenderer>();
            Mesh mesh = CreateOrUpdateSplineTrackMesh(
                GeneratedSplineFolder + "/SplineTrackLab.asset",
                container,
                7f,
                144);
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterials = new[]
            {
                surfaceMaterial,
                edgeMaterial
            };
            meshRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            SplineTrackLabView view =
                labObject.GetComponent<SplineTrackLabView>();
            view.Configure(container, visualRoot, meshFilter, 7f);
            return view;
        }

        private static Mesh CreateOrUpdateSplineTrackMesh(
            string assetPath,
            SplineContainer container,
            float width,
            int sampleCount)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh == null)
            {
                mesh = new Mesh { name = "SplineTrackLab" };
                AssetDatabase.CreateAsset(mesh, assetPath);
            }
            mesh.Clear();
            int rowCount = sampleCount + 1;
            Vector3[] vertices = new Vector3[rowCount * 4];
            Vector2[] uvs = new Vector2[vertices.Length];
            int[] roadTriangles = new int[sampleCount * 6];
            int[] edgeTriangles = new int[sampleCount * 12];
            float halfWidth = width * 0.5f;
            float edgeWidth = 0.16f;
            for (int row = 0; row < rowCount; row++)
            {
                float t = row / (float)sampleCount;
                Vector3 position = container.EvaluatePosition(t);
                Vector3 tangent = container.EvaluateTangent(t);
                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
                if (right.sqrMagnitude < 0.0001f)
                {
                    right = Vector3.right;
                }
                int vertex = row * 4;
                vertices[vertex] = position - (right * halfWidth);
                vertices[vertex + 1] =
                    position - (right * (halfWidth - edgeWidth));
                vertices[vertex + 2] =
                    position + (right * (halfWidth - edgeWidth));
                vertices[vertex + 3] = position + (right * halfWidth);
                float v = t * 16f;
                uvs[vertex] = new Vector2(0f, v);
                uvs[vertex + 1] = new Vector2(0.02f, v);
                uvs[vertex + 2] = new Vector2(0.98f, v);
                uvs[vertex + 3] = new Vector2(1f, v);
            }
            for (int row = 0; row < sampleCount; row++)
            {
                int current = row * 4;
                int next = current + 4;
                int road = row * 6;
                roadTriangles[road] = current + 1;
                roadTriangles[road + 1] = next + 1;
                roadTriangles[road + 2] = current + 2;
                roadTriangles[road + 3] = current + 2;
                roadTriangles[road + 4] = next + 1;
                roadTriangles[road + 5] = next + 2;

                int edge = row * 12;
                edgeTriangles[edge] = current;
                edgeTriangles[edge + 1] = next;
                edgeTriangles[edge + 2] = current + 1;
                edgeTriangles[edge + 3] = current + 1;
                edgeTriangles[edge + 4] = next;
                edgeTriangles[edge + 5] = next + 1;
                edgeTriangles[edge + 6] = current + 2;
                edgeTriangles[edge + 7] = next + 2;
                edgeTriangles[edge + 8] = current + 3;
                edgeTriangles[edge + 9] = current + 3;
                edgeTriangles[edge + 10] = next + 2;
                edgeTriangles[edge + 11] = next + 3;
            }
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.subMeshCount = 2;
            mesh.SetTriangles(roadTriangles, 0);
            mesh.SetTriangles(edgeTriangles, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static void ValidateTrackPoolContinuity(
            TrackPoolController pool)
        {
            TrackSegmentView[] ordered = new TrackSegmentView[pool.SegmentCount];
            for (int index = 0; index < ordered.Length; index++)
            {
                ordered[index] = pool.GetSegment(index);
                float span = ordered[index].EndAnchorPosition.z -
                    ordered[index].StartAnchorPosition.z;
                if (!Mathf.Approximately(span, TrackSegmentLength))
                {
                    throw new InvalidOperationException(
                        $"Track segment {index} anchor span is {span}, " +
                        $"expected {TrackSegmentLength}.");
                }
            }

            Array.Sort(
                ordered,
                (left, right) => left.StartAnchorPosition.z.CompareTo(
                    right.StartAnchorPosition.z));
            for (int index = 1; index < ordered.Length; index++)
            {
                if (!Mathf.Approximately(
                    ordered[index - 1].EndAnchorPosition.z,
                    ordered[index].StartAnchorPosition.z))
                {
                    throw new InvalidOperationException(
                        $"Track pool gap or overlap between segments " +
                        $"{index - 1} and {index}.");
                }
            }
        }

        private static IceRunwayView CreateIceRunway(
            Transform parent,
            Material material)
        {
            GameObject runwayObject = new GameObject(
                "IceRunway",
                typeof(IceRunwayView));
            runwayObject.transform.SetParent(parent, false);
            Renderer[] panels = new Renderer[IceRunwayPanelCount];
            MeshFilter[] meshFilters = new MeshFilter[IceRunwayPanelCount];
            for (int index = 0; index < panels.Length; index++)
            {
                GameObject panel = new GameObject(
                    $"IceRunwayPanel_{index:00}",
                    typeof(MeshFilter),
                    typeof(MeshRenderer));
                panel.transform.SetParent(runwayObject.transform, false);
                MeshRenderer renderer = panel.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                panels[index] = renderer;
                meshFilters[index] = panel.GetComponent<MeshFilter>();
            }

            IceRunwayView runway =
                runwayObject.GetComponent<IceRunwayView>();
            runway.Configure(panels, meshFilters);
            return runway;
        }

        private static string DescribeMissingSerializedReferences(
            StageSceneController[] controllers)
        {
            if (controllers == null || controllers.Length != 1)
            {
                return "controller-count";
            }

            SerializedObject serialized =
                new SerializedObject(controllers[0]);
            SerializedProperty property = serialized.GetIterator();
            string missing = string.Empty;
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (property.propertyType !=
                    SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue != null ||
                    property.name == "m_Script")
                {
                    continue;
                }

                missing += string.IsNullOrEmpty(missing)
                    ? property.propertyPath
                    : "," + property.propertyPath;
            }

            return string.IsNullOrEmpty(missing)
                ? "none"
                : missing;
        }

        private static GameObject CreatePlayer(
            Transform parent,
            Material colorMaterial,
            Material darkMaterial,
            Material neonMaterial,
            Material glassMaterial,
            out Renderer colorRenderer,
            out RunnerColorView colorView,
            out RunnerSteeringView steeringView)
        {
            GameObject player = new GameObject(
                "Player",
                typeof(SphereCollider),
                typeof(Rigidbody));
            player.transform.SetParent(parent, false);
            player.transform.position = PlayerStartPosition;
            SphereCollider collider = player.GetComponent<SphereCollider>();
            collider.center = new Vector3(0f, 0.85f, 0f);
            collider.radius = 0.92f;
            GameObject artwork = InstantiateTheme01Model(
                "CyberOrbRunner.fbx",
                player.transform,
                "CyberOrbRunnerVisual");
            ApplyThemeMaterials(artwork, darkMaterial, neonMaterial);
            Transform shell = FindNamedTransform(artwork.transform, "ColorShell");
            if (shell == null || !shell.TryGetComponent(out colorRenderer))
            {
                throw new InvalidOperationException(
                    "CyberOrbRunner requires a ColorShell renderer.");
            }
            colorRenderer.sharedMaterial = colorMaterial;
            Transform hull = FindNamedTransform(artwork.transform, "HullShell");
            if (hull == null || !hull.TryGetComponent(out Renderer hullRenderer))
            {
                throw new InvalidOperationException(
                    "CyberOrbRunner requires a HullShell renderer.");
            }
            hullRenderer.sharedMaterial = glassMaterial;
            colorView = player.AddComponent<RunnerColorView>();
            colorView.Configure(colorRenderer, hullRenderer);
            colorView.ApplyMaterial(colorMaterial);
            steeringView = player.AddComponent<RunnerSteeringView>();
            steeringView.Configure(artwork.transform);
            Rigidbody body = player.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            return player;
        }

        private static TrailRenderer CreatePlayerTrail(
            Transform player,
            Material material)
        {
            TrailRenderer trail = player.gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.35f;
            trail.endWidth = 0f;
            trail.sharedMaterial = material;
            trail.emitting = false;
            return trail;
        }

        private static TimedFogCurtainView CreateFogCurtain(
            Transform parent,
            Transform camera,
            Material material,
            Material fogWispMaterial,
            Material rainMaterial,
            VolumeProfile weatherProfile)
        {
            GameObject curtain = new GameObject(
                "FogCurtain",
                typeof(TimedFogCurtainView));
            curtain.transform.SetParent(parent, false);
            Transform[] sections =
                new Transform[TimedFogCurtainView.RequiredSectionCount];
            List<Renderer> renderers = new List<Renderer>();
            for (int index = 0; index < sections.Length; index++)
            {
                GameObject section = InstantiateTheme01Model(
                    "FogBankSection.fbx",
                    curtain.transform,
                    $"FogBank_{index:00}");
                sections[index] = section.transform;
                Renderer[] sectionRenderers =
                    section.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0;
                    rendererIndex < sectionRenderers.Length;
                    rendererIndex++)
                {
                    Renderer renderer = sectionRenderers[rendererIndex];
                    renderer.sharedMaterial = material;
                    renderer.shadowCastingMode =
                        UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderers.Add(renderer);
                }
                section.transform.localScale = Vector3.one *
                    (1f + ((index % 4) * 0.08f));
            }
            ParticleSystem fogWisps = CreateFogWispParticles(
                camera,
                fogWispMaterial);
            ParticleSystem rain = CreateFogRainParticles(
                camera,
                rainMaterial);
            GameObject volumeObject = new GameObject(
                "FogWeatherVolume",
                typeof(Volume));
            volumeObject.transform.SetParent(curtain.transform, false);
            Volume volume = volumeObject.GetComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 20f;
            volume.weight = 0f;
            volume.sharedProfile = weatherProfile;
            TimedFogCurtainView view =
                curtain.GetComponent<TimedFogCurtainView>();
            view.Configure(
                sections,
                renderers.ToArray(),
                fogWisps,
                rain,
                volume);
            return view;
        }

        private static ParticleSystem CreateFogWispParticles(
            Transform camera,
            Material material)
        {
            GameObject emitter = new GameObject(
                "FogWeatherWisps",
                typeof(ParticleSystem));
            emitter.transform.SetParent(camera, false);
            emitter.transform.localPosition = new Vector3(0f, 1.5f, 14f);
            ParticleSystem particles = emitter.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.duration = 4f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.4f, 3.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 6.5f);
            main.startColor = new Color(1f, 1f, 1f, 0.45f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = TimedFogCurtainView.FogWispParticleCapacity;
            main.useUnscaledTime = false;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(12f, 7f, 20f);
            shape.randomDirectionAmount = 1f;
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = CreateTwoEndedFade(0.32f);
            ParticleSystemRenderer renderer =
                emitter.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.maxParticleSize = 0.32f;
            renderer.sharedMaterial = material;
            return particles;
        }

        private static ParticleSystem CreateFogRainParticles(
            Transform camera,
            Material material)
        {
            GameObject emitter = new GameObject(
                "FogWeatherRain",
                typeof(ParticleSystem));
            emitter.transform.SetParent(camera, false);
            emitter.transform.localPosition = new Vector3(0f, 7f, 14f);
            ParticleSystem particles = emitter.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.duration = 2f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.75f, 1.2f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.05f);
            main.startColor = new Color(1f, 1f, 1f, 0.72f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = TimedFogCurtainView.RainParticleCapacity;
            main.useUnscaledTime = false;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18f, 8f, 28f);
            shape.randomDirectionAmount = 0f;
            ParticleSystem.VelocityOverLifetimeModule velocity =
                particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = -28f;
            velocity.z = -4f;
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            colorOverLifetime.color = CreateTwoEndedFade(0.72f);
            ParticleSystemRenderer renderer =
                emitter.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.alignment = ParticleSystemRenderSpace.Velocity;
            renderer.lengthScale = 4.5f;
            renderer.velocityScale = 0.1f;
            renderer.maxParticleSize = 0.08f;
            renderer.sharedMaterial = material;
            return particles;
        }

        private static ParticleSystem.MinMaxGradient CreateTwoEndedFade(
            float maximumAlpha)
        {
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(maximumAlpha, 0.18f),
                    new GradientAlphaKey(maximumAlpha * 0.8f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            return new ParticleSystem.MinMaxGradient(fade);
        }

        private static GameObject CreateShieldVisual(
            Transform player,
            Material material,
            Material pulseMaterial,
            out ParticleSystem collapse)
        {
            return CreateProtectionField(
                "ShieldVisual",
                "ShieldFieldSurface",
                "ShieldFieldCollapse",
                player,
                material,
                pulseMaterial,
                CyanColor,
                2.9f,
                out collapse);
        }

        private static GameObject CreateEchoShellVisual(
            Transform player,
            Material material,
            Material pulseMaterial,
            out ParticleSystem collapse)
        {
            GameObject shell = CreateProtectionField(
                "EchoShellVisual",
                "EchoFieldSurface",
                "EchoFieldCollapse",
                player,
                material,
                pulseMaterial,
                BlueColor,
                2.65f,
                out collapse);
            shell.SetActive(false);
            return shell;
        }

        private static GameObject CreateProtectionField(
            string rootName,
            string surfaceName,
            string collapseName,
            Transform player,
            Material material,
            Material pulseMaterial,
            Color initialColor,
            float diameter,
            out ParticleSystem collapse)
        {
            GameObject root = new GameObject(
                rootName,
                typeof(ProtectionFieldView));
            root.transform.SetParent(player, false);
            root.transform.localPosition = new Vector3(0f, 0.85f, 0f);

            GameObject surface =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);
            surface.name = surfaceName;
            surface.transform.SetParent(root.transform, false);
            surface.transform.localScale = Vector3.one * diameter;
            Renderer renderer = surface.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            UnityEngine.Object.DestroyImmediate(surface.GetComponent<Collider>());

            collapse = CreateProtectionCollapse(
                collapseName,
                player,
                pulseMaterial,
                diameter);

            ProtectionFieldView view = root.GetComponent<ProtectionFieldView>();
            view.Configure(new[] { renderer }, collapse, initialColor);
            return root;
        }

        private static ParticleSystem CreateProtectionCollapse(
            string name,
            Transform player,
            Material material,
            float diameter)
        {
            GameObject effect = new GameObject(name, typeof(ParticleSystem));
            effect.transform.SetParent(player, false);
            effect.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.35f;
            main.startLifetime = 0.35f;
            main.startSpeed = 2.5f;
            main.startSize = 0.08f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 36;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(
                new[] { new ParticleSystem.Burst(0f, 32) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = diameter * 0.5f;
            shape.radiusThickness = 0.02f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = fade;

            ParticleSystemRenderer renderer =
                effect.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            return particles;
        }

        private static GameObject CreateGoal(
            Transform parent,
            Material darkMaterial,
            Material neonMaterial)
        {
            GameObject goal = new GameObject("Goal");
            goal.transform.SetParent(parent, false);
            GameObject artwork = InstantiateTheme01Model(
                "NeonGoalPortal.fbx",
                goal.transform,
                "GoalPortalArtwork");
            ApplyThemeMaterials(artwork, darkMaterial, neonMaterial);
            CreateFinishPart(
                "FinishLeftPost",
                goal.transform,
                new Vector3(-2.7f, 2f, 0f),
                new Vector3(0.08f, 3.7f, 0.08f),
                neonMaterial);
            CreateFinishPart(
                "FinishRightPost",
                goal.transform,
                new Vector3(2.7f, 2f, 0f),
                new Vector3(0.08f, 3.7f, 0.08f),
                neonMaterial);
            CreateFinishPart(
                "FinishCrossbar",
                goal.transform,
                new Vector3(0f, 4f, 0f),
                new Vector3(5.1f, 0.08f, 0.08f),
                neonMaterial);
            CreateFinishPart(
                "FinishFloorLine",
                goal.transform,
                new Vector3(0f, 0.06f, 0f),
                new Vector3(5.8f, 0.08f, 0.55f),
                neonMaterial);

            GameObject banner = new GameObject(
                "FinishBanner",
                typeof(TextMesh));
            banner.transform.SetParent(goal.transform, false);
            banner.transform.localPosition = new Vector3(0f, 4f, -0.25f);
            banner.transform.localRotation = Quaternion.identity;
            TextMesh text = banner.GetComponent<TextMesh>();
            text.text = "GOAL";
            text.fontSize = 72;
            text.characterSize = 0.12f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            return goal;
        }

        private static void CreateFinishPart(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
        }

        private static StageGateView[] CreateGatePool(
            Transform parent,
            StageSceneController controller,
            Material darkMaterial,
            Material colorMaterial,
            Material protectionFieldMaterial,
            Material flickerFrameMaterial,
            Material flickerEmblemMaterial,
            Sprite defaultEmblem)
        {
            GameObject pool = new GameObject("StageGatePool");
            pool.transform.SetParent(parent, false);
            StageGateView[] gates = new StageGateView[GatePoolSize];
            for (int index = 0; index < gates.Length; index++)
            {
                GameObject gateObject = new GameObject(
                    $"StageGate_{index:00}",
                    typeof(BoxCollider),
                    typeof(StageGateView));
                gateObject.transform.SetParent(pool.transform, false);
                BoxCollider trigger = gateObject.GetComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(6f, 4f, 0.5f);

                GameObject artwork = InstantiateTheme01Model(
                    "NeonGate.fbx",
                    gateObject.transform,
                    "GateArtwork");
                ApplyThemeMaterials(artwork, darkMaterial, colorMaterial);
                string[] neonNames = { "LeftNeon", "RightNeon", "TopNeon" };
                string[] anchorNames = { "Left", "Right", "Top" };
                Renderer[] renderers = new Renderer[3];
                for (int part = 0; part < renderers.Length; part++)
                {
                    Transform source = FindNamedTransform(
                        artwork.transform,
                        neonNames[part]);
                    if (source == null || !source.TryGetComponent(out renderers[part]))
                    {
                        throw new InvalidOperationException(
                            $"NeonGate requires {neonNames[part]}.");
                    }
                    Transform anchor = new GameObject(anchorNames[part]).transform;
                    anchor.SetParent(gateObject.transform, false);
                }
                GameObject emblemObject = new GameObject(
                    "ColorEmblem",
                    typeof(SpriteRenderer));
                emblemObject.transform.SetParent(gateObject.transform, false);
                emblemObject.transform.localPosition =
                    new Vector3(0f, 3f, -0.40f);
                SpriteRenderer emblem =
                    emblemObject.GetComponent<SpriteRenderer>();
                emblem.sprite = defaultEmblem;
                emblem.color = Color.white;
                emblem.sortingOrder = 20;
                emblem.sharedMaterial = flickerEmblemMaterial;

                GameObject nextEmblemObject = new GameObject(
                    "NextColorEmblem",
                    typeof(SpriteRenderer));
                nextEmblemObject.transform.SetParent(
                    gateObject.transform,
                    false);
                nextEmblemObject.transform.localPosition =
                    emblemObject.transform.localPosition;
                SpriteRenderer nextEmblem =
                    nextEmblemObject.GetComponent<SpriteRenderer>();
                nextEmblem.sprite = defaultEmblem;
                nextEmblem.color = Color.white;
                nextEmblem.sortingOrder = 21;
                nextEmblem.sharedMaterial = flickerEmblemMaterial;
                FlickerGateEmblemView emblemTransition =
                    gateObject.AddComponent<FlickerGateEmblemView>();
                emblemTransition.Configure(
                    emblem,
                    nextEmblem,
                    flickerEmblemMaterial);

                GameObject markerObject = new GameObject(
                    "MechanicMarker",
                    typeof(TextMesh));
                markerObject.transform.SetParent(gateObject.transform, false);
                markerObject.transform.localPosition =
                    new Vector3(0f, 4.05f, -0.39f);
                TextMesh marker = markerObject.GetComponent<TextMesh>();
                marker.text = string.Empty;
                marker.anchor = TextAnchor.MiddleCenter;
                marker.alignment = TextAlignment.Center;
                marker.fontSize = 72;
                marker.characterSize = 0.025f;
                marker.color = Color.white;
                marker.gameObject.SetActive(false);

                GameObject fieldObject = GameObject.CreatePrimitive(
                    PrimitiveType.Quad);
                fieldObject.name = "EchoGateField";
                fieldObject.transform.SetParent(gateObject.transform, false);
                fieldObject.transform.localPosition =
                    new Vector3(0f, 1.55f, -0.3f);
                fieldObject.transform.localRotation = Quaternion.identity;
                fieldObject.transform.localScale =
                    new Vector3(4.5f, 2.65f, 1f);
                Renderer fieldRenderer = fieldObject.GetComponent<Renderer>();
                fieldRenderer.sharedMaterial = protectionFieldMaterial;
                fieldRenderer.shadowCastingMode =
                    UnityEngine.Rendering.ShadowCastingMode.Off;
                fieldRenderer.receiveShadows = false;
                UnityEngine.Object.DestroyImmediate(
                    fieldObject.GetComponent<Collider>());
                EchoGateFieldView echoField =
                    fieldObject.AddComponent<EchoGateFieldView>();
                echoField.Configure(fieldRenderer);

                FlickerGateFrameView flickerFrame =
                    gateObject.AddComponent<FlickerGateFrameView>();
                flickerFrame.Configure(
                    renderers[0],
                    renderers[2],
                    renderers[1],
                    flickerFrameMaterial);

                StageGateView view =
                    gateObject.GetComponent<StageGateView>();
                view.Configure(
                    controller,
                    renderers,
                    emblem,
                    marker,
                    echoField,
                    flickerFrame,
                    emblemTransition);
                gates[index] = view;
            }

            return gates;
        }

        private static ParticleSystem CreateParticleSystem(
            string name,
            Transform parent,
            Material material,
            bool looping)
        {
            GameObject effect = new GameObject(name, typeof(ParticleSystem));
            effect.transform.SetParent(parent, false);
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = looping;
            main.playOnAwake = false;
            main.duration = 0.4f;
            main.startLifetime = looping ? 0.35f : 0.25f;
            main.startSpeed = looping ? 12f : 4f;
            main.startSize = looping ? 0.08f : 0.18f;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = looping ? 45f : 0f;
            if (!looping)
            {
                emission.SetBursts(
                    new[] { new ParticleSystem.Burst(0f, 12) });
            }
            ParticleSystemRenderer renderer =
                effect.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            return particles;
        }

        private static ParticleSystem CreateBoosterSpeedLines(
            Transform player,
            Material cyanMaterial,
            Material goldMaterial)
        {
            GameObject root = new GameObject(
                "BoosterSpeedLines",
                typeof(ParticleSystem));
            root.transform.SetParent(player, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            ParticleSystem rootParticles = root.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule rootMain = rootParticles.main;
            rootMain.loop = true;
            rootMain.playOnAwake = false;
            rootMain.maxParticles = 1;
            ParticleSystem.EmissionModule rootEmission =
                rootParticles.emission;
            rootEmission.enabled = false;
            root.GetComponent<ParticleSystemRenderer>().enabled = false;

            CreateWarpEmitter(
                "BoosterWarpCyan",
                root.transform,
                cyanMaterial,
                55f,
                8.2f,
                6.5f,
                32f);
            CreateWarpEmitter(
                "BoosterWarpGold",
                root.transform,
                goldMaterial,
                40f,
                7.2f,
                5.5f,
                28f);
            return rootParticles;
        }

        private static void CreateWarpEmitter(
            string name,
            Transform parent,
            Material material,
            float emissionRate,
            float width,
            float height,
            float depth)
        {
            GameObject emitter = new GameObject(name, typeof(ParticleSystem));
            emitter.transform.SetParent(parent, false);
            emitter.transform.localPosition = new Vector3(0f, 2.2f, 18f);
            emitter.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            ParticleSystem particles = emitter.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.prewarm = true;
            main.playOnAwake = false;
            main.duration = 2f;
            main.startLifetime = 1.5f;
            main.startSpeed = 35f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.075f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = emissionRate;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(width, height, depth);
            shape.randomDirectionAmount = 0f;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime =
                particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient fade = new Gradient();
            fade.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.12f),
                    new GradientAlphaKey(0.9f, 0.78f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = fade;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime =
                particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.25f),
                new Keyframe(0.18f, 1f),
                new Keyframe(1f, 0.45f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f,
                sizeCurve);

            ParticleSystemRenderer renderer =
                emitter.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 7f;
            renderer.velocityScale = 0.22f;
            renderer.cameraVelocityScale = 0f;
            renderer.maxParticleSize = 0.12f;
            renderer.alignment = ParticleSystemRenderSpace.Velocity;
            renderer.sharedMaterial = material;
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Canvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static GameplayTapSurface CreateTapSurface(Transform parent)
        {
            GameObject surface = CreateUiObject("GameplayTapSurface", parent);
            Stretch(surface.GetComponent<RectTransform>());
            Image image = surface.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            return surface.AddComponent<GameplayTapSurface>();
        }

        private static Transform CreateSafeArea(Transform parent)
        {
            GameObject safe = CreateUiObject("SafeAreaRoot", parent);
            Stretch(safe.GetComponent<RectTransform>());
            safe.AddComponent<SafeAreaLayout>();
            return safe.transform;
        }

        private static GameObject CreateFlowRoot(string name, Transform parent)
        {
            GameObject root = CreateUiObject(name, parent);
            Stretch(root.GetComponent<RectTransform>());
            return root;
        }

        private static void CreateLobbyUi(
            Transform parent,
            out GameObject panel,
            out Text stage,
            out Text stageTitle,
            out Text stageDescription,
            out Text progress,
            out Text tier,
            out Button play,
            out Button experimentLab,
            out GameObject[] tierRoots,
            out Button resetProgress,
            out GameObject resetConfirmation,
            out Button confirmReset,
            out Button cancelReset)
        {
            panel = CreatePanel(
                "LobbyPanel",
                parent,
                new Color(0.035f, 0.055f, 0.09f, 0.97f));
            CreateText(
                "LobbyTitle",
                panel.transform,
                "COLOR GATE",
                64,
                new Vector2(0.1f, 0.86f),
                new Vector2(0.9f, 0.96f));
            stage = CreateText(
                "LobbyCurrentStage",
                panel.transform,
                "STAGE 1",
                42,
                new Vector2(0.12f, 0.64f),
                new Vector2(0.88f, 0.70f));
            stageTitle = CreateText(
                "LobbyStageTitle",
                panel.transform,
                "TWO-COLOR BASICS",
                38,
                new Vector2(0.1f, 0.54f),
                new Vector2(0.9f, 0.62f));
            stageDescription = CreateText(
                "LobbyStageDescription",
                panel.transform,
                "Learn the red and blue match.",
                25,
                new Vector2(0.12f, 0.47f),
                new Vector2(0.88f, 0.54f));
            progress = CreateText(
                "LobbyProgress",
                panel.transform,
                "0 / 5 CLEARED",
                28,
                new Vector2(0.15f, 0.30f),
                new Vector2(0.85f, 0.36f));
            tier = CreateText(
                "LobbyTierLabel",
                panel.transform,
                "LOBBY TIER 0",
                20,
                new Vector2(0.2f, 0.25f),
                new Vector2(0.8f, 0.30f));
            Text playLabel;
            play = CreateButton(
                "LobbyPlayButton",
                panel.transform,
                "PLAY",
                new Vector2(0.14f, 0.10f),
                new Vector2(0.86f, 0.23f),
                out playLabel);
            playLabel.fontSize = 46;
            Text labLabel;
            experimentLab = CreateButton(
                "ExperimentLabButton",
                panel.transform,
                "EXPERIMENT LAB",
                new Vector2(0.30f, 0.045f),
                new Vector2(0.70f, 0.085f),
                out labLabel);
            labLabel.fontSize = 16;
            Text resetLabel;
            resetProgress = CreateButton(
                "ResetProgressButton",
                panel.transform,
                "RESET PROGRESS",
                new Vector2(0.36f, 0.005f),
                new Vector2(0.64f, 0.035f),
                out resetLabel);
            resetLabel.fontSize = 16;
            tierRoots = new GameObject[4];
            Color[] colors =
            {
                new Color(0.15f, 0.18f, 0.26f, 1f),
                new Color(0.20f, 0.32f, 0.48f, 1f),
                new Color(0.22f, 0.48f, 0.38f, 1f),
                new Color(0.52f, 0.38f, 0.18f, 1f)
            };
            for (int index = 0; index < tierRoots.Length; index++)
            {
                GameObject decoration = CreatePanel(
                    $"LobbyTier_{index}",
                    panel.transform,
                    colors[index]);
                SetAnchors(
                    decoration.GetComponent<RectTransform>(),
                    Vector2.zero,
                    Vector2.one);
                decoration.transform.SetAsFirstSibling();
                Image decorationImage = decoration.GetComponent<Image>();
                decorationImage.color = new Color(
                    colors[index].r,
                    colors[index].g,
                    colors[index].b,
                    0.16f);
                decorationImage.raycastTarget = false;
                tierRoots[index] = decoration;
            }

            resetConfirmation = CreatePanel(
                "ResetProgressConfirmation",
                panel.transform,
                new Color(0.02f, 0.03f, 0.05f, 0.98f));
            SetAnchors(
                resetConfirmation.GetComponent<RectTransform>(),
                new Vector2(0.12f, 0.31f),
                new Vector2(0.88f, 0.63f));
            CreateText(
                "ResetProgressConfirmationText",
                resetConfirmation.transform,
                "RESET ALL STAGE PROGRESS?",
                28,
                new Vector2(0.08f, 0.62f),
                new Vector2(0.92f, 0.90f));
            Text confirmLabel;
            confirmReset = CreateButton(
                "ConfirmResetProgressButton",
                resetConfirmation.transform,
                "RESET",
                new Vector2(0.08f, 0.14f),
                new Vector2(0.47f, 0.48f),
                out confirmLabel);
            Text cancelLabel;
            cancelReset = CreateButton(
                "CancelResetProgressButton",
                resetConfirmation.transform,
                "CANCEL",
                new Vector2(0.53f, 0.14f),
                new Vector2(0.92f, 0.48f),
                out cancelLabel);
            resetConfirmation.SetActive(false);
        }

        private static void CreateStageSelectUi(
            Transform parent,
            out GameObject panel,
            out Button[] buttons,
            out Text[] summaries,
            out Button unlockAll)
        {
            panel = CreatePanel(
                "StageSelectPanel",
                parent,
                new Color(0.04f, 0.05f, 0.08f, 0.96f));
            CreateText(
                "StageSelectTitle",
                panel.transform,
                "SELECT STAGE",
                58,
                new Vector2(0.1f, 0.86f),
                new Vector2(0.9f, 0.96f));
            buttons = new Button[StageCatalog.Count];
            summaries = new Text[StageCatalog.Count];
            for (int index = 0; index < StageCatalog.Count; index++)
            {
                int row = index / 2;
                int column = index % 2;
                float top = 0.80f - (row * 0.12f);
                float left = column == 0 ? 0.08f : 0.52f;
                float right = column == 0 ? 0.48f : 0.92f;
                Text label;
                buttons[index] = CreateButton(
                    $"StageButton_{index + 1:00}",
                    panel.transform,
                    $"STAGE {index + 1}",
                    new Vector2(left, top - 0.095f),
                    new Vector2(right, top),
                    out label);
                label.alignment = TextAnchor.MiddleLeft;
                label.fontSize = 21;
                summaries[index] = label;
            }

            Text unlockLabel;
            unlockAll = CreateButton(
                "DeveloperUnlockAllButton",
                panel.transform,
                "DEV UNLOCK ALL",
                new Vector2(0.28f, 0.03f),
                new Vector2(0.72f, 0.08f),
                out unlockLabel);
        }

        private static void CreateItemUi(
            Transform parent,
            out GameObject panel,
            out Text selectedStage,
            out Button shieldButton,
            out Text shieldText,
            out Button boosterButton,
            out Text boosterText,
            out Text statusText,
            out Button startButton,
            out Button backButton,
            out GameObject purchaseModal,
            out Text purchaseTitle,
            out Text purchaseMessage,
            out Button purchaseConfirm,
            out Text purchaseConfirmText,
            out Button purchaseCancel)
        {
            panel = CreatePanel(
                "PreRunItemPanel",
                parent,
                new Color(0.04f, 0.05f, 0.08f, 0.96f));
            selectedStage = CreateText(
                "SelectedStageText",
                panel.transform,
                "STAGE 1",
                44,
                new Vector2(0.1f, 0.80f),
                new Vector2(0.9f, 0.94f));
            CreateText(
                "ChooseItemsText",
                panel.transform,
                "SELECT OWNED START ITEMS\n1 USED WHEN STARTING",
                27,
                new Vector2(0.1f, 0.68f),
                new Vector2(0.9f, 0.78f));
            shieldButton = CreateButton(
                "ShieldItemButton",
                panel.transform,
                "SHIELD: OFF",
                new Vector2(0.12f, 0.50f),
                new Vector2(0.88f, 0.64f),
                out shieldText);
            boosterButton = CreateButton(
                "BoosterItemButton",
                panel.transform,
                "BOOSTER: OFF",
                new Vector2(0.12f, 0.34f),
                new Vector2(0.88f, 0.48f),
                out boosterText);
            statusText = CreateText(
                "PreRunStatusText",
                panel.transform,
                string.Empty,
                18,
                new Vector2(0.12f, 0.28f),
                new Vector2(0.88f, 0.33f));
            statusText.color = new Color(1f, 0.42f, 0.38f, 1f);
            statusText.gameObject.SetActive(false);
            Text startLabel;
            startButton = CreateButton(
                "StartStageButton",
                panel.transform,
                "START",
                new Vector2(0.18f, 0.16f),
                new Vector2(0.82f, 0.27f),
                out startLabel);
            Text backLabel;
            backButton = CreateButton(
                "BackButton",
                panel.transform,
                "BACK",
                new Vector2(0.35f, 0.06f),
                new Vector2(0.65f, 0.12f),
                out backLabel);

            purchaseModal = CreateAnchoredPanel(
                "StartItemPurchaseModal",
                panel.transform,
                Vector2.zero,
                Vector2.one,
                new Color(0.01f, 0.02f, 0.05f, 0.94f));
            GameObject purchaseCard = CreateAnchoredPanel(
                "StartItemPurchaseCard",
                purchaseModal.transform,
                new Vector2(0.08f, 0.27f),
                new Vector2(0.92f, 0.73f),
                new Color(0.04f, 0.09f, 0.16f, 1f));
            purchaseTitle = CreateText(
                "StartItemPurchaseTitle",
                purchaseCard.transform,
                "BUY START ITEM",
                36,
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.92f));
            purchaseMessage = CreateText(
                "StartItemPurchaseMessage",
                purchaseCard.transform,
                string.Empty,
                25,
                new Vector2(0.08f, 0.40f),
                new Vector2(0.92f, 0.70f));
            purchaseConfirm = CreateButton(
                "StartItemPurchaseConfirmButton",
                purchaseCard.transform,
                "BUY 900",
                new Vector2(0.08f, 0.10f),
                new Vector2(0.48f, 0.34f),
                out purchaseConfirmText);
            purchaseCancel = CreateButton(
                "StartItemPurchaseCancelButton",
                purchaseCard.transform,
                "CANCEL",
                new Vector2(0.52f, 0.10f),
                new Vector2(0.92f, 0.34f),
                out Text _);
            purchaseModal.SetActive(false);
        }

        private static void CreateExperimentLauncherUi(
            Transform parent,
            StageSceneController controller)
        {
            GameObject panel = CreatePanel(
                "ExperimentLauncherPanel",
                parent,
                new Color(0.025f, 0.04f, 0.07f, 0.98f));
            SetAnchors(
                panel.GetComponent<RectTransform>(),
                new Vector2(0.08f, 0.08f),
                new Vector2(0.92f, 0.48f));
            ExperimentLauncher launcher =
                panel.AddComponent<ExperimentLauncher>();
            Text label = CreateText(
                "ExperimentLauncherLabel",
                panel.transform,
                "3 COLORS · NONE · SEED 12345",
                24,
                new Vector2(0.06f, 0.78f),
                new Vector2(0.94f, 0.96f));
            Text unused;
            Button previousColors = CreateButton(
                "ExperimentPreviousColorsButton",
                panel.transform,
                "COLORS -",
                new Vector2(0.06f, 0.58f),
                new Vector2(0.29f, 0.74f),
                out unused);
            Button nextColors = CreateButton(
                "ExperimentNextColorsButton",
                panel.transform,
                "COLORS +",
                new Vector2(0.31f, 0.58f),
                new Vector2(0.54f, 0.74f),
                out unused);
            Button mechanic = CreateButton(
                "ExperimentMechanicButton",
                panel.transform,
                "MECHANIC",
                new Vector2(0.56f, 0.58f),
                new Vector2(0.94f, 0.74f),
                out unused);
            Button seedDown = CreateButton(
                "ExperimentSeedDownButton",
                panel.transform,
                "SEED -",
                new Vector2(0.06f, 0.39f),
                new Vector2(0.29f, 0.54f),
                out unused);
            Button seedUp = CreateButton(
                "ExperimentSeedUpButton",
                panel.transform,
                "SEED +",
                new Vector2(0.31f, 0.39f),
                new Vector2(0.54f, 0.54f),
                out unused);
            Button shield = CreateButton(
                "ExperimentShieldButton",
                panel.transform,
                "SHIELD",
                new Vector2(0.56f, 0.39f),
                new Vector2(0.74f, 0.54f),
                out unused);
            Button booster = CreateButton(
                "ExperimentBoosterButton",
                panel.transform,
                "BOOST",
                new Vector2(0.76f, 0.39f),
                new Vector2(0.94f, 0.54f),
                out unused);
            Button start = CreateButton(
                "ExperimentStartButton",
                panel.transform,
                "START EXPERIMENT",
                new Vector2(0.06f, 0.20f),
                new Vector2(0.68f, 0.34f),
                out unused);
            Button splineLab = CreateButton(
                "SplineTrackLabButton",
                panel.transform,
                "SPLINE TRACK LAB",
                new Vector2(0.06f, 0.04f),
                new Vector2(0.68f, 0.17f),
                out unused);
            Button leave = CreateButton(
                "ExperimentLeaveButton",
                panel.transform,
                "LEAVE",
                new Vector2(0.72f, 0.04f),
                new Vector2(0.94f, 0.34f),
                out unused);
            launcher.Configure(
                controller,
                label,
                previousColors,
                nextColors,
                mechanic,
                seedDown,
                seedUp,
                shield,
                booster,
                start,
                splineLab,
                leave);
        }

        private static void CreateSplineLabHud(
            Transform parent,
            out GameObject root,
            out Text title,
            out Text progress,
            out Text instruction,
            out Button restart,
            out Button exit)
        {
            root = CreatePanel(
                "SplineTrackLabHud",
                parent,
                new Color(0.01f, 0.02f, 0.04f, 0.76f));
            SetAnchors(
                root.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.84f),
                new Vector2(0.96f, 0.97f));
            title = CreateText(
                "SplineTrackLabTitle",
                root.transform,
                "SPLINE TRACK LAB",
                24,
                new Vector2(0.03f, 0.54f),
                new Vector2(0.42f, 0.95f));
            title.alignment = TextAnchor.MiddleLeft;
            progress = CreateText(
                "SplineTrackLabProgress",
                root.transform,
                "0 / 12 · RED",
                22,
                new Vector2(0.43f, 0.54f),
                new Vector2(0.72f, 0.95f));
            instruction = CreateText(
                "SplineTrackLabInstruction",
                root.transform,
                "TAP TO SWITCH COLOR · HORIZONTAL S-CURVE",
                17,
                new Vector2(0.03f, 0.05f),
                new Vector2(0.72f, 0.50f));
            instruction.alignment = TextAnchor.MiddleLeft;
            Text unused;
            restart = CreateButton(
                "SplineTrackLabRestartButton",
                root.transform,
                "RESTART",
                new Vector2(0.73f, 0.52f),
                new Vector2(0.97f, 0.94f),
                out unused);
            exit = CreateButton(
                "SplineTrackLabExitButton",
                root.transform,
                "EXIT",
                new Vector2(0.73f, 0.06f),
                new Vector2(0.97f, 0.47f),
                out unused);
        }

        private static void CreateCountdownUi(
            Transform parent,
            out GameObject panel,
            out Text text)
        {
            panel = CreatePanel(
                "CountdownPanel",
                parent,
                new Color(0f, 0f, 0f, 0.35f));
            text = CreateText(
                "CountdownText",
                panel.transform,
                "3",
                120,
                new Vector2(0.15f, 0.35f),
                new Vector2(0.85f, 0.65f));
        }

        private static void CreateHud(
            Transform parent,
            Sprite[] colorEmblems,
            out GameObject panel,
            out Text stage,
            out Text progress,
            out Image fill,
            out GameObject shieldIcon,
            out GameObject boosterMeterRoot,
            out Image boosterMeter,
            out GameObject boosterWarning,
            out GameObject colorHudPanel,
            out GameObject[] colorTiles,
            out Image[] colorTileImages,
            out Image[] colorTileEmblems,
            out GameObject[] nextColorMarkers)
        {
            panel = CreatePanel(
                "StageHud",
                parent,
                new Color(0.02f, 0.03f, 0.05f, 0.72f));
            panel.GetComponent<Image>().raycastTarget = false;
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetAnchors(
                panelRect,
                new Vector2(0.04f, 0.86f),
                new Vector2(0.96f, 0.97f));
            stage = CreateText(
                "StageHudTitle",
                panel.transform,
                "STAGE 1",
                26,
                new Vector2(0.03f, 0.56f),
                new Vector2(0.28f, 0.96f));
            stage.alignment = TextAnchor.MiddleLeft;
            progress = CreateText(
                "StageProgressText",
                panel.transform,
                "0 / 24",
                30,
                new Vector2(0.35f, 0.56f),
                new Vector2(0.65f, 0.96f));
            GameObject bar = CreatePanel(
                "StageProgressBar",
                panel.transform,
                new Color(1f, 1f, 1f, 0.18f));
            bar.GetComponent<Image>().raycastTarget = false;
            SetAnchors(
                bar.GetComponent<RectTransform>(),
                new Vector2(0.28f, 0.35f),
                new Vector2(0.72f, 0.48f));
            GameObject fillObject = CreatePanel(
                "StageProgressFill",
                bar.transform,
                GreenColor);
            fill = fillObject.GetComponent<Image>();
            fill.raycastTarget = false;
            fill.type = Image.Type.Simple;
            fill.fillAmount = 0f;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);

            shieldIcon = CreatePanel(
                "ShieldIcon",
                panel.transform,
                new Color(0.18f, 0.55f, 1f, 0.9f));
            shieldIcon.GetComponent<Image>().raycastTarget = false;
            SetAnchors(
                shieldIcon.GetComponent<RectTransform>(),
                new Vector2(0.86f, 0.56f),
                new Vector2(0.94f, 0.94f));
            CreateText(
                "ShieldIconSymbol",
                shieldIcon.transform,
                "◇",
                24,
                Vector2.zero,
                Vector2.one);

            boosterMeterRoot = CreatePanel(
                "BoosterMeter",
                panel.transform,
                Color.clear);
            boosterMeterRoot.GetComponent<Image>().raycastTarget = false;
            SetAnchors(
                boosterMeterRoot.GetComponent<RectTransform>(),
                new Vector2(0.06f, 0.07f),
                new Vector2(0.94f, 0.27f));
            Text boostLabel = CreateText(
                "BoosterMeterLabel",
                boosterMeterRoot.transform,
                "BOOST",
                18,
                new Vector2(0f, 0f),
                new Vector2(0.15f, 1f));
            boostLabel.alignment = TextAnchor.MiddleLeft;
            GameObject meterTrack = CreatePanel(
                "BoosterMeterTrack",
                boosterMeterRoot.transform,
                new Color(1f, 1f, 1f, 0.22f));
            meterTrack.GetComponent<Image>().raycastTarget = false;
            SetAnchors(
                meterTrack.GetComponent<RectTransform>(),
                new Vector2(0.16f, 0.12f),
                new Vector2(0.70f, 0.88f));
            GameObject meterFill = CreatePanel(
                "BoosterMeterFill",
                meterTrack.transform,
                BlueColor);
            boosterMeter = meterFill.GetComponent<Image>();
            boosterMeter.raycastTarget = false;
            boosterMeter.type = Image.Type.Simple;
            boosterMeter.fillAmount = 0f;
            boosterMeter.rectTransform.anchorMax = new Vector2(0f, 1f);
            colorHudPanel = CreatePanel(
                "ColorHudPanel",
                parent,
                new Color(0.02f, 0.03f, 0.05f, 0.76f));
            colorHudPanel.GetComponent<Image>().raycastTarget = false;
            SetAnchors(
                colorHudPanel.GetComponent<RectTransform>(),
                new Vector2(0.12f, 0.055f),
                new Vector2(0.88f, 0.145f));
            colorTiles = new GameObject[6];
            colorTileImages = new Image[6];
            colorTileEmblems = new Image[6];
            nextColorMarkers = new GameObject[6];
            Color[] tileColors =
            {
                RedColor, BlueColor, GreenColor,
                NeutralColor, NeutralColor, NeutralColor
            };
            for (int index = 0; index < colorTiles.Length; index++)
            {
                GameObject tile = CreatePanel(
                    $"ColorTile_{index}",
                    colorHudPanel.transform,
                    tileColors[index]);
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.anchorMin = new Vector2(0.5f, 0.5f);
                tileRect.anchorMax = new Vector2(0.5f, 0.5f);
                tileRect.pivot = new Vector2(0.5f, 0.5f);
                tileRect.sizeDelta = new Vector2(92f, 52f);
                tileRect.anchoredPosition =
                    new Vector2(index == 0 ? 0f : index == 1 ? 76f : -76f, 0f);
                colorTiles[index] = tile;
                colorTileImages[index] = tile.GetComponent<Image>();
                colorTileImages[index].raycastTarget = false;
                GameObject emblemObject = new GameObject(
                    $"ColorTileEmblem_{index}",
                    typeof(RectTransform),
                    typeof(Image));
                emblemObject.transform.SetParent(tile.transform, false);
                RectTransform emblemRect =
                    emblemObject.GetComponent<RectTransform>();
                emblemRect.anchorMin = new Vector2(0.18f, 0.08f);
                emblemRect.anchorMax = new Vector2(0.82f, 0.92f);
                emblemRect.offsetMin = Vector2.zero;
                emblemRect.offsetMax = Vector2.zero;
                colorTileEmblems[index] =
                    emblemObject.GetComponent<Image>();
                colorTileEmblems[index].sprite = colorEmblems[index];
                colorTileEmblems[index].preserveAspect = true;
                colorTileEmblems[index].raycastTarget = false;
                colorTileEmblems[index].color = Color.white;
                Text nextLabel = CreateText(
                    $"ColorTileNextMarker_{index}",
                    tile.transform,
                    "NEXT",
                    14,
                    new Vector2(0f, 1.02f),
                    new Vector2(1f, 1.45f));
                nextLabel.alignment = TextAnchor.MiddleCenter;
                nextColorMarkers[index] = nextLabel.gameObject;
            }

            boosterWarning = CreateText(
                "BoosterEndWarning",
                panel.transform,
                "BOOST ENDING",
                20,
                new Vector2(0.70f, 0.02f),
                new Vector2(0.98f, 0.30f)).gameObject;
            boosterMeterRoot.SetActive(false);
            shieldIcon.SetActive(false);
            boosterWarning.SetActive(false);
        }

        private static void CreateResultUi(
            Transform parent,
            Sprite victorySprite,
            out GameObject panel,
            out Text titleText,
            out Text details,
            out Button next,
            out Button replay,
            out Button select,
            out GameObject celebrationRoot,
            out CanvasGroup celebrationGroup,
            out RectTransform victoryEmblem,
            out RectTransform[] victoryEmblemEchoes,
            out CanvasGroup[] victoryEmblemEchoGroups,
            out Button skipButton,
            out CanvasGroup skipPromptGroup,
            out GameObject rewardRoot,
            out CanvasGroup rewardGroup,
            out RectTransform[] fireworkSparks,
            out CanvasGroup[] fireworkSparkGroups,
            out GameObject[] rewardRows,
            out CanvasGroup[] rewardRowGroups,
            out Image[] rewardRowIcons,
            out Text[] rewardRowTexts)
        {
            panel = CreatePanel(
                "StageClearPanel",
                parent,
                new Color(0.015f, 0.025f, 0.055f, 0.97f));

            celebrationRoot = CreateUiObject(
                "ClearCelebrationRoot",
                panel.transform);
            Stretch(celebrationRoot.GetComponent<RectTransform>());
            celebrationGroup = celebrationRoot.AddComponent<CanvasGroup>();

            fireworkSparks = new RectTransform[24];
            fireworkSparkGroups = new CanvasGroup[24];
            Color[] sparkColors =
            {
                CyanColor,
                WarpGoldColor,
                new Color(0.95f, 0.22f, 0.82f, 1f)
            };
            for (int index = 0; index < fireworkSparks.Length; index++)
            {
                GameObject spark = CreateUiObject(
                    $"VictorySpark_{index}",
                    celebrationRoot.transform);
                RectTransform rect = spark.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.58f);
                rect.anchorMax = rect.anchorMin;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(
                    index % 3 == 0 ? 14f : 9f,
                    index % 3 == 0 ? 34f : 22f);
                Image sparkImage = spark.AddComponent<Image>();
                sparkImage.color = sparkColors[index % sparkColors.Length];
                sparkImage.raycastTarget = false;
                fireworkSparks[index] = rect;
                fireworkSparkGroups[index] = spark.AddComponent<CanvasGroup>();
            }

            victoryEmblemEchoes = new RectTransform[2];
            victoryEmblemEchoGroups = new CanvasGroup[2];
            Color[] echoColors =
            {
                new Color(0.1f, 0.95f, 1f, 0.8f),
                new Color(1f, 0.72f, 0.12f, 0.72f)
            };
            for (int index = 0; index < victoryEmblemEchoes.Length; index++)
            {
                GameObject echoObject = CreateUiObject(
                    $"VictoryEmblemEcho_{index}",
                    celebrationRoot.transform);
                RectTransform echoRect =
                    echoObject.GetComponent<RectTransform>();
                SetAnchors(
                    echoRect,
                    new Vector2(0.19f, 0.38f),
                    new Vector2(0.81f, 0.76f));
                Image echoImage = echoObject.AddComponent<Image>();
                echoImage.sprite = victorySprite;
                echoImage.preserveAspect = true;
                echoImage.raycastTarget = false;
                echoImage.color = echoColors[index];
                victoryEmblemEchoes[index] = echoRect;
                victoryEmblemEchoGroups[index] =
                    echoObject.AddComponent<CanvasGroup>();
            }

            GameObject emblemObject = CreateUiObject(
                "VictoryEmblem",
                celebrationRoot.transform);
            victoryEmblem = emblemObject.GetComponent<RectTransform>();
            SetAnchors(
                victoryEmblem,
                new Vector2(0.19f, 0.38f),
                new Vector2(0.81f, 0.76f));
            Image emblemImage = emblemObject.AddComponent<Image>();
            emblemImage.sprite = victorySprite;
            emblemImage.preserveAspect = true;
            emblemImage.raycastTarget = false;

            titleText = CreateText(
                "StageClearTitle",
                celebrationRoot.transform,
                "STAGE CLEAR",
                62,
                new Vector2(0.08f, 0.77f),
                new Vector2(0.92f, 0.89f));
            titleText.color = new Color(0.83f, 0.98f, 1f, 1f);

            GameObject skipObject = CreateUiObject(
                "ClearSkipButton",
                celebrationRoot.transform);
            Stretch(skipObject.GetComponent<RectTransform>());
            Image skipImage = skipObject.AddComponent<Image>();
            skipImage.color = new Color(1f, 1f, 1f, 0.001f);
            skipButton = skipObject.AddComponent<Button>();
            skipButton.targetGraphic = skipImage;
            Text skipLabel = CreateText(
                "ClearSkipLabel",
                skipObject.transform,
                "TAP TO CONTINUE",
                22,
                new Vector2(0.2f, 0.08f),
                new Vector2(0.8f, 0.14f));
            skipLabel.color = new Color(0.68f, 0.84f, 0.92f, 1f);
            skipPromptGroup = skipLabel.gameObject.AddComponent<CanvasGroup>();

            rewardRoot = CreateAnchoredPanel(
                "ClearRewardCard",
                panel.transform,
                new Vector2(0.065f, 0.19f),
                new Vector2(0.935f, 0.86f),
                Color.white);
            rewardGroup = rewardRoot.AddComponent<CanvasGroup>();
            CreateText(
                "ClearRewardHeading",
                rewardRoot.transform,
                "REWARDS",
                38,
                new Vector2(0.08f, 0.83f),
                new Vector2(0.92f, 0.96f));
            details = CreateText(
                "StageClearDetails",
                rewardRoot.transform,
                string.Empty,
                24,
                new Vector2(0.08f, 0.68f),
                new Vector2(0.92f, 0.83f));
            details.color = new Color(0.76f, 0.88f, 0.96f, 1f);

            rewardRows = new GameObject[5];
            rewardRowGroups = new CanvasGroup[5];
            rewardRowIcons = new Image[5];
            rewardRowTexts = new Text[5];
            for (int index = 0; index < rewardRows.Length; index++)
            {
                float top = 0.655f - index * 0.115f;
                GameObject row = CreateAnchoredPanel(
                    $"ClearRewardRow_{index}",
                    rewardRoot.transform,
                    new Vector2(0.11f, top - 0.095f),
                    new Vector2(0.89f, top),
                    new Color(0.08f, 0.15f, 0.25f, 0.92f));
                rewardRows[index] = row;
                rewardRowGroups[index] = row.AddComponent<CanvasGroup>();
                rewardRowIcons[index] = Theme01UiSkinBuilder.AddStandaloneIcon(
                    $"ClearRewardIcon_{index}",
                    row.transform,
                    "Coin",
                    new Vector2(0.04f, 0.13f),
                    new Vector2(0.22f, 0.87f));
                rewardRowTexts[index] = CreateText(
                    $"ClearRewardText_{index}",
                    row.transform,
                    string.Empty,
                    27,
                    new Vector2(0.24f, 0f),
                    new Vector2(0.94f, 1f));
                rewardRowTexts[index].alignment = TextAnchor.MiddleLeft;
            }

            next = CreateButton(
                "ClearContinueButton",
                panel.transform,
                "NEXT STAGE",
                new Vector2(0.13f, 0.09f),
                new Vector2(0.87f, 0.17f),
                out Text _);
            replay = CreateButton(
                "ReplayButton",
                panel.transform,
                "REPLAY",
                new Vector2(0.13f, 0.09f),
                new Vector2(0.87f, 0.17f),
                out Text _);
            select = CreateButton(
                "ClearLobbyButton",
                panel.transform,
                "LOBBY",
                new Vector2(0.25f, 0.015f),
                new Vector2(0.75f, 0.075f),
                out Text _);
        }

        private static void CreateFailureUi(
            Transform parent,
            out GameObject panel,
            out Text title,
            out Text details,
            out Text continueStatus,
            out Button ticketContinueButton,
            out Button coinContinueButton,
            out Button rewardedContinueButton,
            out Button retry,
            out Button select,
            out GameObject insufficientCoinsPopup,
            out Text insufficientCoinsMessage,
            out Button insufficientCoinsCloseButton,
            out GameObject continueRoot,
            out GameObject exitConfirmationRoot,
            out Text exitMessage,
            out Button exitConfirm,
            out Button exitCancel,
            out GameObject consequenceRoot,
            out Text consequenceTitle,
            out Text consequenceMessage,
            out Button consequenceContinue,
            out GameObject finalChoiceRoot,
            out Text finalMessage)
        {
            panel = CreatePanel(
                "StageFailedPanel",
                parent,
                new Color(0.02f, 0.025f, 0.055f, 0.97f));
            continueRoot = CreateAnchoredPanel(
                "FailureOfferCard",
                panel.transform,
                new Vector2(0.06f, 0.18f),
                new Vector2(0.94f, 0.88f),
                Color.white);
            title = CreateText(
                "StageFailedTitle",
                continueRoot.transform,
                "STAGE FAILED",
                54,
                new Vector2(0.08f, 0.80f),
                new Vector2(0.92f, 0.94f));
            title.color = new Color(1f, 0.52f, 0.68f, 1f);
            details = CreateText(
                "StageFailedDetails",
                continueRoot.transform,
                string.Empty,
                30,
                new Vector2(0.08f, 0.61f),
                new Vector2(0.92f, 0.79f));
            continueStatus = CreateText(
                "FailContinueStatusText",
                continueRoot.transform,
                string.Empty,
                22,
                new Vector2(0.08f, 0.51f),
                new Vector2(0.92f, 0.60f));
            continueStatus.color = new Color(0.72f, 0.86f, 0.95f, 1f);
            Text ticketLabel;
            ticketContinueButton = CreateButton(
                "TicketContinueButton",
                continueRoot.transform,
                "CONTINUE TICKET x1",
                new Vector2(0.12f, 0.385f),
                new Vector2(0.88f, 0.485f),
                out ticketLabel);
            Text rewardedLabel;
            rewardedContinueButton = CreateButton(
                "RewardedContinueButton",
                continueRoot.transform,
                "WATCH AD TO CONTINUE",
                new Vector2(0.12f, 0.265f),
                new Vector2(0.88f, 0.365f),
                out rewardedLabel);
            Text coinLabel;
            coinContinueButton = CreateButton(
                "CoinContinueButton",
                continueRoot.transform,
                "CONTINUE 900 COINS",
                new Vector2(0.12f, 0.145f),
                new Vector2(0.88f, 0.245f),
                out coinLabel);
            Text retryLabel;
            retry = CreateButton(
                "RetryButton",
                panel.transform,
                "GIVE UP",
                new Vector2(0.13f, 0.09f),
                new Vector2(0.87f, 0.17f),
                out retryLabel);
            Text selectLabel;
            select = CreateButton(
                "FailLobbyButton",
                panel.transform,
                "LOBBY",
                new Vector2(0.25f, 0.015f),
                new Vector2(0.75f, 0.075f),
                out selectLabel);

            exitConfirmationRoot = CreateAnchoredPanel(
                "FailureExitConfirmation",
                panel.transform,
                new Vector2(0.08f, 0.29f),
                new Vector2(0.92f, 0.71f),
                Color.white);
            CreateText(
                "FailureExitTitle",
                exitConfirmationRoot.transform,
                "LEAVE THIS ATTEMPT?",
                40,
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.91f));
            exitMessage = CreateText(
                "FailureExitMessage",
                exitConfirmationRoot.transform,
                string.Empty,
                25,
                new Vector2(0.08f, 0.34f),
                new Vector2(0.92f, 0.70f));
            exitConfirm = CreateButton(
                "FailureExitConfirmButton",
                exitConfirmationRoot.transform,
                "GIVE UP",
                new Vector2(0.08f, 0.08f),
                new Vector2(0.48f, 0.28f),
                out Text _);
            exitCancel = CreateButton(
                "FailureExitCancelButton",
                exitConfirmationRoot.transform,
                "KEEP PLAYING",
                new Vector2(0.52f, 0.08f),
                new Vector2(0.92f, 0.28f),
                out Text _);

            consequenceRoot = CreateAnchoredPanel(
                "FailureConsequenceCard",
                panel.transform,
                new Vector2(0.08f, 0.29f),
                new Vector2(0.92f, 0.71f),
                Color.white);
            consequenceTitle = CreateText(
                "FailureConsequenceTitle",
                consequenceRoot.transform,
                string.Empty,
                40,
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.91f));
            consequenceMessage = CreateText(
                "FailureConsequenceMessage",
                consequenceRoot.transform,
                string.Empty,
                25,
                new Vector2(0.08f, 0.32f),
                new Vector2(0.92f, 0.70f));
            consequenceContinue = CreateButton(
                "FailureConsequenceContinueButton",
                consequenceRoot.transform,
                "CONTINUE",
                new Vector2(0.18f, 0.08f),
                new Vector2(0.82f, 0.28f),
                out Text _);

            finalChoiceRoot = CreateAnchoredPanel(
                "FailureFinalChoice",
                panel.transform,
                new Vector2(0.08f, 0.29f),
                new Vector2(0.92f, 0.73f),
                Color.white);
            CreateText(
                "FailureFinalTitle",
                finalChoiceRoot.transform,
                "TRY AGAIN?",
                44,
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.91f));
            finalMessage = CreateText(
                "FailureFinalMessage",
                finalChoiceRoot.transform,
                string.Empty,
                25,
                new Vector2(0.08f, 0.18f),
                new Vector2(0.92f, 0.70f));

            insufficientCoinsPopup = CreateAnchoredPanel(
                "InsufficientCoinsPopup",
                parent,
                new Vector2(0.10f, 0.31f),
                new Vector2(0.90f, 0.69f),
                new Color(0.03f, 0.04f, 0.08f, 0.99f));
            CreateText(
                "InsufficientCoinsTitle",
                insufficientCoinsPopup.transform,
                "NOT ENOUGH COINS",
                44,
                new Vector2(0.08f, 0.68f),
                new Vector2(0.92f, 0.90f));
            insufficientCoinsMessage = CreateText(
                "InsufficientCoinsMessage",
                insufficientCoinsPopup.transform,
                string.Empty,
                27,
                new Vector2(0.08f, 0.34f),
                new Vector2(0.92f, 0.66f));
            insufficientCoinsCloseButton = CreateButton(
                "InsufficientCoinsCloseButton",
                insufficientCoinsPopup.transform,
                "CLOSE",
                new Vector2(0.20f, 0.08f),
                new Vector2(0.80f, 0.29f),
                out Text _);
            insufficientCoinsPopup.SetActive(false);
            exitConfirmationRoot.SetActive(false);
            consequenceRoot.SetActive(false);
            finalChoiceRoot.SetActive(false);
        }

        private static void CreatePauseUi(
            Transform canvas,
            Transform hud,
            out Button pauseButton,
            out GameObject overlayRoot,
            out Image dim,
            out GameObject panel,
            out Button resume,
            out Button restart,
            out Button settings,
            out Button lobby,
            out GameObject modalRoot,
            out Text modalTitle,
            out Text modalMessage,
            out Button modalConfirm,
            out Text modalConfirmText,
            out Button modalCancel,
            out SettingsPanelController settingsPanel,
            out GameObject transitionBlocker)
        {
            pauseButton = CreateButton(
                "PauseButton",
                hud,
                "II",
                new Vector2(0.83f, 0.76f),
                new Vector2(0.96f, 0.84f),
                out _);

            overlayRoot = CreateUiObject("PauseOverlayRoot", canvas);
            Stretch(overlayRoot.GetComponent<RectTransform>());
            GameObject dimObject = CreatePanel(
                "PauseDim",
                overlayRoot.transform,
                new Color(0f, 0f, 0f, 0.85f));
            dim = dimObject.GetComponent<Image>();
            dim.raycastTarget = true;

            GameObject safeArea = CreateUiObject(
                "PauseSafeAreaRoot",
                overlayRoot.transform);
            Stretch(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<SafeAreaLayout>();

            panel = CreateAnchoredPanel(
                "PausePanel",
                safeArea.transform,
                new Vector2(0.10f, 0.16f),
                new Vector2(0.90f, 0.84f),
                new Color(0.06f, 0.09f, 0.16f, 0.98f));
            CreateText(
                "PauseTitle",
                panel.transform,
                "PAUSED",
                38,
                new Vector2(0.08f, 0.80f),
                new Vector2(0.92f, 0.95f));
            resume = CreateButton(
                "PauseResumeButton",
                panel.transform,
                "RESUME",
                new Vector2(0.12f, 0.61f),
                new Vector2(0.88f, 0.75f),
                out _);
            restart = CreateButton(
                "PauseRestartButton",
                panel.transform,
                "RESTART",
                new Vector2(0.12f, 0.44f),
                new Vector2(0.88f, 0.58f),
                out _);
            settings = CreateButton(
                "PauseSettingsButton",
                panel.transform,
                "SETTINGS",
                new Vector2(0.12f, 0.27f),
                new Vector2(0.88f, 0.41f),
                out _);
            lobby = CreateButton(
                "PauseLobbyButton",
                panel.transform,
                "LOBBY",
                new Vector2(0.12f, 0.10f),
                new Vector2(0.88f, 0.24f),
                out _);

            modalRoot = CreateAnchoredPanel(
                "PauseModalRoot",
                safeArea.transform,
                new Vector2(0.08f, 0.25f),
                new Vector2(0.92f, 0.75f),
                new Color(0.08f, 0.12f, 0.20f, 1f));
            modalTitle = CreateText(
                "PauseModalTitle",
                modalRoot.transform,
                "CONFIRM",
                30,
                new Vector2(0.08f, 0.72f),
                new Vector2(0.92f, 0.94f));
            modalMessage = CreateText(
                "PauseModalMessage",
                modalRoot.transform,
                string.Empty,
                18,
                new Vector2(0.08f, 0.34f),
                new Vector2(0.92f, 0.70f));
            modalConfirm = CreateButton(
                "PauseModalConfirmButton",
                modalRoot.transform,
                "CONFIRM",
                new Vector2(0.08f, 0.08f),
                new Vector2(0.48f, 0.29f),
                out modalConfirmText);
            modalCancel = CreateButton(
                "PauseModalCancelButton",
                modalRoot.transform,
                "CANCEL",
                new Vector2(0.52f, 0.08f),
                new Vector2(0.92f, 0.29f),
                out _);

            settingsPanel = CreatePauseSettingsPanel(safeArea.transform);
            transitionBlocker = CreatePanel(
                "PauseTransitionBlocker",
                overlayRoot.transform,
                new Color(0f, 0f, 0f, 0.01f));
            transitionBlocker.GetComponent<Image>().raycastTarget = true;
        }

        private static SettingsPanelController CreatePauseSettingsPanel(
            Transform parent)
        {
            return UnifiedSettingsPanelBuilder.Create(parent);
        }

        private static Slider CreatePauseSettingSlider(
            string name,
            Transform parent,
            string label,
            float top,
            out Text valueText)
        {
            CreateText(
                name + "Label",
                parent,
                label,
                16,
                new Vector2(0.08f, top),
                new Vector2(0.35f, top + 0.09f));
            valueText = CreateText(
                name + "Value",
                parent,
                "100%",
                16,
                new Vector2(0.72f, top),
                new Vector2(0.92f, top + 0.09f));
            GameObject sliderObject = CreateAnchoredPanel(
                name,
                parent,
                new Vector2(0.34f, top + 0.025f),
                new Vector2(0.70f, top + 0.065f),
                new Color(0.15f, 0.19f, 0.28f, 1f));
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            GameObject fill = CreatePanel(
                "Fill",
                sliderObject.transform,
                BlueColor);
            GameObject handle = CreateAnchoredPanel(
                "Handle",
                sliderObject.transform,
                new Vector2(0f, -0.35f),
                new Vector2(0.08f, 1.35f),
                Color.white);
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            return slider;
        }

        private static Toggle CreatePauseSettingToggle(Transform parent)
        {
            GameObject toggleObject = CreateAnchoredPanel(
                "VibrationToggle",
                parent,
                new Vector2(0.10f, 0.24f),
                new Vector2(0.90f, 0.34f),
                new Color(0.15f, 0.19f, 0.28f, 1f));
            Toggle toggle = toggleObject.AddComponent<Toggle>();
            CreateText(
                "Label",
                toggleObject.transform,
                "VIBRATION",
                17,
                Vector2.zero,
                new Vector2(0.78f, 1f));
            Text check = CreateText(
                "Checkmark",
                toggleObject.transform,
                "ON",
                17,
                new Vector2(0.78f, 0f),
                Vector2.one);
            toggle.targetGraphic = toggleObject.GetComponent<Image>();
            toggle.graphic = check;
            toggle.isOn = true;
            return toggle;
        }

        private static GameObject CreateAnchoredPanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject panel = CreatePanel(name, parent, color);
            SetAnchors(
                panel.GetComponent<RectTransform>(),
                anchorMin,
                anchorMax);
            return panel;
        }

        private static void CreateEventSystem(Transform parent)
        {
            GameObject eventSystem = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(parent, false);
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Color color)
        {
            GameObject panel = CreateUiObject(name, parent);
            Stretch(panel.GetComponent<RectTransform>());
            Image image = panel.AddComponent<Image>();
            image.color = color;
            Theme01UiSkinBuilder.ApplyPanel(image, name);
            return panel;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            SetAnchors(text.rectTransform, anchorMin, anchorMax);
            return text;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string labelValue,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out Text label)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.22f, 0.34f, 1f);
            Button button = buttonObject.AddComponent<Button>();
            SetAnchors(
                buttonObject.GetComponent<RectTransform>(),
                anchorMin,
                anchorMax);
            label = CreateText(
                name + "Label",
                buttonObject.transform,
                labelValue,
                34,
                Vector2.zero,
                Vector2.one);
            Theme01UiSkinBuilder.ApplyButton(button, name, label);
            return button;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject result = new GameObject(name, typeof(RectTransform));
            result.transform.SetParent(parent, false);
            return result;
        }

        private static void Stretch(RectTransform rect)
        {
            SetAnchors(rect, Vector2.zero, Vector2.one);
        }

        private static void SetAnchors(
            RectTransform rect,
            Vector2 minimum,
            Vector2 maximum)
        {
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Material CreateOrUpdateMaterial(
            string path,
            Color color,
            float emissionIntensity)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.72f);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.78f);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                if (emissionIntensity > 0f)
                {
                    material.SetColor(
                        "_EmissionColor",
                        new Color(
                            color.r * emissionIntensity,
                            color.g * emissionIntensity,
                            color.b * emissionIntensity,
                            1f));
                    material.EnableKeyword("_EMISSION");
                    material.globalIlluminationFlags =
                        MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
                else
                {
                    material.SetColor("_EmissionColor", Color.black);
                    material.DisableKeyword("_EMISSION");
                    material.globalIlluminationFlags =
                        MaterialGlobalIlluminationFlags.None;
                }
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateRunnerGlassMaterial(
            string path,
            Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.15f);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.95f);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
            }
            material.DisableKeyword("_EMISSION");
            material.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject InstantiateTheme01Model(
            string filename,
            Transform parent,
            string instanceName)
        {
            string path = Theme01ModelsFolder + "/" + filename;
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null)
            {
                throw new InvalidOperationException(
                    $"Theme 01 model is missing: {path}");
            }
            GameObject instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Theme 01 model could not be instantiated: {path}");
            }
            instance.name = instanceName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        private static void ApplyThemeMaterials(
            GameObject root,
            Material darkMaterial,
            Material neonMaterial)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                string name = renderers[index].name;
                bool neon = name.Contains("Neon") || name.Contains("Glow") ||
                    name.Contains("Pulse") || name.Contains("ColorShell") ||
                    name.Contains("Arrow");
                renderers[index].sharedMaterial = neon
                    ? neonMaterial
                    : darkMaterial;
            }
        }

        private static Transform FindNamedTransform(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                {
                    return transforms[index];
                }
            }
            return null;
        }

        private static Type FindLoadedType(string fullName)
        {
            System.Reflection.Assembly[] assemblies =
                AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblies.Length; index++)
            {
                Type type = assemblies[index].GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        private static Material CreateOrUpdateMappedLitMaterial(
            string path,
            string baseTexturePath,
            string normalTexturePath,
            string surfaceTexturePath,
            Color tint,
            float metallic,
            float smoothness)
        {
            ConfigureTextureImporter(baseTexturePath, false, true);
            ConfigureTextureImporter(normalTexturePath, true, false);
            ConfigureTextureImporter(surfaceTexturePath, false, false);
            Texture2D baseTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(baseTexturePath);
            Texture2D normalTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(normalTexturePath);
            Texture2D surfaceTexture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(surfaceTexturePath);
            if (baseTexture == null || normalTexture == null ||
                surfaceTexture == null)
            {
                throw new InvalidOperationException(
                    $"Mapped surface textures are missing for {path}.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = tint;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }
            material.SetTexture("_BaseMap", baseTexture);
            material.SetTexture("_MainTex", baseTexture);
            material.SetTexture("_BumpMap", normalTexture);
            material.SetTexture("_MetallicGlossMap", surfaceTexture);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
            }
            material.DisableKeyword("_EMISSION");
            material.globalIlluminationFlags =
                MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureTextureImporter(
            string path,
            bool isNormalMap,
            bool isSrgb)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Texture importer is missing: {path}");
            }
            bool changed = false;
            TextureImporterType expectedType = isNormalMap
                ? TextureImporterType.NormalMap
                : TextureImporterType.Default;
            if (importer.textureType != expectedType)
            {
                importer.textureType = expectedType;
                changed = true;
            }
            if (importer.sRGBTexture != isSrgb)
            {
                importer.sRGBTexture = isSrgb;
                changed = true;
            }
            if (importer.wrapMode != TextureWrapMode.Repeat)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                changed = true;
            }
            if (!importer.mipmapEnabled)
            {
                importer.mipmapEnabled = true;
                changed = true;
            }
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static Material CreateOrUpdateTransparentMaterial(
            string path,
            Color color,
            string texturePath)
        {
            ConfigureTextureImporter(texturePath, false, true);
            Texture2D texture =
                AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                throw new InvalidOperationException(
                    $"Transparent texture is missing: {texturePath}");
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader =
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Standard");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            material.SetTexture("_BaseMap", texture);
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat(
                "_SrcBlend",
                (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat(
                "_DstBlend",
                (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateProtectionFieldMaterial(
            string path)
        {
            Shader shader = Shader.Find("ColorGateRunner/ProtectionField");
            if (shader == null)
            {
                throw new InvalidOperationException(
                    "ColorGateRunner/ProtectionField shader is missing.");
            }
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
            {
                if (message.severity.ToString() == "Error")
                {
                    throw new InvalidOperationException(
                        "ColorGateRunner/ProtectionField shader failed to " +
                        $"compile: {message.message} ({message.platform}).");
                }
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }
            material.SetColor("_FieldColor", CyanColor);
            material.SetFloat("_FieldPulse", 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateFlickerFrameMaterial(
            string path)
        {
            Shader shader = GetCheckedShader(
                "ColorGateRunner/FlickerGateFrameDissolve");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }
            material.SetColor("_CurrentColor", CyanColor);
            material.SetColor("_NextColor", RedColor);
            material.SetColor("_CurrentEmission", CyanColor * 4.5f);
            material.SetColor("_NextEmission", RedColor * 4.5f);
            material.SetFloat("_RevealProgress", 0f);
            material.SetFloat("_EdgeWidth", 0.065f);
            material.SetFloat("_NoiseAmount", 0.09f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateFlickerEmblemMaterial(
            string path)
        {
            Shader shader = GetCheckedShader(
                "ColorGateRunner/FlickerEmblemDissolve");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }
            material.SetColor("_Color", Color.white);
            material.SetColor("_EdgeColor", CyanColor);
            material.SetFloat("_DissolveProgress", 0f);
            material.SetFloat("_Incoming", 0f);
            material.SetFloat("_EdgeWidth", 0.075f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader GetCheckedShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException(
                    $"{shaderName} shader is missing.");
            }
            foreach (var message in ShaderUtil.GetShaderMessages(shader))
            {
                if (message.severity.ToString() == "Error")
                {
                    throw new InvalidOperationException(
                        $"{shaderName} shader failed to compile: " +
                        $"{message.message} ({message.platform}).");
                }
            }
            return shader;
        }

        private static Material CreateOrUpdateAdditiveParticleMaterial(
            string path,
            Color color)
        {
            Shader shader =
                Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Standard");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetFloat(
                "_SrcBlend",
                (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat(
                "_DstBlend",
                (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue =
                (int)UnityEngine.Rendering.RenderQueue.Transparent + 10;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateAlphaParticleMaterial(
            string path,
            Color color,
            string texturePath)
        {
            Shader shader =
                Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                Shader.Find("Universal Render Pipeline/Unlit") ??
                Shader.Find("Standard");
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            Texture2D texture = null;
            if (!string.IsNullOrWhiteSpace(texturePath))
            {
                ConfigureTextureImporter(texturePath, false, true);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    throw new InvalidOperationException(
                        $"Particle texture is missing: {texturePath}");
                }
            }
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (texture != null)
            {
                material.SetTexture("_BaseMap", texture);
                material.SetTexture("_MainTex", texture);
            }
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat(
                "_SrcBlend",
                (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat(
                "_DstBlend",
                (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue =
                (int)UnityEngine.Rendering.RenderQueue.Transparent + 5;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static VolumeProfile CreateOrUpdateFogWeatherProfile(
            string path)
        {
            VolumeProfile profile =
                AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            if (!profile.TryGet(out ColorAdjustments adjustments))
            {
                adjustments = profile.Add<ColorAdjustments>(true);
                AssetDatabase.AddObjectToAsset(adjustments, profile);
            }
            adjustments.active = true;
            adjustments.postExposure.Override(-0.45f);
            adjustments.contrast.Override(10f);
            adjustments.hueShift.Override(0f);
            adjustments.saturation.Override(-4f);
            adjustments.colorFilter.Override(Color.white);
            EditorUtility.SetDirty(adjustments);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static void EnsureAssetFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }
                current = next;
            }
        }

        private static bool HasActiveVolume(GameObject root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component is Behaviour behaviour &&
                    behaviour.enabled &&
                    component.GetType().Name == "Volume")
                {
                    return true;
                }
            }
            return false;
        }

        private static int CountNamedTransforms(GameObject root, string name)
        {
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                {
                    count++;
                }
            }
            return count;
        }

        internal static bool IsCameraPostProcessingEnabled(Camera camera)
        {
            Component[] components = camera.GetComponents<Component>();
            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component == null)
                {
                    continue;
                }
                Type type = component.GetType();
                if (type.Name == "UniversalAdditionalCameraData")
                {
                    var property = type.GetProperty("renderPostProcessing");
                    if (property != null &&
                        property.PropertyType == typeof(bool) &&
                        (bool)property.GetValue(component))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                left?.Replace('\\', '/'),
                right?.Replace('\\', '/'),
                StringComparison.OrdinalIgnoreCase);
        }

        private static Color FromHex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }
    }
}
