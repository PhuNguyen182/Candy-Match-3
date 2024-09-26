using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class ShuffleBoardTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly FillBoardTask _fillBoardTask;

        private List<Vector3Int> _activePositions;
        private List<Vector3Int> _shuffleableCells;

        public ShuffleBoardTask(GridCellManager gridCellManager, DetectMoveTask detectMoveTask, FillBoardTask fillBoardTask)
        {
            _gridCellManager = gridCellManager;
            _detectMoveTask = detectMoveTask;
            _fillBoardTask = fillBoardTask;

            _shuffleableCells = new();
        }

        public void BuildActivePositions()
        {
            _activePositions = _gridCellManager.GetActivePositions().ToList();
        }

        public async UniTask Shuffle(bool immediately = false)
        {
            bool canShuffle = TryShuffle();

            if (canShuffle)
                await TransformItems(immediately);
        }

        private bool TryShuffle()
        {
            int shuffleCount = 0;
            CollectShuffleableCell();

            while (shuffleCount < 1000)
            {
                ClearItemColor();
                _fillBoardTask.BuildShuffle(_shuffleableCells);
                _detectMoveTask.DetectPossibleMoves();

                if (_detectMoveTask.HasPossibleMove())
                    return true;

                shuffleCount = shuffleCount + 1;
            }

            return false;
        }

        private void CollectShuffleableCell()
        {
            _shuffleableCells.Clear();
            IGridCell gridCell = null;

            for (int i = 0; i < _activePositions.Count; i++)
            {
                gridCell = _gridCellManager.Get(_activePositions[i]);

                if (!gridCell.HasItem)
                    continue;

                if (gridCell.CandyColor == CandyColor.None)
                    continue;

                if (!gridCell.BlockItem.IsMatchable)
                    continue;

                if (gridCell.BlockItem is IBooster)
                    continue;

                _shuffleableCells.Add(_activePositions[i]);
            }
        }

        private void ClearItemColor()
        {
            for (int i = 0; i < _shuffleableCells.Count; i++)
            {
                IGridCell gridCell = _gridCellManager.Get(_shuffleableCells[i]);

                if (gridCell.BlockItem is IItemTransform itemTransform) // Temporary clear current color of the item
                    itemTransform.SwitchTo(ItemType.None);
            }
        }

        private async UniTask TransformItems(bool immediately)
        {
            if (immediately)
            {
                for (int i = 0; i < _shuffleableCells.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(_shuffleableCells[i]);
                    IItemTransform itemTransform = gridCell.BlockItem as IItemTransform;
                    itemTransform.TransformImmediately();
                }
            }

            else
            {
                using (ListPool<UniTask>.Get(out List<UniTask> transformTasks))
                {
                    for (int i = 0; i < _shuffleableCells.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(_shuffleableCells[i]);
                        IItemTransform itemTransform = gridCell.BlockItem as IItemTransform;
                        transformTasks.Add(itemTransform.Transform(i * 0.005f));
                    }

                    await UniTask.WhenAll(transformTasks);
                }
            }
        }

        public void Dispose()
        {
            _activePositions.Clear();
            _shuffleableCells.Clear();
        }
    }
}
