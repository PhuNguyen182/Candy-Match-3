using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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
        private readonly CheckGridTask _checkGridTask;

        private Vector3Int _direction;
        private CancellationToken _token;
        private CancellationTokenSource _tcs;

        public MoveItemTask(GridCellManager gridCellManager, CheckGridTask checkGridTask)
        {
            _gridCellManager = gridCellManager;
            _checkGridTask = checkGridTask;

            _tcs = new();
            _token = _tcs.Token;
        }

        public async UniTask MoveItems()
        {
            using (var moveListPool = ListPool<UniTask>.Get(out List<UniTask> moveTasks))
            {
                _gridCellManager.ForEach(position =>
                {
                    IGridCell gridCell = _gridCellManager.Get(position);
                    if(CheckMove(gridCell))
                        moveTasks.Add(MoveItem(gridCell));
                });

                await UniTask.WhenAll(moveTasks);
            }
        }

        private async UniTask MoveItem(IGridCell gridCell)
        {
            IGridCell currentCell = gridCell;
            IBlockItem blockItem = currentCell.BlockItem;

            int moveStepCount = 0;
            int moveColumnCheck = 0;
            Vector3Int fromPosition = currentCell.GridPosition;

            while (moveColumnCheck < 3)
            {
                IGridCell toGridCell;
                _direction = moveColumnCheck switch
                {
                    0 => new(0, -1),
                    1 => new(-1, -1),
                    2 => new(1, -1),
                    _ => Vector3Int.zero
                };

                Vector3Int toPosition = fromPosition + _direction;
                toGridCell = _gridCellManager.Get(toPosition);
                
                if (!CheckCellEmpty(toGridCell, out IGridCell targetCell))
                {
                    moveColumnCheck = moveColumnCheck + 1;
                    continue;
                }

                toGridCell = targetCell;
                toPosition = toGridCell.GridPosition;

                moveColumnCheck = 0;
                toGridCell.SetBlockItem(blockItem);
                currentCell.SetBlockItem(null);
                currentCell.LockStates = LockStates.None;
                toGridCell.LockStates = LockStates.Moving;
                await AnimateMovingItem(blockItem, targetCell, 1, false);
                fromPosition = toPosition;
                currentCell = toGridCell;
                moveStepCount = moveStepCount + 1;
            }
        }

        private async UniTask AnimateMovingItem(IBlockItem blockItem, IGridCell targetCell, int stepCount, bool boundOnGround)
        {
            if (blockItem is IItemAnimation animation)
            {
                int boardHeight = _gridCellManager.BoardHeight;
                float moveSpeed = Match3Constants.BaseItemMoveSpeed + Match3Constants.FallenAccelaration * stepCount;
                float fallDuration = (1 + stepCount) / moveSpeed;
                await animation.MoveTo(targetCell.WorldPosition, fallDuration);

                if(boundOnGround)
                    animation.JumpDown(1.0f * stepCount / boardHeight);
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
