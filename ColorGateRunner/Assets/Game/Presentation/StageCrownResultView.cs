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

        // The Combo crown names the Stage's own target, such as "20 COMBO".
        internal static string Label(int index, int comboTarget) =>
            DisplayOrder[index] switch
            {
                StageCrown.Clean => "CLEAN",
                StageCrown.Combo => comboTarget + " COMBO",
                _ => "FULL COMBO"
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
            StageCrown newlyEarned,
            int comboTarget)
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
                    text.text = Label(index, comboTarget) + "\nNEW";
                    text.color = NeonUiPalette.Gold;
                }
                else if ((owned & crown) != 0)
                {
                    text.text = Label(index, comboTarget);
                    text.color = NeonUiPalette.Signal;
                }
                else
                {
                    text.text = Label(index, comboTarget);
                    text.color = NeonUiPalette.TextMuted;
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
