using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchableRegion : IDisposable
    {
        public ItemType RegionType;
        public List<IGridCell> Elements;

        public MatchableRegion()
        {
            Elements = new();
        }

        public void Add(IGridCell gridCell)
        {
            Elements.Add(gridCell);
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
