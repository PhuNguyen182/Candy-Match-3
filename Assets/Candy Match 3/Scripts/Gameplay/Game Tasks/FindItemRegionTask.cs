using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class FindItemRegionTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly List<Vector3Int> _adjacentSteps;

        private HashSet<Vector3Int> _findRegionPosition;
        private List<MatchableRegion> _matchableRegions;

        public FindItemRegionTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
            _matchableRegions = new();
            _findRegionPosition = new();
            _adjacentSteps = new()
            {
                new(0, 1), new(0, -1), new(-1, 0), new(1, 0),
            };
        }

        public void CheckMatchRegion(Vector3Int position)
        {
            _findRegionPosition.Add(position);
        }

        public void Cleanup()
        {
            _findRegionPosition.Clear();
            _gridCellManager.ClearVisitStates();
        }

        public List<MatchableRegion> CollectMatchableRegions()
        {
            _matchableRegions.Clear();
            HashSet<Vector3Int> matchPositions = new();

            foreach(Vector3Int position in _findRegionPosition)
            {
                if (IsSelectedPosition(position))
                    continue;

                IGridCell gridCell = _gridCellManager.Get(position);
                if (!IsValidGridCell(gridCell))
                    continue;

                matchPositions.Clear();
                InspectMatchableRegion(position, matchPositions);

                if (matchPositions.Count < 3)
                    continue;

                MatchableRegion region = new()
                {
                    RegionType = gridCell.ItemType,
                    RegionColor = gridCell.CandyColor,
                    Elements = new(matchPositions)
                };

                _matchableRegions.Add(region);
            }

            return _matchableRegions;
        }

        private void InspectMatchableRegion(Vector3Int position, HashSet<Vector3Int> matchPositions)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (!IsValidGridCell(gridCell) || IsSelectedPosition(position))
                return;

            matchPositions.Add(position);
            CandyColor candyColor = gridCell.CandyColor;

            for (int i = 0; i < _adjacentSteps.Count; i++)
            {
                Vector3Int checkPosition = position + _adjacentSteps[i];
                IGridCell checkCell = _gridCellManager.Get(checkPosition);

                if (!IsValidGridCell(checkCell))
                    continue;

                if (matchPositions.Contains(checkPosition))
                    continue;

                if (IsSelectedPosition(checkPosition))
                    continue;

                if (checkCell.CandyColor != candyColor || checkCell.CandyColor == CandyColor.None)
                    continue;

                matchPositions.Add(checkPosition);
                InspectMatchableRegion(checkPosition, matchPositions);
            }
        }

        private bool IsValidGridCell(IGridCell gridCell)
        {
            return gridCell != null && gridCell.HasItem;
        }

        private bool IsSelectedPosition(Vector3Int position)
        {
            foreach (var region in _matchableRegions)
            {
                if (region.IsInRegion(position))
                    return true;
            }

            return false;
        }

        public void Dispose()
        {
            _adjacentSteps.Clear();
            _findRegionPosition.Clear();
            _matchableRegions.Clear();
        }
    }
}
