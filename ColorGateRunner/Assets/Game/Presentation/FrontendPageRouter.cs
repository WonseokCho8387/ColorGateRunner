using System;

namespace ColorGateRunner.Presentation
{
    public enum FrontendPage
    {
        Title = 0,
        Lobby = 1
    }

    public enum FrontendModal
    {
        None = 0,
        AccountUnavailable = 1,
        Settings = 2,
        ExitConfirmation = 3,
        BootRequired = 4,
        SceneLoadError = 5
    }

    public enum FrontendBackResult
    {
        Ignored = 0,
        ModalClosed = 1,
        NavigatedToTitle = 2,
        ExitConfirmationRequested = 3
    }

    public sealed class FrontendPageRouter
    {
        public FrontendPageRouter(
            FrontendPage initialPage = FrontendPage.Title)
        {
            if (initialPage != FrontendPage.Title &&
                initialPage != FrontendPage.Lobby)
            {
                throw new ArgumentOutOfRangeException(nameof(initialPage));
            }

            CurrentPage = initialPage;
        }

        public FrontendPage CurrentPage { get; private set; }
        public FrontendModal CurrentModal { get; private set; }
        public bool IsTransitioning { get; private set; }

        public event Action<FrontendPage> PageEntered;
        public event Action<FrontendPage> PageExited;
        public event Action<FrontendModal> ModalChanged;
        public event Action<bool> TransitionChanged;

        public bool TryShowPage(FrontendPage page)
        {
            if (page != FrontendPage.Title && page != FrontendPage.Lobby)
            {
                throw new ArgumentOutOfRangeException(nameof(page));
            }
            if (IsTransitioning || CurrentModal != FrontendModal.None)
            {
                return false;
            }
            if (CurrentPage == page)
            {
                return true;
            }

            SetTransitioning(true);
            FrontendPage previous = CurrentPage;
            PageExited?.Invoke(previous);
            CurrentPage = page;
            PageEntered?.Invoke(CurrentPage);
            SetTransitioning(false);
            return true;
        }

        public bool TryBeginSceneTransition()
        {
            if (IsTransitioning || CurrentModal != FrontendModal.None)
            {
                return false;
            }

            SetTransitioning(true);
            return true;
        }

        public void CompleteSceneTransition()
        {
            SetTransitioning(false);
        }

        public bool TryShowModal(FrontendModal modal)
        {
            if (modal == FrontendModal.None)
            {
                throw new ArgumentOutOfRangeException(nameof(modal));
            }
            if (IsTransitioning)
            {
                return false;
            }
            if (CurrentModal == modal)
            {
                return true;
            }
            if (CurrentModal != FrontendModal.None)
            {
                return false;
            }

            CurrentModal = modal;
            ModalChanged?.Invoke(CurrentModal);
            return true;
        }

        public bool CloseModal()
        {
            if (IsTransitioning || CurrentModal == FrontendModal.None)
            {
                return false;
            }

            CurrentModal = FrontendModal.None;
            ModalChanged?.Invoke(CurrentModal);
            return true;
        }

        public FrontendBackResult HandleBack()
        {
            if (IsTransitioning)
            {
                return FrontendBackResult.Ignored;
            }
            if (CurrentModal != FrontendModal.None)
            {
                return IsCurrentModalCancellable() && CloseModal()
                    ? FrontendBackResult.ModalClosed
                    : FrontendBackResult.Ignored;
            }
            if (CurrentPage == FrontendPage.Lobby)
            {
                TryShowPage(FrontendPage.Title);
                return FrontendBackResult.NavigatedToTitle;
            }

            TryShowModal(FrontendModal.ExitConfirmation);
            return FrontendBackResult.ExitConfirmationRequested;
        }

        public bool IsCurrentModalCancellable()
        {
            return CurrentModal != FrontendModal.None &&
                CurrentModal != FrontendModal.BootRequired;
        }

        private void SetTransitioning(bool value)
        {
            if (IsTransitioning == value)
            {
                return;
            }

            IsTransitioning = value;
            TransitionChanged?.Invoke(IsTransitioning);
        }
    }
}
