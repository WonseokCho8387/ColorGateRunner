using System;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace ColorGateRunner.Editor
{
    // Assigns the generated app icon art (Tools/Generate-AppIcon.ps1) to the
    // Android adaptive, round and legacy icon slots.
    public static class AppIconBuilder
    {
        internal const string Folder = "Assets/Game/Art/AppIcon";
        internal const string BackgroundPath = Folder + "/AppIconBackground.png";
        internal const string ForegroundPath = Folder + "/AppIconForeground.png";
        internal const string LegacyPath = Folder + "/AppIconLegacy.png";

        [MenuItem("Tools/Color Gate Runner/Apply App Icons")]
        public static void EnsureConfigured()
        {
            Texture2D background = ImportIcon(BackgroundPath, false);
            Texture2D foreground = ImportIcon(ForegroundPath, true);
            Texture2D legacy = ImportIcon(LegacyPath, false);

            SetLayers(AndroidPlatformIconKind.Adaptive, background, foreground);
            SetLayers(AndroidPlatformIconKind.Round, legacy);
            SetLayers(AndroidPlatformIconKind.Legacy, legacy);
            AssetDatabase.SaveAssets();
        }

        private static void SetLayers(
            PlatformIconKind kind,
            params Texture2D[] layers)
        {
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(
                NamedBuildTarget.Android,
                kind);
            foreach (PlatformIcon icon in icons)
            {
                for (int layer = 0; layer < layers.Length; layer++)
                {
                    icon.SetTexture(layers[layer], layer);
                }
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }

        private static Texture2D ImportIcon(string path, bool transparent)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter ??
                throw new InvalidOperationException($"App icon is missing: {path}");
            TextureImporterAlphaSource alpha = transparent
                ? TextureImporterAlphaSource.FromInput
                : TextureImporterAlphaSource.None;
            if (importer.textureType != TextureImporterType.Default ||
                importer.mipmapEnabled ||
                importer.alphaSource != alpha ||
                importer.alphaIsTransparency != transparent ||
                importer.npotScale != TextureImporterNPOTScale.None ||
                importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.alphaSource = alpha;
                importer.alphaIsTransparency = transparent;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
