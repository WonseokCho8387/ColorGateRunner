using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed partial class FrontendSceneController
    {
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private TutorialCardView howToPlayCard;

        internal Button HowToPlayButton => howToPlayButton;
        internal TutorialCardView HowToPlayCard => howToPlayCard;

        internal void ConfigureHowToPlay(Button button, TutorialCardView card)
        {
            howToPlayButton = button;
            howToPlayCard = card;
        }

        private void ApplyHowToPlay()
        {
            if (howToPlayButton == null || howToPlayCard == null) return;
            howToPlayButton.onClick.RemoveListener(ShowHowToPlay);
            howToPlayButton.onClick.AddListener(ShowHowToPlay);
        }

        internal void ShowHowToPlay()
        {
            howToPlayCard.Show(
                TutorialCards.HowToPlay(StageCrownPolicy.NormalComboTarget));
        }

        private bool TryCloseHowToPlay()
        {
            if (howToPlayCard == null || !howToPlayCard.IsOpen) return false;
            howToPlayCard.Close();
            return true;
        }
    }
}
