using System;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    public static class GrayboxSceneBuilder
    {
        internal const string ScenePath = "Assets/Scenes/SampleScene.unity";
        internal const string GeneratedRootName = "ColorGateRunner_Graybox";
        internal const string GeneratedMaterialsFolder =
            "Assets/Game/Generated/Materials";
        internal const int GatePoolSize = 6;
        internal const int TrackPoolSize = 6;
        internal const float TrackSegmentLength = 40f;

        internal static readonly Color RedColor = FromHex(0xE63946);
        internal static readonly Color BlueColor = FromHex(0x2D7FF9);
        internal static readonly Color GreenColor = FromHex(0x22C55E);
        internal static readonly Color YellowColor = FromHex(0xF4C430);
        internal static readonly Color PurpleColor = FromHex(0x9B5DE5);
        internal static readonly Color CyanColor = FromHex(0x00B8D9);
        internal static readonly Color NeutralColor = FromHex(0xD9D9D9);
        internal static readonly Color FailureColor = FromHex(0x6B7280);

        private static readonly Vector3 PlayerStartPosition =
            new Vector3(0f, 1f, 0f);
        private static readonly Vector3 CameraPosition =
            new Vector3(0f, 8f, -10f);
        private static readonly Vector3 CameraRotation =
            new Vector3(20f, 0f, 0f);

        [MenuItem("Tools/Color Gate Runner/Build Graybox Scene")]
        public static void BuildGrayboxScene()
        {
            string frontendPath = SelectFrontendScenePath(
                EditorBuildSettings.scenes);
            StageCatalogAsset stageCatalogAsset =
                StageCatalogAssetBuilder.EnsureAndConfigure();
            EnsureAssetFolder(GeneratedMaterialsFolder);
            Material red = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Red.mat",
                RedColor);
            Material blue = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Blue.mat",
                BlueColor);
            Material green = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Green.mat",
                GreenColor);
            Material yellow = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Yellow.mat",
                YellowColor);
            Material purple = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Purple.mat",
                PurpleColor);
            Material cyan = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Cyan.mat",
                CyanColor);
            Material neutral = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Neutral.mat",
                NeutralColor);
            Material failure = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Failure.mat",
                FailureColor);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemovePreviousSceneObjects(scene);

            GameObject root = new GameObject(GeneratedRootName);
            StageSceneController controller =
                root.AddComponent<StageSceneController>();
            Camera camera = CreateCamera(root.transform);
            CreateDirectionalLight(root.transform);
            TrackPoolController trackPool =
                CreateTrackPool(root.transform, neutral);
            GameObject playerObject =
                CreatePlayer(root.transform, red);
            Renderer playerRenderer = playerObject.GetComponent<Renderer>();
            Rigidbody playerBody = playerObject.GetComponent<Rigidbody>();
            TrailRenderer trail = CreatePlayerTrail(playerObject.transform, blue);
            GameObject shieldVisual =
                CreateShieldVisual(playerObject.transform, blue);
            GameObject echoShellVisual =
                CreateEchoShellVisual(playerObject.transform, blue);
            GameObject goal = CreateGoal(root.transform, neutral);
            StageGateView[] gates =
                CreateGatePool(root.transform, controller, neutral);
            ParticleSystem successParticles =
                CreateParticleSystem(
                    "SuccessParticles",
                    root.transform,
                    green,
                    false);
            ParticleSystem speedLines =
                CreateBoosterSpeedLines(camera.transform, blue);

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

            GameObject itemPanel;
            Text selectedStageText;
            Button shieldButton;
            Text shieldButtonText;
            Button boosterButton;
            Text boosterButtonText;
            Button startButton;
            Button backButton;
            CreateItemUi(
                flowRoots[1].transform,
                out itemPanel,
                out selectedStageText,
                out shieldButton,
                out shieldButtonText,
                out boosterButton,
                out boosterButtonText,
                out startButton,
                out backButton);

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
            Text[] colorTileSymbols;
            GameObject[] nextColorMarkers;
            CreateHud(
                flowRoots[2].transform,
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
                out colorTileSymbols,
                out nextColorMarkers);

            GameObject clearPanel;
            Text clearTitle;
            Text clearDetails;
            Button clearContinueButton;
            Button replayButton;
            Button clearLobbyButton;
            CreateResultUi(
                "StageClearPanel",
                "STAGE CLEAR",
                flowRoots[4].transform,
                true,
                out clearPanel,
                out clearTitle,
                out clearDetails,
                out clearContinueButton,
                out replayButton,
                out clearLobbyButton);

            GameObject failPanel;
            Text failTitle;
            Text failDetails;
            Button continueButton;
            Button retryButton;
            Button failLobbyButton;
            CreateFailureUi(
                flowRoots[5].transform,
                out failPanel,
                out failTitle,
                out failDetails,
                out continueButton,
                out retryButton,
                out failLobbyButton);

            CreatePauseUi(
                canvas.transform,
                hud.transform,
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
                playerBody,
                camera,
                red,
                blue,
                green,
                yellow,
                purple,
                cyan,
                failure,
                tapSurface,
                trackPool,
                gates,
                goal,
                shieldVisual,
                echoShellVisual,
                successParticles,
                speedLines,
                trail,
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
                startButton,
                backButton,
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
                colorTileSymbols,
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
                continueButton,
                retryButton,
                failLobbyButton);
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
                new[] { successParticles, speedLines },
                frontendPath);

            for (int index = 0; index < flowRoots.Length; index++)
            {
                flowRoots[index].SetActive(index == 0);
            }
            lobbyPanel.SetActive(true);
            stageSelectPanel.SetActive(true);
            itemPanel.SetActive(true);
            countdownPanel.SetActive(true);
            hud.SetActive(true);
            clearPanel.SetActive(true);
            failPanel.SetActive(true);
            pauseOverlayRoot.SetActive(false);
            pausePanel.SetActive(false);
            pauseModalRoot.SetActive(false);
            pauseSettingsPanel.gameObject.SetActive(false);
            pauseTransitionBlocker.SetActive(false);
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
            ExperimentLauncher[] experimentLaunchers =
                generatedRoot.GetComponentsInChildren<ExperimentLauncher>(true);

            if (controllers.Length != 1 ||
                !controllers[0].HasRequiredReferences() ||
                legacyControllers.Length != 0)
            {
                throw new InvalidOperationException(
                    "Stage controller is duplicated, incomplete, or legacy mode is exposed.");
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
            if (trackPools.Length != 1 ||
                !trackPools[0].HasRequiredReferences() ||
                trackPools[0].SegmentCount != TrackPoolSize)
            {
                throw new InvalidOperationException("Track pool is invalid.");
            }
            if (experimentLaunchers.Length != 1 ||
                !experimentLaunchers[0].HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Development experiment launcher is missing or incomplete.");
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
                "ShieldItemButton",
                "BoosterItemButton",
                "StartStageButton",
                "BackButton",
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
                "Goal",
                "FinishLeftPost",
                "FinishRightPost",
                "FinishCrossbar",
                "FinishBanner",
                "FinishFloorLine",
                "StageClearPanel",
                "StageFailedPanel",
                "ClearContinueButton",
                "ContinueButton",
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
                "BoosterSpeedLines",
                "BoosterSpeedLinesLeft",
                "BoosterSpeedLinesRight",
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
                        $"ColorTileSymbol_{tileIndex}") != 1 ||
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
            if (HasActiveVolume(generatedRoot))
            {
                throw new InvalidOperationException(
                    "An active post-processing volume remains.");
            }
            if (IsCameraPostProcessingEnabled(cameras[0]))
            {
                throw new InvalidOperationException(
                    "Camera post-processing remains enabled.");
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

        private static TrackPoolController CreateTrackPool(
            Transform parent,
            Material material)
        {
            GameObject poolObject = new GameObject(
                "TrackPool",
                typeof(TrackPoolController));
            poolObject.transform.SetParent(parent, false);
            TrackSegmentView[] segments =
                new TrackSegmentView[TrackPoolSize];
            for (int index = 0; index < segments.Length; index++)
            {
                GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"TrackSegment_{index:00}";
                segment.transform.SetParent(poolObject.transform, false);
                segment.transform.localScale =
                    new Vector3(7f, 0.2f, TrackSegmentLength);
                segment.GetComponent<Renderer>().sharedMaterial = material;
                UnityEngine.Object.DestroyImmediate(segment.GetComponent<Collider>());

                Transform start = new GameObject("StartAnchor").transform;
                start.SetParent(segment.transform, false);
                start.localPosition = new Vector3(0f, 0f, -0.5f);
                Transform end = new GameObject("EndAnchor").transform;
                end.SetParent(segment.transform, false);
                end.localPosition = new Vector3(0f, 0f, 0.5f);
                TrackSegmentView view =
                    segment.AddComponent<TrackSegmentView>();
                view.Configure(start, end, false);
                segments[index] = view;
            }

            TrackPoolController pool =
                poolObject.GetComponent<TrackPoolController>();
            pool.Configure(segments, TrackSegmentLength, -20f, 20f);
            pool.ResetPool();
            return pool;
        }

        private static GameObject CreatePlayer(
            Transform parent,
            Material material)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.SetParent(parent, false);
            player.transform.position = PlayerStartPosition;
            player.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            player.GetComponent<Renderer>().sharedMaterial = material;
            Rigidbody body = player.AddComponent<Rigidbody>();
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

        private static GameObject CreateShieldVisual(
            Transform player,
            Material material)
        {
            GameObject shield = new GameObject("ShieldVisual");
            shield.transform.SetParent(player, false);
            for (int index = 0; index < 6; index++)
            {
                GameObject segment =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"ShieldArc_{index:00}";
                segment.transform.SetParent(shield.transform, false);
                float angle = index * 60f * Mathf.Deg2Rad;
                segment.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 1.15f,
                    Mathf.Sin(angle) * 1.15f,
                    0f);
                segment.transform.localRotation =
                    Quaternion.Euler(0f, 0f, index * 60f);
                segment.transform.localScale =
                    new Vector3(0.7f, 0.12f, 0.12f);
                segment.GetComponent<Renderer>().sharedMaterial = material;
                UnityEngine.Object.DestroyImmediate(
                    segment.GetComponent<Collider>());
            }
            return shield;
        }

        private static GameObject CreateEchoShellVisual(
            Transform player,
            Material material)
        {
            GameObject shell = new GameObject("EchoShellVisual");
            shell.transform.SetParent(player, false);
            shell.transform.localScale = Vector3.one * 0.92f;
            for (int index = 0; index < 4; index++)
            {
                GameObject segment =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                segment.name = $"EchoShellSegment_{index:00}";
                segment.transform.SetParent(shell.transform, false);
                float angle = index * 90f * Mathf.Deg2Rad;
                segment.transform.localPosition = new Vector3(
                    Mathf.Cos(angle) * 0.82f,
                    Mathf.Sin(angle) * 0.82f,
                    -0.08f);
                segment.transform.localRotation =
                    Quaternion.Euler(0f, 0f, 45f + (index * 90f));
                segment.transform.localScale =
                    new Vector3(0.52f, 0.09f, 0.09f);
                segment.GetComponent<Renderer>().sharedMaterial = material;
                UnityEngine.Object.DestroyImmediate(
                    segment.GetComponent<Collider>());
            }
            shell.SetActive(false);
            return shell;
        }

        private static GameObject CreateGoal(Transform parent, Material material)
        {
            GameObject goal = new GameObject("Goal");
            goal.transform.SetParent(parent, false);
            CreateFinishPart(
                "FinishLeftPost",
                goal.transform,
                new Vector3(-2.7f, 2f, 0f),
                new Vector3(0.35f, 4f, 0.35f),
                material);
            CreateFinishPart(
                "FinishRightPost",
                goal.transform,
                new Vector3(2.7f, 2f, 0f),
                new Vector3(0.35f, 4f, 0.35f),
                material);
            CreateFinishPart(
                "FinishCrossbar",
                goal.transform,
                new Vector3(0f, 4f, 0f),
                new Vector3(5.75f, 0.55f, 0.45f),
                material);
            CreateFinishPart(
                "FinishFloorLine",
                goal.transform,
                new Vector3(0f, 0.06f, 0f),
                new Vector3(6f, 0.12f, 0.8f),
                material);

            GameObject banner = new GameObject(
                "FinishBanner",
                typeof(TextMesh));
            banner.transform.SetParent(goal.transform, false);
            banner.transform.localPosition = new Vector3(0f, 4f, -0.25f);
            banner.transform.localRotation = Quaternion.identity;
            TextMesh text = banner.GetComponent<TextMesh>();
            text.text = "FINISH";
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
            Material material)
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

                Renderer[] renderers = new Renderer[3];
                renderers[0] = CreateGatePart(
                    "Left",
                    gateObject.transform,
                    new Vector3(-2.5f, 1.5f, 0f),
                    material);
                renderers[1] = CreateGatePart(
                    "Right",
                    gateObject.transform,
                    new Vector3(2.5f, 1.5f, 0f),
                    material);
                renderers[2] = CreateGatePart(
                    "Top",
                    gateObject.transform,
                    new Vector3(0f, 3f, 0f),
                    material);
                renderers[2].transform.localScale =
                    new Vector3(5.65f, 0.65f, 0.65f);
                GameObject symbolObject = new GameObject(
                    "ColorSymbol",
                    typeof(TextMesh));
                symbolObject.transform.SetParent(gateObject.transform, false);
                symbolObject.transform.localPosition =
                    new Vector3(0f, 3f, -0.38f);
                TextMesh symbol = symbolObject.GetComponent<TextMesh>();
                symbol.text = "●";
                symbol.anchor = TextAnchor.MiddleCenter;
                symbol.alignment = TextAlignment.Center;
                symbol.fontSize = 96;
                symbol.characterSize = 0.035f;
                symbol.color = Color.white;
                StageGateView view =
                    gateObject.GetComponent<StageGateView>();
                view.Configure(controller, renderers, symbol);
                gates[index] = view;
            }

            return gates;
        }

        private static Renderer CreateGatePart(
            string name,
            Transform parent,
            Vector3 localPosition,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = new Vector3(0.65f, 3f, 0.65f);
            part.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            return part.GetComponent<Renderer>();
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
            Transform camera,
            Material material)
        {
            GameObject root = new GameObject(
                "BoosterSpeedLines",
                typeof(ParticleSystem));
            root.transform.SetParent(camera, false);
            ParticleSystem rootParticles = root.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule rootMain = rootParticles.main;
            rootMain.loop = false;
            rootMain.playOnAwake = false;
            ParticleSystem.EmissionModule rootEmission =
                rootParticles.emission;
            rootEmission.enabled = false;
            root.GetComponent<ParticleSystemRenderer>().enabled = false;

            CreateSpeedLineEmitter(
                "BoosterSpeedLinesLeft",
                root.transform,
                new Vector3(-3.4f, -0.8f, 5f),
                material);
            CreateSpeedLineEmitter(
                "BoosterSpeedLinesRight",
                root.transform,
                new Vector3(3.4f, -0.8f, 5f),
                material);
            return rootParticles;
        }

        private static void CreateSpeedLineEmitter(
            string name,
            Transform parent,
            Vector3 localPosition,
            Material material)
        {
            GameObject emitter = new GameObject(name, typeof(ParticleSystem));
            emitter.transform.SetParent(parent, false);
            emitter.transform.localPosition = localPosition;
            ParticleSystem particles = emitter.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.playOnAwake = false;
            main.duration = 0.5f;
            main.startLifetime = 0.3f;
            main.startSpeed = 18f;
            main.startSize = 0.06f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 32f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1.2f, 5f, 0.2f);
            ParticleSystemRenderer renderer =
                emitter.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3.5f;
            renderer.velocityScale = 0.25f;
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
                new Vector2(0.12f, 0.63f),
                new Vector2(0.88f, 0.70f));
            stageTitle = CreateText(
                "LobbyStageTitle",
                panel.transform,
                "TWO-COLOR BASICS",
                38,
                new Vector2(0.1f, 0.54f),
                new Vector2(0.9f, 0.63f));
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
            out Button startButton,
            out Button backButton)
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
                "CHOOSE START ITEMS\nFREE / UNLIMITED",
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
                new Vector2(0.06f, 0.12f),
                new Vector2(0.68f, 0.31f),
                out unused);
            Button leave = CreateButton(
                "ExperimentLeaveButton",
                panel.transform,
                "LEAVE",
                new Vector2(0.72f, 0.12f),
                new Vector2(0.94f, 0.31f),
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
                leave);
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
            out Text[] colorTileSymbols,
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
                new Vector2(0.035f, 0.55f),
                new Vector2(0.30f, 0.83f));
            colorTiles = new GameObject[6];
            colorTileImages = new Image[6];
            colorTileSymbols = new Text[6];
            nextColorMarkers = new GameObject[6];
            Color[] tileColors =
            {
                RedColor, BlueColor, GreenColor,
                NeutralColor, NeutralColor, NeutralColor
            };
            string[] symbols = { "●", "■", "▲", "★", "◆", "HEX" };
            for (int index = 0; index < colorTiles.Length; index++)
            {
                GameObject tile = CreatePanel(
                    $"ColorTile_{index}",
                    colorHudPanel.transform,
                    tileColors[index]);
                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.anchorMin = new Vector2(0.33f, 1f);
                tileRect.anchorMax = new Vector2(0.33f, 1f);
                tileRect.pivot = new Vector2(0.5f, 0.5f);
                tileRect.sizeDelta = new Vector2(92f, 52f);
                tileRect.anchoredPosition =
                    new Vector2(0f, -34f - (index * 42f));
                colorTiles[index] = tile;
                colorTileImages[index] = tile.GetComponent<Image>();
                colorTileImages[index].raycastTarget = false;
                colorTileSymbols[index] = CreateText(
                    $"ColorTileSymbol_{index}",
                    tile.transform,
                    symbols[index],
                    index == 5 ? 24 : 38,
                    Vector2.zero,
                    Vector2.one);
                Text nextLabel = CreateText(
                    $"ColorTileNextMarker_{index}",
                    tile.transform,
                    "NEXT",
                    17,
                    new Vector2(1.02f, 0f),
                    new Vector2(1.75f, 1f));
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
            string name,
            string title,
            Transform parent,
            bool includeNext,
            out GameObject panel,
            out Text titleText,
            out Text details,
            out Button next,
            out Button replay,
            out Button select)
        {
            panel = CreatePanel(
                name,
                parent,
                new Color(0.04f, 0.05f, 0.08f, 0.96f));
            titleText = CreateText(
                "StageClearTitle",
                panel.transform,
                title,
                58,
                new Vector2(0.1f, 0.76f),
                new Vector2(0.9f, 0.90f));
            details = CreateText(
                "StageClearDetails",
                panel.transform,
                string.Empty,
                32,
                new Vector2(0.1f, 0.46f),
                new Vector2(0.9f, 0.74f));
            Text nextLabel;
            next = CreateButton(
                "ClearContinueButton",
                panel.transform,
                "CONTINUE",
                new Vector2(0.16f, 0.31f),
                new Vector2(0.84f, 0.41f),
                out nextLabel);
            Text replayLabel;
            replay = CreateButton(
                "ReplayButton",
                panel.transform,
                "REPLAY",
                new Vector2(0.16f, 0.19f),
                new Vector2(0.84f, 0.29f),
                out replayLabel);
            Text selectLabel;
            select = CreateButton(
                "ClearLobbyButton",
                panel.transform,
                "LOBBY",
                new Vector2(0.16f, 0.07f),
                new Vector2(0.84f, 0.17f),
                out selectLabel);
        }

        private static void CreateFailureUi(
            Transform parent,
            out GameObject panel,
            out Text title,
            out Text details,
            out Button continueButton,
            out Button retry,
            out Button select)
        {
            panel = CreatePanel(
                "StageFailedPanel",
                parent,
                new Color(0.04f, 0.05f, 0.08f, 0.96f));
            title = CreateText(
                "StageFailedTitle",
                panel.transform,
                "STAGE FAILED",
                58,
                new Vector2(0.1f, 0.72f),
                new Vector2(0.9f, 0.88f));
            details = CreateText(
                "StageFailedDetails",
                panel.transform,
                string.Empty,
                32,
                new Vector2(0.1f, 0.46f),
                new Vector2(0.9f, 0.70f));
            Text continueLabel;
            continueButton = CreateButton(
                "ContinueButton",
                panel.transform,
                "CONTINUE",
                new Vector2(0.16f, 0.37f),
                new Vector2(0.84f, 0.49f),
                out continueLabel);
            Text retryLabel;
            retry = CreateButton(
                "RetryButton",
                panel.transform,
                "RETRY",
                new Vector2(0.16f, 0.23f),
                new Vector2(0.84f, 0.35f),
                out retryLabel);
            Text selectLabel;
            select = CreateButton(
                "FailLobbyButton",
                panel.transform,
                "LOBBY",
                new Vector2(0.16f, 0.09f),
                new Vector2(0.84f, 0.21f),
                out selectLabel);
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
                new Vector2(0.83f, 0.89f),
                new Vector2(0.96f, 0.97f),
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
            GameObject panel = CreateAnchoredPanel(
                "SettingsPanel",
                parent,
                new Vector2(0.07f, 0.12f),
                new Vector2(0.93f, 0.88f),
                new Color(0.09f, 0.13f, 0.21f, 1f));
            SettingsPanelController controller =
                panel.AddComponent<SettingsPanelController>();
            CreateText(
                "SettingsTitle",
                panel.transform,
                "SETTINGS",
                30,
                new Vector2(0.08f, 0.88f),
                new Vector2(0.92f, 0.98f));

            Slider master = CreatePauseSettingSlider(
                "MasterSlider",
                panel.transform,
                "MASTER",
                0.73f,
                out Text masterValue);
            Slider music = CreatePauseSettingSlider(
                "MusicSlider",
                panel.transform,
                "MUSIC",
                0.57f,
                out Text musicValue);
            Slider sfx = CreatePauseSettingSlider(
                "SfxSlider",
                panel.transform,
                "SFX",
                0.41f,
                out Text sfxValue);
            Toggle vibration = CreatePauseSettingToggle(
                panel.transform);
            Text status = CreateText(
                "SettingsStatusText",
                panel.transform,
                string.Empty,
                13,
                new Vector2(0.08f, 0.16f),
                new Vector2(0.92f, 0.23f));
            Button apply = CreateButton(
                "SettingsApplyButton",
                panel.transform,
                "APPLY",
                new Vector2(0.08f, 0.04f),
                new Vector2(0.48f, 0.14f),
                out _);
            Button cancel = CreateButton(
                "SettingsCancelButton",
                panel.transform,
                "CANCEL",
                new Vector2(0.52f, 0.04f),
                new Vector2(0.92f, 0.14f),
                out _);
            controller.Configure(
                master,
                music,
                sfx,
                vibration,
                masterValue,
                musicValue,
                sfxValue,
                status,
                apply,
                cancel);
            return controller;
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

        private static Material CreateOrUpdateMaterial(string path, Color color)
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
            EditorUtility.SetDirty(material);
            return material;
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
