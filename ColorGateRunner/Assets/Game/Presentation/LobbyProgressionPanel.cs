using System;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class LobbyProgressionPanel : MonoBehaviour
    {
        [SerializeField] private Text coinText;
        [SerializeField] private Text inventoryText;
        [SerializeField] private Text themeText;
        [SerializeField] private Text nextUpgradeText;
        [SerializeField] private Text rewardSummaryText;
        [SerializeField] private Image themeBackground;
        [SerializeField] private GameObject[] upgradeVisuals;

        internal Text CoinText => coinText;
        internal Text InventoryText => inventoryText;
        internal Text ThemeText => themeText;
        internal Text NextUpgradeText => nextUpgradeText;
        internal Text RewardText => rewardSummaryText;

        internal int Bind(ProgressionService progression)
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
            inventoryText.text =
                $"SHIELD {progression.Economy.ShieldCount}  " +
                $"BOOST {progression.Economy.BoosterCount}";
            themeText.text =
                $"THEME {themeIndex + 1}  {themeNames[themeIndex]}";
            themeBackground.color = themeColors[themeIndex];
            for (int index = 0; index < upgradeVisuals.Length; index++)
            {
                upgradeVisuals[index].SetActive(index < localApplied);
            }

            nextUpgradeText.text = applied >= 18
                ? "LOBBY COMPLETE"
                : $"NEXT LOBBY UPGRADE  CLEAR STAGE {(applied + 1) * 2}";
            int pending = Math.Max(
                0,
                applied - progression.Lobby.PresentedMilestoneCount);
            rewardSummaryText.gameObject.SetActive(pending > 0);
            rewardSummaryText.text = pending == 1
                ? "LOBBY UPGRADED  REWARD ADDED"
                : $"{pending} LOBBY UPGRADES  REWARDS ADDED";
            return pending;
        }

        internal void Configure(
            Text coins,
            Text inventory,
            Text theme,
            Text nextUpgrade,
            Text rewardSummary,
            Image background,
            GameObject[] visuals)
        {
            coinText = coins;
            inventoryText = inventory;
            themeText = theme;
            nextUpgradeText = nextUpgrade;
            rewardSummaryText = rewardSummary;
            themeBackground = background;
            upgradeVisuals = visuals;
        }

        internal bool HasRequiredReferences()
        {
            if (coinText == null || inventoryText == null ||
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
