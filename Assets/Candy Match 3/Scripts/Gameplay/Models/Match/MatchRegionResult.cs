using System;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchRegionResult : IDisposable
    {
        public MatchType MatchType;
        public CandyColor CandyColor;
        public Vector3Int PivotPosition;
        public HashSet<Vector3Int> Positions;

        public void Dispose()
        {
            Positions?.Clear();
        }
    }
}
