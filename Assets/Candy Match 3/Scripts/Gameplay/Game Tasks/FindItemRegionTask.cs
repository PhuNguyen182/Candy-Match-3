using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class FindItemRegionTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly List<Vector3Int> _adjacentSteps;
        private readonly List<Vector3Int> _allPositions;

        public FindItemRegionTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;

            _allPositions = new();
            _adjacentSteps = new()
            {
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
            };
        }

        public void BuildGridBoardData()
        {
            _allPositions.AddRange(_gridCellManager.GetActivePositions());
        }

        public async UniTask MatchAllRegions()
        {
            await UniTask.CompletedTask;
        }

        public List<MatchableRegion> CollectMatchableRegions()
        {
            HashSet<Vector3Int> regionPositions = new();
            List<MatchableRegion> matchableRegions = new();

            for (int i = 0; i < _allPositions.Count; i++)
            {
                IGridCell gridCell = _gridCellManager.Get(_allPositions[i]);

                if (!IsValidGridCell(gridCell))
                    continue;

                regionPositions.Clear(); // Clear this collection to refresh region positions
                InspectMatchableRegion(_allPositions[i], regionPositions);

                if (regionPositions.Count < 3)
                    continue;

                MatchableRegion region = new()
                {
                    RegionType = gridCell.BlockItem.ItemType,
                    RegionColor = gridCell.BlockItem.CandyColor,
                    Elements = regionPositions
                };

                matchableRegions.Add(region);
            }

            return matchableRegions;
        }

        private void InspectMatchableRegion(Vector3Int position, HashSet<Vector3Int> positions)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (!IsValidGridCell(gridCell))
                return;

            positions.Add(position);
            CandyColor candyColor = gridCell.BlockItem.CandyColor;
            _gridCellManager.SetVisitState(position, true);

            for (int i = 0; i < _adjacentSteps.Count; i++)
            {
                Vector3Int checkPosition = position + _adjacentSteps[i];
                IGridCell checkCell = _gridCellManager.Get(checkPosition);

                if (!IsValidGridCell(checkCell))
                    continue;

                if (checkCell.BlockItem.CandyColor == CandyColor.None)
                    continue;

                if (checkCell.BlockItem.CandyColor != candyColor)
                    continue;

                positions.Add(checkPosition);
                _gridCellManager.SetVisitState(checkPosition, true);
                InspectMatchableRegion(checkPosition, positions);
            }
        }

        private void DetectCapitalOfRegion(MatchableRegion region)
        {
            int maxCount = int.MinValue;
            Vector3Int capitalPosition = Vector3Int.zero;

            foreach(Vector3Int position in region.Elements)
            {
                int extendCount = 0;

                for (int j = 0; j < _adjacentSteps.Count; j++)
                {
                    int count = Extend(position, _adjacentSteps[j], region.IsInRegion);
                    extendCount = count + extendCount;
                }

                if(extendCount > maxCount)
                {
                    maxCount = extendCount;
                    capitalPosition = position;
                }
            }

            region.Capital = capitalPosition;
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

        private bool IsValidGridCell(IGridCell gridCell)
        {
            return gridCell != null && gridCell.HasItem && _gridCellManager.GetVisitState(gridCell.GridPosition);
        }

        public void Dispose()
        {
            _adjacentSteps.Clear();
            _allPositions.Clear();
        }
    }
}
