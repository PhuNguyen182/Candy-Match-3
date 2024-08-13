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
        protected int[] checkAngles = new[] { 0, 90, 180, -90 };

        public abstract MatchType MatchType { get; }

        public BaseMatchModel(GridCellManager gridCellManager)
        {
            this.gridCellManager = gridCellManager;
        }

        protected MatchResult GetMatchResult(Vector3Int gridPosition)
        {
            List<Vector3Int> matchSequence = new();
            int minMatchCount = GetMinMatchCount();

            for (int i = 0; i < matchCellPositions.Count; i++)
            {
                matchSequence = GetMatchCellsFromSequence(gridPosition, matchCellPositions[i]);
                if (matchSequence.Count >= minMatchCount)
                {
                    return new MatchResult
                    {
                        MatchType = MatchType,
                        Position = gridPosition,
                        MatchSequence = matchSequence,
                    };
                }
            }

            return new MatchResult { MatchSequence = new() };
        }

        public bool CheckMatch(Vector3Int gridPosition, out MatchResult matchResult)
        {
            MatchResult result = GetMatchResult(gridPosition);

            int minMatchCount = GetMinMatchCount();
            bool isMatchable = result.MatchSequence.Count >= minMatchCount;

            if (isMatchable)
                result.MatchSequence.Add(gridPosition);
            else
                result.MatchSequence.Clear();

            matchResult = result;
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

        protected List<Vector3Int> GetMatchCellsFromSequence(Vector3Int position, SequencePosition sequence)
        {
            List<Vector3Int> gridCells = new();
            
            IGridCell checkCell = gridCellManager.Get(position);
            CandyColor candyColor = checkCell.CandyColor;
            
            if (candyColor == CandyColor.None)
                return gridCells;

            for (int i = 0; i < sequence.Sequence.Count; i++)
            {
                IGridCell gridCell = gridCellManager.Get(position + sequence.Sequence[i]);

                if (gridCell == null)
                    break;

                if (!gridCell.HasItem)
                    break;

                if (!gridCell.BlockItem.IsMatchable)
                    break;

                if (gridCell.CandyColor != candyColor)
                    break;

                gridCells.Add(gridCell.GridPosition);
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
