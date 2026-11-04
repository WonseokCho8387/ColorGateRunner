using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class StageCollectionRowView : MonoBehaviour
    {
        [SerializeField] private Image cardBackground;
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text[] crownTexts;
        [SerializeField] private Button replayButton;

        private string _stageId;

        internal event Action<string> ReplayRequested;

        internal Text TitleText => titleText;
        internal Text DetailText => detailText;
        internal Text[] CrownTexts => crownTexts;
        internal Button ReplayButton => replayButton;
        internal string StageId => _stageId;

        internal void Configure(
            Image configuredCardBackground,
            Text configuredTitleText,
            Text configuredDetailText,
            Text[] configuredCrownTexts,
            Button configuredReplayButton)
        {
            cardBackground = configuredCardBackground;
            titleText = configuredTitleText;
            detailText = configuredDetailText;
            crownTexts = configuredCrownTexts;
            replayButton = configuredReplayButton;
        }

        private void Awake()
        {
            if (replayButton != null)
            {
                replayButton.onClick.AddListener(RequestReplay);
            }
        }

        private void OnDestroy()
        {
            if (replayButton != null)
            {
                replayButton.onClick.RemoveListener(RequestReplay);
            }
        }

        internal bool HasRequiredReferences()
        {
            if (cardBackground == null || titleText == null ||
                detailText == null || replayButton == null ||
                crownTexts == null ||
                crownTexts.Length != StageCrownResultView.DisplayOrder.Length)
            {
                return false;
            }
            for (int index = 0; index < crownTexts.Length; index++)
            {
                if (crownTexts[index] == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal void Bind(StageCollectionRowModel model, bool replayAvailable)
        {
            _stageId = model.StageId;
            titleText.text = StageCollectionPresentation.FormatTitle(model);
            detailText.text = StageCollectionPresentation.FormatDetail(model);
            replayButton.gameObject.SetActive(model.CanReplay && replayAvailable);

            switch (model.State)
            {
                case StageCollectionRowState.Cleared:
                    cardBackground.color = NeonUiPalette.Signal;
                    titleText.color = NeonUiPalette.TextPrimary;
                    detailText.color = NeonUiPalette.TextPrimary;
                    break;
                case StageCollectionRowState.Next:
                    cardBackground.color = NeonUiPalette.Magenta;
                    titleText.color = NeonUiPalette.TextPrimary;
                    detailText.color = NeonUiPalette.TextPrimary;
                    break;
                default:
                    cardBackground.color = NeonUiPalette.PlateMuted;
                    titleText.color = NeonUiPalette.TextSecondary;
                    detailText.color = NeonUiPalette.TextSecondary;
                    break;
            }

            for (int index = 0; index < crownTexts.Length; index++)
            {
                Text text = crownTexts[index];
                text.text = StageCrownResultView.Labels[index];
                text.color = model.Has(StageCrownResultView.DisplayOrder[index])
                    ? NeonUiPalette.Gold
                    : NeonUiPalette.TextMuted;
            }
        }

        private void RequestReplay()
        {
            if (!string.IsNullOrEmpty(_stageId))
            {
                ReplayRequested?.Invoke(_stageId);
            }
        }
    }
}
