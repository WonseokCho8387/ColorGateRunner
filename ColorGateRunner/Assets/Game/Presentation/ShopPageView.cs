using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class ShopPageView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;

        internal RectTransform ContentRoot => contentRoot;
        internal ScrollRect ScrollRect => scrollRect;

        internal void Configure(
            ScrollRect configuredScrollRect,
            RectTransform configuredContentRoot)
        {
            scrollRect = configuredScrollRect;
            contentRoot = configuredContentRoot;
        }

        internal bool HasRequiredReferences() =>
            scrollRect != null && contentRoot != null &&
            scrollRect.content == contentRoot;

        private void OnEnable()
        {
            RefreshLayout();
            StartCoroutine(RefreshLayoutNextFrame());
        }

        internal void OnPageShown()
        {
            RefreshLayout();
            StartCoroutine(RefreshLayoutNextFrame());
        }

        private IEnumerator RefreshLayoutNextFrame()
        {
            yield return null;
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
