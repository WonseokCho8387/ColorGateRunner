using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageCatalogArchitectureTests
    {
        [Test]
        public void CampaignOrder_TeachesMechanicsInTwoColorsBeforeThreeColors()
        {
            IStageCatalog catalog = Resources.Load<StageCatalogAsset>(
                StageCatalogProvider.ResourceName).BuildCatalog();
            Assert.That(catalog.Count,
                Is.EqualTo(StageCatalogAsset.CampaignOrder.Length));
            var ids = new System.Collections.Generic.HashSet<string>();
            for (int index = 0; index < catalog.Count; index++)
            {
                StageDefinition stage = catalog.GetByIndex(index);
                Assert.That(stage.DisplayNumber, Is.EqualTo(index + 1));
                Assert.That(stage.StageId,
                    Is.EqualTo(StageCatalogAsset.CampaignOrder[index]));
                Assert.That(ids.Add(stage.StageId), Is.True);
                if (stage.DisplayNumber < 13)
                {
                    Assert.That(stage.AllowedColorCount, Is.EqualTo(2),
                        $"Stage {stage.DisplayNumber} stays two-color.");
                }
            }
            Assert.That(catalog.GetByDisplayNumber(4).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Shield));
            Assert.That(catalog.GetByDisplayNumber(5).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Booster));
            Assert.That(catalog.GetByDisplayNumber(6).StageId,
                Is.EqualTo("stage-08"));
            Assert.That(catalog.GetByDisplayNumber(13).StageId,
                Is.EqualTo("stage-04"));
            Assert.That(catalog.GetByDisplayNumber(13).AllowedColorCount,
                Is.EqualTo(3));
            Assert.That(
                TutorialCards.GetStageTopic(
                    catalog, catalog.GetByDisplayNumber(13)),
                Is.EqualTo(TutorialTopic.ThirdColor));
        }

        [Test]
        public void CampaignOrder_MovedStagesKeepTheirAuthoredGates()
        {
            // Stage 3 exception colors and Stage 4 Green teaching follow the
            // Stage ID, so moving a Stage never changes its gates.
            IStageCatalog catalog = Resources.Load<StageCatalogAsset>(
                StageCatalogProvider.ResourceName).BuildCatalog();
            StageDefinition green = catalog.GetByDisplayNumber(13);
            var sequence = new DeterministicStageGateSequence(green);
            int firstGreen = green.GetFirstGateIndex(RunnerColor.Green);
            for (int gate = 0; gate <= firstGreen; gate++)
            {
                GatePlan plan = sequence.GetPlan(gate);
                Assert.That(plan.Color == RunnerColor.Green,
                    Is.EqualTo(gate == firstGreen), $"gate {gate}");
            }
        }

        [Test]
        public void ResourceAsset_BuildsLearningCurveAndPreservesFoundationStages()
        {
            StageCatalogAsset asset =
                Resources.Load<StageCatalogAsset>(
                    StageCatalogProvider.ResourceName);

            Assert.That(asset, Is.Not.Null);
            IStageCatalog catalog = asset.BuildCatalog();
            Assert.That(catalog.Count, Is.EqualTo(26));
            Assert.That(
                AuthoredStage.From(catalog, 1).StartingSpeed,
                Is.EqualTo(28f));
            Assert.That(
                AuthoredStage.From(catalog, 5).MaximumSpeed,
                Is.EqualTo(54f));
            for (int index = 0; index < 5; index++)
            {
                Assert.That(
                    AuthoredStage.From(catalog, index + 1).SpeedProfile.SampleCount,
                    Is.EqualTo(2));
                // Green and three-color Stages now come after the Item
                // Stage, so only the basics keep start items locked.
                bool basics = index < 3;
                Assert.That(
                    AuthoredStage.From(catalog, index + 1).ShieldAllowed,
                    Is.EqualTo(!basics));
                Assert.That(
                    AuthoredStage.From(catalog, index + 1).BoosterAllowed,
                    Is.EqualTo(!basics));
            }
            Assert.That(
                AuthoredStage.From(catalog, 6).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Shield));
            Assert.That(
                AuthoredStage.From(catalog, 7).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Booster));
            Assert.That(
                AuthoredStage.From(catalog, 8).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.None));
            Assert.That(
                AuthoredStage.From(catalog, 9).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Camouflage));
            Assert.That(
                AuthoredStage.From(catalog, 12).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Fog));
            Assert.That(
                AuthoredStage.From(catalog, 15).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Ice));
            for (int number = 15; number <= 17; number++)
            {
                Assert.That(
                    AuthoredStage.From(catalog, number)
                        .IceRunwaySettings.SpeedMultiplier,
                    Is.EqualTo(2f));
            }
            Assert.That(
                AuthoredStage.From(catalog, 18).EchoSettings.Enabled,
                Is.True);
            Assert.That(
                AuthoredStage.From(catalog, 21).HiddenSettings.Enabled,
                Is.True);
            Assert.That(
                AuthoredStage.From(catalog, 24).FlickerSettings.Enabled,
                Is.True);
            for (int number = 1; number <= 26; number++)
            {
                StageDifficulty expected = number == 20
                    ? StageDifficulty.VeryHard
                    : number == 11 || number == 14 || number == 17 ||
                        number == 23 || number == 26
                        ? StageDifficulty.Hard
                        : StageDifficulty.Normal;
                Assert.That(
                    AuthoredStage.From(catalog, number).Difficulty,
                    Is.EqualTo(expected),
                    $"Stage {number} difficulty");
            }
        }

        [Test]
        public void SpeedProfile_UsesDeterministicSampleInterpolation()
        {
            StageSpeedProfile profile = new StageSpeedProfile(
                20f,
                60f,
                new[] { 0f, 0.25f, 1f });

            Assert.That(profile.Evaluate(0f), Is.EqualTo(20f));
            Assert.That(profile.Evaluate(0.25f), Is.EqualTo(25f));
            Assert.That(profile.Evaluate(0.5f), Is.EqualTo(30f));
            Assert.That(profile.Evaluate(0.75f), Is.EqualTo(45f));
            Assert.That(profile.Evaluate(1f), Is.EqualTo(60f));
        }

        [Test]
        public void LinearSpeedProfile_PreservesLegacyFloatOperationOrder()
        {
            StageSpeedProfile profile =
                StageSpeedProfile.Linear(28f, 40f);
            float[] progressValues =
                { 0f, 0.125f, 0.33333334f, 0.875f, 1f };

            for (int index = 0; index < progressValues.Length; index++)
            {
                float progress = progressValues[index];
                float legacyResult = 28f + ((40f - 28f) * progress);
                Assert.That(profile.Evaluate(progress), Is.EqualTo(legacyResult));
            }
        }

        [Test]
        public void InMemoryCatalog_ResolvesDisplayNumberWithoutIndexAssumption()
        {
            StageDefinition source =
                StageCatalog.GetByDisplayNumber(1);
            IStageCatalog catalog =
                new InMemoryStageCatalog(new[] { source });

            Assert.That(
                catalog.GetByDisplayNumber(source.DisplayNumber),
                Is.SameAs(source));
            Assert.That(
                () => catalog.GetByDisplayNumber(99),
                Throws.TypeOf<System.ArgumentOutOfRangeException>());
        }
    }
}
