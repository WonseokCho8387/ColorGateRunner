using System;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class StageCollectionPageView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private Text crownTotalText;
        [SerializeField] private Text breakdownText;
        [SerializeField] private Text comebackText;
        [SerializeField] private StageCollectionRowView[] rowViews;

        internal event Action<string> ReplayRequested;

        internal ScrollRect ScrollRect => scrollRect;
        internal Text CrownTotalText => crownTotalText;
        internal Text BreakdownText => breakdownText;
        internal Text ComebackText => comebackText;
        internal StageCollectionRowView[] RowViews => rowViews;

        internal void Configure(
            ScrollRect configuredScrollRect,
            RectTransform configuredContentRoot,
            Text configuredCrownTotalText,
            Text configuredBreakdownText,
            Text configuredComebackText,
            StageCollectionRowView[] configuredRowViews)
        {
            scrollRect = configuredScrollRect;
            contentRoot = configuredContentRoot;
            crownTotalText = configuredCrownTotalText;
            breakdownText = configuredBreakdownText;
            comebackText = configuredComebackText;
            rowViews = configuredRowViews;
        }

        private void Awake()
        {
            SetRowListeners(true);
        }

        private void OnDestroy()
        {
            SetRowListeners(false);
        }

        internal bool HasRequiredReferences()
        {
            if (scrollRect == null || contentRoot == null ||
                scrollRect.content != contentRoot ||
                crownTotalText == null || breakdownText == null ||
                comebackText == null || rowViews == null ||
                rowViews.Length == 0)
            {
                return false;
            }
            for (int index = 0; index < rowViews.Length; index++)
            {
                if (rowViews[index] == null ||
                    !rowViews[index].HasRequiredReferences())
                {
                    return false;
                }
            }
            return true;
        }

        internal void Bind(StageCollectionModel model, bool replayAvailable)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (model.StageCount != rowViews.Length)
            {
                Debug.LogError(
                    $"Collection has {rowViews.Length} rows for " +
                    $"{model.StageCount} stages; rebuild the Frontend Scene.");
            }

            crownTotalText.text = StageCollectionPresentation.FormatCrownTotal(model);
            breakdownText.text = StageCollectionPresentation.FormatBreakdown(model);
            comebackText.text = StageCollectionPresentation.FormatComeback(model);
            for (int index = 0; index < rowViews.Length; index++)
            {
                bool bound = index < model.StageCount;
                rowViews[index].gameObject.SetActive(bound);
                if (bound)
                {
                    rowViews[index].Bind(model.Rows[index], replayAvailable);
                }
            }
        }

        internal void OnPageShown()
        {
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void SetRowListeners(bool add)
        {
            if (rowViews == null)
            {
                return;
            }
            for (int index = 0; index < rowViews.Length; index++)
            {
                if (rowViews[index] == null)
                {
                    continue;
                }
                if (add)
                {
                    rowViews[index].ReplayRequested += ForwardReplay;
                }
                else
                {
                    rowViews[index].ReplayRequested -= ForwardReplay;
                }
            }
        }

        private void ForwardReplay(string stageId)
        {
            ReplayRequested?.Invoke(stageId);
        }
    }
}
