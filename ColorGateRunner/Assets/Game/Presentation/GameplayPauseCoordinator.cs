using System;
using ColorGateRunner.Core;

namespace ColorGateRunner.Presentation
{
    public enum GameplayPauseModal
    {
        None = 0,
        Settings = 1,
        RestartConfirmation = 2,
        LeaveConfirmation = 3,
        SceneLoadError = 4
    }

    public enum GameplayPauseBackResult
    {
        Ignored = 0,
        Paused = 1,
        Resumed = 2,
        ModalClosed = 3
    }

    public sealed class GameplayPauseCoordinator
    {
        public bool IsPaused { get; private set; }
        public bool IsTransitioning { get; private set; }
        public GameplayPauseModal CurrentModal { get; private set; }

        public bool TryPause(StageFlowState flowState)
        {
            if (IsTransitioning || !IsPauseAllowed(flowState))
            {
                return false;
            }
            if (IsPaused)
            {
                return false;
            }

            IsPaused = true;
            return true;
        }

        public bool TryResume()
        {
            if (!IsPaused || IsTransitioning ||
                CurrentModal != GameplayPauseModal.None)
            {
                return false;
            }

            IsPaused = false;
            return true;
        }

        public bool TryShowModal(GameplayPauseModal modal)
        {
            if (modal == GameplayPauseModal.None)
            {
                throw new ArgumentOutOfRangeException(nameof(modal));
            }
            if (!IsPaused || IsTransitioning ||
                CurrentModal != GameplayPauseModal.None)
            {
                return false;
            }

            CurrentModal = modal;
            return true;
        }

        public bool TryCloseModal()
        {
            if (!IsPaused || IsTransitioning ||
                CurrentModal == GameplayPauseModal.None)
            {
                return false;
            }

            CurrentModal = GameplayPauseModal.None;
            return true;
        }

        public bool TryBeginTransition()
        {
            if (!IsPaused || IsTransitioning ||
                CurrentModal != GameplayPauseModal.LeaveConfirmation)
            {
                return false;
            }

            CurrentModal = GameplayPauseModal.None;
            IsTransitioning = true;
            return true;
        }

        public void CompleteTransition(bool succeeded)
        {
            if (!IsTransitioning)
            {
                return;
            }

            IsTransitioning = false;
            if (!succeeded)
            {
                CurrentModal = GameplayPauseModal.SceneLoadError;
            }
        }

        public void Clear()
        {
            IsPaused = false;
            IsTransitioning = false;
            CurrentModal = GameplayPauseModal.None;
        }

        public GameplayPauseBackResult HandleBack(StageFlowState flowState)
        {
            if (IsTransitioning)
            {
                return GameplayPauseBackResult.Ignored;
            }
            if (CurrentModal != GameplayPauseModal.None)
            {
                TryCloseModal();
                return GameplayPauseBackResult.ModalClosed;
            }
            if (IsPaused)
            {
                return TryResume()
                    ? GameplayPauseBackResult.Resumed
                    : GameplayPauseBackResult.Ignored;
            }

            return TryPause(flowState)
                ? GameplayPauseBackResult.Paused
                : GameplayPauseBackResult.Ignored;
        }

        public static bool IsPauseAllowed(StageFlowState flowState)
        {
            return flowState == StageFlowState.Countdown ||
                flowState == StageFlowState.Playing ||
                flowState == StageFlowState.ShieldRecovery;
        }
    }
}
