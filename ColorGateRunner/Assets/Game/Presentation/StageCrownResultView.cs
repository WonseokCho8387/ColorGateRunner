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
        // A newly earned crown is full color, an owned one slightly dimmed,
        // and a missing one a dark silhouette.
        internal static readonly Color NewIconTint = Color.white;
        internal static readonly Color OwnedIconTint =
            new Color(0.78f, 0.78f, 0.78f, 0.9f);
        internal static readonly Color MissingIconTint =
            new Color(0.26f, 0.29f, 0.38f, 0.55f);

        [SerializeField] private Text[] crownTexts;
        [SerializeField] private Image[] crownIcons;

        internal Text MaxComboText => maxComboText;
        internal Text[] CrownTexts => crownTexts;
        internal Image[] CrownIcons => crownIcons;

        internal bool HasRequiredReferences
        {
            get
            {
                if (maxComboText == null || crownTexts == null ||
                    crownTexts.Length != DisplayOrder.Length ||
                    crownIcons == null ||
                    crownIcons.Length != DisplayOrder.Length)
                {
                    return false;
                }
                for (int index = 0; index < crownTexts.Length; index++)
                {
                    if (crownTexts[index] == null || crownIcons[index] == null ||
                        crownIcons[index].sprite == null)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal void Configure(Text maxCombo, Text[] crowns, Image[] icons)
        {
            maxComboText = maxCombo;
            crownTexts = crowns;
            crownIcons = icons;
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
                Image icon = crownIcons[index];
                text.gameObject.SetActive(true);
                icon.gameObject.SetActive(true);
                if ((newlyEarned & crown) != 0)
                {
                    text.text = Label(index, comboTarget) + "\nNEW";
                    text.color = NeonUiPalette.Gold;
                    icon.color = NewIconTint;
                }
                else if ((owned & crown) != 0)
                {
                    text.text = Label(index, comboTarget);
                    text.color = NeonUiPalette.Signal;
                    icon.color = OwnedIconTint;
                }
                else
                {
                    text.text = Label(index, comboTarget);
                    text.color = NeonUiPalette.TextMuted;
                    icon.color = MissingIconTint;
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
                if (crownIcons != null && index < crownIcons.Length &&
                    crownIcons[index] != null)
                {
                    crownIcons[index].gameObject.SetActive(false);
                }
            }
        }
    }
}
