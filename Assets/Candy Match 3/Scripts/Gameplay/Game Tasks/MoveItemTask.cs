using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MoveItemTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        private CheckGridTask _checkGridTask;

        private int _boardHeight;
        private Vector3Int _direction;

        private CancellationToken _token;
        private CancellationTokenSource _tcs;

        private IDisposable _disposable;

        public MoveItemTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;

            _tcs = new();
            _token = _tcs.Token;
        }

        public async UniTask MoveItems()
        {
            await UniTask.CompletedTask;
            for (int i = -4; i < 5; i++)
            {
                IGridCell cell = _gridCellManager.Get(new(i, 1));
                if (CheckMoveable(cell))
                    MoveItem(cell).Forget();
            }
        }

        public async UniTask MoveItem(IGridCell moveGridCell)
        {
            IGridCell currentGrid = moveGridCell;
            Vector3Int startPosition = currentGrid.GridPosition;
            IBlockItem blockItem = currentGrid.BlockItem;

            int moveStepCount = 0;
            int outputMoveStep = 0;
            int checkColumnIndex = 0;

            while (currentGrid.IsMoveable && checkColumnIndex < 3)
            {
                IGridCell toGridCell;
                _direction = checkColumnIndex switch
                {
                    0 => new(0, -1),
                    1 => new(-1, -1),
                    2 => new(1, -1),
                    _ => new(0, -1)
                };

                Vector3Int toPosition = startPosition + _direction;
                toGridCell = _gridCellManager.Get(toPosition);

                if (!CheckCellEmpty(toGridCell, out IGridCell targetCell))
                {
                    moveStepCount = 0;
                    checkColumnIndex = checkColumnIndex + 1;
                    continue;
                }

                checkColumnIndex = 0;
                toGridCell = targetCell;

                toGridCell.SetBlockItem(blockItem);
                currentGrid.LockStates = LockStates.None;
                toGridCell.LockStates = LockStates.Moving;
                currentGrid.SetBlockItem(null);

                await UniTask.NextFrame(_token);
                moveStepCount = moveStepCount + 1;
                ExportMoveStep(moveStepCount, out outputMoveStep);
                _checkGridTask.CheckInDirection(currentGrid.GridPosition, Vector3Int.up);
                await AnimateMovingItem(blockItem, toGridCell, moveStepCount);

                startPosition = toPosition;
                currentGrid = toGridCell;
                currentGrid.LockStates = LockStates.None;
            }

            currentGrid.LockStates = LockStates.None;
            blockItem.SetWorldPosition(currentGrid.WorldPosition);
            AnimateItemJumpDown(blockItem, outputMoveStep);
            _checkGridTask.CheckInDirection(currentGrid.GridPosition, Vector3Int.up);
        }

        private async UniTask AnimateMovingItem(IBlockItem blockItem, IGridCell targetCell, int stepCount)
        {
            if (blockItem is IItemAnimation animation)
            {
                float moveSpeed = Match3Constants.BaseItemMoveSpeed + Match3Constants.FallenAccelaration * stepCount;
                float fallDuration = 1 / moveSpeed + Time.deltaTime * stepCount;
                await animation.MoveTo(targetCell.WorldPosition, fallDuration);
            }
        }

        private void AnimateItemJumpDown(IBlockItem blockItem, int stepCount)
        {
            if(blockItem is IItemAnimation animation)
            {
                _boardHeight = _gridCellManager.BoardHeight;
                animation.JumpDown(1.0f * stepCount / _boardHeight);
            }
        }

        private void ExportMoveStep(int input, out int output)
        {
            output = input;
        }

        private bool CheckCellEmpty(IGridCell gridCell, out IGridCell targetCell)
        {
            targetCell = gridCell;

            if (gridCell == null)
                return false;

            return gridCell.CanSetItem;
        }

        private bool CheckDiagonalMove(Vector3Int checkPosition, Vector3Int direction)
        {
            Vector3Int position = checkPosition + direction;
            IGridCell checkCell = _gridCellManager.Get(position);

            if (checkCell.CanContainItem)
                return false;
            if (checkCell.HasItem)
                return !checkCell.IsMoveable;

            return true;
        }

        public bool CheckMoveable(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (gridCell.LockStates != LockStates.None)
                return false;

            if (!gridCell.IsMoveable)
                return false;

            IBlockItem blockItem = gridCell.BlockItem;
            if (!blockItem.IsMoveable)
                return false;

            return true;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _tcs.Dispose();
        }
    }
}
