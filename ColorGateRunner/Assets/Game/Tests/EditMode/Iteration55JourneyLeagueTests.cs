using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Product;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Iteration55JourneyLeagueTests
    {
        [Test]
        public void LobbyChapterPolicy_UnlocksAtStages12And24Milestones()
        {
            Assert.That(
                LobbyChapterPolicy.GetUnlockedChapterCount(5),
                Is.EqualTo(1));
            Assert.That(
                LobbyChapterPolicy.GetUnlockedChapterCount(6),
                Is.EqualTo(2));
            Assert.That(
                LobbyChapterPolicy.GetUnlockedChapterCount(12),
                Is.EqualTo(3));
            Assert.That(
                LobbyChapterPolicy.ResolveSelectedThemeId(string.Empty, 12),
                Is.EqualTo(LobbyChapterPolicy.SkyFestivalId));
            Assert.That(
                LobbyChapterPolicy.ResolveSelectedThemeId(
                    LobbyChapterPolicy.ColorCourtyardId,
                    12),
                Is.EqualTo(LobbyChapterPolicy.ColorCourtyardId));
        }

        [Test]
        public void LeagueSelector_IsStableAndUsesEveryStageBeforeRepeat()
        {
            IReadOnlyList<string> stages = new[]
            {
                "stage-01", "stage-02", "stage-03", "stage-04"
            };
            var firstCycle = new HashSet<string>();
            for (int replay = 0; replay < stages.Count; replay++)
            {
                string first = LeagueStageSelector.SelectStageId(
                    "profile-a",
                    stages,
                    replay);
                string repeated = LeagueStageSelector.SelectStageId(
                    "profile-a",
                    stages,
                    replay);
                Assert.That(repeated, Is.EqualTo(first));
                Assert.That(firstCycle.Add(first), Is.True);
            }
            Assert.That(firstCycle.Count, Is.EqualTo(stages.Count));
        }

        [Test]
        public void LeagueSelector_AvoidsImmediateRepeatAcrossCycles()
        {
            IReadOnlyList<string> stages = new[]
            {
                "stage-01", "stage-02", "stage-03"
            };
            for (int cycle = 0; cycle < 20; cycle++)
            {
                int boundary = (cycle + 1) * stages.Count;
                string previous = LeagueStageSelector.SelectStageId(
                    "profile-boundary",
                    stages,
                    boundary - 1);
                string next = LeagueStageSelector.SelectStageId(
                    "profile-boundary",
                    stages,
                    boundary);
                Assert.That(next, Is.Not.EqualTo(previous));
            }
        }

        [Test]
        public void LeagueSelector_AdvancesOnlyWhenReplayClearCountAdvances()
        {
            IReadOnlyList<string> stages = new[]
            {
                "stage-01", "stage-02", "stage-03"
            };
            string pending = LeagueStageSelector.SelectStageId(
                "stable-profile",
                stages,
                7);

            Assert.That(
                LeagueStageSelector.SelectStageId(
                    "stable-profile",
                    stages,
                    7),
                Is.EqualTo(pending));
            Assert.That(
                LeagueStageSelector.SelectStageId(
                    "stable-profile",
                    stages,
                    8),
                Is.Not.EqualTo(pending));
        }
    }
}
