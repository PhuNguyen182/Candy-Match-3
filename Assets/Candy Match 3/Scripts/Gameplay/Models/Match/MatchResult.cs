using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public struct MatchResult
    {
        public MatchType MatchType;
        public Vector3Int Position;
        public List<Vector3Int> MatchSequence;
    }
}
