using System;

namespace CandyMatch3.Scripts.Gameplay.Models
{
    [Serializable]
    public class ScoreRule : IDisposable
    {
        public int MaxScore;
        public int Star1Score;
        public int Star2Score;
        public int Star3Score;

        public void Dispose()
        {
            MaxScore = 0;
            Star1Score = 0;
            Star2Score = 0;
            Star3Score = 0;
        }
    }
}
