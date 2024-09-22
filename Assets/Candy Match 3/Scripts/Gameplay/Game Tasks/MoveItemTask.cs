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
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;

        private CheckGridTask _checkGridTask;

        private int _boardHeight;

        private CancellationToken _token;
        private CancellationTokenSource _tcs;

        private IDisposable _disposable;

        public MoveItemTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _tcs = new();
            _token = _tcs.Token;
        }

        public async UniTask MoveItem(IGridCell moveGridCell)
        {
            IGridCell currentGrid = moveGridCell;
            Vector3Int startPosition = currentGrid.GridPosition;
            IBlockItem blockItem = currentGrid.BlockItem;

            int moveStepCount = 0;
            int outputMoveStep = 0;
            int checkColumnIndex = 0;

            while (checkColumnIndex < 3)
            {
                IGridCell toGridCell;
                Vector3Int moveDirection = Vector3Int.zero;
                Vector3Int checkDirection = Vector3Int.zero;

                moveDirection = checkColumnIndex switch
                {
                    0 => new(0, -1),
                    1 => new(-1, -1),
                    2 => new(1, -1),
                    _ => new(0, -1)
                };

                if (checkColumnIndex != 0)
                    checkDirection = Vector3Int.up;

                Vector3Int toPosition = startPosition + moveDirection;
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
                currentGrid.SetBlockItem(null);
                toGridCell.SetBlockItem(blockItem, false);

                toGridCell.IsMoving = true;
                currentGrid.LockStates = LockStates.None;
                toGridCell.LockStates = LockStates.Moving;

                moveStepCount = moveStepCount + 1;
                ExportMoveStep(moveStepCount, out outputMoveStep);

                _checkGridTask.CheckAroundPosition(startPosition, 1);
                await AnimateFallingItem(blockItem, toGridCell, moveStepCount);

                startPosition = toPosition;
                currentGrid = toGridCell;
            }

            if (!currentGrid.IsMatching)
                AnimateItemJumpDown(blockItem, outputMoveStep);
                
            currentGrid.LockStates = LockStates.None;
            currentGrid.IsMoving = false;

            if (currentGrid.IsCollectible)
            {
                if (blockItem is ICollectible collectible)
                {
                    currentGrid.LockStates = LockStates.Exiting;

                    await collectible.Collect();
                    _breakGridTask.ReleaseGridCell(currentGrid);
                    currentGrid.LockStates = LockStates.None;
                    _checkGridTask.CheckAroundPosition(currentGrid.GridPosition, 1);
                }
            }

            else
            {
                if (outputMoveStep > 0)
                {
                    _checkGridTask.CheckMatchAtPosition(currentGrid.GridPosition);
                    _checkGridTask.CheckAroundPosition(currentGrid.GridPosition, 1);
                }

                else
                    _checkGridTask.CheckMatchAtPosition(currentGrid.GridPosition);
            }
        }

        private async UniTask AnimateFallingItem(IBlockItem blockItem, IGridCell targetCell, int stepCount)
        {
            if (blockItem is IItemAnimation animation)
            {
                _boardHeight = _gridCellManager.BoardHeight;
                animation.FallDown(true, 1.0f * stepCount / _boardHeight);
                float moveSpeed = Match3Constants.BaseItemMoveSpeed + Match3Constants.FallenAccelaration * stepCount;
                await animation.MoveTo(targetCell.WorldPosition, 1f / moveSpeed);
            }
        }

        private void AnimateItemJumpDown(IBlockItem blockItem, int stepCount)
        {
            if(blockItem is IItemAnimation animation)
            {
                animation.FallDown(false, 0);

                if(stepCount > 0)
                {
                    _boardHeight = _gridCellManager.BoardHeight;
                    animation.JumpDown(1.0f * stepCount / _boardHeight);
                }
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
                    return true;

                if (checkCell.IsMoving)
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

            if (gridCell.IsLocked)
                return false;

            return gridCell.CanSetItem;
        }

        public bool CheckMoveable(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (gridCell.IsLocked)
                return false;

            if (!gridCell.HasItem)
                return false;

            if (!gridCell.IsMoveable || gridCell.IsMoving)
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
