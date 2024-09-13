using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.Strategies.Suggests;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;
using Random = UnityEngine.Random;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class DetectMoveTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly SuggestComparer _suggestComparer;

        private List<Vector3Int> _swapDirections;
        private List<Vector3Int> _allGridPositions;
        private List<AvailableSuggest> _availableMoves;

        private const int ColorfulWithColorItemScore = 7;

        public DetectMoveTask(GridCellManager gridCellManager, MatchItemsTask matchItemsTask)
        {
            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;

            _swapDirections = new()
            {
                Vector3Int.right, 
                Vector3Int.down,
            };

            _suggestComparer = new();
            _availableMoves = new();
        }

        public void DetectPossibleMoves()
        {
            ClearResult();

            for (int i = 0; i < _allGridPositions.Count; i++)
            {
                for (int j = 0; j < _swapDirections.Count; j++)
                {
                    Vector3Int fromPosition = _allGridPositions[i];
                    Vector3Int toPosition = fromPosition + _swapDirections[j];

                    if (IsSwappable(fromPosition, toPosition))
                    {
                        int score = 0;
                        IGridCell fromGridCell = _gridCellManager.Get(fromPosition);
                        IGridCell toGridCell = _gridCellManager.Get(toPosition);
                        PseudoSwapItems(fromGridCell, toGridCell);

                        if (AreBoosters(fromGridCell, toGridCell))
                        {
                            score = GetComboBoosterScore(fromGridCell, toGridCell);

                            _availableMoves.Add(new AvailableSuggest
                            {
                                IsSwapWithBooster = true,
                                Position = fromPosition,
                                Direction = _swapDirections[j],
                                Score = score
                            });
                        }

                        else if (IsColorfulWithColorItem(fromGridCell, toGridCell))
                        {
                            score = ColorfulWithColorItemScore;

                            _availableMoves.Add(new AvailableSuggest
                            {
                                IsSwapWithBooster = true,
                                Position = fromPosition,
                                Direction = _swapDirections[j],
                                Score = score
                            });
                        }

                        else
                        {
                            int fromScore = 0, toScore = 0;
                            _matchItemsTask.CheckMatch(fromPosition, out fromScore);
                            _matchItemsTask.CheckMatch(toPosition, out toScore);

                            if (fromScore == 0 && toScore == 0)
                            {
                                PseudoSwapItems(fromGridCell, toGridCell);
                                continue;
                            }

                            Vector3Int position, direction;
                            score = Mathf.Max(fromScore, toScore);
                            
                            if(fromScore >= toScore)
                            {
                                position = fromPosition;
                                direction = _swapDirections[j];
                            }

                            else
                            {
                                position = toPosition;
                                direction = -_swapDirections[j];
                            }

                            _availableMoves.Add(new AvailableSuggest
                            {
                                Score = score,
                                IsSwapWithBooster = false,
                                Position = position,
                                Direction = direction,
                            });

                            // Swap back
                            PseudoSwapItems(fromGridCell, toGridCell);
                        }
                    }
                }

                _availableMoves.Sort(_suggestComparer);
            }
        }

        private bool IsSwappable(Vector3Int fromPosition, Vector3Int toPosition)
        {
            IGridCell fromGridCell = _gridCellManager.Get(fromPosition);
            IGridCell toGridCell = _gridCellManager.Get(toPosition);

            if (fromGridCell == null || toGridCell == null)
                return false;

            if (fromGridCell.IsLocked || toGridCell.IsLocked)
                return false;

            if (!fromGridCell.HasItem || !toGridCell.HasItem)
                return false;

            IBlockItem fromItem = fromGridCell.BlockItem;
            IBlockItem toItem = toGridCell.BlockItem;

            if (!fromItem.IsMoveable || !toItem.IsMoveable)
                return false;

            return true;
        }

        public void PseudoSwapItems(IGridCell fromGridCell, IGridCell toGridCell)
        {
            IBlockItem fromItem = fromGridCell.BlockItem;
            IBlockItem toItem = toGridCell.BlockItem;

            fromGridCell.SetBlockItem(toItem, false);
            toGridCell.SetBlockItem(fromItem, false);
        }

        private int GetComboBoosterScore(IGridCell gridCell1, IGridCell gridCell2)
        {
            int score = 0;
            BoosterType boosterType1 = GetBoosterType(gridCell1);
            BoosterType boosterType2 = GetBoosterType(gridCell2);

            score = (boosterType1, boosterType2) switch
            {
                // Striped + Striped
                (BoosterType.Vertical, BoosterType.Vertical) => 10,
                (BoosterType.Horizontal, BoosterType.Horizontal) => 10,
                (BoosterType.Horizontal, BoosterType.Vertical) => 10,
                (BoosterType.Vertical, BoosterType.Horizontal) => 10,
                
                // Wrapped + Wrapped
                (BoosterType.Wrapped, BoosterType.Wrapped) => 11,
                
                // Wrapped + Striped
                (BoosterType.Wrapped, BoosterType.Horizontal) => 12,
                (BoosterType.Wrapped, BoosterType.Vertical) => 12,
                (BoosterType.Horizontal, BoosterType.Wrapped) => 12,
                (BoosterType.Vertical, BoosterType.Wrapped) => 12,

                // Colorful + Striped
                (BoosterType.Colorful, BoosterType.Horizontal) => 13,
                (BoosterType.Colorful, BoosterType.Vertical) => 13,
                (BoosterType.Horizontal, BoosterType.Colorful) => 13,
                (BoosterType.Vertical, BoosterType.Colorful) => 13,

                // Colorful + Wrapped
                (BoosterType.Colorful, BoosterType.Wrapped) => 14,
                (BoosterType.Wrapped, BoosterType.Colorful) => 14,

                // Colorful = Colorful
                (BoosterType.Colorful, BoosterType.Colorful) => 15,
                _ => 0
            };

            return score;
        }

        private bool AreBoosters(IGridCell gridCell1, IGridCell gridCell2)
        {
            BoosterType boosterType1 = GetBoosterType(gridCell1);
            BoosterType boosterType2 = GetBoosterType(gridCell2);

            return boosterType1 != BoosterType.None && boosterType2 != BoosterType.None;
        }

        private bool IsColorfulWithColorItem(IGridCell gridCell1, IGridCell gridCell2)
        {
            if (!gridCell1.BlockItem.IsMatchable && !gridCell2.BlockItem.IsMatchable)
                return false;

            return gridCell1.ItemType == ItemType.ColorBomb && gridCell1.ItemType != ItemType.ColorBomb
                || gridCell1.ItemType != ItemType.ColorBomb && gridCell1.ItemType == ItemType.ColorBomb;
        }

        private BoosterType GetBoosterType(IGridCell gridCell)
        {
            if(gridCell.BlockItem is IBooster booster)
            {
                if (gridCell.BlockItem.ItemType == ItemType.ColorBomb)
                    return BoosterType.Colorful;

                if (gridCell.BlockItem is IColorBooster colorBooster)
                    return colorBooster.ColorBoosterType;
            }

            return BoosterType.None;
        }

        public AvailableSuggest GetPossibleSwap(int detectCount)
        {
            // If this is first detect, get the most valueable swap
            // After that, get the random swap near the top-valueable swap randomly
            int count = _availableMoves.Count;
            int index = detectCount == 0 ? count - 1 : Random.Range(count * 2 / 3, count);
            return _availableMoves[index];
        }

        public void BuildLevelBoard()
        {
            _allGridPositions = new(_gridCellManager.GetActivePositions());
        }

        public bool HasPossibleMove()
        {
            return _availableMoves.Count > 0;
        }

        public void ClearResult()
        {
            _availableMoves.Clear();
        }

        public void Dispose()
        {
            _allGridPositions.Clear();
            _swapDirections.Clear();
        }
    }
}
