using System;
using System.Globalization;
using ColorGateRunner.Product;

namespace ColorGateRunner.Presentation
{
    public sealed class FrontendDisplayContext
    {
        private FrontendDisplayContext(
            string displayName,
            string accountLabel,
            string settingsSummary,
            string versionLabel,
            bool settingsAvailable,
            bool accountChoiceCompleted,
            bool googleProviderAvailable)
        {
            DisplayName = displayName;
            AccountLabel = accountLabel;
            SettingsSummary = settingsSummary;
            VersionLabel = versionLabel;
            SettingsAvailable = settingsAvailable;
            AccountChoiceCompleted = accountChoiceCompleted;
            GoogleProviderAvailable = googleProviderAvailable;
        }

        public string DisplayName { get; }
        public string AccountLabel { get; }
        public string SettingsSummary { get; }
        public string VersionLabel { get; }
        public bool SettingsAvailable { get; }
        public bool AccountChoiceCompleted { get; }
        public bool GoogleProviderAvailable { get; }

        public static bool TryCreate(
            AppServiceGraph graph,
            string applicationVersion,
            out FrontendDisplayContext context,
            out string error)
        {
            context = null;
            if (graph == null || graph.Profile == null ||
                graph.Profile.Current == null || graph.Account == null)
            {
                error = "BOOT REQUIRED";
                return false;
            }

            LocalProfileData profile = graph.Profile.Current;
            if (string.IsNullOrWhiteSpace(profile.DisplayName))
            {
                error = "PROFILE UNAVAILABLE";
                return false;
            }

            LocalSettingsData settings = graph.Settings?.Current;
            bool settingsAvailable = settings != null;
            context = new FrontendDisplayContext(
                profile.DisplayName,
                GetAccountLabel(graph.Account.CurrentState),
                settingsAvailable
                    ? CreateSettingsSummary(settings)
                    : "SETTINGS UNAVAILABLE",
                "v" + (string.IsNullOrWhiteSpace(applicationVersion)
                    ? "0.0.0"
                    : applicationVersion.Trim()),
                settingsAvailable,
                profile.AccountChoiceCompleted,
                graph.Account.IsProviderAvailable("google"));
            error = string.Empty;
            return true;
        }

        private static string GetAccountLabel(LocalAccountState state)
        {
            return state == LocalAccountState.Guest
                ? "GUEST"
                : "LOCAL ERROR";
        }

        private static string CreateSettingsSummary(LocalSettingsData settings)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "MASTER {0:0}%\nMUSIC {1:0}%\nSFX {2:0}%\nVIBRATION {3}\nLANGUAGE {4}",
                settings.MasterVolume * 100f,
                settings.MusicVolume * 100f,
                settings.SfxVolume * 100f,
                settings.Vibration ? "ON" : "OFF",
                string.IsNullOrWhiteSpace(settings.Language)
                    ? "SYSTEM"
                    : settings.Language.ToUpperInvariant());
        }
    }
}
