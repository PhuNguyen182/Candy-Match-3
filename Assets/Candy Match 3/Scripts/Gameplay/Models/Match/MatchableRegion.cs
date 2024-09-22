using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using GlobalScripts.Extensions;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchableRegion : IDisposable
    {
        public int Score;
        public ItemType RegionType;
        public CandyColor RegionColor;
        public HashSet<Vector3Int> Elements;

        private Vector3Int _capital = new(int.MinValue, int.MaxValue);

        public Vector3Int Capital
        {
            get
            {
                if (_capital == new Vector3Int(int.MinValue, int.MaxValue))
                    _capital = DetectCapitalOfRegion();

                return _capital;
            }
        }

        public MatchableRegion()
        {
            Elements = new();
        }

        public bool IsInRegion(Vector3Int position)
        {
            return Elements.Contains(position);
        }

        public BoundsInt GetRegionBounds()
        {
            return BoundsExtension.EncapsulateExpand(Elements);
        }

        private Vector3Int DetectCapitalOfRegion()
        {
            int maxCount = int.MinValue;
            Vector3Int capitalPosition = Vector3Int.zero;
            List<Vector3Int> adjacentSteps = GetAjacentStep();

            foreach (Vector3Int position in Elements)
            {
                int extendCount = 0;

                for (int j = 0; j < adjacentSteps.Count; j++)
                {
                    int count = Extend(position, adjacentSteps[j], IsInRegion);
                    extendCount = count + extendCount;
                }

                if (extendCount > maxCount)
                {
                    maxCount = extendCount;
                    capitalPosition = position;
                }
            }

            return capitalPosition;
        }

        private int Extend(Vector3Int startPosition, Vector3Int direction, Func<Vector3Int, bool> predicate)
        {
            int count = 0;
            Vector3Int extendPosition = startPosition + direction;

            while (predicate(extendPosition))
            {
                extendPosition = extendPosition + direction;
                count = count + 1;
            }

            return count;
        }

        private List<Vector3Int> GetAjacentStep()
        {
            return new()
            {
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.down
            };
        }

        public void Dispose()
        {
            Elements.Clear();
        }
    }
}
