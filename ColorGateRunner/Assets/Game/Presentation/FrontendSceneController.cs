using System;
using ColorGateRunner.Core;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class FrontendSceneController : MonoBehaviour
    {
        [SerializeField] private GameObject titlePageRoot;
        [SerializeField] private GameObject lobbyPageRoot;
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private GameObject loadingRoot;
        [SerializeField] private GameObject transitionBlockerRoot;
        [SerializeField] private GameObject developmentDebugRoot;
        [SerializeField] private Text titleProfileText;
        [SerializeField] private Text titleAccountText;
        [SerializeField] private Text titleVersionText;
        [SerializeField] private Button titleStartButton;
        [SerializeField] private Button titleAccountButton;
        [SerializeField] private Button titleSettingsButton;
        [SerializeField] private GameObject titleLegalRoot;
        [SerializeField] private Text lobbyProfileText;
        [SerializeField] private Text lobbyAccountText;
        [SerializeField] private Text lobbyStageText;
        [SerializeField] private Text lobbyStageTitleText;
        [SerializeField] private Text lobbyStageMechanicText;
        [SerializeField] private Text lobbyProgressText;
        [SerializeField] private Button lobbyPlayButton;
        [SerializeField] private Button lobbyBackButton;
        [SerializeField] private Button lobbySettingsButton;
        [SerializeField] private GameObject currencySlotRoot;
        [SerializeField] private GameObject eventModuleSlotRoot;
        [SerializeField] private GameObject notificationSlotRoot;
        [SerializeField] private GameObject lobbyThemeRoot;
        [SerializeField] private LobbyProgressionPanel lobbyProgressionPanel;
        [SerializeField] private Text modalTitleText;
        [SerializeField] private Text modalMessageText;
        [SerializeField] private Button modalConfirmButton;
        [SerializeField] private Text modalConfirmText;
        [SerializeField] private Button modalCancelButton;
        [SerializeField] private Text modalCancelText;
        [SerializeField] private SettingsPanelController settingsPanel;
        [SerializeField] private string campaignScenePath;

        private FrontendPageRouter _router;
        private FrontendDisplayContext _context;
        private ISceneTransitionLoader _sceneLoader;
        private IFrontendExitHandler _exitHandler;
        private FrontendDisplayContext _injectedContext;
        private FrontendPage _initialPage = FrontendPage.AccountChoice;
        private bool _listenersBound;
        private bool _productReady;
        private AppRoot _appRoot;
        private ICampaignLaunchHost _campaignLaunchHost;
        private CampaignLobbyReadModel _campaignLobby;
        private string _queuedStageId;

        internal FrontendPageRouter Router => _router;
        internal string CampaignScenePath => campaignScenePath;
        internal bool ProductReady => _productReady;
        internal GameObject TitlePageRoot => titlePageRoot;
        internal GameObject LobbyPageRoot => lobbyPageRoot;
        internal GameObject PopupRoot => popupRoot;
        internal GameObject LoadingRoot => loadingRoot;
        internal GameObject TransitionBlockerRoot => transitionBlockerRoot;
        internal GameObject CurrencySlotRoot => currencySlotRoot;
        internal GameObject LobbyThemeRoot => lobbyThemeRoot;
        internal LobbyProgressionPanel LobbyProgressionPanel =>
            lobbyProgressionPanel;
        internal GameObject EventModuleSlotRoot => eventModuleSlotRoot;
        internal Text TitleProfileText => titleProfileText;
        internal Text TitleAccountText => titleAccountText;
        internal Text TitleVersionText => titleVersionText;
        internal Text LobbyProfileText => lobbyProfileText;
        internal Text LobbyStageText => lobbyStageText;
        internal Text LobbyStageTitleText => lobbyStageTitleText;
        internal Text LobbyStageMechanicText => lobbyStageMechanicText;
        internal Text LobbyProgressText => lobbyProgressText;
        internal Text ModalMessageText => modalMessageText;
        internal GameObject TitleLegalRoot => titleLegalRoot;
        internal Button TitleStartButton => titleStartButton;
        internal Button TitleAccountButton => titleAccountButton;
        internal Button LobbyPlayButton => lobbyPlayButton;
        internal Button LobbyBackButton => lobbyBackButton;
        internal Button LobbySettingsButton => lobbySettingsButton;
        internal SettingsPanelController SettingsPanel => settingsPanel;
        internal Button ModalConfirmButton => modalConfirmButton;
        internal Button ModalCancelButton => modalCancelButton;

        private void Awake()
        {
            ValidateRequiredReferences();
            _sceneLoader ??= new UnitySceneTransitionLoader();
            _exitHandler ??= new UnityFrontendExitHandler();
            BindProductContext();
            _router ??= new FrontendPageRouter(_initialPage);
            BindListeners();
            ApplyPermanentVisibilityPolicy();
            ApplyPage(_router.CurrentPage);
            ApplyTransition(_router.IsTransitioning);
            ApplyModal(_router.CurrentModal);
            if (!_productReady)
            {
                _router.TryShowModal(FrontendModal.BootRequired);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleBack();
            }
        }

        private void OnDestroy()
        {
            UnbindListeners();
        }

        public void HandleBack()
        {
            _router.HandleBack();
        }

        internal void Configure(
            GameObject titleRoot,
            GameObject lobbyRoot,
            GameObject popup,
            GameObject loading,
            GameObject transitionBlocker,
            GameObject debugRoot,
            Text titleProfile,
            Text titleAccount,
            Text titleVersion,
            Button titleStart,
            Button titleAccountAction,
            Button titleSettings,
            GameObject legalRoot,
            Text lobbyProfile,
            Text lobbyAccount,
            Text lobbyStage,
            Text lobbyStageTitle,
            Text lobbyStageMechanic,
            Text lobbyProgress,
            Button lobbyPlay,
            Button lobbyBack,
            Button lobbySettings,
            GameObject currencySlot,
            GameObject eventSlot,
            GameObject notificationSlot,
            GameObject themeRoot,
            LobbyProgressionPanel progressionPanel,
            Text modalTitle,
            Text modalMessage,
            Button modalConfirm,
            Text modalConfirmLabel,
            Button modalCancel,
            Text modalCancelLabel,
            SettingsPanelController sharedSettingsPanel,
            string campaignPath)
        {
            titlePageRoot = titleRoot;
            lobbyPageRoot = lobbyRoot;
            popupRoot = popup;
            loadingRoot = loading;
            transitionBlockerRoot = transitionBlocker;
            developmentDebugRoot = debugRoot;
            titleProfileText = titleProfile;
            titleAccountText = titleAccount;
            titleVersionText = titleVersion;
            titleStartButton = titleStart;
            titleAccountButton = titleAccountAction;
            titleSettingsButton = titleSettings;
            titleLegalRoot = legalRoot;
            lobbyProfileText = lobbyProfile;
            lobbyAccountText = lobbyAccount;
            lobbyStageText = lobbyStage;
            lobbyStageTitleText = lobbyStageTitle;
            lobbyStageMechanicText = lobbyStageMechanic;
            lobbyProgressText = lobbyProgress;
            lobbyPlayButton = lobbyPlay;
            lobbyBackButton = lobbyBack;
            lobbySettingsButton = lobbySettings;
            currencySlotRoot = currencySlot;
            eventModuleSlotRoot = eventSlot;
            notificationSlotRoot = notificationSlot;
            lobbyThemeRoot = themeRoot;
            lobbyProgressionPanel = progressionPanel;
            modalTitleText = modalTitle;
            modalMessageText = modalMessage;
            modalConfirmButton = modalConfirm;
            modalConfirmText = modalConfirmLabel;
            modalCancelButton = modalCancel;
            modalCancelText = modalCancelLabel;
            settingsPanel = sharedSettingsPanel;
            campaignScenePath = campaignPath;
        }

        internal void SetDependenciesForTests(
            FrontendDisplayContext context,
            ISceneTransitionLoader sceneLoader,
            IFrontendExitHandler exitHandler,
            FrontendPage initialPage = FrontendPage.AccountChoice)
        {
            _injectedContext = context;
            _sceneLoader = sceneLoader;
            _exitHandler = exitHandler;
            _initialPage = initialPage;
        }

        internal void SetSceneLoaderForTests(ISceneTransitionLoader sceneLoader)
        {
            _sceneLoader = sceneLoader ??
                throw new ArgumentNullException(nameof(sceneLoader));
        }

        internal void SetExitHandlerForTests(IFrontendExitHandler exitHandler)
        {
            _exitHandler = exitHandler ??
                throw new ArgumentNullException(nameof(exitHandler));
        }

        internal bool HasRequiredReferences()
        {
            return titlePageRoot != null && lobbyPageRoot != null &&
                popupRoot != null && loadingRoot != null &&
                transitionBlockerRoot != null &&
                developmentDebugRoot != null &&
                titleProfileText != null && titleAccountText != null &&
                titleVersionText != null && titleStartButton != null &&
                titleAccountButton != null && titleSettingsButton != null &&
                titleLegalRoot != null && lobbyProfileText != null &&
                lobbyAccountText != null && lobbyStageText != null &&
                lobbyStageTitleText != null &&
                lobbyStageMechanicText != null &&
                lobbyProgressText != null && lobbyPlayButton != null &&
                lobbyBackButton != null && lobbySettingsButton != null &&
                currencySlotRoot != null && eventModuleSlotRoot != null &&
                notificationSlotRoot != null && lobbyThemeRoot != null &&
                lobbyProgressionPanel != null &&
                lobbyProgressionPanel.HasRequiredReferences() &&
                modalTitleText != null && modalMessageText != null &&
                modalConfirmButton != null && modalConfirmText != null &&
                modalCancelButton != null && modalCancelText != null &&
                settingsPanel != null && settingsPanel.HasRequiredReferences() &&
                !string.IsNullOrWhiteSpace(campaignScenePath) &&
                campaignScenePath != gameObject.scene.path;
        }

        internal int ActivePrimaryPageCount()
        {
            int count = titlePageRoot.activeSelf ? 1 : 0;
            count += lobbyPageRoot.activeSelf ? 1 : 0;
            return count;
        }

        private void ValidateRequiredReferences()
        {
            if (!HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Frontend Scene references are incomplete.");
            }
        }

        private void BindProductContext()
        {
            if (_injectedContext != null)
            {
                ApplyContext(_injectedContext);
                return;
            }

            if (!AppRoot.TryGetActive(out AppRoot appRoot) ||
                !appRoot.IsPrimary ||
                !FrontendDisplayContext.TryCreate(
                    appRoot.Graph,
                    Application.version,
                    out FrontendDisplayContext context,
                    out _))
            {
                _productReady = false;
                lobbyPlayButton.interactable = false;
                _initialPage = FrontendPage.AccountChoice;
                return;
            }

            _appRoot = appRoot;
            _campaignLaunchHost = appRoot;
            try
            {
                StageCatalogProvider.EnsureConfigured();
                _campaignLobby = new FrontendCampaignProgressReader(
                    new ProductStageProgressStore(
                        appRoot.Graph.ProductSession,
                        appRoot.Graph.Progression,
                        StageCatalog.Current),
                    StageCatalog.Current).Read();
            }
            catch (Exception exception)
            {
                _productReady = false;
                lobbyPlayButton.interactable = false;
                lobbyStageText.text = "CAMPAIGN UNAVAILABLE";
                lobbyStageTitleText.text = exception.Message;
                lobbyStageMechanicText.text = string.Empty;
                lobbyProgressText.text = string.Empty;
                return;
            }
            ApplyContext(context);
            int pendingMilestones =
                lobbyProgressionPanel.Bind(appRoot.Graph.Progression);
            if (pendingMilestones > 0)
            {
                ProductMutationResult acknowledgement =
                    appRoot.AcknowledgeLobbyMilestones();
                if (!acknowledgement.Succeeded)
                {
                    Debug.LogError(acknowledgement.Error.Diagnostic);
                }
            }
        }

        private void ApplyContext(FrontendDisplayContext context)
        {
            _context = context;
            _productReady = true;
            _initialPage = context.AccountChoiceCompleted
                ? FrontendPage.Lobby
                : FrontendPage.AccountChoice;
            titleProfileText.text = context.DisplayName;
            titleAccountText.text = context.AccountLabel;
            titleVersionText.text = context.VersionLabel;
            lobbyProfileText.text = context.DisplayName;
            lobbyAccountText.text = context.AccountLabel;
            if (_campaignLobby != null)
            {
                lobbyStageText.text =
                    $"STAGE {_campaignLobby.DisplayNumber}";
                lobbyStageTitleText.text = _campaignLobby.Title;
                lobbyStageMechanicText.text =
                    _campaignLobby.MechanicLabel;
                lobbyProgressText.text =
                    $"{_campaignLobby.ClearedCount} / " +
                    $"{_campaignLobby.TotalStageCount} CLEARED";
            }
            titleSettingsButton.gameObject.SetActive(false);
            titleAccountButton.gameObject.SetActive(
                context.GoogleProviderAvailable);
            lobbySettingsButton.gameObject.SetActive(context.SettingsAvailable);
            lobbyPlayButton.interactable = true;
        }

        private void ApplyPermanentVisibilityPolicy()
        {
            titleLegalRoot.SetActive(false);
            currencySlotRoot.SetActive(_productReady);
            eventModuleSlotRoot.SetActive(false);
            notificationSlotRoot.SetActive(false);
            lobbyThemeRoot.SetActive(_productReady);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            developmentDebugRoot.SetActive(true);
#else
            developmentDebugRoot.SetActive(false);
#endif
            loadingRoot.SetActive(false);
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            titleStartButton.onClick.AddListener(ChooseGuest);
            titleAccountButton.onClick.AddListener(ShowAccountUnavailable);
            titleSettingsButton.onClick.AddListener(ShowSettings);
            lobbyPlayButton.onClick.AddListener(PlayCampaign);
            lobbyBackButton.onClick.AddListener(ShowAccountUnavailable);
            lobbySettingsButton.onClick.AddListener(ShowSettings);
            modalConfirmButton.onClick.AddListener(ConfirmModal);
            modalCancelButton.onClick.AddListener(CancelModal);
            settingsPanel.ApplySucceeded += CloseSettings;
            settingsPanel.CancelRequested += CloseSettings;
            _router.PageEntered += ApplyPage;
            _router.ModalChanged += ApplyModal;
            _router.TransitionChanged += ApplyTransition;
            _listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!_listenersBound)
            {
                return;
            }

            titleStartButton.onClick.RemoveListener(ChooseGuest);
            titleAccountButton.onClick.RemoveListener(ShowAccountUnavailable);
            titleSettingsButton.onClick.RemoveListener(ShowSettings);
            lobbyPlayButton.onClick.RemoveListener(PlayCampaign);
            lobbyBackButton.onClick.RemoveListener(ShowAccountUnavailable);
            lobbySettingsButton.onClick.RemoveListener(ShowSettings);
            modalConfirmButton.onClick.RemoveListener(ConfirmModal);
            modalCancelButton.onClick.RemoveListener(CancelModal);
            settingsPanel.ApplySucceeded -= CloseSettings;
            settingsPanel.CancelRequested -= CloseSettings;
            _router.PageEntered -= ApplyPage;
            _router.ModalChanged -= ApplyModal;
            _router.TransitionChanged -= ApplyTransition;
            _listenersBound = false;
        }

        private void ChooseGuest()
        {
            if (!_productReady || _appRoot == null)
            {
                _router.TryShowModal(FrontendModal.BootRequired);
                return;
            }

            ProductMutationResult result =
                _appRoot.CompleteGuestAccountChoice();
            if (!result.Succeeded)
            {
                modalMessageText.text = string.IsNullOrWhiteSpace(
                    result.Error.Diagnostic)
                    ? "ACCOUNT CHOICE SAVE FAILED"
                    : result.Error.Diagnostic;
                _router.TryShowModal(FrontendModal.SaveError);
                return;
            }

            if (FrontendDisplayContext.TryCreate(
                _appRoot.Graph,
                Application.version,
                out FrontendDisplayContext refreshed,
                out _))
            {
                ApplyContext(refreshed);
            }
            _router.TryShowPage(FrontendPage.Lobby);
        }

        private void ShowAccountUnavailable()
        {
            _router.TryShowModal(FrontendModal.AccountUnavailable);
        }

        private void ShowSettings()
        {
            if (_context != null && _context.SettingsAvailable &&
                _appRoot != null &&
                _router.TryShowModal(FrontendModal.Settings))
            {
                settingsPanel.Open(_appRoot);
            }
        }

        private void CloseSettings()
        {
            if (_router.CurrentModal == FrontendModal.Settings)
            {
                _router.CloseModal();
            }
        }

        private void PlayCampaign()
        {
            if (!_productReady || _campaignLobby == null ||
                _campaignLaunchHost == null)
            {
                _router.TryShowModal(FrontendModal.BootRequired);
                return;
            }
            if (!_router.TryBeginSceneTransition())
            {
                return;
            }

            _queuedStageId = _campaignLobby.StageId;
            if (!_campaignLaunchHost.TryQueueCampaignLaunch(_queuedStageId))
            {
                _queuedStageId = null;
                _router.CompleteSceneTransition();
                modalMessageText.text = "CAMPAIGN LAUNCH IS ALREADY PENDING";
                _router.TryShowModal(FrontendModal.SceneLoadError);
                return;
            }

            loadingRoot.SetActive(true);
            _sceneLoader.LoadScene(campaignScenePath, OnSceneLoadCompleted);
        }

        private void OnSceneLoadCompleted(SceneTransitionResult result)
        {
            if (this == null)
            {
                return;
            }

            loadingRoot.SetActive(false);
            _router.CompleteSceneTransition();
            if (!result.Succeeded)
            {
                _campaignLaunchHost?.TryCancelCampaignLaunch(_queuedStageId);
                _queuedStageId = null;
                modalMessageText.text = string.IsNullOrWhiteSpace(result.Error)
                    ? "CAMPAIGN LOAD FAILED"
                    : result.Error;
                _router.TryShowModal(FrontendModal.SceneLoadError);
            }
        }

        private void ConfirmModal()
        {
            if (_router.CurrentModal == FrontendModal.ExitConfirmation)
            {
                _router.CloseModal();
                _exitHandler.RequestExit();
            }
        }

        private void CancelModal()
        {
            if (_router.IsCurrentModalCancellable())
            {
                _router.CloseModal();
            }
        }

        private void ApplyPage(FrontendPage page)
        {
            titlePageRoot.SetActive(page == FrontendPage.AccountChoice);
            lobbyPageRoot.SetActive(page == FrontendPage.Lobby);
        }

        private void ApplyTransition(bool transitioning)
        {
            transitionBlockerRoot.SetActive(transitioning);
        }

        private void ApplyModal(FrontendModal modal)
        {
            popupRoot.SetActive(modal != FrontendModal.None);
            settingsPanel.gameObject.SetActive(modal == FrontendModal.Settings);
            if (modal == FrontendModal.None)
            {
                return;
            }

            bool settingsVisible = modal == FrontendModal.Settings;
            modalTitleText.gameObject.SetActive(!settingsVisible);
            modalMessageText.gameObject.SetActive(!settingsVisible);
            bool confirmVisible = false;
            bool cancelVisible = !settingsVisible;
            string title;
            string message;
            string cancelLabel = "CLOSE";
            switch (modal)
            {
                case FrontendModal.AccountUnavailable:
                    title = "GUEST PROFILE";
                    message = "ACCOUNT LINKING IS NOT AVAILABLE YET";
                    break;
                case FrontendModal.Settings:
                    title = "SETTINGS";
                    message = string.Empty;
                    break;
                case FrontendModal.ExitConfirmation:
                    title = "EXIT GAME?";
                    message = "RETURN TO YOUR DEVICE HOME SCREEN?";
                    confirmVisible = true;
                    cancelLabel = "CANCEL";
                    break;
                case FrontendModal.BootRequired:
                    title = "BOOT REQUIRED";
                    message = "START FROM THE BOOT SCENE TO LOAD LOCAL DATA";
                    cancelVisible = false;
                    break;
                case FrontendModal.SceneLoadError:
                    title = "CAMPAIGN UNAVAILABLE";
                    message = string.IsNullOrWhiteSpace(modalMessageText.text)
                        ? "CAMPAIGN LOAD FAILED"
                        : modalMessageText.text;
                    break;
                case FrontendModal.SaveError:
                    title = "SAVE FAILED";
                    message = string.IsNullOrWhiteSpace(modalMessageText.text)
                        ? "ACCOUNT CHOICE SAVE FAILED"
                        : modalMessageText.text;
                    break;
                default:
                    title = "NOTICE";
                    message = string.Empty;
                    break;
            }

            modalTitleText.text = title;
            modalMessageText.text = message;
            modalConfirmButton.gameObject.SetActive(confirmVisible);
            modalConfirmText.text = "EXIT";
            modalCancelButton.gameObject.SetActive(cancelVisible);
            modalCancelText.text = cancelLabel;
        }
    }

    internal interface IFrontendExitHandler
    {
        void RequestExit();
    }

    internal sealed class UnityFrontendExitHandler : IFrontendExitHandler
    {
        public void RequestExit()
        {
            Application.Quit();
        }
    }
}
