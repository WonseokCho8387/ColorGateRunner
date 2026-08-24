using System;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class LobbyProgressionPanel : MonoBehaviour
    {
        [SerializeField] private Text coinText;
        [SerializeField] private Text heartText;
        [SerializeField] private Text inventoryText;
        [SerializeField] private Text themeText;
        [SerializeField] private Text nextUpgradeText;
        [SerializeField] private Text rewardSummaryText;
        [SerializeField] private Image themeBackground;
        [SerializeField] private Image themeArtworkBackground;
        [SerializeField] private Image themeArtworkMidground;
        [SerializeField] private Image themeArtworkForeground;
        [SerializeField] private LobbyThemeVisualCatalog themeVisualCatalog;
        [SerializeField] private GameObject[] upgradeVisuals;

        private Vector3 _midgroundBaseScale = Vector3.one;
        private bool _midgroundScaleCaptured;

        internal Text CoinText => coinText;
        internal Text HeartText => heartText;
        internal Text InventoryText => inventoryText;
        internal Text ThemeText => themeText;
        internal Text NextUpgradeText => nextUpgradeText;
        internal Text RewardText => rewardSummaryText;
        internal Image ThemeArtworkBackground => themeArtworkBackground;
        internal Image ThemeArtworkMidground => themeArtworkMidground;
        internal Image ThemeArtworkForeground => themeArtworkForeground;
        internal LobbyThemeVisualCatalog ThemeVisualCatalog =>
            themeVisualCatalog;

        internal int ActiveUpgradeVisualCount()
        {
            int active = 0;
            if (upgradeVisuals == null)
            {
                return active;
            }
            for (int index = 0; index < upgradeVisuals.Length; index++)
            {
                if (upgradeVisuals[index] != null &&
                    upgradeVisuals[index].activeSelf)
                {
                    active++;
                }
            }
            return active;
        }

        internal int Bind(
            ProgressionService progression,
            HeartStateSnapshot hearts,
            DateTime utcNow)
        {
            if (progression?.Economy == null || progression.Lobby == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            int applied = Mathf.Clamp(
                progression.Lobby.AppliedMilestoneCount,
                0,
                18);
            int themeIndex = Mathf.Min(2, applied / 6);
            int localApplied = applied == 18 ? 6 : applied % 6;
            coinText.text = $"COINS {progression.Economy.Coins}";
            RefreshHeart(hearts, utcNow);
            inventoryText.text =
                $"SHIELD {progression.Economy.ShieldCount}   " +
                $"BOOSTER {progression.Economy.BoosterCount}";
            ApplyThemeVisual(themeIndex);
            for (int index = 0; index < upgradeVisuals.Length; index++)
            {
                upgradeVisuals[index].SetActive(index < localApplied);
            }

            nextUpgradeText.text = applied >= 18
                ? "LOBBY 18/18   COMPLETE"
                : $"LOBBY {applied}/18   NEXT STAGE {(applied + 1) * 2}";
            int pending = Math.Max(
                0,
                applied - progression.Lobby.PresentedMilestoneCount);
            rewardSummaryText.gameObject.SetActive(pending > 0);
            rewardSummaryText.text = pending == 1
                ? "LOBBY UPGRADE + REWARD"
                : $"{pending} UPGRADES + REWARDS";
            return pending;
        }

        private void Update()
        {
            if (themeArtworkMidground == null ||
                !themeArtworkMidground.gameObject.activeInHierarchy)
            {
                return;
            }

            CaptureMidgroundScale();
            float pulse = 1f +
                (Mathf.Sin(Time.unscaledTime * 1.4f) * 0.008f);
            themeArtworkMidground.rectTransform.localScale =
                _midgroundBaseScale * pulse;
            if (themeArtworkForeground != null &&
                themeArtworkForeground.gameObject.activeInHierarchy)
            {
                Color color = themeArtworkForeground.color;
                color.a = 0.72f +
                    (Mathf.Sin(Time.unscaledTime * 1.1f) * 0.08f);
                themeArtworkForeground.color = color;
            }
        }

        private void ApplyThemeVisual(int themeIndex)
        {
            if (!themeVisualCatalog.TryGet(
                    themeIndex,
                    out LobbyThemeVisualDefinition definition))
            {
                throw new InvalidOperationException(
                    $"Lobby theme {themeIndex} is not configured.");
            }

            themeText.text = definition.DisplayName;
            themeBackground.color = definition.BackgroundTint;
            SetArtwork(themeArtworkBackground, definition.Background);
            SetArtwork(themeArtworkMidground, definition.Midground);
            SetArtwork(themeArtworkForeground, definition.Foreground);
            CaptureMidgroundScale();
        }

        private static void SetArtwork(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(sprite != null);
        }

        private void CaptureMidgroundScale()
        {
            if (_midgroundScaleCaptured || themeArtworkMidground == null)
            {
                return;
            }

            _midgroundBaseScale =
                themeArtworkMidground.rectTransform.localScale;
            _midgroundScaleCaptured = true;
        }

        internal void RefreshHeart(
            HeartStateSnapshot hearts,
            DateTime utcNow)
        {
            heartText.text = FormatHeart(hearts, utcNow);
        }

        internal static string FormatHeart(
            HeartStateSnapshot hearts,
            DateTime utcNow)
        {
            DateTime now = utcNow.Kind switch
            {
                DateTimeKind.Utc => utcNow,
                DateTimeKind.Local => utcNow.ToUniversalTime(),
                _ => DateTime.SpecifyKind(utcNow, DateTimeKind.Utc)
            };
            if (hearts.Unlimited)
            {
                return $"HEARTS UNLIMITED  " +
                    FormatRemaining(hearts.UnlimitedUntilUtc - now);
            }
            if (hearts.Count >= HeartStatePolicy.MaximumHearts ||
                hearts.NextHeartAtUtc == default)
            {
                return $"HEARTS {hearts.Count}/" +
                    HeartStatePolicy.MaximumHearts;
            }
            return $"HEARTS {hearts.Count}/" +
                $"{HeartStatePolicy.MaximumHearts}  " +
                FormatRemaining(hearts.NextHeartAtUtc - now);
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            int seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int remainder = seconds % 60;
            return hours > 0
                ? $"{hours}:{minutes:00}:{remainder:00}"
                : $"{minutes:00}:{remainder:00}";
        }

        internal void Configure(
            Text coins,
            Text hearts,
            Text inventory,
            Text theme,
            Text nextUpgrade,
            Text rewardSummary,
            Image background,
            Image artworkBackground,
            Image artworkMidground,
            Image artworkForeground,
            LobbyThemeVisualCatalog visualCatalog,
            GameObject[] visuals)
        {
            coinText = coins;
            heartText = hearts;
            inventoryText = inventory;
            themeText = theme;
            nextUpgradeText = nextUpgrade;
            rewardSummaryText = rewardSummary;
            themeBackground = background;
            themeArtworkBackground = artworkBackground;
            themeArtworkMidground = artworkMidground;
            themeArtworkForeground = artworkForeground;
            themeVisualCatalog = visualCatalog;
            upgradeVisuals = visuals;
        }

        internal bool HasRequiredReferences()
        {
            if (coinText == null || heartText == null ||
                inventoryText == null ||
                themeText == null || nextUpgradeText == null ||
                rewardSummaryText == null || themeBackground == null ||
                themeArtworkBackground == null ||
                themeArtworkMidground == null ||
                themeArtworkForeground == null ||
                themeVisualCatalog == null ||
                upgradeVisuals == null || upgradeVisuals.Length != 6)
            {
                return false;
            }
            for (int index = 0; index < upgradeVisuals.Length; index++)
            {
                if (upgradeVisuals[index] == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
