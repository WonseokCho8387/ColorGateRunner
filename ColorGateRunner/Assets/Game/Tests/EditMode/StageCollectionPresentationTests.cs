using System.Collections.Generic;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class StageCollectionPresentationTests
    {
        [Test]
        public void Create_ListsEveryCatalogStageWithClearedNextAndLockedStates()
        {
            var progress = new RecordReader(3);
            progress.Set(1, Cleared(StageCrown.Clean, 31.456f, 7));
            progress.Set(2, Cleared(StageCrown.None, 40f, 2));

            StageCollectionModel model = StageCollectionPresentation.Create(
                progress,
                StageCatalog.Current);

            Assert.That(model.StageCount, Is.EqualTo(StageCatalog.Count));
            Assert.That(model.MaxCrownCount, Is.EqualTo(StageCatalog.Count * 3));
            for (int index = 0; index < model.Rows.Count; index++)
            {
                StageDefinition stage = StageCatalog.Current.GetByIndex(index);
                Assert.That(model.Rows[index].StageId, Is.EqualTo(stage.StageId));
                Assert.That(model.Rows[index].DisplayNumber,
                    Is.EqualTo(stage.DisplayNumber));
                Assert.That(model.Rows[index].Title, Is.EqualTo(stage.Title));
            }
            Assert.That(model.Rows[0].State, Is.EqualTo(StageCollectionRowState.Cleared));
            Assert.That(model.Rows[0].CanReplay, Is.True);
            Assert.That(model.Rows[1].State, Is.EqualTo(StageCollectionRowState.Cleared));
            Assert.That(model.Rows[2].State, Is.EqualTo(StageCollectionRowState.Next));
            Assert.That(model.Rows[2].CanReplay, Is.False);
            Assert.That(model.Rows[3].State, Is.EqualTo(StageCollectionRowState.Locked));
            Assert.That(model.Rows[3].CanReplay, Is.False);
            Assert.That(progress.LoadCount, Is.EqualTo(StageCatalog.Count));
        }

        [Test]
        public void Create_CountsDisplayedCrownsAndComebackSeparately()
        {
            var progress = new RecordReader(StageCatalog.Count);
            progress.Set(1, Cleared(
                StageCrown.Clean | StageCrown.Combo | StageCrown.FullCombo,
                20f,
                14));
            progress.Set(2, Cleared(StageCrown.Combo | StageCrown.Comeback, 21f, 11));
            progress.Set(3, Cleared(StageCrown.Comeback, 22f, 0));

            StageCollectionModel model = StageCollectionPresentation.Create(
                progress,
                StageCatalog.Current);

            Assert.That(model.CleanCount, Is.EqualTo(1));
            Assert.That(model.ComboCount, Is.EqualTo(2));
            Assert.That(model.FullComboCount, Is.EqualTo(1));
            Assert.That(model.CrownCount, Is.EqualTo(4));
            Assert.That(model.ComebackCount, Is.EqualTo(2));
            Assert.That(StageCollectionPresentation.FormatCrownTotal(model),
                Is.EqualTo($"CROWNS 4 / {StageCatalog.Count * 3}"));
            Assert.That(StageCollectionPresentation.FormatBreakdown(model),
                Is.EqualTo("CLEAN 1  ·  COMBO 2  ·  " +
                    "FULL COMBO 1  /  " +
                    StageCatalog.Count));
            Assert.That(StageCollectionPresentation.FormatComeback(model),
                Is.EqualTo("COMEBACK CLEARS 2"));
            Assert.That(model.Rows[0].ComboTarget, Is.EqualTo(10));
            Assert.That(model.Rows[AuthoredStage.Number(11) - 1].ComboTarget, Is.EqualTo(20));
            Assert.That(model.Rows[19].ComboTarget, Is.EqualTo(30));
        }

        [Test]
        public void CrownLabel_NamesTheStageComboTarget()
        {
            Assert.That(StageCrownResultView.Label(0, 20), Is.EqualTo("CLEAN"));
            Assert.That(StageCrownResultView.Label(1, 20), Is.EqualTo("20 COMBO"));
            Assert.That(StageCrownResultView.Label(1, 30), Is.EqualTo("30 COMBO"));
            Assert.That(StageCrownResultView.Label(2, 20), Is.EqualTo("FULL COMBO"));
        }

        [Test]
        public void Create_IgnoresUnknownBitsAndAchievementsOnUnclearedRecords()
        {
            var progress = new RecordReader(2);
            progress.Set(1, Cleared((StageCrown)(1 << 6) | StageCrown.Clean, 12f, 3));
            progress.Set(2, new StageRecord(
                false, 0f, 0f, 0, 0, 25, StageCrown.FullCombo, 9));

            StageCollectionModel model = StageCollectionPresentation.Create(
                progress,
                StageCatalog.Current);

            Assert.That(model.Rows[0].Crowns, Is.EqualTo(StageCrown.Clean));
            Assert.That(model.Rows[1].State, Is.EqualTo(StageCollectionRowState.Next));
            Assert.That(model.Rows[1].Crowns, Is.EqualTo(StageCrown.None));
            Assert.That(model.Rows[1].BestMaxCombo, Is.Zero);
            Assert.That(model.CrownCount, Is.EqualTo(1));
            Assert.That(model.FullComboCount, Is.Zero);
        }

        [Test]
        public void Format_DescribesEachRowStateTruthfully()
        {
            var progress = new RecordReader(2);
            progress.Set(1, Cleared(StageCrown.Comeback, 31.456f, 7));

            StageCollectionModel model = StageCollectionPresentation.Create(
                progress,
                StageCatalog.Current);

            Assert.That(StageCollectionPresentation.FormatTitle(model.Rows[0]),
                Is.EqualTo($"STAGE 01  {model.Rows[0].Title}"));
            Assert.That(StageCollectionPresentation.FormatDetail(model.Rows[0]),
                Is.EqualTo("BEST 31.46s  ·  COMBO 7  ·  COMEBACK"));
            Assert.That(StageCollectionPresentation.FormatDetail(model.Rows[1]),
                Is.EqualTo("NOT CLEARED YET"));
            Assert.That(StageCollectionPresentation.FormatDetail(model.Rows[2]),
                Is.EqualTo("LOCKED"));
        }

        [Test]
        public void Create_DoesNotOfferReplayAboveTheUnlockedStage()
        {
            var progress = new RecordReader(1);
            progress.Set(1, Cleared(StageCrown.Clean, 10f, 1));
            progress.Set(3, Cleared(StageCrown.Clean, 10f, 1));

            StageCollectionModel model = StageCollectionPresentation.Create(
                progress,
                StageCatalog.Current);

            Assert.That(model.Rows[0].CanReplay, Is.True);
            Assert.That(model.Rows[2].State,
                Is.EqualTo(StageCollectionRowState.Cleared));
            Assert.That(model.Rows[2].CanReplay, Is.False);
        }

        private static StageRecord Cleared(StageCrown crowns, float time, int combo)
        {
            return new StageRecord(true, time, time, 1, 0, combo, crowns, 0);
        }

        private sealed class RecordReader : IStageProgressReader
        {
            private readonly Dictionary<int, StageRecord> _records =
                new Dictionary<int, StageRecord>();
            private readonly int _highestUnlocked;

            internal RecordReader(int highestUnlocked)
            {
                _highestUnlocked = highestUnlocked;
            }

            internal int LoadCount { get; private set; }

            internal void Set(int stageNumber, StageRecord record)
            {
                _records[stageNumber] = record;
            }

            public int LoadHighestUnlocked() => _highestUnlocked;

            public StageRecord LoadRecord(int stageNumber)
            {
                LoadCount++;
                return _records.TryGetValue(stageNumber, out StageRecord record)
                    ? record
                    : new StageRecord(false, 0f, 0f, 0);
            }
        }
    }
}
