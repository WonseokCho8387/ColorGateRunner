using System;
using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // A modal rule card with optional pages (NEXT ... GOT IT). Used for HOW
    // TO PLAY, the first-clear crown card and new-mechanic intros.
    public sealed class TutorialCardView : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Text pageLabel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Text primaryLabel;
        private TutorialCard[] _pages = Array.Empty<TutorialCard>();
        private int _page;
        private Action _closed;

        internal bool IsOpen => gameObject.activeSelf;
        internal int Page => _page;
        internal int PageCount => _pages.Length;
        internal Text Title => title;
        internal Text Body => body;
        internal Button PrimaryButton => primaryButton;
        internal bool HasRequiredReferences =>
            title != null && body != null && pageLabel != null &&
            primaryButton != null && primaryLabel != null;

        internal void Configure(
            Text heading,
            Text details,
            Text page,
            Button primary,
            Text primaryText)
        {
            title = heading;
            body = details;
            pageLabel = page;
            primaryButton = primary;
            primaryLabel = primaryText;
        }

        internal void Show(TutorialCard[] pages, Action closed = null)
        {
            if (pages == null || pages.Length == 0) return;
            _pages = pages;
            _page = 0;
            _closed = closed;
            primaryButton.onClick.RemoveListener(Advance);
            primaryButton.onClick.AddListener(Advance);
            // Draw order is fixed by the Builder, under loading/blocker roots.
            gameObject.SetActive(true);
            Render();
        }

        internal void Advance()
        {
            if (!IsOpen) return;
            if (_page < _pages.Length - 1)
            {
                _page++;
                Render();
                return;
            }
            Close();
        }

        internal void Close()
        {
            gameObject.SetActive(false);
            Action closed = _closed;
            _closed = null;
            closed?.Invoke();
        }

        private void Render()
        {
            TutorialCard card = _pages[_page];
            title.text = card.Title;
            body.text = card.Body;
            pageLabel.text = _pages.Length > 1
                ? $"{_page + 1} / {_pages.Length}"
                : string.Empty;
            primaryLabel.text = _page < _pages.Length - 1 ? "NEXT" : "GOT IT";
        }

        private void OnDestroy()
        {
            if (primaryButton != null) primaryButton.onClick.RemoveListener(Advance);
        }
    }
}
