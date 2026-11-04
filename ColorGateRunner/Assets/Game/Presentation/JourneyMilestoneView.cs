using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class JourneyMilestoneView : MonoBehaviour
    {
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image nodeGlow;
        [SerializeField] private Text stageText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text coinText;
        [SerializeField] private GameObject shieldRoot;
        [SerializeField] private Text shieldText;
        [SerializeField] private GameObject boosterRoot;
        [SerializeField] private Text boosterText;

        internal Text StageText => stageText;
        internal Text StateText => stateText;

        internal void Configure(
            Image configuredCardBackground,
            Image configuredNodeGlow,
            Text configuredStageText,
            Text configuredStateText,
            Text configuredCoinText,
            GameObject configuredShieldRoot,
            Text configuredShieldText,
            GameObject configuredBoosterRoot,
            Text configuredBoosterText)
        {
            cardBackground = configuredCardBackground;
            nodeGlow = configuredNodeGlow;
            stageText = configuredStageText;
            stateText = configuredStateText;
            coinText = configuredCoinText;
            shieldRoot = configuredShieldRoot;
            shieldText = configuredShieldText;
            boosterRoot = configuredBoosterRoot;
            boosterText = configuredBoosterText;
        }

        internal bool HasRequiredReferences() =>
            cardBackground != null && nodeGlow != null &&
            stageText != null && stateText != null && coinText != null &&
            shieldRoot != null && shieldText != null &&
            boosterRoot != null && boosterText != null;

        internal void SetVerticalPosition(float centerY, float halfHeight)
        {
            SetVertical(
                cardBackground.rectTransform,
                centerY,
                halfHeight);
            SetVertical(
                nodeGlow.rectTransform,
                centerY,
                halfHeight * 0.28f);
        }

        private static void SetVertical(
            RectTransform rect,
            float centerY,
            float halfHeight)
        {
            rect.anchorMin = new Vector2(
                rect.anchorMin.x,
                centerY - halfHeight);
            rect.anchorMax = new Vector2(
                rect.anchorMax.x,
                centerY + halfHeight);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal void Bind(JourneyMilestoneCardModel model)
        {
            stageText.text = $"STAGE {model.StageNumber}";
            coinText.text = model.Reward.MilestoneCoins.ToString("N0");
            shieldRoot.SetActive(model.Reward.Shields > 0);
            shieldText.text = $"x{model.Reward.Shields}";
            boosterRoot.SetActive(model.Reward.Boosters > 0);
            boosterText.text = $"x{model.Reward.Boosters}";

            Color cardColor;
            Color glowColor;
            switch (model.State)
            {
                case JourneyMilestoneState.Collected:
                    stateText.text = "COLLECTED";
                    cardColor = NeonUiPalette.Signal;
                    glowColor = NeonUiPalette.Signal;
                    break;
                case JourneyMilestoneState.Current:
                    stateText.text = "NEXT REWARD";
                    cardColor = NeonUiPalette.Magenta;
                    glowColor = NeonUiPalette.Magenta;
                    break;
                case JourneyMilestoneState.ComingSoon:
                    stateText.text = "COMING SOON";
                    cardColor = NeonUiPalette.PlateMuted;
                    glowColor = NeonUiPalette.TextMuted;
                    break;
                default:
                    stateText.text = "LOCKED";
                    cardColor = NeonUiPalette.PlateMuted;
                    glowColor = NeonUiPalette.TextSecondary;
                    break;
            }
            cardBackground.color = cardColor;
            nodeGlow.color = glowColor;
        }
    }
}
