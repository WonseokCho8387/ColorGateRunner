using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Editor
{
    internal static class Theme01UiSkinBuilder
    {
        internal const string ArtFolder = "Assets/Game/Art/UI/Theme01";
        internal const float SliceBorder = 28f;

        private static readonly string[] PanelNames =
        {
            "Panel", "Modal", "ItemCard", "ButtonPrimary",
            "ButtonSecondary", "ButtonDanger", "ResourceChip"
        };

        private static readonly string[] IconNames =
        {
            "Coin", "Heart", "Shield", "Booster", "Settings",
            "Play", "Back", "Pause", "Retry", "Continue"
        };

        internal static void EnsureAndConfigure()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            for (int index = 0; index < PanelNames.Length; index++)
            {
                ConfigureSprite(PanelNames[index], true);
            }

            for (int index = 0; index < IconNames.Length; index++)
            {
                ConfigureSprite("Icon" + IconNames[index], false);
            }
        }

        internal static void ApplyPanel(Image image, string semanticName)
        {
            if (image == null || !ShouldSkinPanel(semanticName))
            {
                return;
            }

            string asset = ResolvePanelAsset(semanticName);
            Sprite sprite = LoadSprite(asset);
            if (sprite == null)
            {
                throw new InvalidOperationException(
                    $"Theme01 UI sprite is missing: {asset}");
            }

            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
        }

        internal static void ApplyButton(
            Button button,
            string semanticName,
            Text label)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            string asset = ResolveButtonAsset(semanticName);
            image.sprite = LoadSprite(asset);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f),
                pressedColor = new Color(0.68f, 0.84f, 0.95f, 1f),
                selectedColor = new Color(0.86f, 1.0f, 1.0f, 1f),
                disabledColor = new Color(0.28f, 0.34f, 0.42f, 0.62f),
                colorMultiplier = 1f,
                fadeDuration = 0.08f
            };

            string iconName = ResolveIconName(semanticName);
            if (iconName != null)
            {
                AddButtonIcon(button.transform, iconName);
                if (label != null)
                {
                    RectTransform labelRect = label.rectTransform;
                    labelRect.anchorMin = new Vector2(0.23f, 0f);
                    labelRect.anchorMax = new Vector2(0.96f, 1f);
                    labelRect.offsetMin = Vector2.zero;
                    labelRect.offsetMax = Vector2.zero;
                }
            }
        }

        internal static Image AddStandaloneIcon(
            string name,
            Transform parent,
            string iconName,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            var iconObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image));
            iconObject.transform.SetParent(parent, false);
            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = iconObject.GetComponent<Image>();
            image.sprite = LoadSprite("Icon" + iconName);
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = Color.white;
            return image;
        }

        private static void AddButtonIcon(Transform parent, string iconName)
        {
            AddStandaloneIcon(
                "ThemeIcon",
                parent,
                iconName,
                new Vector2(0.045f, 0.18f),
                new Vector2(0.22f, 0.82f));
        }

        private static bool ShouldSkinPanel(string name)
        {
            if (string.IsNullOrEmpty(name) ||
                name.Contains("Background", StringComparison.Ordinal) ||
                name.Contains("Artwork", StringComparison.Ordinal) ||
                name.Contains("Fill", StringComparison.Ordinal) ||
                name.Contains("Dim", StringComparison.Ordinal) ||
                name.Contains("Blocker", StringComparison.Ordinal) ||
                name.Contains("Decoration", StringComparison.Ordinal) ||
                name.Contains("Tier_", StringComparison.Ordinal) ||
                name.Contains("Core", StringComparison.Ordinal))
            {
                return false;
            }

            return name.Contains("Panel", StringComparison.Ordinal) ||
                name.Contains("Popup", StringComparison.Ordinal) ||
                name.Contains("Modal", StringComparison.Ordinal) ||
                name.Contains("Card", StringComparison.Ordinal) ||
                name.Contains("Slot", StringComparison.Ordinal) ||
                name.Contains("Bar", StringComparison.Ordinal) ||
                name.Contains("Track", StringComparison.Ordinal) ||
                name.Contains("StageHud", StringComparison.Ordinal);
        }

        private static string ResolvePanelAsset(string name)
        {
            if (name.Contains("Modal", StringComparison.Ordinal) ||
                name.Contains("Popup", StringComparison.Ordinal) ||
                name.Contains("Confirmation", StringComparison.Ordinal) ||
                name.Contains("Pause", StringComparison.Ordinal) ||
                name.Contains("Result", StringComparison.Ordinal) ||
                name.Contains("Failed", StringComparison.Ordinal) ||
                name.Contains("Clear", StringComparison.Ordinal))
            {
                return "Modal";
            }

            if (name.Contains("Item", StringComparison.Ordinal) ||
                name.Contains("Card", StringComparison.Ordinal))
            {
                return "ItemCard";
            }

            if (name.Contains("Slot", StringComparison.Ordinal) ||
                name.Contains("Bar", StringComparison.Ordinal) ||
                name.Contains("Track", StringComparison.Ordinal))
            {
                return "ResourceChip";
            }

            return "Panel";
        }

        private static string ResolveButtonAsset(string name)
        {
            if (ContainsAny(name, "Reset", "Restart", "Retry", "Leave"))
            {
                return "ButtonDanger";
            }

            if (ContainsAny(
                    name,
                    "Play", "Start", "Continue", "Confirm", "Resume", "Next"))
            {
                return "ButtonPrimary";
            }

            return "ButtonSecondary";
        }

        private static string ResolveIconName(string name)
        {
            if (name.Contains("Settings", StringComparison.Ordinal)) return "Settings";
            if (name.Contains("PauseButton", StringComparison.Ordinal)) return "Pause";
            if (name.Contains("Retry", StringComparison.Ordinal) ||
                name.Contains("Restart", StringComparison.Ordinal)) return "Retry";
            if (name.Contains("Shield", StringComparison.Ordinal)) return "Shield";
            if (name.Contains("Booster", StringComparison.Ordinal)) return "Booster";
            if (name.Contains("CoinContinue", StringComparison.Ordinal)) return "Coin";
            if (name.Contains("Continue", StringComparison.Ordinal) ||
                name.Contains("Next", StringComparison.Ordinal)) return "Continue";
            if (name.Contains("Play", StringComparison.Ordinal) ||
                name.Contains("Start", StringComparison.Ordinal) ||
                name.Contains("Resume", StringComparison.Ordinal)) return "Play";
            if (name.Contains("Back", StringComparison.Ordinal) ||
                name.Contains("Lobby", StringComparison.Ordinal) ||
                name.Contains("Account", StringComparison.Ordinal)) return "Back";
            return null;
        }

        private static bool ContainsAny(string value, params string[] candidates)
        {
            for (int index = 0; index < candidates.Length; index++)
            {
                if (value.Contains(candidates[index], StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(
                $"{ArtFolder}/{name}.png");
        }

        private static void ConfigureSprite(string name, bool sliced)
        {
            string path = $"{ArtFolder}/{name}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    $"Theme01 UI texture is missing: {path}");
            }

            Vector4 border = sliced
                ? new Vector4(SliceBorder, SliceBorder, SliceBorder, SliceBorder)
                : Vector4.zero;
            bool changed = importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                importer.mipmapEnabled ||
                !importer.alphaIsTransparency ||
                importer.spriteBorder != border ||
                importer.filterMode != FilterMode.Bilinear ||
                importer.wrapMode != TextureWrapMode.Clamp;
            if (!changed)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spriteBorder = border;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 256;
            importer.SaveAndReimport();
        }
    }
}
