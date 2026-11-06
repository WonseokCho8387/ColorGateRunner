using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class RankingTests
    {
        private GameObject _root;
        private RankPageView _view;

        [SetUp]
        public void CreateView()
        {
            _root = new GameObject("RankTest");
            _view = _root.AddComponent<RankPageView>();
            _view.Configure(MakeText("Status"), MakeText("Body"), MakeText("List"), MakeText("Player"));
        }

        [TearDown]
        public void DestroyView()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void Total_SumsStageBestsWithinWhatAPerfectClearCanScore()
        {
            var progress = new BestScores();
            StageDefinition first = StageCatalog.GetByIndex(0);
            StageDefinition second = StageCatalog.GetByIndex(1);
            progress.Set(first.DisplayNumber, 780);
            progress.Set(second.DisplayNumber, int.MaxValue);
            int expected = 780 + CampaignRankingScore.MaximumFor(second);
            Assert.That(CampaignRankingScore.Total(progress, StageCatalog.Current),
                Is.EqualTo(expected), "An impossible best is clamped.");
            Assert.That(CampaignRankingScore.MaximumFor(first),
                Is.EqualTo(600 + first.TargetGateCount * 10));
        }

        [Test]
        public void Page_SubmitsTheTotalThenShowsTopAndPlayer()
        {
            var gateway = new FakeGateway
            {
                Board = new RankingBoard(
                    new[] { new RankingEntry(1, "Guest#0001", 9000), new RankingEntry(2, "Guest#0042", 4200) },
                    true,
                    new RankingEntry(2, "Guest#0042", 4200))
            };
            _view.Bind(() => 4200, gateway);

            Task refresh = _view.RefreshAsync();

            Assert.That(refresh.IsCompleted, Is.True);
            Assert.That(gateway.Submitted, Is.EqualTo(new[] { 4200 }));
            Assert.That(_view.StatusText.text, Is.EqualTo(RankPageView.OnlineStatus));
            Assert.That(_view.BodyText.text, Does.Contain("Guest#0042"));
            Assert.That(_view.ListText.text, Does.Contain("1.  Guest#0001  9,000"));
            Assert.That(_view.PlayerText.text, Does.Contain("YOU").And.Contain("2.  Guest#0042"));
        }

        [Test]
        public void Page_WithoutScoresSubmitsNothingAndInvitesAClear()
        {
            var gateway = new FakeGateway
            {
                Board = new RankingBoard(Array.Empty<RankingEntry>(), false, default)
            };
            _view.Bind(() => 0, gateway);
            _view.RefreshAsync();
            Assert.That(gateway.Submitted, Is.Empty);
            Assert.That(_view.ListText.text, Does.Contain("NO SCORES YET"));
            Assert.That(_view.PlayerText.text, Does.Contain("CLEAR A STAGE"));
        }

        [Test]
        public void Page_FailureAndNoServiceShowTruthfulOffline()
        {
            _view.Bind(() => 100, new FakeGateway { Fail = true });
            _view.RefreshAsync();
            Assert.That(_view.StatusText.text, Is.EqualTo(RankPageView.OfflineStatus));
            Assert.That(_view.BodyText.text, Is.EqualTo(RankPageView.OfflineBody));
            Assert.That(_view.Loading, Is.False);

            _view.Bind(() => 100, null);
            _view.RefreshAsync();
            Assert.That(_view.StatusText.text, Is.EqualTo(RankPageView.OfflineStatus));
            Assert.That(_view.ListText.text, Is.Empty);
        }

        private Text MakeText(string name)
        {
            var child = new GameObject(name, typeof(RectTransform), typeof(Text));
            child.transform.SetParent(_root.transform, false);
            return child.GetComponent<Text>();
        }

        private sealed class FakeGateway : IRankingGateway
        {
            internal RankingBoard Board;
            internal bool Fail;
            internal readonly List<int> Submitted = new List<int>();

            public Task<string> SignInAsync() => Fail
                ? Task.FromException<string>(new InvalidOperationException("offline"))
                : Task.FromResult("Guest#0042");

            public Task SubmitTotalAsync(int total)
            {
                Submitted.Add(total);
                return Task.CompletedTask;
            }

            public Task<RankingBoard> LoadAsync(int topCount) => Task.FromResult(Board);
        }

        private sealed class BestScores : IStageProgressReader
        {
            private readonly Dictionary<int, int> _best = new Dictionary<int, int>();

            internal void Set(int stageNumber, int best) => _best[stageNumber] = best;

            public int LoadHighestUnlocked() => 1;

            public StageRecord LoadRecord(int stageNumber) =>
                _best.TryGetValue(stageNumber, out int best)
                    ? new StageRecord(true, 10f, 10f, 1, 0, 0, StageCrown.None, 0, best)
                    : default;
        }
    }
}
