using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.Strategies.Suggests;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

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

        // To do: Check matching logic again, optimize do not generate to much garbage
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
                        using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
                        {
                            IGridCell fromGridCell = _gridCellManager.Get(fromPosition);
                            IGridCell toGridCell = _gridCellManager.Get(toPosition);
                            PseudoSwapItems(fromGridCell, toGridCell);

                            if (AreBoosters(fromGridCell, toGridCell))
                            {
                                positions = new() { fromPosition, toPosition };
                                score = GetComboBoosterScore(fromGridCell, toGridCell);

                                _availableMoves.Add(new AvailableSuggest
                                {
                                    FromPosition = fromPosition,
                                    ToPosition = toPosition,
                                    Positions = positions,
                                    Score = score
                                });
                            }

                            else if (IsColorfulWithColorItem(fromGridCell, toGridCell))
                            {
                                score = ColorfulWithColorItemScore;
                                positions = new() { fromPosition, toPosition };

                                _availableMoves.Add(new AvailableSuggest
                                {
                                    FromPosition = fromPosition,
                                    ToPosition = toPosition,
                                    Positions = positions,
                                    Score = score
                                });
                            }

                            else
                            {
                                int fromScore = 0, toScore = 0;

                                if (_matchItemsTask.IsMatchable(fromPosition, out MatchResult fromMatchResult))
                                    fromScore = GetMatchableSwapScore(fromMatchResult);

                                if (_matchItemsTask.IsMatchable(toPosition, out MatchResult toMatchResult))
                                    toScore = GetMatchableSwapScore(toMatchResult);

                                if (fromScore == 0 && toScore == 0)
                                {
                                    PseudoSwapItems(fromGridCell, toGridCell);
                                    continue;
                                }

                                if (fromScore >= toScore)
                                {
                                    score = fromScore;
                                    positions = new(fromMatchResult.MatchSequence);
                                    int count = fromMatchResult.MatchSequence.Count;
                                    positions[count - 1] = toPosition;
                                }

                                else
                                {
                                    score = toScore;
                                    positions = new(toMatchResult.MatchSequence);
                                    int count = toMatchResult.MatchSequence.Count;
                                    positions[count - 1] = fromPosition;
                                }

                                _availableMoves.Add(new AvailableSuggest
                                {
                                    Score = score,
                                    FromPosition = fromPosition,
                                    ToPosition = toPosition,
                                    Positions = positions
                                });
                            }

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

        private void PseudoSwapItems(IGridCell fromGridCell, IGridCell toGridCell)
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
                (BoosterType.Vertical, BoosterType.Vertical) => 9,
                (BoosterType.Horizontal, BoosterType.Horizontal) => 9,
                (BoosterType.Horizontal, BoosterType.Vertical) => 9,
                (BoosterType.Vertical, BoosterType.Horizontal) => 9,
                
                // Wrapped + Wrapped
                (BoosterType.Wrapped, BoosterType.Wrapped) => 10,
                
                // Wrapped + Striped
                (BoosterType.Wrapped, BoosterType.Horizontal) => 11,
                (BoosterType.Wrapped, BoosterType.Vertical) => 11,
                (BoosterType.Horizontal, BoosterType.Wrapped) => 11,
                (BoosterType.Vertical, BoosterType.Wrapped) => 11,

                // Colorful + Striped
                (BoosterType.Colorful, BoosterType.Horizontal) => 12,
                (BoosterType.Colorful, BoosterType.Vertical) => 12,
                (BoosterType.Horizontal, BoosterType.Colorful) => 12,
                (BoosterType.Vertical, BoosterType.Colorful) => 12,

                // Colorful + Wrapped
                (BoosterType.Colorful, BoosterType.Wrapped) => 13,
                (BoosterType.Wrapped, BoosterType.Colorful) => 13,

                // Colorful = Colorful
                (BoosterType.Colorful, BoosterType.Colorful) => 14,
                _ => 0
            };

            return score;
        }

        private int GetMatchableSwapScore(MatchResult matchResult)
        {
            int score = 0;
            int count = matchResult.Count;
            MatchType matchType = matchResult.MatchType;
            bool hasBooster = matchResult.HasBooster;

            if (count == 3)
                score = 1;
            else if (count == 4)
                score = 2;
            else if (count >= 5)
                score = matchType == MatchType.Match5 ? 4 : 3;

            return score > 0 && hasBooster ? score + 2 : score;
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

        public List<AvailableSuggest> GetPossibleSwaps()
        {
            return _availableMoves;
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
