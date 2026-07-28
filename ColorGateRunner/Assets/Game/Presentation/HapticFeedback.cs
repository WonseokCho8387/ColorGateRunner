using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal interface IHapticFeedback
    {
        void RequestBoosterLaunch();
    }

    internal sealed class UnityHapticFeedback : IHapticFeedback
    {
        public void RequestBoosterLaunch()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
