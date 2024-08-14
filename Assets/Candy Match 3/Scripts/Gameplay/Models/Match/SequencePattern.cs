using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class SequencePattern
    {
        public List<Vector3Int> Pattern { get; }

        public SequencePattern(List<Vector3Int> pattern)
        {
            Pattern = pattern;
        }
    }
}
