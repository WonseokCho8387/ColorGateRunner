using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal interface IHapticFeedback
    {
        void RequestBoosterLaunch();
    }

    internal sealed class UnityHapticFeedback : IHapticFeedback
    {
        private readonly Func<bool> _enabled;
        private readonly Action _vibrationRequest;

        internal UnityHapticFeedback(
            Func<bool> enabled = null,
            Action vibrationRequest = null)
        {
            _enabled = enabled;
            _vibrationRequest = vibrationRequest ?? VibrateDevice;
        }

        public void RequestBoosterLaunch()
        {
            if (_enabled != null && !_enabled())
            {
                return;
            }
            _vibrationRequest();
        }

        private static void VibrateDevice()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
