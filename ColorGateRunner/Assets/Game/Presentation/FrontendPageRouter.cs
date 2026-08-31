using System;

namespace ColorGateRunner.Presentation
{
    public enum FrontendPage
    {
        AccountChoice = 0,
        Lobby = 1,
        Shop = 2
    }

    public enum FrontendModal
    {
        None = 0,
        AccountUnavailable = 1,
        Settings = 2,
        ExitConfirmation = 3,
        BootRequired = 4,
        SceneLoadError = 5,
        SaveError = 6
    }

    public enum FrontendBackResult
    {
        Ignored = 0,
        ModalClosed = 1,
        ExitConfirmationRequested = 2,
        ReturnedToLobby = 3
    }

    public sealed class FrontendPageRouter
    {
        public FrontendPageRouter(
            FrontendPage initialPage = FrontendPage.AccountChoice)
        {
            if (!IsValidPage(initialPage))
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
            if (!IsValidPage(page))
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
            if (CurrentPage == FrontendPage.Shop)
            {
                return TryShowPage(FrontendPage.Lobby)
                    ? FrontendBackResult.ReturnedToLobby
                    : FrontendBackResult.Ignored;
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

        private static bool IsValidPage(FrontendPage page) =>
            page == FrontendPage.AccountChoice ||
            page == FrontendPage.Lobby ||
            page == FrontendPage.Shop;
    }
}
