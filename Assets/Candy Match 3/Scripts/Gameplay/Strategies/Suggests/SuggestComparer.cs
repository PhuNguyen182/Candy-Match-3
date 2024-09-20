using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Strategies.Suggests
{
    public class SuggestComparer : IComparer<AvailableSuggest>
    {
        public int Compare(AvailableSuggest x, AvailableSuggest y)
        {
            return x.Score.CompareTo(y.Score);
        }
    }
}
