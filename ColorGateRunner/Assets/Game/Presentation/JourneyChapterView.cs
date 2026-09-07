using System;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class JourneyChapterView : MonoBehaviour
    {
        [SerializeField] private string themeId;
        [SerializeField] private int chapterIndex;
        [SerializeField] private Image previewImage;
        [SerializeField] private Text titleText;
        [SerializeField] private Text requirementText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Text selectButtonText;

        internal event Action<string> SelectionRequested;
        internal string ThemeId => themeId;
        internal Button SelectButton => selectButton;
        internal Text SelectButtonText => selectButtonText;

        private void Awake()
        {
            selectButton.onClick.AddListener(RequestSelection);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(RequestSelection);
            }
        }

        internal void Configure(
            string configuredThemeId,
            int configuredChapterIndex,
            Image preview,
            Text title,
            Text requirement,
            Button button,
            Text buttonText)
        {
            themeId = configuredThemeId;
            chapterIndex = configuredChapterIndex;
            previewImage = preview;
            titleText = title;
            requirementText = requirement;
            selectButton = button;
            selectButtonText = buttonText;
        }

        internal void Bind(int appliedMilestones, string selectedThemeId)
        {
            bool unlocked = LobbyChapterPolicy.IsUnlocked(
                themeId,
                appliedMilestones);
            bool selected = string.Equals(
                themeId,
                selectedThemeId,
                StringComparison.Ordinal);
            titleText.text = JourneyMilestonePresentation.GetChapterName(
                chapterIndex);
            int requiredStage = LobbyChapterPolicy.GetRequiredStage(
                chapterIndex);
            requirementText.text = unlocked
                ? $"CHAPTER {chapterIndex + 1} UNLOCKED"
                : $"CLEAR STAGE {requiredStage}";
            selectButton.interactable = unlocked && !selected;
            selectButtonText.text = !unlocked
                ? $"LOCKED · CLEAR STAGE {requiredStage}"
                : selected ? "IN USE" : "USE LOBBY";
            Color color = previewImage.color;
            color.a = unlocked ? 1f : 0.34f;
            previewImage.color = color;
        }

        internal void SetVerticalPosition(float centerY, float halfHeight)
        {
            RectTransform rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(
                rect.anchorMin.x,
                centerY - halfHeight);
            rect.anchorMax = new Vector2(
                rect.anchorMax.x,
                centerY + halfHeight);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal bool HasRequiredReferences()
        {
            return LobbyChapterPolicy.IsKnownTheme(themeId) &&
                chapterIndex >= 0 &&
                chapterIndex < LobbyChapterPolicy.ChapterCount &&
                previewImage != null && titleText != null &&
                requirementText != null && selectButton != null &&
                selectButtonText != null;
        }

        private void RequestSelection()
        {
            SelectionRequested?.Invoke(themeId);
        }
    }

}
