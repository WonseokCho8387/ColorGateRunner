using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class LobbyConsolidationEditModeTests
    {
        [Test]
        public void FrontendReader_ReusesLobbyProgressionAndReturnsStableStageId()
        {
            var progress = new MemoryProgressReader(8);
            for (int stage = 1; stage <= 5; stage++)
            {
                progress.SetCleared(stage);
            }

            CampaignLobbyReadModel model =
                new FrontendCampaignProgressReader(
                    progress,
                    StageCatalog.Current).Read();

            Assert.That(model.DisplayNumber, Is.EqualTo(6));
            Assert.That(model.StageId,
                Is.EqualTo(StageCatalog.GetByDisplayNumber(6).StageId));
            Assert.That(model.Title, Is.EqualTo("SHIELD TRAINING"));
            Assert.That(model.MechanicLabel, Is.EqualTo("SHIELD"));
            Assert.That(model.ClearedCount, Is.EqualTo(5));
            Assert.That(progress.WriteCount, Is.Zero);
        }

        [Test]
        public void FrontendReader_WhenUnlockedStagesAreClearedSelectsHighest()
        {
            var progress = new MemoryProgressReader(3);
            progress.SetCleared(1);
            progress.SetCleared(2);
            progress.SetCleared(3);

            CampaignLobbyReadModel model =
                new FrontendCampaignProgressReader(
                    progress,
                    StageCatalog.Current).Read();

            Assert.That(model.DisplayNumber, Is.EqualTo(3));
            Assert.That(model.StageId, Is.EqualTo("stage-03"));
        }

        [Test]
        public void LaunchContext_IsNonPersistentOneShotAndCancellationIsExact()
        {
            var context = new CampaignLaunchContext();

            Assert.That(context.TrySet("stage-06"), Is.True);
            Assert.That(context.TrySet("stage-07"), Is.False);
            Assert.That(context.TryCancel("stage-07"), Is.False);
            Assert.That(context.TryConsume(out CampaignLaunchRequest request),
                Is.True);
            Assert.That(request.StageId, Is.EqualTo("stage-06"));
            Assert.That(context.TryConsume(out _), Is.False);

            Assert.That(context.TrySet("stage-08"), Is.True);
            Assert.That(context.TryCancel("stage-08"), Is.True);
            Assert.That(context.HasPending, Is.False);
        }

        [TestCase("https://example.com/terms", true)]
        [TestCase("http://example.com/terms", false)]
        [TestCase("javascript:alert(1)", false)]
        [TestCase("", false)]
        public void ProductLinks_AllowOnlyAbsoluteHttps(
            string candidate,
            bool expected)
        {
            ProductLinkConfiguration configuration =
                ScriptableObject.CreateInstance<ProductLinkConfiguration>();
            try
            {
                configuration.ConfigureForTests(candidate, string.Empty,
                    string.Empty);

                Assert.That(
                    configuration.TryGetHttpsUrl(
                        ProductLinkType.Terms,
                        out _),
                    Is.EqualTo(expected));
            }
            finally
            {
                Object.DestroyImmediate(configuration);
            }
        }

        private sealed class MemoryProgressReader : IStageProgressReader
        {
            private readonly bool[] _cleared =
                new bool[StageCatalog.Count];

            internal MemoryProgressReader(int highestUnlocked)
            {
                HighestUnlocked = highestUnlocked;
            }

            internal int HighestUnlocked { get; }
            internal int WriteCount { get; private set; }

            internal void SetCleared(int stageNumber)
            {
                _cleared[stageNumber - 1] = true;
            }

            public int LoadHighestUnlocked() => HighestUnlocked;

            public StageRecord LoadRecord(int stageNumber)
            {
                return new StageRecord(
                    _cleared[stageNumber - 1],
                    0f,
                    0f,
                    _cleared[stageNumber - 1] ? 1 : 0);
            }
        }
    }
}
