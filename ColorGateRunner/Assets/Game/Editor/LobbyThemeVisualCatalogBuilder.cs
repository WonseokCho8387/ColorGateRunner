using ColorGateRunner.Presentation;
using UnityEditor;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    internal static class LobbyThemeVisualCatalogBuilder
    {
        internal const string AssetPath =
            "Assets/Game/Resources/LobbyThemeVisualCatalog.asset";
        internal const string BackgroundPath =
            "Assets/Game/Art/Lobby/Theme01/ColorCourtyard_Background.png";
        internal const string ReactorPath =
            "Assets/Game/Art/Lobby/Theme01/ColorCourtyard_Reactor.png";
        internal const string AmbientPath =
            "Assets/Game/Art/Lobby/Theme01/ColorCourtyard_Ambient.png";
        internal const string NeonGardenBackgroundPath =
            "Assets/Game/Art/Lobby/Theme02/NeonGarden_Background.png";
        internal const string NeonGardenReactorPath =
            "Assets/Game/Art/Lobby/Theme02/NeonGarden_Reactor.png";
        internal const string NeonGardenAmbientPath =
            "Assets/Game/Art/Lobby/Theme02/NeonGarden_Ambient.png";
        internal const string SkyFestivalBackgroundPath =
            "Assets/Game/Art/Lobby/Theme03/SkyFestival_Background.png";
        internal const string SkyFestivalBeaconPath =
            "Assets/Game/Art/Lobby/Theme03/SkyFestival_Beacon.png";
        internal const string SkyFestivalAmbientPath =
            "Assets/Game/Art/Lobby/Theme03/SkyFestival_Ambient.png";

        internal static LobbyThemeVisualCatalog EnsureAndConfigure()
        {
            Sprite background = ImportSprite(BackgroundPath, false);
            Sprite reactor = ImportSprite(ReactorPath, true);
            Sprite ambient = ImportSprite(AmbientPath, true);
            Sprite neonGardenBackground = ImportSprite(
                NeonGardenBackgroundPath,
                false);
            Sprite neonGardenReactor = ImportSprite(
                NeonGardenReactorPath,
                true);
            Sprite neonGardenAmbient = ImportSprite(
                NeonGardenAmbientPath,
                true);
            Sprite skyFestivalBackground = ImportSprite(
                SkyFestivalBackgroundPath,
                false);
            Sprite skyFestivalBeacon = ImportSprite(
                SkyFestivalBeaconPath,
                true);
            Sprite skyFestivalAmbient = ImportSprite(
                SkyFestivalAmbientPath,
                true);

            LobbyThemeVisualCatalog catalog =
                AssetDatabase.LoadAssetAtPath<LobbyThemeVisualCatalog>(
                    AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<
                    LobbyThemeVisualCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            catalog.Configure(new[]
            {
                new LobbyThemeVisualDefinition(
                    "color-courtyard",
                    "COLOR COURTYARD",
                    new Color(0.035f, 0.07f, 0.15f, 1f),
                    background,
                    reactor,
                    ambient),
                new LobbyThemeVisualDefinition(
                    "neon-garden",
                    "NEON GARDEN",
                    new Color(0.17f, 0.10f, 0.27f, 1f),
                    neonGardenBackground,
                    neonGardenReactor,
                    neonGardenAmbient),
                new LobbyThemeVisualDefinition(
                    "sky-festival",
                    "SKY FESTIVAL",
                    new Color(0.08f, 0.23f, 0.25f, 1f),
                    skyFestivalBackground,
                    skyFestivalBeacon,
                    skyFestivalAmbient)
            });
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static Sprite ImportSprite(string path, bool hasTransparency)
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new System.InvalidOperationException(
                    $"Lobby artwork is missing or invalid: {path}");
            }

            TextureImporterAlphaSource alphaSource = hasTransparency
                ? TextureImporterAlphaSource.FromInput
                : TextureImporterAlphaSource.None;
            bool requiresUpdate =
                importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                importer.spritePixelsPerUnit != 100f ||
                importer.mipmapEnabled ||
                importer.alphaSource != alphaSource ||
                importer.alphaIsTransparency != hasTransparency ||
                !importer.sRGBTexture ||
                importer.wrapMode != TextureWrapMode.Clamp ||
                importer.filterMode != FilterMode.Bilinear ||
                importer.maxTextureSize != 2048 ||
                importer.textureCompression !=
                    TextureImporterCompression.CompressedHQ;
            if (requiresUpdate)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled = false;
                importer.alphaSource = alphaSource;
                importer.alphaIsTransparency = hasTransparency;
                importer.sRGBTexture = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.textureCompression =
                    TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new System.InvalidOperationException(
                    $"Lobby artwork did not import as a Sprite: {path}");
            }
            return sprite;
        }
    }
}
