using System;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class SettingsPanelController : MonoBehaviour
    {
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Text masterValueText;
        [SerializeField] private Text musicValueText;
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;

        private IProductSettingsHost _host;
        private bool _listenersBound;
        private UnityAction<float> _masterChanged;
        private UnityAction<float> _musicChanged;
        private UnityAction<float> _sfxChanged;

        internal event Action ApplySucceeded;
        internal event Action CancelRequested;

        internal Slider MasterSlider => masterSlider;
        internal Slider MusicSlider => musicSlider;
        internal Slider SfxSlider => sfxSlider;
        internal Toggle VibrationToggle => vibrationToggle;
        internal Text StatusText => statusText;
        internal Button ApplyButton => applyButton;
        internal Button CancelButton => cancelButton;

        private void Awake()
        {
            ValidateRequiredReferences();
            BindListeners();
        }

        private void OnDestroy()
        {
            UnbindListeners();
        }

        internal void Configure(
            Slider master,
            Slider music,
            Slider sfx,
            Toggle vibration,
            Text masterValue,
            Text musicValue,
            Text sfxValue,
            Text status,
            Button apply,
            Button cancel)
        {
            masterSlider = master;
            musicSlider = music;
            sfxSlider = sfx;
            vibrationToggle = vibration;
            masterValueText = masterValue;
            musicValueText = musicValue;
            sfxValueText = sfxValue;
            statusText = status;
            applyButton = apply;
            cancelButton = cancel;
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
            masterSlider.SetValueWithoutNotify(settings.MasterVolume);
            musicSlider.SetValueWithoutNotify(settings.MusicVolume);
            sfxSlider.SetValueWithoutNotify(settings.SfxVolume);
            vibrationToggle.SetIsOnWithoutNotify(settings.Vibration);
            statusText.text = string.Empty;
            RefreshValueLabels();
        }

        internal bool HasRequiredReferences()
        {
            return masterSlider != null && musicSlider != null &&
                sfxSlider != null && vibrationToggle != null &&
                masterValueText != null && musicValueText != null &&
                sfxValueText != null && statusText != null &&
                applyButton != null && cancelButton != null;
        }

        private void Apply()
        {
            if (_host == null)
            {
                statusText.text = "SETTINGS UNAVAILABLE";
                return;
            }

            ProductMutationResult result = _host.ApplySettings(
                masterSlider.value,
                musicSlider.value,
                sfxSlider.value,
                vibrationToggle.isOn);
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

            _masterChanged = _ => RefreshValueLabels();
            _musicChanged = _ => RefreshValueLabels();
            _sfxChanged = _ => RefreshValueLabels();
            masterSlider.onValueChanged.AddListener(_masterChanged);
            musicSlider.onValueChanged.AddListener(_musicChanged);
            sfxSlider.onValueChanged.AddListener(_sfxChanged);
            applyButton.onClick.AddListener(Apply);
            cancelButton.onClick.AddListener(Cancel);
            _listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!_listenersBound)
            {
                return;
            }

            masterSlider.onValueChanged.RemoveListener(_masterChanged);
            musicSlider.onValueChanged.RemoveListener(_musicChanged);
            sfxSlider.onValueChanged.RemoveListener(_sfxChanged);
            applyButton.onClick.RemoveListener(Apply);
            cancelButton.onClick.RemoveListener(Cancel);
            _listenersBound = false;
        }

        private void RefreshValueLabels()
        {
            masterValueText.text = FormatPercent(masterSlider.value);
            musicValueText.text = FormatPercent(musicSlider.value);
            sfxValueText.text = FormatPercent(sfxSlider.value);
        }

        private static string FormatPercent(float value)
        {
            return Mathf.RoundToInt(Mathf.Clamp01(value) * 100f) + "%";
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
