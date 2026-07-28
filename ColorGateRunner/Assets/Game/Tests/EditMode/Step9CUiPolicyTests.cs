using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class Step9CUiPolicyTests
    {
        [Test]
        public void TwoColorHud_UsesStageOrder()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(1);

            Assert.That(MobileUiPolicy.GetActiveColorCount(stage, 0), Is.EqualTo(2));
            Assert.That(MobileUiPolicy.GetColorAt(stage, 0, 0), Is.EqualTo(RunnerColor.Red));
            Assert.That(MobileUiPolicy.GetColorAt(stage, 0, 1), Is.EqualTo(RunnerColor.Blue));
        }

        [Test]
        public void ThreeColorHud_UsesStageOrderAfterIntroduction()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);

            Assert.That(
                MobileUiPolicy.GetActiveColorCount(stage, stage.IntroGateCount),
                Is.EqualTo(3));
            Assert.That(MobileUiPolicy.GetColorAt(stage, stage.IntroGateCount, 2),
                Is.EqualTo(RunnerColor.Green));
        }

        [Test]
        public void ThreeColorIntroduction_ShowsTwoTilesUntilIntroductionEnds()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(4);

            Assert.That(
                MobileUiPolicy.GetActiveColorCount(
                    stage,
                    stage.IntroGateCount - 1),
                Is.EqualTo(2));
        }

        [Test]
        public void StageFive_ShowsThreeTilesImmediately()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(5);

            Assert.That(
                MobileUiPolicy.GetActiveColorCount(stage, 0),
                Is.EqualTo(3));
        }

        [TestCase(RunnerColor.Red, RunnerColor.Blue)]
        [TestCase(RunnerColor.Blue, RunnerColor.Red)]
        public void NextColor_FollowsActiveCycle(
            RunnerColor current,
            RunnerColor expected)
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(1);

            Assert.That(
                MobileUiPolicy.GetNextColor(stage, 0, current),
                Is.EqualTo(expected));
        }

        [Test]
        public void CurrentColor_RemainsAnActiveTile()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(5);

            Assert.That(
                MobileUiPolicy.GetNextColor(stage, 12, RunnerColor.Green),
                Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void LobbyTierMapping_UsesExistingProgressionThresholds()
        {
            Assert.That(LobbyProgression.GetVisualTier(Cleared()), Is.EqualTo(0));
            Assert.That(LobbyProgression.GetVisualTier(Cleared(2)), Is.EqualTo(1));
            Assert.That(LobbyProgression.GetVisualTier(Cleared(4)), Is.EqualTo(2));
            Assert.That(LobbyProgression.GetVisualTier(Cleared(5)), Is.EqualTo(3));
        }

        [TestCase(MobileUiFlow.Lobby)]
        [TestCase(MobileUiFlow.PreRun)]
        [TestCase(MobileUiFlow.Gameplay)]
        [TestCase(MobileUiFlow.ClearResult)]
        [TestCase(MobileUiFlow.FailedResult)]
        [TestCase(MobileUiFlow.Development)]
        public void PrimaryFlow_ActivatesExactlyOneRoot(MobileUiFlow flow)
        {
            MobileUiVisibility visibility = MobileUiPolicy.GetVisibility(flow);

            Assert.That(CountPrimaryRoots(visibility), Is.EqualTo(1));
        }

        [Test]
        public void Countdown_UsesGameplayHudWithCountdownOverlay()
        {
            MobileUiVisibility visibility =
                MobileUiPolicy.GetVisibility(MobileUiFlow.Countdown);

            Assert.That(visibility.GameplayHud, Is.True);
            Assert.That(visibility.Countdown, Is.True);
            Assert.That(CountPrimaryRoots(visibility), Is.EqualTo(1));
        }

        [TestCase(MobileUiFlow.Lobby, false)]
        [TestCase(MobileUiFlow.PreRun, false)]
        [TestCase(MobileUiFlow.Gameplay, false)]
        [TestCase(MobileUiFlow.Gameplay, true)]
        public void BoosterMeter_OnlyAppearsDuringActiveGameplay(
            MobileUiFlow flow,
            bool active)
        {
            Assert.That(
                MobileUiPolicy.IsBoosterMeterVisible(flow, active),
                Is.EqualTo(flow == MobileUiFlow.Gameplay && active));
        }

        [Test]
        public void ItemIcon_OnlyAppearsWhenRelevant()
        {
            Assert.That(
                MobileUiPolicy.IsItemIconVisible(MobileUiFlow.Gameplay, true),
                Is.True);
            Assert.That(
                MobileUiPolicy.IsItemIconVisible(MobileUiFlow.Gameplay, false),
                Is.False);
            Assert.That(
                MobileUiPolicy.IsItemIconVisible(MobileUiFlow.Lobby, true),
                Is.False);
        }

        [TestCase(RunnerColor.Red, RunnerColorSymbol.Circle)]
        [TestCase(RunnerColor.Blue, RunnerColorSymbol.Square)]
        [TestCase(RunnerColor.Green, RunnerColorSymbol.Triangle)]
        public void ColorSymbolMapping_IsStable(
            RunnerColor color,
            RunnerColorSymbol expected)
        {
            Assert.That(MobileUiPolicy.GetSymbol(color), Is.EqualTo(expected));
        }

        private static int CountPrimaryRoots(MobileUiVisibility visibility)
        {
            int count = 0;
            count += visibility.Lobby ? 1 : 0;
            count += visibility.PreRun ? 1 : 0;
            count += visibility.GameplayHud ? 1 : 0;
            count += visibility.ClearResult ? 1 : 0;
            count += visibility.FailedResult ? 1 : 0;
            count += visibility.Development ? 1 : 0;
            return count;
        }

        private static bool[] Cleared(params int[] stages)
        {
            bool[] values = new bool[StageCatalog.Count];
            for (int index = 0; index < stages.Length; index++)
            {
                values[stages[index] - 1] = true;
            }
            return values;
        }
    }
}
