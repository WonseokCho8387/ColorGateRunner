using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignDeferredMechanicStageTests
    {
        [Test]
        public void CampaignStagesOneThroughTwenty_DeferHiddenAndFlicker()
        {
            GateModifierType deferred =
                GateModifierType.Hidden | GateModifierType.Flicker;

            for (int stageNumber = 1; stageNumber <= 20; stageNumber++)
            {
                StageDefinition stage =
                    StageCatalog.GetByDisplayNumber(stageNumber);

                Assert.That(stage.PrimaryMechanic,
                    Is.Not.EqualTo(StagePrimaryMechanic.Hidden));
                Assert.That(stage.PrimaryMechanic,
                    Is.Not.EqualTo(StagePrimaryMechanic.Flicker));
                Assert.That(stage.GateModifiers & deferred,
                    Is.EqualTo(GateModifierType.None));
                Assert.That(stage.HiddenSettings.Enabled, Is.False);
                Assert.That(stage.FlickerSettings.Enabled, Is.False);
            }
        }

        [Test]
        public void ExperimentCatalog_RetainsHiddenAndFlickerImplementations()
        {
            ExperimentDefinition hidden = ExperimentCatalog.Get(
                3,
                MechanicExperimentType.Hidden,
                120012u);
            ExperimentDefinition flicker = ExperimentCatalog.Get(
                3,
                MechanicExperimentType.Flicker,
                130013u);

            Assert.That(hidden.Mechanic,
                Is.EqualTo(MechanicExperimentType.Hidden));
            Assert.That(hidden.Hidden.Enabled, Is.True);
            Assert.That(flicker.Mechanic,
                Is.EqualTo(MechanicExperimentType.Flicker));
            Assert.That(flicker.Flicker.Enabled, Is.True);
        }
    }
}
