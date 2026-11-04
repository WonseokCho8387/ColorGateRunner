using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // Clear-result readout for the run's max combo and the Stage's three
    // crowns. A crown first earned by this saved clear is marked NEW.
    public sealed class StageCrownResultView : MonoBehaviour
    {
        internal static readonly StageCrown[] DisplayOrder =
        {
            StageCrown.Clean,
            StageCrown.Combo,
            StageCrown.FullCombo
        };

        internal static readonly string[] Labels =
        {
            "CLEAN",
            StageCrownPolicy.ComboTarget + " COMBO",
            "FULL COMBO"
        };

        [SerializeField] private Text maxComboText;
        [SerializeField] private Text[] crownTexts;

        internal Text MaxComboText => maxComboText;
        internal Text[] CrownTexts => crownTexts;

        internal bool HasRequiredReferences
        {
            get
            {
                if (maxComboText == null || crownTexts == null ||
                    crownTexts.Length != DisplayOrder.Length)
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
        }

        internal void Configure(Text maxCombo, Text[] crowns)
        {
            maxComboText = maxCombo;
            crownTexts = crowns;
            Hide();
        }

        internal void Show(
            int maxCombo,
            int bestMaxCombo,
            StageCrown owned,
            StageCrown newlyEarned)
        {
            maxComboText.gameObject.SetActive(true);
            maxComboText.text = bestMaxCombo > maxCombo
                ? $"MAX COMBO {maxCombo}  ·  BEST {bestMaxCombo}"
                : $"MAX COMBO {maxCombo}";
            for (int index = 0; index < crownTexts.Length; index++)
            {
                StageCrown crown = DisplayOrder[index];
                Text text = crownTexts[index];
                text.gameObject.SetActive(true);
                if ((newlyEarned & crown) != 0)
                {
                    text.text = Labels[index] + "\nNEW";
                    text.color = NeonUiPalette.Gold;
                }
                else if ((owned & crown) != 0)
                {
                    text.text = Labels[index];
                    text.color = NeonUiPalette.Signal;
                }
                else
                {
                    text.text = Labels[index];
                    text.color = NeonUiPalette.WithAlpha(
                        NeonUiPalette.TextSecondary,
                        0.45f);
                }
            }
        }

        internal void Hide()
        {
            if (maxComboText != null)
            {
                maxComboText.gameObject.SetActive(false);
            }
            if (crownTexts == null)
            {
                return;
            }
            for (int index = 0; index < crownTexts.Length; index++)
            {
                if (crownTexts[index] != null)
                {
                    crownTexts[index].gameObject.SetActive(false);
                }
            }
        }
    }
}
