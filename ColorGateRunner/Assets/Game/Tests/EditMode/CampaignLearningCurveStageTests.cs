using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignLearningCurveStageTests
    {
        private static readonly int[] GateCounts =
            { 40, 38, 42, 46, 40, 44, 48, 42, 46, 50, 42, 46, 50, 42, 46, 50 };

        private static readonly float[] StartingSpeeds =
            { 38f, 38f, 40f, 42f, 40f, 42f, 44f, 40f, 42f, 44f, 42f, 44f, 46f, 42f, 44f, 46f };

        private static readonly float[] MaximumSpeeds =
            { 56f, 54f, 58f, 62f, 56f, 60f, 64f, 56f, 60f, 64f, 58f, 62f, 66f, 58f, 62f, 66f };

        private static readonly float[] CadenceStarts =
            { 1.22f, 1.24f, 1.18f, 1.12f, 1.22f, 1.16f, 1.10f, 1.20f, 1.14f, 1.08f, 1.20f, 1.14f, 1.08f, 1.20f, 1.14f, 1.08f };

        private static readonly float[] CadenceEnds =
            { 0.84f, 0.90f, 0.84f, 0.80f, 0.88f, 0.82f, 0.80f, 0.88f, 0.82f, 0.78f, 0.86f, 0.80f, 0.78f, 0.86f, 0.80f, 0.78f };

        private static readonly int[] ColorCounts =
            { 3, 2, 2, 3, 2, 2, 3, 2, 2, 3, 2, 2, 3, 2, 2, 3 };

        private static readonly StagePrimaryMechanic[] Mechanics =
        {
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

        private static readonly GateModifierType[] Modifiers =
        {
            GateModifierType.None,
            GateModifierType.Camouflage,
            GateModifierType.Camouflage,
            GateModifierType.Camouflage,
            GateModifierType.Fog,
            GateModifierType.Fog,
            GateModifierType.Fog,
            GateModifierType.Ice,
            GateModifierType.Ice,
            GateModifierType.Ice,
            GateModifierType.EchoProvider,
            GateModifierType.EchoProvider,
            GateModifierType.EchoProvider,
            GateModifierType.Hidden,
            GateModifierType.Hidden,
            GateModifierType.Hidden
        };

        [Test]
        public void Catalog_ContainsApprovedLearningBlockCurve()
        {
            for (int index = 0; index < GateCounts.Length; index++)
            {
                int stageNumber = index + 8;
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(stageNumber);

                Assert.That(stage.StageId,
                    Is.EqualTo($"stage-{stageNumber:00}"));
                Assert.That(stage.DisplayNumber, Is.EqualTo(stageNumber));
                Assert.That(stage.TargetGateCount, Is.EqualTo(GateCounts[index]));
                Assert.That(stage.AllowedColorCount, Is.EqualTo(ColorCounts[index]));
                Assert.That(stage.ActiveColorsFromStart, Is.True);
                Assert.That(stage.StartingSpeed,
                    Is.EqualTo(StartingSpeeds[index]));
                Assert.That(stage.MaximumSpeed,
                    Is.EqualTo(MaximumSpeeds[index]));
                Assert.That(stage.CadenceStart,
                    Is.EqualTo(CadenceStarts[index]));
                Assert.That(stage.CadenceEnd,
                    Is.EqualTo(CadenceEnds[index]));
                Assert.That(stage.PrimaryMechanic,
                    Is.EqualTo(Mechanics[index]));
                Assert.That(stage.GateModifiers,
                    Is.EqualTo(Modifiers[index]));
                Assert.That(stage.ShieldAllowed, Is.True);
                Assert.That(stage.BoosterAllowed, Is.True);
                Assert.That(stage.MechanicGrantSettings.Enabled, Is.False);
                Assert.That(stage.SpeedProfile.SampleCount, Is.EqualTo(101));
                Assert.That(stage.IsValid(), Is.True);
            }
        }

        [TestCase(9, 11, StagePrimaryMechanic.Camouflage, GateModifierType.Camouflage)]
        [TestCase(12, 14, StagePrimaryMechanic.Fog, GateModifierType.Fog)]
        [TestCase(15, 17, StagePrimaryMechanic.Ice, GateModifierType.Ice)]
        [TestCase(18, 20, StagePrimaryMechanic.Echo, GateModifierType.EchoProvider)]
        [TestCase(21, 23, StagePrimaryMechanic.Hidden, GateModifierType.Hidden)]
        public void MechanicBlocks_ProgressFromTwoColorIntroToThreeColorMastery(
            int firstStage,
            int lastStage,
            StagePrimaryMechanic mechanic,
            GateModifierType modifier)
        {
            for (int stageNumber = firstStage;
                stageNumber <= lastStage;
                stageNumber++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(stageNumber);
                Assert.That(stage.PrimaryMechanic, Is.EqualTo(mechanic));
                Assert.That(stage.GateModifiers, Is.EqualTo(modifier));
                Assert.That(stage.AllowedColorCount,
                    Is.EqualTo(stageNumber == lastStage ? 3 : 2));
            }
        }

        [Test]
        public void FogBlock_UsesApprovedVisibilityDurationCurve()
        {
            float[] fullOpacitySeconds = { 5f, 6f, 7f };
            for (int index = 0; index < fullOpacitySeconds.Length; index++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(index + 12);

                Assert.That(stage.FogCurtainSettings.FadeInSeconds,
                    Is.EqualTo(0.5f));
                Assert.That(stage.FogCurtainSettings.FullOpacitySeconds,
                    Is.EqualTo(fullOpacitySeconds[index]));
                Assert.That(stage.FogCurtainSettings.FadeOutSeconds,
                    Is.EqualTo(0.5f));
            }
        }

        [Test]
        public void IceBlock_UsesApprovedCampaignSpeedMultiplier()
        {
            for (int stageNumber = 15; stageNumber <= 17; stageNumber++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(stageNumber);

                Assert.That(
                    stage.IceRunwaySettings.SpeedMultiplier,
                    Is.EqualTo(2f));
            }
        }

        [Test]
        public void IceRunwaySettings_RejectNonPositiveSpeed()
        {
            Assert.That(
                () => new IceRunwaySettings(0f),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
            Assert.That(
                () => new IceRunwaySettings(-1f),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }

        [TestCase(15, 12, 12, 0)]
        [TestCase(16, 14, 14, 0)]
        [TestCase(17, 15, 14, 1)]
        public void IceBlock_PrefersOneTapAndKeepsDoubleTapsBelowTenPercent(
            int stageNumber,
            int expectedIceGateCount,
            int expectedSingleTapCount,
            int expectedDoubleTapCount)
        {
            StageDefinition stage =
                StageCatalog.GetByDisplayNumber(stageNumber);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            RunnerColor currentColor = stage.GetAllowedColor(0);
            int iceGateCount = 0;
            int singleTapCount = 0;
            int doubleTapCount = 0;

            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                GatePlan plan = sequence.GetPlan(gate);
                int requiredTaps = stage.GetRequiredTapCount(
                    gate,
                    currentColor,
                    plan.Color);
                if (plan.Modifier.IsIce)
                {
                    iceGateCount++;
                    Assert.That(requiredTaps,
                        Is.InRange(
                            GateModifierRules.PreferredIceTapCount,
                            GateModifierRules.MaximumIceTapCount));
                    if (requiredTaps ==
                        GateModifierRules.PreferredIceTapCount)
                    {
                        singleTapCount++;
                    }
                    if (requiredTaps ==
                        GateModifierRules.MaximumIceTapCount)
                    {
                        doubleTapCount++;
                    }
                }

                currentColor = plan.Color;
            }

            Assert.That(iceGateCount, Is.EqualTo(expectedIceGateCount));
            Assert.That(singleTapCount,
                Is.EqualTo(expectedSingleTapCount));
            Assert.That(doubleTapCount,
                Is.EqualTo(expectedDoubleTapCount));
            Assert.That(
                doubleTapCount / (float)iceGateCount,
                Is.LessThan(
                    GateModifierRules.MaximumIceDoubleTapRate));
        }

        [Test]
        public void IceTapRhythm_RetryReplaysTheSameColorsAndQuota()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(17);
            DeterministicStageGateSequence first =
                new DeterministicStageGateSequence(stage);
            DeterministicStageGateSequence replay =
                new DeterministicStageGateSequence(stage);

            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                GatePlan firstPlan = first.GetPlan(gate);
                GatePlan replayPlan = replay.GetPlan(gate);
                Assert.That(replayPlan.Color, Is.EqualTo(firstPlan.Color));
                Assert.That(replayPlan.Modifier.Types,
                    Is.EqualTo(firstPlan.Modifier.Types));
            }
        }

        [Test]
        public void LearningBlockPlans_AreDeterministicAndKeepModifiersIsolated()
        {
            for (int index = 0; index < Modifiers.Length; index++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(index + 8);
                StageSession first = new StageSession(stage);
                StageSession second = new StageSession(stage);
                int occurrences = 0;

                for (int gate = 0; gate < stage.TargetGateCount; gate++)
                {
                    GatePlan a = first.GetGatePlan(gate);
                    GatePlan b = second.GetGatePlan(gate);

                    Assert.That(a.GateId, Is.EqualTo(gate));
                    Assert.That(a.Color, Is.EqualTo(b.Color));
                    Assert.That(a.Pattern, Is.EqualTo(b.Pattern));
                    Assert.That(a.Spacing, Is.EqualTo(b.Spacing));
                    Assert.That(a.Modifier.Types,
                        Is.EqualTo(b.Modifier.Types));
                    Assert.That(
                        a.Modifier.Types & ~Modifiers[index],
                        Is.EqualTo(GateModifierType.None));
                    if (Modifiers[index] != GateModifierType.None &&
                        a.Modifier.Has(Modifiers[index]))
                    {
                        occurrences++;
                    }
                }

                if (Modifiers[index] == GateModifierType.None)
                {
                    Assert.That(occurrences, Is.Zero);
                }
                else
                {
                    Assert.That(occurrences, Is.GreaterThan(0));
                }
            }
        }
    }
}
