using System;
using NUnit.Framework;
using UnityEngine;

namespace ColorGateRunner.Tests.PlayMode
{
    public sealed class CampaignPlayerPrefsSnapshotTests
    {
        private CampaignPlayerPrefsSnapshot _editorProgressSnapshot;

        [SetUp]
        public void SetUp()
        {
            _editorProgressSnapshot =
                CampaignPlayerPrefsSnapshot.Capture();
        }

        [TearDown]
        public void TearDown()
        {
            _editorProgressSnapshot?.Dispose();
            _editorProgressSnapshot = null;
        }

        [Test]
        public void Restore_PreservesExistingIntAndStringAndDeletesNewKey()
        {
            string existingRecord =
                CampaignPlayerPrefsSnapshot.RecordKey(2);
            string absentRecord =
                CampaignPlayerPrefsSnapshot.RecordKey(3);
            PlayerPrefs.SetInt(
                CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                9);
            PlayerPrefs.SetString(existingRecord, "1|12.5|13.5|4|1");
            PlayerPrefs.DeleteKey(absentRecord);
            PlayerPrefs.Save();

            using (CampaignPlayerPrefsSnapshot snapshot =
                CampaignPlayerPrefsSnapshot.Capture())
            {
                PlayerPrefs.SetInt(
                    CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                    2);
                PlayerPrefs.SetString(existingRecord, "0|0|0|0|0");
                PlayerPrefs.SetString(absentRecord, "1|1|1|1|1");
                PlayerPrefs.Save();
            }

            Assert.That(
                PlayerPrefs.GetInt(
                    CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                    -1),
                Is.EqualTo(9));
            Assert.That(
                PlayerPrefs.GetString(existingRecord, string.Empty),
                Is.EqualTo("1|12.5|13.5|4|1"));
            Assert.That(PlayerPrefs.HasKey(absentRecord), Is.False);
        }

        [Test]
        public void Dispose_RestoresAllThirteenRecords()
        {
            for (int stageNumber = 1;
                stageNumber <= CampaignPlayerPrefsSnapshot.StageCount;
                stageNumber++)
            {
                PlayerPrefs.SetString(
                    CampaignPlayerPrefsSnapshot.RecordKey(stageNumber),
                    $"1|{stageNumber}|{stageNumber}|1|0");
            }
            PlayerPrefs.Save();
            CampaignPlayerPrefsSnapshot snapshot =
                CampaignPlayerPrefsSnapshot.Capture();

            for (int stageNumber = 1;
                stageNumber <= CampaignPlayerPrefsSnapshot.StageCount;
                stageNumber++)
            {
                PlayerPrefs.DeleteKey(
                    CampaignPlayerPrefsSnapshot.RecordKey(stageNumber));
            }
            PlayerPrefs.Save();
            snapshot.Dispose();

            Assert.That(snapshot.MatchesCurrentState(), Is.True);
        }

        [Test]
        public void Dispose_RestoresAfterException()
        {
            string recordKey = CampaignPlayerPrefsSnapshot.RecordKey(7);
            PlayerPrefs.SetInt(
                CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                7);
            PlayerPrefs.SetString(recordKey, "1|21|22|3|0");
            PlayerPrefs.Save();

            Assert.Throws<InvalidOperationException>(() =>
            {
                using (CampaignPlayerPrefsSnapshot snapshot =
                    CampaignPlayerPrefsSnapshot.Capture())
                {
                    PlayerPrefs.SetInt(
                        CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                        1);
                    PlayerPrefs.DeleteKey(recordKey);
                    PlayerPrefs.Save();
                    throw new InvalidOperationException("planned");
                }
            });

            Assert.That(
                PlayerPrefs.GetInt(
                    CampaignPlayerPrefsSnapshot.HighestUnlockedKey,
                    -1),
                Is.EqualTo(7));
            Assert.That(
                PlayerPrefs.GetString(recordKey, string.Empty),
                Is.EqualTo("1|21|22|3|0"));
        }
    }
}
