using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal interface IBestScoreStore
    {
        int Load();

        void Save(int score);
    }

    internal sealed class PlayerPrefsBestScoreStore : IBestScoreStore
    {
        private const string BestScoreKey = "ColorGateRunner.BestScore";

        public int Load()
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(BestScoreKey, 0));
        }

        public void Save(int score)
        {
            PlayerPrefs.SetInt(BestScoreKey, Mathf.Max(0, score));
            PlayerPrefs.Save();
        }
    }
}
