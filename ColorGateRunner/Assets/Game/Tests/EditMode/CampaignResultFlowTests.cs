using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignResultFlowTests
    {
        [Test]
        public void ClearFlow_RequiresCelebrationBeforeRewards()
        {
            var flow = new CampaignResultFlow();

            flow.BeginClear();

            Assert.That(flow.CurrentPage,
                Is.EqualTo(CampaignResultPage.ClearCelebration));
            Assert.That(flow.CompleteClearCelebration(), Is.True);
            Assert.That(flow.CurrentPage,
                Is.EqualTo(CampaignResultPage.ClearRewards));
            Assert.That(flow.CompleteClearCelebration(), Is.False);
        }

        [Test]
        public void FailureExit_CanCancelWithoutLosingContinueOffer()
        {
            var flow = new CampaignResultFlow();
            flow.BeginFailure();

            Assert.That(flow.RequestFailureExit(), Is.True);
            Assert.That(flow.CurrentPage,
                Is.EqualTo(CampaignResultPage.FailureExitConfirmation));
            Assert.That(flow.CancelFailureExit(), Is.True);
            Assert.That(flow.CurrentPage,
                Is.EqualTo(CampaignResultPage.FailureContinue));
        }

        [Test]
        public void EmptyConsequenceQueue_SkipsDirectlyToFinalChoice()
        {
            var flow = new CampaignResultFlow();
            var queue = new FailureConsequenceQueue();
            flow.BeginFailure();
            flow.RequestFailureExit();

            flow.ConfirmFailureExit(queue);

            Assert.That(flow.CurrentPage,
                Is.EqualTo(CampaignResultPage.FailureFinalChoice));
        }

        [Test]
        public void ConsequenceQueue_PreservesFuturePageOrder()
        {
            var flow = new CampaignResultFlow();
            var queue = new FailureConsequenceQueue();
            queue.Add(new FailureConsequencePage(
                "streak",
                "STREAK ENDED",
                "Your streak ended."));
            queue.Add(new FailureConsequencePage(
                "event",
                "EVENT RESULT",
                "Event progress changed."));
            flow.BeginFailure();
            flow.RequestFailureExit();

            flow.ConfirmFailureExit(queue);
            Assert.That(queue.TryGetCurrent(out FailureConsequencePage first),
                Is.True);
            Assert.That(first.Id, Is.EqualTo("streak"));

            flow.AdvanceFailureConsequence(queue);
            Assert.That(queue.TryGetCurrent(out FailureConsequencePage second),
                Is.True);
            Assert.That(second.Id, Is.EqualTo("event"));

            flow.AdvanceFailureConsequence(queue);
            Assert.That(flow.CurrentPage,
                Is.EqualTo(CampaignResultPage.FailureFinalChoice));
        }

        [TestCase(10, 0, 9)]
        [TestCase(10, 8, 1)]
        [TestCase(10, 9, 0)]
        [TestCase(10, 12, 0)]
        public void RemainingGates_ExcludesGateResolvedByContinue(
            int target,
            int passed,
            int expected)
        {
            Assert.That(
                CampaignResultFlow.RemainingGatesAfterContinue(target, passed),
                Is.EqualTo(expected));
        }
    }
}
