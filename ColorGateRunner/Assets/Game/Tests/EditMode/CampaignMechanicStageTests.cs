using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignMechanicStageTests
    {
        [Test]
        public void StagesSixThroughTwentyThree_UseApprovedLearningSequence()
        {
            StagePrimaryMechanic[] expected =
            {
                StagePrimaryMechanic.Shield,
                StagePrimaryMechanic.Booster,
                StagePrimaryMechanic.None,
                StagePrimaryMechanic.Camouflage,
                StagePrimaryMechanic.Camouflage,
                StagePrimaryMechanic.Camouflage,
                StagePrimaryMechanic.Fog,
                StagePrimaryMechanic.Fog,
                StagePrimaryMechanic.Fog,
                StagePrimaryMechanic.Ice,
                StagePrimaryMechanic.Ice,
                StagePrimaryMechanic.Ice,
                StagePrimaryMechanic.Echo,
                StagePrimaryMechanic.Echo,
                StagePrimaryMechanic.Echo,
                StagePrimaryMechanic.Hidden,
                StagePrimaryMechanic.Hidden,
                StagePrimaryMechanic.Hidden
            };

            for (int index = 0; index < expected.Length; index++)
            {
                StageDefinition stage =
                    AuthoredStage.Get(index + 6);
                Assert.That(stage.PrimaryMechanic, Is.EqualTo(expected[index]));
            }
        }

        [Test]
        public void ShieldStage_ProvidesLocalChargeWhileSelectionIsLocked()
        {
            StageSession session = CreatePlaying(
                6,
                new StartItemSelection(true, false));

            Assert.That(session.StageProvidesShield, Is.True);
            Assert.That(session.Stage.ShieldAllowed, Is.False);
            Assert.That(session.Stage.BoosterAllowed, Is.False);
            Assert.That(session.MechanicGrantActivated, Is.True);
            Assert.That(session.ShieldActive, Is.True);
            Assert.That(session.Items.Shield, Is.False);
        }

        [Test]
        public void BoosterStage_ProvidesLocalChargeAtStageStart()
        {
            StageSession session = CreatePlaying(7, default);

            Assert.That(session.MechanicGrantActivated, Is.True);
            Assert.That(session.Stage.ShieldAllowed, Is.False);
            Assert.That(session.Stage.BoosterAllowed, Is.False);
            Assert.That(session.BoosterActive, Is.True);
            Assert.That(
                session.Stage.MechanicGrantSettings.ActivationMode,
                Is.EqualTo(
                    StageMechanicActivationMode.ActiveAtStageStart));
            Assert.That(session.Items.Booster, Is.False);
            Assert.That(
                session.BoosterDistanceRemaining,
                Is.EqualTo(session.Stage.BoosterDistance));
        }

        [Test]
        public void StartItems_AreLockedThroughGrantTrainingAndUnlockAtTheItemStage()
        {
            // Display 1-3 basics, 4 Shield and 5 Booster training lock start
            // items; from the Item Stage (display 6) on they are open.
            for (int display = 1; display <= StageCatalog.Count; display++)
            {
                StageDefinition stage = StageCatalog.GetByDisplayNumber(display);
                bool locked = display <= 5;
                Assert.That(stage.ShieldAllowed, Is.EqualTo(!locked), $"{display}");
                Assert.That(stage.BoosterAllowed, Is.EqualTo(!locked), $"{display}");
                if (!locked)
                {
                    continue;
                }
                StageSession session = CreatePlaying(
                    StageCatalogAsset.LegacyNumber(stage.StageId),
                    new StartItemSelection(true, true));
                Assert.That(session.Items.Shield, Is.False);
                Assert.That(session.Items.Booster, Is.False);
                Assert.That(session.ShieldActive, Is.EqualTo(display == 4));
                Assert.That(session.BoosterActive, Is.EqualTo(display == 5));
            }
        }

        [Test]
        public void GateModifiers_AreDeterministicAndStayInsideAuthoredStages()
        {
            for (int stageNumber = 9; stageNumber <= 11; stageNumber++)
            {
                AssertModifierExists(stageNumber, GateModifierType.Camouflage);
            }
            for (int stageNumber = 12; stageNumber <= 14; stageNumber++)
            {
                AssertModifierExists(stageNumber, GateModifierType.Fog);
            }
            for (int stageNumber = 15; stageNumber <= 17; stageNumber++)
            {
                AssertModifierExists(stageNumber, GateModifierType.Ice);
            }
            for (int stageNumber = 18; stageNumber <= 20; stageNumber++)
            {
                AssertModifierExists(stageNumber, GateModifierType.EchoProvider);
            }
            for (int stageNumber = 21; stageNumber <= 23; stageNumber++)
            {
                AssertModifierExists(stageNumber, GateModifierType.Hidden);
            }
            for (int stageNumber = 24; stageNumber <= 26; stageNumber++)
            {
                AssertModifierExists(stageNumber, GateModifierType.Flicker);
            }

            for (int stageNumber = 1; stageNumber <= 8; stageNumber++)
            {
                DeterministicStageGateSequence sequence =
                    new DeterministicStageGateSequence(
                        AuthoredStage.Get(stageNumber));
                StageDefinition stage =
                    AuthoredStage.Get(stageNumber);
                for (int gate = 0; gate < stage.TargetGateCount; gate++)
                {
                    Assert.That(
                        sequence.GetPlan(gate).Modifier.IsNone,
                        Is.True);
                }
            }
        }

        [Test]
        public void HiddenBlock_DistributesIncreasingPressureAcrossWholeStages()
        {
            int[][] expectedGateNumbers =
            {
                new[] { 6, 12, 16, 21, 27, 33, 36 },
                new[] { 6, 9, 17, 21, 24, 29, 34, 37, 43 },
                new[] { 6, 9, 12, 15, 21, 25, 30, 33, 36, 39, 47 }
            };
            float[] expectedLeadSeconds = { 1.30f, 1.15f, 1.00f };

            for (int stageOffset = 0;
                stageOffset < expectedGateNumbers.Length;
                stageOffset++)
            {
                StageDefinition stage =
                    AuthoredStage.Get(21 + stageOffset);
                var actual = new List<int>();
                var sequence = new DeterministicStageGateSequence(stage);
                for (int gate = 0; gate < stage.TargetGateCount; gate++)
                {
                    if (sequence.GetPlan(gate).Modifier.IsHidden)
                    {
                        actual.Add(gate + 1);
                    }
                }

                Assert.That(actual, Is.EqualTo(expectedGateNumbers[stageOffset]));
                Assert.That(
                    stage.HiddenSettings.HideLeadTimeSeconds,
                    Is.EqualTo(expectedLeadSeconds[stageOffset]));
                Assert.That(
                    stage.HiddenSettings.TransitionSeconds,
                    Is.EqualTo(0.18f));
                Assert.That(
                    actual[actual.Count - 1],
                    Is.GreaterThan(stage.TargetGateCount * 0.7f));
            }
        }

        [Test]
        public void FlickerBlock_UsesIncreasingPressureAndAuthoredTiming()
        {
            int[] expectedCounts = { 6, 8, 10 };
            int[] expectedColors = { 2, 2, 3 };
            float[] expectedSwitchSeconds = { 1.35f, 1.17f, 0.99f };

            for (int offset = 0; offset < 3; offset++)
            {
                StageDefinition stage =
                    AuthoredStage.Get(24 + offset);
                DeterministicStageGateSequence sequence =
                    new DeterministicStageGateSequence(stage);
                List<int> occurrences = new List<int>();
                float authoredDistance = 0f;
                float firstOccurrenceDistance = 0f;
                for (int gate = 0; gate < stage.TargetGateCount; gate++)
                {
                    GatePlan plan = sequence.GetPlan(gate);
                    authoredDistance += plan.Spacing;
                    if (plan.Modifier.IsFlicker)
                    {
                        occurrences.Add(gate + 1);
                        if (firstOccurrenceDistance <= 0f)
                        {
                            firstOccurrenceDistance = authoredDistance;
                        }
                        Assert.That(
                            plan.FlickerPlan.TransitionPulseSeconds,
                            Is.EqualTo(0.24f));
                    }
                }

                Assert.That(occurrences.Count,
                    Is.EqualTo(expectedCounts[offset]),
                    $"Stage {stage.DisplayNumber}: " +
                    string.Join(",", occurrences));
                Assert.That(stage.AllowedColorCount,
                    Is.EqualTo(expectedColors[offset]));
                Assert.That(stage.BoosterAllowed, Is.True);
                Assert.That(
                    stage.FlickerSettings.SwitchIntervalSeconds,
                    Is.EqualTo(expectedSwitchSeconds[offset]));
                Assert.That(
                    occurrences[occurrences.Count - 1],
                    Is.GreaterThan(stage.TargetGateCount * 0.58f));
                Assert.That(
                    firstOccurrenceDistance,
                    Is.GreaterThan(stage.BoosterDistance));

                sequence.Reset();
                List<int> retryOccurrences = new List<int>();
                for (int gate = 0; gate < stage.TargetGateCount; gate++)
                {
                    if (sequence.GetPlan(gate).Modifier.IsFlicker)
                    {
                        retryOccurrences.Add(gate + 1);
                    }
                }
                Assert.That(retryOccurrences, Is.EqualTo(occurrences));
            }
        }

        [Test]
        public void Ice_UsesAuthoredCampaignSpeedAndSharedSpacingMultipliers()
        {
            StageSession session = CreatePlaying(15, default);
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
                    session.Stage.IceRunwaySettings.SpeedMultiplier)
                    .Within(0.001f));
            Assert.That(
                session.Stage.IceRunwaySettings.SpeedMultiplier,
                Is.EqualTo(2f));
        }

        [Test]
        public void EchoStage_AcquiresConsumesAndRestartsDeterministically()
        {
            StageSession session = CreatePlaying(18, default);
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
        public void EchoStage_SelectedShieldBlocksProvidersForWholeAttempt()
        {
            StageSession session = CreatePlaying(
                18,
                new StartItemSelection(true, false));

            for (int gate = 0; gate < session.Stage.TargetGateCount; gate++)
            {
                GatePlan plan = session.GetGatePlan(gate);
                Assert.That(plan.Modifier.IsEchoProvider, Is.False);
            }

            Assert.That(session.EchoActive, Is.False);
            Assert.That(session.EchoAcquisitionCount, Is.Zero);
        }

        [Test]
        public void AuthoredSpeedCurves_AreSampledForDeterministicCoreUse()
        {
            Assert.That(
                AuthoredStage.Get(6).SpeedProfile.SampleCount,
                Is.EqualTo(101));
            Assert.That(
                AuthoredStage.Get(9).SpeedProfile.SampleCount,
                Is.EqualTo(101));
            Assert.That(
                AuthoredStage.Get(11).SpeedProfile.SampleCount,
                Is.EqualTo(101));
            Assert.That(
                AuthoredStage.Get(18).SpeedProfile.SampleCount,
                Is.EqualTo(101));
            Assert.That(
                AuthoredStage.Get(23).SpeedProfile.SampleCount,
                Is.EqualTo(101));
        }

        private static StageSession CreatePlaying(
            int stageNumber,
            StartItemSelection items)
        {
            StageSession session = new StageSession(
                AuthoredStage.Get(stageNumber));
            session.SelectItems(items);
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static void AssertModifierExists(
            int stageNumber,
            GateModifierType modifier)
        {
            StageSession session = CreatePlaying(stageNumber, default);
            bool found = false;
            for (int gate = 0;
                gate < session.Stage.TargetGateCount;
                gate++)
            {
                GatePlan plan = session.GetGatePlan(gate);
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
