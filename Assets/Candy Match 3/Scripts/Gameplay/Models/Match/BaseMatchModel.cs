using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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
        protected abstract int matchScoreCount { get; }
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

        protected (int matchScore, int boosterCount) GetMatchableScore(Vector3Int gridPosition)
        {
            int matchScore = 0;
            int boosterCount = 0;

            for (int i = 0; i < sequencePattern.Count; i++)
            {
                matchScore = GetMatchPointFromSequence(gridPosition, sequencePattern[i], out boosterCount);

                if (matchScore >= requiredItemCount)
                {
                    return (matchScoreCount, boosterCount);
                }
            }

            return (0, 0);
        }

        public MatchResult GetMatchResult(Vector3Int gridPosition)
        {
            using (ListPool<Vector3Int>.Get(out List<Vector3Int> matchSequence))
            {
                for (int i = 0; i < sequencePattern.Count; i++)
                {
                    matchSequence = GetMatchCellsFromSequence(gridPosition, sequencePattern[i]
                                            , out CandyColor matchColor, out bool hasBooster);
                    
                    if (matchSequence.Count >= requiredItemCount)
                    {
                        matchSequence.Add(gridPosition);

                        return new MatchResult
                        {
                            MatchType = MatchType,
                            CandyColor = matchColor,
                            Position = gridPosition,
                            MatchSequence = matchSequence,
                            HasBooster = hasBooster
                        };
                    }
                }

                return new() { MatchSequence = new() };
            }
        }

        public bool CheckMatch(Vector3Int gridPosition, out MatchScore score)
        {
            var (matchScore, boosterCount) = GetMatchableScore(gridPosition);
            
            score = new MatchScore
            {
                HasBooster = boosterCount> 0,
                ItemCount = requiredItemCount,
                Score = matchScore + boosterCount,
                MatchType = MatchType
            };

            return matchScore > 0;
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

        protected int GetMatchPointFromSequence(Vector3Int position, SequencePattern sequence, out int boosterCount)
        {
            int boosterAmount = 0;
            int matchableCount = 0;

            IGridCell checkCell = gridCellManager.Get(position);
            CandyColor candyColor = checkCell.CandyColor;

            if (candyColor == CandyColor.None)
            {
                boosterCount = 0;
                return matchableCount;
            }

            for (int i = 0; i < sequence.Pattern.Count; i++)
            {
                Vector3Int checkPosition = position + sequence.Pattern[i];
                IGridCell gridCell = gridCellManager.Get(checkPosition);

                if (gridCell == null)
                    break;

                if (!gridCell.HasItem || gridCell.IsLocked)
                    break;

                if (!gridCell.BlockItem.IsMatchable)
                    break;

                if (gridCell.BlockItem.CandyColor != candyColor)
                {
                    if (matchableCount < requiredItemCount)
                        break;

                    else
                        continue;
                }

                if (IsBooster(gridCell.BlockItem))
                    boosterAmount = boosterAmount + 1;

                matchableCount = matchableCount + 1;
            }

            boosterCount = boosterAmount;
            return matchableCount;
        }


        protected List<Vector3Int> GetMatchCellsFromSequence(Vector3Int position, SequencePattern sequence, out CandyColor matchColor, out bool hasBooster)
        {
            bool containBooster = false;
            List<Vector3Int> gridCells = new();
            
            IGridCell checkCell = gridCellManager.Get(position);
            CandyColor candyColor = checkCell.CandyColor;
            
            if (candyColor == CandyColor.None)
            {
                matchColor = candyColor;
                hasBooster = false;
                return gridCells;
            }

            for (int i = 0; i < sequence.Pattern.Count; i++)
            {
                Vector3Int checkPosition = position + sequence.Pattern[i];
                IGridCell gridCell = gridCellManager.Get(checkPosition);

                if (gridCell == null)
                    break;

                if (!gridCell.HasItem || gridCell.IsLocked)
                    break;
                
                if (!gridCell.BlockItem.IsMatchable)
                    break;

                if (gridCell.BlockItem.CandyColor != candyColor)
                {
                    if (gridCells.Count < requiredItemCount)
                        break;

                    else 
                        continue;
                }

                if (IsBooster(gridCell.BlockItem))
                    containBooster = true;

                gridCells.Add(gridCell.GridPosition);
            }

            hasBooster = containBooster;
            matchColor = candyColor;
            return gridCells;
        }

        private bool IsBooster(IBlockItem blockItem)
        {
            bool isBooster = blockItem.ItemType switch
            {
                ItemType.BlueHorizontal => true,
                ItemType.GreenHorizontal => true,
                ItemType.OrangeHorizontal => true,
                ItemType.PurpleHorizontal => true,
                ItemType.RedHorizontal => true,
                ItemType.YellowHorizontal => true,
                
                ItemType.BlueVertical => true,
                ItemType.GreenVertical => true,
                ItemType.OrangeVertical => true,
                ItemType.PurpleVertical => true,
                ItemType.RedVertical => true,
                ItemType.YellowVertical => true,
                
                ItemType.BlueWrapped => true,
                ItemType.GreenWrapped => true,
                ItemType.OrangeWrapped => true,
                ItemType.PurpleWrapped => true,
                ItemType.RedWrapped => true,
                ItemType.YellowWrapped => true,
                
                _ => false
            };

            return isBooster;
        }

        public void Dispose()
        {
            disposable.Dispose();
            sequencePattern.Clear();
        }
    }
}
