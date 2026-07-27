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
        internal const string GeneratedMaterialsFolder = "Assets/Game/Generated/Materials";
        internal const int GatePoolSize = 5;
        internal const int TrackPoolSize = 6;
        internal const float TrackSegmentLength = 40f;

        internal static readonly Color RedColor = FromHex(0xE63946);
        internal static readonly Color BlueColor = FromHex(0x2D7FF9);
        internal static readonly Color GreenColor = FromHex(0x22C55E);
        internal static readonly Color NeutralColor = FromHex(0xD9D9D9);
        internal static readonly Color FailureColor = FromHex(0x6B7280);

        private static readonly Vector3 PlayerStartPosition = new Vector3(0f, 1f, 0f);
        private static readonly Vector3 CameraPosition = new Vector3(0f, 8f, -10f);
        private static readonly Vector3 CameraRotation = new Vector3(20f, 0f, 0f);

        [MenuItem("Tools/Color Gate Runner/Build Graybox Scene")]
        public static void BuildGrayboxScene()
        {
            EnsureAssetFolder("Assets/Game/Generated");
            EnsureAssetFolder(GeneratedMaterialsFolder);

            Material redMaterial = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Red.mat",
                RedColor);
            Material blueMaterial = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Blue.mat",
                BlueColor);
            Material greenMaterial = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Green.mat",
                GreenColor);
            Material neutralMaterial = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Neutral.mat",
                NeutralColor);
            Material failureMaterial = CreateOrUpdateMaterial(
                GeneratedMaterialsFolder + "/Failure.mat",
                FailureColor);
            Material particleMaterial = CreateOrUpdateParticleMaterial(
                GeneratedMaterialsFolder + "/SuccessParticle.mat");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemovePreviousSceneObjects(scene);

            GameObject generatedRoot = new GameObject(GeneratedRootName);
            SceneManager.MoveGameObjectToScene(generatedRoot, scene);

            Camera gameplayCamera = CreateCamera(generatedRoot.transform);
            CreateDirectionalLight(generatedRoot.transform);
            TrackPoolController trackPool =
                CreateTrackPool(
                    generatedRoot.transform,
                    neutralMaterial,
                    failureMaterial);

            GameObject playerObject = CreatePlayer(generatedRoot.transform, redMaterial);
            Renderer playerRenderer = playerObject.GetComponent<Renderer>();
            TrailRenderer playerTrail =
                CreatePlayerTrail(playerObject, particleMaterial);
            GameObject shieldVisual =
                CreateShieldVisual(playerObject.transform, particleMaterial);

            GameSceneController controller =
                generatedRoot.AddComponent<GameSceneController>();
            ShieldPickupView shieldPickup = CreateShieldPickup(
                generatedRoot.transform,
                controller,
                particleMaterial);

            GateView[] gates = CreateGatePool(
                generatedRoot.transform,
                controller,
                redMaterial,
                blueMaterial);
            ParticleSystem successParticles = CreateSuccessParticles(
                generatedRoot.transform,
                particleMaterial);
            ParticleSystem shieldParticles = CreateBurstParticles(
                "ShieldParticles",
                generatedRoot.transform,
                particleMaterial,
                64);
            ParticleSystem speedLines = CreateSpeedLines(
                playerObject.transform,
                particleMaterial);

            Canvas canvas = CreateCanvas(generatedRoot.transform);
            GameplayTapSurface tapSurface = CreateTapSurface(canvas.transform);
            Transform safeAreaRoot = CreateSafeAreaRoot(canvas.transform);
            GameObject scorePanel;
            Text scoreLabel;
            Text scoreText;
            Text shieldText;
            Text colorCycleText;
            Text speedStageText;
            RectTransform scorePulseTarget;
            CreateScoreHud(
                safeAreaRoot,
                out scorePanel,
                out scoreLabel,
                out scoreText,
                out shieldText,
                out colorCycleText,
                out scorePulseTarget);
            Text shieldMessage = CreateShieldMessage(canvas.transform);
            Text thirdColorMessage =
                CreateThirdColorMessage(canvas.transform);
            GameObject diagnosticsPanel;
            Text diagnosticsText;
            CreateDiagnosticsUi(
                safeAreaRoot,
                out diagnosticsPanel,
                out diagnosticsText,
                out speedStageText);
            GameObject countdownPanel;
            Text countdownText;
            CreateCountdownUi(
                canvas.transform,
                out countdownPanel,
                out countdownText);
            GameObject readyOverlay;
            Text readyTitle;
            Text readyInstruction;
            Text readyTap;
            CreateReadyOverlay(
                canvas.transform,
                out readyOverlay,
                out readyTitle,
                out readyInstruction,
                out readyTap);
            GameObject gameOverPanel;
            GameObject resultCard;
            Text thisRunLabel;
            Text gameOverScore;
            Text bestScore;
            Text topScores;
            GameObject topScoresPanel;
            GameObject bestBadge;
            Text newBestText;
            GameObject[] topScoreRows;
            Text[] topScoreRankTexts;
            Text[] topScoreValueTexts;
            Text[] topScoreMarkerTexts;
            Button restartButton;
            CreateGameOverUi(
                canvas.transform,
                out gameOverPanel,
                out resultCard,
                out thisRunLabel,
                out gameOverScore,
                out bestScore,
                out topScores,
                out topScoresPanel,
                out bestBadge,
                out newBestText,
                out topScoreRows,
                out topScoreRankTexts,
                out topScoreValueTexts,
                out topScoreMarkerTexts,
                out restartButton);
            CreateEventSystem(generatedRoot.transform);

            tapSurface.Configure(controller);
            controller.Configure(
                GameRules.DefaultSeed,
                playerObject.transform,
                playerRenderer,
                gameplayCamera,
                redMaterial,
                blueMaterial,
                greenMaterial,
                failureMaterial,
                scorePanel,
                scoreLabel,
                scoreText,
                scorePulseTarget,
                shieldText,
                colorCycleText,
                readyOverlay,
                readyTitle,
                readyInstruction,
                readyTap,
                gameOverPanel,
                gameOverScore,
                bestScore,
                topScores,
                restartButton,
                tapSurface,
                successParticles,
                speedLines,
                shieldParticles,
                playerTrail,
                shieldVisual,
                shieldPickup,
                shieldMessage,
                thirdColorMessage,
                speedStageText,
                diagnosticsPanel,
                diagnosticsText,
                trackPool,
                countdownPanel,
                countdownText,
                resultCard,
                thisRunLabel,
                topScoresPanel,
                bestBadge,
                newBestText,
                topScoreRows,
                topScoreRankTexts,
                topScoreValueTexts,
                topScoreMarkerTexts,
                gates);

            gameOverPanel.SetActive(false);
            readyOverlay.SetActive(true);
            countdownPanel.SetActive(false);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

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
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            int generatedRootCount = 0;
            GameObject generatedRoot = null;

            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == GeneratedRootName)
                {
                    generatedRootCount++;
                    generatedRoot = roots[index];
                }
            }

            if (generatedRootCount != 1 || generatedRoot == null)
            {
                throw new InvalidOperationException(
                    $"Expected exactly one {GeneratedRootName} root, found {generatedRootCount}.");
            }

            GameSceneController[] controllers =
                generatedRoot.GetComponentsInChildren<GameSceneController>(true);
            Camera[] cameras = generatedRoot.GetComponentsInChildren<Camera>(true);
            EventSystem[] eventSystems =
                generatedRoot.GetComponentsInChildren<EventSystem>(true);
            GateView[] gates = generatedRoot.GetComponentsInChildren<GateView>(true);
            ParticleSystem[] particleSystems =
                generatedRoot.GetComponentsInChildren<ParticleSystem>(true);
            ShieldPickupView[] shieldPickups =
                generatedRoot.GetComponentsInChildren<ShieldPickupView>(true);
            TrackPoolController[] trackPools =
                generatedRoot.GetComponentsInChildren<TrackPoolController>(true);
            TrackSegmentView[] trackSegments =
                generatedRoot.GetComponentsInChildren<TrackSegmentView>(true);

            if (controllers.Length != 1 || !controllers[0].HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Generated scene controller is duplicated or has missing references.");
            }

            if (cameras.Length != 1 || eventSystems.Length != 1)
            {
                throw new InvalidOperationException(
                    "Generated scene must contain exactly one camera and one EventSystem.");
            }

            if (gates.Length != GatePoolSize)
            {
                throw new InvalidOperationException(
                    $"Expected {GatePoolSize} pre-created gates, found {gates.Length}.");
            }

            if (trackPools.Length != 1 ||
                !trackPools[0].HasRequiredReferences() ||
                trackPools[0].SegmentCount != TrackPoolSize ||
                trackSegments.Length != TrackPoolSize + 1)
            {
                throw new InvalidOperationException(
                    "Generated infinite track pool or curve experiment is invalid.");
            }

            if (particleSystems.Length != 3 ||
                shieldPickups.Length != 1 ||
                !shieldPickups[0].HasRequiredReferences() ||
                CountNamedTransforms(generatedRoot, "Canvas") != 1 ||
                CountNamedTransforms(generatedRoot, "TrackPool") != 1 ||
                CountNamedTransforms(generatedRoot, "CurveSegmentExperiment") != 1 ||
                CountNamedTransforms(generatedRoot, "SafeAreaRoot") != 1 ||
                CountNamedTransforms(generatedRoot, "ReadyOverlay") != 1 ||
                CountNamedTransforms(generatedRoot, "ReadyTitle") != 1 ||
                CountNamedTransforms(generatedRoot, "ReadyInstruction") != 1 ||
                CountNamedTransforms(generatedRoot, "TapToStartVisual") != 1 ||
                CountNamedTransforms(generatedRoot, "ScorePanel") != 1 ||
                CountNamedTransforms(generatedRoot, "ScoreLabel") != 1 ||
                CountNamedTransforms(generatedRoot, "ScoreValue") != 1 ||
                CountNamedTransforms(generatedRoot, "ShieldIndicator") != 1 ||
                CountNamedTransforms(generatedRoot, "ColorCycleIndicator") != 1 ||
                CountNamedTransforms(generatedRoot, "ShieldVisual") != 1 ||
                CountNamedTransforms(generatedRoot, "ShieldPickup") != 1 ||
                CountNamedTransforms(generatedRoot, "ShieldMessage") != 1 ||
                CountNamedTransforms(generatedRoot, "ThirdColorMessage") != 1 ||
                CountNamedTransforms(generatedRoot, "SpeedStage") != 1 ||
                CountNamedTransforms(generatedRoot, "DiagnosticsPanel") != 1 ||
                CountNamedTransforms(generatedRoot, "DiagnosticsText") != 1 ||
                CountNamedTransforms(generatedRoot, "CountdownPanel") != 1 ||
                CountNamedTransforms(generatedRoot, "CountdownText") != 1 ||
                CountNamedTransforms(generatedRoot, "GameOverPanel") != 1 ||
                CountNamedTransforms(generatedRoot, "ResultSafeArea") != 1 ||
                CountNamedTransforms(generatedRoot, "ResultCard") != 1 ||
                CountNamedTransforms(generatedRoot, "ThisRunLabel") != 1 ||
                CountNamedTransforms(generatedRoot, "GameOverScore") != 1 ||
                CountNamedTransforms(generatedRoot, "BestScore") != 1 ||
                CountNamedTransforms(generatedRoot, "TopScores") != 1 ||
                CountNamedTransforms(generatedRoot, "TopScoresPanel") != 1 ||
                CountNamedTransforms(generatedRoot, "BestBadge") != 1 ||
                CountNamedTransforms(generatedRoot, "NewBestText") != 1 ||
                CountNamedTransforms(generatedRoot, "RestartButton") != 1 ||
                CountNamedTransforms(generatedRoot, "SuccessParticles") != 1 ||
                CountNamedTransforms(generatedRoot, "ShieldParticles") != 1 ||
                CountNamedTransforms(generatedRoot, "SpeedLines") != 1 ||
                CountNamedTransforms(generatedRoot, "EventSystem") != 1)
            {
                throw new InvalidOperationException(
                    "Generated feedback or UI objects are missing or duplicated.");
            }

            for (int rank = 1; rank <= ScoreHistory.Capacity; rank++)
            {
                if (CountNamedTransforms(
                    generatedRoot,
                    $"TopScoreRow_{rank:00}") != 1)
                {
                    throw new InvalidOperationException(
                        "Generated result card must contain exactly five rank rows.");
                }
            }

            Transform[] transforms = generatedRoot.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                int missingCount =
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transforms[index].gameObject);
                if (missingCount > 0)
                {
                    throw new InvalidOperationException(
                        $"Missing MonoBehaviour found on {transforms[index].name}.");
                }
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
            {
                throw new InvalidOperationException("Default orientation is not Portrait.");
            }

            if (HasActiveVolume(generatedRoot))
            {
                throw new InvalidOperationException(
                    "An active post-processing Volume remains in the generated scene.");
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
                "Main Camera",
                typeof(Camera),
                typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.SetPositionAndRotation(
                CameraPosition,
                Quaternion.Euler(CameraRotation));

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.14f, 0.16f, 1f);
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.allowHDR = false;

            Type additionalCameraDataType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (additionalCameraDataType != null)
            {
                Component cameraData = cameraObject.AddComponent(additionalCameraDataType);
                SerializedObject serializedCameraData = new SerializedObject(cameraData);
                SerializedProperty postProcessing =
                    serializedCameraData.FindProperty("m_RenderPostProcessing");
                if (postProcessing != null)
                {
                    postProcessing.boolValue = false;
                    serializedCameraData.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            return camera;
        }

        private static void CreateDirectionalLight(Transform parent)
        {
            GameObject lightObject = new GameObject(
                "Directional Light",
                typeof(Light));
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
        }

        private static TrackPoolController CreateTrackPool(
            Transform parent,
            Material groundMaterial,
            Material markerMaterial)
        {
            GameObject poolObject = new GameObject("TrackPool");
            poolObject.transform.SetParent(parent, false);
            TrackPoolController pool =
                poolObject.AddComponent<TrackPoolController>();
            var segments = new TrackSegmentView[TrackPoolSize];

            for (int index = 0; index < segments.Length; index++)
            {
                segments[index] = CreateTrackSegment(
                    $"TrackSegment_{index:00}",
                    poolObject.transform,
                    groundMaterial,
                    markerMaterial,
                    false);
            }

            pool.Configure(segments, TrackSegmentLength, -20f, 20f);
            pool.ResetPool();

            TrackSegmentView curveExperiment = CreateTrackSegment(
                "CurveSegmentExperiment",
                parent,
                groundMaterial,
                markerMaterial,
                true);
            BuildCurveExperimentGeometry(
                curveExperiment.transform,
                groundMaterial);
            curveExperiment.gameObject.SetActive(false);
            return pool;
        }

        private static TrackSegmentView CreateTrackSegment(
            string name,
            Transform parent,
            Material groundMaterial,
            Material markerMaterial,
            bool curveExperiment)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);

            Transform start = new GameObject("StartAnchor").transform;
            start.SetParent(root.transform, false);
            Transform end = new GameObject("EndAnchor").transform;
            end.SetParent(root.transform, false);
            end.localPosition = new Vector3(0f, 0f, TrackSegmentLength);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform, false);
            ground.transform.localPosition =
                new Vector3(0f, -0.1f, TrackSegmentLength * 0.5f);
            ground.transform.localScale =
                new Vector3(8f, 0.2f, TrackSegmentLength);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            for (int markerIndex = 0; markerIndex < 8; markerIndex++)
            {
                GameObject marker =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = $"LaneMarker_{markerIndex:00}";
                marker.transform.SetParent(root.transform, false);
                marker.transform.localPosition =
                    new Vector3(
                        0f,
                        0.03f,
                        2.5f + (markerIndex * 5f));
                marker.transform.localScale =
                    new Vector3(0.18f, 0.03f, 2.2f);
                marker.GetComponent<Renderer>().sharedMaterial = markerMaterial;
                UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
            }

            TrackSegmentView segment = root.AddComponent<TrackSegmentView>();
            segment.Configure(start, end, curveExperiment);
            return segment;
        }

        private static void BuildCurveExperimentGeometry(
            Transform parent,
            Material material)
        {
            const int PieceCount = 8;
            const float TotalYaw = 16f;
            float pieceLength = TrackSegmentLength / PieceCount;
            Vector3 position = Vector3.zero;
            float yaw = 0f;
            for (int index = 0; index < PieceCount; index++)
            {
                GameObject piece =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.name = $"CurvePiece_{index:00}";
                piece.transform.SetParent(parent, false);
                piece.transform.localPosition = position;
                piece.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                piece.transform.localScale =
                    new Vector3(8f, 0.2f, pieceLength + 0.15f);
                piece.GetComponent<Renderer>().sharedMaterial = material;

                Vector3 forward =
                    Quaternion.Euler(0f, yaw, 0f) *
                    Vector3.forward;
                position += forward * pieceLength;
                yaw += TotalYaw / PieceCount;
            }
        }

        private static GameObject CreatePlayer(Transform parent, Material material)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.SetParent(parent, false);
            player.transform.position = PlayerStartPosition;
            player.GetComponent<Renderer>().sharedMaterial = material;

            Rigidbody rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            return player;
        }

        private static TrailRenderer CreatePlayerTrail(
            GameObject player,
            Material material)
        {
            TrailRenderer trail = player.AddComponent<TrailRenderer>();
            trail.sharedMaterial = material;
            trail.time = 0.08f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.12f;
            trail.emitting = true;
            return trail;
        }

        private static GameObject CreateShieldVisual(
            Transform player,
            Material material)
        {
            GameObject root = new GameObject("ShieldVisual");
            root.transform.SetParent(player, false);
            root.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            for (int index = 0; index < 4; index++)
            {
                GameObject arc =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                arc.name = $"ShieldArc_{index:00}";
                arc.transform.SetParent(root.transform, false);
                float angle = index * 90f;
                Vector3 radial =
                    Quaternion.Euler(0f, angle, 0f) *
                    (Vector3.forward * 0.95f);
                arc.transform.localPosition = radial;
                arc.transform.localRotation =
                    Quaternion.Euler(0f, angle, 0f);
                arc.transform.localScale =
                    new Vector3(0.55f, 0.08f, 0.12f);
                arc.GetComponent<Renderer>().sharedMaterial = material;
                UnityEngine.Object.DestroyImmediate(arc.GetComponent<Collider>());
            }

            root.SetActive(false);
            return root;
        }

        private static ShieldPickupView CreateShieldPickup(
            Transform parent,
            GameSceneController controller,
            Material material)
        {
            GameObject root = new GameObject("ShieldPickup");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(0f, 1.35f, 0f);
            BoxCollider trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.2f, 2.2f, 1.2f);

            Renderer[] renderers = new Renderer[4];
            for (int index = 0; index < renderers.Length; index++)
            {
                GameObject part =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);
                part.name = $"PickupDiamond_{index:00}";
                part.transform.SetParent(root.transform, false);
                float angle = index * 90f;
                part.transform.localPosition =
                    Quaternion.Euler(0f, angle, 0f) *
                    (Vector3.forward * 0.55f);
                part.transform.localRotation =
                    Quaternion.Euler(35f, angle + 45f, 35f);
                part.transform.localScale =
                    new Vector3(0.28f, 0.7f, 0.28f);
                renderers[index] = part.GetComponent<Renderer>();
                renderers[index].sharedMaterial = material;
                UnityEngine.Object.DestroyImmediate(
                    part.GetComponent<Collider>());
            }

            ShieldPickupView pickup =
                root.AddComponent<ShieldPickupView>();
            pickup.Configure(controller, renderers);
            root.SetActive(false);
            return pickup;
        }

        private static GateView[] CreateGatePool(
            Transform parent,
            GameSceneController controller,
            Material redMaterial,
            Material blueMaterial)
        {
            GateView[] gates = new GateView[GatePoolSize];
            GameSession previewSession = new GameSession(GameRules.DefaultSeed);
            float gateZ = PlayerStartPosition.z;

            for (int index = 0; index < GatePoolSize; index++)
            {
                GameObject gateObject = new GameObject($"Gate_{index:00}");
                gateObject.transform.SetParent(parent, false);

                BoxCollider trigger = gateObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.5f, 0f);
                trigger.size = new Vector3(4.2f, 3f, 0.5f);

                Renderer[] renderers =
                {
                    CreateGatePart(
                        "Left",
                        gateObject.transform,
                        new Vector3(-1.8f, 1.5f, 0f),
                        new Vector3(0.35f, 3f, 0.35f)),
                    CreateGatePart(
                        "Right",
                        gateObject.transform,
                        new Vector3(1.8f, 1.5f, 0f),
                        new Vector3(0.35f, 3f, 0.35f)),
                    CreateGatePart(
                        "Top",
                        gateObject.transform,
                        new Vector3(0f, 2.85f, 0f),
                        new Vector3(4f, 0.3f, 0.35f))
                };

                GateView gate = gateObject.AddComponent<GateView>();
                gate.Configure(controller, renderers);

                GatePlan plan = previewSession.GetNextGatePlan();
                RunnerColor color = plan.Color;
                Material material =
                    color == RunnerColor.Red ? redMaterial : blueMaterial;
                gateZ += plan.Spacing;
                gate.Activate(color, material, gateZ);
                gates[index] = gate;
            }

            return gates;
        }

        private static Renderer CreateGatePart(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return part.GetComponent<Renderer>();
        }

        private static ParticleSystem CreateSuccessParticles(
            Transform parent,
            Material material)
        {
            GameObject particleObject = new GameObject(
                "SuccessParticles",
                typeof(ParticleSystem));
            particleObject.transform.SetParent(parent, false);

            ParticleSystem particles = particleObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.3f;
            main.startLifetime = 0.28f;
            main.startSpeed = 1.8f;
            main.startSize = 0.14f;
            main.startColor = Color.white;
            main.maxParticles = 32;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            ParticleSystemRenderer renderer =
                particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            return particles;
        }

        private static ParticleSystem CreateBurstParticles(
            string name,
            Transform parent,
            Material material,
            int maximumParticles)
        {
            GameObject particleObject = new GameObject(
                name,
                typeof(ParticleSystem));
            particleObject.transform.SetParent(parent, false);
            ParticleSystem particles =
                particleObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.45f;
            main.startLifetime = 0.4f;
            main.startSpeed = 3.2f;
            main.startSize = 0.2f;
            main.startColor = new Color(0.55f, 0.9f, 1f, 1f);
            main.maxParticles = maximumParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.45f;
            particleObject.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                material;
            return particles;
        }

        private static ParticleSystem CreateSpeedLines(
            Transform player,
            Material material)
        {
            GameObject particleObject = new GameObject(
                "SpeedLines",
                typeof(ParticleSystem));
            particleObject.transform.SetParent(player, false);
            particleObject.transform.localPosition = new Vector3(0f, 1f, 5f);
            ParticleSystem particles =
                particleObject.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.startLifetime = 0.55f;
            main.startSpeed = -18f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.07f);
            main.startColor = new Color(1f, 1f, 1f, 0.45f);
            main.maxParticles = 128;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(7f, 4f, 1f);
            ParticleSystemRenderer renderer =
                particleObject.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 8f;
            renderer.sharedMaterial = material;
            return particles;
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
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
            GameObject tapObject = CreateUiObject("GameplayTapSurface", parent);
            StretchToParent(tapObject.GetComponent<RectTransform>());

            Image image = tapObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            return tapObject.AddComponent<GameplayTapSurface>();
        }

        private static Transform CreateSafeAreaRoot(Transform parent)
        {
            GameObject root = CreateUiObject("SafeAreaRoot", parent);
            StretchToParent(root.GetComponent<RectTransform>());
            root.AddComponent<SafeAreaLayout>();
            return root.transform;
        }

        private static void CreateScoreHud(
            Transform parent,
            out GameObject panel,
            out Text label,
            out Text value,
            out Text shield,
            out Text colorCycle,
            out RectTransform pulseTarget)
        {
            panel = CreateUiObject("ScorePanel", parent);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -64f);
            panelRect.sizeDelta = new Vector2(300f, 190f);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.06f, 0.08f, 0.1f, 0.82f);
            panelImage.raycastTarget = false;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.65f);
            outline.effectDistance = new Vector2(3f, -3f);

            label = CreateText(
                "ScoreLabel",
                panel.transform,
                "SCORE",
                28,
                TextAnchor.MiddleCenter);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0f, 0.72f);
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            value = CreateText(
                "ScoreValue",
                panel.transform,
                "0",
                72,
                TextAnchor.MiddleCenter);
            RectTransform valueRect = value.rectTransform;
            valueRect.anchorMin = new Vector2(0f, 0.2f);
            valueRect.anchorMax = new Vector2(1f, 0.76f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            pulseTarget = valueRect;

            shield = CreateText(
                "ShieldIndicator",
                panel.transform,
                "SHIELD  EMPTY",
                22,
                TextAnchor.MiddleCenter);
            RectTransform shieldRect = shield.rectTransform;
            shieldRect.anchorMin = Vector2.zero;
            shieldRect.anchorMax = new Vector2(1f, 0.22f);
            shieldRect.offsetMin = Vector2.zero;
            shieldRect.offsetMax = Vector2.zero;

            colorCycle = CreateText(
                "ColorCycleIndicator",
                parent,
                "RED  >  BLUE",
                22,
                TextAnchor.MiddleCenter);
            RectTransform cycleRect = colorCycle.rectTransform;
            cycleRect.anchorMin = new Vector2(0.28f, 0.84f);
            cycleRect.anchorMax = new Vector2(0.72f, 0.89f);
            cycleRect.offsetMin = Vector2.zero;
            cycleRect.offsetMax = Vector2.zero;
        }

        private static Text CreateShieldMessage(Transform parent)
        {
            Text message = CreateText(
                "ShieldMessage",
                parent,
                "SHIELD",
                58,
                TextAnchor.MiddleCenter);
            RectTransform rect = message.rectTransform;
            rect.anchorMin = new Vector2(0.1f, 0.56f);
            rect.anchorMax = new Vector2(0.9f, 0.68f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            message.color = new Color(0.55f, 0.9f, 1f, 1f);
            message.gameObject.SetActive(false);
            return message;
        }

        private static Text CreateThirdColorMessage(Transform parent)
        {
            Text message = CreateText(
                "ThirdColorMessage",
                parent,
                "NEW COLOR",
                64,
                TextAnchor.MiddleCenter);
            RectTransform rect = message.rectTransform;
            rect.anchorMin = new Vector2(0.08f, 0.5f);
            rect.anchorMax = new Vector2(0.92f, 0.64f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            message.color = GreenColor;
            message.gameObject.SetActive(false);
            return message;
        }

        private static void CreateDiagnosticsUi(
            Transform parent,
            out GameObject panel,
            out Text diagnostics,
            out Text speedStage)
        {
            panel = CreateUiObject("DiagnosticsPanel", parent);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.7f, 0.72f);
            panelRect.anchorMax = new Vector2(0.98f, 0.93f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);
            panelImage.raycastTarget = false;

            speedStage = CreateText(
                "SpeedStage",
                panel.transform,
                "SPEED  I",
                22,
                TextAnchor.UpperLeft);
            RectTransform stageRect = speedStage.rectTransform;
            stageRect.anchorMin = new Vector2(0.06f, 0.73f);
            stageRect.anchorMax = new Vector2(0.94f, 0.96f);
            stageRect.offsetMin = Vector2.zero;
            stageRect.offsetMax = Vector2.zero;

            diagnostics = CreateText(
                "DiagnosticsText",
                panel.transform,
                string.Empty,
                16,
                TextAnchor.UpperLeft);
            RectTransform diagnosticsRect = diagnostics.rectTransform;
            diagnosticsRect.anchorMin = new Vector2(0.06f, 0.05f);
            diagnosticsRect.anchorMax = new Vector2(0.94f, 0.75f);
            diagnosticsRect.offsetMin = Vector2.zero;
            diagnosticsRect.offsetMax = Vector2.zero;
            panel.SetActive(false);
        }

        private static void CreateCountdownUi(
            Transform parent,
            out GameObject panel,
            out Text value)
        {
            panel = CreateUiObject("CountdownPanel", parent);
            StretchToParent(panel.GetComponent<RectTransform>());
            Image dimmer = panel.AddComponent<Image>();
            dimmer.color = new Color(0f, 0f, 0f, 0.28f);
            dimmer.raycastTarget = false;
            value = CreateText(
                "CountdownText",
                panel.transform,
                "3",
                180,
                TextAnchor.MiddleCenter);
            StretchToParent(value.rectTransform);
            panel.SetActive(false);
        }

        private static void CreateReadyOverlay(
            Transform parent,
            out GameObject overlay,
            out Text title,
            out Text instruction,
            out Text tapText)
        {
            overlay = CreateUiObject("ReadyOverlay", parent);
            StretchToParent(overlay.GetComponent<RectTransform>());

            Image dimmer = overlay.AddComponent<Image>();
            dimmer.color = new Color(0.02f, 0.03f, 0.05f, 0.68f);
            dimmer.raycastTarget = false;

            title = CreateText(
                "ReadyTitle",
                overlay.transform,
                "COLOR GATE",
                88,
                TextAnchor.MiddleCenter);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.08f, 0.62f);
            titleRect.anchorMax = new Vector2(0.92f, 0.78f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            instruction = CreateText(
                "ReadyInstruction",
                overlay.transform,
                "MATCH YOUR COLOR TO EACH GATE",
                34,
                TextAnchor.MiddleCenter);
            RectTransform instructionRect = instruction.rectTransform;
            instructionRect.anchorMin = new Vector2(0.08f, 0.48f);
            instructionRect.anchorMax = new Vector2(0.92f, 0.58f);
            instructionRect.offsetMin = Vector2.zero;
            instructionRect.offsetMax = Vector2.zero;

            GameObject startButton = CreateUiObject(
                "TapToStartVisual",
                overlay.transform);
            RectTransform buttonRect = startButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.2f, 0.3f);
            buttonRect.anchorMax = new Vector2(0.8f, 0.4f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            Image buttonImage = startButton.AddComponent<Image>();
            buttonImage.color = new Color(1f, 1f, 1f, 0.94f);
            buttonImage.raycastTarget = false;
            Outline outline = startButton.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.45f);
            outline.effectDistance = new Vector2(4f, -4f);

            tapText = CreateText(
                "ReadyTapText",
                startButton.transform,
                "TAP TO START",
                44,
                TextAnchor.MiddleCenter);
            tapText.color = new Color(0.07f, 0.09f, 0.12f, 1f);
            StretchToParent(tapText.rectTransform);
        }

        private static void CreateGameOverUi(
            Transform parent,
            out GameObject panel,
            out GameObject resultCard,
            out Text thisRunLabel,
            out Text currentScore,
            out Text bestScore,
            out Text topScores,
            out GameObject topScoresPanel,
            out GameObject bestBadge,
            out Text newBestText,
            out GameObject[] topScoreRows,
            out Text[] topScoreRankTexts,
            out Text[] topScoreValueTexts,
            out Text[] topScoreMarkerTexts,
            out Button restartButton)
        {
            panel = CreateUiObject("GameOverPanel", parent);
            StretchToParent(panel.GetComponent<RectTransform>());
            Image dimmer = panel.AddComponent<Image>();
            dimmer.color = new Color(0f, 0f, 0f, 0.78f);
            dimmer.raycastTarget = false;

            GameObject resultSafeArea =
                CreateUiObject("ResultSafeArea", panel.transform);
            StretchToParent(resultSafeArea.GetComponent<RectTransform>());
            resultSafeArea.AddComponent<SafeAreaLayout>();

            resultCard = CreateUiObject(
                "ResultCard",
                resultSafeArea.transform);
            RectTransform cardRect = resultCard.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.08f, 0.06f);
            cardRect.anchorMax = new Vector2(0.92f, 0.94f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            Image cardImage = resultCard.AddComponent<Image>();
            cardImage.color = new Color(0.035f, 0.05f, 0.08f, 0.98f);
            cardImage.raycastTarget = false;
            Outline cardOutline = resultCard.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.2f, 0.55f, 0.95f, 0.8f);
            cardOutline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText(
                "GameOverText",
                resultCard.transform,
                "GAME OVER",
                72,
                TextAnchor.MiddleCenter);
            SetAnchors(title.rectTransform, 0.08f, 0.86f, 0.92f, 0.97f);

            thisRunLabel = CreateText(
                "ThisRunLabel",
                resultCard.transform,
                "THIS RUN",
                28,
                TextAnchor.MiddleCenter);
            thisRunLabel.color = new Color(0.65f, 0.78f, 0.95f, 1f);
            SetAnchors(
                thisRunLabel.rectTransform,
                0.15f,
                0.77f,
                0.85f,
                0.84f);

            currentScore = CreateText(
                "GameOverScore",
                resultCard.transform,
                "0",
                112,
                TextAnchor.MiddleCenter);
            SetAnchors(
                currentScore.rectTransform,
                0.1f,
                0.63f,
                0.9f,
                0.78f);

            bestBadge = CreateUiObject("BestBadge", resultCard.transform);
            RectTransform badgeRect = bestBadge.GetComponent<RectTransform>();
            SetAnchors(badgeRect, 0.18f, 0.56f, 0.28f, 0.63f);
            Image badgeImage = bestBadge.AddComponent<Image>();
            badgeImage.color = new Color(1f, 0.72f, 0.15f, 1f);
            badgeImage.raycastTarget = false;

            bestScore = CreateText(
                "BestScore",
                resultCard.transform,
                "BEST  0",
                42,
                TextAnchor.MiddleCenter);
            bestScore.color = new Color(1f, 0.78f, 0.25f, 1f);
            SetAnchors(
                bestScore.rectTransform,
                0.24f,
                0.55f,
                0.82f,
                0.64f);

            newBestText = CreateText(
                "NewBestText",
                resultCard.transform,
                "NEW BEST!",
                34,
                TextAnchor.MiddleCenter);
            newBestText.color = new Color(0.35f, 1f, 0.65f, 1f);
            SetAnchors(
                newBestText.rectTransform,
                0.2f,
                0.5f,
                0.8f,
                0.56f);

            topScoresPanel =
                CreateUiObject("TopScoresPanel", resultCard.transform);
            RectTransform topPanelRect =
                topScoresPanel.GetComponent<RectTransform>();
            SetAnchors(topPanelRect, 0.1f, 0.2f, 0.9f, 0.5f);
            Image topPanelImage = topScoresPanel.AddComponent<Image>();
            topPanelImage.color = new Color(0.07f, 0.1f, 0.16f, 0.98f);
            topPanelImage.raycastTarget = false;

            Text topTitle = CreateText(
                "TopScoresTitle",
                topScoresPanel.transform,
                "TOP 5",
                34,
                TextAnchor.MiddleCenter);
            SetAnchors(topTitle.rectTransform, 0f, 0.82f, 1f, 1f);

            topScores = CreateText(
                "TopScores",
                topScoresPanel.transform,
                string.Empty,
                1,
                TextAnchor.MiddleCenter);
            SetAnchors(topScores.rectTransform, 0f, 0f, 0.01f, 0.01f);

            topScoreRows = new GameObject[ScoreHistory.Capacity];
            topScoreRankTexts = new Text[ScoreHistory.Capacity];
            topScoreValueTexts = new Text[ScoreHistory.Capacity];
            topScoreMarkerTexts = new Text[ScoreHistory.Capacity];
            for (int index = 0; index < ScoreHistory.Capacity; index++)
            {
                GameObject row = CreateUiObject(
                    $"TopScoreRow_{index + 1:00}",
                    topScoresPanel.transform);
                RectTransform rowRect = row.GetComponent<RectTransform>();
                float rowTop = 0.81f - (index * 0.16f);
                SetAnchors(
                    rowRect,
                    0.05f,
                    rowTop - 0.14f,
                    0.95f,
                    rowTop);
                Image rowImage = row.AddComponent<Image>();
                rowImage.color = new Color(1f, 1f, 1f, 0.055f);
                rowImage.raycastTarget = false;

                Text rank = CreateText(
                    "Rank",
                    row.transform,
                    (index + 1).ToString(),
                    28,
                    TextAnchor.MiddleCenter);
                SetAnchors(rank.rectTransform, 0f, 0f, 0.2f, 1f);
                Text value = CreateText(
                    "Score",
                    row.transform,
                    "--",
                    32,
                    TextAnchor.MiddleCenter);
                SetAnchors(value.rectTransform, 0.2f, 0f, 0.75f, 1f);
                Text marker = CreateText(
                    "Marker",
                    row.transform,
                    string.Empty,
                    24,
                    TextAnchor.MiddleCenter);
                marker.color = new Color(0.4f, 0.8f, 1f, 1f);
                SetAnchors(marker.rectTransform, 0.75f, 0f, 1f, 1f);

                topScoreRows[index] = row;
                topScoreRankTexts[index] = rank;
                topScoreValueTexts[index] = value;
                topScoreMarkerTexts[index] = marker;
            }

            GameObject buttonObject =
                CreateUiObject("RestartButton", resultCard.transform);
            RectTransform buttonRect =
                buttonObject.GetComponent<RectTransform>();
            SetAnchors(buttonRect, 0.22f, 0.06f, 0.78f, 0.16f);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.92f, 0.95f, 1f, 1f);
            restartButton = buttonObject.AddComponent<Button>();
            restartButton.targetGraphic = buttonImage;
            Text label = CreateText(
                "Label",
                buttonObject.transform,
                "RETRY",
                48,
                TextAnchor.MiddleCenter);
            label.color = new Color(0.05f, 0.08f, 0.12f, 1f);
            StretchToParent(label.rectTransform);
        }

        private static void CreateGameOverUiLegacy(
            Transform parent,
            out GameObject panel,
            out Text currentScore,
            out Text bestScore,
            out Text topScores,
            out GameObject topScoresPanel,
            out GameObject bestBadge,
            out Text newBestText,
            out Button restartButton)
        {
            panel = CreateUiObject("GameOverPanel", parent);
            StretchToParent(panel.GetComponent<RectTransform>());

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.65f);
            panelImage.raycastTarget = false;

            Text title = CreateText(
                "GameOverText",
                panel.transform,
                "GAME OVER",
                76,
                TextAnchor.MiddleCenter);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.1f, 0.55f);
            titleRect.anchorMax = new Vector2(0.9f, 0.7f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            currentScore = CreateText(
                "GameOverScore",
                panel.transform,
                "CURRENT  0",
                56,
                TextAnchor.MiddleCenter);
            RectTransform currentScoreRect = currentScore.rectTransform;
            currentScoreRect.anchorMin = new Vector2(0.1f, 0.47f);
            currentScoreRect.anchorMax = new Vector2(0.9f, 0.57f);
            currentScoreRect.offsetMin = Vector2.zero;
            currentScoreRect.offsetMax = Vector2.zero;

            bestScore = CreateText(
                "BestScore",
                panel.transform,
                "★  BEST  0",
                44,
                TextAnchor.MiddleCenter);
            RectTransform bestScoreRect = bestScore.rectTransform;
            bestScoreRect.anchorMin = new Vector2(0.1f, 0.39f);
            bestScoreRect.anchorMax = new Vector2(0.9f, 0.48f);
            bestScoreRect.offsetMin = Vector2.zero;
            bestScoreRect.offsetMax = Vector2.zero;

            bestBadge = CreateUiObject("BestBadge", panel.transform);
            RectTransform badgeRect = bestBadge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(0.16f, 0.405f);
            badgeRect.anchorMax = new Vector2(0.25f, 0.47f);
            badgeRect.offsetMin = Vector2.zero;
            badgeRect.offsetMax = Vector2.zero;
            Image badgeImage = bestBadge.AddComponent<Image>();
            badgeImage.color = new Color(1f, 0.78f, 0.2f, 0.95f);
            badgeImage.raycastTarget = false;

            newBestText = CreateText(
                "NewBestText",
                panel.transform,
                "NEW BEST!",
                30,
                TextAnchor.MiddleCenter);
            RectTransform newBestRect = newBestText.rectTransform;
            newBestRect.anchorMin = new Vector2(0.3f, 0.35f);
            newBestRect.anchorMax = new Vector2(0.7f, 0.41f);
            newBestRect.offsetMin = Vector2.zero;
            newBestRect.offsetMax = Vector2.zero;
            newBestText.color = new Color(1f, 0.78f, 0.2f, 1f);

            topScoresPanel = CreateUiObject("TopScoresPanel", panel.transform);
            RectTransform topPanelRect =
                topScoresPanel.GetComponent<RectTransform>();
            topPanelRect.anchorMin = new Vector2(0.16f, 0.14f);
            topPanelRect.anchorMax = new Vector2(0.84f, 0.36f);
            topPanelRect.offsetMin = Vector2.zero;
            topPanelRect.offsetMax = Vector2.zero;
            Image topPanelImage = topScoresPanel.AddComponent<Image>();
            topPanelImage.color = new Color(0.06f, 0.08f, 0.12f, 0.94f);
            topPanelImage.raycastTarget = false;
            Outline topOutline = topScoresPanel.AddComponent<Outline>();
            topOutline.effectColor = new Color(1f, 1f, 1f, 0.45f);
            topOutline.effectDistance = new Vector2(2f, -2f);

            Text topTitle = CreateText(
                "TopScoresTitle",
                topScoresPanel.transform,
                "TOP 5",
                34,
                TextAnchor.MiddleCenter);
            RectTransform topTitleRect = topTitle.rectTransform;
            topTitleRect.anchorMin = new Vector2(0f, 0.78f);
            topTitleRect.anchorMax = Vector2.one;
            topTitleRect.offsetMin = Vector2.zero;
            topTitleRect.offsetMax = Vector2.zero;

            topScores = CreateText(
                "TopScores",
                topScoresPanel.transform,
                "--",
                28,
                TextAnchor.UpperLeft);
            RectTransform topScoresRect = topScores.rectTransform;
            topScoresRect.anchorMin = new Vector2(0.1f, 0.05f);
            topScoresRect.anchorMax = new Vector2(0.9f, 0.78f);
            topScoresRect.offsetMin = Vector2.zero;
            topScoresRect.offsetMax = Vector2.zero;

            GameObject buttonObject = CreateUiObject("RestartButton", panel.transform);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.25f, 0.05f);
            buttonRect.anchorMax = new Vector2(0.75f, 0.14f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.92f, 0.92f, 0.92f, 1f);
            restartButton = buttonObject.AddComponent<Button>();
            restartButton.targetGraphic = buttonImage;

            Text label = CreateText(
                "Label",
                buttonObject.transform,
                "RETRY",
                48,
                TextAnchor.MiddleCenter);
            label.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            StretchToParent(label.rectTransform);
        }

        private static void CreateEventSystem(Transform parent)
        {
            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(parent, false);
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject uiObject = new GameObject(name, typeof(RectTransform));
            uiObject.transform.SetParent(parent, false);
            return uiObject;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = CreateUiObject(name, parent);
            Text text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetAnchors(
            RectTransform rect,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Material CreateOrUpdateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateParticleMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find(
                    "Universal Render Pipeline/Particles/Unlit");
                if (shader == null)
                {
                    shader = Shader.Find("Particles/Standard Unlit");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            Color color = new Color(0.96f, 0.98f, 1f, 1f);
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureAssetFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int separator = path.LastIndexOf('/');
            string parent = path.Substring(0, separator);
            string folderName = path.Substring(separator + 1);
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static bool HasActiveVolume(GameObject root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int index = 0; index < components.Length; index++)
            {
                Component component = components[index];
                if (component == null ||
                    component.GetType().FullName != "UnityEngine.Rendering.Volume")
                {
                    continue;
                }

                Behaviour behaviour = component as Behaviour;
                if (behaviour == null || behaviour.enabled)
                {
                    return component.gameObject.activeInHierarchy;
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
            MonoBehaviour[] behaviours = camera.GetComponents<MonoBehaviour>();
            for (int index = 0; index < behaviours.Length; index++)
            {
                MonoBehaviour behaviour = behaviours[index];
                if (behaviour == null ||
                    behaviour.GetType().FullName !=
                    "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData")
                {
                    continue;
                }

                SerializedObject serializedCameraData =
                    new SerializedObject(behaviour);
                SerializedProperty postProcessing =
                    serializedCameraData.FindProperty("m_RenderPostProcessing");
                return postProcessing != null && postProcessing.boolValue;
            }

            return false;
        }

        private static Color FromHex(int rgb)
        {
            float red = ((rgb >> 16) & 0xFF) / 255f;
            float green = ((rgb >> 8) & 0xFF) / 255f;
            float blue = (rgb & 0xFF) / 255f;
            return new Color(red, green, blue, 1f);
        }
    }
}
