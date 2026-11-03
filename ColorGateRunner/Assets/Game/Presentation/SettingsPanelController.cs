using System;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Toggle notificationToggle;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Text notificationStateText;
        [SerializeField] private Text musicStateText;
        [SerializeField] private Text sfxStateText;
        [SerializeField] private Text vibrationStateText;
        [SerializeField] private Text notificationNoticeText;
        [SerializeField] private Button termsButton;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button supportButton;
        [SerializeField] private Text linkStatusText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ProductLinkConfiguration linkConfiguration;
        [SerializeField] private GameObject audioSection;
        [SerializeField] private GameObject gameplaySection;
        [SerializeField] private GameObject linkRow;
        [SerializeField] private Text versionText;

        private IProductSettingsHost _host;
        private IExternalUrlOpener _urlOpener;
        private bool _listenersBound;
        private SettingsAvailability _availability =
            SettingsAvailability.Current;

        internal event Action ApplySucceeded;
        internal event Action CancelRequested;

        internal Toggle NotificationToggle => notificationToggle;
        internal Toggle MusicToggle => musicToggle;
        internal Toggle SfxToggle => sfxToggle;
        internal Toggle VibrationToggle => vibrationToggle;
        internal Text NotificationStateText => notificationStateText;
        internal Text MusicStateText => musicStateText;
        internal Text SfxStateText => sfxStateText;
        internal Text VibrationStateText => vibrationStateText;
        internal Text NotificationNoticeText => notificationNoticeText;
        internal Button TermsButton => termsButton;
        internal Button PrivacyButton => privacyButton;
        internal Button SupportButton => supportButton;
        internal Text LinkStatusText => linkStatusText;
        internal Text StatusText => statusText;
        internal Button ApplyButton => applyButton;
        internal Button CancelButton => cancelButton;
        internal GameObject AudioSection => audioSection;
        internal GameObject GameplaySection => gameplaySection;
        internal GameObject LinkRow => linkRow;
        internal Text VersionText => versionText;

        private void Awake()
        {
            ValidateRequiredReferences();
            _urlOpener ??= new UnityExternalUrlOpener();
            BindListeners();
        }

        private void OnDestroy()
        {
            UnbindListeners();
        }

        internal void Configure(
            Toggle notifications,
            Toggle music,
            Toggle sfx,
            Toggle vibration,
            Text notificationState,
            Text musicState,
            Text sfxState,
            Text vibrationState,
            Text notificationNotice,
            Button terms,
            Button privacy,
            Button support,
            Text linkStatus,
            Text status,
            Button apply,
            Button cancel,
            ProductLinkConfiguration links)
        {
            notificationToggle = notifications;
            musicToggle = music;
            sfxToggle = sfx;
            vibrationToggle = vibration;
            notificationStateText = notificationState;
            musicStateText = musicState;
            sfxStateText = sfxState;
            vibrationStateText = vibrationState;
            notificationNoticeText = notificationNotice;
            termsButton = terms;
            privacyButton = privacy;
            supportButton = support;
            linkStatusText = linkStatus;
            statusText = status;
            applyButton = apply;
            cancelButton = cancel;
            linkConfiguration = links;
        }

        internal void ConfigureSections(
            GameObject audio,
            GameObject gameplay,
            GameObject links,
            Text version)
        {
            audioSection = audio;
            gameplaySection = gameplay;
            linkRow = links;
            versionText = version;
        }

        internal void SetAvailabilityForTests(SettingsAvailability availability)
        {
            _availability = availability;
        }

        internal void Open(IProductSettingsHost host)
        {
            _host = host;
            ApplyAvailability();
            versionText.text = "VERSION " + Application.version;
            LocalSettingsData settings = host?.CurrentSettings;
            if (settings == null)
            {
                statusText.text = "SETTINGS UNAVAILABLE";
                applyButton.interactable = false;
                return;
            }

            applyButton.interactable = true;
            notificationToggle.SetIsOnWithoutNotify(
                settings.NotificationEnabled);
            musicToggle.SetIsOnWithoutNotify(settings.MusicVolume > 0f);
            sfxToggle.SetIsOnWithoutNotify(settings.SfxVolume > 0f);
            vibrationToggle.SetIsOnWithoutNotify(settings.Vibration);
            RefreshTogglePresentation();
            notificationNoticeText.text =
                "PREFERENCE ONLY - NOTIFICATIONS ARE NOT SENT YET";
            statusText.text = string.Empty;
            RefreshLinkState();
        }

        internal void SetUrlOpenerForTests(IExternalUrlOpener opener)
        {
            _urlOpener = opener ??
                throw new ArgumentNullException(nameof(opener));
        }

        internal bool HasRequiredReferences()
        {
            return notificationToggle != null && musicToggle != null &&
                sfxToggle != null && vibrationToggle != null &&
                notificationStateText != null && musicStateText != null &&
                sfxStateText != null && vibrationStateText != null &&
                notificationNoticeText != null && termsButton != null &&
                privacyButton != null && supportButton != null &&
                linkStatusText != null && statusText != null &&
                applyButton != null && cancelButton != null &&
                linkConfiguration != null && audioSection != null &&
                gameplaySection != null && linkRow != null &&
                versionText != null;
        }

        private void Apply()
        {
            if (_host == null || _host.CurrentSettings == null)
            {
                statusText.text = "SETTINGS UNAVAILABLE";
                return;
            }

            LocalSettingsData current = _host.CurrentSettings;
            float music = musicToggle.isOn
                ? ResolveActiveVolume(
                    current.MusicVolume,
                    current.LastNonZeroMusicVolume)
                : 0f;
            float sfx = sfxToggle.isOn
                ? ResolveActiveVolume(
                    current.SfxVolume,
                    current.LastNonZeroSfxVolume)
                : 0f;
            ProductMutationResult result = _host.ApplySettings(
                current.MasterVolume,
                music,
                sfx,
                vibrationToggle.isOn,
                notificationToggle.isOn);
            if (!result.Succeeded)
            {
                statusText.text = string.IsNullOrWhiteSpace(
                    result.Error.Diagnostic)
                    ? "SETTINGS SAVE FAILED"
                    : "SETTINGS SAVE FAILED\n" + result.Error.Diagnostic;
                return;
            }

            statusText.text = result.Changed ? "SAVED" : "NO CHANGES";
            RefreshTogglePresentation();
            ApplySucceeded?.Invoke();
        }

        private void Cancel()
        {
            CancelRequested?.Invoke();
        }

        private void BindListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            applyButton.onClick.AddListener(Apply);
            cancelButton.onClick.AddListener(Cancel);
            termsButton.onClick.AddListener(OpenTerms);
            privacyButton.onClick.AddListener(OpenPrivacy);
            supportButton.onClick.AddListener(OpenSupport);
            notificationToggle.onValueChanged.AddListener(
                HandleToggleChanged);
            musicToggle.onValueChanged.AddListener(HandleToggleChanged);
            sfxToggle.onValueChanged.AddListener(HandleToggleChanged);
            vibrationToggle.onValueChanged.AddListener(HandleToggleChanged);
            _listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!_listenersBound)
            {
                return;
            }

            applyButton.onClick.RemoveListener(Apply);
            cancelButton.onClick.RemoveListener(Cancel);
            termsButton.onClick.RemoveListener(OpenTerms);
            privacyButton.onClick.RemoveListener(OpenPrivacy);
            supportButton.onClick.RemoveListener(OpenSupport);
            notificationToggle.onValueChanged.RemoveListener(
                HandleToggleChanged);
            musicToggle.onValueChanged.RemoveListener(HandleToggleChanged);
            sfxToggle.onValueChanged.RemoveListener(HandleToggleChanged);
            vibrationToggle.onValueChanged.RemoveListener(
                HandleToggleChanged);
            _listenersBound = false;
        }

        private void ApplyAvailability()
        {
            // Hidden rows keep their loaded values, so Apply preserves the
            // persisted preference without exposing an inert control.
            audioSection.SetActive(_availability.AnyAudio);
            musicToggle.gameObject.SetActive(_availability.Music);
            sfxToggle.gameObject.SetActive(_availability.Sfx);
            gameplaySection.SetActive(_availability.AnyGameplay);
            vibrationToggle.gameObject.SetActive(_availability.Vibration);
            notificationToggle.gameObject.SetActive(
                _availability.Notifications);
            notificationNoticeText.gameObject.SetActive(
                _availability.Notifications);
        }

        private void HandleToggleChanged(bool _)
        {
            RefreshTogglePresentation();
        }

        private void RefreshTogglePresentation()
        {
            SetTogglePresentation(notificationToggle, notificationStateText);
            SetTogglePresentation(musicToggle, musicStateText);
            SetTogglePresentation(sfxToggle, sfxStateText);
            SetTogglePresentation(vibrationToggle, vibrationStateText);
        }

        private static void SetTogglePresentation(Toggle toggle, Text label)
        {
            bool enabled = toggle.isOn;
            SettingsSwitchView switchView =
                toggle.GetComponent<SettingsSwitchView>();
            if (switchView != null)
            {
                switchView.Show(enabled);
            }
            label.text = enabled ? "ON" : "OFF";
            label.color = enabled
                ? NeonUiPalette.Signal
                : NeonUiPalette.TextSecondary;
        }

        private void OpenTerms() => OpenLink(ProductLinkType.Terms);
        private void OpenPrivacy() => OpenLink(ProductLinkType.Privacy);
        private void OpenSupport() => OpenLink(ProductLinkType.Support);

        private void OpenLink(ProductLinkType type)
        {
            if (linkConfiguration == null ||
                !linkConfiguration.TryGetHttpsUrl(type, out string url))
            {
                statusText.text = "URL NOT CONFIGURED";
                return;
            }

            if (!_urlOpener.TryOpen(url, out string error))
            {
                statusText.text = string.IsNullOrWhiteSpace(error)
                    ? "LINK OPEN FAILED"
                    : "LINK OPEN FAILED\n" + error;
            }
        }

        private void RefreshLinkState()
        {
            bool terms = ConfigureLinkButton(
                termsButton,
                ProductLinkType.Terms);
            bool privacy = ConfigureLinkButton(
                privacyButton,
                ProductLinkType.Privacy);
            bool support = ConfigureLinkButton(
                supportButton,
                ProductLinkType.Support);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            linkRow.SetActive(true);
            linkStatusText.gameObject.SetActive(true);
            linkStatusText.text = terms && privacy && support
                ? string.Empty
                : "URL NOT CONFIGURED";
#else
            // Release builds hide unconfigured links instead of naming them.
            linkRow.SetActive(terms || privacy || support);
            linkStatusText.gameObject.SetActive(false);
            linkStatusText.text = string.Empty;
#endif
        }

        private bool ConfigureLinkButton(
            Button button,
            ProductLinkType type)
        {
            bool configured = linkConfiguration != null &&
                linkConfiguration.TryGetHttpsUrl(type, out _);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            button.gameObject.SetActive(true);
            button.interactable = configured;
#else
            button.gameObject.SetActive(configured);
            button.interactable = configured;
#endif
            return configured;
        }

        private static float ResolveActiveVolume(
            float current,
            float lastNonZero)
        {
            if (current > 0f)
            {
                return Mathf.Clamp01(current);
            }
            if (lastNonZero > 0f)
            {
                return Mathf.Clamp01(lastNonZero);
            }
            return 1f;
        }

        private void ValidateRequiredReferences()
        {
            if (!HasRequiredReferences())
            {
                throw new InvalidOperationException(
                    "Settings Panel references are incomplete.");
            }
        }
    }
}
