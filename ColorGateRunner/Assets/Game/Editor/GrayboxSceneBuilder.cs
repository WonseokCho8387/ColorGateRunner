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
            TrailRenderer trail = CreatePlayerTrail(playerObject.transform, blue);
            GameObject shieldVisual =
                CreateShieldVisual(playerObject.transform, blue);
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
                CreateParticleSystem(
                    "BoosterSpeedLines",
                    camera.transform,
                    blue,
                    true);

            Canvas canvas = CreateCanvas(root.transform);
            GameplayTapSurface tapSurface =
                CreateTapSurface(canvas.transform);
            Transform safeArea = CreateSafeArea(canvas.transform);

            GameObject stageSelectPanel;
            Button[] stageButtons;
            Text[] stageSummaries;
            Button unlockAllButton;
            CreateStageSelectUi(
                safeArea,
                out stageSelectPanel,
                out stageButtons,
                out stageSummaries,
                out unlockAllButton);

            GameObject itemPanel;
            Text selectedStageText;
            Button shieldButton;
            Text shieldButtonText;
            Button boosterButton;
            Text boosterButtonText;
            Button startButton;
            Button backButton;
            CreateItemUi(
                safeArea,
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
                safeArea,
                out countdownPanel,
                out countdownText);

            GameObject hud;
            Text hudStage;
            Text hudProgress;
            Image progressFill;
            Text hudShield;
            Text hudBooster;
            CreateHud(
                safeArea,
                out hud,
                out hudStage,
                out hudProgress,
                out progressFill,
                out hudShield,
                out hudBooster);

            GameObject clearPanel;
            Text clearTitle;
            Text clearDetails;
            Button nextButton;
            Button replayButton;
            Button clearSelectButton;
            CreateResultUi(
                "StageClearPanel",
                "STAGE CLEAR",
                safeArea,
                true,
                out clearPanel,
                out clearTitle,
                out clearDetails,
                out nextButton,
                out replayButton,
                out clearSelectButton);

            GameObject failPanel;
            Text failTitle;
            Text failDetails;
            Button retryButton;
            Button failSelectButton;
            CreateFailureUi(
                safeArea,
                out failPanel,
                out failTitle,
                out failDetails,
                out retryButton,
                out failSelectButton);

            CreateEventSystem(root.transform);
            tapSurface.Configure(controller);
            controller.Configure(
                playerObject.transform,
                playerRenderer,
                camera,
                red,
                blue,
                green,
                failure,
                tapSurface,
                trackPool,
                gates,
                goal,
                shieldVisual,
                successParticles,
                speedLines,
                trail,
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
                hudShield,
                hudBooster,
                clearPanel,
                clearTitle,
                clearDetails,
                nextButton,
                replayButton,
                clearSelectButton,
                failPanel,
                failTitle,
                failDetails,
                retryButton,
                failSelectButton);

            stageSelectPanel.SetActive(true);
            itemPanel.SetActive(false);
            countdownPanel.SetActive(false);
            hud.SetActive(false);
            clearPanel.SetActive(false);
            failPanel.SetActive(false);
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

            if (controllers.Length != 1 ||
                !controllers[0].HasRequiredReferences() ||
                legacyControllers.Length != 0)
            {
                throw new InvalidOperationException(
                    "Stage controller is duplicated, incomplete, or legacy mode is exposed.");
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

            string[] uniqueNames =
            {
                "Canvas",
                "SafeAreaRoot",
                "GameplayTapSurface",
                "StageSelectPanel",
                "StageSelectTitle",
                "DeveloperUnlockAllButton",
                "PreRunItemPanel",
                "ShieldItemButton",
                "BoosterItemButton",
                "StartStageButton",
                "BackButton",
                "CountdownPanel",
                "CountdownText",
                "StageHud",
                "StageProgressFill",
                "Goal",
                "StageClearPanel",
                "StageFailedPanel",
                "NextStageButton",
                "ReplayButton",
                "RetryButton",
                "SuccessParticles",
                "BoosterSpeedLines",
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
            GameObject shield =
                GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "ShieldVisual";
            shield.transform.SetParent(player, false);
            shield.transform.localScale = Vector3.one * 1.8f;
            shield.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(shield.GetComponent<Collider>());
            return shield;
        }

        private static GameObject CreateGoal(Transform parent, Material material)
        {
            GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            goal.name = "Goal";
            goal.transform.SetParent(parent, false);
            goal.transform.localScale = new Vector3(6f, 3f, 0.35f);
            goal.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(goal.GetComponent<Collider>());
            return goal;
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

                Renderer[] renderers = new Renderer[2];
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
                StageGateView view =
                    gateObject.GetComponent<StageGateView>();
                view.Configure(controller, renderers);
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
                float top = 0.79f - (index * 0.14f);
                Text label;
                buttons[index] = CreateButton(
                    $"StageButton_{index + 1:00}",
                    panel.transform,
                    $"STAGE {index + 1}",
                    new Vector2(0.12f, top - 0.10f),
                    new Vector2(0.88f, top),
                    out label);
                label.alignment = TextAnchor.MiddleLeft;
                label.fontSize = 28;
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
            out Text shield,
            out Text booster)
        {
            panel = CreatePanel(
                "StageHud",
                parent,
                new Color(0.02f, 0.03f, 0.05f, 0.72f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            SetAnchors(
                panelRect,
                new Vector2(0.08f, 0.84f),
                new Vector2(0.92f, 0.97f));
            stage = CreateText(
                "StageHudTitle",
                panel.transform,
                "STAGE 1",
                28,
                new Vector2(0.04f, 0.50f),
                new Vector2(0.35f, 0.96f));
            progress = CreateText(
                "StageProgressText",
                panel.transform,
                "0 / 24",
                30,
                new Vector2(0.62f, 0.50f),
                new Vector2(0.96f, 0.96f));
            GameObject bar = CreatePanel(
                "StageProgressBar",
                panel.transform,
                new Color(1f, 1f, 1f, 0.18f));
            SetAnchors(
                bar.GetComponent<RectTransform>(),
                new Vector2(0.04f, 0.30f),
                new Vector2(0.96f, 0.45f));
            GameObject fillObject = CreatePanel(
                "StageProgressFill",
                bar.transform,
                GreenColor);
            fill = fillObject.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            shield = CreateText(
                "ShieldStatusText",
                panel.transform,
                "SHIELD --",
                20,
                new Vector2(0.04f, 0.02f),
                new Vector2(0.45f, 0.27f));
            booster = CreateText(
                "BoosterStatusText",
                panel.transform,
                string.Empty,
                20,
                new Vector2(0.55f, 0.02f),
                new Vector2(0.96f, 0.27f));
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
                "NextStageButton",
                panel.transform,
                "NEXT STAGE",
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
                "ClearStageSelectButton",
                panel.transform,
                "STAGE SELECT",
                new Vector2(0.16f, 0.07f),
                new Vector2(0.84f, 0.17f),
                out selectLabel);
        }

        private static void CreateFailureUi(
            Transform parent,
            out GameObject panel,
            out Text title,
            out Text details,
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
            Text retryLabel;
            retry = CreateButton(
                "RetryButton",
                panel.transform,
                "RETRY",
                new Vector2(0.16f, 0.25f),
                new Vector2(0.84f, 0.37f),
                out retryLabel);
            Text selectLabel;
            select = CreateButton(
                "FailStageSelectButton",
                panel.transform,
                "STAGE SELECT",
                new Vector2(0.16f, 0.11f),
                new Vector2(0.84f, 0.23f),
                out selectLabel);
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
