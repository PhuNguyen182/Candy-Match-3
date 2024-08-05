using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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

            IBlockItem startItem = startCell.BlockItem;
            
            if (!startItem.IsMoveable)
                return;

            int fallStepCount = 0;
            Vector3Int targetPosition;

            //List<Vector3Int> dropDownPositions = GetDropPositions(startCell);
            //List<Vector3Int> filtedDropPositions = FilterPositions(checkPosition, dropDownPositions);

            //if (filtedDropPositions.Count < 1)
            //    return;

            //targetPosition = filtedDropPositions[filtedDropPositions.Count - 1];

            if (!CanFallDown(startCell, out targetPosition))
                return;

            int boardBottomHeight = _gridCellManager.MinPosition.y;
            fallStepCount = checkPosition.y - boardBottomHeight;

            IGridCell targetCell = _gridCellManager.Get(targetPosition);

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

        private bool CanMoveStepDown(IGridCell gridCell, out Vector3Int targetPosition)
        {
            Vector3Int checkPosition = gridCell.GridPosition + _direction;
            
            if (_checkGridTask.Check(checkPosition))
            {
                targetPosition = checkPosition;
                return true;
            }

            targetPosition = Vector3Int.zero;
            return false;
        }

        private List<Vector3Int> GetDropPositions(IGridCell gridCell)
        {
            List<Vector3Int> dropPositions = new();
            //IGridCell stepCell = gridCell;

            while (CanMoveStepDown(gridCell, out Vector3Int nextPosition))
            {
                gridCell = _gridCellManager.Get(nextPosition);
                dropPositions.Add(nextPosition);
            }

            if (!CanSlideDiagonally(gridCell, out Vector3Int diagonalPosition))
                return dropPositions;

            dropPositions.Add(diagonalPosition);
            IGridCell diagonalCell = _gridCellManager.Get(diagonalPosition);
            dropPositions.AddRange(GetDropPositions(diagonalCell));

            return dropPositions;
        }

        private bool CanSlideDiagonally(IGridCell gridCell, out Vector3Int diagonalPosition)
        {
            bool canSlideLeft = CanSlideDiagonally(gridCell, Vector3Int.left, out diagonalPosition);
            bool canSlideRight = CanSlideDiagonally(gridCell, Vector3Int.right, out diagonalPosition);
            return canSlideLeft || canSlideRight;
        }

        private bool CanSlideDiagonally(IGridCell gridCell, Vector3Int direction, out Vector3Int diagonalPosition)
        {
            Vector3Int nextPosition = gridCell.GridPosition + direction;
            Vector3Int downPosition = gridCell.GridPosition + _direction;
            IGridCell downCell = _gridCellManager.Get(downPosition);

            if (_checkGridTask.Check(nextPosition) && downCell != null && !downCell.GridStateful.IsLocked)
            {
                return CanMoveStepDown(gridCell, out diagonalPosition);
            }

            diagonalPosition = Vector3Int.zero;
            return false;
        }

        private List<Vector3Int> FilterPositions(Vector3Int checkPosition, List<Vector3Int> queuePositions)
        {
            HashSet<Vector3Int> filtedPositions = new();
            // To do: To be added

            if (queuePositions.Count < 2)
                return queuePositions;

            int startColumn = checkPosition.x;

            for (int i = 0; i < queuePositions.Count; i++)
            {
                Vector3Int position = queuePositions[i];

                if(position.x == position.x)
                {
                    if(i == queuePositions.Count - 1)
                    {
                        filtedPositions.Add(position);
                        continue;
                    }
                }

                if (i > 0)
                    filtedPositions.Add(queuePositions[i - 1]);

                filtedPositions.Add(position);
                startColumn = position.x;
            }

            return filtedPositions.ToList();
        }

        public void Dispose()
        {
            _tcs.Dispose();
        }
    }
}
