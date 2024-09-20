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
        public List<Vector3Int> Elements;

        public MatchableRegion()
        {
            Elements = new();
        }

        public void Add(Vector3Int position)
        {
            Elements.Add(position);
        }

        public int Count()
        {
            return Elements.Count;
        }

        public void Dispose()
        {
            Elements.Clear();
        }
    }
}
