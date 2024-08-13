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
        private readonly MatchItemsTask _matchItemsTask;

        private CheckGridTask _checkGridTask;

        private int _boardHeight;
        private Vector3Int _direction;

        private CancellationToken _token;
        private CancellationTokenSource _tcs;

        private IDisposable _disposable;

        public MoveItemTask(GridCellManager gridCellManager, MatchItemsTask matchItemsTask)
        {
            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;

            _tcs = new();
            _token = _tcs.Token;
        }

        public async UniTask MoveItems()
        {
            await UniTask.CompletedTask;
            for (int i = -4; i < 5; i++)
            {
                IGridCell cell = _gridCellManager.Get(new(i, 4));
                if (CheckMoveable(cell))
                    MoveItem(cell).Forget();
            }
        }

        public async UniTask MoveItem(IGridCell moveGridCell)
        {
            IGridCell currentGrid = moveGridCell;
            Vector3Int checkDirection = Vector3Int.zero;
            Vector3Int startPosition = currentGrid.GridPosition;
            IBlockItem blockItem = currentGrid.BlockItem;

            int moveStepCount = 0;
            int outputMoveStep = 0;
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

                if (checkColumnIndex != 0)
                    checkDirection = Vector3Int.up;

                Vector3Int toPosition = startPosition + _direction;
                toGridCell = _gridCellManager.Get(toPosition);

                if (!CheckCellEmpty(toGridCell, out IGridCell targetCell))
                {
                    moveStepCount = 1;
                    checkColumnIndex = checkColumnIndex + 1;
                    continue;
                }

                toGridCell = targetCell;

                if (checkColumnIndex != 0 && !CheckLine(toGridCell.GridPosition, checkDirection))
                {
                    checkColumnIndex = checkColumnIndex + 1;
                    continue;
                }

                checkColumnIndex = 0;
                toGridCell.SetBlockItem(blockItem);
                currentGrid.LockStates = LockStates.None;
                toGridCell.LockStates = LockStates.Moving;
                currentGrid.SetBlockItem(null);

                moveStepCount = moveStepCount + 1;
                ExportMoveStep(moveStepCount, out outputMoveStep);

                _checkGridTask.CheckAt(currentGrid.GridPosition, 1);
                await AnimateMovingItem(blockItem, toGridCell, moveStepCount);

                startPosition = toPosition;
                currentGrid = toGridCell;
            }

            currentGrid.LockStates = LockStates.Moving;
            blockItem.SetWorldPosition(currentGrid.WorldPosition);
            AnimateItemJumpDown(blockItem, outputMoveStep);
            //_matchItemsTask.CheckMatch(currentGrid.GridPosition);
            currentGrid.LockStates = LockStates.None;
        }

        private async UniTask AnimateMovingItem(IBlockItem blockItem, IGridCell targetCell, int stepCount)
        {
            if (blockItem is IItemAnimation animation)
            {
                float moveSpeed = Match3Constants.BaseItemMoveSpeed + Match3Constants.FallenAccelaration * stepCount;
                await animation.MoveTo(targetCell.WorldPosition, 1/ moveSpeed);
            }
        }

        private void AnimateItemJumpDown(IBlockItem blockItem, int stepCount)
        {
            if(blockItem is IItemAnimation animation)
            {
                _boardHeight = _gridCellManager.BoardHeight;
                if(stepCount > 0)
                    animation.JumpDown(1.0f * stepCount / _boardHeight);
            }
        }

        private void ExportMoveStep(int input, out int output)
        {
            output = input;
        }

        private bool CheckLine(Vector3Int checkPosition, Vector3Int directionUnit)
        {
            int index = 1;

            do
            {
                IGridCell checkCell = _gridCellManager.Get(checkPosition + directionUnit * index);
                if (checkCell == null)
                    return false;

                if (checkCell.IsSpawner)
                    return false;

                if (!checkCell.CanContainItem)
                    return false;

                if (!checkCell.CanMove)
                    return false;

                if (checkCell.HasItem)
                    return !checkCell.IsMoveable;

                index = index + 1;
            } while (true);
        }

        private bool CheckCellEmpty(IGridCell gridCell, out IGridCell targetCell)
        {
            targetCell = gridCell;

            if (gridCell == null)
                return false;

            if (!gridCell.CanContainItem)
                return false;

            if (gridCell.LockStates != LockStates.None)
                return false;

            return gridCell.CanSetItem;
        }

        public bool CheckMoveable(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (gridCell.LockStates != LockStates.None)
                return false;

            if (!gridCell.HasItem)
                return false;

            if (!gridCell.IsMoveable)
                return false;

            IGridCell downCell = _gridCellManager.Get(gridCell.GridPosition + Vector3Int.down);
            if (downCell != null && downCell.CanSetItem)
                return true;

            IGridCell leftSide = _gridCellManager.Get(gridCell.GridPosition + new Vector3Int(-1, -1));
            if (leftSide != null && leftSide.CanSetItem)
                return true;

            IGridCell rightSide = _gridCellManager.Get(gridCell.GridPosition + new Vector3Int(1, -1));
            if (rightSide != null && rightSide.CanSetItem)
                return true;

            return false;
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