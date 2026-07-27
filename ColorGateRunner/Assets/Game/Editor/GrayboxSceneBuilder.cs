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

        internal static readonly Color RedColor = FromHex(0xE63946);
        internal static readonly Color BlueColor = FromHex(0x2D7FF9);
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
            CreateFloor(generatedRoot.transform, neutralMaterial);

            GameObject playerObject = CreatePlayer(generatedRoot.transform, redMaterial);
            Renderer playerRenderer = playerObject.GetComponent<Renderer>();

            GameSceneController controller =
                generatedRoot.AddComponent<GameSceneController>();

            GateView[] gates = CreateGatePool(
                generatedRoot.transform,
                controller,
                redMaterial,
                blueMaterial);
            ParticleSystem successParticles = CreateSuccessParticles(
                generatedRoot.transform,
                particleMaterial);

            Canvas canvas = CreateCanvas(generatedRoot.transform);
            GameplayTapSurface tapSurface = CreateTapSurface(canvas.transform);
            GameObject scorePanel;
            Text scoreLabel;
            Text scoreText;
            RectTransform scorePulseTarget;
            CreateScoreHud(
                canvas.transform,
                out scorePanel,
                out scoreLabel,
                out scoreText,
                out scorePulseTarget);
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
            Text gameOverScore;
            Text bestScore;
            Button restartButton;
            CreateGameOverUi(
                canvas.transform,
                out gameOverPanel,
                out gameOverScore,
                out bestScore,
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
                failureMaterial,
                scorePanel,
                scoreLabel,
                scoreText,
                scorePulseTarget,
                readyOverlay,
                readyTitle,
                readyInstruction,
                readyTap,
                gameOverPanel,
                gameOverScore,
                bestScore,
                restartButton,
                tapSurface,
                successParticles,
                gates);

            gameOverPanel.SetActive(false);
            readyOverlay.SetActive(true);

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

            if (particleSystems.Length != 1 ||
                CountNamedTransforms(generatedRoot, "Canvas") != 1 ||
                CountNamedTransforms(generatedRoot, "ReadyOverlay") != 1 ||
                CountNamedTransforms(generatedRoot, "ReadyTitle") != 1 ||
                CountNamedTransforms(generatedRoot, "ReadyInstruction") != 1 ||
                CountNamedTransforms(generatedRoot, "TapToStartVisual") != 1 ||
                CountNamedTransforms(generatedRoot, "ScorePanel") != 1 ||
                CountNamedTransforms(generatedRoot, "ScoreLabel") != 1 ||
                CountNamedTransforms(generatedRoot, "ScoreValue") != 1 ||
                CountNamedTransforms(generatedRoot, "GameOverPanel") != 1 ||
                CountNamedTransforms(generatedRoot, "GameOverScore") != 1 ||
                CountNamedTransforms(generatedRoot, "BestScore") != 1 ||
                CountNamedTransforms(generatedRoot, "RestartButton") != 1 ||
                CountNamedTransforms(generatedRoot, "SuccessParticles") != 1 ||
                CountNamedTransforms(generatedRoot, "EventSystem") != 1)
            {
                throw new InvalidOperationException(
                    "Generated feedback or UI objects are missing or duplicated.");
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

        private static void CreateFloor(Transform parent, Material material)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.position = new Vector3(0f, -0.1f, 500f);
            floor.transform.localScale = new Vector3(8f, 0.2f, 1000f);
            floor.GetComponent<Renderer>().sharedMaterial = material;
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

                RunnerColor color = previewSession.GetNextGateColor();
                Material material =
                    color == RunnerColor.Red ? redMaterial : blueMaterial;
                gateZ += previewSession.GetNextGateSpacing();
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

        private static void CreateScoreHud(
            Transform parent,
            out GameObject panel,
            out Text label,
            out Text value,
            out RectTransform pulseTarget)
        {
            panel = CreateUiObject("ScorePanel", parent);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -64f);
            panelRect.sizeDelta = new Vector2(280f, 150f);

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
            labelRect.anchorMin = new Vector2(0f, 0.65f);
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
            valueRect.anchorMin = Vector2.zero;
            valueRect.anchorMax = new Vector2(1f, 0.72f);
            valueRect.offsetMin = Vector2.zero;
            valueRect.offsetMax = Vector2.zero;
            pulseTarget = valueRect;
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
            out Text currentScore,
            out Text bestScore,
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
                "SCORE  0",
                48,
                TextAnchor.MiddleCenter);
            RectTransform currentScoreRect = currentScore.rectTransform;
            currentScoreRect.anchorMin = new Vector2(0.1f, 0.47f);
            currentScoreRect.anchorMax = new Vector2(0.9f, 0.57f);
            currentScoreRect.offsetMin = Vector2.zero;
            currentScoreRect.offsetMax = Vector2.zero;

            bestScore = CreateText(
                "BestScore",
                panel.transform,
                "BEST  0",
                40,
                TextAnchor.MiddleCenter);
            RectTransform bestScoreRect = bestScore.rectTransform;
            bestScoreRect.anchorMin = new Vector2(0.1f, 0.39f);
            bestScoreRect.anchorMax = new Vector2(0.9f, 0.48f);
            bestScoreRect.offsetMin = Vector2.zero;
            bestScoreRect.offsetMax = Vector2.zero;

            GameObject buttonObject = CreateUiObject("RestartButton", panel.transform);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.25f, 0.24f);
            buttonRect.anchorMax = new Vector2(0.75f, 0.34f);
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
