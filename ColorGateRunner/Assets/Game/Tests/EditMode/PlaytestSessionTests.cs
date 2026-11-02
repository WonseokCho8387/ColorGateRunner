using ColorGateRunner.Core;
using ColorGateRunner.Product;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class PlaytestSessionTests
    {
        [Test]
        public void Route_UsesExistingStagesInLearningOrder_AndStopsAtEight()
        {
            var session = new PlaytestSession();
            string[] expected = { "stage-01", "stage-02", "stage-03", "stage-06",
                "stage-09", "stage-21", "stage-18", "stage-24" };
            foreach (string id in expected)
            {
                Assert.That(session.StageId, Is.EqualTo(id));
                Assert.That(StageCatalog.GetById(id), Is.Not.Null);
                Assert.That(session.Advance(), Is.False);
                Assert.That(session.CompleteStage(id), Is.False);
                Assert.That(session.BeginAttempt(), Is.True);
                Assert.That(session.BeginAttempt(), Is.False);
                Assert.That(session.CompleteStage("wrong-stage"), Is.False);
                Assert.That(session.CompleteStage(id), Is.True);
                Assert.That(session.CompleteStage(id), Is.False);
                Assert.That(session.Retry(), Is.False);
                Assert.That(session.Advance(), Is.EqualTo(id != "stage-24"));
            }
            Assert.That(session.Completed, Is.True);
            Assert.That(session.Position, Is.EqualTo(7));
        }

        [Test]
        public void Retry_HasNoBudget_AndRequiresANewAttempt()
        {
            var session = new PlaytestSession();
            for (int attempt = 0; attempt < 25; attempt++)
            {
                Assert.That(session.BeginAttempt(), Is.True);
                Assert.That(session.Retry(), Is.True);
                Assert.That(session.Retry(), Is.False);
                Assert.That(session.StageId, Is.EqualTo("stage-01"));
            }
            Assert.That(session.RetryCount, Is.EqualTo(25));
        }

        [Test]
        public void Tutorial_RequiresInputThenDirectMatchAndClearInSuccessfulAttempt()
        {
            var session = new PlaytestSession();
            session.RecordInput();
            Assert.That(session.HasFirstInput, Is.False);
            session.BeginAttempt();
            session.RecordInput();
            session.RecordDirectMatch();
            Assert.That(session.TutorialCompleted, Is.False);
            session.Retry();
            session.BeginAttempt();
            session.CompleteStage(session.StageId);
            Assert.That(session.TutorialCompleted, Is.False);
            session.Restart();
            session.BeginAttempt();
            session.RecordDirectMatch();
            session.RecordInput();
            session.RecordDirectMatch();
            session.CompleteStage(session.StageId);
            Assert.That(session.TutorialCompleted, Is.True);
        }

        [Test]
        public void MemorySave_ClonesState_AndDoesNotPersistAcrossInstances()
        {
            var save = new PlaytestMemorySaveService();
            LocalSaveData data = LocalSaveData.CreateEmpty();
            save.Save(data);
            var first = save.Load().Data;
            Assert.That(first, Is.Not.SameAs(data));
            Assert.That(save.Load().Data, Is.Not.SameAs(first));
            Assert.That(new PlaytestMemorySaveService().Load().Succeeded, Is.True);
        }
    }
}
