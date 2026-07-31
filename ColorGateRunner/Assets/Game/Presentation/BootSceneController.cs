using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class BootSceneController : MonoBehaviour
    {
        [SerializeField] private AppRoot appRoot;
        [SerializeField] private GameObject loadingRoot;
        [SerializeField] private Text loadingText;
        [SerializeField] private Text versionText;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private Text errorText;
        [SerializeField] private Button retryButton;
        [SerializeField] private string destinationScenePath;

        private IBootSceneLoader _sceneLoader;
        private bool _initializing;

        internal string DestinationScenePath => destinationScenePath;
        internal GameObject ErrorPanel => errorPanel;
        internal Button RetryButton => retryButton;

        private void Awake()
        {
            _sceneLoader ??= new UnityBootSceneLoader();
            if (versionText != null)
            {
                versionText.text = "v" + Application.version;
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(Retry);
            }
        }

        private void Start()
        {
            InitializeAndContinue();
        }

        private void OnDestroy()
        {
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(Retry);
            }
        }

        public void Retry()
        {
            InitializeAndContinue();
        }

        internal void Configure(
            AppRoot root,
            GameObject loading,
            Text loadingLabel,
            Text versionLabel,
            GameObject error,
            Text errorLabel,
            Button retry,
            string destinationPath)
        {
            appRoot = root;
            loadingRoot = loading;
            loadingText = loadingLabel;
            versionText = versionLabel;
            errorPanel = error;
            errorText = errorLabel;
            retryButton = retry;
            destinationScenePath = destinationPath;
        }

        internal void SetSceneLoaderForTests(IBootSceneLoader loader)
        {
            _sceneLoader = loader;
        }

        internal void InitializeForTests()
        {
            InitializeAndContinue();
        }

        private void InitializeAndContinue()
        {
            if (_initializing)
            {
                return;
            }

            _initializing = true;
            SetLoadingState();
            AppRoot resolvedRoot = ResolveAppRoot();
            if (resolvedRoot == null)
            {
                ShowError("APP ROOT UNAVAILABLE");
                return;
            }

            AppInitializationResult result = resolvedRoot.Initialize();
            if (!result.Succeeded)
            {
                ShowError(result.Error.Diagnostic);
                return;
            }

            if (string.IsNullOrWhiteSpace(destinationScenePath) ||
                destinationScenePath == gameObject.scene.path)
            {
                ShowError("BOOT DESTINATION INVALID");
                return;
            }

            _sceneLoader.LoadScene(destinationScenePath);
            _initializing = false;
        }

        private AppRoot ResolveAppRoot()
        {
            if (appRoot != null && appRoot.IsPrimary)
            {
                return appRoot;
            }

            return AppRoot.TryGetActive(out AppRoot active) ? active : null;
        }

        private void SetLoadingState()
        {
            if (loadingRoot != null)
            {
                loadingRoot.SetActive(true);
            }
            if (loadingText != null)
            {
                loadingText.text = "LOADING";
            }
            if (errorPanel != null)
            {
                errorPanel.SetActive(false);
            }
        }

        private void ShowError(string diagnostic)
        {
            if (loadingRoot != null)
            {
                loadingRoot.SetActive(false);
            }
            if (errorText != null)
            {
                errorText.text = string.IsNullOrWhiteSpace(diagnostic)
                    ? "LOCAL INITIALIZATION FAILED"
                    : diagnostic;
            }
            if (errorPanel != null)
            {
                errorPanel.SetActive(true);
            }

            _initializing = false;
        }
    }

    internal interface IBootSceneLoader
    {
        void LoadScene(string scenePath);
    }

    internal sealed class UnityBootSceneLoader : IBootSceneLoader
    {
        public void LoadScene(string scenePath)
        {
            SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
        }
    }
}
