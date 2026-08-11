using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageCatalogArchitectureTests
    {
        [Test]
        public void ResourceAsset_BuildsExpandedCatalogAndPreservesLegacyStages()
        {
            StageCatalogAsset asset =
                Resources.Load<StageCatalogAsset>(
                    StageCatalogProvider.ResourceName);

            Assert.That(asset, Is.Not.Null);
            IStageCatalog catalog = asset.BuildCatalog();
            Assert.That(catalog.Count, Is.EqualTo(20));
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
                catalog.GetByDisplayNumber(11).EchoSettings.Enabled,
                Is.True);
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
