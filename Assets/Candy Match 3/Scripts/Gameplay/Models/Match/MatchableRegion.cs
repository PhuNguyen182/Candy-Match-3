using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Constants;
using GlobalScripts.Extensions;
using GlobalScripts.Comparers;

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

    public class MatchableRegion : IDisposable
    {
        public ItemType RegionType;
        public CandyColor RegionColor;
        public HashSet<Vector3Int> Elements;

        public MatchableRegion()
        {
            Elements = new();
        }

        public bool IsInRegion(Vector3Int position)
        {
            return Elements.Contains(position);
        }

        public List<Vector3Int> GetSortedElements()
        {
            Vector3IntComparer comparer = new();
            List<Vector3Int> positions = Elements.ToList();
            positions.Sort(comparer);
            return positions;
        }

        public BoundsInt GetRegionBounds()
        {
            return BoundsExtension.EncapsulateExpand(Elements);
        }

        public int GetExtendablePositionCount(Vector3Int position)
        {
            int extendCount = 0;

            for (int i = 0; i < Match3Constants.AdjacentSteps.Count; i++)
            {
                Vector3Int adjacentStep = Match3Constants.AdjacentSteps[i];
                int count = GetExtendedLength(position, adjacentStep);
                extendCount = count + extendCount;
            }

            return extendCount;
        }

        public int GetExtendedLength(Vector3Int startPosition, Vector3Int direction)
        {
            int count = 0;
            Vector3Int extendPosition = startPosition + direction;

            while (IsInRegion(extendPosition))
            {
                extendPosition = extendPosition + direction;
                count = count + 1;
            }

            return count;
        }

        public List<Vector3Int> GetHorizontalExtendedPositions(Vector3Int position)
        {
            List<Vector3Int> positions = new();

            var left = GetExtendedPositions(position, Vector3Int.left);
            if (left.Count > 0)
                positions.AddRange(left);

            var right = GetExtendedPositions(position, Vector3Int.right);
            if (right.Count > 0)
                positions.AddRange(right);

            return positions;
        }

        public List<Vector3Int> GetVerticalExtendedPositions(Vector3Int position)
        {
            List<Vector3Int> positions = new();

            var down = GetExtendedPositions(position, Vector3Int.down);
            if(down.Count > 0)
                positions.AddRange(down);
            
            var up = GetExtendedPositions(position, Vector3Int.up);
            if(up.Count > 0)
                positions.AddRange(up);
            
            return positions;
        }

        public List<Vector3Int> GetExtendedPositions(Vector3Int startPosition, Vector3Int direction)
        {
            List<Vector3Int> positions = new();
            Vector3Int extendPosition = startPosition + direction;

            while (IsInRegion(extendPosition))
            {
                positions.Add(extendPosition);
                extendPosition = extendPosition + direction;
            }

            return positions;
        }

        public void Dispose()
        {
            Elements?.Clear();
        }
    }
}
