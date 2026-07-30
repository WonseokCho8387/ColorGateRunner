using System;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class EchoOfferCoordinatorTests
    {
        [Test]
        public void Settings_RejectInvalidRanges()
        {
            Assert.That(
                () => new EchoSettings(
                    true, 0.5f, 0.1f, true, 0.1f, 0.2f, 0.3f, 2, 3),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => new EchoSettings(
                    true, 0.1f, 0.5f, true, 0.05f, 0.2f, 0.3f, 2, 3),
                Throws.TypeOf<ArgumentException>());
            Assert.That(
                () => new EchoSettings(
                    true, 0.1f, 0.5f, true, 0.1f, 0.2f, 1.1f, 2, 3),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void FirstOffer_IsDeterministicAndGuaranteed()
        {
            EchoSettings settings = EchoSettings.CreateDefault();
            EchoOfferCoordinator first =
                new EchoOfferCoordinator(settings, 12345u, 40);
            EchoOfferCoordinator second =
                new EchoOfferCoordinator(settings, 12345u, 40);

            Assert.That(first.FirstOfferGateId, Is.EqualTo(second.FirstOfferGateId));
            Assert.That(first.FirstOfferGateId, Is.InRange(4, 7));
            GateModifier modifier = first.RegisterGate(
                first.FirstOfferGateId,
                first.FirstOfferGateId / 39f,
                false,
                GateModifier.None);
            Assert.That(modifier.IsEchoProvider, Is.True);
        }

        [Test]
        public void PendingOrActiveEcho_BlocksAdditionalOffers()
        {
            EchoOfferCoordinator coordinator =
                new EchoOfferCoordinator(EchoSettings.CreateDefault(), 7u, 40);
            int offerId = coordinator.FirstOfferGateId;
            GateModifier offered = coordinator.RegisterGate(
                offerId,
                offerId / 39f,
                false,
                GateModifier.None);
            Assert.That(offered.IsEchoProvider, Is.True);
            Assert.That(coordinator.EchoOfferPending, Is.True);

            GateModifier whilePending = coordinator.RegisterGate(
                offerId + 1,
                (offerId + 1) / 39f,
                false,
                GateModifier.None);
            Assert.That(whilePending.IsNone, Is.True);

            Assert.That(
                coordinator.TryAcquire(
                    offerId,
                    RunnerColor.Blue,
                    true),
                Is.True);
            GateModifier whileActive = coordinator.RegisterGate(
                offerId + 2,
                (offerId + 2) / 39f,
                false,
                GateModifier.None);
            Assert.That(whileActive.IsNone, Is.True);
        }

        [Test]
        public void AcquireRequiresPlayerMatch_AndStoresEffectiveColor()
        {
            EchoOfferCoordinator coordinator =
                new EchoOfferCoordinator(EchoSettings.CreateDefault(), 9u, 40);
            int offerId = coordinator.FirstOfferGateId;
            coordinator.RegisterGate(
                offerId,
                offerId / 39f,
                false,
                GateModifier.None);

            Assert.That(
                coordinator.TryAcquire(offerId, RunnerColor.Green, false),
                Is.False);
            Assert.That(coordinator.EchoActive, Is.False);
            Assert.That(
                coordinator.TryAcquire(offerId, RunnerColor.Green, true),
                Is.True);
            Assert.That(coordinator.EchoColor, Is.EqualTo(RunnerColor.Green));
            Assert.That(coordinator.EchoAcquisitionCount, Is.EqualTo(1));
        }

        [Test]
        public void EchoConsumption_DoesNotConsumeOnDifferentColor()
        {
            EchoOfferCoordinator coordinator =
                AcquireEcho(RunnerColor.Blue);

            Assert.That(coordinator.TryConsume(RunnerColor.Red), Is.False);
            Assert.That(coordinator.EchoActive, Is.True);
            Assert.That(coordinator.TryConsume(RunnerColor.Blue), Is.True);
            Assert.That(coordinator.EchoActive, Is.False);
        }

        [Test]
        public void ProviderRole_IsLockedUntilGateRelease()
        {
            EchoOfferCoordinator coordinator =
                new EchoOfferCoordinator(EchoSettings.CreateDefault(), 11u, 40);
            int offerId = coordinator.FirstOfferGateId;
            GateModifier modifier = coordinator.RegisterGate(
                offerId,
                offerId / 39f,
                false,
                GateModifier.None);
            Assert.That(modifier.IsEchoProvider, Is.True);

            coordinator.OnGateReleased(offerId);
            Assert.That(coordinator.EchoOfferPending, Is.False);
            Assert.That(coordinator.ActiveEchoOfferGateId, Is.EqualTo(-1));
            Assert.That(
                modifier.IsEchoProvider,
                Is.True,
                "The already-created plan remains an Echo provider.");
        }

        [Test]
        public void GoalNeverBecomesEchoProvider()
        {
            EchoOfferCoordinator coordinator =
                new EchoOfferCoordinator(EchoSettings.CreateDefault(), 13u, 40);
            GateModifier modifier = coordinator.RegisterGate(
                coordinator.FirstOfferGateId,
                coordinator.FirstOfferGateId / 39f,
                true,
                GateModifier.None);
            Assert.That(modifier.IsNone, Is.True);
        }

        [Test]
        public void Restart_ReproducesOfferAndClearsState()
        {
            EchoOfferCoordinator coordinator =
                AcquireEcho(RunnerColor.Green);
            int firstId = coordinator.FirstOfferGateId;

            coordinator.Restart();

            Assert.That(coordinator.FirstOfferGateId, Is.EqualTo(firstId));
            Assert.That(coordinator.EchoActive, Is.False);
            Assert.That(coordinator.EchoOfferPending, Is.False);
            Assert.That(coordinator.EchoAcquisitionCount, Is.Zero);
            Assert.That(coordinator.EchoCooldownRemaining, Is.Zero);
        }

        private static EchoOfferCoordinator AcquireEcho(RunnerColor color)
        {
            EchoOfferCoordinator coordinator =
                new EchoOfferCoordinator(EchoSettings.CreateDefault(), 5u, 40);
            int offerId = coordinator.FirstOfferGateId;
            coordinator.RegisterGate(
                offerId,
                offerId / 39f,
                false,
                GateModifier.None);
            coordinator.TryAcquire(offerId, color, true);
            return coordinator;
        }
    }
}
