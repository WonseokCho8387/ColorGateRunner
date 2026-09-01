using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class LobbyPagePager : MonoBehaviour
    {
        private const float MinimumSwipeViewportRatio = 0.09f;
        private const float MinimumSwipeVelocity = 720f;
        private const float DirectionLockViewportRatio = 0.025f;

        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform[] pageRoots;
        [SerializeField] private Button[] navigationButtons;
        [SerializeField] private GameObject[] selectionRoots;

        private int _currentIndex = 2;
        private float _visualPage = 2f;
        private float _animationStart;
        private float _animationTarget;
        private float _animationElapsed;
        private float _animationDuration;
        private bool _animating;
        private bool _dragging;
        private bool _horizontalDrag;
        private bool _directionLocked;
        private Vector2 _dragStart;
        private float _dragStartTime;

        internal event Action<FrontendPage> PageRequested;

        internal int CurrentIndex => _currentIndex;
        internal bool IsAnimating => _animating;

        internal void Configure(
            RectTransform configuredViewport,
            RectTransform[] configuredPageRoots,
            Button[] configuredNavigationButtons,
            GameObject[] configuredSelectionRoots)
        {
            viewport = configuredViewport;
            pageRoots = configuredPageRoots;
            navigationButtons = configuredNavigationButtons;
            selectionRoots = configuredSelectionRoots;
        }

        internal bool HasRequiredReferences()
        {
            if (viewport == null || pageRoots == null ||
                navigationButtons == null || selectionRoots == null ||
                pageRoots.Length != 5 || navigationButtons.Length != 5 ||
                selectionRoots.Length != 5)
            {
                return false;
            }

            for (int index = 0; index < 5; index++)
            {
                if (pageRoots[index] == null ||
                    navigationButtons[index] == null ||
                    selectionRoots[index] == null)
                {
                    return false;
                }
            }
            return true;
        }

        internal void SetPage(FrontendPage page, bool animate)
        {
            int targetIndex = LobbyPageOrder.ToIndex(page);
            _currentIndex = targetIndex;
            ApplySelection(targetIndex);

            if (!animate || !gameObject.activeInHierarchy ||
                Mathf.Approximately(_visualPage, targetIndex))
            {
                _animating = false;
                _visualPage = targetIndex;
                ApplyPagePositions();
                return;
            }

            _animationStart = _visualPage;
            _animationTarget = targetIndex;
            _animationElapsed = 0f;
            _animationDuration = Mathf.Min(
                0.32f,
                0.16f + (Mathf.Abs(_animationTarget - _animationStart) * 0.04f));
            _animating = true;
        }

        internal void BeginDrag(PointerEventData eventData)
        {
            if (!HasRequiredReferences() || _animating)
            {
                return;
            }

            _dragging = true;
            _horizontalDrag = false;
            _directionLocked = false;
            _dragStart = eventData.position;
            _dragStartTime = Time.unscaledTime;
        }

        internal void Drag(PointerEventData eventData)
        {
            if (!_dragging || _animating)
            {
                return;
            }

            Vector2 delta = eventData.position - _dragStart;
            float width = Mathf.Max(1f, viewport.rect.width);
            if (!_directionLocked &&
                delta.magnitude >= width * DirectionLockViewportRatio)
            {
                _horizontalDrag = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
                _directionLocked = true;
            }
            if (!_horizontalDrag)
            {
                return;
            }

            float pageOffset = -delta.x / width;
            float candidate = _currentIndex + pageOffset;
            if (candidate < 0f)
            {
                candidate *= 0.22f;
            }
            else if (candidate > 4f)
            {
                candidate = 4f + ((candidate - 4f) * 0.22f);
            }
            _visualPage = candidate;
            ApplyPagePositions();
        }

        internal void EndDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            Vector2 delta = eventData.position - _dragStart;
            float elapsed = Mathf.Max(0.016f, Time.unscaledTime - _dragStartTime);
            float width = Mathf.Max(1f, viewport.rect.width);
            bool qualifies = _horizontalDrag &&
                (Mathf.Abs(delta.x) >= width * MinimumSwipeViewportRatio ||
                 Mathf.Abs(delta.x) / elapsed >= MinimumSwipeVelocity);
            int requestedIndex = _currentIndex;
            if (qualifies)
            {
                requestedIndex += delta.x < 0f ? 1 : -1;
                requestedIndex = Mathf.Clamp(requestedIndex, 0, 4);
            }

            if (requestedIndex == _currentIndex)
            {
                AnimateBackToCurrent();
                return;
            }
            PageRequested?.Invoke(LobbyPageOrder.FromIndex(requestedIndex));
        }

        private void Update()
        {
            if (!_animating)
            {
                return;
            }

            _animationElapsed += Time.unscaledDeltaTime;
            float normalized = _animationDuration <= 0f
                ? 1f
                : Mathf.Clamp01(_animationElapsed / _animationDuration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            _visualPage = Mathf.Lerp(_animationStart, _animationTarget, eased);
            ApplyPagePositions();
            if (normalized >= 1f)
            {
                _animating = false;
                _visualPage = _animationTarget;
                ApplyPagePositions();
            }
        }

        private void OnRectTransformDimensionsChange()
        {
            if (HasRequiredReferences())
            {
                ApplyPagePositions();
            }
        }

        private void AnimateBackToCurrent()
        {
            _animationStart = _visualPage;
            _animationTarget = _currentIndex;
            _animationElapsed = 0f;
            _animationDuration = 0.14f;
            _animating = true;
        }

        private void ApplySelection(int selectedIndex)
        {
            for (int index = 0; index < selectionRoots.Length; index++)
            {
                selectionRoots[index].SetActive(index == selectedIndex);
            }
        }

        private void ApplyPagePositions()
        {
            float width = Mathf.Max(1f, viewport.rect.width);
            for (int index = 0; index < pageRoots.Length; index++)
            {
                RectTransform page = pageRoots[index];
                page.anchoredPosition = new Vector2(
                    (index - _visualPage) * width,
                    page.anchoredPosition.y);
            }
        }
    }

    internal static class LobbyPageOrder
    {
        internal static int ToIndex(FrontendPage page) => page switch
        {
            FrontendPage.Shop => 0,
            FrontendPage.Leaderboard => 1,
            FrontendPage.Lobby => 2,
            FrontendPage.Journey => 3,
            FrontendPage.Collection => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };

        internal static FrontendPage FromIndex(int index) => index switch
        {
            0 => FrontendPage.Shop,
            1 => FrontendPage.Leaderboard,
            2 => FrontendPage.Lobby,
            3 => FrontendPage.Journey,
            4 => FrontendPage.Collection,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}
