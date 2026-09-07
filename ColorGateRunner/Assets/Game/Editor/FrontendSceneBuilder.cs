using System;
using System.Collections.Generic;
using ColorGateRunner.Presentation;
using ColorGateRunner.Product;
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
            Theme01UiSkinBuilder.EnsureAndConfigure();
            LobbyThemeVisualCatalog themeVisualCatalog =
                LobbyThemeVisualCatalogBuilder.EnsureAndConfigure();
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
                out GameObject shopPage,
                out GameObject leaderboardPage,
                out GameObject journeyPage,
                out GameObject collectionPage,
                out Button shopNavigation,
                out Button leaderboardNavigation,
                out Button homeNavigation,
                out Button journeyNavigation,
                out Button collectionNavigation,
                out GameObject shopNavigationSelection,
                out GameObject leaderboardNavigationSelection,
                out GameObject homeNavigationSelection,
                out GameObject journeyNavigationSelection,
                out GameObject collectionNavigationSelection,
                out LobbyPagePager lobbyPagePager,
                out JourneyPageView journeyPageView,
                out GameObject currencySlot,
                out GameObject eventSlot,
                out GameObject notificationSlot,
                out GameObject lobbyTheme,
                out LobbyProgressionPanel lobbyProgression,
                themeVisualCatalog);

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
                "DEV  •  GOOGLE LOGIN OFF",
                9,
                new Vector2(0.22f, 0.958f),
                new Vector2(0.78f, 0.978f));
            CreateEventSystem(root.transform);

            controller.Configure(
                titleRoot,
                lobbyRoot,
                shopPage,
                leaderboardPage,
                journeyPage,
                collectionPage,
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
                shopNavigation,
                leaderboardNavigation,
                homeNavigation,
                journeyNavigation,
                collectionNavigation,
                shopNavigationSelection,
                leaderboardNavigationSelection,
                homeNavigationSelection,
                journeyNavigationSelection,
                collectionNavigationSelection,
                lobbyPagePager,
                journeyPageView,
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
            shopPage.SetActive(true);
            leaderboardPage.SetActive(true);
            journeyPage.SetActive(true);
            collectionPage.SetActive(true);
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
            shopNavigationSelection.SetActive(false);
            leaderboardNavigationSelection.SetActive(false);
            homeNavigationSelection.SetActive(true);
            journeyNavigationSelection.SetActive(false);
            collectionNavigationSelection.SetActive(false);
            lobbyPagePager.SetPage(FrontendPage.Lobby, false);

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
                    "Frontend controller is missing or incomplete: " +
                    (controllers.Length == 1
                        ? controllers[0].FindMissingReferenceGroup()
                        : "controller count"));
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
                "LobbyPageViewport", "ShopPageRoot", "LeaderboardPageRoot",
                "JourneyPageRoot", "CollectionPageRoot",
                "LobbyBottomNavigationBar",
                "PopupRoot", "LoadingRoot", "TransitionBlockerRoot",
                "DevelopmentDebugRoot", "EventSystem",
                "GuestStartButton", "GoogleProviderButton",
                "TitleSettingsButton", "LobbyPlayCampaignButton",
                "LobbyAccountButton", "LobbySettingsButton",
                "ShopNavigationButton", "LeaderboardNavigationButton",
                "HomeNavigationButton", "JourneyNavigationButton",
                "CollectionNavigationButton",
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
            out GameObject shopPage,
            out GameObject leaderboardPage,
            out GameObject journeyPage,
            out GameObject collectionPage,
            out Button shopNavigation,
            out Button leaderboardNavigation,
            out Button homeNavigation,
            out Button journeyNavigation,
            out Button collectionNavigation,
            out GameObject shopNavigationSelection,
            out GameObject leaderboardNavigationSelection,
            out GameObject homeNavigationSelection,
            out GameObject journeyNavigationSelection,
            out GameObject collectionNavigationSelection,
            out LobbyPagePager lobbyPagePager,
            out JourneyPageView journeyPageView,
            out GameObject currencySlot,
            out GameObject eventSlot,
            out GameObject notificationSlot,
            out GameObject themeRoot,
            out LobbyProgressionPanel progressionPanel,
            LobbyThemeVisualCatalog themeVisualCatalog)
        {
            GameObject viewportObject = new GameObject(
                "LobbyPageViewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D),
                typeof(LobbyPagePager));
            viewportObject.transform.SetParent(parent, false);
            Stretch(viewportObject.GetComponent<RectTransform>());
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewportImage.raycastTarget = true;
            lobbyPagePager = viewportObject.GetComponent<LobbyPagePager>();

            themeRoot = CreateFlowRoot(
                "LobbyThemeRoot",
                viewportObject.transform);
            themeRoot.transform.SetAsFirstSibling();
            themeRoot.AddComponent<LobbyPageSwipeForwarder>();
            progressionPanel = themeRoot.AddComponent<LobbyProgressionPanel>();
            Image background = CreatePanel(
                "LobbyThemeBackground",
                themeRoot.transform,
                Vector2.zero,
                Vector2.one,
                new Color(0.10f, 0.16f, 0.24f, 0.84f))
                .GetComponent<Image>();
            background.raycastTarget = false;
            Image artworkBackground = CreatePanel(
                "LobbyThemeArtworkBackground",
                themeRoot.transform,
                Vector2.zero,
                Vector2.one,
                Color.white).GetComponent<Image>();
            artworkBackground.raycastTarget = false;
            artworkBackground.preserveAspect = true;
            CreatePanel(
                "LobbyTopBar",
                themeRoot.transform,
                new Vector2(0.035f, 0.885f),
                new Vector2(0.965f, 0.955f),
                new Color(0.04f, 0.07f, 0.13f, 0.78f))
                .GetComponent<Image>().raycastTarget = false;
            CreatePanel(
                "LobbyHeroPanel",
                themeRoot.transform,
                new Vector2(0.035f, 0.305f),
                new Vector2(0.965f, 0.875f),
                new Color(0.02f, 0.05f, 0.11f, 0.08f))
                .GetComponent<Image>().raycastTarget = false;
            Image artworkMidground = CreatePanel(
                "LobbyThemeArtworkMidground",
                themeRoot.transform,
                new Vector2(0.08f, 0.27f),
                new Vector2(0.92f, 0.81f),
                Color.white).GetComponent<Image>();
            artworkMidground.raycastTarget = false;
            artworkMidground.preserveAspect = true;
            Image artworkForeground = CreatePanel(
                "LobbyThemeArtworkForeground",
                themeRoot.transform,
                Vector2.zero,
                Vector2.one,
                Color.white).GetComponent<Image>();
            artworkForeground.raycastTarget = false;
            artworkForeground.preserveAspect = true;
            CreatePanel(
                "LobbyStageCard",
                themeRoot.transform,
                new Vector2(0.045f, 0.135f),
                new Vector2(0.955f, 0.315f),
                new Color(0.04f, 0.07f, 0.13f, 0.76f))
                .GetComponent<Image>().raycastTarget = false;

            back = CreateButton(
                "LobbyAccountButton",
                parent,
                "GUEST",
                new Vector2(0.05f, 0.895f),
                new Vector2(0.235f, 0.945f),
                FromHex(0x253047),
                out profile);
            settings = CreateButton(
                "LobbySettingsButton",
                parent,
                "SETTINGS",
                new Vector2(0.78f, 0.895f),
                new Vector2(0.95f, 0.945f),
                FromHex(0x253047),
                out _);
            stage = CreateText(
                "LobbyRecommendedStageText",
                themeRoot.transform,
                "STAGE 1",
                20,
                new Vector2(0.10f, 0.255f),
                new Vector2(0.32f, 0.315f));
            stage.alignment = TextAnchor.MiddleLeft;
            stageTitle = CreateText(
                "LobbyStageTitleText",
                themeRoot.transform,
                "TWO-COLOR BASICS",
                22,
                new Vector2(0.32f, 0.255f),
                new Vector2(0.90f, 0.315f));
            stageTitle.alignment = TextAnchor.MiddleRight;
            stageMechanic = CreateText(
                "LobbyStageMechanicText",
                themeRoot.transform,
                "COLOR MATCH   NORMAL",
                14,
                new Vector2(0.10f, 0.215f),
                new Vector2(0.58f, 0.255f));
            stageMechanic.alignment = TextAnchor.MiddleLeft;
            progress = CreateText(
                "LobbyProgressText",
                themeRoot.transform,
                "0 / 20 CLEARED",
                14,
                new Vector2(0.58f, 0.215f),
                new Vector2(0.90f, 0.255f));
            progress.alignment = TextAnchor.MiddleRight;
            play = CreateButton(
                "LobbyPlayCampaignButton",
                themeRoot.transform,
                "PLAY",
                new Vector2(0.18f, 0.15f),
                new Vector2(0.82f, 0.225f),
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
                new Vector2(0.255f, 0.895f),
                new Vector2(0.48f, 0.945f));
            Text hearts = CreateText(
                "LobbyHeartText",
                currencySlot.transform,
                "HEARTS 5/5",
                14,
                new Vector2(0.50f, 0.895f),
                new Vector2(0.765f, 0.945f));
            Theme01UiSkinBuilder.AddStandaloneIcon(
                "LobbyCoinIcon",
                currencySlot.transform,
                "Coin",
                new Vector2(0.245f, 0.901f),
                new Vector2(0.285f, 0.939f));
            Theme01UiSkinBuilder.AddStandaloneIcon(
                "LobbyHeartIcon",
                currencySlot.transform,
                "Heart",
                new Vector2(0.49f, 0.901f),
                new Vector2(0.53f, 0.939f));
            CreatePanel(
                "LobbyInventoryChip",
                themeRoot.transform,
                new Vector2(0.325f, 0.675f),
                new Vector2(0.675f, 0.710f),
                new Color(0.04f, 0.07f, 0.13f, 0.72f))
                .GetComponent<Image>().raycastTarget = false;
            Text inventory = CreateText(
                "LobbyInventoryText",
                themeRoot.transform,
                "SHIELD 0   BOOSTER 0",
                13,
                new Vector2(0.34f, 0.675f),
                new Vector2(0.66f, 0.710f));
            inventory.alignment = TextAnchor.MiddleCenter;
            Text theme = CreateText(
                "LobbyThemeText",
                themeRoot.transform,
                "COLOR COURTYARD",
                26,
                new Vector2(0.08f, 0.815f),
                new Vector2(0.92f, 0.865f));
            Text nextUpgrade = CreateText(
                "LobbyNextUpgradeText",
                themeRoot.transform,
                "LOBBY 0/18   NEXT STAGE 2",
                14,
                new Vector2(0.08f, 0.777f),
                new Vector2(0.92f, 0.817f));
            Text rewardSummary = CreateText(
                "LobbyRewardSummaryText",
                themeRoot.transform,
                "LOBBY UPGRADE + REWARD",
                15,
                new Vector2(0.12f, 0.735f),
                new Vector2(0.88f, 0.770f));
            var visuals = new GameObject[6];
            Color[] colors =
            {
                FromHex(0xE63946), FromHex(0x2D7FF9),
                FromHex(0x22C55E), FromHex(0xF4C430),
                FromHex(0x9B5DE5), FromHex(0x00B8D9)
            };
            Vector2[] visualCenters =
            {
                new Vector2(0.20f, 0.720f),
                new Vector2(0.32f, 0.720f),
                new Vector2(0.44f, 0.720f),
                new Vector2(0.56f, 0.720f),
                new Vector2(0.68f, 0.720f),
                new Vector2(0.80f, 0.720f)
            };
            for (int index = 0; index < visuals.Length; index++)
            {
                Vector2 center = visualCenters[index];
                Vector2 halfSize = new Vector2(0.014f, 0.009f);
                visuals[index] = CreatePanel(
                    $"LobbyUpgradeVisual_{index + 1}",
                    themeRoot.transform,
                    center - halfSize,
                    center + halfSize,
                    colors[index]);
                Image visual = visuals[index].GetComponent<Image>();
                visual.raycastTarget = false;
                visual.rectTransform.localEulerAngles =
                    new Vector3(0f, 0f, 45f);
                Image core = CreatePanel(
                    "EnergyCore",
                    visuals[index].transform,
                    new Vector2(0.25f, 0.25f),
                    new Vector2(0.75f, 0.75f),
                    Color.white).GetComponent<Image>();
                core.raycastTarget = false;
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
                artworkBackground,
                artworkMidground,
                artworkForeground,
                themeVisualCatalog,
                visuals);

            shopPage = CreateShopPage(
                viewportObject.transform,
                lobbyPagePager);
            leaderboardPage = CreateComingSoonPage(
                viewportObject.transform,
                "LeaderboardPageRoot",
                "RANK",
                "COMPETITIVE CIRCUITS ARE BEING CALIBRATED",
                lobbyPagePager);
            journeyPage = CreateJourneyPage(
                viewportObject.transform,
                lobbyPagePager,
                out journeyPageView,
                themeVisualCatalog);
            collectionPage = CreateComingSoonPage(
                viewportObject.transform,
                "CollectionPageRoot",
                "COLLECTION",
                "NEW COLLECTIONS ARE BEING ASSEMBLED",
                lobbyPagePager);
            CreateLobbyNavigation(
                parent,
                out shopNavigation,
                out leaderboardNavigation,
                out homeNavigation,
                out journeyNavigation,
                out collectionNavigation,
                out shopNavigationSelection,
                out leaderboardNavigationSelection,
                out homeNavigationSelection,
                out journeyNavigationSelection,
                out collectionNavigationSelection);

            lobbyPagePager.Configure(
                viewportObject.GetComponent<RectTransform>(),
                new[]
                {
                    shopPage.GetComponent<RectTransform>(),
                    leaderboardPage.GetComponent<RectTransform>(),
                    themeRoot.GetComponent<RectTransform>(),
                    journeyPage.GetComponent<RectTransform>(),
                    collectionPage.GetComponent<RectTransform>()
                },
                new[]
                {
                    shopNavigation,
                    leaderboardNavigation,
                    homeNavigation,
                    journeyNavigation,
                    collectionNavigation
                },
                new[]
                {
                    shopNavigationSelection,
                    leaderboardNavigationSelection,
                    homeNavigationSelection,
                    journeyNavigationSelection,
                    collectionNavigationSelection
                });
            LobbyPageSwipeForwarder[] forwarders =
                viewportObject.GetComponentsInChildren<
                    LobbyPageSwipeForwarder>(true);
            for (int index = 0; index < forwarders.Length; index++)
            {
                forwarders[index].Configure(lobbyPagePager);
            }
        }

        private static GameObject CreateComingSoonPage(
            Transform parent,
            string rootName,
            string titleText,
            string messageText,
            LobbyPagePager pagePager)
        {
            GameObject root = CreateFlowRoot(rootName, parent);
            root.AddComponent<LobbyPageSwipeForwarder>().Configure(pagePager);
            Image background = CreatePanel(
                rootName.Replace("Root", "Background"),
                root.transform,
                Vector2.zero,
                Vector2.one,
                FromHex(0x07152A)).GetComponent<Image>();
            background.raycastTarget = true;
            Image glow = CreatePanel(
                "ComingSoonGlow",
                root.transform,
                new Vector2(0.08f, 0.29f),
                new Vector2(0.92f, 0.72f),
                new Color(0.05f, 0.58f, 0.78f, 0.18f))
                .GetComponent<Image>();
            glow.raycastTarget = false;
            Text title = CreateText(
                "ComingSoonPageTitle",
                root.transform,
                titleText,
                38,
                new Vector2(0.12f, 0.58f),
                new Vector2(0.88f, 0.67f));
            title.color = FromHex(0xE8FCFF);
            Text status = CreateText(
                "ComingSoonStatus",
                root.transform,
                "COMING SOON",
                28,
                new Vector2(0.16f, 0.48f),
                new Vector2(0.84f, 0.56f));
            status.color = FromHex(0x67E8F9);
            Text message = CreateText(
                "ComingSoonMessage",
                root.transform,
                messageText,
                15,
                new Vector2(0.14f, 0.41f),
                new Vector2(0.86f, 0.48f));
            message.color = new Color(0.75f, 0.88f, 0.95f, 0.82f);
            return root;
        }

        private static GameObject CreateJourneyPage(
            Transform parent,
            LobbyPagePager pagePager,
            out JourneyPageView journeyPageView,
            LobbyThemeVisualCatalog themeVisualCatalog)
        {
            GameObject root = CreateFlowRoot("JourneyPageRoot", parent);
            Image background = CreatePanel(
                "JourneyBackground",
                root.transform,
                Vector2.zero,
                Vector2.one,
                FromHex(0x07152A)).GetComponent<Image>();
            background.raycastTarget = false;
            Image glow = CreatePanel(
                "JourneyHeaderGlow",
                root.transform,
                new Vector2(0f, 0.72f),
                Vector2.one,
                new Color(0.17f, 0.18f, 0.62f, 0.34f))
                .GetComponent<Image>();
            glow.raycastTarget = false;
            Text title = CreateText(
                "JourneyTitleText",
                root.transform,
                "NEON JOURNEY",
                34,
                new Vector2(0.14f, 0.78f),
                new Vector2(0.86f, 0.85f));
            title.color = FromHex(0xE8FCFF);
            Text progress = CreateText(
                "JourneyProgressText",
                root.transform,
                "0 / 18  •  NEXT STAGE 2",
                15,
                new Vector2(0.10f, 0.735f),
                new Vector2(0.90f, 0.78f));
            progress.color = FromHex(0x7DDAFF);

            GameObject scrollObject = new GameObject(
                "JourneyScrollView",
                typeof(RectTransform),
                typeof(ScrollRect),
                typeof(LobbyPageSwipeForwarder));
            scrollObject.transform.SetParent(root.transform, false);
            SetAnchors(
                scrollObject.GetComponent<RectTransform>(),
                new Vector2(0.035f, 0.125f),
                new Vector2(0.965f, 0.73f));
            scrollObject.GetComponent<LobbyPageSwipeForwarder>()
                .Configure(pagePager);

            GameObject viewportObject = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            Stretch(viewportObject.GetComponent<RectTransform>());
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

            GameObject contentObject = new GameObject(
                "Content",
                typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 5200f);

            Image path = CreatePanel(
                "JourneyPath",
                contentRect,
                new Vector2(0.492f, 0.035f),
                new Vector2(0.508f, 0.965f),
                FromHex(0x155E75)).GetComponent<Image>();
            path.raycastTarget = false;

            var chapterViews = new JourneyChapterView[
                LobbyChapterPolicy.ChapterCount];
            for (int chapter = 0; chapter < chapterViews.Length; chapter++)
            {
                if (!themeVisualCatalog.TryGet(
                        chapter,
                        out LobbyThemeVisualDefinition theme))
                {
                    throw new InvalidOperationException(
                        $"Journey chapter theme {chapter} is missing.");
                }
                chapterViews[chapter] = CreateJourneyChapterPreview(
                    contentRect,
                    chapter,
                    theme);
            }

            var milestoneViews = new JourneyMilestoneView[
                JourneyMilestonePresentation.TotalMilestones];
            for (int index = 0; index < milestoneViews.Length; index++)
            {
                milestoneViews[index] = CreateJourneyMilestone(
                    contentRect,
                    index);
            }
            JourneyLeagueView leagueView = CreateJourneyLeague(contentRect);

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportObject.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.08f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.scrollSensitivity = 42f;
            journeyPageView = root.AddComponent<JourneyPageView>();
            journeyPageView.Configure(
                scroll,
                contentRect,
                progress,
                milestoneViews,
                chapterViews,
                leagueView);
            glow.transform.SetAsLastSibling();
            title.transform.SetAsLastSibling();
            progress.transform.SetAsLastSibling();
            return root;
        }

        private static JourneyChapterView CreateJourneyChapterPreview(
            Transform parent,
            int chapterIndex,
            LobbyThemeVisualDefinition theme)
        {
            GameObject root = CreatePanel(
                $"JourneyChapterCard_{chapterIndex + 1}",
                parent,
                new Vector2(0.04f, 0.01f),
                new Vector2(0.96f, 0.06f),
                chapterIndex switch
                {
                    0 => FromHex(0x123A5A),
                    1 => FromHex(0x40205E),
                    _ => FromHex(0x194A55)
                });
            Image preview = CreatePanel(
                "ChapterPreview",
                root.transform,
                new Vector2(0.02f, 0.08f),
                new Vector2(0.24f, 0.92f),
                Color.white).GetComponent<Image>();
            preview.sprite = theme.Background;
            preview.preserveAspect = true;
            preview.raycastTarget = false;
            Text title = CreateText(
                "ChapterTitle",
                root.transform,
                theme.DisplayName,
                21,
                new Vector2(0.27f, 0.52f),
                new Vector2(0.67f, 0.90f));
            title.alignment = TextAnchor.MiddleLeft;
            Text requirement = CreateText(
                "ChapterRequirement",
                root.transform,
                $"CHAPTER {chapterIndex + 1}",
                12,
                new Vector2(0.27f, 0.14f),
                new Vector2(0.67f, 0.50f));
            requirement.alignment = TextAnchor.MiddleLeft;
            Button select = CreateButton(
                "ChapterSelectButton",
                root.transform,
                "USE LOBBY",
                new Vector2(0.69f, 0.20f),
                new Vector2(0.97f, 0.80f),
                FromHex(0x0EA5E9),
                out Text selectText);
            var view = root.AddComponent<JourneyChapterView>();
            view.Configure(
                theme.ThemeId,
                chapterIndex,
                preview,
                title,
                requirement,
                select,
                selectText);
            return view;
        }

        private static JourneyLeagueView CreateJourneyLeague(
            Transform parent)
        {
            GameObject root = CreatePanel(
                "JourneyLeagueTerminalCard",
                parent,
                new Vector2(0.08f, 0.01f),
                new Vector2(0.92f, 0.05f),
                FromHex(0x492072));
            Text title = CreateText(
                "LeagueStateText",
                root.transform,
                "LEAGUE LOCKED",
                22,
                new Vector2(0.07f, 0.48f),
                new Vector2(0.93f, 0.90f));
            title.color = FromHex(0xF0ABFC);
            Text description = CreateText(
                "LeagueDescriptionText",
                root.transform,
                "CLEAR ALL LIVE STAGES",
                13,
                new Vector2(0.07f, 0.12f),
                new Vector2(0.93f, 0.50f));
            var view = root.AddComponent<JourneyLeagueView>();
            view.Configure(title, description);
            return view;
        }

        private static JourneyMilestoneView CreateJourneyMilestone(
            Transform parent,
            int index)
        {
            float centerY = 0.065f + (index * (0.87f / 17f));
            bool left = index % 2 == 0;
            Vector2 min = new Vector2(left ? 0.035f : 0.545f, centerY - 0.021f);
            Vector2 max = new Vector2(left ? 0.455f : 0.965f, centerY + 0.021f);
            GameObject root = CreatePanel(
                $"JourneyMilestoneCard_{index + 1}",
                parent,
                min,
                max,
                Color.white);
            Image card = root.GetComponent<Image>();
            card.raycastTarget = false;

            Image nodeGlow = CreatePanel(
                "MilestoneNodeGlow",
                parent,
                new Vector2(0.478f, centerY - 0.006f),
                new Vector2(0.522f, centerY + 0.006f),
                FromHex(0x67E8F9)).GetComponent<Image>();
            nodeGlow.raycastTarget = false;
            nodeGlow.rectTransform.localEulerAngles =
                new Vector3(0f, 0f, 45f);

            Text stage = CreateText(
                "StageText",
                root.transform,
                $"STAGE {(index + 1) * 2}",
                20,
                new Vector2(0.07f, 0.55f),
                new Vector2(0.62f, 0.90f));
            stage.alignment = TextAnchor.MiddleLeft;
            Text state = CreateText(
                "StateText",
                root.transform,
                "LOCKED",
                12,
                new Vector2(0.58f, 0.57f),
                new Vector2(0.94f, 0.88f));
            state.alignment = TextAnchor.MiddleRight;
            state.color = FromHex(0xA5F3FC);

            CreateJourneyRewardChip(
                root.transform,
                "CoinReward",
                "Coin",
                new Vector2(0.06f, 0.08f),
                new Vector2(0.34f, 0.52f),
                out Text coinText);
            GameObject shieldRoot = CreateJourneyRewardChip(
                root.transform,
                "ShieldReward",
                "Shield",
                new Vector2(0.36f, 0.08f),
                new Vector2(0.64f, 0.52f),
                out Text shieldText);
            GameObject boosterRoot = CreateJourneyRewardChip(
                root.transform,
                "BoosterReward",
                "Booster",
                new Vector2(0.66f, 0.08f),
                new Vector2(0.94f, 0.52f),
                out Text boosterText);

            JourneyMilestoneView view = root.AddComponent<JourneyMilestoneView>();
            view.Configure(
                card,
                nodeGlow,
                stage,
                state,
                coinText,
                shieldRoot,
                shieldText,
                boosterRoot,
                boosterText);
            return view;
        }

        private static GameObject CreateJourneyRewardChip(
            Transform parent,
            string name,
            string iconName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out Text amount)
        {
            GameObject root = CreateFlowRoot(name, parent);
            SetAnchors(root.GetComponent<RectTransform>(), anchorMin, anchorMax);
            Theme01UiSkinBuilder.AddStandaloneIcon(
                "Icon",
                root.transform,
                iconName,
                new Vector2(0f, 0.08f),
                new Vector2(0.48f, 0.92f));
            amount = CreateText(
                "Amount",
                root.transform,
                "0",
                13,
                new Vector2(0.45f, 0f),
                Vector2.one);
            amount.alignment = TextAnchor.MiddleLeft;
            return root;
        }

        private static GameObject CreateShopPage(
            Transform parent,
            LobbyPagePager pagePager)
        {
            GameObject root = CreateFlowRoot("ShopPageRoot", parent);
            root.transform.SetSiblingIndex(1);
            Image background = CreatePanel(
                "ShopBackground",
                root.transform,
                Vector2.zero,
                Vector2.one,
                FromHex(0x07152A)).GetComponent<Image>();
            background.raycastTarget = false;
            Image glow = CreatePanel(
                "ShopHeaderGlow",
                root.transform,
                new Vector2(0f, 0.72f),
                Vector2.one,
                new Color(0.05f, 0.45f, 0.72f, 0.34f))
                .GetComponent<Image>();
            glow.raycastTarget = false;

            Text title = CreateText(
                "ShopTitleText",
                root.transform,
                "NEON DEPOT",
                34,
                new Vector2(0.14f, 0.78f),
                new Vector2(0.86f, 0.85f));
            title.color = FromHex(0xE8FCFF);
            Text status = CreateText(
                "ShopStatusText",
                root.transform,
                "CATALOG PREVIEW  •  PURCHASES NOT CONNECTED",
                13,
                new Vector2(0.10f, 0.735f),
                new Vector2(0.90f, 0.78f));
            status.color = FromHex(0x7DDAFF);

            GameObject scrollObject = new GameObject(
                "ShopScrollView",
                typeof(RectTransform),
                typeof(ScrollRect));
            scrollObject.transform.SetParent(root.transform, false);
            SetAnchors(
                scrollObject.GetComponent<RectTransform>(),
                new Vector2(0.035f, 0.125f),
                new Vector2(0.965f, 0.73f));

            GameObject viewportObject = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(RectMask2D));
            viewportObject.transform.SetParent(scrollObject.transform, false);
            Stretch(viewportObject.GetComponent<RectTransform>());
            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

            GameObject contentObject = new GameObject(
                "Content",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            VerticalLayoutGroup layout = contentObject
                .GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 30);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            ContentSizeFitter fitter = contentObject
                .GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewportObject.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.08f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.scrollSensitivity = 42f;
            LobbyPageSwipeForwarder forwarder =
                scrollObject.AddComponent<LobbyPageSwipeForwarder>();
            forwarder.Configure(pagePager);
            ShopPageView shopPageView = root.AddComponent<ShopPageView>();
            shopPageView.Configure(scroll, contentRect);

            IReadOnlyList<ShopProductCardModel> cards =
                ShopCatalogPresentation.CreateCards();
            ShopProductSection? currentSection = null;
            for (int index = 0; index < cards.Count; index++)
            {
                ShopProductCardModel card = cards[index];
                if (currentSection != card.Section)
                {
                    currentSection = card.Section;
                    CreateShopSectionHeader(contentObject.transform, card.Section);
                }
                CreateShopProductCard(contentObject.transform, card, index);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            return root;
        }

        private static void CreateShopSectionHeader(
            Transform parent,
            ShopProductSection section)
        {
            GameObject header = CreatePanel(
                $"ShopSection_{section}",
                parent,
                Vector2.zero,
                Vector2.one,
                section == ShopProductSection.Featured
                    ? FromHex(0xFF8A2A)
                    : FromHex(0x7C3AED));
            LayoutElement element = header.AddComponent<LayoutElement>();
            element.preferredHeight = 72f;
            Text label = CreateText(
                "Label",
                header.transform,
                section == ShopProductSection.Featured
                    ? "FEATURED CIRCUITS"
                    : "COIN VAULT",
                26,
                Vector2.zero,
                Vector2.one);
            label.color = Color.white;
        }

        private static void CreateShopProductCard(
            Transform parent,
            ShopProductCardModel card,
            int index)
        {
            GameObject root = CreatePanel(
                $"ShopProductCard_{index + 1}_{card.ProductId}",
                parent,
                Vector2.zero,
                Vector2.one,
                Color.white);
            LayoutElement element = root.AddComponent<LayoutElement>();
            element.preferredHeight = card.Section == ShopProductSection.Featured
                ? 280f
                : 220f;

            Color accent = card.Section == ShopProductSection.Featured
                ? FromHex(0xFF7A1A)
                : FromHex(0x22D3EE);
            Image accentBar = CreatePanel(
                "AccentBar",
                root.transform,
                new Vector2(0f, 0f),
                new Vector2(0.025f, 1f),
                accent).GetComponent<Image>();
            accentBar.raycastTarget = false;

            Image iconBack = CreatePanel(
                "RewardIconBack",
                root.transform,
                new Vector2(0.025f, 0.08f),
                new Vector2(0.34f, 0.92f),
                new Color(accent.r, accent.g, accent.b, 0.22f))
                .GetComponent<Image>();
            iconBack.raycastTarget = false;
            var heroObject = new GameObject(
                "PackageHeroIcon",
                typeof(RectTransform),
                typeof(Image));
            heroObject.transform.SetParent(root.transform, false);
            SetAnchors(
                heroObject.GetComponent<RectTransform>(),
                new Vector2(0.035f, 0.10f),
                new Vector2(0.33f, 0.90f));
            Image hero = heroObject.GetComponent<Image>();
            hero.sprite = Theme01UiSkinBuilder.LoadShopHeroIcon(
                card.HeroIconName);
            hero.preserveAspect = true;
            hero.raycastTarget = false;

            Text badge = CreateText(
                "BadgeText",
                root.transform,
                card.Badge,
                13,
                new Vector2(0.36f, 0.71f),
                new Vector2(0.59f, 0.90f));
            badge.alignment = TextAnchor.MiddleLeft;
            badge.color = accent;
            Text title = CreateText(
                "TitleText",
                root.transform,
                card.Title,
                25,
                new Vector2(0.36f, 0.45f),
                new Vector2(0.94f, 0.72f));
            title.alignment = TextAnchor.MiddleLeft;
            CreateShopRewardChips(root.transform, card.RewardItems);

            Button action = CreateButton(
                "ShopUnavailablePurchaseButton",
                root.transform,
                card.ActionLabel,
                new Vector2(0.73f, 0.10f),
                new Vector2(0.96f, 0.39f),
                FromHex(0x334155),
                out Text actionLabel);
            action.interactable = false;
            actionLabel.fontSize = 13;
        }

        private static void CreateShopRewardChips(
            Transform parent,
            IReadOnlyList<ShopRewardItemModel> rewardItems)
        {
            int count = rewardItems.Count;
            float areaMin = 0.36f;
            float areaMax = 0.71f;
            float width = (areaMax - areaMin) / Mathf.Max(1, count);
            for (int index = 0; index < count; index++)
            {
                ShopRewardItemModel reward = rewardItems[index];
                float left = areaMin + (index * width);
                float right = left + width;
                GameObject chip = CreateFlowRoot(
                    $"RewardChip_{reward.IconName}",
                    parent);
                SetAnchors(
                    chip.GetComponent<RectTransform>(),
                    new Vector2(left, 0.08f),
                    new Vector2(right, 0.43f));
                Theme01UiSkinBuilder.AddStandaloneIcon(
                    "Icon",
                    chip.transform,
                    reward.IconName,
                    new Vector2(0.08f, 0.34f),
                    new Vector2(0.92f, 0.98f));
                Text amount = CreateText(
                    "Amount",
                    chip.transform,
                    reward.Amount,
                    12,
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0.38f));
                amount.color = FromHex(0xE8FCFF);
            }
        }

        private static void CreateLobbyNavigation(
            Transform parent,
            out Button shop,
            out Button leaderboard,
            out Button home,
            out Button journey,
            out Button collection,
            out GameObject shopSelection,
            out GameObject leaderboardSelection,
            out GameObject homeSelection,
            out GameObject journeySelection,
            out GameObject collectionSelection)
        {
            GameObject bar = CreatePanel(
                "LobbyBottomNavigationBar",
                parent,
                new Vector2(0f, 0f),
                new Vector2(1f, 0.125f),
                FromHex(0x172554));
            Image barImage = bar.GetComponent<Image>();
            barImage.raycastTarget = true;

            shop = CreateNavigationButton(
                "ShopNavigationButton", bar.transform, "SHOP", 0,
                out shopSelection);
            leaderboard = CreateNavigationButton(
                "LeaderboardNavigationButton", bar.transform, "RANK", 1,
                out leaderboardSelection);
            home = CreateNavigationButton(
                "HomeNavigationButton", bar.transform, "HOME", 2,
                out homeSelection);
            journey = CreateNavigationButton(
                "JourneyNavigationButton", bar.transform, "JOURNEY", 3,
                out journeySelection);
            collection = CreateNavigationButton(
                "CollectionNavigationButton", bar.transform, "COLLECTION", 4,
                out collectionSelection);

            leaderboardSelection.SetActive(false);
            journeySelection.SetActive(false);
            collectionSelection.SetActive(false);
        }

        private static Button CreateNavigationButton(
            string name,
            Transform parent,
            string label,
            int index,
            out GameObject selection)
        {
            float left = index * 0.2f + 0.012f;
            float right = (index + 1) * 0.2f - 0.012f;
            selection = CreatePanel(
                name.Replace("Button", "Selection"),
                parent,
                new Vector2(left, 0.025f),
                new Vector2(right, 0.975f),
                FromHex(0x0E7490));
            selection.GetComponent<Image>().raycastTarget = false;

            Button button = CreateButton(
                name,
                parent,
                label,
                new Vector2(left, 0.06f),
                new Vector2(right, 0.94f),
                FromHex(0x1E3A8A),
                out Text labelText);
            labelText.fontSize = 11;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 8;
            labelText.resizeTextMaxSize = 11;
            labelText.rectTransform.anchorMin = new Vector2(0f, 0f);
            labelText.rectTransform.anchorMax = new Vector2(1f, 0.42f);
            CreateNavigationGlyph(button.transform, index);
            return button;
        }

        private static void CreateNavigationGlyph(Transform parent, int index)
        {
            Color color = index == 2
                ? FromHex(0xFDE68A)
                : FromHex(0x67E8F9);
            switch (index)
            {
                case 0:
                    CreateGlyphRect(parent, "Roof", 0.25f, 0.57f, 0.75f, 0.70f, color);
                    CreateGlyphRect(parent, "Store", 0.30f, 0.34f, 0.70f, 0.57f, color);
                    break;
                case 1:
                    CreateGlyphRect(parent, "PodiumLeft", 0.27f, 0.35f, 0.39f, 0.54f, color);
                    CreateGlyphRect(parent, "PodiumCenter", 0.43f, 0.35f, 0.57f, 0.72f, color);
                    CreateGlyphRect(parent, "PodiumRight", 0.61f, 0.35f, 0.73f, 0.61f, color);
                    break;
                case 2:
                    CreateGlyphRect(parent, "HomeRoof", 0.29f, 0.57f, 0.71f, 0.69f, color, 45f);
                    CreateGlyphRect(parent, "HomeBody", 0.34f, 0.34f, 0.66f, 0.58f, color);
                    break;
                case 3:
                    CreateGlyphRect(parent, "PathOne", 0.28f, 0.38f, 0.40f, 0.50f, color);
                    CreateGlyphRect(parent, "PathTwo", 0.46f, 0.49f, 0.58f, 0.61f, color);
                    CreateGlyphRect(parent, "PathThree", 0.62f, 0.60f, 0.74f, 0.72f, color);
                    break;
                default:
                    CreateGlyphRect(parent, "CellOne", 0.31f, 0.51f, 0.47f, 0.68f, color);
                    CreateGlyphRect(parent, "CellTwo", 0.53f, 0.51f, 0.69f, 0.68f, color);
                    CreateGlyphRect(parent, "CellThree", 0.31f, 0.31f, 0.47f, 0.48f, color);
                    CreateGlyphRect(parent, "CellFour", 0.53f, 0.31f, 0.69f, 0.48f, color);
                    break;
            }
        }

        private static void CreateGlyphRect(
            Transform parent,
            string name,
            float minX,
            float minY,
            float maxX,
            float maxY,
            Color color,
            float angle = 0f)
        {
            Image image = CreatePanel(
                name,
                parent,
                new Vector2(minX, minY),
                new Vector2(maxX, maxY),
                color).GetComponent<Image>();
            image.raycastTarget = false;
            image.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);
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
            Image image = panel.GetComponent<Image>();
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
            Button button = buttonObject.GetComponent<Button>();
            Theme01UiSkinBuilder.ApplyButton(button, name, labelText);
            return button;
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
