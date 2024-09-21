using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchableRegion : IDisposable
    {
        public int Score;
        public ItemType RegionType;
        public CandyColor RegionColor;
        public HashSet<Vector3Int> Elements;
        public Vector3Int Capital;

        public MatchableRegion()
        {
            Elements = new();
        }

        public bool IsInRegion(Vector3Int position)
        {
            return Elements.Contains(position);
        }

        public void Dispose()
        {
            Elements.Clear();
        }
    }
}
