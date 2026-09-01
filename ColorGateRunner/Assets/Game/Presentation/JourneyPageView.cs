using System;
using System.Collections;
using System.Collections.Generic;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class JourneyPageView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private Text progressText;
        [SerializeField] private JourneyMilestoneView[] milestoneViews;

        private int _focusMilestoneIndex;

        internal RectTransform ContentRoot => contentRoot;
        internal ScrollRect ScrollRect => scrollRect;
        internal JourneyMilestoneView[] MilestoneViews => milestoneViews;

        internal void Configure(
            ScrollRect configuredScrollRect,
            RectTransform configuredContentRoot,
            Text configuredProgressText,
            JourneyMilestoneView[] configuredMilestoneViews)
        {
            scrollRect = configuredScrollRect;
            contentRoot = configuredContentRoot;
            progressText = configuredProgressText;
            milestoneViews = configuredMilestoneViews;
        }

        internal bool HasRequiredReferences()
        {
            if (scrollRect == null || contentRoot == null ||
                scrollRect.content != contentRoot || progressText == null ||
                milestoneViews == null ||
                milestoneViews.Length !=
                    JourneyMilestonePresentation.TotalMilestones)
            {
                return false;
            }
            for (int index = 0; index < milestoneViews.Length; index++)
            {
                if (milestoneViews[index] == null ||
                    !milestoneViews[index].HasRequiredReferences())
                {
                    return false;
                }
            }
            return true;
        }

        internal void Bind(
            ProgressionService progression,
            int availableStageCount)
        {
            if (progression?.Lobby == null)
            {
                throw new ArgumentNullException(nameof(progression));
            }

            int applied = Mathf.Clamp(
                progression.Lobby.AppliedMilestoneCount,
                0,
                JourneyMilestonePresentation.TotalMilestones);
            IReadOnlyList<JourneyMilestoneCardModel> models =
                JourneyMilestonePresentation.Create(applied, availableStageCount);
            _focusMilestoneIndex = Mathf.Clamp(
                applied,
                0,
                JourneyMilestonePresentation.TotalMilestones - 1);
            for (int index = 0; index < milestoneViews.Length; index++)
            {
                milestoneViews[index].Bind(models[index]);
            }
            progressText.text = applied >=
                JourneyMilestonePresentation.TotalMilestones
                ? "18 / 18  JOURNEY COMPLETE"
                : $"{applied} / 18  •  NEXT STAGE {(applied + 1) * 2}";
        }

        internal void OnPageShown()
        {
            if (!HasRequiredReferences())
            {
                return;
            }
            StartCoroutine(FocusNextFrame());
        }

        private IEnumerator FocusNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            scrollRect.verticalNormalizedPosition =
                JourneyMilestonePresentation.TotalMilestones <= 1
                    ? 1f
                    : _focusMilestoneIndex /
                        (float)(JourneyMilestonePresentation.TotalMilestones - 1);
        }
    }

}
