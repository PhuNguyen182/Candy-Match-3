using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public abstract class BaseMatchModel : IDisposable
    {
        protected readonly GridCellManager gridCellManager;

        /// <summary>
        /// This property can be rotated, so do not list all cases in this collection
        /// </summary>
        protected abstract List<SequencePosition> matchCellPositions { get; }
        protected int[] _checkAngles = new[] { 0, 90, 180, -90 };

        public abstract MatchType MatchType { get; }

        public BaseMatchModel(GridCellManager gridCellManager)
        {
            this.gridCellManager = gridCellManager;
        }

        protected List<IGridCell> GetMatchResult(Vector3Int gridPosition)
        {
            List<IGridCell> matchGrids = new();
            int minMatchCount = GetMinMatchCount();

            for (int i = 0; i < matchCellPositions.Count; i++)
            {
                for (int j = 0; j < _checkAngles.Length; j++)
                {
                    matchGrids = GetMatchCellsFromSequence(gridPosition, matchCellPositions[i], _checkAngles[j]);
                    if (matchGrids.Count >= minMatchCount)
                        return matchGrids;
                }
            }

            return matchGrids;
        }

        public bool CheckMatch(Vector3Int gridPosition, out List<IGridCell> matchCells)
        {
            IGridCell checkGrid = gridCellManager.Get(gridPosition);
            List<IGridCell> matchedCells = GetMatchResult(gridPosition);

            int minMatchCount = GetMinMatchCount();
            bool isMatchable = matchedCells.Count >= minMatchCount;

            if (isMatchable)
                matchedCells.Add(checkGrid);
            else
                matchedCells.Clear();

            matchCells = matchedCells;
            return isMatchable;
        }

        protected List<Vector3Int> GetRotatePositions(List<Vector3Int> checkPositions, int angle)
        {
            int count = checkPositions.Count;
            List<Vector3Int> rotateMatchPositions = new();

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Matrix4x4 rotateMatrix = Matrix4x4.Rotate(rotation);

            for (int i = 0; i < checkPositions.Count; i++)
            {
                Vector3 rotatePosition = rotateMatrix.MultiplyPoint3x4(checkPositions[i]);
                
                int x = Mathf.RoundToInt(rotatePosition.x);
                int y = Mathf.RoundToInt(rotatePosition.y);
                
                Vector3Int newPosition = new(x, y);
                rotateMatchPositions.Add(newPosition);
            }

            return rotateMatchPositions;
        }

        protected List<IGridCell> GetMatchCellsFromSequence(Vector3Int position, SequencePosition sequence, int angle)
        {
            List<IGridCell> gridCells = new();
            List<Vector3Int> checkSteps = GetRotatePositions(sequence.Sequence, angle);
            
            IGridCell checkCell = gridCellManager.Get(position);
            CandyColor candyColor = checkCell.CandyColor;
            
            if (candyColor == CandyColor.None)
                return gridCells;

            for (int i = 0; i < checkSteps.Count; i++)
            {
                IGridCell gridCell = gridCellManager.Get(position + checkSteps[i]);

                if (gridCell == null)
                    break;

                if (!gridCell.HasItem)
                    break;

                if (!gridCell.BlockItem.IsMatchable)
                    break;

                if (gridCell.CandyColor != candyColor)
                    break;

                gridCells.Add(gridCell);
            }

            return gridCells;
        }

        protected int GetMinMatchCount()
        {
            int minMatchCount = MatchType switch
            {
                MatchType.Match3 => 2,
                MatchType.Match4 => 3,
                MatchType.Match5 => 4,
                MatchType.MatchL => 4,
                MatchType.MatchT => 4,
                _ => 100
            };

            return minMatchCount;
        }

        public void Dispose()
        {
            matchCellPositions.Clear();
        }
    }
}
