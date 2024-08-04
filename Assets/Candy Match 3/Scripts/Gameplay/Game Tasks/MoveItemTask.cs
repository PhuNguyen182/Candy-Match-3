using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

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
            _direction = Vector3Int.down;
        }

        public async UniTask MoveItems()
        {
            using (var moveListPool = ListPool<UniTask>.Get(out List<UniTask> moveTasks))
            {
                _gridCellManager.ForEach(position =>
                {
                    moveTasks.Add(MoveItem(position));
                });

                await UniTask.WhenAll(moveTasks);
            }
        }

        private async UniTask MoveItem(Vector3Int checkPosition)
        {
            IGridCell startCell = _gridCellManager.Get(checkPosition);

            if (startCell == null)
                return;

            if (!startCell.IsMoveable)
                return;

            int fallStepCount = 0;
            Vector3Int targetPosition;

            if (!CanFallDown(startCell, out targetPosition))
                return;

            int boardBottomHeight = _gridCellManager.MinPosition.y;
            fallStepCount = checkPosition.y - boardBottomHeight;

            IGridCell targetCell = _gridCellManager.Get(targetPosition);
            IBlockItem startItem = startCell.BlockItem;

            startCell.SetBlockItem(null);
            targetCell.SetBlockItem(startItem);

            if (startItem is IItemAnimation animation)
            {
                int boardHeight = _gridCellManager.BoardHeight;
                float moveSpeed = Match3Constants.BaseItemMoveSpeed + Match3Constants.FallenAccelaration * fallStepCount;
                float fallDuration = (1 + fallStepCount) / moveSpeed;

                await animation.MoveTo(targetCell.WorldPosition, fallDuration);
                animation.JumpDown(1.0f * fallStepCount / boardHeight);
            }

            startItem.SetWorldPosition(targetCell.WorldPosition);
        }

        private bool CanFallDown(IGridCell gridCell, out Vector3Int targetPosition)
        {
            IGridCell targetCell = gridCell;
            Vector3Int checkPosition = targetCell.GridPosition + _direction;

            while (_checkGridTask.Check(checkPosition))
            {
                targetCell = _gridCellManager.Get(checkPosition);
                checkPosition = targetCell.GridPosition + _direction;
            }

            targetPosition = targetCell.GridPosition;
            return targetCell != gridCell;
        }

        public void Dispose()
        {
            _tcs.Dispose();
        }
    }
}
