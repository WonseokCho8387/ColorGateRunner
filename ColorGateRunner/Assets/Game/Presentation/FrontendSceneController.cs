using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
        [SerializeField] private Button lobbyPlayButton;
        [SerializeField] private Button lobbyBackButton;
        [SerializeField] private Button lobbySettingsButton;
        [SerializeField] private GameObject currencySlotRoot;
        [SerializeField] private GameObject eventModuleSlotRoot;
        [SerializeField] private GameObject notificationSlotRoot;
        [SerializeField] private GameObject lobbyThemeRoot;
        [SerializeField] private Text modalTitleText;
        [SerializeField] private Text modalMessageText;
        [SerializeField] private Button modalConfirmButton;
        [SerializeField] private Text modalConfirmText;
        [SerializeField] private Button modalCancelButton;
        [SerializeField] private Text modalCancelText;
        [SerializeField] private string campaignScenePath;

        private FrontendPageRouter _router;
        private FrontendDisplayContext _context;
        private IFrontendSceneLoader _sceneLoader;
        private IFrontendExitHandler _exitHandler;
        private FrontendDisplayContext _injectedContext;
        private FrontendPage _initialPage = FrontendPage.Title;
        private bool _listenersBound;
        private bool _productReady;

        internal FrontendPageRouter Router => _router;
        internal string CampaignScenePath => campaignScenePath;
        internal bool ProductReady => _productReady;
        internal GameObject TitlePageRoot => titlePageRoot;
        internal GameObject LobbyPageRoot => lobbyPageRoot;
        internal GameObject PopupRoot => popupRoot;
        internal GameObject LoadingRoot => loadingRoot;
        internal GameObject TransitionBlockerRoot => transitionBlockerRoot;
        internal GameObject CurrencySlotRoot => currencySlotRoot;
        internal GameObject EventModuleSlotRoot => eventModuleSlotRoot;
        internal Text TitleProfileText => titleProfileText;
        internal Text TitleAccountText => titleAccountText;
        internal Text TitleVersionText => titleVersionText;
        internal Text LobbyProfileText => lobbyProfileText;
        internal Text ModalMessageText => modalMessageText;
        internal GameObject TitleLegalRoot => titleLegalRoot;
        internal Button TitleStartButton => titleStartButton;
        internal Button TitleAccountButton => titleAccountButton;
        internal Button LobbyPlayButton => lobbyPlayButton;
        internal Button LobbyBackButton => lobbyBackButton;
        internal Button ModalConfirmButton => modalConfirmButton;
        internal Button ModalCancelButton => modalCancelButton;

        private void Awake()
        {
            ValidateRequiredReferences();
            _sceneLoader ??= new UnityFrontendSceneLoader();
            _exitHandler ??= new UnityFrontendExitHandler();
            _router ??= new FrontendPageRouter(_initialPage);
            BindListeners();
            ApplyPermanentVisibilityPolicy();
            ApplyPage(_router.CurrentPage);
            ApplyTransition(_router.IsTransitioning);
            ApplyModal(_router.CurrentModal);
            BindProductContext();
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
            Button lobbyPlay,
            Button lobbyBack,
            Button lobbySettings,
            GameObject currencySlot,
            GameObject eventSlot,
            GameObject notificationSlot,
            GameObject themeRoot,
            Text modalTitle,
            Text modalMessage,
            Button modalConfirm,
            Text modalConfirmLabel,
            Button modalCancel,
            Text modalCancelLabel,
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
            lobbyPlayButton = lobbyPlay;
            lobbyBackButton = lobbyBack;
            lobbySettingsButton = lobbySettings;
            currencySlotRoot = currencySlot;
            eventModuleSlotRoot = eventSlot;
            notificationSlotRoot = notificationSlot;
            lobbyThemeRoot = themeRoot;
            modalTitleText = modalTitle;
            modalMessageText = modalMessage;
            modalConfirmButton = modalConfirm;
            modalConfirmText = modalConfirmLabel;
            modalCancelButton = modalCancel;
            modalCancelText = modalCancelLabel;
            campaignScenePath = campaignPath;
        }

        internal void SetDependenciesForTests(
            FrontendDisplayContext context,
            IFrontendSceneLoader sceneLoader,
            IFrontendExitHandler exitHandler,
            FrontendPage initialPage = FrontendPage.Title)
        {
            _injectedContext = context;
            _sceneLoader = sceneLoader;
            _exitHandler = exitHandler;
            _initialPage = initialPage;
        }

        internal void SetSceneLoaderForTests(IFrontendSceneLoader sceneLoader)
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
                lobbyAccountText != null && lobbyPlayButton != null &&
                lobbyBackButton != null && lobbySettingsButton != null &&
                currencySlotRoot != null && eventModuleSlotRoot != null &&
                notificationSlotRoot != null && lobbyThemeRoot != null &&
                modalTitleText != null && modalMessageText != null &&
                modalConfirmButton != null && modalConfirmText != null &&
                modalCancelButton != null && modalCancelText != null &&
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
                _router.TryShowModal(FrontendModal.BootRequired);
                return;
            }

            ApplyContext(context);
        }

        private void ApplyContext(FrontendDisplayContext context)
        {
            _context = context;
            _productReady = true;
            titleProfileText.text = context.DisplayName;
            titleAccountText.text = context.AccountLabel;
            titleVersionText.text = context.VersionLabel;
            lobbyProfileText.text = context.DisplayName;
            lobbyAccountText.text = context.AccountLabel;
            titleSettingsButton.gameObject.SetActive(context.SettingsAvailable);
            lobbySettingsButton.gameObject.SetActive(context.SettingsAvailable);
            lobbyPlayButton.interactable = true;
        }

        private void ApplyPermanentVisibilityPolicy()
        {
            titleLegalRoot.SetActive(false);
            currencySlotRoot.SetActive(false);
            eventModuleSlotRoot.SetActive(false);
            notificationSlotRoot.SetActive(false);
            lobbyThemeRoot.SetActive(false);
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

            titleStartButton.onClick.AddListener(ShowLobby);
            titleAccountButton.onClick.AddListener(ShowAccountUnavailable);
            titleSettingsButton.onClick.AddListener(ShowSettings);
            lobbyPlayButton.onClick.AddListener(PlayCampaign);
            lobbyBackButton.onClick.AddListener(ShowTitle);
            lobbySettingsButton.onClick.AddListener(ShowSettings);
            modalConfirmButton.onClick.AddListener(ConfirmModal);
            modalCancelButton.onClick.AddListener(CancelModal);
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

            titleStartButton.onClick.RemoveListener(ShowLobby);
            titleAccountButton.onClick.RemoveListener(ShowAccountUnavailable);
            titleSettingsButton.onClick.RemoveListener(ShowSettings);
            lobbyPlayButton.onClick.RemoveListener(PlayCampaign);
            lobbyBackButton.onClick.RemoveListener(ShowTitle);
            lobbySettingsButton.onClick.RemoveListener(ShowSettings);
            modalConfirmButton.onClick.RemoveListener(ConfirmModal);
            modalCancelButton.onClick.RemoveListener(CancelModal);
            _router.PageEntered -= ApplyPage;
            _router.ModalChanged -= ApplyModal;
            _router.TransitionChanged -= ApplyTransition;
            _listenersBound = false;
        }

        private void ShowLobby()
        {
            _router.TryShowPage(FrontendPage.Lobby);
        }

        private void ShowTitle()
        {
            _router.TryShowPage(FrontendPage.Title);
        }

        private void ShowAccountUnavailable()
        {
            _router.TryShowModal(FrontendModal.AccountUnavailable);
        }

        private void ShowSettings()
        {
            if (_context != null && _context.SettingsAvailable)
            {
                _router.TryShowModal(FrontendModal.Settings);
            }
        }

        private void PlayCampaign()
        {
            if (!_productReady)
            {
                _router.TryShowModal(FrontendModal.BootRequired);
                return;
            }
            if (!_router.TryBeginSceneTransition())
            {
                return;
            }

            loadingRoot.SetActive(true);
            _sceneLoader.LoadScene(campaignScenePath, OnSceneLoadCompleted);
        }

        private void OnSceneLoadCompleted(FrontendSceneLoadResult result)
        {
            if (this == null)
            {
                return;
            }

            loadingRoot.SetActive(false);
            _router.CompleteSceneTransition();
            if (!result.Succeeded)
            {
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
            titlePageRoot.SetActive(page == FrontendPage.Title);
            lobbyPageRoot.SetActive(page == FrontendPage.Lobby);
        }

        private void ApplyTransition(bool transitioning)
        {
            transitionBlockerRoot.SetActive(transitioning);
        }

        private void ApplyModal(FrontendModal modal)
        {
            popupRoot.SetActive(modal != FrontendModal.None);
            if (modal == FrontendModal.None)
            {
                return;
            }

            bool confirmVisible = false;
            bool cancelVisible = true;
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
                    message = _context?.SettingsSummary ??
                        "SETTINGS UNAVAILABLE";
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

    internal readonly struct FrontendSceneLoadResult
    {
        private FrontendSceneLoadResult(bool succeeded, string error)
        {
            Succeeded = succeeded;
            Error = error;
        }

        public bool Succeeded { get; }
        public string Error { get; }

        public static FrontendSceneLoadResult Success() =>
            new FrontendSceneLoadResult(true, string.Empty);

        public static FrontendSceneLoadResult Failure(string error) =>
            new FrontendSceneLoadResult(false, error);
    }

    internal interface IFrontendSceneLoader
    {
        void LoadScene(
            string scenePath,
            Action<FrontendSceneLoadResult> completed);
    }

    internal sealed class UnityFrontendSceneLoader : IFrontendSceneLoader
    {
        public void LoadScene(
            string scenePath,
            Action<FrontendSceneLoadResult> completed)
        {
            try
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(
                    scenePath,
                    LoadSceneMode.Single);
                if (operation == null)
                {
                    completed?.Invoke(FrontendSceneLoadResult.Failure(
                        "CAMPAIGN LOAD COULD NOT START"));
                    return;
                }

                operation.completed += _ =>
                    completed?.Invoke(FrontendSceneLoadResult.Success());
            }
            catch (Exception exception)
            {
                completed?.Invoke(FrontendSceneLoadResult.Failure(
                    "CAMPAIGN LOAD FAILED: " + exception.Message));
            }
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
