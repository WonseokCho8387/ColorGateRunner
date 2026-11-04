namespace ColorGateRunner.Presentation
{
    // Settings rows are shown only when the build can honor them, so a
    // preference never pretends to control something that does not exist.
    internal readonly struct SettingsAvailability
    {
        // Campaign Stage music, lobby music and UI/gameplay cues ship.
        internal const bool MusicOutputAvailable = true;
        internal const bool SfxOutputAvailable = true;
        // Notification delivery is deferred; the preference stays persisted.
        internal const bool NotificationDeliveryAvailable = false;

        internal SettingsAvailability(
            bool notifications,
            bool music,
            bool sfx,
            bool vibration)
        {
            Notifications = notifications;
            Music = music;
            Sfx = sfx;
            Vibration = vibration;
        }

        internal bool Notifications { get; }
        internal bool Music { get; }
        internal bool Sfx { get; }
        internal bool Vibration { get; }
        internal bool AnyAudio => Music || Sfx;
        internal bool AnyGameplay => Vibration || Notifications;

        internal static SettingsAvailability Current =>
            ForPlatform(PlatformSupportsHaptics);

        internal static SettingsAvailability ForPlatform(bool supportsHaptics)
        {
            return new SettingsAvailability(
                NotificationDeliveryAvailable,
                MusicOutputAvailable,
                SfxOutputAvailable,
                supportsHaptics);
        }

        private static bool PlatformSupportsHaptics
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }
    }
}
