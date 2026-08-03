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
        [SerializeField] private Text notificationNoticeText;
        [SerializeField] private Button termsButton;
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button supportButton;
        [SerializeField] private Text linkStatusText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ProductLinkConfiguration linkConfiguration;

        private IProductSettingsHost _host;
        private IExternalUrlOpener _urlOpener;
        private bool _listenersBound;

        internal event Action ApplySucceeded;
        internal event Action CancelRequested;

        internal Toggle NotificationToggle => notificationToggle;
        internal Toggle MusicToggle => musicToggle;
        internal Toggle SfxToggle => sfxToggle;
        internal Toggle VibrationToggle => vibrationToggle;
        internal Text NotificationNoticeText => notificationNoticeText;
        internal Button TermsButton => termsButton;
        internal Button PrivacyButton => privacyButton;
        internal Button SupportButton => supportButton;
        internal Text LinkStatusText => linkStatusText;
        internal Text StatusText => statusText;
        internal Button ApplyButton => applyButton;
        internal Button CancelButton => cancelButton;

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

        internal void Open(IProductSettingsHost host)
        {
            _host = host;
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
                notificationNoticeText != null && termsButton != null &&
                privacyButton != null && supportButton != null &&
                linkStatusText != null && statusText != null &&
                applyButton != null && cancelButton != null &&
                linkConfiguration != null;
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
            _listenersBound = false;
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
            linkStatusText.text = terms && privacy && support
                ? string.Empty
                : "URL NOT CONFIGURED";
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
