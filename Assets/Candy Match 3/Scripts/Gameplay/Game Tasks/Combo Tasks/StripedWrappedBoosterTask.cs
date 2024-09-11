using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Effects;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class StripedWrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        private CheckGridTask _checkGridTask;

        public StripedWrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            IGridCell actorGridCell = null;
            IGridCell remainGridCell = null;
            IColorBooster actorBooster = null;

            if (gridCell1.BlockItem is IColorBooster booster1 && gridCell2.BlockItem is IColorBooster booster2)
            {
                actorBooster = booster1.ColorBoosterType != BoosterType.Wrapped ? booster1 : booster2;
                actorGridCell = booster1.ColorBoosterType != BoosterType.Wrapped ? gridCell1 : gridCell2;
                remainGridCell = booster1.ColorBoosterType != BoosterType.Wrapped ? gridCell2 : gridCell1;
            }

            _breakGridTask.ReleaseGridCell(remainGridCell);

            Vector3Int checkPosition = gridCell2.GridPosition;
            BoundsInt activeBounds = _gridCellManager.GetActiveBounds();

            // Lock columns
            using var columnListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> columnListPositions);
            AddTripleVerticalLine(checkPosition, activeBounds, ref columnListPositions);

            for (int i = 0; i < columnListPositions.Count; i++)
            {
                if (columnListPositions[i] == checkPosition)
                    continue;

                if (columnListPositions[i] == actorGridCell.GridPosition)
                    continue;

                IGridCell gridCell = _gridCellManager.Get(columnListPositions[i]);
                if (gridCell != null)
                    gridCell.LockStates = LockStates.Preparing;
            }

            actorBooster.ChangeItemSprite(1);
            await actorBooster.PlayBoosterCombo(0, ComboBoosterType.StripedWrapped, true);

            // Break Triple Rows
            using (ListPool<UniTask>.Get(out var breakTasks))
            {
                using var rowListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> rowListPositions);
                AddTripleHorizontalLine(checkPosition, activeBounds, ref rowListPositions);

                for (int i = 0; i < rowListPositions.Count; i++)
                {
                    if (rowListPositions[i] == checkPosition)
                        continue;

                    if (rowListPositions[i] == actorGridCell.GridPosition)
                        continue;

                    breakTasks.Add(_breakGridTask.BreakItem(rowListPositions[i]));
                }

                await UniTask.WhenAll(breakTasks);
                SpawnTripleHorizontal(checkPosition);
                ExpandRange(ref rowListPositions, Vector3Int.up);

                // Lock this area to prevent outside items fall in to
                BoundsInt miniCheckBounds = checkPosition.GetBounds2D(1);
                miniCheckBounds.ForEach2D(position =>
                {
                    IGridCell gridCell = _gridCellManager.Get(position);
                    if (gridCell != null)
                        gridCell.LockStates = LockStates.Preparing;
                });

                BoundsInt horizontalCheckBounds = BoundsExtension.Encapsulate(rowListPositions);
                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(horizontalCheckBounds);
            }

            // Wait a moment
            _checkGridTask.CanCheck = true;
            actorBooster.ChangeItemSprite(2);
            await actorBooster.PlayBoosterCombo(0, ComboBoosterType.StripedWrapped, true);
            _checkGridTask.CanCheck = false;

            // Break Triple Columns
            using (ListPool<UniTask>.Get(out var breakTasks))
            {
                for (int i = 0; i < columnListPositions.Count; i++)
                {
                    breakTasks.Add(_breakGridTask.BreakItem(columnListPositions[i]));
                }

                await UniTask.WhenAll(breakTasks);
                SpawnTripleVertical(checkPosition);
                ExpandRange(ref columnListPositions, Vector3Int.right);

                _breakGridTask.ReleaseGridCell(actorGridCell);
                BoundsInt verticalCheckBounds = BoundsExtension.Encapsulate(columnListPositions);
                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(verticalCheckBounds);
            }
        }

        private void ExpandRange(ref List<Vector3Int> positions, Vector3Int direction)
        {
            int positionCount = positions.Count;
            Vector3Int minVertical = positions[0] - direction;
            Vector3Int maxVertical = positions[positionCount - 1] + direction;
            positions.Add(minVertical);
            positions.Add(maxVertical);
        }

        private void AddTripleHorizontalLine(Vector3Int checkPosition, BoundsInt activeBounds, ref List<Vector3Int> positions)
        {
            positions.AddRange(activeBounds.GetRow(checkPosition));
            positions.AddRange(activeBounds.GetRow(checkPosition + new Vector3Int(0, -1)));
            positions.AddRange(activeBounds.GetRow(checkPosition + new Vector3Int(0, 1)));
        }

        private void AddTripleVerticalLine(Vector3Int checkPosition, BoundsInt activeBounds, ref List<Vector3Int> positions)
        {
            positions.AddRange(activeBounds.GetColumn(checkPosition));
            positions.AddRange(activeBounds.GetColumn(checkPosition + new Vector3Int(-1, 0)));
            positions.AddRange(activeBounds.GetColumn(checkPosition + new Vector3Int(1, 0)));
        }

        private void SpawnTripleHorizontal(Vector3Int checkPosition)
        {
            Vector3 center = _gridCellManager.ConvertGridToWorldFunction(checkPosition);
            Vector3 down = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.down);
            Vector3 up = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.up);

            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, down);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, center);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, up);
        }

        private void SpawnTripleVertical(Vector3Int checkPosition)
        {
            Vector3 center = _gridCellManager.ConvertGridToWorldFunction(checkPosition);
            Vector3 right = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.right);
            Vector3 left = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.left);

            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, left);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, center);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, right);
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
