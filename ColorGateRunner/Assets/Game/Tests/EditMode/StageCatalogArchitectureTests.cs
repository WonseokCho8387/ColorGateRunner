using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageCatalogArchitectureTests
    {
        [Test]
        public void ResourceAsset_BuildsLearningCurveAndPreservesFoundationStages()
        {
            StageCatalogAsset asset =
                Resources.Load<StageCatalogAsset>(
                    StageCatalogProvider.ResourceName);

            Assert.That(asset, Is.Not.Null);
            IStageCatalog catalog = asset.BuildCatalog();
            Assert.That(catalog.Count, Is.EqualTo(23));
            Assert.That(
                catalog.GetByDisplayNumber(1).StartingSpeed,
                Is.EqualTo(28f));
            Assert.That(
                catalog.GetByDisplayNumber(5).MaximumSpeed,
                Is.EqualTo(54f));
            for (int index = 0; index < 5; index++)
            {
                Assert.That(
                    catalog.GetByIndex(index).SpeedProfile.SampleCount,
                    Is.EqualTo(2));
                Assert.That(
                    catalog.GetByIndex(index).ShieldAllowed,
                    Is.False);
                Assert.That(
                    catalog.GetByIndex(index).BoosterAllowed,
                    Is.False);
            }
            Assert.That(
                catalog.GetByDisplayNumber(6).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Shield));
            Assert.That(
                catalog.GetByDisplayNumber(7).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Booster));
            Assert.That(
                catalog.GetByDisplayNumber(8).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.None));
            Assert.That(
                catalog.GetByDisplayNumber(9).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Camouflage));
            Assert.That(
                catalog.GetByDisplayNumber(12).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Fog));
            Assert.That(
                catalog.GetByDisplayNumber(15).PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Ice));
            for (int number = 15; number <= 17; number++)
            {
                Assert.That(
                    catalog.GetByDisplayNumber(number)
                        .IceRunwaySettings.SpeedMultiplier,
                    Is.EqualTo(2f));
            }
            Assert.That(
                catalog.GetByDisplayNumber(18).EchoSettings.Enabled,
                Is.True);
            Assert.That(
                catalog.GetByDisplayNumber(21).HiddenSettings.Enabled,
                Is.True);
            for (int number = 1; number <= 23; number++)
            {
                StageDifficulty expected = number == 20
                    ? StageDifficulty.VeryHard
                    : number == 11 || number == 14 || number == 17 ||
                        number == 23
                        ? StageDifficulty.Hard
                        : StageDifficulty.Normal;
                Assert.That(
                    catalog.GetByDisplayNumber(number).Difficulty,
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
