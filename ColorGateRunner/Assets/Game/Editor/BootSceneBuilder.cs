using System;
using System.Collections.Generic;
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
    public static class BootSceneBuilder
    {
        internal const string ScenePath = "Assets/Scenes/Boot.unity";
        internal const string GeneratedRootName = "ColorGateRunner_Boot";
        internal const string AppRootName = "AppRoot";

        [MenuItem("Tools/Color Gate Runner/Build Boot Scene", priority = 20)]
        public static void BuildBootScene()
        {
            string destinationPath = SelectDestinationScenePath(
                EditorBuildSettings.scenes);
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            var appRootObject = new GameObject(AppRootName);
            AppRoot appRoot = appRootObject.AddComponent<AppRoot>();

            var root = new GameObject(GeneratedRootName);
            BootSceneController controller =
                root.AddComponent<BootSceneController>();
            CreateCamera(root.transform);
            Canvas canvas = CreateCanvas(root.transform);
            GameObject safeArea = CreateSafeArea(canvas.transform);
            CreateBootUi(
                safeArea.transform,
                out GameObject loadingRoot,
                out Text loadingText,
                out Text versionText,
                out GameObject errorPanel,
                out Text errorText,
                out Button retryButton);
            CreateEventSystem(root.transform);

            controller.Configure(
                appRoot,
                loadingRoot,
                loadingText,
                versionText,
                errorPanel,
                errorText,
                retryButton,
                destinationPath);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBootFirstInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateBuiltScene(destinationPath);
            Debug.Log(
                $"Boot Scene built: {ScenePath} -> {destinationPath}");
        }

        public static void BuildBootSceneFromCommandLine()
        {
            BuildBootScene();
        }

        internal static string SelectDestinationScenePath(
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
                    !PathsEqual(candidate.path, ScenePath))
                {
                    return candidate.path;
                }
            }

            throw new InvalidOperationException(
                "Boot requires an active non-Boot destination Scene.");
        }

        internal static EditorBuildSettingsScene[] CreateBootFirstSceneList(
            EditorBuildSettingsScene[] existing)
        {
            var result = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            if (existing == null)
            {
                return result.ToArray();
            }

            for (int index = 0; index < existing.Length; index++)
            {
                EditorBuildSettingsScene scene = existing[index];
                if (!PathsEqual(scene.path, ScenePath))
                {
                    result.Add(new EditorBuildSettingsScene(
                        scene.path,
                        scene.enabled));
                }
            }

            return result.ToArray();
        }

        private static void EnsureBootFirstInBuildSettings()
        {
            EditorBuildSettings.scenes = CreateBootFirstSceneList(
                EditorBuildSettings.scenes);
        }

        private static void ValidateBuiltScene(string destinationPath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length < 2 || !scenes[0].enabled ||
                !PathsEqual(scenes[0].path, ScenePath))
            {
                throw new InvalidOperationException(
                    "Boot is not the first active Build Settings Scene.");
            }
            if (PathsEqual(destinationPath, ScenePath))
            {
                throw new InvalidOperationException(
                    "Boot cannot select itself as its destination.");
            }

            bool destinationActive = false;
            for (int index = 0; index < scenes.Length; index++)
            {
                if (scenes[index].enabled &&
                    PathsEqual(scenes[index].path, destinationPath))
                {
                    destinationActive = true;
                    break;
                }
            }
            if (!destinationActive)
            {
                throw new InvalidOperationException(
                    "The serialized Boot destination is not active.");
            }

            Scene scene = SceneManager.GetActiveScene();
            GameObject root = GameObject.Find(GeneratedRootName);
            GameObject appRoot = GameObject.Find(AppRootName);
            if (scene.path != ScenePath || root == null || appRoot == null ||
                root.GetComponent<BootSceneController>() == null ||
                appRoot.GetComponent<AppRoot>() == null)
            {
                throw new InvalidOperationException(
                    "The Boot Scene required roots are incomplete.");
            }
            if (UnityEngine.Object.FindObjectsByType<EventSystem>().Length != 1)
            {
                throw new InvalidOperationException(
                    "Boot must contain exactly one EventSystem.");
            }
            if (root.GetComponentInChildren<SafeAreaLayout>(true) == null)
            {
                throw new InvalidOperationException(
                    "Boot Safe Area layout is missing.");
            }

            BootSceneController controller =
                root.GetComponent<BootSceneController>();
            SerializedObject serialized = new SerializedObject(controller);
            string[] requiredReferences =
            {
                "appRoot", "loadingRoot", "loadingText", "versionText",
                "errorPanel", "errorText", "retryButton"
            };
            for (int index = 0; index < requiredReferences.Length; index++)
            {
                if (serialized.FindProperty(requiredReferences[index])
                        .objectReferenceValue == null)
                {
                    throw new InvalidOperationException(
                        "Boot reference is missing: " +
                        requiredReferences[index]);
                }
            }
            if (!PathsEqual(
                    serialized.FindProperty("destinationScenePath")
                        .stringValue,
                    destinationPath))
            {
                throw new InvalidOperationException(
                    "Boot destination serialization is invalid.");
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Component[] components = roots[rootIndex]
                    .GetComponentsInChildren<Component>(true);
                for (int componentIndex = 0;
                    componentIndex < components.Length;
                    componentIndex++)
                {
                    if (components[componentIndex] == null)
                    {
                        throw new InvalidOperationException(
                            "Boot contains a Missing Script.");
                    }
                }
            }
        }

        private static void CreateCamera(Transform parent)
        {
            var cameraObject = new GameObject("BootCamera");
            cameraObject.transform.SetParent(parent, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FromHex(0x10131D);
            camera.orthographic = true;
            cameraObject.tag = "MainCamera";
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            var canvasObject = new GameObject(
                "BootCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540f, 960f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static GameObject CreateSafeArea(Transform parent)
        {
            var safeArea = new GameObject(
                "SafeArea",
                typeof(RectTransform),
                typeof(SafeAreaLayout));
            safeArea.transform.SetParent(parent, false);
            RectTransform rect = safeArea.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return safeArea;
        }

        private static void CreateBootUi(
            Transform parent,
            out GameObject loadingRoot,
            out Text loadingText,
            out Text versionText,
            out GameObject errorPanel,
            out Text errorText,
            out Button retryButton)
        {
            CreateText(
                "GameTitle",
                parent,
                "COLOR GATE\nRUNNER",
                48,
                new Vector2(0.08f, 0.55f),
                new Vector2(0.92f, 0.82f));

            loadingRoot = CreatePanel(
                "LoadingRoot",
                parent,
                new Vector2(0.12f, 0.28f),
                new Vector2(0.88f, 0.46f),
                new Color(0.10f, 0.13f, 0.20f, 0.92f));
            loadingText = CreateText(
                "LoadingText",
                loadingRoot.transform,
                "LOADING",
                25,
                Vector2.zero,
                Vector2.one);

            versionText = CreateText(
                "VersionText",
                parent,
                "v0.0.0",
                16,
                new Vector2(0.15f, 0.06f),
                new Vector2(0.85f, 0.12f));
            versionText.color = new Color(1f, 1f, 1f, 0.58f);

            errorPanel = CreatePanel(
                "InitializationErrorPanel",
                parent,
                new Vector2(0.08f, 0.20f),
                new Vector2(0.92f, 0.50f),
                new Color(0.22f, 0.08f, 0.10f, 0.96f));
            errorText = CreateText(
                "ErrorText",
                errorPanel.transform,
                "LOCAL INITIALIZATION FAILED",
                20,
                new Vector2(0.08f, 0.46f),
                new Vector2(0.92f, 0.92f));
            retryButton = CreateButton(
                "RetryButton",
                errorPanel.transform,
                "RETRY",
                new Vector2(0.18f, 0.08f),
                new Vector2(0.82f, 0.40f));
            errorPanel.SetActive(false);
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            var panel = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            SetAnchors(rect, anchorMin, anchorMax);
            panel.GetComponent<Image>().color = color;
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
            var textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            SetAnchors(rect, anchorMin, anchorMax);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            SetAnchors(
                buttonObject.GetComponent<RectTransform>(),
                anchorMin,
                anchorMax);
            buttonObject.GetComponent<Image>().color = FromHex(0x2D7FF9);
            CreateText(
                "Label",
                buttonObject.transform,
                label,
                22,
                Vector2.zero,
                Vector2.one);
            return buttonObject.GetComponent<Button>();
        }

        private static void CreateEventSystem(Transform parent)
        {
            var eventObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventObject.transform.SetParent(parent, false);
        }

        private static void SetAnchors(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                left?.Replace('\\', '/'),
                right?.Replace('\\', '/'),
                StringComparison.OrdinalIgnoreCase);
        }

        private static Color FromHex(int value)
        {
            return new Color(
                ((value >> 16) & 0xFF) / 255f,
                ((value >> 8) & 0xFF) / 255f,
                (value & 0xFF) / 255f,
                1f);
        }
    }
}
