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
        [SerializeField] private GameObject[] upgradeVisuals;

        internal Text CoinText => coinText;
        internal Text HeartText => heartText;
        internal Text InventoryText => inventoryText;
        internal Text ThemeText => themeText;
        internal Text NextUpgradeText => nextUpgradeText;
        internal Text RewardText => rewardSummaryText;

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
            string[] themeNames =
            {
                "COLOR COURTYARD",
                "NEON GARDEN",
                "SKY FESTIVAL"
            };
            Color[] themeColors =
            {
                new Color(0.10f, 0.16f, 0.24f, 0.84f),
                new Color(0.17f, 0.10f, 0.27f, 0.84f),
                new Color(0.08f, 0.23f, 0.25f, 0.84f)
            };

            coinText.text = $"COINS {progression.Economy.Coins}";
            RefreshHeart(hearts, utcNow);
            inventoryText.text =
                $"SHIELD {progression.Economy.ShieldCount}   " +
                $"BOOSTER {progression.Economy.BoosterCount}";
            themeText.text = themeNames[themeIndex];
            themeBackground.color = themeColors[themeIndex];
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
            GameObject[] visuals)
        {
            coinText = coins;
            heartText = hearts;
            inventoryText = inventory;
            themeText = theme;
            nextUpgradeText = nextUpgrade;
            rewardSummaryText = rewardSummary;
            themeBackground = background;
            upgradeVisuals = visuals;
        }

        internal bool HasRequiredReferences()
        {
            if (coinText == null || heartText == null ||
                inventoryText == null ||
                themeText == null || nextUpgradeText == null ||
                rewardSummaryText == null || themeBackground == null ||
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
