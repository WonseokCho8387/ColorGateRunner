using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CamouflageEtaTests
    {
        [Test]
        public void FasterSpeed_RevealsFromFartherDistanceAtSameLeadTime()
        {
            CamouflageSettings settings =
                CamouflageSettings.CreateDefault();
            float distance = settings.RevealLeadTimeSeconds * 20f;

            float fastEta = GateEtaEstimator.EstimateSeconds(distance, 20f);
            float slowEta = GateEtaEstimator.EstimateSeconds(distance, 10f);

            Assert.That(
                GateEtaEstimator.ShouldStartReveal(fastEta, settings),
                Is.True);
            Assert.That(
                GateEtaEstimator.ShouldStartReveal(slowEta, settings),
                Is.False);
        }

        [Test]
        public void Booster_RevealsEarlierInDistanceThanNormalSpeed()
        {
            CamouflageSettings settings =
                CamouflageSettings.CreateDefault();
            float distance =
                settings.RevealLeadTimeSeconds *
                ExperimentItemRules.BoosterSpeed;

            Assert.That(
                GateEtaEstimator.ShouldStartReveal(
                    GateEtaEstimator.EstimateSeconds(
                        distance,
                        ExperimentItemRules.BoosterSpeed),
                    settings),
                Is.True);
            Assert.That(
                GateEtaEstimator.ShouldStartReveal(
                    GateEtaEstimator.EstimateSeconds(distance, 22f),
                    settings),
                Is.False);
        }

        [Test]
        public void IceEffectiveSpeed_IsIncludedInEta()
        {
            ExperimentDefinition definition = ExperimentCatalog.Get(
                4,
                MechanicExperimentType.Ice);
            float baseSpeed = 20f;
            float distance = 30f;
            float normalEta =
                GateEtaEstimator.EstimateSeconds(distance, baseSpeed);
            float iceEta = GateEtaEstimator.EstimateSeconds(
                distance,
                baseSpeed * definition.IceSpeedMultiplier);

            Assert.That(iceEta, Is.LessThan(normalEta));
        }

        [Test]
        public void StoppedFlow_DoesNotRevealDistantGate()
        {
            CamouflageSettings settings =
                CamouflageSettings.CreateDefault();
            float eta = GateEtaEstimator.EstimateSeconds(20f, 0f);

            Assert.That(float.IsPositiveInfinity(eta), Is.True);
            Assert.That(
                GateEtaEstimator.ShouldStartReveal(eta, settings),
                Is.False);
        }
    }
}
