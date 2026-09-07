using System;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    [Serializable]
    internal sealed class LobbyThemeVisualDefinition
    {
        [SerializeField] private string themeId;
        [SerializeField] private string displayName;
        [SerializeField] private Color backgroundTint;
        [SerializeField] private Sprite background;
        [SerializeField] private Sprite midground;
        [SerializeField] private Sprite foreground;

        internal LobbyThemeVisualDefinition(
            string id,
            string name,
            Color tint,
            Sprite backgroundSprite,
            Sprite midgroundSprite,
            Sprite foregroundSprite)
        {
            themeId = id;
            displayName = name;
            backgroundTint = tint;
            background = backgroundSprite;
            midground = midgroundSprite;
            foreground = foregroundSprite;
        }

        internal string ThemeId => themeId;
        internal string DisplayName => displayName;
        internal Color BackgroundTint => backgroundTint;
        internal Sprite Background => background;
        internal Sprite Midground => midground;
        internal Sprite Foreground => foreground;
        internal bool HasCompleteArtwork =>
            background != null && midground != null && foreground != null;
    }

    [CreateAssetMenu(
        fileName = "LobbyThemeVisualCatalog",
        menuName = "Color Gate Runner/Lobby Theme Visual Catalog")]
    public sealed class LobbyThemeVisualCatalog : ScriptableObject
    {
        [SerializeField]
        private LobbyThemeVisualDefinition[] themes =
            Array.Empty<LobbyThemeVisualDefinition>();

        internal int Count => themes?.Length ?? 0;

        internal bool TryGet(
            int themeIndex,
            out LobbyThemeVisualDefinition definition)
        {
            if (themes == null || themeIndex < 0 ||
                themeIndex >= themes.Length || themes[themeIndex] == null)
            {
                definition = null;
                return false;
            }

            definition = themes[themeIndex];
            return true;
        }

        internal bool TryGetById(
            string themeId,
            out LobbyThemeVisualDefinition definition)
        {
            if (themes != null)
            {
                for (int index = 0; index < themes.Length; index++)
                {
                    LobbyThemeVisualDefinition candidate = themes[index];
                    if (candidate != null && string.Equals(
                            candidate.ThemeId,
                            themeId,
                            StringComparison.Ordinal))
                    {
                        definition = candidate;
                        return true;
                    }
                }
            }
            definition = null;
            return false;
        }

        internal void Configure(LobbyThemeVisualDefinition[] definitions)
        {
            themes = definitions ?? Array.Empty<LobbyThemeVisualDefinition>();
        }
    }
}
