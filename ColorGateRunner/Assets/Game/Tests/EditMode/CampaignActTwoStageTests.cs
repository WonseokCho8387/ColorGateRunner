using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignActTwoStageTests
    {
        private static readonly int[] GateCounts =
            { 48, 52, 54, 52, 56, 56, 60 };

        private static readonly float[] StartingSpeeds =
            { 46f, 48f, 48f, 50f, 50f, 50f, 52f };

        private static readonly float[] MaximumSpeeds =
            { 66f, 68f, 68f, 70f, 70f, 70f, 72f };

        private static readonly float[] CadenceStarts =
            { 1.10f, 1.08f, 1.08f, 1.04f, 1.06f, 1.08f, 1.04f };

        private static readonly float[] CadenceEnds =
            { 0.84f, 0.82f, 0.82f, 0.80f, 0.80f, 0.82f, 0.78f };

        private static readonly StagePrimaryMechanic[] Mechanics =
        {
            StagePrimaryMechanic.None,
            StagePrimaryMechanic.Camouflage,
            StagePrimaryMechanic.Fog,
            StagePrimaryMechanic.Ice,
            StagePrimaryMechanic.Echo,
            StagePrimaryMechanic.Hidden,
            StagePrimaryMechanic.Flicker
        };

        private static readonly GateModifierType[] Modifiers =
        {
            GateModifierType.None,
            GateModifierType.Camouflage,
            GateModifierType.Fog,
            GateModifierType.Ice,
            GateModifierType.EchoProvider,
            GateModifierType.Hidden,
            GateModifierType.Flicker
        };

        [Test]
        public void Catalog_ContainsApprovedActTwoCurve()
        {
            for (int index = 0; index < GateCounts.Length; index++)
            {
                int stageNumber = index + 14;
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(stageNumber);

                Assert.That(stage.StageId,
                    Is.EqualTo($"stage-{stageNumber:00}"));
                Assert.That(stage.DisplayNumber, Is.EqualTo(stageNumber));
                Assert.That(stage.TargetGateCount, Is.EqualTo(GateCounts[index]));
                Assert.That(stage.AllowedColorCount, Is.EqualTo(3));
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
                Assert.That(stage.IsValid(), Is.True);
            }
        }

        [Test]
        public void ActTwoPlans_AreDeterministicAndKeepMechanicsIsolated()
        {
            for (int index = 0; index < Modifiers.Length; index++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(index + 14);
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
                    if (a.Modifier.Has(Modifiers[index]))
                    {
                        occurrences++;
                    }
                }

                if (Modifiers[index] == GateModifierType.None)
                {
                    Assert.That(occurrences, Is.EqualTo(stage.TargetGateCount));
                }
                else
                {
                    Assert.That(occurrences, Is.GreaterThan(0));
                }
            }
        }

        [Test]
        public void HiddenAndFlickerFinale_UseExistingBoundedOccurrenceContracts()
        {
            AssertBoundedOccurrences(19, GateModifierType.Hidden, 4);
            AssertBoundedOccurrences(20, GateModifierType.Flicker, 4);
        }

        private static void AssertBoundedOccurrences(
            int stageNumber,
            GateModifierType modifier,
            int expectedCount)
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(stageNumber);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            int count = 0;

            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                if (sequence.GetPlan(gate).Modifier.Has(modifier))
                {
                    count++;
                }
            }

            Assert.That(count, Is.EqualTo(expectedCount));
        }
    }
}
