using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MoveItemTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly CheckGridTask _checkGridTask;

        private int _boardHeight;
        private Vector3Int _direction;
        private CancellationToken _token;
        private CancellationTokenSource _tcs;

        private IDisposable _disposable;

        public MoveItemTask(GridCellManager gridCellManager, CheckGridTask checkGridTask)
        {
            _gridCellManager = gridCellManager;
            _checkGridTask = checkGridTask;

            _tcs = new();
            _token = _tcs.Token;
        }

        public async UniTask MoveItems()
        {
            var x = _gridCellManager.Iterator();
            foreach (var item in x)
            {
                var cell = _gridCellManager.Get(item);
                if (CheckMove(cell))
                {
                    //await MoveItem(cell);
                    MoveItem(cell).Forget();
                }
            }
        }

        private async UniTask MoveItem(IGridCell moveGridCell)
        {
            IGridCell currentGrid = moveGridCell;
            Vector3Int startPosition = currentGrid.GridPosition;
            IBlockItem blockItem = currentGrid.BlockItem;

            int moveStepCount = 0;
            int checkColumnIndex = 0;

            while (checkColumnIndex < 3)
            {
                IGridCell toGridCell;

                _direction = checkColumnIndex switch
                {
                    0 => new(0, -1),
                    1 => new(-1, -1),
                    2 => new(1, -1),
                    _ => new(0, 0)
                };

                Vector3Int toPosition = startPosition + _direction;
                toGridCell = _gridCellManager.Get(toPosition);

                if(!CheckCellEmpty(toGridCell, out IGridCell targetCell))
                {
                    moveStepCount = 0;
                    checkColumnIndex = checkColumnIndex + 1;
                    continue;
                }

                checkColumnIndex = 0;
                toGridCell = targetCell;
                toGridCell.SetBlockItem(blockItem);
                currentGrid.SetBlockItem(null);
                await AnimateMovingItem(blockItem, toGridCell, moveStepCount);
                startPosition = toPosition;
                currentGrid = toGridCell;

                moveStepCount = moveStepCount + 1;
            }

            blockItem.SetWorldPosition(currentGrid.WorldPosition);
            AnimateItemJumpDown(blockItem, moveStepCount);
        }

        private async UniTask AnimateMovingItem(IBlockItem blockItem, IGridCell targetCell, int stepCount)
        {
            if (blockItem is IItemAnimation animation)
            {
                float moveSpeed = Match3Constants.BaseItemMoveSpeed + Match3Constants.FallenAccelaration * stepCount;
                float fallDuration = 1 / moveSpeed;
                await animation.MoveTo(targetCell.WorldPosition, fallDuration);
            }
        }

        private void AnimateItemJumpDown(IBlockItem blockItem, int stepCount)
        {
            if(blockItem is IItemAnimation animation)
            {
                _boardHeight = _gridCellManager.BoardHeight;
                float moveStep = stepCount == 0 ? 2 : stepCount;
                animation.JumpDown(1.0f * moveStep / _boardHeight);
            }
        }

        private bool CheckCellEmpty(IGridCell gridCell, out IGridCell targetCell)
        {
            targetCell = gridCell;

            if (gridCell == null)
                return false;

            return gridCell.CanSetItem;
        }

        private bool CheckMove(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (!gridCell.IsMoveable)
                return false;

            IBlockItem blockItem = gridCell.BlockItem;
            if (!blockItem.IsMoveable)
                return false;

            return true;
        }

        public void Dispose()
        {
            _tcs.Dispose();
        }
    }
}
