using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // Shows the Perfect Switch combo chip and a short PERFECT flash. The
    // flash only advances through Tick, so Pause freezes it with gameplay.
    public sealed class ComboHudView : MonoBehaviour
    {
        internal const float PerfectFlashSeconds = 0.45f;

        [SerializeField] private Text comboText;
        [SerializeField] private Text perfectText;
        [SerializeField] private GameObject perfectPlate;

        private int _shownCombo = -1;
        private float _flashRemaining;

        internal Text ComboText => comboText;
        internal Text PerfectText => perfectText;
        internal GameObject PerfectPlate => perfectPlate;

        internal bool HasRequiredReferences =>
            comboText != null && perfectText != null;

        internal void Configure(Text combo, Text perfect, GameObject plate = null)
        {
            comboText = combo;
            perfectText = perfect;
            perfectPlate = plate;
            Hide();
        }

        internal void Begin()
        {
            _flashRemaining = 0f;
            SetPerfectVisible(false);
            ShowCombo(0);
        }

        internal void Apply(ComboStep step, int combo)
        {
            if (step == ComboStep.Ignored)
            {
                return;
            }
            if (step == ComboStep.Perfect)
            {
                _flashRemaining = PerfectFlashSeconds;
                SetPerfectVisible(true);
            }
            else if (step == ComboStep.Broken)
            {
                // A failure stops Tick, so a lingering flash would sit on
                // the fail panel; a break always clears it.
                _flashRemaining = 0f;
                SetPerfectVisible(false);
            }
            ShowCombo(combo);
        }

        internal void Tick(float deltaSeconds)
        {
            if (_flashRemaining <= 0f)
            {
                return;
            }
            _flashRemaining -= deltaSeconds;
            if (_flashRemaining <= 0f)
            {
                _flashRemaining = 0f;
                SetPerfectVisible(false);
            }
        }

        internal void Hide()
        {
            _shownCombo = -1;
            _flashRemaining = 0f;
            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }
            if (perfectText != null)
            {
                SetPerfectVisible(false);
            }
        }

        // The flash and its opaque plate always appear together.
        private void SetPerfectVisible(bool visible)
        {
            perfectText.gameObject.SetActive(visible);
            if (perfectPlate != null)
            {
                perfectPlate.SetActive(visible);
            }
        }

        private void ShowCombo(int combo)
        {
            if (combo == _shownCombo)
            {
                return;
            }
            _shownCombo = combo;
            comboText.gameObject.SetActive(combo > 0);
            if (combo > 0)
            {
                comboText.text = combo + " COMBO";
            }
        }
    }
}
