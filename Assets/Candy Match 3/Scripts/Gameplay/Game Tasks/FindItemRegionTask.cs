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

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class FindItemRegionTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly List<Vector3Int> _adjacentSteps;

        private HashSet<Vector3Int> _findRegionPosition;

        public FindItemRegionTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;

            _findRegionPosition = new();
            _adjacentSteps = new()
            {
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
            };
        }

        public void CheckRegion(Vector3Int position)
        {
            _findRegionPosition.Add(position);
        }

        public List<MatchableRegion> CollectMatchableRegions()
        {
            HashSet<Vector3Int> regionPositions = new();
            List<MatchableRegion> matchableRegions = new();

            foreach (Vector3Int position in _findRegionPosition)
            {
                IGridCell gridCell = _gridCellManager.Get(position);

                if (!IsValidGridCell(gridCell))
                    continue;

                regionPositions.Clear(); // Clear this collection to refresh region positions
                InspectMatchableRegion(position, regionPositions);

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

            _findRegionPosition.Clear();
            _gridCellManager.ClearVisitStates();
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

        private bool IsValidGridCell(IGridCell gridCell)
        {
            return gridCell != null && gridCell.HasItem && _gridCellManager.GetVisitState(gridCell.GridPosition);
        }

        public void Dispose()
        {
            _adjacentSteps.Clear();
            _findRegionPosition.Clear();
        }
    }
}
