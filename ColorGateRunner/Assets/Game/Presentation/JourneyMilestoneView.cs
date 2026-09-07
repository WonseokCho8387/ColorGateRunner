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
                    cardColor = new Color(0.04f, 0.34f, 0.28f, 0.96f);
                    glowColor = new Color(0.20f, 1f, 0.65f, 1f);
                    break;
                case JourneyMilestoneState.Current:
                    stateText.text = "NEXT REWARD";
                    cardColor = new Color(0.09f, 0.34f, 0.58f, 0.98f);
                    glowColor = new Color(0.25f, 0.95f, 1f, 1f);
                    break;
                case JourneyMilestoneState.ComingSoon:
                    stateText.text = "COMING SOON";
                    cardColor = new Color(0.15f, 0.16f, 0.24f, 0.92f);
                    glowColor = new Color(0.55f, 0.58f, 0.70f, 0.75f);
                    break;
                default:
                    stateText.text = "LOCKED";
                    cardColor = new Color(0.10f, 0.16f, 0.28f, 0.94f);
                    glowColor = new Color(0.25f, 0.48f, 0.68f, 0.72f);
                    break;
            }
            cardBackground.color = cardColor;
            nodeGlow.color = glowColor;
        }
    }
}
