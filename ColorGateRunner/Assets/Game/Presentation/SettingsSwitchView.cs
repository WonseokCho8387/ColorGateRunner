using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class SettingsSwitchView : MonoBehaviour
    {
        [SerializeField] private Image track;
        [SerializeField] private RectTransform knob;
        [SerializeField] private Image knobImage;

        internal Image Track => track;
        internal RectTransform Knob => knob;
        internal bool HasRequiredReferences =>
            track != null && knob != null && knobImage != null;

        internal void Configure(Image switchTrack, Image switchKnob)
        {
            track = switchTrack;
            knobImage = switchKnob;
            knob = switchKnob != null ? switchKnob.rectTransform : null;
        }

        internal void Show(bool on)
        {
            if (!HasRequiredReferences)
            {
                return;
            }

            track.color = on
                ? NeonUiPalette.Magenta
                : NeonUiPalette.PlateMuted;
            knobImage.color = on
                ? NeonUiPalette.TextPrimary
                : NeonUiPalette.TextSecondary;
            knob.anchorMin = new Vector2(on ? 0.54f : 0.06f, 0.14f);
            knob.anchorMax = new Vector2(on ? 0.94f : 0.46f, 0.86f);
            knob.offsetMin = Vector2.zero;
            knob.offsetMax = Vector2.zero;
        }
    }
}
