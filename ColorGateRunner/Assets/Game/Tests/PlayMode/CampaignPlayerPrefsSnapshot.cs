using System;
using UnityEngine;

namespace ColorGateRunner.Tests.PlayMode
{
    internal sealed class CampaignPlayerPrefsSnapshot : IDisposable
    {
        internal const string HighestUnlockedKey =
            "ColorGateRunner.Stage.HighestUnlocked";
        internal const string RecordKeyPrefix =
            "ColorGateRunner.Stage.Record.";
        internal const int StageCount = 20;

        private readonly IntEntry _highestUnlocked;
        private readonly StringEntry[] _records;
        private bool _restored;

        private CampaignPlayerPrefsSnapshot(
            IntEntry highestUnlocked,
            StringEntry[] records)
        {
            _highestUnlocked = highestUnlocked;
            _records = records;
        }

        internal static CampaignPlayerPrefsSnapshot Capture()
        {
            var records = new StringEntry[StageCount];
            for (int stageNumber = 1;
                stageNumber <= StageCount;
                stageNumber++)
            {
                string key = RecordKey(stageNumber);
                records[stageNumber - 1] = new StringEntry(
                    key,
                    PlayerPrefs.HasKey(key),
                    PlayerPrefs.GetString(key, string.Empty));
            }

            return new CampaignPlayerPrefsSnapshot(
                new IntEntry(
                    HighestUnlockedKey,
                    PlayerPrefs.HasKey(HighestUnlockedKey),
                    PlayerPrefs.GetInt(HighestUnlockedKey, 1)),
                records);
        }

        internal static string RecordKey(int stageNumber)
        {
            if (stageNumber < 1 || stageNumber > StageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(stageNumber));
            }

            return RecordKeyPrefix + stageNumber;
        }

        internal bool MatchesCurrentState()
        {
            if (!_highestUnlocked.MatchesCurrentState())
            {
                return false;
            }

            for (int index = 0; index < _records.Length; index++)
            {
                if (!_records[index].MatchesCurrentState())
                {
                    return false;
                }
            }

            return true;
        }

        internal void Restore()
        {
            if (_restored)
            {
                return;
            }

            _highestUnlocked.Restore();
            for (int index = 0; index < _records.Length; index++)
            {
                _records[index].Restore();
            }

            PlayerPrefs.Save();
            _restored = true;
        }

        public void Dispose()
        {
            Restore();
        }

        private readonly struct IntEntry
        {
            private readonly string _key;
            private readonly bool _existed;
            private readonly int _value;

            internal IntEntry(string key, bool existed, int value)
            {
                _key = key;
                _existed = existed;
                _value = value;
            }

            internal bool MatchesCurrentState()
            {
                return _existed
                    ? PlayerPrefs.HasKey(_key) &&
                        PlayerPrefs.GetInt(_key, int.MinValue) == _value
                    : !PlayerPrefs.HasKey(_key);
            }

            internal void Restore()
            {
                if (_existed)
                {
                    PlayerPrefs.SetInt(_key, _value);
                }
                else
                {
                    PlayerPrefs.DeleteKey(_key);
                }
            }
        }

        private readonly struct StringEntry
        {
            private readonly string _key;
            private readonly bool _existed;
            private readonly string _value;

            internal StringEntry(string key, bool existed, string value)
            {
                _key = key;
                _existed = existed;
                _value = value;
            }

            internal bool MatchesCurrentState()
            {
                return _existed
                    ? PlayerPrefs.HasKey(_key) &&
                        PlayerPrefs.GetString(_key, null) == _value
                    : !PlayerPrefs.HasKey(_key);
            }

            internal void Restore()
            {
                if (_existed)
                {
                    PlayerPrefs.SetString(_key, _value);
                }
                else
                {
                    PlayerPrefs.DeleteKey(_key);
                }
            }
        }
    }
}
