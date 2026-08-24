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

        internal static LobbyThemeVisualCatalog EnsureAndConfigure()
        {
            Sprite background = ImportSprite(BackgroundPath, false);
            Sprite reactor = ImportSprite(ReactorPath, true);
            Sprite ambient = ImportSprite(AmbientPath, true);

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
                    null,
                    null,
                    null),
                new LobbyThemeVisualDefinition(
                    "sky-festival",
                    "SKY FESTIVAL",
                    new Color(0.08f, 0.23f, 0.25f, 1f),
                    null,
                    null,
                    null)
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

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaSource = hasTransparency
                ? TextureImporterAlphaSource.FromInput
                : TextureImporterAlphaSource.None;
            importer.alphaIsTransparency = hasTransparency;
            importer.sRGBTexture = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();

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
