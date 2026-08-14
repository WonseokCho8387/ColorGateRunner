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
    public static class FrontendSceneBuilder
    {
        internal const string ScenePath = "Assets/Scenes/Frontend.unity";
        internal const string RootName = "FrontendRoot";

        [MenuItem(
            "Tools/Color Gate Runner/Build Frontend Scene",
            priority = 21)]
        public static void BuildFrontendScene()
        {
            string campaignPath = SelectCampaignScenePath(
                EditorBuildSettings.scenes);
            Scene scene = OpenOrCreateFrontendScene();
            RemoveGeneratedRoot(scene);

            var root = new GameObject(RootName);
            FrontendSceneController controller =
                root.AddComponent<FrontendSceneController>();
            CreateCamera(root.transform);
            Canvas canvas = CreateCanvas(root.transform);
            GameObject safeArea = CreateSafeArea(canvas.transform);
            GameObject sharedHeader = CreateFlowRoot(
                "SharedHeaderRoot",
                safeArea.transform);
            CreateSharedHeader(sharedHeader.transform);

            GameObject titleRoot = CreateFlowRoot(
                "AccountChoicePageRoot",
                safeArea.transform);
            CreateTitlePage(
                titleRoot.transform,
                out Text titleProfile,
                out Text titleAccount,
                out Text titleVersion,
                out Button titleStart,
                out Button titleAccountAction,
                out Button titleSettings,
                out GameObject legalRoot);

            GameObject lobbyRoot = CreateFlowRoot(
                "LobbyPageRoot",
                safeArea.transform);
            CreateLobbyPage(
                lobbyRoot.transform,
                out Text lobbyProfile,
                out Text lobbyStage,
                out Text lobbyStageTitle,
                out Text lobbyStageMechanic,
                out Text lobbyProgress,
                out Button lobbyPlay,
                out Button lobbyBack,
                out Button lobbySettings,
                out GameObject currencySlot,
                out GameObject eventSlot,
                out GameObject notificationSlot,
                out GameObject lobbyTheme,
                out LobbyProgressionPanel lobbyProgression);

            GameObject popupRoot = CreateOverlayRoot(
                "PopupRoot",
                canvas.transform,
                new Color(0f, 0f, 0f, 0.72f),
                true);
            CreatePopup(
                popupRoot.transform,
                out Text modalTitle,
                out Text modalMessage,
                out Button modalConfirm,
                out Text modalConfirmText,
                out Button modalCancel,
                out Text modalCancelText);
            SettingsPanelController settingsPanel =
                CreateSettingsPanel(popupRoot.transform);

            GameObject loadingRoot = CreateOverlayRoot(
                "LoadingRoot",
                canvas.transform,
                new Color(0.03f, 0.05f, 0.09f, 0.82f),
                true);
            CreateText(
                "LoadingText",
                loadingRoot.transform,
                "LOADING",
                32,
                new Vector2(0.2f, 0.42f),
                new Vector2(0.8f, 0.58f));

            GameObject transitionBlocker = CreateOverlayRoot(
                "TransitionBlockerRoot",
                canvas.transform,
                new Color(0f, 0f, 0f, 0.01f),
                true);
            GameObject developmentDebug = CreateFlowRoot(
                "DevelopmentDebugRoot",
                canvas.transform);
            CreateText(
                "DevelopmentLabel",
                developmentDebug.transform,
                "GOOGLE PROVIDER NOT INSTALLED",
                12,
                new Vector2(0.02f, 0.01f),
                new Vector2(0.28f, 0.05f));
            CreateEventSystem(root.transform);

            controller.Configure(
                titleRoot,
                lobbyRoot,
                popupRoot,
                loadingRoot,
                transitionBlocker,
                developmentDebug,
                titleProfile,
                titleAccount,
                titleVersion,
                titleStart,
                titleAccountAction,
                titleSettings,
                legalRoot,
                lobbyProfile,
                lobbyStage,
                lobbyStageTitle,
                lobbyStageMechanic,
                lobbyProgress,
                lobbyPlay,
                lobbyBack,
                lobbySettings,
                currencySlot,
                eventSlot,
                notificationSlot,
                lobbyTheme,
                lobbyProgression,
                modalTitle,
                modalMessage,
                modalConfirm,
                modalConfirmText,
                modalCancel,
                modalCancelText,
                settingsPanel,
                campaignPath);

            titleRoot.SetActive(true);
            lobbyRoot.SetActive(false);
            popupRoot.SetActive(false);
            settingsPanel.gameObject.SetActive(false);
            loadingRoot.SetActive(false);
            transitionBlocker.SetActive(false);
            developmentDebug.SetActive(false);
            legalRoot.SetActive(false);
            currencySlot.SetActive(true);
            eventSlot.SetActive(false);
            notificationSlot.SetActive(false);
            lobbyTheme.SetActive(true);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = CreateFrontendSceneList(campaignPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BootSceneBuilder.BuildBootScene(ScenePath);
            ValidateBootDestination();
            ValidateGeneratedScene(campaignPath);
            Debug.Log(
                $"Frontend Scene built: {ScenePath} -> {campaignPath}");
        }

        public static void BuildFrontendSceneFromCommandLine()
        {
            BuildFrontendScene();
        }

        internal static string SelectCampaignScenePath(
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
                "Frontend requires one active Campaign Scene.");
        }

        internal static EditorBuildSettingsScene[] CreateFrontendSceneList(
            string campaignPath)
        {
            if (string.IsNullOrWhiteSpace(campaignPath) ||
                PathsEqual(campaignPath, BootSceneBuilder.ScenePath) ||
                PathsEqual(campaignPath, ScenePath))
            {
                throw new InvalidOperationException(
                    "Campaign must be distinct from Boot and Frontend.");
            }

            return new[]
            {
                new EditorBuildSettingsScene(
                    BootSceneBuilder.ScenePath,
                    true),
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(campaignPath, true)
            };
        }

        internal static void ValidateGeneratedScene(string campaignPath)
        {
            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
            GameObject root = null;
            int rootCount = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == RootName)
                {
                    root = roots[index];
                    rootCount++;
                }
            }

            if (rootCount != 1 || root == null)
            {
                throw new InvalidOperationException(
                    "FrontendRoot is missing or duplicated.");
            }

            FrontendSceneController[] controllers =
                root.GetComponentsInChildren<FrontendSceneController>(true);
            EventSystem[] eventSystems =
                root.GetComponentsInChildren<EventSystem>(true);
            Camera[] cameras = root.GetComponentsInChildren<Camera>(true);
            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            if (controllers.Length != 1 ||
                !controllers[0].HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Frontend controller is missing or incomplete.");
            }
            if (eventSystems.Length != 1 || cameras.Length != 1 ||
                canvases.Length != 1)
            {
                throw new InvalidOperationException(
                    "Frontend requires one Camera, Canvas, and EventSystem.");
            }
            if (root.GetComponentInChildren<SafeAreaLayout>(true) == null)
            {
                throw new InvalidOperationException(
                    "Frontend Safe Area is missing.");
            }
            if (!PathsEqual(
                    controllers[0].CampaignScenePath,
                    campaignPath))
            {
                throw new InvalidOperationException(
                    "Frontend Campaign destination is invalid.");
            }

            string[] uniqueNames =
            {
                "FrontendCamera", "FrontendCanvas", "SafeAreaRoot",
                "SharedHeaderRoot", "AccountChoicePageRoot", "LobbyPageRoot",
                "PopupRoot", "LoadingRoot", "TransitionBlockerRoot",
                "DevelopmentDebugRoot", "EventSystem",
                "GuestStartButton", "GoogleProviderButton",
                "TitleSettingsButton", "LobbyPlayCampaignButton",
                "LobbyAccountButton", "LobbySettingsButton",
                "CurrencySlotRoot", "EventModuleSlotRoot",
                "NotificationSlotRoot", "LobbyThemeRoot",
                "SettingsPanel"
            };
            for (int index = 0; index < uniqueNames.Length; index++)
            {
                if (CountNamedTransforms(root, uniqueNames[index]) != 1)
                {
                    throw new InvalidOperationException(
                        uniqueNames[index] + " is missing or duplicated.");
                }
            }

            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transforms[index].gameObject) > 0)
                {
                    throw new InvalidOperationException(
                        "Missing script on " + transforms[index].name + ".");
                }
            }

            EditorBuildSettingsScene[] buildScenes =
                EditorBuildSettings.scenes;
            if (buildScenes.Length != 3 ||
                !buildScenes[0].enabled ||
                !PathsEqual(
                    buildScenes[0].path,
                    BootSceneBuilder.ScenePath) ||
                !buildScenes[1].enabled ||
                !PathsEqual(buildScenes[1].path, ScenePath) ||
                !buildScenes[2].enabled ||
                !PathsEqual(buildScenes[2].path, campaignPath))
            {
                throw new InvalidOperationException(
                    "Build Settings must be Boot, Frontend, Campaign.");
            }
        }

        private static Scene OpenOrCreateFrontendScene()
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null
                ? EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single)
                : EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,
                    NewSceneMode.Single);
        }

        private static void RemoveGeneratedRoot(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                if (roots[index].name == RootName)
                {
                    UnityEngine.Object.DestroyImmediate(roots[index]);
                }
            }
        }

        private static void ValidateBootDestination()
        {
            Scene scene = EditorSceneManager.OpenScene(
                BootSceneBuilder.ScenePath,
                OpenSceneMode.Single);
            BootSceneController controller = null;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int index = 0; index < roots.Length; index++)
            {
                controller = roots[index]
                    .GetComponentInChildren<BootSceneController>(true);
                if (controller != null)
                {
                    break;
                }
            }
            if (controller == null ||
                !PathsEqual(controller.DestinationScenePath, ScenePath))
            {
                throw new InvalidOperationException(
                    "Boot does not target the Frontend Scene.");
            }
        }

        private static void CreateCamera(Transform parent)
        {
            var cameraObject = new GameObject(
                "FrontendCamera",
                typeof(Camera));
            cameraObject.transform.SetParent(parent, false);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FromHex(0x0B1020);
            camera.orthographic = true;
            cameraObject.tag = "MainCamera";
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            var canvasObject = new GameObject(
                "FrontendCanvas",
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
                "SafeAreaRoot",
                typeof(RectTransform),
                typeof(SafeAreaLayout));
            safeArea.transform.SetParent(parent, false);
            Stretch(safeArea.GetComponent<RectTransform>());
            return safeArea;
        }

        private static GameObject CreateFlowRoot(string name, Transform parent)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>());
            return root;
        }

        private static GameObject CreateOverlayRoot(
            string name,
            Transform parent,
            Color color,
            bool blocksRaycasts)
        {
            var root = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image));
            root.transform.SetParent(parent, false);
            Stretch(root.GetComponent<RectTransform>());
            Image image = root.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = blocksRaycasts;
            return root;
        }

        private static void CreateSharedHeader(Transform parent)
        {
            Text title = CreateText(
                "SharedHeaderTitle",
                parent,
                "COLOR GATE RUNNER",
                18,
                new Vector2(0.08f, 0.93f),
                new Vector2(0.92f, 0.98f));
            title.color = new Color(1f, 1f, 1f, 0.72f);
        }

        private static void CreateTitlePage(
            Transform parent,
            out Text profile,
            out Text account,
            out Text version,
            out Button start,
            out Button accountAction,
            out Button settings,
            out GameObject legalRoot)
        {
            CreateText(
                "AccountChoiceTitle",
                parent,
                "CHOOSE ACCOUNT",
                42,
                new Vector2(0.08f, 0.59f),
                new Vector2(0.92f, 0.82f));
            profile = CreateText(
                "AccountChoiceProfileText",
                parent,
                "GUEST",
                20,
                new Vector2(0.16f, 0.49f),
                new Vector2(0.84f, 0.55f));
            account = CreateText(
                "AccountChoiceDescriptionText",
                parent,
                "PLAY LOCALLY AS A GUEST",
                14,
                new Vector2(0.16f, 0.45f),
                new Vector2(0.84f, 0.49f));
            start = CreateButton(
                "GuestStartButton",
                parent,
                "START AS GUEST",
                new Vector2(0.12f, 0.29f),
                new Vector2(0.88f, 0.40f),
                FromHex(0x2D7FF9),
                out _);
            accountAction = CreateButton(
                "GoogleProviderButton",
                parent,
                "CONTINUE WITH GOOGLE",
                new Vector2(0.12f, 0.19f),
                new Vector2(0.49f, 0.26f),
                FromHex(0x253047),
                out _);
            settings = CreateButton(
                "TitleSettingsButton",
                parent,
                "SETTINGS",
                new Vector2(0.51f, 0.19f),
                new Vector2(0.88f, 0.26f),
                FromHex(0x253047),
                out _);
            version = CreateText(
                "TitleVersionText",
                parent,
                "v0.0.0",
                13,
                new Vector2(0.2f, 0.08f),
                new Vector2(0.8f, 0.12f));
            version.color = new Color(1f, 1f, 1f, 0.56f);
            legalRoot = CreateFlowRoot("TitleLegalRoot", parent);
        }

        private static void CreateLobbyPage(
            Transform parent,
            out Text profile,
            out Text stage,
            out Text stageTitle,
            out Text stageMechanic,
            out Text progress,
            out Button play,
            out Button back,
            out Button settings,
            out GameObject currencySlot,
            out GameObject eventSlot,
            out GameObject notificationSlot,
            out GameObject themeRoot,
            out LobbyProgressionPanel progressionPanel)
        {
            themeRoot = CreateFlowRoot("LobbyThemeRoot", parent);
            themeRoot.transform.SetAsFirstSibling();
            progressionPanel = themeRoot.AddComponent<LobbyProgressionPanel>();
            Image background = CreatePanel(
                "LobbyThemeBackground",
                themeRoot.transform,
                Vector2.zero,
                Vector2.one,
                new Color(0.10f, 0.16f, 0.24f, 0.84f))
                .GetComponent<Image>();
            background.raycastTarget = false;
            CreatePanel(
                "LobbyTopBar",
                themeRoot.transform,
                new Vector2(0.04f, 0.84f),
                new Vector2(0.96f, 0.96f),
                new Color(0.04f, 0.07f, 0.13f, 0.92f))
                .GetComponent<Image>().raycastTarget = false;
            CreatePanel(
                "LobbyHeroPanel",
                themeRoot.transform,
                new Vector2(0.05f, 0.35f),
                new Vector2(0.95f, 0.82f),
                new Color(0.06f, 0.11f, 0.20f, 0.56f))
                .GetComponent<Image>().raycastTarget = false;
            CreatePanel(
                "LobbyStageCard",
                themeRoot.transform,
                new Vector2(0.05f, 0.05f),
                new Vector2(0.95f, 0.33f),
                new Color(0.04f, 0.07f, 0.13f, 0.92f))
                .GetComponent<Image>().raycastTarget = false;

            back = CreateButton(
                "LobbyAccountButton",
                parent,
                "GUEST",
                new Vector2(0.055f, 0.865f),
                new Vector2(0.25f, 0.935f),
                FromHex(0x253047),
                out profile);
            settings = CreateButton(
                "LobbySettingsButton",
                parent,
                "SETTINGS",
                new Vector2(0.76f, 0.865f),
                new Vector2(0.945f, 0.935f),
                FromHex(0x253047),
                out _);
            stage = CreateText(
                "LobbyRecommendedStageText",
                parent,
                "STAGE 1",
                20,
                new Vector2(0.10f, 0.255f),
                new Vector2(0.32f, 0.315f));
            stage.alignment = TextAnchor.MiddleLeft;
            stageTitle = CreateText(
                "LobbyStageTitleText",
                parent,
                "TWO-COLOR BASICS",
                22,
                new Vector2(0.32f, 0.255f),
                new Vector2(0.90f, 0.315f));
            stageTitle.alignment = TextAnchor.MiddleRight;
            stageMechanic = CreateText(
                "LobbyStageMechanicText",
                parent,
                "COLOR MATCH   NORMAL",
                14,
                new Vector2(0.10f, 0.215f),
                new Vector2(0.58f, 0.255f));
            stageMechanic.alignment = TextAnchor.MiddleLeft;
            progress = CreateText(
                "LobbyProgressText",
                parent,
                "0 / 20 CLEARED",
                14,
                new Vector2(0.58f, 0.215f),
                new Vector2(0.90f, 0.255f));
            progress.alignment = TextAnchor.MiddleRight;
            play = CreateButton(
                "LobbyPlayCampaignButton",
                parent,
                "PLAY",
                new Vector2(0.10f, 0.08f),
                new Vector2(0.90f, 0.20f),
                FromHex(0x22C55E),
                out _);
            currencySlot = CreateFlowRoot("CurrencySlotRoot", parent);
            eventSlot = CreateFlowRoot("EventModuleSlotRoot", parent);
            notificationSlot = CreateFlowRoot(
                "NotificationSlotRoot",
                parent);
            Text coins = CreateText(
                "LobbyCoinText",
                currencySlot.transform,
                "COINS 0",
                16,
                new Vector2(0.27f, 0.865f),
                new Vector2(0.48f, 0.935f));
            Text hearts = CreateText(
                "LobbyHeartText",
                currencySlot.transform,
                "HEARTS 5/5",
                14,
                new Vector2(0.49f, 0.865f),
                new Vector2(0.74f, 0.935f));
            Text inventory = CreateText(
                "LobbyInventoryText",
                currencySlot.transform,
                "SHIELD 0   BOOSTER 0",
                13,
                new Vector2(0.54f, 0.355f),
                new Vector2(0.92f, 0.405f));
            inventory.alignment = TextAnchor.MiddleRight;
            Text theme = CreateText(
                "LobbyThemeText",
                themeRoot.transform,
                "COLOR COURTYARD",
                26,
                new Vector2(0.08f, 0.70f),
                new Vector2(0.92f, 0.78f));
            Text nextUpgrade = CreateText(
                "LobbyNextUpgradeText",
                themeRoot.transform,
                "LOBBY 0/18   NEXT STAGE 2",
                14,
                new Vector2(0.08f, 0.64f),
                new Vector2(0.92f, 0.69f));
            Text rewardSummary = CreateText(
                "LobbyRewardSummaryText",
                themeRoot.transform,
                "LOBBY UPGRADE + REWARD",
                15,
                new Vector2(0.10f, 0.56f),
                new Vector2(0.90f, 0.62f));
            var visuals = new GameObject[6];
            Color[] colors =
            {
                FromHex(0xE63946), FromHex(0x2D7FF9),
                FromHex(0x22C55E), FromHex(0xF4C430),
                FromHex(0x9B5DE5), FromHex(0x00B8D9)
            };
            for (int index = 0; index < visuals.Length; index++)
            {
                float left = 0.10f + (index * 0.135f);
                visuals[index] = CreatePanel(
                    $"LobbyUpgradeVisual_{index + 1}",
                    themeRoot.transform,
                    new Vector2(left, 0.43f),
                    new Vector2(left + 0.10f, 0.50f),
                    colors[index]);
                visuals[index].GetComponent<Image>().raycastTarget = false;
                visuals[index].SetActive(false);
            }
            progressionPanel.Configure(
                coins,
                hearts,
                inventory,
                theme,
                nextUpgrade,
                rewardSummary,
                background,
                visuals);
        }

        private static void CreatePopup(
            Transform parent,
            out Text title,
            out Text message,
            out Button confirm,
            out Text confirmText,
            out Button cancel,
            out Text cancelText)
        {
            GameObject panel = CreatePanel(
                "BlockingModalPanel",
                parent,
                new Vector2(0.08f, 0.29f),
                new Vector2(0.92f, 0.67f),
                FromHex(0x172033));
            title = CreateText(
                "ModalTitleText",
                panel.transform,
                "NOTICE",
                28,
                new Vector2(0.08f, 0.70f),
                new Vector2(0.92f, 0.91f));
            message = CreateText(
                "ModalMessageText",
                panel.transform,
                string.Empty,
                18,
                new Vector2(0.08f, 0.31f),
                new Vector2(0.92f, 0.69f));
            confirm = CreateButton(
                "ModalConfirmButton",
                panel.transform,
                "EXIT",
                new Vector2(0.08f, 0.08f),
                new Vector2(0.48f, 0.27f),
                FromHex(0xC33A48),
                out confirmText);
            cancel = CreateButton(
                "ModalCancelButton",
                panel.transform,
                "CLOSE",
                new Vector2(0.52f, 0.08f),
                new Vector2(0.92f, 0.27f),
                FromHex(0x2D7FF9),
                out cancelText);
        }

        private static SettingsPanelController CreateSettingsPanel(
            Transform parent)
        {
            return UnifiedSettingsPanelBuilder.Create(parent);
        }

        private static Slider CreateSettingSlider(
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
            GameObject sliderObject = CreatePanel(
                name,
                parent,
                new Vector2(0.34f, top + 0.025f),
                new Vector2(0.70f, top + 0.065f),
                FromHex(0x253047));
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            GameObject fill = CreatePanel(
                "Fill",
                sliderObject.transform,
                Vector2.zero,
                Vector2.one,
                FromHex(0x2D7FF9));
            GameObject handle = CreatePanel(
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

        private static Toggle CreateSettingToggle(
            string name,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject toggleObject = CreatePanel(
                name,
                parent,
                anchorMin,
                anchorMax,
                FromHex(0x253047));
            Toggle toggle = toggleObject.AddComponent<Toggle>();
            CreateText(
                "Label",
                toggleObject.transform,
                label,
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
            SetAnchors(
                panel.GetComponent<RectTransform>(),
                anchorMin,
                anchorMax);
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
            SetAnchors(
                textObject.GetComponent<RectTransform>(),
                anchorMin,
                anchorMax);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 11;
            text.resizeTextMaxSize = fontSize;
            return text;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color,
            out Text labelText)
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
            buttonObject.GetComponent<Image>().color = color;
            labelText = CreateText(
                "Label",
                buttonObject.transform,
                label,
                19,
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

        private static int CountNamedTransforms(GameObject root, string name)
        {
            int count = root.name == name ? 1 : 0;
            Transform[] transforms =
                root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index] != root.transform &&
                    transforms[index].name == name)
                {
                    count++;
                }
            }
            return count;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
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
