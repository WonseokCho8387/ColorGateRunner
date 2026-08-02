using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class GameplayPauseCoordinatorTests
    {
        [TestCase(StageFlowState.Countdown, true)]
        [TestCase(StageFlowState.Playing, true)]
        [TestCase(StageFlowState.ShieldRecovery, true)]
        [TestCase(StageFlowState.Lobby, false)]
        [TestCase(StageFlowState.Failed, false)]
        [TestCase(StageFlowState.StageCleared, false)]
        public void PauseAvailability_MatchesAttemptStates(
            StageFlowState state,
            bool expected)
        {
            Assert.That(
                GameplayPauseCoordinator.IsPauseAllowed(state),
                Is.EqualTo(expected));
        }

        [Test]
        public void PauseResumeAndRepeatedRequests_AreIdempotent()
        {
            var coordinator = new GameplayPauseCoordinator();

            Assert.That(
                coordinator.TryPause(StageFlowState.Playing),
                Is.True);
            Assert.That(
                coordinator.TryPause(StageFlowState.Playing),
                Is.False);
            Assert.That(coordinator.IsPaused, Is.True);
            Assert.That(coordinator.TryResume(), Is.True);
            Assert.That(coordinator.TryResume(), Is.False);
            Assert.That(coordinator.IsPaused, Is.False);
        }

        [Test]
        public void Back_ClosesModalThenResumesThenPauses()
        {
            var coordinator = new GameplayPauseCoordinator();
            coordinator.TryPause(StageFlowState.Playing);
            coordinator.TryShowModal(GameplayPauseModal.Settings);

            Assert.That(
                coordinator.HandleBack(StageFlowState.Playing),
                Is.EqualTo(GameplayPauseBackResult.ModalClosed));
            Assert.That(coordinator.IsPaused, Is.True);
            Assert.That(
                coordinator.HandleBack(StageFlowState.Playing),
                Is.EqualTo(GameplayPauseBackResult.Resumed));
            Assert.That(
                coordinator.HandleBack(StageFlowState.Playing),
                Is.EqualTo(GameplayPauseBackResult.Paused));
        }

        [Test]
        public void Transition_BlocksResumeUntilCompletionAndReportsFailureModal()
        {
            var coordinator = new GameplayPauseCoordinator();
            coordinator.TryPause(StageFlowState.Playing);
            coordinator.TryShowModal(
                GameplayPauseModal.LeaveConfirmation);

            Assert.That(coordinator.TryBeginTransition(), Is.True);
            Assert.That(coordinator.TryResume(), Is.False);
            coordinator.CompleteTransition(false);

            Assert.That(coordinator.IsPaused, Is.True);
            Assert.That(coordinator.IsTransitioning, Is.False);
            Assert.That(
                coordinator.CurrentModal,
                Is.EqualTo(GameplayPauseModal.SceneLoadError));
        }

        [Test]
        public void BoosterHaptic_DoesNotInvokeDeviceWhenVibrationIsDisabled()
        {
            int requests = 0;
            var disabled = new UnityHapticFeedback(
                () => false,
                () => requests++);
            var enabled = new UnityHapticFeedback(
                () => true,
                () => requests++);

            disabled.RequestBoosterLaunch();
            enabled.RequestBoosterLaunch();

            Assert.That(requests, Is.EqualTo(1));
        }
    }
}
