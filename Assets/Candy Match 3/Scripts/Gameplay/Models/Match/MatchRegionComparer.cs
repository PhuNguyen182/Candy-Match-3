using System.Collections.Generic;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchRegionComparer : IComparer<MatchableRegion>
    {
        public int Compare(MatchableRegion x, MatchableRegion y)
        {
            return x.Score.CompareTo(y.Score);
        }
    }
}
