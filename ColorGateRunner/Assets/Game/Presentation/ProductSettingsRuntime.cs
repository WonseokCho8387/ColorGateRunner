using ColorGateRunner.Product;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal interface IProductSettingsHost
    {
        LocalSettingsData CurrentSettings { get; }

        ProductMutationResult ApplySettings(
            float masterVolume,
            float musicVolume,
            float sfxVolume,
            bool vibration);
    }

    internal static class UnityProductSettingsRuntime
    {
        internal static void ApplyMasterVolume(LocalSettingsData settings)
        {
            if (settings != null)
            {
                AudioListener.volume = Mathf.Clamp01(settings.MasterVolume);
            }
        }
    }
}
