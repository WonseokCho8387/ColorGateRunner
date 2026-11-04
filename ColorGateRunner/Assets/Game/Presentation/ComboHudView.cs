using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // Shows the combo chip and a short celebratory judgment flash (PERFECT,
    // GREAT, GOOD) under the top HUD. The flash is a feeling, not a reading
    // task: a punch-in word with an additive ghost and streak that fades out.
    // It only advances through Tick, so Pause freezes it with gameplay.
    public sealed class ComboHudView : MonoBehaviour
    {
        internal const float FlashSeconds = 0.6f;
        private const float PunchSeconds = 0.14f;
        private const float FadeStartSeconds = 0.34f;
        private const float GhostSeconds = 0.45f;
        private const float StreakGrowSeconds = 0.2f;

        private static readonly Color GreatColor = new Color32(0x3D, 0xF5, 0xFF, 0xFF);
        private static readonly Color GoodColor = new Color32(0xB7, 0xA6, 0xFF, 0xFF);

        [SerializeField] private Text comboText;
        [SerializeField] private RectTransform judgmentRoot;
        [SerializeField] private CanvasGroup judgmentGroup;
        [SerializeField] private Text judgmentText;
        [SerializeField] private Outline judgmentOutline;
        [SerializeField] private Text judgmentGlow;
        [SerializeField] private Image judgmentStreak;

        private int _shownCombo = -1;
        private float _flashElapsed = -1f;
        private ComboStep _shownStep = ComboStep.Ignored;
        private float _size;
        private float _punch;
        private Color _glowColor;

        internal Text ComboText => comboText;
        internal Text JudgmentText => judgmentText;
        internal GameObject JudgmentRoot =>
            judgmentRoot != null ? judgmentRoot.gameObject : null;
        // The grade currently flashing, or Ignored when nothing is shown.
        internal ComboStep ShownStep => _shownStep;

        internal bool HasRequiredReferences =>
            comboText != null && judgmentRoot != null && judgmentGroup != null &&
            judgmentText != null && judgmentOutline != null &&
            judgmentGlow != null && judgmentStreak != null;

        internal void Configure(
            Text combo,
            RectTransform root,
            CanvasGroup group,
            Text judgment,
            Outline outline,
            Text glow,
            Image streak)
        {
            comboText = combo;
            judgmentRoot = root;
            judgmentGroup = group;
            judgmentText = judgment;
            judgmentOutline = outline;
            judgmentGlow = glow;
            judgmentStreak = streak;
            Hide();
        }

        internal void Begin()
        {
            StopFlash();
            ShowCombo(0);
        }

        internal void Apply(ComboStep step, int combo)
        {
            switch (step)
            {
                case ComboStep.Ignored:
                    return;
                case ComboStep.Perfect:
                    StartFlash(step, "PERFECT", NeonUiPalette.Gold,
                        NeonUiPalette.Magenta, 1f, 1.6f);
                    break;
                case ComboStep.Great:
                    StartFlash(step, "GREAT", GreatColor, GreatColor, 0.86f, 1.35f);
                    break;
                case ComboStep.Good:
                    StartFlash(step, "GOOD", GoodColor, GoodColor, 0.72f, 1.15f);
                    break;
                default:
                    // A gate without a grade shows nothing, and a failure
                    // stops Tick, so a lingering flash would sit on the fail
                    // panel; a break always clears it.
                    StopFlash();
                    break;
            }
            ShowCombo(combo);
        }

        internal void Tick(float deltaSeconds)
        {
            if (_flashElapsed < 0f)
            {
                return;
            }
            _flashElapsed += deltaSeconds;
            if (_flashElapsed >= FlashSeconds)
            {
                StopFlash();
                return;
            }
            Animate(_flashElapsed);
        }

        internal void Hide()
        {
            _shownCombo = -1;
            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }
            StopFlash();
        }

        private void StartFlash(
            ComboStep step,
            string label,
            Color color,
            Color glow,
            float size,
            float punch)
        {
            _shownStep = step;
            _size = size;
            _punch = punch;
            _glowColor = glow;
            _flashElapsed = 0f;
            judgmentText.text = label;
            judgmentGlow.text = label;
            // A bright translucent core with a solid grade-colored rim.
            Color core = Color.Lerp(Color.white, color, 0.35f);
            core.a = 0.9f;
            judgmentText.color = core;
            judgmentOutline.effectColor = color;
            judgmentRoot.gameObject.SetActive(true);
            Animate(0f);
        }

        private void StopFlash()
        {
            _flashElapsed = -1f;
            _shownStep = ComboStep.Ignored;
            if (judgmentRoot != null)
            {
                judgmentRoot.gameObject.SetActive(false);
            }
        }

        private void Animate(float elapsed)
        {
            float punchT = Mathf.Clamp01(elapsed / PunchSeconds);
            float punchEase = 1f - ((1f - punchT) * (1f - punchT));
            float scale = _size * Mathf.Lerp(_punch, 1f, punchEase);
            judgmentText.rectTransform.localScale = new Vector3(scale, scale, 1f);

            float ghostT = Mathf.Clamp01(elapsed / GhostSeconds);
            float ghostScale = _size * (1f + (0.7f * (1f - ((1f - ghostT) * (1f - ghostT)))));
            judgmentGlow.rectTransform.localScale = new Vector3(ghostScale, ghostScale, 1f);
            Color ghost = _glowColor;
            ghost.a = 0.75f * (1f - ghostT);
            judgmentGlow.color = ghost;

            float streakT = Mathf.Clamp01(elapsed / StreakGrowSeconds);
            judgmentStreak.rectTransform.localScale = new Vector3(
                _size * (1f - ((1f - streakT) * (1f - streakT))),
                1f,
                1f);
            Color streak = _glowColor;
            streak.a = 0.9f * (1f - (elapsed / FlashSeconds));
            judgmentStreak.color = streak;

            judgmentGroup.alpha = elapsed <= FadeStartSeconds
                ? 1f
                : 1f - ((elapsed - FadeStartSeconds) / (FlashSeconds - FadeStartSeconds));
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
