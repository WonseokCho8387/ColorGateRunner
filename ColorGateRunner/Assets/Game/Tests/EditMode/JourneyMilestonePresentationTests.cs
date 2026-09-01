using System.Linq;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class JourneyMilestonePresentationTests
    {
        [Test]
        public void Journey_MapsEighteenStableMilestonesAndRewards()
        {
            var models = JourneyMilestonePresentation.Create(0, 26);

            Assert.That(models.Count, Is.EqualTo(18));
            Assert.That(models[0].StageNumber, Is.EqualTo(2));
            Assert.That(models[17].StageNumber, Is.EqualTo(36));
            Assert.That(models[0].ChapterIndex, Is.Zero);
            Assert.That(models[6].ChapterIndex, Is.EqualTo(1));
            Assert.That(models[12].ChapterIndex, Is.EqualTo(2));
            Assert.That(models[0].Reward.MilestoneCoins, Is.EqualTo(200));
            Assert.That(models[2].Reward.MilestoneCoins, Is.EqualTo(300));
            Assert.That(models[0].Reward.Shields, Is.EqualTo(1));
            Assert.That(models[1].Reward.Boosters, Is.EqualTo(1));
        }

        [Test]
        public void Journey_DistinguishesCollectedCurrentLockedAndUnavailable()
        {
            var models = JourneyMilestonePresentation.Create(4, 26);

            Assert.That(models.Take(4), Has.All.Matches<JourneyMilestoneCardModel>(
                model => model.State == JourneyMilestoneState.Collected));
            Assert.That(models[4].State, Is.EqualTo(JourneyMilestoneState.Current));
            Assert.That(models[5].State, Is.EqualTo(JourneyMilestoneState.Locked));
            Assert.That(models[12].StageNumber, Is.EqualTo(26));
            Assert.That(models[12].State, Is.EqualTo(JourneyMilestoneState.Locked));
            Assert.That(models.Skip(13), Has.All.Matches<JourneyMilestoneCardModel>(
                model => model.State == JourneyMilestoneState.ComingSoon));
        }

        [Test]
        public void Journey_ReadModelClampsInvalidProgressWithoutMutation()
        {
            var below = JourneyMilestonePresentation.Create(-5, 26);
            var above = JourneyMilestonePresentation.Create(99, 26);

            Assert.That(below[0].State, Is.EqualTo(JourneyMilestoneState.Current));
            Assert.That(above, Has.All.Matches<JourneyMilestoneCardModel>(
                model => model.State == JourneyMilestoneState.Collected));
        }
    }
}
