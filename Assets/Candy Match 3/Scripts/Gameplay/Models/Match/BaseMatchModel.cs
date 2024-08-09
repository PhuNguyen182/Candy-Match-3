using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public abstract class BaseMatchModel
    {
        protected readonly GridCellManager gridCellManager;

        /// <summary>
        /// This property can be rotated, so do not list all cases in this collection
        /// </summary>
        protected abstract List<SequencePosition> matchCellPositions { get; }
        public abstract MatchType MatchType { get; }

        public BaseMatchModel(GridCellManager gridCellManager)
        {
            this.gridCellManager = gridCellManager;
        }

        public abstract List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection);

        public bool CheckMatch(Vector3Int gridPosition, Vector3Int inDirection, out List<IGridCell> matchCells)
        {
            IGridCell checkGrid = gridCellManager.Get(gridPosition);
            List<IGridCell> matchedCells = GetMatchResult(gridPosition, inDirection);
            
            matchedCells.Add(checkGrid);
            matchCells = matchedCells;
            return matchCells.Count >= 2;
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

        protected List<IGridCell> GetMatchCellSFromSequence(Vector3Int position, SequencePosition sequence, int angle)
        {
            List<IGridCell> gridCells = new();
            List<Vector3Int> seq = GetRotatePositions(sequence.Sequence, angle);
            IGridCell checkCell = gridCellManager.Get(position);
            CandyColor candyColor = checkCell.CandyColor;

            for (int i = 0; i < seq.Count; i++)
            {
                IGridCell gridCell = gridCellManager.Get(seq[i]);

                if (gridCell == null)
                    continue;

                if (!gridCell.HasItem)
                    continue;

                if (!gridCell.BlockItem.IsMatchable)
                    continue;

                if (gridCell.CandyColor == candyColor)
                    gridCells.Add(gridCell);
            }

            return gridCells;
        }
    }
}
