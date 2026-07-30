using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignMechanicStageTests
    {
        [Test]
        public void StagesSixThroughEleven_UseOnePrimaryMechanicEach()
        {
            StagePrimaryMechanic[] expected =
            {
                StagePrimaryMechanic.Shield,
                StagePrimaryMechanic.Booster,
                StagePrimaryMechanic.Camouflage,
                StagePrimaryMechanic.Fog,
                StagePrimaryMechanic.Ice,
                StagePrimaryMechanic.Echo
            };

            for (int index = 0; index < expected.Length; index++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(index + 6);
                Assert.That(stage.PrimaryMechanic, Is.EqualTo(expected[index]));
                Assert.That(
                    stage.AllowedColorCount,
                    Is.EqualTo(index == expected.Length - 1 ? 3 : 2));
            }
        }

        [Test]
        public void ShieldStage_ProvidesLocalChargeAndDisablesDuplicateItem()
        {
            StageSession session = CreatePlaying(
                6,
                new StartItemSelection(true, false));

            Assert.That(session.StageProvidesShield, Is.True);
            Assert.That(session.MechanicGrantActivated, Is.True);
            Assert.That(session.ShieldActive, Is.True);
            Assert.That(session.Items.Shield, Is.False);
        }

        [Test]
        public void BoosterStage_ActivatesLocalGrantAtConfiguredProgress()
        {
            StageSession session = CreatePlaying(7, default);
            Assert.That(session.BoosterActive, Is.False);

            while (session.Progress <
                session.Stage.MechanicGrantSettings.ActivationProgress)
            {
                GatePlan plan = session.GetNextGatePlan();
                MatchCurrentColor(session, plan.Color);
                Assert.That(
                    session.ResolveGate(plan),
                    Is.EqualTo(GateOutcome.Matched));
            }

            Assert.That(session.MechanicGrantActivated, Is.True);
            Assert.That(session.BoosterActive, Is.True);
            Assert.That(
                session.BoosterDistanceRemaining,
                Is.EqualTo(session.Stage.BoosterDistance));
        }

        [Test]
        public void GateModifiers_AreDeterministicAndStayInsideAuthoredStages()
        {
            AssertModifierExists(8, GateModifierType.Camouflage);
            AssertModifierExists(9, GateModifierType.Fog);
            AssertModifierExists(10, GateModifierType.Ice);

            for (int stageNumber = 1; stageNumber <= 7; stageNumber++)
            {
                DeterministicStageGateSequence sequence =
                    new DeterministicStageGateSequence(
                        StageCatalog.GetByDisplayNumber(stageNumber));
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(stageNumber);
                for (int gate = 0; gate < stage.TargetGateCount; gate++)
                {
                    Assert.That(
                        sequence.GetPlan(gate).Modifier.IsNone,
                        Is.True);
                }
            }
        }

        [Test]
        public void Ice_UsesSharedSpeedAndSpacingMultipliers()
        {
            StageSession session = CreatePlaying(10, default);
            GatePlan icePlan = FindModifierPlan(
                session,
                GateModifierType.Ice);

            float baseSpacing =
                session.Stage.GetBaseSpeed(
                    icePlan.GateId /
                    (float)(session.Stage.TargetGateCount - 1)) *
                icePlan.TimeToGate;
            Assert.That(
                icePlan.Spacing,
                Is.EqualTo(
                    baseSpacing *
                    GateModifierRules.IceSpacingMultiplier)
                    .Within(0.001f));
            Assert.That(
                session.GetSpeedForPlan(icePlan),
                Is.EqualTo(
                    session.CurrentSpeed *
                    GateModifierRules.IceSpeedMultiplier)
                    .Within(0.001f));
        }

        [Test]
        public void EchoStage_AcquiresConsumesAndRestartsDeterministically()
        {
            StageSession session = CreatePlaying(11, default);
            int firstOffer = AcquireFirstEcho(session);
            RunnerColor echoColor = session.EchoColor;

            GateOutcome echoOutcome = GateOutcome.Ignored;
            while (session.GatesPassed < session.Stage.TargetGateCount)
            {
                GatePlan plan = session.GetNextGatePlan();
                if (plan.Color == echoColor)
                {
                    while (session.CurrentColor == echoColor)
                    {
                        session.TryToggleColor();
                    }
                    echoOutcome = session.ResolveGate(plan);
                    break;
                }

                MatchCurrentColor(session, plan.Color);
                session.ResolveGate(plan);
            }

            Assert.That(echoOutcome, Is.EqualTo(GateOutcome.Echoed));
            Assert.That(session.EchoActive, Is.False);

            session.RetryToSelection();
            session.BeginCountdown();
            session.CompleteCountdown();
            Assert.That(AcquireFirstEcho(session), Is.EqualTo(firstOffer));
        }

        [Test]
        public void AuthoredSpeedCurves_AreSampledForDeterministicCoreUse()
        {
            Assert.That(
                StageCatalog.GetByDisplayNumber(6).SpeedProfile.SampleCount,
                Is.EqualTo(101));
            Assert.That(
                StageCatalog.GetByDisplayNumber(9).SpeedProfile.SampleCount,
                Is.EqualTo(2));
            Assert.That(
                StageCatalog.GetByDisplayNumber(11).SpeedProfile.SampleCount,
                Is.EqualTo(101));
        }

        private static StageSession CreatePlaying(
            int stageNumber,
            StartItemSelection items)
        {
            StageSession session = new StageSession(
                StageCatalog.GetByDisplayNumber(stageNumber));
            session.SelectItems(items);
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static void AssertModifierExists(
            int stageNumber,
            GateModifierType modifier)
        {
            StageDefinition stage =
                StageCatalog.GetByDisplayNumber(stageNumber);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            bool found = false;
            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                GatePlan plan = sequence.GetPlan(gate);
                found |= plan.Modifier.Has(modifier);
            }
            Assert.That(found, Is.True);
        }

        private static GatePlan FindModifierPlan(
            StageSession session,
            GateModifierType modifier)
        {
            for (int gate = 0;
                gate < session.Stage.TargetGateCount;
                gate++)
            {
                GatePlan plan = session.GetGatePlan(gate);
                if (plan.Modifier.Has(modifier))
                {
                    return plan;
                }
            }
            Assert.Fail($"Modifier {modifier} was not found.");
            return default;
        }

        private static int AcquireFirstEcho(StageSession session)
        {
            while (session.GatesPassed < session.Stage.TargetGateCount)
            {
                GatePlan plan = session.GetNextGatePlan();
                MatchCurrentColor(session, plan.Color);
                session.ResolveGate(plan);
                if (session.EchoActive)
                {
                    return plan.GateId;
                }
            }
            Assert.Fail("No Echo offer was acquired.");
            return -1;
        }

        private static void MatchCurrentColor(
            StageSession session,
            RunnerColor target)
        {
            while (session.CurrentColor != target)
            {
                session.TryToggleColor();
            }
        }
    }
}
