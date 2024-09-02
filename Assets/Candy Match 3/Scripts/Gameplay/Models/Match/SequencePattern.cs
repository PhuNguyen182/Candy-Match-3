using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class SequencePattern : IDisposable
    {
        public List<Vector3Int> Pattern { get; }

        public SequencePattern(List<Vector3Int> pattern)
        {
            Pattern = pattern;
        }

        public void Dispose()
        {
            Pattern.Clear();
        }
    }
}
