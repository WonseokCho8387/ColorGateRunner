using UnityEngine;

namespace ColorGateRunner.Presentation
{
    // Iteration 61 neon-night UI tokens. Gameplay judgment colors stay owned by
    // the gameplay Builder and must never be reused as UI chrome.
    public static class NeonUiPalette
    {
        public static readonly Color Ink = FromHex(0x05060D);
        public static readonly Color Night = FromHex(0x0A0F1F);
        public static readonly Color Line = FromHex(0x1C2540);
        public static readonly Color Signal = FromHex(0xC8F4FF);
        public static readonly Color Magenta = FromHex(0xFF2BD6);
        public static readonly Color Danger = FromHex(0xFF5A4E);
        public static readonly Color Gold = FromHex(0xFFC83D);
        public static readonly Color TextPrimary = FromHex(0xF2F7FF);
        public static readonly Color TextSecondary = FromHex(0x8A96B8);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color FromHex(int value)
        {
            return new Color(
                ((value >> 16) & 0xFF) / 255f,
                ((value >> 8) & 0xFF) / 255f,
                (value & 0xFF) / 255f,
                1f);
        }
    }
}
