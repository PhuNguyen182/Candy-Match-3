using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public abstract class BaseMatchModel : IDisposable
    {
        protected readonly GridCellManager gridCellManager;

        protected int[] checkAngles = new[] { 0, -90, 180, 90 };

        protected IDisposable disposable;
        protected abstract int requiredItemCount { get; }
        protected abstract List<SequencePattern> sequencePattern { get; }

        public abstract MatchType MatchType { get; }

        public BaseMatchModel(GridCellManager gridCellManager)
        {
            this.gridCellManager = gridCellManager;
        }

        protected void OnConstuctor()
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            for (int i = 0; i < sequencePattern.Count; i++)
            {
                sequencePattern[i].AddTo(ref builder);
            }

            disposable = builder.Build();
        }

        protected MatchResult GetMatchResult(Vector3Int gridPosition)
        {
            List<Vector3Int> matchSequence = new();

            for (int i = 0; i < sequencePattern.Count; i++)
            {
                matchSequence = GetMatchCellsFromSequence(gridPosition, sequencePattern[i], out CandyColor matchColor);
                if (matchSequence.Count >= requiredItemCount)
                {
                    return new MatchResult
                    {
                        MatchType = MatchType,
                        CandyColor = matchColor,
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
            bool isMatchable = result.MatchSequence.Count >= requiredItemCount;

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

        protected List<Vector3Int> GetMatchCellsFromSequence(Vector3Int position, SequencePattern sequence, out CandyColor matchColor)
        {
            List<Vector3Int> gridCells = new();
            
            IGridCell checkCell = gridCellManager.Get(position);
            CandyColor candyColor = checkCell.CandyColor;
            
            if (candyColor == CandyColor.None)
            {
                matchColor = candyColor;
                return gridCells;
            }

            for (int i = 0; i < sequence.Pattern.Count; i++)
            {
                IGridCell gridCell = gridCellManager.Get(position + sequence.Pattern[i]);

                if (gridCell == null)
                    break;

                if (!gridCell.HasItem)
                    break;

                if (!gridCell.BlockItem.IsMatchable)
                    break;

                if (gridCell.IsMoving)
                    break;

                if (gridCell.CandyColor != candyColor)
                {
                    if (gridCells.Count < requiredItemCount)
                        break;

                    else continue;
                }

                gridCells.Add(gridCell.GridPosition);
            }

            matchColor = candyColor;
            return gridCells;
        }

        public void Dispose()
        {
            disposable.Dispose();
            sequencePattern.Clear();
        }
    }
}
